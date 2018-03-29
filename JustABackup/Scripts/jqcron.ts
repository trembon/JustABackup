/// <reference path="typings/jqcron.d.ts" />

$(() => {
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

    $('.cron-selector.read-only').each((i, el) => {
        $(el).jqCronGetInstance().disable();
    });
});