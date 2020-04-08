this.kendoGetTitle = (field) ->
  title = field
  $(".k-grid").each (k, grid) ->
    columns = $(grid).data("kendoGrid").columns
    $.each columns, (k, col) ->
      title = col.title if col.field == field
  title