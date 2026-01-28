(function () {

    abp.widgets.BotManagementAdminArea = function ($wrapper) {
        var l = abp.localization.getResource('Esh3arTech');

        var init = function (filters) {

            var _$table = $wrapper.find('#BotTable');

            var dataTable = _$table.DataTable(
                abp.libs.datatables.normalizeConfiguration({
                    serverSide: true,
                    paging: true,
                    order: [[1, "desc"]],
                    searching: false,
                    scrollX: true,
                    ajax: abp.libs.datatables.createAjax(esh3arTech.chats.bots.bot.getUsersWithBotFeature),
                    columnDefs: [
                        {
                            title: l('Clm:Actions'),
                            rowAction: {
                                items: [
                                    {
                                        text: l("Action:ManageBot"),
                                        action: function (data) {
                                            console.log('do somthing here');
                                        }
                                    },
                                    {
                                        text: l("Action:PuseBot"),
                                    },
                                ]
                            }

                        },
                        {
                            title: 'user name',
                            data: "userName"
                        },
                        {
                            title: 'id',
                            data: "id"
                        }
                    ]
                })
            );
        };

        // This return is MANDATORY
        return {
            init: init
        };
    };
})();
