var rrhhApp = angular.module('rrhh', []);

rrhhApp.config(function ($provide, $httpProvider) {

    // Intercept http calls.
    $provide.factory('MyHttpInterceptor', function ($q) {
        return {
            // On request success
            request: function (config) {
                return config || $q.when(config);
            },
            // On request failure
            requestError: function (rejection) {
                return $q.reject(rejection);
            },
            // On response success
            response: function (response) {
                var x = false
                if (typeof response.data === "string" || (typeof response.data == "object" && response.data.constructor === String)) {
                    x = true;
                }
                if (x && response.data.indexOf('ng-app="login"') !== -1) {
                    window.location.assign("sifizplanning/redirect");
                }
                else
                    return response || $q.when(response);
            },
            // On response failture
            responseError: function (rejection) {
                return $q.reject(rejection);
            }
        };
    });

    // Add the interceptor to the $httpProvider.
    $httpProvider.interceptors.push('MyHttpInterceptor');
});

rrhhApp.controller('rrhhController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'VACACIONES Y PERMISOS';
    $scope.rutaImages = "Web/images/";
    $scope.diasCalendar = [];
    $scope.colaboradores = [];

    //Para la seleccion en el menú
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel-vacaciones-permisos").addClass('invisible');
        angular.element("#panel-tiempo-compensatorio").addClass('invisible');
        angular.element("#panel-marcas-electronicas").addClass('invisible');
        angular.element("#panel-permisos").addClass('invisible');
        angular.element("#panel-feriados").addClass('invisible');
        angular.element("#panel-incidencias").addClass('invisible');
        angular.element("#panel-dias-vacaciones").addClass('invisible');
    };

    $scope.IrVacacionesPermisos = function () {
        ocultar();
        angular.element("#panel-vacaciones-permisos").removeClass('invisible');
        $scope.funcionalidad = 'VACACIONES Y PERMISOS';
    };

    $scope.IrTiempoCompensatorio = function () {
        ocultar();
        angular.element("#panel-tiempo-compensatorio").removeClass('invisible');
        $scope.funcionalidad = 'TIEMPOS COMPENSATORIOS';
        $scope.loading.show();

        //Cargando los colaboradores con los tiempos de compensasión de cada uno    
        var ajaxColaboradoresTiempoCompensatorio = $http.post("rrhh/colaboradores-tiempo-compensatorio/", {
            start: 0,
            limit: 0
        });
        ajaxColaboradoresTiempoCompensatorio.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.trabajadoresTiempo = data.trabajadoresTiempo;
                setTimeout(function () {
                    //Actualizando el tamaño de la tabla en height    
                    var tamanioY = angular.element("#panel-tiempo-compensatorio").height() - 35 - angular.element("#thead-colaboradores-tiempo").height();
                    angular.element("#tbody-colaboradores-tiempo").css({
                        height: tamanioY
                    });
                }, 100);
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };

    //Agregando funcionalidades para cargar las marcas electronicas
    $scope.IrMarcasElectronicas = function () {
        ocultar();
        angular.element("#panel-marcas-electronicas").removeClass('invisible');
        $scope.funcionalidad = 'REGISTRO DE ASISTENCIAS';
    };

    $scope.IrPermisos = function () {
        ocultar();
        angular.element("#panel-permisos").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE SOLICITUDES';
    };

    $scope.IrFeriados = function () {
        ocultar();
        angular.element("#panel-feriados").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE FERIADOS';
    };

    $scope.IrIncidencias = function () {
        ocultar();
        angular.element("#panel-incidencias").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE INCIDENCIAS';
    };
    $scope.IrDiasDisponiblesVacaciones = function () {
        ocultar();
        angular.element("#panel-dias-vacaciones").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE DIAS DE VACACIONES';
    };

    $scope.IrPlanificacionVacaciones = function () {
        ocultar();
        angular.element("#panel-planificacion-vacaciones").removeClass('invisible');
        $scope.funcionalidad = 'PLANIFICACION DE VACACIONES';
    };

    $scope.loadingAjax = function () {
        this.show = function (target) {
            angular.element("#loadingDiv").show();
        };
        this.hide = function (target) {
            angular.element("#loadingDiv").hide();
        };
    };

    $scope.loading = new $scope.loadingAjax();

}]);

//controler de vacaciones y permisos
rrhhApp.controller('vacacionesPermisos', ['$scope', '$http', function ($scope, $http) {
    $scope.idTrabajador = 0;//Id del trabajador para la edicion

    angular.element('#dtpk-fecha-desde-vacaciones').datetimepicker({
        format: 'DD/MM/YYYY',
        locale: 'es'
    });

    angular.element('#dtpk-fecha-hasta-vacaciones').datetimepicker({
        format: 'DD/MM/YYYY',
        locale: 'es'
    });

    //Los DatetimePicker
    angular.element('#dtpk-fecha-desde-permiso').datetimepicker({
        format: 'DD/MM/YYYY HH:mm',
        locale: 'es'
    });

    angular.element('#dtpk-fecha-hasta-permiso').datetimepicker({
        format: 'DD/MM/YYYY HH:mm',
        locale: 'es'
    });

    angular.element('#dtpk-fecha-desde-disponibilidad').datetimepicker({
        format: 'DD/MM/YYYY',
        locale: 'es'
    }).on('dp.change', function () {
        $scope.buscarDatosDisponibilidad();
    });


    //Cargando los tipos de permisos    
    var ajaxTiposPermisos = $http.post("rrhh/tipos-permisos/", {});
    ajaxTiposPermisos.success(function (data) {
        if (data.success === true) {
            $scope.tiposPermisos = data.tiposPermisos;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });

    //Funciones de carga de datos
    $scope.cargarTrabajadores = function (start, limit, filtro) {
        $scope.loading.show();
        //Cargando los trabajadores
        var ajaxTrabajadores = $http.post("admin/trabajadores/", {
            start: start,
            limit: limit,
            filtro: filtro
        });
        ajaxTrabajadores.success(function (data) {
            if (data.success === true) {
                $scope.trabajadores = data.trabajadores;
                setTimeout(function () {
                    //Actualizando el tamaño de la tabla en height    
                    var tamanioY = angular.element("#panel-vacaciones-permisos").height() - 35 - angular.element("#thead-colaboradores").height();
                    angular.element("#tbody-colaboradores").css({
                        height: tamanioY
                    });

                    setTimeout(function () { recalcularHead(); $scope.loading.hide(); }, 10);
                }, 100);
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };
    $scope.cargarTrabajadores(0, 0);

    //Seleccion de trabajadores
    angular.element("#panel-vacaciones-permisos").on('click', '.table-tr', function () {
        angular.element('.table-tr').removeClass('tr-select');
        angular.element(this).addClass('tr-select');
        $scope.idTrabajador = angular.element(this).attr('data-id');
        var childrens = angular.element(this).children();
        $scope.nombreTrabajador = angular.element(childrens[1]).html();
        $scope.usuarioActivo = angular.element(childrens[6]).html() === "SI" ? 1 : 0;
    });

    //Establecer Vacaciones
    $scope.modalEstablecerVacaciones = function () {
        if ($scope.idTrabajador === 0) {
            messageDialog.show('Información', "Debe seleccionar el colaborador.");
        }
        else {
            angular.element("#modal-vacaciones-colaborador").modal('show');
        }
    };
    $scope.guardarVacaciones = function () {
        waitingDialog.show('Guardando las vacaciones...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxVacaciones = $http.post("rrhh/guardar-vacaciones/", {
            idColaborador: $scope.idTrabajador,
            desde: angular.element('[ng-model="fechaDesdeVacaciones"]').val(),
            hasta: angular.element('[ng-model="fechaHastaVacaciones"]').val(),
        });
        ajaxVacaciones.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-vacaciones-colaborador").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-vacaciones-colaborador").on('hidden.bs.modal', function (e) {
        datosInicialesVacaciones();
    });
    function datosInicialesVacaciones() {
        $scope.idTrabajador = 0;
        $scope.nombreTrabajador = "";
        $scope.fechaDesdeVacaciones = "";
        $scope.fechaHastaVacaciones = "";

        angular.element("#modal-vacaciones-colaborador .form-control").val("");
        //$('#dtpk-fecha-desde-vacaciones').datepicker('update', "");
        //$('#dtpk-fecha-hasta-vacaciones').datepicker('update', "");
        angular.element('[ng-model="fechaDesdeVacaciones"]').val('');
        angular.element('[ng-model="fechaHastaVacaciones"]').val('');
    }

    //Establecer Permisos
    $scope.modalEstablecerPermisos = function () {
        if ($scope.idTrabajador === 0) {
            messageDialog.show('Información', "Debe seleccionar el colaborador.");
        }
        else {
            angular.element("#modal-permiso-colaborador").modal('show');
        }
    };

    //filtrando las Vacaciones y los Permisos
    $scope.filtrarColaboradoresVacaciones = function () {
        $scope.cargarTrabajadores(0, 0, $scope.filtroColaboradoresVacaciones);
    };

    //Guardando un permiso
    $scope.guardarPermisoColaborador = function () {
        waitingDialog.show('Guardando el permiso...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxPermisos = $http.post("rrhh/guardar-permiso/", {
            idColaborador: $scope.idTrabajador,
            tipoPermiso: $scope.tipoPermiso,
            desde: angular.element('[ng-model="fechaDesdePermiso"]').val(),
            hasta: angular.element('[ng-model="fechaHastaPermiso"]').val(),
            motivo: $scope.motivoPermiso
        });
        ajaxPermisos.success(function (data) {
            if (data.success === true) {
                $scope.aprobarPermisoColaborador(data.idPermiso);
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };
    //Aprovando un permiso
    $scope.aprobarPermisoColaborador = function (idPermiso) {
        var ajaxAprobarPermiso = $http.post("task/aprobar-permiso/", {
            idPermiso: idPermiso
        });
        ajaxAprobarPermiso.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-permiso-colaborador").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-permiso-colaborador").on('hidden.bs.modal', function (e) {
        datosInicialesPermisos();
    });
    function datosInicialesPermisos() {
        $scope.idTrabajador = 0;
        $scope.tipoPermiso = '';
        angular.element('[ng-model="fechaDesdePermiso"]').val('');
        angular.element('[ng-model="fechaHastaPermiso"]').val('');
        $scope.motivoPermiso = '';
        angular.element("#modal-permiso-colaborador .form-control").val("");
    }

    //PARA EL CLICK DERECHO
    //Inicialmente se oculta
    angular.element("#menu-opciones").hide(0);
    $scope.verPanelOpcionesVacaciones = function (e, idTrabajador) {
        $scope.idTrabajador = idTrabajador;
        angular.element("#menu-opciones").hide(0);
        var opcionesMenu = angular.element("#menu-opciones li");
        angular.element(opcionesMenu).hide(0);
        angular.element(opcionesMenu[0]).show();
        angular.element("#menu-opciones").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    };

    //Ocultar con scape
    //cuando hagamos click, el menú desaparecerá
    $(document).click(function (e) {
        if (e.button == 0) {
            $("#menu-opciones").css("display", "none");
        }
    });
    //si pulsamos escape, el menú desaparecerá
    $(document).keydown(function (e) {
        if (e.keyCode == 27) {
            $("#menu-opciones").css("display", "none");
        }
    });

    $scope.anioVacaciones = 0;
    $scope.verVacacionesColaborador = function (difYear) {
        $scope.loading.show();
        if (difYear === undefined || difYear === '') {
            difYear = 0;
        }
        $scope.anioVacaciones = difYear;
        var ajaxVerVacaciones = $http.post("rrhh/ver-vacaciones-colaborador/", {
            idColaborador: $scope.idTrabajador,
            difYear: difYear
        });
        ajaxVerVacaciones.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.dataMeses = data.datos;
                $scope.nombreTrabajador = data.nombreColaborador;
                $scope.yearVacaciones = data.year;
                angular.element("#modal-ver-vacaciones-colaborador").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.disminuirAnios = function () {
        $scope.anioVacaciones--;
        $scope.verVacacionesColaborador($scope.anioVacaciones);
    };
    $scope.aumentarAnios = function () {
        $scope.anioVacaciones++;
        $scope.verVacacionesColaborador($scope.anioVacaciones);
    };

    //Eliminar las vacaciones
    $scope.idDiaEliminar = 0;
    $scope.panelDiaVacaciones = function (e, idDia) {
        if (idDia !== undefined) {
            $scope.idDiaEliminar = idDia;

            angular.element("#menu-opciones").hide(0);
            var opcionesMenu = angular.element("#menu-opciones li");
            angular.element(opcionesMenu).hide(0);
            angular.element(opcionesMenu[1]).show();
            angular.element("#menu-opciones").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
            return false;
        }
        else {
            $scope.idDiaEliminar = 0;
        }
    };
    $scope.eliminarDiaVacaciones = function () {
        if ($scope.idDiaEliminar !== 0) {
            waitingDialog.show('Eliminando el día de vacaciones...', { dialogSize: 'sm', progressType: 'success' });
            var eliminarVacacion = $http.post("rrhh/eliminar-vacacion/", {
                id: $scope.idDiaEliminar
            });
            eliminarVacacion.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.verVacacionesColaborador($scope.anioVacaciones);
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

    angular.element('[data-toggle="tooltip"]').tooltip({ container: 'body' });
    $scope.semanaDisponibilidad = 0;
    $scope.buscarDatosDisponibilidad = function () {
        $scope.loading.show();
        var ajaxDatosDisponibilidad = $http.post('rrhh/disponibilidad-recursos-fechas/', {
            semana: $scope.semanaDisponibilidad,
            cantSemanas: $scope.cantSemanasDisponibilidad,
            fechaSemanaInicio: angular.element('[ng-model="fechaDesdeDisponibilidad"]').val()
        });
        ajaxDatosDisponibilidad.success(function (data) {
            if (data.success === true) {
                $scope.cantDias = data.cantDias;
                $scope.datosDisponibilidad = data.datos;
                $scope.departamentosDisponibilidad = data.departamentos;
                $scope.sedesDisponibilidad = data.sedes;
                $scope.semanasDisponibilidad = data.semanasDisponibilidad;

                setTimeout(function () {
                    var anchoTotal = angular.element('#id-datos-colaboradores').width();
                    var ancho = angular.element(angular.element('.div-tabla-disponibilidad')[0]).width();
                    var altoDiv = angular.element('#id-datos-colaboradores').height();
                    var anchoDiv = (ancho - 10) / data.cantDias;

                    var semanas = $scope.cantDias / 7;
                    var anchoSemana = (ancho - 10) / semanas;
                    angular.element('#div-semanas-disponibilidad').css({ width: anchoTotal });
                    angular.element('.semana-disponibilidad-colab').css({ height: altoDiv + 22 });
                    angular.element('.semana-disponibilidad-colab').css({ width: anchoSemana });
                    var segmentos = angular.element('.div-segmento');
                    angular.element.each(segmentos, function (key, segmento) {
                        var length = angular.element(segmento).attr('data-length') * anchoDiv;
                        angular.element(segmento).css({ width: length });
                    });
                    angular.element('[data-toggle="tooltip"]').tooltip({ container: 'body' });

                    $scope.loading.hide();
                }, 500);
            }
            else {
                $scope.loading.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.modalDisponibilidad = function () {
        angular.element('#modal-vista-disponibilidad-recursos').modal('show');
        if ($scope.cantSemanasDisponibilidad === undefined) {
            $scope.cantSemanasDisponibilidad = 8;
        }
        $scope.buscarDatosDisponibilidad();
    };
    $scope.atrazarSemanaDisponibilidad = function () {
        $scope.semanaDisponibilidad = $scope.semanaDisponibilidad - 1;
        $scope.buscarDatosDisponibilidad();
    };
    $scope.adelantarSemanaDisponibilidad = function () {
        $scope.semanaDisponibilidad = $scope.semanaDisponibilidad + 1;
        $scope.buscarDatosDisponibilidad();
    };
    $scope.actualizarSemanasDisponibilidad = function () {
        $scope.buscarDatosDisponibilidad();
    };
    $scope.actualizarGraficoDisponibilidad = function () {
        setTimeout(function () {
            var ancho = angular.element(angular.element('.div-tabla-disponibilidad')[0]).width();
            var anchoDiv = (ancho - 10) / $scope.cantDias;
            var segmentos = angular.element('.div-segmento');
            angular.element.each(segmentos, function (key, segmento) {
                var length = angular.element(segmento).attr('data-length') * anchoDiv;
                angular.element(segmento).css({ width: length });
            });
            angular.element('[data-toggle="tooltip"]').tooltip({ container: 'body' });
        }, 10);
    };
}]);

rrhhApp.controller('tiemposCompensatorios', ['$scope', '$http', function ($scope, $http) {

    $scope.idColaboradorTiempoCompensatorio = 0;

    //Menu de click derecho sobre las filas de los tiempos compensatorios
    angular.element("#menu-opciones-tiempo-compensatorio").hide(0);
    $scope.verPanelOpcionesVacaciones = function (e, idTrabajador) {
        $scope.idColaboradorTiempoCompensatorio = idTrabajador;
        angular.element("#menu-opciones-tiempo-compensatorio").hide(0);
        var opcionesMenu = angular.element("#menu-opciones-tiempo-compensatorio li");
        angular.element(opcionesMenu).show();
        angular.element("#menu-opciones-tiempo-compensatorio").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    };
    //Ocultar con scape
    //cuando hagamos click, el menú desaparecerá
    $(document).click(function (e) {
        if (e.button == 0) {
            $("#menu-opciones-tiempo-compensatorio").css("display", "none");
        }
    });
    //si pulsamos escape, el menú desaparecerá
    $(document).keydown(function (e) {
        if (e.keyCode == 27) {
            $("#menu-opciones-tiempo-compensatorio").css("display", "none");
        }
    });

    //Muestra el Window de las tareas compensatorias de los colaboradores
    $scope.verTiempoCompensatorioColaborador = function () {
        tareasColaboradorTiempoCompensatorio($scope.idColaboradorTiempoCompensatorio,
            $scope.mostrarTodasTareasCompensatorias,
            function () {
                angular.element("#modal-ver-tareas-compensatorias").modal("show");
            });
    };

    $scope.actualizarTareasTiempoCompensatorioColaborador = function () {
        tareasColaboradorTiempoCompensatorio($scope.idColaboradorTiempoCompensatorio, $scope.mostrarTodasTareasCompensatorias, function () { });
    };

    function tareasColaboradorTiempoCompensatorio(idColaborador, todos, cf) {
        //Cargando los tiempos compensatorios del colaborador    
        var ajaxColaboradorTareasCompensatorias = $http.post("rrhh/colaborador-tareas-compensatorias/", {
            idColaborador: idColaborador,
            todas: todos
        });
        ajaxColaboradorTareasCompensatorias.success(function (data) {
            if (data.success === true) {
                $scope.tareasCompensatorias = data.tareasCompensatorias;
                $scope.tiempoCompensatorio = data.tiempoCompensatorio;
                cf();
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
                return false
            }
        });
    }

}]);

//Controlador para mostrar marcas electronicas
rrhhApp.controller('marcasElectronicas', ['$scope', '$http', function ($scope, $http) {

    $scope.numeroSemanas = 1;
    $scope.controlTiempoFiltroSede = "Quito";
    $scope.fechaLunes = null;
    $scope.esteLunes = null;
    $scope.fechaHoy = null;
    var aplicarFiltro = false;

    //Funciones generales
    // Función que suma o resta días a la fecha indicada formato d/m/Y
    $scope.mySplitEmail = function (string, nb) {
        var array = string.split('@');
        return array[nb];
    };

    sumaFecha = function (d, fecha) {
        if (fecha === null)
            fecha = new Date();
        var aFecha = null;
        var sep = null;
        if (typeof fecha === "string") {
            sep = fecha.indexOf('/') != -1 ? '/' : '-';
            aFecha = fecha.split(sep);
        }
        else {
            var sFecha = fecha.getDate() + "/" + (fecha.getMonth() + 1) + "/" + fecha.getFullYear();
            sep = sFecha.indexOf('/') != -1 ? '/' : '-';
            aFecha = sFecha.split(sep);
        }
        var fechaTmp = aFecha[2] + '/' + aFecha[1] + '/' + aFecha[0];
        fecha = new Date(fechaTmp);
        fecha.setDate(fecha.getDate() + parseInt(d));
        var anno = fecha.getFullYear();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        mes = (mes < 10) ? ("0" + mes) : mes;
        dia = (dia < 10) ? ("0" + dia) : dia;
        var fechaFinal = dia + sep + mes + sep + anno;
        return (fechaFinal);
    };

    $scope.cambiarSede = function () {
        var sede = angular.element("#selectSede").val();
        $scope.controlTiempoFiltroSede = sede;
        if (!aplicarFiltro)
            actualizarDatos($scope.fechaLunes);
        else
            actualizarDatos($scope.fechaLunes, angular.toJson(filtroTareas));
    }

    $scope.cambiarSemanas = function () {
        var semanas = angular.element("#selectSemanas").val();
        $scope.numeroSemanas = semanas;
        if (!aplicarFiltro)
            actualizarDatos($scope.fechaLunes);
        else
            actualizarDatos($scope.fechaLunes, angular.toJson(filtroTareas));
    };

    $scope.semanaProxima = function () {
        var ndias = 7 * $scope.numeroSemanas;
        $scope.fechaLunes = sumaFecha(ndias, $scope.fechaLunes);
        if (!aplicarFiltro)
            actualizarDatos($scope.fechaLunes);
        else
            actualizarDatos($scope.fechaLunes, angular.toJson(filtroTareas));
    };

    $scope.semanaAtras = function () {
        var ndias = 7 * $scope.numeroSemanas * (-1);
        $scope.fechaLunes = sumaFecha(ndias, $scope.fechaLunes);
        if (!aplicarFiltro)
            actualizarDatos($scope.fechaLunes);
        else
            actualizarDatos($scope.fechaLunes, angular.toJson(filtroTareas));
    };

    $scope.diaHoy = function () {
        $scope.fechaLunes = $scope.esteLunes;
        if (!aplicarFiltro)
            actualizarDatos($scope.fechaLunes);
        else
            actualizarDatos($scope.fechaLunes, angular.toJson(filtroTareas));
    };

    angular.element(document).on('click', '[data-toggle="popover"]', function () {
        var obj = this;
        var elements = angular.element('[data-toggle="popover"]');
        angular.element.each(elements, function (i, e) {
            if (e !== obj) {
                angular.element(e).popover('hide');
            }
            else {
                angular.element(obj).popover('toggle');
            }
        });
    });

    angular.element('#filtroFecha.datepicker-filtro').datepicker({
        format: 'dd/mm/yyyy'
    })
        .on('changeDate', function (e) {
            var sFecha = angular.element('#filtroFecha.datepicker-filtro').val();
            var aFecha = sFecha.split('/');
            var fecha = aFecha[2] + '/' + aFecha[1] + '/' + aFecha[0];
            fecha = new Date(fecha);
            var diaSemana = fecha.getDay();
            var ndias = -6;//Por si es domingo
            if (diaSemana !== 0)//Si no es domingo
                var ndias = 1 - diaSemana;
            $scope.fechaLunes = sumaFecha(ndias, sFecha);

            if (!aplicarFiltro)
                actualizarDatos($scope.fechaLunes);
        });

    //Actualizar datos
    actualizarDatos = function (fechaLunes, jsonFiltro) {
        $scope.colaboradores = [];
        $scope.actualizarListaDias(fechaLunes, $scope.fechaHoy);
        if (jsonFiltro === undefined)
            jsonFiltro = "";

        $scope.loading.show();
        //Cargando las marcas de los colaboradores        
        var numeroSemanas = $scope.numeroSemanas;
        var sedeFiltro = $scope.controlTiempoFiltroSede;
        var fechaHasta = sumaFecha(7 * numeroSemanas, fechaLunes);

        ////Quitando los popuovers
        angular.element('[data-toggle="popover"]').popover('hide');
        var ajaxMarcasElectronicas = $http.post("rrhh/marcas-electronicas", {
            sede: sedeFiltro,
            fechaDesde: fechaLunes,
            fechaHasta: fechaHasta
        });
        ajaxMarcasElectronicas.success(function (data) {
            if (data.success === true) {
                $scope.colaboradores = data.colaboradores;
                $scope.loading.hide();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funcion que actualiza la fecha de los dias de la semana en el calendario de las tareas
    $scope.actualizarListaDias = function (fechaLunes, fechaHoy) {
        $scope.diasCalendar = [];

        var dias = ['Lun ', 'Mar ', 'Mie ', 'Jue ', 'Vie ', 'Sab ', 'Dom '];
        var clases = ['dia-tarea1', 'dia-tarea2', 'dia-tarea3', 'dia-tarea3'];
        var numeroSemanas = angular.element("#selectSemanas").val();
        var cant = numeroSemanas * 7;
        var i = 0;
        var clase = clases[numeroSemanas - 1];
        while (i < cant) {
            for (var j = 0; j < dias.length; j++) {
                var dia = dias[j];
                var fecha = sumaFecha(i, fechaLunes);
                i++;

                var obj = {
                    dia: dias[j] + fecha,
                    clase: clase,
                    date: fecha
                };
                if (fecha === fechaHoy) {
                    obj.clase = obj.clase + " dia-hoy";
                }
                else if (j === 5 || j === 6) {
                    obj.clase = obj.clase + " fin-semana";
                }
                $scope.diasCalendar.push(obj);
            }
        }
        var ancho = (1021 - (numeroSemanas * 7) + 1) / (numeroSemanas * 7);
        $scope.anchoCeldaTarea = ancho;
        angular.element(".td-tareas").css({ width: ancho });
    };

    //Cargando el ultimo lunes
    var ajaxUltimoLunes = $http.post("user/ultimo-lunes", {});
    ajaxUltimoLunes.success(function (data) {
        if (data.success === true) {
            $scope.fechaLunes = data.lunes;
            $scope.esteLunes = data.lunes;
            $scope.fechaHoy = data.hoy;
            actualizarDatos($scope.fechaLunes);
            $('#filtroFecha .datepicker-filtro').datepicker('update', $scope.fechaHoy);
        }
        else {
            //alert("Error en la obtención de la fecha.")
        }
    });

}]);

//Directivas y Filtros
rrhhApp.directive('ngRightClick', function ($parse) {
    return function (scope, element, attrs) {
        var fn = $parse(attrs.ngRightClick);
        element.bind('contextmenu', function (event) {
            scope.$apply(function () {
                event.preventDefault();
                fn(scope, { $event: event });
            });
        });
    };
});

rrhhApp.filter('filtroDisponibilidad', function () {
    return function (colabs, depart, sede, filtro) {
        var output = Array();
        output = colabs;
        if (depart !== undefined && sede != undefined && filtro != undefined) {
            output = colabs.filter(function (item) {
                return item.departamento.toString().indexOf(depart) > -1 && item.sede.toString().indexOf(sede) > -1 && item.colaborador.nombre.toString().indexOf(filtro) > -1;
            });
        }
        else if (depart !== undefined && sede !== undefined) {
            output = colabs.filter(function (item) {
                return item.departamento.toString().indexOf(depart) > -1 && item.sede.toString().indexOf(sede) > -1;
            });
        }
        else if (depart !== undefined && filtro != undefined) {
            return item.departamento.toString().indexOf(depart) > -1 && item.colaborador.nombre.toString().indexOf(filtro) > -1;
        }
        else if (sede !== undefined && filtro != undefined) {
            return item.sede.toString().indexOf(sede) > -1 && item.colaborador.nombre.toString().indexOf(filtro) > -1;
        }
        else if (depart !== undefined) {
            output = colabs.filter(function (item) {
                return item.departamento.toString().indexOf(depart) > -1;
            });
        }
        else if (sede !== undefined) {
            output = colabs.filter(function (item) {
                return item.sede.toString().indexOf(sede) > -1;
            });
        }
        else if (filtro !== undefined) {
            output = colabs.filter(function (item) {
                return item.colaborador.nombre.toString().indexOf(filtro) > -1;
            });
        }

        return output;
    }
});

var recalcularHead = function () {
    //Calculando los anchos del thead
    var thead = angular.element("#thead-colaboradores");
    var tbody = angular.element("#tbody-colaboradores");
    var ths = thead.find("th");
    var tds = tbody.find("td");

    angular.element.each(tds, function (key, value) {
        var withtd = angular.element(value).width();
        var th = ths[key];
        angular.element(th).width(withtd);
    });
};

$(window).resize(function () {
    //Actualizando el tamaño de la tabla en height    
    var tamanioY = angular.element("#panel-vacaciones-permisos").height() - 35 - angular.element("#thead-colaboradores").height();
    angular.element("#tbody-colaboradores").css({
        height: tamanioY
    });

    recalcularHead();
});

rrhhApp.controller('incidenciasController', ['$scope', '$http', function ($scope, $http) {

    //Funciones de carga de datos
    $scope.cargarIncidencias = function (start, filtro) {
        $scope.loading.show();
        //Cargando las Incidencias
        var ajaxIncidencias = $http.post("rrhh/incidencias-colaborador/", {
            start: start,
            filtro: filtro
        });
        ajaxIncidencias.success(function (data) {
            if (data.success === true) {
                $scope.incidencias = data.incidencias;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        $scope.loading.hide();

    };
    $scope.cargarIncidencias(0);

    //filtrando las Incidencias
    $scope.filtrarColaboradoresIncidencias = function () {
        $scope.cargarIncidencias(0, $scope.filtroColaboradoresIncidencias);
    };


    angular.element('#fecha-incidencia').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    //ABRIR MODAL NUEVA INCIDENCIA
    $scope.modalNuevaIncidencia = function () {
        $scope.estaActivo = true;
        angular.element("#modal-agregar-incidencias").modal('show');
    };

    var ajaxColaboradores = $http.post("rrhh/dar-colaboradores-modal-incidencias", {});

    ajaxColaboradores.success(function (data) {
        if (data.success === true) {
            $scope.colaboradores = data.colaboradores;
        }
    });

    $scope.GuardarIncidenciasColaboradores = function () {


        waitingDialog.show('Guardando las incidencias...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxIncidencias = $http.post("rrhh/guardar-incidencias-colaborador/", {
            idColaborador: $scope.colaboradorSeleccionado,
            incidencia: $scope.nuevoIncidente,
            descripcion: $scope.descripcion,
            fechaIncidencia: $scope.fechaIncidencia,
            estaActivo: $scope.estaActivo,
        });
        ajaxIncidencias.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.colaboradorSeleccionado = "";
                $scope.nuevoIncidente = "";
                $scope.descripcion = "";
                $scope.fechaIncidencia = "";
                angular.element("#modal-agregar-incidencias").modal('hide');
                $scope.cargarIncidencias(0);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };


    //Filter de angular para las fechas
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

}]);


/*******************************DIAS VACACIONES*******************************************/

rrhhApp.controller('diasVacacionesController', ['$scope', '$http', function ($scope, $http) {

    //Funciones de carga de datos
    $scope.cargarDiasVacasiones = function (start, filtro) {
        $scope.loading.show();
        //Cargando las Incidencias
        var ajaxIncidencias = $http.post("rrhh/dias-vacaciones-colaborador/", {
            start: start,
            filtro: filtro
        });
        ajaxIncidencias.success(function (data) {
            if (data.success === true) {
                $scope.diasVacacionesColaboradores = data.diasVacacionesColaboradores;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        $scope.loading.hide();

    };
    $scope.cargarDiasVacasiones(0);

    //filtrando las Incidencias
    $scope.filtrarColaboradoresDiasVacaciones = function () {
        $scope.cargarDiasVacasiones(0, $scope.filtroColaboradoresDiasVacaciones);
    };
    $scope.guardarDiasColaboradores = function (secuencial, cantidad) {

        waitingDialog.show('Guardando los días...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxDiasVacaciones = $http.post("rrhh/guardar-dias-vacaciones-colaborador/", {
            idColaborador: secuencial,
            cantidad: cantidad
        });
        ajaxDiasVacaciones.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.cargarDiasVacasiones(0);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };


    //Filter de angular para las fechas
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

}]);


/*******************************PLANIFICACION VACACIONES*******************************************/

rrhhApp.controller('planificacionVacacionesController', ['$scope', '$http', function ($scope, $http) {

    // Días feriados
    $scope.ObtenerFeriados = function (anno, mes) {
        var ajaxObtenerFeriados = $http.post("user/dar-feriados-mes",
            {
                anno: anno,
                mes: mes
            });

        ajaxObtenerFeriados.success(function (data) {
            if (data.success === true) {
                $scope.diasFeriados = data.diasFeriados;
                $scope.getDaysOfMonth(anno, mes);
            }
        });
    };

    // Variable para manejar la selección
    $scope.isSelecting = false;

    // Función para obtener los días del mes
    $scope.getDaysOfMonth = function (anno, mes) {
        let days = [];
        let firstDay = new Date(anno, mes - 1, 1);
        let lastDay = new Date(anno, mes, 0);

        // Generar los días del mes
        for (let i = 1; i <= lastDay.getDate(); i++) {
            let currentDay = new Date(anno, mes - 1, i);
            let dayOfWeek = currentDay.getDay();
            days.push({
                dayNumber: i,
                day: ['D', 'L', 'M', 'M', 'J', 'V', 'S'][dayOfWeek],
                isHoliday: $scope.diasFeriados.includes(i),
                isWeekend: dayOfWeek === 0 || dayOfWeek === 6
            });
        }

        $scope.daysOfMonth = days; // Guardar en el scope para la tabla
    };


    $scope.verTabla = false;

    $scope.ObtenerPropuesta = function (anno, mes) {
        var ajaxObtenerPropuestaVac = $http.post("user/dar-datos-dias-vacaciones",
            {
                usuario: false,
                anno: anno,
                mes: mes
            });

        ajaxObtenerPropuestaVac.success(function (data) {
            if (data.success === true) {
                $scope.verTabla = true;
                $scope.colaboradores = data.datosColaborador;
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };

    // Función que se ejecuta al cambiar el mes
    $scope.onChangeMes = function (mes) {
        let anno = $scope.annos;
        $scope.ObtenerFeriados(anno, mes);
        $scope.ObtenerPropuesta(anno, mes);

    };

    // Variables para rastrear la operación
    $scope.isSelecting = false;
    $scope.isDeselecting = false;
    $scope.activeColaboradorId = null;

    // Función para iniciar la selección
    $scope.startSelection = function (colaborador, day) {
        let isCurrentlySelected = false;
        if (colaborador.diasDeVacaciones) {
            isCurrentlySelected = (colaborador.diasDeVacaciones.includes(day.dayNumber) || (colaborador.tempSelectedDays && colaborador.tempSelectedDays.includes(day.dayNumber))) && !(colaborador.tempDeselectedDays && colaborador.tempDeselectedDays.includes(day.dayNumber));
        }

        // Validación de reglas de negocio
        if ((day.isHoliday || day.isWeekend) && !isCurrentlySelected) return;

        $scope.activeColaboradorId = colaborador.id;
        if (!colaborador.tempSelectedDays) colaborador.tempSelectedDays = [];
        if (!colaborador.tempDeselectedDays) colaborador.tempDeselectedDays = [];

        colaborador.tempSelectedDays = [];
        colaborador.tempDeselectedDays = [];

        if (colaborador.diasDeVacaciones && colaborador.diasDeVacaciones.includes(day.dayNumber))
            day.isSelected = true;
        else
            day.isSelected = false;

        // Determina si es una operación de selección o deselección
        $scope.isSelecting = !day.isSelected && colaborador.diasDisponibles > 0;
        $scope.isDeselecting = day.isSelected;

        // Limpia listas temporales
        $scope.selectedDays = [];
        $scope.deselectedDays = [];

        // Marca el primer día
        if ($scope.isSelecting && (colaborador.diasDisponibles - colaborador.tempSelectedDays.length) > 0) {
            $scope.markDay(colaborador, day);
        } else if ($scope.isDeselecting) {
            $scope.unmarkDay(colaborador, day);
        }
    };

    // Función para arrastrar la selección
    $scope.dragSelection = function (colaborador, day) {
        if ($scope.activeColaboradorId !== colaborador.id) return;
        
        let isCurrentlySelected = (colaborador.diasDeVacaciones && colaborador.diasDeVacaciones.includes(day.dayNumber) || colaborador.tempSelectedDays.includes(day.dayNumber)) && !colaborador.tempDeselectedDays.includes(day.dayNumber);

        if ((day.isHoliday || day.isWeekend) && !isCurrentlySelected) return;

        if ($scope.isSelecting && !isCurrentlySelected && (colaborador.diasDisponibles - colaborador.tempSelectedDays.length) > 0) {
            $scope.markDay(colaborador, day);
        } else if ($scope.isDeselecting && isCurrentlySelected) {
            $scope.unmarkDay(colaborador, day);
        }
    };

    // Función para finalizar la selección
    $scope.endSelection = function (colaborador) {
        if ($scope.activeColaboradorId !== colaborador.id) return;

        let daysToProcessSelect = colaborador.tempSelectedDays.length > 0 ? [...colaborador.tempSelectedDays] : [];
        let daysToProcessDeselect = colaborador.tempDeselectedDays.length > 0 ? [...colaborador.tempDeselectedDays] : [];
        
        // Procesa las selecciones en bloque
        if (daysToProcessSelect.length > 0) {
            $http.post("user/actualizar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: daysToProcessSelect,
                mes: $scope.mes,
                anno: $scope.annos
            }).success(function (data) {
                if (!data.success) {
                    alert("Error al guardar las vacaciones: " + data.msg);
                } else {
                    if (!colaborador.diasDeVacaciones) colaborador.diasDeVacaciones = [];
                    colaborador.diasDeVacaciones.push(...daysToProcessSelect);
                    colaborador.diasMarcadosCount += daysToProcessSelect.length;
                    colaborador.diasDisponibles -= daysToProcessSelect.length;
                }
                colaborador.tempSelectedDays = [];
            }).error(function () {
                alert("Error al conectar con el servidor.");
                colaborador.tempSelectedDays = [];
            });
        }

        // Procesa las deselecciones en bloque
        if (daysToProcessDeselect.length > 0) {
            $http.post("user/eliminar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: daysToProcessDeselect,
                mes: $scope.mes,
                anno: $scope.annos
            }).success(function (data) {
                if (!data.success) {
                    alert("Error al eliminar las vacaciones: " + data.msg);
                } else {
                    daysToProcessDeselect.forEach(d => {
                        const index = colaborador.diasDeVacaciones.indexOf(d);
                        if (index > -1) colaborador.diasDeVacaciones.splice(index, 1);
                    });
                    colaborador.diasMarcadosCount -= daysToProcessDeselect.length;
                    colaborador.diasDisponibles += daysToProcessDeselect.length;
                }
                colaborador.tempDeselectedDays = [];
            }).error(function () {
                alert("Error al conectar con el servidor.");
                colaborador.tempDeselectedDays = [];
            });
        }

        // Reinicia las variables de estado
        $scope.isSelecting = false;
        $scope.isDeselecting = false;
        $scope.selectedDays = [];
        $scope.deselectedDays = [];
        $scope.activeColaboradorId = null;
    };

    // Marca un día como seleccionado visualmente
    $scope.markDay = function (colaborador, day) {
        if (!colaborador.tempSelectedDays.includes(day.dayNumber)) {
            colaborador.tempSelectedDays.push(day.dayNumber);
        }
    };

    // Marca un día como deseleccionado visualmente
    $scope.unmarkDay = function (colaborador, day) {
        if (!colaborador.tempDeselectedDays.includes(day.dayNumber)) {
            colaborador.tempDeselectedDays.push(day.dayNumber);
        }
    };

    //$scope.toggleSelection = function (colaborador, day) {
    //    let anno = $scope.annos;
    //    let mes = $scope.mes;

    //    // Prevenir selección de días festivos
    //    if (day.isHoliday) {
    //        return;
    //    }

    //    // Asegurar que el colaborador tenga una lista de días seleccionados
    //    if (!colaborador.diasDeVacaciones) {
    //        colaborador.diasDeVacaciones = [];
    //    }

    //    // Convertir day.dayNumber a número
    //    const dayNumber = Number(day.dayNumber);
    //    console.log(dayNumber);
    //    console.log(colaborador.diasDeVacaciones);

    //    // Verificar si el día está en los días de vacaciones
    //    const estaSeleccionado = colaborador.diasDeVacaciones.includes(dayNumber);
    //    console.log(estaSeleccionado);

    //    if (estaSeleccionado == true) {
    //        // Si el día ya está seleccionado, eliminar
    //        $http.post("user/eliminar-propuesta-vacaciones", {
    //            idColaborador: colaborador.id,
    //            days: [dayNumber],
    //            mes: mes,
    //            anno: anno
    //        }).then(function (response) {
    //            if (response.data.success === true) {
    //                // Eliminar el día de vacaciones
    //                colaborador.diasDeVacaciones = colaborador.diasDeVacaciones.filter(dia => dia !== dayNumber);
    //                day.isSelected = false;
    //                colaborador.diasMarcadosCount--;
    //                colaborador.diasDisponibles++;
    //                $scope.onChangeMes(mes);
    //            } else {
    //                alert("Error al eliminar la propuesta de vacaciones: " + response.data.msg);
    //            }
    //        }, function () {
    //            alert("Error en la conexión al servidor. Intenta nuevamente.");
    //        });
    //    } else if (estaSeleccionado == false) {
    //        // Si el día no está seleccionado y hay días disponibles, marcar
    //        if (colaborador.diasDisponibles > 0) {
    //            $http.post("user/actualizar-propuesta-vacaciones", {
    //                idColaborador: colaborador.id,
    //                days: [dayNumber],
    //                mes: mes,
    //                anno: anno
    //            }).then(function (response) {
    //                if (response.data.success === true) {
    //                    // Añadir el día de vacaciones si no existe
    //                    if (!colaborador.diasDeVacaciones.includes(dayNumber)) {
    //                        colaborador.diasDeVacaciones.push(dayNumber);
    //                    }
    //                    day.isSelected = true;
    //                    colaborador.diasMarcadosCount++;
    //                    colaborador.diasDisponibles--;
    //                    $scope.onChangeMes(mes);
    //                } else {
    //                    alert("Error al guardar la propuesta de vacaciones: " + response.data.msg);
    //                }
    //            }, function () {
    //                alert("Error en la conexión al servidor. Intenta nuevamente.");
    //            });
    //        } else {
    //            alert("No puedes seleccionar más días de los disponibles.");
    //        }
    //    }
    //};


    // Inicializar con el mes y año por defecto
    $scope.annos = '2025';
    $scope.mes = '0'; // Enero


    //Filter de angular para las fechas
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

}]);