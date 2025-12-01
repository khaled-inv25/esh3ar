using System;
using Volo.Abp.Application.Dtos;

namespace Esh3arTech.Plans.Subscriptions
{
    public class SubscriptionInListDto : EntityDto<Guid>
    {
        public string UserName { get; set; }

        public string Plan { get; set; }

        public DateTime StartDay { get; set; }

        public DateTime EndDay { get; set; }

        public decimal Price { get; set; }


    }
}
