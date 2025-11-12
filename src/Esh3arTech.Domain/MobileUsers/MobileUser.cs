using Esh3arTech.Registretions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Esh3arTech.MobileUsers
{
    [Table(Esh3arTechConsts.TblMobileUsers)]
    public class MobileUser : FullAuditedEntity<Guid>
    {
        public string MobileNumber { get; set; }

        public MobileUserRegisterStatus Status { get; set; }

        public bool IsStatic { get; set; }

        public ICollection<RegistretionRequest> Requests { get; set; }

        /*
        //public Guid CurrentRegistrationId { get; set; }

        //[NotMapped]
        //public OS oS => CurrentRegistration.OS;

        //[NotMapped]
        //public bool? Verifiyed => CurrentRegistration.Verified;

        //[NotMapped]
        //public DateTime? VerifiyedTime => CurrentRegistration.VerifiedTime;

        //[NotMapped]
        //public MobileUserRegistritionRequest CurrentRegistration => Requests.FirstOrDefault(req => req.Id == CurrentRegistrationId);
        */


        public MobileUser() { /* Required for entity framework */ }

        public MobileUser(Guid id, string mobileNumber, bool isStatic) : base(id)
        {
            SetInternalMobileNumber(mobileNumber);
            IsStatic = isStatic;
            Status = MobileUserRegisterStatus.Pending;
            //Requests = new List<MobileUserRegistritionRequest>();
        }

        internal MobileUser SetInternalMobileNumber(string mobileNumber)
        {
            MobileNumber = Check.NotNullOrWhiteSpace(mobileNumber, nameof(mobileNumber));

            return this;
        }

    }
}
