var ticketApp = angular.module('ticket', []);

ticketApp.config(function ($provide, $httpProvider) {

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

ticketApp.controller('ticketController', ['$scope', '$http', function ($scope, $http) {
    var taskProxie = angular.element.connection.websocket;
    angular.element.connection.hub.start();

    //Mensaje para notificaciones por falla en el envío de correos
    taskProxie.client.erroresEnvioEmail = function (msg) {
        messageDialog.show('Información', msg);
    };
    //Mensaje para notificaciones por falla en el envío de correos
    taskProxie.client.nuevoComentarioMuyImportante = function (msg) {
        ejecutarSonido();
    };

    //Variables para el trabajo del historico
    $scope.idTicketHistorico = 0;

    //Funciones para el menú de navegación.
    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('#menu-principal [role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    //Los Datepicker
    angular.element('#fecha-asignacion-tarea-ticket').datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element('#fechaRevisado').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

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

    //Cargando los tipos de version
    var ajaxTicketVersionCliente = $http.post("catalogos/ticketVersionCliente", {});
    ajaxTicketVersionCliente.success(function (data) {
        if (data.success === true) {
            $scope.ticketVersionClientes = data.ticketVersionClientes;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });

    function ocultar() {
        angular.element("#panel_home").addClass('invisible');
        angular.element("#panel_seguimiento").addClass('invisible');
        angular.element("#panel_estimaciones").addClass('invisible');
        angular.element("#panel_seguimiento_gestor").addClass('invisible');
        angular.element("#panel_adicion").addClass('invisible');
        angular.element("#panel_cotizacion").addClass('invisible');
        angular.element("#panel_resolucion_ticket").addClass('invisible');
        angular.element("#panel_ticket_en_aprobacion").addClass('invisible');
        angular.element("#panel_ticket_en_espera").addClass('invisible');
        angular.element("#panel_biTicket").addClass('invisible');
        angular.element("#panel_resumenTicket").addClass('invisible');
        angular.element("#panel_reporteTicket").addClass('invisible');
    };

    //Funciones de Menu
    $scope.IrInicio = function () {
        $scope.loading.show();
        ocultar();
        angular.element("#panel_home").removeClass('invisible');
        $scope.funcionalidad = 'INICIO';
        actualizarGraficosInicio();
        $scope.loading.hide();
    };

    $scope.IrInicio();

    //Funcion para la actualización de los gráficos de la primera página
    function actualizarGraficosInicio() {
        var jQ = angular.element;

        //Cargando la información relativa a los gadgets
        var ajaxGaugesData = $http.post("ticket/gaugesTicket/", {});
        ajaxGaugesData.success(function (data) {
            if (data.success === true) {
                data.gaugesDataList.forEach(function (gaugeItem, index) {
                    var id = "gauge" + index;
                    var divHtml = jQ('<div id="' + id + '" style="width: 180px; height: 160px; float: left"></div>');
                    jQ("#datos-gauges").append(divHtml);
                    dibujarGauge(id, gaugeItem.nombre, gaugeItem.textoEstado, gaugeItem.minimo, gaugeItem.maximo, gaugeItem.indicador);
                });
            }
            else {
                alert("Error en el acceso a los datos.")
            }
        });
    };

    function dibujarGauge(id, nombre, sufix, minValue, maxValue, valueSelected) {
        var gaugeOptions = {

            chart: {
                type: 'solidgauge'
            },

            title: null,

            pane: {
                center: ['50%', '85%'],
                size: '100%',
                startAngle: -90,
                endAngle: 90,
                background: {
                    backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || '#EEE',
                    innerRadius: '60%',
                    outerRadius: '100%',
                    shape: 'arc'
                }
            },

            tooltip: {
                enabled: false
            },

            // the value axis
            yAxis: {
                stops: [
                    [0.1, '#55BF3B'], // green
                    [0.5, '#DDDF0D'], // yellow
                    [0.9, '#DF5353'] // red
                ],
                lineWidth: 0,
                minorTickInterval: null,
                tickAmount: 2,
                title: {
                    y: -70
                },
                labels: {
                    y: 16
                }
            },

            plotOptions: {
                solidgauge: {
                    dataLabels: {
                        y: 5,
                        borderWidth: 0,
                        useHTML: true
                    }
                }
            }
        };

        // The speed gauge
        var chartSpeed = Highcharts.chart(id, Highcharts.merge(gaugeOptions, {
            yAxis: {
                min: minValue,
                max: maxValue,
                title: {
                    text: nombre
                }
            },

            credits: {
                enabled: false
            },

            series: [{
                name: 'Speed',
                data: [valueSelected],
                dataLabels: {
                    format: '<div style="text-align:center;"><span style="font-size:12px;color:' +
                        ((Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black') + '">{y}</span><br/>' +
                        '<span style="font-size:10px;color:silver">' + sufix + '</span></div>'
                },
                tooltip: {
                    valueSuffix: ' ' + sufix
                }
            }]

        }));
    };

    $scope.IrGestionarTickets = function () {
        ocultar();
        angular.element("#panel_seguimiento").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE TICKETS';
    };

    $scope.IrGestionarEstimaciones = function () {
        ocultar();
        angular.element("#panel_estimaciones").removeClass('invisible');
        $scope.funcionalidad = 'ESTIMACIONES';
    };

    $scope.IrGestionarTicketsGestor = function () {
        ocultar();
        angular.element("#panel_seguimiento_gestor").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE TICKETS';
    };

    $scope.IrAdicionarTickets = function () {
        ocultar();
        angular.element("#panel_adicion").removeClass('invisible');
        $scope.funcionalidad = 'ADICIÓN DE TICKETS';
    };

    $scope.IrGestionTicketsResueltos = function () {
        ocultar();
        angular.element("#panel_resolucion_ticket").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE TICKETS RESUELTOS';
    };

    $scope.IrGestionTicketsEnAprobacion = function () {
        ocultar();
        angular.element("#panel_ticket_en_aprobacion").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE TICKETS EN APROBACIÓN';
    };

    $scope.IrGestionTicketsEnEspera = function () {
        ocultar();
        angular.element("#panel_ticket_en_espera").removeClass('invisible');
        $scope.funcionalidad = 'GESTIÓN DE TICKETS EN ESPERA';
    };

    $scope.IrCotizarTickets = function () {
        $scope.loading.show();
        ocultar();
        angular.element("#panel_cotizacion").removeClass('invisible');
        $scope.funcionalidad = 'COTIZACIÓN DE TICKETS';

        //cargando las cotizaciones del usuario
        $scope.cargarCotizacionesUsuario();
    };
    $scope.cargarCotizacionesUsuario = function (filtro) {
        if (filtro === undefined)
            filtro = "";
        var ajaxCotizacionesUsuario = $http.post("tickets/cotizaciones-ticket",
            { filtro: filtro });
        ajaxCotizacionesUsuario.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.ticketsCotizables = data.cotizaciones
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.IrBI = function () {
        ocultar();
        angular.element("#panel_biTicket").removeClass('invisible');
        $scope.funcionalidad = 'CUADROS DE MANDO';
    };

    $scope.IrResumenTicket = function () {
        ocultar();
        angular.element("#panel_resumenTicket").removeClass('invisible');
        $scope.funcionalidad = 'RESUMEN DE TICKETS';
    };

    $scope.IrReporteTicket = function () {
        ocultar();
        angular.element("#panel_reporteTicket").removeClass('invisible');
        $scope.funcionalidad = 'HORAS DE MANTENIMIENTO';
    };

    //Cargando los datos iniciales para la gestión de los tickets
    //Cargando los lugares
    var ajaxLugares = $http.post("catalogos/lugares", {});
    ajaxLugares.success(function (data) {
        if (data.success === true) {
            $scope.lugares = data.lugares;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los lugares.");
        }
    });

    //Cargando las actividades
    var ajaxActividades = $http.post("catalogos/actividades", {});
    ajaxActividades.success(function (data) {
        if (data.success === true) {
            $scope.actividades = data.actividades;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a las actividades");
        }
    });

    //Cargando los modulos
    var ajaxModulos = $http.post("catalogos/modulos", {});
    ajaxModulos.success(function (data) {
        if (data.success === true) {
            $scope.modulos = data.modulos;
        }
        else {
            //alert("Error en el acceso a los datos.")
        }
    });

    //Cargando los coordinadores
    var ajaxCoordinadores = $http.post("catalogos/coordinadores", {});
    ajaxCoordinadores.success(function (data) {
        if (data.success === true) {
            $scope.coordinadores = data.coordinadores;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los coordinadores");
        }
    });

    //Cargando los colaboradores
    var ajaxColaboradores = $http.post("tickets/colaboradores-asignaciones/", {});
    ajaxColaboradores.success(function (data) {
        if (data.success === true) {
            $scope.colaboradores = data.colaboradores;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los colaboradores");
        }
    });

    //Cargando los clientes
    var ajaxClientes = $http.post("catalogos/clientes", {});
    ajaxClientes.success(function (data) {
        if (data.success === true) {
            $scope.clientes = data.clientes;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los coordinadores");
        }
    });

    //Cargando las prioridades de los tickets
    var ajaxPrioridades = $http.post("tickets/prioridades-ticket", {});
    ajaxPrioridades.success(function (data) {
        if (data.success === true) {
            $scope.prioridades = data.prioridades;
            setTimeout(function () {
                $('.selectpicker').selectpicker('refresh');
            }, 1000);
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los coordinadores");
        }
    });

    //Cargando las categorias de los tickets
    var ajaxCategorias = $http.post("tickets/categorias-ticket", {});
    ajaxCategorias.success(function (data) {
        if (data.success === true) {
            $scope.categorias = data.categorias;
            setTimeout(function () {
                $('.selectpicker').selectpicker('refresh');
            }, 1000);
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los coordinadores");
        }
    });

    //Cargando los cotizadores de los tickets
    var ajaxCotizadores = $http.post("tickets/cotizadores-ticket", {});
    ajaxCotizadores.success(function (data) {
        if (data.success === true) {
            $scope.cotizadores = data.cotizadores;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los cotizadores");
        }
    });

    //Cargando las proximas actividades
    var ajaxProximaActividad = $http.post("catalogos/datos-catalogos/",
        {
            nombre: 'PROXIMA ACTIVIDAD'
        });
    ajaxProximaActividad.success(function (data) {
        if (data.success === true) {
            $scope.proximasActividades = data.datos;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a las proximas actividades");
        }
    });

    //Cargando los tipos de recursos
    var ajaxTipoRecurso = $http.post("catalogos/datos-catalogos/",
        {
            nombre: 'TIPO RECURSO'
        });
    ajaxTipoRecurso.success(function (data) {
        if (data.success === true) {
            $scope.tiposRecursos = data.datos;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los tipos de recursos");
        }
    });

    //Cargando los estados de los tickets
    var ajaxEstadoTicket = $http.post("catalogos/datos-catalogos/",
        {
            nombre: 'ESTADO TICKET'
        });
    ajaxEstadoTicket.success(function (data) {
        if (data.success === true) {
            $scope.estadosTickets = data.datos;
            setTimeout(function () {
                $('.selectpicker').selectpicker('refresh');
            }, 1000);

        }
        else {
            messageDialog.show('Información', "No se pudo acceder a las proximas actividades");
        }
    });

    $scope.verInformacionTicket = function () {
        angular.element("#informacion-ticket").modal("show");
        //$scope.cargarEventosTickets();
    };

    //Cargando el Historico del ticket
    $scope.idVersionTicketAddHistorico = -1;
    $scope.verHistoricoTicket = function () {
        angular.element("#modal-historico-ticket").modal("show");
        $scope.cargarEventosTickets();
    };
    angular.element('#modal-historico-ticket').on('hidden.bs.modal', function (e) {
        angular.element("#datos-historico-ticket").html("");
        $scope.eventosHistorico = [];
        $scope.numeroHistoricoTicket = "";
        $scope.clienteHistoricoTicket = "";
        $scope.categoriaHistoricoTicket = "";
        $scope.prioridadHistoricoTicket = "";
        $scope.asuntoHistoricoTicket = "";
        $scope.idVersionTicketAddHistorico = -1;
    })
    //Cargando Eventos del ticket
    $scope.cargarEventosTickets = function () {
        angular.element("#datos-historico-ticket").html("");
        $scope.eventosHistorico = [];

        var eventosTickets = $http.post("tickets/eventos-historicos-ticket", {
            idTicket: $scope.idTicketHistorico,
            filtro: $scope.filtroEventoTicket
        });
        eventosTickets.success(function (data) {
            if (data.success === true) {
                $scope.eventosHistorico = data.eventos;
                $scope.numeroHistoricoTicket = data.numero;
                $scope.clienteHistoricoTicket = data.cliente;
                $scope.categoriaHistoricoTicket = data.categoria;
                $scope.prioridadHistoricoTicket = data.prioridad;
                $scope.asuntoHistoricoTicket = data.asunto;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    //Cargando Datos del evento del ticket
    $scope.verDatosEventoTicket = function (tipo, idObjeto) {

        $scope.ActualizarEstiloEventos(idObjeto);

        if (tipo === 1) {
            $scope.idVersionTicketAddHistorico = idObjeto;
        }
        else {
            $scope.idVersionTicketAddHistorico = -1;
        }
        var eventoTicket = $http.post("tickets/datos-evento-historico-ticket", {
            idTicket: $scope.idTicketHistorico,
            tipo: tipo,
            secuencialObjeto: idObjeto
        });
        eventoTicket.success(function (data) {
            if (data.success === true) {
                angular.element("#datos-historico-ticket").html(data.datos);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.ActualizarEstiloEventos = function (idObjeto) {
        for (var i = 0; i < $scope.eventosHistorico.length; i++) {
            if (idObjeto !== i) {
                angular.element("#evento" + $scope.eventosHistorico[i].SecuencialObjeto).removeClass("evento-historico-selected");
                angular.element("#evento" + $scope.eventosHistorico[i].SecuencialObjeto).addClass("evento-historico");
            }
        }
        angular.element("#evento" + idObjeto).removeClass("evento-historico");
        angular.element("#evento" + idObjeto).addClass("evento-historico-selected");
    };

    //Adicionando Informacion Historico
    $scope.adicionarEmailHistorico = function () {
        if ($scope.idVersionTicketAddHistorico === -1) {
            messageDialog.show('Información', "Seleccione un evento historico del ticket, (fondo azul).");
        }
        else {
            angular.element('#modal-historico-informacion-ticket').modal('show');
        }
    };
    $scope.grabarInformacionHistorico = function () {
        waitingDialog.show('Grabando la información al histórico...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();
        formData.append('idTicket', $scope.idTicketHistorico);
        formData.append('secuencialObjeto', $scope.idVersionTicketAddHistorico);
        formData.append('texto', $scope.textoInformacionHistorico);
        angular.element.each(angular.element('#modal-historico-informacion-ticket').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });

        var ajaxEmailHistorico = $http.post("tickets/grabar-informacion-historico",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        ajaxEmailHistorico.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.textoInformacionHistorico = "";
                angular.element("#form-grabar-historico").trigger("reset");
                angular.element("#modal-historico-informacion-ticket").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.adicionarFileAnterior = function () {
        var listaBotones = angular.element("#barra-botones-file");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element('#deleteInputFileAnterior').removeAttr('disabled');
        }
    };
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

    function ejecutarSonido() {
        var sonido = document.getElementById('audio-chat');
        sonido.play();
    }
}]);

ticketApp.controller('gestionTicketController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
    $scope.idTicket = 0;
    $scope.tickets = [];
    $scope.listaPaginas = [];
    $scope.botones = [];
    $scope.idEstimacion = 0;
    $scope.totalEstimacionTicket = 0;
    $scope.totalEstimaciones = 0;
    $scope.mostrarTodos = false;
    $scope.totalHorasAsignadas = 0;

    $scope.wind2Opciones = new questionMsgDialog();

    var orderStatus = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

    var filterTickets = {
        numero: '',
        cliente: '',
        fecha: '',
        asunto: '',
        asignado: '',
        prioridad: '',
        categoria: '',
        estado: '',
        proximaActividad: '',
        ticketVersionCliente: ''
    };

    angular.element('#fecha-asignacion-ticket').datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element('#fecha-limite-estimacion').datepicker({
        format: 'dd/mm/yyyy'
    });

    $scope.resetearAdjunto = function () {
        if (document.getElementById('selectedFile').files[0].name.length > 20) {
            document.getElementById('buttonAdjuntoModal').innerHTML = "..." + document.getElementById('selectedFile').files[0].name.substring(document.getElementById('selectedFile').files[0].name.length - 17, document.getElementById('selectedFile').files[0].name.length);
            document.getElementById('buttonAdjunto').innerHTML = "..." + document.getElementById('selectedFile').files[0].name.substring(document.getElementById('selectedFile').files[0].name.length - 17, document.getElementById('selectedFile').files[0].name.length);
        } else {
            document.getElementById('buttonAdjuntoModal').innerHTML = document.getElementById('selectedFile').files[0].name.substring(0, 20);
            document.getElementById('buttonAdjunto').innerHTML = "..." + document.getElementById('selectedFile').files[0].name.substring(document.getElementById('selectedFile').files[0].name.length - 17, document.getElementById('selectedFile').files[0].name.length);
        }
    }

    //funcion para cargar los tickets desde el servidor
    $scope.recargarTickets = function (start, lenght, filtro, order, asc) {
        $scope.loading.show();
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {
            if (filterTickets.numero == undefined)
                filterTickets.numero = '';
            if (filterTickets.cliente == undefined)
                filterTickets.cliente = '';
            if (filterTickets.fecha == undefined)
                filterTickets.fecha = '';
            if (filterTickets.asunto == undefined)
                filterTickets.asunto = '';
            if (filterTickets.asignado == undefined)
                filterTickets.asignado = '';

            filtro = angular.toJson(filterTickets)
        }
        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;
        var buscarTickets = $http.post("tickets/dar-tickets",
            {
                start: start,
                lenght: lenght,
                filtro: filtro,
                order: order,
                asc: asc,
                todos: $scope.mostrarTodos,
                tipoFacturable: $scope.filtroFacturacion
            });
        buscarTickets.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;
                $scope.tickets = data.tickets;

                $scope.cantPaginas = Math.ceil(data.totalTickets / numerosPorPagina);

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

                $scope.totalTickets = data.totalTickets;

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-gestion-ticket .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

                //Solo debe llenarse una vez al inicio con las proximas actividades existentes en los tickets
                if ($scope.proximasActividadesFilter == undefined) {
                    $scope.proximasActividadesFilter = data.proximasActividades;
                }

                setTimeout(function () {
                    $('.selectpicker').selectpicker('refresh');
                }, 1000);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }
    $scope.recargarTickets();
    $scope.mostrarTodosLosTickets = function () {
        $scope.recargarTickets();
    };


    $scope.actualizarCantidadMostrar = function () {
        numerosPorPagina = $scope.cantidadMostrarPorPagina;
        $scope.recargarTickets();
    }


    $scope.mostrarPorFacturados = function () {
        $scope.recargarTickets();
    };

    //Ordenando las columnas
    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna");
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
        $scope.recargarTickets(start, lenght, "", globalOrder, globalAsc);
    };

    $("#mySelectCategoria").on("hide.bs.select", function () {
        $scope.filterData(7);
    });
    $("#mySelectEstado").on("hide.bs.select", function () {
        $scope.filterData(8);
    });
    $("#mySelectPrioridad").on("hide.bs.select", function () {
        $scope.filterData(9);
    });
    $("#mySelectedVersionCliente").on("hide.bs.select", function () {
        $scope.filterData(10);
    });

    //Añadiendo valores a cada uno de los filtros
    $scope.filterData = function (valor) {

        if ($scope.numero != undefined) {
            filterTickets.numero = $scope.numero;
        }

        if ($scope.cliente != undefined) {
            filterTickets.cliente = $scope.cliente;
        }

        if ($scope.fecha != undefined) {
            filterTickets.fecha = $scope.fecha;
        }

        if ($scope.asunto != undefined) {
            filterTickets.asunto = $scope.asunto;
        }

        if ($scope.asignado != undefined) {
            filterTickets.asignado = $scope.asignado;
        }

        switch (valor) {
            case 1:
                filterTickets.numero = $scope.numero;
                break;
            case 2:
                filterTickets.cliente = $scope.cliente;
                break;
            case 3:
                filterTickets.fecha = $scope.fecha;
                break;
            case 4:
                filterTickets.asunto = $scope.asunto;
                break;
            case 5:
                filterTickets.asignado = $scope.asignado;
                break;
            case 6:
                filterTickets.prioridad = $scope.selectedPrioridad != '' ? $scope.selectedPrioridad : null;
                break;
            case 7:
                filterTickets.categoria = $scope.selectedCategoria != '' ? $scope.selectedCategoria : null;
                break;
            case 8:
                filterTickets.estado = $scope.selectedEstado != '' ? $scope.selectedEstado : null;
                break;
            case 9:
                filterTickets.proximaActividad = $scope.selectedPA != '' ? $scope.selectedPA : null;
                break;
            case 10:
                filterTickets.ticketVersionCliente = $scope.selectedVersionCliente != '' ? $scope.selectedVersionCliente : null;
                break;
        };

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarTickets(start, lenght, "", globalOrder, globalAsc);
    };

    //Para el paginador
    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.recargarTickets(start, lenght, "", globalOrder, globalAsc);
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

    //Mostrando el windows de Datos del ticket
    $scope.mostrarDetalleTicket = function (idTicket) {
        $scope.$parent.loading.show();
        angular.element("#div-check-funcionalidades").html('');
        $scope.idTicket = idTicket;
        $scope.$parent.idTicketHistorico = idTicket

        //VERIFICANDO LA PROXIMA ACTIVIDAD EN EL TICKET
        var ajaxVerificarProximaActividad = $http.post("tickets/verificar-estado-proxima-actividad",
            {
                idTicket: idTicket
            });
        ajaxVerificarProximaActividad.success(function (data) {
            if (data.success === true) {
                if (data.concuerda === false) {
                    $scope.$parent.loading.hide();
                    $scope.proximasActividadesChange = data.proximasActividades;
                    $scope.mensajeNoConcuerda = data.mensaje;
                    angular.element("#modal-cambio-proxima-actividad").modal("show");
                }
                else {
                    //ACTUALIZANDO LAS PROXIMAS ACTIVIDADES
                    var ajaxProximaActividad = $http.post("tickets/dar-proxima-actividad-segun-ticket",
                        {
                            idTicket: idTicket
                        });
                    ajaxProximaActividad.success(function (data) {
                        if (data.success === true) {
                            $scope.proximasActividades = data.proximasActividades;

                            var buscarDatosTicket = $http.post("tickets/dar-datos-ticket",
                                {
                                    idTicket: idTicket
                                });
                            buscarDatosTicket.success(function (data) {
                                $scope.$parent.loading.hide();
                                if (data.success === true) {

                                    $scope.idTicketCliente = data.datosTicket.id;
                                    $scope.idCliente = data.datosTicket.idCliente;
                                    $scope.fechaTicket = data.datosTicket.fecha;
                                    if (data.fechaRevisado !== "01/01/0001") {
                                        $scope.fechaRevisado = data.fechaRevisado;
                                    } else {
                                        $scope.fechaRevisado = data.fecha;
                                    };
                                    $scope.nombreCliente = data.datosTicket.cliente;
                                    $scope.usuarioCliente = data.datosTicket.usuarioCliente;
                                    $scope.telefonoCliente = data.datosTicket.telefono;
                                    $scope.usuarioTelefono = data.datosTicket.clienteTelefono;
                                    $scope.clienteReporta = data.datosTicket.reporto;
                                    $scope.reputacionCliente = data.reputacion;
                                    $scope.reputacionTicket = data.datosTicket.reputacion;
                                    $scope.asuntoTicket = data.datosTicket.asunto;
                                    $scope.seFactura = data.datosTicket.seFactura;
                                    $scope.facturado = data.datosTicket.facturado;
                                    $scope.reprocesos = data.datosTicket.reprocesos;
                                    $scope.ingresoInterno = data.datosTicket.ingresoInterno;
                                    $scope.tipoRecurso = data.datosTicket.tipoRecurso;
                                    $scope.ticketVersionCliente = data.datosTicket.ticketVersionCliente;
                                    $scope.ticketVC = data.datosTicket.ticketVersionClienteId;
                                    $scope.diasGarantia = data.datosTicket.diasGarantia;
                                    $scope.pendienteAPago = data.datosTicket.pendienteAPago;

                                    $scope.actividadProxima = data.datosTicket.actividadProxima;
                                    $scope.actividadProximaDelTicket = false;
                                    $scope.proximasActividades.forEach(function (element) {
                                        if (element.nombre === "ESTIMAR" && element.id === data.datosTicket.actividadProxima) {
                                            $scope.actividadProximaDelTicket = true;
                                        }
                                    });
                                    $scope.esCotizar = false;
                                    $scope.proximasActividades.forEach(function (element) {
                                        if (element.nombre === "COTIZAR" && element.id === data.datosTicket.actividadProxima) {
                                            $scope.esCotizar = true;
                                        }
                                    });

                                    angular.element('[ng-model="actividadProxima"]').val(data.datosTicket.actividadProxima)

                                    $scope.estadoTicket = data.datosTicket.estado;
                                    $scope.prioridadTicket = data.datosTicket.prioridadDesc;
                                    $scope.semaforo = data.datosTicket.semaforo;
                                    $scope.semaforoResolucion = data.datosTicket.semaforoResolucion;
                                    $scope.categoriaTicket = data.datosTicket.categoriaDesc;
                                    $scope.adjuntosTicket = data.datosTicket.adjuntos;
                                    $scope.estimacionTicket = data.datosTicket.estimacion;
                                    $scope.detalleTicket = data.datosTicket.detalle;
                                    $scope.entregableTicket = data.datosTicket.entregableGarantia !== null ? data.datosTicket.entregableGarantia["nombre"] : "null";
                                    $scope.botones = data.datosTicket.botones;
                                    $scope.verificador = data.datosTicket.verificador;
                                    $scope.categoria = data.categoria !== 0 ? data.categoria : "";
                                    $scope.prioridad = data.prioridad;
                                    $scope.justificacionPrioridad = data.datosTicket.justificacionPrioridad;
                                    //$scope.error = data.error;
                                    $scope.requiereTesting = data.datosTicket.requiereTesting;

                                    $scope.asignaciones = data.asignaciones;
                                    $scope.motivoTrabajo = data.motivoTrabajo;
                                    $scope, esEstimacion = false;

                                    if (data.tecnicoSugerido !== null) {
                                        $scope.tecnicoSugerido = data.tecnicoSugerido.idTec;
                                        $scope.observaciones = data.tecnicoSugerido.observaciones;
                                    }
                                    else {
                                        $scope.tecnicoSugerido = 0;
                                        $scope.observaciones = "";
                                    }

                                    $scope.totalHorasAsignadas = 0;
                                    angular.element.each($scope.asignaciones, function (key, value) {
                                        $scope.totalHorasAsignadas += parseInt(value.numeroHoras);
                                    });
                                    document.getElementById('buttonAdjunto').innerHTML = '<span class="glyphicon glyphicon-paperclip"></span> Adjuntar';
                                    document.getElementById('selectedFile').value = "";
                                    $scope.validarContratoMantenimiento();

                                    angular.element("#modal-gestion-ticket").modal('show');

                                    //Actualizando los colaboradores asignados
                                    setTimeout(function () {
                                        angular.element.each($scope.asignaciones, function (key, value) {
                                            var selector = '#panel-colaboradores-ticket [type="checkbox"][value="' + value.idColab + '"]';
                                            angular.element(selector).click();
                                        });
                                    }, 500);

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
                }
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Eliminar Adjunto Ticket
    $scope.eliminarAdjunto = function (idAdjunto) {
        $scope.$parent.loading.show();
        var ajaxEliminarAdjunto = $http.post("tickets/eliminar-adjunto-ticket",
            {
                secuencial: idAdjunto
            });
        ajaxEliminarAdjunto.success(function (data) {
            if (data.success === true) {
                $scope.$parent.loading.hide();
                $scope.mostrarDetalleTicket($scope.idTicket);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Mostrando el windows de Datos del ticket para Asignar solamente
    $scope.mostrarDetalleTicketAsignaciones = function (idTicket) {
        $scope.$parent.loading.show();
        angular.element("#div-check-funcionalidades").html('');
        $scope.idTicket = idTicket;
        $scope.$parent.idTicketHistorico = idTicket

        //VERIFICANDO LA PROXIMA ACTIVIDAD EN EL TICKET
        var ajaxVerificarProximaActividad = $http.post("tickets/verificar-estado-proxima-actividad",
            {
                idTicket: idTicket
            });
        ajaxVerificarProximaActividad.success(function (data) {
            if (data.success === true) {
                if (data.concuerda === false) {
                    $scope.$parent.loading.hide();
                    $scope.proximasActividadesChange = data.proximasActividades;
                    $scope.mensajeNoConcuerda = data.mensaje;
                    angular.element("#modal-cambio-proxima-actividad").modal("show");
                }
                else {
                    //ACTUALIZANDO LAS PROXIMAS ACTIVIDADES
                    var ajaxProximaActividad = $http.post("tickets/dar-proxima-actividad-segun-ticket",
                        {
                            idTicket: idTicket
                        });
                    ajaxProximaActividad.success(function (data) {
                        if (data.success === true) {
                            $scope.proximasActividades = data.proximasActividades;

                            var buscarDatosTicket = $http.post("tickets/dar-datos-ticket",
                                {
                                    idTicket: idTicket
                                });
                            buscarDatosTicket.success(function (data) {
                                $scope.$parent.loading.hide();
                                if (data.success === true) {
                                    $scope.idTicketCliente = data.datosTicket.id;
                                    $scope.idCliente = data.datosTicket.idCliente;
                                    $scope.fechaTicket = data.datosTicket.fecha;
                                    $scope.nombreCliente = data.datosTicket.cliente;
                                    $scope.usuarioCliente = data.datosTicket.usuarioCliente;
                                    $scope.telefonoCliente = data.datosTicket.telefono;
                                    $scope.usuarioTelefono = data.datosTicket.clienteTelefono;
                                    $scope.clienteReporta = data.datosTicket.reporto;
                                    $scope.reputacionCliente = data.reputacion;
                                    //$scope.reputacionTicket = data.datosTicket.reputacion;
                                    $scope.asuntoTicket = data.datosTicket.asunto;
                                    //$scope.seFactura = data.datosTicket.seFactura;
                                    //$scope.facturado = data.datosTicket.facturado;
                                    $scope.reprocesos = data.datosTicket.reprocesos;
                                    //$scope.ingresoInterno = data.datosTicket.ingresoInterno;
                                    //$scope.tipoRecurso = data.datosTicket.tipoRecurso;
                                    //$scope.diasGarantia = data.datosTicket.diasGarantia;

                                    $scope.actividadProxima = data.datosTicket.actividadProxima;
                                    angular.element('[ng-model="actividadProxima"]').val(data.datosTicket.actividadProxima)

                                    $scope.estadoTicket = data.datosTicket.estado;
                                    $scope.prioridadTicket = data.datosTicket.prioridadDesc;
                                    $scope.categoriaTicket = data.datosTicket.categoriaDesc;
                                    $scope.adjuntosTicket = data.datosTicket.adjuntos;
                                    $scope.detalleTicket = data.datosTicket.detalle;
                                    $scope.botones = data.datosTicket.botones;
                                    $scope.verificador = data.datosTicket.verificador;

                                    $scope.asignaciones = data.asignaciones;

                                    $scope.totalHorasAsignadas = 0;
                                    angular.element.each($scope.asignaciones, function (key, value) {
                                        $scope.totalHorasAsignadas += parseInt(value.numeroHoras);
                                    });

                                    angular.element("#modal-gestion-ticket").modal('show');

                                    //Actualizando los colaboradores asignados
                                    setTimeout(function () {
                                        angular.element.each($scope.asignaciones, function (key, value) {
                                            var selector = '#panel-colaboradores-ticket [type="checkbox"][value="' + value.idColab + '"]';
                                            angular.element(selector).click();
                                        });
                                    }, 500);

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
                }
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Cambiando la proxima actividad de un ticket
    $scope.cambiarProximaActividadTicket = function () {
        var ajaxCambiarProximaActividad = $http.post("tickets/cambiar-proxima-actividad",
            {
                idTicket: $scope.idTicket,
                idProximaActividad: $scope.changeProximaActividad
            });
        ajaxCambiarProximaActividad.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-cambio-proxima-actividad").modal("hide");
                $scope.mostrarDetalleTicketAsignaciones($scope.idTicket);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Mostrando la window con los datos de las asignaciones del colaborador seleccionado
    $scope.addMes = 0; //Para sumar o restar meses
    $scope.idCola = 0; //Para el id del colaborador
    $scope.idSede = 0; //Para el id del colaborador
    $scope.verAsignacionesColaborador = function (idColab, idSede) {
        $scope.idCola = idColab;
        $scope.idSede = idSede;
        var buscarDatosTicket = $http.post("tickets/asignaciones-colaborador",
            {
                idColaborador: idColab,
                addMes: $scope.addMes
            });

        buscarDatosTicket.success(function (data) {
            if (data.success === true) {
                $scope.mesCalendar = data.mesCalendar;
                $scope.semanasAsignaciones = data.semanasAsignaciones;
                angular.element("#modal-asignacion-colaboradores").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.irMesAtras = function () {
        $scope.addMes -= 1;
        $scope.verAsignacionesColaborador($scope.idCola);
    };
    $scope.irMesAlante = function () {
        $scope.addMes += 1;
        $scope.verAsignacionesColaborador($scope.idCola);
    };

    //Funcion para guardar la información general de un ticket
    $scope.guardarTicket = function () {
        waitingDialog.show('Grabando los datos del ticket...', { dialogSize: 'sm', progressType: 'success' });
        var formData = new FormData();
        formData.append('idTicket', $scope.idTicket);
        formData.append('prioridad', $scope.prioridad);
        formData.append('categoria', $scope.categoria);
        formData.append('actividadProxima', $scope.actividadProxima);
        formData.append('tipoRecurso', $scope.tipoRecurso);
        formData.append('ticketVC', $scope.ticketVC);
        formData.append('reputacion', $scope.reputacionTicket);
        formData.append('estimacion', $scope.estimacionTicket);
        formData.append('seFactura', $scope.seFactura);
        formData.append('facturado', $scope.facturado);
        formData.append('reprocesos', $scope.reprocesos);
        formData.append('ingresoInterno', $scope.ingresoInterno);
        formData.append('fechaRevisado', $scope.fechaRevisado);
        formData.append('colaborador', $scope.tecnicoSugerido);
        formData.append('observaciones', $scope.observaciones);
        formData.append('error', $scope.error);
        formData.append('requiereTesting', $scope.requiereTesting);
        formData.append('pendienteAPago', $scope.pendienteAPago);
        formData.append('adjuntos', document.getElementById('selectedFile').files[0]);
        formData.append('esEstimacion', $scope.esEstimacion);
        formData.append('diasGarantia', $scope.diasGarantia);

        var guardarTicket = $http.post("tickets/guardar-datos-ticket",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });

        guardarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                document.getElementById('buttonAdjuntoModal').innerHTML = 'Seleccione el archivo';
                $scope.esEstimacion = false;
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets                
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funcion para pasar un ticket a abierto
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="abrirTicket"]', function () {
        $scope.$apply(function () {
            waitingDialog.show('Abriendo Ticket...', { dialogSize: 'sm', progressType: 'success' });
            var ajaxAbrirTicket = $http.post("tickets/abrir-ticket",
                {
                    idTicket: $scope.idTicket,
                    reputacion: $scope.reputacionTicket
                });

            ajaxAbrirTicket.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-gestion-ticket").modal('hide');
                    //Recargando la lista de tickets                
                    setTimeout(function () {
                        $scope.mostrarDetalleTicket($scope.idTicket);
                    }, 500);
                    $scope.paginar();
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        });
    });

    //Funciones para asignar un ticket a varias tareas
    $scope.asignaciones = [];
    $scope.adicionarTareaAsignacion = function () {
        if ($scope.estimacionTicket <= $scope.totalHorasAsignadas) {
            messageDialog.show('Información', "No le queda tiempo disponible para más asignaciones");
            return false;
        }
        angular.element("#modal-new-task-ticket").modal("show");
        angular.element('[ng-model="vecesRepetirAsignacion"]').prop('disabled', false);
        $scope.idAsignacion = 0;
    };
    $scope.guardarTareaPorTicket = function () {
        $scope.totalHorasAsignadas = 0;
        angular.element.each($scope.asignaciones, function (key, value) {
            $scope.totalHorasAsignadas += parseInt(value.numeroHoras);
        });

        if ($scope.idAsignacion === 0) {
            if ($scope.estimacionTicket < $scope.totalHorasAsignadas + parseInt($scope.numeroHorasAsignacion)) {
                messageDialog.show('Información', "El tiempo total de las tareas supera el tiempo de estimación");
                return false;
            }

            var cant = 0;
            var diferencia = $scope.estimacionTicket - $scope.totalHorasAsignadas;
            while (cant < $scope.vecesRepetirAsignacion && diferencia > 0) {
                if (diferencia > parseInt($scope.numeroHorasAsignacion)) {
                    var id = $scope.asignaciones.length + 1;
                    var nuevaTarea = {
                        id: id,
                        idColaborador: $scope.colaboradorAsignacion,
                        colaborador: angular.element('[ng-model="colaboradorAsignacion"] option:selected').html(),
                        fecha: $scope.fechaAsignacion,
                        ubicacion: $scope.ubicacionAsignacion,
                        idActividad: $scope.actividadAsignacion,
                        actividad: angular.element('[ng-model="actividadAsignacion"] option:selected').html(),
                        idModulo: $scope.moduloAsignacion,
                        modulo: angular.element('[ng-model="moduloAsignacion"] option:selected').html(),
                        numeroHoras: $scope.numeroHorasAsignacion,
                        coordinador: $scope.coordinadorAsignacion !== undefined ? $scope.coordinadorAsignacion : 0,
                        detalle: $scope.detalleAsignacion
                    };
                    $scope.asignaciones.push(nuevaTarea);
                    diferencia -= parseInt($scope.numeroHorasAsignacion);
                }
                else if (diferencia > 0) {
                    var id = $scope.asignaciones.length + 1;
                    var nuevaTarea = {
                        idTarea: 0,
                        id: id,
                        idColaborador: $scope.colaboradorAsignacion,
                        colaborador: angular.element('[ng-model="colaboradorAsignacion"] option:selected').html(),
                        fecha: $scope.fechaAsignacion,
                        ubicacion: $scope.ubicacionAsignacion,
                        idActividad: $scope.actividadAsignacion,
                        actividad: angular.element('[ng-model="actividadAsignacion"] option:selected').html(),
                        idModulo: $scope.moduloAsignacion,
                        modulo: angular.element('[ng-model="moduloAsignacion"] option:selected').html(),
                        numeroHoras: diferencia,
                        coordinador: $scope.coordinadorAsignacion !== undefined ? $scope.coordinadorAsignacion : 0,
                        detalle: $scope.detalleAsignacion
                    };
                    $scope.asignaciones.push(nuevaTarea);
                    diferencia -= diferencia;
                }
                cant++;
            }
        }
        else {
            var horasAntigua = $scope.asignaciones[parseInt($scope.idAsignacion) - 1].numeroHoras;

            if ($scope.estimacionTicket < ($scope.totalHorasAsignadas - horasAntigua) + parseInt($scope.numeroHorasAsignacion)) {
                messageDialog.show('Información', "El tiempo total de las tareas supera el tiempo de estimación");
                return false;
            }

            $scope.asignaciones[parseInt($scope.idAsignacion) - 1] = {
                idTarea: 0,
                id: $scope.idAsignacion,
                idColaborador: $scope.colaboradorAsignacion,
                colaborador: angular.element('[ng-model="colaboradorAsignacion"] option:selected').html(),
                fecha: $scope.fechaAsignacion,
                ubicacion: $scope.ubicacionAsignacion,
                idActividad: $scope.actividadAsignacion,
                actividad: angular.element('[ng-model="actividadAsignacion"] option:selected').html(),
                idModulo: $scope.moduloAsignacion,
                modulo: angular.element('[ng-model="moduloAsignacion"] option:selected').html(),
                numeroHoras: $scope.numeroHorasAsignacion,
                coordinador: $scope.coordinadorAsignacion !== undefined ? $scope.coordinadorAsignacion : 0,
                detalle: $scope.detalleAsignacion
            };
        }

        $scope.totalHorasAsignadas = 0;
        angular.element.each($scope.asignaciones, function (key, value) {
            $scope.totalHorasAsignadas += parseInt(value.numeroHoras);
        });

        $scope.colaboradorAsignacion = "";
        $scope.fechaAsignacion = "";
        $scope.ubicacionAsignacion = "";
        $scope.actividadAsignacion = "";
        $scope.moduloAsignacion = "";
        $scope.numeroHorasAsignacion = "";
        $scope.coordinadorAsignacion = 0;
        $scope.detalleAsignacion = "";
        $scope.idAsignacion = 0;
        $scope.vecesRepetirAsignacion = 1;
        $scope.totalHorasAsignacion = 0;

        angular.element("#modal-new-task-ticket").modal("hide");
    };

    $scope.eliminarTareaAsignacion = function (id, idTarea) {
        var pos = id - 1;
        $scope.asignaciones.splice(pos, 1);

        var i = 1;
        $scope.totalHorasAsignadas = 0;
        angular.element.each($scope.asignaciones, function (key, value) {
            value.id = i++;
            $scope.totalHorasAsignadas += parseInt(value.numeroHoras);
        });

        if (idTarea !== undefined && idTarea !== 0) {
            var buscarDatosTicket = $http.post("tickets/anular-tarea-ticket",
                {
                    idTarea: idTarea
                });
            buscarDatosTicket.success(function (data) {
                $scope.$parent.loading.hide();
                if (data.success === true) {
                    angular.element("#modal-gestion-ticket").modal('hide');
                    //Recargando la lista de tickets                
                    setTimeout(function () {
                        $scope.mostrarDetalleTicket($scope.idTicket);
                    }, 500);
                    $scope.paginar();
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
            buscarDatosTicket.error(function () {
                messageDialog.show('Información', "Error en la comunicación con el servidor.");
            });
        }
    };
    $scope.verTareaAsignacion = function (id) {
        var asignacion = $scope.asignaciones[parseInt(id) - 1];

        $scope.idAsignacion = asignacion.id;
        $scope.colaboradorAsignacion = asignacion.idColaborador;
        $scope.fechaAsignacion = asignacion.fecha;
        $scope.ubicacionAsignacion = asignacion.ubicacion;
        $scope.actividadAsignacion = asignacion.idActividad;
        $scope.moduloAsignacion = asignacion.idModulo;
        $scope.numeroHorasAsignacion = asignacion.numeroHoras;
        $scope.coordinadorAsignacion = asignacion.coordinador;
        $scope.detalleAsignacion = asignacion.detalle;

        angular.element('[ng-model="vecesRepetirAsignacion"]').prop('disabled', true);

        angular.element("#modal-new-task-ticket").modal("show");
    };

    $scope.asignarTicketProyectoTfs = function () {
        $scope.submitDisabled = true;
        //if ($scope.idProyectoTFS === undefined || $scope.idProyectoTFS === "") {
        //    messageDialog.show('Información', "Indique el proyecto en el TFS al que se asociaran las tareas.");
        //    return false;
        //}

        waitingDialog.show('Realizando asignaciones del Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxAsignacionesTicket = $http.post("tickets/asignar-tarea-ticket",
            {
                idTicket: $scope.idTicket,
                datos: angular.toJson($scope.asignaciones),
                reputacion: $scope.reputacionTicket,
                idProyectoTFS: 0
            });

        ajaxAsignacionesTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                var prioridad = $scope.prioridad;
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets                
                setTimeout(function () {
                    $scope.mostrarDetalleTicketAsignaciones($scope.idTicket);
                }, 500);
                $scope.paginar();
                $scope.prioridad = prioridad;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="asignarTicket"]', function () {
        var buscarDataTicket = $http.post("tickets/dar-datos-ticket",
            {
                idTicket: $scope.idTicket
            });
        buscarDataTicket.success(function (data) {
            if (data.success === true) {
                if (data.categoria === 0) {
                    messageDialog.show('Información', "Debe ingresar y guardar la categoría revisada para poder asignar el ticket");
                    return false;
                }
            }
        });
        if ($scope.asignaciones.length === 0) {
            messageDialog.show('Información', "Debe adicionar asignaciones para el ticket");
            return false;
        }

        //ver si no coinciden las horas totales con las estimadas
        if (parseInt($scope.estimacionTicket) !== $scope.totalHorasAsignadas) {
            $scope.wind2Opciones.show("Aviso", "Las horas de estimación no coinciden con el total de horas las asignadas. ¿Desea continuar de todos modos?", "Si, Continuar",
                function () {
                    $scope.wind2Opciones.hide();
                    //Verificando que el ticket y el usuario cumplen las condiciones para asignarse al TFS
                    //$scope.$apply(function () {
                    //    waitingDialog.show('Realizando comprobaciones de asignación del Ticket...', { dialogSize: 'sm', progressType: 'success' });
                    //    var ajaxComprobacionWorkItenTicket = $http.post("tickets/comprobar-ticket-workitem",
                    //        {
                    //            idTicket: $scope.idTicket,
                    //            datos: angular.toJson($scope.asignaciones)
                    //        });
                    //    ajaxComprobacionWorkItenTicket.success(function (data) {
                    //        waitingDialog.hide();
                    //        if (data.success === true) {
                    //            $scope.submitDisabled = false;
                    //            angular.element("#modal-comprobacion-workitem-ticket").modal('show');
                    //            //Recargando la lista de proyectos                
                    //            $scope.proyectosTFS = proyectosTfs;
                    //        }
                    //        else {
                    //            messageDialog.show('Información', data.msg);
                    //        }
                    //    });
                    //});
                    $scope.asignarTicketProyectoTfs();
                });
        }
        else {
            //Verificando que el ticket y el usuario cumplen las condiciones para asignarse al TFS
            $scope.$apply(function () {
                //waitingDialog.show('Realizando comprobaciones de asignación del Ticket...', { dialogSize: 'sm', progressType: 'success' });
                //var ajaxComprobacionWorkItenTicket = $http.post("tickets/comprobar-ticket-workitem",
                //    {
                //        idTicket: $scope.idTicket,
                //        datos: angular.toJson($scope.asignaciones)
                //    });
                //ajaxComprobacionWorkItenTicket.success(function (data) {
                //    waitingDialog.hide();
                //    if (data.success === true) {
                //        $scope.submitDisabled = false;
                //        angular.element("#modal-comprobacion-workitem-ticket").modal('show');
                //        //Recargando la lista de proyectos                
                //        $scope.proyectosTFS = data.proyectosTfs;
                //    }
                //    else {
                //        messageDialog.show('Información', data.msg);
                //    }
                //});
                $scope.asignarTicketProyectoTfs();

            });
        }
    });


    $scope.crearAsignacionDia = function (fecha) {
        if (fecha !== '-') {
            if ($scope.estimacionTicket <= $scope.totalHorasAsignadas) {
                messageDialog.show('Información', "No le queda tiempo disponible para más asignaciones");
                return false;
            }
            angular.element('[ng-model="vecesRepetirAsignacion"]').prop('disabled', false);
            angular.element("#modal-new-task-ticket").modal("show");
            $scope.idAsignacion = 0;

            $scope.colaboradorAsignacion = $scope.idCola;
            angular.element("#fecha-asignacion-tarea-ticket").datepicker('update', fecha);
            $scope.fechaAsignacion = fecha;

            $scope.ubicacionAsignacion = $scope.idSede
        }
    };
    $scope.calcularRepetir = function () {
        if (isNaN($scope.vecesRepetirAsignacion) || isNaN($scope.numeroHorasAsignacion))
            $scope.totalHorasAsignacion = 0;
        else
            $scope.totalHorasAsignacion = $scope.vecesRepetirAsignacion * $scope.numeroHorasAsignacion;
    };

    //Funciones para cotizar un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="cotizarTicket"]', function () {
        angular.element("#modal-pedir-cotizacion-ticket").modal("show");
    });
    $scope.pedirCotizarTicket = function () {
        waitingDialog.show('Pidiendo Cotización de Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxCotizandoTicket = $http.post("tickets/pedir-cotizar-ticket",
            {
                idTicket: $scope.idTicket,
                idColaborador: $scope.cotizadorTicket,
                estimacion: $scope.estimacionTicket,
                prioridad: $scope.prioridadTicket,
                categoria: $scope.categoriaTicket,
                reputacion: $scope.reputacionTicket
            });

        ajaxCotizandoTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-gestion-ticket").modal('hide');
                angular.element("#modal-pedir-cotizacion-ticket").modal('hide');
                //Recargando la lista de tickets                
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funcion para estimar un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="estimarTicket"]', function () {
        angular.element("#modal-estimacion-ticket").modal('show');
    });
    $scope.modalEstimarTicket = function () {
        angular.element("#modal-estimacion-ticket").modal('show');
    };
    $scope.estimarTicket = function () {
        var colaboradoresChecked = angular.element("#colaboradores-estimacion input:checked");
        if (colaboradoresChecked.length === 0) {
            messageDialog.show('Información', "Debe seleccionar al menos un colaborador");
        }
        else {
            waitingDialog.show('Solicitando Estimación...', { dialogSize: 'sm', progressType: 'success' });
            var idColaboradores = [];
            angular.element.each(colaboradoresChecked, function (key, value) {
                idColaboradores.push(angular.element(value).val());
            });
            var estimarTicket = $http.post("tickets/ordenar-estimar-ticket",
                {
                    idTicket: $scope.idTicket,
                    idColaboradores: angular.toJson(idColaboradores),
                    fechaLimite: $scope.fechaLimiteEstimacion,
                    prioridad: $scope.prioridadTicket,
                    categoria: $scope.categoriaTicket,
                    reputacion: $scope.reputacionTicket
                });
            estimarTicket.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.recargarTickets();
                    angular.element("#modal-estimacion-ticket").modal('hide');
                    angular.element("#modal-gestion-ticket").modal('hide');
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });

        }
    };
    angular.element("#modal-estimacion-ticket").on('hidden.bs.modal', function (e) {
        angular.element('#colaboradores-estimacion [type="checkbox"]').prop("checked", false);
    });

    //Funciones para terminar la estimacion de un ticket    
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="terminarEstimacion"]', function () {
        angular.element("#modal-terminar-estimacion-ticket").modal('show');

        //Cargando los niveles de los colaboradores
        var niveles = $http.post("tickets/niveles-colaborador",
            {});
        niveles.success(function (data) {
            if (data.success === true) {
                $scope.niveles = data.niveles
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        //Cargando las estimaciones sobre un ticket
        cargarEstimacionTicket();
    });
    angular.element("#modal-terminar-estimacion-ticket").on('hidden.bs.modal', function (e) {
        $scope.colaboradoresEstimacion = [];
        limpiarDetallesEstimacion();
    });
    //cargando las estimaciones sobre un ticket
    function cargarEstimacionTicket() {
        var estimacionesTicket = $http.post("tickets/datos-estimacion-ticket",
            {
                idTicket: $scope.idTicket
            });
        estimacionesTicket.success(function (data) {
            if (data.success === true) {
                $scope.colaboradoresEstimacion = data.estimaciones
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }
    function limpiarDetallesEstimacion() {
        $scope.idEstimacion = 0;
        $scope.totalEstimacionTicket = 0;
        $scope.entregablesAdicionales = "";
        $scope.informacionComplementaria = "";
        angular.element('#lista-detalles-tareas').html("");
        angular.element('#lista-detalles-tareas').append(angular.element('#add-informacion-estimacion-inicial').html());
    };
    //mostrar los datos de la estimación cuando se selecciona una estimacion de usuario...
    $scope.verDatosEstimacionTicket = function (idEstimacion) {
        limpiarDetallesEstimacion();
        $scope.idEstimacion = idEstimacion;
        var loadDatosEstimacionUsuarios = $http.post("tickets/datos-estimacion-usuario",
            {
                idEstimacion: idEstimacion
            });
        loadDatosEstimacionUsuarios.success(function (data) {
            if (data.success === true) {
                var detalles = data.detallesEstimacion;

                var divDetalles = angular.element('#lista-detalles-tareas');
                var hijosDivDetalles = angular.element(divDetalles).children();
                for (i = 0; i < detalles.length; i++) {
                    var detalle = detalles[i];
                    var texto = detalle.detalle;
                    var tiempo = detalle.tiempo;
                    var nivel = detalle.nivel;

                    var hijo = undefined;
                    if (i === 0) {
                        hijo = hijosDivDetalles[0];
                    }
                    else {//Adicionar una nueva fila
                        angular.element('#lista-detalles-tareas').append(angular.element('#add-informacion-estimacion').html());
                        hijo = angular.element(divDetalles).children().last();
                    }

                    angular.element(hijo).find('textarea').val(texto);
                    angular.element(hijo).find('input').val(tiempo);
                    angular.element(hijo).find('select').val(nivel);
                }
                $scope.entregablesAdicionales = data.entregables;
                $scope.informacionComplementaria = data.informacion;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-terminar-estimacion-ticket").on("click", "#colaboradores-estado-estimacion tbody tr", function () {
        angular.element("#colaboradores-estado-estimacion tbody tr").removeClass("tabla-catalogo-select");
        angular.element(this).addClass("tabla-catalogo-select");
    });
    //cambiando el valor de la estimacion
    $scope.catalogarEstimacion = function (factor) {
        waitingDialog.show('Catalogando Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var catalogarTicket = $http.post("tickets/catalogar-ticket/",
            {
                idEstimacion: $scope.idEstimacion,
                catalogar: factor
            });
        catalogarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                cargarEstimacionTicket();
                $scope.totalEstimacionTicket = data.totalEstimacion;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    //terminando la estimación del ticket, cambia a ticket estimado
    $scope.terminarEstimacionTicket = function () {
        waitingDialog.show('Terminando Estimación de Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var terminarEstimacionTicket = $http.post("tickets/terminar-estimacion-ticket/",
            {
                idTicket: $scope.idTicket,
                estimacion: $scope.totalEstimacionTicket
            });
        terminarEstimacionTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-terminar-estimacion-ticket").modal('hide');
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets                
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.mostrarDetalleEstimacion = function () {
        var detalleEstimacionTicket = $http.post("tickets/detalle-estimacion-ticket/",
            {
                idTicket: $scope.idTicket
            });
        detalleEstimacionTicket.success(function (data) {
            if (data.success === true) {
                $scope.estimacionesTicket = data.estimaciones;
                $scope.idTicketDetalle = data.idTicketDetalle;
                $scope.tiempoTotal = data.tiempoTotal;
                angular.element("#modal-detalle-estimacion").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.emailRechzarEstimacion = function () {
        $scope.comentarioEstimacionRechazada = "";
        angular.element("#modal-rechazar-estimacion").modal('show');
    };

    $scope.rechazarTodasEstimaciones = function () {
        var rechazadas = $http.post("tickets/rechazar-estimaciones-ticket/",
            {
                numero: $scope.idTicketDetalle,
                comentario: $scope.comentarioEstimacionRechazada
            });

        rechazadas.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-rechazar-estimacion").modal('hide');
                angular.element("#modal-detalle-estimacion").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.rechazarEstimacion = function (idEstimacion) {
        var rechazarEstimacion = $http.post("tickets/rechazar-estimacion-ticket/",
            {
                idEstimacion: idEstimacion,
                comentario: $scope.comentarioEstimacionRechazada
            });

        rechazarEstimacion.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-rechazar-estimacion").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //$scope.emailAceptarEstimacion = function () {
    //    var aceptarEstimacion = $http.post("tickets/aceptar-estimacion-ticket/",
    //        {
    //            idEstimacion: $scope.idEstimacionTicket
    //        });

    //    aceptarEstimacion.success(function (data) {
    //        if (data.success === true) {
    //            $scope.recargarEstimaciones();
    //        }
    //        else {
    //            messageDialog.show('Información', data.msg);
    //        }
    //    });
    //};

    //Funcion para anular un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="anularTicket"]', function () {
        angular.element("#modal-comentario-ticket-anulado").modal('show');
    });
    $scope.anularTicket = function () {
        waitingDialog.show('Anulando Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var anularTicket = $http.post("tickets/anular-ticket/",
            {
                idTicket: $scope.idTicket
            });
        anularTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                var guardarComentario = $http.post("tickets/guardar-comentario-ticket/",
                    {
                        idTicket: $scope.idTicket,
                        comentario: "TICKET ANULADO: " + $scope.comentarioTicketAnulado
                    });
                guardarComentario.success(function (data) {
                    if (data.success === true) {
                        $scope.comentarioTicketAnulado = "";
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });

                angular.element("#modal-comentario-ticket-anulado").modal('hide');
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets    
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funcion para aprobar un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="aprobarTicket"]', function () {
        waitingDialog.show('Aprobando el Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var aprobarTicket = $http.post("tickets/aprobar-ticket/",
            {
                idTicket: $scope.idTicket
            });
        aprobarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Funcion para cerrar un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="cerrarTicket"]', function () {
        waitingDialog.show('Cerrando el Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var aprobarTicket = $http.post("tickets/cerrar-ticket/",
            {
                idTicket: $scope.idTicket
            });
        aprobarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Funciones para pasar un ticket a Esperando Respuesta
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="esperarRespuestaTicket"]', function () {
        angular.element('#modal-historico-informacion-er-ticket').modal('show');
    });
    $scope.esperarRespuestaTicket = function () {
        waitingDialog.show('Cambiando el estado del Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var formData = new FormData();
        formData.append('idTicket', $scope.idTicket);
        formData.append('texto', $scope.textoInformacionER);
        angular.element.each(angular.element('#modal-historico-informacion-er-ticket').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });
        var esperandoRespuestaTicket = $http.post("tickets/esperando-respuesta-ticket/",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        esperandoRespuestaTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.textoInformacionER = '';

                while (angular.element('#modal-historico-informacion-er-ticket [name="file[]"]').length > 1) {
                    angular.element("#deleteInputFileAnteriorER").click();
                }

                angular.element('#modal-historico-informacion-er-ticket [data-dismiss="fileinput"]').click();

                angular.element("#modal-historico-informacion-er-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.adicionarFileAnteriorER = function () {
        var listaBotones = angular.element("#barra-botones-file-er");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element('#deleteInputFileAnteriorER').removeAttr('disabled');
        }
    };
    $scope.deleteFileAnteriorER = function () {
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element("#barra-botones-file-er").prev().remove();
            cantFileInput = angular.element(".file-adj-contrato:visible").length;
            if (cantFileInput < 2) {
                angular.element('#deleteInputFileAnteriorER').attr('disabled', 'disabled');
            }
        }
    };

    //Funciones para pasar un ticket a aceptado
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="aceptarTicket"]', function () {
        waitingDialog.show('Aceptar el Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var aceptarTicket = $http.post("tickets/aceptar-ticket/",
            {
                idTicket: $scope.idTicket
            });
        aceptarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });

    //Funciones para poner pendiente un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="pendienteTicket"]', function () {
        //Mostrando pantalla de comentario por ticket pendiente
        angular.element('#modal-comentario-ticket-pendiente').modal('show');
    });
    $scope.pendienteTicket = function () {
        waitingDialog.show('Pasando a Pendiente el Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var pendienteTicket = $http.post("tickets/pendiente-ticket/", {
            idTicket: $scope.idTicket,
            comentario: $scope.comentarioTicketPendiente
        });
        pendienteTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-comentario-ticket-pendiente").modal('hide');
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funciones para rechazar un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="rechazarTicket"]', function () {
        //Mostrando pantalla de comentario por rechazo del ticket
        angular.element('#modal-comentario-ticket-rechazado').modal('show');
    });
    $scope.rechazarTicket = function () {
        waitingDialog.show('Rechazando el ticket...', { dialogSize: 'sm', progressType: 'success' });
        var rechazandoTicket = $http.post("tickets/rechazar-ticket/",
            {
                idTicket: $scope.idTicket,
                comentario: $scope.comentarioTicketRechazado
            });
        rechazandoTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-comentario-ticket-rechazado").modal('hide');
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funcion para poner suspender un ticket
    angular.element("#frm-edit-tickets").on("click", '[data-id-btn="suspenderTicket"]', function () {
        waitingDialog.show('Pasando el ticket a Suspendido...', { dialogSize: 'sm', progressType: 'success' });
        var suspenderTicket = $http.post("tickets/suspender-ticket/", {
            idTicket: $scope.idTicket
        });
        suspenderTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-gestion-ticket").modal('hide');
                //Recargando la lista de tickets   
                setTimeout(function () {
                    $scope.mostrarDetalleTicket($scope.idTicket);
                }, 500);
                $scope.paginar();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    });
    //Funciones para desacuerdo con una resolucion
    $scope.mostrarPantallaDesacuerdo = function () {
        angular.element("#modal-desacuerdo-resolucion-ticket").modal('show');
        $scope.cargarDesacuerdos();
    };
    $scope.cargarDesacuerdos = function () {
        $scope.$parent.loading.show();
        var cargarDesacuerdos = $http.post("tickets/cargar-desacuerdos-ticket/",
            {
                idTicket: $scope.idTicket
            });
        cargarDesacuerdos.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.desacuerdos = data.desacuerdos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.guardarDesacuerdo = function () {
        waitingDialog.show('Grabando el desacuerdo...', { dialogSize: 'sm', progressType: 'success' });
        var guardarDesacuerdo = $http.post("tickets/guardar-desacuerdo-ticket/",
            {
                idTicket: $scope.idTicket,
                desacuerdo: $scope.desacuerdoTicket
            });
        guardarDesacuerdo.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.desacuerdoTicket = "";
                $scope.cargarDesacuerdos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funciones del adjunto
    $scope.cargarAdjunto = function () {
        angular.element("#modal-adjuntos").modal('show');
    };

    //Funciones del comentario
    $scope.verComentarioTicket = function () {
        angular.element("#modal-comentarios-ticket").modal('show');
        $scope.cargarComentarios();
    };
    $scope.cargarComentarios = function () {
        $scope.$parent.loading.show();
        var cargarComentario = $http.post("tickets/cargar-comentarios-ticket/",
            {
                idTicket: $scope.idTicket
            });
        cargarComentario.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.comentarios = data.comentarios;
                $scope.mostrarComentario = false;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.guardarComentario = function () {
        waitingDialog.show('Grabando el comentario...', { dialogSize: 'sm', progressType: 'success' });
        //console.log($scope.mostrarComentario);
        var guardarComentario = $http.post("tickets/guardar-comentario-ticket/",
            {
                idTicket: $scope.idTicket,
                comentario: $scope.comentarioTicket,
                verTodos: $scope.mostrarComentario
            });
        guardarComentario.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.comentarioTicket = "";
                $scope.cargarComentarios();
            }
            else {
                messageDialog.show('Información', data.msg);
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
                idCliente: $scope.idCliente,
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
                idCliente: $scope.idCliente,
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
    $scope.panelEmail = function (colaborador, fecha, detalle) {
        $scope.destinatariosEmailTicket = colaborador + ";";
        $scope.asuntoEmailTicket = "Comentario del Ticket:" + " " + $scope.idTicketCliente;

        var fech = new Date(parseInt(fecha.replace('/Date(', '')));
        var result = dateToStr(fech, 'dd/mm/yyyy', '/', true);
        $scope.comentarioEmailTicket = colaborador + "  " + result + ":  " + "\n" + detalle;

        angular.element("#modal-email-comentarios").modal('show');
    };
    $scope.enviarEmail = function () {
        waitingDialog.show('Enviando correo...', { dialogSize: 'sm', progressType: 'success' });
        var enviarEmail = $http.post("tickets/enviar-email-comentario/",
            {
                idTicket: $scope.idTicket,
                destinatariosEmailTicket: $scope.destinatariosEmailTicket,
                asuntoEmailTicket: $scope.asuntoEmailTicket,
                comentarioEmailTicket: $scope.comentarioEmailTicket
            });
        enviarEmail.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.cargarComentarios();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
}]);

ticketApp.controller('gestionEstimacionesController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
    $scope.estimaciones = [];
    $scope.listaPaginas = [];
    $scope.botones = [];
    $scope.idEstimacion = 0;
    $scope.mostrarTodos = false;

    $scope.wind2Opciones = new questionMsgDialog();

    var orderStatus = [1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

    var filterEstimaciones = {
        numero: '',
        colaborador: '',
        cliente: '',
        prioridad: '',
        detalle: '',
        fechaLimite: '',
        estadoEstimacion: ''
    };

    $scope.mostrarTodasLasEstimaciones = function () {
        $scope.recargarEstimaciones(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    //funcion para cargar las estimaciones desde el servidor
    $scope.recargarEstimaciones = function (start, lenght, filtro, order, asc) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {
            if (filterEstimaciones.numero == undefined)
                filterEstimaciones.numero = '';
            if (filterEstimaciones.colaborador == undefined)
                filterEstimaciones.colaborador = '';
            if (filterEstimaciones.cliente == undefined)
                filterEstimaciones.cliente = '';
            if (filterEstimaciones.prioridad == undefined)
                filterEstimaciones.prioridad = '';
            if (filterEstimaciones.detalle == undefined)
                filterEstimaciones.detalle = '';
            if (filterEstimaciones.fechaLimite == undefined)
                filterEstimaciones.fechaLimite = '';
            if (filterEstimaciones.estadoEstimacion == undefined)
                filterEstimaciones.estadoEstimacion = '';

            filtro = angular.toJson(filterEstimaciones)
        }
        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;
        var buscarEstimaciones = $http.post("tickets/estimaciones",
            {
                start: start,
                lenght: lenght,
                filtro: filtro,
                order: order,
                asc: asc,
                todos: $scope.mostrarTodos,
            });
        buscarEstimaciones.success(function (data) {
            $scope.loading.hide();

            if (data.success === true) {
                var posPagin = pagina;
                $scope.estimaciones = data.estimaciones;
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

                $scope.totalEstimaciones = data.total;

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-gestion-estimaciones .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

                setTimeout(function () {
                    $('.selectpicker').selectpicker('refresh');
                }, 1000);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }


    $scope.mostrarDetalleEstimacionTicket = function (numero, id, estado) {
        var detalleEstimacionTicket = $http.post("tickets/detalle-estimacion-ticket/",
            {
                idTicket: numero,
                estado: estado
            });

        detalleEstimacionTicket.success(function (data) {
            if (data.success === true) {
                $scope.detallesEstimaciones = data.estimaciones;
                $scope.tiempoTotal = data.tiempoTotal;
                $scope.idEstimacionTicket = id;
                $scope.permitirValidar = data.permitirValidar;

                angular.element("#modal-detalle-estimacion-gestion").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.rechazarEstimacionTicket = function () {
        angular.element("#modal-comentarios-rechazar-ticket").modal('show');
    };

    $scope.enviarCorreoRechazarEstimacionTicket = function (comentarioTicket) {

        var rechazarEstimacion = $http.post("tickets/rechazar-estimacion-ticket/",
            {
                idEstimacion: $scope.idEstimacionTicket,
                comentario: comentarioTicket
            });

        rechazarEstimacion.success(function (data) {
            if (data.success === true) {
                $scope.recargarEstimaciones();
                angular.element("#modal-comentarios-rechazar-ticket").modal('hide');
                angular.element("#modal-detalle-estimacion-gestion").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.aceptarEstimacionTicket = function () {
        var aceptarEstimacion = $http.post("tickets/aceptar-estimacion-ticket/",
            {
                idEstimacion: $scope.idEstimacionTicket
            });

        aceptarEstimacion.success(function (data) {
            if (data.success === true) {
                $scope.recargarEstimaciones();
                angular.element("#modal-detalle-estimacion-gestion").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.recargarEstimaciones();
    $scope.mostrarTodasLasEstimaciones = function () {
        $scope.recargarEstimaciones();
    };
    $scope.actualizarCantidadMostrar = function () {
        numerosPorPagina = $scope.cantidadMostrarPorPagina;
        $scope.recargarEstimaciones();
    }

    //Ordenando las columnas
    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna");
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
        $scope.recargarEstimaciones(start, lenght, "", globalOrder, globalAsc);
    };

    //Añadiendo valores a cada uno de los filtros
    $scope.filterData = function (valor) {

        if ($scope.numero != undefined) {
            filterEstimaciones.numero = $scope.numero;
        }

        if ($scope.colaborador != undefined) {
            filterEstimaciones.colaborador = $scope.colaborador;
        }

        if ($scope.cliente != undefined) {
            filterEstimaciones.cliente = $scope.cliente;
        }

        if ($scope.prioridad != undefined) {
            filterEstimaciones.prioridad = $scope.prioridad;
        }

        if ($scope.detalle != undefined) {
            filterEstimaciones.detalle = $scope.detalle;
        }

        if ($scope.fechaLimite != undefined) {
            filterEstimaciones.fechaLimite = $scope.fechaLimite;
        }

        if ($scope.estadoEstimacion != undefined) {
            filterEstimaciones.estadoEstimacion = $scope.estadoEstimacion;
        }

        switch (valor) {
            case 1:
                filterEstimaciones.numero = $scope.numero;
                break;
            case 2:
                filterEstimaciones.colaborador = $scope.colaborador;
                break;
            case 3:
                filterEstimaciones.cliente = $scope.cliente;
                break;
            case 4:
                var filtroPrioridad = angular.element('#select-prioridad-estimacion').val();
                if (filtroPrioridad === "Seleccione...") {
                    filterEstimaciones.prioridad = "";
                } else {
                    filterEstimaciones.prioridad = filtroPrioridad;
                }
                break;
            case 5:
                filterEstimaciones.detalle = $scope.detalle;
                break;
            case 6:
                filterEstimaciones.fechaLimite = $scope.fechaLimite;
                break;
            case 7:
                var filtroEstadoEstimacion = angular.element('#select-estado-estimacion').val();
                if (filtroEstadoEstimacion === "Seleccione...") {
                    filterEstimaciones.estadoEstimacion = "";
                } else {
                    filterEstimaciones.estadoEstimacion = filtroEstadoEstimacion;
                }
                break;
        };

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarEstimaciones(start, lenght, "", globalOrder, globalAsc);
    };

    //Para los que son por selección
    angular.element('#select-prioridad-estimacion').on('click', function (e) {
        $scope.filterData(4);
    });

    angular.element('#select-estado-estimacion').on('click', function (e) {
        $scope.filterData(7);
    });


    //Para el paginador
    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.recargarEstimaciones(start, lenght, "", globalOrder, globalAsc);
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

}]);

ticketApp.controller('adicionTicketController', ['$scope', '$http', function ($scope, $http) {
    $scope.adicionarFileAnteriorNewTicket = function () {
        var listaBotones = angular.element("#barra-botones-file-new-ticket");
        var html = angular.element("#htmlFileNewTicket").html();
        angular.element(html).insertBefore(listaBotones);
        var cantFileInput = angular.element(".file-adj-contrato-new-ticket:visible").length;
        if (cantFileInput > 1) {
            angular.element('#deleteInputFileAnteriorNewTicket').removeAttr('disabled');
        }
    };
    $scope.deleteFileAnteriorNewTicket = function () {
        var cantFileInput = angular.element(".file-adj-contrato-new-ticket:visible").length;
        if (cantFileInput > 1) {
            angular.element("#barra-botones-file-new-ticket").prev().remove();
            cantFileInput = angular.element(".file-adj-contrato-new-ticket:visible").length;
            if (cantFileInput < 2) {
                angular.element('#deleteInputFileAnteriorNewTicket').attr('disabled', 'disabled');
            }
        }
    };

    $scope.seleccionClienteNewTicket = function () {
        var ajaxDarUsuariosCliente = $http.post("tickets/informacion-usuarios-clientes", {
            idCliente: $scope.clienteNewTicket
        });
        ajaxDarUsuariosCliente.success(function (data) {
            if (data.success === true) {
                $scope.usuariosCliente = data.clientes;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
        ajaxDarUsuariosCliente.error(function () {
            messageDialog.show('Información', "Error en la conexión con el servidor.");
        });
    };

    $scope.idPersonaCliente = 0;
    $scope.seleccionUsuarioNewTicket = function (telefono, email) {
        var dataUser = JSON.parse($scope.usuarioNewTicket);
        $scope.idPersonaCliente = dataUser.idPersonaCliente;
        $scope.telefonoClienteNewTicket = dataUser.telefono;
        $scope.emailClienteNewTicket = dataUser.email;
    };

    $scope.adicionarTicket = function () {
        waitingDialog.show('Grabando el ticket...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();
        formData.append('idPersonaCliente', $scope.idPersonaCliente);
        formData.append('reportado', $scope.reportadoPorNewTicket);
        formData.append('telefono', $scope.telefonoNewTicket);
        formData.append('asunto', $scope.asuntoNewTicket);
        formData.append('detalle', $scope.detalleTicket);
        formData.append('prioridad', $scope.prioridadNewTicket);
        formData.append('categoria', $scope.categoriaNewTicket);
        formData.append('ticketVersionCliente', $scope.ticketVersionClienteNewTicket);

        angular.element.each(angular.element('#frm-add-new-ticket').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });

        var ajaxAdicionarTicket = $http.post("tickets/adicionar-ticket",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        ajaxAdicionarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element('[data-dismiss="fileinput"]:visible').click();
                $scope.clienteNewTicket = "";
                $scope.usuarioNewTicket = "";
                $scope.telefonoClienteNewTicket = "";
                $scope.emailClienteNewTicket = "";
                $scope.reportadoPorNewTicket = "";
                $scope.telefonoNewTicket = "";
                $scope.asuntoNewTicket = "";
                $scope.detalleTicket = "";
                $scope.prioridadNewTicket = "";
                $scope.categoriaNewTicket = "";
                $scope.ticketVersionClienteNewTicket = "";
                $scope.idPersonaCliente = 0;
                messageDialog.show('Información', data.msg);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

}]);

ticketApp.controller('ticketsResueltosController', ['$scope', '$http', function ($scope, $http) {

    $scope.buscarTicketResueltos = function () {
        if ($scope.cantDias === "" || $scope.cantDias === undefined) {
            messageDialog.show('Información', 'Establezca la cantidad de días.');
            return false;
        }
        $scope.$parent.loading.show();
        $('#seleccion-todos-gtr').prop("checked", false);
        var ajaxBuscarTicketsResueltos = $http.post("tickets/buscar-tickets-resueltos",
            {
                cantDias: $scope.cantDias
            });
        ajaxBuscarTicketsResueltos.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.totalTicketSeleccionados = 0;
                $scope.ticketsResueltos = data.ticketsResueltos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.selectChange = function (checkboxSelectTicket) {
        if (checkboxSelectTicket === false || checkboxSelectTicket === 0) {
            $scope.totalTicketSeleccionados--;
        } else {
            $scope.totalTicketSeleccionados++;
        }
    };

    $scope.buscarTotalTicket = function () {
        $scope.totalTicketSeleccionados = 0;
        if ($scope.todosCheck) {
            $scope.ticketFiltrados.forEach(function (element) {
                element.selected = true;
            });
            $scope.totalTicketSeleccionados = $scope.ticketFiltrados.length;
        } else {
            $scope.ticketsResueltos.forEach(function (element) {
                element.selected = false;
            });
            $scope.totalTicketSeleccionados = 0;
        }
    };

    //$scope.$watch("ticketFiltrados", function (newValue, oldValue) {
    //    $scope.totalTicketSeleccionados = $('#div-tabla-gestion-ticket-resueltos tbody [type="checkbox"]:checked').length;
    //});

    $scope.cerrarTicketsSeleccionados = function () {
        var idsTicketsSeleccionados = new Array();
        var seleccionados = $('#div-tabla-gestion-ticket-resueltos tbody [type="checkbox"]:checked');
        angular.element.each(seleccionados, function (key, check) {
            idsTicketsSeleccionados.push(angular.element(check).val());
        });

        if (idsTicketsSeleccionados.length === 0) {
            messageDialog.show('Información', 'Por favor seleccione un ticket.');
            return false;
        }

        waitingDialog.show('Cerrando los tickets seleccionados...', { dialogSize: 'sm', progressType: 'success' });

        var ajaxCerrarTickets = $http.post("tickets/cerrar-tickets-resueltos",
            {
                idTickets: idsTicketsSeleccionados
            });
        ajaxCerrarTickets.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.buscarTicketResueltos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.verHistoricoTicketResuelto = function (idTicket) {
        $scope.$parent.idTicketHistorico = idTicket;
        $scope.$parent.verHistoricoTicket();
    };

}]);
//Tickets en Aprobación
ticketApp.controller('ticketsEnAprobacionController', ['$scope', '$http', function ($scope, $http) {

    $scope.buscarTicketEnAprobacion = function () {
        if ($scope.cantDiasEnAprobacion === "" || $scope.cantDiasEnAprobacion === undefined) {
            messageDialog.show('Información', 'Establezca la cantidad de días.');
            return false;
        }
        $scope.$parent.loading.show();
        $('#seleccion-todos-gtr-aprobacion').prop("checked", false);
        var ajaxBuscarticketsEnAprobacion = $http.post("tickets/buscar-tickets-en-aprobacion",
            {
                cantDias: $scope.cantDiasEnAprobacion
            });
        ajaxBuscarticketsEnAprobacion.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.totalTicketSeleccionadosEnAprobacion = 0;
                $scope.ticketsEnAprobacion = data.ticketsEnAprobacion;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.selectEnAprobacionChange = function (checkboxSelectTicket) {
        if (checkboxSelectTicket === false || checkboxSelectTicket === 0) {
            $scope.totalTicketSeleccionadosEnAprobacion--;
        } else {
            $scope.totalTicketSeleccionadosEnAprobacion++;
        }
    };

    $scope.buscarTotalTicketEnAprobacion = function () {
        $scope.totalTicketSeleccionadosEnAprobacion = 0;
        if ($scope.todosEnAprobacionCheck) {
            $scope.ticketEnAprobacionFiltrados.forEach(function (element) {
                element.selected = true;
            });
            $scope.totalTicketSeleccionadosEnAprobacion = $scope.ticketEnAprobacionFiltrados.length;
        } else {
            $scope.ticketsEnAprobacion.forEach(function (element) {
                element.selected = false;
            });
            $scope.totalTicketSeleccionadosEnAprobacion = 0;
        }
    };

    //$scope.$watch("ticketEnAprobacionFiltrados", function (newValue, oldValue) {
    //    $scope.totalTicketSeleccionadosEnAprobacion = $('#div-tabla-gestion-ticket-en-aprobacion tbody [type="checkbox"]:checked').length;
    //});

    $scope.cerrarTicketsSeleccionadosEnAprobacion = function () {
        var idsTicketsSeleccionados = new Array();
        var seleccionados = $('#div-tabla-gestion-ticket-en-aprobacion tbody [type="checkbox"]:checked');
        angular.element.each(seleccionados, function (key, check) {
            idsTicketsSeleccionados.push(angular.element(check).val());
        });

        if (idsTicketsSeleccionados.length === 0) {
            messageDialog.show('Información', 'Por favor seleccione un ticket.');
            return false;
        }

        waitingDialog.show('Cerrando los tickets seleccionados...', { dialogSize: 'sm', progressType: 'success' });

        var ajaxCerrarTickets = $http.post("tickets/cerrar-tickets-en-aprobacion",
            {
                idTickets: idsTicketsSeleccionados
            });
        ajaxCerrarTickets.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.buscarTicketEnAprobacion();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.verHistoricoTicketEnAprobacion = function (idTicket) {
        $scope.$parent.idTicketHistorico = idTicket;
        $scope.$parent.verHistoricoTicket();
    };

}]);

ticketApp.controller('ticketsEnEsperaController', ['$scope', '$http', function ($scope, $http) {

    $scope.buscarTicketEnEspera = function () {
        if ($scope.cantDias === "" || $scope.cantDias === undefined) {
            messageDialog.show('Información', 'Establezca la cantidad de días.');
            return false;
        }
        $scope.$parent.loading.show();
        $('#seleccion-todos-gtr').prop("checked", false);
        var ajaxBuscarTicketsEnEspera = $http.post("tickets/buscar-tickets-en-espera",
            {
                cantDias: $scope.cantDias
            });
        ajaxBuscarTicketsEnEspera.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.ticketsEnEspera = data.ticketsEnEspera;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $('#seleccion-todos-gtr').prop("checked", false);
    angular.element("#div-tabla-gestion-ticket-en-espera").on('change', '#seleccion-todos-gtr', function () {
        var value = $(this).prop("checked");
        $('#div-tabla-gestion-ticket-en-espera tbody [type="checkbox"]').prop("checked", value);
    });

    $scope.cerrarTicketsSeleccionados = function () {
        var idsTicketsSeleccionados = new Array();
        var seleccionados = $('#div-tabla-gestion-ticket-en-espera tbody [type="checkbox"]:checked');
        angular.element.each(seleccionados, function (key, check) {
            idsTicketsSeleccionados.push(angular.element(check).val());
        });

        if (idsTicketsSeleccionados.length === 0) {
            messageDialog.show('Información', 'Por favor seleccione un ticket.');
            return false;
        }

        waitingDialog.show('Cerrando los tickets seleccionados...', { dialogSize: 'sm', progressType: 'success' });

        var ajaxCerrarTickets = $http.post("tickets/cerrar-tickets-resueltos",
            {
                idTickets: idsTicketsSeleccionados
            });
        ajaxCerrarTickets.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.buscarTicketEnEspera();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.verHistoricoTicketResuelto = function (idTicket) {
        $scope.$parent.idTicketHistorico = idTicket;
        $scope.$parent.verHistoricoTicket();
    };

}]);

ticketApp.controller('cotizacionTicketController', ['$scope', '$http', function ($scope, $http) {
    $scope.$parent.ticketsCotizables = [];
    $scope.colaboradoresAsignaciones = [];
    $('#editor-texto-cotizacion').wysiwyg();
    $scope.idCotizacionTicket = 0;
    $scope.idTicketCotizacion = 0;

    angular.element('#fecha-limite-aceptacion').datepicker({
        orientation: "bottom auto",
        format: 'dd/mm/yyyy'
    });

    //filtrando los tickets
    $scope.filtrarCotizacionesTickets = function () {
        $scope.$parent.cargarCotizacionesUsuario($scope.filtroCotizacion);
    };

    $scope.mostrarCotizacionTicket = function (idCotizacion, idTicket) {
        $scope.idCotizacionTicket = idCotizacion;
        $scope.idTicketCotizacion = idTicket;
        $scope.$parent.idTicketHistorico = idTicket;

        angular.element("#modal-cotizar-ticket").modal('show');

        //Cargando las estimaciones
        var estimacionesCotizacion = $http.post("tickets/estimaciones-aceptadas-ticket",
            {
                idTicket: $scope.idTicketCotizacion
            });
        estimacionesCotizacion.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.estimacionesAceptadasTicket = data.estimaciones,
                    $scope.totalEstimadoTicket = data.estimacion
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        //Cargando los colaboradores
        var colaboradoresVerAsignacion = $http.post("tickets/colaboradores-asignaciones",
            {});
        colaboradoresVerAsignacion.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.colaboradoresAsignaciones = data.colaboradores;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    angular.element("#modal-cotizar-ticket").on('hidden.bs.modal', function (e) {
        $scope.inicializarWindowCotizacion();
    });

    $scope.inicializarWindowCotizacion = function () {
        $scope.estimacionesAceptadasTicket = [];
        $scope.totalEstimadoTicket = 0;
        $scope.totalIncrementoTicket = 0;
        $scope.totalHorasCotizacionTicket = 0;
        $scope.precioHoraCotizacion = 0;
        $scope.totalPrecioCotizacion = 0;
        $scope.fechaLimiteAceptacion = "";
        angular.element("#fecha-limite-aceptacion").val("");
    };
    $scope.inicializarWindowCotizacion();

    //Mostrando la window con los datos de las asignaciones del colaborador seleccionado
    $scope.addMes = 0; //Para sumar o restar meses
    $scope.idCola = 0; //Para el id del colaborador
    $scope.verAsignacionesColaborador = function (idColab) {
        $scope.idCola = idColab;
        var buscarDatosTicket = $http.post("tickets/asignaciones-colaborador",
            {
                idColaborador: idColab,
                addMes: $scope.addMes
            });

        buscarDatosTicket.success(function (data) {
            if (data.success === true) {
                $scope.mesCalendar = data.mesCalendar;
                $scope.semanasAsignaciones = data.semanasAsignaciones;
                angular.element("#modal-asignacion-colaboradores2").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.irMesAtras = function () {
        $scope.addMes -= 1;
        $scope.verAsignacionesColaborador($scope.idCola);
    };
    $scope.irMesAlante = function () {
        $scope.addMes += 1;
        $scope.verAsignacionesColaborador($scope.idCola);
    };

    //Calculando los precios y las horas
    $scope.calcularTotalHoras = function () {
        if (isNaN($scope.totalEstimadoTicket) === false && isNaN($scope.totalIncrementoTicket) === false) {
            $scope.totalHorasCotizacionTicket = parseFloat($scope.totalEstimadoTicket) + ($scope.totalIncrementoTicket * $scope.totalEstimadoTicket) / 100;
            $scope.totalHorasCotizacionTicket = Math.round($scope.totalHorasCotizacionTicket * 100) / 100;
        }
        if (isNaN($scope.totalHorasCotizacionTicket) === false && isNaN($scope.precioHoraCotizacion) === false) {
            $scope.totalPrecioCotizacion = $scope.totalHorasCotizacionTicket * $scope.precioHoraCotizacion;
            $scope.totalPrecioCotizacion = Math.round($scope.totalPrecioCotizacion * 100) / 100;
        }
        return false;
    };

    $scope.calcularTotalIncremento = function () {
        if (isNaN($scope.totalHorasCotizacionTicket) === false && isNaN($scope.totalEstimadoTicket) === false) {
            $scope.totalIncrementoTicket = ($scope.totalHorasCotizacionTicket - $scope.totalEstimadoTicket) * 100 / $scope.totalEstimadoTicket;
            $scope.totalIncrementoTicket = Math.round($scope.totalIncrementoTicket * 100) / 100;
        }
        return false;
    };

    $scope.calcularPrecioTotal = function () {
        if (isNaN($scope.totalHorasCotizacionTicket) === false && isNaN($scope.precioHoraCotizacion) === false) {
            $scope.totalPrecioCotizacion = $scope.totalHorasCotizacionTicket * $scope.precioHoraCotizacion;
            $scope.totalPrecioCotizacion = Math.round($scope.totalPrecioCotizacion * 100) / 100;
        }
        return false;
    };

    $scope.calcularPrecioXHoras = function () {
        if (isNaN($scope.totalHorasCotizacionTicket) === false && isNaN($scope.totalPrecioCotizacion) === false) {
            $scope.precioHoraCotizacion = $scope.totalPrecioCotizacion / $scope.totalHorasCotizacionTicket;
            $scope.precioHoraCotizacion = Math.round($scope.precioHoraCotizacion * 100) / 100;
        }
        return false;
    };

    //Consolidacion de los textos de la estimación
    $scope.consolidarDatos = function () {
        angular.element("#editor-texto-cotizacion").html("");
        angular.element("#panel-estimaciones-cotizacion").hide();
        angular.element("#texto-cotizacion").show();

        var textoHtml = "<b>Actividades:</b><br/>";
        var textosActividades = angular.element(".text-estimacion-cotizacion");

        var x = 0;

        textoHtml += "<ul>"
        angular.element.each(textosActividades, function (key, value) {
            if (angular.element(value).val() !== "" && angular.element(value).val() !== undefined)
                textoHtml += '<li>' + angular.element(value).val() + '</li>';
        });
        textoHtml += "</ul>"

        textoHtml += "<b>Entregables:</b>";
        textoHtml += "<ul>"
        var entregables = angular.element(".estimacion-text.entregables");
        angular.element.each(entregables, function (key, value) {
            if (angular.element(value).val() !== "" && angular.element(value).val() !== undefined)
                textoHtml += '<li>' + angular.element(value).val() + '</li>';
        });
        textoHtml += "</ul>"

        textoHtml += "<b>Informacion Complementaria:</b>";
        textoHtml += "<ul>"
        var informacionComplementaria = angular.element(".estimacion-text.inf-complementaria");
        angular.element.each(informacionComplementaria, function (key, value) {
            if (angular.element(value).val() !== "" && angular.element(value).val() !== undefined)
                textoHtml += '<li>' + angular.element(value).val() + '</li>';
        });
        textoHtml += "</ul>"

        Math.round($scope.totalPrecioCotizacion * 100) / 100

        textoHtml += "<b>Total de Horas: </b>" + Math.round($scope.totalHorasCotizacionTicket * 100) / 100 + ' h<br/>';
        textoHtml += "<b>Precio por Horas: </b>" + Math.round($scope.precioHoraCotizacion * 100) / 100 + ' $<br/>';
        textoHtml += "<b>Precio Total: </b>" + Math.round($scope.totalPrecioCotizacion * 100) / 100 + ' $<br/><br/>';
        textoHtml += "<b>Fecha límite para respuesta: </b>" + $scope.fechaLimiteAceptacion + '<br/>';

        angular.element("#editor-texto-cotizacion").html(textoHtml);

        angular.element("#boton-consolidar").hide();
        angular.element("#boton-desconsolidar").show();
    };
    angular.element("#boton-desconsolidar").hide();
    $scope.desConsolidarDatos = function () {
        angular.element("#panel-estimaciones-cotizacion").show();
        angular.element("#texto-cotizacion").hide();

        angular.element("#boton-consolidar").show();
        angular.element("#boton-desconsolidar").hide();
    };

    //Realizar la oferta de cotización de ticket
    $scope.ofertarCotizacionTicket = function () {
        if (angular.element("#texto-cotizacion").is(':visible')) {
            waitingDialog.show('Enviando Propuesta de Cotización...', { dialogSize: 'sm', progressType: 'success' });
            var textoHtml = angular.element("#editor-texto-cotizacion").html();

            var formData = new FormData();
            formData.append('idCotizacion', $scope.idCotizacionTicket);
            formData.append('texto', textoHtml);
            formData.append('porcientoIncremento', $scope.totalIncrementoTicket);
            formData.append('totalHoras', $scope.totalHorasCotizacionTicket);
            formData.append('precioHoras', $scope.precioHoraCotizacion);
            formData.append('precioTotal', $scope.totalPrecioCotizacion);
            formData.append('fechaLimite', $scope.fechaLimiteAceptacion);
            formData.append('renegociable', $scope.ofertaRenegociable);
            angular.element.each(angular.element('#div-add-ficheros-cotizacion').find('[type="file"]'), function (pos, fileInput) {
                formData.append('adjuntos', fileInput.files[0]);
            });

            var estimacionesCotizacion = $http.post("tickets/nueva-oferta-cotizacion",
                /*{
                    idCotizacion: $scope.idCotizacionTicket,
                    texto: textoHtml,
                    porcientoIncremento: $scope.totalIncrementoTicket,
                    totalHoras: $scope.totalHorasCotizacionTicket,
                    precioHoras: $scope.precioHoraCotizacion,
                    precioTotal: $scope.totalPrecioCotizacion,
                    fechaLimite: $scope.fechaLimiteAceptacion,
                    renegociable: $scope.ofertaRenegociable
                }*/
                formData,
                {
                    headers: { 'Content-Type': undefined }
                });
            estimacionesCotizacion.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.$parent.cargarCotizacionesUsuario($scope.filtroCotizacion);
                    angular.element("#modal-cotizar-ticket").modal('hide');
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });

        }
        else {
            messageDialog.show('Información', "Debe consolidar la oferta antes de cotizar el ticket");
        }
    };

    //Informacion del ticket
    $scope.informacionTicket = function () {
        var informacionTicket = $http.post("tickets/dar-datos-ticket/",
            {
                idTicket: $scope.idTicketCotizacion
            });
        informacionTicket.success(function (data) {
            if (data.success === true) {
                $scope.fechaTicket = data.datosTicket.fecha;
                $scope.nombreCliente = data.datosTicket.cliente;
                $scope.usuarioCliente = data.datosTicket.usuarioCliente;
                $scope.usuarioTelefono = data.datosTicket.clienteTelefono;
                $scope.estadoTicket = data.datosTicket.estado;
                $scope.prioridadTicket = data.datosTicket.prioridadDesc;
                $scope.categoriaTicket = data.datosTicket.categoriaDesc;
                $scope.clienteReporta = data.datosTicket.reporto;
                $scope.telefonoCliente = data.datosTicket.telefono;
                $scope.reputacionCliente = data.reputacion;
                $scope.detalleTicket = data.datosTicket.detalle;

                $scope.adjuntosTicket = data.datosTicket.adjuntos;

                angular.element("#modal-vista-ticket").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Informacion del Cliente
    $scope.ticketNoCerradosCliente = function () {
        var ticketsNoCerrados = $http.post("tickets/ticket-no-cerrados-cliente/",
            {
                idTicket: $scope.idTicketCotizacion
            });
        ticketsNoCerrados.success(function (data) {
            if (data.success === true) {
                $scope.ticketNoCerrados = data.tickets;
                angular.element("#modal-ticket-no-cerrados").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Informacion del Cliente
    $scope.cotizacionesDelCliente = function () {
        var ticketsNoCerrados = $http.post("tickets/cotizaciones-cliente/",
            {
                idTicket: $scope.idTicketCotizacion
            });
        ticketsNoCerrados.success(function (data) {
            if (data.success === true) {
                $scope.ticketCotizados = data.cotizaciones;
                angular.element("#modal-cotizaciones-clientes").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Adjuntos de la Cotización
    $scope.windowAdjuntar = function () {
        angular.element("#modal-cotizaciones-adjuntos").modal().show();
    };
    $scope.adicionarAdjuntoCotizacion = function () {
        angular.element("#div-add-ficheros-cotizacion").append(
            angular.element("#htmlFile").html()
        );
    };
    $scope.eliminarAdjuntoCotizacion = function () {
        var hijos = angular.element("#div-add-ficheros-cotizacion").children();
        var cant = hijos.length;
        if (cant > 1) {
            angular.element(hijos[cant - 1]).remove();
        }
    };

}]);

ticketApp.controller('BITicketController', ['$scope', '$http', function ($scope, $http) { }]);

ticketApp.controller('resumenTicketController', ['$scope', '$http', function ($scope, $http) {
    angular.element('#dtp-fecha-semana-resumen').datetimepicker({
        locale: 'es',
        format: 'DD/MM/YYYY'
    });

    $scope.buscarDatos = function () {
        var fecha = $('#dtp-fecha-semana-resumen input').first().val();
        var datosResumen = $http.post("tickets/resumen-ticket/", {
            strFecha: fecha
        });
        datosResumen.success(function (data) {
            if (data.success === true) {
                $scope.semana = data.semana;
                $scope.anteriores = data.anteriores;
                $scope.ticketsClientes = data.ticketsClientes;
                $scope.ticketsTecnicos = data.ticketsTecnicos;
                /*
                $scope.ticketCotizados = data.cotizaciones;
                angular.element("#modal-cotizaciones-clientes").modal("show");
                */
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
}]);

ticketApp.controller('reporteTicketController', ['$scope', '$http', function ($scope, $http) {

    var orderStatus = [1, 1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

    var filterReporteTickets = {
        noReporteTicketFiltro: '',
        detalleReporteTicketFiltro: '',
        tecnicoAsignado: '',
        fechaAddTicketFiltro: '',
        estadosFiltro: '',
        reportadoReporteTicketFiltro: '',
        categoriaRevisadaFiltro: '',
        tiempoReporteTicketFiltro: ''
    };

    angular.element('#dtp-fecha-inicio-reporte').datetimepicker({
        locale: 'es',
        format: 'DD/MM/YYYY'
    });
    angular.element('#dtp-fecha-fin-reporte').datetimepicker({
        locale: 'es',
        format: 'DD/MM/YYYY'
    });
    angular.element('#dtp-fecha-add-reporte').datepicker({
        locale: 'es',
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    $scope.agregarComentarioHoras = function () {
        waitingDialog.show('Cargando...', { dialogSize: 'sm', progressType: 'success' });
        var cliente = document.getElementById("clienteMantenimiento");
        var selectedCliente = cliente.options[cliente.selectedIndex].value;
        var horasMantenimiento = document.getElementById("selectHorasMantenimiento");
        var selectedHoras = horasMantenimiento.options[horasMantenimiento.selectedIndex].value;
        var fechaInicio = $('#dtp-fecha-inicio-reporte input').first().val();

        var tickets = $http.post("tickets/agregar-comentario-horas/",
            {
                cliente: selectedCliente,
                fecha: fechaInicio,
                comentario: selectedHoras
            });
        tickets.success(function (data) {
            if (data.success === true) {
                $scope.buscarTicteksReporte();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
            waitingDialog.hide();
        });
    };

    var ajaxColaboradores = $http.post("catalogos/coordinadores", {});

    ajaxColaboradores.success(function (data) {
        if (data.success === true) {
            $scope.asignados = data.coordinadores;
        }
    });

    var ajaxEstadoTicket = $http.post("catalogos/datos-catalogos/",
        {
            nombre: 'ESTADO TICKET'
        });
    ajaxEstadoTicket.success(function (data) {
        if (data.success === true) {
            $scope.estadosTicket = data.datos;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los estados");
        }
    });

    var ajaxEstadoTicket = $http.post("catalogos/datos-catalogos/",
        {
            nombre: 'COMENTARIO MANTENIMIENTO TICKETS'
        });
    ajaxEstadoTicket.success(function (data) {
        if (data.success === true) {
            $scope.catalogoComentario = data.datos;
        }
        else {
            messageDialog.show('Información', "No se pudo acceder a los comentarios del mantenimiento");
        }
    });

    $scope.buscarTicteksReporte = function () {

        var token = this.user;
        console.log(token);

        waitingDialog.show('Cargando...', { dialogSize: 'sm', progressType: 'success' });
        var fechaInicio = $('#dtp-fecha-inicio-reporte input').first().val();
        var fechaFin = $('#dtp-fecha-fin-reporte input').first().val();
        var datosResumen = $http.post("tickets/reporte-ticket/", {
            idCliente: $scope.clienteReporteTicket,
            strFechaInicio: fechaInicio,
            strFechaFin: fechaFin,
        });

        datosResumen.success(function (data) {
            if (data.success === true) {
                $scope.tickets = data.tickets;
                $scope.horasMes = data.horas;
                $scope.totalHoras = data.total;
                $scope.horasRestantes = data.horasRestantes;
                $scope.totalTicketMantenimiento = data.tickets.length;
                $scope.destinatariosEmailTicket = data.destinatarios;
                $scope.comentarioReporte = data.comentario;
                $scope.categoriasRevisadas = data.categoriasRevisadas;
                $scope.tecnicosAsignados = data.tecnicosAsignados;
                $scope.estados = data.estados;

                $scope.sumaTiempos = data.tickets.reduce((total, ticket) => total + ticket.tiempo, 0);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
            waitingDialog.hide();
        });
    };

    $scope.sumarTiempos = function () {
        var suma = 0;
        angular.forEach($scope.reporteTicketFiltrados, function (ticket) {
            suma += ticket.tiempo;
        });
        return suma;
    };

    $scope.panelEmail = function (cliente) {
        var fechaInicio = $('#dtp-fecha-inicio-reporte input').first().val();
        var fechaFin = $('#dtp-fecha-fin-reporte input').first().val();

        var combo = document.getElementById("clienteMantenimiento");
        var selected = combo.options[combo.selectedIndex].text;

        $scope.asuntoEmailTicket = "Mantenimiento de Tickets"
        $scope.comentarioEmailTicket = "Tabla Mantenimiento de Tickets, cliente" + " " + selected + " (" + fechaInicio + " - " + fechaFin + ")";

        angular.element("#modal-email-mantenimiento-tickets").modal('show');
    };

    $scope.enviarPorEmail = function () {
        var datosEmail = $http.post("tickets/email-reporte-ticket/", {
            destinatariosEmail: $scope.destinatariosEmailTicket.toString(),
            asuntoEmail: $scope.asuntoEmailTicket,
            comentarioEmail: $scope.comentarioEmailTicket,
            tickets: angular.toJson($scope.tickets)
        });
        datosEmail.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-email-mantenimiento-tickets").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.editarTiempo = function (index) {
        var cliente = document.getElementById("clienteMantenimiento");
        var selectedCliente = cliente.options[cliente.selectedIndex].text;
        var tickets = $http.post("tickets/editar-tiempo-mantenimiento/",
            {
                idTicket: $scope.tickets[index].numero,
                idTarea: $scope.tickets[index].tarea,
                tiempo: $scope.tickets[index].tiempo,
                fecha: $scope.tickets[index].fecha,
                esTicket: $scope.tickets[index].numero !== 0,
                cliente: selectedCliente
            });
        tickets.success(function (data) {
            if (data.success === true) {
                $scope.buscarTicteksReporte();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.agregarTicketTarea = function () {
        var cliente = document.getElementById("clienteMantenimiento");
        var selectedCliente = cliente.options[cliente.selectedIndex].text;
        var tecnico = document.getElementById("select-tecnico-reporte");
        var selectedTecnico = tecnico.options[tecnico.selectedIndex].text;
        var estado = document.getElementById("select-estado-reporte");
        var selectedEstado = estado.options[estado.selectedIndex].text;
        var tickets = $http.post("tickets/agregar-ticket-tarea/",
            {
                cliente: selectedCliente,
                noReporteTicket: $scope.noReporteTicket,
                detalleReporteTicket: $scope.detalleReporteTicket,
                reportadoReporteTicket: $scope.reportadoReporteTicket,
                tecnicoReporteTicket: selectedTecnico,
                fecha: $scope.fechaAddTicket,
                estadoReporteTicket: selectedEstado,
                tiempo: $scope.tiempoReporteTicket
            });
        tickets.success(function (data) {
            if (data.success === true) {
                $scope.buscarTicteksReporte();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.eliminarTicketTarea = function (index) {
        console.log($scope.tickets);
        var cliente = document.getElementById("clienteMantenimiento");
        var selectedCliente = cliente.options[cliente.selectedIndex].text;
        var tickets = $http.post("tickets/eliminar-mantenimiento/",
            {
                numeroTticket: $scope.tickets[index].numero,
                secuencialTarea: $scope.tickets[index].tarea,
                fecha: $scope.tickets[index].fecha,
                esTicket: $scope.tickets[index].numero !== 0,
                cliente: selectedCliente
            });
        tickets.success(function (data) {
            if (data.success === true) {
                $scope.buscarTicteksReporte();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.acumuladoTicket = function () {
        var fechaInicio = $('#dtp-fecha-inicio-reporte input').first().val();
        var fechaFin = $('#dtp-fecha-fin-reporte input').first().val();
        var date = new Date(fechaInicio);

        var combo = document.getElementById("clienteMantenimiento");
        var selected = combo.options[combo.selectedIndex].text;

        $scope.acumuladoCliente = selected;
        $scope.acumuladoFecha = date.getMonth() + " / " + date.getFullYear();

        if ($scope.acumuladoCliente !== 'Seleccione...' && $scope.acumuladoFecha !== '') {
            angular.element("#modal-acumulado-mantenimiento-tickets").modal('show');
        }
        else {
            messageDialog.show('Información', "Seleccione el cliente y la fecha inicio");
        }

    };

}]);


//Filters
//Filter de angular para las fechas
ticketApp.filter("strDateToStr", function () {
    return function (textDate) {
        if (textDate !== undefined) {
            var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
            return dateToStr(fecha, 'dd/mm/yyyy', '/', true).substring(0, 10);
        }
        return "";
    }
});

function dateToStr(dateObj, format, separator, includeHour) {
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

    if (includeHour) {
        var hour = dateObj.getHours();
        var hour = (hour < 10) ? '0' + hour : hour;
        var minute = dateObj.getMinutes();
        var minute = (minute < 10) ? '0' + minute : minute;
        var second = dateObj.getSeconds();
        var second = (second < 10) ? '0' + second : second;
        var outTime = [hour, minute, second];
        return out.join(sep) + " " + outTime.join(':');
    }
    return out.join(sep);
};

ticketApp.filter("strDateToStrTime", function () {
    return function (textDate) {
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStrTime(fecha);
    }
});

function dateToStrTime(dateObj, format, separator) {
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

    var hour = dateObj.getHours();
    var hour = (hour < 10) ? '0' + hour : hour;
    var minutes = dateObj.getMinutes();
    var minutes = (minutes < 10) ? '0' + minutes : minutes;


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
    return out.join(sep) + " " + hour + ':' + minutes;
};

//Filter para el relleno a la izquierda
ticketApp.filter("rellenarIzq", function () {
    return function (number, character, lenght) {
        if (lenght === undefined) {
            lenght = 6;
        }
        if (character === undefined) {
            character = ' ';
        }
        var cadena = '';
        while (lenght > 0) {
            cadena = cadena + character;
            lenght--;
        }
        var numDigitos = Math.max(Math.floor(Math.log10(Math.abs(number))), 0) + 1;

        return (cadena + number).slice(numDigitos);
    }
});

//Filter de tamaño de texto
ticketApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);

//Filter para la seleccion en lista, se queda lo que coincide y el escogido
ticketApp.filter("filterSelected", function () {
    return function (array, filtro) {
        if (array !== undefined) {
            var colaboradoresChecked = angular.element("#colaboradores-estimacion input:checked");
            var idColaboradores = [];
            angular.element.each(colaboradoresChecked, function (key, value) {
                idColaboradores.push(parseInt(angular.element(value).val()));
            });

            var resp = [];
            if (filtro !== "" && filtro !== undefined) {
                angular.element.each(array, function (key, value) {
                    if (value.nombre.toLowerCase().contains(filtro.toLowerCase()) === true) {
                        resp.push(value);
                    }
                    else if (idColaboradores.indexOf(value.id) > -1) {
                        resp.push(value);
                    }
                });
            }
            else {
                return array;
            }
            return resp;
        }
        return [];
    }
});

$('.solo-numero').keyup(function () {
    this.value = (this.value).replace(/[^0-9]/g, '');
});

//Direcctivas propias
ticketApp.directive('ngEnter', function ($parse) {
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

//Funciones del prototipe
String.prototype.contains = function (it) { return this.indexOf(it) !== -1; };