using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace Esh3arTech.MobileUsers.Specs
{
    public class MobileVerifiedSpecification : Specification<MobileUser>
    {
        public string PhoneNumber { get; }

        public MobileVerifiedSpecification(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public override Expression<Func<MobileUser, bool>> ToExpression()
        {
            return m => m.MobileNumber.Equals(PhoneNumber) && m.Status.Equals(MobileUserRegisterStatus.Verified);
        }
    }
}
