$(document).ready(function () {
    var table1=$("#datatable").DataTable({ "aaSorting": []});
        $("#datatable-buttons")
            .DataTable({ lengthChange: !1, buttons: ["copy", "excel", "pdf", "colvis"] })
            .buttons()
            .container()
            .appendTo("#datatable-buttons_wrapper .col-md-6:eq(0)"),
        $(".dataTables_length select").addClass("form-select form-select-sm");

    $('#datatable tfoot th').each(function () {
        var title = $(this).text();
        $(this).html('<input type="text" class="form-control" style="width:100%" />');
    });
    table1.draw();    
    table1.columns().every(function () {
        var that = this;

        $('input', this.footer()).on('keyup change', function () {
            if (that.search() !== this.value) {
                that
                    .search(this.value)
                    .draw();
            }
        });
    });
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

    var table = $('#datatableNoPage').DataTable({
       "aaSorting": [],
       // "aaSorting": [[0, 'desc']],
        //"order": [[2, "asc"]],
        "searching": true,
        "bPaginate": false,        
        //"pagingType": "simple",
        //aoColumnDefs: [
        //    { "aTargets": [0], "bSortable": true, "sType": "date" }            
        //]
    });

    var table = $('#datatableNoPage1').DataTable({
        "aaSorting": [],
        // "aaSorting": [[0, 'desc']],
        //"order": [[2, "asc"]],
        "searching": true,
        "bPaginate": false,
        //"pagingType": "simple",
        //aoColumnDefs: [
        //    { "aTargets": [0], "bSortable": true, "sType": "date" }            
        //]
    });

     
});
