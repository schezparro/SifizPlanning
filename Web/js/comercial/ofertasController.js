comercialApp.controller('ofertasController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'OFERTAS';
    $scope.rutaImages = "Web/images/";

    // Variables para manejo de ofertas
    $scope.ofertaSeleccionada = {};
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

        // Cargar catálogo de tipos de oferta
        $http.post("comercial/catalogo-tipos-oferta", {}).success(function (data) {
            if (data.success === true) {
                $scope.tiposOferta = data.tiposOferta;
            }
        });
    };
    $scope.cargarCatalogos();

    // Función para cargar listado de ofertas
    $scope.cargarOfertas = function (start, lenght) {
        if (start === undefined) start = 0;
        if (lenght === undefined) lenght = numerosPorPagina;

        $http.post("comercial/listar-ofertas", {
            start: start,
            lenght: lenght,
            filtro: $scope.filtroOfertas
        }).success(function (data) {
            if (data.success === true) {
                $scope.ofertasLista = data.ofertas;
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
        // Generar código de oferta automáticamente
        $scope.generarCodigoOferta();

        // Limpiar campos
        $scope.ofertaSeleccionada = {
            fechaGeneracion: new Date(),
            cliente: '',
            tipoOferta: '',
            observaciones: ''
        };

        angular.element("#modal-nueva-oferta").modal("show");
    };

    // Generar código de oferta automáticamente
    $scope.generarCodigoOferta = function () {
        $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
            if (data.success === true) {
                $scope.ofertaSeleccionada.codigo = data.codigoOferta;
            }
        });
    };

    // Método para editar oferta
    $scope.editarOferta = function (codigoOferta) {
        $http.post("comercial/obtener-datos-oferta", { codigo: codigoOferta })
            .success(function (data) {
                if (data.success === true) {
                    // Convertir fechas de milisegundos a objetos Date
                    $scope.ofertaSeleccionada = {
                        codigo: data.oferta.codigo,
                        fechaGeneracion: new Date(data.oferta.fechaGeneracion),
                        fechaAprobacion: data.oferta.fechaAprobacion ? new Date(data.oferta.fechaAprobacion) : null,
                        fechaEnvio: data.oferta.fechaEnvio ? new Date(data.oferta.fechaEnvio) : null,
                        cliente: data.oferta.cliente,
                        tipoOferta: data.oferta.tipoOferta,
                        observaciones: data.oferta.observaciones
                    };
                    angular.element("#modal-nueva-oferta").modal("show");
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
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        $http.post("comercial/guardar-oferta", $scope.ofertaSeleccionada)
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

    // Configurar datepicker
    angular.element('#fecha-oferta').datepicker({
        format: 'dd/mm/yyyy',
        locale: 'es'
    });
}]);