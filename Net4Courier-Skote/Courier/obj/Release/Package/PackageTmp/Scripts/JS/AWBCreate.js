var _decimal = "2";

//other charge modal functions
function checkduplication(potherchargeid) {
    //OtherCharge_
    var maxrow = $('#OtherChargeTable > tr').length;
    var totalothercharge = 0;
    for (i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        if (deleted == 'false' || deleted == '' || deleted == null) {
            var otherchargeid = $('#OtherCharge_' + i).val();
            if (potherchargeid == otherchargeid) {
                
                Swal.fire("Data Validation","Could not add Duplicate Other Charge Item!", "error");

                return true;
            }
        }
    }
    return false;
}
function deleteOthertrans(obj) {
    $(obj).parent().parent().addClass('hide');
    var obj1 = $(obj).parent().parent().find('.hdndeleted');
    $(obj1).val(true);
    calculateothertotal();

}

function deleteItemtrans(obj) {
    $(obj).parent().parent().addClass('hide');
    var obj1 = $(obj).parent().parent().find('.hdndeleted');
    $(obj1).val(true);
    getTotalweight();

}

function calculateothertotal() {
    //Amount_@otherchargecount
    debugger;
    var maxrow = $('#OtherChargeTable > tr').length;
    var totalothercharge = 0;
    $('#OtherCharge').val(totalothercharge);
    for (i = 0; i < maxrow; i++) {
        var deleted = $('#hdndeleted_' + i).val();
        if (deleted == 'false' || deleted == '' || deleted == null) {
            var othercharge = $('#Amount_' + i).val();
            totalothercharge = parseFloat(totalothercharge) + parseFloat(othercharge);
            $('#OtherCharge').val(parseFloat(totalothercharge).toFixed(_decimal));
        }
        if ((i + 1) == maxrow) {
            CalculateTax();
        }

    }
}
///////////////////////////////

function SaveCustomerEntry() {

    if ($('#NewCustomerName').val() == '') {
        Swal.fire('Data Validation',"Enter Customer Name", "error");
        return;
    }
    else if ($('#NewPhone').val() == '') {
        Swal.fire('Data Validation', "Enter Customer Phone", "error");
        
        return;
    }
    else if ($('#NewCityName').val() == '' && $('#NewCountryName').val() == '') {
        Swal.fire('Data Validation', "Enter Customer Location Details", "error");
        
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
                
                Swal.fire('Save Stsatus', response.message, "success");

            }
            else {
                $('#customerpopup').modal('hide');
                $('#customer').val(response.data.CustomerName);
                Swal.fire('Save Stsatus', response.message, "error");
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
        
        Swal.fire('Save Stsatus', "Enter Location Details", "error");
        return;
    }
    else if ($('#LLocationName').val() != '' && ($('#LCityName').val() == '' || $('#LCountryName').val() == '')) {
   
        Swal.fire('Save Stsatus', "Enter Location's City/Country Details", "error");
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
            if (response.status == 'ok') {
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
                Swal.fire('Save Status', response.message, "success");
                

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
                Swal.fire('Save Status', response.message, "error");

            }
        }
    });
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

    $('#totalCharge').val(parseFloat(net).toFixed(_decimal));

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
    //if ($('#OriginCountry').val() != $('#BranchCountry').val() && $('#DeliveryCountry').val() != $('#BranchCountry').val()) {
    //    $('#MovementTypeID').val(4).trigger('change');//Transhipment
    //}
    //else if ($('#OriginCountry').val() != $('#BranchCountry').val()) {
    //    $('#MovementTypeID').val(3).trigger('change'); //import
    //}
    //else if ($('#DeliveryCountry').val() != $('#BranchCountry').val()) {
    //    $('#MovementTypeID').val(2).trigger('change');//Export
    //}
    //else if ($('#DeliveryCountry').val() == $('#BranchCountry').val()) {
    //    $('#MovementTypeID').val(1).trigger('change'); //Domestic
    //}

    debugger;
    console.log($('#BranchCountry').val());
    if ($('#ConsignorCountryName').val() != $('#BranchCountry').val() && $('#ConsigneeCountryName').val() != $('#BranchCountry').val()) {
        $('#MovementTypeID').val(4).trigger('change');//Transhipment
    }
    else if ($('#ConsignorCountryName').val() != $('#BranchCountry').val()) {
        $('#MovementTypeID').val(3).trigger('change'); //import
        $('#lblfagent').html('Import Agent');
        $('#lblfagentno').html('Import Agent No.');
        $('#lblfagentrate').html('Import Agent Rate');
    }
    else if ($('#ConsigneeCountryName').val() != $('#BranchCountry').val()) {
        $('#MovementTypeID').val(2).trigger('change');//Export
        $('#lblfagent').html('Forwarding Agent');
        $('#lblfagentno').html('Forwarding Agent No.');
        $('#lblfagentrate').html('Forwarding Agent Rate');
    }
    else if ($('#ConsigneeCountryName').val() == $('#BranchCountry').val()) {
        $('#MovementTypeID').val(1).trigger('change'); //Domestic
    }


}

function SaveAWB() {
    debugger;
    if ($('#MovementTypeID').val()!=1 && $('#ConsignorCountryName').val() == $('#ConsigneeCountryName').val()) {
        Swal.fire('Data Validation', 'Invalid Movement type', 'Error');
        return false;
    }
    $('#btnsave').attr('disabled', 'disabled');
    var otheritemcount1 = $('#OtherChargeTable > tr').length;
    var itemitemcount = $('#ItemBody > tr').length;    
    var awbentry = {
        InScanID: $('#InScanID').val(),
        HAWBNo: $('#HAWBNo').val(),
        TransactionDate: $('#TransactionDate').val(),                                
        PaymentModeId: $('#PaymentModeId').val(),
        CustomerID: $('#CustomerID').val(),        
        ConsignorCountryName: $('#ConsignorCountryName').val(),
        ConsignorCityName: $('#ConsignorCityName').val(),
        ConsignorLocationName: $('#ConsignorLocationName').val(),
        ConsignorLocationID: $('#ConsignorLocationID').val(),
        ConsignorPhone: $('#ConsignorPhone').val(),      
        ConsignorMobile: $('#ConsignorMobile').val(),
        CustomerShipperSame: $('#CustomerShipperSame').val(),
        shippername: $('#shippername').val(),
        ConsignorAddress1_Building: $('#ConsignorAddress1_Building').val(),
        ConsignorAddress2_Street: $('#ConsignorAddress2_Street').val(),
        ConsignorAddress3_PinCode: $('#ConsignorAddress3_PinCode').val(),
        ConsignorContact: $('#ConsignorContact').val(),        
        Consignee: $('#Consignee').val(),
        ConsigneePhone: $('#ConsigneePhone').val(),
        ConsigneeContact: $('#ConsigneeContact').val(),
        ConsigneeCountryName: $('#ConsigneeCountryName').val(),
        ConsigneeCityName: $('#ConsigneeCityName').val(),
        ConsigneeLocationName: $('#ConsigneeLocationName').val(),
        ConsigneeAddress1_Building: $('#ConsigneeAddress1_Building').val(),
        ConsigneeAddress2_Street: $('#ConsigneeAddress2_Street').val(),
        ConsigneeAddress3_PinCode: $('#ConsigneeAddress3_PinCode').val(),
        ConsigneeMobile: $('#ConsigneeMobile').val(),
        MovementTypeID: $('#MovementTypeID').val(),
        ProductTypeID: $('#ProductTypeID').val(),
        ParcelTypeID: $('#ParcelTypeID').val(),
        remarks: $('#remarks').val(),
        CourierCharge: $('#CourierCharge').val(),
        OtherCharge: $('#OtherCharge').val(),
        materialcost: $('#materialcost').val(),
        totalCharge: $('#totalCharge').val(),
        TaxPercent: $('#TaxPercent').val(),
        TaxAmount: $('#TaxAmount').val(),
        SurchargePercent: $('#SurchargePercent').val(),
        SurchargeAmount: $('#SurchargeAmount').val(),
        CustomerRateTypeID: $('#CustomerRateTypeID').val(),
        PickedBy: $('#PickedBy').val(),
        ReceivedBy: $('#ReceivedBy').val(),
        IsNCND: $('#IsNCND').prop('checked'),
        IsCashOnly: $('#IsCashOnly').prop('checked'),
        IsChequeOnly: $('#IsChequeOnly').prop('checked'),
        IsCollectMaterial: $('#IsCollectMaterial').prop('checked'),
        IsDOCopyBack: $('#IsDOCopyBack').prop('checked'),
        PickupLocation: $('#PickupLocation').val(),
        DeliveryLocation: $('#DeliveryLocation').val(),
        ShipmentModeID: $('#ShipmentModeID').val(),//air,sea,land              
        Description: $('#Description').val(),
        Pieces: $('#Pieces').val(),
        Weight: $('#Weight').val(),
        ManifestWeight: $('#ManifestWeight').val(),
        CustomsValue: $('#CustomsValue').val(),
        CurrencyID: $('#CurrencyID').val(),
        AcHeadID: $('#AcheadID').val(),
        CBMLength: $('#CBMLength').val(),
        CBMWidth: $('#CBMWidth').val(),
        CBMHeight: $('#CBMHeight').val(),
        SpotRate: $('#SpotRate').val(),
        MarginPercent: $('#MarginPercent').val(),
        VerifiedWeight: $('#VerifiedWeight').val(),
        FAWBNo: $('#FAWBNo').val(),
        ForwardingCharge: $('#ForwardingCharge').val(),
        FagentID: $('#FagentID').val()
    }
    if (otheritemcount1 == 0 && itemitemcount == 0) {
        CallSaveAWBService(awbentry, '', '');
    }
    else if (otheritemcount1 >0 && itemitemcount == 0) {
        getOtherChargePopupDetails(awbentry);
    }
    else if (otheritemcount1 == 0 && itemitemcount >0) {
        getItemPopupDetails(awbentry);
    }
 
}
function getOtherChargePopupDetails(awbentry) {
    debugger;
    var itemcount1 = $('#OtherChargeTable > tr').length;
    var othercharges = [];
    var idtext1 = 'OtherCharge_';
    if (itemcount1 > 0) {
        $('[id^=' + idtext1 + ']').each(function (index, item) {
            var deleted = $('#hdndeleted_' + index).val();
            var deletestatus = false;

            if (deleted == 'true') {
                deletestatus = true;
            }
                var OtherChargeID = $('#OtherCharge_' + index).val();
                var oAmount = $('#Amount_' + index).val();

                if (oAmount > 0) {
                    var data = {
                        OtherChargeID: OtherChargeID,
                        Amount: oAmount,
                        Deleted: deletestatus
                    }
                    othercharges.push(data);
              //  }
            }
            if ((index + 1) == (itemcount1)) {

                var OtherChargeDetails = JSON.stringify(othercharges);
                var itemitemcount = $('#ItemBody > tr').length;    
                if (itemitemcount == 0)
                    CallSaveAWBService(awbentry, OtherChargeDetails, '');
                else {
                    getItemPopupDetails(awbentry, OtherChargeDetails);
                }
                return OtherChargeDetails;
            }
        });


    }
    else {
        return '';
    }
}
function getItemPopupDetails(awbentry,OtherChargeDetails) {
    var itemcount = $('#ItemBody > tr').length;
    var idtext = 'ShipmentItem_';
    var Items = [];
    if (itemcount > 0) {
        $('[id^=' + idtext + ']').each(function (index, item) {
            var deleted = $('#hdnItemdeleted_' + index).val();
            if (deleted != 'true') {
                var ID = $('#ShipmentItem_' + index).val();
                var BoxName = $('#ItemBoxName_' + index).val();
                var Contents = $('#ItemContents_' + index).val();
                var Qty = $('#ItemQty_' + index).val();
                var WeightPerCarton = $('#ItemWeight_' + index).val();
                var TotalWeight = $('#ItemTotalWeight_' + index).val();
                var Value = $("#ItemValue_" + index).val();

                 
                    var data = {
                        ID: ID,
                        BoxName: BoxName,
                        Contents: Contents,
                        Qty: Qty,
                        WeightPerCarton: WeightPerCarton,
                        TotalWeight: TotalWeight,
                        Value: Value
                    }

                    Items.push(data);
                
            }
            if ((index + 1) == itemcount) {
                var ItemDetails = JSON.stringify(Items);
                CallSaveAWBService(awbentry, OtherChargeDetails, ItemDetails);                
            }
        });
    }
    else {
        return '';
    }
}
function CallSaveAWBService(awbentry, pOtherCharge, pItemDescription) {
    //console.log(pOtherCharge);
    //console.log(pItemDescription);
    $.ajax({
        type: "POST",
        url: '/AWB/SaveAWB/',
        datatype: "html",
        data: { v: awbentry, OtherCharge:pOtherCharge,ItemDescription:pItemDescription },
        success: function (response) {
            debugger;
            if (response.status == "OK") {
                Swal.fire("Save Status!", response.message, "success");
                //$('#InScanID').val(response.InscanId);
                $('#divothermenu').removeClass('hide');
                $('#btnsave').removeAttr('disabled');
                var t = document.getElementsByClassName("needs-validation");
                $(t).removeClass('was-validated');
                if ($('#InScanID').val()>0)
                    window.location.href = '/AWB/Index';
                else
                    window.location.href = '/AWB/Create?id=0'; // + response.InscanId;
                //window.location.reload();

            }
            else {
                $('#btnsave').removeAttr('disabled');
                Swal.fire("Save Status!", response.message, "warning");
                //window.location.reload();
            }


        }
    });
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

        $("#customer").autocomplete({
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
                $('#customer').val(ui.item.label);
                $('#customer').attr('customername', ui.item.label);
                $('#CustomerID').val(ui.item.CustomerId);
                $('#CustomerID').attr('value', ui.item.CustomerId);
                $('#hdnCustomerType').val(ui.item.type);
            },
            select: function (e, i) {

                e.preventDefault();
                $("#customer").val(i.item.label);
                $('#customer').attr('customername', i.item.label);
                $('#CustomerID').val(i.item.CustomerId);
                $('#CustomerID').attr('value', i.item.CustomerId);
                $('#hdnCustomerType').val(i.item.type);
            },

        });
        $('#customer').change(function () {
            $.ajax({
                url: '/AWB/GetCustomerName',
                datatype: "json",
                data: {
                    term: $('#customer').val()
                },
                success: function (data) {
                    if (data.length > 0) {
                        $.each(data, function (val, item) {
                            if (item.CustomerName == $('#customer').val()) {
                                $('#CustomerID').val(item.CustomerID);
                                $("#customer").val(item.CustomerName);
                            }
                        })
                        if ($("#CustomerandShipperSame").is(':checked')) {
                            $("#shippername").val($('#customer').val());
                            $("#shippername").attr('readonly', 'readonly');
                            LoadCustomerDetail();
                        }
                    }
                    else {
                        $('#CustomerID').val(0);
                        $("#customer").val('');

                    }
                }
            });
            //if ($('#customer').val() != '') {
            //    console.log($('#customer').attr('customername'));
            //    if ($('#customer').val().trim().toLowerCase() != $('#customer').attr('customername').trim().toLowerCase()) {
            //        $('#customer').val('');
            //        $('#CustomerID').val(0);
            //        $('#customer').attr('customername', '');
            //        if ($("#CustomerandShipperSame").is(':checked')) {
            //            $("#shippername").val('');
            //        }
            //        $('#customer').focus();
            //    }
            //    else {
            //        if ($("#CustomerandShipperSame").is(':checked')) {
            //            $("#shippername").val($('#customer').val());
            //            $("#shippername").attr('readonly', 'readonly');
            //            LoadCustomerDetail();
            //        }
            //    }
            //}
        })
        //checkbox check to load credit customer address for shipper
        $("#CustomerandShipperSame").click(function () {

            if ($("#CustomerandShipperSame").is(':checked')) {
                $("#shippername").val($('#customer').val());
                $("#shippername").attr('readonly', 'readonly');
                LoadCustomerDetail();
            }
            else {

                $("#shippername").removeAttr('readonly');
                $("#shippername").val('');
                $("#ConsignorContact").val('');
                $("#ConsignorAddress1_Building").val('');
                $("#ConsignorAddress2_Street").val('');
                $("#ConsignorAddress3_PinCode").val('');

                $("#ConsignorPhone").val('');
                $("#ConsignorCountryName").val('');
                $("#ConsignorCityName").val('');
                $("#ConsignorLocationName").val('');
                $("#OfficeTimeFrom").val('');
                $("#OfficeTimeTo").val('');

            }
        });

        $("#shippername").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/AWBBatch/GetShipperName',
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
                $('#shippername').val(i.item.value);
                $("#ConsignorContact").val(i.item.ContactPerson);
                $("#ConsignorAddress1_Building").val(i.item.Address1);
                $("#ConsignorAddress2_Street").val(i.item.Address2);

                $("#ConsignorAddress3_PinCode").val(i.item.Pincode);

                $("#ConsignorPhone").val(i.item.Phone);
                $("#ConsignorCountryName").val(i.item.CountryName);
                $("#ConsignorCityName").val(i.item.CityName);
                $("#ConsignorLocationName").val(i.item.LocationName);
                $('#ConsignorMobile').val(i.item.MobileNo);
                getMovementType();
            },
            select: function (e, i) {
                e.preventDefault();
                $("#shippername").val(i.item.label);
                $("#ConsignorContact").val(i.item.ContactPerson);
                $("#ConsignorAddress1_Building").val(i.item.Address1);
                $("#ConsignorAddress2_Street").val(i.item.Address2);

                $("#ConsignorAddress3_PinCode").val(i.item.Pincode);

                $("#ConsignorPhone").val(i.item.Phone);
                $("#ConsignorCountryName").val(i.item.CountryName);
                $("#ConsignorCityName").val(i.item.CityName);
                $("#ConsignorLocationName").val(i.item.LocationName);
                $('#ConsignorMobile').val(i.item.MobileNo);
                getMovementType();

            },

        });
        $("#Consignee").autocomplete({
            source: function (request, response) {
                $.ajax({
                    url: '/AWBBatch/GetReceiverName', //'/AWB/GetConsigneeName',
                    datatype: "json",
                    data: {
                        term: request.term, Shipper: $('#shippername').val(), ShowAll: $('#ShowAllConsignee').prop('checked')
                    },
                    success: function (data) {
                        response($.map(data, function (val, item) {
                            return {
                                label: val.Name,
                                value: val.Name,
                                ContactPerson: val.ContactPerson,
                                Address1: val.Address1,
                                Address2: val.Address2,
                                Pincode: val.PinCode,
                                Phone: val.Phone,
                                CountryName: val.CountryName,
                                CityName: val.CityName,
                                LocationName: val.LocationName,
                                MobileNo: val.ConsigneeMobileNo

                            }
                        }))
                    }
                })
            },
            minLength: 1,
            autoFocus: false,
            focus: function (event, i) {
                $("#Consignee").val(i.item.label);
                $("#ConsigneeContact").val(i.item.ContactPerson);
                $("#ConsigneeAddress1_Building").val(i.item.Address1);
                $("#ConsigneeAddress2_Street").val(i.item.Address2);

                $("#ConsigneeAddress3_PinCode").val(i.item.Pincode);
                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeCountryName").val(i.item.CountryName);

                $("#ConsigneeCityName").val(i.item.CityName);
                $("#ConsigneeLocationName").val(i.item.LocationName);
                //$("#ConsigneeCountryName").val('');
                //$("#ConsigneeCityName").val('');
                //$("#ConsigneeLocationName").val('');
                $('#ConsigneeMobile').val(i.item.MobileNo);
                getMovementType();
            },
            select: function (e, i) {
                e.preventDefault();
                $("#Consignee").val(i.item.label);
                $("#ConsigneeContact").val(i.item.ContactPerson);
                $("#ConsigneeAddress1_Building").val(i.item.Address1);
                $("#ConsigneeAddress2_Street").val(i.item.Address2);

                $("#ConsigneeAddress3_PinCode").val(i.item.Pincode);
                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeCountryName").val(i.item.CountryName);
                $("#ConsigneeCityName").val(i.item.CityName);
                $("#ConsigneeLocationName").val(i.item.LocationName);
                //$("#ConsigneeCountryName").val('');
                //$("#ConsigneeCityName").val('');
                //$("#ConsigneeLocationName").val('');
                $('#ConsigneeMobile').val(i.item.MobileNo);
                getMovementType();
            },

        });

        $('#remarks').change(function () {
            $('#ChkSurcharge').focus();
        })

        var accounturl = '/Accounts/GetHeadsForCash';

        $("#AcHeadName").autocomplete({
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
            }, minLength: 0,
            focus: function (event, i) {
                $("#AcHeadName").val(i.item.label);
                $('#AcheadID').val(i.item.AcHeadID);
            },
            select: function (e, i) {
                e.preventDefault();
                $("#AcHeadName").val(i.item.label);
                $('#AcheadID').val(i.item.AcHeadID);

            }
        });


        if ($('#InScanID').val() > 0) {
            $('#btnsave').html('Update');
            $('#divothermenu').removeClass('hide');
            $('#divothermenu1').removeClass('hide');
        }

      
    }
    function initpopupControl() {
        $('#AddItem').click(function () {

            var Total = 0;
            var MainTotal = 0;
            var boxname = $("#BoxName").val();
            var contents = $("#ItemContents").val();
            var exists = false;

            if (boxname == "" || contents == "") {
                Swal.fire('Data Validation','Please Enter Box Name and Content Name!');
                return;
            }
            var RowCount = parseInt($('#ItemRowCount').val());
            var RowHtml = '<tr><td>' + boxname + '<input type="hidden" name="shipmentItemVM[' + RowCount + '].Deleted" class="hdndeleted" value="false" id="hdnItemdeleted_' + RowCount + '" /><input type="hidden" class="ID"  name="shipmentItemVM[' + RowCount + '].ID"  id="ShipmentItem_' + RowCount + '" value="0"/><input type="hidden" class="ID"  name="shipmentItemVM[' + RowCount + '].BoxName"  id="ItemBoxName_' + RowCount + '" value="' + boxname + '"/></td>';
            RowHtml = RowHtml + '<td style="text-align:right">' + $('#ItemContents').val() + '<input type="hidden" id="ItemContents_' + RowCount + '"  name="shipmentItemVM[' + RowCount + '].Contents"  value="' + $('#ItemContents').val() + '"/></td>';
            RowHtml = RowHtml + '<td style="text-align:right">' + $('#ItemQty').val() + '<input type="hidden" id="ItemQty_' + RowCount + '"  name="shipmentItemVM[' + RowCount + '].Qty" class="ExpAllocatedAmountDetails" value="' + $('#ItemQty').val() + '"/></td>';
            RowHtml = RowHtml + '<td style="text-align:right">' + $('#ItemWeight').val() + '<input type="hidden" id="ItemWeight_' + RowCount + '"  name="shipmentItemVM[' + RowCount + '].WeightPerCarton" class="ExpAllocatedAmountDetails" value="' + $('#ItemWeight').val() + '"/></td>';
            RowHtml = RowHtml + '<td style="text-align:right">' + $('#ItemTotalWeight').val() + '<input type="hidden" id="ItemTotalWeight_' + RowCount + '"  name="shipmentItemVM[' + RowCount + '].TotalWeight" class="ExpAllocatedAmountDetails" value="' + $('#ItemTotalWeight').val() + '"/></td>';
            RowHtml = RowHtml + '<td style="text-align:right">' + $('#ItemValue').val() + '<input type="hidden" id="ItemValue_' + RowCount + '"  name="shipmentItemVM[' + RowCount + '].Value" class="ExpAllocatedAmountDetails" value="' + $('#ItemValue').val() + '"/></td>';
            RowHtml = RowHtml + '<td><a href="javascript:void(0)"  onclick="deleteItemtrans(this)"  class="deleteallocrow" id="DeleteAllocationRow"><i class="fa fa-times"></i></a></td>';
            RowHtml = RowHtml + '</tr>';
            $('#ItemBody').append(RowHtml);
            $('#ItemRowCount').val(RowCount + 1);
            $("#ItemValue").val('');
            $("#ItemQty").val('');
            $("#ItemWeight").val('');
            $("#ItemTotalWeight").val('');
            $("#ItemContents").val('');
            $('#ItemContents').focus();
            getTotalweight();


        });

        //other charge add click
        $('#AddExpAllocation').click(function () {
            //if ($('#OtherCharge').attr('readonly') != 'readonly') {
            if ($('#InvoiceId').val() == '' || $('#InvoiceId').val() == '0' || $('#InvoiceId').val() == null) {
                var Total = 0;
                var MainTotal = 0;
                var selectedval = $("#OtherChargeID").val();
                var exists = false;

                if (selectedval == 0) {
                    alert('Please select Other Charge Name!');
                    return;
                }
                $('#OtherChargeTable').find('.ExpAllocatedAmountDetails').each(function () {

                    if (isNaN(parseFloat($(this).val())) === false) {
                        Total += parseFloat($(this).val());
                    }
                });
                if (checkduplication(selectedval) == true) {
                    exists = true;
                    return;
                }
                
                if (exists == true)
                    return;

                if ($('#ExpAmount').val() == '' || $('#ExpAmount').val() == 0 || $('#ExpAmount').val() == undefined) {
                    alert('Enter Amount!')
                    return;
                }
                if (isNaN(parseFloat($('#ExpAmount').val())) === false) {
                    Total += parseFloat($('#ExpAmount').val());
                }
                $('#OtherCharge').val(Total);
                var x = $("#CourierCharge").val();
                //var y = $("#PackingCharge").val();
                var z = $("#OtherCharge").val();
                //var a = $("#CustomCharge").val();
                var tot = parseFloat(x) + parseFloat(z); // + parseFloat(a); parseFloat(y)

                $("#totalCharge").val(tot.toFixed(2));


                var othercharngename = $("#OtherChargeID option:selected").text();                

                var RowCount = parseInt($('#RowCount').val());
                var RowHtml = '<tr><td>' + othercharngename + '<input type="hidden" name="otherchargesVM[' + RowCount + '].Deleted" class="hdndeleted" value="false" id="hdndeleted_' + RowCount + '" /><input type="hidden" class="OChargeID"  name="otherchargesVM[' + RowCount + '].OtherChargeID"  id="OtherCharge_' + RowCount + '" value="' + $('#OtherChargeID').val() + '"/></td>';
                RowHtml = RowHtml + '<td style="text-align:right">' + $('#ExpAmount').val() + '<input type="hidden" id="Amount_' + RowCount + '"  name="otherchargesVM[' + RowCount + '].Amount" class="ExpAllocatedAmountDetails" value="' + $('#ExpAmount').val() + '"/></td>';
                RowHtml = RowHtml + '<td><a href="javascript:void(0)"  onclick="deleteOthertrans(this)"  class="deleteallocrow" id="DeleteAllocationRow"><i class="fa fa-times"></i></a></td>';
                RowHtml = RowHtml + '</tr>';
                $('#OtherChargeTable').append(RowHtml);
                $('#RowCount').val(RowCount + 1);
                ///otherchargecount++;
                $("#OtherChargeName").val('');
                $("#OtherChargeID").val(0);
                $("#ExpAmount").val(0);
                $('#OtherChargeName').focus()
                calculateothertotal();
                CalculateTax();
    }
         else {
            $('#spanotherchargeerror').html('Charges could not be edited in the Invoiced AWB!');
    }
});

    }
    function init() {
        initformControl();
        initpopupControl();
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
                            SaveAWB();

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
