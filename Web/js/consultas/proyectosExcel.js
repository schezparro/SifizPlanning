angular.module('consultas').controller('proyectosExcelController', ['$scope', '$http', function ($scope, $http) {
    $scope.proyectos = [];
    $scope.filtro = {};
    $scope.proyectoFormData = {};

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

    $scope.seleccionarProyecto = function (proyecto) {
        $scope.proyectoFormData = angular.copy(proyecto);
    };

    $scope.editarProyecto = function (proyecto, $event) {
        $event.stopPropagation();
        $scope.seleccionarProyecto(proyecto);
    };

    $scope.limpiarFormulario = function () {
        $scope.proyectoFormData = {};
        $scope.proyectoForm.$setPristine();
        $scope.proyectoForm.$setUntouched();
    };

    $scope.guardarProyecto = function () {
        if ($scope.proyectoForm.$invalid) {
            alert('Por favor, complete los campos requeridos.');
            return;
        }

        var url = $scope.proyectoFormData.id ? '/Consultas/ActualizarProyecto' : '/Consultas/CrearProyecto';

        $http.post(url, $scope.proyectoFormData).then(function (response) {
            if (response.data.success) {
                alert('Proyecto guardado correctamente.');
                $scope.limpiarFormulario();
                $scope.cargarProyectos();
            } else {
                alert('Error al guardar proyecto: ' + response.data.msg);
            }
        }, function (error) {
            alert('Error en la petición: ' + error.statusText);
        });
    };

    // Initial load
    $scope.cargarProyectos();
}]);
