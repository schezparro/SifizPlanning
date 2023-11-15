devApp.controller('solicitudesController', ['$scope', '$http', function ($scope, $http) {
    $scope.recargarVacaciones = function () {
        var solicitudes = $http.post("user/vacaciones-usuario", {});
        solicitudes.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.vacaciones = data.solicitudes;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }
    $scope.recargarVacaciones();

    $scope.recargarPermisos = function () {
        var solicitudes = $http.post("user/permisos-usuario", {});
        solicitudes.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.permisos = data.solicitudes;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }
    $scope.recargarPermisos();


    //Para la solicitud de vacaciones
    $scope.windowSolicitarVacaciones = function () {
        angular.element("#modal-solicitud-vacaciones").modal("show");
    };

    $scope.enviarSolicitudVacaciones = function () {
        waitingDialog.show('Solicitando vacaciones...', { dialogSize: 'sm', progressType: 'success' });
        var solicitudVacaciones = $http.post("user/solicitar-vacaciones",
            {
                solicitud: $scope.solVacaciones
            });
        solicitudVacaciones.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.recargarVacaciones();
                angular.element("#modal-solicitud-vacaciones").modal("hide");
            }
            else {
                angular.element("#modal-solicitud-vacaciones").modal("hide");
                messageDialog.show('Información', data.msg);
            }
        });
    };
    //Para la solicitud de permisos
    $scope.windowSolicitarPermiso = function () {
        angular.element("#modal-solicitud-permisos").modal("show");
    };

    $scope.enviarSolicitudPermiso = function () {
        waitingDialog.show('Solicitando permisos...', { dialogSize: 'sm', progressType: 'success' });
        var solicitudPermisos = $http.post("user/solicitar-permisos",
            {
                solicitud: $scope.solicitudPer
            });
        solicitudPermisos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.recargarPermisos();
                angular.element("#modal-solicitud-permisos").modal("hide");
            }
            else {
                angular.element("#modal-solicitud-permisos").modal("hide");
                messageDialog.show('Información', data.msg);
            }
        });
    };
}]);

devApp.filter("strDateToStr", function () {
    return function (textDate) {
        if (textDate !== undefined) {
            var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
            return dateToStr(fecha);
        }
        return "";
    }
});

function dateToStr(dateObj, format, separator) {
    /**
     * Convert a date object to a string
     * @argument dateObj {Date} A date object
     * @argument format {string} An string representation of the date format. Default: dd-mm-yyyy. More could be added as necessary
     * @argument separator {string} Character used for join the parts of the date
     * @returns {string} An string representation of a Date
     */
    var year = dateObj.getFullYear().toString();
    var month = dateObj.getMonth() + 1;
    var month = (month < 10) ? '0' + month : month;
    var day = dateObj.getDate();
    var day = (day < 10) ? '0' + day : day;
    var sep = (separator) ? separator : '/';
    switch (format) {
        case 'mm/dd/yyyy':
            var out = [month, day, year];
            break;
        case 'yyyy/mm/dd':
            var out = [year, month, day];
            break;
        default: //dd/mm/yyyy
            var out = [day, month, year];
    }
    return out.join(sep);
};

devApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);