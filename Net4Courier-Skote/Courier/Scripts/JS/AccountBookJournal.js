
function DeleteJVEntry(id) {

    Swal.fire({ title: "Are you sure?", text: "Do you want to delete this?", icon: "warning", showCancelButton: !0, confirmButtonColor: "#34c38f", cancelButtonColor: "#f46a6a", confirmButtonText: "Yes, delete it!" }).then(
        function (t) {
            if (t.value) {

                $.ajax({
                    type: "POST",
                    url: '/Accounts/DeleteCashBankBook',
                    datatype: "json",
                    data: {
                        'id': id
                    },
                    success: function (data) {
                        if (data.status == "OK") {
                            Swal.fire("Delete Status!", "JV Deleted Successfully", "success");
                            window.location.reload();
                        }
                        else
                            Swal.fire("Delete Status!", data.message, "error");
                    }
                });

            }
        });
}

function funExportToPDF(id) {
    //showLoading();
    $.ajax({
        url: '/Accounts/AcJournalVoucherPrint',
        type: "GET",
        data: { id: id },
        dataType: "json",
        success: function (response) {
            if (response.result == "ok") {
                $('#frmPDF').attr('src', '/ReportsPDF/' + response.path); //''

    setTimeout(function () {
        //hideLoading();
        window.open('/ReportsPDF/' + response.path);
        //frame = document.getElementById("frmPDF");
        //framedoc = frame.contentWindow;
        //framedoc.focus();
        //framedoc.print();
    }, 500);
} else {
    alert(resuponse.message);
    //hideLoading();
}
            },
        });

return false;
    }






(function ($) {

    'use strict';
    function initformControl() {

        var table = $('#datatablesaccount').DataTable({ "sPaginationType": "full_numbers" });

        function format(d) {
            // `d` is the original data object for the row
            debugger;
            var JournalDetailId = d[4];
            var DataHtml = '';
            $.ajax({
                type: "POST",
                url: "/Accounts/AcBookDetails",
                datatype: "Json",
                data: { DetailId: JournalDetailId },
                success: function (data) {
                    debugger;
                    DataHtml = '<table width="100%" style="border:1px solid #74788d" class="table mb-0"><thead><th>Account</th><th>Amount</th><th>Remarks</th></thead><tbody>';
                    $.each(data, function (index, value) {
                        debugger;
                        DataHtml = DataHtml + '<tr><td>' + value.AcHead + '</td><td class="textright">' + parseFloat(value.Amount).toFixed(2) + '</td><td>' + value.Remarks + '</td></tr>';
                    });
                    debugger;
                    DataHtml = DataHtml + '</tbody></table>';
                    $('#pr_' + JournalDetailId).append(DataHtml);
                }
            });

            return '<p id="pr_' + JournalDetailId + '"></p>';
        }

        $('#datatablesaccount tbody').on('click', 'td.details-control', function () {
            debugger;
            var tr = $(this).closest('tr');
            var row = table.row(tr);
            if (row.child.isShown()) {
                // This row is already open - close it
                row.child.hide();
                tr.removeClass('shown');
                $(this).find('img').attr('src', '/assets/images/details_open.png');
            }
            else {
                // Open this row
                row.child(format(row.data())).show();
                tr.addClass('shown');
                $(this).find('img').attr('src', '/assets/images/details_close.png');
            }
        });


    }

    function init() {
        initformControl();
    }
    $(document).ready(function () {
        init();


    })


})(jQuery)
