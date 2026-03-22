consultasApp.controller('garantiaTicketsController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
    $scope.FiltroNumero = true;

    var orderStatus = [1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

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
        $scope.recargarTickets(start, lenght, "", globalOrder, globalAsc);
    };

    var filterTickets = {
        noTicket: '',
        cliente: '',
        asunto: '',
        asignado: '',
        fecha: '',
        fechaVencimiento: '',
        diasRestantes: ''
    };

    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna-tickets i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna-tickets");
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

    $scope.filterData = function (valor) {
        if ($scope.noTicket !== undefined) {
            filterTickets.noTicket = $scope.noTicket;
        }

        if ($scope.cliente !== undefined) {
            filterTickets.cliente = $scope.cliente;
        }

        if ($scope.asunto !== undefined) {
            filterTickets.asunto = $scope.asunto;
        }

        if ($scope.asignado !== undefined) {
            filterTickets.asignado = $scope.asignado;
        }

        if ($scope.fecha !== undefined) {
            filterTickets.fecha = $scope.fecha;
        }

        if ($scope.fechaVencimiento !== undefined) {
            filterTickets.fechaVencimiento = $scope.fechaVencimiento;
        }

        if ($scope.diasRestantes !== undefined) {
            filterTickets.diasRestantes = $scope.diasRestantes;
        }

        switch (valor) {
            case 1:
                filterTickets.noTicket = $scope.noTicket;
                $scope.FiltroNumero = true;
                break;
            case 2:
                filterTickets.cliente = $scope.cliente;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterTickets.asunto = $scope.asunto;
                $scope.FiltroNumero = false;
                break;
            case 4:
                filterTickets.asignado = $scope.asignado;
                $scope.FiltroNumero = false;
                break;
            case 5:
                filterTickets.fecha = $scope.fecha;
                $scope.FiltroNumero = false;
                break;
            case 6:
                filterTickets.fechaVencimiento = $scope.fechaVencimiento;
                $scope.FiltroNumero = false;
                break;
            case 7:
                filterTickets.diasRestantes = $scope.diasRestantes;
                $scope.FiltroNumero = true;
                break;
        }

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarTickets(start, lenght, "", globalOrder, globalAsc);
    };
    
    $scope.mostrarTodosLosTickets = function () {
        $scope.recargarTickets(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.recargarTickets = function (start, lenght, filtro, order, asc) {
        waitingDialog.show('Cargando lista de Tickets..', { dialogSize: 'sm', progressType: 'success' });

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {

            if (filterTickets.noTicket === undefined)
                filterTickets.noTicket = '';
            if (filterTickets.cliente === undefined)
                filterTickets.cliente = '';
            if (filterTickets.asunto === undefined)
                filterTickets.asunto = '';
            if (filterTickets.asignado === undefined)
                filterTickets.asignado = '';
            if (filterTickets.fecha === undefined)
                filterTickets.fecha = '';
            if (filterTickets.fechaVencimiento === undefined)
                filterTickets.fechaVencimiento = '';
            if (filterTickets.diasRestantes === undefined)
                filterTickets.diasRestantes = '';

            filtro = angular.toJson(filterTickets)
        }

        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;

        var ajaxGarantias = $http.post("consultas/cargar-garantia-tickets", {
            start: start,
            lenght: lenght,
            filtro: filtro,
            order: order,
            asc: asc,
            todos: $scope.mostrarTodos
        });

        ajaxGarantias.success(function (data) {
            if (data.success === true) {
                waitingDialog.hide();
                $scope.datosTickets = data.tickets;
                $scope.totalTickets = data.totalTickets;

                var posPagin = pagina;
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
                    var listaPaginador = angular.element("#tabla-garantia-tickets .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
                setTimeout(function () {
                    messageDialog.hide(); 
                }, 3000);

            }
        });
    };
}]);