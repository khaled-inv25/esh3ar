$(function () {
    var l = abp.localization.getResource('Esh3arTech');

    var dataTable = $('#SubscriptionTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(esh3arTech.plans.subscriptions.subscription.getAllSubscriptions),
            columnDefs: [
                {
                    title: 'User name',
                    data: "userName"
                },
                {
                    title: 'Plan',
                    data: "plan"
                },
                {
                    title: 'Start date',
                    data: "startDate"
                },
                {
                    title: 'End date',
                    data: "endDate"
                },
                {
                    title: 'Price',
                    data: "price"
                },
            ]
        })
    );
});