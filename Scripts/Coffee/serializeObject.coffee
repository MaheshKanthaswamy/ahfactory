$.fn.serializeObject = () ->
  obj = {}
  $(this).find('select, textarea, input').each () ->
    obj[$(this).attr('name')] = $(this).val()
  $(this).find("input[type=checkbox]").each () ->
    obj[$(this).attr('name')] = null
    obj[$(this).attr('name')] = $(this).is(':checked') if $(this).is(':visible')
  $(this).find('input[type=radio]').each () ->
    val = $('input[type=radio][name="'+$(this).attr('name')+'"]:checked').val()
    val = $('input[type=radio][name="'+$(this).attr('name')+'"][checked]').val() if !val
    obj[$(this).attr('name')] = val
  obj