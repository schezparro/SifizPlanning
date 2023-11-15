consultasApp.controller('garantiasController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
    $scope.FiltroNumero = true;

    var orderStatus = [1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

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
        $scope.recargarGarantias(start, lenght, "", globalOrder, globalAsc);
    };

    var filterGarantias = {
        codigo: '',
        cliente: '',
        descripcion: '',
        fechaProduccion: '',
        fechaVencimiento: '',
        diasRestantes: ''
    };

    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna-garantias i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna-garantias");
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
        $scope.recargarGarantias(start, lenght, "", globalOrder, globalAsc);
    };

    $scope.filterData = function (valor) {
        if ($scope.codigo !== undefined) {
            filterGarantias.codigo = $scope.codigo;
        }

        if ($scope.cliente !== undefined) {
            filterGarantias.cliente = $scope.cliente;
        }

        if ($scope.descripcion !== undefined) {
            filterGarantias.descripcion = $scope.descripcion;
        }

        if ($scope.fechaProduccion !== undefined) {
            filterGarantias.fechaProduccion = $scope.fechaProduccion;
        }

        if ($scope.fechaVencimiento !== undefined) {
            filterGarantias.fechaVencimiento = $scope.fechaVencimiento;
        }

        if ($scope.diasRestantes !== undefined) {
            filterGarantias.diasRestantes = $scope.diasRestantes;
        }

        switch (valor) {
            case 1:
                filterGarantias.codigo = $scope.codigo;
                $scope.FiltroNumero = false;
                break;
            case 2:
                filterGarantias.cliente = $scope.cliente;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterGarantias.descripcion = $scope.descripcion;
                $scope.FiltroNumero = false;
                break;
            case 4:
                filterGarantias.fechaProduccion = $scope.fechaProduccion;
                $scope.FiltroNumero = false;
                break;
            case 5:
                filterGarantias.fechaVencimiento = $scope.fechaVencimiento;
                $scope.FiltroNumero = false;
                break;
            case 6:
                filterGarantias.diasRestantes = $scope.diasRestantes;
                $scope.FiltroNumero = false;
                break;
        }

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarGarantias(start, lenght, "", globalOrder, globalAsc);
    };

    //Para los selects
    //angular.element('#select-estado-contrato').on('click', function (e) {
    //    $scope.filterData(5);
    //});
    //angular.element('#select-tipo-acta').on('click', function (e) {
    //    $scope.filterData(8);
    //});

    $scope.mostrarPorTipo = function () {
        $scope.recargarGarantias(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.mostrarTodasLasGarantias = function () {
        $scope.recargarGarantias(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.recargarGarantias = function (start, lenght, filtro, order, asc) {

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {

            if (filterGarantias.codigo === undefined)
                filterGarantias.codigo = '';
            if (filterGarantias.cliente === undefined)
                filterGarantias.cliente = '';
            if (filterGarantias.descripcion === undefined)
                filterGarantias.descripcion = '';
            if (filterGarantias.fechaProduccion === undefined)
                filterGarantias.fechaProduccion = '';
            if (filterGarantias.fechaVencimiento === undefined)
                filterGarantias.fechaVencimiento = '';
            if (filterGarantias.diasRestantes === undefined)
                filterGarantias.diasRestantes = '';

            filtro = angular.toJson(filterGarantias)
        }

        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;

        var ajaxGarantias = $http.post("task/cargar-garantias-trabajo", {
            start: start,
            lenght: lenght,
            filtro: filtro,
            order: order,
            asc: asc,
            todos: $scope.mostrarTodos,
            tipoMotivoTrabajo: $scope.filtroTipo,
        });

        ajaxGarantias.success(function (data) {
            if (data.success === true) {
                $scope.datosGarantias = data.garantias;
                $scope.totalGarantias = data.cantidadGarantias;

                $scope.entregablesPorContrato = data.garantias[5].entregables;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadGarantias / numerosPorPagina);

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
                    var listaPaginador = angular.element("#tabla-consulta-garantias .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    //Mostrando la modal con los detalles de los entregables
    $scope.mostrarDetalleGarantia = function (codigoContrato) {
        if (codigoContrato !== "") {
            $scope.codigoContrato = codigoContrato;

            var ajaxDetallesContrato = $http.post("task/dar-detalles-garantia",
                {
                    codigoContrato: codigoContrato
                });
            ajaxDetallesContrato.success(function (data) {
                if (data.success) {
                    $scope.entregablesGarantia = data.datos;
                    angular.element("#modal-consulta-garantias").modal('show');
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

}]);