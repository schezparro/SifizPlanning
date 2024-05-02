devApp.controller('proyectosController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var start = 0;
    var length = 10;

    $scope.nuevoTiempo = {
        Servicio: "",
        HorasServicio: "",
        RP: "",
        HorasConsumidas: "",
        Detalle: "",
        Colaborador: "",
        FechaRegistro: ""
    };

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

    angular.element('#fecha-inicio-etapa-info').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-fin-etapa-info').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-inicio-subetapa-info').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-fin-subetapa-info').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    $scope.updateMinDateSubEtapa = function () {

        if ($scope.fechaIniSubE < $scope.fechaInicioEtapa) {
            $scope.fechaIniSubE = $scope.fechaInicioEtapa;
        }

        if ($scope.fechaFinSubE > $scope.fechaFinEtapa) {
            $scope.fechaFinSubE = $scope.fechaFinEtapa;
        }

        angular.element('#fecha-inicio-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
        angular.element('#fecha-inicio-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);
        angular.element('#fecha-fin-subetapa').datepicker('setStartDate', $scope.fechaIniSubE);
        angular.element('#fecha-fin-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);
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
            $scope.etapasCat = data.etapasCat;
        }
    });

    //RECURSOS SUBETAPAS
    var ajaxrecursosSubEtapas = $http.post("user/tipo-recursos-subetapas", {});
    ajaxrecursosSubEtapas.success(function (data) {
        if (data.success === true) {
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
        $scope.proyectoModal = proyecto;

        var esLider = $http.post("user/usuario-es-lider",
            {
                secuencialProyecto: proyecto.id
            });
        esLider.success(function (data) {
            if (data.success === true || data.esAdmin === true) {
                $scope.mostrarBotones = true;
            } else {
                $scope.mostrarBotones = false;
            }
        });

        angular.element("#modal-proyectos").modal('show');
    };

    $scope.cargarEtapas = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

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

        console.log("id etapa par guardar en el editar de etapas " + $scope.etapaId);

        var formData = new FormData();
        if ($scope.etapaId != undefined)
            formData.append('secuencial', $scope.etapaId);
        else
            formData.append('secuencial', 0);

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

    $scope.abrirModalTiemposProyecto = function (proyectoId) {
        $scope.projId = proyectoId
        $scope.cargarTiemposProyecto();
        angular.element("#modal-tiempos-proyecto").modal("show");
    };

    $scope.abrirModalGenerarInforme = function (proyectoId) {
        $scope.proyectoId = proyectoId
        $scope.cargarEtapas();
        angular.element("#modal-etapas-proyecto").modal("show");
    };

    $scope.abrirModalGenerarReporte = function (proyectoId) {
        $scope.proyectoId = proyectoId;

        $scope.cargarDatosInforme();
    };


    $scope.GuardarValor = function (tipo) {

        console.log('Guardando valor para', $scope.itemModificado, 'tipo:', tipo);

        var formData = new FormData();
        formData.append('tipo', tipo);

        if (tipo === 'etapa') {

            formData.append('seleccionado', $scope.itemModificado.selected);
            formData.append('secuencialEtapaId', $scope.itemModificado.idEtapa);
            formData.append('detalle', $scope.detalleEtaInfo);

        } else if (tipo === 'subetapa') {

            formData.append('seleccionado', $scope.itemModificado.selected);
            formData.append('secuencialSubEtapaId', $scope.itemModificado.idSubEtapa);
            formData.append('porciento', $scope.porcentajeSubEInfo);
            formData.append('detalle', $scope.detalleSubEInfo);
        }

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-valor-informe",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {

                if (tipo === 'etapa')
                    angular.element("#modal-editar-etapas-proyecto").modal("hide");
                else
                    angular.element("#modal-agregar-subetapas-proyecto").modal("hide");

                $scope.cargarDatosInforme();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    $scope.abrirModalAgregarSubTarea = function (etapa) {
        $scope.etapaId = etapa.id;
        $scope.fechaInicioEtapa = etapa.fechaInicio;
        $scope.fechaFinEtapa = etapa.fechaFin;
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

    $scope.windowAgregarSubEtapasProyecto = function () {
        $scope.subEtapaId = 0;
        $scope.etapaId = $scope.etapaId;
        $scope.descripcionSubE = '';
        $scope.recursosSubE = '';
        $scope.fechaIniSubE = '';
        $scope.fechaFinSubE = '';

        angular.element('#fecha-inicio-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
        angular.element('#fecha-inicio-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);
        angular.element('#fecha-fin-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
        angular.element('#fecha-fin-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);

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

        var formData = new FormData();
        if ($scope.subEtapaId != undefined)
            formData.append('secuencial', $scope.subEtapaId);
        else
            formData.append('secuencial', 0);

        formData.append('descripcion', $scope.descripcionSubE);
        formData.append('recurso', recurso);
        formData.append('etapaId', $scope.etapaId);
        formData.append('fechaIni', fechaInicioEta);
        formData.append('fechaFin', fechaFinEta);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-sub-etapas-proyecto/",
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

    $scope.itemModificado = null;
    $scope.editarEtapasProyectos = function (etapa) {
        $scope.itemModificado = etapa;
        $scope.proyectoId = etapa.secuencialClienteAux,
            $scope.etapaId = etapa.id,
            $scope.etapasPro = etapa.secuencialEtapa;
        $scope.fechaIniEta = etapa.fechaInicio;
        $scope.fechaFinEta = etapa.fechaFin;

        angular.element("#modal-agregar-etapas-proyecto").modal("show");

    };

    $scope.editarSubEtapasProyectos = function (subetapa) {
        $scope.itemModificado = subetapa;
        $scope.etapaId = subetapa.secuencialEtapa;
        $scope.subEtapaId = subetapa.id;
        $scope.descripcionSubE = subetapa.descripcion;
        $scope.recursosSubE = subetapa.recursoId;
        $scope.fechaIniSubE = subetapa.fechaInicio;
        $scope.fechaFinSubE = subetapa.fechaFin;

        angular.element('#fecha-inicio-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
        angular.element('#fecha-inicio-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);
        angular.element('#fecha-fin-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
        angular.element('#fecha-fin-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);

        angular.element("#modal-agregar-sub-etapas-proyecto").modal("show");
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

    $scope.PrevisualizarInforme = function (subEtapasId) {
        var subInforme = $http.post("user/dar-cronograma-proyecto", {
            idProyecto: $scope.proyectoId
        });
        subInforme.success(function (data) {
            if (data.success === true) {

                $scope.cliente = data.datosGenerales.cliente;
                $scope.lider = data.datosGenerales.liderProyecto;
                $scope.responsable = data.datosGenerales.responsableProyecto;
                $scope.fecha = (function () {
                    var fechaActual = new Date();
                    var dia = fechaActual.getDate();
                    var mes = fechaActual.getMonth() + 1;
                    var año = fechaActual.getFullYear();

                    dia = (dia < 10) ? '0' + dia : dia;
                    mes = (mes < 10) ? '0' + mes : mes;

                    return dia + '/' + mes + '/' + año;
                })();

                var datosInforme = [];
                data.datosCronograma.forEach(function (etapa) {

                    var colorEstado = "";
                    var porciento = parseFloat(etapa.porciento);
                    var diasRestantes = etapa.duracion;

                    if ((diasRestantes <= 0 && porciento < 100) || (porciento >= 0 && porciento <= 39)) {
                        colorEstado = 'red';
                    } else if (porciento >= 80 && porciento <= 100) {
                        colorEstado = 'green';
                    } else if (porciento >= 40 && porciento < 80) {
                        colorEstado = 'yellow';
                    }

                    // Agrega la etapa al arreglo aplanado
                    datosInforme.push({
                        tipo: 'etapa',
                        descripcion: etapa.descripcion,
                        duracion: etapa.duracion,
                        fechaInicio: etapa.fechaInicio,
                        fechaFin: etapa.fechaFin,
                        detalle: etapa.detalle,
                        idEtapa: etapa.idEtapa,
                        idCatEtapa: etapa.idCatEtapa,
                        porciento: etapa.porciento,
                        selected: etapa.selected,
                        colorEstado: colorEstado
                    });

                    // Agrega cada subetapa al arreglo aplanado
                    etapa.subEtapas.forEach(function (subetapa) {

                        var colorEstado = "";
                        var porciento = parseFloat(subetapa.porciento);
                        var diasRestantes = subetapa.duracion;

                        if ((diasRestantes <= 0 && porciento < 100) || (porciento >= 0 && porciento <= 39)) {
                            colorEstado = 'red';
                        } else if (porciento >= 80 && porciento <= 100) {
                            colorEstado = 'green';
                        } else if (porciento >= 40 && porciento < 80) {
                            colorEstado = 'yellow';
                        }

                        datosInforme.push({
                            tipo: 'subetapa',
                            descripcion: subetapa.descripcionSE,
                            duracion: subetapa.duracionSE,
                            fechaInicio: subetapa.fechaInicioSE,
                            fechaFin: subetapa.fechaFinSE,
                            recurso: subetapa.recursoSE,
                            idRecurso: subetapa.idRecurso,
                            idEtapa: subetapa.idEtapaSE,
                            idSubEtapa: subetapa.idSubEtapa,
                            detalle: subetapa.detalle,
                            porciento: subetapa.porciento,
                            selected: subetapa.selected,
                            colorEstado: colorEstado
                        });
                    });
                });

                $scope.datosCronograma = datosInforme;
                angular.element("#cronogramaModal").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.generarInformePDF = function () {
        // Crear una nueva instancia de jsPDF
        const { jsPDF } = window.jspdf;
        var doc = new jsPDF();

        // Crear un objeto de mapeo para convertir nombres de colores a valores hexadecimales
        var colorMapping = {
            'red': '#FF0000',
            'yellow': '#FFFF00',
            'green': '#008000'
            // Agrega más colores según sea necesario
        };

        // Obtener la tabla por su ID
        var table = document.getElementById('tabla-cronograma-proyecto');

        // Utilizar autoTable para añadir la tabla al documento PDF
        doc.autoTable({
            html: table,
            head: 'INFORME AVANCE DE PROYECTO', // Agrega una fila de encabezado con el título
            didParseCell: function (data) {
                // Verificar si la celda es la última de su fila
                if (data.column.index === data.table.columns.length - 1) {
                    // Obtener el valor del color del input oculto
                    var colorInput = data.cell.raw.getElementsByClassName('color-estado')[0];
                    if (colorInput) {
                        var colorName = colorInput.value;
                        // Convertir el nombre del color a su valor hexadecimal usando el objeto de mapeo
                        var color = colorMapping[colorName];
                        if (color) {
                            // Aplicar el color a la celda
                            data.cell.styles.fillColor = color;
                        }
                    }
                }
            },
            didDrawCell: function (data) {
                // Verificar si la celda es una celda de encabezado
                if (data.section === 'head') {
                    // Verificar si la fila es una de las cuatro primeras filas de encabezado
                    if (data.row.index < 4) {
                        // Eliminar el color de fondo de la celda
                        data.cell.styles.fillColor = null;
                    }
                }
            }
        });

        // Guardar el PDF
        doc.save('tabla-cronograma-proyecto.pdf');
    };

    $scope.nombreFichero;
    $scope.pathFichero;

    $scope.generarInforme = function () {
        var subInforme = $http.post("user/dar-excel-informe-proyecto", {
            secuencial: $scope.proyectoId
        });
        subInforme.success(function (data) {
            if (data.success === true) {
                messageDialog.show('Información', "Excel generado correctamente.");
                $scope.nombreFichero = data.newNameFile;
                $scope.pathFichero = data.path;
                window.open(data.path);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //$scope.enviarInforme = function () {
    //    console.log($scope.nombreFichero);
    //    if ($scope.nombreFichero != null) {
    //        var envioCorreo = $http.post("user/enviar-correo-informe", {
    //            nombreFichero: $scope.nombreFichero
    //        });
    //        envioCorreo.success(function (data) {
    //            if (data.success === true) {

    //                messageDialog.show('Información', data.msg);

    //            }
    //        });

    //    } else {
    //        var subInforme = $http.post("user/dar-excel-informe-proyecto", {
    //            secuencial: $scope.proyectoId
    //        });
    //        subInforme.success(function (data) {
    //            if (data.success === true) {
    //                console.log("excel generado correctamente " + data.newNameFile + " --- " + data.path);
    //                $scope.nombreFichero = data.newNameFile;
    //                $scope.pathFichero = data.path;

    //                // Abre el archivo Excel en una nueva pestaña del navegador
    //                window.open($scope.pathFichero, '_blank');

    //                var envioCorreo = $http.post("user/enviar-correo-informe", {
    //                    nombreFichero: $scope.nombreFichero
    //                });
    //                envioCorreo.success(function (data) {
    //                    if (data.success === true) {

    //                        messageDialog.show('Información', data.msg);

    //                    }
    //                });
    //            }
    //            else {
    //                messageDialog.show('Información', data.msg);
    //            }
    //        });
    //    }
    //};


    $scope.enviarInforme = function () {
        // Crear una nueva instancia de jsPDF
        const { jsPDF } = window.jspdf;
        var doc = new jsPDF();

        // Crear un objeto de mapeo para convertir nombres de colores a valores hexadecimales
        var colorMapping = {
            'red': '#FF0000',
            'yellow': '#FFFF00',
            'green': '#008000'
            // Agrega más colores según sea necesario
        };

        // Obtener la tabla por su ID
        var table = document.getElementById('tabla-cronograma-proyecto');

        // Utilizar autoTable para añadir la tabla al documento PDF
        doc.autoTable({
            html: table,
            head: 'INFORME AVANCE DE PROYECTO', // Agrega una fila de encabezado con el título
            didParseCell: function (data) {
                // Verificar si la celda es la última de su fila
                if (data.column.index === data.table.columns.length - 1) {
                    // Obtener el valor del color del input oculto
                    var colorInput = data.cell.raw.getElementsByClassName('color-estado')[0];
                    if (colorInput) {
                        var colorName = colorInput.value;
                        // Convertir el nombre del color a su valor hexadecimal usando el objeto de mapeo
                        var color = colorMapping[colorName];
                        if (color) {
                            // Aplicar el color a la celda
                            data.cell.styles.fillColor = color;
                        }
                    }
                }
            },
            didDrawCell: function (data) {
                // Verificar si la celda es una celda de encabezado
                if (data.section === 'head') {
                    // Verificar si la fila es una de las cuatro primeras filas de encabezado
                    if (data.row.index < 4) {
                        // Eliminar el color de fondo de la celda
                        data.cell.styles.fillColor = null;
                    }
                }
            }
        });

        // Guardar el PDF
        doc.save('tabla-cronograma-proyecto.pdf');

        // Obtener el PDF como un ArrayBuffer
        var pdfArrayBuffer = doc.output('arraybuffer');

        // Crear un Blob a partir del ArrayBuffer
        var blob = new Blob([pdfArrayBuffer], { type: 'application/pdf' });

        // Crear un objeto FormData
        var formData = new FormData();
        // Agregar el Blob del PDF al FormData
        formData.append('pdf', blob, 'tabla-cronograma-proyecto.pdf');

        console.log("pueba");

        // Agregar cualquier otro dato que necesites enviar al servidor
        // formData.append('otroDato', valor);

        // Enviar el FormData mediante AJAX
        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/enviar-correo-informe/",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
               
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };
   
    $scope.exportarPdf = function () {
        var doc = new jsPDF();
        var element = document.getElementById('tabla-cronograma-proyecto');
        var opt = {
            margin: 1,
            filename: 'cronograma.pdf',
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2 },
            jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
        };
        html2pdf().set(opt).from(element).save();
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
        angular.forEach($scope.datosInforme, function (item) {
            item.selected = $scope.selectAll;
        });

        $scope.actualizarSeleccionInforme("all", $scope.selectAll, "0");
    };
    $scope.toggleSubEtapas = function (etapa) {
        $scope.actualizarSeleccionInforme("subetapa", etapa.selected, etapa.idSubEtapa);
    };

    $scope.toggleEtapas = function (etapa) {
        angular.forEach($scope.datosInforme, function (item) {
            if (item.tipo === 'subetapa' && item.idEtapa === etapa.idEtapa) {
                item.selected = etapa.selected;
            }
        });

        $scope.actualizarSeleccionInforme("etapa", etapa.selected, etapa.idEtapa);
    };

    $scope.actualizarSeleccionInforme = function (tipo, seleccionado, secuencial) {

        var formData = new FormData();
        formData.append('tipo', tipo);
        formData.append('seleccionado', seleccionado);
        formData.append('secuencial', secuencial);

        var actualizarSeleccion = $http({
            method: 'POST',
            url: "user/actualizar-seleccion-informe",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });
        actualizarSeleccion.success(function (data) {
            if (data.success === true) {
                console.log("prueba entrar despues del metodo ");
                $scope.cargarDatosInforme();
            }
        });
    }

    $scope.abrirModalEditarItem = function (item) {
        $scope.itemModificado = item;

        if (item.tipo === 'etapa') {
            $scope.proyectoIdInfo = item.secuencialClienteAux;
            $scope.etapaId = item.etapaId;
            $scope.etapaDescripcion = item.descripcion;
            $scope.etapasProInfo = item.idCatEtapa;
            $scope.fechaIniEtaInfo = item.fechaInicio;
            $scope.fechaFinEtaInfo = item.fechaFin;
            $scope.porcentajeEtaInfo = item.porciento;
            $scope.detalleEtaInfo = item.detalle;

            angular.element("#modal-editar-etapas-proyecto").modal("show");
        } else if (item.tipo === 'subetapa') {
            $scope.etapaId = item.etapaId;
            $scope.descripcionSubEInfo = item.descripcion;
            $scope.recursosSubEInfo = item.idRecurso;
            $scope.recursosDescSubEInfo = item.recurso;
            $scope.fechaIniSubEInfo = item.fechaInicio;
            $scope.fechaFinSubEInfo = item.fechaFin;
            $scope.porcentajeSubEInfo = item.porciento;
            $scope.detalleSubEInfo = item.detalle;

            angular.element('#fecha-inicio-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
            angular.element('#fecha-inicio-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);
            angular.element('#fecha-fin-subetapa').datepicker('setStartDate', $scope.fechaInicioEtapa);
            angular.element('#fecha-fin-subetapa').datepicker('setEndDate', $scope.fechaFinEtapa);

            angular.element("#modal-agregar-subetapas-proyecto").modal("show");
        }
    };

    $scope.cargarDatosInforme = function () {
        var obtenerDatosInforme = $http.post("user/dar-datos-proyecto", {
            idProyecto: $scope.proyectoId
        });
        obtenerDatosInforme.success(function (data) {
            if (data.success === true) {

                var datosInforme = [];
                data.datosInforme.forEach(function (etapa) {
                    // Agrega la etapa al arreglo aplanado
                    datosInforme.push({
                        tipo: 'etapa',
                        descripcion: etapa.descripcion,
                        duracion: etapa.duracion,
                        fechaInicio: etapa.fechaInicio,
                        fechaFin: etapa.fechaFin,
                        detalle: etapa.detalle,
                        idEtapa: etapa.idEtapa,
                        idCatEtapa: etapa.idCatEtapa,
                        porciento: etapa.porciento,
                        selected: etapa.selected
                    });

                    // Agrega cada subetapa al arreglo aplanado
                    etapa.subEtapas.forEach(function (subetapa) {
                        datosInforme.push({
                            tipo: 'subetapa',
                            descripcion: subetapa.descripcionSE,
                            duracion: subetapa.duracionSE,
                            fechaInicio: subetapa.fechaInicioSE,
                            fechaFin: subetapa.fechaFinSE,
                            recurso: subetapa.recursoSE,
                            idRecurso: subetapa.idRecurso,
                            idEtapa: subetapa.idEtapaSE,
                            idSubEtapa: subetapa.idSubEtapa,
                            detalle: subetapa.detalle,
                            porciento: subetapa.porciento,
                            selected: subetapa.selected
                        });
                    });
                });

                $scope.datosInforme = datosInforme;
                angular.element("#modal-informe-proyecto").modal("show");
            }
            else {
                messageDialog.show('Información', "No existen etapas ni subetapas registradas en el proyecto.");
            }
        });
    };

    $scope.getSemaforoStyle = function (item) {
        var style = {};
        var porciento = parseFloat(item.porciento);
        var diasRestantes = item.duracion;

        if ((diasRestantes <= 0 && porciento < 100) || (porciento >= 0 && porciento <= 39)) {
            style.backgroundColor = 'red';
            style.width = '10px';
            style.height = '10px';
            style.borderRadius = '50%';
        } else if (porciento >= 80 && porciento <= 100) {
            style.backgroundColor = 'green';
            style.width = '10px';
            style.height = '10px';
            style.borderRadius = '50%';
        } else if (porciento >= 40 && porciento < 80) {
            style.backgroundColor = 'yellow';
            style.width = '10px';
            style.height = '10px';
            style.borderRadius = '50%';
        }

        return style;
    };

    $scope.getBackgroundStyle = function (item) {
        var style = {};
        var porciento = parseFloat(item.porciento);
        var diasRestantes = item.duracion;

        if ((diasRestantes <= 0 && porciento < 100) || (porciento >= 0 && porciento <= 39)) {
            style.backgroundColor = 'red';
        } else if (porciento >= 80 && porciento <= 100) {
            style.backgroundColor = 'green';
        } else if (porciento >= 40 && porciento < 80) {
            style.backgroundColor = 'yellow';
        }

        return style;
    };


    $scope.agregarTiempos = function () {
        var servicioSeleccionado = 0;
        var colaboradorSeleccionado = 0;
        if ($scope.nuevoTiempo.catalogoServicio !== "") {
            servicioSeleccionado = $scope.servicios.find(function (servicio) {
                return servicio.id === $scope.nuevoTiempo.catalogoServicio;
            });
        }
        if ($scope.nuevoTiempo.colaborador !== "") {
            colaboradorSeleccionado = $scope.colaboradores.find(function (colaborador) {
                return colaborador.id === $scope.nuevoTiempo.colaborador;
            });
        }


        var nuevoTiempo = {
            Servicio: servicioSeleccionado,
            HorasServicio: $scope.nuevoTiempo.horasServicio || 0,
            RP: $scope.nuevoTiempo.Rp,
            HorasConsumidas: $scope.nuevoTiempo.horasConsumidas || 0,
            Detalle: $scope.nuevoTiempo.detalle,
            Colaborador: colaboradorSeleccionado,
            FechaRegistro: $scope.nuevoTiempo.fechaRegistro ?
                new Date(...$scope.nuevoTiempo.fechaRegistro.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)) :
                null,
            editable: false
        };

        var formData = new FormData();
        formData.append("servicio", nuevoTiempo.Servicio.id);
        formData.append("horasServicio", nuevoTiempo.HorasServicio);
        formData.append("rp", nuevoTiempo.RP);
        formData.append("horasConsumidas", nuevoTiempo.HorasConsumidas);
        formData.append("detalle", nuevoTiempo.Detalle);
        formData.append("colaborador", nuevoTiempo.Colaborador.id);
        formData.append("fechaRegistro", nuevoTiempo.FechaRegistro);
        formData.append('cliAux', $scope.projId);

        var ajaxTiempos = $http({
            method: 'POST',
            url: "user/agregar-tiempo-proyecto",
            data: formData,

            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxTiempos.success(function (data) {
            if (data.success === true) {
                $scope.paginar()
                $scope.nuevoTiempo = {
                    catalogoServicio: "",
                    horasServicio: "",
                    Rp: "",
                    horasConsumidas: "",
                    detalle: "",
                    colaborador: "",
                    fechaRegistro: "",
                    editable: false
                };

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    var ajaxCatalogoServicios = $http.post("user/dar-catalogo-servicio-proyectos", {});

    ajaxCatalogoServicios.success(function (data) {
        if (data.success === true) {
            $scope.servicios = data.servicios;
        }
    });

    var ajaxCatalogoServicios = $http.post("catalogos/dar-colaboradores", {});
    ajaxCatalogoServicios.success(function (data) {
        if (data.success === true) {
            $scope.colaboradores = data.colaboradores;
        }
    });

    $scope.cargarTiemposProyecto = function () {
        var ajaxTiempo = $http.post("user/cargar-tiempo-proyecto", {
            start: start,
            length: length,
            filtro: $scope.filtroTiempos,
            idProyecto: $scope.projId
        });

        ajaxTiempo.success(function (data) {
            if (data.success === true) {

                $scope.tmpProyecto = data.tiempoProyecto;
                $scope.totalTiempos = data.cantidad;

                var posPagin = pagina;
                $scope.cantPaginasTiempos = Math.ceil($scope.totalTiempos / numerosPorPagina);

                if ($scope.cantPaginasTiempos === 0 || $scope.cantPaginasTiempos === undefined) {
                    $scope.cantPaginasTiempos = 1;
                }

                $scope.listaPaginasTiempos = [];
                if ($scope.cantPaginasTiempos > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginasTiempos.push(i);
                    }
                }
                else if ($scope.cantPaginasTiempos <= 5) {
                    for (var i = 1; i <= $scope.cantPaginasTiempos; i++) {
                        $scope.listaPaginasTiempos.push(i);
                    }
                }
                else if ($scope.cantPaginasTiempos > 5) {
                    for (var i = pagina - 4; i <= pagina; i++) {
                        $scope.listaPaginasTiempos.push(i);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginasTiempos) {
                    pagina = $scope.cantPaginasTiempos;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-tiempos-proyecto .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            } else {
                messageDialog.show("Información", data.msg);
            }
        })
    };

    $scope.guardarCambiosTiempo = function (tiempo) {
        angular.element('#fecha-registro-' + tiempo.id).datepicker('destroy');
        tiempo.editable = false;

        var formData = new FormData();
        formData.append("ID", tiempo.id);
        formData.append("servicio", tiempo.servicio.id);
        formData.append("horasServicio", tiempo.horasServicio);
        formData.append("rp", tiempo.rp);
        formData.append("horasConsumidas", tiempo.horasConsumidas);
        formData.append("detalle", tiempo.detalle);
        formData.append("colaborador", tiempo.colaborador.id);
        formData.append("fechaRegistro", new Date(...tiempo.fechaRegistro.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)));


        var ajaxTiemposPrj = $http({
            method: 'POST',
            url: "user/editar-tiempos-proyecto",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxTiemposPrj.success(function (data) {
            if (data.success === true) {
                $scope.paginar()

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    $scope.atrazarPagina = function () {
        if (pagina > 1) {
            pagina--;
            $scope.paginar();
        }
    };

    $scope.avanzarPagina = function () {
        if (pagina < $scope.cantPaginasTiempos) {
            pagina++;
            $scope.paginar();
        }
    };

    $scope.actualizarCantidadMostrar = function () {
        numerosPorPagina = $scope.cantidadMostrarPorPagina;
        $scope.paginar();
    };

    $scope.paginar = function () {
        start = (pagina - 1) * numerosPorPagina;
        length = numerosPorPagina;

        $scope.cargarTiemposProyecto();
    };


    $scope.eliminarTiemposProyecto = function (id) {
        var ajaxTmpProyecto = $http.post("user/eliminar-tiempos-proyecto", {
            id: id
        });

        ajaxTmpProyecto.success(function (data) {
            if (data.success === true) {
                $scope.cargarTiemposProyecto()
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };


    $scope.editarTiemposProyecto = function (tiempo, index) {
        tiempo.editable = true;
        tiempo.fechaRegistro = '';
        angular.element('#fecha-registro-' + index).datepicker({
            format: 'dd/mm/yyyy',
            language: 'es'
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