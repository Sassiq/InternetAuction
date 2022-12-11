    $(document).ready(function () {
        var loginLink = $("a[id='loginLink']");
        loginLink.attr({ "href": "#", "data-toggle": "modal", "data-target": "#loginModal" })
        
        $("#loginform").submit(function (event) {
            $("#WaitingImg").css("display", "inline");
            if ($("#loginform").valid()) {
                var username = $("#Username").val();
                var password = $("#Password").val();
                var antiForgeryToken = Sample.Web.ModalLogin.Views.Common.getAntiForgeryValue();
                Sample.Web.ModalLogin.Identity.LoginIntoStd
                (username, password, antiForgeryToken,
                Sample.Web.ModalLogin.Views.LoginModal.loginSuccess,
                Sample.Web.ModalLogin.Views.LoginModal.loginFailure);
            }
            return false;
        });
        $("#loginModal").on("hidden.bs.modal", function (e) {
            Sample.Web.ModalLogin.Views.LoginModal.resetLoginForm();
        });
        $("#loginModal").on("shown.bs.modal", function (e) {
            $("#Username").focus();
        });
    });

var Sample = Sample || {};
Sample.Web = Sample.Web || {};
Sample.Web.ModalLogin = Sample.Web.ModalLogin || {};
Sample.Web.ModalLogin.Views = Sample.Web.ModalLogin.Views || {}
Sample.Web.ModalLogin.Views.Common = {
    getAntiForgeryValue: function () {
        return $('input[name="__RequestVerificationToken"]').val();
    }
}
Sample.Web.ModalLogin.Views.LoginModal = {
    resetLoginForm: function () {
        $("#loginform").get(0).reset();
        $("#alertBox").css("display", "none");
    },
    loginFailure: function (message) {
        var alertBox = $("#alertBox");
        alertBox.html(message);
        alertBox.css("display", "block");

    },
    loginSuccess: function (response) {
        window.location.href = window.location.href;
    }
}

Sample.Web.ModalLogin.Identity = {
    LoginIntoStd: function (username, password, antiForgeryToken,
                            successCallback, failureCallback) {
        var data = {
            "__RequestVerificationToken": antiForgeryToken,
            "Username": username, "Password": password
        };
        $.ajax({
            url: "/Account/Login",
            type: "POST",
            data: data
        })
        .done(function (loginSuccessful) {
            if (loginSuccessful) {
                successCallback();
            }
            else {
                $("#WaitingImg").css("display", "none");
                if ($("#WebsiteLanguage").val() == "en-US") {
                    failureCallback("Invalid login attempt");
                }
                else {
                    failureCallback("فشل تسجيل الدخول");
                }
            }
        })
        .fail(function (jqxhr, textStatus, errorThrown) {
            $("#WaitingImg").css("display", "none");
            failureCallback(errorThrown);
        });
    }
}
