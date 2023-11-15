rrhhApp.controller('solicitudesController', ['$scope', '$http', function ($scope, $http) {
    $scope.recargarVacaciones = function () {
        var solicitudes = $http.post("rrhh/vacaciones-usuarios", {});
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
        var solicitudes = $http.post("rrhh/permisos-usuarios", {});
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

    $scope.aceptarVacaciones = function (solVac) {
        waitingDialog.show('Aprobando solicitud...', { dialogSize: 'sm', progressType: 'success' });
        var fechaIngresoSolicitud = new Date(parseInt(solVac.FechaIngresoSolicitud.substr(6)));
        var fechaIngresoInstitucion = new Date(parseInt(solVac.FechaIngresoInstitucion.substr(6)));
        var fechaInicioVacaciones = new Date(parseInt(solVac.FechaInicioVacaciones.substr(6)));
        var fechaFinVacaciones = new Date(parseInt(solVac.FechaFinVacaciones.substr(6)));
        var fechaPresentarseTrabajar = new Date(parseInt(solVac.FechaPresentarseTrabajar.substr(6)));

        solVac.FechaIngresoSolicitud = fechaIngresoSolicitud.getFullYear() + '-' + fechaIngresoSolicitud.getDate() + '-' + (fechaIngresoSolicitud.getMonth() + 1);
        solVac.FechaIngresoInstitucion = fechaIngresoInstitucion.getFullYear() + '-' + fechaIngresoInstitucion.getDate() + '-' + (fechaIngresoInstitucion.getMonth() + 1);
        solVac.FechaInicioVacaciones = fechaInicioVacaciones.getFullYear() + '-' + fechaInicioVacaciones.getDate() + '-' + (fechaInicioVacaciones.getMonth() + 1);
        solVac.FechaFinVacaciones = fechaFinVacaciones.getFullYear() + '-' + fechaFinVacaciones.getDate() + '-' + (fechaFinVacaciones.getMonth() + 1);
        solVac.FechaPresentarseTrabajar = fechaPresentarseTrabajar.getFullYear() + '-' + fechaPresentarseTrabajar.getDate() + '-' + (fechaPresentarseTrabajar.getMonth() + 1);

        solVac.Estado = "APROBADA"
        var solicitudVacaciones = $http.post("user/solicitar-vacaciones",
            {
                solicitud: solVac
            });
        solicitudVacaciones.success(function (data) {
            if (data.success === true) {
                $scope.recargarVacaciones();
                waitingDialog.hide();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.rechazarVacaciones = function (solVac) {
        waitingDialog.show('Rechazando solicitud...', { dialogSize: 'sm', progressType: 'success' });

        solVac.Estado = "RECHAZADA"
        var solicitudVacaciones = $http.post("user/solicitar-vacaciones",
            {
                solicitud: solVac
            });
        solicitudVacaciones.success(function (data) {
            if (data.success === true) {
                $scope.recargarVacaciones();
                waitingDialog.hide();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.aceptarPermiso = function (solPer) {
        waitingDialog.show('Aprobando solicitud...', { dialogSize: 'sm', progressType: 'success' });
        solPer.Estado = "APROBADA"
        var solicitudPermisos = $http.post("user/solicitar-permisos",
            {
                solicitud: solPer
            });
        solicitudPermisos.success(function (data) {
            if (data.success === true) {
                $scope.recargarPermisos();
                waitingDialog.hide();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.rechazarPermiso = function (solPer) {
        waitingDialog.show('Rechazando solicitud...', { dialogSize: 'sm', progressType: 'success' });

        solPer.Estado = "RECHAZADA"
        var solicitudPermisos = $http.post("user/solicitar-permisos",
            {
                solicitud: solPer
            });
        solicitudPermisos.success(function (data) {
            if (data.success === true) {
                $scope.recargarPermisos();
                waitingDialog.hide();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.showVacDetail = function (sol) {
        $scope.solicitudVacacionesUsuario = sol;

        angular.element("#modal-show-vacaciones").modal('show');
    };

    $scope.showPerDetail = function (sol) {
        $scope.solicitudPerUsuario = sol;

        angular.element("#modal-show-solicitud").modal('show');
    };

}]);

rrhhApp.filter("strDateToStr", function () {
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

rrhhApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);