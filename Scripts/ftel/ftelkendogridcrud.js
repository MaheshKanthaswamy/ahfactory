function ftelkendogriderror(args, $this) {
    if (args.errors) {
        var grid = $this.data("kendoGrid");
        grid.one("dataBinding", function (e) {
            e.preventDefault();
            var jalerted = false;
            $.each(args.errors, function (k, v) {
                var errors = v.errors;
                if (errors.length > 0) {
                    if (!k) {
                        if (!jalerted) {
                            jalerted = true;
                            alert(errors[0]);
                        }
                    } else {
                        var icon = $("<span>").addClass("k-icon").addClass("k-warning");
                        var container = $("[data-container-for=" + k + "]");
                        var field = container.find("[data-bind]");
                        field.addClass("input-validation-error");
                        var msg = $("[data-valmsg-for=" + k + "]");
                        msg.addClass("k-widget").addClass("k-tooltip").addClass(" k-tooltip-validation").addClass("k-invalid-msg");
                        msg.html(icon);
                        msg.append(errors[0]);
                        msg.show();
                        field.change(function() {
                            field.removeClass("input-validation-error");
                            msg.hide();
                        });
                    }
                }
            });
        });
    }
};

$.fn.bindFtelKendoGridError = function () {
    this.each(function () {
        var $this = $(this);
        $this
            .data("kendoGrid")
            .dataSource
            ._events
            .error
            .unshift(function (args) {
                ftelkendogriderror(args, $this);
            });
    });
}

$(function() {
    $(".k-grid").bindFtelKendoGridError();
});