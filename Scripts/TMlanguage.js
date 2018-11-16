//TMLanguage
(function ($) {
    'use strict';
    var DF = {
        host: window.location.protocol + '//' + window.location.host + '/',
        dir: 'Language',
        lang: 'vi-vn',
        langContent: 'tm-lang',
        langAtrr: 'tm-lang-attr',
        langSegment: 'tm-lang-segment',
        langAresSegment: 'tm-lang-ares-segment',
        val: '',
        attrList: [
            'title',
            'placeholder',
            'data-val-required',
            'data-val-number',
            'data-val-email',
            'data-val-equalto',
            'data-val-remote',
            'data-val-length',
            'data-val-minlength',
            'data-val-maxlength',
            'data-val-range',
            'data-val-regex',
            'data-val-creditcard',
            'data-val-select',
            'data-val-checkbox'
        ]
    }
    var Segment = (function (a) {
        return a;
    })(window.location.pathname.substr(1).split('/'));
    $.fn.TMLanguage = function (op) {
        $.extend(DF, op);
        console.log(DF.host);
        //var host = window.location.protocol + '//' + window.location.host + '/';
        var rs;
        $.ajax({
            url: DF.host + DF.dir + '/' + DF.lang + '.json',
            //url: '../' + DF.dir + '/' + DF.lang + '.json',
            async: false,
            dataType: 'json',
            success: function (json) {
                json = rs = ConvertKeysToLowerCase(json); //ConvertKeysToLowerCase(json);
                //langContent
                $('[' + DF.langContent + ']').each(function () {
                    var thisAttr = $(this).attr(DF.langContent).toLowerCase();
                    var val = thisAttr.split('.');
                    if (val.length > 1 && json.hasOwnProperty(val[0]))
                        $(this).html(json[val[0]].hasOwnProperty(val[1]) ? json[val[0]][val[1]] : thisAttr)
                    else
                        $(this).html(thisAttr)
                });
                //langAtrr
                $('[' + DF.langAtrr + '="true"]').each(function () {
                    for (var i = 0; i < DF.attrList.length; i++) {
                        var thisAttr = $(this).attr(DF.attrList[i]);
                        if (thisAttr === undefined) continue;
                        var val = thisAttr.toLowerCase().split('.');
                        if (val.length > 1 && json.hasOwnProperty(val[0]))
                            $(this).attr(DF.attrList[i], json[val[0]].hasOwnProperty(val[1]) ? json[val[0]][val[1]] : thisAttr)
                        else
                            $(this).attr(DF.attrList[i], thisAttr)
                    }
                });
                //Segment
                $('[' + DF.langSegment + ']').each(function () {
                    var thisAttr = $(this).attr(DF.langSegment).toLowerCase();
                    var val = thisAttr.split('.');
                    if (val.length === 1) {
                        if (Segment.length === 1 && json.hasOwnProperty('Global'))
                            $(this).html(json['Global'].hasOwnProperty(val[0]) ? json['Global'][val[0]] : thisAttr)
                        else if (Segment.length > 1 && json.hasOwnProperty(Segment[1]))
                            $(this).html(json[Segment[1]].hasOwnProperty(val[0]) ? json[Segment[1]][val[0]] : thisAttr)
                    }
                    else if (val.length > 1) {
                        if (Segment.length === 1 && json.hasOwnProperty(val[0]))
                            $(this).html(json[val[0]].hasOwnProperty(val[1]) ? json[val[0]][val[1]] : thisAttr)
                        else if (Segment.length > 1 && json.hasOwnProperty(Segment[1]))
                            $(this).html(json[Segment[1]].hasOwnProperty(val[1]) ? json[Segment[1]][val[1]] : thisAttr)
                    } else
                        $(this).html(thisAttr)
                });
                //Segment Ares
            }
        });
        return rs;
    }
    function readTextFile(file, callback) {
        var rawFile = new XMLHttpRequest();
        rawFile.overrideMimeType("application/json");
        rawFile.open("GET", file, true);
        rawFile.onreadystatechange = function () {
            if (rawFile.readyState === 4 && rawFile.status === "200") {
                callback(rawFile.responseText);
            }
        }
        rawFile.send(null);
    }
    function SetLangAttr(json, attr) {
        //if ($('[' + $attr[i] + ']') === 'true') {

        //} else {

        //}
        var $attr = attr.split(',');
        if ($attr.length > 1)
            for (var i = 0; i < $attr.length; i++) {
                $('[' + $attr[i] + ']').each(function () {
                    var thisAttr = $(this).attr($attr[i]);
                    var val = thisAttr.split('.');
                    if (val.length > 1 && json.hasOwnProperty(val[0]))
                        $(this).attr($attr[i], json[val[0]].hasOwnProperty(val[1]) ? json[val[0]][val[1]] : thisAttr)
                    else
                        $(this).attr($attr[i], thisAttr)
                });
            }
        else
            $('[' + attr + ']').each(function () {
                var thisAttr = $(this).attr(attr);
                var val = thisAttr.split('.');
                if (val.length > 1 && json.hasOwnProperty(val[0]))
                    $(this).html(json[val[0]].hasOwnProperty(val[1]) ? json[val[0]][val[1]] : thisAttr)
                else
                    $(this).html(thisAttr)
            });
    }

})($);
//var TMLanguages = $.fn.TMLanguage();
//function TMLanguage(lang) {
//    lang = lang.toLowerCase().split('.');
//    if (lang.length > 1)
//        return TMLanguages[lang[0]][lang[1]];
//};
//function TMLanguageTitle(afterFix) {
//    afterFix = afterFix !== undefined ? ' - ' + afterFix : '';
//    if (Segment.length == 1) {
//        if (TMLanguages.hasOwnProperty(Segment[0].toLowerCase()))
//            document.title = TMLanguages[Segment[0].toString().toLowerCase()]['title'] + afterFix;
//        else
//            if (TMLanguages.hasOwnProperty(Segment[0].toLowerCase()))
//                document.title = TMLanguages[Segment[0].toLowerCase()]['title'] + afterFix;
//    }
//    else if (Segment.length > 1)
//        if (TMLanguages.hasOwnProperty(Segment[1].toLowerCase()))
//            document.title = TMLanguages[Segment[1].toLowerCase()]['title'] + afterFix;//TMLanguages[Segment[1]]['title'];
//};

//$('#indexAdd').on('click', function () {
//    $('#PartialCreateLoad').load('http://localhost:57920/CMS/Users/PartialCreate', function () {
//        $.validator.unobtrusive.parseDynamicContent('#FormCreate input[name="Username"]');
//        //$('#FormCreate').removeData('validator');
//        //$('#FormCreate').removeData('unobtrusiveValidation');
//        //$.validator.unobtrusive.parse($('#FormCreate'));
//        ////$.validator.unobtrusive.parse('#FormCreate');
//        $('#ModalCreate').modal('show')
//        console.log($('#FormCreate'))
//    })
//})