taskApp.controller('solicitudesController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    var taskProxie = angular.element.connection.websocket;
    angular.element.connection.hub.start();
    var cantMostrar = 10;
    var start = 0;
    var pagina = 1;
    $scope.solicitudEscogida = null;
    $scope.frecuenciasRepetir = ['DIARIO', 'SEMANAL', 'MENSUAL'];

    angular.element('#repetirFechaHastaNewSolicitud').datepicker({
        orientation: "botton left",
        format: 'dd/mm/yyyy'
    });
    
    angular.element('#fecha-solicitud-tarea').datepicker({
        orientation: "top left",
        format: 'dd/mm/yyyy'
    });    

    //Recargar las solicitudes
    $scope.recargarSolicitudes = function (mostrarTodas, filtro, start, length ) {
        if (mostrarTodas === undefined) {
            mostrarTodas = false;
        }
        if (filtro === undefined) {
            filtro = "";
        }
        if (start === undefined) {
            start = 0;
        }
        if (length === undefined) {
            length = cantMostrar;
        }

        //Cargando las solicitudes
        var ajaxSolicitudes = $http.post("task/solicitudes-task", {
            mostrarTodas: mostrarTodas,
            filtro: filtro,
            start: start,
            length: length
        });
        ajaxSolicitudes.success(function (data) {
            if (data.success === true) {
                $scope.loading.hide();
                $scope.solicitudes = data.solicitudes;
                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidad / cantMostrar);
                if ($scope.cantPaginas === 0) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas < 5) {
                    for (var i = 1; i <= $scope.cantPaginas; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas > 5) {
                    for (var i = pagina - 4; i <= pagina; i++) {
                        $scope.listaPaginas.push(i);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginas) {
                    pagina = $scope.cantPaginas;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-solicitud-ticket .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Filtrar las solicitudes
    $scope.filtrarSolicitudes = function () {
        start = (pagina - 1) * cantMostrar;
        $scope.recargarSolicitudes($scope.mostrarTodas, $scope.filtroSolicitudTarea, start, cantMostrar);
    };

    //Mostrar todas las solicitudes
    $scope.mostrarTodasSolicitudes = function () {
        start = (pagina - 1) * cantMostrar;
        $scope.recargarSolicitudes($scope.mostrarTodas, $scope.filtroSolicitudTarea, start, cantMostrar);
    };

    //Para el paginador
    $scope.paginar = function () {
        var start = (pagina - 1) * cantMostrar;
        var lenght = cantMostrar;
        $scope.recargarSolicitudes($scope.mostrarTodas, $scope.filtroSolicitudTarea, start, cantMostrar);
    };

    $scope.cambiarPagina = function (pag) {
        pagina = pag;
        $scope.paginar();
    };

    $scope.atrazarPagina = function () {
        if (pagina > 1) {
            pagina--;
            $scope.paginar();
        }
    };

    $scope.avanzarPagina = function () {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.paginar();
        }
    };

    //Window Asignacion de Tareas a Solicitud
    $scope.windowGestionarSolicitudTicket = function (solicitud) {
        var botones = angular.element("#modal-descripcion-solicitud button");        
        if (solicitud.gestionable === 1 || solicitud.gestionable === "1") {            
            angular.element(botones).removeAttr('disabled');
        }
        else {           
            angular.element(botones[1]).attr('disabled', 'disabled');
            angular.element(botones[3]).attr('disabled', 'disabled');
        }

        $scope.solicitudEscogida = solicitud;        
        var fecha = new Date(parseInt(solicitud.fecha.replace('/Date(', '')));        
        $scope.horaPeticionTarea = dateToStrTime(fecha);
        $scope.descripcionTarea = solicitud.descripcion;
        angular.element("#modal-descripcion-solicitud").modal('show');
    };

    $scope.generarTarea = function () {
        $scope.idSolicitudTrabajador = $scope.solicitudEscogida.id;
        $scope.idTrabajador = $scope.solicitudEscogida.idTrabajador;
        $scope.nombreColaborador = $scope.solicitudEscogida.usuario;
        $scope.sede = $scope.solicitudEscogida.sede;
        $scope.ubicacion = $scope.solicitudEscogida.idSede;
        angular.element("#modal-new-task-solicitud").modal('show');
    };

    //Para cuando se repite a tarea
    $scope.repetirTarea = function () {
        if ($scope.tarea_repetir !== "") {//Mostrar los radio
            angular.element("#tipoRepetirTareaNewSolicitud").show(200);
        }
        else {
            angular.element("#tipoRepetirTareaNewSolicitud").hide(200);
            angular.element("#repetirNumVecesNewSolicitud").hide(200);
            angular.element("#repetirFechaHastaNewSolicitud").hide(200);
        }
    };
    angular.element("#tipoRepetirTareaNewSolicitud").hide(0);
    angular.element("#repetirNumVecesNewSolicitud").hide(0);
    angular.element("#repetirFechaHastaNewSolicitud").hide(0);

    $scope.mostrarCampoRepetir = function () {
        if ($scope.repetirTipoFin == 1) {
            angular.element("#repetirNumVecesNewSolicitud").fadeIn(200);
            angular.element("#repetirFechaHastaNewSolicitud").fadeOut(200);
        }
        else {
            angular.element("#repetirFechaHastaNewSolicitud").fadeIn(200);
            angular.element("#repetirNumVecesNewSolicitud").fadeOut(200);
        }
    };

    angular.element("#modal-new-task-solicitud").on('hidden.bs.modal', function (e) {
        $scope.$apply(function () {
            datosInicialesNewTaskSolicitud();
        });
    });

    function datosInicialesNewTaskSolicitud() {
        $scope.idTrabajador = "";
        $scope.fechaSolicitudTarea = "";
        $scope.cliente = "";
        $scope.ubicacion = "";
        $scope.modulo = "";
        $scope.actividad = "";
        $scope.numero_horas = "";
        $scope.detalle = "";
        $scope.referencia = "";
        $scope.coordinador = "";        
        $scope.tarea_repetir = "";
        $scope.fin_semana = "";
        $scope.repetirTipoFin = "";
        $scope.numVeces = "";
        $scope.fechaHasta = "";

        angular.element("#tipoRepetirTareaNewSolicitud").hide(0);
        angular.element("#repetirNumVecesNewSolicitud").hide(0);
        angular.element("#repetirFechaHastaNewSolicitud").hide(0);
                
        angular.element("#frm-new-task-solicitud").each(function () {
            this.reset();
        });

        angular.element("#edicion-user-edicion-tarea").hide();
        angular.element("#data-user-tarea").show();
    }

    //Cargando los entregables de trabajos segun el cliente
    $scope.cargarMotivosTrabajo = function () {

        var ajaxMotivosTrabajo = $http.post("task/dar-entregables-trabajo",
                                    {
                                        idCliente: $scope.cliente
                                    });
        ajaxMotivosTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.referencias = data.datos
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });


    };

    $scope.guardarTareaSolicitud = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();        

        formData.append('idTrabajador', $scope.solicitudEscogida.idTrabajador);
        formData.append('idSolicitud', $scope.solicitudEscogida.id);
        formData.append('fecha', $scope.fechaSolicitudTarea);

        formData.append('cliente', $scope.cliente);
        formData.append('ubicacion', $scope.ubicacion);
        formData.append('modulo', $scope.modulo);
        formData.append('actividad', $scope.actividad);
        formData.append('horas', $scope.numero_horas);
        formData.append('detalle', $scope.detalle);
        formData.append('referencia', $scope.referencia);
        formData.append('coordinador', $scope.coordinador);

        formData.append('repetir', $scope.tarea_repetir);
        formData.append('finSemana', $scope.fin_semana);

        formData.append('repetirTipoFin', $scope.repetirTipoFin);
        formData.append('numVeces', $scope.numVeces);
        formData.append('fechaHasta', $scope.fechaHasta);             

        var newTask = $http.post("task/nueva-tarea-solicitud",
                                    formData,
                                    {
                                        headers: { 'Content-Type': undefined }
                                    });

        newTask.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
                angular.element("#modal-new-task-solicitud").modal('hide');
                angular.element("#modal-descripcion-solicitud").modal('hide');                
                $scope.recargarSolicitudes($scope.mostrarTodas, $scope.filtroSolicitudTarea, start, cantMostrar);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.rechazarSolicitud = function () {
        var ajaxRechazarSolicitud = $http.post("task/rechazar-tarea-solicitud", {
            idSolicitud: $scope.solicitudEscogida.id
        });
        ajaxRechazarSolicitud.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-descripcion-solicitud").modal('hide');
                $scope.recargarSolicitudes($scope.mostrarTodas, $scope.filtroSolicitudTarea, start, cantMostrar);
            }
            else{
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.aceptarSolicitud = function () {
        var ajaxAceptarSolicitud = $http.post("task/aceptar-tarea-solicitud", {
            idSolicitud: $scope.solicitudEscogida.id
        });
        ajaxAceptarSolicitud.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-descripcion-solicitud").modal('hide');
                $scope.recargarSolicitudes($scope.mostrarTodas, $scope.filtroSolicitudTarea, start, cantMostrar);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
}]);