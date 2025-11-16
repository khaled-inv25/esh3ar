using Esh3arTech.MobileUsers;
using Esh3arTech.RegistrationRequests;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Timing;

namespace Esh3arTech.Registretions
{
    [Table(Esh3arTechConsts.TblRegistretionRequest)]
    public class RegistretionRequest : CreationAuditedEntity<Guid>
    {
        public OS OS { get; private set; }

        public string Secret { get; private set; }

        public bool Verified { get; private set; }

        public DateTime? VerifiedTime { get; private set; }

        public Guid MobileUserId { get; private set; }
        public MobileUser? MobileUser { get; set; }

        public RegistretionRequest() { /* Required for entity framework */  }

        public RegistretionRequest(Guid id, OS oS, string secret, Guid mobileUserId)
            : base(id)
        {
            SetOsInternal(oS);
            SetSecretInternal(secret);
            MobileUserId = mobileUserId;
        }

        public RegistretionRequest SetAsVerified(DateTime verifedDateTime)
        {
            Verified = true;
            VerifiedTime = verifedDateTime;
            return this;
        }

        internal RegistretionRequest SetOsInternal(OS oS)
        {
            if (!Enum.IsDefined(oS))
            {
                throw new ArgumentException("Unkowing os");
            }

            OS = oS;

            return this;
        }

        internal RegistretionRequest SetSecretInternal(string secret)
        {
            Secret = Check.NotNullOrEmpty(secret, nameof(secret));

            return this;
        }
    }
}
