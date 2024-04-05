rrhhApp.controller('solicitudesController', ['$scope', '$http', function ($scope, $http) {
    $scope.recargarVacaciones = function () {
        var solicitudes = $http.post("rrhh/vacaciones-usuarios", {
            filtro: $scope.filtroPermisos
        });
        solicitudes.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                console.log(data);
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
                console.log($scope.permisos)
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

        if ($scope.rechazoVacacionesComentario === null || $scope.rechazoVacacionesComentario === "") {
            messageDialog.show('Información', "El comentario no puede estar vacío.");
        } else {
            waitingDialog.show('Rechazando solicitud...', { dialogSize: 'sm', progressType: 'success' });

            $scope.solVacTemp.Estado = "RECHAZADA"
            console.log($scope.solVacTemp);
            var solicitudVacaciones = $http.post("user/solicitar-vacaciones",
                {
                    solicitud: $scope.solVacTemp,
                    rechazoVacacionesComentario: $scope.rechazoVacacionesComentario
                });
            solicitudVacaciones.success(function (data) {
                if (data.success === true) {
                    $scope.recargarVacaciones();
                    waitingDialog.hide();
                }
                else {
                    console.log(data);
                    messageDialog.show('Información', data.msg);
                }
            });
        }
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

    $scope.rechazarPermiso = function () {

        if ($scope.rechazoPermisoComentario === null || $scope.rechazoPermisoComentario === "") {
            messageDialog.show('Información', "El comentario no puede estar vacío.");
        } else {
            waitingDialog.show('Rechazando solicitud...', { dialogSize: 'sm', progressType: 'success' });

            $scope.solPerTemp.Estado = "RECHAZADA"
            var solicitudPermisos = $http.post("user/solicitar-permisos",
                {
                    solicitud: $scope.solPerTemp,
                    rechazoPermisoComentario: $scope.rechazoPermisoComentario
                });
            solicitudPermisos.success(function (data) {
                if (data.success === true) {
                    $scope.recargarPermisos();
                    waitingDialog.hide();
                }
                else {
                    console.log(data.msg);
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

    $scope.rechazarSolicitudPermiso = function (solPer) {
        $scope.rechazoPermisoComentario = "";
        $scope.solPerTemp = solPer;
        angular.element("#modal-rechazar-permiso").modal('show');
    }

    $scope.rechazarSolicitudVacaciones = function (solVac) {
        $scope.rechazoVacacionesComentario = "";
        $scope.solVacTemp = solVac;
        angular.element("#modal-rechazar-vacaciones").modal('show');
    }

    $scope.showVacDetail = function (sol) {
        $scope.solicitudVacacionesUsuario = sol;

        angular.element("#modal-show-vacaciones").modal('show');
    };

    $scope.showPerDetail = function (sol) {
        $scope.solicitudPerUsuario = sol;

        angular.element("#modal-show-solicitud").modal('show');
    };

   

    $scope.generarVacacionesPDF = function () {
        // Obtener la tabla original
        var table = document.getElementById("table-show-vacaciones").cloneNode(true);

        // Crear un iframe oculto
        var iframe = document.createElement('iframe');
        iframe.style.display = 'none'; // Ocultar el iframe
        document.body.appendChild(iframe);

        // Configurar el estilo de la tabla
        table.setAttribute('style', 'width: 100%; margin-bottom: 1rem; color: #212529; background-color: transparent; border-collapse: collapse;');
        Array.from(table.querySelectorAll("th, td")).forEach(function (cell) {
            cell.setAttribute('style', 'padding: .3rem; border: 1px solid black;');
        });

        // Cargar el contenido del PDF en el iframe
        iframe.contentDocument.body.appendChild(table);

        // Imprimir el contenido del iframe
        iframe.contentWindow.print();

        // Opcional: Eliminar el iframe después de imprimir
        iframe.onload = function () {
            setTimeout(function () {
                document.body.removeChild(iframe);
            }, 0);
        };
    };

   

    $scope.generarPermisoPDF = function () {
        var table = document.getElementById("table-show-permiso").cloneNode(true);
        var iframe = document.createElement('iframe');
        iframe.style.display = 'none'; // Ocultar el iframe
        document.body.appendChild(iframe);

        // Configurar el estilo de la tabla
        table.setAttribute('style', 'width: 100%; margin-bottom: 1rem; color: #212529; background-color: transparent; border-collapse: collapse;');
        Array.from(table.querySelectorAll("th, td")).forEach(function (cell) {
            cell.setAttribute('style', 'padding: .3rem; border: 1px solid black;');
        });

        // Cargar el contenido del PDF en el iframe
        iframe.contentDocument.body.appendChild(table);

        // Imprimir el contenido del iframe
        iframe.contentWindow.print();

        // Opcional: Eliminar el iframe después de imprimir
        iframe.onload = function () {
            setTimeout(function () {
                document.body.removeChild(iframe);
            }, 0);
        };
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