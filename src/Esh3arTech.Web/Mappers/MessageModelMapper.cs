using Esh3arTech.Messages.Delivery;
using Esh3arTech.Web.Models;
using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace Esh3arTech.Web.Mappers
{
    public class MessageModelMapper : IObjectMapper<DeliverMessageDto, MessageModel>, ITransientDependency
    {
        private readonly ICurrentUser _currentUser;

        public MessageModelMapper(ICurrentUser currentUser) => _currentUser = currentUser;

        public MessageModel Map(DeliverMessageDto source)
        {
            return new MessageModel
            {
                Id = source.Id,
                RecipientPhoneNumber = source.RecipientPhoneNumber,
                MessageContent = source.MessageContent,
                CreatorId = source.CreatorId,
                Status = source.Status,
                AccessUrl = source.AccessUrl,
                UrlExpiresAt = source.UrlExpiresAt,
                From = _currentUser.UserName!
            };
        }

        public MessageModel Map(DeliverMessageDto source, MessageModel destination)
        {
            // We did not used this method in our codebase, so we can leave it unimplemented for now.
            throw new System.NotImplementedException();
        }
    }
}
