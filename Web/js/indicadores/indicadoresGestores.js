indicadoresApp.controller('indicadoresGestores', ['$scope', '$http', '$filter', function ($scope, $http, $filter) {
    $http.post("catalogos/dar-gestores/", {}).success(function (data) {
        if (data.success) {
            $scope.gestores = data.gestores;
            $scope.anioSeleccionado = new Date().getFullYear();
            $scope.gestorSeleccionado = 'TODOS';
            $scope.secuencialGestorIndicadores = "0";
            
            $scope.darTicketsPorAnioGestor();
        } else {
            console.log("No se pudieron cargar los datos");
        }
    });
    $scope.gestorSeleccionado = 'TODOS';

    $scope.anios = [2020, 2021, 2022, 2023, 2024]; // Ajusta según tus necesidades
    $scope.meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];

    $scope.initDatepickers = function () {
        angular.element('#fecha-inicio-gestores').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.finicioGestores);

        angular.element('#fecha-fin-gestores').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.ffinGestores);

        angular.element('#finit-analizados').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.finitAnalizados);

        angular.element('#ffin-analizados').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.ffinAnalizados);

        angular.element('#fecha-inicio-invertido').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaInicioInvertido);

        angular.element('#fecha-fin-invertido').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaFinInvertido);

        angular.element('#fecha-inicio-anulado').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaInicioAnulado);

        angular.element('#fecha-fin-anulado').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaFinAnulado);

        angular.element('#fecha-inicio-anulado-indicadores').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaInicioAnuladoIndicadores);

        angular.element('#fecha-fin-anulado-indicadores').datepicker({
            format: 'dd/mm/yyyy',
            locale: 'es'
        }).datepicker('setDate', $scope.fechaFinAnuladoIndicadores);
    };

    var fechaActual = new Date();
    $scope.finicioGestores = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.ffinGestores = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);
    $scope.finitAnalizados = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.ffinAnalizados = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);
    $scope.fechaInicioInvertido = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.fechaFinInvertido = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);
    $scope.fechaInicioAnulado = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.fechaFinAnulado = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);
    $scope.fechaInicioAnuladoIndicadores = new Date(fechaActual.getFullYear(), fechaActual.getMonth(), 1);
    $scope.fechaFinAnuladoIndicadores = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + 1, 0);

    $scope.initDatepickers();
    $scope.darTicketsPorAnioGestor = function () {
        $http.post('indicadores/dar-tickets-por-anio-gestor/', {
            anio: $scope.anioSeleccionado,
            gestor: $scope.gestorSeleccionado != undefined ? $scope.gestorSeleccionado : 'TODOS'
        }).success(function (data) {
            if (data.success) {
                $scope.datos = data.resumenTickets;
                $scope.totalTickets = data.totalTickets;
                $scope.totalesPorMes = data.totalesPorMes; // Añadir los totales por mes al 
                generarGraficoTicketAnioGestor(data.resumenTickets);
            } else {
                console.log('Error: ' + data.msg);
            }
        });
    };
    $scope.darTicketIntervaloGestores = function () {
        $scope.loading.show();
        var data = {
            fechaInicio: $filter('date')($scope.finicioGestores, 'dd/MM/yyyy'),
            fechaFin: $filter('date')($scope.ffinGestores, 'dd/MM/yyyy')
        };
        var request = $http.post("indicadores/dar-tickets-intervalo-gestores/", data);
        request.success(function (response) {
            $scope.loading.hide();
            if (response.success) {
                $scope.ticketIntervaloGestores = response.ticketIntervaloGestores;
                $scope.generarGraficoTicketPorIntervalo($scope.ticketIntervaloGestores);
            } else {
                console.log('Error: ' + response.msg);
            }
        });
    };

    $scope.getTotal = function (column) {
        return $scope.ticketIntervaloGestores.reduce(function (total, ticket) {
            return total + ticket[column];
        }, 0);
    };


    $scope.darTicketTiempoGestores = function () {
        $scope.loading.show();
        var data = {
            fechaInicio: $filter('date')($scope.fechaInicioInvertido, 'dd/MM/yyyy'),
            fechaFin: $filter('date')($scope.fechaFinInvertido, 'dd/MM/yyyy')
        };
        var request = $http.post("indicadores/dar-tickets-tiempo-gestores/", data);
        request.success(function (response) {
            $scope.loading.hide();
            if (response.success) {
                $scope.ticketsTiempoInvertido = response.tiempoIntervaloGestores;
                $scope.darTicketTiempoInvertido($scope.ticketsTiempoInvertido);
            } else {
                console.log('Error: ' + response.msg);
            }
        });
    };

    // Función para obtener el total por columna
    $scope.getTotalPorColumna = function (label) {
        return Object.values($scope.ticketsTiempoInvertidoAgrupado).reduce(function (total, tiempos) {
            return total + (tiempos[label] || 0);
        }, 0);
    };

    // Función para obtener el total general
    $scope.getTotalGeneral = function () {
        return $scope.ticketsTiempoInvertidoLabels.reduce(function (total, label) {
            return total + parseFloat($scope.getTotalPorColumna(label));
        }, 0).toFixed(2);
    };


    $scope.darTicketAnalizadosGestores = function () {
        $scope.loading.show();
        var data = {
            fechaInicio: $filter('date')($scope.finitAnalizados, 'dd/MM/yyyy'),
            fechaFin: $filter('date')($scope.ffinAnalizados, 'dd/MM/yyyy')
        };
        var request = $http.post("indicadores/dar-tickets-analizados-gestores/", data);
        request.success(function (response) {
            $scope.loading.hide();
            if (response.success) {
                $scope.ticketsAnalizados = response.ticketsAnalizados;
                $scope.generarGraficoTicketAnalizados($scope.ticketsAnalizados);
            } else {
                console.log('Error: ' + response.msg);
            }
        });
    };

    $scope.getTotalTicketsAnalizados = function () {
        return $scope.ticketsAnalizados.reduce(function (total, ticket) {
            return total + ticket.NumeroTickets;
        }, 0);
    };


    $scope.darTicketAnuladosRechazados = function () {
        $scope.loading.show();
        var data = {
            fechaInicio: $filter('date')($scope.fechaInicioAnulado, 'dd/MM/yyyy'),
            fechaFin: $filter('date')($scope.fechaFinAnulado, 'dd/MM/yyyy')
        };
        var request = $http.post("indicadores/dar-tickets-anulados-rechazados/", data);
        request.success(function (response) {
            $scope.loading.hide();
            if (response.success) {
                $scope.anuladosRechazados = response.anuladosRechazados;
                $scope.generarGraficoTicketAnulados($scope.anuladosRechazados);
            } else {
                console.log('Error: ' + response.msg);
            }
        });
    };

    $scope.darTicketAnuladosRechazadosIndicadores = function () {
        $scope.loading.show();
        var data = {
            fechaInicio: $filter('date')($scope.fechaInicioAnuladoIndicadores, 'dd/MM/yyyy'),
            fechaFin: $filter('date')($scope.fechaFinAnuladoIndicadores, 'dd/MM/yyyy'),
            secuencialGestor: $scope.secuencialGestorIndicadores
        };

        var request = $http.post("indicadores/dar-tickets-anulados-rechazados-indicadores/", data);
        request.success(function (response) {
            $scope.loading.hide();
            if (response.success) {
                $scope.anuladosRechazadosIndicadores = response.anuladosRechazados;

                $scope.generateTableARI();
            } else {
                console.log('Error: ' + response.msg);
            }
        });
    };

    $scope.generateTableARI = function () {
        $scope.mesesFormateados = [];
        $scope.gestoresTicket = [];

        function getNombreMes(numeroMes) {
            var meses = ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'];
            return meses[numeroMes - 1];
        }

        $scope.anuladosRechazadosIndicadores.sort(function (a, b) {
            if (a.Anio !== b.Anio) return a.Anio - b.Anio;
            return a.Mes - b.Mes;
        });

        $scope.anuladosRechazadosIndicadores.forEach(function (ticket) {
            var mesFormateado = getNombreMes(ticket.Mes) + '/' + ticket.Anio;
            if ($scope.mesesFormateados.indexOf(mesFormateado) === -1) {
                $scope.mesesFormateados.push(mesFormateado);
            }
        });

        $scope.tableData = {};
        $scope.gestores.forEach(function (gestor) {
            $scope.tableData[gestor.nombre] = {
                totalIngresados: 0,
                totalAnuladosRechazados: 0
            };
            $scope.mesesFormateados.forEach(function (mesFormateado) {
                $scope.tableData[gestor.nombre][mesFormateado] = { ingresado: 0, anuladoRechazado: 0, porcentage: 0 };
            });
        });

        $scope.tableData["Desconocido"] = {
            totalIngresados: 0,
            totalAnuladosRechazados: 0
        };
        $scope.mesesFormateados.forEach(function (mesFormateado) {
            $scope.tableData["Desconocido"][mesFormateado] = { ingresado: 0, anuladoRechazado: 0, porcentage: 0 };
        });

        $scope.anuladosRechazadosIndicadores.forEach(function (ticket) {
            var mesFormateado = getNombreMes(ticket.Mes) + '/' + ticket.Anio;
            $scope.tableData[ticket.Gestor][mesFormateado].ingresado = ticket.Ingresado;
            $scope.tableData[ticket.Gestor][mesFormateado].anuladoRechazado = ticket.AnuladoRechazado;
            $scope.tableData[ticket.Gestor][mesFormateado].porcentage = ticket.PorcentajeAnuladoRechazado;
            $scope.tableData[ticket.Gestor].totalIngresados += ticket.Ingresado;
            $scope.tableData[ticket.Gestor].totalAnuladosRechazados += ticket.AnuladoRechazado;
        });

        $scope.totalPorcentagePorMes = {};
        $scope.mesesFormateados.forEach(function (mesFormateado) {
            var totalIngresadosMes = 0;
            var totalAnuladosRechazadosMes = 0;
            $scope.gestores.forEach(function (gestor) {
                totalIngresadosMes += $scope.tableData[gestor.nombre][mesFormateado].ingresado;
                totalAnuladosRechazadosMes += $scope.tableData[gestor.nombre][mesFormateado].anuladoRechazado;
            });
            $scope.totalPorcentagePorMes[mesFormateado] = totalIngresadosMes > 0 ? ((totalAnuladosRechazadosMes / totalIngresadosMes) * 100).toFixed(2) : 0;
        });

        var totalIngresadosGeneral = 0;
        var totalAnuladosRechazadosGeneral = 0;
        $scope.gestores.forEach(function (gestor) {
            totalIngresadosGeneral += $scope.tableData[gestor.nombre].totalIngresados;
            totalAnuladosRechazadosGeneral += $scope.tableData[gestor.nombre].totalAnuladosRechazados;
        });
        $scope.totalPorcentageGeneral = totalIngresadosGeneral > 0 ? ((totalAnuladosRechazadosGeneral / totalIngresadosGeneral) * 100).toFixed(2) : 0;
        $scope.totalPorcentageValidado = (100 - $scope.totalPorcentageGeneral).toFixed(2);
    };


    $scope.getTotalIngresados = function () {
        return $scope.anuladosRechazados.reduce(function (total, ticket) {
            return total + ticket.Ingresado;
        }, 0);
    };

    $scope.getTotalAnuladosRechazados = function () {
        return $scope.anuladosRechazados.reduce(function (total, ticket) {
            return total + ticket.AnuladoRechazado;
        }, 0);
    };


    function generarGraficoTicketAnioGestor(data) {
        if ($scope.ticketsAnioGestorChart) $scope.ticketsAnioGestorChart.destroy();
        var labels = data.map(item => item.Cliente);
        var datasets = $scope.meses.map((mes, index) => {
            var filteredData = data.filter(item => item.TicketsPorMes[index] > 0 && item.Total > 0);
            return {
                label: mes,
                data: filteredData.map(item => item.TicketsPorMes[index]),
                backgroundColor: '#' + Math.floor(Math.random() * 16777215).toString(16),
                hidden: index >= 3,
            };
        });
        $scope.ticketsAnioGestorChart = new Chart(document.getElementById('ticketsAnioGestorChart').getContext('2d'), {
            type: 'bar',
            data: { labels: labels, datasets: datasets },
            options: {
                responsive: true,
                title: { display: true, text: 'Tickets por Cliente y Mes' },
                scales: {
                    x: { display: true, title: { display: true, text: 'Cliente' }, ticks: { autoSkip: true } },
                    y: { display: true, title: { display: true, text: 'Cantidad de Tickets' }, ticks: { stepSize: 1, beginAtZero: true }, suggestedMax: 3 },
                },
                elements: { bar: { barThickness: 150 } }
            }
        });
    }
    $scope.darTicketTiempoInvertido = function (data) {
        if ($scope.ticketsInvertidoChart) $scope.ticketsInvertidoChart.destroy();
        if (!data || data.length === 0) return console.log('No hay datos para mostrar en el gráfico');

        $scope.ticketsTiempoInvertido = data; // Asignar los datos a la variable para la tabla

        var ctx = document.getElementById('ticketsInvertidoChart');
        if (!ctx) return console.log('No se pudo encontrar el elemento del gráfico');

        var labels = [];
        var groupedData = {};

        data.forEach(item => {
            var gestor = item.Gestor;
            var label = `${item.Mes}/${item.Anio}`;

            if (!groupedData[gestor]) {
                groupedData[gestor] = {};
            }
            if (!groupedData[gestor][label]) {
                groupedData[gestor][label] = 0;
            }

            groupedData[gestor][label] += item.TiempoTotal;

            if (!labels.includes(label)) {
                labels.push(label);
            }
        });

        $scope.ticketsTiempoInvertidoLabels = labels;
        $scope.ticketsTiempoInvertidoAgrupado = groupedData;

        // Crear datasets para el gráfico
        var datasets = [];
        Object.keys(groupedData).forEach(gestor => {
            var color = `rgba(${Math.floor(Math.random() * 256)}, ${Math.floor(Math.random() * 256)}, ${Math.floor(Math.random() * 256)}, 0.6)`;
            datasets.push({
                label: gestor,
                data: labels.map(label => groupedData[gestor][label] || 0),
                backgroundColor: color,
                borderColor: color.replace('0.6', '1'),
                borderWidth: 1
            });
        });

        $scope.ticketsInvertidoChart = new Chart(ctx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                title: {
                    display: true,
                    text: 'Tiempo Invertido por Mes y Gestor'
                },
                scales: {
                    x: {
                        display: true,
                        title: {
                            display: true,
                            text: 'Mes y Año'
                        },
                        ticks: {
                            autoSkip: true
                        }
                    },
                    y: {
                        display: true,
                        title: {
                            display: true,
                            text: 'Tiempo Invertido (Horas)'
                        },
                        ticks: {
                            beginAtZero: true
                        }
                    }
                },
                legend: {
                    display: true
                },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem) {
                            return `${tooltipItem.dataset.label}: ${tooltipItem.raw} horas`;
                        }
                    }
                }
            }
        });
    };

    // Función para sumar tiempos en la tabla
    $scope.sumarTiempos = function (tiempos) {
        return Object.values(tiempos).reduce((sum, tiempo) => sum + tiempo, 0).toFixed(1);
    };

    $scope.generarGraficoTicketAnalizados = function (data) {
        if ($scope.ticketsAnalizadosChart) $scope.ticketsAnalizadosChart.destroy();
        if (!data || data.length === 0) return console.log('No hay datos para mostrar en el gráfico');
        var ctx = document.getElementById('ticketsAnalizadosChart');
        if (!ctx) return console.log('No se pudo encontrar el elemento del gráfico');
        $scope.ticketsAnalizadosChart = new Chart(ctx.getContext('2d'), {
            type: 'line',
            data: {
                labels: data.map(item => item && item.Gestor && item.Anio && item.Mes ? item.Gestor + ' ' + item.Anio + '/' + item.Mes : 'Desconocido'),
                datasets: [{
                    label: 'Número de Tickets Analizados',
                    data: data.map(item => item && item.NumeroTickets ? item.NumeroTickets : 0),
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                title: { display: true, text: 'Tickets Analizados por Mes y Gestor' },
                scales: {
                    x: {
                        display: true, title: { display: true, text: 'Gestor y Mes' }, ticks: { autoSkip: true }
                    },
                    y: { display: true, title: { display: true, text: 'Cantidad' }, ticks: { beginAtZero: true } }
                }
            }
        });
    };

    $scope.generarGraficoTicketAnulados = function (data) {
        if ($scope.anuladoRechazadoChart) $scope.anuladoRechazadoChart.destroy();
        if (!data || data.length === 0) return console.log('No hay datos para mostrar en el gráfico');
        var ctx = document.getElementById('anuladoRechazadoChart');
        if (!ctx) return console.log('No se pudo encontrar el elemento del gráfico');
        $scope.anuladoRechazadoChart = new Chart(ctx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: data.map(item => item && item.Gestor && item.Anio && item.Mes ? item.Gestor + ' ' + item.Anio + '/' + item.Mes : 'Desconocido'),
                datasets: [{
                    label: 'Tickets Ingresados',
                    data: data.map(item => item && item.Ingresado ? item.Ingresado : 0),
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                },
                {
                    label: 'Tickets Anulados/Rechazados',
                    data: data.map(item => item && item.AnuladoRechazado ? item.AnuladoRechazado : 0),
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                title: { display: true, text: 'Tickets Anulados/Rechazados por Mes y Gestor' },
                scales: {
                    x: {
                        display: true, title: { display: true, text: 'Gestor y Mes' }, ticks: { autoSkip: true }
                    },
                    y: { display: true, title: { display: true, text: 'Cantidad' }, ticks: { beginAtZero: true } }
                }
            }
        });
    };

    $scope.generarGraficoTicketPorIntervalo = function (data) {
        if ($scope.ticketsIntervaloChart) $scope.ticketsIntervaloChart.destroy();
        if (!data || data.length === 0) return console.log('No hay datos para mostrar en el gráfico');
        var labels = data.map(item => item.Gestor);
        var clientesData = data.map(item => item.ClientesAtendidos);
        var carteraData = data.map(item => item.CarteraAsignada);
        $scope.ticketsIntervaloChart = new Chart(document.getElementById('ticketsIntervaloChart').getContext('2d'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Clientes Atendidos',
                    data: clientesData,
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    borderColor: 'rgba(75, 192, 192, 1)',
                    borderWidth: 1
                }, {
                    label: 'Cartera Asignada',
                    data: carteraData,
                    backgroundColor: 'rgba(153, 102, 255, 0.2)',
                    borderColor: 'rgba(153, 102, 255, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                title: { display: true, text: 'Clientes Atendidos y Cartera Asignada por Gestor' },
                scales: {
                    x: { display: true, title: { display: true, text: 'Gestor' }, ticks: { autoSkip: true } },
                    y: { display: true, title: { display: true, text: 'Cantidad' }, ticks: { beginAtZero: true } }
                }
            }
        });
    };

    //$scope.darTicketIntervaloGestores();
    //$scope.darTicketTiempoGestores();
    //$scope.darTicketAnalizadosGestores();
    //$scope.darTicketAnuladosRechazados();

}]);

indicadoresApp.directive('formatDate', function ($filter) {
    return {
        require: 'ngModel',
        link: function (scope, elem, attr, ngModelCtrl) {
            ngModelCtrl.$formatters.push(function (modelValue) {
                if (modelValue) {
                    return $filter('date')(modelValue, 'dd/MM/yyyy');
                }
            });
        }
    };
});
