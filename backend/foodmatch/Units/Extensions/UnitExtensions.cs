using foodmatch.Units.Responses;
using foodmatch.Units.Model;

namespace foodmatch.Units.Extensions;

public static class UnitExtensions
{
    public static UnitDto ToDto(this Unit unit)
    {
        return new UnitDto
        {
            UnitId = unit.UnitId,
            Name = unit.Name,
            Description = unit.Description,
            PromptDescription = unit.PromptDescription
        };
    }

    public static IEnumerable<UnitDto> ToDtoList(this IEnumerable<Unit> units)
        => units.Select(u => u.ToDto());
}

