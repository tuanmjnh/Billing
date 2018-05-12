+function ($) {
    "use strict";
    var DF = {
        action: 'click',
        html: '.htmls',//lg,sm
        fileName: 'TMJsonToCSV',
        modalShow: function () { },
        modalHidden: function () { },
        modalOk: function () { },
        modalCancel: function () { }
    };
    function checkJsonData(value) {
        return value == 'null' || value === null || value == 'Invalid date' ? '' : value;
    };
    $.fn.TMJsonToCSV = function (op) {
        //function JSONToCSVConvertor(JSONData, filename = 'ExportDataCSV', ReportTitle, ShowLabel)
        return $(this).each(function () {
            var $this = $(this);
            //$this.attr('data-confirm', 'true');
            //$(document).on('click', '[data-confirm="true"]', function (e) {

            //});
            $this.off('click').on('click', function (e) {
                $.extend(DF, op);
                //If JSONData is not an object then JSON.parse will parse the JSON string in an Object
                var arrData = typeof JSONData != 'object' ? JSON.parse(JSONData) : JSONData;
                var CSV = '';
                //This condition will generate the Label/Header
                if (ShowLabel) {
                    var row = "";

                    //This loop will extract the label from 1st index of on array
                    for (var index in arrData[0]) {
                        //Now convert each value to string and comma-seprated
                        row += index + ',';
                    }
                    row = row.slice(0, -1);
                    //append Label row with line break
                    CSV += row + '\r\n';
                }

                //1st loop is to extract each row
                for (var i = 0; i < arrData.length; i++) {
                    var row = "";
                    //2nd loop will extract each column and convert it in string comma-seprated
                    for (var index in arrData[i]) {
                        row += '"' + arrData[i][index] + '",';
                    }
                    row.slice(0, row.length - 1);
                    //add a line break after each row
                    CSV += row + '\r\n';
                }

                if (CSV == '') {
                    alert("Invalid data");
                    return;
                }

                //this trick will generate a temp "a" tag
                var link = document.createElement("a");
                link.id = "lnkDwnldLnk";

                //this part will append the anchor tag and remove it after automatic click
                document.body.appendChild(link);

                var BOM = "\uFEFF";
                var csv = BOM + CSV;
                blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
                var csvUrl = window.webkitURL.createObjectURL(blob);
                filename = filename + '.csv';
                $("#lnkDwnldLnk").attr({ 'download': filename, 'href': csvUrl });
                $('#lnkDwnldLnk')[0].click();
                document.body.removeChild(link);
            });
        });
    };
}(jQuery);