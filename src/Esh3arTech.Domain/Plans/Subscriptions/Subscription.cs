using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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

        public ICollection<SubscriptionRenewalHistory> RenewalHistories { get; private set; }

        public Subscription(
            Guid id,
            Guid userId,
            Guid planId,
            BillingInterval billingInterval)
            : base(id)
        {
            UserId = userId;
            PlanId = planId;
            SetBillingInterval(billingInterval);
            IsAutoRenew = false;
            IsActive = false;
            RenewalHistories = new Collection<SubscriptionRenewalHistory>();
        }

        public Subscription SetBillingInterval(BillingInterval billingInterval)
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

            EndDate = CalcCurrentBillingInterval();

            return this;
        }

        public Subscription ExtendPeriod()
        {
            EndDate = EndDate = CalcCurrentBillingInterval();

            SetNextBilling();

            return this;
        }

        public Subscription SetPrice(decimal price)
        {
            Price = Check.Range(price, nameof(price), 0.00m, decimal.MaxValue);
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

        public Subscription RenewManually(decimal amount, InitialAssignment type)
        {
            if (!IsActive)
            {
                throw new BusinessException("Cannot renew an inactive subscription. please active this subscription or contact the admin");
            }

            AddHistory(amount, type);
            return this;
        }

        internal Subscription AddHistory(decimal amount, InitialAssignment type = InitialAssignment.NewAssignment)
        {
            if (InitialAssignment.NewAssignment.Equals(type))
            {
                var periodEndDate = CalcCurrentBillingInterval();

                RenewalHistories.Add(new SubscriptionRenewalHistory(
                    subscriptionId: Id,
                    renewalDate: DateTime.Now,
                    periodStartDate: DateTime.Now,
                    periodEndDate: periodEndDate,
                    amount,
                    BillingInterval,
                    isManual: true,
                    InitialAssignment.NewAssignment
                    ));
            }
            else
            {
                int daysLeft = 0;
                var lastHistory = RenewalHistories.OrderByDescending(srh => srh.CreationTime).FirstOrDefault();
                if (!HasExpired())
                {
                   daysLeft = (int)(EndDate - DateTime.Now).TotalDays;
                }

                var periodEndDate = BillingInterval switch
                {
                    BillingInterval.Daily => DateTime.Now.AddDays(1),
                    BillingInterval.Weekly => DateTime.Now.AddDays(7),
                    BillingInterval.Monthly => DateTime.Now.AddMonths(1),
                    BillingInterval.Annually => DateTime.Now.AddYears(1),
                    _ => throw new ArgumentOutOfRangeException(),
                };

                RenewalHistories.Add(new SubscriptionRenewalHistory(
                    subscriptionId: Id,
                    renewalDate: lastHistory!.RenewalDate,
                    periodStartDate: DateTime.Now.AddDays(daysLeft),
                    periodEndDate: periodEndDate,
                    amount,
                    BillingInterval,
                    isManual: true,
                    type
                    ));
            }
            
            return this;
        }

        private bool HasExpired()
        {
            return EndDate <= DateTime.UtcNow;
        }

        private DateTime CalcCurrentBillingInterval()
        {
            return BillingInterval switch
            {
                BillingInterval.Daily => EndDate.AddDays(1),
                BillingInterval.Weekly => EndDate.AddDays(7),
                BillingInterval.Monthly => EndDate.AddMonths(1),
                BillingInterval.Annually => EndDate.AddYears(1),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
