using Esh3arTech.Messages;
using Esh3arTech.Messages.Eto;
using System;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;

namespace Esh3arTech.Mappers
{
    public class SendMessageEtoToMessageMapper : IObjectMapper<SendOneWayMessageEto, Message>, ITransientDependency
    {
        public Message Map(SendOneWayMessageEto source)
        {
            return Message.CreateOneWayMessage(
                source.Id,
                source.CreatorId!.Value,
                source.RecipientPhoneNumber,
                source.MessageContent,
                source.Subject
            );
        }

        public Message Map(SendOneWayMessageEto source, Message destination)
        {
            // We did not used this method in our codebase, so we can leave it unimplemented for now.
            throw new NotImplementedException();
        }
    }
}
