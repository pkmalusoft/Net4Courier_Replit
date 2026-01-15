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
        $.notify("Enter Location Details", "error");
        return;
    }
    else if ($('#LLocationName').val() != '' && ($('#LCityName').val() == '' || $('#LCountryName').val() == '')) {
        $.notify("Enter Location's City/Country Details", "error");
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

                $.notify(response.message, "success");

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
                $.notify(response.message, "error");

            }
        }
    });
}

function setTwoNumberDecimal(obj) {

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

function SaveAWB() {
    debugger;
    var shipitems = $('#SelectedValues').val();
    if (shipitems != null) {
        var shipperinstruction = '';
        $.each(shipitems, function (index, item) {
            if (shipperinstruction == '')
                shipperinstruction = item;
            else
                shipperinstruction = shipperinstruction + ',' + item;
        });
    }

    $('#btnsave').attr('disabled', 'disabled');
    var otheritemcount1 = $('#OtherChargeTable > tr').length;
    var itemitemcount = $('#ItemBody > tr').length;
    var awbentry = {
        ShipmentID: $('#ShipmentID').val(),
        AWBNo: $('#AWBNo').val(),
        AWBDate: $('#TransactionDate').val(),
        PaymentModeId: $('#PaymentModeId').val(),
        CustomerID: $('#CustomerID').val(),
        ConsignorCountryName: $('#ConsignorCountryName').val(),
        ConsignorCityName: $('#ConsignorCityName').val(),
        ConsignorLocationName: $('#ConsignorLocationName').val(),        
        CustomerShipperSame: $('#CustomerShipperSame').val(),
        Consignor: $('#Consignor').val(),
        ConsignorAddress1_Building: $('#ConsignorAddress1_Building').val(),
        ConsignorAddress2_Street: $('#ConsignorAddress2_Street').val(),
        ConsignorAddress3_PinCode: $('#ConsignorAddress3_PinCode').val(),
        ConsignorContact: $('#ConsignorContact').val(),        
        ConsignorMobileNo: $('#ConsignorMobileNo').val(),
        ConsignorPhone: $('#ConsignorPhone').val(),
        Consignee: $('#Consignee').val(),
        ConsigneePhone: $('#ConsigneePhone').val(),
        ConsigneeCountryName: $('#ConsigneeCountryName').val(),
        ConsigneeCityName: $('#ConsigneeCityName').val(),
        ConsigneeLocationName: $('#ConsigneeLocationName').val(),
        ConsigneeAddress1_Building: $('#ConsigneeAddress1_Building').val(),
        ConsigneeAddress2_Street: $('#ConsigneeAddress2_Street').val(),
        ConsigneeAddress3_PinCode: $('#ConsigneeAddress3_PinCode').val(),
        ConsigneeContact: $('#ConsigneeContact').val(),  
        ConsigneeMobileNo: $('#ConsigneeMobileNo').val(),
        MovementID: $('#MovementID').val(),
        ProductTypeID: $('#ProductTypeID').val(),
        ParcelTypeID: $('#ParcelTypeID').val(),
        Remarks: $('#Remarks').val(),
        MaterialCost: $('#MaterialCost').val(),
        CourierCharge: $('#CourierCharge').val(),
        OtherCharge: $('#OtherCharge').val(),
        TaxPercent: $('#TaxPercent').val(),
        TaxAmount: $('#TaxAmount').val(),
        SurchargePercent: $('#SurchargePercent').val(),
        SurchargeAmount: $('#SurchargeAmount').val(),
        NetTotal: $('#NetTotal').val(),
        CargoDescription: $('#CargoDescription').val(),
        Pieces: $('#Pieces').val(),
        Weight: $('#Weight').val(),
        CurrencyID: $('#CurrencyID').val(),
        CustomsValue: $('#CustomsValue').val(),
        ManifestWeight: $('#ManifestWeight').val(),
        SpecialInstructions: shipperinstruction
        
    }
    CallSaveAWBService(awbentry);


}


function LoadCustomerDetail() {
    debugger;
    if ($('#CustomerName').val() != '') {
        $.ajax({
            type: "POST",
            url: "/Shipment/GetCustomerDataByName",
            datatype: "Json",
            data: { CustomerName: $('#CustomerName').val() },
            success: function (data) {
                $('#CustomerID').val(data.CustID);                
                $("#Consignor").val(data.CustName);                
                $("#ConsignorAddress").val(data.Address1 + data.Address2 + data.Address3);                
                $("#ConsignorPhone").val(data.Phone);
                $("#ConsignorCountryName").val(data.CountryName);
                $("#ConsignorCityName").val(data.CityName);
                $("#ConsignorLocationName").val(data.LocationName);                                                
            }
        });
    }
}


function CallSaveAWBService(awbentry) {

    $.ajax({
        type: "POST",
        url: '/Shipment/SaveAWB/',
        datatype: "html",
        data: { v: awbentry },
        success: function (response) {
            debugger;
            if (response.status == "OK") {
                Swal.fire("Save Status!", response.message, "success");
                $('#InScanID').val(response.InscanId);
                $('#divothermenu').removeClass('hide');
                $('#btnsave').removeAttr('disabled');
                var t = document.getElementsByClassName("needs-validation");
                $(t).removeClass('was-validated');
                if ($('#ShipmentID').val() == 0) {
                    window.location.href = '/Shipment/Create?id=0';
                }
                else {
                    window.location.href = '/Shipment/Create?id=' + data.InscanId;
                }
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
                $('#CustomerID').val(ui.item.CustomerId);
                $('#CustomerID').attr('value', ui.item.CustomerId);
                $('#hdnCustomerType').val(ui.item.type);
            },
            select: function (e, i) {

                e.preventDefault();
                $("#CustomerName").val(i.item.label);
                $('#CustomerName').attr('customername', i.item.label);
                $('#CustomerID').val(i.item.CustomerId);
                $('#CustomerID').attr('value', i.item.CustomerId);
                $('#hdnCustomerType').val(i.item.type);
            },

        });
        $('#CustomerName').change(function () {
            $.ajax({
                url: '/Shipment/GetCustomerName',
                datatype: "json",
                data: {
                    term: $('#CustomerName').val()
                },
                success: function (data) {
                    $.each(data, function (val, item) {
                        if (item.CustomerName == $('#customer').val()) {
                            $('#CustomerID').val(item.CustomerId);
                            $("#CustomerName").val(item.CustomerName);
                        }
                    })
                    if ($("#CustomerandShipperSame").is(':checked')) {
                        $("#Consignor").val($('#customer').val());
                        $("#Consignor").attr('readonly', 'readonly');
                        LoadCustomerDetail();
                    }
                }
            });
            
        })
        //checkbox check to load credit customer address for shipper
        $("#CustomerandShipperSame").click(function () {

            if ($("#CustomerandShipperSame").is(':checked')) {
                $("#Consignor").val($('#customer').val());
                $("#Consignor").attr('readonly', 'readonly');
                LoadCustomerDetail();
            }
            else {

                $("#Consignor").removeAttr('readonly');
                $("#Consignor").val('');
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
                                MobileNo:val.ConsignorMobileNo,
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
                $("#ConsignorContact").val(i.item.ContactPerson);
                $("#ConsignorAddress1_Building").val(i.item.Address1);   
                $("#ConsignorAddress2_Street").val(i.item.Address2);       
                $("#ConsignorAddress3_PinCode").val(i.item.PinCode);     
                
                $("#ConsignorPhone").val(i.item.Phone);
                $("#ConsignorMobileNo").val(i.item.ConsignorMobileNo);
                $("#ConsignorCountryName").val(i.item.CountryName);
                $("#ConsignorCityName").val(i.item.CityName);
                $("#ConsignorLocationName").val(i.item.LocationName);                
                getMovementType();
            },
            select: function (e, i) {
                e.preventDefault();
                $("#Consignor").val(i.item.label);
                $("#ConsignorContact").val(i.item.ContactPerson);
                $("#ConsignorAddress1_Building").val(i.item.Address1);
                $("#ConsignorAddress2_Street").val(i.item.Address2);
                $("#ConsignorAddress3_PinCode").val(i.item.PinCode);

                $("#ConsignorPhone").val(i.item.Phone);
                $("#ConsignorMobileNo").val(i.item.ConsignorMobileNo);    
                $("#ConsignorPhone").val(i.item.Phone);
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
                                PinCode :val.PinCode,
                                Phone: val.Phone,
                                MobileNo :val.ConsignorMobileNo,
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
                $("#ConsigneeContact").val(i.item.ContactPerson);
                $("#ConsigneeAddress1_Building").val(i.item.Address1);
                $("#ConsigneeAddress2_Street").val(i.item.Address2);
                $("#ConsigneeAddress3_PinCode").val(i.item.PinCode);

                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeMobileNo").val(i.item.MobileNo);
                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeCountryName").val(i.item.CountryName);
                $("#ConsigneeCityName").val(i.item.CityName);
                $("#ConsigneeLocationName").val(i.item.LocationName);
                getMovementType();
            },
            select: function (e, i) {
                e.preventDefault();
                $("#Consignee").val(i.item.label);
                $("#ConsigneeContact").val(i.item.ContactPerson);
                $("#ConsigneeAddress").val(i.item.Address1);
                $("#ConsigneeAddress1_Building").val(i.item.Address1);
                $("#ConsigneeAddress2_Street").val(i.item.Address2);
                $("#ConsigneeAddress3_PinCode").val(i.item.PinCode);

                $("#ConsigneePhone").val(i.item.Phone);
                $("#ConsigneeMobileNo").val(i.item.MobileNo);
                $("#ConsigneeCountryName").val(i.item.CountryName);
                $("#ConsigneeCityName").val(i.item.CityName);
                $("#ConsigneeLocationName").val(i.item.LocationName);
                getMovementType();
            },

        });




        if ($('#ShipmentID').val() > 0) {
            $('#btnsave').html('Update');
            $('#divothermenu').removeClass('hide');
            $('#divothermenu1').removeClass('hide');
            if (parseFloat($('#TaxAmount').val()) != 0) {
                $('#ChkTaxPercent').prop('checked', true);
            }
            else {
                $('#ChkTaxPercent').prop('checked', false);
            }
            if (parseFloat($('#SurchargeAmount').val()) != 0) {
                $('#ChkSurcharge').prop('checked', true);
            }
            else {
                $('#ChkSurcharge').prop('checked', false);
            }

            setTwoNumberDecimal($("#CourierCharge")); //.val('0.00');

            setTwoNumberDecimal($("#OtherCharge")); //.val('0.00');
            //  setTwoNumberDecimal($("#CustomCharge")); //.val('0.00');
            setTwoNumberDecimal($("#NetTotal")); //.val('0.00');
            setTwoNumberDecimal($("#MaterialCost")); //.val('0.00');
            if ($('#SpecialInstructions').val() != null && $('#SpecialInstructions').val() != '') {
                var data = $('#SpecialInstructions').val().split(',');
                $('#SelectedValues').select2();
                $('#SelectedValues').val(data).trigger('change');
            }
            else {
                $('#SelectedValues').select2();
            }
            

        }
        else {

            $('#SelectedValues').select2();
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
