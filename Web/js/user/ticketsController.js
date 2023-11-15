devApp.controller('ticketsController', ['$scope', '$http', function ($scope, $http) {
    $scope.recargarSeguimientoTickets = function (filtro, todos) {
        if (filtro === undefined) {
            filtro = "";
        }
        if (todos === undefined) {
            todos = false;
        }
        //Recargando los seguimientos de los ticket.
        var seguimientos = $http.post("user/tickets-usuario", {
            filtro: filtro,
            todos: todos
        });
        seguimientos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.tickets = data.tickets;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    }
    $scope.recargarSeguimientoTickets();

    var ajaxMotivosDevolucion = $http.post("catalogos/motivos-devolucion-ticket", {});
    ajaxMotivosDevolucion.success(function (data) {
        if (data.success === true) {
            $scope.motivos = data.motivos;
        }
    });

    $scope.mostrarTodosSeguimientos = function () {
        $scope.recargarSeguimientoTickets($scope.filtroSeguimientoTicket, $scope.mostrarTodosSeguimientoTicket);
    };

    $scope.filtrarSeguimientosTickets = function () {
        $scope.recargarSeguimientoTickets($scope.filtroSeguimientoTicket, $scope.mostrarTodosSeguimientoTicket);
    };

    $scope.wind2Opciones = new questionMsgDialog();
    //PARA VER LOS DATOS DEL TICKET
    $scope.verDatosTicketCliente = function (id) {
        $scope.idTicket = id;
        var informacionTicket = $http.post("clientes/datos-ticket/",
            {
                idTicket: id
            });
        informacionTicket.success(function (data) {
            if (data.success === true) {
                $scope.fechaTicket = data.datosTicket.fecha;
                $scope.nombreCliente = data.datosTicket.cliente;
                $scope.usuarioCliente = data.datosTicket.usuarioCliente;
                $scope.usuarioTelefono = data.datosTicket.clienteTelefono;
                $scope.estadoTicket = data.datosTicket.estado;
                $scope.prioridadTicket = data.datosTicket.prioridad;
                $scope.categoriaTicket = data.datosTicket.categoria;
                $scope.clienteReporta = data.datosTicket.reporto;
                $scope.telefonoCliente = data.datosTicket.telefono;
                $scope.reputacionTicket = data.datosTicket.reputacion;
                $scope.asuntoTicket = data.datosTicket.asunto;
                $scope.detalleTicket = data.datosTicket.detalle;

                $scope.adjuntosTicket = data.datosTicket.adjuntos;

                angular.element("#modal-vista-ticket-user").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };
    
    $scope.limpiarVistaTicketCliente = function () {
        angular.element("#panel-add-adjuntos-ticket").html("");
    };
    $('#modal-vista-ticket-user').on('hidden.bs.modal', function (e) {
        $scope.limpiarVistaTicketCliente();
        $scope.mostrarTodosSeguimientos = function () {
            $scope.recargarSeguimientoTickets($scope.filtroSeguimientoTicket, $scope.mostrarTodosSeguimientoTicket);
        };
    });
    $scope.motivoDevolucion = function () {
        $scope.comentarioTicketDevuelto = "";
        angular.element("#modal-comentario-ticket-devuelto").modal("show");
    };
    $scope.argumentarDevolucionTicket = function () {
        waitingDialog.show('Guardando en histórico del Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var historicoDevolucionTicket = $http.post("tickets/grabar-informacion-historico-devolucion",
            {
                idTicket: $scope.idTicket,
                texto: "<b>Motivo Devolución: </b>" + $scope.motivoDevolucionTicket + ". <br> " + $scope.comentarioTicketDevuelto
            });
        historicoDevolucionTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-vista-ticket-user").modal('hide');
                angular.element("#modal-comentario-ticket-devuelto").modal("hide");
                //Recargando la lista de tickets    
                $scope.mostrarTodosSeguimientos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funciones del comentario
    $scope.verComentarioTicket = function () {
        angular.element("#modal-comentarios-ticket").modal('show');
        $scope.cargarComentarios();
    };
    $scope.cargarComentarios = function () {
        // $scope.$parent.loading.show();
        var cargarComentario = $http.post("user/cargar-comentarios-ticket/",
            {
                idTicket: $scope.idTicket
            });
        cargarComentario.success(function (data) {
            //$scope.$parent.loading.hide();
            if (data.success === true) {
                $scope.comentarios = data.comentarios;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.guardarComentario = function () {
        waitingDialog.show('Grabando el comentario...', { dialogSize: 'sm', progressType: 'success' });
        var guardarComentario = $http.post("user/guardar-comentario-ticket/",
            {
                idTicket: $scope.idTicket,
                comentario: $scope.comentarioTicket
            });
        guardarComentario.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.comentarioTicket = "";
                $scope.cargarComentarios();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.panelEmail = function (colaborador, fecha, detalle) {
        $scope.destinatariosEmailTicket = colaborador + ";";
        $scope.asuntoEmailTicket = "Comentario del Ticket:" + " " + $scope.idTicket;

        var fech = new Date(parseInt(fecha.replace('/Date(', '')));
        var result = dateToStrTime(fech, 'dd/mm/yyyy', '/', true);
        $scope.comentarioEmailTicket = colaborador + "  " + result + ":  " + "\n" + detalle;

        angular.element("#modal-email-comentarios").modal('show');
    };
    $scope.enviarEmail = function () {
        waitingDialog.show('Enviando correo...', { dialogSize: 'sm', progressType: 'success' });
        var enviarEmail = $http.post("tickets/enviar-email-comentario/",
            {
                idTicket: $scope.idTicket,
                destinatariosEmailTicket: $scope.destinatariosEmailTicket,
                asuntoEmailTicket: $scope.asuntoEmailTicket,
                comentarioEmailTicket: $scope.comentarioEmailTicket
            });
        enviarEmail.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.cargarComentarios();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

}]);

//Filters
//Filter de angular para las fechas
devApp.filter("strDateToStr", function () {
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

//Filter de tamaño de texto
devApp.filter("strLimit", ['$filter', function ($filter) {
    return function (input, limit) {
        if (!input) return;
        if (input.length <= limit) {
            return input;
        }
        return $filter('limitTo')(input, limit) + '...';
    };
}]);