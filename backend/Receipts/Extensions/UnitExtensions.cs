using inzynierka.Receipts.Model;
using inzynierka.Receipts.Responses;

namespace inzynierka.Receipts.Extensions;

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

