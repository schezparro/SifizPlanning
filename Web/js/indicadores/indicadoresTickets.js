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
                function getRandomColor() {
                    var letters = '0123456789ABCDEF';
                    var color = '#';
                    for (var i = 0; i < 6; i++) {
                        color += letters[Math.floor(Math.random() * 16)];
                    }
                    return color;
                }

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

    function generarTablaHTML(datos, cabeceras) {
        let tablaHTML = '<table>';
        tablaHTML += '<thead><tr>';
        cabeceras.forEach(cabecera => {
            tablaHTML += `<th>${cabecera}</th>`;
        });
        tablaHTML += '</tr></thead>';
        tablaHTML += '<tbody>';

        datos.forEach(data => {
            tablaHTML += '<tr>';
            Object.values(data).forEach(valor => {
                tablaHTML += `<td>${valor}</td>`;
            });
            tablaHTML += '</tr>';
        });

        tablaHTML += '</tbody></table>';
        return tablaHTML;
    }
}]);