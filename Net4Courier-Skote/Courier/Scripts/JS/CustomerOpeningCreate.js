function SaveOpening() {
    debugger;
    if (parseFloat($('#txtDebit').val()) == 0 && parseFloat($('#txtCredit').val())==0)
    {
        Swal.fire("Save Status!", "Enter Amount", "error");
        return false;
    }

    var _StatusSDSC = '';
    if ($('#CustomerType').val() == 'CR') {
        _StatusSDSC = 'C'
    }
    else {
        _StatusSDSC = 'L';
    }
    $('#btnsave').attr('disabled', 'disabled');
    var dataobj= {
        AcOPInvoiceMasterID: $('#AcOPInvoiceMasterID').val(),
        AcOPInvoiceDetailId: $('#AcOPInvoiceDetailId').val(),
        PartyID: $('#PartyID').val(),
        Debit: $('#txtDebit').val(),
        Credit: $('#txtCredit').val(),
        InvoiceNo: $('#InvoiceNo').val(),
        InvoiceDate: $('#InvoiceDate').val(),
        AcHeadID: $('#AcHeadID').val(),
        StatusSDSC: _StatusSDSC
    }
    $.ajax({
        type: "POST",
        url: "/CustomerOpening/SaveOpeningInvoice",
        datatype: "Json",
        data: { model: dataobj },        
        success: function (response) {
            if (response.status == "ok") {
                
                Swal.fire("Save Status!", response.message, "success");
                setTimeout(function () {
                    window.location.reload();
                }, 300)
            } else {
                Swal.fire("Save Status!", response.message, "error");
                $('#btnsave').removeAttr('disabled');
            }
        }
    });
}
(function ($) {

    'use strict';
    function initformControl() {
        $('#OpeningDate').datepicker({
            dateFormat: 'dd-mm-yy',
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
                    url: '/CustomerReceipt/GetCustomerNameByType',
                    datatype: "json",
                    data: {
                        term: request.term, CustomerType: $('#CustomerType').val()
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.CustomerName,
                                value: val.CustomerName,
                                ID: val.CustomerID,
                                type: val.CustomerType
                            }
                        }))
                    }
                })
            },
            minLength: 1,
            autoFocus: false,
            focus: function (e, i) {
                $("#PartyName").val(i.item.label);
                $('#PartyID').val(i.item.ID);                
                
            },
            select: function (e, i) {
                e.preventDefault();
                $("#PartyName").val(i.item.label);
                $('#PartyID').val(i.item.ID);
            },

        });

        //$("#PartyName").autocomplete({
        //    source: function (request, response) {
        //        $.ajax({
        //            url: '/CustomerMaster/GetCustomerName',
        //            datatype: "json",
        //            data: {
        //                term: request.term
        //            },
        //            success: function (data) {
        //                response($.map(data, function (val, item) {
        //                    return {
        //                        label: val.CustomerName,
        //                        value: val.CustomerName,
        //                        CustomerId: val.CustomerID,
        //                        type: val.CustomerType
        //                    }
        //                }))
        //            }
        //        })
        //    },
        //    minLength: 1,
        //    autoFocus: false,
        //    focus: function (event, ui) {
        //        $("#PartyName").val(ui.item.value);
        //        $('#PartyID').val(ui.item.CustomerId);
        //    },
        //    select: function (e, i) {
        //        e.preventDefault();
        //        $("#PartyName").val(i.item.label);
        //        $('#PartyID').val(i.item.CustomerId);
        //    },

        //});

        $("#PartyName").focus();
        
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