var taskApp = angular.module('task', ['ngDragDrop', 'btorfs.multiselect']);

taskApp.config(function ($provide, $httpProvider) {

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
                var x = false;
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

taskApp.controller('mainTaskController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'INICIO';
    $scope.coordinados = false;

    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    angular.element("#tbody-colaboradores-tareas").scroll(function () {
        angular.element('[data-toggle="popover"]').popover('hide');
        angular.element(".popover").hide();
    });

    angular.element("#panel_tasks").on('click', '[data-toggle="popover"]', function () {
        var obj = this;
        angular.element(".popover").hide();
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

    angular.element(document).on('mouseover', '[data-toggle="tooltip"]', function () {
        angular.element(this).tooltip('show');
    });

    function ocultar() {
        angular.element("#panel_home").addClass('invisible');
        angular.element("#panel_tasks").addClass('invisible');
        angular.element("#panel_solicitudes").addClass('invisible');
        angular.element("#panel_consolidacion_tareas").addClass('invisible');
        angular.element("#panel_disponibilidad_recursos").addClass('invisible');
        angular.element("#panel_incidencias_colaboradores").addClass('invisible');
    };

    /**********************************************/
    //Carga Inicial
    //Cargando los modulos
    var ajaxModulos = $http.post("catalogos/modulos", {});
    ajaxModulos.success(function (data) {
        if (data.success === true) {
            $scope.modulos = data.modulos;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los clientes
    var ajaxClientes = $http.post("catalogos/clientes", {});
    ajaxClientes.success(function (data) {
        if (data.success === true) {
            $scope.clientes = data.clientes;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los estados de los Contratos
    var ajaxEstadosContrato = $http.post("catalogos/estados-contrato", {});
    ajaxEstadosContrato.success(function (data) {
        if (data.success === true) {
            $scope.estadosContrato = data.estadosContrato;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los Tipos de Plazo
    var ajaxTiposPlazo = $http.post("catalogos/tipos-plazo", {});
    ajaxTiposPlazo.success(function (data) {
        if (data.success === true) {
            $scope.tiposPlazo = data.tiposPlazo;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los lugares
    var ajaxLugares = $http.post("catalogos/lugares", {});
    ajaxLugares.success(function (data) {
        if (data.success === true) {
            $scope.lugares = data.lugares;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando las actividades
    var ajaxActividades = $http.post("catalogos/actividades", {});
    ajaxActividades.success(function (data) {
        if (data.success === true) {
            $scope.actividades = data.actividades;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los coordinadores
    var ajaxCoordinadores = $http.post("catalogos/coordinadores", {});
    ajaxCoordinadores.success(function (data) {
        if (data.success === true) {
            $scope.coordinadores = data.coordinadores;
        }
        else {
            //alert("Error en el acceso a los datos.")
        }
    });

    //Cargando los tipos de actividades realizadas
    var ajaxActividadesR = $http.post("catalogos/actividades-realizadas", {});
    ajaxActividadesR.success(function (data) {
        if (data.success === true) {
            $scope.actividadesTarea = data.actividadesRealizadas;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los datos del tipo de error
    var ajaxTipoError = $http.post("catalogos/datos-catalogos", {
        nombre: 'TIPO ERROR'
    });
    ajaxTipoError.success(function (data) {
        if (data.success === true) {
            $scope.datosTiposError = data.datos;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los datos del nivel de implicacion de error
    var ajaxImplicacionError = $http.post("catalogos/implicaciones-error-puntos", {});
    ajaxImplicacionError.success(function (data) {
        if (data.success === true) {
            $scope.datosImplicacionesError = data.implicacionesError;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });

    //Cargando los modulos
    var ajaxCompetencias = $http.post("catalogos/datos-catalogos", {
        nombre: 'COMPETENCIA'
    });
    ajaxCompetencias.success(function (data) {
        if (data.success === true) {
            $scope.competencias = data.datos;
        }
        else {
            messageDialog.show('Información', "Error en los datos de acceso");
        }
    });
    /**********************************************/

    //Funciones de Menu
    $scope.IrInicio = function () {
        ocultar();
        angular.element("#panel_home").removeClass('invisible');
        $scope.funcionalidad = 'INICIO';
        actualizarGraficosInicio();
    };

    $scope.IrTareas = function () {
        ocultar();
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
    };

    $scope.IrSolicitudes = function () {
        ocultar();
        $scope.loading.show();
        angular.element("#panel_solicitudes").removeClass('invisible');
        $scope.funcionalidad = 'SOLICITUDES DE TAREAS';

        //Cargando las solicitudes
        var ajaxSolicitudes = $http.post("task/solicitudes-task", {
            mostrarTodas: $scope.mostrarTodas,
            filtro: $scope.filtroSolicitudTarea
        });
        ajaxSolicitudes.success(function (data) {
            var pagina = 1;
            var posPagin = pagina;
            if (data.success === true) {
                $scope.loading.hide();
                $scope.solicitudes = data.solicitudes;

                $scope.cantPaginas = Math.ceil(data.cantidad / 10);
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

    $scope.IrConsolidacionTareas = function () {
        ocultar();
        angular.element("#panel_consolidacion_tareas").removeClass('invisible');
        $scope.funcionalidad = 'CONSOLIDACIÓN DE TAREAS';
        cargarConsolidaciones();
        //cargarDatosActas();

        //Actualizar Datos
        if ($scope.trabajadores === undefined || $scope.trabajadores === null)
            actualizarDatosTarea();
    };

    $scope.IrDisponibilidadRecursos = function () {
        ocultar();
        angular.element("#panel_disponibilidad_recursos").removeClass('invisible');
        $scope.funcionalidad = 'DISPONIBILIDAD DE RECURSOS';
        cargarDisponibilidadRecursos();
    };

    $scope.IrIncidenciasColaboradores = function () {
        ocultar();
        angular.element("#panel_incidencias_colaboradores").removeClass('invisible');
        $scope.funcionalidad = 'INCIDENCIAS DE COLABORADORES';
        cargarIncidenciaColaboradores();
    };

    //Actualizar datosTarea
    actualizarDatosTarea = function (fechaLunes, filtro) {
        if (filtro === undefined)
            filtro = "";
        $scope.loading.show();
        //Cargando las tareas
        var numeroSemanas = angular.element("#selectSemanas").val();
        //Quitando los popuovers
        angular.element('[data-toggle="popover"]').popover('hide');
        var ajaxDatosTareas = $http.post("task/tareas-trabajadores", {
            fechaLunes: fechaLunes,
            semanas: numeroSemanas,
            json: filtro,
            coordinados: $scope.coordinados
        });
        ajaxDatosTareas.success(function (data) {
            if (data.success === true) {
                $scope.trabajadores = data.trabajadores;
                console.log(data.trabajadores);
                $scope.actualizarListaDias(fechaLunes, $scope.fechaHoy);
                setTimeout(function () {
                    $scope.loading.hide();
                    ajustarTablaTrabajadores();
                }, 200);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funciones generales
    // Función que suma o resta días a la fecha indicada formato d/m/Y
    sumaFecha = function (d, fecha) {
        var Fecha = new Date();
        var sFecha = fecha || (Fecha.getDate() + "/" + (Fecha.getMonth() + 1) + "/" + Fecha.getFullYear());
        var sep = sFecha.indexOf('/') != -1 ? '/' : '-';
        var aFecha = sFecha.split(sep);
        var fecha = aFecha[2] + '/' + aFecha[1] + '/' + aFecha[0];
        fecha = new Date(fecha);
        fecha.setDate(fecha.getDate() + parseInt(d));
        var anno = fecha.getFullYear();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        mes = (mes < 10) ? ("0" + mes) : mes;
        dia = (dia < 10) ? ("0" + dia) : dia;
        var fechaFinal = dia + sep + mes + sep + anno;
        return (fechaFinal);
    };

    //Funcion que actualiza la fecha de los dias de la semana en el calendario de las tareas
    $scope.actualizarListaDias = function (fechaLunes, fechaHoy) {
        $scope.diasCalendar = [];
        //angular.element(".dia-tarea").removeClass('dia-hoy');

        //var diasSemanas = angular.element(".dia-tarea");
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

    $scope.loadingAjax = function () {
        this.show = function (target) {
            angular.element("#loadingDiv").show();
        };
        this.hide = function (target) {
            angular.element("#loadingDiv").hide();
        };
    };

    $scope.loading = new $scope.loadingAjax();

    $('.solo-numero').keyup(function () {
        this.value = (this.value).replace(/[^0-9]/g, '');
    });

    $scope.wind3Opciones = new questionMsgDialog3Option();
    $scope.wind2Opciones = new questionMsgDialog();

    //Funcion para la actualización de los gráficos de la primera página
    function actualizarGraficosInicio() {
        //Cargando las tareas del día
        var ajaxGraph1 = $http.post("task/graph-tareas-dia", {});
        ajaxGraph1.success(function (data) {
            if (data.success === true) {
                dibujarGraficaPie("#charpie-hoy", "Total de tareas para hoy", "tareas", data.datos)
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });

        //Cargando las tareas de la semana
        var ajaxGraph2 = $http.post("task/graph-tareas-semana", {});
        ajaxGraph2.success(function (data) {
            if (data.success === true) {
                dibujarGraficaPie("#charpie-semana", "Total de tareas para esta semana", "tareas", data.datos)
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });

        //Cargando las tareas desde 7 días
        var ajaxGraph3 = $http.post("task/graph-tareas-desde7dias", {});
        ajaxGraph3.success(function (data) {
            if (data.success === true) {
                dibujarGraficaPie("#charpie-ultimos7dias", "Total de tareas desde hace siete días", "tareas", data.datos)
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });

        //Cargando las tareas desde 30 días
        var ajaxGraph4 = $http.post("task/graph-tareas-desde30dias", {});
        ajaxGraph4.success(function (data) {
            if (data.success === true) {
                dibujarGraficoLine("#charline-ultimos30dias", "Tareas díarias desde hace 30 días", data.axisX, "Tareas", data.datos)
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
    };
    actualizarGraficosInicio();
    //FUNCION QUE DIBUJA LOS GRAFICOS DE PASTEL
    function dibujarGraficaPie(selector, texto, name, datos) {
        Highcharts.getOptions().colors = Highcharts.map(Highcharts.getOptions().colors, function (color) {
            return {
                radialGradient: { cx: 0.5, cy: 0.3, r: 0.7 },
                stops: [
                    [0, color],
                    [1, Highcharts.Color(color).brighten(-0.3).get('rgb')] // darken
                ]
            };
        });

        Highcharts.theme = {
            colors: ['#ffcaca', '#ceecee', '#97ff8b', '#eeeeee']
        };

        Highcharts.setOptions(Highcharts.theme);

        angular.element(selector).highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false
            },
            title: {
                text: texto
            },
            tooltip: {
                pointFormat: '{series.name}: <b>{point.y:.0f}</b>'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: true,
                        format: '<b>{point.name}</b>: {point.y:.0f}',
                        style: {
                            color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
                        }
                    }
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                type: 'pie',
                name: name,
                data: datos,
                //datos: [{"name":"ASIGNADA","y":7},{"name":"DESARROLLO","y":0},{"name":"TERMINADA","y":0}]
            }]
        });
    }

    function dibujarGraficoLine(selector, texto, categoriasX, nameY, datos) {
        Highcharts.theme = {
            colors: ['#ffcaca', '#ceecee', '#97ff8b', '#eeeeee']
        };

        Highcharts.setOptions(Highcharts.theme);

        angular.element(selector).highcharts({
            title: {
                text: texto,
                x: -20 //center
            },
            xAxis: {
                categories: categoriasX
            },
            yAxis: {
                title: {
                    text: nameY
                },
                plotLines: [{
                    value: 0,
                    width: 1,
                    color: '#808080'
                }]
            },
            tooltip: {
                //valueSuffix: '°C'
            },
            credits: {
                enabled: false
            },
            legend: {
                layout: 'vertical',
                align: 'right',
                verticalAlign: 'middle',
                borderWidth: 0
            },
            series: datos
        });
    }

    ajustarTablaTrabajadores = function () {
        var primeraFila = angular.element("#tbody-colaboradores-tareas tr").first();
        var celdasCabecera = angular.element("#thead-colaboradores-tareas td");

        var celdasDatos = angular.element(primeraFila).children();
        for (var i = 0; i < celdasDatos.length; i++) {
            var celdaDato = celdasDatos[i];
            var celdaCabecera = celdasCabecera[i];

            var width = angular.element(celdaDato).innerWidth();
            angular.element(celdaCabecera).innerWidth(width);
        }

        //Actualizando el tamaño de la tabla en height
        var tamanioY = angular.element("#task-container").height() - angular.element("#thead-colaboradores-tareas").height();
        angular.element("#tbody-colaboradores-tareas, #tbody-colaboradores").css({
            height: tamanioY
        });
    }

    //PARA LAS CONSOLIDACIONES
    cargarConsolidaciones = function (varSemanas, filtro) {
        $scope.loading.show();
        //Cargando el grafico de las fechas y tareas
        var ajaxConsolidacion = $http.post("task/consolidacion-tareas", {
            varSemanas: varSemanas,
            filtro: filtro
        });
        ajaxConsolidacion.success(function (data) {
            if (data.success === true) {

                //Ajustando el panel de las fechas segun el width
                var anchoTotalContenedor = angular.element("#div-panel-consolidacion").width();
                var panelFechas = angular.element("#panel-fechas-consolidacion");
                var anchoPanelFechas = anchoTotalContenedor - 200;
                panelFechas.css({
                    width: anchoPanelFechas
                });

                $scope.semanasConsolidacion = data.semanas;
                $scope.tiposTrabajo = data.tiposTrabajo;

                setTimeout(function () {
                    var anchoPanelesSemanas = anchoPanelFechas / 14;
                    var anchoDia = anchoPanelFechas / (14 * 7);
                    var diferenciaDias = data.diferenciaDias;
                    if (diferenciaDias >= 0 && diferenciaDias < (14 * 7)) {
                        angular.element("#dia-referencia-consolidacion").show();
                        angular.element("#dia-referencia-consolidacion").css({
                            left: (180 + diferenciaDias * anchoDia)
                        });
                    }
                    else {
                        angular.element("#dia-referencia-consolidacion").hide();
                    }

                    anchoPanelesSemanas = Math.round(anchoPanelesSemanas * 100) / 100;
                    anchoPanelesSemanas -= 0.05;
                    var paneles = angular.element("#div-panel-consolidacion .semana-consolidacion");
                    paneles.css({
                        width: anchoPanelesSemanas
                    });

                    ajustarMesesConsolidacion(data.semanas);
                    ajustarCronogramaTrabajos(data.semanas.length, data.tiposTrabajo);

                    $scope.loading.hide();

                    angular.element(".expandir-trabajo").click();
                }, 1000);

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }

    function ajustarMesesConsolidacion(semanas) {
        angular.element("#panel-meses-fechas-consolidacion").html("");

        cantSemanas = semanas.length;
        var anchoTotal = angular.element("#panel-fechas-consolidacion").width();
        var anchoDia = anchoTotal / (cantSemanas * 7);

        var mesInicial = "";
        var cantDiasMes = 0;

        for (var i = 0; i < cantSemanas; i++) {
            semana = semanas[i];

            var dias = semana.dias;
            for (var j = 0; j < 7; j++) {
                var dia = dias[j];
                var mes = dia.mes
                if (mes !== mesInicial && mesInicial !== "") {
                    var divMes = angular.element("<div/>");
                    divMes.addClass('mes-consolidacion');
                    var anchoMes = anchoDia * cantDiasMes;
                    divMes.css({ 'width': anchoMes });

                    if (cantDiasMes > (mesInicial.length))//Verificando si cabe el mes
                        divMes.html(mesInicial);

                    angular.element("#panel-meses-fechas-consolidacion").append(divMes);

                    cantDiasMes = 0;
                    mesInicial = mes;
                }
                else if (mes !== mesInicial) {
                    mesInicial = mes;
                }
                cantDiasMes++;
            }
        }
        //PONER EL ULTIMO MES
        var divMes = angular.element("<div/>");
        divMes.addClass('mes-consolidacion-ultimo');
        var anchoMes = Math.floor(anchoDia * cantDiasMes);
        divMes.css({ 'width': anchoMes });

        if (cantDiasMes > (mesInicial.length))//Verificando si cabe el mes
            divMes.html(mesInicial);
        angular.element("#panel-meses-fechas-consolidacion").append(divMes);
    }
    function ajustarCronogramaTrabajos(cantSemanas, listaDatos) {
        var anchoTotal = angular.element("#panel-fechas-consolidacion").width();
        var anchoDia = anchoTotal / (cantSemanas * 7);
        var anchoSemana = anchoDia * 7;
        var divClear = angular.element('<div/>');
        divClear.addClass('separador-left');

        angular.element.each(listaDatos, function (key, tipoTrabajo) {

            angular.element.each(tipoTrabajo.trabajos, function (key1, trabajo) {
                var idTrabajo = trabajo.id;
                var selectorTrabajo = "#motivo-trabajo-" + idTrabajo;
                var mTrabajo = angular.element(selectorTrabajo);

                var alto = mTrabajo.height();

                //Div Separador
                var divGanttVacio = angular.element('<div/>');
                var anchoVacio = (trabajo.difInicial) * anchoDia;
                divGanttVacio.css({
                    height: alto,
                    width: anchoVacio
                });
                divGanttVacio.addClass('left');

                //Div Verde
                var divGanttVerde = angular.element('<div/>');
                var anchoVerde = (trabajo.difFinalV - trabajo.difInicial) * anchoDia;
                divGanttVerde.css({
                    height: alto,
                    width: anchoVerde,
                    background: '#AAFFAA',
                    opacity: 0.5
                });
                divGanttVerde.addClass('left');

                //Div Rojo
                var divGanttRojo = angular.element('<div/>');
                var ancho = (trabajo.difFinal - trabajo.difFinalV) * anchoDia;
                divGanttRojo.css({
                    height: alto,
                    width: ancho,
                    background: '#FFAAAA',
                    opacity: 0.5
                });
                divGanttRojo.addClass('left');

                //Adicionando los div del GANTT
                angular.element(selectorTrabajo + " .clear").remove();//Eliminando el clear
                mTrabajo.append(divGanttVacio);
                mTrabajo.append(divGanttVerde);
                mTrabajo.append(divGanttRojo);
                mTrabajo.append('<div class="clear"></div>');

                //Por cada uno de los entregables
                angular.element.each(trabajo.entregables, function (key2, entregable) {
                    var idEntregable = entregable.id;
                    var selectorEntregable = "#entregable-" + idEntregable;
                    var divEntregable = angular.element(selectorEntregable);

                    angular.element(selectorEntregable + " > .clear").remove();//Eliminando el clear

                    //Consolidacion de tareas en el entregable por semana                    
                    var tareasEntregables = entregable.tareas;
                    angular.element.each(tareasEntregables, function (key3, tareasConsolidadas) {
                        var divTareasContenedor = angular.element('<div/>');
                        var divTareasConsolidadas = angular.element('<div/>');

                        angular.element.each(tareasConsolidadas, function (key4, tareaConsolidada) {
                            var divTareaConsolidada = angular.element('<div/>');
                            divTareaConsolidada.attr({ 'data-toggle': 'tooltip', 'data-placement': 'top', 'title': tareaConsolidada.nombreCompleto, 'data-id-colab-cons': tareaConsolidada.idColaborador });
                            divTareaConsolidada.html(tareaConsolidada.horas + tareaConsolidada.iniciales);
                            divTareaConsolidada.addClass('tarea-consolidada');

                            divTareasConsolidadas.append(divTareaConsolidada);
                        });

                        divTareasConsolidadas.addClass('tareas-consolidadas');
                        divTareasContenedor.append(divTareasConsolidadas);
                        divTareasContenedor.addClass('contenedor-tarea-consolidada');
                        divTareasContenedor.css({
                            width: anchoSemana
                        });
                        divEntregable.append(divTareasContenedor);
                    });

                    //angular.element(selectorEntregable + " .clear").remove();//Eliminando el clear

                    var altoEntregable = 20;

                    //Div Separador
                    var divGanttVacioEntregable = angular.element('<div/>');
                    var anchoVacioEntregable = (entregable.difInicial) * anchoDia;
                    divGanttVacioEntregable.css({
                        height: altoEntregable,
                        width: anchoVacioEntregable
                    });
                    divGanttVacioEntregable.addClass('left');

                    //Div Verde                    
                    var divGanttVerdeEntregable = angular.element('<div/>');
                    var anchoVerdeEntregable = (entregable.difFinalV - entregable.difInicial) * anchoDia;
                    divGanttVerdeEntregable.css({
                        height: altoEntregable,
                        width: anchoVerdeEntregable,
                        background: '#AAFFAA',
                        opacity: 0.3
                    });
                    divGanttVerdeEntregable.addClass('left');

                    //Div Rojo
                    var divGanttRojoEntregable = angular.element('<div/>');
                    var anchoEntregable = (entregable.difFinal - entregable.difFinalV) * anchoDia;
                    divGanttRojoEntregable.css({
                        height: altoEntregable,
                        width: anchoEntregable,
                        background: '#FFAAAA',
                        opacity: 0.3
                    });
                    divGanttRojoEntregable.addClass('left');

                    //Adicionando los div del GANTT
                    var divGanttTotalEntregable = angular.element('<div/>');
                    divGanttTotalEntregable.append(divGanttVacioEntregable);
                    divGanttTotalEntregable.append(divGanttVerdeEntregable);
                    divGanttTotalEntregable.append(divGanttRojoEntregable);

                    divGanttTotalEntregable.addClass('absoluto-inicio');
                    divEntregable.append(divGanttTotalEntregable);

                    //Agregando nuevamente el clear
                    divEntregable.append('<div class="clear"></div>');

                    //Reconfigurando
                    var altoEntregableF = divEntregable.height();
                    angular.element(selectorEntregable + " .tareas-consolidadas").css({
                        height: altoEntregableF - 1
                    });
                    angular.element(selectorEntregable + " .absoluto-inicio div").css({
                        height: altoEntregableF
                    });
                });

            });

        });
    }

    //PARA LA DISPONIBILIDAD DE RECURSOS
    cargarDisponibilidadRecursos = function (varSemanas, filtro) {
        $scope.loading.show();
        //Cargando el grafico de las fechas y tareas por recursos
        var ajaxDisponibilidad = $http.post("task/disponibilidad-recursos", {
            varSemanas: varSemanas,
            filtro: filtro
        });
        ajaxDisponibilidad.success(function (data) {
            if (data.success === true) {
                //Ajustando el panel de las fechas segun el width
                var anchoTotalContenedor = angular.element("#div-panel-disponibilidad").width();
                var panelFechas = angular.element("#panel-fechas-disponibilidad");
                var anchoPanelFechas = anchoTotalContenedor - 200;
                panelFechas.css({
                    width: anchoPanelFechas
                });

                $scope.semanasDisponibilidad = data.semanas;
                $scope.colaboradoresDisponibilidad = data.data;

                var anchoDia = anchoPanelFechas / (14 * 7);
                var diferenciaDias = data.diferenciaDias;
                if (diferenciaDias >= 0 && diferenciaDias < (14 * 7)) {
                    angular.element("#dia-referencia-disponibilidad-recursos").show();
                    angular.element("#dia-referencia-disponibilidad-recursos").css({
                        left: (180 + diferenciaDias * anchoDia)
                    });
                }
                else {
                    angular.element("#dia-referencia-disponibilidad-recursos").hide();
                }

                setTimeout(function () {
                    ajustarMesesDisponibilidad(data.semanas);
                    ajustarDisponibilidadRecursos(data.semanas.length, data.data);
                    var anchoPanelesSemanas = anchoPanelFechas / 14;

                    anchoPanelesSemanas = Math.round(anchoPanelesSemanas * 100) / 100;
                    anchoPanelesSemanas -= 0.05;
                    var paneles = angular.element("#div-panel-disponibilidad .semana-disponibilidad");
                    paneles.css({
                        width: anchoPanelesSemanas
                    });
                    $scope.loading.hide();
                }, 700);

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    function ajustarMesesDisponibilidad(semanas) {
        angular.element("#panel-meses-fechas-disponibilidad").html("");

        cantSemanas = semanas.length;
        var anchoTotal = angular.element("#panel-fechas-disponibilidad").width();
        var anchoDia = anchoTotal / (cantSemanas * 7);

        var mesInicial = "";
        var cantDiasMes = 0;

        for (var i = 0; i < cantSemanas; i++) {
            semana = semanas[i];

            var dias = semana.dias;
            for (var j = 0; j < 7; j++) {
                var dia = dias[j];
                var mes = dia.mes
                if (mes !== mesInicial && mesInicial !== "") {
                    var divMes = angular.element("<div/>");
                    divMes.addClass('mes-consolidacion');
                    var anchoMes = anchoDia * cantDiasMes;
                    divMes.css({ 'width': anchoMes });

                    if (cantDiasMes > (mesInicial.length))//Verificando si cabe el mes
                        divMes.html(mesInicial);

                    angular.element("#panel-meses-fechas-disponibilidad").append(divMes);

                    cantDiasMes = 0;
                    mesInicial = mes;
                }
                else if (mes !== mesInicial) {
                    mesInicial = mes;
                }
                cantDiasMes++;
            }
        }
        //PONER EL ULTIMO MES
        var divMes = angular.element("<div/>");
        divMes.addClass('mes-consolidacion-ultimo');
        var anchoMes = Math.floor(anchoDia * cantDiasMes);
        divMes.css({ 'width': anchoMes });

        if (cantDiasMes > (mesInicial.length))//Verificando si cabe el mes
            divMes.html(mesInicial);
        angular.element("#panel-meses-fechas-disponibilidad").append(divMes);
    }
    function ajustarDisponibilidadRecursos(semanas, data) {
        var anchoPanelFechas = angular.element('#panel-fechas-disponibilidad').width();
        var anchoDia = anchoPanelFechas / (semanas * 7);

        angular.element.each(data, function (key, value) {
            var colab = value.colab;
            var asignaciones = value.lineaAsignacion;

            var selector = '[data-id-colaborador="' + colab.idColaborador + '"]';
            var elemento = angular.element(selector)[0];
            var divClear = angular.element(elemento).next().first();

            angular.element.each(asignaciones, function (key1, asignacion) {
                var newDiv = angular.element('<div/>');
                var newDivInside = angular.element('<div/>');

                var strAsignado = "No Asignado<br/>";
                if (asignacion.asignado === true) {
                    if (asignacion.clase === "tar")
                        strAsignado = "Asignado<br/>";
                    else
                        strAsignado = "Vacaciones<br/>";
                }
                var strTitle = colab.nombre + "<br/>" + strAsignado + asignacion.fechaI + " - " + asignacion.fechaF;

                newDiv.attr({
                    'data-toggle': 'tooltip',
                    'data-placement': 'top',
                    'title': strTitle,
                    'data-html': true
                });

                newDiv.append(newDivInside);

                if (asignacion.asignado) {
                    if (asignacion.clase === "tar")
                        newDiv.addClass('asignado left');
                    else
                        newDiv.addClass('asig-vac left');
                }
                else {
                    newDiv.addClass('no-asignado left');
                }

                var anchodiv = anchoDia * asignacion.length;
                newDiv.css({
                    width: anchodiv,
                    height: angular.element(elemento).height()
                });
                angular.element(newDiv).insertBefore(divClear);
            });


        });
    }

    //PARA LAS INCIDENCIAS DE LOS COLABORADORES
    cargarIncidenciaColaboradores = function (varSemanas, filtro) {
        $scope.loading.show();
        //Cargando el grafico de las fechas e incidencias por recurso
        var ajaxIncidencias = $http.post("task/incidencias-recursos", {
            varSemanas: varSemanas,
            filtro: filtro
        });
        ajaxIncidencias.success(function (data) {
            if (data.success === true) {
                //Ajustando el panel de las fechas segun el width
                var anchoTotalContenedor = angular.element("#div-panel-incidencia").width();
                var panelFechas = angular.element("#panel-fechas-incidencia");
                var anchoPanelFechas = anchoTotalContenedor - 200;
                panelFechas.css({
                    width: anchoPanelFechas
                });

                $scope.semanasIncidencia = data.semanas;
                $scope.colaboradoresIncidencia = data.data;

                setTimeout(function () {
                    ajustarMesesIncidencias(data.semanas);
                    ajustarDisponibilidadIncidencias(data.semanas.length, data.data);

                    var anchoPanelesSemanas = anchoPanelFechas / 14;

                    anchoPanelesSemanas = Math.round(anchoPanelesSemanas * 100) / 100;
                    anchoPanelesSemanas -= 0.05;
                    var paneles = angular.element("#div-panel-incidencia .semana-incidencia");
                    paneles.css({
                        width: anchoPanelesSemanas
                    });

                    $scope.loading.hide();
                }, 700);

            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    function ajustarMesesIncidencias(semanas) {
        angular.element("#panel-meses-fechas-incidencia").html("");

        cantSemanas = semanas.length;
        var anchoTotal = angular.element("#panel-fechas-incidencia").width();
        var anchoDia = anchoTotal / (cantSemanas * 7);

        var mesInicial = "";
        var cantDiasMes = 0;

        for (var i = 0; i < cantSemanas; i++) {
            semana = semanas[i];

            var dias = semana.dias;
            for (var j = 0; j < 7; j++) {
                var dia = dias[j];
                var mes = dia.mes
                if (mes !== mesInicial && mesInicial !== "") {
                    var divMes = angular.element("<div/>");
                    divMes.addClass('mes-consolidacion');
                    var anchoMes = anchoDia * cantDiasMes;
                    divMes.css({ 'width': anchoMes });

                    if (cantDiasMes > (mesInicial.length))//Verificando si cabe el mes
                        divMes.html(mesInicial);

                    angular.element("#panel-meses-fechas-incidencia").append(divMes);

                    cantDiasMes = 0;
                    mesInicial = mes;
                }
                else if (mes !== mesInicial) {
                    mesInicial = mes;
                }
                cantDiasMes++;
            }
        }
        //PONER EL ULTIMO MES
        var divMes = angular.element("<div/>");
        divMes.addClass('mes-consolidacion-ultimo');
        var anchoMes = Math.floor(anchoDia * cantDiasMes);
        divMes.css({ 'width': anchoMes });

        if (cantDiasMes > (mesInicial.length))//Verificando si cabe el mes
            divMes.html(mesInicial);
        angular.element("#panel-meses-fechas-incidencia").append(divMes);
    }
    function ajustarDisponibilidadIncidencias(semanas, data) {
        var anchoPanelFechas = angular.element('#panel-fechas-incidencia').width();
        var anchoDia = anchoPanelFechas / (semanas * 7);


        angular.element.each(data, function (key, value) {
            var colab = value.colab;
            var incidencias = value.lineaIncidencias;

            var selector = '[data-id-colaborador-inc="' + colab.idColaborador + '"]';
            var elemento = angular.element(selector)[0];

            var divClear = angular.element(elemento).next().first();

            angular.element.each(incidencias, function (key1, incidencia) {
                var newDiv = angular.element('<div/>');
                var newDivInside = angular.element('<div/>');

                var strIncidencia = "Sin Incidencias<br/>";
                var strTitle = colab.nombre + "<br/>" + strIncidencia;
                if (incidencia.type === "I") {
                    strIncidencia = "Incidencias<br/>";
                    strTitle = colab.nombre + "<br/>" + strIncidencia + "Cantidad: " + incidencia.cant + "<br/>" + "Nivel: " + incidencia.nivel + "<br/>" + "Fecha: " + incidencia.fecha;
                }

                newDiv.addClass('left');

                if (incidencia.type === "I") {
                    newDiv.attr({
                        'data-toggle': 'tooltip',
                        'data-placement': 'top',
                        'title': strTitle,
                        'data-html': true,
                        'data-type-inc': 1,
                        'data-id-inc': incidencia.id
                    });

                    newDiv.css({
                        "background-color": incidencia.color,
                        "border-radius": 3,
                        "padding": 0.5,
                        "cursor": "pointer"
                    })
                }
                else {
                    newDiv.attr({
                        'data-toggle': 'tooltip',
                        'data-placement': 'top',
                        'title': strTitle,
                        'data-html': true,
                        'data-type-inc': 0
                    });

                    newDiv.addClass('no-incidencia');
                }

                newDiv.append(newDivInside);

                var anchodiv = anchoDia * incidencia.length;
                newDiv.css({
                    width: anchodiv,
                    height: angular.element(elemento).height()
                });
                angular.element(newDiv).insertBefore(divClear);
            });


        });

    }

}]);

//Filters
//Filter de angular para las fechas
taskApp.filter("strDateToStr", function () {
    return function (textDate) {
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStr(fecha);
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

//Filter de angular para las fechas y Hora
taskApp.filter("strDateToStrTime", function () {
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

//filter para el usuario de el email
taskApp.filter("userEmail", function () {
    return function (textEmail) {
        var array = textEmail.split('@');
        return array[0];
    }
});

$(window).resize(function () {
    //Actualizando el tamaño de la tabla en height    
    var tamanioY = angular.element("#task-container").height() - angular.element("#thead-colaboradores-tareas").height();
    angular.element("#tbody-colaboradores-tareas, #tbody-colaboradores").css({
        height: tamanioY
    });
});