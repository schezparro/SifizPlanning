var clientApp = angular.module('client', []);

clientApp.config(function ($provide, $httpProvider) {

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
                if (typeof response.data === "string" || (typeof response.data == "object" && response.data.constructor === String)) {
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

clientApp.controller('clientController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'SEGUIMIENTOS';

    angular.element('#menu-principal li[role="presentation"]').on("click", function () {
        angular.element('#menu-principal li[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#funcionalidad > div").addClass('invisible');
    };

    $scope.SeguirTickets = function () {
        ocultar();
        angular.element("#panel_seguimiento").removeClass('invisible');
        $scope.funcionalidad = 'SEGUIMIENTOS';
        $scope.cargarSeguimientoTickets();
    };

    $scope.SolicitarTicket = function () {
        ocultar();
        angular.element("#panel_newTicket").removeClass('invisible');
        $scope.funcionalidad = 'NUEVA SOLICITUD';
    };

    $scope.VerReporteTicket = function () {
        angular.element("#ver-reporte").attr({ "href": "/Report/VerReporteCliente?cliente=" + $scope.idCliente });
        angular.element("#ver-reporte")[0].click();
    };

    //Cambiar la contraseña del usuario
    $scope.cambiarPassUsuario = function () {
        waitingDialog.show('Cambiando la contraseña del usuario...', { dialogSize: 'sm', progressType: 'success' });
        var anular = $http.post("cambiar/user-password",
            {
                p: $scope.passw,
                p1: $scope.passw1,
                p2: $scope.passw2
            });
        anular.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modalNewChangePassUser").modal('hide');
                messageDialog.show('Información', data.msg);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.cargarSeguimientoTickets = function () {
        var ajaxColaboradores = $http.post("catalogos/coordinadores", {});
        ajaxColaboradores.success(function (data) {
            if (data.success === true) {
                $scope.colaboradores = data.coordinadores;
            }
        });
        var ajaxPrioridades = $http.post("catalogos/tickets-prioridades", {});
        ajaxPrioridades.success(function (data) {
            if (data.success === true) {
                $scope.prioridades = data.prioridades;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        var ajaxCategorias = $http.post("clientes/categorias-ticket", {});
        ajaxCategorias.success(function (data) {
            if (data.success === true) {
                $scope.categorias = data.categorias;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        var ajaxEstados = $http.post("catalogos/estados-tickets", {});
        ajaxEstados.success(function (data) {
            if (data.success === true) {
                $scope.estados = data.estados;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });

        var numerosPorPagina = 10;
        var pagina = 1;
        $scope.listaPaginas = [];

        var filterTickets = {
            numero: '',
            clienteSolicitud: '',
            fecha: '',
            fechaVencimiento: '',
            prioridad: '',
            categoria: '',
            asunto: '',
            responsable: '',
            estado: '',
        };

        filtro = angular.toJson(filterTickets);
        //Cargando los seguimientos de los ticket.
        var seguimientos = $http.post("clientes/seguimientos-ticket", {
            start: 0,
            lenght: 10,
            filtro: filtro
        });
        seguimientos.success(function (data) {
            if (data.success === true) {
                $scope.actualizarReputacionCliente(data.reputacion);
                $scope.tickets = data.tickets;
                $scope.totalTickets = data.cantidadTickets;
                $scope.idCliente = data.idCliente;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadTickets / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
                    for (var i = 1; i <= $scope.cantPaginas; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas > 5) {
                    for (var i = pagina - 4; i <= pagina; i++) {
                        $scope.listaPaginas.push(i);
                    }
                    posPagin = 5;
                }

                if (pagina > $scope.cantPaginas) {
                    pagina = $scope.cantPaginas;
                    posPagin = pagina;
                }

                setTimeout(function () {
                    var listaPaginador = angular.element("#tabla-seguimiento-tickets-cliente .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }
    //Graficando la Reputacion del cliente
    $scope.actualizarReputacionCliente = function (porciento) {
        if (porciento < 20)
            color = 'black';
        else if (porciento >= 20 && porciento < 40)
            color = 'red';
        else if (porciento >= 40 && porciento < 60)
            color = 'orange';
        else if (porciento >= 60 && porciento < 80)
            color = 'yellow';
        else if (porciento >= 80 && porciento < 90)
            color = 'yellowgreen';
        else if (porciento >= 90)
            color = 'green';
        angular.element("#valor-reputacion-ticket").css(
            { width: porciento + "%", 'background-color': color }
        );

        $scope.reputacion = porciento;
    };

    $scope.SeguirTickets();

}]);

clientApp.filter("dateNoNull", function () {
    return function (textDate) {
        if (textDate === '30/01/1')
            textDate = "NO APLICA";
        return textDate;
    }
});

//Filters
//Filter de angular para las fechas
clientApp.filter("strDateToStrTime", function () {
    return function (textDate) {
        if (textDate !== undefined) {
            var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
            return dateToStrTime(fecha, 'dd/mm/yyyy', '/', true);
        }
        return "";
    }
});

function dateToStrTime(dateObj, format, separator, includeHour) {
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

    if (includeHour) {
        var hour = dateObj.getHours();
        var hour = (hour < 10) ? '0' + hour : hour;
        var minute = dateObj.getMinutes();
        var minute = (minute < 10) ? '0' + minute : minute;
        var second = dateObj.getSeconds();
        var second = (second < 10) ? '0' + second : second;
        var outTime = [hour, minute, second];
        return out.join(sep) + " " + outTime.join(':');
    }
    return out.join(sep);
};

clientApp.filter("strDateToStr", function () {
    return function (textDate) {
        if (textDate === undefined)
            textDate = "";
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStr(fecha);
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

//Filter para el relleno a la izquierda
clientApp.filter("rellenarIzq", function () {
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
    }
});

$('.solo-numero').keyup(function () {
    this.value = (this.value).replace(/[^0-9]/g, '');
});

$('[ng-controller="clientController"]').on('focusout', 'input[ng-required="true"],input[required="required"],input[required="1"],input[required="true"],select[ng-required="true"],select[required="required"],select[required="1"],select[required="true"],textarea[ng-required="true"],textarea[required="required"],textarea[required="1"],textarea[required="true"]', function () {
    var text = $.trim($(this).val());

    if (text === "" || text === undefined || text === '? undefined:undefined ?') {
        $(this).val($.trim($(this).val()));
        $(this).addClass('invalid');
    }
    else {
        $(this).val($.trim($(this).val()));
        $(this).removeClass('invalid');
        $(this).removeClass('campoRequerido');
    }
});
$('input[ng-required="true"],input[required="required"],input[required="1"],input[required="true"],select[ng-required="true"],select[required="required"],select[required="1"],select[required="true"],textarea[ng-required="true"],textarea[required="required"],textarea[required="1"],textarea[required="true"]').addClass('campoRequerido');