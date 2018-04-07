$(() => {
    $('.card table').on('change', 'input[type="checkbox"]', e => {
        let $card = $(e.currentTarget).closest('.card');
        console.log('card', $card);

        let $boxes = $card.find('table input[type="checkbox"]');
        console.log('boxes', $boxes);
        if ($boxes.is(':checked')) {
            console.log('boxes checked');
            console.log('btns', $card.find('.header-block.pull-right .btn'));
            $card.find('.header-block.pull-right .btn').removeAttr('disabled').removeClass('disabled');
        } else {
            console.log('boxes not checked');
            console.log('btns', $card.find('.header-block.pull-right .btn'));
            $card.find('.header-block.pull-right .btn').attr('disabled', 'disabled').addClass('disabled');
        }
    });

    $('.header-block.pull-right .btn').click(e => {
        let $t = $(e.currentTarget);
        let $card = $t.closest('.card');

        let $list = $card.find('input[type="checkbox"]:checked');
        if ($list.length === 0) {
            return;
        }

        let $ids = $list.map((i, el) => {
            return 'ids=' + $(el).closest('tr').data('id');
        });

        $.getJSON('/api/job/' + $t.data('action') + '?' + $ids.get().join('&'), function (result) {
            for (let job in result) {
                let $col = $card.find('tr[data-id="' + job + '"] td.next-run');
                if (result[job] === null) {
                    $col.text('Paused');
                } else {
                    $col.text(new Date(result[job]).toLocaleString());
                }
            }
        });

        $card.find('input[type="checkbox"]').prop('checked', false);
    });
});