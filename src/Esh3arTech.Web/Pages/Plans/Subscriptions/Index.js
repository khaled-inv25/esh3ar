$(function () {
    var l = abp.localization.getResource('Esh3arTech');
    var renewModal = new abp.ModalManager(abp.appPath + 'Plans/Subscriptions/RenewModal');
    var changePlanModal = new abp.ModalManager(abp.appPath + 'Plans/Subscriptions/ChangePlan');

    var subscriptionDataTable = $('#SubscriptionTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(esh3arTech.plans.subscriptions.subscription.getAllSubscriptions),
            columnDefs: [
                {
                    title: l('Clm:Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l("Action:Renew"),
                                action: function (data) {
                                    renewModal.open({ subscriptionId: data.record.id });
                                    renewModal.onOpen(function () {
                                        $('#Price').prop('disabled', true);
                                    });
                                }
                            },
                            {
                                text: l("Action:ChangePlan"),
                                action: function (data) {
                                    changePlanModal.open({ subscriptionId: data.record.id });
                                    console.log(data.record.id);
                                }
                            },
                            {
                                text: l("Action:CancelSubscription"),
                            },
                            {
                                text: l("Action:PauseSubscription"),
                            },
                            {
                                text: l("Action:ResumeSubscription"),
                            },
                            {
                                text: l("Action:ChangeBillingInterval"),
                            },
                            {
                                text: l("Action:ViewSubscriptionDetails"),
                            },
                            {
                                text: l("Action:History"),
                                action: function (data) {
                                    window.location.href = abp.appPath + 'Plans/Subscriptions/History?subscriptionId=' + data.record.id;
                                }
                            },
                        ]
                    }
                },
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
                    data: "startDate",
                    render: formatDateTimeWithAMPM
                },
                {
                    title: 'End date',
                    data: "endDate",
                    render: formatDateTimeWithAMPM
                },
                {
                    title: 'Price',
                    data: "price"
                },
            ]
        })
    );

    var historyDataTable = $('#HistoryTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [1, "asc"],
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
                    data: "startDate",
                    render: formatDateTimeWithAMPM
                },
                {
                    title: 'End date',
                    data: "endDate",
                    render: formatDateTimeWithAMPM
                },
                {
                    title: 'Price',
                    data: "price"
                },
            ]
        })
    );
    
    function formatDateTimeWithAMPM(data) {
        if (data) {
            return moment(data).format('YYYY-MM-DD hh:mm:ss A');
        }
        return '';
    }
});