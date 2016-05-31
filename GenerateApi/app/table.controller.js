(function () {
    'use strict';

    angular
        .module('app')
        .controller('tableController', tableController);

    tableController.$inject = ['$http', 'apiUrl','$uibModal'];

    function tableController($http, apiUrl,$uibModal) {
        var vm = this;
        var url=apiUrl+'table';
        vm.generateDto = generateDto;

        onInit();

        function onInit() {
            $http.get(url).then(function (data) {
                vm.data = data.data;
            }, function () {
                swal("連線錯誤或網路有問題");
            });
        }

        function generateDto(table) {
            $http.get(url + '?tableName=' + table).then(function (data) {
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
            }, function () {
                swal("連線錯誤或網路有問題");
            });
        }
    }
})();