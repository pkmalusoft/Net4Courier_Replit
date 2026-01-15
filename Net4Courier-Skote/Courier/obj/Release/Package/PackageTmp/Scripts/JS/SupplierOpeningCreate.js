function SaveOpening() {
    debugger;
    if (parseFloat($('#txtDebit').val()) == 0 && parseFloat($('#txtCredit').val())==0)
    {
        Swal.fire("Save Status!", "Enter Amount", "error");
        return false;
    }
    $('#btnsave').attr('disabled', 'disabled');
    var dataobj= {
        AcOPInvoiceMasterID: $('#AcOPInvoiceMasterID').val(),
        AcOPInvoiceDetailId: $('#AcOPInvoiceDetailId').val(),
        PartyID: $('#PartyID').val(),
        SupplierTypeID: $('#SupplierTypeID').val(),
        Debit: $('#txtDebit').val(),
        Credit: $('#txtCredit').val(),
        InvoiceNo: $('#InvoiceNo').val(),
        InvoiceDate: $('#InvoiceDate').val(),
        AcHeadID :$('#AcHeadID').val()
    }
    $.ajax({
        type: "POST",
        url: "/SupplierOpening/SaveOpeningInvoice",
        datatype: "Json",
        data: { model: dataobj },        
        success: function (response) {
            if (response.status == "ok") {
                
                Swal.fire("Save Status!", response.message, "success");
                setTimeout(function () {
                    window.location.reload();
                }, 300)
            } else {
                $('#btnsave').removeAttr('disabled');
                Swal.fire("Save Status!", response.message, "error");
            }
        }
    });
}
(function ($) {

    'use strict';
    function initformControl() {
        $('#OpeningDate').datepicker({
            format: 'dd-mm-yyyy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $('#InvoiceDate').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        
        $("#PartyName").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/SupplierPayment/GetSupplierName',
                    datatype: "json",
                    data: {
                        term: request.term, SupplierTypeId: $('#SupplierTypeID').val(),
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.SupplierName,
                                value: val.SupplierName,
                                ID: val.SupplierID,
                            }
                        }))
                    }
                })
            },
            minLength: 1,
            autoFocus: true,
            select: function (e, i) {
                e.preventDefault();
                $("#PartyName").val(i.item.label);
                $('#PartyID').val(i.item.ID);
                $('#PartyID').attr('label', i.item.label);


            },

        });

        $("#SupplierTypeID").focus();
    }
    function init() {
        initformControl();
    }
    window.addEventListener(
        "load",
        function () {
            var t = document.getElementsByClassName("needs-validation");
            Array.prototype.filter.call(t, function (e) {
                e.addEventListener(
                    "submit",
                    function (t) {

                        if (false === e.checkValidity()) {
                            e.classList.add("was-validated");
                        }
                        else {
                            t.preventDefault();
                            t.stopPropagation();
                            e.classList.remove("was-validated");
                            SaveOpening();

                        }
                    },
                    !1
                );
            });
        },
        !1
    );
    $(document).ready(function () {
        init();

    })

})(jQuery)