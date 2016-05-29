(function () {
    'use strict';

    angular
        .module('app')
        .controller('tableController', tableController);

    tableController.$inject = ['$http', 'apiUrl','$uibModal'];

    function tableController($http, apiUrl,$uibModal) {
        var vm = this;
        var url=apiUrl+'table';
        vm.generateTable = generateTable;

        onInit();

        function onInit() {
            $http.get(url).then(function (data) {
                vm.data = data.data;
            });
        }

        function generateTable(name) {
            $http.get(url + '?tableName=' + name).then(function (data) {
                $uibModal.open({
                    templateUrl: '/app/modal/tableModal.html',
                    controller: 'tableModalController as vm',
                    size: 'lg',
                    resolve: {
                        items: function () {
                            return data.data;
                        }
                    }
                })
            });
        }
    }
})();