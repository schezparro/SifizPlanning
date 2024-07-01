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

    $scope.mostrarGraficos = false;

    $scope.buscarOtrosDatos = function () {

        if (isSpecificDateFormat($scope.fechaInicio.toString())) {
            $scope.fechaInicio = dateToStr($scope.fechaInicio);
        }

        // Haz lo mismo para otras fechas que necesites comprobar
        if (isSpecificDateFormat($scope.fechaFin.toString())) {
            $scope.fechaFin = dateToStr($scope.fechaFin);
        }

        console.log($scope.fechaInicio);
        console.log($scope.fechaFin);

        // Convierte las fechas a string usando dateToStr
       // $scope.fechaInicio = dateToStr(fechaInicio);
       // $scope.fechaFin = dateToStr(fechaFin);

        console.log("fecha inicio " + $scope.fechaInicio);
        console.log("fecha fin " + $scope.fechaFin);

        if ($scope.tendenciaChart) {
            $scope.tendenciaChart.destroy();
        }

        if ($scope.ticketCerradosChart) {
            $scope.ticketCerradosChart.destroy();
        }

        if ($scope.ticketEnGestionChart) {
            $scope.ticketEnGestionChart.destroy();
        }

        if ($scope.ticketPorCategoriaBarrasChart) {
            $scope.ticketPorCategoriaBarrasChart.destroy();
        }

        if ($scope.ticketPorCategoriaPastelChart) {
            $scope.ticketPorCategoriaPastelChart.destroy();
        }

        if ($scope.ticketPorEstadoBarrasChart) {
            $scope.ticketPorEstadoBarrasChart.destroy();
        }

        if ($scope.ticketPorEstadoPastelChart) {
            $scope.ticketPorEstadoPastelChart.destroy();
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

        if ($scope.ticketPorClienteEstadoBarrasChart1) {
            $scope.ticketPorClienteEstadoBarrasChart1.destroy();
        }

        if ($scope.ticketPorClienteEstadoBarrasChart2) {
            $scope.ticketPorClienteEstadoBarrasChart2.destroy();
        }

        $scope.TicketsNuevos();
        $scope.TicketsCerrados();
        //$scope.TicketsEnGestion();
        //$scope.TicketsPorCategoria();
        //$scope.TicketsPorEstados();
        $scope.TicketsAplica();
        $scope.TicketsPorMantenimiento();
        $scope.TicketsPorGarantia();
        //$scope.TicketsPorClientesEstados();

    };

    //TICKETS NUEVOS
    $scope.TicketsNuevos = function () {

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

    $scope.buscarDatosAlDia = function () {

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

        $scope.TicketsEnGestionAlDia();
        $scope.TicketsPorCategoriaAlDia();
        $scope.TicketsPorEstadosAlDia();
        $scope.TicketsPorClientesEstadosAlDia();

    };

    $scope.TicketsEnGestionAlDia = function () {
        if ($scope.ticketEnGestionChartAlDia) {
            $scope.ticketEnGestionChartAlDia.destroy();
        }

        console.log("entro a gestion al dia " + $scope.cliente);

        var infoTicketsEnGestionAlDia = $http.post("indicadores/dar-tickets-en-gestion-aldia/", {
            cliente: $scope.cliente
        });

        infoTicketsEnGestionAlDia.success(function (data) {
            $scope.loading.hide();


            console.log("entroooooooooooo");

            if (data.success) {
                $scope.infoTicketsEnGestionAlDia = data.infoTickets;
                $scope.totalCantidadesTicketsEnGestionAlDia = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                console.log("entroooooooooooo 1111111111111111");

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var semanas = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketEnGestionChartAlDia').getContext('2d');
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

                $scope.mostrarGraficosAlDia = true;
            } else {
                // Manejar el caso en que data.success es false
                alert("Error: " + data.msg);
            }
        });
    };


    $scope.TicketsPorCategoriaAlDia = function () {

        if ($scope.ticketPorCategoriaBarrasChartAlDia) {
            $scope.ticketPorCategoriaBarrasChartAlDia.destroy();
        }

        if ($scope.ticketPorCategoriaPastelChartAlDia) {
            $scope.ticketPorCategoriaPastelChartAlDia.destroy();
        }

        var infoTicketsPorCategoriasAlDia = $http.post("indicadores/dar-tickets-por-categorias-aldia/", {
            cliente: $scope.cliente
        });
        infoTicketsPorCategoriasAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                $scope.infoTicketsPorCategoriasAlDia = data.infoTickets;
                $scope.totalCantidadesTicketsPorCategoriaAlDia = data.totalCantidades;
                $scope.mostrarGraficosAlDia = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var categorias = [];
                var cantidades = [];
                var porcentaje = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketPorCategoriaBarrasChartAlDia').getContext('2d');
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

                var tendenciaChartCtx1 = document.getElementById('ticketPorCategoriaPastelChartAlDia').getContext('2d');
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


    $scope.TicketsPorEstadosAlDia = function () {

        if ($scope.ticketPorEstadoBarrasChartAlDia) {
            $scope.ticketPorEstadoBarrasChartAlDia.destroy();
        }

        if ($scope.ticketPorEstadoPastelChartAlDia) {
            $scope.ticketPorEstadoPastelChartAlDia.destroy();
        }

        var infoTicketsPorEstadosAlDia = $http.post("indicadores/dar-tickets-por-estados-aldia/", {
            cliente: $scope.cliente
        });

        infoTicketsPorEstadosAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                $scope.infoTicketsPorEstadosAlDia = data.infoTickets;
                $scope.totalCantidadesTicketsPorEstadosAlDia = data.totalCantidades;
                $scope.mostrarGraficosAlDia = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var estados = [];
                var cantidades = [];
                var porcentaje = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
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

                // Ahora puedes usar getRandomColor() sin problemas
                var coloresAleatorios = cantidades.map(getRandomColor);

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('ticketPorEstadoBarrasChartAlDia').getContext('2d');
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

                var tendenciaChartCtx1 = document.getElementById('ticketPorEstadoPastelChartAlDia').getContext('2d');
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


    $scope.TicketsPorClientesEstadosAlDia = function () {

        if ($scope.ticketPorClienteEstadoBarrasChart1AlDia) {
            $scope.ticketPorClienteEstadoBarrasChart1AlDia.destroy();
        }

        if ($scope.ticketPorClienteEstadoBarrasChart2AlDia) {
            $scope.ticketPorClienteEstadoBarrasChart2AlDia.destroy();
        }

        var infoTicketsPorClienteEstadoAlDia = $http.post("indicadores/dar-tickets-por-cliente-estado-aldia/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin,
            cliente: $scope.cliente
        });

        infoTicketsPorClienteEstadoAlDia.success(function (data) {
            $scope.loading.hide();

            if (data.success) {

                $scope.infoTicketsPorClienteEstadoAlDia = data.infoTickets;
                $scope.totalCantidadesTicketsPorClientesEstadoAlDia = data.totalCantidades;
                $scope.mostrarGraficosAlDia = true; // Para mostrar la tabla y el gráfico

                // Crear una estructura para los datos de la tabla
                $scope.clientesEstados = {};
                var cantidades = [];

                // Llenar los datos del objeto clientesEstados
                angular.forEach($scope.infoTicketsPorClienteEstadoAlDia, function (ticket) {
                    // Si el cliente no existe en el objeto, agrégalo
                    if (!$scope.clientesEstados[ticket.Cliente]) {
                        $scope.clientesEstados[ticket.Cliente] = {};
                    }

                    // Agregar o actualizar la cantidad para el estado actual
                    $scope.clientesEstados[ticket.Cliente][ticket.Estado] = ticket.Cantidad;
                    cantidades.push(ticket.Cantidad);
                });

                // Calcular el total general para cada cliente
                angular.forEach($scope.clientesEstados, function (val, key) {
                    var totalGeneral = 0;
                    angular.forEach(val, function (value, estado) {
                        totalGeneral += value;
                    });
                    $scope.clientesEstados[key].TotalGeneral = totalGeneral;
                });

                // Obtener la lista de estados disponibles
                $scope.estados = Object.keys($scope.clientesEstados[$scope.infoTicketsPorClienteEstadoAlDia[0].Cliente]);


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

                var clientes1 = {};
                var ticketsPorEstado = {};
                $scope.infoTicketsPorClienteEstadoAlDia.forEach(function (ticket) {
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
                var clientes = [...new Set($scope.infoTicketsPorClienteEstadoAlDia.map(ticket => ticket.Cliente))];
                console.log(clientes);

                // Divide tus clientes en dos partes
                var mitad = Math.ceil(clientes.length / 2); // Redondea hacia arriba para manejar números impares
                var clientes1 = clientes.slice(0, mitad);
                var clientes2 = clientes.slice(mitad);

                // Crea un conjunto de datos para cada estado y cada grupo de clientes
                var datasets1 = createDatasets(clientes1, ticketsPorEstado);
                var datasets2 = createDatasets(clientes2, ticketsPorEstado);

                // Crea los gráficos
                createChart('ticketPorClienteEstadoBarrasChart1AlDia', clientes1, datasets1);
                createChart('ticketPorClienteEstadoBarrasChart2AlDia', clientes2, datasets2);

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
