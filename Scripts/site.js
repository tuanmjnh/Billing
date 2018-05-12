//Declares
var $TMAlert = '#TMAlert',
    $isTooltip = '.isTooltip',
    $tinymce = '.tinymce',
    FormModal = '#FormModal',
    DateNow = new Date();
var $baseUrl = Host + '/',
    $controllerUrl = $baseUrl + Segment[0] + '/',
    $currentUrl = CurrentUrl;
var TMLanguages = $.fn.TMLanguage({ host: $baseUrl });
function TMLanguage(lang) {
    lang = lang.toLowerCase().split('.');
    if (lang.length > 1)
        return TMLanguages[lang[0]][lang[1]];
};
function TMLanguageTitle(afterFix) {
    afterFix = afterFix !== undefined ? ' - ' + afterFix : '';
    if (Segment.length == 1) {
        if (TMLanguages.hasOwnProperty(Segment[0].toLowerCase()))
            document.title = TMLanguages[Segment[0].toString().toLowerCase()]['title'] + afterFix;
        else
            if (TMLanguages.hasOwnProperty(Segment[0].toLowerCase()))
                document.title = TMLanguages[Segment[0].toLowerCase()]['title'] + afterFix;
    }
    else if (Segment.length > 1)
        if (TMLanguages.hasOwnProperty(Segment[1].toLowerCase()))
            document.title = TMLanguages[Segment[1].toLowerCase()]['title'] + afterFix;//TMLanguages[Segment[1]]['title'];
};

$(function () {
    TMLanguageTitle('Billing');
    $($isTooltip).tooltip({ animation: false });
    $(FormModal).find('.modal').on('hidden.bs.modal', function (e) {
        $('.modal-backdrop').remove();
        $(this).remove();
    });
});

//btnGetForm Global Function
$('.btnGetForm').GetForm({ form: FormModal });

//Action link
//$('.btnAction').on('click', function (e) {
//    e.preventDefault();
//    ActionLink({ url: $(this).attr('data-url'), data: { data_id: $(this).attr('data-id') } });
//});

//ActionLink
function ActionLink(obj) {
    var formdata = new FormData($('form')[0]);
    for (var i in obj.data)
        formdata.append(i, obj.data[i]);
    //obj.data = $.extend({}, { '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() }, obj.data);
    $.ajax({
        url: obj.url,
        type: obj.type ? obj.type : 'POST',
        dataType: 'json',
        data: formdata,
        cache: false,
        contentType: false,
        processData: false,
        success: function (d) {
            if (d.success) {
                $('#TMAlert').TMAlert({ type: "success", message: d.success });
                $("form")[0].reset();
            }
            if (d.danger)
                $('#TMAlert').TMAlert({ type: "danger", message: d.danger });
            if (d.warning)
                $('#TMAlert').TMAlert({ type: "warning", message: d.warning });
            if (d.url)
                setTimeout(function () { window.location = d.url }, (obj.urlTimeOut ? obj.urlTimeOut : 0));
                //window.location = d.url;
            $('.custom-file-label').html('');
            $('.lblUpload').html('');
            if (typeof obj.success == 'function') obj.success();
        },
        error: function (xhr, error, status) {
            $('#TMAlert').TMAlert({ type: "danger", message: window.location.protocol + '//' + window.location.host + this.url + ' is ' + xhr.status + ' ' + xhr.statusText });
            //console.log(error, status);
        }
    })
        .done(function () { if (typeof obj.done == 'function') obj.done(); })
        .fail(function () { if (typeof obj.fail == 'function') obj.fail(); })
        .always(function () { if (typeof obj.always == 'function') obj.always(); });
};

//TinyMCE
function getTinyMCE() {
    tinymce.init({
        selector: $tinymce,
        mode: 'specific_textareas',//'textareas'
        //theme: 'advanced',
        //force_br_newlines: false,
        //force_p_newlines: false,
        //forced_root_block: '',
        //encoding: 'xml',
        //entity_encoding: "raw",
        //convert_urls: false,
        theme: 'modern',
        //width: 500,
        //height: 300,
        plugins: [
            'advlist autolink link image lists charmap print preview hr anchor pagebreak spellchecker',
            'searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking',
            'save table contextmenu directionality emoticons template paste textcolor'
        ],
        //content_css: 'css/content.css',
        toolbar: 'insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | print preview media fullpage | forecolor backcolor emoticons',
        style_formats: [
            { title: 'Bold text', inline: 'b' },
            { title: 'Red text', inline: 'span', styles: { color: '#ff0000' } },
            { title: 'Red header', block: 'h1', styles: { color: '#ff0000' } },
            { title: 'Example 1', inline: 'span', classes: 'example1' },
            { title: 'Example 2', inline: 'span', classes: 'example2' },
            { title: 'Table styles' },
            { title: 'Table row 1', selector: 'tr', classes: 'tablerow1' }
        ],
        setup: function (editor) {
            editor.on('change', function (i) {
                tinymce.triggerSave();
            });
            editor.on("SaveContent", function (i) {
                i.content = i.content.replace(/&#39/g, "&apos");
            });
        }
    });
}
//Fix TinyMCE in Modal
$(document).on('focusin', function (e) {
    if ($(e.target).closest(".mce-window").length) {
        e.stopImmediatePropagation();
    }
});
//
//$.fn.TMCheckBox('.chkall', '.chkitem', '.btn-chk');