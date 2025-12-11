$(function () {
    const urlParams = new URLSearchParams(window.location.search);
    const subscriptionId = urlParams.get('subscriptionId');

    console.log(typeof subscriptionId);

    var historyDataTable = $('#HistoryTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [1, "asc"],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(
                esh3arTech.plans.subscriptions.subscription.getSubscriptionHistoryById,
                function () { return { subscriptionId: subscriptionId }; }
            ),
            columnDefs: [
                {
                    title: "Amount",
                    data: "amount"
                },
            ]
        })
    );
    
});