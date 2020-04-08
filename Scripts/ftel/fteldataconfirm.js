(function ($) {
    $("body").on("click", "a[data-confirm]", function (e) {
        var text = $(this).attr("data-confirm");
        return confirm(text);
    });
})(jQuery);