+function ($) {
    "use strict";
    var DF = {
        typeOn: 'dblclick',//'mouseenter',
        typeOff: 'click',
        editable: 'tm-editable',
        editableType: 'tm-editable-type',
        dataVal: 'data-val',
        url: '/',
        data: null,
        success: function () { },
        done: function () { },
        fail: function () { },
        always: function () { }
    }
    $.fn.TMUpdateValue = function (op) {
        var $this = $(this);
        if (DF.typeOn == 'mouseenter')
            DF.typeOff = 'mouseleave';
        else if (DF.typeOn == 'dblclick')
            DF.typeOff = 'click';

        var value = "";
        $this.off(DF.typeOn).on(DF.typeOn, getAttr(DF.editable), function (e) {
            $.extend(DF, op);
            var t = $(this);
            if (t.children('input[type="text"]').length < 1) {
                var editableType = t.attr(DF.editableType);
                value = t.html();
                var html = '<input type="text" id="tmInput" class="form-control" value="' + t.html() + '">';
                t.html(html);
                //if (!editableType) {
                //    t.html(html);
                //}
                //else {
                //    console.log(editableType);
                //}
                t.find('#tmInput').focus();
            };
        });

        $this.off(DF.typeOff).on(DF.typeOff, getAttr(DF.editable), function (e) {
            var t = $(this);
            var input = $this.find('#tmInput');
            var td = $this.find('#tmInput').parent();
            if ((input.length > 0 && !input.is(e.target) && input.has(e.target).length === 0) || e.keyCode == 13) {
                var data = $.extend({}, DF.data, { id: td.parent().attr('data-id'), col: td.attr(DF.dataVal), val: input.val() });
                setTimeout(function () {
                    if (DF.url != null)
                        $.post(DF.url, data, function (d) {
                            td.html(input.val());
                        });
                    else
                        td.html(input.val());
                }, 1);
            };
            if (e.keyCode == 27) {
                td.html(value);
            }
        });
    };
    function getAttr(attr) {
        return '[' + attr + ']';
    }
}(jQuery);