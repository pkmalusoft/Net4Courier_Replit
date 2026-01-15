var _decimal = "2";
function TotalItems() {
    var maxrow = $('#listContainer > tr').length;
    var totalMAWBweight = 0;
    var totalVerifedWeight = 0;
    var TotalRate = 0;
    for (var i = 0; i < maxrow; i++) {
        var checked = $('#chkawb_' + i).prop('checked');
        if (checked == true) {
            var weight = $('#hdnWeight_' + i).val();
            if (weight == '')
                weight = 0;

            var vweight = $('#txtFwdVerifiedWeight_' + i).val();
            if (vweight == '')
                vweight = 0;

            var vrate = $('#txtFwdRate_' + i).val();
            if (vrate == '')
                vrate = 0;

            totalMAWBweight = parseFloat(totalMAWBweight) + parseFloat(weight);
            totalVerifedWeight = parseFloat(totalVerifedWeight) + parseFloat(vweight);
            TotalRate = parseFloat(TotalRate) + parseFloat(vrate);

            SetAutocompleteFwdAgent(i);

          

        }
        

        if ((i + 1) == maxrow) {
            $('#txtTotalMAWBWt').val(parseFloat(totalMAWBweight).toFixed(3));
            $('#txtTotalVerifiedWt').val(parseFloat(totalVerifedWeight).toFixed(3));
            $('#txtTotalFwdRate').val(parseFloat(TotalRate).toFixed(_decimal));
        }
    }
}
function setTwoNumberDecimal(obj) {


    $(obj).val(parseFloat($(obj).val()).toFixed(_decimal));
}
function setThreeNumberDecimal(obj) {


    $(obj).val(parseFloat($(obj).val()).toFixed(3));
}
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
function searchoption(chkoption) {
    var option = 0;
      
    if ($('#ChkBulkAWB').prop('checked') == true)
                option = 2; //for bulk
            else
                option = 1; //for single
                

    
    if (option == 1) { //search by single awb
        $('#ChkBulkAWB').prop('checked', false);
        
        $('#txtOriginCountry').attr('disabled', 'disabled');
        $('#txtDestinationCountry').attr('disabled', 'disabled');
        $('#txtColoader').attr('disabled', 'disabled');
        $('#txtAWBNo').removeAttr('disabled');
        $('#btnaddsingle').removeAttr('disabled');
        $('#btnaddbulk').attr('disabled', 'disabled');
    }
    else { //search by multiple awb
        $('#ChkBulkAWB').prop('checked', true);
        

        $('#txtOriginCountry').removeAttr('disabled');
        $('#txtDestinationCountry').removeAttr('disabled');
        $('#txtColoader').removeAttr('disabled');
        $('#txtAWBNo').attr('disabled', 'disabled');
        $('#btnaddsingle').attr('disabled', 'disabled');
        $('#btnaddbulk').removeAttr('disabled');
    }
}
function SetAutocompleteFwdAgent(index) {
    $("#txtFwdAgent_" + index).autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '/Supplier/GetFwdAgentName',
                datatype: "json",
                data: {
                    term: request.term
                },
                success: function (data) {
                    response($.map(data, function (val, item) {
                        return {
                            label: val.FAgentName,
                            value: val.FAgentID
                             
                        }
                    }))
                }
            })
        },
        minLength: 1,
        autoFocus: false,
        focus: function (event, ui) {
            $('#txtFwdAgent_' + index).val(ui.item.label);
            $('#txtFwdAgent_' + index).attr('fagentid', ui.item.value);
            

        },
        select: function (e, ui) {

            e.preventDefault();
            $('#txtFwdAgent_' + index).val(ui.item.label);
            $('#txtFwdAgent_' + index).attr('fagentid', ui.item.value);

        },

    });
}
function checkduplicate(awbno) {
    debugger;
    var idtext = 'hdnAWBNo_';
    var maxrow = $('#listContainer > tr').length;
    if (maxrow > 0) {
        var duplicate = false;
        $('[id^=' + idtext + ']').each(function (index, item) {

            var con = $('#hdnAWBNo_' + index).val();
            if (con.trim() == awbno.trim()) {
                duplicate = true;
                var message = 'Duplicate AWB No.!';
                
                Swal.fire("AWB Search Status!", "Duplicate AWB No. not allowed!", "success");
                //$('#ulerr').removeClass('show-green');
                //$('#ulerr').addClass('show-error');
                //$('#lierr').html(message);
               // $('#btnAdd').attr('disabled', 'disabled');
                //break;


                if (duplicate == false && index == (parseInt(maxrow - 1))) {
                    return true;
                }
            }
        });
        if (duplicate == true) {
            return false;
        }
    }
    else {

        return true;
    }
}
function AddSingleAWB() {
    debugger;
    if ($('#txtAWBNo').val() == '') {
        return;
    }
    if (checkduplicate($('#txtAWBNo').val()) == false) {
        $('#txtAWBNo').val('');
        $('#txtAWBNo').focus();
        return false;

    }
    $.ajax({
        type: "POST",
        url: "/ExportShipment/GetAWBDetail",
        datatype: "Json",
        data: {
            SearchOption: 1, id: $('#txtAWBNo').val(), OriginCountry: '', DestinationCountry:'', CoLoaderID: 0, exportid: $('#ID').val()
        },
        success: function (response) {
            if (response.status == "ok") {
                debugger;
                if (response.data.length == 0) {
                    Swal.fire("AWB Search Status!", "AWB No. Not found", "success");
                    return false;
                }
                var data = response.data[0];
                console.log(data);
                
              
                $('#btnAdd').attr('disabled', 'disabled');
                 
                var dateString = data.AWBDate.substr(6);
                var currentTime = new Date(parseInt(dateString));
                var month = currentTime.getMonth() + 1;
                var day = currentTime.getDate();
                var year = currentTime.getFullYear();
                var tempdate = day + "-" + month + "-" + year;
                 
                var awbentry = {
                    ShipmentDetailID: 0,
                    ExportID: $('#ID').val(),
                    InboundShipmentID: data.ShipmentID,
                    InscanId: data.InScanId,
                    AWBNo: data.AWBNo,
                    AWBDate: tempdate,                    
                    Consignor: data.Consignor,                   
                    Consignee: data.Consignee, 
                    OriginCountry: data.ConsignorCountryName,
                    OriginCity: data.ConsignorCityName,  
                    DestinationCountry: data.ConsigneeCountryName,
                    DestinationCity: data.ConsigneeCityName,                    
                    Contents: data.CargoDescription,
                    PCS: data.Pieces,
                    Weight: data.Weight,                    
                    FAgentID: $('#FAgentID').val(),
                    FAgentName :$('#FAgentID option:selected').text(),
                    BagNo: data.BagNo,                    
                    MAWB: $('#MAWB').val()
                   

                }
                var entrymode = $('#btnAdd').attr('entrymode');
                var i = $('#listContainer >tr').length;
                var html = '<tr><td>' +
                    '<input type="hidden" id="hdnShipmentDetailID_' + i + '" value="' + awbentry.ShipmentDetailID + '" />' +
                    '<input type="hidden" id="hdnExportID_' + i + '" value="' + awbentry.ExporID + '" />' +
                    '<input type="hidden" id="hdnInboundShipmentID_' + i + '" value="' + awbentry.InboundShipmentID + '" />' +
                    '<input type="hidden" id="hdnInscanId_' + i + '" value="' + awbentry.InscanId+ '" />' +
                    '<input type="hidden" id="hdnAWBNo_' + i + '" value="' + awbentry.AWBNo + '" />' +
                    '<input type="hidden" id="hdnAWBDate_' + i + '" value="' + awbentry.AWBDate + '" />' +
                   
                    '<input type="hidden" id="hdnShipper_' + i + '" value="' + awbentry.Consignor + '" />' +             
                    '<input type="hidden" id="hdnOriginCity_' + i + '" value="' + awbentry.OriginCity + '" />' +
                    '<input type="hidden" id="hdnOriginCountry_' + i + '" value="' + awbentry.OriginCountry + '" />' +
                    '<input type="hidden" id="hdnReceiver_' + i + '" value="' + awbentry.Consignee + '" />' +                     
                    '<input type="hidden" id="hdnDestinationCity_' + i + '" value="' + awbentry.DestinationCity + '" />' +
                    '<input type="hidden" id="hdnDestinationCountry_' + i + '" value="' + awbentry.DestinationCountry + '" />' +  
                    '<input type="hidden" id="hdnBagNo_' + i + '" value="' + awbentry.BagNo + '" />' +
                    '<input type="hidden" id="hdnContents_' + i + '" value="' + awbentry.Contents + '" />' +
                    '<input type="hidden" id="hdnPCS_' + i + '" value="' + awbentry.PCS + '" />' +
                    '<input type="hidden" id="hdnWeight_' + i + '" value="' + awbentry.Weight + '" />' + 
                    '<input type="hidden" id="hdnMAWB_' + i + '" value="' + awbentry.MAWB + '" />' +
                  //  '<input type="hidden" id="hdnValue_' + i + '" value="' + awbentry.Value + '" />' +
                    '<input type="checkbox" id="chkawb_' + i  + '" checked /> '+
                    '<td>' + awbentry.AWBNo + '<br/>' + awbentry.AWBDate + '</td>' +
                    '<td>' + awbentry.Consignor + '<br/>' + awbentry.OriginCountry + '</td>' +
                    '<td>' + awbentry.Consignee + '<br/>' + awbentry.DestinationCountry + '</td>' +
                    '<td>' + awbentry.Contents + '</td>' +
                    '<td class="">' + awbentry.PCS + '<br/>' + awbentry.Weight + '<br/>' + awbentry.BagNo + '</td>' +
                    '<td class=""><input type="text" class="form-control" id="txtBagNo_' + i + '" value="' + awbentry.BagNo  + '"/></td>' +
                    '<td class=""><input type="text" class="form-select"  id="txtFwdAgent_' + i + '" value="' + awbentry.FAgentName + '" fagentid="' + awbentry.FAgentID + '"/></td>' +
                    '<td class=""><input type="text" class="form-control"  id="txtFwdAgentNo_' + i + '"/></td>' +
                    '<td class=""><input type="text" class="form-control textright"  id="txtFwdRate_' + i + '"/></td>' +
                    '<td class=""><input type="text" class="form-control textright3"  id="txtFwdVerifiedWeight_' + i + '"/></td>' +
                    '</tr>';

                $('#listContainer').append(html);
                SetAutocompleteFwdAgent(i);
                $('#txtAWBNo').val('');
                $('#txtAWBNo').focus();

            }
            else {
                $('#btnadd').removeAttr('disabled');
                $('#txtAWBNo').val('');
                $('#txtAWBNo').focus();
                alert(response.message);
            }

        }


    });

}

function AddBulkAWB() {
    debugger;
    $('#btnaddbulk').attr('disabled', 'disabled');
    $.ajax({
        type: "POST",
        url: "/ExportShipment/GetAWBDetail",
        datatype: "Json",
        data: { SearchOption: 2, id: $('#txtAWBNo').val(), OriginCountry: $('#txtOriginCountry').val(), DestinationCountry: $('#txtDestinationCountry').val(), CoLoaderID: $('#hdnCoLoaderID').val(), exportid: $('#ID').val() },
        success: function (response) {
            if (response.status == "ok") {
                debugger;
                if (response.data.length == 0) {
                    Swal.fire("AWB Search Status!", "AWB No. Not found", "success");
                    $('#listContainer').html('');
                    $('#btnaddbulk').removeAttr('disabled');
                    return false;
                }
                $('#listContainer').html('');
                $('#Selectall').prop('checked', true);
                for (var index = 0; index < response.data.length; index++) {

                    var data = response.data[index];


                    var dateString = data.AWBDate.substr(6);
                    var currentTime = new Date(parseInt(dateString));
                    var month = currentTime.getMonth() + 1;
                    var day = currentTime.getDate();
                    var year = currentTime.getFullYear();
                    var tempdate = day + "-" + month + "-" + year;

                    var awbentry = {
                        ShipmentDetailID: 0,
                        ExportID: $('#ID').val(),
                        InboundShipmentID: data.ShipmentID,
                        InscanId: data.InScanId,
                        AWBNo: data.AWBNo,
                        AWBDate: tempdate,
                        Consignor: data.Consignor,
                        Consignee: data.Consignee,
                        OriginCountry: data.ConsignorCountryName,
                        OriginCity: data.ConsignorCityName,
                        DestinationCountry: data.ConsigneeCountryName,
                        DestinationCity: data.ConsigneeCityName,
                        Contents: data.CargoDescription,
                        PCS: data.Pieces,
                        Weight: data.Weight,
                        FAgentID: $('#FAgentID').val(),
                        FAgentName: $('#FAgentID option:selected').text(),
                        BagNo: data.BagNo,
                        MAWB: $('#MAWB').val()

                    }
                    var entrymode = $('#btnAdd').attr('entrymode');
                    var i = $('#listContainer >tr').length;
                    var html = '<tr><td>' +
                        '<input type="hidden" id="hdnShipmentDetailID_' + i + '" value="' + awbentry.ShipmentDetailID + '" />' +
                        '<input type="hidden" id="hdnExportID_' + i + '" value="' + awbentry.ExporID + '" />' +
                        '<input type="hidden" id="hdnInboundShipmentID_' + i + '" value="' + awbentry.InboundShipmentID + '" />' +
                        '<input type="hidden" id="hdnInscanId_' + i + '" value="' + awbentry.InscanId + '" />' +
                        '<input type="hidden" id="hdnAWBNo_' + i + '" value="' + awbentry.AWBNo + '" />' +
                        '<input type="hidden" id="hdnAWBDate_' + i + '" value="' + awbentry.AWBDate + '" />' +

                        '<input type="hidden" id="hdnShipper_' + i + '" value="' + awbentry.Consignor + '" />' +
                        '<input type="hidden" id="hdnOriginCity_' + i + '" value="' + awbentry.OriginCity + '" />' +
                        '<input type="hidden" id="hdnOriginCountry_' + i + '" value="' + awbentry.OriginCountry + '" />' +
                        '<input type="hidden" id="hdnReceiver_' + i + '" value="' + awbentry.Consignee + '" />' +
                        '<input type="hidden" id="hdnDestinationCity_' + i + '" value="' + awbentry.DestinationCity + '" />' +
                        '<input type="hidden" id="hdnDestinationCountry_' + i + '" value="' + awbentry.DestinationCountry + '" />' +
                        '<input type="hidden" id="hdnBagNo_' + i + '" value="' + awbentry.BagNo + '" />' +
                        '<input type="hidden" id="hdnContents_' + i + '" value="' + awbentry.Contents + '" />' +
                        '<input type="hidden" id="hdnPCS_' + i + '" value="' + awbentry.PCS + '" />' +
                        '<input type="hidden" id="hdnWeight_' + i + '" value="' + awbentry.Weight + '" />' +
                        '<input type="hidden" id="hdnMAWB_' + i + '" value="' + awbentry.MAWB + '" />' +
                        //  '<input type="hidden" id="hdnValue_' + i + '" value="' + awbentry.Value + '" />' +
                        '<input type="checkbox" id="chkawb_' + i + '" checked /> ' +
                        '<td>' + awbentry.AWBNo + '<br/>' + awbentry.AWBDate + '</td>' +
                        '<td>' + awbentry.Consignor + '<br/>' + awbentry.OriginCountry + '</td>' +
                        '<td>' + awbentry.Consignee + '<br/>' + awbentry.DestinationCountry + '</td>' +
                        '<td>' + awbentry.Contents + '</td>' +
                        '<td class="">' + awbentry.PCS + '<br/>' + awbentry.Weight + '<br/>' + awbentry.BagNo + '</td>' +
                        '<td class=""><input type="text" class="form-control" id="txtBagNo_' + i + '" value="' + awbentry.BagNo + '"/></td>' +
                        '<td class=""><input type="text" class="form-control"  id="txtFwdAgent_' + i + '" value="' + awbentry.FAgentName + '" fagentid="' + awbentry.FAgentID + '"/></td>' +
                        '<td class=""><input type="text" class="form-control"  id="txtFwdAgentNo_' + i + '"/></td>' +
                        '<td class=""><input type="text" class="form-control textright"  id="txtFwdRate_' + i + '"/></td>' +
                        '<td class=""><input type="text" class="form-control textright3"  id="txtFwdVerifiedWeight_' + i + '"/></td>' +
                        '</tr>';

                    $('#listContainer').append(html);
                    SetAutocompleteFwdAgent(i);
                }

                $('#btnaddbulk').removeAttr('disabled');
            }
            else {
                $('#btnaddbulk').removeAttr('disabled');
                $('#txtAWBNo').val('');
                $('#txtAWBNo').focus();
                alert(response.message);
            }

        }


    });



}
function SaveExportShipment() {
    debugger;
    var maxrow = $('#listContainer > tr').length;
    if (maxrow == 0) {
        Swal.fire("Save Status!", "Add AWB No.", "error");
        return false;
    }
    $('#loaderpopup').modal('show');
    $('#h4LoaderTitle').html('Manifest Saving')
    $('#btnSave').attr('disabled', 'disabled');
      
    
    var manifestentry = {
        ID: $('#ID').val(),
        ManifestDate :$('#ManifestDate').val(),
        ManifestNumber: $('#ManifestNumber').val(),
        OriginAirportCity: $('#OriginAirportCity').val(),
        DestinationAirportCity: $('#DestinationAirportCity').val(),
        FlightDate: $('#FlightDate').val(),
        FlightNo: $('#FlightNo').val(),
        MAWB :$('#MAWB').val(),    
        CD: $('#CD').val(),
        RunNo: $('#RunNo').val(),
        Bags:$('#Bags').val(),
        RunNo:$('#RunNo').val(),
        Type:'Export',
        TotalAWB:$('#TotalAWB').val(),
        AgentID :$('#AgentID').val(),
        ShipmentTypeId :$('#ShipmentTypeId').val(),
        FAgentID: $('#FAgentID').val(),
        MAWBWeight: $('#MAWBWeight').val()
    }
    var items = [];
    var deleteditems = [];
    for (var i = 0; i < maxrow; i++) {
        var checked = $('#chkawb_' + i).prop('checked');
        if (checked == true) {
            var fagentid = 0;
            if ($('#txtFwdAgent_' + i).attr('fagentid') == '' || $('#txtFwdAgent_' + i).attr('fagentid') == 'undefined')
                fagentid = 0;
            else
                fagentid = $('#txtFwdAgent_' + i).attr('fagentid');
            var awbentry = {
                ShipmentDetailID: $('#hdnShipmentDetailID_' + i).val(),
                InboundShipmentID: $('#hdnInboundShipmentID_' + i).val(),
                InscanId: $('#hdnInscanId_' + i).val(),
                ExportID: $('#ID').val(), 
                AWBNo: $('#hdnAWBNo_' + i).val(),
                AWBDate: checkDate($('#hdnAWBDate_' + i)),
                Shipper: $('#hdnShipper_' + i).val(),
                Receiver: $('#hdnReceiver_' + i).val(),
                OriginCountry: $('#hdnOriginCountry_' + i).val(),
                OriginCity : $('#hdnOriginCity_' + i).val(),                   
                DestinationCountry: $('#hdnDestinationCountry_' + i).val(),
                DestinationCity: $('#hdnDestinationCity_' + i).val(),
                Contents: $('#hdnContents_' + i).val(),
                Weight: $('#hdnWeight_' + i).val(),
                PCS: $('#hdnPCS_'+i).val(),
                BagNo: $('#txtBagNo_' + i).val(),              
                FwdAgentId: fagentid,
                FwdAgentAWBNo: $('#txtFwdAgentNo_' + i).val(),
                FwdCharge: $('#txtFwdRate_' + i).val().replace(',',''),
                VerifiedWeight: $('#txtFwdVerifiedWeight_' + i).val(),
               // CurrencyID: $('#hdnCurrencyID_' + i).val(),
                //Currency: $('#hdnCurrency_' + i).val(),
                //CustomsValue: $('#hdnCustomsValue_' + i).val(),
                MAWB: $('#hdnMAWB_' + i).val(),
            }

            items.push(awbentry);

        }
        else if ($('#hdnShipmentDetailID_' + i).val()!=0 && checked==false) {
            var awbentry = {
                ShipmentDetailID: $('#hdnShipmentDetailID_' + i).val(),
                AWBNo: $('#hdnAWBNo_' + i).val()
              }

            deleteditems.push(awbentry);
        }
       
        if ((i + 1) == maxrow) {
            $.ajax({
                type: "POST",
                url: '/ExportShipment/SaveExportShipment/',
                datatype: "html",
                data: { model: manifestentry, Details: JSON.stringify(items), DeleteDetails: JSON.stringify(deleteditems) },
                success: function (response) {
                    debugger;
                    if (response.Status == "Ok") {

                        $('#loaderpopup').modal('hide');
                        
                        setTimeout(function () {
                            window.location.href = '/ExportShipment/CreateExport?id=' + response.ExportID;
                        }, 200)

                        Swal.fire("Save Status!", response.message, "success");
                        //window.location.reload();

                    }
                    else {
                        $('#loaderpopup').modal('hide');
                        $('#btnSave').removeAttr('disabled');
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

        var minDate = $('#hdnMinDate').val();
        var maxDate = $('#hdnMaxDate').val();
        var startdate = minDate;
        var enddate = maxDate;
        var sd = new Date(startdate.split('-')[1] + '-' + startdate.split('-')[0] + '-' + startdate.split('-')[2]);
        var ed = new Date(enddate.split('-')[1] + '-' + enddate.split('-')[0] + '-' + enddate.split('-')[2]);
        $('#FlightDate').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $('#OriginAirportCity').autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/ZoneChart/GetCityList",
                    data: {
                        SearchText: request.term
                    },
                    dataType: "json",
                    type: "GET",
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.City,
                                value: val.City,
                                Country: val.CountryName
                            }
                        }))
                    }
                });
            },
            minLength: 1,
            autoFocus: false,
            select: function (event, ui) {
                event.preventDefault();
                $('#OriginAirportCity').val(ui.item.label);
                //$('#DeliveryLocation').val(ui.item.Country);
                return false;
            },
            focus: function (event, ui) {
                $('#OriginAirportCity').val(ui.item.label);

                //$('#DeliveryLocation').val(ui.item.Country);

                return false;
            }
        });
        $('#DestinationAirportCity').autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/ZoneChart/GetCityList",
                    data: {
                        SearchText: request.term
                    },
                    dataType: "json",
                    type: "GET",
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.City,
                                value: val.City,
                                Country: val.CountryName
                            }
                        }))
                    }
                });
            },
            minLength: 1,
            autoFocus: false,
            select: function (event, ui) {
                event.preventDefault();
                $('#DestinationAirportCity').val(ui.item.label);
                //$('#DeliveryLocation').val(ui.item.Country);
                return false;
            },
            focus: function (event, ui) {
                $('#DestinationAirportCity').val(ui.item.label);

                //$('#DeliveryLocation').val(ui.item.Country);

                return false;
            }
        });

        $('#btnSingleAWB').click(function () {
            debugger;
            $('#SingleAWBpopup').modal('show');
        })
        $('#btnBulkAWB').click(function () {
            $('#BulkAWBpopup').modal('show');
        })
        $(document).on("click", "#btnUpload", function () {

            if ($('#CustomerID').val() == '' || $('#CustomerID').val() == '0') {
                Swal.fire("Data Importing Status!", "Select Customer!", "error");
                $('#CustomerName').focus();
                return false;
            }

            if ($('#CourierStatusID').val() == '' || $('#CourierStatusID').val() == '0') {
                Swal.fire("Data Importing Status!", "Select Status!", "error");
                $('#CourierStatusID').focus();
                return false;
            }

            $('#importFile').trigger('click');
        });
        $('#ShipmentTypeId').change(function () {
            debugger;
            //if ($('#ShipmentTypeId option:selected').text() == 'Domestic') {
                if ($('#ShipmentTypeId').val() == 1) {
                    $('#flightarea').addClass('hide');
                    $('#FAgentID').attr('disabled', 'disabled');
                    $('#AgentID').removeAttr('disabled');
                //$('#AgentID').addClass('hide');
                //$('#AgentID').removeAttr('required');
                //$('#FAgentID').removeClass('hide');
                //$('#FAgentID').attr('required', 'required');
            }
            else if ($('#ShipmentTypeId').val() == 2) {
                    $('#flightarea').removeClass('hide');
                    $('#FAgentID').removeAttr('disabled');
                    $('#AgentID').attr('disabled','disabled');
                //$('#FAgentID').addClass('hide');
                //$('#FAgentID').removeAttr('required');
                //$('#AgentID').removeClass('hide');
                //$('#AgentID').attr('required', 'required');

            }
        });

        //search dropdowndown
        $('#txtOriginCountry').autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/ZoneChart/GetCountryList",
                    data: { SearchText: request.term },
                    dataType: "json",
                    type: "GET",
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.CountryName,
                                value: val.CountryName
                            }
                        }))
                    }
                });
            },
            minLength: 1,
            autoFocus: false,
            select: function (event, ui) {

                $('#txtOriginCountry').val(ui.item.label);
                $('#txtOriginCountry').attr('label', ui.item.label);


            },
            focus: function (event, ui) {
                event.preventDefault();
                $('#txtOriginCountry').val(ui.item.label);
                $('#txtOriginCountry').attr('label', ui.item.label);


            }
        });
        $('#txtDestinationCountry').autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: "/ZoneChart/GetCountryList",
                    data: { SearchText: request.term },
                    dataType: "json",
                    type: "GET",
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.CountryName,
                                value: val.CountryName
                            }
                        }))
                    }
                });
            },
            minLength: 1,
            autoFocus: false,
            select: function (event, ui) {

                $('#txtDestinationCountry').val(ui.item.label);
                $('#txtDestinationCountry').attr('label', ui.item.label);


            },
            focus: function (event, ui) {
                event.preventDefault();
                $('#txtDestinationCountry').val(ui.item.label);
                $('#txtDestinationCountry').attr('label', ui.item.label);


            }
        });
        $("#txtColoader").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Shipment/GetCustomerName',
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
                $('#txtColoader').val(ui.item.label);
                $('#txtColoader').attr('customername', ui.item.label);
                $('#hdnCoLoaderID').val(ui.item.CustomerId);
                $('#hdnCoLoaderID').attr('value', ui.item.CustomerId);

            },
            select: function (e, i) {

                e.preventDefault();
                $("#txtColoader").val(i.item.label);
                $('#txtColoader').attr('customername', i.item.label);
                $('#hdnCoLoaderID').val(i.item.CustomerId);
                $('#hdnCoLoaderID').attr('value', i.item.CustomerId);

            },

        });
     
        var table = $('#datatable1').DataTable({
            "aaSorting": [],
            // "aaSorting": [[0, 'desc']],
            //"order": [[2, "asc"]],
            "searching": true,
            "bPaginate": false,
            "Sortable":false,
            //"pagingType": "simple",
            aoColumnDefs: [
                { "aTargets": [0], "bSortable": false } //"sType": "date"
            ]
        });
        if ($('#ID').val() > 0) {
            $('#btnSave').val('Update');
            $('#divothermenu').removeClass('hide');
            $('#divothermenu1').removeClass('hide');
            searchoption(1);
            $('#ChkSingleAWB').attr('disabled', 'disabled');

            $('#ShipmentTypeID').trigger('change');
            if ($('#hdnDetailCount').val() == 0) {
                $('#listContainer').html('');
            }
            else {
                TotalItems();
            }
            
        }
        else {
            $('#listContainer').html('');
            
            searchoption(1);
        }
        //$('#txtsearch').keyup(function () {
             
        //    var searchinvoiceno = $('#txtsearch').val();
        //    var filter = searchinvoiceno.toUpperCase();
        //    table = document.getElementById("datatable1");
        //    tr = $(table).find(tr);

        //    // Loop through all table rows, and hide those who don't match the search query
        //    for (i = 0; i < tr.length; i++) {
        //        td = tr[i].getElementsByTagName("td")[0];
        //        if (td) {
        //            txtValue = td.textContent || td.innerText;
        //            if (txtValue.toUpperCase().indexOf(filter) > -1) {
        //                tr[i].style.display = "";
        //            } else {
        //                tr[i].style.display = "none";
        //            }
        //        }
        //    }

        //})
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
                            SaveExportShipment();

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
            formData.append('ExportID', $('#ID').val());
            if (files.length > 0) {
                $('#btnUpload').attr('disabled', 'disabled');
                $.ajax({
                    url: '/ExportShipment/ImportFile',
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
                            $.ajax({
                                type: 'POST',
                                url: '/ExportShipment/ShowShipmentList',
                                datatype: "html",
                                success: function (data) {
                                    $('#EntrySource').val('EXL');
                                    $("#listContainer").html(data);
                                    //var max = $('#detailsbody > tr').length;
                                    //var table = $('#datatable1').DataTable({
                                    //    "aaSorting": [],
                                    //    "searching": true,
                                    //    "bPaginate": false,

                                    //});
                                    $('#loaderpopup').modal('hide');
                                    $('#btnUpload').removeAttr('disabled');
                                    //$('#TotalAWB').val(max);


                                }
                            });

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