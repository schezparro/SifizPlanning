devApp.controller('proyectosController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;

    $scope.cargarProyectos = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        $scope.loading.show();
        var proyectos = $http.post("user/proyectos-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroProyectos
            });
        proyectos.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;
                $scope.proyectos = data.proyectos;
                $scope.cantPaginas = Math.ceil(data.total / numerosPorPagina);
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
                    var listaPaginador = angular.element("#tabla-user-proyectos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarProyectos();

    angular.element('#fecha-salida-proyecto').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-inicio-etapa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-fin-etapa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-inicio-subetapa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-fin-subetapa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    $scope.updateMinDateEtapa = function () {
        angular.element('#fecha-fin-etapa').datepicker('setStartDate', $scope.fechaIniEta);
    };

    $scope.updateMinDateSubEtapa = function () {
        angular.element('#fecha-fin-subetapa').datepicker('setStartDate', $scope.fechaIniSubE);
    };

    $scope.windowAgregarProyecto = function () {
        $scope.ubicacionPro = '';
        $scope.codigoFuentePro = false;
        $scope.responsableCodigoPro = '';
        $scope.versioDesarrolloDPro = '';
        $scope.clientePro = '';
        $scope.responsablePublicacionPro = '';
        $scope.fechaSalidaPro = '';
        $scope.responsableAccesoPro = '';
        $scope.repositoriosPro = '';
        $scope.versionBDPro = '';
        $scope.liderPro = '';
        $scope.solucionesProxiesPro = false;
        $scope.estaActivoPro = true;
        angular.element("#modal-agregar-proyecto").modal("show");
    };

    //CLIENTES
    var ajaxClienteProyectos = $http.post("user/cliente-incidencias", {});
    ajaxClienteProyectos.success(function (data) {
        if (data.success === true) {
            $scope.clientes = data.clientes;
        }
    });

    //VERSION BASE DATOS
    var ajaxRolVersionBaseDatos = $http.post("user/tipo-version-base-datos", {});
    ajaxRolVersionBaseDatos.success(function (data) {
        if (data.success === true) {
            $scope.versionBD = data.versionBD;
        }
    });

    //VERSION DESARROLLO
    var ajaxVersionDesarrollo = $http.post("user/tipo-version-desarrollo", {});
    ajaxVersionDesarrollo.success(function (data) {
        if (data.success === true) {
            $scope.versionDesarrollo = data.versionDesarrollo;
        }
    });

    //RESPONSABLE PROYECTO
    var ajaxResponsableProyecto = $http.post("user/tipo-responsable-proyectos", {});
    ajaxResponsableProyecto.success(function (data) {
        if (data.success === true) {
            $scope.responsableProyectos = data.responsableProyectos;
        }
    });

    //REPOSITORIO
    var ajaxRepositorio = $http.post("user/tipo-repositorio", {});
    ajaxRepositorio.success(function (data) {
        if (data.success === true) {
            $scope.repositorios = data.repositorios;
        }
    });

    //ETAPAS
    var ajaxetapas = $http.post("user/tipo-etapas", {});
    ajaxetapas.success(function (data) {
        if (data.success === true) {

            console.log(data.etapasCat);
            $scope.etapasCat = data.etapasCat;
        }
    });

    //RECURSOS SUBETAPAS
    var ajaxrecursosSubEtapas = $http.post("user/tipo-recursos-subetapas", {});
    ajaxrecursosSubEtapas.success(function (data) {
        if (data.success === true) {

            console.log("subetapas " + data.recursosSubEtapas);
            $scope.recursosSubEtapas = data.recursosSubEtapas;
        }
    });

    //LIDERES
    var ajaxLideres = $http.post("user/rol-colaborador-incidencias", {});
    ajaxLideres.success(function (data) {
        if (data.success === true) {
            $scope.lideres = data.lideres;
        }
    });

    $scope.GuardarNuevoProyecto = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var ubicacion = angular.element("#select-ubicacion-proyecto")[0].value;
        var responsableCodigo = angular.element("#select-responsableCodigo-proyecto")[0].value;
        var versionDesarrollo = angular.element("#select-versionDesarrollo-proyecto")[0].value;
        var cliente = angular.element("#select-cliente-proyecto")[0].value;
        var responsablePublicacion = angular.element("#select-responsablePublicacion-proyecto")[0].value;
        var salidaProduccion = angular.element("#fecha-salida-proyecto")[0].value;
        var responsableAcceso = angular.element("#select-responsableAcceso-proyecto")[0].value;
        var repositorio = angular.element("#select-repositorio-proyecto")[0].value;
        var versionBD = angular.element("#select-versionBD-proyecto")[0].value;
        var liderProyecto = angular.element("#select-liderProyecto-proyecto")[0].value;

        var formData = new FormData();
        formData.append('ubicacion', ubicacion);
        formData.append('liderProyecto', liderProyecto);
        formData.append('responsableCodigo', responsableCodigo);
        formData.append('versionDesarrollo', versionDesarrollo);
        formData.append('cliente', cliente);
        formData.append('responsablePublicacion', responsablePublicacion);
        formData.append('salidaProduccion', salidaProduccion);
        formData.append('responsableAcceso', responsableAcceso);
        formData.append('repositorio', repositorio);
        formData.append('versionBD', versionBD);
        formData.append('codigoFuente', $scope.codigoFuentePro);
        formData.append('estaActivo', $scope.estaActivoPro);
        formData.append('solucionProxies', $scope.solucionesProxiesPro);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-proyecto",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-proyecto").modal("hide");
                $scope.cargarProyectos();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };


    //Mostrar el modal de proyectos
    $scope.mostrarWindowProyectos = function (proyecto) {

        $scope.lider = proyecto.lider;
        $scope.gestor = proyecto.gestor;
        $scope.fechaProduccion = proyecto.fechaProduccion;
        $scope.versionDesarrollo = proyecto.versionDesarrollo;
        $scope.tieneSolucionProxies = proyecto.tieneSolucionProxies;
        $scope.versionBD = proyecto.versionBaseDatos;

        angular.element("#modal-proyectos").modal('show');
    };

    $scope.cargarEtapas = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        console.log("$scope.proyectoId " + $scope.proyectoId);

        $scope.loading.show();
        var etapas = $http.post("user/etapas-proyectos-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroEtapas,
                idProyecto: $scope.proyectoId
            });
        etapas.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;
                $scope.etapas = data.etapas;
                $scope.cantPaginasE = Math.ceil(data.total / numerosPorPagina);
                if ($scope.cantPaginasE === 0) {
                    $scope.cantPaginasE = 1;
                }

                $scope.listaPaginasE = [];
                if ($scope.cantPaginasE > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginasE.push(i);
                    }
                }
                else if ($scope.cantPaginasE < 5) {
                    for (var i = 1; i <= $scope.cantPaginasE; i++) {
                        $scope.listaPaginasE.push(i);
                    }
                }
                else if ($scope.cantPaginasE > 5) {
                    for (var i = pagina - 4; i <= pagina; i++) {
                        $scope.listaPaginasE.push(i);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginasE) {
                    pagina = $scope.cantPaginasE;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-user-etapas-proyectos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.GuardarNuevaEtapa = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var etapa = angular.element("#select-etapas-cliente-proyecto")[0].value;

        //var cliAux = proyectoId;
        var fechaInicioEta = angular.element("#fecha-inicio-etapa")[0].value;
        var fechaFinEta = angular.element("#fecha-fin-etapa")[0].value;

        if (fechaInicioEta > fechaFinEta) {
            $scope.informacion = "La fecha de inicio no puede ser mayor que la fecha final"
            angular.element("#modal-info-proyectos").modal("show");
        };

        var formData = new FormData();
        formData.append('etapa', etapa);
        formData.append('cliAux', $scope.proyectoId);
        formData.append('fechaInicioEta', fechaInicioEta);
        formData.append('fechaFinEta', fechaFinEta);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-etapas-proyecto",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-etapas-proyecto").modal("hide");
                $scope.cargarEtapas();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    $scope.abrirModalGenerarInforme = function (proyectoId) {
        $scope.proyectoId = proyectoId
        $scope.cargarEtapas();
        angular.element("#modal-etapas-proyecto").modal("show");
    };

    $scope.abrirModalGenerarReporte = function (proyectoId) {
        console.log("id proyecto " + proyectoId);

        var obtenerDatosInforme = $http.post("user/dar-datos-proyecto", {
            idProyecto: proyectoId
        });
        obtenerDatosInforme.success(function (data) {
            if (data.success === true) {

                var datosAplanados = [];
                data.datosInforme.forEach(function (etapa) {
                    // Agrega la etapa al arreglo aplanado
                    datosAplanados.push({
                        tipo: 'etapa',
                        descripcion: etapa.descripcion,
                        duracion: etapa.duracion,
                        fechaInicio: etapa.fechaInicio,
                        fechaFin: etapa.fechaFin,
                        detalle: etapa.detalle, 
                        idEtapa: etapa.idEtapa,
                        porciento: etapa.porciento,
                        selected: etapa.selected
                    });

                    // Agrega cada subetapa al arreglo aplanado
                    etapa.subEtapas.forEach(function (subetapa) {
                        datosAplanados.push({
                            tipo: 'subetapa',
                            descripcion: subetapa.descripcionSE,
                            duracion: subetapa.duracionSE,
                            fechaInicio: subetapa.fechaInicioSE,
                            fechaFin: subetapa.fechaFinSE,
                            recurso: subetapa.recursoSE,
                            idEtapa: subetapa.idEtapaSE,
                            detalle: subetapa.detalle,
                            porciento: subetapa.porciento,
                            selected: subetapa.selected
                        });
                    });
                });

                $scope.datosAplanados = datosAplanados;
                angular.element("#modal-informe-proyecto").modal("show");
            }
            else {
                messageDialog.show('Información', "No existen etapas ni subetapas registradas en el proyecto.");
            }
        });
    };

    $scope.GuardarValor = function (item, tipo) {

        console.log('Guardando valor para', item, 'tipo:', tipo);

        let tipoItem = item.tipo;
        let secuencial = item.idEtapa;
        var valorGuardar;

        if (tipo == 'mensaje')
            valorGuardar = item.mensaje;
        else (tipo == 'porciento')
        valorGuardar = item.porciento;

        var formData = new FormData();
        formData.append('etapa', tipoItem);
        formData.append('cliAux', $scope.proyectoId);
        formData.append('fechaInicioEta', fechaInicioEta);
        formData.append('fechaFinEta', fechaFinEta);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-etapas-proyecto",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-etapas-proyecto").modal("hide");
                $scope.cargarEtapas();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });


        //waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        //var etapa = angular.element("#select-etapas-cliente-proyecto")[0].value;

        ////var cliAux = proyectoId;
        //var fechaInicioEta = angular.element("#fecha-inicio-etapa")[0].value;
        //var fechaFinEta = angular.element("#fecha-fin-etapa")[0].value;

        //if (fechaInicioEta > fechaFinEta) {
        //    $scope.informacion = "La fecha de inicio no puede ser mayor que la fecha final"
        //    angular.element("#modal-info-proyectos").modal("show");
        //};

        //var formData = new FormData();
        //formData.append('etapa', etapa);
        //formData.append('cliAux', $scope.proyectoId);
        //formData.append('fechaInicioEta', fechaInicioEta);
        //formData.append('fechaFinEta', fechaFinEta);

        //var ajaxEnvioDatos = $http({
        //    method: 'POST',
        //    url: "user/guardar-etapas-proyecto",
        //    data: formData,
        //    headers: { 'Content-Type': undefined },
        //    transformRequest: angular.identity
        //});

        //ajaxEnvioDatos.success(function (data) {
        //    waitingDialog.hide();
        //    if (data.success === true) {
        //        angular.element("#modal-agregar-etapas-proyecto").modal("hide");
        //        $scope.cargarEtapas();
        //    } else {
        //        messageDialog.show("Información", data.msg);
        //    }
        //});
    };

    $scope.abrirModalAgregarSubTarea = function (etapaId) {
        $scope.etapaId = etapaId,
        $scope.cargarSubEtapas();
        angular.element("#modal-sub-etapas-proyecto").modal("show");
    };

    $scope.windowAgregarEtapasProyecto = function (proyectoId) {
        $scope.proyectoId = proyectoId,
        $scope.etapasPro = '';
        $scope.fechaIniEta = '';
        $scope.fechaFinEta = '';

        angular.element("#modal-agregar-etapas-proyecto").modal("show");
    };

    $scope.windowAgregarSubEtapasProyecto = function (etapaId) {
        $scope.etapaId = etapaId;
        $scope.descripcionSubE = '';
        $scope.recursosSubE = '';
        $scope.fechaIniSubE = '';
        $scope.fechaFinSubE = '';

        angular.element("#modal-agregar-sub-etapas-proyecto").modal("show");
    };

    $scope.cargarSubEtapas = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        $scope.loading.show();
        var subetapas = $http.post("user/sub-etapas-proyectos-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroSubEtapas,
                idEtapa: $scope.etapaId
            });
        subetapas.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;

                $scope.subEtapas = data.subetapas;
                $scope.cantPaginasSE = Math.ceil(data.total / numerosPorPagina);
                if ($scope.cantPaginasSE === 0) {
                    $scope.cantPaginasSE = 1;
                }

                $scope.listaPaginasSE = [];
                if ($scope.cantPaginasSE > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginasSE.push(i);
                    }
                }
                else if ($scope.cantPaginasSE < 5) {
                    for (var i = 1; i <= $scope.cantPaginasSE; i++) {
                        $scope.listaPaginasSE.push(i);
                    }
                }
                else if ($scope.cantPaginasSE > 5) {
                    for (var i = pagina - 4; i <= pagina; i++) {
                        $scope.listaPaginasSE.push(i);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginasSE) {
                    pagina = $scope.cantPaginasSE;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-user-sub-etapas-proyectos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.GuardarNuevaSubEtapa = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var recurso = angular.element("#select-recursos-sub-etapas")[0].value;
        var fechaInicioEta = angular.element("#fecha-inicio-subetapa")[0].value;
        var fechaFinEta = angular.element("#fecha-fin-subetapa")[0].value;

        console.log("etapa id " + $scope.etapaId);

        var formData = new FormData();
        formData.append('descripcion', $scope.descripcionSubE);
        formData.append('recurso', recurso);
        formData.append('etapaId', $scope.etapaId);
        formData.append('fechaIni', fechaInicioEta);
        formData.append('fechaFin', fechaFinEta);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-sub-etapas-proyecto",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-sub-etapas-proyecto").modal("hide");
                $scope.cargarSubEtapas();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    $scope.EliminarEtapaProyecto = function (etapaId) {
        var qMsgDialog = new questionMsgDialog();

        qMsgDialog.show('Información', 'Está seguro de querer eliminar la etapa del proyecto. Al eliminar la etapa se eliminarán todas las subetapas de esta etapa.', 'Si, estoy seguro', function () {
            waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'success' });
            var eliminarEtapa = $http.post("user/eliminar-etapa-proyecto", {
                idEtapa: etapaId
            });
            eliminarEtapa.success(function (data) {
                waitingDialog.hide();
                qMsgDialog.hide();
                if (data.success === true) {
                    messageDialog.show('Información', data.msg);
                    $scope.cargarEtapas();
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        });
    };

    $scope.EliminarSubEtapaProyecto = function (subEtapasId) {
        var qMsgDialog = new questionMsgDialog();

        qMsgDialog.show('Información', 'Está seguro de querer eliminar la subetapa.', 'Si, estoy seguro', function () {
            waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'success' });
            var eliminarSubEtapa = $http.post("user/eliminar-sub-etapa-proyecto", {
                idSubEtapa: subEtapasId
            });
            eliminarSubEtapa.success(function (data) {
                waitingDialog.hide();
                qMsgDialog.hide();
                if (data.success === true) {
                    messageDialog.show('Información', data.msg);
                    $scope.cargarSubEtapas();
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        });
    };

    //El Paginador
    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.cargarProyectos(start, lenght);
    };

    $scope.paginarEtapas = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.cargarEtapas(start, lenght);
    };

    $scope.paginarSubEtapas = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.cargarSubEtapas(start, lenght);
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

    $scope.cambiarPaginaEtapas = function (pag) {
        pagina = pag;
        $scope.paginarEtapas();
    };

    $scope.atrazarPaginaEtapas = function () {
        if (pagina > 1) {
            pagina--;
            $scope.paginarEtapas();
        }
    };

    $scope.avanzarPaginaEtapas = function () {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.paginarEtapas();
        }
    };

    $scope.cambiarPaginaSubEtapas = function (pag) {
        pagina = pag;
        $scope.paginarSubEtapas();
    };

    $scope.atrazarPaginaSubEtapas = function () {
        if (pagina > 1) {
            pagina--;
            $scope.paginarSubEtapas();
        }
    };

    $scope.avanzarPaginaSubEtapas = function () {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.paginarSubEtapas();
        }
    };

    $scope.selectAll = false;

    $scope.toggleAll = function () {
        angular.forEach($scope.datosAplanados, function (item) {
            item.selected = $scope.selectAll;
        });
    };

    $scope.toggleSubEtapas = function (etapa) {
        angular.forEach($scope.datosAplanados, function (item) {
            if (item.tipo === 'subetapa' && item.idEtapa === etapa.idEtapa) {
                item.selected = etapa.selected;
            }
        });
    };

    $scope.cargarDatosInforme = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        console.log("id de la etapa en subetapa " + $scope.etapaId);

        $scope.loading.show();
        var subetapas = $http.post("user/sub-etapas-proyectos-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroSubEtapas,
                idEtapa: $scope.etapaId
            });
        subetapas.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;

                $scope.subEtapas = data.subetapas;
                $scope.cantPaginasSE = Math.ceil(data.total / numerosPorPagina);
                if ($scope.cantPaginasSE === 0) {
                    $scope.cantPaginasSE = 1;
                }

                $scope.listaPaginasSE = [];
                if ($scope.cantPaginasSE > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginasSE.push(i);
                    }
                }
                else if ($scope.cantPaginasSE < 5) {
                    for (var i = 1; i <= $scope.cantPaginasSE; i++) {
                        $scope.listaPaginasSE.push(i);
                    }
                }
                else if ($scope.cantPaginasSE > 5) {
                    for (var i = pagina - 4; i <= pagina; i++) {
                        $scope.listaPaginasSE.push(i);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginasSE) {
                    pagina = $scope.cantPaginasSE;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-user-sub-etapas-proyectos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
}]);

//Filters
//Filter de angular para las fechas
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

//Filter de tamaño de texto
devApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);