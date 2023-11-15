var tfs = angular.module('tfs', []);

tfs.config(function ($provide, $httpProvider) {
    // Intercept http calls.
    $provide.factory('MyHttpInterceptor', function ($q) {
        return {
            // On request success
            request: function (config) {
                return config || $q.when(config);
            },
            // On request failure
            requestError: function (rejection) {
                return $q.reject(rejection);
            },
            // On response success
            response: function (response) {
                var x = false
                if (typeof response.data === "string" || (typeof response.data === "object" && response.data.constructor === String)) {
                    x = true;
                }
                if (x && response.data.indexOf('ng-app="login"') !== -1) {
                    window.location.assign("sifizplanning/redirect");
                }
                else
                    return response || $q.when(response);
            },
            // On response failture
            responseError: function (rejection) {
                return $q.reject(rejection);
            }
        };
    });
    // Add the interceptor to the $httpProvider.
    $httpProvider.interceptors.push('MyHttpInterceptor');
});

tfs.controller('tfsController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'CONSULTA DE CAMBIOS';
    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel-consulta-tfs").addClass('invisible');
        angular.element("#panel-consulta-proyectos").addClass('invisible');
    }
    $scope.IrConsultaCambios = function () {
        ocultar();
        angular.element("#panel-consulta-tfs").removeClass('invisible');
        $scope.funcionalidad = 'CONSULTA DE CAMBIOS';

        cargarDatosConsultaCambios();
    };
    $scope.IrConsultaProyectos = function () {
        ocultar();
        angular.element("#panel-consulta-proyectos").removeClass('invisible');
        $scope.funcionalidad = 'CONSULTA DE PROYECTOS';
    };

    cargarDatosConsultaCambios = function () {
        $scope.proyectos = [];
        var ajaxProyectos = $http.post("tfs/proyectos-tfs", {});
        ajaxProyectos.success(function (data) {
            if (data.success === true) {
                $scope.proyectos = data.proyectos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        $scope.usuarios = [];
        var ajaxUsuarios = $http.post("tfs/usuarios-tfs", {});
        ajaxUsuarios.success(function (data) {
            if (data.success === true) {
                $scope.usuarios = data.usuarios;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        var date = new Date();

        var mes = (date.getMonth() + 1) < 10 ? "0" + (date.getMonth() + 1) : (date.getMonth() + 1);
        $scope.fechaInicio = "01/" + mes + "/" + date.getFullYear();
        $scope.fechaFin = date.getDate() + "/" + mes + "/" + date.getFullYear();
        $scope.fechaCambio = "";

        angular.element('#fechaInicio.datepicker-filtro').datepicker({
            format: 'dd/mm/yyyy'
        }).on('changeDate', function (e) {
            var sFecha = angular.element('#fechaInicio.datepicker-filtro').val();
            $scope.$apply(function () {
                $scope.fechaInicio = sFecha;
            });
        });

        angular.element('#fechaFin.datepicker-filtro').datepicker({
            format: 'dd/mm/yyyy'
        }).on('changeDate', function (e) {
            var sFecha = angular.element('#fechaFin.datepicker-filtro').val();
            $scope.$apply(function () {
                $scope.fechaFin = sFecha;
            });
        });
    };

    $scope.IrConsultaCambios();

}]);

//Filter para el relleno a la izquierda
tfs.filter("rellenarIzq", function () {
    return function (number, character, lenght) {
        if (lenght === undefined) {
            lenght = 6;
        }
        if (character === undefined) {
            character = ' ';
        }
        var cadena = '';
        while (lenght > 0) {
            cadena = cadena + character;
            lenght--;
        }
        var numDigitos = Math.max(Math.floor(Math.log10(Math.abs(number))), 0) + 1;

        return (cadena + number).slice(numDigitos);
    };
});