var indicadoresApp = angular.module('indicadores', []);

indicadoresApp.config(function ($provide, $httpProvider) {
    $provide.factory('MyHttpInterceptor', function ($q) {
        return {
            request: function (config) {
                return config || $q.when(config);
            },
            requestError: function (rejection) {
                return $q.reject(rejection);
            },
            response: function (response) {
                var x = false
                if (typeof response.data === "string" || (typeof response.data == "object" && response.data.constructor === String)) {
                    x = true;
                }
                if (x && response.data.indexOf('ng-app="login"') !== -1) {
                    window.location.assign("sifizplanning/redirect");
                }
                else
                    return response || $q.when(response);
            },
            responseError: function (rejection) {
                return $q.reject(rejection);
            }
        };
    });

    $httpProvider.interceptors.push('MyHttpInterceptor');
});

indicadoresApp.controller('indicadoresController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'INDICADORES TICKETS';
    $scope.rutaImages = "Web/images/";
    $scope.diasCalendar = [];
    $scope.colaboradores = [];

    //Para la seleccion en el menú
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel-indicadores-tickets").addClass('invisible');
        angular.element("#panel-indicadores-gestores").addClass('invisible');
    };

    $scope.IrIndicadoresTickets = function () {
        ocultar();
        angular.element("#panel-indicadores-tickets").removeClass('invisible');
        $scope.funcionalidad = 'INDICADORES TICKETS';
    };

    $scope.IrIndicadoresGestores = function () {
        ocultar();
        angular.element("#panel-indicadores-gestores").removeClass('invisible');
        $scope.funcionalidad = 'INDICADORES GESTORES';
    };
    $scope.loadingAjax = function () {
        this.show = function (target) {
            angular.element("#loadingDiv").show();
        };
        this.hide = function (target) {
            angular.element("#loadingDiv").hide();
        };
    };

    $scope.loading = new $scope.loadingAjax();

}]);

//Directivas y Filtros
indicadoresApp.directive('ngRightClick', function ($parse) {
    return function (scope, element, attrs) {
        var fn = $parse(attrs.ngRightClick);
        element.bind('contextmenu', function (event) {
            scope.$apply(function () {
                event.preventDefault();
                fn(scope, { $event: event });
            });
        });
    };
});

//Filter de angular para las fechas
indicadoresApp.filter("strDateToStr", function () {
    return function (textDate) {
        if (textDate !== undefined) {
            var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
            return dateToStr(fecha);
        }
        return "";
    }
});
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

indicadoresApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);






