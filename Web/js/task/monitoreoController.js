var taskApp = angular.module('task', []);
taskApp.controller('monitoreoController', ['$scope', '$http', function ($scope, $http) {
    var taskProxie = angular.element.connection.websocket;
    angular.element.connection.hub.start();
    
    $scope.trabajadores = [];
    var listaFullTrabajadores = [];
            
    $scope.fechaLunes = "";
    $scope.esteLunes = "";
    $scope.fechaHoy = "";

    setTimeout(function () {
        //Cargando los datos
        //Cargando el ultimo lunes
        var ajaxUltimoDomingo = $http.post("../task/ultimo-lunes", {});
        ajaxUltimoDomingo.success(function (data) {
            if (data.success === true) {
                $scope.fechaLunes = data.lunes;
                $scope.esteLunes = data.lunes;
                $scope.fechaHoy = data.hoy;

                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });

                $scope.actualizaListaEstadosTarea();
            }
            else {
                //alert("Error en la obtención de la fecha.")
            }
        });
    }, 2000);
        
    taskProxie.client.actTareas = function (trabs) {        
        $scope.actualizarListaDias($scope.fechaLunes, $scope.fechaHoy);
        listaFullTrabajadores = JSON.parse(trabs);                
    };

    angular.element.connection.hub.start().done(function () {
        taskProxie.server.actualizarTareas();
    });    
    
    //Funciones generales
    // Función que suma o resta días a la fecha indicada formato d/m/Y
    sumaFecha = function (d, fecha) {
        var Fecha = new Date();
        var sFecha = fecha || (Fecha.getDate() + "/" + (Fecha.getMonth() + 1) + "/" + Fecha.getFullYear());
        var sep = sFecha.indexOf('/') != -1 ? '/' : '-';
        var aFecha = sFecha.split(sep);
        var fecha = aFecha[2] + '/' + aFecha[1] + '/' + aFecha[0];
        fecha = new Date(fecha);
        fecha.setDate(fecha.getDate() + parseInt(d));
        var anno = fecha.getFullYear();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        mes = (mes < 10) ? ("0" + mes) : mes;
        dia = (dia < 10) ? ("0" + dia) : dia;
        var fechaFinal = dia + sep + mes + sep + anno;
        return (fechaFinal);
    };

    //Funcion que actualiza la fecha de los dias de la semana en el calendario de las tareas
    $scope.actualizarListaDias = function (fechaLunes, fechaHoy) {
        $scope.diasCalendar = [];

        var dias = ['Lun ', 'Mar ', 'Mie ', 'Jue ', 'Vie ', 'Sab ', 'Dom '];
        var clases = ['dia-tarea1', 'dia-tarea2', 'dia-tarea3', 'dia-tarea3'];
        var numeroSemanas = 2;
        var cant = numeroSemanas * 7;
        var i = 0;
        var clase = clases[numeroSemanas - 1];
        while (i < cant) {
            for (var j = 0; j < dias.length; j++) {
                var dia = dias[j];
                var fecha = sumaFecha(i, fechaLunes);
                i++;

                var obj = {
                    dia: dias[j] + fecha,
                    clase: clase,
                    date: fecha
                };
                if (fecha === fechaHoy) {
                    obj.clase = obj.clase + " dia-hoy";
                }
                else if (j === 5 || j === 6) {
                    obj.clase = obj.clase + " fin-semana";
                }
                $scope.diasCalendar.push(obj);
            }
        }
        var ancho = (1021 - (numeroSemanas * 7) + 1) / (numeroSemanas * 7);
        $scope.anchoCeldaTarea = ancho;
        angular.element(".td-tareas").css({ width: ancho });
    };

    //Funcion recursiva cada 2 segundos
    var i = 0;
    var arrayMonitoreo = [7];
    $scope.actualizaListaEstadosTarea = function () {
        var cantFilas = 6;
        var iaux = i;
        while (cantFilas >= 0) {
            if (i >= listaFullTrabajadores.length) {
                i = 0;
                iaux = i;
            }
            arrayMonitoreo[cantFilas] = (listaFullTrabajadores[i]);
            i++;
            cantFilas--;
        }
        
        $scope.$apply(function () {
            $scope.trabajadores = arrayMonitoreo;
        });       

        i = iaux+1;

        setTimeout(function () {
            $scope.actualizaListaEstadosTarea();
        }, 2000);       
    };

    $scope.mySplitEmail = function (string, nb) {
        if (string === undefined)
            string = "";
        $scope.array = string.split('@');
        return $scope.result = $scope.array[nb];
    }    
}]);