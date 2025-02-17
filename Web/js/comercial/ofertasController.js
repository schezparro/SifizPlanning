comercialApp.controller('ofertasController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'OFERTAS';
    $scope.rutaImages = "Web/images/";

    $scope.esTicket = false;
    // Variables para manejo de ofertas
    $scope.ofertaSeleccionada = {};
    $scope.ofertaSeleccionadaEditar = {};
    var numerosPorPagina = 10;
    var pagina = 1;

    // Cargar catálogos iniciales
    $scope.cargarCatalogos = function () {
        // Cargar catálogo de clientes
        $http.post("catalogos/clientes", {}).success(function (data) {
            if (data.success === true) {
                $scope.clientes = data.clientes;
            }
        });

        var ajaxRequerimientos = $http.post("comercial/requerimientos_ofertas", {});
        ajaxRequerimientos.success(function (data) {
            if (data.success === true) {
                $scope.ofertaRequerimientos = data.requerimientosComercial;
            }
        });
    };
    $scope.cargarCatalogos();

    // Función para cargar listado de ofertas
    $scope.cargarOfertas = function (start, lenght) {
        if (start === undefined) start = 0;
        if (lenght === undefined) lenght = numerosPorPagina;

        $http.post("comercial/dar-ofertas-comercial", {
            start: start,
            lenght: lenght,
            filtro: $scope.filtroOfertas
        }).success(function (data) {
            if (data.success === true) {
                $scope.ofertasLista = data.ofertasComercial;
                $scope.cantPaginas = Math.ceil(data.total / numerosPorPagina);
                $scope.cantPaginas = $scope.cantPaginas || 1;

                // Lógica de paginación similar al script original
                $scope.configurarPaginacion(pagina);
            } else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarOfertas();

    // Configurar paginación (similar al script original)
    $scope.configurarPaginacion = function (paginaActual) {
        $scope.listaPaginas = [];
        if ($scope.cantPaginas > 5 && paginaActual <= 5) {
            for (var i = 1; i <= 5; i++) {
                $scope.listaPaginas.push(i);
            }
        } else if ($scope.cantPaginas <= 5) {
            for (var i = 1; i <= $scope.cantPaginas; i++) {
                $scope.listaPaginas.push(i);
            }
        } else if ($scope.cantPaginas > 5) {
            for (var i = Math.max(1, paginaActual - 4); i <= paginaActual; i++) {
                $scope.listaPaginas.push(i);
            }
        }
    };

    // Métodos de navegación de página
    $scope.cambiarPagina = function (pag) {
        pagina = pag;
        $scope.cargarOfertas((pag - 1) * numerosPorPagina, numerosPorPagina);
    };

    // Abrir modal para nueva oferta
    $scope.abrirNuevaOferta = function () {
        $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
            if (data.success === true) {
                $scope.newCodigo = data.nuevoCodigo;
                console.log(data.nuevoCodigo);

                $scope.esTicket = false;
                // Limpiar campos y asignar el nuevo código cuando se reciba la respuesta
                $scope.ofertaSeleccionada = {
                    fechaGeneracion: new Date(),
                    codigo: $scope.newCodigo,
                    fechaEstimacion: '',
                    fechaRevision: '',
                    fechaAprobacionGerencia: '',
                    fechaEnvioOferta: '',
                    fechaVencimiento: ''
                };

                // Mostrar el modal solo después de asignar el código
                angular.element("#modal-nueva-oferta").modal("show");
            }
        });
    };

    // Generar código de oferta automáticamente
    $scope.generarCodigoOferta = function () {
        $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
            if (data.success === true) {
                $scope.newCodigo = data.nuevoCodigo;
                console.log(data.nuevoCodigo);
            }
        });
    };

    // Método para editar oferta
    $scope.verOferta = function (codigo) {
        console.log(codigo);
        $http.post("comercial/dar-datos-oferta", { codigo: codigo })
            .success(function (data) {
                if (data.success === true) {
                    // Convertir fechas de milisegundos a objetos Date
                    console.log(data.oferta);
                    function convertirFecha(fechaStr) {
                        if (fechaStr) {
                            // Extraer los milisegundos entre paréntesis usando una expresión regular
                            var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                            return new Date(milisegundos);
                        }
                        return null;
                    }

                    // Crear el objeto con las fechas convertidas
                    $scope.ofertaSeleccionadaEditar = {
                        fechaGeneracion: convertirFecha(data.oferta.fechaGeneracion),
                        codigo: data.oferta.codigo,
                        fechaEstimacion: convertirFecha(data.oferta.fechaEstimacion),
                        fechaRevision: convertirFecha(data.oferta.fechaRevision),
                        fechaAprobacionGerencia: convertirFecha(data.oferta.fechaAprobacionGerencia),
                        fechaEnvioOferta: convertirFecha(data.oferta.fechaEnvioOferta),
                        fechaVencimiento: convertirFecha(data.oferta.fechaVencimiento),
                        OfertaRequerimiento: data.oferta.OfertaRequerimiento
                    };

                    angular.element("#modal-editar-oferta").modal("show");
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                messageDialog.show('Error', 'No se pudo obtener los datos de la oferta');
            });
    };

    // Método para borrar oferta
    $scope.borrarOferta = function (codigoOferta) {
        // Mostrar confirmación antes de borrar
        var confirmacion = confirm('¿Está seguro que desea eliminar esta oferta?');

        if (confirmacion) {
            waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'danger' });

            $http.post("comercial/eliminar-oferta", { codigo: codigoOferta })
                .success(function (data) {
                    waitingDialog.hide();
                    if (data.success === true) {
                        // Recargar la lista de ofertas
                        $scope.cargarOfertas();
                        messageDialog.show('Éxito', 'Oferta eliminada correctamente');
                    } else {
                        messageDialog.show('Información', data.msg);
                    }
                })
                .error(function () {
                    waitingDialog.hide();
                    messageDialog.show('Error', 'No se pudo eliminar la oferta');
                });
        }
    };

    // Modificar el método de guardar para manejar tanto nuevas ofertas como ediciones
    $scope.guardarOferta = function () {
        console.log($scope.ofertaSeleccionada);
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        $http.post("comercial/guardar-oferta", {
            OfertaRequerimiento: $scope.ofertaSeleccionada.OfertaRequerimiento,
            codigo: $scope.ofertaSeleccionada.codigo,
            fechaEstimacion: $scope.ofertaSeleccionada.fechaEstimacion,
            fechaRevision: $scope.ofertaSeleccionada.fechaRevision,
            fechaEnvioOferta: $scope.ofertaSeleccionada.fechaEnvioOferta,
            fechaGeneracion: $scope.ofertaSeleccionada.fechaGeneracion,
            fechaAprobacionGerencia: $scope.ofertaSeleccionada.fechaAprobacionGerencia,
            fechaVencimiento: $scope.ofertaSeleccionada.fechaVencimiento,
        })
            .success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-nueva-oferta").modal("hide");
                    $scope.cargarOfertas();
                    messageDialog.show('Éxito', 'Oferta guardada correctamente');
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                waitingDialog.hide();
                messageDialog.show('Error', 'Error al guardar la oferta');
            });
    };

    // Modificar el método de guardar para manejar tanto nuevas ofertas como ediciones
    $scope.guardarOfertaEditada = function () {
        console.log($scope.ofertaSeleccionadaEditar);
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        $http.post("comercial/guardar-oferta", {
            OfertaRequerimiento: $scope.ofertaSeleccionadaEditar.OfertaRequerimiento,
            codigo: $scope.ofertaSeleccionadaEditar.codigo,
            fechaEstimacion: $scope.ofertaSeleccionadaEditar.fechaEstimacion,
            fechaRevision: $scope.ofertaSeleccionadaEditar.fechaRevision,
            fechaEnvioOferta: $scope.ofertaSeleccionadaEditar.fechaEnvioOferta,
            fechaGeneracion: $scope.ofertaSeleccionadaEditar.fechaGeneracion,
            fechaAprobacionGerencia: $scope.ofertaSeleccionadaEditar.fechaAprobacionGerencia,
            fechaVencimiento: $scope.ofertaSeleccionadaEditar.fechaVencimiento,
        })
            .success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-nueva-oferta").modal("hide");
                    $scope.cargarOfertas();
                    messageDialog.show('Éxito', 'Oferta guardada correctamente');
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                waitingDialog.hide();
                messageDialog.show('Error', 'Error al guardar la oferta');
            });
    };

    $scope.editarOferta = function () {
        console.log($scope.ofertaSeleccionadaEditar);
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        $http.post("comercial/editar-oferta", {
            OfertaRequerimiento: $scope.ofertaSeleccionadaEditar.OfertaRequerimiento,
            codigo: $scope.ofertaSeleccionadaEditar.codigo,
            fechaEstimacion: $scope.ofertaSeleccionadaEditar.fechaEstimacion,
            fechaRevision: $scope.ofertaSeleccionadaEditar.fechaRevision,
            fechaEnvioOferta: $scope.ofertaSeleccionadaEditar.fechaEnvioOferta,
            fechaGeneracion: $scope.ofertaSeleccionadaEditar.fechaGeneracion,
            fechaAprobacionGerencia: $scope.ofertaSeleccionadaEditar.fechaAprobacionGerencia,
            fechaVencimiento: $scope.ofertaSeleccionadaEditar.fechaVencimiento,
        })
            .success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-nueva-oferta").modal("hide");
                    $scope.cargarOfertas();
                    messageDialog.show('Éxito', 'Oferta guardada correctamente');
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                waitingDialog.hide();
                messageDialog.show('Error', 'Error al guardar la oferta');
            });
    };

    $scope.toggleTicketFields = function () {
        if ($scope.esTicket) {
            $scope.ofertaSeleccionada.fechaEstimacion = null;
            $scope.ofertaSeleccionada.fechaRevision = null;
            $scope.ofertaSeleccionada.fechaAprobacionGerencia = null;
            $scope.ofertaSeleccionada.fechaEnvioOferta = null;
        }
    };

    $scope.toggleTicketFieldsEditar = function () {
        if ($scope.esTicketEditar) {
            $scope.ofertaSeleccionadaEditar.fechaEstimacion = null;
            $scope.ofertaSeleccionadaEditar.fechaRevision = null;
            $scope.ofertaSeleccionadaEditar.fechaAprobacionGerencia = null;
            $scope.ofertaSeleccionadaEditar.fechaEnvioOferta = null;
        }
    };

    $scope.buscarDatosTicket = function (ticket) {
        waitingDialog.show('Validando el número de ticket ingresado...', { dialogSize: 'sm', progressType: 'success' });
        if (!ticket || isNaN(ticket)) {
            $scope.ticketValido = false;
            return;
        }

        var ajaxObtenerTicket = $http.post("tickets/dar-datos-ticket-ofertas",
            {
                idTicket: ticket
            });

        ajaxObtenerTicket.success(function (data) {
            if (data.success === true) {
                waitingDialog.hide();

                console.log(data.datosTicket);

                function convertirFecha(fechaStr) {
                    if (fechaStr) {
                        // Extraer los milisegundos entre paréntesis usando una expresión regular
                        var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                        return new Date(milisegundos);
                    }
                    return null;
                }

                if (data.datosTicket.FechaRecepcionEstimacion) {
                    $scope.ofertaSeleccionada.fechaEstimacion = convertirFecha(data.datosTicket.FechaRecepcionEstimacion);
                } else {
                    $scope.ofertaSeleccionada.fechaEstimacion = "";
                }

                if (data.datosTicket.FechaEnvioRevision) {
                    $scope.ofertaSeleccionada.fechaRevision = convertirFecha(data.datosTicket.FechaEnvioRevision);
                } else {
                    $scope.ofertaSeleccionada.fechaRevision = "";
                }

                if (data.datosTicket.FechaAprobacionGerencia) {
                    $scope.ofertaSeleccionada.fechaAprobacionGerencia = convertirFecha(data.datosTicket.FechaAprobacionGerencia);
                } else {
                    $scope.ofertaSeleccionada.fechaAprobacionGerencia = "";
                }

                if (data.datosTicket.FechaEnvioOfertaCliente) {
                    $scope.ofertaSeleccionada.fechaEnvioOferta = convertirFecha(data.datosTicket.FechaEnvioOfertaCliente);
                } else {
                    $scope.ofertaSeleccionada.fechaEnvioOferta = "";
                }

            } else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };

    // Configurar datepicker
    angular.element('#fecha-oferta').datepicker({
        format: 'dd/mm/yyyy',
        locale: 'es'
    });
    angular.element('#fechaActa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fechaActa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fechaActa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fechaActa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fechaActa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
}]);