+function ($) {
    "use strict";
    var DF = {
        html: '#print',
        width: null,
        height: null,
        title: 'TMPrinter',
        style: '',
        modalShow: function () { },
        modalHidden: function () { },
        modalOk: function () { },
        modalCancel: function () { }
    };
    $.fn.TMPrinter = function (op) {
        return $(this).each(function () {
            var $this = $(this);
            $this.off('click').on('click', function (e) {
                $.extend(DF, op);
                var mywindow = window.open('', '', (DF.width != null ? 'width=' + DF.width : '') + (DF.height != null ? 'height=' + DF.height : ''));//window.open('', 'PRINT', 'width=1280,height=600');
                mywindow.document.write('<html><head><title>' + DF.title + '</title>');
                mywindow.document.write(DF.style);
                mywindow.document.write('</head><body><div id="print-content" class="container body-content">');
                //mywindow.document.write('<h3>' + document.title + '</h1>');
                //mywindow.document.write(document.getElementById(elem).innerHTML);
                mywindow.document.write($(DF.html).html());
                mywindow.document.write('</div></body><script>window.print();window.close();</script></html>');
                //mywindow.document.write('</div></body><script></script></html>');
                //
                mywindow.document.close(); // necessary for IE >= 10
                mywindow.focus(); // necessary for IE >= 10*/
                //
                //mywindow.print();
                //mywindow.close();
                return true;
            });
        });
    };
}(jQuery);