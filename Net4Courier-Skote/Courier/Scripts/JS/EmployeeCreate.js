
function ValidateEmail(inputText) {
    //var mailformat = /^\w+([\.-]?\w+)*@@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
    //if (inputText.match(mailformat)) {
    if (/^\w+([\.-]?\w+)*@@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test(inputText)) {
        return true;
    }
    else {
        $('#msg1').html("Invalid Email address!");
        $('#Email').val('');
        $('#Email').focus();
        return false;
    }
}

function IsEmail(email) {
    var regex = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    if (!regex.test(email)) {
        return false;
    } else {
        return true;
    }
}
function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
function CheckEmail() {
    var emailstatus = false;
    //if ($('#Email').val() != "") {
    //    emailstatus = ValidateEmail($('#Email').val());
    //}
    //if (IsEmail($('#Email').val())==false)
    //{
    //    $('#msg1').html("Invalid Email address!");
    //    $('#Email').val('');
    //    $('#Email').focus();
    //    return false;
    //}    
    if ($('#Email').val().trim() != "") {
        $.ajax({
            type: "Get",
            url: "/EmployeeMaster/CheckUserEmailExist",
            datatype: "Json",
            data: { EmailId: $('#Email').val() },
            success: function (data) {
                debugger;
                console.log(data);
                if (data == "true") {
                    $("#msg1").html("Employee Emailid already exists!");
                    $("#msg1").show();
                    $('#Email').val('');
                    $('#Email').focus();
                    return false;
                }
                else {
                    $("#validations").hide();
                    return true;
                }

            }
        });
    }
}


function SaveEmployee() {
    debugger;
    $('#btnsave').attr('disabled', 'disabled');

    
        
        if (IsEmail($('#Email').val())==false)
    {
        $('#msg1').html("Invalid Email address!");
        $('#Email').val('');
        $('#Email').focus();
        return false;
    }    
         
            $('#msg1').hide();
            var RecPObj = {
                EmployeeID: $('#EmployeeID').val(),
                EmployeeCode: $('#EmployeeCode').val(),
                EmployeeName: $('#EmployeeName').val(),
                JoinDate: $('#JoinDate').val(),
                Address1: $('#Address1').val(),
                Address2: $('#Address2').val(),
                Fax: $('#Fax').val(),
                Email: $('#Email').val(),
                Phone: $('#Phone').val(),
                MobileNo: $('#MobileNo').val(),
                CountryName: $('#CountryName').val(),
                CityName: $('#CityName').val(),
                DesignationID: $('#DesignationID').val(),
                RoleID: $('#RoleID').val(),
                AcHeadID: $('#AcHeadID').val(),
                StatusActive: $('#StatusActive').prop('checked')
            }


            $.ajax({
                type: "POST",
                url: "/EmployeeMaster/SaveEmployee",
                datatype: "Json",
                data: { v: RecPObj },
                success: function (response) {
                    if (response.status == "OK") {

                        //Swal.fire("Save Status", response.message, "success");
                        Swal.fire({ title: "Save Status", text: response.message }).then(
                            function (t) {
                                if (t.value) {
                                    location.href = '/EmployeeMaster/Index';
                                }
                            });
                    }
                    else {
                        $('#btnsave').removeAttr('disabled');
                        Swal.fire("Save Status", response.message, "error");
                        return false;
                    }
                }
            });
         
    } 
  
    

(function ($) {

    'use strict';
    function initformControl() {
        $('#JoinDate').datepicker({
            dateFormat: 'dd-mm-yy',
            changeMonth: true,
            changeYear:true
        }).on('changeDate', function (e) {
            $(this).datepicker('hide');
        });

        
      
        $("#Email").change(function () {
            var emailstatus = false;
            
            if ($('#Email').val().trim() != "") {
                $.ajax({
                    type: "Get",
                    url: "/EmployeeMaster/CheckUserEmailExist",
                    datatype: "Json",
                    data: { EmailId: $('#Email').val() ,UserId:$('#UserID').val() },
                    success: function (data) {
                        debugger;
                        console.log(data);
                        if (data == "true") {
                            $("#msg1").html("User Emailid already exists!");
                            $("#msg1").show();
                            $('#Email').val('');
                            $('#Email').focus();
                            return false;
                        }
                        else {
                            $("#msg1").hide();
                            return true;
                        }

                    }
                });
            }

        });

        $('#CountryName').autocomplete({
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
                event.preventDefault();
                $('#CountryName').val(ui.item.label);
                return false;
            },
            focus: function (event, ui) {
                $('#CountryName').val(ui.item.label);

                return false;
            }
        });

        setTimeout(function () {
            if ($('#EmployeeID').val() > 0) {                
                $('#btnsave').val('Update');
                $('#divothermenu').removeClass('hide');
                //$('#Email').attr('readonly', 'readonly');
            }
            else {
                $('#divothermenu').addClass('hide');
               
            }
        },100)
      
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
                            SaveEmployee();

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
        $("#EmployeeCode").attr('readonly', 'readonly');
        $("#EmployeeName").focus();

    })

})(jQuery)