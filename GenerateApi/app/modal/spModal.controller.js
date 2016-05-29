(function () {
    'use strict';

    angular
        .module('app')
        .controller('spModalController', spModalController);

    spModalController.$inject = ['$uibModalInstance', 'viewModel'];

    function spModalController($uibModalInstance, viewModel) {
        var vm = this;
        vm.data = viewModel;
        vm.close = close;
        function close() {
            $uibModalInstance.close();
        }
    }
})();
