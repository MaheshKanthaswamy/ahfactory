(function ($, kendo) {
    var DefaultFilterMenu = kendo.ui.FilterMenu.extend({
        init: function (element, options) {
            var that = this,
                  type = "string",
                  operators,
                  initial,
                  link,
                  field;
            var proxy = $.proxy;
            kendo.ui.Widget.fn.init.call(that, element, options);
            operators = that.operators = options.operators || {};
            element = that.element;
            options = that.options;
            if (!options.appendToElement) {
                link = element.addClass("k-filterable").find(".k-grid-filter");
                if (!link[0]) {
                    link = element.prepend('<a class="k-grid-filter" href="#"><span class="k-icon k-filter"/></a>').find(".k-grid-filter");
                }
                link.attr("tabindex", -1).on("click.kendoFilterMenu", proxy(that._click, that));
                link.click(function () {
                    setTimeout(function () {
                        $(".k-filter-menu:visible input[type=text]:visible:first").focus();
                    }, 200);
                })
            }
            that.link = link || $();
            that.dataSource = options.dataSource;
            that.field = options.field || element.attr(kendo.attr("field"));
            that.model = that.dataSource.reader.model;
            that._parse = function (value) {
                return value + "";
            };
            if (that.model && that.model.fields) {
                field = that.model.fields[that.field];
                if (field) {
                    type = field.type || "string";
                    if (field.parse) {
                        that._parse = proxy(field.parse, field);
                    }
                }
            }
            if (options.values) {
                type = "enums";
            }
            that.type = type;
            operators = operators[type] || options.operators[type];
            if (field && field.type == "string") {
                initial = "contains";
            }
            that._defaultFilter = function () {
                return { field: that.field, operator: initial || "eq", value: "" };
            };
            that._refreshHandler = proxy(that.refresh, that);
            that.dataSource.bind("change", that._refreshHandler);
            if (options.appendToElement) { // force creation if used in column menu
                that._init();
            } else {
                that.refresh(); //refresh if DataSource is fitered before menu is created
            }
        }
    });
    kendo.ui.plugin(DefaultFilterMenu);
})(window.kendo.jQuery, window.kendo);