﻿// Generated by IcedCoffeeScript 108.0.11
(function() {
  var $,
    __slice = [].slice;

  $ = jQuery;

  $.fn.extend({
    updateImage: function(options) {
      var log, send, sendXHR1, sendXHR2, settings;
      settings = {
        url: 'UpdateImage',
        debug: false
      };
      settings = $.extend(settings, options);
      log = function() {
        var msg;
        msg = 1 <= arguments.length ? __slice.call(arguments, 0) : [];
        if (settings.debug) {
          return typeof console !== "undefined" && console !== null ? console.log.apply(console, msg) : void 0;
        }
      };
      send = function() {
        if (FormData) {
          log("envoie via XHR2");
          return sendXHR2;
        } else {
          log("envoie via XHR1");
          return sendXHR1;
        }
      };
      sendXHR2 = function() {
        var $img;
        $img = $(this);
        return $("<input>").attr("type", "file").click().change(function() {
          var formData, request;
          $img.css("opacity", "0.5");
          formData = new FormData();
          formData.append("image", this.files[0]);
          request = new XMLHttpRequest();
          request.open("POST", settings.url);
          request.onreadystatechange = function() {
            var src;
            if (request.readyState === 4 && request.status === 200) {
              src = $img.attr("src");
              src += src.indexOf("?") > 0 ? "&" : "?";
              src += "time=" + new Date().getTime();
              $img.attr("src", src);
              return setTimeout((function() {
                return $img.css("opacity", "1");
              }), 600);
            }
          };
          return request.send(formData);
        });
      };
      sendXHR1 = function() {
        var $file, $form;
        log("création du formulaire");
        $form = $("<form>").attr("action", settings.url).attr("method", "post").attr("enctype", "multipart/form-data").css("display", "none");
        log("création de l'input type file");
        $file = $("<input>").attr("type", "file").attr("name", "image").appendTo($form);
        log("add change action to input");
        $file.change(function() {
          return $form.submit();
        });
        $("body").append($form);
        return $file.click();
      };
      return this.each(function() {
        var $img;
        $img = $(this);
        log("add click action to", $img);
        return $img.css("cursor", "pointer").click(send());
      });
    }
  });

}).call(this);
