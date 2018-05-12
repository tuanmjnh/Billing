+function ($) {
    "use strict";
    $.fn.TMCheckBox = function (all, item, btn) {
        var countItem = $(item).length;
        var count = 0;
        $(document).find(btn).hide();
        //Check all
        $(document).on('change', all, function () {
            countItem = $(item).length;
            if ($(this).prop('checked') === true) {
                $(item).prop('checked', true).attr('checked', true).parent().addClass('checked');
                $(all).prop('checked', true).attr('checked', true).parent().addClass('checked');
                count = countItem;
                if (countItem > 0)
                    $(btn).show();
            } else {
                $(item).prop('checked', false).removeAttr('checked', false).parent().removeClass('checked');
                $(all).prop('checked', false).removeAttr('checked', false).parent().removeClass('checked');
                count = 0;
                $(btn).hide();
            }
        });
        //Check item
        $(document).on('click', item, function () {
            countItem = $(item).length;
            if ($(this).prop('checked') === true) {
                count += 1;
                $(btn).show();
            } else
                count -= 1;
            if (count === countItem)
                $(all).prop('checked', true).attr('checked', true).parent().addClass('checked');
            else
                $(all).prop('checked', false).removeAttr('checked', false).parent().removeClass('checked');
            if (count === 0)
                $(btn).hide();
        });
        //        $(btn).click(function () {
        //            $(all).prop('checked', false)
        //        })
    };
}(jQuery);
+function ($) {
    "use strict";
    $.fn.readURL = function (img) {
        return $(this).each(function () {
            $(this).change(function () {
                var reader = new FileReader();
                reader.readAsDataURL(this.files[0]);
                reader.onload = function (e) {
                    $(img).html('<img src="' + e.target.result + '" class="' + $(img).attr('data-class') + '" />');
                };
            });
        });
    };
}(jQuery);
+function ($) {
    "use strict";
    $.fn.checkedSelect = function (checkedSelect) {
        $(checkedSelect + ' input').click(function () {
            $(checkedSelect + ' input').each(function () {
                $(this).removeClass('disabled');
            });
            $(this).addClass('disabled');
        });
    };
}(jQuery);
+function ($) {
    "use strict";
    var DF = {
        pointer: 'true',
        isWidth: 'true',
        type: 'hover', //click, cycle
        inputWith: '143',
        bgCorlor: '#FFF',
        border: 'ccc',
        subFloat: 'right',
        subWith: '26',
        subHeight: '26',
        subPadding: '6',
        ICEye: 'img/eyes.png',
        ICEmpty: 'glyphicon glyphicon-remove'
    }
    //isThis
    function setIsThisCss(isThis) {
        isThis.css({ 'border': '1px solid #' + (DF.border != '' ? DF.border : 'ccc'), 'display': 'inline-flex', 'background-color': DF.bgCorlor != '' ? DF.bgCorlor : '#FFF' })
    }
    //setInputCss
    function setInputCss(input) {
        if (input != undefined)
            input.css({ 'border': 'none', 'height': DF.subHeight })
    }
    //Sub
    function setSubCss(cdt, sub, ic) {
        sub.css({
            'width': DF.subWith != '' ? DF.subWith : '26', 'border-left': '1px solid #' + (DF.border != '' ? DF.border : 'ccc'),
            //'float': DF.subFloat != '' ? DF.subFloat : 'right',
            //'height': DF.subHeight != '' ? DF.subHeight : '26',
            //'padding': DF.subPadding != '' ? DF.subPadding : '6'
        })
        //value DF.pointer
        if (cdt == 'false')
            sub.css('cursor', 'default')
        else
            sub.css('cursor', 'pointer')
    }
    function setIsWidth(isThis, input) {
        //Set is with
        if (DF.isWidth == 'true') {
            isThis.css('width', DF.inputWith);
            input.css('width', DF.inputWith)
        } else {
            var width = isThis.css('width');
            isThis.css('width', width);
            input.css('width', width)
        }
    }
    function setChange(input, sub) {
        if (input.val() != undefined && sub.val() != undefined) {
            if (input.val().length < 1)
                sub.hide()
            else
                sub.show()
            //
            input.keyup(function () {
                if (input.val().length > 0)
                    sub.show()
                else
                    sub.hide()
            })
        } else
            $(this).hide()
    }
    //eyes pass
    $.fn.eyesPass = function (op) {
        $.extend(DF, op)
        return $(this).each(function () {
            var isThis = $(this)
            var input = isThis.children('input')
            var eyes = $(this).find('.eyes')
            setIsWidth(isThis, input)                   //Set width
            setIsThisCss(isThis)                        //Function set css isThis
            setInputCss(input)                          //Function set css isInput
            setSubCss(DF.pointer, eyes, DF.ICEye)       //Function set css isSub
            setChange(input, eyes)                      //Function set change
            //Function set change
            setChange(input, eyes)
            //Action
            if (DF.type == 'cycle')
                eyes.mousedown(function () {
                    input.removeAttr('type')
                })
                        .mouseup(function () {
                            input.attr('type', 'password')
                        })
                        .blur(function () {
                            input.attr('type', 'password')
                        })
                        .mouseleave(function () {
                            input.attr('type', 'password')
                        })
            else if (DF.type == 'click')
                eyes.mousedown(function () {
                    if (input.attr('type') == 'password')
                        input.removeAttr('type')
                    else
                        input.attr('type', 'password')
                })
            else
                eyes.mouseover(function () {
                    if (input.attr('type') == 'password')
                        input.removeAttr('type')
                    else
                        input.attr('type', 'password')
                })
                        .mouseleave(function () {
                            input.attr('type', 'password')
                        })
        })
    };
    //Empty
    $.fn.emptyText = function (op) {
        DF.type = 'click'
        $.extend(DF, op)
        return $(this).each(function () {
            var isThis = $(this)
            var input = isThis.children('input')
            var empty = $(this).find('.empty')
            setIsWidth(isThis, input)                   //Set width
            setIsThisCss(isThis)                        //Function set css isThis
            setInputCss(input)                          //Function set css isInput
            setSubCss(DF.pointer, empty, DF.ICEmpty)    //Function set css isSub
            setChange(input, empty)                     //Function set change
            //Action
            if (DF.type == 'click')
                empty.mousedown(function () {
                    input.val('');
                    empty.hide()
                })
            else
                empty.mouseover(function () {
                    input.val('');
                    empty.hide()
                })
        })
    }
}(jQuery);
+function ($) {
    "use strict";
    $.fn.ValueNull = function (val) {
        return $(this).each(function () {
            $(this).keyup(function () {
                if ($(this).val() == '' || $(this).val() == 'Null')
                    $(this).val(val);
            })
        })
    }
    $.fn.ValueNull1 = function () {
        return $(this).each(function () {
            if ($(this).val() == 'Null')
                $(this).val('')
        })
    }
    $.fn.ValueNull2 = function (val) {
        return $(this).each(function () {
            if ($(this).val() == 'Null')
                $(this).val(val)
            $(this).keyup(function () {
                if ($(this).val() == '' || $(this).val() == 'Null')
                    $(this).val(val);
            }).blur(function () {
                if ($(this).val() == '' || $(this).val() == 'Null')
                    $(this).val(val);
            }).mouseout(function () {
                if ($(this).val() == '' || $(this).val() == 'Null')
                    $(this).val(val);
            })
            $(this).mousedown(function () {
                if ($(this).val() == val)
                    $(this).val('')
            }).mouseover(function () {
                if ($(this).val() == val)
                    $(this).val('')
            }).focus(function () {
                if ($(this).val() == val)
                    $(this).val('')
            })
        })
    }
    $.fn.ReturnValueFocus = function (input) {
        return $(this).each(function () {
            $(input).val($(this).val());
            $(this).change(function () {
                $(input).val($(this).val());
                $(input).attr('value', $(this).val());
            })
        })
    }
    $.fn.replaceArray = function (str, reg, val) {
        for (var i = 0; i < str.length; i++) {
            for (var j = 0; j < reg.length; j++) {
                str = str[i] == reg[j] ? str.replace(str[i], val) : str;
            }
        }
        return str;
    }
    function TMAscii(isThis) {
        var s = $(isThis).val().toLowerCase();
        s = $.fn.replaceArray(s, '[]', 'a');
        s = $.fn.replaceArray(s, 'áàãạảâầấậẫẩăằắẵặẳ', 'a');
        s = $.fn.replaceArray(s, 'èéẹẽẻêếềễểệ', 'e');
        s = $.fn.replaceArray(s, 'ìíịỉĩ', 'i');
        s = $.fn.replaceArray(s, 'òóõọỏôỗộồốổơỡờớợỡở', 'o');
        s = $.fn.replaceArray(s, 'ùúụũủưừứựữử', 'u');
        s = $.fn.replaceArray(s, 'ýỳỹỷỵ', 'y');
        s = $.fn.replaceArray(s, 'đ', 'd');
        s = $.fn.replaceArray(s, '~`!@#$%^&*()-_+={}\\|;:\'"<,>.?/”“‘’„‰‾–—', '');
        //s = $.fn.replaceArray(s, '[ ]', ' ');
        s = $.fn.replaceArray(s, ' ', '-');
        return s;
    }
    $.fn.ReturnAsciiFocus = function (input) {
        return $(this).each(function () {
            var ascii = TMAscii($(this));
            $(input).val(ascii);
            $(input).attr('value', ascii);
            $(this).change(function () {
                ascii = TMAscii($(this));
                $(input).val(ascii);
                $(input).attr('value', ascii);
            })
        })
    }
}(jQuery);
//+ function ($) {
//    "use strict";
function ConvertKeysToLowerCase(obj) {
    var json = JSON.stringify(obj);
    var newJson = json.replace(/"([\w]+)":/g, function ($0, $1) {
        return ('"' + $1.toLowerCase() + '":');
    });
    var newObj = JSON.parse(newJson);
    return newObj;
};
function isJson(str) {
    try {
        $.parseJSON(str);
    } catch (e) {
        return false;
    }
    return true;
}
function isUndefined(val, char) {
    if (val === undefined || val === '' || val === null)
        if (char === undefined)
            val = '';
        else
            val = char;
    return val;
}
function ArrayToStr(val, char) {
    if (char === undefined)
        char = ',';
    var rs = '';
    for (var i = 0; i < val.length; i++)
        rs += val[i].value + char;
    return rs.substr(0, rs.length - 1);
}
String.prototype.trim = function (char) {
    var str = String(this);
    return str.replace(new RegExp('^' + char + '|' + char + '$', 'g'), '');
}
String.prototype.ReplaceAt = function (char, index, leap) {
    leap = leap != null ? leap : 1;
    var str = String(this);
    if (index != -1)
        str = str.substr(0, index) + char + str.substr(index + leap, str.length);
    return str;
}
String.prototype.StringFormat = function (key) {
    var str = String(this);
    if (str.indexOf('%s') != -1) {
        if ($.isArray(key))
            for (var k in key)
                str = str.ReplaceAt(key[k], str.indexOf('%s'), 2);
        else
            str = str.replace('%s', key);
    } else {
        if ($.isArray(key))
            for (var k in key)
                str = str.replace('{' + k + '}', key[k]);
        else
            str = str.replace('{0}', key);
    }
    return str;
}
String.prototype.htmlDecode = function () {
    var str = String(this);
    var e = document.createElement('div');
    e.innerHTML = str;
    return e.childNodes.length === 0 ? "" : e.childNodes[0].nodeValue;
}
var sprintf = function (str) {
    var args = arguments,
        flag = true,
        i = 1;

    str = str.replace(/%s/g, function () {
        var arg = args[i++];

        if (typeof arg === 'undefined') {
            flag = false;
            return '';
        }
        return arg;
    });
    return flag ? str : '';
};
String.prototype.RemoveHTMLTag = function () {
    //var str = ;
    return String(this).replace(/(<([^>]+)>)/ig, "");
}

var QueryString = (function (a) {
    if (a == "")
        return {};
    var b = {};
    for (var i = 0; i < a.length; ++i) {
        var p = a[i].split('=', 2);
        if (p.length == 1)
            b[p[0]] = "";
        else
            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
    }
    return b;
})(window.location.search.substr(1).split('&'));
var CurrentUrlFull = (function (a) {
    return a;
})(window.location.href);
var CurrentUrl = (function (a) {
    return a;
})(window.location.href.replace(window.location.search, ''));
var Host = (function (a) {
    return a;
})(window.location.protocol + '//' + window.location.host);
var Segment = (function (a) {
    return a;
})(window.location.pathname.substr(1).split('/'));
var Areas = (function (a) {
    return a;
})(Segment[0]);
var AreasUrl = (function (a) {
    return a;
})(Host + '/' + Areas);
var serializeArrayToObject = (function (array) {
    var data = {};
    $(array).each(function (index, obj) {
        data[obj.name] = obj.value;
    });
    return data;
});
Number.prototype.format = function (n, x) {
    try {
        var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
        return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
    } catch (e) {
        console.log(e);
    }
    
};
//}(jQuery)
//+function ($) {
//    var DF = {
//        hidden: true,
//        remove: true,
//        css: '',
//        alert: 'This is TMAlert',
//        close: true
//    }
//    $.fn.TMAlert = function (op) {
//        return $(this).each(function () {
//            $.extend(DF, op);
//            var isThis = $(this);
//            isThis.fadeIn('fast', function () {
//                var alert = '<div class="alert alert-success alert-dismissible ' + DF.css + '" role="alert">';
//                if (DF.close)
//                    alert += '<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>';
//                alert += DF.alert + '</div>';
//                isThis.html(alert);
//            });
//            if (DF.hidden)
//                setTimeout(function () {
//                    isThis.fadeOut("slow", function () {
//                        if (DF.remove)
//                            isThis.html('');
//                    })
//                }, 5000);
//        });
//    };
//}(jQuery);
+function ($) {
    "use strict";
    var DF = {
        type: 'success',
        message: 'TMAlert v1.0',
        timeout: 8000,
        remove: '.message-alert',
        fixed: true,
    }
    $.fn.TMAlert = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var $this = $(this);
            var class_type = 'alert-success';
            if (DF.type == 'danger' || DF.type == 2)
                class_type = "alert-danger";
            else if (DF.type == 'warning' || DF.type == 3)
                class_type = 'alert-warning';
            else if (DF.type == 'info' || DF.type == 4)
                class_type = 'alert-info';
            else
                class_type = 'alert-success';
            //
            $(DF.remove).remove();
            //fixed
            if (DF.fixed) {
                $this.css({ 'position': 'fixed', 'z-index': '1051', 'top': '3px', 'right': '20px' })//,'height':'48px'
            }
            //
            var html = set_alert(class_type, DF.message);
            $this.append(html);
            if (parseInt(DF.timeout) > 0)
                html.fadeIn('fast').delay(DF.timeout).fadeOut('slow', function () {
                    $(this).remove()
                });
            else
                html.fadeIn('fast');
        });
    }
    $.fn.TMAlertRemove = function (op) {
        return $(this).each(function () {
            var isThis = $(this);
            isThis.remove();
        });
    }
    function set_alert(class_type, message) {
        return $('<div style="display:none;margin-bottom:5px" class="alert ' + class_type + ' alert-dismissible">' +
            '<button type="button" class="close" data-dismiss="alert"><span aria-hidden="true">×</span></button>' + message + '</div>');
    }
}(jQuery);
+function ($) {
    "use strict";
    var DF = {
        title: "",
        content: "",
        location: "",
        icon: "",
        url: "",
        data: "",
        interval: 5000
    }
    document.addEventListener('DOMContentLoaded', function () {
        if (!Notification) {
            alert('Vui lòng bật thông báo trên Chrome!');
            return;
        }
        if (Notification.permission !== "granted")
            Notification.requestPermission();
    });
    function Push(title, content, location, icon) {
        if (!Notification) { return; }
        if (Notification.permission !== "granted") { return; }
        var notification = new Notification(title, {
            icon: icon,
            body: content,
        });
        notification.onclick = function () {
            window.open(location);
        };
    }
    $.fn.PushNotification = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var isThis = $(this);
            Push(DF.title, DF.content, DF.location, DF.icon);
        });
    }
    $.fn.Notification = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var isThis = $(this);
            $(document).ready(function () {
                $.post(DF.url, DF.data).done(function (d) {
                    if (d.title)
                        Push(d.title, d.content, d.location, DF.icon);
                });
            });
        });
    }
    $.fn.NotificationInterval = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var isThis = $(this);
            setInterval(function () {
                $(document).ready(function () {
                    $.post(DF.url, DF.data).done(function (d) {
                        if (d.title)
                            Push(d.title, d.content, d.location, DF.icon);
                    });
                });
            }, DF.interval);
        });
    }
}(jQuery);
+function ($) {
    var DF = {
        remove: null,
        fixList: [{ name: null, parent: null }],
        iconSuccess: true,
        iconError: true
    };
    $.fn.ValidateSuccess = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var p = $(this).parent();
            if (!$.isArray(DF.fixList)) {
                if (DF.fixList.name != null && DF.fixList.parent != null)
                    if ($(this).attr('name') == DF.fixList.name)
                        p = $(document).find(DF.fixList.parent);
            } else {
                for (var i = 0; i < DF.fixList.length; i++) {
                    if (DF.fixList[i].name != null && DF.fixList[i].parent != null)
                        if ($(this).attr('name') == DF.fixList[i].name)
                            p = $(document).find(DF.fixList[i].parent);
                }
            }
            $(this).hide();
            p.find('.has-error').removeClass('has-error');
            p.removeClass('has-error').addClass('has-feedback has-success');
            p.find('.form-control-feedback').remove();
            if (DF.iconSuccess)
                p.prepend('<span class="glyphicon glyphicon-ok form-control-feedback" aria-hidden="true"></span>');
            if (DF.remove != null)
                $(document).find(DF.remove).parent().remove();
        });
    };
    $.fn.ValidateError = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var p = $(this).parent();
            if (!$.isArray(DF.fixList)) {
                if (DF.fixList.name != null && DF.fixList.parent != null)
                    if ($(this).attr('name') == DF.fixList.name)
                        p = $(document).find(DF.fixList.parent);
            } else {
                for (var i = 0; i < DF.fixList.length; i++) {
                    if (DF.fixList[i].name != null && DF.fixList[i].parent != null)
                        if ($(this).attr('name') == DF.fixList[i].name)
                            p = $(document).find(DF.fixList[i].parent);
                }
            }
            p.removeClass('has-success').addClass('has-feedback has-error');
            p.find('.form-control-feedback').remove();
            if (DF.iconError)
                p.prepend('<span class="glyphicon glyphicon-remove form-control-feedback" aria-hidden="true"></span>');
        });
    };
}(jQuery);
//if (typeof $.validator.addMethod === "function") {
//    $.validator.addMethod("alpha_numeric", function (value, element) {
//        return this.optional(element) || /^[a-z0-9]+$/i.test(value);
//    }, "Username must contain only letters, numbers, or dashes.");
//}
+function ($) {
    "use strict";
    var DF = {
        url: window.location.href,
        data: null,
        type: 'POST',
        processData: false,
        contentType: false, //application/x-www-form-urlencoded  //'multipart/form-data'
        reset: true,
        alHidden: true,
        alRemove: true,
        alCSS: '',
        alClose: true,
        async: true,
        success: function () {
        },
        error: function () {
        }
    };
    $.fn.TMAjaxPost = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var isThis = $(this);
            if (DF.data === null) {
                DF.data = new FormData(isThis[0]);
                DF.data.append('csrf_token_name', $.cookie('csrf_cookie_name'));
            } else
                $.extend(DF.data, { 'csrf_token_name': $.cookie('csrf_cookie_name') });
            $.ajax({
                url: DF.url,
                data: DF.data,
                type: DF.type,
                processData: DF.processData,
                contentType: DF.contentType,
                async: DF.async,
                success: function (d) {
                    if (DF.reset)
                        isThis.trigger('reset');
                    if (isJson(d))
                        isThis.find('.TMAlert').TMAlert({
                            hidden: DF.alHidden, remove: DF.alRemove,
                            css: DF.alCSS, close: DF.alClose, alert: $.parseJSON(d)
                        });
                    else if (d == 'reload')
                        location.reload();
                    else
                        window.location = d;
                    isThis.find('.form-control-feedback').remove();
                    isThis.find('.has-success').removeClass('has-success');
                    DF.data = null;
                    DF.success(d);
                },
                error: function (xhr, rrr, error) {
                    DF.error();
                    console.log(error);
                }
            });
        });
    };
}(jQuery);
+function ($) {
    "use strict";
    var DF = {
        url: window.location.href,
        data: null,
        reset: true,
        alHidden: true,
        alRemove: true,
        alCSS: '',
        alClose: true,
        event: null,
        element: null,
        success: function (event, element) { },
        error: function (event, element) { }
    };
    $.fn.TMConfirmAjaxPost = function (op) {
        var isThis = $(this);
        $(document).ajaxComplete(function () {
            $(document).find(isThis.selector).confirmation({
                onConfirm: function (e, a) {
                    $.extend(DF, op);
                    if (DF.data === null)
                        DF.data = { id: $(a).attr('value') };
                    $.extend(DF.data, { csrf_token_name: $.cookie('csrf_cookie_name') });
                    $.post(DF.url, DF.data, function (d) {
                        DF.data = null;
                        if (isJson(d))
                            $(document).find('.TMAlert').TMAlert({
                                hidden: DF.alHidden, remove: DF.alRemove,
                                css: DF.alCSS, close: DF.alClose, alert: $.parseJSON(d)
                            });
                        else if (d == 'reload')
                            location.reload();
                        else
                            window.location = d;
                        DF.success(e, a);
                    });
                }
            });
        });
    };
    $.fn.TMSingleAjaxPost = function (op) {
        //return $(this).each(function () {
        var isThis = $(this);
        $.extend(DF, op);
        $(document).on('click', isThis.selector, function (e) {
            if (DF.data === null)
                DF.data = { id: $(isThis).attr('value') };
            $.extend(DF.data, { csrf_token_name: $.cookie('csrf_cookie_name') });
            if (DF.event != null)
                DF.event.preventDefault();
            $.post(DF.url, DF.data, function (d) {
                DF.data = null;
                if (isJson(d))
                    $(document).find('.TMAlert').TMAlert({
                        hidden: DF.alHidden, remove: DF.alRemove,
                        css: DF.alCSS, close: DF.alClose, alert: $.parseJSON(d)
                    });
                else if (d == 'reload')
                    location.reload();
                else
                    window.location = d;
                DF.success(d);
                //$(document).off('click');
                e.preventDefault();
                $(document).unbind(e);
            });
            return this;
        });
        //});
    };
}(jQuery);
+function ($) {
    "use strict";
    var DF = {
        url: window.location.href,
        data: null,
        type: 'POST',
        processData: false,
        contentType: false,
        autotTarget: false,
        success: function () {
        },
        error: function () {
        },
        cancel_func: function () {
        },
        close_func: function () {
        },
        always_func: function () {
        }
    }
    function getModal(id) {
        return '<div id="' + id + '" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true"></div>';
    }
    $.fn.TMCallModal = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var isThis = $(this);
            if ($(document).find('#general-model').length < 1)
                $(document).find('body').append(getModal('general-model'));
            if ($(document).find(isThis.attr('data-target')).length < 1)
                $(document).find('body').append(getModal(isThis.attr('data-target').replace('#', '')));
            isThis.on('click', function () {
                DF.data = { csrf_token_name: $.cookie('csrf_cookie_name') };
                if (isThis.attr('value') !== undefined)
                    $.extend(DF.data, { id: isThis.attr('value') });
                $.post(isThis.attr('data-url'), DF.data, function (json) {
                    $(isThis.attr('data-target')).html(json);
                }).done(function () {
                    cancel_modal('cancel-model');
                    cancel_modal('close-modal');
                    DF.success();
                    DF.cancel_func();
                    DF.close_func();
                }).fail(function () {
                    DF.error();
                }).always(function () {
                    DF.always_func();
                });
            });
        });
    };
    $.fn.TMCallModalEdit = function (url) {
        var isThis = $(this);
        $(document).ajaxComplete(function () {
            $(document).find(isThis.selector)
                    .attr('data-toggle', 'modal')
                    .attr('data-target', '#general-model')
                    .attr('data-url', url)
                    .attr('href', '#').TMCallModal({ autotTarget: false });
        });
        //$(document).find('body').append(getModal('details_modal'));
    };
    function cancel_modal(id) {
        $(document).on('click', '#' + id, function () {
            $(document).find('.modal').html('');
        })
    }
}(jQuery);
+function ($) {
    "use strict";
    $.fn.TMImageCheck = function () {
        return $(this).each(function () {
            var i = $(this).find('img');
            if (i.attr('src') == '')
                i.attr('alt', '').attr('title', '');
            else
                $(this).css('background', 'none');
        });
    };
}(jQuery);
+function ($) {
    "use strict";
    var DF = {
        data: null,
        key: { title: 'Parent', icon: 'fa fa-connectdevelop', level: '0' },
        hidden: { id: null, sId: ',', level: '-1' },
        dHidden: { id: null, sId: ',', level: '-1' },
        selected: { id: 'id', value: null, index: null },
        noParent: false,
        caret: ' <span class="caret"></span>',
        classCss: ['custom-drop', 'dropdown-menu-none'],
        fncSelected: function (event, target) { }
    };
    $.fn.SelectDropdownParent = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            var isThis = $(this).children();
            $(isThis).find('ul li a').on('click', function (e) {
                console.log(isThis)
                e.preventDefault();
                isThis.children('a').html($(this).html() + DF.caret);
                setAttrHidden($(this));
                DF.fncSelected(e, this);
            });
        });
    };
    $.fn.DropdownParent = function (op) {
        return $(this).each(function (e) {
            var isThis = $(this);
            var t = DF.key[Object.keys(DF.key)[0]];
            var i = DF.key[Object.keys(DF.key)[1]];
            $.extend(DF, op);
            t = DF.key[Object.keys(DF.key)[0]] == null ? t : DF.key[Object.keys(DF.key)[0]];
            i = DF.key[Object.keys(DF.key)[1]] == null ? i : DF.key[Object.keys(DF.key)[1]];
            var rs = '<div class="dropdown ' + DF.classCss[0] + '">';
            rs += '<ul class="dropdown-menu ' + DF.classCss[1] + '">';
            $.each(DF.data, function (k, v) {
                var leap = '';
                for (var i = 0; i < parseInt(v[Object.keys(DF.key)[2]]) ; i++)
                    leap += '-';
                rs += '<li class="dropdown-level' + v[Object.keys(DF.key)[2]] +
                        '"><a href="#" ' + getAttrKey(DF.dHidden, v) + '> ' + leap + ' <i class="' +
                        v[Object.keys(DF.key)[1]] + '"></i> ' + v[Object.keys(DF.key)[0]] + '</a></li>';
                rs += '<li class="divider"></li>';
            });
            if (!DF.noParent) {
                rs += '<li><a href="#" ' + getAttrHidden(DF.hidden) + '><i class="' + i + '"></i> ' + t + '</a></li>';
                rs += '<li class="divider"></li>';
            }
            rs += '</ul></div>';
            isThis.html(rs);
            getHidden(isThis.children());
            getSelected(isThis.children());
            isThis.SelectDropdownParent(DF);
        });
    };
    function getHidden(obj) {
        for (var k in DF.hidden) {
            obj.children('a').attr(k, DF.hidden[k]);
            obj.after('<input type="hidden" id="' + k + '" name="' + k + '" value="' + DF.hidden[k] + '">');
        }
    }
    function getSelected(obj) {
        if (DF.selected.index == null) {
            var lastItem = $(obj).find('a').last();
            var title = lastItem.html().RemoveHTMLTag();
            var icon = lastItem.find('i').attr('class');
            //var title = DF.key[Object.keys(DF.key)[0]];
            //var icon = DF.key[Object.keys(DF.key)[1]];
            if (DF.selected.value !== null) {
                $(obj).find('a').each(function () {
                    if ($(this).attr(DF.selected.id) === DF.selected.value) {
                        title = $(this).text();
                        icon = $(this).children('i').attr('class');
                        setAttrHidden($(this));
                    }
                });
            }

        } else {
            var index_selected = $(obj).children('ul').find('a')[DF.selected.index];
            if (index_selected === undefined)
                index_selected = $(obj).children('ul').find('a')[0];
            title = $(index_selected).text();
            icon = $(index_selected).children('i').attr('class');
            setAttrHidden($(index_selected));
        }
        var a = $('<a href="#" class="btn btn-default dropdown-toggle" data-toggle="dropdown"></a>');
        var i = '<i class="' + icon + '"></i> ';
        a.html(i + title + DF.caret);
        obj.prepend(a.prop('outerHTML'));
    }
    function getAttrHidden(obj) {
        var rs = '';
        for (var k in obj) {
            rs += k + '="' + obj[k] + '" ';
        }
        return rs;
    }
    function setAttrHidden(obj) {
        for (var k in DF.hidden) {
            $('#' + k).val(obj.attr(k));
        }
    }
    function getAttrKey(hidden, obj) {
        var rs = '';
        for (var k in hidden) {
            var arr = hidden[k].split('|');
            if (arr.length < 2)
                rs += k + '="' + obj[hidden[k]] + '" ';
            else {
                var tmp = '';
                for (var i = 0; i < arr.length; i++) {
                    tmp += obj[arr[i]] + ',';
                }
                rs += k + '="' + tmp.replace(',,', ',') + '"';
            }
        }
        return rs;
    }
}(jQuery);
//+function ($) {
//    "use strict";
//    $(document).ajaxStart(function () {
//        $('#ajaxLoadTM').html('<img src="' + $('#ajaxLoadTM').attr('url') + 'assets/loading/load1.gif"/>');
//    });
//    $(document).ajaxComplete(function () {
//        $('#ajaxLoadTM').html('');
//    });
//}(jQuery);

//Ajax Loadding
var backdropTag = '<div class="modal-backdrop fade show"></div>';
function AjaxLoaddingSpinner(backdrop) {
    var tag = 'css3Loader';
    $('body').append('<div style="display:none" class="' + tag + '"></div>')
    $(document).ajaxStart(function () {
        if (backdrop)
            $('body').append(backdropTag);
        $('.' + tag).fadeIn('fast');
    });
    $(document).ajaxComplete(function () {
        if (backdrop)
            $('.' + tag).fadeOut('slow', function () {
                $('body').find('.modal-backdrop').remove();
            });
        else
            $('.' + tag).fadeOut('fast');
    });
};
function AjaxLoaddingBounce(backdrop) {
    var tag = 'ajax-loader';
    $('body').append(
        '<div class="' + tag + '" style="display:none">' +
        '<div class="spinner"><div class="bounce1"></div><div class="bounce2"></div><div class="bounce3"></div></div>' +
        '<div class="modal-backdrop fade show"></div></div>')
    $(document).ajaxStart(function () {
        $('.' + tag).fadeIn('fast');
    });
    $(document).ajaxComplete(function () {
        $('.' + tag).fadeOut('fast');
    });
};
+function ($) {
    $.fn.GetExtension = function () {
        return $(this).selector.substring($(this).selector.lastIndexOf('.') + 1).toLowerCase();
    };
    $.fn.CheckExtension = function (ext) {
        if ($.isArray(ext)) {
            if ($.inArray($(this).GetExtension(), ext) === -1)
                return false;
            else
                return true;
        } else {
            var tmp = ext.split('|');
            if ($.inArray($(this).GetExtension(), tmp) === -1)
                return false;
            else
                return true;
        }
    };
    var DF = {
        ext: 'gif|jpg|jpeg|png',
        messages: 'File input error!',
        imgClass: 'img50',
        imgLoad: ''
    };
    $.fn.ValidateFile = function (op) {
        return $(this).each(function () {
            $.extend(DF, op);
            $(this).parent().find('.imgCheck').addClass(DF.imgClass);
            $(this).change(function () {
                var isThis = $(this);
                isThis.parent().find('.glyphicon').remove();
                isThis.parent().find('.imgCheck').remove();
                if (!$($(this).val()).CheckExtension(DF.ext)) {
                    isThis.parent()
                            .removeClass('has-success')
                            .addClass('has-feedback has-error')
                            .find('#' + $(this).context.id + '-error').remove();
                    isThis.after('<span class="glyphicon glyphicon-remove form-control-feedback" aria-hidden="true"></span>');
                    isThis.after('<label id="' + $(this).context.id + '-error" class="file-error" for="' +
                            $(this).context.id + '">' + DF.messages + '</label>');
                    return false;
                } else {
                    isThis.parent()
                            .removeClass('has-error')
                            .addClass('has-feedback has-success')
                            .find('#' + $(this).context.id + '-error').remove();
                    isThis.after('<span class="glyphicon glyphicon-ok form-control-feedback" aria-hidden="true"></span>');
                    //Show images
                    var img = $('<div class="' + DF.imgClass + ' imgCheck"></div>');
                    isThis.parent().prepend(img);
                    var reader = new FileReader();
                    reader.readAsDataURL(this.files[0]);
                    reader.onload = function (e) {
                        //isThis.parent().prepend('<div class="' + DF.imgClass + ' imgCheck"><img src="' + e.target.result + '" class="img-tmp" /></div>');
                        $(img).html('<img src="' + e.target.result + '" class="img-tmp" />');
                    };
                    reader.onprogress = function (data) {
                        $(img).html('<img src="' + DF.imgLoad + '" class="img-tmp" />');
                        console.log(DF.imgLoad);
                    };
                    isThis.parent().find('.imgCheck').TMImageCheck();
                    return true;
                }
                //                var reader = new FileReader();
                //                reader.readAsDataURL(this.files[0]);
                //                reader.onload = function (e) {
                //                    console.log(e.target.result);
                //                };
            });
        });
    };
}(jQuery);

+function ($) {
    var event_body = false;
    var TMComfirm = function (element, options) {
        var that = this;
        //this.init('TMAjax', element, options);
        //$(document).ajaxComplete(function () {
        $(element).on('click.TM.TMAjax', function (e) {
            //var options = that.options;
            //this.options = $.extend({}, TMComfirm.DEFAULTS, options);
            this.options = $.extend(TMComfirm.DEFAULTS, options);
            //console.log(options);
            //var all = options.all_selector;
            if (!event_body) {
                event_body = $('body').on('click', function (e) {
                    //console.log(TMComfirm.DEFAULTS.url);
                    if (TMComfirm.DEFAULTS.data === null)
                        TMComfirm.DEFAULTS.data = { id: $(element).attr('value') };
                    if (TMComfirm.DEFAULTS.csrf)
                        $.extend(TMComfirm.DEFAULTS.data, { csrf_token_name: $.cookie('csrf_cookie_name') });
                    $.post(TMComfirm.DEFAULTS.url, TMComfirm.DEFAULTS.data, function (d) {
                        //console.log(TMComfirm.DEFAULTS.data);
                        TMComfirm.DEFAULTS.data = null;
                        TMComfirm.DEFAULTS.success(d);
                    });
                    $('body').unbind(e);
                    event_body = false;
                    return;
                });
            }
        });

        $(element).on('click', function (e) {
            e.preventDefault();
        });
        //});
    };
    TMComfirm.DEFAULTS = {
        url: window.location.href,
        data: null,
        csrf: true,
        reset: true,
        alHidden: true,
        alRemove: true,
        alCSS: '',
        alClose: true,
        event: null,
        element: null,
        success: function (event, element) { },
        error: function (event, element) { }
    };
    TMComfirm.prototype.constructor = TMComfirm;
    TMComfirm.prototype.getDefaults = function () {
        return TMComfirm.DEFAULTS;
    };
    //    TMComfirm.prototype.ajax = function () {
    //        console.log(TMComfirm.DEFAULTS.url);
    //    };
    var old = $.fn.TMAjax;
    $.fn.TMAjax = function (option) {
        var that = this;
        return this.each(function () {
            var $this = $(this);
            var data = $this.data('TM.TMAjax');
            var options = $.extend({}, TMComfirm.DEFAULTS, $this.data(), typeof option == 'object' && option)
            options = options || {};
            options.all_selector = that.selector;
            if (!data && option == 'destroy')
                return;
            if (!data)
                $this.data('TM.TMAjax', (data = new TMComfirm(this, options)));
            if (typeof option == 'string')
                data[option]();
        });
    };
    $.fn.TMAjax.Constructor = TMComfirm
    $.fn.TMAjax.noConflict = function () {
        $.fn.TMAjax = old;
        return this;
    }
}(jQuery);

+function ($) {
    var event_body = false;
    var TMComfirm = function (element, options) {
        var that = this;
        $(element).on('click.TM.TMAjaxSingle', function (e) {
            this.options = $.extend(TMComfirm.DEFAULTS, options);
            if (!event_body) {
                event_body = $(element).on('click', function (e) {
                    //console.log(TMComfirm.DEFAULTS.url);
                    if (TMComfirm.DEFAULTS.data === null)
                        TMComfirm.DEFAULTS.data = { id: $(element).attr('value') };
                    if (TMComfirm.DEFAULTS.csrf)
                        $.extend(TMComfirm.DEFAULTS.data, { csrf_token_name: $.cookie('csrf_cookie_name') });
                    $.post(TMComfirm.DEFAULTS.url, TMComfirm.DEFAULTS.data, function (d) {
                        //console.log(TMComfirm.DEFAULTS.data);
                        TMComfirm.DEFAULTS.data = null;
                        TMComfirm.DEFAULTS.success(d);
                    });
                    $('body').unbind(e);
                    event_body = false;
                    return;
                });
            }
        });

        $(element).on('click', function (e) {
            e.preventDefault();
        });
        //});
    };
    TMComfirm.DEFAULTS = {
        url: window.location.href,
        data: null,
        csrf: true,
        reset: true,
        alHidden: true,
        alRemove: true,
        alCSS: '',
        alClose: true,
        event: null,
        element: null,
        success: function (event, element) { },
        error: function (event, element) { }
    };
    TMComfirm.prototype.constructor = TMComfirm;
    TMComfirm.prototype.getDefaults = function () {
        return TMComfirm.DEFAULTS;
    };
    //    TMComfirm.prototype.ajax = function () {
    //        console.log(TMComfirm.DEFAULTS.url);
    //    };
    var old = $.fn.TMAjaxSingle;
    $.fn.TMAjaxSingle = function (option) {
        var that = this;
        return this.each(function () {
            var $this = $(this);
            var data = $this.data('TM.TMAjaxSingle');
            var options = $.extend({}, TMComfirm.DEFAULTS, $this.data(), typeof option == 'object' && option)
            options = options || {};
            options.all_selector = that.selector;
            if (!data && option == 'destroy')
                return;
            if (!data)
                $this.data('TM.TMAjaxSingle', (data = new TMComfirm(this, options)));
            if (typeof option == 'string')
                data[option]();
        });
    };
    $.fn.TMAjaxSingle.Constructor = TMComfirm
    $.fn.TMAjaxSingle.noConflict = function () {
        $.fn.TMAjaxSingle = old;
        return this;
    }
}(jQuery);


+ function ($) {
    'use strict';
    var DF = {
        tooltip: true,
        place: 'right',
        parentInput: '',
    }
    $.fn.TMSupportInput = function (option) {
        return this.each(function () {
            var $this = $(this)
            var options = $.extend({}, DF, $this.data(), typeof option == 'object' && option)
            $this.find('.control-label').each(function () {
                var text = $(this).html();
                var input = $(this).parent().find('input[type=text]');
                var number = $(this).parent().find('input[type=number]');
                var password = $(this).parent().find('input[type=password]');
                var tel = $(this).parent().find('input[type=tel]');
                var email = $(this).parent().find('input[type=email]');
                var textarea = $(this).parent().find('textarea');

                input.attr('placeholder', text).attr('title', text).parent().addClass(options.parentInput);
                number.attr('placeholder', text).attr('title', text).parent().addClass(options.parentInput);
                password.attr('placeholder', text).attr('title', text).parent().addClass(options.parentInput);
                tel.attr('placeholder', text).attr('title', text).parent().addClass(options.parentInput);
                email.attr('placeholder', text).attr('title', text).parent().addClass(options.parentInput);
                textarea.attr('placeholder', text).attr('title', text).parent().addClass(options.parentInput);
                if (options.tooltip) {
                    input.attr('data-toggle', 'tooltip').attr('data-placement', options.place)
                    number.attr('data-toggle', 'tooltip').attr('data-placement', options.place)
                    password.attr('data-toggle', 'tooltip').attr('data-placement', options.place)
                    tel.attr('data-toggle', 'tooltip').attr('data-placement', options.place)
                    email.attr('data-toggle', 'tooltip').attr('data-placement', options.place)
                    textarea.attr('data-toggle', 'tooltip').attr('data-placement', options.place)
                }
            })
        })
    }

}(jQuery);

+function ($) {
    'use strict';
    var TMTest = function (element, options) {
        this.$element = $(element)
        this.options = options
        //console.log(element);
    };
    TMTest.DEFAULTS = {
        test: 'loading...',
        id: 1,
        value: 'tmtest'
    };
    TMTest.prototype.action = function (e) {
        console.log('action');
    };
    TMTest.prototype.show = function (e) {
        console.log('show');
    };
    TMTest.prototype.getDefault = function (e) {
        console.log(TMTest.DEFAULTS)
    };
    // ALERT PLUGIN DEFINITION
    // =======================
    var old = $.fn.tmtest;
    $.fn.tmtest = function (option) {
        return this.each(function () {
            var $this = $(this)
            var data = $this.data('tm.tmtest')
            var options = $.extend({}, TMTest.DEFAULTS, $this.data(), typeof option == 'object' && option)
            if (!data)
                $this.data('tm.tmtest', (data = new TMTest(this, options)))
            if (typeof option == 'string')
                data[option].call($this)
            console.log(options);
        })
    }
    $.fn.tmtest.Constructor = TMTest
    // ALERT NO CONFLICT
    // =================
    $.fn.tmtest.noConflict = function () {
        $.fn.tmtest = old
        return this
    }
    // TMTest DATA-API
    // ==============

    $(document)
            .on('action.tm.tmtest.data-api', '[data-tm="tmtest"]', TMTest.prototype.action)
            .on('show.tm.tmtest.data-api', '[data-tm="tmtest"]', TMTest.prototype.show)
}(jQuery);
////return $(this).each(function () {
//        $.extend(DF, op);
//        var isThis = $(element);
//        //$(document).ajaxComplete(function () {
//        if (!event_body) {
//            event_body = $(document).on('click', element, function (e) {
//                console.log($(this));
//                DF.element = $(this);
//                if (DF.data === null)
//                    DF.data = {id: $(this).attr('value')};
//                $.extend(DF.data, {csrf_token_name: $.cookie('csrf_cookie_name')});
//                $.post(DF.url, DF.data, function (d) {
//                    DF.data = null;
//                    if (isJson(d))
//                        $('.TMAlert').TMAlert({hidden: false, remove: false, css: 'customer-alert', alert: $.parseJSON(d)});
//                    else if (d == 'reload')
//                        location.reload();
//                    else
//                        window.location = d;
//                    DF.success();
//                    //$(document).unbind(e);
//                    event_body = false;
//                });
//                return;
//            });
//        }
//        //});
//        //});