$(function () {
    var l = abp.localization.getResource('Esh3arTech');
    var sendModal = new abp.ModalManager(abp.appPath + 'Messages/SendMessageModal');

    var dataTable = $('#MessagesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[1, "desc"]],
            searching: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(esh3arTech.messages.message.getAllMessages),
            columnDefs: [
                {
                    title: l('Clm:To'),
                    data: "recipientPhoneNumber"
                },
                {
                    title: l('Clm:MessageContent'),
                    data: "messageContent"
                },
                {
                    title: l('Clm:CreationTime'),
                    data: "creationTime"
                },
                {
                    title: l('Clm:MessageStatus'),
                    data: "status",
                    render: function (data) {
                        return l('Enum:MessageStatus.' + data);
                    }
                },
            ]
        })
    );

    sendModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#SendMsgBtn').click(function (e) {
        e.preventDefault();
        sendModal.open();
    });

});