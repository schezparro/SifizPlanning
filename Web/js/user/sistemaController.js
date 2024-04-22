var devApp = angular.module('desarrolladores', ['ngRoute']);

devApp.config(function ($provide, $httpProvider) {

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

devApp.controller('desarrolladoresController', ['$scope', '$http', 'filtroService', function ($scope, $http, filtroService) {
    //FUNCIONES DEL MENU
    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('#menu-principal [role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel_tasks").addClass('invisible');
        angular.element("#panel_estimaciones").addClass('invisible');
        angular.element("#panel_tareas_compensatorias").addClass('invisible');
        angular.element("#panel_publicaciones").addClass('invisible');
        angular.element("#panel_tickets").addClass('invisible');
        angular.element("#panel_proyectos").addClass('invisible');
        angular.element("#panel_solicitudes").addClass('invisible');
        angular.element("#panel_ftp").addClass('invisible');
        angular.element("#panel_incidencias").addClass('invisible');
        angular.element("#panel_recursos").addClass('invisible');
    };

    $scope.IrMisTareas = function () {
        ocultar();
        angular.element("#panel_tasks").removeClass('invisible');
        $scope.funcionalidad = "Tareas";
    };

    $scope.IrEstimaciones = function () {
        ocultar();
        angular.element("#panel_estimaciones").removeClass('invisible');
        $scope.funcionalidad = "Estimaciones";
    };

    $scope.IrPanelTareasCompensatorias = function () {
        ocultar();
        angular.element("#panel_tareas_compensatorias").removeClass('invisible');
        $scope.funcionalidad = "Compensatorias";
    };

    $scope.IrRegistrarPublicaciones = function () {
        $scope.loading.show();
        ocultar();
        angular.element("#panel_publicaciones").removeClass('invisible');
        $scope.funcionalidad = "Publicaciones";
        $scope.buscarTicketPorPublicar();
    };

    $scope.IrPanelTickets = function () {
        ocultar();
        angular.element("#panel_tickets").removeClass('invisible');
        $scope.funcionalidad = "Tickets";
    };

    $scope.IrPanelProyectos = function () {
        ocultar();
        angular.element("#panel_proyectos").removeClass('invisible');
        $scope.funcionalidad = "Proyectos";
    };

    $scope.IrPanelSolicitudes = function () {
        ocultar();
        angular.element("#panel_solicitudes").removeClass('invisible');
        $scope.funcionalidad = "Permisos";
    };

    $scope.IrPanelFTP = function () {
        ocultar();
        angular.element("#panel_ftp").removeClass('invisible');
        $scope.funcionalidad = "FTP";
    };

    $scope.IrPanelIncidencias = function () {
        ocultar();
        angular.element("#panel_incidencias").removeClass('invisible');
        $scope.funcionalidad = "Incidencias";
        filtroService.filtroIncidencias = '';
    };

    $scope.IrPanelRecursos = function () {
        ocultar();
        angular.element("#panel_recursos").removeClass('invisible');
        $scope.funcionalidad = "Capacitaciones";
        filtroService.filtroRecursos = '';
    };

    $scope.buscarTicketPorPublicar = function () {
        var jsonTicketPorPublicar = $http.post("user/ticket-por-publicar", {});
        jsonTicketPorPublicar.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.ticketsPorPublicar = data.ticketsPorPublicar;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
        jsonTicketPorPublicar.error(function (data) {
            messageDialog.show('Información', data.msg);
        });
    };

    //MANEJO DE LA FECHA EN EL MODAL DE TIEMPOS DEL PROYECTO
    angular.element('#fecha-registro').datepicker({
        format: 'dd/mm/yyyy',
        language: 'es'
    });

    //FIN DE LAS FUNCIONES DEL MENU

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

    //HISTORICO DEL TICKET
    $scope.idTicketHistorico = 0;
    //Cargando el Historico del ticket
    $scope.verHistoricoTicket = function () {
        angular.element("#modal-historico-ticket").modal("show");
        $scope.cargarEventosTickets();
    };
    angular.element('#modal-historico-ticket').on('hidden.bs.modal', function (e) {
        angular.element("#datos-historico-ticket").html("");
        $scope.eventosHistorico = [];
        $scope.numeroHistoricoTicket = "";
        $scope.clienteHistoricoTicket = "";
        $scope.categoriaHistoricoTicket = "";
        $scope.prioridadHistoricoTicket = "";
    })
    //Cargando Eventos del ticket
    $scope.cargarEventosTickets = function () {
        angular.element("#datos-historico-ticket").html("");
        $scope.eventosHistorico = [];

        var eventosTickets = $http.post("tickets/eventos-historicos-ticket", {
            idTicket: $scope.idTicketHistorico,
            filtro: $scope.filtroEventoTicket
        });
        eventosTickets.success(function (data) {
            if (data.success === true) {
                $scope.eventosHistorico = data.eventos;
                $scope.numeroHistoricoTicket = data.numero;
                $scope.clienteHistoricoTicket = data.cliente;
                $scope.categoriaHistoricoTicket = data.categoria;
                $scope.prioridadHistoricoTicket = data.prioridad;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    //Cargando Datos del evento del ticket
    $scope.verDatosEventoTicket = function (tipo, idObjeto) {
        var eventoTicket = $http.post("tickets/datos-evento-historico-ticket", {
            idTicket: $scope.idTicketHistorico,
            tipo: tipo,
            secuencialObjeto: idObjeto
        });
        eventoTicket.success(function (data) {
            if (data.success === true) {
                angular.element("#datos-historico-ticket").html(data.datos);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
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

    //PARA EL LOADING
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

devApp.controller('publicacionesController', ['$scope', '$http', function ($scope, $http) {
    $scope.idTicket = 0;
    $scope.publicarEnUno = false;
    //Una publicación se ha realizado...
    $scope.seleccionarParaPublicar = function (id) {
        $scope.idTicket = id;
        angular.element("#modal-enviar-publicacion").modal('show');
    };

    $scope.seleccionarPublicarEnUno = function () {
        $scope.publicarEnUno = true;
        angular.element("#modal-enviar-publicacion").modal('show');
    };

    $scope.publicacionRealizada = function () {
        if ($scope.idTicket === 0 && $scope.publicarEnUno === false) {
            messageDialog.show('Información', "No se ha escogido el ticket.");
            return false;
        }

        $scope.$parent.loading.show();
        var jsonTicketPublicacion = $http.post("user/ticket-publicado",
            {
                id: $scope.idTicket,
                publicacionClienteServidor: $scope.tipoPublicacion,
                publicacionPruebasProd: $scope.objetivoPublicacion,
                dirFTP: $scope.ftpPublicacion,
                usuarioFTP: $scope.usuarioFtp,
                claveFTP: $scope.claveFtp,
                pathFTP: $scope.pathPublicacion,
                publicarEnUno: $scope.publicarEnUno,
                idPublicacionLote: $scope.idPublicacionLote
            });
        jsonTicketPublicacion.success(function (data) {
            $scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.$parent.buscarTicketPorPublicar();

                $scope.idTicket = 0;
                $scope.idPublicacionLote = [];
                $scope.publicarEnUno = false;
                $scope.tipoPublicacion = "";
                $scope.objetivoPublicacion = "";
                $scope.ftpPublicacion = "";
                $scope.usuarioFtp = "";
                $scope.claveFtp = "";
                $scope.pathPublicacion = "";

                angular.element("#modal-enviar-publicacion").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
        jsonTicketPublicacion.error(function (data) {
            messageDialog.show('Información', data.msg);
        });
    };

    $scope.idPublicacionLote = [];
    //Escogiendo la publicación
    angular.element("#tabla-publicaciones").on("click", ".fila-publicacion", function () {
        var jQ = angular.element;
        var idTicket = jQ(this).attr('data-id');
        if (jQ(this).hasClass("selected")) {
            jQ(this).removeClass("selected");

            $scope.idPublicacionLote = jQ.grep($scope.idPublicacionLote, function (value) {
                return value != idTicket;
            });
        }
        else {
            jQ(this).addClass("selected");
            $scope.idPublicacionLote.push(idTicket);

        }
    });

}]);

//Filter para el relleno a la izquierda
devApp.filter("rellenarIzq", function () {
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

//Filter de angular para las fechas y Hora
devApp.filter("strDateToStrTime", function () {
    return function (textDate) {
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStrTime(fecha);
    }
});

function dateToStrTime(dateObj, format, separator) {
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

    var hour = dateObj.getHours();
    var hour = (hour < 10) ? '0' + hour : hour;
    var minutes = dateObj.getMinutes();
    var minutes = (minutes < 10) ? '0' + minutes : minutes;


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
    return out.join(sep) + " " + hour + ':' + minutes;
};

$(document).on('keyup', '.solo-numero', function () {
    this.value = (this.value).replace(/[^0-9]/g, '');
});

//filter para el usuario de el email
devApp.filter("userEmail", function () {
    return function (textEmail) {
        var array = textEmail.split('@');
        return array[0];
    }
});