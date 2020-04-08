
/* Composants Custom Javascript
--------------------------------*/
$(document).on('change', '.btn-file :file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});
$(document).ready(function () {
    $('.btn-file :file').on('fileselect', function (event, numFiles, label) {
        var input = $(this).parents('.button-group').find(':text'),
            log = numFiles > 1 ? numFiles + ' files selected' : label;

        if (input.length) {
            input.val(log);
        } else {
            if (log) alert(log);
        }

    });
});


/* Fonctions Utils
-----------------------------------------*/
function refreshGrid(gridId) {
    var grid = $('#' + gridId);
    //console.log(grid);
    if (grid && grid.data('kendoGrid'))
        grid.data('kendoGrid').dataSource.read();
}
function openWindow(windowId) {
    var modal = $('#' + windowId);
    if (modal)
        modal.foundation('reveal', 'open');
}
function closeWindow(windowId) {
    var modal = $('#' + windowId);
    if (modal)
        modal.foundation('reveal', 'close');
}
function LoadStart() {
    openWindow('loadingWindow');
}
function LoadStop() {
    closeWindow('loadingWindow');
}

function onDataBound(e) {
    var grid = e.sender;
    if (grid.dataSource.total() == 0) {
        var colCount = grid.columns.length;
        $(e.sender.wrapper)
            .find('tbody')
            .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" class="no-data">Aucune données</td></tr>');
    }
}