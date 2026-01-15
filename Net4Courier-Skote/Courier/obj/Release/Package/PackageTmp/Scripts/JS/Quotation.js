var _decimal = $('#hdncompanydecimal').val();
 
 
function calculatevalue1() {
    debugger;
    var rate = $('#QtxtRate').val();
    var qty = $('#QtxtQty').val();
    if (rate == '')
        rate = 0;
    if (qty == '')
        qty = 0;
    $('#QtxtRate').val(parseFloat(rate).toFixed(_decimal));

    var value = parseFloat(rate) * parseFloat(qty);
    $('#QtxtValue').val(parseFloat(value).toFixed(2));

}

//based on value rate calculate
function calculatevalue2() {
    debugger;
    var value = $('#QtxtValue').val();
    var qty = $('#QtxtQty').val();
    var rate = $('#QtxtRate').val();
    
    if (value == '')
        valu = 0;
    if (qty == '')
        qty = 0;
    if (parseFloat(value) > 0 && parseFloat(qty) > 0)
    {
        rate = parseFloat(value) / parseFloat(qty);
    }    
    $('#QtxtRate').val(parseFloat(rate).toFixed(_decimal));

    //var value = parseFloat(rate) * parseFloat(qty);
    //$('#QtxtValue').val(parseFloat(value).toFixed(2));

}

function calculatevalue(index) {
    var rate = $('#QtxtRate_' + index).val();
    var qty = $('#QtxtQty_' + index).val();
    if (rate == '')
        rate = 0;
    if (qty == '')
        qty = 0;
    
    $('#QtxtRate').val(parseFloat(rate).toFixed(_decimal));
    var value = parseFloat(rate) * parseFloat(qty);
    $('#QtxtValue_' + index).val(parseFloat(value).toFixed(2));
    calculatequotationvalue();
}

function calculatevalue3(index) {
    var value = $('#QtxtValue_' + index).val();
    var qty = $('#QtxtQty_' + index).val();
    if (value == '')
        value = 0;
    if (qty == '')
        qty = 0;

    if (parseFloat(value) > 0 && parseFloat(qty) > 0) {

        var rate = parseFloat(value) / parseFloat(qty);
        $('#QtxtRate_' + index).val(parseFloat(rate).toFixed(_decimal));
        //$('#QtxtValue_' + index).val(parseFloat(value).toFixed(2));
        calculatequotationvalue();
    }
    else {
        $('#QtxtRate_' + index).val(0);
        calculatequotationvalue();
    }
}
function checkdeletedentry() {
    var idtext = 'quodtr_'
    $('[id^=' + idtext + ']').each(function (index, item) {
        if ($('#QDeleted_' + index).val() == 'True') {
            $('#quodtr_' + index).addClass('hide');
        }
        else {
            $('#quodtr_' + index).removeClass('hide');
        }
    });

}
function setUOMcombo() {
    var idtext = 'QUOM_'
    $('[id^=' + idtext + ']').each(function (index, item) {
        $('#QUOM_' + index).val($('#QUOM_' + index).attr('value')).trigger('change');
    });
}
function setquotationowactive(index1) {
    var idtext = 'quotr_'
    $('[id^=' + idtext + ']').each(function (index, item) {
        $('#quotr_' + index).removeClass('rowactive');
    });
    $('#quotr_' + index1).addClass('rowactive');
}

function AddQuotationDetail(obj) {
    debugger;
    
    var itemcount = $('#QuotationDetailTables > tbody > tr').length;
    if ($('#QtxtDescription').val() == '') {
        Swal.fire('Data Validation','Enter Item Description!','error');    
        $('#QtxtDescription').focus();
        return false;
    }
    else if ($('#QUOM').val() == '') {
        Swal.fire('Data Validation', 'Enter Unit of Measurement!', 'error');
        
        $('#QUOM').focus();
        return false;
    }
    else if ($('#QtxtQty').val() == '' || $('#QtxtQty').val() =='0' ) {
        
        Swal.fire('Data Validation', 'Enter Qty!', 'error');
        $('#QtxtQty').focus();
        return false;
    }
    else if ($('#QtxtRate').val() == '' || $('#QtxtRate').val() == '0') {
        
        Swal.fire('Data Validation', 'Enter Rate!', 'error');
        $('#QtxtRate').focus();
        return false;
    }
    $(obj).attr('disabled', 'disabled');
    var quotation = {
        ItemDescription: $('#QtxtDescription').val(),
        UOM: $('#QUOM').val(),
        Quantity: $('#QtxtQty').val(),
        Rate: $('#QtxtRate').val(),
        Value: $('#QtxtValue').val(),
        Remarks: $('#QtxtRemarks').val()
    }

    var quotationdetails = [];
    for (i = 0; i < itemcount; i++) {
        var quotationdetail = {
            ItemDescription: $('#QtxtDescription_' + i).val(),
            GroupName :$('#QtxtGroupName_' +i).val(),
            UOM: $('#QUOM_' + i).val(),
            Quantity: $('#QtxtQty_' + i).val(),
            Rate: $('#QtxtRate_' + i).val(),
            Value: $('#QtxtValue_' + i).val(),
            Remarks: $('#QtxtRemarks_' + i).val(),
            Deleted:$('#QDeleted_'+i).val()
        }

        quotationdetails.push(quotationdetail);

    }
    
    $.ajax({
        type: "POST",
        url: '/Quotation/AddQuotationInventory/',
        datatype: "html",
        data: { invoice: quotation, index: -1, Details: JSON.stringify(quotationdetails)},
        success: function (data) {
            $("#QuotationDetailContainer").html(data);           
            
            $(obj).removeAttr('disabled');
            $('#QtxtDescription').focus()
            calculatequotationvalue();
            clearQuotationDetail();
            
        }
    });
}


function SaveQuotation() {
    debugger;
    var emptyrow = $('#QuotationDetailTables > tbody > tr').html();
    if (emptyrow != undefined) {
        if (emptyrow.indexOf('No data available in table') >= 0) {
            $('#QuotationDetailTables > tbody').html('');
        }
    }
    var itemcount = $('#QuotationDetailTables > tbody > tr').length;
    
    //if ($('#QuotationDate').val() == '') {
    //    $.notify('Select Quoation Date!');
    //      $('#QuotationDate').focus();
    //    return false;
    //}
    //else if ($('#QCurrencyId').val() == '') {
    //    $('#QCurrencyId').focus();
    //    $.notify('Select Currency!');
    //      return false;
    //}
  
    //else if ($('#Validity').val() == '' || $('#Validity').val() == null) {
    //    $.notify('Select Validity!');
    //    $('#Validity').focus();
    //      return false;
    //}
    
    //else if ($('#Version').val() == '' || $('#Version').val() == null) {
    //    $.notify('Enter Version!');
    //    $('#Version').focus();
    //      return false;
    //}
    //else if ($('#QuotationContact').val() == '' || $('#QuotationContact').val() == null) {
    //    $.notify('Enter Contact Person Name!');
    //    $('#QuotationContact').focus();
    //      return false;
    //}
    //else if ($('#QuotationMobile').val() == '' || $('#QuotationMobile').val() == null) {
    //    $.notify('Enter Mobile No.!');
    //    $('#QuotationMobile').focus();
    //      return false;
    //}
    //else if ($('#QuotationPayment').val() == '') {
    //    $('#QuotationPayment').focus();
    //    $.notify('Enter Payment Terms!');
    //      return false;
    //}
    //else if ($('#QtxtSubject').val() == '') {
    //    $('#QtxtSubject').focus();
    //    $.notify('Enter Subject!');
    //      return false;
    //}
    //else if ($('#QtxtSalutation').val() == '') {
    //    $('#QtxtSalutation').focus();
    //    $.notify('Enter Saluation!');
    //      return false;
    //}    
    
    //else if ($('#QtxtTerms').val() == '') {
    //    $('#QtxtTerms').focus();
    //    $.notify('Enter Terms and Conditions!');
    //      return false;
    //}
    //else if ($('#QtxtDescription').val() != '') {        
    //    $.notify('Click + button to complete Item adding!');
    //      return false;
    //}
    if ( itemcount == 0) {
        $('#spanerr').html('Enter Quotation Item Detail!');
        return false;
    }
   
    var quotation = {
        QuotationID: $('#QuotationID').val(),
        QuotationNo: $('#QuotationNo').val(),
        QuotationDate: $('#QuotationDate').val(),
        CurrencyId: $('#QCurrencyId').val(),
        Validity: $('#Validity').val(),
        Version: $('#Version').val(),
        ContactPerson: $('#QuotationContact').val(),
        MobileNumber: $('#QuotationMobile').val(),
        PaymentTerms: $('#QuotationPayment').val(),
        TermsandConditions: $('#QtxtTerms').val(),
        Salutation: $('#QtxtSalutation').val(),
        QuotationValue: $('#QuotationValue').val(),
        CustomerID: $('#CustomerID').val(),
        SubjectText: $('#QtxtSubject').val(),
        ClientDetail: $('#ClientDetail').val()

    }
    var quotationdetails = [];
    for (i = 0; i < itemcount; i++) {
        var quotationdetail = {
            GroupName: $('#QtxtGroupName_' + i).val(),
            ItemDescription: $('#QtxtDescription_' + i).val(),
            UOM: $('#QUOM_' +i).val(),
            Quantity: $('#QtxtQty_' + i).val(),
            Rate: $('#QtxtRate_' + i).val(),
            Value: $('#QtxtValue_'+ i).val(),
            Remarks: $('#QtxtRemarks_' + i).val(),
            Deleted: $('#QDeleted_' + i).val()
        }

        quotationdetails.push(quotationdetail);

    }

    $.ajax({
        type: "POST",
        url: '/Quotation/SaveQuotation/',
        datatype: "json",
        data: { quotation: quotation, Details: JSON.stringify(quotationdetails)},
        success: function (response) {
            if (response.status == "ok") {
                Swal.fire("Save Status!", response.message, "success");

                setTimeout(function () {
                    window.location.href = '/Quotation/Index'
                    //window.location.reload();
                }, 100)


            }
            else {
                $('#btnSaveQuotation').removeAttr('disabled');
                Swal.fire("Save Status!", response.message, "warning");
                //window.location.reload();
            }

        }
    });
}
function showQuotationEntry(QuotationId, index1) {
    //if (QuotationId > 0)
    //    setquotationowactive(index1);
    $.ajax({
        type: "POST",
        url: '/Quotation/ShowQuotationEntry/',
        //datatype: "html",
        data: { Id: QuotationId,JobId:$('#JobID').val() },
        success: function (response) {
            console.log(response.data);
            var data = response.data;
            var myDate = new Date(data.QuotationDate.match(/\d+/)[0] * 1);
            var cmon = myDate.getMonth() + 1;
            var entrydate = myDate.getDate() + "/" + cmon + "/" + myDate.getFullYear();

            $('#QuotationID').val(data.QuotationID);
            $('#QuotationNo').val(data.QuotationNo);
            $('#QuotationDate').val(entrydate);
            $('#QuotationDate').val(entrydate).trigger('change');
            $('#QCurrencyId').val(data.CurrencyId).trigger('change');            
            $('#Validity').attr('svalue', data.Validity);
          
            $('#Version').val(data.Version);
            $('#QuotationContact').val(data.ContactPerson);
            $('#QuotationMobile').val(data.MobileNumber);
            $('#QuotationPayment').val(data.PaymentTerms)
            $('#QtxtTerms').val(data.TermsandConditions);
            $('#QtxtSalutation').val(data.Salutation);
            $('#QuotationValue').val(data.QuotationValue);
            $('#JobID').val(data.JobID);
            $('#QtxtSubject').val(data.SubjectText);
            $('#ClientDetail').val(data.ClientDetail);

            //$("#QuotationEntryContainer").html(data);                       
            $.ajax({
                type: "POST",
                url: '/Job/ShowQuotationDetailList/',
                datatype: "html",
                data: { QuotationId: QuotationId },
                success: function (data1) {
                    $("#QuotationDetailContainer").html(data1);
                    if (QuotationId > 0) {
                       
                        //$('#btnSaveQuotation').html('Update');         
                        setTimeout(function () {
                            $('#Validity').val($('#Validity').attr('svalue')).trigger('change');
                        },100)
                        
                    }
                    
                }
            });
        }
    });

}
function clearQuotationDetail() {
    $('#QtxtDescription').val('');
    $('#QUOM').val('').trigger('change');
    $('#QtxtQty').val(1);
        $('#QtxtRate').val(0);
    $('#QtxtValue').val('');
    $('#QtxtRemarks').val('');
   
    var idtext = 'quotr_';
    $('[id^=' + idtext + ']').each(function (index, item) {
        $('#boetr_' + index).removeClass('rowactive');
    });
   // $('#btnSaveQuotation').html('Add & Save');
}
function clearQuotation() {
    $('#QuotationContact').val('');
    $('#QuotationNo').val('');
    $('#QuotationDate').val('');
    $('#QuotationMobile').val('');
    $('#QuotationPayment').val('');
    $('#QCurrencyId').val(0).trigger('change');
    $('#QuotationValue').val('');
    $('#QtxtTerms').val('');
    $('#QtxtSalutation').val('');
    $('#QtxtSubject').val('');
    $('#ClientDetail').val('');
    //$('#Version').val($('#Version').attr('NewVersion'));
    var idtext = 'quotr_'
    $('[id^=' + idtext + ']').each(function (index, item) {
        $('#quotr_' + index).removeClass('rowactive');
    });
    showQuotationEntry(0);
  //  $('#btnSaveQuotation').html('Add & Save');    
}

function DeleteQuotationEntry() {
    if ($('#SelectedQuotationId').val() != '') {        
            var id = $('#SelectedQuotationId').val();
            Swal.fire({ title: "Are you sure?", text: "You won't be able to revert this!", icon: "warning", showCancelButton: !0, confirmButtonColor: "#34c38f", cancelButtonColor: "#f46a6a", confirmButtonText: "Yes, delete it!" }).then(
                function (t) {
                    if (t.value) {
                        $.ajax({
                            type: "POST",
                            url: '/Job/DeleteQuotation',
                            datatype: "html",
                            data: {
                                'id': id
                            },
                            success: function (response) {
                                if (response.status == "ok") {
                                    Swal.fire("Delete Status!", response.message, "success");
                                    setTimeout(function () {
                                        window.location.reload();
                                    }, 100)

                                }
                                else {
                                    Swal.fire("Delete Status!", response.message, "warning");
                                }

                            }
                        });

                    }
                });
        }
   
}
function DeleteQuotationDetailEntry(index) {
    $('#QDeleted_' + index).val(true);
    $('#quodtr_' + index).addClass('hide');
}

function calculatequotationvalue() {
    debugger;
    var idtext = 'QtxtValue_'
    var total = 0;
    $('[id^=' + idtext + ']').each(function (index, item) {
        total = parseFloat(total) + parseFloat($('#QtxtValue_' + index).val());
    });
    $('#QuotationValue').val(total);
}
function showquotationprint() {
    debugger;
    if ($('#SelectedQuotationId').val() != '') {
        console.log($('#JobID').val());
        console.log($('#SelectedQuotationId').val());
        $('#aqprint').attr('href', '/Job/ReportPrint?id=' + $('#JobID').val() + '&option=6&quotationid=' + $('#SelectedQuotationId').val());
        $('#aqprint').trigger('click');
    }
}
function LoadCustomerDetail() {
    debugger;
    if ($('#CustomerName').val() != '') {
        $.ajax({
            type: "POST",
            url: "/PickUpRequest/GetCustomerDataByName",
            datatype: "Json",
            data: { CustomerName: $('#CustomerName').val() },
            success: function (data) {
                
                var customeraddress = '';
                if (data.Address1 != 'null' && data.Address1 != '')
                    customeraddress = data.Address1;

                if (data.Address2 != 'null' && data.Address2 != '')
                    if (customeraddress == '')
                        customeraddress = data.Address2;
                    else
                        customeraddress += '\n' + data.Address2;

                if (data.Address3 != 'null' && data.Address3 != '')
                    if (customeraddress == '')
                        customeraddress = data.Address3;
                    else
                        customeraddress += '\n' + data.Address3;

                if (data.CityName != 'null' && data.CityName != '')
                    if (customeraddress == '')
                        customeraddress = data.CityName;
                    else
                        customeraddress += '\n' + data.CityName;

                if (data.CountryName != 'null' && data.CountryName != '')
                    if (customeraddress == '')
                        customeraddress = data.CountryName;
                    else
                        customeraddress += '\n' + data.CountryName;

                
                $('#ClientDetail').html(customeraddress);
            }
        });
    }
}
function SaveCustomerEntry() {

    if ($('#NewCustomerName').val() == '') {
        $.notify("Enter Customer Name", "error");
        return;
    }
    else if ($('#NewPhone').val() == '') {
        $.notify("Enter Customer Phone", "error");
        return;
    }
    else if ($('#NewCityName').val() == '' && $('#NewCountryName').val() == '') {
        $.notify("Enter Customer Location Details", "error");
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
                $.notify(response.message, "success");

            }
            else {
                $('#customerpopup').modal('hide');
                $('#customer').val(response.data.CustomerName);
                $.notify(response.message, "error");
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
(function ($) {

    'use strict';
    function initformControl() {

        $('#QuotationDate').datepicker({
            format: 'dd/mm/yyyy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $('#QuotionValidity').datepicker({
            format: 'dd/mm/yyyy',
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });
        $('#lblcustomer').click(function () {
            showcustomerentry();
        })
        $("#CustomerName").autocomplete({
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
                $("#CustomerName").val(ui.item.value);

                $('#CustomerID').val(ui.item.CustomerId);
            },
            select: function (e, i) {
                e.preventDefault();
                $("#CustomerName").val(i.item.label);

                $('#CustomerID').val(i.item.CustomerId);
            },

        });
        
        $('#CustomerName').change(function () {
            LoadCustomerDetail();
        })
        if ($('#QuotationID').val() > 0) {
            $('#Validity').val($('#Validity').attr('svalue')).trigger('change');
        }
    }
    function init() {
        initformControl();
    }
    $(document).ready(function () {
        init();

    })

})(jQuery);


    (function () {
        "use strict";
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

                                SaveQuotation();

                            }
                        },
                        !1
                    );
                });
            },
            !1
        );
    })(),
    $(document).ready(function () {
        $(".custom-validation").parsley();
       
    });
