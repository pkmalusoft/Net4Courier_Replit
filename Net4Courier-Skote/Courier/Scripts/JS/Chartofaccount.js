function DeleteHead(id) {

    Swal.fire({ title: "Are you sure?", text: "Do you want to delete this?", icon: "warning", showCancelButton: !0, confirmButtonColor: "#34c38f", cancelButtonColor: "#f46a6a", confirmButtonText: "Yes, delete it!" }).then(
        function (t) {
            if (t.value) {

                $.ajax({
                    type: "POST",
                    url: '/Accounts/DeleteAcHead',
                    datatype: "json",
                    data: {
                        'id': id
                    },
                    success: function (data) {
                        if (data.status == "OK") {
                            Swal.fire("Delete Status!", data.message, "success");
                            window.location.reload();
                        }
                        else
                            Swal.fire("Delete Status!", data.message, "error");
                    }
                });

            }
        });
}


function saveaccounthead() {
    debugger;
    $('#btnSaveAccounts').attr('disabled', 'disabled');
    var accountobj = {
        AcHeadID: $('#AcHeadID').val(),
        AcHead1: $('#AcHead1').val(),
        AcHeadKey: $('#AcHeadKey').val(),
        AccountDescription: $('#AccountDescription').val(),
        AcGroupID: $('#AcGroupID').val(),
        TaxApplicable: $('#TaxApplicable').prop('checked'),
        TaxPercent: $('#TaxPercent').val(),
        StatusControlAC: $('#StatusControlAC').prop('checked')

    }
    $.ajax({
        type: "POST",
        url: '/Accounts/SaveAccountHead/',
        datatype: "json",
        data: { a: accountobj },
        success: function (response) {
            debugger;
            if (response.status == "OK") {
                Swal.fire("Save Status!", response.message, "success");
                //$('#divothermenu').removeClass('hide');
                $('#btnSaveAccounts').removeAttr('disabled');
                var t = document.getElementsByClassName("needs-validation");
                $(t).removeClass('was-validated');
                setTimeout(function () {
                    window.location.href = '/Accounts/CreateAcHead?id=0' 
                },350)
                
                //window.location.reload();

            }
            else {
                $('#btnSaveAccounts').removeAttr('disabled');
                Swal.fire("Save Status!", response.message, "warning");
                //window.location.reload();
            }


        }
    });

}







(function ($) {

    'use strict';
    function initformControl() {

        $("#TaxApplicable").click(function () {
            if ($("#TaxApplicable").is(':checked')) {
                $("#TaxPercent").removeAttr("readonly");

            }
            else {
                $("#TaxPercent").val(0);
                $("#TaxPercent").attr("readonly", "readonly");
            }
        });
        //$("#btnsave").click(function () {
        //    var acgroup = $("#AcGroupID option:selected").val();
        //    var head = $("#AcHead1").val();
        //    var code = $("#AcHeadKey").val();
        //    var prefix = $("#Prefix").val();




        //    if (acgroup == "") {
        //        $("#alert").show();
        //        return false;
        //    }
        //    else if (head == "") {
        //        $("#alert").show();
        //        return false;
        //    }

        //    else {
        //        $("#alert").hide();
        //        return true;
        //    }
        //});





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
                        debugger;
                        if (false === e.checkValidity()) {
                            e.classList.add("was-validated");
                        }
                        else {
                            t.preventDefault();
                            t.stopPropagation();
                            e.classList.remove("was-validated");

                            saveaccounthead();



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
