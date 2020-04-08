function notify(data, type, position) {
    if (!position) position = { right: 30, top: 30 };
    var span = $("<span>").appendTo("body");
    var notif = span.kendoNotification({
        position: position,
        hide: function () {
            setTimeout(function () {
                $(notif.element).remove();
                span.remove();
            }, notif.options.animation.close.duration);
        }
    }).data("kendoNotification");
    notif.show(data, type);
}