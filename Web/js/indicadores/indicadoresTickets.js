indicadoresApp.controller('indicadoresTickets', ['$scope', '$http', function ($scope, $http) {
    $scope.initDatepickersTickets = function () {
        angular.element('#fecha-inicio-indicadores-tickets').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaInicio);

        angular.element('#fecha-fin-indicadores-tickets').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaFin);
    };

    angular.element('#fecha-hasta-indicadores-tickets').datepicker({
        format: 'dd/mm/yyyy',
        locale: 'es'
    }).datepicker({
        format: 'dd/mm/yyyy'
    });

    var fechaActual = new Date();
    let today = new Date();
    let day = String(today.getDate()).padStart(2, '0');
    let month = String(today.getMonth() + 1).padStart(2, '0');
    let year = today.getFullYear();

    $scope.formattedDate = `${day}/${month}/${year}`;


    //$scope.fechaActual = dateToStr(fechaActual);
    $scope.fechaInicio = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.fechaFin = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);
    $scope.initDatepickersTickets();

    //CLIENTES
    var ajaxClienteProyectos = $http.post("user/cliente-incidencias", {});
    ajaxClienteProyectos.success(function (data) {
        if (data.success === true) {
            $scope.clientes = data.clientes;
        }
    });

    $scope.mostrarPanelGraficos = false;
    $scope.mostrarPanelGraficosAlDia = false;

    $scope.buscarOtrosDatos = function () {
        $scope.mostrarPanelGraficos = true;
        if (isSpecificDateFormat($scope.fechaInicio.toString())) {
            $scope.fechaInicio = dateToStr($scope.fechaInicio);
        }

        if (isSpecificDateFormat($scope.fechaFin.toString())) {
            $scope.fechaFin = dateToStr($scope.fechaFin);
        }

        if ($scope.tendenciaChart) {
            $scope.tendenciaChart.destroy();
        }

        if ($scope.ticketCerradosChart) {
            $scope.ticketCerradosChart.destroy();
        }

        if ($scope.ticketPorAplicaBarrasChart) {
            $scope.ticketPorAplicaBarrasChart.destroy();
        }

        if ($scope.ticketPorMttoBarrasChart) {
            $scope.ticketPorMttoBarrasChart.destroy();
        }

        if ($scope.ticketPorGarantiaBarrasChart) {
            $scope.ticketPorGarantiaBarrasChart.destroy();
        }

        $scope.TicketsNuevos();
        $scope.TicketsCerrados();
        $scope.TicketsAplica();
        $scope.TicketsPorMantenimiento();
        $scope.TicketsPorGarantia();

    };

    //TICKETS NUEVOS
    $scope.TicketsNuevos = function () {

        if ($scope.tendenciaChart) {
            $scope.tendenciaChart.destroy();
        }

        var infoTickets = $http.post("indicadores/dar-cantidadTickets/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTickets.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {

                    $scope.mostrarGraficosNuevosTickets = false;
                    $scope.mensajeNoDataNuevosTickets = 'No existen tickets nuevos en el período del ' + $scope.fechaInicio + ' al ' + $scope.fechaFin + '.';

                } else {
                    $scope.mensajeNoDataNuevosTickets = '';

                    $scope.infoTickets = data.infoTickets;
                    $scope.totalCantidades = data.totalCantidades;
                    $scope.mostrarGraficos = true;

                    var semanas = [];
                    var cantidades = [];

                    $scope.infoTickets.forEach(function (ticket) {
                        semanas.push(ticket.Descripcion);
                        cantidades.push(ticket.Cantidad);
                    });

                    var tendenciaChartCtx = document.getElementById('tendenciaChart').getContext('2d');
                    $scope.tendenciaChart = new Chart(tendenciaChartCtx, {
                        type: 'line',
                        data: {
                            labels: semanas,
                            datasets: [{
                                label: 'Tickets ingresados',
                                data: cantidades,
                                fill: false,
                                borderColor: '#add8e6',
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });
                    $scope.mostrarGraficosNuevosTickets = true;
                }
            } else {
                alert("Error: " + data.msg);
            }
        });
    };

    //TICKETS CERRADOS
    $scope.TicketsCerrados = function () {

        if ($scope.ticketCerradosChart) {
            $scope.ticketCerradosChart.destroy();
        }

        var infoTicketsCerrados = $http.post("indicadores/dar-tickets-cerrados/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsCerrados.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {
                    $scope.mostrarGraficoTicketsCerrados = false;
                    $scope.mensajeNoDataTicketsCerrados = 'No existen tickets cerrados en el período del ' + $scope.fechaInicio + ' al ' + $scope.fechaFin + '.';

                } else {
                    $scope.infoTicketsCerrados = data.infoTickets;
                    $scope.totalCantidadesCerrados = data.totalCantidades;
                    var semanas = [];
                    var cantidades = [];
                    $scope.mensajeNoDataTicketsCerrados = '';

                    $scope.infoTicketsCerrados.forEach(function (ticket) {
                        semanas.push(ticket.Descripcion);
                        cantidades.push(ticket.Cantidad);
                    });

                    var tendenciaChartCtx = document.getElementById('ticketCerradosChart').getContext('2d');
                    $scope.ticketCerradosChart = new Chart(tendenciaChartCtx, {
                        type: 'line',
                        data: {
                            labels: semanas,
                            datasets: [{
                                label: 'Tickets cerrados',
                                data: cantidades,
                                fill: true,
                                borderColor: '#ffc0cb',
                                backgroundColor: '#ffc0cb',
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficoTicketsCerrados = true;
                }
            } else {
                alert("Error: " + data.msg);
            }


        });
    };

    $scope.TicketsAplica = function () {

        if ($scope.ticketPorAplicaBarrasChart) {
            $scope.ticketPorAplicaBarrasChart.destroy();
        }

        var infoTicketsPorAplica = $http.post("indicadores/dar-tickets-por-aplica/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsPorAplica.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {
                    $scope.mostrarGraficoTicketsAplica = false;
                    $scope.mensajeNoDataTicketsAplica = 'No existen datos disponibles';

                } else {
                    $scope.infoTicketsPorAplica = data.infoTickets;
                    $scope.totalCantidadesTicketsPorAplica = data.totalCantidades;
                    $scope.mostrarGraficos = true;
                    $scope.mensajeNoDataTicketsAplica = '';
                    var aplica = [];
                    var cantidades = [];
                    $scope.infoTicketsPorAplica.forEach(function (ticket) {
                        aplica.push(ticket.Aplica);
                        cantidades.push(ticket.Cantidad);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);

                    var tendenciaChartCtx = document.getElementById('ticketPorAplicaBarrasChart').getContext('2d');
                    $scope.ticketPorAplicaBarrasChart = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: aplica,
                            datasets: [{
                                label: 'Cantidad de tickets aplicados a:',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficoTicketsAplica = true;
                }
            } else {
                alert("Error: " + data.msg);
            }
        });
    };

    $scope.TicketsPorMantenimiento = function () {

        if ($scope.ticketPorMttoBarrasChart) {
            $scope.ticketPorMttoBarrasChart.destroy();
        }

        var infoTicketsPorMantenimiento = $http.post("indicadores/dar-tickets-por-mantenimiento/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsPorMantenimiento.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {
                    $scope.mostrarGraficoTicketsMtto = false;
                    $scope.mensajeNoDataTicketsMtto = 'No existen datos disponibles';

                } else {
                    $scope.infoTicketsPorMantenimiento = data.infoTickets;
                    $scope.totalCantidadesTicketsPorMantenimiento = data.totalCantidades;
                    $scope.mostrarGraficos = true;
                    $scope.mensajeNoDataTicketsMtto = '';
                    var clientes = [];
                    var cantidades = [];

                    $scope.infoTicketsPorMantenimiento.forEach(function (ticket) {
                        clientes.push(ticket.Cliente);
                        cantidades.push(ticket.Cantidad);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);

                    var tendenciaChartCtx = document.getElementById('ticketPorMttoBarrasChart').getContext('2d');
                    $scope.ticketPorMttoBarrasChart = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: clientes,
                            datasets: [{
                                label: 'Cantidad de tickets por clientes en mantenimiento',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficoTicketsMtto = true;
                }
            } else {
                alert("Error: " + data.msg);
            }
        });
    };

    //TICKET POR GARANTIA TECNICA
    $scope.TicketsPorGarantia = function () {

        if ($scope.ticketPorGarantiaBarrasChart) {
            $scope.ticketPorGarantiaBarrasChart.destroy();
        }

        var infoTicketsPorGarantia = $http.post("indicadores/dar-tickets-por-garantia/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsPorGarantia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                if (data.infoTickets.length === 0) {

                    $scope.mostrarGraficoTicketsGarantia = false;
                    $scope.mensajeNoDataTicketsGarantia = 'No existen datos disponibles';

                } else {
                    $scope.infoTicketsPorGarantia = data.infoTickets;
                    $scope.totalCantidadesTicketsPorGarantia = data.totalCantidades;
                    $scope.mostrarGraficos = true;
                    $scope.mensajeNoDataTicketsGarantia = '';
                    var clientes = [];
                    var cantidades = [];

                    $scope.infoTicketsPorGarantia.forEach(function (ticket) {
                        clientes.push(ticket.Cliente);
                        cantidades.push(ticket.Cantidad);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);

                    var tendenciaChartCtx = document.getElementById('ticketPorGarantiaBarrasChart').getContext('2d');
                    $scope.ticketPorGarantiaBarrasChart = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: clientes,
                            datasets: [{
                                label: 'Cantidad de tickets por clientes en garantía técnica',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficoTicketsGarantia = true;
                }
            } else {
                alert("Error: " + data.msg);
            }
        });
    };

    // *****************************************************************************************************************************************************************************

    let clienteSeleccionado;

    //******************************************************************************************************************************** */
    //*******************************************   INDICADORES AL DIA   **************************************************************** */
    //******************************************************************************************************************************** */
    $scope.buscarDatosAlDia = function () {
        $scope.mostrarPanelGraficosAlDia = true;
        $scope.verTodosClientes = false;
        $scope.verUnCliente = false;
        $scope.cliente = angular.element("#select-cliente-indicadores-tickets-aldia")[0].value;

        if ($scope.ticketEnGestionChartAlDia) {
            $scope.ticketEnGestionChartAlDia.destroy();
        }

        if ($scope.ticketPorCategoriaBarrasChartAlDia) {
            $scope.ticketPorCategoriaBarrasChartAlDia.destroy();
        }

        if ($scope.ticketPorCategoriaPastelChartAlDia) {
            $scope.ticketPorCategoriaPastelChartAlDia.destroy();
        }

        if ($scope.ticketPorEstadoBarrasChartAlDia) {
            $scope.ticketPorEstadoBarrasChartAlDia.destroy();
        }

        if ($scope.ticketPorEstadoPastelChartAlDia) {
            $scope.ticketPorEstadoPastelChartAlDia.destroy();
        }

        if ($scope.ticketPorClienteEstadoBarrasChart1AlDia) {
            $scope.ticketPorClienteEstadoBarrasChart1AlDia.destroy();
        }

        if ($scope.ticketPorClienteEstadoBarrasChart2AlDia) {
            $scope.ticketPorClienteEstadoBarrasChart2AlDia.destroy();
        }

        if ($scope.ticketPendientesChartAlDia) {
            $scope.ticketPendientesChartAlDia.destroy();
        }

        if ($scope.cliente == "")
            clienteSeleccionado = 0
        else
            clienteSeleccionado = $scope.cliente

        $scope.TicketsEnGestionAlDia();
        $scope.TicketsPorCategoriaAlDia();
        $scope.TicketsPorEstadosAlDia();
        $scope.TicketsPorClientesEstadosAlDia();
        $scope.TicketsPendientesAlDia();

    };

    //Tickets en gestión
    $scope.TicketsEnGestionAlDia = function () {
        if ($scope.ticketEnGestionChartAlDia) {
            $scope.ticketEnGestionChartAlDia.destroy();
        }

        var infoTicketsEnGestionAlDia = $http.post("indicadores/dar-tickets-en-gestion-aldia/", {
            cliente: clienteSeleccionado
        });

        infoTicketsEnGestionAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {

                    $scope.mensajeNoDataTicketsEnGestionAlDia = 'No existen datos disponibles'
                    $scope.mostrarGraficosTicketsEnGestionAlDia = false

                } else {
                    $scope.infoTicketsEnGestionAlDia = data.infoTickets;
                    $scope.totalCantidadesTicketsEnGestionAlDia = data.totalCantidades;
                    $scope.mensajeNoDataTicketsEnGestionAlDia = '';

                    var semanas = [];
                    var cantidades = [];

                    $scope.infoTicketsEnGestionAlDia.forEach(function (ticket) {
                        semanas.push(ticket.Descripcion);
                        cantidades.push(ticket.Cantidad);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);

                    var tendenciaChartCtx = document.getElementById('ticketEnGestionChartAlDia').getContext('2d');
                    $scope.ticketEnGestionChartAlDia = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: semanas,
                            datasets: [{
                                label: 'Tickets en gestión',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficosTicketsEnGestionAlDia = true;
                }

            } else {
                alert("Error: " + data.msg);
            }
        });
    };

    //Tickets por Categorias
    $scope.TicketsPorCategoriaAlDia = function () {

        if ($scope.ticketPorCategoriaBarrasChartAlDia) {
            $scope.ticketPorCategoriaBarrasChartAlDia.destroy();
        }

        if ($scope.ticketPorCategoriaPastelChartAlDia) {
            $scope.ticketPorCategoriaPastelChartAlDia.destroy();
        }

        var infoTicketsPorCategoriasAlDia = $http.post("indicadores/dar-tickets-por-categorias-aldia/", {
            cliente: clienteSeleccionado
        });
        infoTicketsPorCategoriasAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {

                    $scope.mensajeNoDataTicketsEnGestionAlDia = 'No existen datos disponibles'
                    $scope.mostrarGraficoTicketsPorCategoriaAlDia = false

                } else {

                    $scope.mensajeNoDataTicketsEnGestionAlDia = ''
                    $scope.infoTicketsPorCategoriasAlDia = data.infoTickets;
                    $scope.totalCantidadesTicketsPorCategoriaAlDia = data.totalCantidades;

                    var categorias = [];
                    var cantidades = [];
                    var porcentaje = [];
                    $scope.infoTicketsPorCategoriasAlDia.forEach(function (ticket) {
                        categorias.push(ticket.Categoria);
                        cantidades.push(ticket.Cantidad);
                        porcentaje.push(ticket.Porcentaje);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);
                    //Grafico de barras
                    var tendenciaChartCtx = document.getElementById('ticketPorCategoriaBarrasChartAlDia').getContext('2d');
                    $scope.ticketPorCategoriaBarrasChartAlDia = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: categorias,
                            datasets: [{
                                label: 'Cantidades por categorías',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    //Grafico de pastel
                    var tendenciaChartCtx1 = document.getElementById('ticketPorCategoriaPastelChartAlDia').getContext('2d');
                    $scope.ticketPorCategoriaPastelChartAlDia = new Chart(tendenciaChartCtx1, {
                        type: 'doughnut',
                        data: {
                            labels: cantidades,
                            datasets: [{
                                backgroundColor: coloresAleatorios,
                                hoverBorderColor: 'white',
                                data: cantidades,
                                datalabels: {
                                    labels: {
                                        name: {
                                            align: 'center',
                                            anchor: 'center',
                                            color: function (ctx) {
                                                return ctx.dataset.backgroundColor;
                                            },
                                            font: { size: 16 },
                                            formatter: function (value, ctx) {
                                                return categorias[ctx.dataIndex];
                                            }
                                        }
                                    }
                                }
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    color: 'white',
                                    display: function (ctx) {
                                        return ctx.dataset.data[ctx.dataIndex] > 10;
                                    },
                                    font: {
                                        weight: 'bold',
                                    },
                                }
                            },
                            plugins: [ChartDataLabels]
                        }
                    });

                    $scope.mostrarGraficoTicketsPorCategoriaAlDia = true;

                }
            };

        });
    };

    //Tickets por Estados
    $scope.TicketsPorEstadosAlDia = function () {

        if ($scope.ticketPorEstadoBarrasChartAlDia) {
            $scope.ticketPorEstadoBarrasChartAlDia.destroy();
        }

        if ($scope.ticketPorEstadoPastelChartAlDia) {
            $scope.ticketPorEstadoPastelChartAlDia.destroy();
        }

        var infoTicketsPorEstadosAlDia = $http.post("indicadores/dar-tickets-por-estados-aldia/", {
            cliente: clienteSeleccionado
        });

        infoTicketsPorEstadosAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {

                    $scope.mensajeNoDataTicketsPorEstadosAlDia = 'No existen datos disponibles'
                    $scope.mostrarGraficoTicketsPorEstadosAlDia = false

                } else {

                    $scope.mensajeNoDataTicketsPorEstadosAlDia = ''
                    $scope.infoTicketsPorEstadosAlDia = data.infoTickets;
                    $scope.totalCantidadesTicketsPorEstadosAlDia = data.totalCantidades;

                    var estados = [];
                    var cantidades = [];
                    var porcentaje = [];
                    $scope.infoTicketsPorEstadosAlDia.forEach(function (ticket) {
                        estados.push(ticket.Estado);
                        cantidades.push(ticket.Cantidad);
                        porcentaje.push(ticket.Porcentaje);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);

                    // Gráfico de Barras
                    var tendenciaChartCtx = document.getElementById('ticketPorEstadoBarrasChartAlDia').getContext('2d');
                    $scope.ticketPorEstadoBarrasChartAlDia = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: estados,
                            datasets: [{
                                label: 'Cantidades por estados',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    // Gráfico de Pastel
                    var tendenciaChartCtx1 = document.getElementById('ticketPorEstadoPastelChartAlDia').getContext('2d');
                    $scope.ticketPorEstadoPastelChartAlDia = new Chart(tendenciaChartCtx1, {
                        type: 'bar',
                        data: {
                            labels: estados,
                            datasets: [{
                                label: 'Porciento por estados',
                                data: porcentaje,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                x: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficoTicketsPorEstadosAlDia = true;
                }
            } else {
                alert("Error: " + data.msg);
            }
        });
    };

    //tickets de clientes por estados
    $scope.TicketsPorClientesEstadosAlDia = function () {

        if ($scope.ticketPorClienteEstadoBarrasChart1AlDia) {
            $scope.ticketPorClienteEstadoBarrasChart1AlDia.destroy();
        }

        var infoTicketsPorClienteEstadoAlDia = $http.post("indicadores/dar-tickets-por-cliente-estado-aldia/", {
            cliente: clienteSeleccionado
        });

        infoTicketsPorClienteEstadoAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {

                    $scope.mensajeNoDataTicketsClientesEstadosAldia = 'No existen datos disponibles'
                    $scope.mostrarGraficoTicketsClientesEstadosAlDia = false

                } else {
                    $scope.mensajeNoDataTicketsClientesEstadosAldia = ''
                    $scope.mostrarGraficoTicketsClientesEstadosAlDia = true
                    $scope.infoTicketsPorClienteEstadoAlDia = data.infoTickets;
                    $scope.totalCantidadesTicketsPorClientesEstadoAlDia = data.totalCantidades;
                    $scope.groupedTicketsPCEAD = data.infoTickets;

                    // Obtener estados únicos y ordenarlos
                    $scope.estadosUnicos = [...new Set(data.infoTickets.map(ticket => ticket.Estado))].sort();

                    // Crear estructura de clientes
                    $scope.clientesEstructurados = {};

                    // Inicializar todos los clientes con todos los estados en 0
                    data.infoTickets.forEach(ticket => {
                        if (!$scope.clientesEstructurados[ticket.Cliente]) {
                            $scope.clientesEstructurados[ticket.Cliente] = $scope.estadosUnicos.map(estado => ({
                                Estado: estado,
                                Cantidad: 0
                            }));
                        }
                    });

                    // Actualizar las cantidades con los datos reales y calcular el total
                    Object.keys($scope.clientesEstructurados).forEach(cliente => {
                        let total = 0;
                        $scope.clientesEstructurados[cliente].forEach(item => {
                            const ticketEncontrado = data.infoTickets.find(t => t.Cliente === cliente && t.Estado === item.Estado);
                            if (ticketEncontrado) {
                                item.Cantidad = ticketEncontrado.Cantidad;
                            }
                            total += item.Cantidad;
                        });
                        // Agregar el total al final del array de cada cliente
                        $scope.clientesEstructurados[cliente].push({
                            Estado: 'Total',
                            Cantidad: total
                        });
                    });

                    // Calcular totales por estado
                    $scope.totalesPorEstado = {};
                    $scope.estadosUnicos.forEach(estado => {
                        $scope.totalesPorEstado[estado] = 0;
                    });

                    Object.values($scope.clientesEstructurados).forEach(cliente => {
                        cliente.forEach(item => {
                            if (item.Estado !== 'Total') {
                                $scope.totalesPorEstado[item.Estado] += item.Cantidad;
                            }
                        });
                    });

                    // Calcular gran total
                    $scope.granTotal = Object.values($scope.totalesPorEstado).reduce((a, b) => a + b, 0);


                    // Obtener la lista de clientes únicos y ordenarlos alfabéticamente
                    var clientes = Object.keys($scope.clientesEstructurados).sort();


                    // Función para crear datasets
                    function createDatasets(clientesGroup) {
                        var mostrarTodosLosEstados = clientesGroup.length === 1;
                        return $scope.estadosUnicos.map(function (estado, index) {
                            var data = clientesGroup.map(function (cliente) {
                                var estadoObj = $scope.clientesEstructurados[cliente].find(e => e.Estado === estado);
                                return estadoObj ? estadoObj.Cantidad : 0;
                            });

                            return {
                                label: estado,
                                data: data,
                                backgroundColor: getRandomColor(),
                                borderColor: 'rgba(255, 99, 132, 1)',
                                borderWidth: 1,
                                hidden: !mostrarTodosLosEstados && index >= 1
                            };
                        });
                    }

                    // Función para generar colores aleatorios
                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    function createChart(canvasId, clientes, datasets) {
                        var ctx = document.getElementById(canvasId).getContext('2d');
                        new Chart(ctx, {
                            type: 'bar', // You can choose a different chart type here
                            data: {
                                labels: clientes,
                                datasets: datasets
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: true,
                                scales: {
                                    x: { display: true, title: { display: true, text: 'Clientes' }, ticks: { autoSkip: true } },
                                    y: { display: true, title: { display: true, text: 'Cantidad de Tickets' }, ticks: { stepSize: 1, beginAtZero: true }, suggestedMax: 3 },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Cliente y Estado'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'top',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        font: {
                                            weight: 'bold'
                                        }
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.7, // Reduce bar width (adjust as needed)
                                categoryPercentage: 0.8 // Adjust spacing between bars (optional)
                            },
                            plugins: [ChartDataLabels]
                        });
                    };

                    var clientes1, clientes2;
                    // Dividir clientes en dos partes
                    if (clientes.length > 20) {
                        $scope.verTodosClientes = true;
                        var mitad = Math.ceil(clientes.length / 2);
                        clientes1 = clientes.slice(0, mitad);
                        clientes2 = clientes.slice(mitad);

                        // Crear datasets para cada grupo de clientes
                        var datasets1 = createDatasets(clientes1);
                        var datasets2 = createDatasets(clientes2);

                        if ($scope.ticketPorClienteEstadoBarrasChart1AlDia) {
                            $scope.ticketPorClienteEstadoBarrasChart1AlDia.destroy();
                        }

                        if ($scope.ticketPorClienteEstadoBarrasChart2AlDia) {
                            $scope.ticketPorClienteEstadoBarrasChart2AlDia.destroy();
                        }

                        // Crear los gráficos
                        createChart('ticketPorClienteEstadoBarrasChart1AlDia', clientes1, datasets1);
                        createChart('ticketPorClienteEstadoBarrasChart2AlDia', clientes2, datasets2);
                    } else {
                        $scope.verUnCliente = true;

                        console.log(clientes);
                        var datasets = createDatasets(clientes);

                        if ($scope.ticketPorClienteEstadoBarrasChart3AlDia) {
                            $scope.ticketPorClienteEstadoBarrasChart3AlDia.destroy(); // Asegúrate de que esta línea funcione
                            $scope.ticketPorClienteEstadoBarrasChart3AlDia = null; // Limpiar la referencia
                        }

                        var ctx = document.getElementById("ticketPorClienteEstadoBarrasChart3AlDia").getContext('2d');
                        $scope.ticketPorClienteEstadoBarrasChart3AlDia = new Chart(ctx, {
                            type: 'bar', // You can choose a different chart type here
                            data: {
                                labels: clientes,
                                datasets: datasets
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: true,
                                scales: {
                                    x: { display: true, title: { display: true, text: 'Clientes' }, ticks: { autoSkip: true } },
                                    y: { display: true, title: { display: true, text: 'Cantidad de Tickets' }, ticks: { stepSize: 1, beginAtZero: true }, suggestedMax: 3 },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Cliente y Estado'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'top',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        font: {
                                            weight: 'bold'
                                        }
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.7, // Reduce bar width (adjust as needed)
                                categoryPercentage: 0.8 // Adjust spacing between bars (optional)
                            },
                            plugins: [ChartDataLabels]
                        });
                    };
                }
            }
        });
    };

    $scope.TicketsPendientesAlDia = function () {
        if ($scope.ticketPendientesChartAlDia) {
            $scope.ticketPendientesChartAlDia.destroy();
        }

        var infoTicketsPendientesAlDia = $http.post("indicadores/dar-tickets-pendientes-aldia/", {
            cliente: clienteSeleccionado
        });

        infoTicketsPendientesAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                if (data.infoTickets.length === 0) {

                    $scope.mensajeNoDataTicketPendientesAlDia = 'No existen datos disponibles'
                    $scope.mostrarGraficosTicketsPendientesAlDia = false

                } else {
                    $scope.infoTicketsPendientesAlDia = data.infoTickets;
                    $scope.totalCantidadesTicketsPendientesAlDia = data.totalCantidades;
                    $scope.mensajeNoDataTicketPendientesAlDia = '';

                    var semanas = [];
                    var cantidades = [];

                    $scope.infoTicketsPendientesAlDia.forEach(function (ticket) {
                        semanas.push(ticket.Descripcion);
                        cantidades.push(ticket.Cantidad);
                    });

                    function getRandomColor() {
                        var letters = '0123456789ABCDEF';
                        var color = '#';
                        for (var i = 0; i < 6; i++) {
                            color += letters[Math.floor(Math.random() * 16)];
                        }
                        return color;
                    }

                    var coloresAleatorios = cantidades.map(getRandomColor);

                    var tendenciaChartCtx = document.getElementById('ticketPendientesChartAlDia').getContext('2d');
                    $scope.ticketPendientesChartAlDia = new Chart(tendenciaChartCtx, {
                        type: 'bar',
                        data: {
                            labels: semanas,
                            datasets: [{
                                label: 'Tickets pendientes',
                                data: cantidades,
                                backgroundColor: coloresAleatorios,
                                hoverBackgroundColor: coloresAleatorios,
                                borderWidth: 1
                            }]
                        },
                        options: {
                            plugins: {
                                datalabels: {
                                    display: true,
                                    align: 'top',
                                    backgroundColor: '#D3D3D3',
                                    borderRadius: 3,
                                    font: {
                                        weight: 'bold'
                                    }
                                }
                            },
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        },
                        plugins: [ChartDataLabels]
                    });

                    $scope.mostrarGraficosTicketsPendientesAlDia = true;
                }

            } else {
                alert("Error: " + data.msg);
            }
        });
    };


    //******************************************************************************************************************************** */
    //*******************************************   INDICADORES GENERALES   **************************************************************** */
    //******************************************************************************************************************************** */

    $('#select-annos-indicadores-tickets').select2({
        placeholder: "Seleccione...",
        allowClear: true, // Permite borrar la selección
        maximumSelectionLength: 3,
        language: {
            maximumSelected: function (e) {
                return "Solo puedes seleccionar " + e.maximum + " meses";
            }
        },
        closeOnSelect: false,
        theme: "classic",
        width: '300px',
        templateResult: formatResult // Función para formatear cómo se muestra cada opción
    }).on('change', function (e) {
        $scope.$apply(function () {
            var selectedValues = $(e.target).val();
            $scope.annosIndicadoresGenerales = selectedValues ? selectedValues : [];
        });
    });

    $('#select-meses-indicadores-tickets').select2({
        placeholder: "Seleccione...",
        allowClear: true, // Permite borrar la selección
        maximumSelectionLength: 3,
        language: {
            maximumSelected: function (e) {
                return "Solo puedes seleccionar " + e.maximum + " meses";
            }
        },
        closeOnSelect: false,
        width: '300px',
        templateResult: formatResult // Función para formatear cómo se muestra cada opción
    }).on('change', function (e) {
        $scope.$apply(function () {
            var selectedValues = $(e.target).val();
            $scope.mesesIndicadoresGenerales = selectedValues ? selectedValues : [];
        });
    });

    function formatResult(item) {
        if (!item.id) {
            return item.text; // Devuelve el texto si no hay ID
        }
        var $result = $("<span>").text(item.text);
        return $result;
    };

    $scope.buscarDatosIndicadoresGenerales = function () {

        if (!$scope.annosIndicadoresGenerales && !$scope.mesesIndicadoresGenerales) {
            alert("Seleccione en los filtros");
            return;
        }

        if ($scope.annosIndicadoresGenerales || $scope.annosIndicadoresGenerales.length > 0 || $scope.mesesIndicadoresGenerales || $scope.mesesIndicadoresGenerales.length > 0) {
            $scope.mostrarPanelGraficosPorMesesYAnnos = true;
            $scope.mostrarPanelGraficosTicketsAplica = true;
        }

        if ($scope.ticketPorAplicaBarrasChartAnno) {
            $scope.ticketPorAplicaBarrasChartAnno.destroy();
        }
        if ($scope.ticketPorAplicaBarrasChartMes) {
            $scope.ticketPorAplicaBarrasChartMes.destroy();
        }

        if ($scope.ticketPorEstadosBarrasChartAnno) {
            $scope.ticketPorEstadosBarrasChartAnno.destroy();
        }
        if ($scope.ticketPorEstadosBarrasChartMes) {
            $scope.ticketPorEstadosBarrasChartMes.destroy();
        }

        if ($scope.ticketPorGarantiaBarrasChartAnno) {
            $scope.ticketPorGarantiaBarrasChartAnno.destroy();
        }
        if ($scope.ticketPorGarantiaBarrasChartMes) {
            $scope.ticketPorGarantiaBarrasChartMes.destroy();
        }

        $scope.mostrarPanelGraficosIndicadoresGenerales = true;

        $scope.TicketsAplicaIndicadoresGenerales();
        $scope.TicketsEstadosIndicadoresGenerales();
        $scope.TicketsGarantiaIndicadoresGenerales();

    };

    ////// Aplica
    $scope.TicketsAplicaIndicadoresGenerales = function () {
        if ($scope.ticketPorAplicaBarrasChartAnno) {
            $scope.ticketPorAplicaBarrasChartAnno.destroy();
        }
        if ($scope.ticketPorAplicaBarrasChartMes) {
            $scope.ticketPorAplicaBarrasChartMes.destroy();
        }

        var infoTicketsPorAplica = $http.post("indicadores/dar-tickets-aplica-indicadores-generales/", {
            annos: $scope.annosIndicadoresGenerales,
            meses: $scope.mesesIndicadoresGenerales
        });

        infoTicketsPorAplica.success(function (data) {
            $scope.loading.hide();
            if (data.success) {

                if (data.ticketsPorAnno.length === 0 && data.ticketsPorAnnoMes.length === 0) {
                    $scope.mostrarGraficoTicketsAplica = false;
                    $scope.mensajeNoDataTicketsAplica = 'No existen datos disponibles';

                } else if (data.ticketsPorAnno.length !== 0 || data.ticketsPorAnnoMes.length !== 0) {
                    $scope.mostrarGraficoTicketsAplica = true;

                    if (data.ticketsPorAnno.length > 0) {
                        $scope.infoTicketsPorAplicaAnno = data.ticketsPorAnno;
                        $scope.mostrarGraficoTicketsAplicaAnno = true;

                        // Formatear datos para la tabla por año
                        var ticketsPorAplicaAnno = {};
                        var anos = [];
                        data.ticketsPorAnno.forEach(function (ticket) {
                            if (!ticketsPorAplicaAnno[ticket.Aplica]) {
                                ticketsPorAplicaAnno[ticket.Aplica] = {};
                            }
                            ticketsPorAplicaAnno[ticket.Aplica][ticket.Anno] = ticket.Cantidad;
                            if (!anos.includes(ticket.Anno)) {
                                anos.push(ticket.Anno);
                            }
                        });
                    };

                    if (data.ticketsPorAnnoMes.length > 0) {
                        $scope.infoTicketsPorAplicaMes = data.ticketsPorAnnoMes;
                        $scope.mostrarGraficoTicketsAplicaMeses = true;

                        // Procesamiento de datos
                        var ticketsPorAplicaAnnoMes = {};
                        var anos = [];
                        var meses = [];
                        var anoActual = new Date().getFullYear();

                        if (data && data.ticketsPorAnnoMes && Array.isArray(data.ticketsPorAnnoMes)) {
                            $scope.anosTickets = [];
                            $scope.mesesTickets = [];
                            $scope.aplicasList = [];
                            $scope.ticketsPorAplicaAnnoMes = {};
                            data.ticketsPorAnnoMes.forEach(function (ticket) {
                                if (!ticketsPorAplicaAnnoMes[ticket.Aplica]) {
                                    ticketsPorAplicaAnnoMes[ticket.Aplica] = {};
                                }
                                if (!ticketsPorAplicaAnnoMes[ticket.Aplica][ticket.Anno]) {
                                    ticketsPorAplicaAnnoMes[ticket.Aplica][ticket.Anno] = {};
                                }
                                ticketsPorAplicaAnnoMes[ticket.Aplica][ticket.Anno][ticket.Mes] = ticket.Cantidad;

                                if (!anos.includes(ticket.Anno)) {
                                    anos.push(ticket.Anno);
                                }
                                if (!meses.includes(ticket.Mes)) {
                                    meses.push(ticket.Mes);
                                }

                                if (!$scope.anosTickets.includes(ticket.Anno)) {
                                    $scope.anosTickets.push(ticket.Anno);
                                }
                                if (!$scope.mesesTickets.includes(ticket.Mes)) {
                                    $scope.mesesTickets.push(ticket.Mes);
                                }
                                if (!$scope.aplicasList.includes(ticket.Aplica)) {
                                    $scope.aplicasList.push(ticket.Aplica);
                                }

                                if (!$scope.ticketsPorAplicaAnnoMes[ticket.Aplica]) {
                                    $scope.ticketsPorAplicaAnnoMes[ticket.Aplica] = {};
                                }
                                if (!$scope.ticketsPorAplicaAnnoMes[ticket.Aplica][ticket.Anno]) {
                                    $scope.ticketsPorAplicaAnnoMes[ticket.Aplica][ticket.Anno] = {};
                                }
                                $scope.ticketsPorAplicaAnnoMes[ticket.Aplica][ticket.Anno][ticket.Mes] = ticket.Cantidad;

                            });
                        };

                        $scope.anosTickets.sort((a, b) => a - b);
                        $scope.mesesTickets.sort((a, b) => a - b);
                        $scope.aplicasList.sort();

                        console.log($scope.mesesTickets);

                        $scope.getCantidadAnnos = function (aplica, ano) {
                            return ($scope.ticketsPorAplicaAnno[aplica] &&
                                $scope.ticketsPorAplicaAnno[aplica][ano]) || 0;
                        };

                        $scope.getCantidad = function (aplica, ano, mes) {
                            return ($scope.ticketsPorAplicaAnnoMes[aplica] &&
                                $scope.ticketsPorAplicaAnnoMes[aplica][ano] &&
                                $scope.ticketsPorAplicaAnnoMes[aplica][ano][mes]) || 0;
                        };

                        function isMonthEmpty(ano, mes) {
                            return $scope.aplicasList.every(function (aplica) {
                                return $scope.getCantidad(aplica, ano, mes) === 0;
                            });
                        };

                        function isYearEmpty(ano) {
                            return $scope.aplicasList.every(function (aplica) {
                                return $scope.getCantidad(aplica, ano) === 0;
                            });
                        };

                        function getMesByNumero(mesNumero) {
                            var meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
                            return meses[mesNumero - 1] || '';
                        };

                        $scope.calcularTotal = function (ano, mes) {
                            var total = 0;
                            $scope.aplicasList.forEach(function (aplica) {
                                total += $scope.getCantidad(aplica, ano, mes);
                            });
                            return total;
                        };

                        // Si no se seleccionaron años, usar el año actual
                        if (anos.length === 0) {
                            anos.push(anoActual);
                        }

                        $scope.ticketsPorAplicaAnnoMes = ticketsPorAplicaAnnoMes;
                        $scope.anosTickets = anos.sort();
                        $scope.mesesTickets = meses.sort();

                        $scope.ticketsPorAplicaAnno = ticketsPorAplicaAnno;
                        $scope.anosTickets = anos.sort();

                        // Calcular totales por año
                        $scope.totalesPorAnno = {};
                        anos.forEach(function (ano) {
                            $scope.totalesPorAnno[ano] = Object.values(ticketsPorAplicaAnno).reduce(function (total, aplica) {
                                return total + (aplica[ano] || 0);
                            }, 0);
                        });

                        // Función para crear datasets
                        function createDatasetsAnno() {
                            var filteredLabels = [];
                            var datasets = [];

                            $scope.aplicasList.forEach(function (aplica) {
                                var data = [];
                                $scope.anosTickets.forEach(function (ano) {

                                    var cantidad = $scope.getCantidadAnnos(aplica, ano);
                                    if (cantidad > 0 || !isYearEmpty(ano)) {
                                        data.push(cantidad);
                                        if (!filteredLabels.includes(ano)) {
                                            filteredLabels.push(ano);
                                        }
                                    }
                                });

                                if (data.length > 0) {
                                    datasets.push({
                                        label: aplica,
                                        data: data,
                                        backgroundColor: getRandomColor(),
                                        borderColor: 'rgba(255, 99, 132, 1)',
                                        borderWidth: 1
                                    });
                                }
                            });

                            return { datasets: datasets, labels: filteredLabels };
                        };

                        function createDatasetsMeses() {
                            var filteredLabels = [];
                            var datasets = [];

                            $scope.aplicasList.forEach(function (aplica) {
                                var data = [];
                                $scope.anosTickets.forEach(function (ano) {
                                    $scope.mesesTickets.forEach(function (mes) {
                                        var cantidad = $scope.getCantidad(aplica, ano, mes);
                                        if (cantidad > 0 || !isMonthEmpty(ano, mes)) {
                                            data.push(cantidad);
                                            if (!filteredLabels.includes(ano + '-' + getMesByNumero(mes))) {
                                                filteredLabels.push(ano + '-' + getMesByNumero(mes));
                                            }
                                        }
                                    });
                                });

                                if (data.length > 0) {
                                    datasets.push({
                                        label: aplica,
                                        data: data,
                                        backgroundColor: getRandomColor(),
                                        borderColor: 'rgba(255, 99, 132, 1)',
                                        borderWidth: 1
                                    });
                                }
                            });

                            return { datasets: datasets, labels: filteredLabels };
                        };

                        // Función para generar colores aleatorios (sin cambios)
                        function getRandomColor() {
                            var letters = '0123456789ABCDEF';
                            var color = '#';
                            for (var i = 0; i < 6; i++) {
                                color += letters[Math.floor(Math.random() * 16)];
                            }
                            return color;
                        };

                        var chartDataAnno = createDatasetsAnno();
                        var ctxAnno = document.getElementById('ticketPorAplicaBarrasChartAnno').getContext('2d');
                        $scope.ticketPorAplicaBarrasChartAnno = new Chart(ctxAnno, {
                            type: 'bar',
                            data: {
                                labels: chartDataAnno.labels,
                                datasets: chartDataAnno.datasets
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        display: true,
                                        title: { display: true, text: 'Cantidad de Tickets' },
                                        ticks: { beginAtZero: true },
                                    },
                                    y: {
                                        display: true,
                                        title: { display: true, text: 'Año' },
                                    },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Año y Aplica'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'center',
                                        anchor: 'center',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        color: '#000',
                                        font: {
                                            weight: 'bold'
                                        },
                                        formatter: function (value, context) {
                                            return value > 0 ? value : '';
                                        }
                                    },
                                    legend: {
                                        position: 'top',
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.8,
                                categoryPercentage: 0.9
                            },
                            plugins: [ChartDataLabels]
                        });

                        var chartDataMes = createDatasetsMeses();
                        var ctxMes = document.getElementById('ticketPorAplicaBarrasChartMes').getContext('2d');
                        $scope.ticketPorAplicaBarrasChartMes = new Chart(ctxMes, {
                            type: 'bar',
                            data: {
                                labels: chartDataMes.labels,
                                datasets: chartDataMes.datasets
                            },
                            options: {
                                //indexAxis: 'y',
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        display: true,
                                        title: { display: true, text: 'Año - Meses' },
                                        ticks: { beginAtZero: true }
                                    },
                                    y: {
                                        display: true,
                                        title: { display: true, text: 'Cantidad de Tickets' },
                                    },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Año y Aplica'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'center',
                                        anchor: 'center',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        color: '#000',
                                        font: {
                                            weight: 'bold'
                                        },
                                        formatter: function (value, context) {
                                            return value > 0 ? value : '';
                                        }
                                    },
                                    legend: {
                                        position: 'top',
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.8,
                                categoryPercentage: 0.9
                            },
                            plugins: [ChartDataLabels]
                        });

                        $scope.mostrarGraficoTicketsAplica = true;
                    };
                };
            } else {
                console.log("errorrrrrr " + data);
            }
        });
    };

    ////// Estados
    $scope.TicketsEstadosIndicadoresGenerales = function () {
        if ($scope.ticketPorEstadosBarrasChartAnno) {
            $scope.ticketPorEstadosBarrasChartAnno.destroy();
        }
        if ($scope.ticketPorEstadosBarrasChartMes) {
            $scope.ticketPorEstadosBarrasChartMes.destroy();
        }

        $scope.mostrarPanelGraficosPorMesesYAnnosEstados = true;

        var infoTicketsPorEstados = $http.post("indicadores/dar-tickets-estados-indicadores-generales/", {
            annos: $scope.annosIndicadoresGenerales,
            meses: $scope.mesesIndicadoresGenerales
        });

        infoTicketsPorEstados.success(function (data) {
            $scope.loading.hide();
            if (data.success) {

                if (data.ticketsPorAnnoEstados.length === 0 && data.ticketsPorAnnoMesEstados.length === 0) {
                    $scope.mostrarPanelGraficosTicketsEstados = false;
                    $scope.mensajeNoDataTicketsEstados = 'No existen datos disponibles';

                } else if (data.ticketsPorAnnoEstados.length !== 0 || data.ticketsPorAnnoMesEstados.length !== 0) {
                    $scope.mostrarPanelGraficosTicketsEstados = true;

                    if (data.ticketsPorAnnoEstados.length > 0) {
                        $scope.infoTicketsPorEstadosAnno = data.ticketsPorAnnoEstados;
                        $scope.mostrarGraficoTicketsEstadosAnno = true;

                        // Formatear datos para la tabla por año
                        var ticketsPorAnnoEstados = {};
                        var anos = [];
                        data.ticketsPorAnnoEstados.forEach(function (ticket) {
                            if (!ticketsPorAnnoEstados[ticket.Estado]) {
                                ticketsPorAnnoEstados[ticket.Estado] = {};
                            }
                            ticketsPorAnnoEstados[ticket.Estado][ticket.Anno] = ticket.Cantidad;
                            if (!anos.includes(ticket.Anno)) {
                                anos.push(ticket.Anno);
                            }
                        });
                    };

                    if (data.ticketsPorAnnoMesEstados.length > 0) {
                        $scope.infoTicketsPorEstadosMes = data.ticketsPorAnnoMesEstados;
                        $scope.mostrarGraficoTicketsEstadosMeses = true;

                        // Procesamiento de datos
                        var ticketsPorEstadosAnnoMes = {};
                        var anos = [];
                        var meses = [];
                        var anoActual = new Date().getFullYear();

                        if (data && data.ticketsPorAnnoMesEstados && Array.isArray(data.ticketsPorAnnoMesEstados)) {
                            $scope.anosTicketsEstados = [];
                            $scope.mesesTicketsEstados = [];
                            $scope.estadosList = [];
                            $scope.ticketsPorEstadosAnnoMes = {};
                            data.ticketsPorAnnoMesEstados.forEach(function (ticket) {
                                if (!ticketsPorEstadosAnnoMes[ticket.Estado]) {
                                    ticketsPorEstadosAnnoMes[ticket.Estado] = {};
                                }
                                if (!ticketsPorEstadosAnnoMes[ticket.Estado][ticket.Anno]) {
                                    ticketsPorEstadosAnnoMes[ticket.Estado][ticket.Anno] = {};
                                }
                                ticketsPorEstadosAnnoMes[ticket.Estado][ticket.Anno][ticket.Mes] = ticket.Cantidad;

                                if (!anos.includes(ticket.Anno)) {
                                    anos.push(ticket.Anno);
                                }
                                if (!meses.includes(ticket.Mes)) {
                                    meses.push(ticket.Mes);
                                }

                                if (!$scope.anosTicketsEstados.includes(ticket.Anno)) {
                                    $scope.anosTicketsEstados.push(ticket.Anno);
                                }
                                if (!$scope.mesesTicketsEstados.includes(ticket.Mes)) {
                                    $scope.mesesTicketsEstados.push(ticket.Mes);
                                }
                                if (!$scope.estadosList.includes(ticket.Estado)) {
                                    $scope.estadosList.push(ticket.Estado);
                                }

                                if (!$scope.ticketsPorEstadosAnnoMes[ticket.Estado]) {
                                    $scope.ticketsPorEstadosAnnoMes[ticket.Estado] = {};
                                }
                                if (!$scope.ticketsPorEstadosAnnoMes[ticket.Estado][ticket.Anno]) {
                                    $scope.ticketsPorEstadosAnnoMes[ticket.Estado][ticket.Anno] = {};
                                }
                                $scope.ticketsPorEstadosAnnoMes[ticket.Estado][ticket.Anno][ticket.Mes] = ticket.Cantidad;

                            });
                        };

                        $scope.anosTicketsEstados.sort((a, b) => a - b);
                        $scope.mesesTicketsEstados.sort((a, b) => a - b);
                        $scope.estadosList.sort();

                        $scope.getCantidadAnnosEstados = function (estado, ano) {
                            return ($scope.ticketsPorEstadosAnno[estado] &&
                                $scope.ticketsPorEstadosAnno[estado][ano]) || 0;
                        };

                        $scope.getCantidadEstados = function (estado, ano, mes) {
                            return ($scope.ticketsPorEstadosAnnoMes[estado] &&
                                $scope.ticketsPorEstadosAnnoMes[estado][ano] &&
                                $scope.ticketsPorEstadosAnnoMes[estado][ano][mes]) || 0;
                        };

                        function isMonthEmpty(ano, mes) {
                            return $scope.estadosList.every(function (estado) {
                                return $scope.getCantidadEstados(estado, ano, mes) === 0;
                            });
                        };

                        function isYearEmpty(ano) {
                            return $scope.estadosList.every(function (estado) {
                                return $scope.getCantidadEstados(estado, ano) === 0;
                            });
                        };

                        $scope.getMesByNumero = function (mesNumero) {
                            var meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
                            return meses[mesNumero - 1] || '';
                        };

                        $scope.calcularTotal = function (ano, mes) {
                            var total = 0;
                            $scope.estadosList.forEach(function (estado) {
                                total += $scope.getCantidadEstados(estado, ano, mes);
                            });
                            return total;
                        };

                        // Si no se seleccionaron años, usar el año actual
                        if (anos.length === 0) {
                            anos.push(anoActual);
                        }

                        $scope.ticketsPorEstadosAnnoMes = ticketsPorEstadosAnnoMes;
                        $scope.anosTicketsEstados = anos.sort();
                        $scope.mesesTicketsEstados = meses.sort();

                        $scope.ticketsPorEstadosAnno = ticketsPorAnnoEstados;
                        $scope.anosTickets = anos.sort();

                        // Calcular totales por año
                        $scope.totalesPorAnno = {};
                        anos.forEach(function (ano) {
                            $scope.totalesPorAnno[ano] = Object.values(ticketsPorAnnoEstados).reduce(function (total, estado) {
                                return total + (estado[ano] || 0);
                            }, 0);
                        });

                        // Función para crear datasets
                        function createDatasetsAnno() {
                            var filteredLabels = [];
                            var datasets = [];

                            $scope.estadosList.forEach(function (estado) {
                                var data = [];
                                $scope.anosTicketsEstados.forEach(function (ano) {

                                    var cantidad = $scope.getCantidadAnnosEstados(estado, ano);
                                    if (cantidad > 0 || !isYearEmpty(ano)) {
                                        data.push(cantidad);
                                        if (!filteredLabels.includes(ano)) {
                                            filteredLabels.push(ano);
                                        }
                                    }
                                });

                                if (data.length > 0) {
                                    datasets.push({
                                        label: estado,
                                        data: data,
                                        backgroundColor: getRandomColor(),
                                        borderColor: 'rgba(255, 99, 132, 1)',
                                        borderWidth: 1
                                    });
                                }
                            });

                            return { datasets: datasets, labels: filteredLabels };
                        };

                        function createDatasetsMeses() {
                            var filteredLabels = [];
                            var datasets = [];

                            $scope.estadosList.forEach(function (estado) {
                                var data = [];
                                $scope.anosTicketsEstados.forEach(function (ano) {
                                    $scope.mesesTicketsEstados.forEach(function (mes) {
                                        var cantidad = $scope.getCantidadEstados(estado, ano, mes);
                                        if (cantidad > 0 || !isMonthEmpty(ano, mes)) {
                                            data.push(cantidad);
                                            if (!filteredLabels.includes(ano + '-' + $scope.getMesByNumero(mes))) {
                                                filteredLabels.push(ano + '-' + $scope.getMesByNumero(mes));
                                            }
                                        }
                                    });
                                });

                                if (data.length > 0) {
                                    datasets.push({
                                        label: estado,
                                        data: data,
                                        backgroundColor: getRandomColor(),
                                        borderColor: 'rgba(255, 99, 132, 1)',
                                        borderWidth: 1
                                    });
                                }
                            });

                            return { datasets: datasets, labels: filteredLabels };
                        };

                        // Función para generar colores aleatorios (sin cambios)
                        function getRandomColor() {
                            var letters = '0123456789ABCDEF';
                            var color = '#';
                            for (var i = 0; i < 6; i++) {
                                color += letters[Math.floor(Math.random() * 16)];
                            }
                            return color;
                        };

                        var chartDataAnnoEstados = createDatasetsAnno();
                        var ctxAnnoEstados = document.getElementById('ticketPorEstadosBarrasChartAnno').getContext('2d');
                        $scope.ticketPorEstadosBarrasChartAnno = new Chart(ctxAnnoEstados, {
                            type: 'bar',
                            data: {
                                labels: chartDataAnnoEstados.labels,
                                datasets: chartDataAnnoEstados.datasets
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        display: true,
                                        title: { display: true, text: 'Cantidad de Tickets' },
                                        ticks: { beginAtZero: true },
                                    },
                                    y: {
                                        type: 'logarithmic',
                                        display: true,
                                        title: { display: true, text: 'Año' },
                                    },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Año y Estados'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'center',
                                        anchor: 'center',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        color: '#000',
                                        font: {
                                            weight: 'bold'
                                        },
                                        formatter: function (value, context) {
                                            return value > 0 ? value : '';
                                        }
                                    },
                                    legend: {
                                        position: 'top',
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.8,
                                categoryPercentage: 0.9
                            },
                            plugins: [ChartDataLabels]
                        });

                        var chartDataMesEstados = createDatasetsMeses();
                        var ctxMesEstados = document.getElementById('ticketPorEstadosBarrasChartMes').getContext('2d');
                        $scope.ticketPorAplicaBarrasChartMes = new Chart(ctxMesEstados, {
                            type: 'bar',
                            data: {
                                labels: chartDataMesEstados.labels,
                                datasets: chartDataMesEstados.datasets
                            },
                            options: {
                                //indexAxis: 'y',
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        display: true,
                                        title: { display: true, text: 'Año - Meses' },
                                        ticks: { beginAtZero: true }
                                    },
                                    y: {
                                        type: 'logarithmic',
                                        display: true,
                                        title: { display: true, text: 'Cantidad de Tickets' },
                                    },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Año y Estados'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'center',
                                        anchor: 'center',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        color: '#000',
                                        font: {
                                            weight: 'bold'
                                        },
                                        formatter: function (value, context) {
                                            return value > 0 ? value : '';
                                        }
                                    },
                                    legend: {
                                        position: 'top',
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.8,
                                categoryPercentage: 0.9
                            },
                            plugins: [ChartDataLabels]
                        });

                       
                    };
                };

                $scope.mostrarPanelGraficosTicketsEstados = true;
            } else {
                console.log("errorrrrrr " + data);
            }
        });
    };

    /////////// garantia
    $scope.TicketsGarantiaIndicadoresGenerales = function () {
        if ($scope.ticketPorGarantiaBarrasChartAnno) {
            $scope.ticketPorGarantiaBarrasChartAnno.destroy();
        }
        if ($scope.ticketPorGarantiaBarrasChartMes) {
            $scope.ticketPorGarantiaBarrasChartMes.destroy();
        }

        $scope.mostrarPanelGraficosPorMesesYAnnosGarantia = true;

        var infoTicketsPorGarantia = $http.post("indicadores/dar-tickets-garantia-indicadores-generales/", {
            annos: $scope.annosIndicadoresGenerales,
            meses: $scope.mesesIndicadoresGenerales
        });

        infoTicketsPorGarantia.success(function (data) {
            $scope.loading.hide();
            if (data.success) {

                if (data.ticketsPorAnnoGarantia.length === 0 && data.ticketsPorAnnoMesGarantia.length === 0) {
                    $scope.mostrarPanelGraficosTicketsGarantia = false;
                    $scope.mensajeNoDataTicketsGarantia = 'No existen datos disponibles';

                } else if (data.ticketsPorAnnoGarantia.length !== 0 || data.ticketsPorAnnoMesGarantia.length !== 0) {
                    $scope.mostrarPanelGraficosTicketsGarantia = true;

                    if (data.ticketsPorAnnoGarantia.length > 0) {
                        $scope.infoTicketsPorGarantiaAnno = data.ticketsPorAnnoGarantia;
                        $scope.mostrarGraficoTicketsGarantiaAnno = true;

                        // Formatear datos para la tabla por año
                        var ticketsPorAnnoGarantia = {};
                        var anos = [];
                        data.ticketsPorAnnoGarantia.forEach(function (ticket) {
                            if (!ticketsPorAnnoGarantia[ticket.Cliente]) {
                                ticketsPorAnnoGarantia[ticket.Cliente] = {};
                            }
                            ticketsPorAnnoGarantia[ticket.Cliente][ticket.Anno] = ticket.Cantidad;
                            if (!anos.includes(ticket.Anno)) {
                                anos.push(ticket.Anno);
                            }
                        });
                    };

                    if (data.ticketsPorAnnoMesGarantia.length > 0) {
                        $scope.infoTicketsPorGarantiaMes = data.ticketsPorAnnoMesGarantia;
                        $scope.mostrarGraficoTicketsGarantiaMeses = true;

                        // Procesamiento de datos
                        var ticketsPorGarantiaAnnoMes = {};
                        var anos = [];
                        var meses = [];
                        var anoActual = new Date().getFullYear();

                        if (data && data.ticketsPorAnnoMesGarantia && Array.isArray(data.ticketsPorAnnoMesGarantia)) {
                            $scope.anosTicketsGarantia = [];
                            $scope.mesesTicketsGarantia = [];
                            $scope.clientesList = [];
                            $scope.ticketsPorGarantiaAnnoMes = {};
                            data.ticketsPorAnnoMesGarantia.forEach(function (ticket) {
                                if (!ticketsPorGarantiaAnnoMes[ticket.Cliente]) {
                                    ticketsPorGarantiaAnnoMes[ticket.Cliente] = {};
                                }
                                if (!ticketsPorGarantiaAnnoMes[ticket.Cliente][ticket.Anno]) {
                                    ticketsPorGarantiaAnnoMes[ticket.Cliente][ticket.Anno] = {};
                                }
                                ticketsPorGarantiaAnnoMes[ticket.Cliente][ticket.Anno][ticket.Mes] = ticket.Cantidad;

                                if (!anos.includes(ticket.Anno)) {
                                    anos.push(ticket.Anno);
                                }
                                if (!meses.includes(ticket.Mes)) {
                                    meses.push(ticket.Mes);
                                }

                                if (!$scope.anosTicketsGarantia.includes(ticket.Anno)) {
                                    $scope.anosTicketsGarantia.push(ticket.Anno);
                                }
                                if (!$scope.mesesTicketsGarantia.includes(ticket.Mes)) {
                                    $scope.mesesTicketsGarantia.push(ticket.Mes);
                                }
                                if (!$scope.clientesList.includes(ticket.Cliente)) {
                                    $scope.clientesList.push(ticket.Cliente);
                                }

                                if (!$scope.ticketsPorGarantiaAnnoMes[ticket.Cliente]) {
                                    $scope.ticketsPorGarantiaAnnoMes[ticket.Cliente] = {};
                                }
                                if (!$scope.ticketsPorGarantiaAnnoMes[ticket.Cliente][ticket.Anno]) {
                                    $scope.ticketsPorGarantiaAnnoMes[ticket.Cliente][ticket.Anno] = {};
                                }
                                $scope.ticketsPorGarantiaAnnoMes[ticket.Cliente][ticket.Anno][ticket.Mes] = ticket.Cantidad;

                            });
                        };

                        $scope.anosTicketsGarantia.sort((a, b) => a - b);
                        $scope.mesesTicketsGarantia.sort((a, b) => a - b);
                        $scope.clientesList.sort();

                        $scope.getCantidadAnnosGarantia  = function (cliente, ano) {
                            return ($scope.ticketsPorGarantiaAnno[cliente] &&
                                $scope.ticketsPorGarantiaAnno[cliente][ano]) || 0;
                        };

                        $scope.getCantidadGarantia = function (cliente, ano, mes) {
                            return ($scope.ticketsPorGarantiaAnnoMes[cliente] &&
                                $scope.ticketsPorGarantiaAnnoMes[cliente][ano] &&
                                $scope.ticketsPorGarantiaAnnoMes[cliente][ano][mes]) || 0;
                        };

                        function isMonthEmpty(ano, mes) {
                            return $scope.clientesList.every(function (cliente) {
                                return $scope.getCantidadGarantia(cliente, ano, mes) === 0;
                            });
                        };

                        function isYearEmpty(ano) {
                            return $scope.clientesList.every(function (cliente) {
                                return $scope.getCantidadGarantia(cliente, ano) === 0;
                            });
                        };

                        $scope.getMesByNumero = function (mesNumero) {
                            var meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
                            return meses[mesNumero - 1] || '';
                        };

                        $scope.calcularTotal = function (ano, mes) {
                            var total = 0;
                            $scope.clientesList.forEach(function (clientes) {
                                total += $scope.getCantidadGarantia(clientes, ano, mes);
                            });
                            return total;
                        };

                        // Si no se seleccionaron años, usar el año actual
                        if (anos.length === 0) {
                            anos.push(anoActual);
                        }

                        $scope.ticketsPorGarantiaAnnoMes = ticketsPorGarantiaAnnoMes;
                        $scope.anosTicketsGarantia = anos.sort();
                        $scope.mesesTicketsGarantia = meses.sort();

                        $scope.ticketsPorGarantiaAnno = ticketsPorAnnoGarantia;
                        $scope.anosTickets = anos.sort();

                        // Calcular totales por año
                        $scope.totalesPorAnno = {};
                        anos.forEach(function (ano) {
                            $scope.totalesPorAnno[ano] = Object.values(ticketsPorAnnoGarantia).reduce(function (total, cliente) {
                                return total + (cliente[ano] || 0);
                            }, 0);
                        });

                        // Función para crear datasets
                        function createDatasetsAnno() {
                            var filteredLabels = [];
                            var datasets = [];

                            $scope.clientesList.forEach(function (cliente) {
                                var data = [];
                                $scope.anosTicketsGarantia.forEach(function (ano) {

                                    var cantidad = $scope.getCantidadAnnosGarantia(cliente, ano);
                                    if (cantidad > 0 || !isYearEmpty(ano)) {
                                        data.push(cantidad);
                                        if (!filteredLabels.includes(ano)) {
                                            filteredLabels.push(ano);
                                        }
                                    }
                                });

                                if (data.length > 0) {
                                    datasets.push({
                                        label: cliente,
                                        data: data,
                                        backgroundColor: getRandomColor(),
                                        borderColor: 'rgba(255, 99, 132, 1)',
                                        borderWidth: 1
                                    });
                                }
                            });

                            return { datasets: datasets, labels: filteredLabels };
                        };

                        function createDatasetsMeses() {
                            var filteredLabels = [];
                            var datasets = [];

                            $scope.clientesList.forEach(function (cliente) {
                                var data = [];
                                $scope.anosTicketsGarantia.forEach(function (ano) {
                                    $scope.mesesTicketsGarantia.forEach(function (mes) {
                                        var cantidad = $scope.getCantidadGarantia(cliente, ano, mes);
                                        if (cantidad > 0 || !isMonthEmpty(ano, mes)) {
                                            data.push(cantidad);
                                            if (!filteredLabels.includes(ano + '-' + $scope.getMesByNumero(mes))) {
                                                filteredLabels.push(ano + '-' + $scope.getMesByNumero(mes));
                                            }
                                        }
                                    });
                                });

                                if (data.length > 0) {
                                    datasets.push({
                                        label: cliente,
                                        data: data,
                                        backgroundColor: getRandomColor(),
                                        borderColor: 'rgba(255, 99, 132, 1)',
                                        borderWidth: 1
                                    });
                                }
                            });

                            return { datasets: datasets, labels: filteredLabels };
                        };

                        // Función para generar colores aleatorios (sin cambios)
                        function getRandomColor() {
                            var letters = '0123456789ABCDEF';
                            var color = '#';
                            for (var i = 0; i < 6; i++) {
                                color += letters[Math.floor(Math.random() * 16)];
                            }
                            return color;
                        };

                        var chartDataAnnoGarantia = createDatasetsAnno();
                        var ctxAnnoGarantia = document.getElementById('ticketPorGarantiaBarrasChartAnno').getContext('2d');
                        $scope.ticketPorGarantiaBarrasChartAnno = new Chart(ctxAnnoGarantia, {
                            type: 'bar',
                            data: {
                                labels: chartDataAnnoGarantia.labels,
                                datasets: chartDataAnnoGarantia.datasets
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        display: true,
                                        title: { display: true, text: 'Cantidad de Tickets' },
                                        ticks: { beginAtZero: true },
                                    },
                                    y: {
                                        type: 'logarithmic',
                                        display: true,
                                        title: { display: true, text: 'Año' },
                                    },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Año y Clientes'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'center',
                                        anchor: 'center',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        color: '#000',
                                        font: {
                                            weight: 'bold'
                                        },
                                        formatter: function (value, context) {
                                            return value > 0 ? value : '';
                                        }
                                    },
                                    legend: {
                                        position: 'top',
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.8,
                                categoryPercentage: 0.9
                            },
                            plugins: [ChartDataLabels]
                        });

                        var chartDataMesGarantia = createDatasetsMeses();
                        var ctxMesGarantia = document.getElementById('ticketPorGarantiaBarrasChartMes').getContext('2d');
                        $scope.ticketPorGarantiaBarrasChartMes = new Chart(ctxMesGarantia, {
                            type: 'bar',
                            data: {
                                labels: chartDataMesGarantia.labels,
                                datasets: chartDataMesGarantia.datasets
                            },
                            options: {
                                //indexAxis: 'y',
                                responsive: true,
                                maintainAspectRatio: false,
                                scales: {
                                    x: {
                                        display: true,
                                        title: { display: true, text: 'Año - Meses' },
                                        ticks: { beginAtZero: true }
                                    },
                                    y: {
                                        type: 'logarithmic',
                                        display: true,
                                        title: { display: true, text: 'Cantidad de Tickets' },
                                    },
                                },
                                plugins: {
                                    title: {
                                        display: true,
                                        text: 'Tickets por Año y Clientes'
                                    },
                                    datalabels: {
                                        display: true,
                                        align: 'center',
                                        anchor: 'center',
                                        backgroundColor: '#D3D3D3',
                                        borderRadius: 3,
                                        color: '#000',
                                        font: {
                                            weight: 'bold'
                                        },
                                        formatter: function (value, context) {
                                            return value > 0 ? value : '';
                                        }
                                    },
                                    legend: {
                                        position: 'top',
                                    }
                                },
                                layout: {
                                    padding: {
                                        top: 20
                                    }
                                },
                                barPercentage: 0.8,
                                categoryPercentage: 0.9
                            },
                            plugins: [ChartDataLabels]
                        });


                    };
                };

                $scope.mostrarPanelGraficosTicketsGarantia = true;
            } else {
                console.log("errorrrrrr " + data);
            }
        });
    };

    function getRandomColor() {
        var letters = '0123456789ABCDEF';
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

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

    function isSpecificDateFormat(dateString) {
        const regex = /^[A-Z][a-z]{2}\s[A-Z][a-z]{2}\s\d{2}\s\d{4}\s\d{2}:\d{2}:\d{2}\sGMT[-+]\d{4}\s\(.*\)$/;
        return regex.test(dateString);
    }

}]);
