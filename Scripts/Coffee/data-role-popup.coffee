# Reference jQuery
$ = jQuery

changeTitle = (a, e, fn) ->
  elm = e.sender.element
  if elm.children().length > 0
    title = $(elm).find("h1, h2, h3, h4, h5").first()
    text = title.text()
    text = a.text() if !text
    elm.data("kendoWindow").title(text).center()
    title.remove()
    fn(e) if fn
  else
    setTimeout (() -> changeTitle a, e, fn), 200

openPopup = () ->
  a = $ this
  href = a.attr "href"
  onclose = a.attr "data-popup-on-close"
  onopen = a.attr "data-popup-on-open"
  $("<div>").appendTo("body").kendoWindow
    content: href
    minWidth: "65%"
    modal: true
    activate: (e) -> changeTitle a, e, window[onopen]
    deactivate: (e) ->
        this.destroy()
        fn = window[onclose]
        fn(e) if fn
  false

$.fn.addOpenPopupEvents = () -> this.find("a[data-popup]").click openPopup

$ () -> $(document.body).addOpenPopupEvents()