taskApp.controller('taskController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    var taskProxie = angular.element.connection.websocket;
    angular.element.connection.hub.start();

    //Mensaje para notificaciones por falla en el envío de correos
    taskProxie.client.erroresEnvioEmail = function (msg) {
        messageDialog.show('Información', msg);
    };

    //Loading
    $scope.loadingAjax = function () {
        this.show = function (target) {
            angular.element("#loadingDiv").show();
        };
        this.hide = function (target) {
            angular.element("#loadingDiv").hide();
        };
    };
    $scope.loading = new $scope.loadingAjax();

    $scope.numeroSemanas = 1;
    var aplicarFiltro = false;
    $scope.tipoAsignacion = 1;//asignacion masiva, esta variable se utiliza como bandera para saber que tipo de asignacion,
    //es la que se va a realizar
    $scope.datosColaboradoresLugar = [];

    angular.element('#repetirFechaHasta').datepicker({
        orientation: "botton left",
        format: 'dd/mm/yyyy'
    });
    angular.element('#fecha-edit-tarea').datepicker({
        format: 'dd/mm/yyyy'
    });
    /*
    angular.element('.datepicker').datepicker({
        format: 'dd/mm/yyyy'
    });
    */
    $scope.frecuenciasRepetir = ['DIARIO', 'SEMANAL', 'MENSUAL'];

    $scope.diasActividadTarea = [];
    $scope.diasActividad = [];
    $scope.diasActividadCord = [];
    //Cargando los dias de tareas desde el negocio
    var ajaxDiasDeTareas = $http.post("task/dias-actividades-tareas", {});
    ajaxDiasDeTareas.success(function (data) {
        if (data.success === true) {
            $scope.diasActividadTarea = data.dias;
            $scope.diasActividad = data.dias;
            $scope.diasActividadCord = data.diasCord;
        }
        else {
            messageDialog.show('Información', data.msg);
        }
    });
    $scope.horas = []; for (var i = 0; i < 24; i++) { $scope.horas.push(i); }
    $scope.minutos = []; for (var i = 0; i < 60; i++) { $scope.minutos.push(i); }

    $scope.semanaProxima = function () {
        var ndias = 7;
        $scope.$parent.fechaLunes = sumaFecha(ndias, $scope.$parent.fechaLunes);
        if (!aplicarFiltro)
            actualizarDatosTarea($scope.$parent.fechaLunes);
        else
            actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
    };

    $scope.cambiarSemanas = function () {
        //$scope.actualizarListaDias($scope.$parent.fechaLunes, $scope.fechaHoy);
        if (!aplicarFiltro)
            actualizarDatosTarea($scope.$parent.fechaLunes);
        else
            actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
    }

    $scope.semanaAtras = function () {
        var ndias = 7 * (-1);
        $scope.$parent.fechaLunes = sumaFecha(ndias, $scope.$parent.fechaLunes);
        if (!aplicarFiltro)
            actualizarDatosTarea($scope.$parent.fechaLunes);
        else
            actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
    };

    $scope.diaHoy = function () {
        $scope.$parent.fechaLunes = $scope.esteLunes;
        if (!aplicarFiltro)
            actualizarDatosTarea($scope.$parent.fechaLunes);
        else
            actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
    };

    $scope.fechaTarea = function () {
        var hoy = new Date();
        $scope.fechaDiaTarea = hoy.getDate() + "/" + (hoy.getMonth() + 1) + "/" + hoy.getFullYear();
    }

    //angular.element("#span-terminar-tarea").on("click", function (e) {
    //    $scope.fechaTarea = angular.element("#span-terminar-tarea")[0];
    //});

    angular.element('#control-filtrar .datepicker-filtro').datepicker({
        format: 'dd/mm/yyyy'
    })
        .on('changeDate', function (e) {
            var sFecha = angular.element('#control-filtrar .datepicker-filtro').val();
            var aFecha = sFecha.split('/');
            var fecha = aFecha[2] + '/' + aFecha[1] + '/' + aFecha[0];
            fecha = new Date(fecha);
            var diaSemana = fecha.getDay();
            var ndias = -6;//Por si es domingo
            if (diaSemana !== 0)//Si no es domingo
                var ndias = 1 - diaSemana;

            $scope.$parent.fechaLunes = sumaFecha(ndias, sFecha);
            if (!aplicarFiltro)
                actualizarDatosTarea($scope.$parent.fechaLunes);
            else
                actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
        });

    //Anular Tarea
    angular.element(document).on("click", ".btn-anulartask", function () {
        waitingDialog.show('Anulando la tarea...', { dialogSize: 'sm', progressType: 'success' });
        var id = parseInt(angular.element(this).attr('data-id-tarea'));
        var anular = $http.post("task/anular-tarea",
            { idTarea: id });
        anular.success(function (data) {
            setTimeout(function () {
                waitingDialog.hide();
            }, 200)
            if (data.success === true) {
                if (data.tareaSeriada === true) {
                    $scope.wind3Opciones.show("Confirmación de Anulación",
                        data.msg,
                        "Anular solo esta tarea",
                        "Anular toda la serie",
                        function () {
                            $scope.wind3Opciones.hide();
                            waitingDialog.show('Anulando las tareas...', { dialogSize: 'sm', progressType: 'success' });
                            var anular1 = $http.post("task/anular-tarea",
                                {
                                    idTarea: id,
                                    tipoAnulacion: 1
                                });
                            anular1.success(function (data) {
                                waitingDialog.hide();
                                if (data.success === true) {
                                    angular.element.connection.hub.start().done(function () {
                                        taskProxie.server.actualizarTareas();
                                    });
                                    //if (!aplicarFiltro)
                                    //    actualizarDatosTarea($scope.$parent.fechaLunes);
                                    //else
                                    //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));                                                               
                                }
                                else {
                                    messageDialog.show('Información', data.msg);
                                }
                            });
                        },
                        function () {
                            $scope.wind3Opciones.hide();
                            waitingDialog.show('Anulando las tareas...', { dialogSize: 'sm', progressType: 'success' });
                            var anular2 = $http.post("task/anular-tarea",
                                {
                                    idTarea: id,
                                    tipoAnulacion: 2
                                });
                            anular2.success(function (data) {
                                waitingDialog.hide();
                                if (data.success === true) {
                                    angular.element.connection.hub.start().done(function () {
                                        taskProxie.server.actualizarTareas();
                                    });
                                    //if (!aplicarFiltro)
                                    //    actualizarDatosTarea($scope.$parent.fechaLunes);
                                    //else
                                    //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));                                                               
                                }
                                else {
                                    messageDialog.show('Información', data.msg);
                                }
                            });
                        }
                    );
                }
                else {
                    angular.element.connection.hub.start().done(function () {
                        taskProxie.server.actualizarTareas();
                    });
                    //if (!aplicarFiltro)
                    //    actualizarDatosTarea($scope.$parent.fechaLunes);
                    //else
                    //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));                    
                }

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Editar tarea, solo muestra el PopUp con los datos de la nueva tarea
    angular.element(document).on("click", ".btn-edittask", function () {
        var id = parseInt(angular.element(this).attr('data-id-tarea'));

        angular.element("#title-new-task").html("Editar Tarea");

        var editar = $http.post("task/datos-tarea",
            { idTarea: id });
        editar.success(function (data) {
            if (data.success === true) {
                $scope.idTarea = id,
                    $scope.idTrabajador = data.tarea.idTrabajador;
                $scope.nombreColaborador = data.tarea.nombre;
                $scope.sede = data.tarea.sede;
                $scope.fecha_tarea = data.fecha;

                $scope.cliente = data.tarea.cliente;
                $scope.ubicacion = data.tarea.ubicacion;
                $scope.modulo = data.tarea.modulo;
                $scope.actividad = data.tarea.actividad;
                //$('[ng-model="numero_horas"]').val(data.horas),
                $scope.numero_horas_horas = data.horas;
                $scope.numero_horas_minutos = parseInt(data.minutos);

                $scope.numero_horas_estimadas = data.tarea.horasEstimadas;
                $scope.numero_minutos_estimados = parseInt(data.tarea.minutosEstimados);

                angular.element("#numberMinutos").val(parseInt(data.minutos));
                angular.element("#numberMinutosEstimados").val(parseInt(data.tarea.minutosEstimados));

                $scope.coordinador = data.tarea.coordinador;
                $scope.detalle = data.tarea.detalle;
                $scope.nuevaTareaExtraordinaria = data.extraordinaria;
                $scope.ticketTarea = data.ticketTarea != 0 ? data.ticketTarea : '';
                $scope.edicionTareaExtraordinaria = true;

                $scope.cargarMotivosTrabajo(function () {
                    setTimeout(function () {
                        angular.element("#referencia-trabajo-task").val(data.tarea.referencia);
                        $scope.referencia = data.tarea.referencia;
                    }, 500);
                });

                $scope.verificadorTarea = data.tarea.verificador;

                $scope.idColaborador = data.tarea.idTrabajador;
                angular.element('[ng-model="idColaborador"]').val(data.tarea.idTrabajador);

                angular.element("#fecha-edit-tarea").datepicker('update', $scope.fecha_tarea);
                $scope.fechaEditTarea = $scope.fecha_tarea;

                angular.element("#edicion-user-edicion-tarea").show();
                //angular.element("#data-user-tarea").hide();

                angular.element("#modalNewTask").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Cambiar de estados las tareas
    angular.element(document).on("click", ".btn-tarea-desarrollo", function () {
        waitingDialog.show('Cambiando el estado de la tarea...', { dialogSize: 'sm', progressType: 'success' });
        var id = parseInt(angular.element(this).attr('data-id-tarea'));
        cambiarEstadoDeTarea(id, 2);
    });

    angular.element(document).on("click", ".btn-tarea-finish", function () {
        var obj = this;
        $scope.$apply(function () {
            $scope.idTareaTerminar = parseInt(angular.element(obj).attr('data-id-tarea'));
        });

        var ajaxActividadRealizadaTarea = $http.post("user/dar-actividades-segun-actividad-tarea",
            {
                idTarea: $scope.idTareaTerminar
            });
        ajaxActividadRealizadaTarea.success(function (data) {
            if (data.success === true) {
                $scope.actividadesTarea = data.actividadesTarea;
                angular.element("#modal-final-tarea").modal("show");
            }
            else {
                angular.element("#modal-final-tarea").modal("hide");
                messageDialog.show('Información', data.msg);
            }
        });

        var ajaxActividades = $http.post("task/dar-actividades-tarea",
            {
                idTarea: $scope.idTareaTerminar
            });
        ajaxActividades.success(function (data) {
            if (data.success === true) {
                $scope.actividadesRealizadas = data.actividadesTarea;
                $scope.tiempoUtilizado = data.totalHoras;
                if (data.tareaPropia == false) {
                    $scope.diasActividadTarea = $scope.diasActividadCord;
                }
                else {
                    $scope.diasActividadTarea = $scope.diasActividad;
                }
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    angular.element(document).on("click", ".btn-tarea-pausa", function () {
        waitingDialog.show('Cambiando el estado de la tarea', { dialogSize: 'sm', progressType: 'success' });
        var obj = this;
        $scope.$apply(function () {
            $scope.idTareaTerminar = parseInt(angular.element(obj).attr('data-id-tarea'));
        });
        cambiarEstadoDeTarea($scope.idTareaTerminar, 5);
    });

    angular.element(document).on("click", ".btn-tarea-asignada", function () {
        waitingDialog.show('Cambiando el estado de la tarea...', { dialogSize: 'sm', progressType: 'success' });
        var obj = this;
        $scope.$apply(function () {
            $scope.idTareaTerminar = parseInt(angular.element(obj).attr('data-id-tarea'));
        });
        cambiarEstadoDeTarea($scope.idTareaTerminar, 1);
    });

    angular.element(document).on("click", ".btn-tarea-preasignada", function () {
        waitingDialog.show('Cambiando el estado de la tarea...', { dialogSize: 'sm', progressType: 'success' });
        var obj = this;
        $scope.$apply(function () {
            $scope.idTareaTerminar = parseInt(angular.element(obj).attr('data-id-tarea'));
        });
        cambiarEstadoDeTarea($scope.idTareaTerminar, 6);
    });

    //Anular el permiso
    angular.element(document).on("click", ".btn-anularPermiso", function () {
        waitingDialog.show('Anulando el permiso...', { dialogSize: 'sm', progressType: 'success' });
        var id = parseInt(angular.element(this).attr('data-id-tarea'));
        var anular = $http.post("task/anular-permiso",
            { id: id });
        anular.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Repetir verticalmente la tarea a otros colaboradores
    angular.element(document).on("click", ".btn-tarea-repetir-vertical", function () {
        var obj = this;

        if ($scope.datosColaboradoresLugar.length === 0) {
            angular.forEach($scope.trabajadores, function (value, key) {
                $scope.datosColaboradoresLugar.push(
                    {
                        id: value.trab.id,
                        email: value.trab.email,
                        sede: value.trab.sede
                    }
                );
            });
        }

        $scope.$apply(function () {
            $scope.idTareaRepetirVertical = parseInt(angular.element(obj).attr('data-id-tarea'));
        });
        angular.element("#modal-asign-vertical-colab").modal("show");
    });

    $scope.marcarTodosRepeticionVertical = function () {
        if ($scope.checkRepetirVertical) {
            angular.element(".trab-check-rep-task").prop('checked', true);
        }
        else {
            angular.element(".trab-check-rep-task").prop('checked', false);
        }
    };

    $scope.repetirTareaColaboradoresSeleccionados = function () {
        waitingDialog.show('Adicionando las tareas...', { dialogSize: 'sm', progressType: 'success' });
        var idColaboradoresRepetirTarea = [];

        var colabSeleccionados = angular.element(".trab-check-rep-task:checked");
        angular.forEach(colabSeleccionados, function (value, key) {
            idColaboradoresRepetirTarea.push(angular.element(value).val());
        });

        var repetirVerticalTask = $http.post("task/repetir-tarea-vertical",
            {
                idTarea: $scope.idTareaRepetirVertical,
                idColaboradores: angular.toJson(idColaboradoresRepetirTarea)
            }
        );

        repetirVerticalTask.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
                angular.element("#modal-asign-vertical-colab").modal("hide");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Poner tarea como compensatoria
    angular.element(document).on("click", ".btn-tarea-compensatoria", function () {
        var obj = this;
        var idTareaCompensatoria = angular.element(obj).attr('data-id-tarea');

        var diaCompensatorio = $http.post("task/establecer-quitar-tarea-compensatoria",
            {
                idTarea: idTareaCompensatoria
            }
        );
        diaCompensatorio.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Adiciona una actividad a una tarea
    var dtpkHoraInicioActividad = angular.element("#dtpk-hora-inicio-actividad-tarea").datetimepicker({
        format: 'HH:mm',
        locale: 'es'
    });
    angular.element(document).on('dp.update', "#dtpk-hora-inicio-actividad-tarea", function (e) {
        console.log(e);
        $scope.horaInicioActividadTarea = e.date._i;
    });
    var dtpkHoraInicioActividad = angular.element("#dtpk-horas-tarea").datetimepicker({
        format: 'HH:mm',
        locale: 'es'
    });
    //angular.element(document).on('dp.update', "#dtpk-horas-tarea", function (e) {
    //    console.log(e);
    //    $scope.numero_horas = e.date._i;
    //});
    var dtpkHoraFinActividad = angular.element("#dtpk-hora-fin-actividad-tarea").datetimepicker({
        format: 'HH:mm',
        locale: 'es'
    });
    angular.element(document).on('dp.update', "#dtpk-hora-fin-actividad-tarea", function (e) {
        $scope.horaFinActividadTarea = e.date._i;
    });
    var dtpkCambioHoraInicioActividad = angular.element("#dtpk-cambio-hora-inicio-actividad-tarea").datetimepicker({
        format: 'HH:mm',
        locale: 'es'
    });
    angular.element(document).on('dp.update', "#dtpk-cambio-hora-inicio-actividad-tarea", function (e) {
        console.log(e);
        $scope.cambioHoraInicioActividadTarea = e.date._i;
    });
    var dtpkCambioHoraFinActividad = angular.element("#dtpk-cambio-hora-fin-actividad-tarea").datetimepicker({
        format: 'HH:mm',
        locale: 'es'
    });
    angular.element(document).on('dp.update', "#dtpk-cambio-hora-fin-actividad-tarea", function (e) {
        $scope.cambioHoraFinActividadTarea = e.date._i;
    });

    $scope.adicionarActividadTarea = function () {
        waitingDialog.show('Adicionando Actividad...', { dialogSize: 'sm', progressType: 'success' });

        var adicionar = $http.post("task/adicionar-actividad-tarea",
            {
                idTarea: $scope.idTareaTerminar,
                tipoTarea: $scope.tipoActividadTarea,
                fecha: $scope.diaActividadTara,
                horaInicio: $('[ng-model="horaInicioActividadTarea"]').val(),
                horaFin: $('[ng-model="horaFinActividadTarea"]').val()
            });
        adicionar.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.tipoActividadTarea = "",
                    $scope.diaActividadTara = "",
                    $scope.horaInicioActividadTarea = "",
                    $scope.minutoInicioActividadTarea = "",
                    $scope.horaFinActividadTarea = "",
                    $scope.minutoFinActividadTarea = ""

                $scope.actividadesRealizadas = data.actividadesTarea;
                $scope.tiempoUtilizado = data.totalHoras;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.idActividadTarea = "";
    $scope.editarHoraActividad = function (id) {
        $scope.idActividadTarea = id;
        $scope.cambioHoraInicioActividadTarea = this.actividad.horaInicio;
        $scope.cambioHoraFinActividadTarea = this.actividad.horaFin;
        angular.element("#modal-cambio-horario-actividad").modal('show');
    };

    $scope.actualizarHorasTarea = function () {
        waitingDialog.show('Actualizando Horarios de Actividad...', { dialogSize: 'sm', progressType: 'success' });

        var actualizar = $http.post("task/actualizar-hora-actividad-tarea",
            {
                idActividadTarea: $scope.idActividadTarea,
                horaInicio: $('[ng-model="cambioHoraInicioActividadTarea"]').val(),
                horaFin: $('[ng-model="cambioHoraFinActividadTarea"]').val()
            });
        actualizar.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.horaInicioActividadTarea = "";
                $scope.horaFinActividadTarea = "";

                $scope.actividadesRealizadas = data.actividadesTarea;
                $scope.tiempoUtilizado = data.totalHoras;
                angular.element("#modal-cambio-horario-actividad").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.eliminarActividad = function (id) {
        waitingDialog.show('Eliminando la actividad de la tarea...', { dialogSize: 'sm', progressType: 'success' });

        var ajaxActividad = $http.post("task/eliminar-actividad-tarea",
            {
                idActividadTarea: id
            });
        ajaxActividad.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                var ajaxActividades = $http.post("task/dar-actividades-tarea",
                    {
                        idTarea: $scope.idTareaTerminar
                    });
                ajaxActividades.success(function (data) {
                    if (data.success === true) {
                        $scope.actividadesRealizadas = data.actividadesTarea;
                        $scope.tiempoUtilizado = data.totalHoras;
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.finalizarTarea = function () {
        waitingDialog.show('Cambiando el estado de la tarea...', { dialogSize: 'sm', progressType: 'success' });

        var finalizar = $http.post("task/actualizar-tarea-usuario",
            { idTarea: $scope.idTareaTerminar, estado: 3 });
        finalizar.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-final-tarea").modal("hide");
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
                $scope.tiempoUtilizado = '';
                //if (!aplicarFiltro)
                //    actualizarDatosTarea($scope.$parent.fechaLunes);
                //else
                //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));                
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    function cambiarEstadoDeTarea(id, estado) {
        var cambiar = $http.post("task/actualizar-tarea-usuario",
            { idTarea: id, estado: estado });
        cambiar.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
                //if (!aplicarFiltro)
                //    actualizarDatosTarea($scope.$parent.fechaLunes);
                //else
                //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
                //messageDialog.show('Información', data.msg);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }

    //Gestión de comentarios a las actividades
    $scope.verComentarios = function (id) {
        $scope.idActividadTarea = id;
        angular.element("#modal-comentario-actividad").modal("show");
        var verComentarios = $http.post("task/dar-comentarios",
            {
                idActividad: $scope.idActividadTarea
            });
        verComentarios.success(function (data) {
            if (data.success === true) {
                $scope.comentarios = data.comentarios;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    angular.element("#modal-comentario-actividad").on('hidden.bs.modal', function (e) {
        $scope.$apply(function () {
            $scope.comentarios = [];
        });
    });

    $scope.importanciaActividad = "Normal";

    $scope.adicionarComentario = function () {
        waitingDialog.show('Adicionando comentario...', { dialogSize: 'sm', progressType: 'success' });
        var addComentario = $http.post("task/adicionar-comentario",
            {
                idActividad: $scope.idActividadTarea,
                descripcion: $scope.descripcionComentario,
                importancia: $scope.importanciaActividad
            });
        addComentario.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.descripcionComentario = "";
                $scope.comentarios = data.comentarios;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.eliminarComentario = function (id) {
        waitingDialog.show('Eliminando comentario...', { dialogSize: 'sm', progressType: 'success' });
        var removeComentario = $http.post("task/eliminar-comentario",
            {
                idComentario: id
            });
        removeComentario.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.comentarios = data.comentarios;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Prepara el windows para una nueva tarea
    $scope.addNewTask = function () {
        $scope.idTrabajador = this.trab.trab.id;
        $scope.nombreColaborador = this.trab.trab.nombre;
        $scope.sede = this.trab.trab.sede;
        if (this.trab.trab.idSede == 4)
            this.trab.trab.idSede = 5;
        $scope.ubicacion = this.trab.trab.idSede;

        angular.element('[ng-model="ubicacion"]').val(this.trab.trab.idSede);

        $scope.fecha_tarea = this.diasCalendar[this.$index].date;
        $scope.extraordinaria = false;
        $scope.numero_horas_horas = 0;
        $scope.numero_horas_minutos = 0;
        $scope.numero_horas_estimadas = 0;
        $scope.numero_minutos_estimados = 0;
        $scope.nuevaTareaExtraordinaria = true;
        $scope.edicionTareaExtraordinaria = false;
        $scope.ticketTarea = "";
        angular.element("#modalNewTask").modal('show');
    };

    //Para cuando se repite a tarea
    $scope.repetirTarea = function () {
        if ($scope.tarea_repetir !== "") {//Mostrar los radio
            angular.element("#tipoRepetirTarea").show(200);
        }
        else {
            angular.element("#tipoRepetirTarea").hide(200);
            angular.element("#repetirNumVeces").hide(200);
            angular.element("#repetirFechaHasta").hide(200);
        }
    };

    $scope.mostrarCampoRepetir = function () {
        if ($scope.repetirTipoFin == 1) {
            angular.element("#repetirNumVeces").show(200);
            angular.element("#repetirFechaHasta").hide(200);
        }
        else {
            angular.element("#repetirFechaHasta").show(200);
            angular.element("#repetirNumVeces").hide(200);
        }
    };

    angular.element("#modalNewTask").on('hidden.bs.modal', function (e) {
        datosInicialesNewTask();
    });

    function datosInicialesNewTask() {
        $scope.idTrabajador = "";
        $scope.fecha_tarea = "";
        $scope.cliente = "";
        $scope.ubicacion = "";
        $scope.modulo = "";
        $scope.actividad = "";
        $scope.numero_horas_horas = 0;
        $scope.numero_horas_minutos = 0;
        $scope.numero_horas_estimadas = 0;
        $scope.numero_minutos_estimados = 0;
        $scope.detalle = "";
        $scope.ticketTarea = "";
        $scope.referencia = "";
        $scope.coordinador = "";
        $scope.idTarea = "";
        $scope.verificadorTarea = "";
        $scope.tarea_repetir = "";
        $scope.numVeces = "";
        $scope.fechaHasta = "";

        angular.element("#tipoRepetirTarea").hide();
        angular.element("#repetirNumVeces").hide();
        angular.element("#repetirFechaHasta").hide();

        angular.element("#title-new-task").html("Nueva Tarea");
        angular.element("#frm-newTask").each(function () {
            this.reset();
        });

        angular.element("#edicion-user-edicion-tarea").hide();
        $scope.nuevaTareaExtraordinaria = true;
        $scope.edicionTareaExtraordinaria = false;
        //angular.element("#data-user-tarea").show();
    }

    //Cargando los entregables de trabajos segun el cliente
    $scope.cargarMotivosTrabajo = function (onSuccess) {
        var ajaxMotivosTrabajo = $http.post("task/dar-entregables-trabajo",
            {
                idCliente: $scope.cliente
            });
        ajaxMotivosTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.referencias = data.datos;
                if ((onSuccess) && (typeof onSuccess === 'function')) {
                    onSuccess();
                }
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Insertar una nueva tarea
    angular.element("#frm-newTask").submit(function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();

        //Verificando cambios de colaborrador y fecha
        if ($scope.idTarea !== undefined && $scope.idTarea > 0) {
            if ($scope.idTrabajador !== $scope.idColaborador) {
                formData.append('idTrabajador', $scope.idColaborador);
            }
            else {
                formData.append('idTrabajador', $scope.idTrabajador);
            }

            if ($scope.fechaEditTarea !== $scope.fecha_tarea) {
                formData.append('fecha', $scope.fechaEditTarea);
            }
            else {
                formData.append('fecha', $scope.fecha_tarea);
            }
        }
        else {
            formData.append('idTrabajador', $scope.idTrabajador);
            formData.append('fecha', $scope.fecha_tarea);
        }

        formData.append('cliente', $scope.cliente);
        formData.append('ubicacion', $scope.ubicacion);
        formData.append('modulo', $scope.modulo);
        formData.append('actividad', $scope.actividad);
        formData.append('horas', $scope.numero_horas_horas);
        formData.append('minutos', $scope.numero_horas_minutos);
        formData.append('horasEstimadas', $scope.numero_horas_estimadas);
        formData.append('minutosEstimados', $scope.numero_minutos_estimados);
        const detalleString = JSON.stringify($scope.detalle);
        let detalleNew = detalleString.replace(/</g, '%3C').replace(/>/g, '%3E');
        formData.append('detalle', detalleNew);
        console.log(detalleString);


        formData.append('referencia', $scope.referencia);

        formData.append('coordinador', $scope.coordinador);

        formData.append('repetir', $scope.tarea_repetir);
        formData.append('finSemana', $scope.fin_semana);

        formData.append('repetirTipoFin', $scope.repetirTipoFin);
        formData.append('numVeces', $scope.numVeces);
        formData.append('fechaHasta', $scope.fechaHasta);
        formData.append('extraordinaria', $scope.extraordinaria);
        formData.append('ticketTarea', $scope.ticketTarea != '' ? $scope.ticketTarea : 0);

        formData.append('idTarea', $scope.idTarea);
        formData.append('verificador', $scope.verificadorTarea);

        var newTask = $http.post("task/nueva-tarea",
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
                angular.element("#modalNewTask").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        $http.post("task/email-nueva-tarea",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });

        return false;
    });

    //Actualizando las tareas de un containerTarea
    $scope.tareasTD = [];
    taskProxie.client.actualizarDiaTareaColaborador = function (objsArray) {
        objsArray = JSON.parse(objsArray);
        //objsArray = [{ fecha: fecha, idColaborador: idColaborador }];        
        $scope.$apply(function () {
            $scope.actualizarTDTarea(objsArray);
        });
    };
    $scope.actualizarTDTarea = function (objsArray) {
        var obj = objsArray[0];
        var fechaTarea = obj.fecha;
        var idColaborador = obj.idColaborador;

        var encontrado = false;
        var pos = 0;
        var numeroSemanas = angular.element("#selectSemanas").val();
        var cantDias = 7 * numeroSemanas;
        while (!encontrado && pos < cantDias) {
            if (fechaTarea === sumaFecha(pos, $scope.$parent.fechaLunes)) {
                encontrado = true;
            }
            else {
                pos++;
            }
        }
        if (encontrado) {//La tarea está en el Rango, dibujar

            //Ubicando el contenedor
            var selector = '.td-colaborador [data-id="' + idColaborador + '"]';
            var tdColaborador = angular.element(selector)[0];
            var tdHermanos = angular.element(angular.element(tdColaborador).parent()).siblings();
            var contenedorTarea = tdHermanos[pos];

            if (contenedorTarea !== undefined) {
                var jsonFTD = "";
                if (aplicarFiltro)
                    jsonFTD = angular.toJson(filtroTareas);

                //Llamar a la funcion de datos del dia tarea                
                var datosTareaDia = $http.post("task/datos-dia-tarea",
                    { fecha: fechaTarea, idColaborador: idColaborador, json: jsonFTD }
                );

                datosTareaDia.success(function (data) {
                    if (data.success === true) {
                        angular.element(contenedorTarea).find(".bottonAddTask").siblings().remove();
                        $scope.tareasTD = data.tareasTD;
                        setTimeout(function () {
                            angular.element(contenedorTarea).append(angular.element("#div-actualizacion-td-tareas").html());

                            if (objsArray.length > 1) {
                                objsArray.splice(0, 1);
                                $scope.actualizarTDTarea(objsArray);
                            }
                            else {
                                ajustarTablaTrabajadores();
                            }

                        }, 50);
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });
            }
        }
    };

    //Variable del filtro
    var filtroTareas = {
        colaboradores: [],
        clientes: [],
        estadoTarea: [],
        lugarTarea: [],
        modulo: [],
        sede: [],
        departamento: []
    };

    var objetoFiltro = 0;

    //Filtrar las tareas
    $scope.filtrarTareas = function () {
        angular.element("#modalNewFilter").modal('show');
        //$scope.filtroText = angular.toJson(filtroTareas);        
    };

    $scope.anadirFiltroColaboradores = function () {
        objetoFiltro = 0;
        angular.element(".panel-valores-filtro").html("");
        var cantTrabajadores = $scope.trabajadores.length;
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: trab.trab.id, class: "filtroData", "data-text": $scope.mySplitEmail(trab.trab.email, 0) });
            if (buscarEnArreglo(filtroTareas.colaboradores, trab.trab.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + $scope.mySplitEmail(trab.trab.email, 0));

            angular.element(".panel-valores-filtro").append(divDato);
        }
    };

    $scope.anadirFiltroClientes = function () {
        objetoFiltro = 1;
        angular.element(".panel-valores-filtro").html("");

        var cantTrabajadores = $scope.trabajadores.length;
        var arregloClientes = [];
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];

            var tareasPorDia = trab.tareasPorDia;
            var cantDias = tareasPorDia.length;
            for (var j = 0; j < cantDias; j++) {
                var tareas = tareasPorDia[j].tareas;
                var cantTareas = tareas.length;
                for (var k = 0; k < cantTareas; k++) {
                    var tarea = tareas[k];
                    var clienteTarea = {
                        id: tarea.idCliente,
                        text: tarea.dCliente
                    };
                    if (buscarEnArreglo(arregloClientes, clienteTarea.id) === false && clienteTarea.text !== 'PERMISO') {
                        arregloClientes.push(clienteTarea);
                    }
                }
            }
        }

        arregloClientes.sort((cliente1, cliente2) => cliente1.text.localeCompare(cliente2.text));

        for (var i = 0; i < arregloClientes.length; i++) {
            var cliente = arregloClientes[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: cliente.id, class: "filtroData", "data-text": cliente.text });
            if (buscarEnArreglo(filtroTareas.clientes, cliente.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + cliente.text);

            angular.element(".panel-valores-filtro").append(divDato);
        }

    };

    $scope.anadirFiltroEstadoTarea = function () {
        objetoFiltro = 2;
        angular.element(".panel-valores-filtro").html("");

        var cantTrabajadores = $scope.trabajadores.length;
        var arregloEstados = [];
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];

            var tareasPorDia = trab.tareasPorDia;
            var cantDias = tareasPorDia.length;
            for (var j = 0; j < cantDias; j++) {
                var tareas = tareasPorDia[j].tareas;
                var cantTareas = tareas.length;
                for (var k = 0; k < cantTareas; k++) {
                    var tarea = tareas[k];
                    var estadoTarea = {
                        id: tarea.idEstado,
                        text: tarea.estado
                    };
                    if (buscarEnArreglo(arregloEstados, estadoTarea.id) === false && estadoTarea.text !== '') {
                        arregloEstados.push(estadoTarea);
                    }
                }
            }
        }

        for (var i = 0; i < arregloEstados.length; i++) {
            var estado = arregloEstados[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: estado.id, class: "filtroData", "data-text": estado.text });
            if (buscarEnArreglo(filtroTareas.estadoTarea, estado.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + estado.text);

            angular.element(".panel-valores-filtro").append(divDato);
        }

    };

    $scope.anadirFiltroLugarTarea = function () {
        objetoFiltro = 3;
        angular.element(".panel-valores-filtro").html("");

        var cantTrabajadores = $scope.trabajadores.length;
        var arregloLugares = [];
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];

            var tareasPorDia = trab.tareasPorDia;
            var cantDias = tareasPorDia.length;
            for (var j = 0; j < cantDias; j++) {
                var tareas = tareasPorDia[j].tareas;
                var cantTareas = tareas.length;
                for (var k = 0; k < cantTareas; k++) {
                    var tarea = tareas[k];
                    var lugarTarea = {
                        id: tarea.idLugar,
                        text: tarea.dLugar
                    };
                    if (buscarEnArreglo(arregloLugares, lugarTarea.id) === false && lugarTarea.text !== '') {
                        arregloLugares.push(lugarTarea);
                    }
                }
            }
        }

        for (var i = 0; i < arregloLugares.length; i++) {
            var lugar = arregloLugares[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: lugar.id, class: "filtroData", "data-text": lugar.text });
            if (buscarEnArreglo(filtroTareas.lugarTarea, lugar.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + lugar.text);

            angular.element(".panel-valores-filtro").append(divDato);
        }
    };

    $scope.anadirFiltroModulo = function () {
        objetoFiltro = 4;
        angular.element(".panel-valores-filtro").html("");

        var cantTrabajadores = $scope.trabajadores.length;
        var arregloModulos = [];
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];

            var tareasPorDia = trab.tareasPorDia;
            var cantDias = tareasPorDia.length;
            for (var j = 0; j < cantDias; j++) {
                var tareas = tareasPorDia[j].tareas;
                var cantTareas = tareas.length;
                for (var k = 0; k < cantTareas; k++) {
                    var tarea = tareas[k];
                    var moduloTarea = {
                        id: tarea.idModulo,
                        text: tarea.dModulo
                    };
                    if (buscarEnArreglo(arregloModulos, moduloTarea.id) === false && moduloTarea.text !== '') {
                        arregloModulos.push(moduloTarea);
                    }
                }
            }
        }

        arregloModulos.sort((m1, m2) => m1.text.localeCompare(m2.text));

        for (var i = 0; i < arregloModulos.length; i++) {
            var modulo = arregloModulos[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: modulo.id, class: "filtroData", "data-text": modulo.text });
            if (buscarEnArreglo(filtroTareas.modulo, modulo.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + modulo.text);

            angular.element(".panel-valores-filtro").append(divDato);
        }
    };

    $scope.anadirFiltroSede = function () {
        objetoFiltro = 5;
        angular.element(".panel-valores-filtro").html("");

        var cantTrabajadores = $scope.trabajadores.length;
        var arregloSedes = [];
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];
            var datosTrab = trab.trab;
            var sede = {
                id: datosTrab.idSede,
                text: datosTrab.dSede
            };
            if (buscarEnArreglo(arregloSedes, sede.id) === false) {
                arregloSedes.push(sede);
            }
        }

        for (var i = 0; i < arregloSedes.length; i++) {
            var sede = arregloSedes[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: sede.id, class: "filtroData", "data-text": sede.text });
            if (buscarEnArreglo(filtroTareas.sede, sede.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + sede.text);

            angular.element(".panel-valores-filtro").append(divDato);
        }
    };

    $scope.anadirFiltroDepartamento = function () {
        objetoFiltro = 6;
        angular.element(".panel-valores-filtro").html("");

        var cantTrabajadores = $scope.trabajadores.length;
        var arregloDepartamentos = [];
        for (var i = 0; i < cantTrabajadores; i++) {
            var trab = $scope.trabajadores[i];

            var tareasPorDia = trab.tareasPorDia;
            var cantDias = tareasPorDia.length;
            for (var j = 0; j < cantDias; j++) {
                var tareas = tareasPorDia[j].tareas;
                var cantTareas = tareas.length;
                for (var k = 0; k < cantTareas; k++) {
                    var tarea = tareas[k];
                    var departamento = {
                        id: tarea.idDepartamento,
                        text: tarea.dDepartamento
                    };
                    if (buscarEnArreglo(arregloDepartamentos, departamento.id) === false && departamento.text !== '') {
                        arregloDepartamentos.push(departamento);
                    }
                }
            }
        }

        for (var i = 0; i < arregloDepartamentos.length; i++) {
            var departamento = arregloDepartamentos[i];
            var divDato = document.createElement("div");
            angular.element(divDato).attr({ class: "divDatosFiltro" });

            var newCheck = document.createElement("input");
            angular.element(newCheck).attr({ type: "checkbox", value: departamento.id, class: "filtroData", "data-text": departamento.text });
            if (buscarEnArreglo(filtroTareas.departamento, departamento.id)) {
                angular.element(newCheck).attr({ checked: true });
            }

            angular.element(divDato).append(newCheck);
            angular.element(divDato).append(":" + departamento.text);

            angular.element(".panel-valores-filtro").append(divDato);
        }

    };

    angular.element("#modalNewFilter").on('click', '.filtroData', function () {
        actualizarDatosFiltro();
    });

    function actualizarDatosFiltro() {
        var tipoFiltro = null;
        switch (objetoFiltro) {
            case 0:
                tipoFiltro = "colaboradores";
                break;
            case 1:
                tipoFiltro = "clientes";
                break;
            case 2:
                tipoFiltro = "estadoTarea";
                break;
            case 3:
                tipoFiltro = "lugarTarea";
                break;
            case 4:
                tipoFiltro = "modulo";
                break;
            case 5:
                tipoFiltro = "sede";
                break;
            case 6:
                tipoFiltro = "departamento";
                break;
        }

        filtroTareas[tipoFiltro] = [];
        angular.element('#modalNewFilter .filtroData:checked').each(function () {
            var element = {
                id: angular.element(this).val(),
                text: angular.element(this).attr("data-text")
            };
            filtroTareas[tipoFiltro].push(element);
        });
        ponerDatosFiltro();
    }

    $scope.marcarTodosValores = function () {
        angular.element('.filtroData:not(checked)').prop("checked", true);
        actualizarDatosFiltro();
    };

    $scope.desmarcarTodosValores = function () {
        angular.element('.filtroData:checked').prop("checked", false);
        actualizarDatosFiltro();
    };

    $scope.aplicarFiltroTareas = function () {
        aplicarFiltro = true;
        actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
        angular.element("#modalNewFilter").modal('hide');
    };

    $scope.limpiarFiltroTareas = function () {
        aplicarFiltro = false;
        $scope.$parent.coordinados = false;
        actualizarDatosTarea($scope.$parent.fechaLunes);
        filtroTareas = {
            colaboradores: [],
            clientes: [],
            estadoTarea: [],
            lugarTarea: [],
            modulo: [],
            sede: [],
            departamento: []
        };
        angular.element(".panel-valores-filtro").html("Por favor seleccione un campo de filtrado");
        angular.element(".panel-json-filtro").html("Filtro");
    };

    function ponerDatosFiltro() {
        var html = "Filtro:<br/>Colaboradores: ";
        for (var i = 0; i < filtroTareas.colaboradores.length; i++) {
            html = html + filtroTareas.colaboradores[i].text;
            if (i < filtroTareas.colaboradores.length - 1) {
                html = html + ", ";
            }
        }

        html = html + "<br/>Clientes:";

        for (var i = 0; i < filtroTareas.clientes.length; i++) {
            html = html + filtroTareas.clientes[i].text;
            if (i < filtroTareas.clientes.length - 1) {
                html = html + ", ";
            }
        }

        html = html + "<br/>Estado de tarea:";

        for (var i = 0; i < filtroTareas.estadoTarea.length; i++) {
            html = html + filtroTareas.estadoTarea[i].text;
            if (i < filtroTareas.estadoTarea.length - 1) {
                html = html + ", ";
            }
        }

        html = html + "<br/>Lugar de tarea:";

        for (var i = 0; i < filtroTareas.lugarTarea.length; i++) {
            html = html + filtroTareas.lugarTarea[i].text;
            if (i < filtroTareas.lugarTarea.length - 1) {
                html = html + ", ";
            }
        }

        html = html + "<br/>Módulo:";

        for (var i = 0; i < filtroTareas.modulo.length; i++) {
            html = html + filtroTareas.modulo[i].text;
            if (i < filtroTareas.modulo.length - 1) {
                html = html + ", ";
            }
        }

        html = html + "<br/>Sede:";

        for (var i = 0; i < filtroTareas.sede.length; i++) {
            html = html + filtroTareas.sede[i].text;
            if (i < filtroTareas.sede.length - 1) {
                html = html + ", ";
            }
        }

        html = html + "<br/>Departamento:";

        for (var i = 0; i < filtroTareas.departamento.length; i++) {
            html = html + filtroTareas.departamento[i].text;
            if (i < filtroTareas.departamento.length - 1) {
                html = html + ", ";
            }
        }

        angular.element(".panel-json-filtro").html(html);
    }

    //Funcion auxilar
    function buscarEnArreglo(arreglo, clave) {
        var cant = arreglo.length;
        for (var i = 0; i < cant; i++) {
            var item = arreglo[i];
            if (item.id == clave) {
                return true;
            }
        }
        return false;
    }

    //Filtrar mostrar solo los coordinados
    $scope.verCoordinadosPersona = function (idTrabajador) {
        filtroTareas.colaboradores = [];
        if ($scope.$parent.coordinados === false) {
            aplicarFiltro = true;
            $scope.$parent.coordinados = true;
            var buscarCoordinados = $http.post("task/coordinados-persona",
                {
                    idTrabajador: idTrabajador,
                    fechaLunes: $scope.$parent.fechaLunes,
                    semanas: $scope.numeroSemanas
                });
            buscarCoordinados.success(function (data) {
                if (data.success === true) {
                    angular.element.each(data.dataFilter, function (key, obj) {
                        var element = {
                            id: obj.id.toString(),
                            text: obj.text
                        };
                        filtroTareas.colaboradores.push(element);
                    });
                    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
        else {
            $scope.$parent.coordinados = false;
            aplicarFiltro = false;
            actualizarDatosTarea($scope.$parent.fechaLunes);
        }
    };
    $scope.estiloSegunEstadoCoordinacion = function (id, index) {
        if ($scope.$parent.coordinados === false)
            return Array('btn-ver-coordinaciones-persona right glyphicon glyphicon-hand-right', 'Ver coordinados por el colaborador');
        //return $sce.trustAsHtml('btn-ver-coordinaciones-persona right glyphicon glyphicon-hand-right" ng-click="verCoordinadosPersona('+id+')" title="Ver coordinados por el colaborador"></span>');
        else if (index === 0)
            return Array('btn-ver-coordinaciones-persona right glyphicon glyphicon-ban-circle', 'Quitar filtro de coordinados por el colaborador');
        //return $sce.trustAsHtml('<span class="btn-ver-coordinaciones-persona right glyphicon glyphicon-ban-circle" ng-click="quitarCoordinadosPersona()" title="Quitar filtro de coordinados por el colaborador"></span>');
        else
            return '';
    };

    $scope.mySplitEmail = function (string, nb) {
        $scope.array = string.split('@');
        return $scope.result = $scope.array[nb];
    }

    //Envío de las asignaciones a todos, mostrar el modal
    $scope.modalAsignacionesSemana = function () {
        $scope.tipoAsignacion = 1;//Asignacion masiva
        $scope.textoEnviarEmail = "Usted ha seleccionado enviar las asignaciones de tareas en la semana a partir del lunes";
        angular.element("#modalEnvioAsig").modal("show");
    };

    //Envío de las asignaciones a todos, mostrar el modal
    $scope.modalSolicitudTFS = function () {
        $scope.colaboradorTFS = "";
        $scope.clienteTFS = "";

        var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});
        ajaxColaboradores.success(function (data) {
            if (data.success === true) {
                $scope.colaboradoresTFS = data.colaboradores;
            }
            else {
                messageDialog.show('Información', "Error al cargar los colaboradores");
            }
        });

        //Cargando los clientes
        var ajaxClientes = $http.post("catalogos/clientes", {});
        ajaxClientes.success(function (data) {
            if (data.success === true) {
                $scope.clientesTFS = data.clientes;
            }
            else {
                messageDialog.show('Información', "Error en los datos de acceso");
            }
        });


        angular.element("#modalAsignacionesTFS").modal("show");
    };

    //Enviar las asignaciones segun el caso
    $scope.enviarCorreoTFS = function () {
        waitingDialog.show('Enviando correo...', { dialogSize: 'sm', progressType: 'success' });
        angular.element("#modalAsignacionesTFS").modal("hide");
        var ajax = $http.post("task/solicitud-acceso-tfs",
            {
                idCliente: $scope.clienteTFS,
                idColaborador: $scope.colaboradorTFS
            });
        ajax.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.correoEnviado = true;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Cuando se oculta el modal
    angular.element("#modalEnvioAsig").on('hidden.bs.modal', function (e) {
        angular.element("#desplegar-cuadro-emails-copia").html('CC:...');
        angular.element("#cuadro-emails-copia").fadeOut(0);
        angular.element(".div-email-add").remove();
    });

    //Envío de las asignaciones a una sola persona, mostrar el modal
    $scope.enviarAsignacionesPersona = function () {
        $scope.idTrabajador = this.trab.trab.id;
        $scope.tipoAsignacion = 2;//Asignacion personal
        $scope.textoEnviarEmail = "Usted ha seleccionado enviar las asignaciones de tareas para " + this.trab.trab.email.toLowerCase() + " a partir del";
        angular.element("#modalEnvioAsig").modal("show");
    };

    //Enviar las asignaciones segun el caso
    $scope.enviarAsignacionesSemana = function () {
        var emailAdd = [];
        var divEmailAdd = angular.element(".div-email-add");
        angular.element.each(divEmailAdd, function (key, divValue) {
            emailAdd.push(angular.element(divValue).children().first().text());
        });

        if ($scope.tipoAsignacion === 1) {//Se envia a todos
            waitingDialog.show('Enviando correos...', { dialogSize: 'sm', progressType: 'success' });
            angular.element.connection.hub.start().done(function () {
                taskProxie.server.asignacionesSemana($scope.passEmail, $scope.$parent.fechaLunes, angular.toJson(emailAdd));//Llamando al web socket
            });
        }
        else {//Se envia al usuario seleccionado
            waitingDialog.show('Enviando correo...', { dialogSize: 'sm', progressType: 'success' });
            angular.element("#modalEnvioAsig").modal("hide");
            var newWorker = $http.post("task/asignaciones-persona",
                {
                    idTrabajador: $scope.idTrabajador,
                    password: $scope.passEmail,
                    fechaLunes: $scope.$parent.fechaLunes,
                    jsonCC: angular.toJson(emailAdd)
                });
            newWorker.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    messageDialog.show('Información', data.msg);
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

    taskProxie.client.actualizarCorreoEnviado = function (data) {
        datos = JSON.parse(data);
        if (datos.terminado === false) {
            waitingDialog.setText(datos.msg);
        }
        else {
            waitingDialog.hide();
            if (datos.error === false) {
                messageDialog.show('Información', datos.msg);
            }
            else {
                messageDialog.show('Error', datos.msg);
            }
        }
        angular.element("#modalEnvioAsig").modal("hide");
    };

    //ADICIONANDO LAS COPIAS DE EMAIL
    $scope.adicionarEmail = function (event) {
        if (event.keyCode === 13) {
            var regex = /[\w-\.]{2,}@([\w-]{2,}\.)*([\w-]{2,}\.)[\w-]{2,4}/;
            var email = $scope.emailCopiaAdd;
            if (regex.test(email)) {
                var divNewEmail = angular.element('<div/>');
                angular.element(divNewEmail).html('<span>' + $scope.emailCopiaAdd + '</span>' + ' <span class="remove-email-copia" title="Eliminar">x</span>');
                angular.element(divNewEmail).addClass('div-email-add');
                angular.element(divNewEmail).insertBefore('#adicionar-email-copia');
                $scope.emailCopiaAdd = "";
            }
            else {
                messageDialog.show('Información', "La direccion de correo proporcionada no es válida");
            }
        }
    };
    //eliminando las copias de email.
    angular.element("#frm-envio-email").on("click", ".remove-email-copia", function () {
        angular.element(this).parent().remove();
    });
    angular.element("#cuadro-emails-copia").fadeOut(0);//Al inicio no se ve
    angular.element("#desplegar-cuadro-emails-copia").click(function () {
        angular.element(this).html('<b>CC:</b>');
        angular.element("#cuadro-emails-copia").fadeIn(300);
    });

    //DRAG AND DROP DE FUNCIONES
    //cuando empieza a moverse
    $scope.idTaskDrag = 0;
    $scope.fechaTareaDrag = '';
    $scope.idColaboradorDrag = 0;
    $scope.tipoTareaDrag = '';
    $scope.startDrag = function (event, ui, idTask, idColaboradorStart, tipo) {
        $scope.idTaskDrag = idTask;
        var divTarea = angular.element(event.target).parent();
        $scope.fechaTareaDrag = this.diasCalendar[angular.element(divTarea).index() - 1].date;
        $scope.idColaboradorDrag = idColaboradorStart;

        $scope.tipoTareaDrag = tipo;
    };

    //Cuando se mueve
    $scope.actualizarTabla = function () {
        ajustarTablaTrabajadores();
    };

    //Cuando suelta una tarea
    $scope.pegarTarea = function (event, ui, idColaboradorDrop) {
        $scope.fechaTareaDrop = this.diasCalendar[this.$index].date;

        if ($scope.tipoTareaDrag === 't') {//Una tarea
            if (($scope.fechaTareaDrag !== $scope.fechaTareaDrop) || idColaboradorDrop !== $scope.idColaboradorDrag) {//Cambiar de fecha

                var moveTask = $http.post("task/mover-tarea",
                    {
                        idTarea: $scope.idTaskDrag,
                        idColaborador: idColaboradorDrop,
                        fecha: $scope.fechaTareaDrop
                    });

                moveTask.success(function (data) {
                    waitingDialog.hide();
                    if (data.success === true) {
                        angular.element.connection.hub.start().done(function () {
                            taskProxie.server.actualizarTareas();
                        });
                        //messageDialog.show('Información', data.msg);
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                    //if (!aplicarFiltro)
                    //    actualizarDatosTarea($scope.$parent.fechaLunes);
                    //else
                    //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
                });

            }
            else {//En la misma Fecha
                var contenedor = event.target;
                var hijos = angular.element(contenedor).children();

                var tareasHijas = [];
                for (var i = 1; i < hijos.length; i++) {
                    var tipo = angular.element(hijos[i]).attr('data-tipo');
                    if (tipo === 't')//Si es una tarea la entro al arreglo
                        tareasHijas.push(hijos[i]);
                }

                for (var i = 0; i < tareasHijas.length - 1; i++) {
                    for (var j = i + 1; j < tareasHijas.length; j++) {
                        var aux = null;
                        var posHI = angular.element(tareasHijas[i]).position().top;
                        var posHJ = angular.element(tareasHijas[j]).position().top;
                        if (posHJ < posHI) {
                            aux = tareasHijas[i];
                            tareasHijas[i] = tareasHijas[j];
                            tareasHijas[j] = aux;
                        }
                    }
                }

                var ordenTareas = [];
                //Ya tareas hijas es un array ordenado
                for (var i = 0; i < tareasHijas.length; i++) {
                    ordenTareas.push(
                        angular.element(tareasHijas[i]).attr('data-id-tarea')
                    );
                }

                //ReOrdenar en la misma fecha.
                var orderTasks = $http.post("task/ordenar-tareas",
                    {
                        idOrden: angular.toJson(ordenTareas)
                    });

                orderTasks.success(function (data) {
                    waitingDialog.hide();
                    if (data.success === true) {
                        angular.element.connection.hub.start().done(function () {
                            taskProxie.server.actualizarTareas();
                        });
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }

                    //if (!aplicarFiltro)
                    //    actualizarDatosTarea($scope.$parent.fechaLunes);
                    //else
                    //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
                    //messageDialog.show('Información', data.msg);
                });
            }
        }
        else if ($scope.tipoTareaDrag === 'p') {//Un permiso
            var ajaxMoverPermiso = $http.post("task/mover-permiso",
                {
                    idPermiso: $scope.idTaskDrag,
                    idColaborador: idColaboradorDrop,
                    fecha: $scope.fechaTareaDrop
                });

            ajaxMoverPermiso.success(function (data) {
                if (data.success === true) {
                    angular.element.connection.hub.start().done(function () {
                        taskProxie.server.actualizarTareas();
                    });
                }
                else {
                    messageDialog.show('Información', data.msg);
                }

                //if (!aplicarFiltro)
                //    actualizarDatosTarea($scope.$parent.fechaLunes);
                //else
                //    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
            });
        }
    };

    //Cuando tiene encima una tarea
    $scope.tareaEncima = function (event, ui) {
        var contenedor = event.target;
        angular.element(contenedor).addClass('tarea-ensima');
    };

    //cuando la tarea se va
    $scope.tareaFuera = function (event, ui) {
        var contenedor = event.target;
        angular.element(contenedor).removeClass('tarea-ensima');
    };

    //Click derecho sobre los cuadros de las fechas    
    $scope.fechaFeriado = "";
    //Ocultando el Menú del clic derecho
    angular.element("#menu-opciones-feriado").hide(0);//Inicialmente se oculta
    //Click Derecho en un cuadro de tarea
    $scope.mostarOpcionesFeriados = function (e, fecha) {
        $scope.fechaFeriado = fecha;
        var opcionesMenu = angular.element("#menu-opciones-feriado li");
        //angular.element(opcionesMenu).hide();
        angular.element(opcionesMenu).show();

        //angular.element(opcionesMenu).show();
        angular.element("#menu-opciones-feriado").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    };
    //cuando hagamos click, el menú desaparecerá
    $(document).click(function (e) {
        if (e.button == 0) {
            $("#menu-opciones-feriado").css("display", "none");
        }
    });
    //si pulsamos escape, el menú desaparecerá
    $(document).keydown(function (e) {
        if (e.keyCode == 27) {
            $("#menu-opciones-feriado").css("display", "none");
        }
    });
    //Toogle de los feriados
    $scope.gestionarFeriado = function () {
        var ajaxFeriado = $http.post("task/gestionar-feriado",
            {
                fecha: $scope.fechaFeriado
            });
        ajaxFeriado.success(function (data) {
            if (data.success === true) {
                if (!aplicarFiltro)
                    actualizarDatosTarea($scope.$parent.fechaLunes);
                else
                    actualizarDatosTarea($scope.$parent.fechaLunes, angular.toJson(filtroTareas));
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //GESTION DE LAS INCIDENCIAS
    $scope.idIncidenciaColaborador = 0;
    $scope.verificadorIncidencia = 0;
    $scope.addNewIncidence = function () {
        $scope.idTrabajador = this.trab.trab.id;
        $scope.nombreColaboradorIncidencia = this.trab.trab.nombre;
        //$scope.sede = this.trab.trab.sede;
        $scope.fechaIncidencia = this.diasCalendar[this.$index].date;
        $scope.cargarIncidenciasColaborador($scope.idTrabajador, $scope.fechaIncidencia);
        angular.element("#modal-incidencias-colaboradores").modal("show");
    };
    $scope.cambiarValorImpacto = function () {
        /*Esta funcion carga el impacto segun el tipo de error*/
        var ajaxImplicacionError = $http.post("catalogos/implicacionerror-x-tipoerror", {
            idTipoError: $scope.tipoErrorIncidencia
        });
        ajaxImplicacionError.success(function (data) {
            if (data.success === true) {
                if (data.implicacionError === null) {
                    messageDialog.show('Información', "No se encontró el nivel del error correspondiente, se debe asignar niveles de implicacion a los tipos de errores en el admin.");
                }
                else {
                    $scope.implicacionIncidencia = data.implicacionError.id;
                }
            }
            else {
                messageDialog.show('Información', "Error en los datos de acceso");
            }
        });
    };
    $scope.adicionarIncidenciaColaborador = function () {
        waitingDialog.show('Adicionando Incidencia...', { dialogSize: 'sm', progressType: 'success' });
        var insertarIncidencia = $http.post("task/editar-incidencia-colaborador",
            {
                idColaborador: $scope.idTrabajador,
                fecha: $scope.fechaIncidencia,
                cliente: $scope.clienteIncidencia,
                tipoError: $scope.tipoErrorIncidencia,
                implicacion: $scope.implicacionIncidencia,
                hecho: $scope.hechoIncidencia,
                justificacion: $scope.justificacionIncidencia,
                idIncidencia: $scope.idIncidenciaColaborador,
                verificador: $scope.verificadorIncidencia
            });

        insertarIncidencia.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.cargarIncidenciasColaborador($scope.idTrabajador, $scope.fechaIncidencia);
                $scope.clienteIncidencia = "";
                $scope.tipoErrorIncidencia = "";
                $scope.implicacionIncidencia = "";
                $scope.hechoIncidencia = "";
                $scope.justificacionIncidencia = "";
                $scope.idIncidenciaColaborador = 0;
                $scope.verificadorIncidencia = 0;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarIncidenciasColaborador = function (idColaborador, fecha) {
        var cargarIncidencias = $http.post("task/dar-incidencia-colaborador",
            {
                idColaborador: idColaborador,
                fecha: fecha
            });
        cargarIncidencias.success(function (data) {
            if (data.success === true) {
                $scope.incidenciasColaborador = data.incidencias
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.editarIncidenciaColaborador = function () {
        $scope.clienteIncidencia = this.incid.idCliente;
        $scope.tipoErrorIncidencia = this.incid.idTipoError;
        $scope.implicacionIncidencia = this.incid.idImplicacion;
        $scope.hechoIncidencia = this.incid.hecho;
        $scope.justificacionIncidencia = this.incid.justificacion;
        $scope.idIncidenciaColaborador = this.incid.idIncidencia;
        $scope.verificadorIncidencia = this.incid.verificador;
    };
    $scope.eliminarIncidenciaColaborador = function (idIncidencia) {
        waitingDialog.show('Eliminando Incidencia...', { dialogSize: 'sm', progressType: 'success' });
        var eliminarIncidencia = $http.post("task/eliminar-incidencia-colaborador",
            {
                idIncidencia: idIncidencia
            });

        eliminarIncidencia.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.cargarIncidenciasColaborador($scope.idTrabajador, $scope.fechaIncidencia);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.cargarTecnologias = function () {
        var ajaxTecnologias = $http.post("catalogos/tecnologias/")

        ajaxTecnologias.success(function (data) {
            if (data.success === true) {
                setTimeout(function () {
                    $scope.tecnologias = data.tecnologias;
                }, 200);
            } else {
                messageDialog.show('Información', data.msg);
                return false
            }
        });
    };


    //VISTA DE NIVEL DE COLABORADOR EN LOS MODULOS
    $scope.modalNivelConocimientoColaboradores = function () {
        $scope.cargarConocimientosColaboradores(function () {

            $scope.cargarTecnologias();
            angular.element("#modal-task-nivel-colaboradores").modal("show");
        });
    };
    $scope.cargarConocimientosColaboradores = function (fn) {
        var cargarConocimientos = $http.post("task/dar-conocimientos-colaboradores",
            {
                modulo: $scope.moduloNivelColaborador,
                tecnologia: $scope.tecnologiaNivelColaborador,
                nivel: $scope.nivelNivelCompetencia
            });
        cargarConocimientos.success(function (data) {
            if (data.success === true) {
                $scope.filasConocimientos = data.filasConocimientos;

                setTimeout(function () {
                    var tdEstrellas = angular.element(".competencias-nivel-star");
                    angular.forEach(tdEstrellas, function (value, key) {
                        var cantEstrellas = angular.element(value).attr('data-nivel');

                        if (cantEstrellas > 3) {
                            cantEstrellas = 3
                        }

                        for (var i = 0; i < cantEstrellas; i++) {
                            angular.element(value).append('<span class="glyphicon glyphicon-star"></span>');
                        }
                    });

                }, 200);

                fn();
            }
            else {
                messageDialog.show('Información', data.msg);
                return false
            }
        });
    };
    $scope.filtrarConocimientosColaboradores = function () {
        var cargarConocimientos = $http.post("task/dar-conocimientos-colaboradores",
            {
                modulo: $scope.moduloNivelColaborador,
                tecnologia: $scope.tecnologiaNivelColaborador,
                nivel: $scope.nivelNivelCompetencia
            });
        cargarConocimientos.success(function (data) {
            if (data.success === true) {
                $scope.filasConocimientos = data.filasConocimientos;

                setTimeout(function () {
                    var tdEstrellas = angular.element(".competencias-nivel-star");
                    angular.forEach(tdEstrellas, function (value, key) {
                        var cantEstrellas = angular.element(value).attr('data-nivel');

                        if (cantEstrellas > 3) {
                            cantEstrellas = 3
                        }

                        for (var i = 0; i < cantEstrellas; i++) {
                            angular.element(value).append('<span class="glyphicon glyphicon-star"></span>');
                        }
                    });

                }, 200);
            }
            else {
                messageDialog.show('Información', data.msg);
                return false
            }
        });
        //$scope.cargarConocimientosColaboradores();
    };

    $scope.cargarExcel = function () {
        angular.element("#modalUploadFile").modal("show");
    }

    $scope.subirArchivo = function () {

        $scope.loading.show();

        var formData = new FormData();
        var file = document.getElementById("input-excel").files[0];
        formData.append("file", file);

        $.ajax({
            url: "task/subir-excel",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            async: false,
            success: function (data) {
                if (data.success == true) {
                    $scope.leerExcel();
                } else {
                    $scope.loading.hide();
                    messageDialog.show('Información', data.msg);
                }
            },
            error: function (data) {
                $scope.loading.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    }

    $scope.leerExcel = function () {

        var leerExcel = $http.post("task/leer-excel",
            {

            });
        leerExcel.success(function (data) {
            if (data.success === true) {
                $scope.loading.hide();
                messageDialog.show('Información', data.msg);

                angular.element("#modalUploadFile").modal("hide");

                angular.element("#panel_home").addClass('invisible');
                angular.element("#panel_tasks").addClass('invisible');
                angular.element("#panel_solicitudes").addClass('invisible');
                angular.element("#panel_consolidacion_tareas").addClass('invisible');
                angular.element("#panel_actas_contratos").addClass('invisible');
                angular.element("#panel_disponibilidad_recursos").addClass('invisible');
                angular.element("#panel_incidencias_colaboradores").addClass('invisible');
                angular.element("#panel_tasks").removeClass('invisible');
                $scope.funcionalidad = 'TAREAS';

                //Cargando el ultimo lunes
                var ajaxUltimoLunes = $http.post("task/ultimo-lunes", {});
                ajaxUltimoLunes.success(function (data) {
                    if (data.success === true) {
                        $scope.fechaLunes = data.lunes;
                        $scope.esteLunes = data.lunes;
                        $scope.fechaHoy = data.hoy;
                        actualizarDatosTarea($scope.fechaLunes);
                        $('#control-filtrar .datepicker-filtro').datepicker('update', $scope.fechaHoy);
                    }
                    else {
                        messageDialog.show('Información', "Error en los datos de acceso");
                    }
                });
            }
            else {
                $scope.loading.hide();
                messageDialog.show('Información', data.msg);
                return false;
            }
        });

    }

}]);

taskApp.directive('ngRightClick', function ($parse) {
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