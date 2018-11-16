+function ($) {
    "use strict";
    var DF = {
        target: 'table-fixed-header'
    }
    $.fn.TMTableFixedHeader = function (op) {
        var $this = $(this);
        $.extend(DF, op);
        var table = $this.find('table');
        var thead = table.find('thead');
        var tbody = table.find('tbody');
        var html = '<table class="' + table.attr('class') + '"><thead class="' + thead.attr('class') + '">' + thead.html() + '</thead></table>';
        $this.prepend(html);
        var counter = 0;
        //
        table
            .css('display', 'block')
            .css('overflow-y', 'scroll')
            .css('max-height', '500px')
            .css('overflow-x', 'hidden');
        //
        $($this.find('table')[0])
            .css('margin-bottom', 'initial')
            .css('position', 'absolute')
            .css('z-index', '1')
            .width(tbody.width() + 1);
        //
        $(tbody).find('tr:eq(0) td').each(function () {
            var width = $(this).width();
            $($this.find('table')[0]).find('tr th:eq(' + counter + ')').width(width);
            $(tbody).find('tr td:eq(' + counter + ')').width(width);
            counter++;
        });
    };
}(jQuery);