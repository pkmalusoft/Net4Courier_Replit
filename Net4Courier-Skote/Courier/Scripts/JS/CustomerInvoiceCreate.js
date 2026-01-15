
function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
 
function SaveCustomerInvoice() {
    debugger;
 
    $('#btnsave').attr('disabled', 'disabled');
    var maxrow = $('#details > tbody > tr').length;
    var totalcharge = parseFloat($('#lbltotalcharge').html());
    if ($('#CustomerID').val() == '' || $('#CustomerID').val() == 0 || $('#CustomerName').val()=='') {

        Swal.fire("Save Status", "Select customer to add Invoice!", "warning");
        $('#btnsave').removeAttr('disabled');
        $('#CustomerName').focus();
        return false;
    }

    if ((totalcharge.toString() == '0' || totalcharge.toString() == 'NaN') && $('#CustomerInvoiceID').val()=='0' ) {
       $('#btnsave').removeAttr('disabled');
        Swal.fire("Save Status", "Select AWB Details to add Invoice!", "warning");

      return false;
   }
    //else if (maxrow == 0) {
    //    $('#btnsave1').removeAttr('disabled');

    //    Swal.fire("Save Status", "Select AWB Details to add Invoice!", "warning");
    //    return false;
    //}
    //else if (totalcharge == 0) {
    //    $('#btnsave1').removeAttr('disabled');
    //    Swal.fire("Save Status", "Select AWB Details to add Invoice!", "warning");

    //    return false;
    //}
    if ($('#ChargeableWT').val() == 'NaN') {
        $('#ChargeableWT').val(0);
    }
   
    var RecPObj = {
        CustomerInvoiceID:$('#CustomerInvoiceID').val(),
        CustomerInvoiceNo :$('#CustomerInvoiceNo').val(),
        InvoiceDate: $('#InvoiceDate').val(),
         DueDate :$('#DueDate').val(),
        CustomerID:$('#CustomerID').val() ,
        CustomerInvoiceTax:$('#CustomerInvoiceTax').val(),
        ChargeableWT:$('#ChargeableWT').val(),
        AdminPer:$('#AdminPer').val(),
        AdminAmt :$('#AdminAmt').val(),
        FuelPer :$('#FuelPer').val(),
        FuelAmt :$('#FuelAmt').val(),
        ClearingCharge: $('#ClearingCharge').val(),
        OtherCharge: $('#OtherCharge').val(), 
        InvoiceTotal:$('#InvoiceTotal').val(),
         Discount :$('#Discount').val()

    }

    if (totalcharge == 0) {

        $.ajax({
            type: "POST",
            url: "/CustomerInvoice/SaveCustomerInvoice",
            datatype: "Json",
            data: { model: RecPObj, Details: '' },
            success: function (response) {
                if (response.status == "OK") {

                    Swal.fire("Save Status", response.message, "success");
                    setTimeout(function () {
                        location.href = '/CustomerInvoice/Create?id=0';
                    }, 200)

                }
                else {
                    $('#btnsave').removeAttr('disabled');
                    Swal.fire("Save Status", response.message, "error");
                    return false;
                }
            }
        });

    }
    else {
        var Items = [];

        var itemcount = $('#details > tbody > tr').length;
        var idtext = 'hdnInscanID_';

        $('[id^=' + idtext + ']').each(function (index, item) {
            var awbchecked = $('#hdnawbchecked_' + index).prop('checked');
            if (awbchecked == true) {
                var inscanid = $('#hdnInscanID_' + index).val();
                var awbno = $('#hdnAWBNo_' + index).val();
                var couriercharge = $('#hdnCourierCharge_' + index).val();
                var othercharge = $('#hdnOtherCharge_' + index).val();
                var totalcharge = $('#hdnTotalCharge_' + index).val();
                var vatamount = $('#hdnVatAmount_' + index).val();
                var surcharge = $('#hdnSurCharge_' + index).val();
                var surchargepercent = $("#hdnSurchargePercent_" + index).val();
                var customcharge = $('#hdnCustomCharge_' + index).val();
                var invoicedetaild = $('#hdnCustomerInvoiceDetailID_' + index).val();
                var data = {
                    CustomerInvoiceDetailID: invoicedetaild,
                    AWBChecked: awbchecked,
                    InscanID: inscanid,
                    AWBNo: awbno,
                    CourierCharge: couriercharge,
                    OtherCharge: othercharge,
                    VatAmount: vatamount,
                    //TotalCharge: totalcharge,
                    FuelSurcharge: surcharge,
                    SurchargePercent: surchargepercent,
                    CustomCharge: customcharge,
                    NetValue: totalcharge

                }

                Items.push(data);

            }
            if ((index+1) == (itemcount)) {
                debugger;
                $.ajax({
                    type: "POST",
                    url: "/CustomerInvoice/SaveCustomerInvoice",
                    datatype: "Json",
                    data: { model: RecPObj, Details: JSON.stringify(Items) },
                    success: function (response) {
                        if (response.status == "OK") {

                            Swal.fire("Save Status", response.message, "success");
                            $('#btnsave').removeAttr('disabled');
                            setTimeout(function () {
                                if ($('#CustomerInvoiceID').val() == 0) {
                                    location.href = '/CustomerInvoice/Create?id=0';
                                }
                                else {
                                    location.href = '/CustomerInvoice/Create?id=' + response.CustomerInvoiceID;
                                }
                                
                            }, 200)

                        }
                        else {
                            $('#btnsave').removeAttr('disabled');
                            Swal.fire("Save Status", response.message, "error");
                            return false;
                        }
                    }
                });
            }
        });
    }
   
}
(function ($) {

    'use strict';
    function initformControl() {
        $('#Date').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
       

        $('#btnsearch').click(function () {

            if ($('#CustomerID').val() == 0 || $('#CustomerID').val() == '' || $('#CustomerID').val() == null) {
                $('#CustomerName').focus();
                return false;
            }
            else if ($('#SelectedValues').val() == null) {
                $('#SelectedValue').focus();
                
                Swal.fire("Data Validation", "Select Courier type!", "warning");
                return false;
            }

            $.ajax({
                type: 'POST',
                url: '/CustomerInvoice/ShowItemList',
                datatype: "html",
                data: {
                    CustomerId: $('#CustomerID').val(), FromDate: $('#FromDate').val(), ToDate: $('#ToDate').val(), MovementId: $('#SelectedValues').val(), InvoiceId: $('#CustomerInvoiceID').val()
                },
                success: function (data) {
                    debugger;
                    $("#listContainer").html(data);
                    $("#CustomerName").attr('CustomerId', $('#CustomerID').val());
                    $('#btnsave1').removeAttr('disabled');
                    
                    var max = $('#details > tbody > tr').length;// - 5;
                
                    setTimeout(function (){
                    if (max <=0) {

                        Swal.fire("Data Validation", "AWB are not found!", "warning");
                    }
                    else {
                        ValidateTotal();
                    }
                }, 100);

                }
            });

        });

        if ($('#CustomerInvoiceID').val() > 0) {
            $('#btnsave').val('Update');
        
            if ($('#InvoiceTotal').val() == '' || $('#InvoiceTotal').val() == '0') {

            }
            else {
                $('#CustomerName').attr('disabled', 'disabled');
            }
            
            ValidateTotal();
        }
       

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
                              SaveCustomerInvoice();

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
        $("#CustomerName").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/AWB/GetCustomerName',
                    datatype: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.CustomerName,
                                value: val.CustomerName,
                                CustomerId: val.CustomerID,
                                type: val.CustomerType
                            }
                        }))
                    }
                })
            },
            minLength: 1,
            autoFocus: false,
            focus: function (event, ui) {
                $("#CustomerName").val(ui.item.value);

                $('#CustomerID').val(ui.item.CustomerId);
            },
            select: function (e, i) {
                e.preventDefault();
                $("#CustomerName").val(i.item.label);

                $('#CustomerID').val(i.item.CustomerId);
            },

        });
        $("#CustomerName").change(function () {
            debugger;
            var customerid = $("#CustomerName").attr('CustomerId');
            var maxrow = $('#details > tbody > tr').length - 7;
            if (customerid == '' || customerid == 'undefined') {

            }
            else {
                if ($('#CustomerID').val() != customerid && maxrow > 0) {

                    Swal.fire("Customer Status", "Changing Customer name,please click refresh button!", "warning");
                    $('#btnsave').attr('disabled', 'disabled');
                }
                else {
                    $('#btnsave').removeAttr('disabled');
                }
            }

        })
    })

})(jQuery)