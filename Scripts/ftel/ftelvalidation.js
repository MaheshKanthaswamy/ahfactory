// Ne pas oublier de mettre le filtre "ValidateAjaxAttribute" dans le filter config ou en attribut du controller ou de la méthode
function ftelvalidateform(e) {
    var _form = $(this);
    if (_form.data('isvalidated') === 'true') {
        return;
    }

    e.preventDefault();

    var validator = $(_form).validate();

    var anyError = false;
    _form.find("input, textarea, select").each(function () {
        if ($(this).attr("type") !== "hidden") {
            if (!validator.element(this)) {
                anyError = true;
            }
        }
    });
    if (anyError) {
        return false;
    }

    var params = _form.serializeArray();
    params.push({ name: '__ajaxvalidation__', value: true });

    var submit = _form.find(".button[type=submit]");
    if (submit.hasClass("loading"))
        return;

    submit.attr("disabled", "disabled");
    submit.addClass("loading");

    $.post(_form.attr("action"), params, function (data) {
        var errors = false;
        if ($.isArray(data)) {
            $.each(data, function (k, v) {
                if (v.key && v.key !== "") {
                    var error = {};
                    error[v.key] = v.errors;
                    try {
                        validator.showErrors(error);
                        errors = true;
                    } catch (e) {}
                }
            });
        }

        submit.removeAttr("disabled");
        if (!errors) {
            _form.data('isvalidated', 'true');
            _form.submit();
        } else {
            _form.find(".input-validation-error:first").focus();
            submit.removeClass("loading");
        }
    });

    return false;
}

function ftelvalidatefield(e) {
    var _this = $(this);
    var _form = _this.closest("form");
    e.preventDefault();
    var validator = $(_form).validate();
    if (validator.element(this)) {
        var params = _form.serializeArray();
        params.push({ name: '__ajaxvalidation__', value: true });
        $.post(_form.attr("action"), params, function (data) {
            var errors = false;
            if ($.isArray(data)) {
                $.each(data, function (k, v) {
                    if (v.key && v.key !== "" && v.key === _this.prop("name")) {
                        var error = {};
                        error[v.key] = v.errors;
                        try {
                            validator.showErrors(error);
                            errors = true;
                        } catch (e) { }
                    }
                });
            }
        });
    }
}

(function ($) {
    $.validator.addMethod("enforcetrue", function (value, element, param) {
        return element.checked;
    });
    $.validator.unobtrusive.adapters.addBool("enforcetrue");

    $.fn.ftelvalidation = function () {
        $(this).each(function () {
            var _this = $(this);
            if ('FORM' === _this.prop("nodeName")) {
                _this.submit(ftelvalidateform);
            } else {
                _this.focusout(ftelvalidatefield);
            }
        })
    };
    // Format de la date (inverse mois et jour)
    $.validator.addMethod(
        "date",
        function (value, element) {
            var bits = value.match(/([0-9]+)/gi), str;
            if (!bits)
                return this.optional(element) || false;
            str = bits[1] + '/' + bits[0] + '/' + bits[2];
            return this.optional(element) || !/Invalid|NaN/.test(new Date(str));
        },
        "Please enter a date in the format dd/mm/yyyy" // Surchargé par messages_fr.js
    );
})(jQuery);