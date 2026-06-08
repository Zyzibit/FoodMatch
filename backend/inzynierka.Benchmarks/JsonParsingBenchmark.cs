using System.Text;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using inzynierka.IO.Parsing;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Benchmarks;

/// <summary>
/// Porównuje trzy ścieżki parsowania jednej linii JSONL OpenFoodFacts:
///   1. klasyczna: bajty → string → Deserialize (reflection)  [baseline, stary tor]
///   2. bajty → Deserialize (reflection)                       [obecny generyczny default]
///   3. bajty → Deserialize (source-gen, JsonTypeInfo)         [tor OFF po optymalizacji]
///
/// Linia zawiera pola modelu + ~40 nieznanych pól (zagnieżdżone obiekty/tablice),
/// by odwzorować duże, rzadkie dokumenty z prawdziwego dumpu (~60 GB).
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class JsonParsingBenchmark
{
    private byte[] _utf8 = null!;

    private static readonly JsonSerializerOptions ReflectionOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private JsonRecordParser<OpenFoodFactsProduct> _reflection = null!;
    private JsonRecordParser<OpenFoodFactsProduct> _sourceGen = null!;

    [GlobalSetup]
    public void Setup()
    {
        _utf8 = Encoding.UTF8.GetBytes(SampleLine());
        _reflection = new JsonRecordParser<OpenFoodFactsProduct>(ReflectionOptions);
        _sourceGen = new JsonRecordParser<OpenFoodFactsProduct>(BenchmarkJsonContext.Default.OpenFoodFactsProduct);
    }

    [Benchmark(Baseline = true)]
    public OpenFoodFactsProduct? Classic_String_Reflection()
    {
        var json = Encoding.UTF8.GetString(_utf8); // alokacja string na każdą linię (stary tor)
        return JsonSerializer.Deserialize<OpenFoodFactsProduct>(json, ReflectionOptions);
    }

    [Benchmark]
    public OpenFoodFactsProduct Bytes_Reflection()
    {
        _reflection.TryParse(_utf8, out var value);
        return value;
    }

    [Benchmark]
    public OpenFoodFactsProduct Bytes_SourceGen()
    {
        _sourceGen.TryParse(_utf8, out var value);
        return value;
    }

    private static string SampleLine()
    {
        var sb = new StringBuilder(4096);
        sb.Append('{');

        // Pola, których faktycznie używamy.
        sb.Append("\"code\":\"3017620422003\",");
        sb.Append("\"lang\":\"fr\",\"lc\":\"fr\",");
        sb.Append("\"product_name\":\"Nutella pâte à tartiner aux noisettes et au cacao\",");
        sb.Append("\"brand_owner\":\"Ferrero\",\"brands\":\"Nutella,Ferrero\",");
        sb.Append("\"categories\":\"Spreads,Sweet spreads\",");
        sb.Append("\"nutrition_grades\":\"e\",\"nova_group\":4,\"ecoscore_grade\":\"d\",");
        sb.Append("\"ingredients_text\":\"Sucre, huile de palme, NOISETTES 13%, cacao maigre 7.4%, LAIT écrémé en poudre 8.7%\",");
        sb.Append("\"serving_size\":\"15 g\",\"is_vegetarian\":\"yes\",\"is_vegan\":\"no\",");
        sb.Append("\"last_updated_t\":1700000000,");
        sb.Append("\"categories_tags\":[\"en:spreads\",\"en:sweet-spreads\",\"en:chocolate-spreads\",\"en:hazelnut-spreads\"],");
        sb.Append("\"countries_tags\":[\"en:france\",\"en:germany\",\"en:belgium\"],");
        sb.Append("\"allergens_tags\":[\"en:milk\",\"en:nuts\",\"en:soybeans\"],");
        sb.Append("\"ingredients_tags\":[\"en:sugar\",\"en:palm-oil\",\"en:hazelnut\",\"en:cocoa\",\"en:skimmed-milk-powder\"],");
        sb.Append("\"nutriments\":{");
        sb.Append("\"energy_100g\":2252,\"energy-kcal_100g\":539,\"fat_100g\":30.9,\"saturated-fat_100g\":10.6,");
        sb.Append("\"carbohydrates_100g\":57.5,\"sugars_100g\":56.3,\"fiber_100g\":3.4,\"proteins_100g\":6.3,");
        sb.Append("\"salt_100g\":0.107,\"sodium_100g\":0.0428,\"energy-kcal_serving\":80,");
        sb.Append("\"fat_serving\":4.6,\"sugars_serving\":8.4,\"energy_serving\":338,\"salt_serving\":0.016");
        sb.Append("},");

        // ~40 nieznanych pól odwzorowujących rozdęty dokument OFF.
        for (var i = 0; i < 40; i++)
        {
            sb.Append("\"unknown_field_").Append(i).Append("\":");
            switch (i % 4)
            {
                case 0: sb.Append('"').Append("value_").Append(i).Append("\","); break;
                case 1: sb.Append(i).Append(','); break;
                case 2: sb.Append("[\"a\",\"b\",\"c\"],"); break;
                default: sb.Append("{\"x\":").Append(i).Append(",\"y\":\"z\"},"); break;
            }
        }

        sb.Append("\"_id\":\"3017620422003\"");
        sb.Append('}');
        return sb.ToString();
    }
}
