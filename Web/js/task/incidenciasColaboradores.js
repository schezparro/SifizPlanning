taskApp.controller('incidenciasController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    $scope.varSemanas = 0;
    $scope.semanaActualIncidencia = function () {
        $scope.varSemanas = 0;
        cargarIncidenciaColaboradores($scope.varSemanas, $scope.filtroIncidencia);
    };
    $scope.semanaAlanteIncidencia = function () {
        $scope.varSemanas++;
        cargarIncidenciaColaboradores($scope.varSemanas, $scope.filtroIncidencia);
    };
    $scope.semanaAtrasIncidencia = function () {
        $scope.varSemanas--;
        cargarIncidenciaColaboradores($scope.varSemanas, $scope.filtroIncidencia);
    };
    $scope.filtrarIncidencia = function () {
        cargarIncidenciaColaboradores($scope.varSemanas, $scope.filtroIncidencia);
    };

    angular.element('#panel-incidencia-colaboradores').on('click', '[data-type-inc="1"]', function () {
        var idIncidencia = angular.element(this).attr('data-id-inc');
        
        var datosIncidencia = $http.post("task/dar-incidencia-x-incidencia", {
            idIncidencia: idIncidencia
        });
        datosIncidencia.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-datos-incidencias-colaboradores").modal('show');
                $scope.nombreColaboradorIncidencia = data.colaborador;
                $scope.fechaIncidencia = data.fecha;

                $scope.incidenciasColaborador = data.datos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    });

    $scope.verHechoFundamentacionIncidencia = function () {
        $scope.hechoIncidencia = this.incid.hecho;
        $scope.justificacionIncidencia = this.incid.justificacion;
    };
    
    angular.element('#modal-datos-incidencias-colaboradores').on('hidden.bs.modal', function (e) {
        $scope.hechoIncidencia = "";
        $scope.justificacionIncidencia = "";
    });

}]);