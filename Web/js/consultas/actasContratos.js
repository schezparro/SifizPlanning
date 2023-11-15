consultasApp.controller('actasController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
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
        $scope.recargarActas(start, lenght, "", globalOrder, globalAsc);
    };

    var filterActas = {
        codigo: '',
        numeroContrato: '',
        cliente: '',
        asunto: '',
        fecha: '',
        colaborador: '',
        tipo: '',
        estado: '',
        contrato: '',
    };

    angular.element('#input-fecha-firma-adicionar').datepicker({
        format: 'dd/mm/yyyy',
        todayBtn: true
    });

    angular.element('#input-fecha-firma-editar').datepicker({
        format: 'dd/mm/yyyy',
        todayBtn: true
    });

    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna-actas i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna-actas");
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
        $scope.recargarActas(start, lenght, "", globalOrder, globalAsc);
    };

    $scope.filterData = function (valor) {
        if ($scope.codigo != undefined) {
            filterActas.codigo = $scope.codigo;
        }

        if ($scope.cliente != undefined) {
            filterActas.cliente = $scope.cliente;
        }

        if ($scope.asunto != undefined) {
            filterActas.asunto = $scope.asunto;
        }

        if ($scope.fechaFirma != undefined) {
            filterActas.fecha = $scope.fechaFirma;
        }

        if ($scope.colaborador != undefined) {
            filterActas.colaborador = $scope.colaborador;
        }

        if ($scope.contrato != undefined) {
            filterActas.contrato = $scope.contrato;
        }

        switch (valor) {
            case 1:
                filterActas.codigo = $scope.codigo;
                $scope.FiltroNumero = false;
                break;
            case 2:
                filterActas.cliente = $scope.cliente;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterActas.asunto = $scope.asunto;
                $scope.FiltroNumero = false;
                break;
            case 4:
                filterActas.fecha = $scope.fechaFirma;
                $scope.FiltroNumero = false;
                break;
            case 5:
                filterActas.colaborador = $scope.colaborador;
                $scope.FiltroNumero = false;
                break;
            case 6:
                var filtroTipo = angular.element('#select-tipo-acta').val();

                if (filtroTipo === "Seleccione...") {
                    filterActas.tipo = "";
                } else {
                    filterActas.tipo = filtroTipo;
                }

                $scope.FiltroNumero = false;
                break;
            case 7:
                var filtroEstado = angular.element('#select-estado-acta').val();

                if (filtroEstado === "Seleccione...") {
                    filterActas.estado = "";
                } else {
                    filterActas.estado = filtroEstado;
                }

                $scope.FiltroNumero = false;
                break;
            case 8:
                $scope.numeroContrato = $scope.numeroContrato.toUpperCase();
                filterActas.numeroContrato = $scope.numeroContrato;
                $scope.FiltroNumero = true;
                break;
            case 9:
                filterActas.contrato = $scope.contrato;
                $scope.FiltroNumero = false;
                break;
        }

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarActas(start, lenght, "", globalOrder, globalAsc);
    };

    //Para los selects
    angular.element('#select-tipo-acta').on('click', function (e) {
        $scope.filterData(6);
    });
    angular.element('#select-estado-acta').on('click', function (e) {
        $scope.filterData(7);
    });

    $scope.mostrarPorTipo = function () {
        $scope.recargarActas(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.recargarActas = function (start, lenght, filtro, order, asc) {

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {

            if (filterActas.codigo == undefined)
                filterActas.codigo = '';
            if (filterActas.cliente == undefined)
                filterActas.cliente = '';
            if (filterActas.fecha == undefined)
                filterActas.fecha = '';
            if (filterActas.colaborador == undefined)
                filterActas.colaborador = '';
            if (filterActas.tipo == undefined)
                filterActas.tipo = '';
            if (filterActas.estado == undefined)
                filterActas.estado = '';
            if (filterActas.linkOpenkm == undefined)
                filterActas.linkOpenkm = '';
            if (filterActas.numeroContrato == undefined)
                filterActas.numeroContrato = '';
            if (filterActas.contrato == undefined)
                filterActas.contrato = '';

            filtro = angular.toJson(filterActas)
        }

        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;

        var ajaxActas = $http.post("task/cargar-actas", {
            start: start,
            lenght: lenght,
            filtro: filtro,
            order: order,
            asc: asc,
            tipoMotivoTrabajo: $scope.filtroTipo,
        });

        ajaxActas.success(function (data) {
            if (data.success === true) {
                if ($scope.numeroContrato != undefined) {
                    if ($scope.numeroContrato != "" && data.cantidadActas === 0 && $scope.FiltroNumero === true) {
                        messageDialog.show('Información', "No se han encontrado actas con el numero de contrato: '" + $scope.numeroContrato + "'");
                    }
                }

                $scope.datosActas = data.actas;
                $scope.totalActas = data.cantidadActas;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadActas / numerosPorPagina);

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
                    var listaPaginador = angular.element("#tabla-actas-contratos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    $scope.AbrirModalActa = function () {

        var labelNumContrato = angular.element("#label-numero-contrato-adicionar")[0];
        var labelCliente = angular.element("#label-cliente-adicionar")[0];
        var selectColaborador = angular.element("#select-colaborador-adicionar")[0];
        var inputFechaFirma = angular.element("#input-fecha-firma-adicionar")[0];
        var selectTipo = angular.element("#select-tipo-adicionar")[0];
        var selectEstado = angular.element("#select-estado-adicionar")[0];
        var inputLinkOpenkm = angular.element("#input-linkOpenkm-adicionar")[0];
        var inputCodigoActa = angular.element("#input-codigo-acta-adicionar")[0];

        labelNumContrato.innerText = "Contrato:";
        labelCliente.innerText = "Cliente:";
        inputFechaFirma.value = "";
        selectColaborador.value = "Seleccione...";
        selectTipo.value = "Seleccione...";
        selectEstado.value = "Seleccione...";
        inputLinkOpenkm.value = "";
        inputCodigoActa.value = "";
        $scope.entregableAdicionar = "";
        $scope.tipoSeleccionado = "";

        var inputContrato = angular.element("#actas-numero-contrato")[0];

        $scope.numeroContrato = inputContrato.value.toUpperCase();

        if ($scope.numeroContrato === "" || $scope.numeroContrato == undefined) {
            messageDialog.show('Información', "Porfavor ingrese el número del contrato para el que desea ingresar una nueva acta");
        } else {

            var ajaxDatosContrato = $http.post("task/obtener-contrato", {
                numeroContrato: $scope.numeroContrato
            });

            ajaxDatosContrato.success(function (data) {
                if (data.success == true) {
                    var datosContrato = data.datos;

                    $scope.calcularUltimaActa($scope.numeroContrato);

                    if (datosContrato.length == 0 || datosContrato.length == undefined) {
                        messageDialog.show('Información', "No se ha encontrado un contrato con el numero '" + $scope.numeroContrato + "'");
                    } else {
                        $scope.obtenerEntregables($scope.numeroContrato);
                        $scope.contratoAdicionar = data.datos[0].numero;
                        $scope.clienteAdicionar = data.datos[0].cliente;
                        $scope.asuntoAdicionar = data.datos[0].asunto;

                        angular.element("#modal-nueva-acta").modal("show");
                    }
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        }

    }

    $scope.AbrirModalEditarActa = function (secuencialActa, numeroContrato) {

        $scope.secuencialActa = secuencialActa;
        $scope.entregableEditar = "";
        $scope.obtenerEntregables(numeroContrato);

        angular.element("#input-fecha-firma-editar")[0].value = "";

        var ajaxObtenerDatosActa = $http.post("task/obtener-datos-acta",
            {
                secuencialActa: secuencialActa
            });

        ajaxObtenerDatosActa.success(function (data) {
            if (data.success === true) {

                var selectColaborador = angular.element("#select-colaborador-editar")[0];
                var selectEstado = angular.element("#select-estado-editar")[0];
                var inputLinkOpenkm = angular.element("#input-linkOpenkm-editar")[0];

                $scope.entregableEditar = data.datos[0].entregable != null ? data.datos[0].entregable : '';
                $scope.contratoEditar = data.datos[0].numeroContrato;
                $scope.actaEditar = data.datos[0].codigo;
                $scope.clienteEditar = data.datos[0].cliente;
                $scope.asuntoEditar = data.datos[0].asunto;
                $scope.fechaFirmaEditar = data.fechaFirma;
                $scope.tipoSeleccionadoEdicion = data.datos[0].tipo;
                
                $scope.esActaPerzonalizada = data.datos[0].esActaPerzonalizada;
                selectColaborador.value = data.datos[0].colaborador;
                selectEstado.value = data.datos[0].estado;
                inputLinkOpenkm.value = data.datos[0].linkopenkm;
                $scope.diasFacturacionEditar = data.datos[0].diasFacturacion;

                angular.element("#modal-editar-acta").modal("show");

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.abrirConfirmacionEliminar = function () {
        angular.element("#modal-confirmar-eliminar").modal("show");
    }

    $scope.guardarNuevaActa = function () {

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var colaborador = angular.element("#select-colaborador-adicionar")[0].value
        var estado = angular.element("#select-estado-adicionar")[0].value
        var tipo = angular.element("#select-tipo-adicionar")[0].value
        var numeroActa = angular.element("#input-codigo-acta-adicionar")[0].value

        var datosEnvio = {
            id: 0,
            cliente: $scope.clienteAdicionar,
            colaborador: colaborador,
            estado: estado,
            tipo: tipo,
            numeroContrato: $scope.contratoAdicionar,
            fechaFirma: $scope.fechaFirmaAdicionar,
            linkOpenkm: $scope.linkOpenkmAdicionar !== undefined ? $scope.linkOpenkmAdicionar : "",
            numeroActa: numeroActa,
            entregable: $scope.entregableAdicionar != "" ? $scope.entregableAdicionar : 0,
            diasFacturacion: $scope.inputDiasFacturacion !== undefined ? $scope.inputDiasFacturacion : 0
        };

        var ajaxEnvioDatos = $http.post("task/guardar-acta",
            {
                datos: angular.toJson(datosEnvio)
            });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                messageDialog.show("Información", data.msg);
                angular.element("#modal-nueva-acta").modal("hide");
                $scope.paginar();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    }

    $scope.eliminarActa = function () {

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        angular.element("#modal-confirmar-eliminar").modal("hide");

        var ajaxEliminarActa = $http.post("task/eliminar-acta",
            {
                secuencialActa: $scope.secuencialActa
            });


        ajaxEliminarActa.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                messageDialog.show("Información", data.msg);
                angular.element("#modal-editar-acta").modal("hide");
                $scope.paginar();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    }

    $scope.editarActa = function () {

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });


        var colaborador = angular.element("#select-colaborador-editar")[0].value;
        var estado = angular.element("#select-estado-editar")[0].value;
        var tipo = angular.element("#select-tipo-editar")[0].value;
        var fechaFirmaEditar = "";

        if (angular.element("#input-fecha-firma-editar")[0].value === "") {
            fechaFirmaEditar = $scope.fechaFirmaEditar;
        } else {
            fechaFirmaEditar = angular.element("#input-fecha-firma-editar")[0].value;
        }

        var linkOpenkmEditar = angular.element("#input-linkOpenkm-editar")[0].value;

        var numeroActa = $scope.actaEditar;


        var datosEnvio = {
            id: $scope.secuencialActa,
            cliente: $scope.clienteEditar,
            colaborador: colaborador,
            estado: estado,
            tipo: tipo,
            numeroContrato: $scope.contratoEditar,
            fechaFirma: fechaFirmaEditar,
            linkOpenkm: linkOpenkmEditar,
            numeroActa: numeroActa,
            entregable: $scope.entregableEditar != "" ? $scope.entregableEditar : 0,
            diasFacturacion: $scope.diasFacturacionEditar,
        };

        var ajaxEnvioDatos = $http.post("task/guardar-acta",
            {
                datos: angular.toJson(datosEnvio)
            });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                messageDialog.show("Información", data.msg);
                angular.element("#modal-editar-acta").modal("hide");
                $scope.paginar();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.AbrirLinkOpenkm = function () {

        var link = angular.element("#input-linkOpenkm-editar")[0].value;

        angular.element("#abrir-link-openkm").attr({ "href": link, "target": "_blank" });
        angular.element("#abrir-link-openkm")[0].click();
    }

    $scope.calcularUltimaActa = function (numeroContrato) {

        var ajaxCalcularActa = $http.post("task/calcular-codigo-acta",
            {
                numeroContrato: numeroContrato
            });

        ajaxCalcularActa.success(function (data) {
            if (data.success === true) {
                var inputCodigoActa = angular.element("#input-codigo-acta-adicionar")[0];
                $scope.codigoSiguienteActa = data.numeroActa;
                inputCodigoActa.value = $scope.codigoSiguienteActa;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.obtenerEntregables = function (numeroContrato) {
        var ajaxDatosMotivoTrabajo = $http.post(
            "task/dar-entregables-para-actas",
            {
                numeroContrato: numeroContrato,
            }
        );

        ajaxDatosMotivoTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.entregables = data.datos;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    }

}]);