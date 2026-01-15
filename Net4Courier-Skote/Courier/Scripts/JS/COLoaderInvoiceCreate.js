
function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
 
function SaveCustomerInvoice() {
    debugger;
 
   
    var maxrow = $('#details > tbody > tr').length;
    var totalcharge = parseFloat($('#lbltotalcharge').html());
    if ($('#CustomerID').val() == '' || $('#CustomerID').val() == 0) {

        Swal.fire("Save Status", "Select customer to add Invoice!", "warning");
        $('#btnsave1').removeAttr('disabled');
        $('#CustomerName').focus();
        return false;
    }
    else if (maxrow == 0) {
        $('#btnsave1').removeAttr('disabled');

        Swal.fire("Save Status", "Select AWB Details to add Invoice!", "warning");
        return false;
    }
    else if (totalcharge == 0) {
        $('#btnsave1').removeAttr('disabled');
        Swal.fire("Save Status", "Select AWB Details to add Invoice!", "warning");

        return false;
    }
    
    $('#btnsave').attr('disabled', 'disabled');
    var RecPObj = {
        AgentInvoiceID:$('#AgentInvoiceID').val(),
        InvoiceNo :$('#InvoiceNo').val(),
         InvoiceDate:$('#InvoiceDate').val(),
        CustomerID:$('#CustomerID').val() ,
        //CustomerInvoiceTax:$('#CustomerInvoiceTax').val(),
        ChargeableWT:$('#ChargeableWT').val(),
        AdminPer:$('#AdminPer').val(),
        AdminAmt :$('#AdminAmt').val(),
        FuelPer :$('#FuelPer').val(),
        FuelAmt :$('#FuelAmt').val(),
        ClearingCharge: $('#ClearingCharge').val(),
        OtherCharge : $('#OtherCharge').val(),
        InvoiceTotal:$('#InvoiceTotal').val(),
        Discount :$('#Discount').val()

    }

    var Items = [];

    var itemcount = $('#details > tbody > tr').length;
    var idtext = 'hdnInscanID_';

    $('[id^=' + idtext + ']').each(function (index, item) {
        var awbchecked = $('#hdnawbchecked_' + index).prop('checked');
        if (awbchecked ==true) {
            var inscanid = $('#hdnInscanID_' + index).val();
            var ShipmentId = $('#hdnShipmentID_' + index).val();
            var awbno = $('#hdnAWBNo_' + index).val();
            var couriercharge= $('#hdnCourierCharge_' + index).val();
            var othercharge = $('#hdnOtherCharge_' + index).val();
            var totalcharge = $('#hdnTotalCharge_' + index).val();
            var vatamount = $('#hdnVatAmount_' + index).val();
            var surcharge = $('#hdnSurCharge_' + index).val();
            var surchargepercent = $("#hdnSurchargePercent_" + index).val();
            var customcharge= $('#hdnCustomCharge_' + index).val();
            var invoicedetaild = $('#hdnCustomerInvoiceDetailID_' + index).val();
                var data = {
                    AgentInvoiceDetailID: invoicedetaild,
                    AWBChecked: awbchecked,
                    ShipmentID: ShipmentId,
                    InscanID: inscanid,  
                    AWBNo :awbno,
                    CourierCharge: couriercharge,
                    OtherCharge: othercharge,
                    //VatAmount: vatamount,
                    //TotalCharge: totalcharge,
                    FuelSurcharge: surcharge,
                    //SurchargePercent: surchargepercent,
                    //CustomCharge: customcharge,
                    NetValue: totalcharge
                 
                }

                Items.push(data);
            
        } 
        if ((index+1) == (itemcount)) {
            $.ajax({
                type: "POST",
                url: "/COLoaderInvoice/SaveCustomerInvoice",
                datatype: "Json",
                data: { model: RecPObj, Details: JSON.stringify(Items) },
                success: function (response) {
                    if (response.status == "OK") {

                        Swal.fire("Save Status", response.message, "success");

                        setTimeout(function () {
                            location.href = '/COLoaderInvoice/Index';
                        }, 200)
                        //if ($('#AgentInvoiceID').val() == 0) {
                        //    setTimeout(function () {
                        //        location.href = '/COLoaderInvoice/Create?id=0';
                        //    }, 200)
                        //}
                        //else {
                        //    setTimeout(function () {
                        //        location.href = '/COLoaderInvoice/Index';
                        //    }, 200)

                        //}
                        

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
(function ($) {

    'use strict';
    function initformControl() {
        $('#Date').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $("#CustomerName").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/COLoaderInvoice/GetCustomerName',
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
                $('#CustomerId').val(ui.item.CustomerId);
                $('#CustomerID').val(ui.item.CustomerId);
            },
            select: function (e, i) {
                e.preventDefault();
                $("#CustomerName").val(i.item.label);
                $('#CustomerId').val(i.item.CustomerId);
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
                if ($   ('#CustomerID').val() != customerid && maxrow > 0) {
                 
                    Swal.fire("Customer Status", "Changing Customer name,please click refresh button!", "warning");
                    $('#btnsave1').attr('disabled', 'disabled');
                }
                else {
                    $('#btnsave1').removeAttr('disabled');
                }
            }

        })

        $('#btnsearch').click(function () {
            debugger;
            if ($('#CustomerID').val() == 0 || $('#CustomerID').val() == '' || $('#CustomerID').val() == null) {
                $('#CustomerName').focus();
                return false;
            }
            if ($('#FromDate').val() == null || $('#FromDate').val() == '') {
                $('#FromDate').focus();
                Swal.fire("Data Validation", "Select From Date!", "warning");
               
                return false;
            }
            if ($('#ToDate').val() == null || $('#ToDate').val() == '') {
                $('#ToDate').focus();
                Swal.fire("Data Validation", "Select To Date!", "warning");

                return false;
            }
            if ($('#InvoiceDate').val() == null || $('#InvoiceDate').val() == '') {
                $('#InvoiceDate').focus();
                Swal.fire("Data Validation", "Select Invoice Date!", "warning");
              
                return false;
            }
            var m1 = $('#FromDate').val().split('-')[1];
            var m2 = $('#ToDate').val().split('-')[1];
            var m3 = $('#InvoiceDate').val().split('-')[1];

            var y1 = $('#FromDate').val().split('-')[2];
            var y2 = $('#ToDate').val().split('-')[2];
            var y3 = $('#InvoiceDate').val().split('-')[2];

            if (parseInt(m1) != parseInt(m2) || parseInt(y1) != parseInt(y2)) {
                //$.notify('The transaction date should not overlap a month!', "error");
                Swal.fire("Data Validation", "The transaction date should not overlap a month!", "warning");
                $('#FromDate').focus();

                return false;
            }
            if (parseInt(m1) != parseInt(m3) || parseInt(y1) != parseInt(y3)) {
                //$.notify('Invoice Date should be in the same month of Transaction Date!', "error");
                Swal.fire("Data Validation", "Invoice Date should be in the same month of Transaction Date!!", "warning");
                $('#InvoiceDate').focus();
                return false;
            }
            $.ajax({
                type: 'POST',
                url: '/COLoaderInvoice/ShowItemList',
                datatype: "html",
                data: {
                    CustomerId: $('#CustomerID').val(), FromDate: $('#FromDate').val(), ToDate: $('#ToDate').val(), MAWB: $('#MAWB').val(), MovementId:$('#SelectedValues').val()
                },
                success: function (data) {
                    debugger;
                    $("#listContainer").html(data);
                    var max = $('#inputtable > tr').length;
                    var html = $('#inputtable > tr >td:first').html();
                    $('#TotalAWB').val(max);
                    if (html == 'No data available in table') {
                        
                        Swal.fire("Data Validation", "AWB are not found!", "warning");
                    }
                    ValidateTotal();
                }
            });

        });

        if ($('#AgentInvoiceID').val() > 0) {
            $('#btnsave').val('Update');
            $('#CustomerName').attr('disabled', 'disabled');
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

    })

})(jQuery)