(function () {
    var methods = ['addClass', 'toggleClass', 'removeClass'];

    $.each(methods, function (index, method) {
        var originalMethod = $.fn[method];

        $.fn[method] = function () {
            var result = originalMethod.apply(this, arguments);
            this.trigger(method, arguments[0]);
            return result;
        };
    });
})();

interface String {
    startsWith(search: string, pos?: number): boolean;
}

if (!String.prototype.startsWith) {
    String.prototype.startsWith = function (search, pos) {
        return this.substr(!pos || pos < 0 ? 0 : +pos, search.length) === search;
    };
}