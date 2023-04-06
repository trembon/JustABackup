$(() => {
    // find the active link, if any
    let $activeLink = $(`nav.menu a[href="${window.location.pathname}"]`);
    if ($activeLink.length === 1) {
        let $parents = $activeLink.parentsUntil('nav.menu', 'li');

        // add the active class to all li parents, to let the UI show where the user is
        $parents.addClass('active');

        // all parent-parent li nodes will receive a click event, to receive the open class and be opened
        $parents.slice(1).children('a').click();
    }
});