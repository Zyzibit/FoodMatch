using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Mappings;

public interface IUnitMapper
{
    UnitDto MapToDto(Unit unit);
    IEnumerable<UnitDto> MapToDtoList(IEnumerable<Unit> units);
}

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

