using AutoMapper;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Products.OpenFoodFacts.Mappings;

/// <summary>
/// AutoMapper profile for mapping OpenFoodFacts data to Product models.
/// </summary>
public class OpenFoodFactsProfile: Profile {
    public OpenFoodFactsProfile() {
        CreateMap<string, ProductIngredientTag>()
            .ForMember(dest => dest.IngredientTag, opt => opt.MapFrom(src => new IngredientTag
            {
                Name = src
            }));
        CreateMap<string, ProductCategoryTag>()
            .ForMember(dest => dest.CategoryTag, opt => opt.MapFrom(src => new CategoryTag
            {
                Name = src
            })); 
        CreateMap<string, ProductCountryTag>()
            .ForMember(dest => dest.CountryTag, opt => opt.MapFrom(src => new CountryTag
            {
                Name = src
            }));
        CreateMap<string, ProductAllergenTag>()
            .ForMember(dest => dest.AllergenTag, opt => opt.MapFrom(src => new AllergenTag
            {
                Name = src
            }));
        
        CreateMap<OpenFoodFactsProduct, Product>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.BrandOwner, opt => opt.MapFrom(src => src.BrandOwner))
            .ForMember(dest => dest.ProductCategoryTags, opt => opt.MapFrom(src => src.CategoriesTags))
            .ForMember(dest => dest.ProductCountryTags, opt => opt.MapFrom(src => src.CountriesTags))
            .ForMember(dest => dest.Countries, opt => opt.MapFrom(src => src.Countries))
            .ForMember(dest => dest.CountriesCode, opt => opt.MapFrom(src => src.CountriesCode))
            .ForMember(dest => dest.Brands, opt => opt.MapFrom(src => src.Brands))
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories))
            .ForMember(dest => dest.NutritionGrade, opt => opt.MapFrom(src => src.NutritionGrade))
            .ForMember(dest => dest.NovaGroup, opt => opt.MapFrom(src => src.NovaGroup))
            .ForMember(dest => dest.EcoScoreGrade, opt => opt.MapFrom(src => src.EcoScoreGrade))
            .ForMember(dest => dest.IngredientsText, opt => opt.MapFrom(src => src.IngredientsText))
            .ForMember(dest => dest.ProductAllergenTags, opt => opt.MapFrom(src => src.AllergensTags))
            .ForMember(dest => dest.ProductIngredientTags, opt => opt.MapFrom(src => src.IngredientsTags))
            .ForMember(dest => dest.ServingSize, opt => opt.MapFrom(src => src.ServingSize))
            .ForMember(dest => dest.IsVegetarian, opt => opt.MapFrom(src => src.IsVegetarian))
            .ForMember(dest => dest.IsVegan, opt => opt.MapFrom(src => src.IsVegan))
            .ForMember(dest => dest.Energy100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Energy100g))
            .ForMember(dest => dest.EnergyKcal100g,
                opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.EnergyKcal100g))
            .ForMember(dest => dest.Fat100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Fat100g))
            .ForMember(dest => dest.SaturatedFat100g,
                opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.SaturatedFat100g))
            .ForMember(dest => dest.Carbohydrates100g,
                opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Carbohydrates100g))
            .ForMember(dest => dest.Sugars100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Sugars100g))
            .ForMember(dest => dest.Fiber100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Fiber100g))
            .ForMember(dest => dest.Proteins100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Proteins100g))
            .ForMember(dest => dest.Salt100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Salt100g))
            .ForMember(dest => dest.Sodium100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.Sodium100g))
            .ForMember(dest => dest.EnergyKcalServing,
                opt => opt.MapFrom(src => src.OpenFoodFactsNutriments.EnergyKcalServing))
            .ForMember(dest => dest.LastUpdated,
                opt => opt.MapFrom(src => DateTime.SpecifyKind(
                    DateTimeOffset.FromUnixTimeSeconds(src.LastUpdatedT).DateTime, DateTimeKind.Utc)));
    }
}