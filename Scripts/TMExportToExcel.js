+function ($) {
    "use strict";
    var DF = {
        action: 'click',
        table: 'table',
        sheet: 'Sheet1',
        filename: 'TMExportToExcel',
        modalShow: function () { },
        modalHidden: function () { },
        modalOk: function () { },
        modalCancel: function () { }
    }
    $.fn.TMExportToExcel = function (op) {
        return $(this).each(function () {
            var $this = $(this);
            //$this.attr('data-confirm', 'true');
            //$(document).on('click', '[data-confirm="true"]', function (e) {

            //});
            $this.off('click').on('click', function (e) {
                $.extend(DF, op);
                TMExportToExcel(DF.table, DF.filename, DF.sheet);
                //var uri = 'data:application/vnd.ms-excel;base64,';
                //var template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>';
                //var base64 = function (s) {
                //    return window.btoa(unescape(encodeURIComponent(s)))
                //};

                //var formats = function (s, c) {
                //    return s.replace(/{(\w+)}/g, function (m, p) {
                //        return c[p];
                //    })
                //};

                //var ctx = {
                //    worksheet: 'Sheet1',
                //    table: DF.html
                //}

                //var link = document.createElement("a");
                //link.download = DF.fileName + ".xls";
                //link.href = uri + base64(formats(template, ctx));
                //link.click();
            });
        });
    };
}(jQuery);

var TMExportToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--><meta http-equiv="content-type" content="text/plain; charset=UTF-8"/></head><body><table>{table}</table></body></html>'
        , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, filename = 'TMExportToExcel', sheet = 'Sheet1') {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: sheet || 'Worksheet', table: table.innerHTML }

        var link = document.createElement("a");
        link.download = filename + ".xls";
        link.href = uri + base64(format(template, ctx));
        link.click();
        //window.download = "abc.xls";
        //window.location.href = uri + base64(format(template, ctx))
    }
})();