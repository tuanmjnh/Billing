+function ($) {
    "use strict";
    var DF = {
        TagTxt: 'TagTxt',
        TagButton: 'TagButton',
        TagList: '#TagList',
        TagListSpan: '.tag',
        HiddenName: 'TMTag',
        data: null
    }
    $.fn.TMTag = function (op) {
        var $this = $(this);
        $.extend(DF, op);
        if (DF.data != null) {
            getTemplateTag({ TagList: DF.TagList, TagListSpan: DF.TagListSpan, val: DF.data });
            getTagList({ list: DF.TagList, TagListSpan: DF.TagListSpan, HiddenName: DF.HiddenName });
            TagListClick(TagList, DF.TagListSpan, DF.HiddenName);
        };
        $($this).find('[data-tag-button="' + DF.TagButton + '"]').off('click').on('click', function (e) {
            $.extend(DF, op);
            var TagTxt = $this.find('[data-tag-txt="' + DF.TagTxt + '"]');
            if ($(TagTxt).val() != '') {
                getTemplateTag({ TagList: DF.TagList, TagListSpan: DF.TagListSpan, val: $(TagTxt).val() });
                //$(DF.TagList).append('<div class="tag tag-success" style="display:none"><span>' + $(TagTxt).val() + '</span><i class="fa fa-times"></i></div>');
                //$(DF.TagList).find(DF.TagListSpan).fadeIn('fast');
                $(TagTxt).val('');
            }
            //
            getTagList({ list: DF.TagList, TagListSpan: DF.TagListSpan, HiddenName: DF.HiddenName });
            //
            TagListClick(TagList, DF.TagListSpan, DF.HiddenName);
        });
        function TagListClick(TagList, TagListSpan, HiddenName) {
            $(TagList).on('click', TagListSpan + ' i.fa-times', function () {
                $(this).parent().fadeOut('fast', function () {
                    $(this).remove();
                    //var hidden = $(TagList).find('[name="' + HiddenName + '"]');
                    //hidden.val(getTagList({ list: TagList, TagListSpan: DF.TagListSpan }));
                    getTagList({ list: TagList, TagListSpan: DF.TagListSpan, HiddenName: HiddenName });
                })
            });
        }
        function getTagList(obj) {
            var tag = $(obj.list).find(obj.TagListSpan);
            var val = '';
            for (var i = 0; i < tag.length; i++) {
                val += $(tag[i]).children('span').html() + ',';
            }
            var hidden = $(obj.list).find('[name="' + obj.HiddenName + '"]');
            if (hidden.length > 0)
                hidden.val(val.trim(','));
            else
                $(obj.list).append('<input type="hidden" value="' + $(obj.list).find(obj.TagListSpan + ' span').html() + '" name="' + obj.HiddenName + '" />')
        }
        function getTemplateTag(obj) {
            var html = '';
            if (typeof obj.val == 'object')
                for (var i = 0; i < obj.val.length; i++)
                    html += '<div class="tag tag-success" style="display:none"><span>' + obj.val[i] + '</span><i class="fa fa-times"></i></div>';
            else
                html = '<div class="tag tag-success" style="display:none"><span>' + obj.val + '</span><i class="fa fa-times"></i></div>';
            //$(obj.TagList).append('<div class="tag tag-success" style="display:none"><span>' + obj.val + '</span><i class="fa fa-times"></i></div>');
            $(obj.TagList).append(html);
            $(obj.TagList).find(obj.TagListSpan).fadeIn('fast');
        }
    };
}(jQuery);