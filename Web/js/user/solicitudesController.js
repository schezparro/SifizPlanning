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

    $scope.recargarFeriados = function () {
        var ajaxFeriados = $http.post("rrhh/dar-feriado/", {});

        ajaxFeriados.success(function (data) {
            if (data.success) {
                var jsonFeriados = data.feriados;
                $scope.feriados = jsonFeriados.map(f => {

                    var fechaMilisegundos = f.fecha.match(/\d+/)[0];

                    var fecha = new Date(parseInt(fechaMilisegundos));

                    return {
                        id: f.id,
                        fecha: fecha
                    };
                });

            } else {
                alert("No se pudieron cargar los datos");
            }
        });
    };
    $scope.recargarFeriados();

    $scope.obtenerDatosUsuario = function () {
        var ajaxUsuario = $http.post("user/dar-datos-usuario/", {});

        ajaxUsuario.success(function (data) {
            if (data.success) {
                $scope.datosUsuario = data.datosUsuario;
            }
        });
    };
    $scope.obtenerDatosUsuario();


    //Para la solicitud de vacaciones
    $scope.windowSolicitarVacaciones = function () {
        $scope.solVacaciones.id = "undefined";
        $scope.solVacaciones.apellidosNombres = $scope.datosUsuario.nombre;
        $scope.solVacaciones.cargo = $scope.datosUsuario.cargo;
        $scope.isEmpresaVacacionesSelectEditable = false;
        $scope.btnEditar = false;
       
        if ($scope.datosUsuario.empresa === "SIFIZSOFT") {
            $scope.solVacaciones.empresa = "SIFIZSOFT";
            $scope.isEmpresaVacacionesSelectEditable = true;
        } else if ($scope.datosUsuario.empresa === "INTECSOFT") {
            $scope.solVacaciones.empresa = "INTECSOFT";
            $scope.isEmpresaVacacionesSelectEditable = true;
        } else {
            $scope.isEmpresaVacacionesSelectEditable = false;
        }

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

    $scope.editarSolVacaciones = function (event) {
        event.preventDefault();
        waitingDialog.show('Solicitando vacaciones...', { dialogSize: 'sm', progressType: 'success' });

        var solicitudVacaciones = $http.post("user/editar-solicitar-vacaciones",
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

    $scope.fechaInicioVacacionesChange = function (fechaInicioVacaciones) {
        var coincide = $scope.feriados.some(f => f.fecha.getTime() === fechaInicioVacaciones.getTime());
        $scope.hayFechaInicioVacaciones = true;

        if (coincide) {
            angular.element("#enviar-solicitud-vacaciones-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-inicio-vacaciones").css("display", "block");
        } else {
            angular.element("#enviar-solicitud-vacaciones-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-inicio-vacaciones").css("display", "none");
        }

        if ($scope.hayFechaInicioVacaciones && $scope.hayFechaFinVacaciones) {

            var fechaInicioVaciones = $scope.solVacaciones.fechaInicioVacaciones
            var fechaFinVacaciones = $scope.solVacaciones.fechaFinVacaciones

            var feriadosFiltrados = $scope.feriados.filter(f => f.fecha.getTime() >= fechaInicioVaciones.getTime() && f.fecha.getTime() <= fechaFinVacaciones.getTime());
            var feriadosEnMedio = feriadosFiltrados.map(f => {
                var fecha = new Date(f.fecha);
                var dia = ("0" + fecha.getDate()).slice(-2);
                var mes = ("0" + (fecha.getMonth() + 1)).slice(-2);
                var anio = fecha.getFullYear();
                return dia + "/" + mes + "/" + anio;
            });

            if (feriadosEnMedio.length > 0) {
                angular.element("#feriados-medio-vacaciones").css("display", "block");
                angular.element("#feriados-medio-vacaciones").html('<td colspan="5"><p style="background-color: #ffc107;">Día feriados entre las vacaciones: ' + feriadosEnMedio.join(', ') + '</p></td>');
            } else {
                angular.element("#feriados-medio-vacaciones").css("display", "none");
            }
        }
    };

    $scope.fechaFinVacacionesChange = function (fechaFinVacaciones) {
        var coincide = $scope.feriados.some(f => f.fecha.getTime() === fechaFinVacaciones.getTime());
        $scope.hayFechaFinVacaciones = true;

        if (coincide) {
            angular.element("#enviar-solicitud-vacaciones-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-fin-vacaciones").css("display", "block");
        } else {
            angular.element("#enviar-solicitud-vacaciones-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-fin-vacaciones").css("display", "none");
        }

        if ($scope.hayFechaInicioVacaciones && $scope.hayFechaFinVacaciones) {

            var fechaInicioVaciones = $scope.solVacaciones.fechaInicioVacaciones
            var fechaFinVacaciones = $scope.solVacaciones.fechaFinVacaciones

            var feriadosFiltrados = $scope.feriados.filter(f => f.fecha.getTime() >= fechaInicioVaciones.getTime() && f.fecha.getTime() <= fechaFinVacaciones.getTime());
            var feriadosEnMedio = feriadosFiltrados.map(f => {
                var fecha = new Date(f.fecha);
                var dia = ("0" + fecha.getDate()).slice(-2);
                var mes = ("0" + (fecha.getMonth() + 1)).slice(-2);
                var anio = fecha.getFullYear();
                return dia + "/" + mes + "/" + anio;
            });

            if (feriadosEnMedio.length > 0) {
                angular.element("#feriados-medio-vacaciones").css("display", "block");
                angular.element("#feriados-medio-vacaciones").html('<td colspan="5"><p style="background-color: #ffc107;">Día feriados entre las vacaciones: ' + feriadosEnMedio.join(', ') + '</p></td>');
            } else {
                angular.element("#feriados-medio-vacaciones").css("display", "none");
            }
        }
    };

    //Para la solicitud de permisos
    $scope.windowSolicitarPermiso = function () {
        $scope.solicitudPer.id = "undefined";
        $scope.solicitudPer.apellidosNombres = $scope.datosUsuario.nombre;
        $scope.solicitudPer.cargo = $scope.datosUsuario.cargo;
        $scope.solicitudPer.area = $scope.datosUsuario.departamento;
        $scope.isEmpresaPermisoSelectEditable = false;

        if ($scope.datosUsuario.empresa === "SIFIZSOFT") {
            $scope.solicitudPer.empresa = "SIFIZSOFT";
            $scope.isEmpresaPermisoSelectEditable = true;
        } else if ($scope.datosUsuario.empresa === "INTECSOFT") {
            $scope.solicitudPer.empresa = "INTECSOFT";
            $scope.isEmpresaPermisoSelectEditable = true;
        } else {
            $scope.isEmpresaPermisoSelectEditable = false;
        }

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

    $scope.editarSolPermisos = function (event) {
        event.preventDefault();
        waitingDialog.show('Solicitando permisos...', { dialogSize: 'sm', progressType: 'success' });
        var solicitudPermisos = $http.post("user/editar-solicitar-permisos",
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

    $scope.fechaInicioPermisoChange = function (fechaInicioPermiso) {
        var coincide = $scope.feriados.some(f => f.fecha.getTime() === fechaInicioPermiso.getTime());
        $scope.hayFechaInicioPermiso = true;

        if (coincide) {
            angular.element("#enviar-solicitud-permiso-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-inicio-permiso").css("display", "block");
        } else {
            angular.element("#enviar-solicitud-permiso-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-inicio-permiso").css("display", "none");
        }

        if ($scope.hayFechaInicioPermiso && $scope.hayFechaFinPermiso) {

            var fechaInicioPermiso = $scope.solicitudPer.fechaDesde
            var fechaFinPermiso = $scope.solicitudPer.fechaHasta

            var feriadosFiltrados = $scope.feriados.filter(f => f.fecha.getTime() >= fechaInicioPermiso.getTime() && f.fecha.getTime() <= fechaFinPermiso.getTime());
            var feriadosEnMedio = feriadosFiltrados.map(f => {
                var fecha = new Date(f.fecha);
                var dia = ("0" + fecha.getDate()).slice(-2);
                var mes = ("0" + (fecha.getMonth() + 1)).slice(-2);
                var anio = fecha.getFullYear();
                return dia + "/" + mes + "/" + anio;
            });

            if (feriadosEnMedio.length > 0) {
                angular.element("#feriados-medio-permiso").css("display", "block");
                angular.element("#feriados-medio-permiso").html('<td colspan="5"><p style="background-color: #ffc107;">Día feriados entre los días de permiso: ' + feriadosEnMedio.join(', ') + '</p></td>');
            } else {
                angular.element("#feriados-medio-permiso").css("display", "none");
            }
        }
    };

    $scope.fechaFinPermisoChange = function (fechaFinPermiso) {
        var coincide = $scope.feriados.some(f => f.fecha.getTime() === fechaFinPermiso.getTime());
        $scope.hayFechaFinPermiso = true;

        if (coincide) {
            angular.element("#enviar-solicitud-permiso-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-inicio-permiso").css("display", "block");
        } else {
            angular.element("#enviar-solicitud-permiso-btn").prop("disabled", coincide);
            angular.element("#alert-coincide-inicio-permiso").css("display", "none");
        }

        if ($scope.hayFechaInicioPermiso && $scope.hayFechaFinPermiso) {

            var fechaInicioPermiso = $scope.solicitudPer.fechaDesde
            var fechaFinPermiso = $scope.solicitudPer.fechaHasta

            var feriadosFiltrados = $scope.feriados.filter(f => f.fecha.getTime() >= fechaInicioPermiso.getTime() && f.fecha.getTime() <= fechaFinPermiso.getTime());
            var feriadosEnMedio = feriadosFiltrados.map(f => {
                var fecha = new Date(f.fecha);
                var dia = ("0" + fecha.getDate()).slice(-2);
                var mes = ("0" + (fecha.getMonth() + 1)).slice(-2);
                var anio = fecha.getFullYear();
                return dia + "/" + mes + "/" + anio;
            });

            if (feriadosEnMedio.length > 0) {
                angular.element("#feriados-medio-permiso").css("display", "block");
                angular.element("#feriados-medio-permiso").html('<td colspan="5"><p style="background-color: #ffc107;">Día feriados entre los días de permiso: ' + feriadosEnMedio.join(', ') + '</p></td>');
            } else {
                angular.element("#feriados-medio-permiso").css("display", "none");
            }
        }
    };

    $scope.editarSolicitudVacaciones = function (solVac) {
        if (solVac.Estado === "RECHAZADA") {
            $scope.solVacaciones.cedula = solVac.Cedula;
            $scope.solVacaciones.fechaIngresoSolicitud = convertDate(solVac.FechaIngresoSolicitud);
            $scope.solVacaciones.empresa = solVac.Empresa;
            $scope.solVacaciones.fechaIngresoInstitucion = convertDate(solVac.FechaIngresoInstitucion);
            $scope.solVacaciones.aniosServicio = solVac.AniosServicio;
            $scope.solVacaciones.diasCorresponden = solVac.DiasCorresponden;
            $scope.solVacaciones.diasDisfrutar = solVac.DiasDisfrutar;
            $scope.solVacaciones.diasPendientes = solVac.DiasPendientes;
            $scope.solVacaciones.delAnio = solVac.DelAnio;
            $scope.solVacaciones.alAnio = solVac.AlAnio;
            $scope.solVacaciones.fechaInicioVacaciones = convertDate(solVac.FechaInicioVacaciones);
            $scope.solVacaciones.fechaFinVacaciones = convertDate(solVac.FechaFinVacaciones);
            $scope.solVacaciones.fechaPresentarseTrabajar = convertDate(solVac.FechaPresentarseTrabajar);
            $scope.solVacaciones.observaciones = solVac.Observaciones;
            $scope.solVacaciones.jefe = solVac.Jefe;
            $scope.solVacaciones.estado = solVac.Estado;
            $scope.windowSolicitarVacaciones();          
            $scope.solVacaciones.id = solVac.ID;
        }
    };

    $scope.editarSolicitudPermiso = function (solPer) {
        if (solPer.Estado === "RECHAZADA") {
            $scope.solicitudPer.cedula = solPer.Cedula;
            $scope.solicitudPer.fechaIngresoSolicitud = convertDate(solPer.FechaIngresoSolicitud);
            $scope.solicitudPer.empresa = solPer.Empresa;
            $scope.solicitudPer.personal = solPer.Personal;
            $scope.solicitudPer.matrimonio = solPer.Matrimonio;
            $scope.solicitudPer.comida = solPer.Comida;
            $scope.solicitudPer.paternidad = solPer.Paternidad;
            $scope.solicitudPer.otros = solPer.Otros;
            $scope.solicitudPer.fechaDesde = convertDate(solPer.FechaDesde);
            $scope.solicitudPer.horaSalida = solPer.HoraSalida;
            $scope.solicitudPer.fechaHasta = convertDate(solPer.FechaHasta);
            $scope.solicitudPer.horaRetorno = solPer.HoraRetorno;
            $scope.solicitudPer.motivo = solPer.Motivo;
            $scope.solicitudPer.jefe = solPer.Jefe;
            $scope.solicitudPer.estado = solPer.Estado;           
            $scope.windowSolicitarPermiso();
            $scope.solicitudPer.id = solPer.ID;
        }
    };

    //*********************** REGISTRO DE VACACIONES**************/
    $scope.registroVacaciones = function () {
        if (solPer.Estado === "RECHAZADA") {
            $scope.solicitudPer.cedula = solPer.Cedula;
            $scope.solicitudPer.fechaIngresoSolicitud = convertDate(solPer.FechaIngresoSolicitud);
            $scope.solicitudPer.empresa = solPer.Empresa;
            $scope.solicitudPer.personal = solPer.Personal;
            $scope.solicitudPer.matrimonio = solPer.Matrimonio;
            $scope.solicitudPer.comida = solPer.Comida;
            $scope.solicitudPer.paternidad = solPer.Paternidad;
            $scope.solicitudPer.otros = solPer.Otros;
            $scope.solicitudPer.fechaDesde = convertDate(solPer.FechaDesde);
            $scope.solicitudPer.horaSalida = solPer.HoraSalida;
            $scope.solicitudPer.fechaHasta = convertDate(solPer.FechaHasta);
            $scope.solicitudPer.horaRetorno = solPer.HoraRetorno;
            $scope.solicitudPer.motivo = solPer.Motivo;
            $scope.solicitudPer.jefe = solPer.Jefe;
            $scope.solicitudPer.estado = solPer.Estado;
            $scope.windowSolicitarPermiso();
            $scope.solicitudPer.id = solPer.ID;
        }
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

function convertDate(dateStr) {
    return new Date(parseInt(dateStr.replace(/\/Date\((\d+)\)\//, "$1")));
}

function convertTime(time) {
    var partes = time.split(":");
    return partes[0] + ":" + partes[1];
}


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