using inzynierka.Receipts.Responses;
using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Mappings;

public interface IUnitMapper
{
    UnitDto MapToDto(Unit unit);
    IEnumerable<UnitDto> MapToDtoList(IEnumerable<Unit> units);
}

