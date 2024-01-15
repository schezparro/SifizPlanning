clientApp.controller('clientSeguimiento', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;
    $scope.FiltroNumero = true;

    var orderStatus = [1, 1, 1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

    $scope.cambiarPagina = function (pag) {
        pagina = pag;
        $scope.paginar();
    };

    $scope.atrazarPagina = function () {
        if (pagina > 1) {
            pagina--;
            $scope.paginar();
        }
    };

    $scope.avanzarPagina = function () {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.paginar();
        }
    };

    $scope.actualizarCantidadMostrar = function () {
        numerosPorPagina = $scope.cantidadMostrarPorPagina;
        $scope.paginar();
    };

    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.recargarSeguimientoTickets(start, lenght, "", globalOrder, globalAsc);
    };

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

    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna-seguimientos i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna-seguimientos");
        var columna = columnas[valor - 1];
        var flecha = angular.element(columna).find("i");

        angular.element(flecha).removeClass('invisible');

        if (globalAsc === 0) {
            angular.element(flecha).removeClass('glyphicon-chevron-up');
            angular.element(flecha).addClass('glyphicon-chevron-down');
        }
        else {
            angular.element(flecha).addClass('glyphicon-chevron-up');
            angular.element(flecha).removeClass('glyphicon-chevron-down');
        }
        $scope.recargarSeguimientoTickets(start, lenght, "", globalOrder, globalAsc);
    };

    $scope.filterData = function (valor) {
        if ($scope.numero !== undefined) {
            filterTickets.numero = $scope.numero;
        }

        if ($scope.clienteSolicitud !== undefined) {
            filterTickets.clienteSolicitud = $scope.clienteSolicitud;
        }

        if ($scope.fecha !== undefined) {
            filterTickets.fecha = $scope.fecha;
        }

        if ($scope.fechaVencimiento !== undefined) {
            filterTickets.fechaVencimiento = $scope.fechaVencimiento;
        }

        if ($scope.asunto !== undefined) {
            filterTickets.asunto = $scope.asunto;
        }
        
        switch (valor) {
            case 1:
                filterTickets.numero = $scope.numero;
                $scope.FiltroNumero = false;
                break;
            case 2:
                filterTickets.clienteSolicitud = $scope.clienteSolicitud;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterTickets.fecha = $scope.fecha;
                $scope.FiltroNumero = false;
                break;
            case 4:
                filterTickets.fechaVencimiento = $scope.fechaVencimiento;
                $scope.FiltroNumero = false;
                break;
            case 5:
                var filtroPrioridad = angular.element('#select-prioridad').val();

                if (filtroPrioridad === "Seleccione...") {
                    filterTickets.prioridad = "";
                } else {
                    filterTickets.prioridad = filtroPrioridad;
                }

                $scope.FiltroNumero = false;
                break;
            case 6:
                var filtroCategoria = angular.element('#select-categoria').val();

                if (filtroCategoria === "Seleccione...") {
                    filterTickets.categoria = "";
                } else {
                    filterTickets.categoria = filtroCategoria;
                }

                $scope.FiltroNumero = false;
                break;
            case 7:
                filterTickets.asunto = $scope.asunto;
                $scope.FiltroNumero = false;
                break;
            case 8:
                var filtroResponsable = angular.element('#select-responsable').val();

                if (filtroResponsable === "Seleccione...") {
                    filterTickets.responsable = "";
                } else {
                    filterTickets.responsable = filtroResponsable;
                }

                $scope.FiltroNumero = false;
                break;
            case 9:
                var filtroEstado = angular.element('#select-estado').val();

                if (filtroEstado === "Seleccione...") {
                    filterTickets.estado = "";
                } else {
                    filterTickets.estado = filtroEstado;
                }

                $scope.FiltroNumero = false;
                break;
        }

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarSeguimientoTickets(start, lenght, "", globalOrder, globalAsc);
    };

    //Para los selects
    angular.element('#select-prioridad').on('click', function (e) {
        $scope.filterData(5);
    });
    angular.element('#select-categoria').on('click', function (e) {
        $scope.filterData(6);
    });
    angular.element('#select-responsable').on('click', function (e) {
        $scope.filterData(8);
    });
    angular.element('#select-estado').on('click', function (e) {
        $scope.filterData(9);
    });

    $scope.mostrarTodosLosTickets = function () {
        $scope.recargarSeguimientoTickets(0, $scope.cantidadMostrarPorPagina, "", globalOrder, globalAsc);
    };

    $scope.recargarSeguimientoTickets = function (start, lenght, filtro, order, asc) {

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {

            if (filterTickets.numero === undefined)
                filterTickets.numero = '';
            if (filterTickets.clienteSolicitud === undefined)
                filterTickets.clienteSolicitud = '';
            if (filterTickets.fecha === undefined)
                filterTickets.fecha = '';
            if (filterTickets.fechaVencimiento === undefined)
                filterTickets.fechaVencimiento = '';
            if (filterTickets.prioridad === undefined)
                filterTickets.prioridad = '';
            if (filterTickets.categoria === undefined)
                filterTickets.categoria = '';
            if (filterTickets.asunto === undefined)
                filterTickets.asunto = '';
            if (filterTickets.responsable === undefined)
                filterTickets.responsable = '';
            if (filterTickets.estado === undefined)
                filterTickets.estado = '';

            filtro = angular.toJson(filterTickets)
        }

        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;

        //Recargando los seguimientos de los ticket.
        var seguimientos = $http.post("clientes/seguimientos-ticket", {
            start: start,
            lenght: lenght,
            filtro: filtro,
            order: order,
            asc: asc,
            todos: $scope.mostrarTodos
        });
        seguimientos.success(function (data) {
            if (data.success === true) {
                $scope.tickets = data.tickets;
                $scope.actualizarReputacionCliente(data.reputacion);
                $scope.totalTickets = data.cantidadTickets;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadTickets / numerosPorPagina);
                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
                    for (var j = 1; j <= $scope.cantPaginas; j++) {
                        $scope.listaPaginas.push(j);
                    }
                }
                else if ($scope.cantPaginas > 5) {
                    for (var k = pagina - 4; k <= pagina; k++) {
                        $scope.listaPaginas.push(k);
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
    };

    $scope.mostrarOfertaCotizacion = function (idTicket) {
        $scope.idTicket = idTicket;
        //Recargando los seguimientos de los ticket.
        var propuesta = $http.post("clientes/propuesta-cotizacion", {
            idTicket: idTicket
        });
        propuesta.success(function (data) {
            if (data.success === true) {
                if (data.propuesta === true) {
                    angular.element("#modal-propuesta-cotizacion").modal("show");
                    angular.element("#detalleCotizacion").html(data.texto);
                    if (data.renegociar === true) {
                        angular.element('[ng-click="renegociarCotizacion()"]').show();
                    }
                    else {
                        angular.element('[ng-click="renegociarCotizacion()"]').hide();
                    }
                }
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.wind2Opciones = new questionMsgDialog();
    //Para rechazar la cotizacion
    $scope.rechazarCotizacion = function () {
        $scope.wind2Opciones.show('Información', 'Está seguro de querer rechazar la oferta.', 'Si, estoy seguro', function () {
            $scope.respuestaCotizacion(false);
            $scope.wind2Opciones.hide();
        });
    };
    //Para aceptar la cotizacion
    $scope.aceptarCotizacion = function () {
        $scope.respuestaCotizacion(true);
    };
    //Para cada una de las respuestas de cotizacion
    $scope.respuestaCotizacion = function (resp) {
        waitingDialog.show('Enviando...', { dialogSize: 'sm', progressType: 'success' });
        var respuesta = $http.post("clientes/respuesta-cotizacion", {
            idTicket: $scope.idTicket,
            respuesta: resp
        });
        respuesta.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-propuesta-cotizacion").modal("hide");
                $scope.$parent.recargarSeguimientoTickets($scope.filtroSeguimientoTicket, $scope.mostrarTodosSeguimientoTicket);
            }
            messageDialog.show('Información', data.msg);
        });
    };
    //Para renegociar la cotizacion
    $scope.renegociarCotizacion = function () {
        angular.element("#modal-regenociar-cotizacion").modal("show");
    };
    $scope.enviarRenegociacion = function () {
        waitingDialog.show('Enviando Renegociación...', { dialogSize: 'sm', progressType: 'success' });
        var renegociar = $http.post("clientes/enviar-renegociacion", {
            idTicket: $scope.idTicket,
            texto: $scope.detalleRenegociacion
        });
        renegociar.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-regenociar-cotizacion").modal("hide");
                angular.element("#modal-propuesta-cotizacion").modal("hide");
                $scope.$parent.recargarSeguimientoTickets($scope.filtroSeguimientoTicket, $scope.mostrarTodosSeguimientoTicket);
            }
            messageDialog.show('Información', data.msg);
        });
    };

    //PARA VER LOS DATOS DEL TICKET
    $scope.verDatosTicketCliente = function (id) {
        $scope.idTicket = id;
        var informacionTicket = $http.post("clientes/datos-ticket/",
            {
                idTicket: id
            });
        informacionTicket.success(function (data) {
            if (data.success === true) {
                console.log(data);

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
                $scope.ticketVersionCliente = data.datosTicket.ticketVersionCliente;

                $scope.adjuntosTicket = data.datosTicket.adjuntos;

                angular.element("#modal-vista-ticket-user").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };
    //Adicionando adjuntos al ticket
    angular.element("#eliminar-adj-ticket").prop("disabled", true);
    $scope.adicionarAdjuntoTicket = function () {
        angular.element("#panel-add-adjuntos-ticket").append(angular.element("#htmlFile").html());
        angular.element("#eliminar-adj-ticket").prop("disabled", false);
    };
    $scope.eliminarAdjuntoTicket = function () {
        var elementos = angular.element("#panel-add-adjuntos-ticket .file-adj-contrato");
        if (elementos.length > 0) {
            angular.element(elementos).last().remove();
        }
        if (elementos.length === 1) {
            angular.element("#eliminar-adj-ticket").prop("disabled", true);
        }
    };
    $scope.grabarAdjuntosTicket = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var formData = new FormData();
        formData.append('idTicket', $scope.idTicket);
        formData.append('asunto', $scope.asuntoTicket);
        formData.append('detalle', $scope.detalleTicket);
        formData.append('edit', $scope.estadoTicket === "ABIERTO" ? true : false);
        angular.element.each(angular.element('#modal-vista-ticket-user').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });
        var nuevosAdjuntosTicket = $http.post("clientes/anadir-adjuntos-ticket",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        nuevosAdjuntosTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.recargarSeguimientoTickets(0, 10, "");
                //recargar los seguimientos de ticket
                angular.element("#modal-vista-ticket-user").modal('hide');
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
            $scope.$parent.recargarSeguimientoTickets($scope.filtroSeguimientoTicket, $scope.mostrarTodosSeguimientoTicket);
        };
    });

    //Funciones del comentario
    $scope.verComentarioTicket = function () {
        angular.element("#modal-comentarios-ticket").modal('show');
        $scope.cargarComentarios();
    };
    $scope.cargarComentarios = function () {
       // $scope.$parent.loading.show();
        var cargarComentario = $http.post("clientes/cargar-comentarios-ticket/",
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
        var guardarComentario = $http.post("clientes/guardar-comentario-ticket/",
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

    $scope.abrirModalAnular = function () {
        $scope.comentarioTicketAnulado = "";
        angular.element("#modal-comentario-ticket-anulado").modal("show");
    };
    $scope.abrirModalRechazar = function () {
        $scope.textoDesacuerdo = "";
        angular.element('#form-enviar-email-desacuerdo-ticket').show();
        angular.element('#div-gracias').hide();

        var jQ = angular.element;
        var divAdjuntos = jQ('#deleteInputFileAnteriorCliente').parent().parent();
        divAdjuntos.find('.file-adj-contrato').remove();
        angular.element('#deleteInputFileAnteriorCliente').attr('disabled', 'disabled');
        angular.element(".file-adj-contrato:visible").remove();
        angular.element("#modal-comentario-ticket-rechazado").modal("show");
    };
    $scope.anularTicket = function () {
        waitingDialog.show('Anulando Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var anularTicket = $http.post("tickets/anular-ticket/",
            {
                idTicket: $scope.idTicket
            });
        anularTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                var guardarComentario = $http.post("tickets/guardar-comentario-ticket/",
                    {
                        idTicket: $scope.idTicket,
                        comentario: "ANULADO: " + $scope.comentarioTicketAnulado
                    });
                guardarComentario.success(function (data) {
                    if (data.success === true) {
                        $scope.comentarioTicketAnulado = "";
                    }
                    else {
                        messageDialog.show('Información', data.msg);
                    }
                });

                angular.element("#modal-vista-ticket-user").modal('hide');
                angular.element("#modal-comentario-ticket-anulado").modal("hide");
                //Recargando la lista de tickets    
                $scope.mostrarTodosSeguimientos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.rechazarTicket = function () {
        waitingDialog.show('Rechazando el ticket...', { dialogSize: 'sm', progressType: 'success' });
        var rechazandoTicket = $http.post("tickets/rechazar-ticket/",
            {
                idTicket: $scope.idTicket,
                comentario: "RECHAZADO: " + $scope.comentarioTicketRechazado
            });
        rechazandoTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-comentario-ticket-rechazado").modal('hide');
                angular.element("#modal-vista-ticket-user").modal('hide');
                $scope.mostrarTodosSeguimientos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Funcion para cerrar un ticket
    $scope.cerrarTicket = function () {
        waitingDialog.show('Cerrando el Ticket...', { dialogSize: 'sm', progressType: 'success' });
        var aprobarTicket = $http.post("tickets/cerrar-ticket/",
            {
                idTicket: $scope.idTicket
            });
        aprobarTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-vista-ticket-user").modal('hide');
                $scope.mostrarTodosSeguimientos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    //Adicionar un campo de fichero
    $scope.adicionarFileAnterior = function () {
        var listaBotones = angular.element("#barra-botones-file-cliente");
        var html = angular.element("#htmlFileCliente").html();
        angular.element(html).insertBefore(listaBotones);
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 0) {
            angular.element('#deleteInputFileAnteriorCliente').removeAttr('disabled');
        }
    };

    //Eliminar un campo de fichero
    $scope.deleteFileAnterior = function () {
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 0) {
            angular.element("#barra-botones-file-cliente").prev().remove();
            cantFileInput = angular.element(".file-adj-contrato:visible").length;
            if (cantFileInput < 1) {
                angular.element('#deleteInputFileAnteriorCliente').attr('disabled', 'disabled');
            }
        }
    };

    $scope.enviarDesacuerdoResolucionTicketCliente = function () {

        var numeroTicket = $scope.idTicket;

        waitingDialog.show('Enviando email...', { dialogSize: 'sm', progressType: 'success' });
        var formData = new FormData();
        formData.append('numeroTicket', numeroTicket);
        formData.append('detalle', $scope.textoDesacuerdo);

        angular.element.each(angular.element('#form-enviar-email-desacuerdo-ticket').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });

        var informacionDevueltoTicket = $http.post("clientes/enviar-email-ticket-devuelto-cliente",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        informacionDevueltoTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element('#form-enviar-email-desacuerdo-ticket').hide();
                angular.element("#modal-comentario-ticket-rechazado").modal('hide');
                angular.element("#modal-vista-ticket-user").modal('hide');
                $scope.mostrarTodosSeguimientos();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    

    //ABRIR MODAL DE CALIFICAR
    $scope.abrirModalCalificar = function () {
    
        var ajaxEnvioDatos = $http.post("clientes/preguntas-calificar-ticket",
            {
                idticket: $scope.idTicket
            });
       
        
        ajaxEnvioDatos.success(function (data) {
            if (data.success === true) {
                $scope.preguntasCatalogo = data.preguntas;

                setTimeout(function () {
                    var filas = angular.element(".nivel-star-calificar");
                    angular.forEach(filas, function (fila, key) {
                        var nivelFila = angular.element(fila).attr('data-calificacion');
                        var estrellasFila = angular.element(fila).children();

                        for (var i = nivelFila; i < 5; i++) {
                            angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                            angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                        }
                    });

                }, 100);

                angular.element("#modal-calificar-ticket").modal('show');
            }
            else {
                messageDialog.show(data.msg);
            }
        });
    };

    //Guardar calificacion de Ticket
    angular.element("#tabla-calificacion-ticket-cliente").on('click', '.level-star', function () {
        var calificacion = angular.element(this).index() + 1;
        var tdEstrellas = angular.element(this).parent();
        var idPregunta = angular.element(tdEstrellas).attr('data-id-calificacion');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerCalificacion = $http.post("clientes/guardar-calificacion-ticket/", {
            idPregunta: idPregunta,
            idticket: $scope.idTicket,
            calificacion: calificacion
        });
        ajaxEstablecerCalificacion.success(function (data) {
            if (data.success === true) {
                for (var i = 0; i < 5; i++) {
                    if (i < calificacion) {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star-empty');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star');
                    }
                    else {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                    }
                }
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

    angular.element("#tabla-calificacion-ticket-cliente").on('click', '.level-star-eliminar', function () {
        var calificacion = 0;
        var tdEstrellas = angular.element(this).parent().prev();

        var idPregunta = angular.element(tdEstrellas).attr('data-id-calificacion');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerCalificacion = $http.post("clientes/guardar-calificacion-ticket/", {
            idPregunta: idPregunta,
            idticket: $scope.idTicket,
            calificacion: calificacion
        });
        ajaxEstablecerCalificacion.success(function (data) {
            if (data.success === true) {
                angular.element(estrellasFila).removeClass('glyphicon-star');
                angular.element(estrellasFila).addClass('glyphicon-star-empty');
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });
}]);