+function ($) {
    "use strict";
    var DF = {
        action: 'click',
        type: 'confirm',//lg,sm
        txtTitle: 'Yêu cầu xác nhận!',
        txtContent: 'Bạn có chắc chắn muốn thực hiện?',
        labelCancel: 'Hủy bỏ',
        labelOk: 'Xác nhận',
        target: null,
        fade: false,
        backdrop: true,
        keyboard: true,
        resetForm: false,
        modalShow: function () { },
        modalHidden: function () { },
        modalOk: function () { },
        modalCancel: function () { }
    }
    $.fn.TMConfirm = function (op) {
        return $(this).each(function () {
            var $this = $(this);
            $.extend(DF, op);
            if (DF.target === null)
                $this.off('click').on('click', function (e) {
                    GetHtml($this, DF);
                });
            else {
                GetHtml($this, DF);
            }
        });
    };
    function GetHtml($this, DF) {
        var html = '<div class="modal ' + (DF.fade ? 'fade' : '') + '" id="TMConfirm" tabindex="-1" role="dialog" style="display:block" data-backdrop="' + (DF.backdrop ? 'true' : 'false') + '" data-keyboard="' + (DF.keyboard ? 'true' : 'false') + '">\
                        <div class="modal-dialog modal-'+ DF.type + '" role="document">\
                            <div class="modal-content">';
        html += '<div class="modal-header">\
                    <h5 class="modal-title">'+ DF.txtTitle + '</h5>\
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>\
                </div>';
        html += '<div class="modal-body">' + DF.txtContent + '</div>';
        html += '<div class="modal-footer">\
                    <button type="button" class="btn btn-primary" data-dismiss="modal">'+ DF.labelOk + '</button>\
                    <button type="button" class="btn btn-default" data-dismiss="modal">'+ DF.labelCancel + '</button>\
                </div>';
        html += '</div></div></div>';
        //
        $('body').append(html);
        var modal = $(document).find('#TMConfirm');
        //
        //$(modal).modal('show');
        //
        $(modal).find('.btn.btn-primary').on('click', function () {
            if (typeof DF.modalOk == 'function') {
                DF.modalOk($this);
                console.log("OK");
            }
        });
        //
        $(modal).find('.btn.btn-default').on('click', function () {
            if (typeof DF.modalCancel == 'function') {
                DF.modalCancel($this);
                console.log("Cancel");
            }
        });
        //
        $(modal).on('show.bs.modal', function (e) {
            if (typeof DF.modalShow == 'function') {
                DF.modalShow($this);
                console.log("Show");
            }
        });
        //
        $(modal).on('hidden.bs.modal', function (e) {
            modal.remove();
            if (typeof DF.modalHidden == 'function') {
                DF.modalHidden($this);
                console.log("Hide");
            }
        });
        if (DF.resetForm) $this.trigger('reset');
        //
        $(modal).find('button').on('click', function () { modal.remove(); })
    };
}(jQuery);