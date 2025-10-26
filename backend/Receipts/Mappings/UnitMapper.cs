using inzynierka.Receipts.Model;
using inzynierka.Receipts.Responses;

namespace inzynierka.Receipts.Mappings;


public class UnitMapper : IUnitMapper
{
    public UnitDto MapToDto(Unit unit)
    {
        return new UnitDto
        {
            UnitId = unit.UnitId,
            Name = unit.Name,
            Description = unit.Description,
            PromptDescription = unit.PromptDescription
        };
    }

    public IEnumerable<UnitDto> MapToDtoList(IEnumerable<Unit> units)
    {
        return units.Select(MapToDto);
    }
}