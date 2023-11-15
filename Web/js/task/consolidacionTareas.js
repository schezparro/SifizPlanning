taskApp.controller('consolidacionController', ['$scope', '$http', '$sce', '$window', function ($scope, $http, $sce, $window) {
    $scope.varSemanas = 0;
    $scope.idMotivoTrabajo = 0;
    $scope.idTipoMotivoTrabajo = 0;
    $scope.nombreMotivoTrabajo = "";
    $scope.idEntregable = 0;
    $scope.adjuntos = [];

    angular.element("#fecha-inicio-motivo-trabajo").datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element("#fecha-fin-motivo-trabajo").datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element("#fecha-produccion-entregable").datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element("#fecha-inicio-entregable-motivo-tarea").datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element("#fecha-fin-entregable-motivo-tarea").datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element("#fecha-desde-proximo-entregable").datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element("#fecha-hasta-proximo-entregable").datepicker({
        format: 'dd/mm/yyyy'
    });

    $scope.porciento = [];
    for (var i = 0; i <= 100; i++) {
        $scope.porciento.push(i);
    }

    $scope.horasTrabajo = [1, 2, 3, 4, 5, 6, 7, 8];

    //Navegación en la consolidación de las tareas
    $scope.semanaAtrasConsolidacion = function () {
        $scope.varSemanas--;
        cargarConsolidaciones($scope.varSemanas);
    };
    $scope.semanaAlanteConsolidacion = function () {
        $scope.varSemanas++;
        cargarConsolidaciones($scope.varSemanas);
    };
    $scope.semanaActualConsolidacion = function () {
        $scope.varSemanas = 0;
        cargarConsolidaciones($scope.varSemanas);
    };
    $scope.filtrarConsolidacion = function () {
        cargarConsolidaciones($scope.varSemanas, $scope.filtroConsolidacion);
    };

    //Ocultando el Menú del clic derecho
    angular.element("#menu-opciones").hide(0);//Inicialmente se oculta
    //Click Derecho en adicionar trabajo
    angular.element("#panel-motivos-trabajo").on("contextmenu", ".tipo-motivo-trabajo .heading", function (e) {
        $scope.idTipoMotivoTrabajo = angular.element(this).attr("data-tipo-trabajo-id");
        $scope.nombreTipoMotivoTrabajo = angular.element(this).attr("data-tipo");

        var opcionesMenu = angular.element("#menu-opciones li");
        angular.element(opcionesMenu).hide();
        angular.element(opcionesMenu[0]).show();
        angular.element("#menu-opciones").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    });
    //Click Derecho en Trabajos
    angular.element("#panel-motivos-trabajo").on("contextmenu", ".tipo-motivo-trabajo .motivos-trabajos", function (e) {

        $scope.nombreMotivoTrabajo = angular.element(this).attr("data-nombre-motivo-trabajo");
        $scope.codigoMotivoTrabajo = angular.element(this).attr("data-codigo-motivo-trabajo");
        $scope.idMotivoTrabajoEntregable = angular.element(this).attr("data-id-motivo-trabajo");

        var opcionesMenu = angular.element("#menu-opciones li");
        angular.element(opcionesMenu).hide();

        var numero = angular.element(opcionesMenu).length;

        //Si hay 8 elementos en la vista del menu clic derecho, entonces el usuario tiene el rol Actas y se cargan 4 opciones al hacer clic derecho 
        angular.element(opcionesMenu[1]).show();
        angular.element(opcionesMenu[2]).show();
        angular.element(opcionesMenu[3]).show();
        angular.element(opcionesMenu[4]).show();

        var ajaxEsContrato = $http.post("task/comprobar-contrato",
            {
                codigoContrato: $scope.codigoMotivoTrabajo
            });

        ajaxEsContrato.success(function (data) {
            if (data.success === true) {
                if (data.esProyecto === true) {
                    angular.element(opcionesMenu[8]).show();
                }
            }
        });

        angular.element("#menu-opciones").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    });
    //Click Derecho en entregables
    angular.element("#panel-motivos-trabajo").on("contextmenu", ".entregable-trabajo", function (e) {
        $scope.nombreEntregable = angular.element(this).attr("data-nombre-entregable");
        $scope.idEntregableMotivoTrabajo = angular.element(this).attr("data-id-entregable");

        var opcionesMenu = angular.element("#menu-opciones li");
        angular.element(opcionesMenu).hide();

        var numero = angular.element(opcionesMenu).length;

        if (numero == 7) {
            angular.element(opcionesMenu[4]).show();
            angular.element(opcionesMenu[5]).show();
        } else {
            angular.element(opcionesMenu[5]).show();
            angular.element(opcionesMenu[6]).show();
        }

        angular.element("#menu-opciones").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    });
    //Click Derecho sobre las semanas
    angular.element("#panel-motivos-trabajo").on("contextmenu", ".contenedor-tarea-consolidada", function (e) {
        var parent = angular.element(this).parent();
        var hijos = parent.children('.contenedor-tarea-consolidada');
        var pos = 0;
        var cant = hijos.length;
        while (pos < cant) {
            if (hijos[pos] === this)
                break;
            pos++;
        }

        var primerHijo = parent.children().first();
        $scope.idEntregable = primerHijo.attr("data-id-entregable");
        $scope.semanaRelativa = $scope.varSemanas + pos;

        var opcionesMenu = angular.element("#menu-opciones li");
        angular.element(opcionesMenu).hide();

        angular.element(opcionesMenu[7]).show();
        angular.element("#menu-opciones").css({ 'display': 'block', 'left': e.pageX, 'top': e.pageY });
        return false;
    });


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

    //Adicionar Acta
    $scope.adicionarActa = function () {
        localStorage.setItem("codCont", $scope.codigoMotivoTrabajo);
        $window.open(
            'consultas#actas',
            '_blank' // <- This is what makes it open in a new window.
        );
        //$window.location.href = '/consultas#actas';
    };

    //Adicionando un motivo de trabajo
    $scope.adicionarMotivoTrabajo = function () {
        $scope.estadoContratoMotivoTrabajo = "";
        $scope.tipoPlazo = "";

        $scope.adjuntos = '';
        angular.element("#frm-new-motivo-trabajo .file-adj-contrato").remove();
        var listaBotones = angular.element("#barra-botones-file");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);

        angular.element("#modal-add-trabajos").modal("show");
    };
    $scope.guardarMotivoTrabajo = function () {
        waitingDialog.show('Guardando el Trabajo...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();

        var files = new Array();
        angular.element.each(angular.element('#frm-new-motivo-trabajo').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });

        formData.append('tipo', $scope.idTipoMotivoTrabajo);
        formData.append('codigo', $scope.codigoMotivoTrabajo);
        formData.append('cliente', $scope.clienteMotivoTrabajo);
        formData.append('nombre', $scope.nombreMotivoTrabajo);
        formData.append('descripcion', $scope.detalleMotivoTrabajo);
        formData.append('fechaInicio', $scope.fechaInicioMotivoTrabajo);
        formData.append('fechaFin', $scope.fechaFinMotivoTrabajo);
        formData.append('estadoContrato', $scope.estadoContratoMotivoTrabajo);
        formData.append('colaborador', $scope.colaboradorContrato);
        formData.append('tipoPlazo', $scope.tipoPlazo);
        formData.append('diasPlazo', $scope.diasPlazo);
        formData.append('horasMes', $scope.horasMes != undefined ? $scope.horasMes : -1);
        formData.append('nombreTipoMotivoTrabajo', $scope.nombreTipoMotivoTrabajo);
        formData.append('idMotivoTrabajo', $scope.idMotivoTrabajo);
        formData.append('numeroVerificador', $scope.verificadorMotivoTrabajo);

        var ajaxMotivosTrabajo = $http.post("task/guardar-motivos-trabajo",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        ajaxMotivosTrabajo.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-add-trabajos").modal("hide");
                cargarConsolidaciones();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-add-trabajos").on('hidden.bs.modal', function (e) {
        limpiarMotivosTrabajo();
        angular.element("#title-modal-add-trabajos").html("Nuevo Trabajo");
    });
    function limpiarMotivosTrabajo() {
        $scope.idTipoMotivoTrabajo = 0;
        $scope.codigoMotivoTrabajo = "";
        $scope.clienteMotivoTrabajo = 0;
        $scope.estadoContratoMotivoTrabajo = 0;
        $scope.colaborador = 0;
        $scope.tipoPlazo = 0;
        $scope.diasPlazo = "";
        $scope.nombreMotivoTrabajo = "";
        $scope.detalleMotivoTrabajo = "";
        $scope.fechaInicioMotivoTrabajo = "";
        $scope.fechaFinMotivoTrabajo = "";
        $scope.horasMes = "";

        $scope.idMotivoTrabajo = 0;
        $scope.verificadorMotivoTrabajo = 0;

        angular.element("#frm-new-motivo-trabajo").each(function () {
            this.reset();
        });
    }

    //Editando Motivo de Trabajo
    $scope.editarMotivoTrabajo = function () {
        angular.element("#title-modal-add-trabajos").html("Editar Trabajo");
        angular.element("#frm-new-motivo-trabajo .file-adj-contrato").remove();

        var listaBotones = angular.element("#barra-botones-file");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);

        var ajaxDatosMotivoTrabajo = $http.post("task/datos-motivo-trabajo",
            {
                idMotivoTrabajo: $scope.idMotivoTrabajoEntregable
            });
        ajaxDatosMotivoTrabajo.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-add-trabajos").modal("show");
                $scope.idMotivoTrabajo = data.datos.id;
                $scope.idTipoMotivoTrabajo = data.datos.idTipo;
                $scope.nombreTipoMotivoTrabajo = data.datos.tipo != "PENDIENTES" ? data.datos.tipo : "SOPORTE Y MANTENIMIENTO";
                $scope.codigoMotivoTrabajo = data.datos.codigo;
                $scope.clienteMotivoTrabajo = data.datos.cliente;
                $scope.estadoContratoMotivoTrabajo = data.datos.estadoContrato != null ? data.datos.estadoContrato : "";
                $scope.colaboradorContrato = data.datos.colaborador != null ? data.datos.colaborador : "";
                $scope.tipoPlazo = data.datos.tipoPlazo != null ? data.datos.tipoPlazo : "";
                $scope.diasPlazo = data.datos.diasPlazo;
                $scope.nombreMotivoTrabajo = data.datos.nombre;
                $scope.fechaInicioMotivoTrabajo = data.datos.fechaInicio;
                $scope.fechaFinMotivoTrabajo = data.datos.fechaFin;
                $scope.detalleMotivoTrabajo = data.datos.detalle;
                $scope.verificadorMotivoTrabajo = data.datos.verificador;
                $scope.horasMes = data.datos.horasMes;
                $scope.adjuntos = data.datos.adjuntos;

                angular.element("#nombre-motivo-trabajo").val(data.datos.nombre);
                angular.element("#codigo-motivo-trabajo").val(data.datos.codigo);

                $('#fecha-inicio-motivo-trabajo').datepicker('update', data.datos.fechaInicio);
                $('#fecha-fin-motivo-trabajo').datepicker('update', data.datos.fechaFin);

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Eliminar Motivo de Trabajo
    $scope.eliminarMotivoTrabajo = function () {
        $scope.wind2Opciones.show('Eliminar Trabajo', 'Usted está a punto de eliminar un trabajo, ¿desea continuar?.',
            'Sí', function () {
                $scope.wind2Opciones.hide();
                var ajaxEliminarMotivoTrabajo = $http.post("task/eliminar-motivo-trabajo",
                    {
                        idMotivoTrabajo: $scope.idMotivoTrabajoEntregable
                    });
                ajaxEliminarMotivoTrabajo.success(function (data) {
                    if (data.success === true) {
                        cargarConsolidaciones();
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });
            });
    };

    $scope.multiselectInit = function () {
        $scope.$broadcast('someEvent');
    }
    //Adicionando varios entregables
    $scope.adicionarEntregables = function () {
        $scope.multiselectInit();
        var lista = angular.copy($scope.modulos);
        var entregablesMotivo = $http.post("task/dar-entregables-para-actas",
            {
                numeroContrato: $scope.codigoMotivoTrabajo
            });
        entregablesMotivo.success(function (data) {
            if (data.success === true) {
                $scope.listaEntregables = data.datos;
                lista = lista.filter((el) => {
                    return $scope.listaEntregables.every((f) => {
                        return f.descripcion !== el.nombre;
                    });
                });
                $scope.listaModulos = lista;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
        //$scope.listaModulos = lista;
        angular.element("#modal-adicionar-entregables-modulo").modal("show");
    };

    $scope.guardarListaEntregables = function () {
        waitingDialog.show('Guardando lista de Entregables...', { dialogSize: 'sm', progressType: 'success' });
        var entregablesTrabajo = $http.post("task/guardar-lista-entregables",
            {
                motivo: $scope.codigoMotivoTrabajo,
                entregables: angular.toJson($scope.modulosEntregables)
            });
        entregablesTrabajo.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-adicionar-entregables-modulo").modal("hide");
                cargarConsolidaciones();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Adicionando un entregable
    $scope.adicionarEntregable = function () {
        $scope.colaboradorMotivoTrabajo = "";
        angular.element("#modal-add-entregables-trabajos").modal("show");
    };
    $scope.guardarEntregableMotivoTrabajo = function () {
        waitingDialog.show('Guardando el Entregable...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxMotivosTrabajo = $http.post("task/guardar-entregable-motivo-trabajo",
            {
                motivo: $scope.idMotivoTrabajoEntregable,
                nombre: $scope.nombreEntregableMotivoTrabajo,
                descripcion: $scope.detalleEntregableMotivoTrabajo,
                fechaInicio: $scope.fechaInicioEntregableMotivoTrabajo,
                fechaFin: $scope.fechaFinEntregableMotivoTrabajo,
                fechaProduccionEntregable: $scope.fechaProduccionEntregable,
                avance: $scope.avanceEntregableMotivoTrabajo,
                colaboradorMotivoTrabajo: $scope.colaboradorMotivoTrabajo,

                idEntregable: $scope.idEntregableMotivoTrabajo,
                numeroVerificador: $scope.verificadorEntregableMotivoTrabajo,
            });
        ajaxMotivosTrabajo.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-add-entregables-trabajos").modal("hide");
                cargarConsolidaciones();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-add-entregables-trabajos").on('hidden.bs.modal', function (e) {
        limpiarEntregableMotivosTrabajo();
        angular.element("#title-modal-add-entregables-trabajos").html("Nuevo Entregable");
    });
    function limpiarEntregableMotivosTrabajo() {
        $scope.idEntregableMotivoTrabajo = 0;
        $scope.idMotivoTrabajoEntregable = 0;
        $scope.nombreMotivoTrabajo = "";

        $scope.nombreEntregableMotivoTrabajo = "";
        $scope.fechaInicioEntregableMotivoTrabajo = "";
        $scope.fechaFinEntregableMotivoTrabajo = "";
        $scope.detalleEntregableMotivoTrabajo = "";
        $scope.avanceEntregableMotivoTrabajo = 0;
        $scope.colaboradorMotivoTrabajo = 0;

        $scope.verificadorEntregableMotivoTrabajo = 0;

        angular.element("#frm-new-entregable-trabajo").each(function () {
            this.reset();
        });
    }

    //Editando los Entregables de Trabajo
    $scope.editarEntregable = function () {
        angular.element("#title-modal-add-entregables-trabajos").html("Editar Entregable");
        var ajaxDatosEntregableTrabajo = $http.post("task/datos-entregable-trabajo",
            {
                idEntregableTrabajo: $scope.idEntregableMotivoTrabajo
            });
        ajaxDatosEntregableTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.idEntregableMotivoTrabajo = data.datos.id;
                $scope.idMotivoTrabajoEntregable = data.datos.idMotivo;
                $scope.nombreMotivoTrabajo = data.datos.motivo;

                $scope.nombreEntregableMotivoTrabajo = data.datos.nombre;
                $scope.fechaInicioEntregableMotivoTrabajo = data.datos.fechaInicio;
                $scope.fechaFinEntregableMotivoTrabajo = data.datos.fechaFin;
                $scope.detalleEntregableMotivoTrabajo = data.datos.detalle;
                $scope.avanceEntregableMotivoTrabajo = data.datos.avance;
                $scope.colaboradorMotivoTrabajo = data.datos.colaboradorMotivoTrabajo != null ? data.datos.colaboradorMotivoTrabajo : "";

                $scope.verificadorEntregableMotivoTrabajo = data.datos.verificador;

                $('#fecha-inicio-entregable-motivo-tarea').datepicker('update', data.datos.fechaInicio);
                $('#fecha-fin-entregable-motivo-tarea').datepicker('update', data.datos.fechaFin);
                if (data.datos.fechaProduccion != "") {
                    $scope.fechaProduccionEntregable = data.datos.fechaProduccion;
                    $('#fecha-produccion-entregable').datepicker('update', data.datos.fechaProduccion);
                } else {
                    $scope.fechaProduccionEntregable = "";
                }

                angular.element("#modal-add-entregables-trabajos").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Eliminando los Entregables de Trabajo
    $scope.eliminarEntregable = function () {
        $scope.wind2Opciones.show('Eliminar Entregable', 'Usted está a punto de eliminar un entregable, ¿desea continuar?.',
            'Sí', function () {
                $scope.wind2Opciones.hide();
                var ajaxEliminarEntregableTrabajo = $http.post("task/eliminar-entregable-trabajo",
                    {
                        idEntregableTrabajo: $scope.idEntregableMotivoTrabajo
                    });
                ajaxEliminarEntregableTrabajo.success(function (data) {
                    if (data.success === true) {
                        cargarConsolidaciones();
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });
            });
    };

    //Expandir y contraer
    angular.element("#panel-motivos-trabajo").on("click", ".expandir-motivos", function () {
        var abuelo = angular.element(this).parent().parent();
        var proximo = angular.element(abuelo).next();
        proximo.toggle();

        if (angular.element(proximo).is(":visible")) {
            angular.element(this).removeClass('glyphicon-plus');
            angular.element(this).addClass('glyphicon-minus');
        }
        else {
            angular.element(this).removeClass('glyphicon-minus');
            angular.element(this).addClass('glyphicon-plus');
        }
    });
    angular.element("#panel-motivos-trabajo").on("click", ".expandir-trabajo", function () {
        var vabuelo = angular.element(this).parent().parent().parent();
        var proximo = angular.element(vabuelo).next();
        proximo.toggle();

        if (angular.element(proximo).is(":visible")) {
            angular.element(this).removeClass('glyphicon-plus');
            angular.element(this).addClass('glyphicon-minus');
        }
        else {
            angular.element(this).removeClass('glyphicon-minus');
            angular.element(this).addClass('glyphicon-plus');
        }
    });

    //AdicionandoTareas
    $scope.adicionarTareas = function () {
        var ajaxDatosTarea = $http.post("task/datos-new-tarea-consolidacion",
            {
                idEntregable: $scope.idEntregable,
                semanaRelativa: $scope.semanaRelativa
            });
        ajaxDatosTarea.success(function (data) {
            if (data.success === true) {
                $scope.idEntregableTaskConsolidacion = $scope.idEntregable;
                $scope.lunesSemanaTaskConsolidacion = data.data.lunesSemana;
                $scope.domingoSemanaTaskConsolidacion = data.data.domingoSemana;
                $scope.clienteTaskConsolidacion = data.data.cliente;
                $scope.entregableTaskConsolidacion = data.data.entregable;

                angular.element("#modal-add-task-consolidacion").modal("show");

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.seleccionarSedePorColaborador = function () {
        angular.element.each($scope.trabajadores, function (key, trab) {
            if (trab.trab.id === parseInt($scope.colaboradorTaskConsolidacion)) {
                $scope.ubicacionTaskConsolidacion = trab.trab.idSede;
                return false;
            }
        });
    };
    $scope.guardarTareaConsolidacion = function () {
        waitingDialog.show('Guardando las Tareas...', { dialogSize: 'sm', progressType: 'success' });
        $scope.horasDiasArray = [];
        $scope.horasDiasArray.push($scope.horasLunesTaskConsolidacion);
        $scope.horasDiasArray.push($scope.horasMartesTaskConsolidacion);
        $scope.horasDiasArray.push($scope.horasMiercolesTaskConsolidacion);
        $scope.horasDiasArray.push($scope.horasJuevesTaskConsolidacion);
        $scope.horasDiasArray.push($scope.horasViernesTaskConsolidacion);
        $scope.horasDiasArray.push($scope.horasSabadoTaskConsolidacion);
        $scope.horasDiasArray.push($scope.horasDomingoTaskConsolidacion);

        var ajaxGuardarTareaConsolidacion = $http.post("task/new-tarea-consolidacion",
            {
                idColaborador: $scope.colaboradorTaskConsolidacion,
                fechaLunes: $scope.lunesSemanaTaskConsolidacion,
                idLugar: $scope.ubicacionTaskConsolidacion,
                idActividad: $scope.actividadTaskConsolidacion,
                idModulo: $scope.moduloTaskConsolidacion,
                detalle: $scope.detalleTaskConsolidacion,
                idEntregable: $scope.idEntregableTaskConsolidacion,
                horasDias: angular.toJson($scope.horasDiasArray),
                coordinador: $scope.coordinadorTaskConsolidacion
            });
        ajaxGuardarTareaConsolidacion.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-add-task-consolidacion").modal("hide");
                cargarConsolidaciones($scope.varSemanas);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-add-task-consolidacion").on('hidden.bs.modal', function (e) {
        limpiarNewTareaConsolidacion();
    });
    function limpiarNewTareaConsolidacion() {
        $scope.colaboradorTaskConsolidacion = "";
        $scope.lunesSemanaTaskConsolidacion = "";
        $scope.ubicacionTaskConsolidacion = "";
        $scope.actividadTaskConsolidacion = "";
        $scope.moduloTaskConsolidacion = "";
        $scope.detalleTaskConsolidacion = "";
        $scope.idEntregableTaskConsolidacion = "";
        $scope.horasDiasArray = [];
        $scope.coordinadorTaskConsolidacion = "";

        $scope.horasLunesTaskConsolidacion = 0;
        $scope.horasMartesTaskConsolidacion = 0;
        $scope.horasMiercolesTaskConsolidacion = 0;
        $scope.horasJuevesTaskConsolidacion = 0;
        $scope.horasViernesTaskConsolidacion = 0;
        $scope.horasSabadoTaskConsolidacion = 0;
        $scope.horasDomingoTaskConsolidacion = 0;

        angular.element("#frm-new-task-consolidacion").each(function () {
            this.reset();
        });
    }

    //Ver los próximos entregables
    $scope.paginaProximosEntregables;
    $scope.cantEntregables = 0;
    $scope.verProximosEntregables = function (pagina, length) {
        $scope.$parent.loading.show();
        if (pagina === undefined || pagina === 0) {
            pagina = 1;
        };
        if (length === undefined || length === 0) {
            length = 6;
        }
        $scope.paginaProximosEntregables = pagina;
        var ajaxProximosEntregables = $http.post("task/proximos-entregables", {
            pagina: pagina,
            length: length,
            desde: $scope.fechaDesdeProximoEntregable,
            hasta: $scope.fechaHastaProximoEntregable,
            cliente: $scope.clienteProximoEntregable
        });
        ajaxProximosEntregables.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.proximosEntregables = data.entregables;
                $scope.cantEntregables = data.cantPaginas;
                $scope.listaPaginas = [];
                for (var i = 1; i <= data.cantPaginas; i++) {
                    $scope.listaPaginas.push(i);
                }
                if (angular.element("#modal-proximos-entregables").is(':visible') === false)
                    angular.element("#modal-proximos-entregables").modal("show");

                setTimeout(function () {
                    angular.element("#modal-proximos-entregables .pagination li").removeClass('active')
                    angular.element(angular.element("#modal-proximos-entregables .pagination li")[pagina]).addClass('active');
                }, 50);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.atrazarPagina = function () {
        if ($scope.paginaProximosEntregables > 1) {
            $scope.paginaProximosEntregables--;
            $scope.verProximosEntregables($scope.paginaProximosEntregables);
        }
    };
    $scope.avanzarPagina = function () {
        if ($scope.paginaProximosEntregables < $scope.cantEntregables) {
            $scope.paginaProximosEntregables++;
            $scope.verProximosEntregables($scope.paginaProximosEntregables);
        }
    };
    $scope.cambiarPagina = function (pagina) {
        $scope.verProximosEntregables(pagina);
    };
    $scope.cargarPagina = function () {
        $scope.verProximosEntregables(1);
    };

    //Para ver los detalles de las asignaciones
    angular.element("#panel-container-consolidacion-trabajo").on("click", ".tarea-consolidada", function () {
        var idColaboradorCons = angular.element(this).attr("data-id-colab-cons");
        var contTareaConsolidada = angular.element(this).parent().parent();
        var primerContenedor = angular.element(contTareaConsolidada).siblings().first();
        var idEntregableCons = angular.element(primerContenedor).attr("data-id-entregable");

        var divHijos = angular.element(primerContenedor).parent().children();
        var i = angular.element(contTareaConsolidada).index();

        var posSemana = (i - 1) + $scope.varSemanas;

        var ajaxDatos = $http.post("task/asignaciones-semana-entregable",
            {
                idColaborador: idColaboradorCons,
                posSemana: posSemana,
                idEntregable: idEntregableCons
            });
        ajaxDatos.success(function (data) {
            if (data.success === true) {

                $scope.usuarioColaborador = data.emailColaborador;
                $scope.actividades = data.actividades;

                angular.element("#modal-asignaciones-entregables").modal('show');

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    });

    //Eliminando los adjuntos
    $scope.eliminarAdjunto = function (idAdj) {
        waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'success' });
        var deleteAdjunto = $http.post("task/eliminar-adjunto-contrato",
            {
                idAdjunto: idAdj
            });
        deleteAdjunto.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.adjuntos = data.adjuntos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Adicionar un campo de fichero
    $scope.adicionarFileAnterior = function () {
        var listaBotones = angular.element("#barra-botones-file");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element('#deleteInputFileAnterior').removeAttr('disabled');
        }
    };

    //Eliminar un campo de fichero
    $scope.deleteFileAnterior = function () {
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element("#barra-botones-file").prev().remove();
            cantFileInput = angular.element(".file-adj-contrato:visible").length;
            if (cantFileInput < 2) {
                angular.element('#deleteInputFileAnterior').attr('disabled', 'disabled');
            }
        }
    };

}]);

//Filters
//Filter de tamaño de texto
taskApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);

taskApp.directive('ngEnter', function ($parse) {
    return function (scope, element, attrs) {
        var fn = $parse(attrs.ngEnter);
        element.bind('keydown keypress', function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    event.preventDefault();
                    fn(scope, { $event: event });
                });
            }

        });
    };
});