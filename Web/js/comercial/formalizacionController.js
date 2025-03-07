comercialApp.controller('formalizacionController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'FORMALIZACION';
    $scope.rutaImages = "Web/images/";

    $scope.esTicket = false;
    // Variables para manejo de ofertas
    $scope.formalizacion = {};
    $scope.formalizacionEditar = {};
    var numerosPorPagina = 10;
    var pagina = 1;

    // Cargar catálogos iniciales
    $scope.cargarCatalogos = function () {

        var ajaxOfertaOferta = $http.post("comercial/dar-oferta-oferta", {});
        ajaxOfertaOferta.success(function (data) {
            if (data.success === true) {
                $scope.ofertas = data.ofertasComercial;
            }
        });

        var ajaxEstadosOferta = $http.post("comercial/estado-oferta-comercial", {});
        ajaxEstadosOferta.success(function (data) {
            if (data.success === true) {
                $scope.estadosOferta = data.estadosOferta;
            }
        });
    };
    $scope.cargarCatalogos();

    // Función para cargar listado de ofertas
    $scope.cargarFormalizacionOfertas = function (start, lenght) {
        if (start === undefined) start = 0;
        if (lenght === undefined) lenght = numerosPorPagina;

        $http.post("comercial/dar-formalizacion-comercial", {
            start: start,
            lenght: lenght,
            filtro: $scope.filtroFormalizarOfertas
        }).success(function (data) {
            if (data.success === true) {
                $scope.formalizacionLista = data.formalizacionLista;
                $scope.cantPaginas = Math.ceil(data.total / numerosPorPagina);
                $scope.cantPaginas = $scope.cantPaginas || 1;

                // Lógica de paginación similar al script original
                $scope.configurarPaginacion(pagina);
            } else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarFormalizacionOfertas();

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
        $scope.cargarFormalizacionOfertas((pag - 1) * numerosPorPagina, numerosPorPagina);
    };

    // Abrir modal para nueva oferta
    $scope.formalizarOferta = function () {
        $http.post("comercial/generar-formalizacion-oferta", {}).success(function (data) {
            if (data.success === true) {
                var newCodigo = data.nuevoCodigo;
                console.log(data.nuevoCodigo);

                $scope.esTicket = false;
                // Limpiar campos y asignar el nuevo código cuando se reciba la respuesta
                $scope.formalizacion = {
                    noContrato: newCodigo,
                    secuencialOferta: '',
                    estadoOferta: '',
                    fechaAprobacionOferta: '',
                    valorAnticipo: '',
                    valor: '',
                    fechaFacturacionAnticipo: '',
                    valorEntrega: '',
                    ordenFacturacionEntrega: '',
                    valorFacturacionProgramada: '',
                    ordenFacturacionProgramada: '',
                    formalizado: 0,
                    seguimientoFormalizacion: ''
                };

                // Mostrar el modal solo después de asignar el código
                angular.element("#modal-nueva-formalizacion").modal("show");
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
    $scope.verFormalizacion = function (secuencial) {
        console.log(secuencial);
        $http.post("comercial/dar-datos-formalizacion", { secuencial: secuencial })
            .success(function (data) {
                if (data.success === true) {
                    function convertirFecha(fechaStr) {
                        if (fechaStr) {
                            // Extraer los milisegundos entre paréntesis usando una expresión regular
                            var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                            return new Date(milisegundos);
                        }
                        return null;
                    }

                    // Crear el objeto con las fechas convertidas
                    $scope.formalizacionEditar = {
                        secuencial: data.formalizacion.secuencial,
                        noContrato: data.formalizacion.noContrato,
                        valor: data.formalizacion.valor,
                        secuencialOferta: data.formalizacion.ofertaSecuencial,
                        estadoOferta: data.formalizacion.estadoSecuencial,
                        fechaAprobacionOferta: convertirFecha(data.formalizacion.fechaAprobacionOferta),
                        valorAnticipo: data.formalizacion.valorAnticipo,
                        fechaFacturacionAnticipo: convertirFecha(data.formalizacion.fechaFacturacionAnticipo),
                        valorEntrega: data.formalizacion.valorEntrega,
                        ordenFacturacionEntrega: data.formalizacion.ordenFacturacionEntrega,
                        valorFacturacionProgramada: data.formalizacion.valorFacturacionProgramada,
                        ordenFacturacionProgramada: data.formalizacion.ordenFacturacionProgramada,
                        formalizado: data.formalizacion.fomalizacionContrato == 1 ? true : false,
                        seguimientoFormalizacion: data.formalizacion.seguimientoContrato
                    };
                    console.log($scope.formalizacionEditar);
                    angular.element("#modal-editar-formalizacion").modal("show");
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                messageDialog.show('Error', 'No se pudo obtener los datos de la oferta');
            });
    };

    // Método para borrar oferta
    $scope.borrarFormalizacion = function (secuencial) {
        // Mostrar confirmación antes de borrar
        var confirmacion = confirm('¿Está seguro que desea eliminar esta formalización?');

        if (confirmacion) {
            waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'danger' });

            $http.post("comercial/eliminar-formalizacion", { secuencial: secuencial })
                .success(function (data) {
                    waitingDialog.hide();
                    if (data.success === true) {
                        // Recargar la lista de ofertas
                        $scope.cargarFormalizacionOfertas();
                        messageDialog.show('Éxito', 'Formalización eliminada correctamente');
                    } else {
                        messageDialog.show('Información', data.msg);
                    }
                })
                .error(function () {
                    waitingDialog.hide();
                    messageDialog.show('Error', 'No se pudo eliminar la formalización');
                });
        }
    };

    // Modificar el método de guardar para manejar tanto nuevas ofertas como ediciones
    $scope.guardarFormalizacion = function () {
        console.log($scope.formalizacion);
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        console.log("guardando formalizacion");
        $http.post("comercial/guardar-formalizacion", {
            valor: $scope.formalizacion.valor,
            estadoSecuencial: $scope.formalizacion.estadoOferta,
            secuencialOferta: $scope.formalizacion.secuencialOferta,
            fechaAprobacionOferta: $scope.formalizacion.fechaAprobacionOferta,
            noContrato: $scope.formalizacion.noContrato,
            formalizacionContrato: $scope.formalizacion.formalizado,
            seguimientoContrato: $scope.formalizacion.seguimientoFormalizacion,
            valorAnticipo: $scope.formalizacion.valorAnticipo,
            fechaFacturacionAnticipo: $scope.formalizacion.fechaFacturacionAnticipo,
            ordenFacturacionAnticipo: $scope.formalizacion.ordenFacturacionAnticipo,
            valorEntrega: $scope.formalizacion.valorEntrega,
            ordenFacturacionEntrega: $scope.formalizacion.ordenFacturacionEntrega,
            valorFacturacionProgramada: $scope.formalizacion.valorFacturacionProgramada,
            ordenFacturacionProgramada: $scope.formalizacion.ordenFacturacionProgramada
        })
            .success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-nueva-formalizacion").modal("hide");
                    messageDialog.show('Éxito', 'Formalización guardada correctamente');
                    $scope.cargarFormalizacionOfertas();
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                waitingDialog.hide();
                messageDialog.show('Error', 'Error al guardar la formalización');
            });
    };
    $scope.editarFormalizacion = function () {
        console.log($scope.formalizacionEditar);
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        console.log("editando");
        $http.post("comercial/editar-formalizacion", {
            secuencial: $scope.formalizacionEditar.secuencial,
            valor: $scope.formalizacionEditar.valor,
            estadoSecuencial: $scope.formalizacionEditar.estadoOferta,
            secuencialOferta: $scope.formalizacionEditar.secuencialOferta,
            fechaAprobacionOferta: $scope.formalizacionEditar.fechaAprobacionOferta,
            noContrato: $scope.formalizacionEditar.noContrato,
            formalizacionContrato: $scope.formalizacionEditar.formalizado,
            seguimientoContrato: $scope.formalizacionEditar.seguimientoFormalizacion,
            valorAnticipo: $scope.formalizacionEditar.valorAnticipo,
            fechaFacturacionAnticipo: $scope.formalizacionEditar.fechaFacturacionAnticipo,
            ordenFacturacionAnticipo: $scope.formalizacionEditar.ordenFacturacionAnticipo,
            valorEntrega: $scope.formalizacionEditar.valorEntrega,
            ordenFacturacionEntrega: $scope.formalizacionEditar.ordenFacturacionEntrega,
            valorFacturacionProgramada: $scope.formalizacionEditar.valorFacturacionProgramada,
            ordenFacturacionProgramada: $scope.formalizacionEditar.ordenFacturacionProgramada
        })
            .success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-editar-formalizacion").modal("hide");
                    $scope.cargarFormalizacionOfertas();
                    messageDialog.show('Éxito', 'Formalización guardada correctamente');
                } else {
                    messageDialog.show('Información', data.msg);
                }
            })
            .error(function () {
                waitingDialog.hide();
                messageDialog.show('Error', 'Error al guardar la formalización');
            });
    };

    // Configurar datepicker
    //angular.element('#fecha-oferta').datepicker({
    //    format: 'dd/mm/yyyy',
    //    locale: 'es'
    //});
 }]);