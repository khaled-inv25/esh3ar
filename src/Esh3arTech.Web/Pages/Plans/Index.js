$(function () {
    var l = abp.localization.getResource('Esh3arTech');

    var createModal = new abp.ModalManager(abp.appPath + 'Plans/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'Plans/EditModal');
    var assignToUserModal = new abp.ModalManager(abp.appPath + 'Plans/Subscriptions/AssignToUser');

    var dataTable = $('#PlanTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "asc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(esh3arTech.plans.plan.getAllPlans),
            columnDefs: [
                {
                    title: l('Clm:Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l("Action:Edit"),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l("Action:Delete"),
                                confirmMessage: function (data) {
                                    return l('PlanDeletionConfirmationMessage', data.record.displayName);
                                },
                                action: function (data) {
                                    esh3arTech.plans.plan.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        })
                                }
                            },
                            {
                                text: l("Action:AssignToUser"),
                                action: function (data) {
                                    console.log(data.record.id);
                                    assignToUserModal.open({ planId: data.record.id });
                                }
                            },
                            {
                                text: l("Action:MoveUsersToPlan"),
                                confirmMessage: function (data) {
                                    return l('PlanMoveUsersConfirmationMessage', data.record.displayName);
                                },
                                action: function (data) {
                                    esh3arTech.plans.plan.moveUsersToPlan(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l('UsersMovedSuccessfully'));
                                            dataTable.ajax.reload();
                                        })
                                }
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

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewPlanBtn').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});