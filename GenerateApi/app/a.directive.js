(function () {
    'use strict';

    angular
        .module('app')
        .directive('a', a);

    //a.$inject = ['$window'];

    function a() {
        var directive = {
            link: link,
            restrict: 'E'
        };
        return directive;

        function link(scope, elem, attrs) {
            if (attrs.ngClick || attrs.href === '' || attrs.href === '#') {
                elem.on('click', function (e) {
                    e.preventDefault();
                });
            }
        }
    }
})();