$(document).ready(function () {
    $("#datatable").DataTable({ "aaSorting": []});
        $("#datatable-buttons")
            .DataTable({ lengthChange: !1, buttons: ["copy", "excel", "pdf", "colvis"] })
            .buttons()
            .container()
            .appendTo("#datatable-buttons_wrapper .col-md-6:eq(0)"),
        $(".dataTables_length select").addClass("form-select form-select-sm");


    var table = $('#datatableCustom').DataTable({
        //"aaSorting": [],
        "aaSorting": [[0, 'desc']],
        //"order": [[2, "asc"]],
        "searching": true,
        "pagingType": "full_numbers",
        //"pagingType": "simple",
        aoColumnDefs: [
            { "aTargets": [0], "bSortable": true, "sType": "date" },
            { "aTargets": [1], "bSortable": true },
            { "aTargets": [2], "bSortable": true },
            { "aTargets": [3], "bSortable": true },
            { "aTargets": [4], "bSortable": true },
            { "aTargets": [5], "bSortable": true },
            { "aTargets": [6], "bSortable": false },
            { "aTargets": [7], "bSortable": false }
        ]
    });
});
