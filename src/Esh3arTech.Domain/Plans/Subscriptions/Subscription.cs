using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Validation;

namespace Esh3arTech.Plans.Subscriptions
{
    [Table(Esh3arTechConsts.TblSubscriptions)]
    public class Subscription : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; private set; }

        public Guid PlanId { get; private set; }

        public decimal Price { get; private set; }

        public BillingInterval BillingInterval { get; private set; }

        public DateTime StartDate { get; private set; }

        public DateTime EndDate { get; private set; }

        public DateTime NextBill { get; private set; }

        public bool IsAutoRenew { get; private set; }

        public bool IsActive { get; private set; }

        public LastPaymentStatus LastPaymentStatus { get; set; }

        public SubscriptionStatus Status { get; set; }

        public Subscription(
            Guid id,
            Guid userId,
            Guid planId,
            BillingInterval billingInterval)
            : base(id)
        {
            UserId = userId;
            PlanId = planId;
            SetBillingIntervalInternal(billingInterval);
            IsAutoRenew = false;
            IsActive = false;
        }

        private Subscription SetBillingIntervalInternal(BillingInterval billingInterval)
        {
            if (!Enum.IsDefined(typeof(BillingInterval), billingInterval))
            {
                throw new AbpValidationException();
            }

            BillingInterval = billingInterval;

            return this;
        }


        public Subscription SetInitialPeriod()
        {
            StartDate = DateTime.UtcNow;

            EndDate = BillingInterval switch
            {
                BillingInterval.Daily => StartDate.AddDays(1),
                BillingInterval.Weekly => StartDate.AddDays(7),
                BillingInterval.Monthly => StartDate.AddMonths(1),
                BillingInterval.Annually => StartDate.AddYears(1),
                _ => throw new ArgumentOutOfRangeException(),
            };

            return this;
        }

        public Subscription SetPrice(decimal price)
        {
            Price = Check.Range(price, nameof(price), 0.000001m, decimal.MaxValue);
            return this;
        }

        public Subscription SetNextBilling()
        {
            NextBill = EndDate;
            return this;
        }

        public Subscription AutoRenew()
        {
            IsAutoRenew = true;
            return this;
        }

        public Subscription Active()
        {
            IsActive = true;
            return this;
        }

        public Subscription SetLastPaymentStatus()
        {
            if (IsActive)
            {
                LastPaymentStatus = LastPaymentStatus.Succeeded;
                Status = SubscriptionStatus.Active;
            }
            else
            {
                LastPaymentStatus = LastPaymentStatus.Unknown;
                Status = SubscriptionStatus.Inactive;
            }

            return this;
        }
    }
}
