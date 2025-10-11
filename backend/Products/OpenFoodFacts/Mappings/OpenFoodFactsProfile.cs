using AutoMapper;
using inzynierka.Products.Model;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;

namespace inzynierka.Products.OpenFoodFacts.Mappings;

/// <summary>
/// AutoMapper profile for mapping OpenFoodFacts data to Product models.
/// </summary>
public class OpenFoodFactsProfile : Profile
{
    public OpenFoodFactsProfile()
    {
        CreateMap<OpenFoodFactsProduct, Product>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
            .ForMember(dest => dest.BrandOwner, opt => opt.MapFrom(src => src.BrandOwner))
            .ForMember(dest => dest.LanguageCode, opt => opt.MapFrom(src => src.LanguageCode))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
            .ForMember(dest => dest.Brands, opt => opt.MapFrom(src => src.Brands))
            .ForMember(dest => dest.NutritionGrade, opt => opt.MapFrom(src => src.NutritionGrade))
            .ForMember(dest => dest.NovaGroup, opt => opt.MapFrom(src => src.NovaGroup))
            .ForMember(dest => dest.EcoScoreGrade, opt => opt.MapFrom(src => src.EcoScoreGrade))
            .ForMember(dest => dest.IngredientsText, opt => opt.MapFrom(src => src.IngredientsText))
            .ForMember(dest => dest.ServingSize, opt => opt.MapFrom(src => src.ServingSize))
            .ForMember(dest => dest.IsVegetarian, opt => opt.MapFrom(src => src.IsVegetarian))
            .ForMember(dest => dest.IsVegan, opt => opt.MapFrom(src => src.IsVegan))
            .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => 
                src.LastUpdatedT > 0 
                    ? DateTime.SpecifyKind(DateTimeOffset.FromUnixTimeSeconds(src.LastUpdatedT).DateTime, DateTimeKind.Utc)
                    : (DateTime?)null))
            .ForMember(dest => dest.Energy100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Energy100g : null))
            .ForMember(dest => dest.EnergyKcal100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.EnergyKcal100g : null))
            .ForMember(dest => dest.Fat100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Fat100g : null))
            .ForMember(dest => dest.SaturatedFat100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.SaturatedFat100g : null))
            .ForMember(dest => dest.Carbohydrates100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Carbohydrates100g : null))
            .ForMember(dest => dest.Sugars100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Sugars100g : null))
            .ForMember(dest => dest.Fiber100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Fiber100g : null))
            .ForMember(dest => dest.Proteins100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Proteins100g : 0))
            .ForMember(dest => dest.Salt100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Salt100g : null))
            .ForMember(dest => dest.Sodium100g, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.Sodium100g : null))
            .ForMember(dest => dest.EnergyKcalServing, opt => opt.MapFrom(src => src.OpenFoodFactsNutriments != null ? src.OpenFoodFactsNutriments.EnergyKcalServing : null))
            .ForMember(dest => dest.ProductCountryTags, opt => opt.Ignore())
            .ForMember(dest => dest.ProductCategoryTags, opt => opt.Ignore())
            .ForMember(dest => dest.ProductAllergenTags, opt => opt.Ignore())
            .ForMember(dest => dest.ProductIngredientTags, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}