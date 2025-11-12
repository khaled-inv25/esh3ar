using Esh3arTech.MobileUsers;
using Esh3arTech.RegistrationRequests;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.Registretions
{
    [Table(Esh3arTechConsts.TblRegistretionRequest)]
    public class RegistretionRequest : CreationAuditedEntity<Guid>
    {
        public OS OS { get; set; }

        public string Secret { get; set; }

        public bool Verified { get; set; }

        public DateTime? VerifiedTime { get; set; }

        public Guid MobileUserId { get; set; }
        public MobileUser? MobileUser { get; set; }

        public RegistretionRequest() { /* Required for entity framework */  }
    }
}
