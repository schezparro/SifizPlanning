indicadoresApp.controller('indicadoresGestores', ['$scope', '$http', function ($scope, $http) {
    $scope.loading.show();
    var infoTickets = $http.post("indicadores/dar-infotickets/", {});
    infoTickets.success(function (data) {
        $scope.loading.hide();

        if (data.success) {
            $scope.infoTickets = data.infoTickets;
        } else {
            alert("No se pudieron cargar los datos");
        }
    });
}]);