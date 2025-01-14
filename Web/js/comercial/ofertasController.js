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

        var ajaxRequerimientos = $http.post("comercial/requerimientos_ofertas", {});
        ajaxRequerimientos.success(function (data) {
            if (data.success === true) {
                $scope.ofertaRequerimientos = data.requerimientosComercial;
            }
        });
    };
    $scope.cargarCatalogos();

    function convertirFecha1(fechaStr) {
        // Extraer los componentes de la fecha usando una expresión regular
        var match = fechaStr.match(/(\w{3}) (\d{1,2}) (\d{4}) (\d{1,2}):(\d{2})(AM|PM)/);
        if (!match) return null; // Si no coincide el formato, devolver null

        var meses = { Jan: 0, Feb: 1, Mar: 2, Apr: 3, May: 4, Jun: 5, Jul: 6, Aug: 7, Sep: 8, Oct: 9, Nov: 10, Dec: 11 };

        var mes = meses[match[1]];          // Convertir el mes a número (0-11)
        var dia = parseInt(match[2], 10);   // Día del mes
        var anio = parseInt(match[3], 10);  // Año
        var hora = parseInt(match[4], 10);  // Hora
        var minuto = parseInt(match[5], 10); // Minuto
        var ampm = match[6];                // AM o PM

        if (ampm === "PM" && hora < 12) hora += 12;
        if (ampm === "AM" && hora === 12) hora = 0;

        return new Date(anio, mes, dia, hora, minuto);
    }

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

                console.log(data.ofertasComercial);
                function convertirFecha(fechaStr) {
                    if (fechaStr && fechaStr.startsWith('/Date(')) {
                        var milisegundos = parseInt(fechaStr.match(/\d+/)[0], 10);
                        return new Date(milisegundos);
                    }
                    return null; // Devolver null si la fecha es inválida o vacía
                }

                $scope.ofertasLista = data.ofertasComercial.map(function (oferta) {
                    return {
                        ...oferta,
                        fechaEstimacion: convertirFecha1(oferta.fechaEstimacion),
                        fechaGeneracion: convertirFecha1(oferta.fechaGeneracion),
                        fechaRevision: convertirFecha1(oferta.fechaRevision),
                        fechaAprobacionGerencia: convertirFecha1(oferta.fechaAprobacionGerencia),
                        fechaEnvioOferta: convertirFecha1(oferta.fechaEnvioOferta),
                        fechaVencimiento: convertirFecha1(oferta.fechaVencimiento)
                    };
                });



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
    $scope.editarOferta = function (codigo) {
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
                    $scope.ofertaSeleccionada = {
                        fechaGeneracion: convertirFecha(data.oferta.fechaGeneracion),
                        codigo: data.oferta.codigo,
                        fechaEstimacion: convertirFecha(data.oferta.fechaEstimacion),
                        fechaRevision: convertirFecha(data.oferta.fechaRevision),
                        fechaAprobacionGerencia: convertirFecha(data.oferta.fechaAprobacionGerencia),
                        fechaEnvioOferta: convertirFecha(data.oferta.fechaEnvioOferta),
                        fechaVencimiento: convertirFecha(data.oferta.fechaVencimiento),
                        OfertaRequerimiento: data.oferta.OfertaRequerimiento
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

    // Configurar datepicker
    angular.element('#fecha-oferta').datepicker({
        format: 'dd/mm/yyyy',
        locale: 'es'
    });
}]);