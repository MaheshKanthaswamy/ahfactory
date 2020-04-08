(function ($) {
    $("body").on("click", "a[data-reveal-popup]", function (e) {
        var id = "popup_" + Math.floor((Math.random() * 1000000) + 1);
        e.preventDefault();
        var $this = $(this);
        var url = $this.attr("href");
        var elem = $("<div>").attr("id", id).addClass("reveal");

        $("body").append(elem);

        var width = $this.attr("data-reveal-width");
        var height = $this.attr("data-reveal-height");
        var fwidth = $this.attr("data-reveal-force-width");
        var fheight = $this.attr("data-reveal-force-height");
        var classes = $this.attr("data-reveal-classes");
        var closeother = $this.attr("data-reveal-close-other");

        var closebutton = $this.attr("data-reveal-close-button");
        var onclose = $this.attr("data-reveal-onclose");

        var modal = new Foundation.Reveal(elem); 

        $.ajax(url).done(function (resp) {
            var randomId = "reveal-overlay_" + Math.floor((Math.random() * 1000000) + 1);
            var $modal = $("#" + id);
            $modal.closest(".reveal-overlay").attr("id", randomId);
            $modal.addClass("data-reveal-reveal");
            $modal.addClass(classes);
            $modal.html(resp);

            if (fwidth === "true" || fheight === "true") {
                $modal[0].style.setProperty("min-height", "0px", "");
                $modal[0].style.setProperty("height", "auto", "important");
            }

            if (width) {
                var w = width;
                var iw = "";
                if (fwidth === "true") {
                    iw = "important";
                }
                $modal[0].style.setProperty("width", w, iw);
            }
            if (height) {
                var h = height;
                var ih = "";
                if (fheight === "true") {
                    ih = "important";
                }
                $modal[0].style.setProperty("height", h, ih);
            }
            if (closebutton !== "false") {
                $modal.append($('<div class="close-button" data-close aria-label="Close modal"><span aria-hidden="true">&times;</span></div>'));
            }
            if (closeother) {
                if (closeother === "true") {
                    $(".reveal-overlay:not(#" + randomId + ") > .reveal").foundation('close');
                }
            }
           

            $modal.foundation('open');

            var $form = $modal.find("form");
            if ($form.length > 0) {
                $form.ftelvalidation(true);
            }
            
            $modal.on("closed.zf.reveal", function () { // onclose
                if (typeof kendo !== "undefined") {
                    kendo.destroy($modal);
                }
                $modal.closest(".reveal-overlay").remove();
                if (onclose && window[onclose]) window[onclose](); // appel à la fonction "onclose"
            });
        });

        return false;
    });
})(jQuery);