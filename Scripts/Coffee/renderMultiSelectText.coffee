#http://blog.credera.com/technology-insights/microsoft-solutions/kendo-multi-select-mvc-kendo-grid/

this.renderMultiSelectText = (selectListArray, attribute = 'Text', char = ', ') ->
  displayText = ''
  if selectListArray && selectListArray[0] 
    $.each selectListArray, (k,v) ->
      displayText += v[attribute] + char
    displayText = displayText.slice 0, -2
  displayText