using Microsoft.IdentityModel.Tokens;
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
            StartDate = DateTime.Now;

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
                    renewalDate: StartDate,
                    periodStartDate: StartDate,
                    periodEndDate: EndDate,
                    amount,
                    BillingInterval,
                    isManual: true,
                    InitialAssignment.NewAssignment
                    ));
            }
            else
            {
                var lastHistory = RenewalHistories.OrderByDescending(srh => srh.CreationTime).First();
                DateTime periodStartDate;
                DateTime periodEndDate;
                if (!HasExpired())
                {
                    periodStartDate = lastHistory.PeriodEndDate;
                    periodEndDate = BillingInterval switch
                    {
                        BillingInterval.Daily => periodStartDate.AddDays(1),
                        BillingInterval.Weekly => periodStartDate.AddDays(7),
                        BillingInterval.Monthly => periodStartDate.AddMonths(1),
                        BillingInterval.Annually => periodStartDate.AddYears(1),
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                    ExtendPeriod(periodEndDate);
                }

                else
                {
                    periodStartDate = DateTime.Now;
                    periodEndDate = BillingInterval switch
                    {
                        BillingInterval.Daily => DateTime.Now.AddDays(1),
                        BillingInterval.Weekly => DateTime.Now.AddDays(7),
                        BillingInterval.Monthly => DateTime.Now.AddMonths(1),
                        BillingInterval.Annually => DateTime.Now.AddYears(1),
                        _ => throw new ArgumentOutOfRangeException(),
                    };

                }

                RenewalHistories.Add(new SubscriptionRenewalHistory(
                        subscriptionId: Id,
                        renewalDate: DateTime.Now,
                        periodStartDate,
                        periodEndDate,
                        amount,
                        BillingInterval,
                        isManual: true,
                        type
                        ));
            }
            
            return this;
        }

        public IReadOnlyList<SubscriptionRenewalHistory> GetRenewalHistories()
        {
            return RenewalHistories
                .Where(srh => srh.SubscriptionId == Id).ToList();
        }

        private bool HasExpired()
        {
            return EndDate <= DateTime.Now;
        }

        private Subscription ExtendPeriod(DateTime period)
        {
            EndDate = period;
            SetNextBilling();

            return this;
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
