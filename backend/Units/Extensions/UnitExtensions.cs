using inzynierka.Units.Dto;
using inzynierka.Units.Models;

namespace inzynierka.Units.Extensions;

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

