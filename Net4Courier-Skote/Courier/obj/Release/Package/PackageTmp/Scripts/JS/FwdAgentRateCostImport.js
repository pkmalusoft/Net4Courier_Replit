
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
        Swal.fire("Save Status!", "Add Items to save", "error");
        return false;
    }
    $('#btnsave').attr('disbled', 'disabled');
    $('#h4LoaderTitle').html('Data Saving');
    $('#loaderpopup').modal('show');
        
    var items = [];
  
    for (var i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        //if (deleted == 'false' || deleted == '' || deleted == null) {
            var awbentry = {
                Deleted : deleted,
                AWBNo: $('#txtAWBNo_' +i).val(),
                TotalCost : $('#txtTotalCost_' + i).val(),
                OtherCharge: $('#txtOtherCharge_' + i).val(),
                Remarks: $('#txtRemarks_' + i).val(),
                Weight: $('#txtWeight_' + i).val(),
                ForwardingAWBNo: $('#txtfwdno_'+i).val()
           }

            items.push(awbentry);

        //}
         

        if ((i + 1) == maxrow) {
            $.ajax({
                type: "POST",
                url: '/ForwardingAWBCost/SaveRate/',
                datatype: "html",
                data: { AgentID: $('#FAgentID').val(), Details :  JSON.stringify(items)},
                success: function (response) {
                    debugger;
                    if (response.Status == "OK") {

                        $('#loaderpopup').modal('hide');
                        Swal.fire("Save Status!", response.message, "success");
                        setTimeout(function () {
                            window.location.href = '/ForwardingAWBCost/Index';
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

        
    
      
       

        
    } function deletetrans(obj, i) {
        
            $('#hdndeleted_' + i).val(true);
            $(obj).parent().parent().addClass('hide');
            
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
            //$('#loaderpopup').modal('show');
            var files = $("#importFile").get(0).files;

            var formData = new FormData();
            formData.append('importFile', files[0]);
            formData.append('CustomerID', 0);
            if (files.length > 0) {
                $('#btnUpload').attr('disabled', 'disabled');
                $.ajax({
                    url: '/ForwardingAWBCost/ImportFile',
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
                                $("#listContainer").html('');
                                

                                Swal.fire("Data Importing Status!", "No AWB found!", "success");
                                setTimeout(function () {
                                    $('#loaderpopup').modal('hide');
                                },100)
                                
                                return false;
                            }
                            else {
                                $('#loaderpopup').modal('hide');
                            }
                            $.each(response.data, function (index, item) {
                                console.log(index);
                                var _AWBNo = item.AWBNo;
                                var _TotalCost = item.TotalCost;
                                var _OtherCharge1= item.OtherCharge;                                
                                var _remarks = item.Remarks;
                                var _weight = item.Weight;
                                var _fwdno = item.ForwardingAWBNo;
                                var html = '<tr><td><input class="hdndeleted" type="hidden" name="FAgentRateDetails[' + index + '].Deleted" value = "false" id = "hdndeleted_' + index + '" /><input type="text" class="text-right" id="txtAWBNo_' + index + '" value="' + _AWBNo + '"/></td>' +
                                    '<td><input type="text"class="text-right" id="txtTotalCost_' + index + '"   value="' + _TotalCost + '"></td>' +
                                    '<td><input type="text" class="text-right"  id="txtOtherCharge_' + index + '"  value="' + _OtherCharge1 + '"></td>' +                                    
                                    '<td><input type="text" class="text-right crate"  id="txtRemarks_' + index + '" value="' + _remarks + '"></td>' +
                                    '<td><input type="text" class="text-right crate"  id="txtWeight_' + index + '" value="' + _weight + '"></td>' +
                                    '<td><input type="text" class="text-right crate"  id="txtfwdno_' + index + '" value="' + _fwdno + '"></td>' +
                                    '<td><a href="javascript:void(0)" class="text-danger" onclick="deletetrans(this,' + index + ')"><i class="mdi mdi-delete font-size-18"></i></a></td>' +
                                    +'</tr>';
                                $("#details").append(html);
                            })

                            Swal.fire("Data Importing Status!", "AWB listed Successfully!", "success");
 
                            
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
