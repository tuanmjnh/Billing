//Get Table Bootstrap
var $table = '#table',
    $urlSelect = $controllerUrl + $($table).attr('data-url-select'),
    $urlCreate = $controllerUrl + $($table).attr('data-url-create'),
    $urlEdit = $controllerUrl + $($table).attr('data-url-edit'),
    $remove = '#remove',
    selections = [],
    params_flag = 1,
    params_dateStart = null,
    params_dateEnd = null;
//Button
var $indexBtnAdd = '#indexBtnAdd',
    $indexBtnDeleteRecover = '#indexBtnDeleteRecover',
    $indexBtnDeleteForever = '#indexBtnDeleteForever',
    $indexBtnUse = '#indexBtnUse',
    $indexBtnTrash = '#indexBtnTrash',
    $dateStart = '#dateStart',
    $dateEnd = '#dateEnd';
//bootstrapTable
$(function () {
    //initTable();
    $($table).bootstrapTable({
        url: $urlSelect,
        locales: 'customs'
    });
    $($table).on('load-success.bs.table', function () {
        $('#table ' + $isTooltip).tooltip({ animation: false });
    });
    //
    $('#toolbar').find('select').change(function () {
        $($table).bootstrapTable('refresh').bootstrapTable({
            exportDataType: $(this).val()
        });
    });
    //Default Flag
    setFlagActive(params_flag);
})
//function initTable() {
//    $($table).bootstrapTable({
//        locales: 'customs',
//        columns: [
//            //{
//            //    field: 'state',
//            //    checkbox: true,
//            //    rowspan: 2,
//            //    align: 'center',
//            //    valign: 'middle'
//            //},
//            {
//                field: 'username',
//                title: 'username',
//                sortable: true,
//                editable: true,
//                valign: 'middle'
//            },
//            {
//                field: 'fullName',
//                title: 'fullName',
//                sortable: true,
//                valign: 'middle'
//            },
//            {
//                field: 'operate',
//                title: 'Item Operate',
//                events: operateEvents,
//                formatter: operateFormatter
//            }
//        ]
//    });
//    // sometimes footer render error.
//    setTimeout(function () {
//        $($table).bootstrapTable('resetView');
//    }, 200);
//    $($table).on('check.bs.table uncheck.bs.table ' +
//        'check-all.bs.table uncheck-all.bs.table', function () {
//            $($remove).prop('disabled', !$($table).bootstrapTable('getSelections').length);
//            // save your data, here just save the current page
//            selections = getIdSelections();
//            // push or splice the selections if you want to save all data selections
//        });
//    $($table).on('expand-row.bs.table', function (e, index, row, $detail) {
//        if (index % 2 == 1) {
//            $detail.html('Loading from ajax request...');
//            $.get('LICENSE', function (res) {
//                $detail.html(res.replace(/\n/g, '<br>'));
//            });
//        }
//    });
//    $($table).on('all.bs.table', function (e, name, args) {
//        console.log(name, args);
//    });
//    $($remove).click(function () {
//        var ids = getIdSelections();
//        $($table).bootstrapTable('remove', {
//            field: 'id',
//            values: ids
//        });
//        $($remove).prop('disabled', true);
//    });
//    $(window).resize(function () {
//        $($table).bootstrapTable('resetView', {
//            height: getHeight()
//        });
//    });
//};
//
function getIdSelections() {
    return $.map($($table).bootstrapTable('getSelections'), function (row) {
        return row.id
    });
}
function responseHandler(res) {
    $.each(res.rows, function (i, row) {
        row.state = $.inArray(row.id, selections) !== -1;
    });
    return res;
    //return res.rows;
}
function detailFormatter(index, row) {
    var html = [];
    $.each(row, function (key, value) {
        html.push('<p><b>' + key + ':</b> ' + value + '</p>');
    });
    return html.join('');
}
function dateTimeFormatter(value, row, index) {
    if (value !== null)
        return moment(value).format('DD/MM/YYYY');
    else
        return '';
}
function cmdFormatter(value, row, index) {
    return [
        '<a class="btnEdit isTooltip" href="javascript:void(0)" data-toggle="modal" data-target="modal" title="' + TMLanguage('Global.edit') + '">',
        '<i class="fa fa-pencil-square-o" aria-hidden="true"></i></a>  ',
        '<a class="btnDelete isTooltip" href="javascript:void(0)" title="' + (params_flag === 0 ? TMLanguage('Global.recover') : TMLanguage('Global.delete')) + '">',
        '<i class="fa ' + (params_flag === 0 ? 'fa-recycle' : 'fa-times') + '" aria-hidden="true"></i></a>'
    ].join('');
}
function numberFormatter(value, row, index) {
    return value.format();
}
function iconFormatter(value, row, index) {
    return '<i class="' + value + '" aria-hidden="true"></i>';
}
function resetPassFormatter(value, row, index) {
    return '<span class="badge badge-success resetPass" role="button">Reset</span>';
}
function totalTextFormatter(data) {
    return 'Total';
}
function totalNameFormatter(data) {
    return data.length;
}
function totalPriceFormatter(data) {
    var total = 0;
    $.each(data, function (i, row) {
        total += +(row.price.substring(1));
    });
    return '$' + total;
}
//params bootstrapTable
function queryParams(params) {
    params.flag = params_flag;
    if (params_dateStart !== null)
        params.dateStart = params_dateStart; //$('.dateStart').val(); // add param1
    if (params_dateEnd !== null)
        params.dateEnd = params_dateEnd;//$('.dateEnd').val(); // add param2
    // console.log(JSON.stringify(params));
    // {"limit":10,"offset":0,"order":"asc","your_param1":1,"your_param2":2}
    return params;
    //var params = {};
    //$('#toolbar').find('input[name]').each(function () {
    //    params[$(this).attr('name')] = $(this).val();
    //});
    //return params;
}
//Events bootstrapTable

window.cmdEvents = {
    'click .btnEdit': function (e, value, row, index) {
        getForm({ element: e, url: $urlEdit, data: { id: row.BGCUOCID }, form: FormModal, TinyMCE: true })
    },
    'click .btnDelete': function (e, value, row, index) {
        $(document).TMConfirm({
            target: '.btnDelete',
            modalOk: function () {
                $.post($baseUrl + Segment[0] + '/Delete', $.extend({}, { id: row.BGCUOCID }, token), function (d) {
                    if (d.success) {
                        $($TMAlert).TMAlert({ type: "success", message: d.success });
                        $($table).bootstrapTable('remove', { field: 'BGCUOCID', values: [row.BGCUOCID] });
                    }
                    else if (d.danger) $($TMAlert).TMAlert({ type: "danger", message: d.danger });
                })
            }
        });
    },
    'click .resetPass': function (e, value, row, index) {
        $('#ModalComfirmBody').html(TMLanguage('Users.msgResetPassword'));
        $('#ModalComfirm').modal('show');
        $('#ModalComfirmBtnConfirm').off('click').on('click', function () {
            $.post($url + '/resetPassword', { id: row.id, __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val() }, function (d) {
                if (d.success) {
                    $($TMAlert).TMAlert({ type: "success", message: TMLanguage(d.success).StringFormat(row.username, d.data) });
                }
                else if (d.danger) $($TMAlert).TMAlert({ type: "danger", message: TMLanguage(d.danger) });
            })
        })
    },
};
//Set Language bootstrapTable
$.fn.bootstrapTable.locales['customs'] = {
    formatLoadingMessage: function () {
        return TMLanguage('BSTable.formatLoadingMessage');// $('#BSTableformatLoadingMessage').attr('title');
    },
    formatRecordsPerPage: function (pageNumber) {
        return TMLanguage('BSTable.formatRecordsPerPage').StringFormat(pageNumber);//$('#BSTableformatRecordsPerPage').attr('title').StringFormat(pageNumber);
    },
    formatShowingRows: function (pageFrom, pageTo, totalRows) {
        //return sprintf('Showing %s to %s of %s rows', pageFrom, pageTo, totalRows);
        return TMLanguage('BSTable.formatShowingRows').StringFormat([pageFrom, pageTo, totalRows]);//$('#BSTableformatShowingRows').attr('title').StringFormat(pageFrom, pageTo, totalRows);
    },
    formatDetailPagination: function (totalRows) {
        return TMLanguage('BSTable.formatDetailPagination').attr('title').StringFormat(totalRows);//$('#BSTableformatDetailPagination').attr('title').StringFormat(totalRows);
    },
    formatSearch: function () {
        return TMLanguage('BSTable.formatSearch');//$('#BSTableformatSearch').attr('title');
    },
    formatNoMatches: function () {
        return TMLanguage('BSTable.formatNoMatches');//$('#BSTableformatNoMatches').attr('title');
    },
    formatPaginationSwitch: function () {
        return TMLanguage('BSTable.formatPaginationSwitch');//$('#BSTableformatPaginationSwitch').attr('title');
    },
    formatRefresh: function () {
        return TMLanguage('BSTable.formatRefresh');//$('#BSTableformatRefresh').attr('title');
    },
    formatToggle: function () {
        return TMLanguage('BSTable.formatToggle');//$('#BSTableformatToggle').attr('title');
    },
    formatColumns: function () {
        return TMLanguage('BSTable.formatColumns');//$('#BSTableformatColumns').attr('title');
    },
    formatAllRows: function () {
        return TMLanguage('BSTable.formatAllRows');//$('#BSTableformatAllRows').attr('title');
    },
    formatExport: function () {
        return TMLanguage('BSTable.formatExport');//$('#BSTableformatExport').attr('title');
    }
};
$.extend($.fn.bootstrapTable.defaults, $.fn.bootstrapTable.locales['customs']);

//datetimepicker
$('#dateStart').datetimepicker({
    defaultDate: new Date(DateNow.getFullYear(), DateNow.getMonth(), 1),
    format: 'DD/MM/YYYY',
});
$('#dateEnd').datetimepicker({
    defaultDate: DateNow,
    format: 'DD/MM/YYYY',
});
//Datetime
$($dateStart).on('blur', function () {
    params_dateStart = $($dateStart).val();
    params_dateEnd = $($dateEnd).val();
    $($table).bootstrapTable('refresh');
});
$($dateEnd).on('blur', function () {
    params_dateStart = $($dateStart).val();
    params_dateEnd = $($dateEnd).val();
    $($table).bootstrapTable('refresh');
});
//AjaxLoadding
AjaxLoaddingBounce();
//Create
//$($indexBtnAdd).off('click').on('click', function () {
//    var ma = $(document).find(FormModal);
//    var mc = ma.find('.modal');
//    if (mc.length < 1)
//        $.get($urlCreate, function (d) {
//            ma.append(d);
//            ma.find('.modal').modal('show');
//        }).done(function () {
//            getTinyMCE();
//        }).fail(function () {
//            console.log('Error');
//        })
//});
//Delete Recover
$($indexBtnDeleteRecover).off('click').on('click', function () {
    var chk = $($table).find('[name="btSelectItem"]:checked');
    if (chk.length > 0) {
        if (params_flag === 1)
            $('#ModalComfirmBody').html(TMLanguage('Global.msgDeleteRecord'));
        else
            $('#ModalComfirmBody').html(TMLanguage('Global.msgRecoverRecord'));

        $(document).TMConfirm({
            target: $indexBtnDeleteRecover,
            modalOk: function () {
                var data = ',';
                chk.each(function (index, e) { data += ($(this).val()) + ','; });
                $.post($baseUrl + Segment[0] + '/Delete', $.extend({}, { id: data }, token), function (d) {
                    if (d.success) {
                        $($TMAlert).TMAlert({ type: "success", message: d.success });
                        $($table).bootstrapTable('refresh');
                    }
                    else if (d.danger) $($TMAlert).TMAlert({ type: "danger", message: d.danger });
                })
            }
        });

        //$('#ModalComfirm').modal('show');
        //$('#ModalComfirmBtnConfirm').off('click').on('click', function () {
        //    var data = ',';
            
        //    $.ajax({
        //        type: 'POST',
        //        //dataType: 'json',
        //        //contentType: 'application/json; charset=utf-8',
        //        url: $url + '/Delete',//Host + '/cms/api/UsersAPI',
        //        data: {
        //            id: data,
        //            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        //        },
        //        success: function (d) {
        //            if (d.success) {
        //                $($TMAlert).TMAlert({ type: "success", message: TMLanguage(d.success) });
        //                $($table).bootstrapTable('refresh');
        //            }
        //            else if (d.danger)
        //                $($TMAlert).TMAlert({ type: "danger", message: TMLanguage(d.danger) });
        //        }
        //    })
        //})
    }
})
//Delete Forever
$($indexBtnDeleteForever).off('click').on('click', function () {
    var chk = $($table).find('[name="btSelectItem"]:checked');
    if (chk.length > 0) {
        $('#ModalComfirmBody').html(TMLanguage('Global.msgDeleteRecord'));
        $(document).TMConfirm({
            target: $indexBtnDeleteForever,
            modalOk: function () {
                var data = ',';
                chk.each(function (index, e) { data += ($(this).val()) + ','; });
                $.post($baseUrl + Segment[0] + '/Remove', $.extend({}, { id: data }, token), function (d) {
                    if (d.success) {
                        $($TMAlert).TMAlert({ type: "success", message: d.success });
                        $($table).bootstrapTable('refresh');
                    }
                    else if (d.danger) $($TMAlert).TMAlert({ type: "danger", message: d.danger });
                })
            }
        });
        //$('#ModalComfirm').modal('show');
        //$('#ModalComfirmBtnConfirm').off('click').on('click', function () {
        //    var data = ',';
        //    chk.each(function (index, e) { data += ($(this).val()) + ','; })
        //    $.ajax({
        //        type: 'DELETE',
        //        //dataType: 'json',
        //        //contentType: 'application/json; charset=utf-8',
        //        url: $url,//Host + '/cms/api/UsersAPI',
        //        data: {
        //            id: data,
        //            __RequestVerificationToken: $('[name="__RequestVerificationToken"]').val()
        //        },
        //        success: function (d) {
        //            if (d.success) {
        //                $($TMAlert).TMAlert({ type: "success", message: TMLanguage(d.success) });
        //                $($table).bootstrapTable('refresh');
        //            }
        //            else if (d.danger)
        //                $($TMAlert).TMAlert({ type: "danger", message: TMLanguage(d.danger) });
        //        }
        //    })
        //})
    }
})
//Users Set Roles
$('#indexBtnSetRoles').off('click').on('click', function () {
    var chk = $($table).find('[name="btSelectItem"]:checked');
    if (chk.length > 0) {
        var data = ',';
        chk.each(function (index, e) { data += ($(this).val()) + ','; })
        getForm({ url: 'Users/PartialUsersSetRoles', data: { id: data }, form: FormModal, modal: ModalEdit, })
    }
})
//Flag
function setFlagActive(flag) {
    if (params_flag === 1) {
        $($indexBtnUse).addClass('active');
        $($indexBtnTrash).removeClass('active');
        $($indexBtnDeleteForever).hide();
        $($indexBtnDeleteRecover)
            .removeClass('btn-warning')
            .addClass('btn-danger')
            .attr('data-original-title', TMLanguage('Global.delete'))
            .html('<i class="fa fa-times" aria-hidden="true"></i>');
    } else {
        $($indexBtnTrash).addClass('active');
        $($indexBtnUse).removeClass('active');
        $($indexBtnDeleteForever).show();
        $($indexBtnDeleteRecover)
            .removeClass('btn-danger')
            .addClass('btn-warning')
            .attr('data-original-title', TMLanguage('Global.recover'))
            .html('<i class="fa fa-recycle" aria-hidden="true"></i>');
    }
}
$($indexBtnUse).off('click').on('click', function () {
    params_flag = 1;
    setFlagActive(params_flag);
    $($table).bootstrapTable('refresh');
});
$($indexBtnTrash).off('click').on('click', function () {
    params_flag = 0;
    setFlagActive(params_flag);
    $($table).bootstrapTable('refresh');
});