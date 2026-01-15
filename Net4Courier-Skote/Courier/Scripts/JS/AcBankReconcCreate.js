
var _decimal = "2";
 
function checkDate(obj) {
    var value = '';
    if ($(obj).val() == '' || $(obj).val() == undefined || $(obj).val() == 'undefined' || $(obj).val() == null) {
        return '';
    }
    else {
        var date = $(obj).val().split('-');
        value = date[1] + '/' + date[0] + '/' + date[2];
    }

    return value

}
function DeleteAWB(i) {
    $('#trrow_' + i).addClass('hide');
    $('#hdndeleted_' + i).val('true');


}

 
function SaveRate() {
    debugger;
    //if ($('#CustomerID').val() == '' || $('#CustomerID').val() == '0') {
    //    Swal.fire("Data Importing Status!", "Select Customer!", "error");
    //    $('#CustomerName').focus();
    //    return false;
    //}

    //if ($('#CourierStatusID').val() == '' || $('#CourierStatusID').val() == '0') {
    //    Swal.fire("Data Importing Status!", "Select Status!", "error");
    //    $('#CourierStatusID').focus();
    //    return false;
    //}
    var maxrow = $('#details > tr').length;
    if (maxrow == 0) {
        Swal.fire("Save Status!", "Add Rate to save", "error");
        return false;
    }
    $('#btnsave').attr('disbled', 'disabled');
    $('#h4LoaderTitle').html('Data Saving');
    $('#loaderpopup').modal('show');

    var obj = {
        CustomerRateID: $('#CustomerRateID').val(),
        ContractRateTypeID: $('#ContractRateTypeID').val(),
        ZoneChartID: $('#ZoneChartID').val(),
        MovementID: $('#MovementID').val(),
        ProductTypeID: $('#ProductTypeID').val(),
        FAgentID: $('#FAgentID').val(),
        PaymentModeID: $('#PaymentModeID').val(),
        BaseMargin: $('#BaseMargin').val(),
        BaseWt: $('#BaseWt').val(),
        BaseRate: $('#BaseRate').val(),
        withtax: $('#withtax').prop('checked'),


    }
    var items = [];
  
    for (var i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        //if (deleted == 'false' || deleted == '' || deleted == null) {
            var awbentry = {
                Deleted : deleted,
                hdnCustomerRateDetID :$('#hdnCustomerRateDetID_' +i).val(),
                CustomerRateID: $('#CustomerRateID').val(),
                AddWtFrom: $('#txtWtFrom_' + i).val(),
                AddWtTo: $('#txtWtTo_' + i).val(),
                IncrWt: $('#txtIncrWt_' + i).val(),
                ContractRate: $('#txtContractRate_' + i).val(),
                AddRate: $('#txtAddRate_' + i).val()                
           }

            items.push(awbentry);

        //}
         

        if ((i + 1) == maxrow) {
            $.ajax({
                type: "POST",
                url: '/CustomerRatesMaster/SaveRate/',
                datatype: "html",
                data: { v:obj,Details :  JSON.stringify(items)},
                success: function (response) {
                    debugger;
                    if (response.Status == "OK") {

                        $('#loaderpopup').modal('hide');
                        Swal.fire("Save Status!", response.message, "success");
                        setTimeout(function () {
                            window.location.href = '/CustomerRatesMaster/Create?id=' + response.CustomerRateID;
                        }, 200)


                        //window.location.reload();

                    }
                    else {
                        $('#loaderpopup').modal('hide');
                        $('#btnSaveBatch').removeAttr('disabled');
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
         
      
    }
    function initAWBControl() {
       

        $(document).on("click", "#btnUpload", function () {

            if ($('#ChangeStatus').val() == '') {
                Swal.fire('Data Validation', 'Select Change Status!', 'error');
                return false;
            }

            $('#importFile').trigger('click');
        });
     

   
    }
    function init() {
        initformControl();
        initAWBControl();
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
                            SaveRate();
                            

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
        $('#btnAutoReconc').click(function () {
            if ($('#csvtablebody').html().indexOf('No data available in table')>0) {
                Swal.fire('Data Validation', 'Upload CSV file for Auto Reconciliation1', 'error');
                return false;
            }

            if ($('#ChangeStatus').val() == '') {
                Swal.fire('Data Validation', 'Select Change Status!', 'error');
                return false;
            }
            $.ajax({
                type: "POST",
                url: "/BankReconciliation/ShowAutoReconc",
                datatype: "html",
                data: { StatusTrans: $('#ChangeStatus').val() },
                success: function (response) {
                    $("#listContainer1").html(response);
                    
                    $('#loaderpopup').modal('hide');
                    setTimeout(function () {
                        var maxrow1 = $('#chequetablebody > tr').length;
                        for (j = 0; j < maxrow1; j++) {
                            debugger;
                            $('#dwstatus_' + j).val($('#dwstatus_' + j).attr('value')).trigger('change');
                        }
                    }, 100)
                    Swal.fire("Reconciliation Status!", 'Auto Reconciliation Processed,Click Update to Save!', "success");

                }
            });

        })
        
        $(document).on("change", "#importFile", function () {
            debugger;
        
            $('#h4LoaderTitle').html('Data Importing');
            //$('#loaderpopup').modal('show');
            var files = $("#importFile").get(0).files;
            
            var formData = new FormData();
            formData.append('importFile', files[0]);
        
            if (files.length > 0) {
                $('#btnUpload').attr('disabled', 'disabled');
                $.ajax({
                    url: '/BankReconciliation/ImportFile',
                    data: formData,
                    type: 'POST',
                    contentType: false,
                    processData: false,
                    success: function (response) {
                        if (response.Status === 1) {
                            debugger;
                            var data = response.data;
                            var max = data.length;

                            if (max == 0) {
                                $('#loaderpopup').modal('hide');
                                Swal.fire("Data Importing Status!", "No data found", "success");
                                $("#listContainer").html('');
                                $('#loaderpopup').modal('hide');
                                $('#btnUpload').remvoveAttr('disabled');
                                return false;
                            }
                            else {
                                $('#loaderpopup').modal('hide');
                                $("#details").html('');
                            }
                            $.ajax({
                                type: "POST",
                                url: "/BankReconciliation/ShowBankCSVList",
                                datatype: "html",                               
                                success: function (response) {
                                    $("#listContainer2").html(response);
                                    $('#btnUpload').removeAttr('disabled');
                                    $('#loaderpopup').modal('hide');
                                    Swal.fire("Data Importing Status!", 'Data Imported Successfully', "success");

                                }
                            });
                           
                        }
                        else {
                            $('#loaderpopup').modal('hide');
                            $('#btnUpload').removeAttr('disabled');
                            Swal.fire("Data Importing Status!", response.Message, "error");
                        }
                    },
                    error: function (err) {
                        $('#loaderpopup').modal('hide');
                        console.log(err);
                    }

                });
            }
            else {
                $('#btnUpload').removeAttr('disabled');
                $('#loaderpopup').modal('hide');
            }
        });

    })


})(jQuery)
