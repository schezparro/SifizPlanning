consultasApp.controller('contratosController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
    var numerosPorPaginaSeguimientos = 5;
    var paginaSeguimientos = 1;
    $scope.FiltroNumero = true;

    var orderStatus = [1, 1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

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

    $scope.actualizarCantidadMostrar = function () {
        numerosPorPagina = $scope.cantidadMostrarPorPagina;
        $scope.paginar();
    };

    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.recargarContratos(start, lenght, "", globalOrder, globalAsc);
    };

    var filterContratos = {
        codigo: '',
        cliente: '',
        descripcion: '',
        fechaInicio: '',
        fechaVencimiento: '',
        estado: '',
        diasRestantes: '',
        responsable: '',
    };

    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna-contratos i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna-contratos");
        var columna = columnas[valor - 1];
        var flecha = angular.element(columna).find("i");

        angular.element(flecha).removeClass('invisible');

        if (globalAsc === 0) {
            angular.element(flecha).removeClass('glyphicon-chevron-up');
            angular.element(flecha).addClass('glyphicon-chevron-down');
        }
        else {
            angular.element(flecha).addClass('glyphicon-chevron-up');
            angular.element(flecha).removeClass('glyphicon-chevron-down');
        }
        $scope.recargarContratos(start, lenght, "", globalOrder, globalAsc);
    };

    $scope.filterData = function (valor) {
        if ($scope.codigo !== undefined) {
            filterContratos.codigo = $scope.codigo;
        }

        if ($scope.cliente !== undefined) {
            filterContratos.cliente = $scope.cliente;
        }

        if ($scope.descripcion !== undefined) {
            filterContratos.descripcion = $scope.descripcion;
        }

        if ($scope.fechaInicio !== undefined) {
            filterContratos.fechaInicio = $scope.fechaInicio;
        }

        if ($scope.fechaVencimiento !== undefined) {
            filterContratos.fechaVencimiento = $scope.fechaVencimiento;
        }

        if ($scope.estado !== undefined) {
            filterContratos.estado = $scope.estado;
        }

        if ($scope.diasRestantes !== undefined) {
            filterContratos.diasRestantes = $scope.diasRestantes;
        }

        //if ($scope.formaPago !== undefined) {
        //    filterContratos.formaPago = $scope.formaPago;
        //}

        switch (valor) {
            case 1:
                filterContratos.codigo = $scope.codigo;
                $scope.FiltroNumero = false;
                break;
            case 2:
                filterContratos.cliente = $scope.cliente;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterContratos.descripcion = $scope.descripcion;
                $scope.FiltroNumero = false;
                break;
            case 4:
                filterContratos.fechaInicio = $scope.fechaInicio;
                $scope.FiltroNumero = false;
                break;
            case 5:
                filterContratos.fechaVencimiento = $scope.fechaVencimiento;
                $scope.FiltroNumero = false;
                break;
            case 6:
                var filtroEstado = angular.element('#select-estado-contrato').val();

                if (filtroEstado === "Seleccione...") {
                    filterContratos.estado = "";
                } else {
                    filterContratos.estado = filtroEstado;
                }

                $scope.FiltroNumero = false;
                break;
            case 7:
                filterContratos.diasRestantes = $scope.diasRestantes;
                $scope.FiltroNumero = false;
                break;
            case 8:
                var filtroResponsable = angular.element('#select-responsable').val();

                if (filtroResponsable === "Seleccione...") {
                    filterContratos.responsable = "";
                } else {
                    filterContratos.responsable = filtroResponsable;
                }

                $scope.FiltroNumero = false;
                break;
        }

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarContratos(start, lenght, "", globalOrder, globalAsc);
    };

    //Para los selects
    angular.element('#select-estado-contrato').on('click', function (e) {
        $scope.filterData(6);
    });
    angular.element('#select-responsable').on('click', function (e) {
        $scope.filterData(8);
    });

    $scope.mostrarPorTipo = function () {
        $scope.recargarContratos(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.mostrarTodosLosContratos = function () {
        $scope.recargarContratos(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.recargarContratos = function (start, lenght, filtro, order, asc) {

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {

            if (filterContratos.codigo === undefined)
                filterContratos.codigo = '';
            if (filterContratos.cliente === undefined)
                filterContratos.cliente = '';
            if (filterContratos.descripcion === undefined)
                filterContratos.descripcion = '';
            if (filterContratos.fechaInicio === undefined)
                filterContratos.fechaInicio = '';
            if (filterContratos.fechaVencimiento === undefined)
                filterContratos.fechaVencimiento = '';
            if (filterContratos.estado === undefined)
                filterContratos.estado = '';
            if (filterContratos.diasRestantes === undefined)
                filterContratos.diasRestantes = '';
            if (filterContratos.responsable === undefined)
                filterContratos.responsable = '';

            filtro = angular.toJson(filterContratos)
        }

        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;

        var ajaxContratos = $http.post("task/cargar-motivos-trabajo", {
            start: start,
            lenght: lenght,
            filtro: filtro,
            order: order,
            asc: asc,
            todos: $scope.mostrarTodos,
            tipoMotivoTrabajo: $scope.filtroTipo,
        });

        ajaxContratos.success(function (data) {
            if (data.success === true) {
                $scope.datosContratos = data.contratos;
                $scope.totalContratos = data.cantidadContratos;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadContratos / numerosPorPagina);

                if ($scope.cantPaginas === 0) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
                    for (var j = 1; j <= $scope.cantPaginas; j++) {
                        $scope.listaPaginas.push(j);
                    }
                }
                else if ($scope.cantPaginas > 5) {
                    for (var k = pagina - 4; k <= pagina; k++) {
                        $scope.listaPaginas.push(k);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginas) {
                    pagina = $scope.cantPaginas;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-consulta-contratos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };
    //Mostrando la modal con los detalles del contrato seleccionado
    $scope.mostrarDetalleContrato = function (codigoContrato) {
        if (codigoContrato !== "") {
            $scope.codigoContrato = codigoContrato;

            var ajaxDetallesContrato = $http.post("task/dar-detalles-motivo-trabajo",
                {
                    codigoContrato: codigoContrato
                });
            ajaxDetallesContrato.success(function (data) {
                if (data.success === true) {

                    $scope.secuencialContrato = data.datos.secuencial;
                    $scope.numeroVerificadorContrato = data.datos.numeroVerificador;
                    $scope.codigoContrato = data.datos.codigo;
                    $scope.clienteContrato = data.datos.cliente;
                    $scope.idClienteContrato = data.datos.idCliente;
                    $scope.descripcionContrato = data.datos.descripcion;
                    $scope.avanceContrato = data.datos.avance;
                    $scope.entregablesContrato = data.datos.entregables;
                    $scope.tipoContrato = data.datos.tipo;
                    $scope.estadoContrato = data.datos.estado;
                    $scope.idEstadoContrato = data.datos.estadoId;
                    $scope.fechaInicioContrato = data.datos.fechaInicio;
                    $scope.fechaVencimientoContrato = data.datos.fechaFin;
                    $scope.fechaInicioPlanificacionContrato = data.datos.fechaInicioPlanificacion;
                    $scope.fechaFinPlanificacionContrato = data.datos.fechaFinPlanificacion;
                    $scope.nombreContrato = data.datos.nombre;
                    $scope.adendasContrato = data.adendas;
                    $scope.faseContrato = data.datos.fase !== 0 ? data.datos.fase : '';
                    $scope.estimacion = data.datos.estimacion;
                    $scope.isRossi = data.datos.isRossi;
                    $scope.cronogramaContrato = data.datos.cronograma;
                    $scope.pagado = data.datos.pagado;
                    $scope.diasRestantesContrato = data.datos.diasRestantes;
                    $scope.formaPagoContrato = data.datos.formaPago;
                    $scope.linkOpenKm = data.datos.linkOpenKm;
                    $scope.colaborador = data.datos.colaborador;
                    $scope.plazo = data.datos.plazo !== -1 ? data.datos.plazo : "NO ASIGNADO";
                    $scope.tipoPlazo = data.datos.tipoPlazo;
                    $scope.horasMes = data.datos.horasMes;
                    $scope.adjuntos = data.datos.adjuntos;
                    $scope.aceptaNormativos = data.datos.aceptaNormativos;

                    if (data.fechaActa !== "01/01/0001") {
                        $scope.fechaActa = data.fechaActa;
                    } else {
                        $scope.fechaActa = '';
                    };
                    if (data.fechaProduccion !== "01/01/0001") {
                        $scope.fechaProduccion = data.fechaProduccion;
                    } else {
                        $scope.fechaProduccion = '';
                    };
                    if (data.datos.diasGarantia !== 0) {
                        $scope.diasGarantia = data.datos.diasGarantia;
                    } else {
                        $scope.diasGarantia = '';
                    };

                    $scope.cargarSeguimientos(0, 5, codigoContrato);

                    $scope.nuevasAdendasContrato = [];
                    $scope.eliminarAdendasContrato = [];
                    $scope.adendacodigoContrato = "";
                    $scope.adendafechaVencimientoContrato = "";

                    $scope.validarContratoMantenimiento();

                    angular.element("#modal-consulta-contratos").modal('show');
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

    $scope.guardarInformacionContrato = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxDetallesContrato = $http.post("task/guardar-informacion-trabajo",
            {
                tieneCronograma: $scope.cronogramaContrato,
                pagado: $scope.pagado,
                secuencialFase: $scope.faseContrato,
                estimacion: $scope.estimacion,
                estadoContrato: $scope.idEstadoContrato,
                aceptaNormativos: $scope.aceptaNormativos,
                linkOpenKm: $scope.linkOpenKm,
                fechaActa: $scope.fechaActa,
                fechaProduccion: $scope.fechaProduccion,
                diasGarantia: $scope.diasGarantia !== null ? $scope.diasGarantia : 0,
                secuencialMotivoTrabajo: $scope.secuencialContrato,
                colaborador: $scope.colaborador,
                numeroVerificador: $scope.numeroVerificadorContrato
            });

        ajaxDetallesContrato.success(function (data) {
            if (data.success === true) {
                $scope.numeroVerificadorContrato++;

                var ajaxGuardarSeguimiento = $http.post("task/guardar-seguimiento-contrato",
                    {
                        datosNuevos: angular.toJson($scope.nuevosSeguimientosContrato),
                        datosEliminar: angular.toJson($scope.eliminarSeguimientosContrato),
                        datosEditar: angular.toJson($scope.edicionSeguimientosContrato),
                        secuencialMotivo: $scope.secuencialContrato
                    });
                ajaxGuardarSeguimiento.success(function (data) {
                    if (data.success === true) {
                        $scope.nuevosSeguimientosContrato = [];
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });
                var ajaxGuardarAdendas = $http.post("task/guardar-adendas-contrato",
                    {
                        datosNuevos: angular.toJson($scope.nuevasAdendasContrato),
                        datosEliminar: angular.toJson($scope.eliminarAdendasContrato),
                        secuencialMotivo: $scope.secuencialContrato
                    });
                ajaxGuardarAdendas.success(function (data) {
                    if (data.success === true) {
                        $scope.nuevasAdendasContrato = [];
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });
                waitingDialog.hide();
                $scope.recargarContratos();
                angular.element("#modal-consulta-contratos").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.adicionarAdenda = function () {
        if ($scope.adendaCodigoContrato !== "" && $scope.adendafechaVencimientoContrato !== "") {
            $scope.adendasContrato.push({
                Secuencial: 0,
                Codigo: $scope.adendaCodigoContrato,
                FechaVencimiento: $scope.adendafechaVencimientoContrato,
                LinkOpenKM: $scope.adendaLinkOpenKM,
                NumeroVerificador: 0
            });
            $scope.nuevasAdendasContrato.push({
                Secuencial: 0,
                Codigo: $scope.adendaCodigoContrato,
                FechaVencimiento: $scope.adendafechaVencimientoContrato,
                LinkOpenKM: $scope.adendaLinkOpenKM,
                NumeroVerificador: 0
            });
            messageDialog.show('Información', 'Adenda añadida, debe salvar para guardar los cambios!!!');

            $scope.adendaCodigoContrato = "";
            $scope.adendafechaVencimientoContrato = "";
            $scope.adendaLinkOpenKM = "";
        } else {
            messageDialog.show('Información', "Debe ingresar el Código y la Fecha de Vencimiento");
        }
    }

    $scope.guardarEdicionAdenda = function () {
        if ($scope.editarCodigoAdenda !== "" && $scope.editarFechaAdenda !== "") {
            $scope.eliminarAdenda($scope.editarIndexAdenda);
            $scope.adendasContrato.push({
                Secuencial: 0,
                Codigo: $scope.editarCodigoAdenda,
                FechaVencimiento: $scope.editarFechaAdenda,
                LinkOpenKM: $scope.editarLinkAdenda,
                NumeroVerificador: 0
            });
            $scope.nuevasAdendasContrato.push({
                Secuencial: 0,
                Codigo: $scope.editarCodigoAdenda,
                FechaVencimiento: $scope.editarFechaAdenda,
                LinkOpenKM: $scope.editarLinkAdenda,
                NumeroVerificador: 0
            });
            $scope.editarCodigoAdenda = "";
            $scope.editarFechaAdenda = "";
            $scope.editarLinkAdenda = "";
            $scope.editarIndexAdenda = "";
            angular.element("#modal-new-adenda").modal("hide");

        } else {
            messageDialog.show('Información', "Debe ingresar el Código y la Fecha de Vencimiento");
        }
    }

    $scope.eliminarAdenda = function (index) {
        if ($scope.adendasContrato[index].Secuencial !== 0) {
            $scope.eliminarAdendasContrato.push({
                Secuencial: $scope.adendasContrato[index].Secuencial
            });
        } else {
            $scope.nuevasAdendasContrato.forEach(function (element, indexador) {
                if ($scope.adendasContrato[index].FechaVencimiento === element.FechaVencimiento
                    && $scope.adendasContrato[index].Codigo === element.Codigo) {
                    $scope.nuevasAdendasContrato.splice(indexador, 1);
                }
            });
        }
        $scope.adendasContrato.splice(index, 1);
    }

    $scope.editarAdenda = function (index) {
        $scope.editarIndexAdenda = index;
        $scope.editarCodigoAdenda = $scope.adendasContrato[index].Codigo;
        $scope.editarFechaAdenda = $scope.adendasContrato[index].FechaVencimiento;
        $scope.editarLinkAdenda = $scope.adendasContrato[index].LinkOpenKM;
        angular.element("#modal-new-adenda").modal("show");
    };

    $scope.editarSeguimiento = function (index) {
        $scope.editarSeg = true;
        $scope.indexSeguimiento = index;
        $scope.fechaSeguimiento = $scope.seguimientosContrato[index].FechaSeguimiento;
        $scope.realizado = $scope.seguimientosContrato[index].Realizado;
        $scope.porRealizar = $scope.seguimientosContrato[index].PorRealizar;
        $scope.pendiente = $scope.seguimientosContrato[index].Pendiente;
        $scope.usuario = $scope.seguimientosContrato[index].Usuario;
        angular.element("#modal-new-task-ticket").modal("show");
    };

    $scope.adicionarSeguimiento = function () {
        $scope.editarSeg = false;
        $scope.fechaSeguimiento = '';
        $scope.realizado = '';
        $scope.porRealizar = '';
        $scope.pendiente = '';
        $scope.usuario = '';
        angular.element("#modal-new-task-ticket").modal("show");
    };

    $scope.guardarSeguimientos = function () {
        if ($scope.editarSeg === false) {
            if ($scope.fechaSeguimiento !== undefined && ($scope.realizado !== undefined || $scope.porRealizar !== undefined || $scope.pendiente !== undefined)) {
                $scope.nuevosSeguimientosContrato.push({
                    Secuencial: $scope.secuencialSeguimientoContrato++,
                    FechaSeguimiento: $scope.fechaSeguimiento,
                    Realizado: $scope.realizado !== undefined ? $scope.realizado : "",
                    PorRealizar: $scope.porRealizar !== undefined ? $scope.porRealizar : "",
                    Pendiente: $scope.pendiente !== undefined ? $scope.pendiente : "",
                    NumeroVerificador: 0
                });
                paginaSeguimientos = 1;
                $scope.recargarSeguimientos(0, 5, $scope.codigoContrato);
                messageDialog.show('Información', 'Seguimiento añadido a la tabla, debe salvar para guardar los cambios!!!');
                $scope.fechaSeguimiento = '';
                $scope.realizado = '';
                $scope.porRealizar = '';
                $scope.pendiente = '';
            } else {
                messageDialog.show('Información', 'Debe Agregar la fecha y al menos un seguimiento');
            }
        }
        else {
            if ($scope.fechaSeguimiento !== undefined && ($scope.realizado !== undefined || $scope.porRealizar !== undefined || $scope.pendiente !== undefined)) {
                if ($scope.seguimientosContrato[$scope.indexSeguimiento].NumeroVerificador === 0) {
                    $scope.nuevosSeguimientosContrato.forEach(function (element) {
                        if ($scope.seguimientosContrato[$scope.indexSeguimiento].SecuencialMotivoTrabajo === element.Secuencial) {
                            element.FechaSeguimiento = $scope.fechaSeguimiento;
                            element.Realizado = $scope.realizado !== undefined ? $scope.realizado : "";
                            element.PorRealizar = $scope.porRealizar !== undefined ? $scope.porRealizar : "";
                            element.Pendiente = $scope.pendiente !== undefined ? $scope.pendiente : "";
                        }
                    });
                }
                else {
                    $scope.edicionSeguimientosContrato.push({
                        Secuencial: $scope.seguimientosContrato[$scope.indexSeguimiento].SecuencialMotivoTrabajo,
                        FechaSeguimiento: $scope.fechaSeguimiento,
                        Realizado: $scope.realizado !== undefined ? $scope.realizado : "",
                        PorRealizar: $scope.porRealizar !== undefined ? $scope.porRealizar : "",
                        Pendiente: $scope.pendiente !== undefined ? $scope.pendiente : "",
                        NumeroVerificador: $scope.seguimientosContrato[$scope.indexSeguimiento].NumeroVerificador
                    });
                }
                paginaSeguimientos = 1;
                $scope.recargarSeguimientos(0, 5, $scope.codigoContrato);
                messageDialog.show('Información', 'Seguimiento editado, debe salvar para guardar los cambios!!!');
                $scope.fechaSeguimiento = '';
                $scope.realizado = '';
                $scope.porRealizar = '';
                $scope.pendiente = '';
            } else {
                messageDialog.show('Información', 'Debe Agregar la fecha y al menos un seguimiento');
            }
        }
    };

    $scope.eliminarSeguimiento = function (index) {
        if ($scope.seguimientosContrato[index].NumeroVerificador !== 0) {
            $scope.eliminarSeguimientosContrato.push({
                Secuencial: $scope.seguimientosContrato[index].SecuencialMotivoTrabajo
            });
        } else {
            $scope.nuevosSeguimientosContrato.forEach(function (element, indexador) {
                if ($scope.seguimientosContrato[index].SecuencialMotivoTrabajo === element.Secuencial) {
                    $scope.nuevosSeguimientosContrato.splice(indexador, 1);
                }
            });
        }
        paginaSeguimientos = 1;
        $scope.recargarSeguimientos(0, 5, $scope.codigoContrato);
    };

    // Pagnación de Seguimientos

    $scope.cambiarPaginaSeguimientos = function (pag) {
        paginaSeguimientos = pag;
        $scope.paginarSeguimientos();
    };

    $scope.atrazarPaginaSeguimientos = function () {
        if (paginaSeguimientos > 1) {
            paginaSeguimientos--;
            $scope.paginarSeguimientos();
        }
    };

    $scope.avanzarPaginaSeguimientos = function () {
        if (paginaSeguimientos < $scope.cantPaginasSeguimientos) {
            paginaSeguimientos++;
            $scope.paginarSeguimientos();
        }
    };

    $scope.paginarSeguimientos = function () {
        var startSeguimientos = (paginaSeguimientos - 1) * numerosPorPaginaSeguimientos;
        var lenghtSeguimientos = numerosPorPaginaSeguimientos;
        $scope.recargarSeguimientos(startSeguimientos, lenghtSeguimientos, $scope.codigoContrato);
    };

    $scope.cargarSeguimientos = function (start, lenght, codigoContrato) {
        numerosPorPaginaSeguimientos = 5;
        paginaSeguimientos = 1;
        $scope.nuevosSeguimientosContrato = [];
        $scope.eliminarSeguimientosContrato = [];
        $scope.edicionSeguimientosContrato = [];
        $scope.secuencialSeguimientoContrato = 1;
        $scope.recargarSeguimientos(start, lenght, codigoContrato);
    }

    $scope.recargarSeguimientos = function (start, lenght, codigoContrato) {

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPaginaSeguimientos;

        var ajaxSeguimiento = $http.post("task/cargar-seguimientos-trabajo", {
            start: start,
            lenght: lenght,
            codigoContrato: codigoContrato,
            datosNuevos: angular.toJson($scope.nuevosSeguimientosContrato),
            datosEliminar: angular.toJson($scope.eliminarSeguimientosContrato),
            datosEditar: angular.toJson($scope.edicionSeguimientosContrato)
        });

        ajaxSeguimiento.success(function (data) {
            if (data.success === true) {
                $scope.seguimientosContrato = data.seguimientos;
                $scope.totalSeguimientosContratos = data.cantidadSeguimientos;

                var posPagin = paginaSeguimientos;
                $scope.cantPaginasSeguimientos = Math.ceil(data.cantidadSeguimientos / numerosPorPaginaSeguimientos);

                if ($scope.cantPaginasSeguimientos === 0) {
                    $scope.cantPaginasSeguimientos = 1;
                }

                $scope.listaPaginasSeguimientos = [];
                if ($scope.cantPaginasSeguimientos > 5 && paginaSeguimientos <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginasSeguimientos.push(i);
                    }
                }
                else if ($scope.cantPaginasSeguimientos <= 5) {
                    for (var j = 1; j <= $scope.cantPaginasSeguimientos; j++) {
                        $scope.listaPaginasSeguimientos.push(j);
                    }
                }
                else if ($scope.cantPaginasSeguimientos > 5) {
                    for (var k = paginaSeguimientos - 4; k <= paginaSeguimientos; k++) {
                        $scope.listaPaginasSeguimientos.push(k);
                    }
                    posPagin = 5;
                }

                if (paginaSeguimientos > $scope.cantPaginasSeguimientos) {
                    paginaSeguimientos = $scope.cantPaginasSeguimientos;
                    posPagin = paginaSeguimientos;
                }

                setTimeout(function () {
                    var listaPaginadorSeguimiento = angular.element("#tabla-seguimiento-contratos .pagination li a");
                    angular.element(listaPaginadorSeguimiento).removeClass('pagSelect');
                    angular.element(listaPaginadorSeguimiento[posPagin]).addClass('pagSelect');
                }, 100);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    angular.element('#fechaMantenimiento').datetimepicker({
        format: "MM/YYYY",
        locale: 'es',
        defaultDate: new Date(Date.now())
    }).on('dp.change', function (e) {
        $scope.contratosMantenimiento = "";
        var fecha = $('#fechaMantenimiento input').first().val();
        var darContratosMantenimiento = $http.post("tickets/dar-contratos-mantenimiento/",
            {
                idCliente: $scope.idClienteContrato,
                fecha: fecha
            });
        darContratosMantenimiento.success(function (data) {
            if (data.success === true) {
                $scope.contratosMantenimiento = data.datos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });
    $scope.validarContratos = function () {
        angular.element("#modal-contratos-cliente").modal('show');
    };
    $scope.validarContratoMantenimiento = function () {
        $scope.contratosMantenimiento = "";
        $scope.fechaMantenimiento = "";
        var darContratosMantenimiento = $http.post("tickets/dar-contratos-mantenimiento/",
            {
                idCliente: $scope.idClienteContrato,
                fecha: null
            });
        darContratosMantenimiento.success(function (data) {
            if (data.success === true) {
                $scope.contratosMantenimiento = data.datos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.editarAvance = function (index) {
        var editarAvanceEntregable = $http.post("task/editar-porcentaje-entregable/",
            {
                idEntregableTrabajo: $scope.entregablesContrato[index].Id,
                porcentaje: $scope.entregablesContrato[index].Avance,
                colaboradorID: angular.element('#select-colaborador-entregable' + index).val(),
                codigoContrato: $scope.codigoContrato
            });
        editarAvanceEntregable.success(function (data) {
            if (data.success === true) {
                $scope.avanceContrato = data.datos.porcentaje;
                $scope.entregablesContrato = data.datos.entregables;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.editarSelectAvance = function (index) {
        var select = document.getElementById('select-colaborador-entregable' + index).value = $scope.entregablesContrato[index].ColaboradorId;
    };

}]);