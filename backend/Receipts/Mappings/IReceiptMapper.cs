using inzynierka.Receipts.Responses;
using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Mappings;

public interface IReceiptMapper
{
    ReceiptDto MapToDto(Receipt receipt);
    IEnumerable<ReceiptDto> MapToDtoList(IEnumerable<Receipt> receipts);
}
