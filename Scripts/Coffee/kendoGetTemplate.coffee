this.kendoGetTemplate = (field, data) ->
  template = ""
  $(".k-grid").each (k, grid) ->
    columns = $(grid).data("kendoGrid").columns
    $.each columns, (k, col) ->
      template = col.template if col.field == field
  template = kendo.template template
  if !data
    template
  else
    obj = data : {}
    obj.data[field] = data
    template obj