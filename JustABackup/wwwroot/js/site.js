$(function (){
    $('.cron-selector').jqCron({
        enabled_minute: true,
        multiple_dom: true,
        multiple_month: true,
        multiple_mins: true,
        multiple_dow: true,
        multiple_time_hours: true,
        multiple_time_minutes: true,
        default_period: 'week',
        default_value: '15 10-12 * * 1-5',
        no_reset_button: true,
        lang: 'en'
    });

    $('.cron-selector.read-only').each(function (i, el) {
        $(el).jqCronGetInstance().disable();
    });

    $('#job-list').on('change', 'input[type="checkbox"]', function () {
        var list = $('#job-list input[type="checkbox"]');
        if (list.is(':checked')) {
            $('#job-list-buttons .btn').removeAttr('disabled');
        } else {
            $('#job-list-buttons .btn').attr('disabled', 'disabled');
        }
    });

    $('#job-list-buttons a.btn').click(function () {
        var $list = $('#job-list input[type="checkbox"]:checked');
        if ($list.length === 0) {
            return;
        }

        var $ids = $list.map(function (i, el) {
            return 'ids=' + $(el).closest('tr').data('id');
        });

        $.getJSON('/job/' + $(this).data('action') + '/?' + $ids.get().join('&'), function (result) {
            for (var job in result) {
                var $col = $('#job-list tr[data-id="' + job + '"] td.next-run');
                if (result[job] === null) {
                    $col.text('Paused');
                } else {
                    $col.text(new Date(result[job]).toLocaleString());
                }
            }
        });

        $('#job-list input[type="checkbox"]').prop('checked', false);
    });
});