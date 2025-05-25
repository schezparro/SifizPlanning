angular.module('consultas').controller('proyectosExcelController', ['$scope', '$http', function ($scope, $http) {
    $scope.proyectos = [];
    $scope.filtro = {};

    $scope.cargarProyectos = function () {
        $http.post('/Consultas/DarProyectosExcel').then(function (response) {
            if (response.data.success) {
                $scope.proyectos = response.data.proyectos;
            } else {
                alert('Error al cargar proyectos: ' + response.data.msg);
            }
        }, function (error) {
            alert('Error en la petición: ' + error.statusText);
        });
    };

    $scope.filtrarDatos = function () {
        // Filtering is handled by AngularJS filter in the view
    };

    // Initial load
    $scope.cargarProyectos();
}]);
