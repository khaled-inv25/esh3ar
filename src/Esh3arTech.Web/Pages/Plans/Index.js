$(function () {
    var l = abp.localization.getResource('Esh3arTech');

    var editModal = new abp.ModalManager(abp.appPath + 'Plans/CreateModal');

    var dataTable = $('#PlanTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(esh3arTech.userPlans.plans.plan.getAllPlans),
            columnDefs: [
                {
                    title: l('Clm:Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l("Action:Edit"),
                                action: function (data) {
                                    console.log(data.record.id);
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Action:Delete"),
                                //action: function (data) {
                                //    editModal.open({ id: data.record.id });
                                //}
                            }
                        ]
                    }
                },
                {
                    title: l('Clm:DisplayName'),
                    data: "displayName"
                },
                {
                    title: l('Clm:ExpiringPlan'),
                    data: "expiringPlan"
                },
                {
                    title: l('Clm:DailyPrice'),
                    data: "dailyPrice"
                },
                {
                    title: l('Clm:WeeklyPrice'),
                    data: "weeklyPrice"
                },
                {
                    title: l('Clm:MonthlayPrice'),
                    data: "monthlayPrice"
                },
                {
                    title: l('Clm:AnnualPrice'),
                    data: "annualPrice"
                },
                {
                    title: l('Clm:TrialDayCount'),
                    data: "trialDayCount"
                },
                {
                    title: l('Clm:WaitingDayAfterExpire'),
                    data: "waitingDayAfterExpire"
                },
            ]
        })
    );

    var createModal = new abp.ModalManager(abp.appPath + 'Plans/CreateModal');

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewPlanBtn').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});