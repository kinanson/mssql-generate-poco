(function () {
    'use strict';

    angular
        .module('app')
        .controller('spController', spController);

    spController.$inject = ['$http', 'apiUrl','$uibModal'];

    function spController($http, apiUrl, $uibModal) {
        var vm = this;
        var url = apiUrl + 'sp/';
        vm.generateSp = generateSp;

        onInit();

        function onInit() {
            $http.get(url).then(function (data) {
                vm.data = data.data;
            });
        }

        function generateSp(spName) {
            $http.get(url + '?spName=' + spName).then(function (data) {
                $uibModal.open({
                    templateUrl: '/app/modal/spModal.html',
                    controller: 'spModalController as vm',
                    size: 'lg',
                    resolve: {
                        viewModel: function () {
                            return data.data;
                        }
                    }
                })
            })
        }
    }
})();