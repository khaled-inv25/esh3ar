using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Esh3arTech.Web.Pages.Plans.Subscriptions
{
    public class HistoryModel : Esh3arTechPageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid SubscriptionId { get; set; }

        public async Task OnGetAsync()
        {
        }
    }
}
