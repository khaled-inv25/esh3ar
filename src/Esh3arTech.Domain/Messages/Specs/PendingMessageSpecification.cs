using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace Esh3arTech.Messages.Specs
{
    public class PendingMessageSpecification : Specification<Message>
    {
        private string PhoneNumber { get; }

        public PendingMessageSpecification(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public override Expression<Func<Message, bool>> ToExpression()
        {
            return m => m.RecipientPhoneNumber.Equals(PhoneNumber) && m.Status.Equals(MessageStatus.Pending);
        }
    }
}
