indicadoresApp.controller('indicadoresTickets', ['$scope', '$http', function ($scope, $http) {

    var hoy = new Date();
    var primerDiaDelMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1);

    angular.element('#fecha-inicio-indicadores-tickets').datepicker({
        //startDate: primerDiaDelMes,
        format: 'dd/mm/yyyy',
        locale: 'es'
    }).val(primerDiaDelMes.toISOString().split('T')[0]); // Establecer el valor predeterminado

    // Configurar el datepicker de fin con la fecha actual
    angular.element('#fecha-fin-indicadores-tickets').datepicker({
        //startDate: hoy,
        format: 'dd/mm/yyyy',
        locale: 'es'
    }).val(hoy.toISOString().split('T')[0]); // Establecer el valor predeterminado

    //var fechaInicioEta = angular.element("#fecha-inicio-etapa")[0].value;
    //var fechaFinEta = angular.element("#fecha-fin-etapa")[0].value;

    $scope.mostrarGraficos = false;

    $scope.buscarDatos = function () {
        $scope.loading.show();

        // Destruye las instancias de gráficos existentes
        if ($scope.clientesChart) {
            $scope.clientesChart.destroy();
        }
        if ($scope.estadosPrioridadesChart) {
            $scope.estadosPrioridadesChart.destroy();
        }
        if ($scope.tiposChart) {
            $scope.tiposChart.destroy();
        }
        if ($scope.tendenciaChart) {
            $scope.tendenciaChart.destroy();
        }
        if ($scope.distribucionChart) {
            $scope.distribucionChart.destroy();
        }

        var infoTickets = $http.post("indicadores/dar-infotickets/", {
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });

        infoTickets.success(function (data) {
            $scope.loading.hide();

            if (data.success) {
                $scope.infoTickets = data.infoTickets;

                // Preparar datos para el gráfico de barras de clientes
                var clientesData = {};
                $scope.infoTickets.forEach(function (ticket) {
                    if (clientesData[ticket.Cliente]) {
                        clientesData[ticket.Cliente]++;
                    } else {
                        clientesData[ticket.Cliente] = 1;
                    }
                });
                var clientesLabels = Object.keys(clientesData);
                var clientesValues = Object.values(clientesData);

                console.log(clientesLabels);
                console.log(clientesValues);

                $scope.clientesLabels = clientesLabels;
                $scope.clientesValues = clientesValues;

                // Preparar datos para el gráfico de barras apiladas
                var estadosPrioridadesData = {};
                $scope.infoTickets.forEach(function (ticket) {
                    if (!estadosPrioridadesData[ticket.Estado]) {
                        estadosPrioridadesData[ticket.Estado] = {};
                    }
                    if (estadosPrioridadesData[ticket.Estado][ticket.Prioridad]) {
                        estadosPrioridadesData[ticket.Estado][ticket.Prioridad]++;
                    } else {
                        estadosPrioridadesData[ticket.Estado][ticket.Prioridad] = 1;
                    }
                });
                var estados = Object.keys(estadosPrioridadesData);
                var prioridades = Array.from(new Set($scope.infoTickets.map(ticket => ticket.Prioridad)));
                var estadosPrioridadesValues = prioridades.map(function (prioridad) {
                    return estados.map(function (estado) {
                        return estadosPrioridadesData[estado][prioridad] || 0;
                    });
                });

                // Preparar datos para el gráfico de donut de tipos de tickets
                var tiposData = {};
                $scope.infoTickets.forEach(function (ticket) {
                    if (tiposData[ticket.Tipo]) {
                        tiposData[ticket.Tipo]++;
                    } else {
                        tiposData[ticket.Tipo] = 1;
                    }
                });
                var tiposLabels = Object.keys(tiposData);
                var tiposValues = Object.values(tiposData);

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var ingresosPorDia = {};
                var cierresPorDia = {};
                $scope.infoTickets.forEach(function (ticket) {
                    var fechaIngreso = new Date(parseInt(ticket.FechaIngreso.substr(6)));
                    var fechaCierre = ticket.FechaCierre ? new Date(parseInt(ticket.FechaCierre.substr(6))) : null;

                    var ingresoDia = fechaIngreso.toDateString();
                    var cierreDia = fechaCierre ? fechaCierre.toDateString() : null;

                    ingresosPorDia[ingresoDia] = ingresosPorDia[ingresoDia] ? ingresosPorDia[ingresoDia] + 1 : 1;
                    if (cierreDia) {
                        cierresPorDia[cierreDia] = cierresPorDia[cierreDia] ? cierresPorDia[cierreDia] + 1 : 1;
                    }
                });

                var fechas = Object.keys(ingresosPorDia);
                var ingresos = fechas.map(function (fecha) {
                    return ingresosPorDia[fecha];
                });
                var cierres = fechas.map(function (fecha) {
                    return cierresPorDia[fecha] || 0;
                });

                // Gráfico de barras para contar tickets por cliente
                var clientesChartCtx = document.getElementById('clientesChart').getContext('2d');
                var clientesChart = new Chart(clientesChartCtx, {
                    type: 'bar',
                    data: {
                        labels: clientesLabels,
                        datasets: [{
                            label: 'Tickets por cliente',
                            data: clientesValues,
                            backgroundColor: 'rgba(54, 162, 235, 0.5)',
                            borderColor: 'rgba(54, 162, 235, 1)',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });

                // Gráfico de barras apiladas para contar tickets por estado y prioridad
                var estadosPrioridadesChartCtx = document.getElementById('estadosPrioridadesChart').getContext('2d');
                var estadosPrioridadesChart = new Chart(estadosPrioridadesChartCtx, {
                    type: 'bar',
                    data: {
                        labels: estados,
                        datasets: prioridades.map(function (prioridad, index) {
                            var color = getRandomColor(); // Función para obtener un color aleatorio
                            return {
                                label: prioridad,
                                data: estadosPrioridadesValues[index],
                                backgroundColor: color,
                                borderColor: color,
                                borderWidth: 1
                            };
                        })
                    },
                    options: {
                        scales: {
                            y: {
                                beginAtZero: true
                            },
                            x: {
                                stacked: true
                            },
                            y: {
                                stacked: true
                            }
                        }
                    }
                });

                // Función para obtener un color aleatorio

                // Gráfico de donut para mostrar la proporción de tipos de tickets
                var tiposChartCtx = document.getElementById('tiposChart').getContext('2d');
                var tiposChart = new Chart(tiposChartCtx, {
                    type: 'doughnut',
                    data: {
                        labels: tiposLabels,
                        datasets: [{
                            data: tiposValues,
                            backgroundColor: [
                                'rgba(255, 99, 132, 0.5)',
                                'rgba(54, 162, 235, 0.5)',
                                'rgba(255, 206, 86, 0.5)',
                                'rgba(75, 192, 192, 0.5)',
                                'rgba(153, 102, 255, 0.5)'
                                // Puedes agregar más colores si tienes más tipos de tickets
                            ],
                            borderColor: [
                                'rgba(255, 99, 132, 1)',
                                'rgba(54, 162, 235, 1)',
                                'rgba(255, 206, 86, 1)',
                                'rgba(75, 192, 192, 1)',
                                'rgba(153, 102, 255, 1)'
                                // Puedes agregar más colores si tienes más tipos de tickets
                            ],
                            borderWidth: 1
                        }]
                    }
                });

                // Gráfico de líneas para mostrar la tendencia de tickets a lo largo del tiempo
                var tendenciaChartCtx = document.getElementById('tendenciaChart').getContext('2d');
                var tendenciaChart = new Chart(tendenciaChartCtx, {
                    type: 'line',
                    data: {
                        labels: fechas,
                        datasets: [{
                            label: 'Tickets ingresados',
                            data: ingresos,
                            fill: false,
                            borderColor: 'rgba(255, 99, 132, 1)',
                            borderWidth: 1
                        }, {
                            label: 'Tickets cerrados',
                            data: cierres,
                            fill: false,
                            borderColor: 'rgba(54, 162, 235, 1)',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        scales: {
                            y: {
                                beginAtZero: true
                            }
                        }
                    }
                });

                // Suponiendo que obtienes los datos de algún lugar y los asignas a $scope.infoTickets

                // Función para obtener la distribución de tickets por cliente y estado
                function getDistribucionTicketsPorClienteEstado(tickets) {
                    var distribucion = {};
                    tickets.forEach(function (ticket) {
                        var cliente = ticket.Cliente;
                        var estado = ticket.Estado;
                        if (!distribucion[cliente]) {
                            distribucion[cliente] = {};
                        }
                        if (!distribucion[cliente][estado]) {
                            distribucion[cliente][estado] = 0;
                        }
                        distribucion[cliente][estado]++;
                    });
                    return distribucion;
                }

                // Obtener la distribución de tickets por cliente y estado
                var distribucionTicketsPorClienteEstado = getDistribucionTicketsPorClienteEstado($scope.infoTickets);

                // Función para obtener colores únicos para cada cliente
                function getUniqueColors(numColors) {
                    var colors = [];
                    var goldenRatio = 0.618033988749895;
                    var hue = Math.random(); // Start at a random hue
                    for (var i = 0; i < numColors; i++) {
                        hue += goldenRatio;
                        hue %= 1;
                        colors.push(hslToRgb(hue, 0.5, 0.6));
                    }
                    return colors;
                }

                // Función auxiliar para convertir HSL a RGB
                function hslToRgb(h, s, l) {
                    var r, g, b;

                    if (s == 0) {
                        r = g = b = l; // Achromatic
                    } else {
                        function hue2rgb(p, q, t) {
                            if (t < 0) t += 1;
                            if (t > 1) t -= 1;
                            if (t < 1 / 6) return p + (q - p) * 6 * t;
                            if (t < 1 / 2) return q;
                            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
                            return p;
                        }

                        var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                        var p = 2 * l - q;
                        r = hue2rgb(p, q, h + 1 / 3);
                        g = hue2rgb(p, q, h);
                        b = hue2rgb(p, q, h - 1 / 3);
                    }

                    return '#' + Math.round(r * 255).toString(16).padStart(2, '0') +
                        Math.round(g * 255).toString(16).padStart(2, '0') +
                        Math.round(b * 255).toString(16).padStart(2, '0');
                }

                // Obtener clientes y sus estados únicos
                var clientes = Object.keys(distribucionTicketsPorClienteEstado);
                var estados = Object.keys(distribucionTicketsPorClienteEstado[clientes[0]]);
                var numClientes = clientes.length;

                // Preparar colores únicos para cada cliente
                var coloresClientes = getUniqueColors(numClientes);

                // Obtener clientes y sus estados únicos
                var clientes = Object.keys(distribucionTicketsPorClienteEstado);
                var estados = Object.keys(distribucionTicketsPorClienteEstado[clientes[0]]);
                var numClientes = clientes.length;

                // Preparar colores únicos para cada cliente
                var coloresClientes = getUniqueColors(numClientes);

                // Preparar datos para el gráfico de barras apiladas
                var data = estados.map(function (estado) {
                    return {
                        label: estado,
                        data: clientes.map(function (cliente, index) {
                            return distribucionTicketsPorClienteEstado[cliente][estado] || 0;
                        }),
                        backgroundColor: coloresClientes
                    };
                });

                // Configurar y renderizar el gráfico de barras apiladas
                var distribucionChartCtx = document.getElementById('distribucionChart').getContext('2d');
                var distribucionChart = new Chart(distribucionChartCtx, {
                    type: 'bar',
                    data: {
                        labels: clientes, // Usar los nombres de los clientes como etiquetas en el eje X
                        datasets: data
                    },
                    options: {
                        scales: {
                            x: {
                                stacked: true,
                                title: {
                                    display: true,
                                    text: 'Clientes' // Título del eje X
                                }
                            },
                            y: {
                                stacked: true,
                                title: {
                                    display: true,
                                    text: 'Cantidad de Tickets' // Título del eje Y
                                }
                            }
                        }
                    }
                });

                $scope.mostrarGraficos = true;

            } else {
                alert("No se pudieron cargar los datos");
            }


        });
    };

    $scope.buscarOtrosDatos = function () {
        
        $scope.TicketsNuevos();
        $scope.TicketsCerrados();
        $scope.TicketsEnGestion();
        $scope.TicketsPorCategoria();
        $scope.TicketsPorEstados();
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
                $scope.infoTickets = data.infoTickets;
                $scope.totalCantidades = data.totalCantidades;
                $scope.mostrarGraficos = true; // Para mostrar la tabla y el gráfico

                // Preparar datos para el gráfico de líneas de tendencia temporal
                var semanas = [];
                var cantidades = [];

                // Llenar las etiquetas y los datos del gráfico con la información de las semanas y cantidades
                $scope.infoTickets.forEach(function (ticket) {
                    semanas.push('Semana ' + ticket.Semana);
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
                    semanas.push('Semana ' + ticket.Semana);
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
                    semanas.push('Semana ' + ticket.Semana);
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
                    type: 'pie', // Tipo de gráfico
                    data: {
                        labels: porcentaje, // Usamos las categorías como etiquetas
                        datasets: [{
                            label: 'Porcentaje por categorías',
                            data: porcentaje, // Usamos los porcentajes como datos
                            backgroundColor: coloresAleatorios, // Asignamos colores a cada segmento
                            hoverOffset: 4
                        }]
                    },
                    options: {
                        plugins: {
                            title: {
                                display: true,
                                text: 'Porcentaje por categorías', // El título de la leyenda
                                font: {
                                    size: 12
                                },
                                padding: {
                                    top: 10,
                                    bottom: 30
                                },
                                color: '#000' // Color del título
                            },
                            legend: {
                                display: true,
                                position: 'top', // Posición de la leyenda (top, bottom, left, right)
                                labels: {
                                    font: {
                                        size: 14,
                                        weight: 'bold'
                                    },
                                    color: '#808080' // Color de las etiquetas de la leyenda
                                }
                            },
                            datalabels: {
                                display: true, // Mostrar las etiquetas permanentemente
                                formatter: (value) => {
                                    return value + "%"; // Mostrar el valor directamente con el signo de porcentaje
                                },
                                color: '#fff',
                                font: {
                                    weight: 'bold',
                                    size: 16 // Tamaño de la fuente para las etiquetas de datos
                                },
                                anchor: 'end',
                                align: 'start',
                                offset: 10,
                                padding: {
                                    top: 10,
                                    bottom: 10
                                }
                            }
                        }
                    },
                });

                $scope.mostrarGraficos = true;
            } else {
                // Manejar el caso en que data.success es false
                alert("Error: " + data.msg);
            }
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

    function getRandomColor() {
        var letters = '0123456789ABCDEF';
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

}]);