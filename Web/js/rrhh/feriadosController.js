rrhhApp.controller('feriadosController', ['$scope', '$http', function ($scope, $http) {

    var currentYear = new Date().getFullYear();
    $scope.minYear = currentYear - 5;
    $scope.maxYear = currentYear + 5;
    $scope.selectedYear = currentYear;

    $scope.selectedDay = null;
    $scope.selectedMonth = null;

    $scope.months = [
        'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
    ];

    $scope.days = Array.from({ length: 5 }, (_, i) => Array.from({ length: 7 }, (_, j) => i * 7 + j + 1));

    $scope.selectDay = function (day) {
        $scope.selectedDay = day;
    }

    $scope.initController = function () {
        $scope.loadFeriados();
    };

    $scope.loadFeriados = function () {
        var ajaxFeriados = $http.post("rrhh/dar-feriado/", {});

        ajaxFeriados.success(function (data) {
            if (data.success) {
                var jsonFeriados = data.feriados;
                $scope.feriados = jsonFeriados.map(f => {
                    // Extrae el número de milisegundos de la cadena de fecha
                    var fechaMilisegundos = f.fecha.match(/\d+/)[0];

                    // Crea un nuevo objeto Date con el número de milisegundos
                    var fecha = new Date(parseInt(fechaMilisegundos));

                    // Obtiene el día, mes y año del objeto Date
                    var dia = fecha.getDate();
                    var mes = $scope.months[fecha.getMonth()]; // Obtiene el nombre del mes desde la lista 'months'
                    var ano = fecha.getFullYear();

                    // Retorna un nuevo objeto con las propiedades 'id', 'dia', 'mes' y 'ano'
                    return {
                        id: f.id,
                        dia: dia,
                        mes: mes,
                        ano: ano
                    };
                });
                
            } else {
                alert("No se pudieron cargar los datos");
            }
        });
    };


    $scope.initController();

    $scope.guardarFeriado = function () {
        // Encuentra la posición del mes seleccionado en la lista de meses
        if ($scope.selectedMonth != null && $scope.selectedDay != null) {
            var selectedMonthPosition = $scope.months.indexOf($scope.selectedMonth);

            // Asegúrate de que el mes seleccionado está en la lista de meses
            if (selectedMonthPosition === -1) {
                alert('El mes seleccionado no es válido.');
                return;
            }

            // Usa la posición del mes como el valor del mes en el objeto Date
            var date = new Date($scope.selectedYear, selectedMonthPosition, $scope.selectedDay);

            // Verificar si la fecha es válida
            if (isNaN(date.getTime())) {
                alert('La fecha seleccionada no es válida.');
                return;
            }

            var ajaxTest = $http.post("rrhh/guardar-feriado/", {
                fecha: date.toISOString()
            });

            ajaxTest.success(function (data) {

                $scope.selectedDay = null;
                $scope.selectedMonth = null;

                messageDialog.show('Información', data.msg);
                $scope.loadFeriados();
                angular.element("#modal-agregar-feriado").modal('hide');
            });
        } else {
            messageDialog.show('Información', "Seleccione mes y día");
        }
    };

    $scope.eliminarFeriado = function (feriadoId) {
        var ajaxEliminarFeriado = $http.post("rrhh/eliminar-feriado/", {id: feriadoId});

        ajaxEliminarFeriado.success(function (data) {
            messageDialog.show('Información', data.msg);
            $scope.loadFeriados();
        });
    };
    
    $scope.modalAgregarFeriado = function () {

        angular.element("#modal-agregar-feriado").modal('show');

    };
}]);