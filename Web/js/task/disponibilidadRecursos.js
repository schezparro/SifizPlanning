taskApp.controller('disponibilidadRecursosController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    $scope.varSemanas = 0;
    $scope.semanaActualDisponibilidad = function () {
        $scope.varSemanas = 0;
        cargarDisponibilidadRecursos($scope.varSemanas, $scope.filtroDisponibilidad);
    };
    $scope.semanaAlanteDisponibilidad = function () {
        $scope.varSemanas++;
        cargarDisponibilidadRecursos($scope.varSemanas, $scope.filtroDisponibilidad);
    };
    $scope.semanaAtrasDisponibilidad = function () {
        $scope.varSemanas--;
        cargarDisponibilidadRecursos($scope.varSemanas, $scope.filtroDisponibilidad);
    };
    $scope.filtrarDisponibilidad = function () {
        cargarDisponibilidadRecursos($scope.varSemanas, $scope.filtroDisponibilidad);
    };

}]);