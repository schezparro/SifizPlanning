var gestionErrores = angular.module('gestionErrores', []);

gestionErrores.config(function ($provide, $httpProvider) {
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

gestionErrores.controller('gestionErroresController', ['$scope', '$http', function ($scope, $http) {
    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel_gestion-errores-seguimiento").addClass('invisible');
        angular.element("#panel_gestion-errores-grafico").addClass('invisible');
    };
    ocultar();

    $scope.irSeguimientoErrores = function () {
        ocultar();
        angular.element("#panel_gestion-errores-seguimiento").removeClass('invisible');
        $scope.funcionalidad = 'SEGUIMIENTO DE ERRORES';
    };
    $scope.irSeguimientoErrores();

    $scope.irGraficosErrores = function () {
        ocultar();
        angular.element("#panel_gestion-errores-grafico").removeClass('invisible');
        $scope.funcionalidad = 'GRAFICOS DE ERRORES';
        cargarDatosGraficosError();
    };

}]);

//Controlador de la informacion general
gestionErrores.controller('seguimientoErrores', ['$scope', '$http', function ($scope, $http) {
    //Cargando los datepicker
    var fechaDesdeDate = angular.element("#dpk-fecha-desde").datetimepicker({
        format: 'DD/MM/YYYY',
        locale: 'es'
    });
    var fechaHastaDate = angular.element("#dpk-fecha-hasta").datetimepicker({
        format: 'DD/MM/YYYY',
        locale: 'es'
    });
    fechaDesdeDate.on('dp.change', function (e) {
        fechaHastaDate.data("DateTimePicker").minDate(e.date);
        $scope.fechaDesde = e.date.format("DD/MM/YYYY");
        $scope.cargaInformacionGeneral();
    });
    fechaHastaDate.on('dp.change', function (e) {
        fechaDesdeDate.data("DateTimePicker").maxDate(e.date);
        $scope.fechaHasta = e.date.format("DD/MM/YYYY");
        $scope.cargaInformacionGeneral();
    });

    //Actualizacion de la data desde el servidor
    $scope.cargaInformacionGeneral = function () {
        var ajaxInformacionGeneral = $http.post("bugs/informacion-general", {
            fechaDesde: $scope.fechaDesde,
            fechaHasta: $scope.fechaHasta
        });
        ajaxInformacionGeneral.success(function (data) {
            if (data.success == true) {
                $scope.resumenErrores = data.tablaErrores;
                ajustarTableHeaderFixedPorID("tabla-general-errores");
                $scope.fechaDesde = data.fechaDesde;
                $scope.fechaHasta = data.fechaHasta;

                $scope.totalErrores = data.totalErrores;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargaInformacionGeneral();

    //----------------  CARGA DE DATOS --------------------
    $scope.cantListaPaginacion = 8;

    $scope.mostrarErroresEmpresa = function (empresa) {
        $scope.empresaSeleccionada = empresa;
        $scope.startPaginEmpresaErrores = 0;
        paginarErroresEmpresa();
    }

    function paginarErroresEmpresa() {
        var ajaxErroresEmpresa = $http.post("bugs/errores-empresa", {
            empresa: $scope.empresaSeleccionada,
            fechaDesde: $scope.fechaDesde,
            fechaHasta: $scope.fechaHasta,
            start: $scope.startPaginEmpresaErrores,
            leng: $scope.cantListaPaginacion
        });
        ajaxErroresEmpresa.success(function (data) {
            if (data.success === true) {
                $scope.erroresEmpresa = data.listaErrores;
                $scope.cantRegistrosEmpresaErrores = data.cantErrores;
                angular.element("#window-log-data-empresa").modal('show');

                $scope.inicioListaErrores = 1 + $scope.startPaginEmpresaErrores;
                $scope.finalListaErrores = $scope.startPaginEmpresaErrores + data.listaErrores.length;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };

    $scope.paginacionAnterior = function () {
        if (($scope.startPaginEmpresaErrores - $scope.cantListaPaginacion) >= 0) {
            $scope.startPaginEmpresaErrores -= $scope.cantListaPaginacion;
            paginarErroresEmpresa();
        }
    };

    $scope.paginacionSiguiente = function () {
        if (($scope.startPaginEmpresaErrores + $scope.cantListaPaginacion) < $scope.cantRegistrosEmpresaErrores) {
            $scope.startPaginEmpresaErrores += $scope.cantListaPaginacion;
            paginarErroresEmpresa();
        }
    };

    $scope.mostrarDetalleError = function (idError) {
        var ajaxDataError = $http.post("bugs/dar-data-error", {
            idError: idError
        });
        ajaxDataError.success(function (data) {
            if (data.success === true) {
                $scope.dataError = data.dataError;
                $("#window-log-data-error").modal("show");
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };

}]);

//Controlador de la informacion de errores
gestionErrores.controller('graficosErrores', ['$scope', '$http', function ($scope, $http) {

    $scope.cargarDataGraficosError = function () {
        var titleGraficoCalor = "ERRORES POR CLIENTES, ULTIMA SEMANA";
        var ajaxDataGraficosError = $http.post("bugs/dar-errores-grafico-semana-calor", {});
        ajaxDataGraficosError.success(function (data) {
            if (data.success == true) {
                angular.element('#mapa-graph-calor-7d').highcharts({
                    chart: {
                        type: 'heatmap',
                        marginTop: 40,
                        marginBottom: 60,
                        plotBorderWidth: 1
                    },
                    title: {
                        text: titleGraficoCalor
                    },
                    xAxis: {
                        categories: data.xDataCalor
                    },
                    yAxis: {
                        categories: data.yDataCalor,
                        title: null
                    },
                    credits: {
                        enabled: false
                    },
                    colorAxis: {
                        min: 0,
                        minColor: '#FFFFFF',
                        maxColor: '#FF5555'
                    },
                    legend: {
                        align: 'right',
                        layout: 'vertical',
                        margin: 0,
                        verticalAlign: 'top',
                        y: 25,
                        symbolHeight: 280
                    },
                    tooltip: {
                        formatter: function () {
                            return '<b>' + this.series.xAxis.categories[this.point.x] + '</b> <br><b>' +
                                this.point.value + '</b> errores el <br><b>' + this.series.yAxis.categories[this.point.y] + '</b>';
                        }
                    },
                    series: [{
                        name: 'Errores por clientes',
                        borderWidth: 1,
                        data: data.listaDatosCalor,
                        dataLabels: {
                            enabled: true,
                            color: '#000000'
                        }
                    }]
                });
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    cargarDatosGraficosError = function () {
        $scope.cargarDataGraficosError();
    };

}]);