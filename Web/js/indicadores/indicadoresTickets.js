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

    //TICKETS EN GESTION
    //$scope.TicketsEnGestion = function () {

    //    var infoTicketsEnGestion = $http.post("indicadores/dar-tickets-en-gestion/", {
    //        fechaInicio: $scope.fechaInicio,
    //        fechaFin: $scope.fechaFin
    //    });

    //    infoTicketsEnGestion.success(function (data) {
    //        $scope.loading.hide();

    //        if (data.success) {
    //            $scope.infoTicketsEnGestion = data.infoTickets;
    //            $scope.totalCantidadesTicketsEnGestion = data.totalCantidades;
    //            $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

    //            // Preparar datos para el gráfico de líneas de tendencia temporal
    //            var semanas = [];
    //            var cantidades = [];

    //            // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
    //            $scope.infoTicketsEnGestion.forEach(function (ticket) {
    //                semanas.push(ticket.Descripcion);
    //                cantidades.push(ticket.Cantidad);
    //            });

    //            function getRandomColor() {
    //                var letters = '0123456789ABCDEF';
    //                var color = '#';
    //                for (var i = 0; i < 6; i++) {
    //                    color += letters[Math.floor(Math.random() * 16)];
    //                }
    //                return color;
    //            }

    //            // Ahora puedes usar getRandomColor() sin problemas
    //            var coloresAleatorios = cantidades.map(getRandomColor);

    //            // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
    //            var tendenciaChartCtx = document.getElementById('ticketEnGestionChart').getContext('2d');
    //            var tendenciaChart = new Chart(tendenciaChartCtx, {
    //                type: 'bar', // Cambiado a 'bar'
    //                data: {
    //                    labels: semanas, // Etiquetas de las semanas
    //                    datasets: [{
    //                        label: 'Tickets en gestión',
    //                        data: cantidades, // Datos de las cantidades
    //                        backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
    //                        hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
    //                        borderWidth: 1
    //                    }]
    //                },
    //                options: {
    //                    plugins: {
    //                        datalabels: {
    //                            display: true,
    //                            align: 'top',
    //                            backgroundColor: '#D3D3D3',
    //                            borderRadius: 3,
    //                            font: {
    //                                weight: 'bold'
    //                            }
    //                        }
    //                    },
    //                    scales: {
    //                        y: {
    //                            beginAtZero: true
    //                        }
    //                    }
    //                },
    //                plugins: [ChartDataLabels]
    //            });

    //            $scope.mostrarGraficos = true;
    //        } else {
    //            // Manejar el caso en que data.success es false
    //            alert("Error: " + data.msg);
    //        }
    //    });
    //};

    ////TICKETS POR CATEGORIAS
    //$scope.TicketsPorCategoria = function () {

    //    var infoTicketsPorCategorias = $http.post("indicadores/dar-tickets-por-categorias/", {
    //        fechaInicio: $scope.fechaInicio,
    //        fechaFin: $scope.fechaFin
    //    });
    //    infoTicketsPorCategorias.success(function (data) {
    //        $scope.loading.hide();

    //        if (data.success) {
    //            $scope.infoTicketsPorCategorias = data.infoTickets;
    //            $scope.totalCantidadesTicketsPorCategoria = data.totalCantidades;
    //            $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

    //            // Preparar datos para el gráfico de líneas de tendencia temporal
    //            var categorias = [];
    //            var cantidades = [];
    //            var porcentaje = [];

    //            // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
    //            $scope.infoTicketsPorCategorias.forEach(function (ticket) {
    //                categorias.push(ticket.Categoria);
    //                cantidades.push(ticket.Cantidad);
    //                porcentaje.push(ticket.Porcentaje);
    //            });

    //            function getRandomColor() {
    //                var letters = '0123456789ABCDEF';
    //                var color = '#';
    //                for (var i = 0; i < 6; i++) {
    //                    color += letters[Math.floor(Math.random() * 16)];
    //                }
    //                return color;
    //            }

    //            // Ahora puedes usar getRandomColor() sin problemas
    //            var coloresAleatorios = cantidades.map(getRandomColor);

    //            // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
    //            var tendenciaChartCtx = document.getElementById('ticketPorCategoriaBarrasChart').getContext('2d');
    //            var tendenciaChart = new Chart(tendenciaChartCtx, {
    //                type: 'bar', // Cambiado a 'bar'
    //                data: {
    //                    labels: categorias, // Etiquetas de las semanas
    //                    datasets: [{
    //                        label: 'Cantidades por categorías',
    //                        data: cantidades, // Datos de las cantidades
    //                        backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
    //                        hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
    //                        borderWidth: 1
    //                    }]
    //                },
    //                options: {
    //                    plugins: {
    //                        datalabels: {
    //                            display: true,
    //                            align: 'top',
    //                            backgroundColor: '#D3D3D3',
    //                            borderRadius: 3,
    //                            font: {
    //                                weight: 'bold'
    //                            }
    //                        }
    //                    },
    //                    scales: {
    //                        y: {
    //                            beginAtZero: true
    //                        }
    //                    }
    //                },
    //                plugins: [ChartDataLabels]
    //            });

    //            var tendenciaChartCtx1 = document.getElementById('ticketPorCategoriaPastelChart').getContext('2d');
    //            var tendenciaChart = new Chart(tendenciaChartCtx1, {
    //                type: 'doughnut',
    //                data: {
    //                    labels: cantidades,
    //                    datasets: [{
    //                        backgroundColor: coloresAleatorios, // Usamos getRandomColor para generar colores aleatorios
    //                        hoverBorderColor: 'white',
    //                        data: cantidades,
    //                        datalabels: {
    //                            labels: {
    //                                name: {
    //                                    align: 'center',
    //                                    anchor: 'center',
    //                                    color: function (ctx) {
    //                                        return ctx.dataset.backgroundColor;
    //                                    },
    //                                    font: { size: 16 },
    //                                    formatter: function (value, ctx) {
    //                                        return categorias[ctx.dataIndex];
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }]
    //                },
    //                options: {
    //                    plugins: {
    //                        datalabels: {
    //                            color: 'white',
    //                            display: function (ctx) {
    //                                return ctx.dataset.data[ctx.dataIndex] > 10;
    //                            },
    //                            font: {
    //                                weight: 'bold',
    //                            },
    //                        }
    //                    },
    //                    plugins: [ChartDataLabels]
    //                }
    //            });




    //        };

    //    });
    //};

    ////TICKETS POR ESTADOS
    //$scope.TicketsPorEstados = function () {

    //    var infoTicketsPorEstados = $http.post("indicadores/dar-tickets-por-estados/", {
    //        fechaInicio: $scope.fechaInicio,
    //        fechaFin: $scope.fechaFin
    //    });

    //    infoTicketsPorEstados.success(function (data) {
    //        $scope.loading.hide();

    //        if (data.success) {
    //            $scope.infoTicketsPorEstados = data.infoTickets;
    //            $scope.totalCantidadesTicketsPorEstados = data.totalCantidades;
    //            $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

    //            // Preparar datos para el gráfico de líneas de tendencia temporal
    //            var estados = [];
    //            var cantidades = [];
    //            var porcentaje = [];

    //            // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
    //            $scope.infoTicketsPorEstados.forEach(function (ticket) {
    //                estados.push(ticket.Estado);
    //                cantidades.push(ticket.Cantidad);
    //                porcentaje.push(ticket.Porcentaje);
    //            });

    //            function getRandomColor() {
    //                var letters = '0123456789ABCDEF';
    //                var color = '#';
    //                for (var i = 0; i < 6; i++) {
    //                    color += letters[Math.floor(Math.random() * 16)];
    //                }
    //                return color;
    //            }

    //            // Ahora puedes usar getRandomColor() sin problemas
    //            var coloresAleatorios = cantidades.map(getRandomColor);

    //            // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
    //            var tendenciaChartCtx = document.getElementById('ticketPorEstadoBarrasChart').getContext('2d');
    //            var tendenciaChart = new Chart(tendenciaChartCtx, {
    //                type: 'bar', // Cambiado a 'bar'
    //                data: {
    //                    labels: estados, // Etiquetas de las semanas
    //                    datasets: [{
    //                        label: 'Cantidades por estados',
    //                        data: cantidades, // Datos de las cantidades
    //                        backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
    //                        hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
    //                        borderWidth: 1
    //                    }]
    //                },
    //                options: {
    //                    plugins: {
    //                        datalabels: {
    //                            display: true,
    //                            align: 'top',
    //                            backgroundColor: '#D3D3D3',
    //                            borderRadius: 3,
    //                            font: {
    //                                weight: 'bold'
    //                            }
    //                        }
    //                    },
    //                    scales: {
    //                        y: {
    //                            beginAtZero: true
    //                        }
    //                    }
    //                },
    //                plugins: [ChartDataLabels]
    //            });

    //            var tendenciaChartCtx1 = document.getElementById('ticketPorEstadoPastelChart').getContext('2d');
    //            var tendenciaChart = new Chart(tendenciaChartCtx1, {
    //                type: 'bar', // Cambiado a 'horizontalBar' para barras horizontales
    //                data: {
    //                    labels: estados, // Etiquetas de las semanas
    //                    datasets: [{
    //                        label: 'Porciento por estados',
    //                        data: porcentaje, // Datos de las cantidades
    //                        backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
    //                        hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
    //                        borderWidth: 1
    //                    }]
    //                },
    //                options: {
    //                    plugins: {
    //                        datalabels: {
    //                            display: true,
    //                            align: 'top',
    //                            backgroundColor: '#D3D3D3',
    //                            borderRadius: 3,
    //                            font: {
    //                                weight: 'bold'
    //                            }
    //                        }
    //                    },
    //                    scales: {
    //                        x: { // Cambiado de 'y' a 'x' para ajustar el eje horizontal
    //                            beginAtZero: true
    //                        }
    //                    }
    //                },
    //                plugins: [ChartDataLabels]
    //            });

    //            $scope.mostrarGraficos = true;
    //        } else {
    //            // Manejar el caso en que data.success es false
    //            alert("Error: " + data.msg);
    //        }
    //    });
    //};

    //RICKETS APLICADOS A:
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

    //TICKET POR Clientes por Estados
    //$scope.TicketsPorClientesEstados = function () {

    //    var infoTicketsPorClienteEstado = $http.post("indicadores/dar-tickets-por-cliente-estado/", {
    //        fechaInicio: $scope.fechaInicio,
    //        fechaFin: $scope.fechaFin
    //    });

    //    infoTicketsPorClienteEstado.success(function (data) {
    //        $scope.loading.hide();

    //        if (data.success) {

    //            $scope.infoTicketsPorClienteEstado = data.infoTickets;
    //            $scope.totalCantidadesTicketsPorClientesEstado = data.totalCantidades;
    //            $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

    //            // Crear una estructura para los datos de la tabla
    //            $scope.clientesEstados = {};
    //            var cantidades = [];

    //            // Llenar los datos del objeto clientesEstados
    //            angular.forEach($scope.infoTicketsPorClienteEstado, function (ticket) {
    //                // Si el cliente no existe en el objeto, agrégalo
    //                if (!$scope.clientesEstados[ticket.Cliente]) {
    //                    $scope.clientesEstados[ticket.Cliente] = {};
    //                }

    //                // Agregar o actualizar la cantidad para el estado actual
    //                $scope.clientesEstados[ticket.Cliente][ticket.Estado] = ticket.Cantidad;
    //                cantidades.push(ticket.Cantidad);
    //            });

    //            // Calcular el total general para cada cliente
    //            angular.forEach($scope.clientesEstados, function (val, key) {
    //                var totalGeneral = 0;
    //                angular.forEach(val, function (value, estado) {
    //                    totalGeneral += value;
    //                });
    //                $scope.clientesEstados[key].TotalGeneral = totalGeneral;
    //            });

    //            // Obtener la lista de estados disponibles
    //            if ($scope.infoTicketsPorClienteEstado && $scope.infoTicketsPorClienteEstado.length > 0 && $scope.infoTicketsPorClienteEstado[0].Cliente) {
    //                $scope.estados = Object.keys($scope.clientesEstados[$scope.infoTicketsPorClienteEstado[0].Cliente]);
    //            } else {
    //                $scope.estados = []; // O cualquier valor por defecto que prefieras
    //            }


    //            function getRandomColor() {
    //                var letters = '0123456789ABCDEF';
    //                var color = '#';
    //                for (var i = 0; i < 6; i++) {
    //                    color += letters[Math.floor(Math.random() * 16)];
    //                }
    //                return color;
    //            }

    //            // Ahora puedes usar getRandomColor() sin problemas
    //            var coloresAleatorios = cantidades.map(getRandomColor);

    //            var clientes1 = {};
    //            var ticketsPorEstado = {};
    //            $scope.infoTicketsPorClienteEstado.forEach(function (ticket) {
    //                if (!ticketsPorEstado[ticket.Estado]) {
    //                    ticketsPorEstado[ticket.Estado] = [];
    //                }
    //                ticketsPorEstado[ticket.Estado].push(ticket);

    //                if (!clientes1[ticket.Cliente]) {
    //                    clientes1[ticket.Cliente] = {};
    //                }
    //                clientes1[ticket.Cliente][ticket.Estado] = ticket.Cantidad;
    //            });

    //            // Obtén todos los estados únicos
    //            let estadosUnicos = {};
    //            for (let cliente in clientes1) {
    //                for (let estado in clientes1[cliente]) {
    //                    estadosUnicos[estado] = estado;
    //                }
    //            }

    //            $scope.clientes1 = clientes1;
    //            $scope.estadosUnicos = estadosUnicos;

    //            console.log(clientes1);
    //            console.log(estadosUnicos);

    //            // Obtén la lista de clientes únicos
    //            var clientes = [...new Set($scope.infoTicketsPorClienteEstado.map(ticket => ticket.Cliente))];
    //            console.log(clientes);

    //            // Divide tus clientes en dos partes
    //            var mitad = Math.ceil(clientes.length / 2); // Redondea hacia arriba para manejar números impares
    //            var clientes1 = clientes.slice(0, mitad);
    //            var clientes2 = clientes.slice(mitad);

    //            // Crea un conjunto de datos para cada estado y cada grupo de clientes
    //            var datasets1 = createDatasets(clientes1, ticketsPorEstado);
    //            var datasets2 = createDatasets(clientes2, ticketsPorEstado);

    //            // Crea los gráficos
    //            createChart('ticketPorClienteEstadoBarrasChart1', clientes1, datasets1);
    //            createChart('ticketPorClienteEstadoBarrasChart2', clientes2, datasets2);

    //            function createDatasets(clientes, ticketsPorEstado) {
    //                return Object.keys(ticketsPorEstado).map(function (estado) {
    //                    var data = clientes.map(function (cliente) {
    //                        var ticket = ticketsPorEstado[estado].find(function (t) { return t.Cliente === cliente; });
    //                        return ticket ? ticket.Cantidad : 0;
    //                    });
    //                    return {
    //                        label: estado,
    //                        data: data,
    //                        backgroundColor: getRandomColor(),
    //                        borderColor: 'rgba(255, 99, 132, 1)',
    //                        borderWidth: 1
    //                    };
    //                });
    //            }

    //            function createChart(canvasId, clientes, datasets) {
    //                var ctx = document.getElementById(canvasId).getContext('2d');
    //                new Chart(ctx, {
    //                    type: 'bar',
    //                    data: {
    //                        labels: clientes,
    //                        datasets: datasets
    //                    },
    //                    options: {
    //                        scales: {
    //                            y: {
    //                                beginAtZero: true
    //                            }
    //                        },
    //                        plugins: {
    //                            title: {
    //                                display: true,
    //                                text: 'Tickets por Cliente y Estado'
    //                            }
    //                        }
    //                    }
    //                });
    //            }

    //            $scope.mostrarGraficos = true;
    //        } else {
    //            // Manejar el caso en que data.success es false
    //            alert("Error: " + data.msg);
    //        }
    //    });
    //};

    // *****************************************************************************************************************************************************************************

    let clienteSeleccionado;

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
