var loginApp = angular.module('login', []);

loginApp.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });
                event.preventDefault();
            }
        });
    };
});

loginApp.controller('loginController', ['$scope', '$http', '$location', function ($scope, $http, $location) {
    $scope.loadingAjax = function () {
        this.show = function (target) {
            angular.element("#loadingDiv").show();
        };
        this.hide = function (target) {
            angular.element("#loadingDiv").hide();
        };
    };

    $scope.loading = new $scope.loadingAjax();
    if ($location.absUrl().includes("respuesta-resolucion")) {
        var miCadena = $location.absUrl();
        var subcadena = miCadena.substring(miCadena.indexOf("respuesta-resolucion") + "respuesta-resolucion".length);
    }
    $scope.autenticar = function () {
        $scope.loading.show();
        $scope.msg = '';
        var ajax = $http.post("sifizplanning/login/", {
            email: $scope.email,
            password: $scope.passw
        });
        ajax.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                if ($location.absUrl().includes("respuesta-resolucion")) {
                    window.location.href = "clientes/respuesta-resolucion?cod=" + encodeURIComponent(subcadena);
                } else {
                    window.location.href = "sifizplanning/redirect";
                }
            }
            else {
                $scope.msg = data.msg;
            }
        });
    };    

}]);

//-------------------- PAGINA DE MENU -----------------------------------

var menuApp = angular.module('menu', []);

menuApp.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });
                event.preventDefault();
            }
        });
    };
});

//Filters
//Filter de Angular para mostrar cadenas de texto con etiquetas html
menuApp.filter("trust", ['$sce', function ($sce) {
    return function (htmlCode) {
        return $sce.trustAsHtml(htmlCode);
    }
}]);

//Filter de angular para las fechas
menuApp.filter("strDateToStr", function () {
    return function (textDate) {
        if (textDate !== undefined) {
            var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
            return dateToStr(fecha);
        }
        return "";
    }
});

//Filter para el relleno a la izquierda
menuApp.filter("rellenarIzq", function () {
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

menuApp.controller('menuController', ['$scope', '$http', function ($scope, $http) {
    $scope.importanciasTareas = [
        { nombre: "Todos", tipo: "", cantidad: 0 },
        { nombre: "Normal", tipo: "1", cantidad: 0 },
        { nombre: "Importante", tipo: "2", cantidad: 0 },
        { nombre: "Muy Importante", tipo: "3", cantidad: 0 }
    ];

    $scope.importanciasTickets = [
        { nombre: "Todos", tipo: "", cantidad: 0 },
        { nombre: "Normal", tipo: "1", cantidad: 0 },
        { nombre: "Importante", tipo: "2", cantidad: 0 },
        { nombre: "Muy Importante", tipo: "3", cantidad: 0 }
    ];

    $scope.importanciasDesacuerdos = [
        { nombre: "Todos", tipo: "", cantidad: 0 },
        { nombre: "Normal", tipo: "1", cantidad: 0 },
        { nombre: "Importante", tipo: "2", cantidad: 0 },
        { nombre: "Muy Importante", tipo: "3", cantidad: 0 }
    ];

    //Inicio del SignalR
    var menuProxie = angular.element.connection.websocket;
    angular.element.connection.hub.start();
    menuProxie.client.recargarPanelComentarios = function () {
        $scope.updateComments("tareaticket");
    };
    menuProxie.client.nuevoComentarioMuyImportante = function () {
        ejecutarSonido();
    };
    //Fin del signalR

    $scope.ComentariosNoLeidosTareas = [];
    $scope.ComentariosNoLeidosTickets = [];

    $scope.importTareaTipo = "";
    $scope.importTicketTipo = "";

    //Muestra la ventana modal con los comentarios no leidos
    $scope.showModalVerComentarios = function () {
        angular.element("#modalVerComentarios").modal('show');
        $scope.updateComments("tareaticket");
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

    $scope.mostrarWindowsComentarios = function () {
        var jQ = angular.element;
        jQ('#modal-comentarios-generales').modal('show');
    };

    //Tipo: tarea (Actualiza sólo los mentarios de tipo TAREA)
    //Tipo: ticket (Actualiza sólo los mentarios de tipo TICKET)
    //Tipo: tareaticket (Actualiza los mentarios de ambos tipos)
    $scope.updateCommentsData = function (data, tipo) {
        if (tipo.indexOf("tarea") >= 0)
            $scope.importanciasTareas[3].cantidad = $scope.importanciasTareas[2].cantidad = $scope.importanciasTareas[1].cantidad = $scope.importanciasTareas[0].cantidad = 0;

        if (tipo.indexOf("ticket") >= 0)
            $scope.importanciasTickets[3].cantidad = $scope.importanciasTickets[2].cantidad = $scope.importanciasTickets[1].cantidad = $scope.importanciasTickets[0].cantidad = 0;

        if (tipo.indexOf("tarea") >= 0) {
            $scope.ComentariosNoLeidosTareas = data.comentariosDeTareas;
            angular.forEach($scope.ComentariosNoLeidosTareas, function (comentario) {
                switch (comentario.tipo) {
                    case "1":
                        $scope.importanciasTareas[1].cantidad++;
                        break;
                    case "2":
                        $scope.importanciasTareas[2].cantidad++;
                        break;
                    case "3":
                        $scope.importanciasTareas[3].cantidad++;
                        break;
                };
            });
            $scope.importanciasTareas[0].cantidad = $scope.importanciasTareas[1].cantidad + $scope.importanciasTareas[2].cantidad + $scope.importanciasTareas[3].cantidad;
        }

        if (tipo.indexOf("ticket") >= 0) {
            $scope.ComentariosNoLeidosTickets = data.comentariosDeTickets;
            angular.forEach($scope.ComentariosNoLeidosTickets, function (comentario) {
                switch (comentario.tipo) {
                    case "1":
                        $scope.importanciasTickets[1].cantidad++;
                        break;
                    case "2":
                        $scope.importanciasTickets[2].cantidad++;
                        break;
                    case "3":
                        $scope.importanciasTickets[3].cantidad++;
                        break;
                };
            });
            $scope.importanciasTickets[0].cantidad = $scope.importanciasTickets[1].cantidad + $scope.importanciasTickets[2].cantidad + $scope.importanciasTickets[3].cantidad;
        }
    };

    //Tipo: tarea (Actualiza sólo los mentarios de tipo TAREA)
    //Tipo: ticket (Actualiza sólo los mentarios de tipo TICKET)
    //Tipo: tareaticket (Actualiza los mentarios de ambos tipos)
    $scope.updateComments = function (tipo) {        
        $scope.loading.show();
        var ajaxComentariosNoLeidos = $http.post("comentarios-no-leidos");
        ajaxComentariosNoLeidos.success(function (data) {
            $scope.loading.hide();

            if (data.success === true) {
                $scope.updateCommentsData(data, tipo);
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        angular.element('[data-toggle="tooltip"]').tooltip();
    };

    $scope.MarcarComentarioLeido = function (id, tipoComentario) {
        $scope.loading.show();
        var ajax = $http.post("marcar-comentario-general-leido", { idComentario: id });
        ajax.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.updateComments(tipoComentario);
            }
            else {
                $scope.msg = data.msg;
            }
        });
    };

    $scope.marcarTodosLeidos = function (tipoComentario) {
        $scope.loading.show();

        var importanciaSeleccionada;
        var comentariosNoLeidos;

        switch (tipoComentario) {
            case "tareas":
                comentariosNoLeidos = $scope.ComentariosNoLeidosTareas;
                importanciaSeleccionada = $scope.importTareaTipo;
                break;
            default: //tickets
                comentariosNoLeidos = $scope.ComentariosNoLeidosTickets;
                importanciaSeleccionada = $scope.importTicketTipo;
                break;
        }

        if (importanciaSeleccionada != "") {
            var ajax = $http.post("marcar-comentario-general-leido", {
                idComentario: 0,
                tipoComentario: tipoComentario,
                importanciaSeleccionada: parseInt(importanciaSeleccionada)
            });
            ajax.success(function (data) {
                if (data.success === true) {
                    $scope.loading.hide();
                    $scope.updateComments(tipoComentario);
                }
                else {
                    $scope.msg = data.msg;
                }
            });
        }
    };

    $scope.mostrarTodosLosComentarios = function (tipoComentario) {
        var noLeidos = angular.element("#" + tipoComentario + "MostrarLeidosNoLeidos").prop('checked');

        var ajax = $http.post("Mostrar-Todos-Comentarios", {
            tipoComentario: tipoComentario,
            noLeidos: noLeidos ? 1 : 0
        });
        ajax.success(function (data) {
            if (data.success === true) {
                $scope.loading.hide();
                $scope.updateCommentsData(data, tipoComentario);
            }
            else {
                $scope.msg = data.msg;
            }
        });
    };

    function ejecutarSonido() {
        var sonido = document.getElementById('audio-chat');
        sonido.play();
    }

    //Mostrando la modal con los detalles del ticket seleccionado
    $scope.mostrarDetalleTicket = function (idTicket) {
        if (idTicket != "") {
            $scope.loading.show();
            //angular.element("#div-check-funcionalidades").html('');
            $scope.idTicket = idTicket;
            $scope.$parent.idTicketHistorico = idTicket

            //OBTENIENDO LAS PROXIMAS ACTIVIDADES
            var ajaxProximaActividad = $http.post("tickets/dar-proxima-actividad-segun-ticket",
                                {
                                    idTicket: idTicket
                                });
            ajaxProximaActividad.success(function (data) {
                if (data.success === true) {
                    $scope.proximasActividades = data.proximasActividades;

                    var buscarDatosTicket = $http.post("tickets/dar-datos-ticket",
                                                {
                                                    idTicket: idTicket
                                                });
                    buscarDatosTicket.success(function (data) {
                        $scope.loading.hide();
                        if (data.success === true) {
                            //console.log(data);
                            $scope.idTicketCliente = data.datosTicket.id;
                            $scope.fechaTicket = data.datosTicket.fecha;
                            $scope.nombreCliente = data.datosTicket.cliente;
                            $scope.usuarioCliente = data.datosTicket.usuarioCliente;
                            $scope.telefonoCliente = data.datosTicket.telefono;
                            $scope.usuarioTelefono = data.datosTicket.clienteTelefono;
                            $scope.clienteReporta = data.datosTicket.reporto;
                            $scope.reputacionCliente = data.reputacion;
                            $scope.reputacionTicket = data.datosTicket.reputacion;
                            $scope.asuntoTicket = data.datosTicket.asunto;
                            $scope.seFactura = data.datosTicket.seFactura;
                            $scope.facturado = data.datosTicket.facturado;
                            $scope.reprocesos = data.datosTicket.reprocesos;
                            $scope.ingresoInterno = data.datosTicket.ingresoInterno;
                            $scope.tipoRecurso = data.datosTicket.tipoRecurso;
                            $scope.tipoRecursoDescripcion = data.datosTicket.tipoRecursoDesc;

                            $scope.actividadProxima = data.datosTicket.actividadProxima;
                            $scope.actividadProximaDescripcion = data.datosTicket.actividadProximaDesc;
                            angular.element('[ng-model="actividadProxima"]').val(data.datosTicket.actividadProxima)

                            $scope.estadoTicket = data.datosTicket.estado;
                            $scope.prioridadTicket = data.datosTicket.prioridadDesc;
                            $scope.categoriaTicket = data.datosTicket.categoriaDesc;
                            $scope.adjuntosTicket = data.datosTicket.adjuntos;
                            $scope.estimacionTicket = data.datosTicket.estimacion;
                            $scope.detalleTicket = data.datosTicket.detalle;
                            //$scope.botones = data.datosTicket.botones;
                            $scope.verificador = data.datosTicket.verificador;
                            $scope.categoria = data.categoria;
                            $scope.categoriaDescripcion = data.datosTicket.categoriaDesc;
                            $scope.prioridad = data.prioridad;
                            $scope.prioridadDescripcion = data.datosTicket.prioridadDesc;
                            $scope.error = data.error;
                            $scope.requiereTesting = data.datosTicket.requiereTesting;
                            $scope.ticketVersionCliente = data.ticketVersionCliente;

                            $scope.asignaciones = data.asignaciones;

                            //console.log($scope.$parent);

                            if (data.tecnicoSugerido !== null) {
                                $scope.tecnicoSugerido = data.tecnicoSugerido.idTec;
                                $scope.tecnicoSugeridoNombre = data.tecnicoSugerido.nombreTecnico;
                                $scope.observaciones = data.tecnicoSugerido.observaciones;
                            }
                            else {
                                $scope.tecnicoSugerido = 0;
                                $scope.tecnicoSugeridoNombre = "No especificado";
                                $scope.observaciones = "";
                            }

                            $scope.totalHorasAsignadas = 0;
                            angular.element.each($scope.asignaciones, function (key, value) {
                                $scope.totalHorasAsignadas += parseInt(value.numeroHoras);
                            });

                            angular.element("#modal-gestion-ticket").modal('show');

                            //Actualizando los colaboradores asignados
                            setTimeout(function () {
                                angular.element.each($scope.asignaciones, function (key, value) {
                                    var selector = '#panel-colaboradores-ticket [type="checkbox"][value="' + value.idColab + '"]';
                                    angular.element(selector).click();
                                });
                            }, 500);

                        }
                        else {
                            messageDialog.show('Información', data.msg);
                        }
                    });
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

    //Cambiando la proxima actividad de un ticket
    $scope.cambiarProximaActividadTicket = function () {
        var ajaxCambiarProximaActividad = $http.post("tickets/cambiar-proxima-actividad",
                            {
                                idTicket: $scope.idTicket,
                                idProximaActividad: $scope.changeProximaActividad
                            });
        ajaxCambiarProximaActividad.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-cambio-proxima-actividad").modal("hide");
                $scope.mostrarDetalleTicket($scope.idTicket);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

}]);
