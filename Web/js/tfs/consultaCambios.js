tfs.controller('consultasTFS', ['$scope', '$http', function ($scope, $http) {  

    $scope.cambios = [];
    $scope.pagina = 1;
    $scope.cantPaginas = 0;
    $scope.totalCambios = 0;
    $scope.filaInicio = 1;
    $scope.filaFinal = 10;
    $scope.cantidadMostrarPorPagina = 10;
    $scope.paginas = [];

    $scope.actualizarCantidadMostrar = function () {
        if ($scope.pagina === 0) {
            $scope.filaInicio = 1;
            $scope.filaFinal = $scope.cantidadMostrarPorPagina;
        }
        else {
            $scope.filaInicio = $scope.cantidadMostrarPorPagina * ($scope.pagina - 1);
            $scope.filaFinal = $scope.pagina * $scope.cantidadMostrarPorPagina;
        }
        devuelveCambios();
    };

    function devuelveCambios() {
        waitingDialog.show('Buscando...', { dialogSize: 'sm', progressType: 'success' });
        var ajaxCambios = $http.post("tfs/cambios-tfs", {
            proyecto: $scope.proyecto,
            usuario: $scope.usuario,
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin,
            filaInicio: $scope.filaInicio,
            filaFinal: $scope.cantidadMostrarPorPagina,
            mostrarComentariosConFormatoIncorrecto: $scope.mostrarComentariosErrores
        });
        ajaxCambios.success(function (data) {
            if (data.success === true) {
                $scope.cambios = data.cambios;
                devuelveCantidadTotalCambios();
                waitingDialog.hide();
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    }
    //devuelveCambios();

    function devuelveCantidadTotalCambios() {
        var ajaxCantidadCambios = $http.post("tfs/cantidad-cambios-tfs", {
            proyecto: $scope.proyecto,
            usuario: $scope.usuario,
            fechaInicio: $scope.fechaInicio,
            fechaFin: $scope.fechaFin
        });
        ajaxCantidadCambios.success(function (data) {
            if (data.success === true) {
                $scope.totalCambios = data.cantidadTotalCambios;
                if ($scope.totalCambios === 0) {
                    $scope.pagina = 0;
                }
                $scope.cantPaginas = Math.round(Math.ceil($scope.totalCambios / $scope.cantidadMostrarPorPagina));
                $scope.paginas = [];
                for (var i = 1; i <= $scope.cantPaginas; i++)
                    $scope.paginas.push(i);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }

    //devuelveCambios();

    function validaFechaDDMMAAAA(fecha) {
        var dtCh = "/";
        var minYear = 1900;
        var maxYear = 2100;
        function isInteger(s) {
            var i;
            for (i = 0; i < s.length; i++) {
                var c = s.charAt(i);
                if (((c < "0") || (c > "9"))) return false;
            }
            return true;
        }
        function stripCharsInBag(s, bag) {
            var i;
            var returnString = "";
            for (i = 0; i < s.length; i++) {
                var c = s.charAt(i);
                if (bag.indexOf(c) == -1) returnString += c;
            }
            return returnString;
        }
        function daysInFebruary(year) {
            return (((year % 4 == 0) && ((!(year % 100 == 0)) || (year % 400 == 0))) ? 29 : 28);
        }
        function DaysArray(n) {
            for (var i = 1; i <= n; i++) {
                this[i] = 31
                if (i == 4 || i == 6 || i == 9 || i == 11) { this[i] = 30 }
                if (i == 2) { this[i] = 29 }
            }
            return this
        }
        function isDate(dtStr) {
            var daysInMonth = DaysArray(12)
            var pos1 = dtStr.indexOf(dtCh)
            var pos2 = dtStr.indexOf(dtCh, pos1 + 1)
            var strDay = dtStr.substring(0, pos1)
            var strMonth = dtStr.substring(pos1 + 1, pos2)
            var strYear = dtStr.substring(pos2 + 1)
            strYr = strYear
            if (strDay.charAt(0) == "0" && strDay.length > 1) strDay = strDay.substring(1)
            if (strMonth.charAt(0) == "0" && strMonth.length > 1) strMonth = strMonth.substring(1)
            for (var i = 1; i <= 3; i++) {
                if (strYr.charAt(0) == "0" && strYr.length > 1) strYr = strYr.substring(1)
            }
            month = parseInt(strMonth)
            day = parseInt(strDay)
            year = parseInt(strYr)
            if (pos1 == -1 || pos2 == -1) {
                return false
            }
            if (strMonth.length < 1 || month < 1 || month > 12) {
                return false
            }
            if (strDay.length < 1 || day < 1 || day > 31 || (month == 2 && day > daysInFebruary(year)) || day > daysInMonth[month]) {
                return false
            }
            if (strYear.length != 4 || year == 0 || year < minYear || year > maxYear) {
                return false
            }
            if (dtStr.indexOf(dtCh, pos2 + 1) != -1 || isInteger(stripCharsInBag(dtStr, dtCh)) == false) {
                return false
            }
            return true
        }
        if (isDate(fecha)) {
            return true;
        } else {
            return false;
        }
    }

    $scope.filtrarCambios = function () {
        if ($scope.fechaInicio === "" || $scope.fechaInicio === undefined) {
            messageDialog.show('Información', 'Seleccione la fecha inicio.');
            return false;
        }

        if ($scope.fechaFin === "" || $scope.fechaFin === undefined) {
            messageDialog.show('Información', 'Seleccione la fecha de fin.');
            return false;
        }

        var strFechaI = angular.element('#fechaInicio.datepicker-filtro').val();
        if (!validaFechaDDMMAAAA(strFechaI)) {
            messageDialog.show('Información', 'El formato de la fecha de inicio esta incorrecto');
            return false;
        }
        var _fechaInicio = new Date(strFechaI);
        var strFechaF = angular.element('#fechaFin.datepicker-filtro').val();
        if (!validaFechaDDMMAAAA(strFechaF)) {
            messageDialog.show('Información', 'El formato de la fecha de fin esta incorrecto');
            return false;
        }
        var _fechaFin = new Date(strFechaF);
        if (_fechaInicio > _fechaFin) {
            messageDialog.show('Información', 'La fecha de inicio debe ser que la fecha de fin');
            return false;
        }
        $scope.pagina = 1;
        $scope.filaInicio = 1;
        $scope.filaFinal = $scope.cantidadMostrarPorPagina;
        devuelveCambios();
    };

    $scope.conjuntoCambios = [];
    $scope.proyectoCambio = "";
    $scope.usuarioCambio = "";


    $scope.selectConjuntoCambios = function (proyecto, financial, usuario, fecha, conjuntoCambiosID) {
        waitingDialog.show('Buscando...', { dialogSize: 'sm', progressType: 'success' });
        $scope.proyectoCambio = proyecto;
        $scope.usuarioCambio = usuario;
        $scope.fechaCambio = fecha;
        var ajaxConjuntoCambios = $http.post("tfs/conjunto-cambios-tfs", {
            proyecto: proyecto,
            financial: financial,
            usuario: usuario,
            fecha: fecha,
            conjuntoCambiosID: conjuntoCambiosID
        });
        ajaxConjuntoCambios.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.conjuntoCambios = data.cambios;
                angular.element("#modal-edit-competencias-software").modal('show');
            }
            else {
                messageDialog.show(data.msg);
            }
        });
    };

    $scope.paginar = function () {
        $scope.filaInicio = $scope.cantidadMostrarPorPagina * ($scope.pagina - 1);
        $scope.filaFinal = $scope.pagina * $scope.cantidadMostrarPorPagina;
        devuelveCambios();
    };

    $scope.cambiarPagina = function (pag) {
        $scope.pagina = pag;
        $scope.paginar();
    };

    $scope.atrazarPagina = function () {
        if ($scope.pagina > 1) {
            $scope.pagina--;
            $scope.paginar();
        }
    };

    $scope.avanzarPagina = function () {
        if ($scope.pagina < $scope.cantPaginas) {
            $scope.pagina++;
            $scope.paginar();
        }
    };

    $scope.mostrarComentariosErrores = false;

    $scope.filtrarComentariosErrores = function () {
        devuelveCambios();
        $scope.cantidadTotalCambios = $scope.cambios.lenght;
    };

}]);