(function () {
    'use strict';

    angular.module('app', [
        'ui.bootstrap',
        'ngclipboard',
        'angular-loading-bar'
    ])
    .constant('apiUrl','//localhost:8177/api/')
})();