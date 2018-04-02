/// <reference path="typings/jqcron.d.ts" />

$(() => {
    cron.initialize();
});

namespace cron {

    export function initialize(): void {
        activateCronOnElements($('input.cron'));
    }

    export function activateCron($wrapper: JQuery): void {
        activateCronOnElements($wrapper.find('input.cron'));
    }

    function activateCronOnElements($elements: JQuery): void {
        $elements.jqCron({
            enabled_minute: true,
            multiple_dom: true,
            multiple_month: true,
            multiple_mins: true,
            multiple_dow: true,
            multiple_time_hours: true,
            multiple_time_minutes: true,
            default_period: 'week',
            no_reset_button: true,
            lang: 'en'
        });

        $elements.filter('.read-only').each((i, el) => {
            $(el).jqCronGetInstance().disable();
        });
    }
}