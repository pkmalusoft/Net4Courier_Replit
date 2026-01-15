
(function ($) {

    'use strict';
    function initformControl() {
        var table = $('#datatablesaccount').DataTable({ "aaSorting": [], "sPaginationType": "full_numbers" });
        function format(d) {
            // `d` is the original data object for the row
            debugger;
            var AcOpInvoiceMasterID = d[6];
            var DataHtml = '';
            $.ajax({
                type: "POST",
                url: "/CustomerOpening/ShowOpeningDetails",
                datatype: "html",
                data: { AcOpInvoiceMasterID: AcOpInvoiceMasterID},
                success: function (data) {
                    debugger;                 
                    $('#pr_' + AcOpInvoiceMasterID).html(data);
                }
            });

            return '<p id="pr_' + AcOpInvoiceMasterID + '"></p>';
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