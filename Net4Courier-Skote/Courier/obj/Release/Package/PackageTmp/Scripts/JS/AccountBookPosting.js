function DeleteTrans(obj, index) {
    $(obj).parent().parent().addClass('hide');
    //var obj1 = $(this).parent().parent().find('.hdndeleted');
    $('#hdndeleted_' + index).val(true);
    var maxrow = $('#details > tr').length;
    debugger;
    var TotalDebit = 0;
    var TotalCredit = 0;
    for (i = 0; i < maxrow; i++) {
        var debit = $('#JDebitAmount_' + i).val();
        var credit = $('#JCreditAmount_' + i).val();


        var deleteobj = $('#hdndeleted_' + i).val();
        if (deleteobj != "true") {
            TotalDebit += parseFloat(debit);
            TotalCredit += parseFloat(credit);
        }
        $('#spandebittotal').html(parseFloat(TotalDebit).toFixed(2))
        $('#spancredittotal').html(parseFloat(TotalCredit).toFixed(2))
    }
}
function checkamount(type) {
    if (type == '1') {
        $('#CreditAmount').val(0);
    }
    else {
        $('#DebitAmount').val(0);
    }

}
function CalculateTotal(index, type) {
    debugger;
    if (type == '1') {
        $('#JCreditAmount_' + index).val(0);
    }
    else {
        $('#JDebitAmount_' + index).val(0);
    }
    var maxrow = $('#details > tr').length;
    var TotalDebit = 0;
    var TotalCredit = 0;
    for (i = 0; i < maxrow; i++) {
        var debit = $('#JDebitAmount_' + i).val();
        var credit = $('#JCreditAmount_' + i).val();
        

        var deleteobj = $('#hdndeleted_' +i).val();
        if (deleteobj != "true") {
            TotalDebit += parseFloat(debit);
            TotalCredit += parseFloat(credit);
        }
        $('#spandebittotal').html(parseFloat(TotalDebit).toFixed(2))
        $('#spancredittotal').html(parseFloat(TotalCredit).toFixed(2))
    }
    
    
}
function SaveAccount() {
    var itemcount = $('#details >  tr').length;
    if (itemcount==0) {
        $('#h2error').html("Add Transaction!");
        return false;
    }
    if (parseFloat($('#spandebittotal').html()) != parseFloat($('#spancredittotal').html())) {
        $('#h2error').html("Invalid Entry,Debit and Credit should be equal!");
        return false;
    }
    $('#h2error').html("");
    
    $('#btnSaveAccounts').attr('disabled', 'disabled');
    var accountobj = {
        AcJournalID: $('#AcJournalID').val(),                        
        TransDate: $('#TransDate').val(),        
        Remark: $('#Remark').val(),
        reference: $('#Refference').val()
    }
    
    var obj = [];
    for (var i = 0; i < itemcount; i++)
    {
        
        var item = {
            ID: $('#hdnAcJournalDetID_' + i).val(),
            IsDeleted : $('#hdndeleted_' + i).val(),
            acHeadID: $('#AcHeadId_' + i).val(),
            AcRemark: $('#txtremark_' + i).val(),
            DebitAmount: $('#JDebitAmount_' + i).val(),
            CreditAmount: $('#JCreditAmount_' + i).val()          
            
            
        }
        obj.push(item);
        if (itemcount == (i+1)) {
            $.ajax({
                type: "POST",
                url: '/Accounts/SaveAcJournal/',
                datatype: "json",
                data: { data: accountobj, Details: JSON.stringify(obj) },
                success: function (response) {
                    debugger;
                    if (response.status == "OK") {
                        Swal.fire("Save Status!", response.message, "success");                        
                        //$('#divothermenu').removeClass('hide');
                        $('#btnSaveAccounts').removeAttr('disabled');
                        var t = document.getElementsByClassName("needs-validation");
                        $(t).removeClass('was-validated');
                        window.location.href = '/Accounts/CreateAcJournal?id=' + response.AcJournalID;
                        //window.location.reload();

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




(function ($) {

    'use strict';
    function initformControl() {
        var _decimal = "2";
        var accounturl = '/Accounts/GetHeadsForCash';
        $('#TransDate').datepicker({
            dateFormat: 'dd-mm-yy'
        });

      
        $("#AcHead").autocomplete({
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
            }
        });

        $("#btnAdd").click(function () {

            debugger;
            var str = "";
            var acheadid = $('#SelectedAcHead').val(); //$("#acHeadId option:selected").val();
            var Achead = $('#AcHead').val(); //$("#acHeadId option:selected").text();
            var DebitAmount = $("#DebitAmount").val();
            var CreditAmount = $("#CreditAmount").val();
            //var PaymentType = $("#IsDebit option:selected").val();
            //var PaymentTypetext = $("#IsDebit option:selected").text();
            var remark = $("#AcRemark").val();

            if (Achead == "" || acheadid == 0 || acheadid == '') {
                $('#h2error').html("Please Select Account Head!");
                $('#AcHead').focus();
                return;
            }
            
            if (DebitAmount == '') {
                DebitAmount = 0;
            }
            if (CreditAmount == '') {
                CreditAmount = 0;
            }

            if (DebitAmount == 0 && CreditAmount == 0) {
                $('#h2error').html("Please Enter Amount!");
                $('#DebitAmount').focus();
                return;
            }

            var i = $('#details > tr').length;

            var html = '<tr>' + '<td><input type="hidden" id="hdnAcJournalDetID_' + i + '" value="0"><input id="hdndeleted_' + i + '" type ="hidden" name ="acJournalDetailsList[' + i + '].IsDeleted" class="hdndeleted" value="false" /> <input id="AcHead_' + i + '" type="text" class="form-control" value="' + Achead + '" name="acJournalDetailsList[' + i + '].AcHead"><input type="hidden" id="AcHeadId_' + i + '" value=' + acheadid + ' name="acJournalDetailsList[' + i + '].acHeadID"></td>';
            html += '<td class="textright"><input class="textright form-control" type="text"  onchange="CalculateTotal(' +  i + ',1)" value="' + parseFloat(DebitAmount).toFixed(2) + '" name="acJournalDetailsList[' + i + '].DebitAmount" id="JDebitAmount_' + i + '" /></td>';
            html += '<td class="textright"><input class="textright form-control" type="text"  onchange="CalculateTotal(' + i + ',2)" value="' + parseFloat(CreditAmount).toFixed(2) + '" name="acJournalDetailsList[' + i + '].CreditAmount" id="JCreditAmount_' + i + '" /></td>'; 
            //html +='<td>' + PaymentTypetext + '<input type="hidden" id="Paytype_' + i + '" value=' + PaymentType + ' name="acJournalDetailsList[' + i + '].IsDebit"></td>' +
            html += '<td><input type="text" id="txtremark_' + i + '" class="form-control" value="' + remark + '" name="acJournalDetailsList[' + i + '].AcRemark"></td>';
            html +='<td><a class="text-danger" href="javascript:void(0)" onclick="DeleteTrans(this,' + i + ')" ><i class="mdi mdi-delete font-size-18"></i></a></td></tr >';

            $("#details").append(html);
            $("#AcHead_" + i).autocomplete({
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
                    $("#AcHead_" + i).val(a.item.label);
                    $("#AcHeadId_" + i).val(a.item.AcHeadID);
                    $("#AcHead_" + i).attr('label', a.item.label);                    

                },
                select: function (e, a) {
                    e.preventDefault();
                    $("#AcHead_" + i).val(a.item.label);
                    $("#AcHeadId_" + i).val(a.item.AcHeadID);
                    $("#AcHead_" + i).attr('label', a.item.label);                    

                }

            });
            $("#ReceivedFrom_" + i).change(function () {
                //if ($("#AcHead_" + i).val().trim() == "") {
                //    $("#AcHead_" + i).val('0');
                //    $("#AcHead_" + i).attr('label', '');
                //}
                //else if ($("#ReceivedFrom_" + i).val() != $("#JAcHead_" + i).attr('label')) {
                //    $("#AcHead_" + i).val('');
                //    $("#AcHead_" + i).attr('label', '');                    
                //}
            });
        // $("#acHeadId").val($("#acHeadId option:first").val());
            $("#DebitAmount").val('');
            $("#CreditAmount").val('');
            $("#AcRemark").val('');
            $('#AcHead').val('');
            $('#SelectedAcHead').val(0);
            CalculateTotal();
            $('#AcHead').focus();
        });

        if ($('#AcJournalID').val() > 0) {
            $('#btnSaveAccounts').html('Update');
        }
        $.ajax({
            type: "GET",
            url: "/Accounts/GetJournalAcJDetails",
            datatype: "Json",
            data: { id: $("#AcJournalID").val() },
            success: function (data) {
                
                
                $.each(data, function (index, value) {

                    var i = $('#details > tr').length;
                    var ID = value.ID;
                    var Achead = value.AcHead;
                    var acheadid = value.acHeadID;
                    var CreditAmount = value.CreditAmount;
                    var DebitAmount = value.DebitAmount;
                    var remarks = value.AcRemark;
                    var html = '<tr>' + '<td><input type="hidden" id="AcJournalDetID_' + i + '" value="' + ID  + '"><input id="hdndeleted_' + i + '" type ="hidden" name ="acJournalDetailsList[' + i + '].IsDeleted" class="hdndeleted" value="false"   /> <input id="AcHead_' + i + '" type="text" readonly class="form-control" value="' + Achead + '" name="acJournalDetailsList[' + i + '].AcHead"><input type="hidden" id="AcHeadId_' + i + '" value=' + acheadid + ' name="acJournalDetailsList[' + i + '].acHeadID"></td>';
                    html += '<td class="textright"><input  readonly class="textright form-control" type="text"  onchange="CalculateTotal(' + i + ',1)" value=' + parseFloat(DebitAmount).toFixed(2) + ' name="acJournalDetailsList[' + i + '].DebitAmount" id="JDebitAmount_' + i + '" /></td>';
                    html += '<td class="textright"><input readonly class="textright form-control" type="text"  onchange="CalculateTotal(' + i + ',2)" value=' + parseFloat(CreditAmount).toFixed(2) + ' name="acJournalDetailsList[' + i + '].CreditAmount" id="JCreditAmount_' + i + '" /></td>';
                    //html +='<td>' + PaymentTypetext + '<input type="hidden" id="Paytype_' + i + '" value=' + PaymentType + ' name="acJournalDetailsList[' + i + '].IsDebit"></td>' +
                    html += '<td><input readonly type="text" id="txtremark_' + i + '" class="form-control" value="' + remarks + '" name="acJournalDetailsList[' + i + '].AcRemark"></td>';
                    html += '</tr >';

                    $('#details').append(html);
                    if ((index + 1) == data.length) {
                        CalculateTotal();
                        
                    }
                });
            }


        });
       

       
    
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


})(jQuery)
