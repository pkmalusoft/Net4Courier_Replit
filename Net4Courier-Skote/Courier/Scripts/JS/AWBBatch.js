$(document).ready(function () {

   

   

    $('#PickedUpEmpID').change(function () {
        $.ajax({
            type: "GET",
            url: "/InScan/GetVehicle",
            datatype: "Json",
            data: { EmployeeId: $('#PickedUpEmpID').val() },
            success: function (response) {
                debugger;
                if (response.VehicleId != 0) {
                    $('#InscanVehicleId').val(response.VehicleId).trigger('change');
                } else {
                    $('#InscanVehicleId').val('').trigger('change');
                }
            }
        });

    });
    $('#OutScanDeliveredID').change(function () {
        $.ajax({
            type: "GET",
            url: "/InScan/GetVehicle",
            datatype: "Json",
            data: { EmployeeId: $('#OutScanDeliveredID').val() },
            success: function (response) {
                debugger;
                if (response.VehicleId != 0) {
                    $('#OutscanVehicleId').val(response.VehicleId).trigger('change');
                } else {
                    $('#OutscanVehicleId').val('').trigger('change');
                }
            }
        });

    })
    $('#PaymentTypeId').change(function () {
        var modeid = $('#PaymentTypeId').val();
        if (modeid == 3) { //Account
            $('#CustomerName').removeAttr('readonly');
            var awbcustomerid = $('#CustomerName').attr('AWBCustomerId');
            var awbcustomer = $('#CustomerName').attr('AWBCustomer');
            if ($('#CustomerID').val() == $('#CASHCustomerId').val() || $('#CustomerID').val() == $('#CODCustomerID').val() || $('#CustomerID').val() == $('#FOCCustomerID').val()) {
                $('#CustomerName').val('');
                $('#CustomerID').val(0);
            }
            $('#CustomerName').val(awbcustomer);
            $('#CustomerID').val(awbcustomerid);

        }
        else {
            if (modeid == 1) {
                $('#CustomerName').val($('#CASHCustomerName').val());
                $('#CustomerID').val($('#CASHCustomerId').val());
            }
            else if (modeid == 2) {
                $('#CustomerName').val($('#CODCustomerName').val());
                $('#CustomerID').val($('#CODCustomerID').val());
                if ($('#Shipper').attr('readonly') == 'readonly') {
                    $('#Consignee').focus();
                }
                else {
                    $('#Shipper').focus();
                }
            }
            else if (modeid == 4) {
                $('#CustomerName').val($('#FOCCustomerName').val());
                $('#CustomerID').val($('#FOCCustomerID').val());
            }
            $('#CustomerName').attr('readonly', 'readonly');
        }
    });
    $("#Shipper").autocomplete({
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
                            ConsignorMobileNo: val.ConsignorMobileNo

                        }
                    }))
                }
            })
        },
        minLength: 1,
        autoFocus: false,
        focus: function (event, i) {
            $('#Shipper').val(i.item.value);
            $("#ConsignorContact").val(i.item.ContactPerson);
            $("#ConsignorAddress1_Building").val(i.item.Address1);
            $("#ConsignorAddress2_Street").val(i.item.Address2);

            $("#ConsignorAddress3_PinCode").val(i.item.Pincode);

            $("#ConsignorPhone").val(i.item.Phone);
            $("#ConsignorCountryName").val(i.item.CountryName);
            $("#ConsignorCityName").val(i.item.CityName);
            $("#ConsignorLocationName").val(i.item.LocationName);
            $("#PickUpLocation").val(i.item.LocationName);
            $('#DeliveryCity').val(i.item.CityName);
            $('#DeliveryCountry').val(i.item.CountryName);
            // getMovementType();

            if (i.item.ConsignorMobileNo == null || i.item.ConsignorMobileNo == 'undefined' || i.item.ConsignorMobileNo == '')
                $("#ConsignorMobileNo").val('');
            else
                $("#ConsignorMobileNo").val(i.item.ConsignorMobileNo);


        },
        select: function (e, i) {
            e.preventDefault();
            $("#Shipper").val(i.item.label);
            $("#ConsignorContact").val(i.item.ContactPerson);
            $("#ConsignorAddress1_Building").val(i.item.Address1);
            $("#ConsignorAddress2_Street").val(i.item.Address2);

            $("#ConsignorAddress3_PinCode").val(i.item.Pincode);

            $("#ConsignorPhone").val(i.item.Phone);
            $("#ConsignorCountryName").val(i.item.CountryName);
            $("#ConsignorCityName").val(i.item.CityName);
            $("#ConsignorLocationName").val(i.item.LocationName);
            $("#PickUpLocation").val(i.item.LocationName);
            $('#OriginCity').val(i.item.CityName);
            $('#OriginCountry').val(i.item.CountryName);
            //  getMovementType();
            if (i.item.ConsignorMobileNo == null || i.item.ConsignorMobileNo == 'undefined')
                $("#ConsignorMobileNo").val('');
            else
                $("#ConsignorMobileNo").val(i.item.ConsignorMobileNo);
            //fillPickupLocation();

        },

    });
    $('#CustomerName').change(function () {

        if ($('#CustomerName').val() != '') {
            if ($('#CustomerName').attr('customername') != undefined && $('#CustomerName').val().trim().toLowerCase() != $('#CustomerName').attr('customername').trim().toLowerCase()) {
                $('#CustomerName').val('');
                $('#CustomerID').val(0);
                $('#CustomerName').attr('customername', '');
                if ($("#CustomerandShipperSame").is(':checked')) {
                    $("#Shipper").val($('#CustomerName').val());
                    $("#Shipper").attr('readonly', 'readonly');
                    LoadCustomerDetail();
                }
                $('#CustomerName').focus();
            }
            else {
                if ($("#CustomerandShipperSame").is(':checked')) {
                    $("#Shipper").val($('#CustomerName').val());
                    $("#Shipper").attr('readonly', 'readonly');
                    LoadCustomerDetail();
                }
            }
        }
    })
   
    //$('#Shipper').change(function () {

    //    $.ajax({
    //        url: '/AWBBatch/GetConsignorCustomer', //'/AWB/GetConsigneeName',
    //        datatype: "json",
    //        data: {
    //            customername: $('#Shipper').val()
    //        },
    //        success: function (data) {

    //            if (data == null) {
    //                $('#CustomerID').val(0);
    //            }
    //            else {
    //                $('#CustomerID').val(data.CustomerID);
    //            }
    //        }
    //    });
    //});
    $("#Consignee").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: '/AWBBatch/GetReceiverName', //'/AWB/GetConsigneeName',
                datatype: "json",
                data: {
                    term: request.term, Shipper: $('#Shipper').val()
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
                            ConsigneeMobileNo: val.ConsigneeMobileNo

                        }
                    }))
                }
            })
        },
        minLength: 1,
        autoFocus: false,
        focus: function (event, i) {
            $('#Consignee').val(i.item.value);
            $("#ConsigneeContact").val(i.item.ContactPerson);
            $("#ConsigneeAddress1_Building").val(i.item.Address1);
            $("#ConsigneeAddress2_Street").val(i.item.Address2);

            $("#ConsigneeAddress3_PinCode").val(i.item.Pincode);
            $("#ConsigneePhone").val(i.item.Phone);
            $("#ConsigneeCountryName").val(i.item.CountryName);
            $("#ConsigneeCityName").val(i.item.CityName);
            $("#ConsigneeLocationName").val(i.item.LocationName);
            $("#ConsigneeLocation").val(i.item.LocationName);


            // getMovementType();

            if (i.item.ConsigneeMobileNo == null || i.item.ConsigneeMobileNo == 'undefined')
                $("#ConsigneeMobileNo").val('');
            else
                $("#ConsigneeMobileNo").val(i.item.ConsigneeMobileNo);
            //fillDeliveryLocation();
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
            if ($('#DeliveryLocation').attr('disabled') != 'disabled')
                $('#DeliveryLocation').val(i.item.LocationName);

            $('#DeliveryCity').val(i.item.CityName);
            $('#DeliveryCountry').val(i.item.CountryName);
            //getMovementType();

            $("#ConsigneeLocation").val(i.item.LocationName);
            if (i.item.ConsigneeMobileNo == null || i.item.ConsigneeMobileNo == 'undefined')
                $("#ConsigneeMobileNo").val('');
            else
                $("#ConsigneeMobileNo").val(i.item.ConsigneeMobileNo);
            //fillDeliveryLocation();
        },

    });

    $('#Consignee').change(function () {

        if ($('#PaymentTypeId').val() == 5) //prepaid
        {
            if ($('#ConsigneeCountryName') != $('#DeliveryCountry').val()) {
                Swal.fire('Data Validation','Delivery country not matched with AWB Delivery Country!');
                $('#DeliveryLocation').val('');
            }
            else {
                $('#DeliveryLocation').val(i.item.LocationName);
            }
        }
        else {
            $('#DeliveryCity').val($('#ConsigneeCityName').val());
            $('#DeliveryCountry').val($('#ConsigneeCountryName').val());
        }

    });
});