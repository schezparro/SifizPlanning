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
    var fechaActual = new Date();
    $scope.fechaInicio = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.fechaFin = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);
    $scope.initDatepickersTickets();

    $scope.mostrarGraficos = false;

    $scope.buscarOtrosDatos = function () {

        console.log($scope.fechaInicio);
        console.log($scope.fechaFin);

        $scope.TicketsNuevos();
        $scope.TicketsCerrados();
        $scope.TicketsEnGestion();
        $scope.TicketsPorCategoria();
        $scope.TicketsPorEstados();
        $scope.TicketsAplica();
        $scope.TicketsPorMantenimiento();
        $scope.TicketsPorGarantia();
        $scope.TicketsPorClientesEstados();

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
                $scope.infoTickets = data.infoTickets;
                $scope.totalCantidades = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var semanas = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTickets.forEach(function (ticket) {
                    semanas.push(ticket.Descripcion);
                    cantidades.push(ticket.Cantidad);
                });

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('tendenciaChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'line',
                    data: {
                        labels: semanas, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Tickets ingresados',
                            data: cantidades, // Datos de las cantidades
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

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
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
                $scope.infoTicketsCerrados = data.infoTickets;
                $scope.totalCantidadesCerrados = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var semanas = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTicketsCerrados.forEach(function (ticket) {
                    semanas.push(ticket.Descripcion);
                    cantidades.push(ticket.Cantidad);
                });

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketCerradosChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'line',
                    data: {
                        labels: semanas, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Tickets cerrados',
                            data: cantidades, // Datos de las cantidades
                            fill: true, // Habilita el relleno
                            borderColor: '#ffc0cb', // Color del borde en rosado
                            backgroundColor: '#ffc0cb', // Color de relleno en rosado
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

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
                alert("Error: " + data.msg);
            }
        });
    };

    //TICKETS EN GESTION
    $scope.TicketsEnGestion = function () {
        if ($scope.ticketEnGestionChart) {
            $scope.ticketEnGestionChart.destroy();
        }

        var infoTicketsEnGestion = $http.post("indicadores/dar-tickets-en-gestion/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsEnGestion.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                $scope.infoTicketsEnGestion = data.infoTickets;
                $scope.totalCantidadesTicketsEnGestion = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var semanas = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTicketsEnGestion.forEach(function (ticket) {
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketEnGestionChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'bar', // Cambiado a 'bar'
                    data: {
                        labels: semanas, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Tickets en gestión',
                            data: cantidades, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
                alert("Error: " + data.msg);
            }
        });
    };

    //TICKETS POR CATEGORIAS
    $scope.TicketsPorCategoria = function () {

        if ($scope.ticketPorCategoriaBarrasChart) {
            $scope.ticketPorCategoriaBarrasChart.destroy();
        }

        if ($scope.ticketPorCategoriaPastelChart) {
            $scope.ticketPorCategoriaPastelChart.destroy();
        }

        var infoTicketsPorCategorias = $http.post("indicadores/dar-tickets-por-categorias/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });
        infoTicketsPorCategorias.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                $scope.infoTicketsPorCategorias = data.infoTickets;
                $scope.totalCantidadesTicketsPorCategoria = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var categorias = [];
                var cantidades = [];
                var porcentaje = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTicketsPorCategorias.forEach(function (ticket) {
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketPorCategoriaBarrasChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'bar', // Cambiado a 'bar'
                    data: {
                        labels: categorias, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Cantidades por categorías',
                            data: cantidades, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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

                var tendenciaChartCtx1 = document.getElementById('ticketPorCategoriaPastelChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx1, {
                    type: 'doughnut',
                    data: {
                        labels: cantidades,
                        datasets: [{
                            backgroundColor: coloresAleatorios, // Usamos getRandomColor para generar colores aleatorios
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




            };

        });
    };

    //TICKETS POR ESTADOS
    $scope.TicketsPorEstados = function () {

        if ($scope.ticketPorEstadoBarrasChart) {
            $scope.ticketPorEstadoBarrasChart.destroy();
        }

        if ($scope.ticketPorEstadoPastelChart) {
            $scope.ticketPorEstadoPastelChart.destroy();
        }

        var infoTicketsPorEstados = $http.post("indicadores/dar-tickets-por-estados/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsPorEstados.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                $scope.infoTicketsPorEstados = data.infoTickets;
                $scope.totalCantidadesTicketsPorEstados = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var estados = [];
                var cantidades = [];
                var porcentaje = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTicketsPorEstados.forEach(function (ticket) {
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketPorEstadoBarrasChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'bar', // Cambiado a 'bar'
                    data: {
                        labels: estados, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Cantidades por estados',
                            data: cantidades, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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

                var tendenciaChartCtx1 = document.getElementById('ticketPorEstadoPastelChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx1, {
                    type: 'bar', // Cambiado a 'horizontalBar' para barras horizontales
                    data: {
                        labels: estados, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Porciento por estados',
                            data: porcentaje, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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
                            x: { // Cambiado de 'y' a 'x' para ajustar el eje horizontal
                                beginAtZero: true
                            }
                        }
                    },
                    plugins: [ChartDataLabels]
                });

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
                alert("Error: " + data.msg);
            }
        });
    };

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
                $scope.infoTicketsPorAplica = data.infoTickets;
                $scope.totalCantidadesTicketsPorAplica = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var aplica = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                var tendenciaChartCtx = document.getElementById('ticketPorAplicaBarrasChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'bar', // Cambiado a 'bar'
                    data: {
                        labels: aplica, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Cantidad de tickets aplicados a:',
                            data: cantidades, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
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
                $scope.infoTicketsPorMantenimiento = data.infoTickets;
                $scope.totalCantidadesTicketsPorMantenimiento = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var clientes = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                var tendenciaChartCtx = document.getElementById('ticketPorMttoBarrasChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'bar', // Cambiado a 'bar'
                    data: {
                        labels: clientes, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Cantidad de tickets por clientes en mantenimiento',
                            data: cantidades, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
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
                $scope.infoTicketsPorGarantia = data.infoTickets;
                $scope.totalCantidadesTicketsPorGarantia = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var clientes = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                var tendenciaChartCtx = document.getElementById('ticketPorGarantiaBarrasChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'bar', // Cambiado a 'bar'
                    data: {
                        labels: clientes, // Etiquetas de las semanas
                        datasets: [{
                            label: 'Cantidad de tickets por clientes en garantía técnica',
                            data: cantidades, // Datos de las cantidades
                            backgroundColor: coloresAleatorios, // Asignar colores aleatorios a cada barra
                            hoverBackgroundColor: coloresAleatorios, // Mantener el color al pasar el mouse
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

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
                alert("Error: " + data.msg);
            }
        });
    };

    //TICKET POR GARANTIA TECNICA
    $scope.TicketsPorClientesEstados = function () {

        if ($scope.ticketPorClienteEstadoBarrasChart1) {
            $scope.ticketPorClienteEstadoBarrasChart1.destroy();
        }

        if ($scope.ticketPorClienteEstadoBarrasChart2) {
            $scope.ticketPorClienteEstadoBarrasChart2.destroy();
        }

        var infoTicketsPorClienteEstado = $http.post("indicadores/dar-tickets-por-cliente-estado/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTicketsPorClienteEstado.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                $scope.infoTicketsPorClienteEstado = data.infoTickets;
                $scope.totalCantidadesTicketsPorClientesEstado = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var clientes = [];
                var estados = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTicketsPorClienteEstado.forEach(function (ticket) {
                    clientes.push(ticket.Cliente);
                    estados.push(ticket.Estado);
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Agrupar los tickets por estado
                //var ticketsPorEstado = {};
                //$scope.infoTicketsPorClienteEstado.forEach(function (ticket) {
                //    if (!ticketsPorEstado[ticket.Estado]) {
                //        ticketsPorEstado[ticket.Estado] = [];
                //    }
                //    ticketsPorEstado[ticket.Estado].push(ticket);
                //});

                //// Crear un conjunto de datos para cada estado
                //var datasets = Object.keys(ticketsPorEstado).map(function (estado) {
                //    var data = clientes.map(function (cliente) {
                //        var ticket = ticketsPorEstado[estado].find(function (t) { return t.Cliente === cliente; });
                //        return ticket ? ticket.Cantidad : 0;
                //    });
                //    return {
                //        label: estado,
                //        data: data,
                //        backgroundColor: getRandomColor(),
                //        borderColor: 'rgba(255, 99, 132, 1)',
                //        borderWidth: 1
                //    };
                //});

                //var tendenciaChartCtx = document.getElementById('ticketPorClienteEstadoBarrasChart').getContext('2d');
                //var myChart = new Chart(tendenciaChartCtx, {
                //    type: 'bar',
                //    data: {
                //        labels: clientes,
                //        datasets: datasets
                //    },
                //    options: {
                //        scales: {
                //            y: {
                //                beginAtZero: true
                //            }
                //        },
                //        plugins: {
                //            title: {
                //                display: true,
                //                text: 'Tickets por Cliente y Estado'
                //            }
                //        }
                //    }
                //});
                var clientes1 = {};
                var ticketsPorEstado = {};
                $scope.infoTicketsPorClienteEstado.forEach(function (ticket) {
                    if (!ticketsPorEstado[ticket.Estado]) {
                        ticketsPorEstado[ticket.Estado] = [];
                    }
                    ticketsPorEstado[ticket.Estado].push(ticket);

                    if (!clientes1[ticket.Cliente]) {
                        clientes1[ticket.Cliente] = {};
                    }
                    clientes1[ticket.Cliente][ticket.Estado] = ticket.Cantidad;
                });

                // Obtén todos los estados únicos
                let estadosUnicos = {};
                for (let cliente in clientes1) {
                    for (let estado in clientes1[cliente]) {
                        estadosUnicos[estado] = estado;
                    }
                }

                $scope.clientes1 = clientes1;
                $scope.estadosUnicos = estadosUnicos;

                console.log(clientes1);
                console.log(estadosUnicos);

                // Obtén la lista de clientes únicos
                var clientes = [...new Set($scope.infoTicketsPorClienteEstado.map(ticket => ticket.Cliente))];
                console.log(clientes);

                // Divide tus clientes en dos partes
                var mitad = Math.ceil(clientes.length / 2); // Redondea hacia arriba para manejar números impares
                var clientes1 = clientes.slice(0, mitad);
                var clientes2 = clientes.slice(mitad);

                // Crea un conjunto de datos para cada estado y cada grupo de clientes
                var datasets1 = createDatasets(clientes1, ticketsPorEstado);
                var datasets2 = createDatasets(clientes2, ticketsPorEstado);

                // Crea los gráficos
                createChart('ticketPorClienteEstadoBarrasChart1', clientes1, datasets1);
                createChart('ticketPorClienteEstadoBarrasChart2', clientes2, datasets2);

                function createDatasets(clientes, ticketsPorEstado) {
                    return Object.keys(ticketsPorEstado).map(function (estado) {
                        var data = clientes.map(function (cliente) {
                            var ticket = ticketsPorEstado[estado].find(function (t) { return t.Cliente === cliente; });
                            return ticket ? ticket.Cantidad : 0;
                        });
                        return {
                            label: estado,
                            data: data,
                            backgroundColor: getRandomColor(),
                            borderColor: 'rgba(255, 99, 132, 1)',
                            borderWidth: 1
                        };
                    });
                }

                function createChart(canvasId, clientes, datasets) {
                    var ctx = document.getElementById(canvasId).getContext('2d');
                    new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: clientes,
                            datasets: datasets
                        },
                        options: {
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            },
                            plugins: {
                                title: {
                                    display: true,
                                    text: 'Tickets por Cliente y Estado'
                                }
                            }
                        }
                    });
                }

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
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

}]);
