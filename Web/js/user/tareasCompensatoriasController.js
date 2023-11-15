devApp.controller('tareasCompensatoriasController', ['$scope', '$http', function ($scope, $http) {
    $scope.tiempoCompensatorio = 0;
    $scope.actualizarTareasTiempoCompensatorioColaborador = function () {
        tareasColaboradorTiempoCompensatorio();
    };

    function tareasColaboradorTiempoCompensatorio() {
        //Cargando los tiempos compensatorios del colaborador    
        var ajaxColaboradorTareasCompensatorias = $http.post("user/colaborador-tareas-compensatorias/", {
            todas: $scope.mostrarTodasTareasCompensatorias
        });
        ajaxColaboradorTareasCompensatorias.success(function (data) {
            if (data.success === true) {
                $scope.tareasCompensatorias = data.tareasCompensatorias;
                $scope.tiempoCompensatorio = data.tiempoCompensatorio;                
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
                return false
            }
        });
    }
    //Llamada inicial a las tareas compensatorias del colaborador
    tareasColaboradorTiempoCompensatorio();
}]);