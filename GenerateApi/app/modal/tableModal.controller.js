(function () {
    'use strict';

    angular
        .module('app')
        .controller('tableModalController', tableModalController);

    tableModalController.$inject = ['$uibModalInstance','items']; 

    function tableModalController($uibModalInstance,items) {
        /* jshint validthis:true */
        var vm = this;
        vm.ok = ok;
        vm.items = items;

        function ok() {
            $uibModalInstance.dismiss('cancel');
        }
    }
})();
