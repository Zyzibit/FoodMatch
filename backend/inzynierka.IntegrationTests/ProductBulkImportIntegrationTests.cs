using System.Text;
using inzynierka.Data;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Products.OpenFoodFacts.Import;
using inzynierka.Products.OpenFoodFacts.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;

namespace inzynierka.IntegrationTests;

/// <summary>
/// Test ścieżki masowego importu na PRAWDZIWYM PostgreSQL (Testcontainers).
/// Pokrywa to, czego nie da się sprawdzić na EF InMemory: binary COPY, INSERT ON CONFLICT,
/// MERGE linków, dosiewanie tagów oraz round-trip znaczników czasu (poprawka DateTime.Kind).
///
/// Wymaga działającego Dockera.
/// </summary>
public class ProductBulkImportIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private AppDbContext _db = null!;
    private readonly List<string> _tempFiles = new();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _db = new AppDbContext(options);
        await _db.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _postgres.DisposeAsync();
        foreach (var f in _tempFiles)
        {
            try { File.Delete(f); } catch { /* best effort */ }
        }
    }

    // ---- Dane testowe -------------------------------------------------------

    // Pełny, poprawny produkt z tagami wszystkich rodzajów.
    private const string Product001 =
        @"{""code"":""001"",""product_name"":""Milk"",""lang"":""en"",""lc"":""en"",""brands"":""BrandA"",""nutrition_grades"":""a"",""nova_group"":1,""ecoscore_grade"":""b"",""ingredients_text"":""milk"",""serving_size"":""100ml"",""is_vegetarian"":""yes"",""is_vegan"":""no"",""last_updated_t"":1700000000,""categories_tags"":[""en:dairies"",""en:milks""],""countries_tags"":[""en:poland""],""allergens_tags"":[""en:milk""],""ingredients_tags"":[""en:milk""],""nutriments"":{""energy-kcal_100g"":42,""fat_100g"":1.0,""saturated-fat_100g"":0.6,""carbohydrates_100g"":5.0,""proteins_100g"":3.4,""sugars_100g"":5.0,""salt_100g"":0.1,""sodium_100g"":0.04,""fiber_100g"":0,""energy_100g"":176,""energy-kcal_serving"":42}}";

    // Poprawny, minimalny, bez tagów.
    private const string Product002 =
        @"{""code"":""002"",""product_name"":""Water"",""last_updated_t"":1700000001,""nutriments"":{""energy-kcal_100g"":0,""fat_100g"":0,""carbohydrates_100g"":0,""proteins_100g"":0}}";

    // Brak nutriments → pomijany (SkippedNoNutrition).
    private const string ProductNoNutrition =
        @"{""code"":""003"",""product_name"":""NoNutri"",""last_updated_t"":1700000002}";

    // Pusty kod → pomijany (SkippedNoCode).
    private const string ProductNoCode =
        @"{""code"":"""",""product_name"":""NoCode"",""nutriments"":{""energy-kcal_100g"":1,""fat_100g"":1,""carbohydrates_100g"":1,""proteins_100g"":1}}";

    // Uszkodzony JSON → liczony jako Failed, nie wywala importu.
    private const string Malformed = "{ this is not valid json";

    private static readonly DateTime Product001ExpectedLastUpdated =
        DateTimeOffset.FromUnixTimeSeconds(1700000000).UtcDateTime;

    private string WriteJsonl(IEnumerable<string> lines, bool withBom = false, string newline = "\n")
    {
        var path = Path.GetTempFileName();
        _tempFiles.Add(path);
        var bytes = Encoding.UTF8.GetBytes(string.Join(newline, lines));
        if (withBom)
            bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(bytes).ToArray();
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private async Task ImportAsync(string path)
    {
        var repo = new OpenFoodFactsRepository(_db, NullLogger<OpenFoodFactsRepository>.Instance);
        var importer = new ProductImporter(repo, NullLogger<ProductImporter>.Instance);
        await importer.ImportJsonlAsync(path);
    }

    // ---- Testy --------------------------------------------------------------

    [Fact]
    public async Task Import_InsertsValidProducts_SkipsInvalid_AndIsTagged()
    {
        // BOM + CRLF, żeby przy okazji sprawdzić obsługę tych przypadków na realnej bazie.
        var path = WriteJsonl(
            new[] { Product001, Product002, ProductNoNutrition, ProductNoCode, Malformed },
            withBom: true, newline: "\r\n");

        await ImportAsync(path);

        // Tylko 001 i 002 są poprawne — reszta pominięta/uszkodzona.
        Assert.Equal(2, await _db.Products.AsNoTracking().CountAsync());

        var milk = await _db.Products.AsNoTracking().SingleAsync(p => p.Code == "001");
        Assert.Equal("Milk", milk.ProductName);
        Assert.Equal("BrandA", milk.Brands);
        Assert.Equal(3.4, milk.Proteins100g);
        Assert.Equal(42, milk.EnergyKcal100g);
        Assert.NotNull(milk.LastUpdated);
        // Round-trip znacznika czasu (poprawka DateTime.Kind=Unspecified dla `timestamp`).
        Assert.Equal(Product001ExpectedLastUpdated.Ticks, milk.LastUpdated!.Value.Ticks);

        // Tagi dosiane ze stage'a linków.
        Assert.Equal(2, await _db.CategoryTags.AsNoTracking().CountAsync());   // en:dairies, en:milks
        Assert.Equal(1, await _db.CountryTags.AsNoTracking().CountAsync());    // en:poland
        Assert.Equal(1, await _db.AllergenTags.AsNoTracking().CountAsync());   // en:milk
        Assert.Equal(1, await _db.IngredientTags.AsNoTracking().CountAsync()); // en:milk

        // Linki tylko dla 001 (002 nie ma tagów).
        Assert.Equal(2, await _db.Set<ProductCategoryTag>().AsNoTracking().CountAsync());
        Assert.Equal(1, await _db.Set<ProductCountryTag>().AsNoTracking().CountAsync());
        Assert.Equal(1, await _db.Set<ProductAllergenTag>().AsNoTracking().CountAsync());
        Assert.Equal(1, await _db.Set<ProductIngredientTag>().AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task Reimport_NewerRecord_UpsertsProduct_AndDoesNotDuplicateTagsOrLinks()
    {
        await ImportAsync(WriteJsonl(new[] { Product001 }));

        var updated = Product001
            .Replace(@"""product_name"":""Milk""", @"""product_name"":""Milk Updated""")
            .Replace(@"""last_updated_t"":1700000000", @"""last_updated_t"":1700000100");

        await ImportAsync(WriteJsonl(new[] { updated }));

        // Wciąż jeden produkt (upsert, nie insert).
        Assert.Equal(1, await _db.Products.AsNoTracking().CountAsync());

        var milk = await _db.Products.AsNoTracking().SingleAsync(p => p.Code == "001");
        Assert.Equal("Milk Updated", milk.ProductName); // nowszy rekord wygrywa

        // MERGE i dosiewanie tagów idempotentne — brak duplikatów.
        Assert.Equal(1, await _db.IngredientTags.AsNoTracking().CountAsync());
        Assert.Equal(2, await _db.CategoryTags.AsNoTracking().CountAsync());
        Assert.Equal(1, await _db.Set<ProductIngredientTag>().AsNoTracking().CountAsync());
        Assert.Equal(2, await _db.Set<ProductCategoryTag>().AsNoTracking().CountAsync());
    }

    [Fact]
    public async Task Reimport_OlderRecord_DoesNotOverwriteNewerData()
    {
        var newer = Product001
            .Replace(@"""product_name"":""Milk""", @"""product_name"":""Newer""")
            .Replace(@"""last_updated_t"":1700000000", @"""last_updated_t"":1700000200");
        await ImportAsync(WriteJsonl(new[] { newer }));

        var older = Product001
            .Replace(@"""product_name"":""Milk""", @"""product_name"":""Older""")
            .Replace(@"""last_updated_t"":1700000000", @"""last_updated_t"":1699999000");
        await ImportAsync(WriteJsonl(new[] { older }));

        var milk = await _db.Products.AsNoTracking().SingleAsync(p => p.Code == "001");
        Assert.Equal("Newer", milk.ProductName); // starszy rekord NIE nadpisuje (WHERE EXCLUDED.LastUpdated > ...)
    }
}
