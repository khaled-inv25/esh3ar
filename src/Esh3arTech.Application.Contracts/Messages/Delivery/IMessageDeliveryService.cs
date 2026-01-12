using System.Threading.Tasks;

namespace Esh3arTech.Messages.Delivery
{
    public interface IMessageDeliveryService
    {
        Task DeliverBatchMessageAsync(DeliverBatchMessageDto dtos);
        Task DeliverMessageAsync(DeliverMessageDto dto);
    }
}
