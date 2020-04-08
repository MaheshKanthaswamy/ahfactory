# Reference jQuery
$ = jQuery

# Adds plugin object to jQuery
$.fn.extend
  updateImage: (options) ->
    # Default settings
    settings =
      url: 'UpdateImage'
      debug: false

    # Merge default settings with options.
    settings = $.extend settings, options

    # Simple logger.
    log = (msg...) ->
      console?.log msg... if settings.debug

    send = () ->
      if FormData
        log "envoie via XHR2"
        sendXHR2
      else
        log "envoie via XHR1"
        sendXHR1

    # Bons navigateurs
    sendXHR2 = () ->
      $img = $ this
      $("<input>")
      .attr("type", "file")
      .click()
      .change () ->
        $img.css("opacity", "0.5")
        formData = new FormData()
        formData.append "image", this.files[0]
        request = new XMLHttpRequest()
        request.open("POST", settings.url);
        request.onreadystatechange = () ->
          if request.readyState == 4 && request.status == 200
            src = $img.attr("src")
            src += if src.indexOf("?") > 0 then "&" else "?"
            src += "time=" + new Date().getTime()
            $img.attr("src", src)
            setTimeout (() ->  $img.css("opacity", "1")), 600
        request.send(formData)

    # IE 8 & 9
    sendXHR1 = () ->
      log "création du formulaire"
      $form = $("<form>")
        .attr("action", settings.url)
        .attr("method", "post")
        .attr("enctype", "multipart/form-data")
        .css("display", "none")
      log "création de l'input type file"
      $file = $("<input>")
        .attr("type", "file")
        .attr("name", "image")
        .appendTo($form)
      log "add change action to input"
      $file.change () ->
        $form.submit()
      $("body").append $form
      $file.click()

    # _Insert magic here._
    return @each () ->
      $img = $(this)
      log "add click action to", $img
      $img
        .css "cursor", "pointer"
        .click send()