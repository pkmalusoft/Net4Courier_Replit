
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
        FAgentRateID: $('#FAgentRateID').val(),
        FAgentID: $('#FAgentID').val(),
        ProductTypeID: $('#ProductTypeID').val(),
        ZoneChartID: $('#ZoneChartID').val(),
        BaseWeight: $('#BaseWeight').val(),
        BaseRate: $('#BaseRate').val(),


    }
    var items = [];
  
    for (var i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        //if (deleted == 'false' || deleted == '' || deleted == null) {
            var awbentry = {
                Deleted : deleted,
                FAgentRateDetID: $('#FAgentRateID').val(),
                FAgentRateID: $('#hdnFAgentRateID_' + i).val(),
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
                url: '/ForwardingAgentRates/SaveRate/',
                datatype: "html",
                data: { v:obj,Details :  JSON.stringify(items)},
                success: function (response) {
                    debugger;
                    if (response.Status == "OK") {

                        $('#loaderpopup').modal('hide');
                        Swal.fire("Save Status!", response.message, "success");
                        setTimeout(function () {
                            window.location.href = '/ForwardingAgentRates/Create?id=' + response.FAgentRateID;
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

        
    
      
       

        if ($('#BATCHID').val() > 0) {
            $('#btnSaveBatch').val('Update');
            $('#divothermenu').removeClass('hide');
            $('#divothermenu1').removeClass('hide');
            $('#CourierStatusID').attr('disabled', 'disabled');
            var table = $('#datatable1').DataTable({
                "aaSorting": [],
                "searching": true,
                "bPaginate": false,

            });
        }
        else {
            $('#tblbody').html('');
            var table = $('#datatable1').DataTable({
                "aaSorting": [],
                "searching": true,
                "bPaginate": false,

            });
            $('#datatable1 >tbody').html('');
            $('#CurrencyID').val($('#DefaultCurrencyId').val()).trigger('change');
        }
    }
    function initAWBControl() {
       

        $(document).on("click", "#btnUpload", function () {

            

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
        $(document).on("change", "#importFile", function () {
            debugger;
            $('#h4LoaderTitle').html('Data Importing');
            $('#loaderpopup').modal('show');
            var files = $("#importFile").get(0).files;

            var formData = new FormData();
            formData.append('importFile', files[0]);
            formData.append('CustomerID', 0);
            if (files.length > 0) {
                $('#btnUpload').attr('disabled', 'disabled');
                $.ajax({
                    url: '/ForwardingAgentRates/ImportFile',
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
                                Swal.fire("Data Importing Status!", "Duplicate AWB Not allowed to import", "success");
                                $("#listContainer").html('');
                                $('#loaderpopup').modal('hide');
                                return false;
                            }
                            else {
                                $('#loaderpopup').modal('hide');
                            }
                            $.each(response.data, function (index, item) {
                                console.log(index);
                                var _AddWtFrom = item.AddWtFrom;
                                var _AddWtTo = item.AddWtTo;
                                var _IncrWt = item.IncrWt;
                                var _ContractRate= item.ContractRate;
                                var _AddRate = item.AddRate;
                                var html = '<tr><td><input type="hidden" name="FAgentRateDetails[' + index + '].FAgentRateDetID" value="0" id="hdnFAgentRateDetID_' + index + '" /><input type="hidden" name="FAgentRateDetails[' + index + '].FAgentRateID" value ="0" id = "hdnFAgentRateID_' + index + '" /><input class="hdndeleted" type="hidden" name="FAgentRateDetails[' + index + '].Deleted" value = "false" id = "hdndeleted_' + index + '" /><input type="text" class="text-right" id="txtWtFrom_' + index + '" value="' + _AddWtFrom + '" name="FAgentRateDetails[' + index + '].AddWtFrom" /></td>' +
                                    '<td><input type="text"class="text-right" id="txtWtTo_' + index + '"   value="' + _AddWtTo + '" name="FAgentRateDetails[' + index + '].AddWtTo"></td>' +
                                    '<td><input type="text" class="text-right"  id="txtIncrWt_' + index + '"  value="' + _IncrWt + '" name="FAgentRateDetails[' + index + '].IncrWt"></td>' +
                                    '<td><input type="text" class="text-right" id="txtContractRate_' + index + '" value="' + _ContractRate + '" name="FAgentRateDetails[' + index + '].ContractRate" onchange="contractdatecal(' + index + ')" ></td>' +
                                    '<td><input type="text" class="text-right crate"  id="txtAddRate_' + index + '" value="' + _AddRate + '" name="FAgentRateDetails[' + index + '].AddRate"></td>' +
                                    //'<td><a href="javascript:void(0)" onclick="check(this)" value="Click"><i class="fa fa-pencil"></i></a></td>' +
                                    '<td><a href="javascript:void(0)" class="text-danger" onclick="deletetrans(this,' + index + ')"><i class="mdi mdi-delete font-size-18"></i></a></td>' +
                                    +'</tr>';
                                $("#details").append(html);
                            })

                          
 
                            
                        } else {
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
