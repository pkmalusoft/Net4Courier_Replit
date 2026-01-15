var _decimal = "2";
function funExportToPDF(id) {
    //showLoading();
    $.ajax({
        url: '/CreditNote/CreditNoteVoucher',
        type: "GET",
        data: { id: id },
        dataType: "json",
        success: function (response) {
            if (response.result == "ok") {
                $('#frmPDF').attr('src', "/ReportsPDF/" + response.path); //''

            setTimeout(function () {
    
                   window.open('/ReportsPDF/'  + response.path);    
             }, 500);
        } else {
            alert(resuponse.message);
            //hideLoading();
            }
            },
        });

return false;
    }
function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
function additem() {
    debugger;
    if ($('#AcDetailHeadID').val() == '' || $('#AcDetailHeadID').val() == 0) {
        $('#msg1').show();
        $('#msg1').html('Select Account Head');
        $('#AcDetailHeadID').focus();
        return false;
    }
    var Amount = 0;

    if ($('#TransType').val() == "CN") {
        Amount = $('#txtDebitAmount').val();
        if (Amount == '')
            Amount = 0;

        if (Amount == 0) {
            $('#msg1').show();
            $('#msg1').html('Enter Debit Amount');
            $('#txtDebitAmount').focus();
            return false;
        }
    }
    else {
        Amount = $('#txtCreditAmount').val();
        if (Amount == '')
            Amount = 0;

        if (Amount == 0) {
            $('#msg1').show();
            $('#msg1').html('Enter Credit Amount');
            $('#txtCreditAmount').focus();
            return false;
        }
    }
    $('#msg1').hide();
    var i = $('#details >tr').length;
    var acheadid = $('#AcDetailHeadID').val();
    var acheadname = $('#AcDetailHeadID option:selected').text();
    var remarks = $('#AcDetailRemarks').val();
   

    var debitamount = $('#txtDebitAmount').val();
    var creditamount = $('#txtCreditAmount').val();
    var html = '<tr><td>' + acheadname + '<input type="hidden" id="txtHeadID_' + i + '" value="' + acheadid + '" />';
    html += '<input type="hidden" id="txtRemarks_' + i + '" value="' + remarks + '" />';
    html += '<input type="hidden" id="txtDebitAmt_' + i + '" value="' + debitamount + '" />';
    html += '<input type="hidden" id="txtCreditAmt_' + i + '" value="' + creditamount + '" />';
    html += '<input type="hidden" id="txtAmt_' + i + '" value="' + Amount + '" />';                        
    html += '<input type="hidden" id="hdndeleted_' + i + '" value="false" /> </td>';                        
    
    html += '<td class="text-right">' + numberWithCommas(debitamount) + '</td>';                        
    html += '<td class="text-right">' + numberWithCommas(creditamount) + '</td>';                        
    html += '<td>' + remarks + '</td>';
    html += '<td><a class="text-danger" href="javascript:void(0)" onclick="Deleterow(' + i + ')" id="DeleteRow_' + i + '"><i class="mdi mdi-delete font-size-18"></i></a></td>';                   

    $('#details').append(html);
    $('#AcDetailHeadID').val(0).trigger('change');
    $("#AcDetailAmount").val(0);
    $('#AcDetailRemarks').val('');
    $('#txtDebitAmount').val('');
    $('#txtCreditAmount').val('')
    $('#AcDetailHeadID').focus();

    $('#TransType').attr('disabled', 'disabled');
    getTotal();
    ValidateTotal();
    //$.ajax({
    //    type: "POST",
    //    url: '/CreditNote/AddAccount/',
    //    datatype: "html",
    //    data: {
    //        'AcHeadID': $('#AcDetailHeadID').val(), 'Amount': Amount, Remarks: $('#AcDetailRemarks').val(), TransType: $('#TransType').val()
    //    },
    //    success: function (data) {
    //        $("#listContainer").html(data);
    //        $('#AcDetailHeadID').val(0).trigger('change');
    //        $("#AcDetailAmount").val(0);
    //        $('#AcDetailRemarks').val('');
    //        $('#txtDebitAmount').val('');
    //        $('#txtCreditAmount').val('')
    //        $('#AcDetailHeadID').focus();

    //        $('#TransType').attr('disabled','disabled');
             
    //    }
    //});
}

function Deleterow(index) {
    $('#hdndeleted_' + index).val('true');
    $('#hdndeleted_' + index).parent().parent().addClass('hide');
    
    getTotal();

    //$.ajax({
    //    type: "POST",
    //    url: '/CreditNote/DeleteAccount',
    //    datatype: "html",
    //    data: {
    //        'index': index, TransType:$('#TransType').val()
    //    },
    //    success: function (data) {
    //        $("#listContainer").html(data);
    //        $('#AcDetailHeadID').val(0).trigger('change');
    //        $("#AcDetailAmount").val(0);
    //        $('#AcDetailRemarks').val('');
    //        $('#txtDebitAmount').val('');
    //        $('#txtCreditAmount').val('')
    //        var amount = 0;
    //        if ($('#TransType').val() == "CN") {
    //            if ($('#lblcredittotal').html() == '')
    //                amount = 0;
    //            else
    //                amount = parseFloat($('#lblcredittotal').attr('val'));
    //        }
    //        else {
    //            if ($('#lbldebittotal').html() == '')
    //                amount = 0;
    //            else
    //                amount = parrseFloat($('#lbldebittotal').attr('val'));
    //        }

    //        if (amount == 0) {
    //            $('#TransType').attr('disabled', 'disabled');
    //        }
    //        else {
    //            $('#TransType').removeAttr('disabled');
    //        }            
    //        $('#AcDetailHeadID').focus();
            
    //    }
    //});
}

function getTotal() {
    debugger;
    var idtext = 'txtDebitAmt_';    
    var debittotal = 0;
    var credittotal = 0;
    $('#lbldebittotal').attr('val', credittotal);
    $('#lbldebittotal').html(numberWithCommas(credittotal));
    $('#lblcredittotal').attr('val', debittotal);
    $('#lblcredittotal').html(numberWithCommas(debittotal));
    $('[id^=' + idtext + ']').each(function (index, item) {

        var deleted = $('#hdndeleted_' + index).val();
        amount = 0;
        if (deleted != 'true') {
            var headid = $('#txtHeadID_' + index).val();
            var remarks = $('#txtRemarks_' + index).val();
            amount = 0;
            amount = $("#txtDebitAmt_" + index).val();
            if (amount == '')
                amount = 0;
            debittotal = parseFloat(debittotal) + parseFloat(amount);
            $('#lblcredittotal').attr('val', debittotal);
            $('#lblcredittotal').html(numberWithCommas(parseFloat(debittotal).toFixed(2)));

            amount = 0;
            amount = $("#txtCreditAmt_" + index).val();
            if (amount == '')
                amount = 0;

            credittotal = parseFloat(credittotal) + parseFloat(amount);            
            $('#lbldebittotal').attr('val', credittotal);
            $('#lbldebittotal').html(numberWithCommas(parseFloat(credittotal).toFixed(2)));
            
        }

    });

}
function SaveCreditNote() {
    debugger;
  
    var amount = 0;
    if ($('#TransType').val() == "CN") {
        if ($('#lblcredittotal').html() == '')
            amount = 0;
        else
            amount = parseFloat($('#lblcredittotal').attr('val'));

        var allocated = $('#spantotalallocate').html();
        if (parseFloat(allocated) > parseFloat(amount)) {
            Swal.fire('Data Validation', 'Invoice Allocation should not be excess than Credit Not Amount!', 'error');
            return false;
        }
    }
    else {
        if ($('#lbldebittotal').html() == '')
            amount = 0;
        else
            amount = parseFloat($('#lbldebittotal').attr('val'));
    }

    if (amount == 0) {
        $('#msg1').show();
        $('#msg1').html("Invalid Transactions!");
        return false;
    }
    $('#btnsave').attr('disabled', 'disabled');
    var RecPObj = {
        CreditNoteID: $('#CreditNoteID').val(),
        CreditNoteNo: $('#CreditNoteNo').val(),
        Date: $('#Date').val(),
        CustomerID: $('#CustomerID').val(),
        TransType: $('#TransType').val(),
        InvoiceType:$('#InvoiceType').val(),
        ReferenceType: $('#ReferenceType').val(),
        InvoiceID: $('#InvoiceID').val(),
        AcHeadID: $('#AcHeadID').val(),
        Amount: amount,
        Description: $('#Description').val(),
        CustomerType:$('#CustomerType').val()
    }

    var Items = [];

    var itemcount = $('#details > tr').length;
    var allocateitemcount = $('#tbl1 > tr').length;
    var idtext = 'txtAmt_';    

    $('[id^=' + idtext + ']').each(function (index, item) {
        var deleted = $('#hdndeleted_' + index).val();
        if (deleted != 'true') {
            var headid = $('#txtHeadID_' + index).val();
            var remarks = $('#txtRemarks_' + index).val();
            var amount = $("#txtAmt_" + index).val();
            if (amount > 0) {
                var data = {
                    CreditNoteDetailID: 0,
                    AcHeadID: headid,
                    Amount: amount,
                    Remarks: remarks
                }

                Items.push(data);
            }
        }
        if ((index + 1) == itemcount && allocateitemcount == 0) {
            $.ajax({
                type: "POST",
                url: "/CreditNote/SaveCreditNote",
                datatype: "Json",
                data: { v: RecPObj, Details: JSON.stringify(Items), Allocation: '' },
                success: function (response) {
                    if (response.status == "OK") {

                        Swal.fire("Save Status", response.message, "success");
                        setTimeout(function () {
                            if ($('#CreditNoteID').val() == 0) {
                                location.href = '/CreditNote/Create?id=0';
                            }
                            else {
                                location.href = '/CreditNote/Create?id=' + response.CreditNoteID;
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
        else if ((index + 1) == itemcount && allocateitemcount > 0) {
            SaveEntry(RecPObj, Items);
        }
    });
}
function SaveEntry(RecPObj, Items) {
    debugger;
    var itemcount = $('#tbl1 > tr').length;
    var idtext = 'txtamount_';
    var AllocateItems = [];
    $('[id^=' + idtext + ']').each(function (index, item) {
        var checked = $('#chkallocate_' + index).prop('checked');
        var amount = $("#txtamount_" + index).val();
        if (checked == true && parseFloat(amount) > 0) {
            var InvoiceID = $('#hdnInvoiceId_' + index).val();
            var InvoiceType = $('#hdnInvoiceType_' + index).val();
            var AcOPInvoiceDetailID = $('#hdnAcOPInvoiceDetailID_' + index).val();

            var InvoiceNo = $('#hdnInvoiceNo_' + index).val();
            var InvoiceDate = $('#hdnInvoiceDate_' + index).val();

            var data = {
                InvoiceID: InvoiceID,
                AcOPInvoiceDetailID: AcOPInvoiceDetailID,
                InvoiceType: InvoiceType,
                InvoiceNo: InvoiceNo,
                InvoiceDate: InvoiceDate,
                Amount: amount
            }

            AllocateItems.push(data);

        }
        if ((index + 1) == itemcount) {
            $.ajax({
                type: "POST",
                url: "/CreditNote/SaveCreditNote",
                datatype: "Json",
                data: { v: RecPObj, Details: JSON.stringify(Items), Allocation: JSON.stringify(AllocateItems) },
                success: function (response) {
                    if (response.status == "OK") {

                        Swal.fire("Save Status", response.message, "success");
                        setTimeout(function () {
                            if ($('#CreditNoteID').val() == 0) {
                                location.href = '/CreditNote/Create?id=0';
                            }
                            else {
                                location.href = '/CreditNote/Create?id=' + response.CreditNoteID;
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
function allocate(obj) {

    var balamt = $('#hdnAdvance').val();
    if (balamt == "")
        balamt = "0";

    var idindex = $(obj).attr('id').split('_')[1];
    var txinvoice = $('#txtamount_' + idindex);
    if (parseFloat(balamt) > 0) {

        var idtext = 'txtamount_' + idindex;

        var balance = $('#txtbalance_' + idindex).val();
        if (balance == "") {
            balance = 0;
        }
        var adjust = 0;
            
        if (parseFloat(balance) > 0 && parseFloat(balamt) > parseFloat(balance)) {
            balance = parseFloat(balance) - parseFloat(adjust);
        }
        else {
            balance = parseFloat(balamt); //parseFloat(balance) - parseFloat(adjust);
        }
        if ($(obj).is(':checked')) {
            $(txinvoice).val(parseFloat(balance).toFixed(_decimal));
        }
        else {
            $(txinvoice).val(parseFloat("0").toFixed(_decimal));
        }
        ValidateTotal();
    }
    else {
        $('#chkallocate_' + idindex).prop('checked', false);
        $(txinvoice).val(parseFloat("0").toFixed(_decimal));
        ValidateTotal();
    }
}
function ValidateTotal() {
    debugger;
    var TotalAmount = 0;
    if ($('#TransType').val() == "CN") {
        if ($('#lblcredittotal').html() == '')
            TotalAmount = 0;
        else
            TotalAmount = parseFloat($('#lblcredittotal').attr('val'));
    }
    else {
        if ($('#lbldebittotal').html() == '')
            TotalAmount = 0;
        else
            TotalAmount = parseFloat($('#lbldebittotal').attr('val'));
    }
    var idtext = 'txtamount_';
    var amt = 0;

    if (parseFloat(TotalAmount > 0)) {
        $('#btnsearch').removeAttr('disabled');
    }
    //if ($('#Balance').val() == "") {
    //    $('#Balance').val(0);
    //}
    //var balance =parseFloat($('#Balance').val());
    var totalallocate = 0;
    var totaloutstanding = 0;
    var totaladjust = 0;
    $('[id^=' + idtext + ']').each(function (index, item) {
        var id = $(item).attr('id').split('_')[1];
        if ($('#chkallocate_' + id).prop('checked')) {
            if ($(item).val() == "" || $(item).val() == null) {
                $(item).val(0);
            }
            var itemval = $(item).val();
            itemval = itemval.replace(',', '');

            var paidamount = parseFloat(itemval);
            totalallocate = parseFloat(totalallocate) + parseFloat(paidamount);
        }
        
        $('#spantotalallocate').html(parseFloat(totalallocate).toFixed(2));
        
        $('#hdnAdvance').val(parseFloat(TotalAmount) - parseFloat(totalallocate));
        $('#h5Advance').html('Advance : ' + parseFloat($('#hdnAdvance').val()).toFixed(_decimal))
    //if (parseFloat(amt) == 0) {
    //    $('#btnsave').attr('disabled', 'disabled');
    //}



    var alloamt = numberWithCommas(parseFloat(amt).toFixed(_decimal));
    var balance1 = parseFloat(TotalAmount) - parseFloat(amt);
    $('#BalanceAmount').val(parseFloat(balance1).toFixed(_decimal));
    $('#AllocatedAmount').val(alloamt);
    var payingamount = parseFloat($('#FMoney').val());
    var allocatedamount = parseFloat(amt).toFixed(_decimal);
    var advance = 0;
    if (parseFloat($("#FMoney").val()) == 0) {
        //$("#FMoney").val(parseFloat(wt).toFixed(@_decimal));
        $('#msg1').show();
        $('#msg1').text('Received Amount Required!');
        $('#btnsave').attr('disabled', 'disabled');
    }
    else if (allocatedamount > 0 && parseFloat(TotalAmount) < allocatedamount) {
        $('#btnsave').attr('disabled', 'disabled');
        $('#msg1').show();
        $('#msg1').text('Allocation amount should not be more than Received amount!');
    }
    else {
        $('#btnsave').removeAttr('disabled', 'disabled');
        //$("#FMoney").val(parseFloat(wt).toFixed(@_decimal));
        $('#msg1').hide();
        $('#msg1').text('');
    }

});


    }
function receiptallocation() {
    var id = $('#CreditNoteID').val();
    $.ajax({
        type: "POST",
        url: '/ReceiptPaymentAllocation/ShowReceiptAllocation/',
        datatype: "html",
        data: { id: id, EntryType: "CN" },
        success: function (response) {
            debugger;
            $('#allocationcontainer').html(response);
            var autoallocate = true;
            var TotalAmount = 0;
            var recpayid = $("#RecPayID").val();
            if (autoallocate == true)
                TotalAmount = parseFloat($('#lbldebittotal').html()).toFixed(3);
            if ($('#CustomerID').val() == 0) {
                alert('Select Customer!');
                return;
            }

            if ($('#FMoney').val() == "") {// || $('#FMoney').val()=="0") {
                //alert('Enter Receipt Amount!');
                $('#FMoney').val(0);
            }
            var ID = $('#CustomerID').val();
            $.ajax({
                type: "POST",
                url: '/ReceiptPaymentAllocation/GetReceiptAllocated/',
                data: {
                    'ID': ID, 'amountreceived': parseFloat(TotalAmount), 'RecPayId': recpayid, 'RecPayType': "CR", 'EntryType': "CN"
                },
                success: function (response) {
                    debugger;
                    var data = response.salesinvoice;
                    var advance = response.advance;
                    $('#hdnAdvance').val(advance);
                    $('#tbl1').html('');
                    if (data.length == 0) {
                        $('#BalanceAmount').val(parseFloat(advance).toFixed(2));
                    }

                    var amt = 0;
                    for (var i = 0; i < data.length; i++) {
                        var date = new Date(data[i].date);
                        amt = parseFloat(amt) + parseFloat(data[i].Amount);
                        var tempdate = new Date(date).getDate() + '/' + (new Date(date).getMonth() + 1) + '/' + new Date(date).getFullYear();
                        var invoiceno = "'" + data[i].InvoiceNo + "'";
                        html = '<tr>' +
                            '<td><a href="JavaScript:void(0)" onclick="ShowInvoiceReceipts(' + invoiceno + ')" >' + data[i].InvoiceNo + '</a><input type="hidden" id="hdnInvoiceType_' + i + '"  value="' + data[i].InvoiceType + '" /><input id="hdnCreditNoteId"   value=' + data[i].CreditNoteID + ' type="hidden">  <input id="hdnAcOPInvoiceDetailID" value=' + data[i].AcOPInvoiceDetailID + ' type="hidden"><input id="hdnInvoiceId_' + i + '"    value=' + data[i].SalesInvoiceID + ' type="hidden"></td>' +
                            '<td>' + data[i].DateTime + '<input id="" name="CustomerRcieptChildVM[' + i + '].InvoiceDate" value=' + data[i].DateTime + ' type="hidden"></td>' +
                            '<td class="text-right">' + parseFloat(data[i].InvoiceAmount).toFixed(_decimal) + '</td>' +
                            '<td class="text-right">' + parseFloat(data[i].AmountReceived).toFixed(_decimal) + '</td>' +
                            '<td class="text-right">' + (parseFloat(data[i].InvoiceAmount) - parseFloat(data[i].AmountReceived)).toFixed(_decimal) + '</td>' +
                            '<td class="text-right">' + parseFloat(data[i].Balance).toFixed(_decimal) + '</td>' +
                            '<td class="text-right">' + parseFloat(data[i].AdjustmentAmount).toFixed(_decimal) + '</td>' +
                            '<td class="text-right">' + '<a href="#" onclick="DeleteAllocation(' + data[i].RecPayDetailID + ')" class="text-danger"><i class="mdi mdi-delete font-size-18 text-danger"></i></a></td>';
                        html = html + '</tr>';

                        $('#tbl1').append(html);


                    }

                }
            });
        }
    })
}
(function ($) {

    'use strict';
    function initformControl() {
        $('#Date').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $('#CustomerName').change(function () {
            if ($('#TransType').val() != '' && $('#ReferenceType').val() == "1") {
                var ID = $('#CustomerID').val();
                if (ID > 0) {
                    $.ajax({
                        type: "POST",
                        url: '/CreditNote/GetTradeInvoiceOfCustomer/' + ID,
                        data: { 'ID': ID, 'amountreceived': 0, TransType: $('#TransType').val() },
                        success: function (data) {
                            if (data.length == 0) {
                                alert("There is no Item!");
                                $('#InvoiceNo').val('');
                                $('#InvoiceNo').attr('disabled', 'disabled');
                            }
                            else {
                                $('#InvoiceNo').removeAttr('disabled');
                            }

                        }
                    });
                }
            }
        })
        $("#CustomerName").autocomplete({
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

        $('#CustomerType').change(function () {
            
                $.ajax({
                    type: "GET",
                    url: '/CreditNote/GetCustomerTypeAccount/',
                    data: { 'Type': $('#CustomerType').val() },
                    success: function (data) {
                        console.log(data);
                        $('#lblAcHeadName').html(data.AcHeadName);
                        $('#AcHeadID').val(data.AcHeadID);
                      }
        });
            
        });

        $('#TransType').change(function () {
            if ($('#CreditNoteID').val() == 0) {
                $.ajax({
                    type: "GET",
                    url: '/CreditNote/GetCreditNoteNo/',
                    data: { 'Type': $('#TransType').val() },
                    success: function (data) {
                        $('#CreditNoteNo').val(data);

                    }
                });
            }
        });

        setTimeout(function () {
            if ($('#CreditNoteID').val() > 0) {
                $("#CustomerName").attr('disabled', 'disabled');
                $('#TransType').trigger('change');
                $('#TransType').attr('disabled', 'disabled');
                $('#ReferenceType').attr('disabled', 'disabled');
               
                $("#ReferenceType").val($("#ReferenceType").attr('svalue')).trigger('change');
                 
                $('#btnsave').val('Update');
                $('#divothermenu').removeClass('hide');
                $('#divothermenu1').removeClass('hide');
                if ($('#TransType').val() == 'CN') {
                    $('#lblaccount').html('Chart of Account - Credit');
                    $('#lblaccount1').html('Chart of Account - Debit');
                    $('#lblamount').html('Credit Amount');
                    $('#lblamount1').html('Debit Amount');
                    
                    $('#txtCreditAmount').attr('readonly', 'readonly');
                    $('#txtDebitAmount').removeAttr('readonly');
                   
                }
                else if ($('#TransType').val() == 'CJ') {
                    $('#lblaccount').html('Chart of Account - Debit');
                    $('#lblaccount1').html('Chart of Account - Credit');
                    $('#lblamount').html('Debit Amount');
                    $('#lblamount1').html('Credit Amount');
                    $('#lblchkInvoice').html('For Recpt');
                    $('#lblInvoice').html('Receipt No.');
                    $('#lblInvoiceDate').html('Receipt Date');
                    $('#lblInvoiceAmt').html('Receipt Amount')
                    $('#lblReceivedAmt').html('Settled Amount');
                    $('#AmountType').val(1).trigger('change');
                    $('#txtDebitAmount').attr('readonly', 'readonly');
                    $('#txtCreditAmount').removeAttr('readonly');
                    $('#AcDetailAmountType').val(0).trigger('change');
                    
                    
                }

                if ($('#ReferenceType').val() == '1') {

                    $('#lblInvoice').html('Invoice No.');
                    $('#lblInvoiceDate').html('Invoice Date');
                    $('#lblInvoiceAmt').html('Invoice Amount')
                    $('#lblReceivedAmt').html('OutStanding Amount');

                }
                else if ($('#ReferenceType').val() == '2') {

                    $('#lblInvoice').html('Receipt No.');
                    $('#lblInvoiceDate').html('Receipt Date');
                    $('#lblInvoiceAmt').html('Receipt Amount')
                    $('#lblReceivedAmt').html('Settled Amount');

                }
            }
            else {
                $('#InvoiceNo').attr('disabled', 'disabled');
                $('#InvoiceDate').attr('readonly', 'readonly');
                $('#TransType').val('CN').trigger('change');
                $("#ReferenceType").trigger('change');
                $('#divothermenu').addClass('hide');
                $('#divothermenu1').addClass('hide');
            }
        },100)
      
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
                            SaveCreditNote();

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