
var _decimal = "2";

function SaveCustomerEntry() {

    if ($('#NewCustomerName').val() == '') {
        
        Swal.fire("Save Status!", "Enter Customer Name", "error");
        return;
    }
    else if ($('#NewPhone').val() == '') {
        Swal.fire("Save Status!", "Enter Customer Phone", "error");
        
        return;
    }
    else if ($('#NewCityName').val() == '' && $('#NewCountryName').val() == '') {
        Swal.fire("Save Status!", "Enter Customer Location Details", "error");
      
        return;
    }

    var cust = {
        CustomerName: $('#NewCustomerName').val(),
        Address1: $('#NewAddress1').val(),
        Address2: $('#NewAddress2').val(),
        Address3: $('#NewAddress3').val(),
        LocationName: $('#NewLocationName').val(),
        CityName: $('#NewCityName').val(),
        CountryName: $('#NewCountryName').val(),
        ContactPerson: $('#NewContactPerson').val(),
        Phone: $('#NewPhone').val(),
        Mobile: $('#NewMobile').val(),
        CustomerType: $('#CustomerType1').val()
    }

    $.ajax({
        type: 'POST',
        url: '/AWB/SaveCustomerEntry',
        datatype: "json",
        data: cust,
        success: function (response) {
            debugger;
            if (response.status == 'Ok') {

                $('#customer').val(response.data.CustomerName);
                $('#customer').attr('customername', response.data.CustomerName)
                $('#customerpopup').modal('hide');
                $('#customer').trigger('change');
                
                Swal.fire("Save Status!", response.message, "success");
            }
            else {
                $('#customerpopup').modal('hide');
                $('#customer').val(response.data.CustomerName);
               
                Swal.fire("Save Status!", response.message, "error");
                $('#customer').trigger('change');
            }
        }
    });
}
function showcustomerentry() {
    debugger;
    $.ajax({
        type: 'POST',
        url: '/AWB/ShowCustomerEntry',
        datatype: "html",
        data: {
            FieldName: 's'
        },
        success: function (data) {
            $("#customerContainer").html(data);
            $('#customerpopup').modal('show');
            setTimeout(function () {
                $('#NewCustomerName').focus();
            }, 300)
        }
    });
}

function showLocationEntry() {
    debugger;
    $.ajax({
        type: 'POST',
        url: '/AWB/ShowLocationEntry',
        datatype: "html",
        success: function (data) {
            $("#LocationContainer").html(data);
            $('#Locationpopup').modal('show');
            $('#LLocationName').focus();
        }
    });
}
function SaveLocationEntry() {

    if ($('#LLocationName').val() == '' && $('#LCityName').val() == '' && $('#LCountryName').val() == '') {
       
        Swal.fire("Save Status!", "Enter Location Details", "error");
        return;
    }
    else if ($('#LLocationName').val() != '' && ($('#LCityName').val() == '' || $('#LCountryName').val() == '')) {
      
        Swal.fire("Save Status!", "Enter Location's City/Country Details", "error");
        return;
    }
    var cust = {

        Location: $('#LLocationName').val(),
        CityName: $('#LCityName').val(),
        CountryName: $('#LCountryName').val()
    }

    $.ajax({
        type: 'POST',
        url: '/AWB/SaveLocationEntry',
        datatype: "json",
        data: cust,
        success: function (response) {
            debugger;
            if (response.status == 'Ok') {
                if ($('#lblconsignorLocation').attr('popup') == 'open') {
                    $('#ConsignorLocationName').val(response.data.LocationName);
                    $('#ConsignorCityName').val(response.data.CityName);
                    $('#ConsignorCountryName').val(response.data.CountryName);
                }
                else if ($('#lblconsigneeLocation').attr('popup') == 'open') {
                    $('#ConsigneeLocationName').val(response.data.LocationName);
                    $('#ConsigneeCityName').val(response.data.CityName);
                    $('#ConsigneeCountryName').val(response.data.CountryName);
                }
                $('#lblconsigneeLocation').attr('popup', '');
                $('#lblconsignorLocation').attr('popup', '');

                $('#Locationpopup').modal('hide');

               
                Swal.fire("Save Status!", response.message, "success");
            }
            else {

                if ($('#lblconsignorLocation').attr('popup') == 'open') {
                    $('#ConsignorLocationName').val(response.data.Location);
                    $('#ConsignorCityName').val(response.data.CityName);
                    $('#ConsignorCountryName').val(response.data.CountryName);
                }
                else if ($('#lblconsigneeLocation').attr('popup') == 'open') {
                    $('#ConsigneeLocationName').val(response.data.Location);
                    $('#ConsigneeCityName').val(response.data.CityName);
                    $('#ConsigneeCountryName').val(response.data.CountryName);
                }

                $('#lblconsigneeLocation').attr('popup', '');
                $('#lblconsignorLocation').attr('popup', '');

                $('#Locationpopup').modal('hide');
                Swal.fire("Save Status!", response.message, "error");

            }
        }
    });
}
function setTwoNumberDecimal(obj) {
    if ($(obj).val() == '') {
        $(obj.val(0));
    }
    $(obj).val(parseFloat($(obj).val()).toFixed(_decimal));
    CalculateTax();
}
function CalculateTax() {
    debugger;
    var tax = $('#ChkTaxPercent').prop('checked');
    var surcharge = $('#ChkSurcharge').prop('checked');
    var taxpercent = 0;
    var surchargepercent = 0;
    var taxval = 0;
    var surchargeval = 0;
    if (tax == true) {
        taxpercent = parseFloat($('#TaxPercent').val());
    }
    else if (tax == false) {
        $('#TaxAmount').val(0);
    }
    if (surcharge == true) {
        surchargepercent = parseFloat($('#SurchargePercent').val());
    }
    else if (surcharge == false) {
        $('#SurchargeAmount').val(0);
    }


    if ($('#CourierCharge').val() == '' || $('#CourierCharge').val() == null)
        $('#CourierCharge').val(0);
    if ($('#OtherCharge').val() == '' || $('#OtherCharge').val() == null)
        $('#OtherCharge').val(0);

    var charge = parseFloat($('#CourierCharge').val()) + parseFloat($('#OtherCharge').val());

    if (tax == true) {
        taxval = parseFloat(charge) * (parseFloat(taxpercent) / 100.00);
        $('#TaxAmount').val(parseFloat(taxval).toFixed(_decimal));
    }
    if (surcharge == true && parseFloat(surchargepercent) > 0) {
        var ccharge = parseFloat($('#CourierCharge').val());
        surchargeval = parseFloat(ccharge) * (parseFloat(surchargepercent) / 100.00);
        $('#SurchargeAmount').val(parseFloat(surchargeval).toFixed(_decimal));
    }

    var net = parseFloat(charge) + parseFloat(taxval) + parseFloat(surchargeval);

    $('#NetTotal').val(parseFloat(net).toFixed(_decimal));

}

function SetSurcharge() {
    debugger;
    var surcharge = $('#ChkSurcharge').prop('checked');
    if (surcharge == true) {
        $('#SurchargePercent').val(parseFloat($('#DefaultSurchargePercent').val()).toFixed(2));
        $('#SurchargePercent').removeAttr('readonly');
        //$('#SurchargeAmount').val(0);
        $('#SurchargePercent').focus();

        CalculateTax();
    }
    else {
        $('#SurchargePercent').val(0);
        $('#SurchargePercent').attr('readonly', 'readonly');
        $('#SurchargeAmount').val(0);
        CalculateTax();
    }
}
function getMovementType() {
       
    debugger;
    console.log($('#BranchCountry').val());
    if ($('#ConsignorCountryName').val() != $('#BranchCountry').val() && $('#ConsigneeCountryName').val() != $('#BranchCountry').val()) {
        $('#MovementID').val(4).trigger('change');//Transhipment
    }
    else if ($('#ConsignorCountryName').val() != $('#BranchCountry').val()) {
        $('#MovementID').val(3).trigger('change'); //import
        
    }
    else if ($('#ConsigneeCountryName').val() != $('#BranchCountry').val()) {
        $('#MovementID').val(2).trigger('change');//Export
        
    }
    else if ($('#ConsigneeCountryName').val() == $('#BranchCountry').val()) {
        $('#MovementID').val(1).trigger('change'); //Domestic
    }


}

function savemanualfixation() {
    debugger;
    var Field = $('#FieldName').val();
    var SourceValue = $('#SourceValue').val();
    var DestinationValue = $('#DestinationValue').val();
    var DestinationCountry = $('#DestinationCountry').val();
    var DestinationCity = $('#DestinationCity').val();
    var DestinationLocation = $('#DestinationLocation').val();
    $.ajax({
        type: 'POST',
        url: '/AWBImport/UpdateDataFixation1',
      //  datatype: "html",
        data: { TargetColumn: Field, SourceValue: SourceValue, TargetValue: DestinationCountry  },
        success: function (response) {
            if (response.status == "OK") {
                Swal.fire("Fixaton Status!", "Data Updated", "success");
                setTimeout(function () {
                    $('#fixationpopup').modal('hide');
                },200)
                
            }
            else {
                Swal.fire("Fixaton Status!", "Update Failed", "error");
            }
        }
    });
}
function savefixation() {
    debugger;
    var Field = $('#FieldName').val();
    var SourceValue = $('#SourceValue').val();
    var DestinationValue = $('#DestinationValue').val();
    var DestinationCountry = $('#DestinationCountry').val();
    var DestinationCity = $('#DestinationCity').val();
    var DestinationLocation = $('#DestinationLocation').val();
    var maxrow = $('#tblbody > tr').length;
    if (Field == 'ConsigneeLocationName')
        DestinationValue = DestinationLocation;
    else if (Field == 'ConsigneeCityName')
        DestinationValue = DestinationCity;
    else if (Field == 'ConsigneeCountryName')
        DestinationValue = DestinationCountry;

    var totalothercharge = 0;
    var items = [];
    for (i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        if (deleted == 'false' || deleted == '' || deleted == null) {
            var awbentry = {
                ShipmentID: $('#hdnShipmentID_' + i).val(),
                CustomerID: $('#hdnCustomerID_' + i).val(),
                AWBNo: $('#hdnAWBNo_' + i).val(),
                AWBDate: checkDate($('#hdnAWBDate_' + i)),
                PaymentModeId: $('#hdnPaymentModeId_' + i).val(),
                ConsignorCountryName: $('#hdnConsignorCountryName_' + i).val(),
                ConsignorCityName: $('#hdnConsignorCityName_' + i).val(),
                ConsignorLocationName: $('#hdnConsignorLocationName_' + i).val(),
                ConsignorPhone: $('#ConsignorPhone').val(),
                CustomerShipperSame: 0,//$('#hdnCustomerShipperSame_'+i).val(),
                Consignor: $('#hdnConsignor_' + i).val(),
                ConsignorAddress: $('#hdnConsignorAddress_' + i).val(),
                Consignee: $('#hdnConsignee_' + i).val(),
                ConsigneePhone: $('#hdnConsigneePhone_' + i).val(),
                ConsigneeCountryName: $('#hdnConsigneeCountryName_' + i).val(),
                ConsigneeCityName: $('#hdnConsigneeCityName_' + i).val(),
                ConsigneeLocationName: $('#hdnConsigneeLocationName_' + i).val(),
                ConsigneeAddress: $('#hdnConsigneeAddress_' + i).val(),
                MovementID: $('#hdnMovementID_' + i).val(),
                ProductTypeID: $('#hdnProductTypeID_' + i).val(),
                ParcelTypeID: $('#hdnParcelTypeID_' + i).val(),
                Remarks: $('#hdnRemarks_' + i).val(),
                MaterialCost: $('#hdnMaterialCost_' + i).val(),
                NetTotal: 0,//$('#hdnNetTotal_'+i).val(),
                CargoDescription: $('#hdnCargoDescription_' + i).val(),
                Pieces: $('#hdnPieces_' + i).val(),
                Weight: $('#hdnWeight_' + i).val(),
                Remarks: $('#hdnRemarks_' + i).val(),
                EntrySource: $('#hdnEntrySource_' + i).val(),
                CourierCharge: $('#hdnCourierCharge_' + i).val(),
                OtherCharge: $('#hdnOtherCharge_' + i).val(),
                BagNo: $('#hdnBagNo_' + i).val(),
                CurrencyID: $('#hdnCurrencyID_' + i).val(),
                Currency: $('#hdnCurrency_' + i).val(),
                CustomsValue: $('#hdnCustomsValue_' + i).val(),
                IsDeleted: $('#hdndeleted_' + i).val(),
                ParcelType: $('#hdnParcelType_' + i).val(),
                MovementType: $('#hdnMovementType_' + i).val(),
                ProductType: $('#hdnProductType_' + i).val(),
                MAWB: $('#hdnMAWB_' + i).val(),
            }

            items.push(awbentry);

        }

        if (maxrow == (i + 1)) {
            //UpdateDataFixation
            $.ajax({
                type: 'POST',
                url: '/AWBImport/UpdateDataFixation',
                datatype: "html",
                data: { TargetColumn: Field, SourceValue: SourceValue, TargetValue: DestinationValue, Details: JSON.stringify(items) },
                success: function (data) {
                    $("#listContainer").html(data);
                    Swal.fire("Fixaton Status!", "Data Updated", "success");
                }
            });
        }
    }    
}
function EditAWB(i) {
    $('#btnAdd').attr('entrymode', 'Update');
    $('#btnAdd').attr('entryindex', i);
    $('#btnAdd').html('Update');
    setrowactiverev(i);
    $('#InScanID').val($('#hdnInScanID_' + i).val());
    $('#AWBNo').val($('#hdnAWBNo_' + i).val());
    $('#TransactionDate').val($('#hdnAWBDate_' + i).val());
    $('#PaymentModeId').val($('#hdnPaymentModeId_'+ i).val());
    $('#CustomerID').val($('#hdnCustomerID_' + i).val());
    $('#ConsignorCountryName').val($('#hdnConsignorCountryName_' + i).val());
    $('#ConsignorCityName').val($('#hdnConsignorCityName_' + i).val());
    $('#ConsignorLocationName').val($('#hdnConsignorLocationName_' + i).val());
    $('#ConsignorPhone').val($('#hdnConsignorPhone_' + i).val());
    $('#ConsignorMobileNo').val($('#hdnConsignorMobileNo_' + i).val());
        //CustomerShipperSame: $('#CustomerShipperSame').val(),
    $('#Consignor').val($('#hdnConsignor_' + i).val());
    $('#ConsignorContact').val($('#hdnConsignorContact_' + i).val());
    $('#ConsignorAddress1_Building').val($('#hdnConsignorAddress1_Building_' + i).val());
    $('#ConsignorAddress2_Street').val($('#hdnConsignorAddress2_Street_' + i).val());
    $('#ConsignorAddress3_PinCode').val($('#hdnConsignorAddress3_PinCode_' + i).val());
    $('#Consignee').val($('#hdnConsignee_' + i).val());
    $('#ConsigneeContact').val($('#hdnConsigneeContact_' + i).val());
    $('#ConsigneePhone').val($('#hdnConsigneePhone_' + i).val());
    $('#ConsigneeMobileNo').val($('#hdnConsigneeMobileNo_' + i).val());
    $('#ConsigneeCountryName').val($('#hdnConsigneeCountryName_' + i).val());
    $('#ConsigneeCityName').val($('#hdnConsigneeCityName_' + i).val());
    $('#ConsigneeLocationName').val($('#hdnConsigneeLocationName_' + i).val());
    $('#ConsigneeAddress1_Building').val($('#hdnConsigneeAddress1_Building_' + i).val());
    $('#ConsigneeAddress2_Street').val($('#hdnConsigneeAddress2_Street_' + i).val());
    $('#ConsigneeAddress3_PinCode').val($('#hdnConsigneeAddress3_PinCode_' + i).val());
    $('#MovementID').val($('#hdnMovementID_' + i).val()).trigger('change');
    $('#ProductTypeID').val($('#hdnProductTypeID_' + i).val()).trigger('change');
    $('#ParcelTypeID').val($('#hdnParcelTypeID_' + i).val()).trigger('change');
    $('#Remarks').val($('#hdnRemarks_' + i).val());
    $('#ItemDescription').val($('#hdnCargoDescription_' + i).val());
    $('#MaterialCost').val($('#hdnMaterialCost_' + i).val());
    $('#CourierCharge').val($('#hdnCourierCharge_' + i).val());
    $('#OtherCharge').val($('#hdnOtherCharge_' + i).val());
    $('#NetTotal').val($('#hdnNetTotal_' + i).val());
    $('#FAgentID').val($('#hdnFAgentID_' + i).val());
    $('#ForwardingAWBNo').val($('#hdnForwardingAWBNo_' + i).val());
   
    //$('#TaxPercent').val($('#hdnTaxPercent_' + i).val());
    //if ($('#hdnTaxPercent_' + i).val() == '')
    //    $('#TaxPercent').val(5);

    if ($('#hdnTaxAmount_' + i).val() == '') {
        $('#hdnTaxAmount_' + i).val(0);
    }
    $('#TaxAmount').val($('#hdnTaxAmount_' + i).val());

    if (parseFloat($('#TaxAmount').val()) > 0) {
        $('#ChkTaxPercent').prop('checked', true);

    }
    else {
        $('#ChkTaxPercent').prop('checked', false);
    }
    $('#SurchargePercent').val($('#hdnSurchargePercent_' + i).val());
    $('#SurchargeAmount').val($('#hdnSurchargeAmount_' + i).val());
    if (parseFloat($('#SurchargePercent').val()) > 0) {
        $('#ChkSurcharge').prop('checked', true);
    } else {
        $('#ChkSurcharge').prop('checked', false);
    }
    $('#CustomsValue').val($('#hdnCustomsValue_' + i).val());
    
    
    //NetTotal: 0,//$('#totalCharge').val(),
    $('#CargoDescription').val($('#hdnCargoDescription_' + i).val());
    $('#Pieces').val($('#hdnPieces_' + i).val());
    $('#Weight').val($('#hdnWeight_' + i).val());
    
    $('#EntrySource').val($('#hdnEntrySource_' + i).val());// 2, //--batch add
    $('#CustomerRateID').val($('#hdnCustomerRateID_' + i).val());// 2, //--batch add            
    
    debugger;
    $('#BagNo').val($('#hdnBagNo_' + i).val());  
    
    $('#ManifestWeight').val($('#hdnManifestWeight_' + i).val());    
    $('#CurrencyID').val($('#hdnCurrencyID_' + i).val()).trigger('change');
    setTwoNumberDecimal($('#CourierCharge'));
    setTwoNumberDecimal($('#OtherCharge'));
    setTwoNumberDecimal($('#NetTotal'));
    setTwoNumberDecimal($('#CustomsValue'));
    setTwoNumberDecimal($('#MaterialCost'));
}
function AddAWB(i,t) {
    debugger;

    //if (false === t.checkValidity()) {
    //    $('#quickAWB').classList.add("was-validated");
    //    return false;
    //}
    //else {
    //    t.preventDefault();
    //    t.stopPropagation();
    //    e.classList.remove("was-validated");
    //}
    
    $('#btnAdd').attr('disabled', 'disabled');
    
    var awbentry = {
        InScanID: $('#InScanID').val(),
        AWBNo: $('#AWBNo').val(),
        AWBDate: $('#TransactionDate').val(),                                
        PaymentModeId: $('#PaymentTypeId').val(),
        CustomerID: $('#CustomerID').val(),        
        ConsignorCountryName: $('#ConsignorCountryName').val(),
        ConsignorCityName: $('#ConsignorCityName').val(),
        ConsignorLocationName: $('#ConsignorLocationName').val(),        
        ConsignorPhone: $('#ConsignorPhone').val(),        
        ConsignorMobileNo: $('#ConsignorMobileNo').val(),        
        CustomerShipperSame: $('#CustomerShipperSame').val(),
        Consignor: $('#Consignor').val(),
        ConsignorContact: $('#ConsignorContact').val(),
        ConsignorAddress1_Building: $('#ConsignorAddress1_Building').val(),        
        ConsignorAddress2_Street: $('#ConsignorAddress2_Street').val(),        
        ConsignorAddress3_PinCode: $('#ConsignorAddress3_PinCode').val(),        
        
        Consignee: $('#Consignee').val(),
        ConsigneeContact: $('#ConsigneeContact').val(),
        ConsigneePhone: $('#ConsigneePhone').val(),        
        ConsigneeMobileNo: $('#ConsigneeMobileNo').val(),        
        ConsigneeCountryName: $('#ConsigneeCountryName').val(),
        ConsigneeCityName: $('#ConsigneeCityName').val(),
        ConsigneeLocationName: $('#ConsigneeLocationName').val(),
        ConsigneeAddress1_Building: $('#ConsigneeAddress1_Building').val(),
        ConsigneeAddress2_Street: $('#ConsigneeAddress2_Street').val(),        
        ConsigneeAddress3_PinCode: $('#ConsigneeAddress3_PinCode').val(),        
        MovementID: $('#MovementID').val(),
        ProductTypeID: $('#ProductTypeID').val(),
        ParcelTypeId: $('#ParcelTypeID').val(),
        Remarks: $('#Remarks').val(),               
        MaterialCost: $('#MaterialCost').val(),
        CourierCharge: $('#CourierCharge').val(),
        OtherCharge: $('#OtherCharge').val(),
        SurchargePercent: $('#SurchargePercent').val(),
        TaxPercent :$('#TaxPercent').val(),
        TaxAmount: $('#TaxAmount').val(),
        SurchargeAmount: $('#SurchargeAmount').val(),
        NetTotal: $('#NetTotal').val(),
        CargoDescription: $('#CargoDescription').val(),
        Pieces: $('#Pieces').val(),
        Weight: $('#Weight').val(),
        EntrySource:$('#EntrySource').val(), //--batch add
        CustomerRateID :0,
        FAgentID: $('#FAgentID').val(),
        ForwardingAWBNo: $('#ForwardingAWBNo').val(),
        BagNo: $('#BagNo').val(),
       
        IsDeleted: false,
        ParcelType: $('#ParcelTypeID option:selected').text(),
        MovementType: $('#MovementID option:selected').text(),
        ProductType :$('#ProductTypeID option:selected').text(),
       
        CustomsValue: $('#CustomsValue').val(),
        CurrencyID: $('#CurrencyID').val(),
        ManifestWeight: $('#ManifestWeight').val()
     
    }
    var entrymode = $('#btnAdd').attr('entrymode');
    
    var html = '<td>' +
        '<input type="hidden" id="hdnCustomerID_' + i + '" value="' + awbentry.CustomerID  +  '" />' +
        '<input type="hidden" id="hdnInScanID_' + i + '" value="' + awbentry.InScanID + '" />' +
        '<input type="hidden" id="hdnAWBNo_' + i + '" value="' + awbentry.AWBNo + '" />' +
        '<input type="hidden" id="hdnAWBDate_' + i + '" value="' + awbentry.AWBDate + '" />' +
        '<input type="hidden" id="hdnPaymentModeId_' + i + '" value="3" />' +
        '<input type="hidden" id="hdnConsignor_' + i + '" value="' + awbentry.Consignor + '" />' +
        '<input type="hidden" id="hdnConsignorContact_' + i + '" value="' + awbentry.ConsignorContact + '" />' +
        '<input type="hidden" id="hdnConsignorAddress1_Building_' + i + '" value="' + awbentry.ConsignorAddress1_Building + '" />' +
        '<input type="hidden" id="hdnConsignorAddress2_Street_' + i + '" value="' + awbentry.ConsignorAddress2_Street + '" />' +
        '<input type="hidden" id="hdnConsignorAddress3_PinCode_' + i + '" value="' + awbentry.ConsignorAddress3_PinCode + '" />' +
        '<input type="hidden" id="hdnConsignorPhone_' + i + '" value="' + awbentry.ConsignorPhone + '" />' +
        '<input type="hidden" id="hdnConsignorMobileNo_' + i + '" value="' + awbentry.ConsignorMobileNo + '" />' +
        '<input type="hidden" id="hdnConsignorLocationName_' + i + '" value="' + awbentry.ConsignorLocationName + '" />' +
        '<input type="hidden" id="hdnConsignorCityName_' + i + '" value="' + awbentry.ConsignorCityName + '" />' +
        '<input type="hidden" id="hdnConsignorCountryName_' + i + '" value="' + awbentry.ConsignorCountryName + '" />' +
        '<input type="hidden" id="hdnConsignee_' + i + '" value="' + awbentry.Consignee + '" />' +
        '<input type="hidden" id="hdnConsigneeContact_' + i + '" value="' + awbentry.ConsigneeContact + '" />' +
        '<input type="hidden" id="hdnConsigneeAddress1_Building_' + i + '" value="' + awbentry.ConsigneeAddress1_Building + '" />' +
        '<input type="hidden" id="hdnConsigneeAddress2_Street_' + i + '" value="' + awbentry.ConsigneeAddress2_Street + '" />' +
        '<input type="hidden" id="hdnConsigneeAddress3_PinCode_' + i + '" value="' + awbentry.ConsigneeAddress3_PinCode + '" />' +
        '<input type="hidden" id="hdnConsigneePhone_' + i + '" value="' + awbentry.ConsigneePhone + '" />' +
        '<input type="hidden" id="hdnConsigneeMobileNo_' + i + '" value="' + awbentry.ConsigneeMobileNo + '" />' +
        '<input type="hidden" id="hdnConsigneeLocationName_' + i + '" value="' + awbentry.ConsigneeLocationName + '" />' +
        '<input type="hidden" id="hdnConsigneeCityName_' + i + '" value="' + awbentry.ConsigneeCityName + '" />' +
        '<input type="hidden" id="hdnConsigneeCountryName_' + i + '" value="' + awbentry.ConsigneeCountryName + '" />' +
        '<input type="hidden" id="hdnCourierCharge_' + i + '" value="' + awbentry.CourierCharge + '" />' +
        '<input type="hidden" id="hdnOtherCharge_' + i + '" value="' + awbentry.OtherCharge + '" />' +
        '<input type="hidden" id="hdnTaxPercent_' + i + '" value="' + awbentry.TaxPercent + '" />' +
        '<input type="hidden" id="hdnTaxAmount_' + i + '" value="' + awbentry.TaxAmount + '" />' +
        '<input type="hidden" id="hdnBagNo_' + i + '" value="' + awbentry.BagNo + '" />' +
        '<input type="hidden" id="hdnNetTotal_' + i + '" value="' + awbentry.NetTotal + '" />' +
        '<input type="hidden" id="hdnParcelTypeID_' + i + '" value="' + awbentry.ParcelTypeId + '" />' +
        '<input type="hidden" id="hdnProductTypeID_' + i + '" value="' + awbentry.ProductTypeID + '" />' +
        '<input type="hidden" id="hdnCargoDescription_' + i + '" value="' + awbentry.CargoDescription + '" />' +
        '<input type="hidden" id="hdnEntrySource_' + i + '" value="' + awbentry.EntrySource + '" />' +
        '<input type="hidden" id="hdnMovementID_' + i + '" value="' + awbentry.MovementID + '" />' +
        '<input type="hidden" id="hdnPieces_' + i + '" value="' + awbentry.Pieces + '" />' +
        '<input type="hidden" id="hdnWeight_' + i + '" value="' + awbentry.Weight + '" />' +
        '<input type="hidden" id="hdnMaterialCost_' + i + '" value="' + awbentry.MaterialCost + '" />' +
        '<input type="hidden" id="hdnSurchargePercent_' + i + '" value="' + awbentry.SurchargePercent + '" />' +
        '<input type="hidden" id="hdnSurchargeAmount_' + i + '" value="' + awbentry.SurchargeAmount + '" />' +
        '<input type="hidden" id="hdnRemarks_' + i + '" value="' + awbentry.Remarks + '" />' +
        
        '<input type="hidden" id="hdndeleted_' + i + '" value="' + awbentry.IsDeleted + '" />' +
        '<input type="hidden" id="hdnParcelType_' + i + '" value="' + awbentry.ParcelType + '" />' +
        '<input type="hidden" id="hdnMovementType_' + i + '" value="' + awbentry.MovementType + '" />' +
        '<input type="hidden" id="hdnProductType_' + i + '" value="' + awbentry.ProductType + '" />' +        
        '<input type="hidden" id="hdnCustomsValue_' + i + '" value="' + awbentry.CustomsValue + '" />' +
        '<input type="hidden" id="hdnCurrencyID_' + i + '" value="' + awbentry.CurrencyID + '" />' +
        '<input type="hidden" id="hdnFAgentID_' + i + '" value="' + awbentry.FAgentID + '" />' +
        '<input type="hidden" id="hdnForwardingAWBNo_' + i + '" value="' + awbentry.ForwardingAWBNo + '" />' +
        '<input type="hidden" id="hdnManifestWeight_' + i + '" value="' + awbentry.ManifestWeight + '" />' +
        awbentry.AWBNo + '<br/>' + awbentry.AWBDate  + '</td>' +                
        '<td>' + awbentry.Consignor + '<br/>' + awbentry.ConsignorCountryName +   '</td>' +
        '<td>' + awbentry.Consignee + '<br/>' + awbentry.ConsigneeCountryName + '</td>' +
        '<td class="">' + awbentry.MovementType + '<br/>' + awbentry.ParcelType + '<br/>' + awbentry.ProductType + '</td>' +
        '<td class="text-right">' + awbentry.Weight + '</td>' +
        '<td class="text-right">' + parseFloat(awbentry.CourierCharge).toFixed(_decimal) + '<br/>' + parseFloat(awbentry.OtherCharge).toFixed(_decimal) + '<br/>' + parseFloat(awbentry.NetTotal).toFixed(_decimal) + '</td>' +
        '<td><div class="d-flex gap-3">' +
        '<a href="javascript:void(0)" onclick="EditAWB(' + i + ')" class="text-success"><i class="mdi mdi-pencil font-size-18"></i></a>' +
        '<a href="#" class="text-danger" onclick="DeleteAWB(' + i + ')"><i class="mdi mdi-delete font-size-18"></i></a>' +
        '</div></td>';

    if (entrymode == 'Add') {
        html = '<tr id="trrow_' + i + '">' + html + '</tr>';
        $('#tblbody').append(html);

    }
    else {
        
        $('#trrow_' + i).html(html);
    }
    clearcontrols();
    $('#btnAdd').attr('entrymode', 'Add');
    $('#btnAdd').attr('entryindex', -1);
    $('#btnAdd').html('Add');
     
}
function ShowBatchShipmentStatusModal() {

    $('#StatusModal').modal('show');

    

}

function showimportapi() {
    Swal.fire('Data Validation', 'Import API Option not  Enabled', 'error');
}

function savetrackstatus() {
    debugger;
    $.ajax({
        type: "POST",
        url: "/AWBImport/SaveBatchTrackStatus",
        datatype: "Json",
        data: {
            'BatchID': $('#ID').val(), NewCourierStatusID: $('#NewCourierStatusID').val(),EntryDate:$('#txtChangeDate').val(),Remarks:$('#txtChangeRemarks').val() },
        success: function (response) {
            debugger;
            console.log(response);
            if (response.Status == "OK") {
                Swal.fire("Save Status!", response.message, "success");
                setTimeout(function () {
                    window.location.href = '/AWBImport/Create?id=' + response.BatchID;
                }, 200)
            } else {
                Swal.fire("Save Status!", response.message, "error");
            }

        }
    });
}
function setrowactiverev(index1) {
    var idtext = 'trrow_'
    $('[id^=' + idtext + ']').each(function (index, item) {
        $('#trrow_' + index).removeClass('rowactive');
    });
    $('#trrow_' + index1).addClass('rowactive');
}
function clearcontrols() {
    $('#ShipmentID').val(0);
    $('#AWBNo').val('');

    var d = new Date();
    var curr_date = d.getDate();
    var curr_month = d.getMonth() + 1;
    var curr_year = d.getFullYear();

    var reqdate = curr_date + "-" + curr_month + "-" + curr_year; //+ ' ' + d.getHours() + ':' + d.getMinutes();
    
    $('#TransactionDate').val(reqdate);
    $('#PaymentModeId').val(3);
    //$('#CustomerID').val($('#hdnCustomerID_' + i).val());
    $('#ConsignorCountryName').val('');
    $('#ConsignorCityName').val('');
    $('#ConsignorLocationName').val('');
    $('#ConsignorPhone').val('');
    $('#ConsignorMobileNo').val('');
    //CustomerShipperSame: $('#CustomerShipperSame').val(),
    $('#Consignor').val('');
    $('#ConsignorContact').val('');
    $('#ConsignorAddress1_Building').val('');
    $('#ConsignorAddress2_Street').val('');
    $('#ConsignorAddress3_PinCode').val('');
    $('#Consignee').val('');
    $('#ConsigneeContact').val('');
    $('#ConsigneePhone').val('');
    $('#ConsigneeMobileNo').val('');
    $('#ConsigneeCountryName').val('');
    $('#ConsigneeCityName').val('');
    $('#ConsigneeLocationName').val('');
     
    $('#ConsigneeAddress1_Building').val('');
    $('#ConsigneeAddress2_Street').val('');
    $('#ConsigneeAddress3_PinCode').val('');
    $('#MovementID').val('').trigger('change');
    $('#ProductTypeID').val('').trigger('change');
    $('#ParcelTypeID').val('').trigger('change');
    $('#Remarks').val('');
    $('#MaterialCost').val(0);
    //NetTotal: 0,//$('#totalCharge').val(),
    $('#CargoDescription').val('');
    $('#Pieces').val('');
    $('#Weight').val(0);
    $('#EntrySource').val(2);// 2, //--batch add
    $('#CustomerRateID').val(0);// 2, //--batch add            
    //$('#FAgentID').val($('#hdnBagNo_' + i).val());
    $('#BagNo').val('');
    $('#CourierCharge').val(0);
    $('#OtherCharge').val(0);    
    
    $('#Currency').val('');
    $('#CustomsValue').val(0);
    $('#MAWB').val('');
    $('#ManifestWeight').val('');
    $('#NetTotal').val('');
    $('#SurchargePerecent').val('');
    $('#SurchargeAmount').val('');
    //$('#TaxPercent').val('');
    $('#TaxAmount').val('');
    $('#CurrencyID').val($('#DefaultCurrencyId').val()).trigger('change');
     $('#ChkTaxPercent').prop('checked',false);
     $('#ChkSurcharge').prop('checked',false);
    $('#btnAdd').removeAttr('disabled');
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
function DeleteAWB(i) {
    $('#trrow_' + i).addClass('hide');
    $('#hdndeleted_' + i).val('true');


}

function DeleteBatchAWB(items) {
    debugger;
            $.ajax({
                type: "POST",
                url: '/AWBImport/DeleteBatchAWB/',
                datatype: "html",
                data: { BatchID: $('#BATCHID').val(), BatchDate: $('#BatchDate').val(), Details: JSON.stringify(items) },
                success: function (response) {
                    debugger;
                    if (response.Status == "OK") {
                        return "OK";                                                 
                    }
                    else {
                        return response.message;
                    }

                }
            });       
}
function SaveAWB() {
    debugger;
    //if ($('#BatchCustomerID').val() == '' || $('#BatchCustomerID').val() == '0') {
    //    Swal.fire("Data Importing Status!", "Select Customer!", "error");
    //    $('#CustomerName').focus();
    //    return false;
    //}

    if ($('#CourierStatusID').val() == '' || $('#CourierStatusID').val() == '0') {
        Swal.fire("Data Importing Status!", "Select Status!", "error");
        $('#CourierStatusID').focus();
        return false;
    }
    var maxrow = $('#tblbody > tr').length;
    if (maxrow == 0) {
        Swal.fire("Save Status!", "Add AWB to save", "error");
        return false;
    }
    $('#btnSaveBatch').attr('disbled', 'disabled');    
    $('#h4LoaderTitle').html('Data Saving');    
    $('#loaderpopup').modal('show');
    var items = [];
    var deleteditems = [];
    for (var i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        if (deleted=='False' ||  deleted == 'false' || deleted == '' || deleted == null) {
            var awbentry = {
                InScanID: $('#hdnInScanID_' + i).val(),
                CustomerID: $('#hdnCustomerID_' + i).val(),
                AWBNo: $('#hdnAWBNo_' + i).val(),
                AWBDate: checkDate($('#hdnAWBDate_' + i)),
                PaymentModeId: $('#hdnPaymentModeId_' + i).val(),
                ConsignorCountryName: $('#hdnConsignorCountryName_' + i).val(),
                ConsignorCityName: $('#hdnConsignorCityName_' + i).val(),
                ConsignorLocationName: $('#hdnConsignorLocationName_' + i).val(),
                
                CustomerShipperSame: 0,//$('#hdnCustomerShipperSame_'+i).val(),
                Consignor: $('#hdnConsignor_' + i).val(),
                ConsignorContact: $('#hdnConsignorContact_' + i).val(),
                ConsignorAddress1_Building: $('#hdnConsignorAddress1_Building_' + i).val(),
                ConsignorAddress2_Street: $('#hdnConsignorAddress2_Street_' + i).val(),
                ConsignorAddress3_PinCode: $('#hdnConsignorAddress3_PinCode_' + i).val(),
                ConsignorPhone: $('#hdnConsignorPhone_' +i).val(),
                ConsignorMobileNo: $('#hdnConsignorMobileNo_' +i).val(),
                Consignee: $('#hdnConsignee_' + i).val(),
                ConsigneeContact: $('#hdnConsigneeContact_' + i).val(),
                ConsigneePhone: $('#hdnConsigneePhone_' + i).val(),
                ConsigneeMobileNo: $('#hdnConsigneeMobileNo_' + i).val(),
                ConsigneeCountryName: $('#hdnConsigneeCountryName_' + i).val(),
                ConsigneeCityName: $('#hdnConsigneeCityName_' + i).val(),
                ConsigneeLocationName: $('#hdnConsigneeLocationName_' + i).val(),
                ConsigneeAddress1_Building: $('#hdnConsigneeAddress1_Building_' + i).val(),
                ConsigneeAddress2_Street: $('#hdnConsigneeAddress2_Street_' + i).val(),
                ConsigneeAddress3_PinCode: $('#hdnConsigneeAddress3_PinCode_' + i).val(),
                MovementID: $('#hdnMovementID_' + i).val(),
                ProductTypeID: $('#hdnProductTypeID_' + i).val(),
                ParcelTypeID: $('#hdnParcelTypeID_' + i).val(),
                Remarks: $('#hdnRemarks_' + i).val(),
                MaterialCost: $('#hdnMaterialCost_' + i).val(),
                NetTotal: $('#hdnNetTotal_'+i).val(),
                CargoDescription: $('#hdnCargoDescription_' + i).val(),
                Pieces: $('#hdnPieces_' + i).val(),
                Weight: $('#hdnWeight_' + i).val(),
                Remarks: $('#hdnRemarks_' + i).val(),
                EntrySource: $('#hdnEntrySource_' + i).val(),
                CourierCharge: $('#hdnCourierCharge_' + i).val(),
                OtherCharge: $('#hdnOtherCharge_' + i).val(),
                SurchargePercent: $('#hdnSurchargePercent_' + i).val(),
                SurchargeAmount: $('#hdnSurchargeAmount_' + i).val(),
                TaxPercent: $('#hdnTaxPercent_' + i).val(),
                TaxAmount: $('#hdnTaxAmount_' + i).val(),
               /* BagNo: $('#hdnBagNo_' + i).val(),*/
                 CurrencyID: $('#hdnCurrencyID_' + i).val(),
                Currency: $('#hdnCurrency_' + i).val(),
                CustomsValue: $('#hdnCustomsValue_' + i).val(),
                /*MAWB: $('#hdnMAWB_' + i).val(),*/
                ManifestWeight: $('#hdnManifestWeight_' + i).val(),
                FAgentID : $('#hdnFAgentID_' + i).val(),
                ForwardingAWBNo: $('#hdnForwardingAWBNo_' + i).val()
                /*Route: $('#hdnRoute_'+i).val()*/
            }

            items.push(awbentry);

        }
        else if ($('#hdnInScanID_' + i).val() != 0) {
            var awbentry = {
                InScanID: $('#hdnInScanID_' + i).val(),
                AWBNo: $('#hdnAWBNo_' + i).val(),
            }

            deleteditems.push(awbentry);
        }

        if ((i + 1) == maxrow) {

            var vm = {
                ID: $('#ID').val(),
                BatchDate: $('#BatchDate').val(),                
                EntrySource: $('#EntrySource').val(),
                CourierStatusID: $('#CourierStatusID').val(),                
                Remarks :$('#Remarks').val()
                
            }
            //BatchID: $('#BATCHID').val(), BatchDate: $('#BatchDate').val(), CustomerID: $('#CustomerID').val(), EntrySource: $('#EntrySource').val(), CourierStatusID: $('#CourierStatusID').val()
            $.ajax({
                type: "POST",
                url: '/AWBImport/SaveBatchAWB/',
                datatype: "html",
                data: { vm:vm, Details: JSON.stringify(items),DeleteDetails:JSON.stringify(deleteditems) },
                success: function (response) {
                    debugger;
                    if (response.Status == "OK") {
                        $('#loaderpopup').modal('hide');
                        Swal.fire("Save Status!", response.message, "success");
                        setTimeout(function () {
                            window.location.href = '/AWBImport/Index';
                        }, 200)
                         
                        
                       

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

        var minDate = $('#hdnMinDate').val();
        var maxDate = $('#hdnMaxDate').val();
        var startdate = minDate;
        var enddate = maxDate;
        var sd = new Date(startdate.split('-')[1] + '-' + startdate.split('-')[0] + '-' + startdate.split('-')[2]);
        var ed = new Date(enddate.split('-')[1] + '-' + enddate.split('-')[0] + '-' + enddate.split('-')[2]);
        $('#TransactionDate').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $('#BatchDate').datepicker({
            dateFormat: 'dd-mm-yy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        //paymenttype change
        $('#PaymentModeId').change(function () {
            debugger;
            if ($('#PaymentModeId').attr('readonly') == 'readonly') {
                $('#PaymentModeId').val($('#PaymentModeId').attr('paymentid'));
                return false;
            }
            var modeid = $('#PaymentModeId').val();
            if (modeid == 1) { //PKP
                $('#divcustomer').addClass('hide');
                $('#divcash').removeClass('hide');
                if ($('#DRRProcess').val() == 'True') {
                    $('#divcustomer').removeClass('hide');
                    $('#divcash').addClass('hide');
                    $('#AcHeadName').removeAttr('required');
                    $('#CustomerID').val($('#CASHCustomerId').val());
                    $('#customer').val($('#CASHCustomerName').val());
                    $('#customer').attr('readonly', 'readonly');
                }
                else if ($('#DRRID').val() == 0 || $('#DRRID').val() == null) {
                    $('#AcHeadName').removeAttr('readonly');
                    $('#AcHeadName').attr('required', 'required');
                }
                else {
                    $('#AcHeadName').attr('readonly', 'readonly');
                    $('#AcheadID').val(0);
                }
                $('#shippername').removeAttr('readonly');
            }
            else {
                $('#divcustomer').removeClass('hide');
                $('#divcash').addClass('hide');
                $('#AcHeadName').removeAttr('required');
            }

            if (modeid == 1 || modeid == 2 || modeid == 4) { //PKP COD FOC
                $("#customer").attr("readonly", "readonly");
                $('#shippername').removeAttr('readonly');
                $("#CustomerandShipperSame").attr("disabled", "disabled");
                if (modeid == 1) { //cash

                    $('#customer').val($('#CASHCustomerName').val());
                    $('#CustomerID').val($('#CASHCustomerId').val());
                    $('#AcHeadName').focus();
                }
                else if (modeid == 2) { //cod
                    $('#customer').val($('#CODCustomerName').val());
                    $('#CustomerID').val($('#CODCustomerID').val());
                    $('#customer').attr('readonly', 'readonly');
                    $('#shippername').focus();
                }
                else if (modeid == 4) { //FOC
                    $('#customer').val($('#FOCCustomerName').val());
                    $('#CustomerID').val($('#FOCCustomerID').val());
                    $('#customer').attr('readonly', 'readonly');
                    $('#shippername').focus();
                }

                // fillcustomerdetail(modeid);

            }
            else {
                if ($('#CustomerID').val() == $('#CASHCustomerId').val() || $('#CustomerID').val() == $('#CODCustomerID').val()) {
                    $('#customer').val('');
                    $('#CustomerID').val(0);
                }
                $("#customer").removeAttr("readonly");
                $("#CustomerandShipperSame").removeAttr("disabled", "disabled");
                $("#customer").focus();
            }

            //if ($('#CustomerID').val() == 0 && modeid != 1) {
            //    alert("For New Cash Customer....Selected Payment Type is not allowed");
            //    $('#PaymentModeId').val(1).trigger('change');
            //}
            //else {

            //    if ($("#hdnCustomerType").val() == "CS"  && modeid != 1) {
            //        //alert("Not a Credit Customer....Selected Payment Type is not allowed");
            //        alert("For Cash Customer....Selected Payment Type is not allowed");
            //        $('#PaymentModeId').val(1).trigger('change');
            //    }
            //}

        });
        $("#CustomerName").autocomplete({
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
                $('#CustomerName').val(ui.item.label);
                $('#CustomerName').attr('customername', ui.item.label);
                $('#BatchCustomerID').val(ui.item.CustomerId);
                $('#BatchCustomerID').attr('value', ui.item.CustomerId);
                $('#hdnCustomerType').val(ui.item.type);
            },
            select: function (e, i) {

                e.preventDefault();
                $("#CustomerName").val(i.item.label);
                $('#CustomerName').attr('customername', i.item.label);
                $('#BatchCustomerID').val(i.item.CustomerId);
                $('#BatchCustomerID').attr('value', i.item.CustomerId);
                $('#hdnCustomerType').val(i.item.type);
            },

        });
      
        $("#Consignor").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Shipment/GetShipperName',
                    datatype: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.ShipperName,
                                value: val.ShipperName,
                                AcHeadID: val.AcHeadID,
                                ContactPerson: val.ContactPerson,
                                Address1: val.Address1,
                                Address2: val.Address2,
                                Pincode: val.PinCode,
                                Phone: val.Phone,
                                CountryName: val.CountryName,
                                CityName: val.CityName,
                                LocationName: val.LocationName,
                                MobileNo: val.ConsignorMobileNo

                            }
                        }))
                    }
                })
            },
            minLength: 1,
            autoFocus: false,
            focus: function (event, i) {
                $('#Consignor').val(i.item.value);
                $('#ConsignorContact').val(i.item.ContactPerson);
                $("#ConsignorAddress1_Building").val(i.item.Address1);
                $("#ConsignorAddress2_Street").val(i.item.Address2);
                $("#ConsignorAddress3_PinCode").val(i.item.Pincode);
                $("#ConsignorPhone").val(i.item.Phone);
                $("#ConsignorMobileNo").val(i.item.MobileNo);
                $("#ConsignorCountryName").val(i.item.CountryName);
                $("#ConsignorCityName").val(i.item.CityName);
                $("#ConsignorLocationName").val(i.item.LocationName);
                getMovementType();
            },
            select: function (e, i) {
                e.preventDefault();
                $("#Consignor").val(i.item.label);
                $('#ConsignorContact').val(i.item.ContactPerson);
                $("#ConsignorAddress1_Building").val(i.item.Address1);
                $("#ConsignorAddress2_Street").val(i.item.Address2);
                $("#ConsignorAddress3_PinCode").val(i.item.Pincode);
                $("#ConsignorPhone").val(i.item.Phone);
                $("#ConsignorMobileNo").val(i.item.MobileNo);
                $("#ConsignorCountryName").val(i.item.CountryName);
                $("#ConsignorCityName").val(i.item.CityName);
                $("#ConsignorLocationName").val(i.item.LocationName);
                getMovementType();

            },

        });
        $("#Consignee").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/Shipment/GetConsigneeName', //'/AWB/GetConsigneeName',
                    datatype: "json",
                    data: {
                        term: request.term, Shipper: $('#Consignor').val(), ShowAll: $('#ShowAllConsignee').prop('checked')
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.ConsignorName,
                                value: val.ConsignorName,
                                ContactPerson: val.ContactPerson,
                                Address1: val.Address1,
                                Address2: val.Address2,
                                Pincode: val.PinCode,
                                Phone: val.Phone,
                                CountryName: val.CountryName,
                                CityName: val.CityName,
                                LocationName: val.LocationName                                

                            }
                        }))
                    }
                })
            },
            minLength: 1,
            autoFocus: false,
            focus: function (event, i) {
                $("#Consignee").val(i.item.label);                
                $('#ConsigneeContact').val(i.item.ContactPerson);
                $("#ConsigneeAddress1_Building").val(i.item.Address1);
                $("#ConsigneeAddress2_Street").val(i.item.Address2);
                $("#ConsigneeAddress3_PinCode").val(i.item.Pincode);
                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeMobileNo").val(i.item.MobileNo);
 
                $("#ConsigneeCountryName").val(i.item.CountryName);
                $("#ConsigneeCityName").val(i.item.CityName);
                $("#ConsigneeLocationName").val(i.item.LocationName);                                
                getMovementType();
            },
            select: function (e, i) {
                e.preventDefault();
                $("#Consignee").val(i.item.label);
                $('#ConsigneeContact').val(i.item.ContactPerson);
                $("#ConsigneeAddress1_Building").val(i.item.Address1);
                $("#ConsigneeAddress2_Street").val(i.item.Address2);
                $("#ConsigneeAddress3_PinCode").val(i.item.Pincode);
                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeMobileNo").val(i.item.MobileNo);

                
                $("#ConsigneeCountryName").val(i.item.CountryName);
                $("#ConsigneeCityName").val(i.item.CityName);
                $("#ConsigneeLocationName").val(i.item.LocationName);                                
                getMovementType();
            },

        });
        $('#btnClear').click(function () {
            $('#btnAdd').attr('entrymode', 'Add');
            $('#btnAdd').attr('entryindex', -1);
            $('#btnAdd').html('Add');
            clearcontrols();
        })
        $('#btnSaveBatch').click(function () {
            SaveAWB();
        })      
      
        if ($('#ID').val() > 0) {
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
        //$('#btnAdd').click(function (t) {
        //    if ($('#btnAdd').attr('entrymode') == 'Add') {
        //        var itemcount = $('#tblbody').length;
        //        $('#btnAdd').attr('entryindex', -1);
        //        AddAWB(itemcount,t);
        //    }
        //    else {
        //        var itemindex = $('#btnAdd').attr('entryindex');
        //        AddAWB(itemindex,t);
        //    }
        //})
        
        $(document).on("click", "#btnUpload", function () {

           
            if ($('#CourierStatusID').val() == '' || $('#CourierStatusID').val() == '0') {
                Swal.fire("Data Importing Status!", "Select Status!", "error");
                $('#CourierStatusID').focus();
                return false;
            }
                
            $('#importFile').trigger('click');
        });
        $(document).on("click", "#btnFixation", function () {

 
            debugger;
            var maxrow = $('#tblbody > tr').length;
            var totalothercharge = 0;
            var items = [];
            for (var r = 0; r < maxrow; r++) {
                debugger;
                var deleted = $('#hdndeleted_' + i).val();
                if (deleted == 'false' || deleted == '' || deleted == null) {
                   
                        var awbentry = {
                            ShipmentID: $('#hdnShipmentID_' + r).val(),
                            CustomerID: $('#hdnCustomerID_' + r).val(),
                            AWBNo: $('#hdnAWBNo_' + r).val(),
                            AWBDate: checkDate($('#hdnAWBDate_' + r)),
                            PaymentModeId: $('#hdnPaymentModeId_' + r).val(),
                            ConsignorCountryName: $('#hdnConsignorCountryName_' + r).val(),
                            ConsignorCityName: $('#hdnConsignorCityName_' + r).val(),
                            ConsignorLocationName: $('#hdnConsignorLocationName_' + r).val(),

                            CustomerShipperSame: 0,//$('#hdnCustomerShipperSame_'+i).val(),
                            Consignor: $('#hdnConsignor_' + r).val(),
                            ConsignorContact: $('#hdnConsignorContact_' + r).val(),
                            ConsignorAddress1_Building: $('#hdnConsignorAddress1_Building_' + r).val(),
                            ConsignorAddress2_Street: $('#hdnConsignorAddress2_Street_' + r).val(),
                            ConsignorAddress3_PinCode: $('#hdnConsignorAddress3_PinCode_' + r).val(),
                            ConsignorPhone: $('#hdnConsignorPhone_' + r).val(),
                            ConsignorMobileNo: $('#hdnConsignorMobileNo_' + r).val(),
                            Consignee: $('#hdnConsignee_' + r).val(),
                            ConsigneeContact: $('#hdnConsigneeContact_' + r).val(),
                            ConsigneePhone: $('#hdnConsigneePhone_' + r).val(),
                            ConsigneeMobileNo: $('#hdnConsigneeMobileNo_' + r).val(),
                            ConsigneeCountryName: $('#hdnConsigneeCountryName_' + r).val(),
                            ConsigneeCityName: $('#hdnConsigneeCityName_' + r).val(),
                            ConsigneeLocationName: $('#hdnConsigneeLocationName_' + r).val(),
                            ConsigneeAddress1_Building: $('#hdnConsigneeAddress1_Building_' + r).val(),
                            ConsigneeAddress2_Street: $('#hdnConsigneeAddress2_Street_' + r).val(),
                            ConsigneeAddress3_PinCode: $('#hdnConsigneeAddress3_PinCode_' + r).val(),
                            MovementID: $('#hdnMovementID_' + r).val(),
                            ProductTypeID: $('#hdnProductTypeID_' + r).val(),
                            ParcelTypeID: $('#hdnParcelTypeID_' + r).val(),
                            Remarks: $('#hdnRemarks_' + r).val(),
                            MaterialCost: $('#hdnMaterialCost_' + r).val(),
                            NetTotal: $('#hdnNetTotal_' + r).val(),
                            CargoDescription: $('#hdnCargoDescription_' + r).val(),
                            Pieces: $('#hdnPieces_' + r).val(),
                            Weight: $('#hdnWeight_' + r).val(),
                            Remarks: $('#hdnRemarks_' + r).val(),
                            EntrySource: $('#hdnEntrySource_' + r).val(),
                            CourierCharge: $('#hdnCourierCharge_' + r).val(),
                            OtherCharge: $('#hdnOtherCharge_' + r).val(),
                            SurchargePercent: $('#hdnSurchargePercent_' + r).val(),
                            SurchargeAmount: $('#hdnSurchargeAmount_' + r).val(),
                            TaxPercent: $('#hdnTaxPercent_' + r).val(),
                            TaxAmount: $('#hdnTaxAmount_' + r).val(),
                            BagNo: $('#hdnBagNo_' + r).val(),
                            CurrencyID: $('#hdnCurrencyID_' + r).val(),
                            Currency: $('#hdnCurrency_' + r).val(),
                            CustomsValue: $('#hdnCustomsValue_' + r).val(),
                            MAWB: $('#hdnMAWB_' + r).val(),
                            ManifestWeight: $('#hdnManifestWeight_' + r).val(),
                            FAgentID: $('#hdnFAgentID_' + r).val(),
                            ForwardingAWBNo: $('#hdnForwardingAWBNo_' + r).val(),
                            Route: $('#hdnRoute_' + r).val()
                        }

                        items.push(awbentry);

                    
                    
                }

                if ((r + 1) == maxrow) {
                    $.ajax({
                        type: 'POST',
                        url: '/Shipment/ShowImportDataFixation',
                        datatype: "html",
                        data: {
                            FieldName: 'DestinationLocation', Details: JSON.stringify(items)
                        },
                        success: function (data) {
                            $("#fixationpopupContainer").html(data);
                            $('#fixationpopup').modal('show');
                        }
                    });
                }
            }



        })

        $(document).on("click", "#btnAutoFixation", function () {
            debugger;
            $('#h4LoaderTitle').html('Data Fixation');
            $('#loaderpopup').modal('show');
            var maxrow = $('#tblbody > tr').length;
            var totalothercharge = 0;
            var items = [];
            for (var i = 0; i < maxrow; i++) {
                var deleted = $('#hdndeleted_' + i).val();
                if (deleted == 'false' || deleted == '' || deleted == null) {
                    var awbentry = {
                        ShipmentID: $('#hdnShipmentID_' + i).val(),
                        CustomerID: $('#hdnCustomerID_' + i).val(),
                        AWBNo: $('#hdnAWBNo_' + i).val(),
                        AWBDate: checkDate($('#hdnAWBDate_' + i)),
                        PaymentModeId: $('#hdnPaymentModeId_' + i).val(),
                        ConsignorCountryName: $('#hdnConsignorCountryName_' + i).val(),
                        ConsignorCityName: $('#hdnConsignorCityName_' + i).val(),
                        ConsignorLocationName: $('#hdnConsignorLocationName_' + i).val(),
                        ConsignorPhone: $('#ConsignorPhone').val(),
                        CustomerShipperSame: 0,//$('#hdnCustomerShipperSame_'+i).val(),
                        Consignor: $('#hdnConsignor_' + i).val(),
                        ConsignorAddress: $('#hdnConsignorAddress_' + i).val(),
                        Consignee: $('#hdnConsignee_' + i).val(),
                        ConsigneePhone: $('#hdnConsigneePhone_' + i).val(),
                        ConsigneeCountryName: $('#hdnConsigneeCountryName_' + i).val(),
                        ConsigneeCityName: $('#hdnConsigneeCityName_' + i).val(),
                        ConsigneeLocationName: $('#hdnConsigneeLocationName_' + i).val(),
                        ConsigneeAddress: $('#hdnConsigneeAddress_' + i).val(),
                        MovementID: $('#hdnMovementID_' + i).val(),
                        ProductTypeID: $('#hdnProductTypeID_' + i).val(),
                        ParcelTypeID: $('#hdnParcelTypeID_' + i).val(),
                        Remarks: $('#hdnRemarks_' + i).val(),
                        MaterialCost: $('#hdnMaterialCost_' + i).val(),
                        NetTotal: 0,//$('#hdnNetTotal_'+i).val(),
                        CargoDescription: $('#hdnCargoDescription_' + i).val(),
                        Pieces: $('#hdnPieces_' + i).val(),
                        Weight: $('#hdnWeight_' + i).val(),
                        Remarks: $('#hdnRemarks_' + i).val(),
                        EntrySource: $('#hdnEntrySource_' + i).val(),
                        CourierCharge: $('#hdnCourierCharge_' + i).val(),
                        OtherCharge: $('#hdnOtherCharge_' + i).val(),
                        BagNo: $('#hdnBagNo_' + i).val(),
                        CurrencyID: $('#hdnCurrencyID_' + i).val(),
                        Currency: $('#hdnCurrency_' + i).val(),
                        CustomsValue: $('#hdnCustomsValue_' + i).val(),
                        IsDeleted: $('#hdndeleted_' + i).val(),
                        ParcelType: $('#hdnParcelType_'+i).val(),
                        MovementType: $('#hdnMovementType_'+i).val(),
                        ProductType: $('#hdnProductType_'+i).val(),
                        MAWB: $('#hdnMAWB_' +i).val(),
                    }

                    items.push(awbentry);

                }

                if ((i + 1) == maxrow) {
                    $('#loaderpopup').modal('hide');
                    $.ajax({
                        type: 'POST',
                        url: '/Shipment/AutoDataFixation',
                        datatype: "html",
                        data: {
                            Details: JSON.stringify(items)
                        },
                        success: function (data) {
                            
                            $("#listContainer").html(data);
                            Swal.fire("Fixaton Status!", "Data Updated", "success");
                        }
                    });
                }
            }



        })


        $(document).on("click", "#btnManualFixation", function () {


            $.ajax({
                type: 'POST',
                url: '/Shipment/ShowImportDataFixation1',
                datatype: "html",
                data: {
                    FieldName: 'ConsigneeCountryName', BatchID: $('#BATCHID').val()
                },
                success: function (data) {
                    $("#fixationpopupContainer").html(data);
                    $('#fixationpopup').modal('show');
                }
            });

        })
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
                            //SaveAWB();
                            if ($('#btnAdd').attr('entrymode') == 'Add') {
                                var itemcount = $('#tblbody > tr').length;
                                $('#btnAdd').attr('entryindex', -1);
                                AddAWB(itemcount, t);
                            }
                            else {
                                var itemindex = $('#btnAdd').attr('entryindex');
                                AddAWB(itemindex, t);
                            }

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

        $(document).on("click", "#btnImportAPI", function () {

            //if ($('#BatchCustomerID').val() == '' || $('#BatchCustomerID').val() == '0') {
            //    Swal.fire("Data Importing Status!", "Select Customer!", "error");
            //    $('#CustomerName').focus();
            //    return false;
            //}

            if ($('#CourierStatusID').val() == '' || $('#CourierStatusID').val() == '0') {
                Swal.fire("Data Importing Status!", "Select Status!", "error");
                $('#CourierStatusID').focus();
                return false;
            }

            $('#loaderpopup').modal('show');
            $('#btnImportAPI').attr('disabled', 'disabled');
            $.ajax({
                type: 'POST',
                url: '/AWBImport/ImportAPI',
                data: { EntryDate: $('#BatchDate').val(), CustomerID: 0 },
                datatype: "html",
                //contentType: false,
                //processData: false,
                success: function (response) {
                   
                    if (response.Status === "OK") {
                        debugger;
                     
                        //var data = response.data;
                        //var max = data.length;

                        //if (max == 0) {
                        //    $('#loaderpopup').modal('hide');
                        //    Swal.fire("Data Importing Status!", "Duplicate AWB Not allowed to import", "success");
                        //    $("#listContainer").html('');
                        //    $('#loaderpopup').modal('hide');
                        //    return false;
                        //}
                        //else {
                        //    $('#loaderpopup').modal('hide');
                        //}
                        $.ajax({
                            type: 'POST',
                            url: '/AWBImport/ShowShipmentList',
                            datatype: "html",
                            success: function (data) {
                                $('#EntrySource').val('API');
                                $("#listContainer").html(data);
                                //var max = $('#detailsbody > tr').length;
                                var table = $('#datatable1').DataTable({
                                    "aaSorting": [],
                                    "searching": true,
                                    "bPaginate": false,

                                });
                                $('#loaderpopup').modal('hide');
                                $('#btnImportAPI').removeAttr('disabled');
                                //$('#TotalAWB').val(max);


                            }
                        });

                    } else {
                        $('#loaderpopup').modal('hide');
                        $('#btnImportAPI').removeAttr('disabled');
                        Swal.fire("Data Importing Status!", response.Message, "error");
                    }
                },
                error: function (err) {
                    $('#loaderpopup').modal('hide');
                    console.log(err);
                }

            });
        });

        $(document).on("change", "#importFile", function () {
            debugger;
            $('#h4LoaderTitle').html('Data Importing');
            $('#loaderpopup').modal('show');
            var files = $("#importFile").get(0).files;

            var formData = new FormData();
            formData.append('importFile', files[0]);
          
            if (files.length > 0) {
                $('#btnUpload').attr('disabled', 'disabled');
                $.ajax({
                    url: '/AWBImport/ImportFile',
                    data: formData,
                    type: 'POST',
                    contentType: false,
                    processData: false,
                    success: function (response) {
                        if (response.Status === 1) {
                            debugger;
                            //var data = response.data;
                            var max = response.dataCount;
                              
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
                                url: '/AWBImport/ShowShipmentList',
                                datatype: "html",
                                success: function (data) {
                                    $('#EntrySource').val('3');
                                    $("#listContainer").html(data);
                                    //var max = $('#detailsbody > tr').length;
                                    $('#detailsbody > tr').html('Processing....');
                                    var table = $('#datatable1').DataTable({
                                        "aaSorting": [],                                        
                                        "searching": true,
                                        "bPaginate": false,
                                         
                                    });
                                    setTimeout(function () {
                                        $('#loaderpopup').modal('hide');
                                    }, 300);
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
