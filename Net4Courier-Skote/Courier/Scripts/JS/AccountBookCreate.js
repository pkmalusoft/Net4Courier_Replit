function SaveAccount() {
    var itemcount = $('#details >  tr').length;
    var vouchervalue = 0;
    for (var i = 0; i < itemcount; i++) {
        if ($('#hdndeleted_' + i).val() != 'true' && $('#hdndeleted_' + i).val() != true) {
            vouchervalue = parseFloat(vouchervalue) + parseFloat($('#JAcAmount_' + i).val());
        }
    }

    if (vouchervalue == 0) {
        Swal.fire('Data Validation', 'Invalid Transaction,Enter Account and Amount', 'error');
        return false;
    }

    if ($('#paytype').val() >1) { //bank entry 
        if ($('#chequeno').val() == '') {
            Swal.fire('Data Validation', 'Enter Cheque No.', 'error');
            $('#chequeno').focus();
            return false;
        }
        if ($('#chequedate').val() == '') {
            
            $('#chequedate').val($('#transdate').val());
            
        }
    }


        $('#btnSaveAccounts').attr('disabled', 'disabled');
        var accountobj = {
            AcJournalID: $('#AcJournalID').val(),
            SelectedAcHead: $('#SelectedAcHead').val(),
            paytype: $('#paytype').val(),
            chequeno: $('#chequeno').val(),
            chequedate: $('#chequedate').val(),
            transtype: $('#transtype').val(),
            bankname: $('#bankname').val(),
            transdate: $('#transdate').val(),
            TransactionType: $('#TransactionType').val(),
            remarks: $('#remark1').val(),
            reference: $('#reference').val(),
            PartyName: $('#partyname').val(),
            AcBankDetailID: $('#AcBankDetailID').val()
        }
       
        var obj = [];
        for (var i = 0; i < itemcount; i++) {

            var item = {
                ID: $('#hdnAcJournalDetID_' + i).val(),
                IsDeleted: $('#hdndeleted_' + i).val(),
                AcHeadID: $('#JAcHead_' + i).val(),
                Rem: $('#editremark_' + i).val(),
                Amt: $('#JAcAmount_' + i).val(),
                TaxPercent: $('#JAcTaxPercent_' + i).val(),
                TaxAmount: $('#JAcTaxAmount_' + i).val(),
                AmountIncludingTax: $('#ChkAmountIncludingTax_' + i).prop('checked')
            }
            obj.push(item);
            if (itemcount == (i + 1)) {
                $.ajax({
                    type: "POST",
                    url: '/Accounts/SaveAcBook/',
                    datatype: "json",
                    data: { v: accountobj, Details: JSON.stringify(obj) },
                    success: function (response) {
                        debugger;
                        if (response.status == "OK") {
                            Swal.fire("Save Status!", response.message, "success");
                            //$('#divothermenu').removeClass('hide');
                            $('#btnSaveAccounts').removeAttr('disabled');
                            var t = document.getElementsByClassName("needs-validation");
                            $(t).removeClass('was-validated');
                            if ($('#AcJournalID').val() == 0) {
                                window.location.href = '/Accounts/CreateAcBook?id=0';
                            }
                            else {
                                window.location.href = '/Accounts/IndexAcBook';
                            }

                        }
                        else {
                            $('#btnSaveAccounts').removeAttr('disabled');
                            Swal.fire("Save Status!", response.message, "warning");
                            //window.location.reload();
                        }


                    }
                });
            }
        }
   
}
function ValidateTotal() {
    debugger;

    var idtext = 'JAcAmount_';
    var amt = 0;
    var totalamount = 0;
    $('#spanTotal').html(0);
    $('#spanTaxTotal').html(0);
    $('[id^=' + idtext + ']').each(function (index, item) {
        var hdndel = $(item).parent().parent().find('.hdndeleted');
        if ($(hdndel).val() != 'true') {

            if ($(item).val() == "" || $(item).val() == null) {
                $(item).val(0);
            }
            var itemval = $(item).val();
            itemval = itemval.replace(',', '');

            var paidamount = parseFloat(itemval);
            amt = parseFloat(amt) + parseFloat(paidamount);
            $('#spanTotal').html(parseFloat(amt).toFixed(2));
            totalamount = parseFloat($('#spanTaxTotal').html()) + parseFloat($('#spanTotal').html());
            
            $('#TotalAmt').val(parseFloat(totalamount).toFixed(2));
        }
    });

    //JAcTaxAmount_
    idtext = 'JAcTaxAmount_';
    amt = 0;
    $('[id^=' + idtext + ']').each(function (index, item) {
        var hdndel = $(item).parent().parent().find('.hdndeleted');
        if ($(hdndel).val() != 'true') {
            if ($(item).val() == "" || $(item).val() == null) {
                $(item).val(0);
            }
            var itemval = $(item).val();
            itemval = itemval.replace(',', '');

            var paidamount = parseFloat(itemval);
            amt = parseFloat(amt) + parseFloat(paidamount);
            $('#spanTaxTotal').html(parseFloat(amt).toFixed(2));
            totalamount = parseFloat($('#spanTaxTotal').html()) + parseFloat($('#spanTotal').html());
         
            $('#TotalAmt').val(parseFloat(totalamount).toFixed(2));
        }
    });
    
}

function DeleteTrans(obj,index) {
    $(obj).parent().parent().addClass('hide');
    //var obj1 = $(this).parent().parent().find('.hdndeleted');
    $('#hdndeleted_' + index).val(true);
    //$(obj1).val(true);
    ValidateTotal();
}
function GetMaxVoucherNo() {
    var vouchertype = $('#TransactionType').val();
            $.ajax({
                    type: "POST",
                    url: "/Accounts/GetMaxVoucherNo",
                    datatype: "Json",
                    data: { VoucherType:vouchertype },
                    success: function (response) {
                        if (response.status == "OK") {
                            $('#VoucherNo').val(response.VoucherNo);
                        }
                        else {
                            $('#VoucherNo').val('Unknown');
                        }
                    }
                });
}


(function ($) {

    'use strict';
    function initformControl() {
        var _decimal = "2";
        var accounturl = '/Accounts/GetHeadsForCash';
        //$('#transdate').datepicker({
        //    dateFormat: 'dd-mm-yy'
        //});
        $('#transdate').change(function () {
            $('#chequedate').val($('#transdate').val());
        })
        $("#chequedate").datepicker({
            dateFormat: 'dd-mm-yy'
        });
        $("#ReceivedFrom").keydown(function (event) {
            if (event.keyCode == 9) {
                if ($("#ReceivedFrom").val().length == 0) {
                    event.preventDefault();
                    $("#btnsave").focus();
                }
            }
        });

        $("#ReceivedFrom").change(function () {
            if ($("#ReceivedFrom").val().trim() == '') {
                $('#ReceivedFrom').val('');
                $('#SelectedReceivedFrom').val(0);
                $('#SelectedReceivedFrom').attr('label', '')
            }
            else if ($("#ReceivedFrom").val() != $("#SelectedReceivedFrom").attr('label')) {
                $('#ReceivedFrom').val('');
                $('#SelectedReceivedFrom').val(0);
                $('#SelectedReceivedFrom').attr('label', '')
                $('#ReceivedFrom').focus();
            }
        });

        $("#bankname").prop('disabled', true);
        $("#chequeno").prop('disabled', true);
        $("#chequedate").prop('disabled', true);
        //$("#partyname").prop('disabled', true);

        Bankdetails_enable();

        function Bankdetails_enable() {

            var rcbpaytype = $("#transtype option:selected").val();
            var rcbType = $("#paytype option:selected").val();
            if (rcbpaytype == "1" && rcbType == "1") {
                $("#TransactionType").val("CBR");
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                $("#bankname").prop('disabled', true);
                //$("#partyname").prop('disabled', true);

                //$("#chequeno").val('');
                //$("#bankname").val('');
                //$("#partyname").val('');
            }
            else if (rcbpaytype == "1" && rcbType > 1) {
                $("#TransactionType").val("BKR");
                $("#bankname").prop('disabled', false);
                //$("#partyname").prop('disabled', false);
                $("#chequeno").prop('disabled', false);
                $("#chequedate").prop('disabled', false);
            }
            else if (rcbpaytype == "2" && rcbType == "1") {
                $("#TransactionType").val("CBP");
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                $("#bankname").prop('disabled', true);

                //$("#partyname").prop('disabled', true);
                //$("#chequeno").val('');
                //$("#bankname").val('');
                //$("#partyname").val('')
            }
            else if (rcbpaytype == "2" && rcbType > 1) {
                $("#TransactionType").val("BKP");
                $("#bankname").prop('disabled', false);

                //$("#partyname").prop('disabled', true);
                $("#chequeno").prop('disabled', false);
                $("#chequedate").prop('disabled', false);
                //$("#bankname").val('');
                //$("#partyname").val('');
            } else {
                $("#bankname").prop('disabled', true);
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                //$("#partyname").prop('disabled', true);
            }
        }
        $("#transtype").change(function () {
            var v = $("#transtype option:selected").val();
            if (v == "1") {
                $(".label1").html("Received From");
                $(".label2").html("Credit Account");
            }
            else {
                $(".label1").html("Paid To");
                $(".label2").html("Debit Account");
            }

            var rcbpaytype = $("#transtype option:selected").val();
            var rcbType = $("#paytype option:selected").val();
            if (rcbpaytype == "1") //Receipt
            {
                $('#lblcheckingaccount').html('Debit Account');
                $('#lblcheckingamount1').html('Debit Amount');
                $('#lblcheckingamount2').html('Credit Amount');
                $('#lblcheckingaccount').removeClass('text-danger')
                $('#lblcheckingaccount').addClass('text-primary');
                $('#lblcheckingamount1').removeClass('text-danger');
                $('#lblcheckingamount1').addClass('text-primary');
                $('#lblcheckingamount2').removeClass('text-primary')
                $('#lblcheckingamount2').addClass('text-danger');
            }
            else if (rcbpaytype == "2") {
                $('#lblcheckingaccount').html('Credit Account');
                $('#lblcheckingamount1').html('Credit Amount');
                $('#lblcheckingamount2').html('Debit Amount');
                $('#lblcheckingaccount').removeClass('text-primary')
                $('#lblcheckingaccount').addClass('text-danger');

                $('#lblcheckingamount1').removeClass('text-primary');
                $('#lblcheckingamount1').addClass('text-danger')

                $('#lblcheckingamount2').removeClass('text-danger');
                $('#lblcheckingamount2').addClass('text-primary');
            }
            if (rcbpaytype == "1" && rcbType == "1") {
                $("#TransactionType").val("CBR");
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                $("#bankname").prop('disabled', true);

                //$("#partyname").prop('disabled', true);
                $("#chequeno").val('');
                $("#bankname").val('');
                //$("#partyname").val('');
            }
            else if (rcbpaytype == "1" && rcbType > 1) {
                $("#TransactionType").val("BKR");
                $("#bankname").prop('disabled', false);

                // $("#partyname").prop('disabled', false);
                $("#chequeno").prop('disabled', false);
                $("#chequedate").prop('disabled', false);

            }
            else if (rcbpaytype == "2" && rcbType == "1") {
                $("#TransactionType").val("CBP");
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                $("#bankname").prop('disabled', true);

                //$("#partyname").prop('disabled', true);
                $("#chequeno").val('');
                $("#bankname").val('');
                //$("#partyname").val('')
            }
            else if (rcbpaytype == "2" && rcbType > 1) {
                $("#TransactionType").val("BKP");
                $("#bankname").prop('disabled', true);

                // $("#partyname").prop('disabled', true);
                $("#chequeno").prop('disabled', false);
                $("#chequedate").prop('disabled', false);
                $("#bankname").val('');
                //$("#partyname").val('');
            }

            GetMaxVoucherNo();
        });


        $("#paytype").change(function () {

            var v = $("#paytype option:selected").val();
            if (v == "1") {
                accounturl = '/Accounts/GetHeadsForCash';
                //$.ajax({
                //    type: "POST",
                //    url: "/Accounts/GetHeadsForCash",
                //    datatype: "Json",
                //    success: function (data) {
                //        $("#AcHead").empty();

                //        $.each(data, function (index, value) {
                //            $('#AcHead').append('<option value="' + value.AcHeadID + '">' + value.AcHead + '</option>');
                //        });

                //    }
                //});
            }
            else {
                accounturl = '/Accounts/GetHeadsForBank';
                //$.ajax({
                //    type: "POST",
                //    url: "/Accounts/GetHeadsForBank",
                //    datatype: "Json",
                //    success: function (data) {
                //        $("#AcHead").empty();

                //        $.each(data, function (index, value) {
                //            $('#AcHead').append('<option value="' + value.AcHeadID + '">' + value.AcHead + '</option>');
                //        });

                //    }
                //});
            }


            var rcbpaytype = $("#transtype option:selected").val();
            var rcbType = $("#paytype option:selected").val();

            if (rcbpaytype == "1" && rcbType == "1") {
                $("#TransactionType").val("CBR");
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                $("#bankname").prop('disabled', true);

                //$("#partyname").prop('disabled', true);
                $("#chequeno").val('');
                $("#bankname").val('');
                //$("#partyname").val('');
            }
            else if (rcbpaytype == "1" && rcbType > 1) {
                $("#TransactionType").val("BKR");
                $("#bankname").prop('disabled', false);

                //$("#partyname").prop('disabled', false);
                $("#chequeno").prop('disabled', false);
                $("#chequedate").prop('disabled', false);
            }
            else if (rcbpaytype == "2" && rcbType == "1") {
                $("#TransactionType").val("CBP");
                $("#chequeno").prop('disabled', true);
                $("#chequedate").prop('disabled', true);
                $("#bankname").prop('disabled', true);

                //$("#partyname").prop('disabled', true);
                $("#chequeno").val('');
                $("#bankname").val('');
                //$("#partyname").val('')
            }
            else if (rcbpaytype == "2" && rcbType > 1) {
                $("#TransactionType").val("BKP");
                $("#bankname").prop('disabled', true);

                //$("#partyname").prop('disabled', true);
                $("#chequeno").prop('disabled', false);
                $("#chequedate").prop('disabled', false);
                $("#bankname").val('');
                //$("#partyname").val('');
            }
            GetMaxVoucherNo();
        });


        $("#AmountIncludingTax").click(function () {
            if ($("#AmountIncludingTax").is(':checked')) {
                if ($("#TaxPercent").val() > 0 && $("#amount").val() > 0) {
                    var amount = parseFloat($("#amount").val());
                    var taxpercent = parseFloat($("#TaxPercent").val()) / 100.00;
                    var taxamount = amount - (amount / (1 + taxpercent));
                    $("#TaxAmount").val(taxamount.toFixed(_decimal));
                }
                else {
                    $("#TaxAmount").val(0);
                }
            }
            else {
                if ($("#TaxPercent").val() > 0 && $("#amount").val() > 0) {
                    var tamount = ($("#amount").val() * $("#TaxPercent").val()) / 100.00;
                    $("#TaxAmount").val(tamount.toFixed(_decimal));
                }
                else {
                    $("#TaxAmount").val(0);
                }
            }
            ValidateTotal();
        });
        $("#amount").change(function () {
            if ($("#AmountIncludingTax").is(':checked')) {
                if ($("#TaxPercent").val() > 0 && $("#amount").val() > 0) {
                    var amount = parseFloat($("#amount").val());
                    var taxpercent = parseFloat($("#TaxPercent").val()) / 100.00;
                    var taxamount = amount - (amount / (1 + taxpercent));
                    $("#TaxAmount").val(taxamount.toFixed(_decimal));
                }
                else {
                    $("#TaxAmount").val(0);
                }
            }
            else {
                if ($("#TaxPercent").val() > 0 && $("#amount").val() > 0) {
                    var tamount = ($("#amount").val() * $("#TaxPercent").val()) / 100.00;
                    $("#TaxAmount").val(tamount.toFixed(_decimal));
                }
                else {
                    $("#TaxAmount").val(0);
                }
            }
        });

        $("#TaxPercent").change(function () {
            if ($("#AmountIncludingTax").is(':checked')) {
                if ($("#TaxPercent").val() > 0 && $("#amount").val() > 0) {
                    var amount = parseFloat($("#amount").val());
                    var taxpercent = parseFloat($("#TaxPercent").val()) / 100.00;
                    var taxamount = amount - (amount / (1 + taxpercent));
                    $("#TaxAmount").val(taxamount.toFixed(2));
                }
                else {
                    $("#TaxAmount").val(0);
                }
            }
            else {
                if ($("#TaxPercent").val() > 0 && $("#amount").val() > 0) {
                    var tamount = ($("#amount").val() * $("#TaxPercent").val()) / 100.00;
                    $("#TaxAmount").val(tamount.toFixed(_decimal));
                }
                else {
                    $("#TaxAmount").val(0);
                }
            }
            ValidateTotal();
        });


        $("#btnAdd").click(function () {
            debugger;
            if ($("#SelectedReceivedFrom").val() == "" || $("#SelectedReceivedFrom").val() == "0" || $("#ReceivedFrom").val() == "") {
                alert('Please Select "Received From"');
                $("#ReceivedFrom").focus();
                return false;
            }
            else if ($("#ReceivedFrom option:selected").val() == "") {
                alert("Please Select Head");
                $("#ReceivedFrom").focus();
                return false;
            }
            else if ($("#amount").val() == "") {
                alert("Please Enter The Amount");
                $("#amount").focus();
                return false;
            }
            else if (parseInt($("#amount").val()) == 0) {
                alert("Please Enter The Amount");
                $("#amount").focus();
                return false;
            }
            else {
                var str = "";
                var totalamt = 0;

                if ($('#details > tr').length == 1) {
                    var emptyrow = $('#details > tr').html();
                    if (emptyrow.indexOf('No data available in table') >= 0) {
                        $('#details > tbody').html('');
                    }
                }

                var i = $('#details > tr').length; // - 1;


                var amounttaxcheck = '';
                var amounttaxcheck = '';
                if ($("#AmountIncludingTax").is(':checked')) {
                    amounttaxcheck = '<input type="checkbox" id="ChkAmountIncludingTax_' + i + '" value="true" checked  name="AcJDetailVM[' + i + '].AmountIncludingTax" />';
                }
                else {
                    amounttaxcheck = '<input type="checkbox" id="ChkAmountIncludingTax_' + i + '" value="false"  name="AcJDetailVM[' + i + '].AmountIncludingTax" />';
                }

                var objHtml = '<tr>' + '<td style="padding-left:10px"><input type = "hidden" value="false" id="hdndeleted_' + i + '" name="AcJDetailVM[' + i + '].IsDeleted" class="hdndeleted" /><input type="hidden" class=JAcHead id="JAcHead_' + i + '" value=' + $("#SelectedReceivedFrom").val() + ' name="AcJDetailVM[' + i + '].AcHeadID"><input type="text" class="form-control clsreceivedfrom" id="ReceivedFrom_' + i + '" value="' + $("#ReceivedFrom").val() + '" name="AcJDetailVM[' + i + '].AcHead"></td><td style="padding-left:10px">' + amounttaxcheck + '<input type="hidden" value=' + $("#AmountIncludingTax").is(':checked') + ' name="AcJDetailVM[' + i + '].AmountIncludingTax"></td><td class="textright" style="padding-right:10px"><input    id="JAcAmount_' + i + '" type="text" class="form-control JAcAmt text-right txtNum" value=' + parseFloat($("#amount").val()).toFixed(_decimal) + ' name="AcJDetailVM[' + i + '].Amt"></td>' + '<td class=textright style="padding-right:10px"><input type="text"    id="JAcTaxPercent_' + i + '" class="form-control JAcAmt textright txtNum" value=' + $("#TaxPercent").val() + ' name="AcJDetailVM[' + i + '].TaxPercent"></td>' + '<td class="textright" style="padding-right:10px"><input type="text" class="JAcAmt textright form-control txtNum" readonly="readonly" id="JAcTaxAmount_' + i + '" value=' + parseFloat($("#TaxAmount").val()).toFixed(3) + ' name="AcJDetailVM[' + i + '].TaxAmount"></td>';
                objHtml = objHtml + '<td><input type="text" class="form-control" id="editremark_' + i + '" value="' + $("#remarks").val() + '"  name="AcJDetailVM[' + i + '].Rem" /></td>';
                objHtml = objHtml + '<td><a class="text-danger" href="javascript:void(0)" onclick="DeleteTrans(this,' + i + ')" ><i class="mdi mdi-delete font-size-18"></i></a><input type="hidden" class="AcJournalDetID" id="AcJournalDetID_' + i + '" value="0" /></td>';
                objHtml = objHtml + '</tr>';
                $("#details").append(objHtml);
                ValidateTotal();
                $('#RowCount').val("0");
                $("#ReceivedFrom").val("");
                $("#SelectedReceivedFrom").val("0");
                $("#amount").val('');
                $("#TaxPercent").val("");
                $("#TaxAmount").val("");
                $('#remarks').val('');

                $("#ReceivedFrom_" + i).autocomplete({
                    source: function (request, response) {
                        $.ajax({
                            url: '/Accounts/AccountHead',
                            datatype: "json",
                            data: {
                                term: request.term
                            },
                            success: function (data) {
                                response($.map(data, function (val, item) {
                                    return {
                                        label: val.AcHead,
                                        value: val.AcHead,
                                        AcHeadID: val.AcHeadID,
                                        TaxPercent: val.TaxPercent
                                    }
                                }))
                            }
                        })
                    }, minLength: 1,
                    autoFocus: false,
                    focus: function (e, a) {
                        $("#ReceivedFrom_" + i).val(a.item.label);
                        $("#JAcHead_" + i).val(a.item.AcHeadID);
                        $("#JAcHead_" + i).attr('label', a.item.label);
                        $("#TaxPercent_" + i).val(a.item.TaxPercent);

                    },
                    select: function (e, a) {
                        e.preventDefault();
                        $("#ReceivedFrom_" + i).val(a.item.label);
                        $("#JAcHead_" + i).val(a.item.AcHeadID);
                        $("#JAcHead_" + i).attr('label', a.item.label);
                        $("#TaxPercent_" + i).val(a.item.TaxPercent);

                    }

                });
                $("#ReceivedFrom_" + i).change(function () {
                    if ($("#ReceivedFrom_" + i).val().trim() == "") {
                        $("#JAcHead_" + i).val('0');
                        $("#JAcHead_" + i).attr('label', '');
                    }
                    else if ($("#ReceivedFrom_" + i).val() != $("#JAcHead_" + i).attr('label')) {
                        $("#JAcHead_" + i).val('');
                        $("#JAcHead_" + i).attr('label', '');
                        $("#ReceivedFrom_" + i).val('');
                    }
                });

                $("#ChkAmountIncludingTax_" + i).click(function () {

                    if ($("#ChkAmountIncludingTax_" + i).is(':checked')) {
                        $("#ChkAmountIncludingTax_" + i).val(true);
                        if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                            var amount = parseFloat($("#JAcAmount_" + i).val());
                            var taxpercent = parseFloat($("#JAcTaxPercent_" + i).val()) / 100.00;
                            var taxamount = amount - (amount / (1 + taxpercent));
                            $("#JAcTaxAmount_" + i).val(taxamount.toFixed(_decimal));
                        }
                        else {
                            $("#JAcTaxAmount_" + i).val(0);
                        }
                    }
                    else {
                        $("#ChkAmountIncludingTax_" + i).val(false);
                        if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                            var tamount = ($("#JAcAmount_" + i).val() * $("#JAcTaxPercent_" + i).val()) / 100.00;
                            parseFloat($("#JAcTaxAmount_" + i).val(tamount)).toFixed(_decimal);
                        }
                        else {
                            $("#JAcTaxAmount_" + i).val(0);
                        }
                    }

                    ValidateTotal();
                });

                $("#JAcAmount_" + i).change(function () {

                    if ($("#ChkAmountIncludingTax_" + i).is(':checked')) {
                        if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                            var amount = parseFloat($("#JAcAmount_" + i).val());
                            var taxpercent = parseFloat($("#JAcTaxPercent_" + i).val()) / 100.00;
                            var taxamount = amount - (amount / (1 + taxpercent));
                            $("#JAcTaxAmount_" + i).val(taxamount.toFixed(_decimal));
                        }
                        else {
                            $("#JAcTaxAmount_" + i).val(0);
                        }
                    }
                    else {
                        if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                            var tamount = ($("#JAcAmount_" + i).val() * $("#JAcTaxPercent_" + i).val()) / 100.00;
                            parseFloat($("#JAcTaxAmount_" + i).val(tamount)).toFixed(_decimal);
                        }
                        else {
                            $("#JAcTaxAmount_" + i).val(0);
                        }
                    }
                    ValidateTotal();

                });

                $("#JAcTaxPercent_" + i).change(function () {
                    if ($("#ChkAmountIncludingTax_" + i).is(':checked')) {
                        if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                            var amount = parseFloat($("#JAcAmount_" + i).val());
                            var taxpercent = parseFloat($("#JAcTaxPercent_" + i).val()) / 100.00;
                            var taxamount = amount - (amount / (1 + taxpercent));
                            $("#JAcTaxAmount_" + i).val(taxamount.toFixed(_decimal));
                        }
                        else {
                            $("#JAcTaxAmount_" + i).val(0);
                        }
                    }
                    else {
                        if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                            var tamount = ($("#JAcAmount_" + i).val() * $("#JAcTaxPercent_" + i).val()) / 100.00;
                            parseFloat($("#JAcTaxAmount_" + i).val(tamount)).toFixed(_decimal);
                        }
                        else {
                            $("#JAcTaxAmount_" + i).val(0);
                        }
                    }
                    ValidateTotal();
                });

            }

        });


        if ($("#AcJournalID").val() > 0) {
            $('#transtype').attr('disabled', 'disabled');
            $('#paytype').attr('disabled', 'disabled');
            $('#btnSaveAccounts').html('Update');
            var v = $("#paytype option:selected").val();
            if (v == 1) {
                accounturl = '/Accounts/GetHeadsForCash';
            }
            else {
                accounturl = '/Accounts/GetHeadsForBank';
            }



        }
        var v = $("#transtype option:selected").val();

        $.ajax({
            type: "POST",
            url: "/Accounts/GetAcJDetails",
            datatype: "Json",
            data: { id: $("#AcJournalID").val(), transtype: v },
            success: function (data) {
                if ($('#details >  tr').length == 1) {
                    var emptyrow = $('#details > tr').html();
                    if (emptyrow.indexOf('No data available in table') >= 0) {
                        $('#details').html('');
                    }
                }
                var i = 0;

                $.each(data, function (index, value) {
                    i = index;

                    var amounttaxcheck = '';
                    if (value.AmountIncludingTax) {
                        amounttaxcheck = '<input type="checkbox" id="ChkAmountIncludingTax_' + i + '" value="true" checked name="AcJDetailVM[' + i + '].AmountIncludingTax" />';
                    }
                    else {
                        amounttaxcheck = '<input type="checkbox" id="ChkAmountIncludingTax_' + i + '" value="false"  name="AcJDetailVM[' + i + '].AmountIncludingTax"  />';
                    }

                    var objHtml = '<tr>' + '<td style="padding-left:10px"><input type ="hidden" id="hdndeleted_' + i + '" name="AcJDetailVM[' + i + '].IsDeleted" value="' + value.IsDeleted + '" class="hdndeleted" /> <input type="hidden" class=JAcHead id="JAcHead_' + i + '" value=' + value.AcHeadID + ' name="AcJDetailVM[' + i + '].AcHeadID"><input type="text" class="form-select" id="ReceivedFrom_' + i + '" value=" ' + value.AcHead + '" name="AcJDetailVM[' + i + '].AcHead"></td>' + '<td style="padding-left:10px">' + amounttaxcheck + '</td>' + '<td  style="padding-right:10px"><input type="text"   id="JAcAmount_' + i + '" class="form-control textright JAcAmt textright form-control" value=' + parseFloat(value.Amt).toFixed(_decimal) + ' name="AcJDetailVM[' + i + '].Amt"></td>' + '<td class=textright style="padding-right:10px"><input type="text" id="JAcTaxPercent_' + i + '" class="JAcAmt textright form-control" value=' + parseFloat(value.TaxPercent).toFixed(2) + ' name="AcJDetailVM[' + i + '].TaxPercent"></td>' + '<td class="" style="padding-right:10px"><input class="textright form-control" type="text" readonly="readonly"  id="JAcTaxAmount_' + i + '" class="JAcAmt textright form-control" value=' + parseFloat(value.TaxAmount).toFixed(_decimal) + ' name="AcJDetailVM[' + i + '].TaxAmount"></td>';
                    objHtml = objHtml + '<td><input class="form-control" type="text" id="editremark_' + i + '"   title="' + value.Rem + '" value="' + value.Rem + '" name="AcJDetailVM[' + i + '].Rem" /></td>';

                    objHtml = objHtml + '<td><a href="javascript:void(0)" class="text-danger" onclick="DeleteTrans(this,' + i + ')" ><i class="mdi mdi-delete font-size-18"></i></a><input type="hidden" id="hdnAcJournalDetID_' + i + '" class="AcJournalDetID" name="AcJDetailVM[' + i + '].AcJournalDetID" value=' + value.AcJournalDetID + ' /></td>';
                    objHtml = objHtml + '</tr>';
                    $("#details").append(objHtml);



                    $("#ReceivedFrom_" + i).autocomplete({
                        source: function (request, response) {
                            $.ajax({
                                url: '/Accounts/AccountHeadCreate',
                                datatype: "json",
                                data: {
                                    term: request.term
                                },
                                success: function (data) {
                                    response($.map(data, function (val, item) {
                                        return {
                                            label: val.AcHead,
                                            value: val.AcHead,
                                            AcHeadID: val.AcHeadID,
                                            TaxPercent: val.TaxPercent
                                        }
                                    }))
                                }
                            })
                        }, minLength: 1,
                        focus: function (e, a) {
                            $("#ReceivedFrom_" + i).val(a.item.label);
                            $("#JAcHead_" + i).val(a.item.AcHeadID);
                            $("#JAcHead_" + i).attr('label', a.item.label);
                            $("#JAcTaxPercent_" + i).val(a.item.TaxPercent);
                        },
                        select: function (e, a) {
                            e.preventDefault();
                            $("#ReceivedFrom_" + i).val(a.item.label);
                            $("#JAcHead_" + i).val(a.item.AcHeadID);
                            $("#JAcHead_" + i).attr('label', a.item.label);
                            $("#JAcTaxPercent_" + i).val(a.item.TaxPercent);

                        }

                    });
                    $("#ReceivedFrom_" + i).change(function () {
                        if ($("#ReceivedFrom_" + i).val().trim() == "") {
                            $("#ReceivedFrom_" + i).val('');
                            $("#JAcHead_" + i).val('0');
                            $("#JAcHead_" + i).attr('label', '');
                        }
                        else if ($("#ReceivedFrom_" + i).val() != $("#JAcHead_" + i).attr('label')) {
                            $("#JAcHead_" + i).val('');
                            $("#JAcHead_" + i).attr('label', '');
                            $("#ReceivedFrom_" + i).val('');
                        }
                    })


                    //ChkAmountIncludingTax_2
                    $("#ChkAmountIncludingTax_" + i).change(function () {
                        debugger;
                        var j = $(this).attr('id').split('_')[1];
                        if ($("#ChkAmountIncludingTax_" + j).prop('checked') == true) {
                            //$("#ChkAmountIncludingTax_" + i).val(true);
                            if ($("#JAcTaxPercent_" + j).val() > 0 && $("#JAcAmount_" + j).val() > 0) {
                                var amount = parseFloat($("#JAcAmount_" + j).val());
                                var taxpercent = parseFloat($("#JAcTaxPercent_" + j).val()) / 100.00;
                                var taxamount = amount - (amount / (1 + taxpercent));
                                $("#JAcTaxAmount_" + j).val(taxamount.toFixed(2));
                            }
                            else {
                                $("#JAcTaxAmount_" + j).val(0);
                            }
                        }
                        else {
                            //$("#ChkAmountIncludingTax_" + i).val(false);
                            if ($("#JAcTaxPercent_" + j).val() > 0 && $("#JAcAmount_" + j).val() > 0) {
                                var tamount = ($("#JAcAmount_" + j).val() * $("#JAcTaxPercent_" + j).val()) / 100.00;
                                parseFloat($("#JAcTaxAmount_" + j).val(tamount)).toFixed();
                            }
                            else {
                                $("#JAcTaxAmount_" + j).val(0);
                            }
                        }

                        ValidateTotal();
                    });

                    $("#JAcAmount_" + i).change(function () {

                        if ($("#ChkAmountIncludingTax_" + i).is(':checked')) {
                            if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                                var amount = parseFloat($("#JAcAmount_" + i).val());
                                var taxpercent = parseFloat($("#JAcTaxPercent_" + i).val()) / 100.00;
                                var taxamount = amount - (amount / (1 + taxpercent));
                                $("#JAcTaxAmount_" + i).val(taxamount.toFixed(_decimal));
                            }
                            else {
                                $("#JAcTaxAmount_" + i).val(0);
                            }
                        }
                        else {
                            if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                                var tamount = ($("#JAcAmount_" + i).val() * $("#JAcTaxPercent_" + i).val()) / 100.00;
                                parseFloat($("#JAcTaxAmount_" + i).val(tamount)).toFixed(_decimal);
                            }
                            else {
                                $("#JAcTaxAmount_" + i).val(0);
                            }
                        }
                        ValidateTotal();
                    });

                    $("#JAcTaxPercent_" + i).change(function () {
                        if ($("#ChkAmountIncludingTax_" + i).is(':checked')) {
                            if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                                var amount = parseFloat($("#JAcAmount_" + i).val());
                                var taxpercent = parseFloat($("#JAcTaxPercent_" + i).val()) / 100.00;
                                var taxamount = amount - (amount / (1 + taxpercent));
                                $("#JAcTaxAmount_" + i).val(taxamount.toFixed(_decimal));
                            }
                            else {
                                $("#JAcTaxAmount_" + i).val(0);
                            }
                        }
                        else {
                            if ($("#JAcTaxPercent_" + i).val() > 0 && $("#JAcAmount_" + i).val() > 0) {
                                var tamount = ($("#JAcAmount_" + i).val() * $("#JAcTaxPercent_" + i).val()) / 100.00;
                                parseFloat($("#JAcTaxAmount_" + i).val(tamount)).toFixed(_decimal);
                            }
                            else {
                                $("#JAcTaxAmount_" + i).val(0);
                            }
                        }
                        ValidateTotal();

                    });

                    ValidateTotal();

                });

            }
        });


        
        
        $("#AcHead").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: accounturl,
                    datatype: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.AcHead,
                                value: val.AcHead,
                                AcHeadID: val.AcHeadID
                            }
                        }))
                    }
                })
            }, minLength: 1,
            select: function (e, i) {
                e.preventDefault();
                $("#AcHead").val(i.item.label);
                $('#SelectedAcHead').val(i.item.AcHeadID);
                $('#SelectedAcHead').attr('label', i.item.label);
            }
        });
        $("#AcHead").change(function () {
            if ($("#AcHead").val() != "" && $("#AcHead").val() != $('#SelectedAcHead').attr('label')) {
                $("#AcHead").val('');
                $('#SelectedAcHead').val(0);
            }

        })



        $("#ReceivedFrom").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Accounts/AccountHead',
                    datatype: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.AcHead,
                                value: val.AcHead,
                                AcHeadID: val.AcHeadID,
                                TaxPercent: val.TaxPercent
                            }
                        }))
                    }
                })
            }, minLength: 1,
            autoFocus: false,
            select: function (e, i) {
                $("#ReceivedFrom").val(i.item.label);
                $('#SelectedReceivedFrom').val(i.item.AcHeadID);
                $('#SelectedReceivedFrom').attr('label', i.item.label)
                $('#TaxPercent').val(i.item.TaxPercent);
            },
            select: function (e, i) {
                e.preventDefault();
                $("#ReceivedFrom").val(i.item.label);
                $('#SelectedReceivedFrom').val(i.item.AcHeadID);
                $('#SelectedReceivedFrom').attr('label', i.item.label)
                $('#TaxPercent').val(i.item.TaxPercent);
            }
        });

        /* CheckPeriodLock($('#transdate').val());*/
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
                        debugger;
                        if (false === e.checkValidity()) {
                            e.classList.add("was-validated");
                        }
                        else {

                            t.preventDefault();
                            t.stopPropagation();
                            e.classList.remove("was-validated");

                            SaveAccount();



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


})(jQuery);
