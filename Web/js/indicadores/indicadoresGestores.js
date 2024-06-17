indicadoresApp.controller('indicadoresGestores', ['$scope', '$http', '$filter', function ($scope, $http, $filter) {

    var gestores = $http.post("catalogos/dar-gestores/", {});
    gestores.success(function (data) {
        if (data.success) {
            $scope.gestores = data.gestores;
            if ($scope.gestores.length > 0) {
                $scope.gestorSeleccionado = $scope.gestores[0].idColaborador;
            }
        } else {
            alert("No se pudieron cargar los datos");
        }
    });

    $scope.initDatepickers = function () {
        angular.element('#fecha-inicio-gestores').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.finicioGestores);

        angular.element('#fecha-fin-gestores').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.ffinGestores);
    };

    var fechaActual = new Date();
    $scope.finicioGestores = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.ffinGestores = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);

    $scope.initDatepickers();

    $scope.refrech = function () {
        $scope.loading.show();
        var data = {
            fechaInicio: $filter('date')($scope.finicioGestores, 'dd/MM/yyyy'),
            fechaFin: $filter('date')($scope.ffinGestores, 'dd/MM/yyyy'),
            gestor: $scope.gestorSeleccionado
        };
        $http.post("indicadores/dar-tickets-por-gestor/", data).then(function (response) {
            $scope.loading.hide();
            if (response.data.success) {
                $scope.infoTickets = response.data.tickets;
            } else {
                alert("No se pudieron cargar los datos");
            }
        });
    };
    //$scope.refrech();

}]);

indicadoresApp.directive('formatDate', function ($filter) {
    return {
        require: 'ngModel',
        link: function (scope, elem, attr, ngModelCtrl) {
            ngModelCtrl.$formatters.push(function (modelValue) {
                if (modelValue) {
                    return $filter('date')(modelValue, 'dd/MM/yyyy');
                }
            });
        }
    };
});
