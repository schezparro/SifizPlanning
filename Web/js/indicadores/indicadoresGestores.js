indicadoresApp.controller('indicadoresGestores', ['$scope', '$http', '$filter', function ($scope, $http, $filter) {
    $http.post("catalogos/dar-gestores/", {}).success(function (data) {
        if (data.success) {
            $scope.gestores = data.gestores;
            console.log($scope.gestores);
            $scope.anioSeleccionado = new Date().getFullYear();
            $scope.gestorSeleccionado = 'BOLIVAR VALLEJO';
            $scope.darTicketsPorAnioGestor();
        } else {
            alert("No se pudieron cargar los datos");
        }
    });
    $scope.gestorSeleccionado = 'BOLIVAR VALLEJO';

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

    $scope.initDatepickers();
    $scope.darTicketsPorAnioGestor = function () {
        $http.post('indicadores/dar-tickets-por-anio-gestor/', {
            anio: $scope.anioSeleccionado,
            gestor: $scope.gestorSeleccionado != undefined ? $scope.gestorSeleccionado : 'BOLIVAR VALLEJO'
        }).success(function (data) {
            if (data.success) {
                $scope.datos = data.resumenTickets;
                $scope.totalTickets = data.totalTickets;
                generarGraficoTicketAnioGestor(data.resumenTickets);
            } else {
                alert('Error: ' + data.msg);
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
                alert('Error: ' + response.msg);
            }
        });
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
                alert('Error: ' + response.msg);
            }
        });
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
                alert('Error: ' + response.msg);
            }
        });
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
                alert('Error: ' + response.msg);
            }
        });
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

    $scope.generarGraficoTicketAnalizados = function (data) {
        if ($scope.ticketsAnalizadosChart) $scope.ticketsAnalizadosChart.destroy();
        if (!data || data.length === 0) return alert('No hay datos para mostrar en el gráfico');
        var ctx = document.getElementById('ticketsAnalizadosChart');
        if (!ctx) return alert('No se pudo encontrar el elemento del gráfico');
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
        if (!data || data.length === 0) return alert('No hay datos para mostrar en el gráfico');
        var ctx = document.getElementById('anuladoRechazadoChart');
        if (!ctx) return alert('No se pudo encontrar el elemento del gráfico');
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

    $scope.darTicketIntervaloGestores();
    $scope.darTicketTiempoGestores();
    $scope.darTicketAnalizadosGestores();
    $scope.darTicketAnuladosRechazados();

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
