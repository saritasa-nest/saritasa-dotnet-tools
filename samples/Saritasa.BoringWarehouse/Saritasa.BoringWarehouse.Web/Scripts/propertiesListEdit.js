(function () {
    "use strict";

    var idTmpl = '<script id="propertyIdTmpl" type="text/x-jquery-tmpl">\
        <div>\
            <input type="hidden" id="Properties_${Index}__Id" name="Properties[${Index}].Id" value="${Id}" data-index="${Index}" />\
        </div>\
    </script>';

    var nameTmpl = '<script id="propertyNameTmpl" type="text/x-jquery-tmpl">\
        <div>\
            <div class="editor-value input-group col-md-12" style="display: none">\
                <input data-index="${Index}" type="text" name="Properties[${Index}].Name" id="Properties_${Index}__Name" value="${Name}" class="form-control input-sm" />\
                <div class="btn-group">\
                    <btn class="btn btn-success btn-xs glyphicon glyphicon-ok btn-commit" />\
                </div>\
                <div class="btn-group">\
                    <btn class="btn btn-danger btn-xs glyphicon glyphicon-remove btn-cancel" />\
                </div>\
            </div>\
            <span class="display-value">${Name}</span>\
        </div>\
    </script>';

    var valueTmpl = '<script id="propertyValueTmpl" type="text/x-jquery-tmpl">\
        <div>\
            <div class="editor-value input-group col-md-12" style="display: none">\
                <input data-index="${Index}" type="text" name="Properties[${Index}].Value" id="Properties_${Index}__Value" value="${Value}" class="form-control input-sm" />\
                <div class="btn-group">\
                    <btn class="btn btn-success btn-xs glyphicon glyphicon-ok btn-commit" title="Apply" />\
                </div>\
                <div class="btn-group">\
                    <btn class="btn btn-danger btn-xs glyphicon glyphicon-remove btn-cancel" title="Cancel" />\
                </div>\
            </div>\
            <span class="display-value">${Value}</span>\
        </div>\
    </script>';

    $.fn.PropertiesListEdit = function (options) {
        var table = this;
        var propertiesTable = this.DataTable({
            dom: "Bfrtip",
            paging: false,
            info: false,
            searching: false,
            rowId: "Id",
            data: options.data,
            columns: [
                {
                    data: "Id",
                    className: "hidden-column",
                    render: function (data, type, row, meta) {
                        return $(idTmpl).tmpl({
                            Index: meta.row,
                            Id: data
                        }).html();
                    }
                },
                {
                    data: "Name",
                    render: function (data, type, row, meta) {
                        return $(nameTmpl).tmpl({
                            Index: meta.row,
                            Name: data
                        }).html();
                    }
                },
                {
                    data: "Value",
                    render: function (data, type, row, meta) {
                        return $(valueTmpl).tmpl({
                            Index: meta.row,
                            Value: data
                        }).html();
                    }
                },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    width: 1,
                    render: function (data, type, row, meta) {
                        return $('<span>')
                                .append($("<btn title='Remove property' class='btn btn-danger btn-xs glyphicon glyphicon-minus delete-property'>"))
                                .html();
                    }
                },
            ],
            buttons: [
                {
                    text: "Add property",
                    action: function (e, dt, node, config) {
                        var length = $(table).find('tbody>tr').length;
                        propertiesTable.row.add({
                            Id: 0,
                            Name: 'Property #' + (length + 1),
                            Value: 'Value'
                        }).draw();
                    }
                }
            ]
        });

        $(table).on('click', '.display-value', function (e) {
            // Hide all editors
            $('td .editor-value').hide();
            $('td .display-value').show();
            // Show editor for current and hide title
            $(this).parent().children('.editor-value').show().find('input').val($(this).text());
            $(this).hide();
        });

        $(table).on('click', '.editor-value .btn-commit', function (e) {
            // Set text value and hide editor
            var editor = $(this).closest('td').find('.editor-value');
            var display = $(this).closest('td').find('.display-value');
            display.text(editor.find('input').val());
            editor.hide();
            display.show();
        });

        $(table).on('click', '.editor-value .btn-cancel', function (e) {
            // Reset value for editor and switch controls
            var editor = $(this).closest('td').find('.editor-value');
            var display = $(this).closest('td').find('.display-value');
            editor.find('input').val(display.text());
            editor.hide();
            display.show();
        });

        $(table).on('click', '.delete-property', function (e) {
            var row = $(this).closest('tr');
            propertiesTable.row(row).remove();
            // Redraw for inputs names corrects
            var dataBack = propertiesTable.data();
            propertiesTable.clear();
            propertiesTable.rows.add(dataBack).draw();
        });
    };
})();
