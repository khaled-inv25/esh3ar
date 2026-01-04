using Volo.Abp.EventBus;

namespace Esh3arTech.Messages.Eto
{
    [EventName("Esh3arTech.Messages.MessageIngestionEto")]
    public class MessageIngestionEto
    {
        public string JsonMessages { get; set; }
    }
}
