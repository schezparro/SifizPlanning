devApp.controller('estimacionesController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.idEstimacion = 0;
    $scope.mostrarTodasEstimaciones = false;

    $scope.cargarEstimaciones = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        $scope.loading.show();
        var estimaciones = $http.post("user/estimaciones-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroEstimaciones,
                todos: $scope.mostrarTodasEstimaciones
            });
        estimaciones.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;
                $scope.estimaciones = data.estimaciones;
                $scope.cantPaginas = Math.ceil(data.total / numerosPorPagina);
                if ($scope.cantPaginas === 0) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas < 5) {
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
                    var listaPaginador = angular.element("#tabla-estimacion-ticket .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarEstimaciones();

    //Cargando los niveles de un colaborador
    var niveles = $http.post("user/niveles-colaborador",
        {});
    niveles.success(function (data) {
        $scope.loading.hide();
        if (data.success === true) {
            $scope.niveles = data.niveles
        }
        else {
            messageDialog.show('Información', data.msg);
        }
    });

    //El Paginador
    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.cargarEstimaciones(start, lenght);
    };

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

    //Mostrar el windows de estimacion
    $scope.mostrarWindowEstimacion = function (id) {
        $scope.idEstimacion = id;
        var estimacion = $http.post("user/estimacion-usuario",
            {
                idEstimacion: id
            });
        estimacion.success(function (data) {

            if (data.success === true) {
                datosInicialesEstimacion();
                $scope.detallesEstimacion = data.detallesEstimacion;
                $scope.tiempoTotal = data.tiempoTotal;
                $scope.entregablesAdicionales = data.entregables;
                $scope.informacionComplementaria = data.informacion;

                $scope.requerimientoNuevo = data.requerimientoNuevo;
                $scope.tiempoInicial = data.tiempoInicial;
                $scope.tiempoPegado = data.tiempoPegado;

                if (data.estimacionTerminada === 1) {
                    $scope.edicionEstimacion = true;
                    angular.element('#frm-modal-estimar-ticket button').prop('disabled', true);
                    angular.element('#frm-modal-estimar-ticket textarea').prop('disabled', true);
                    angular.element('#frm-modal-estimar-ticket select').prop('disabled', true);
                    angular.element('#frm-modal-estimar-ticket input').prop('disabled', true);
                    angular.element('#frm-modal-estimar-ticket button[data-dismiss="modal"]').prop('disabled', false);
                    angular.element('#editarEstimacionTicket').prop('disabled', false);

                }
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        if ($scope.requerimientoNuevo === true) {
            angular.element('#div-tiempo-incial').css('display', 'none');
            angular.element('#div-tiempo-pegado').css('display', 'block');
        } else {
            angular.element('#div-tiempo-incial').css('display', 'block');
            angular.element('#div-tiempo-pegado').css('display', 'none');
        }

        $scope.estimacionIdTmp = id;
        $scope.cargarItemsEspeciales(id);
        $scope.cargarItemsEspecialesCatalogo();

        angular.element("#modal-estimacion-dev-ticket").modal('show');
    };

    $scope.changeRequerimientoNuevo = function () {

        if ($scope.requerimientoNuevo === true) {
            angular.element('#div-tiempo-incial').css('display', 'none');
            angular.element('#div-tiempo-pegado').css('display', 'block');
        } else {
            angular.element('#div-tiempo-incial').css('display', 'block');
            angular.element('#div-tiempo-pegado').css('display', 'none');
        }
    };


    //Mostrar el windows de estimacioneditarEntregable
    $scope.editarEntregable = function (id) {
        var estimacion = $http.post("user/edcion-estimacion-usuario",
            {
                idEntregableEstimacion: id
            });
        estimacion.success(function (data) {
            if (data.success === true) {
                datosInicialesEstimacionEdicion();

                $scope.secuencialEdicionEntregable = data.entregable.id;
                $scope.nombreEdicionEntregable = data.entregable.nombre;
                var detalles = data.detalles;
                var divDetalles = angular.element('#edicion-lista-detalles-tareas');
                var hijosDivDetalles = angular.element(divDetalles).children();
                for (i = 0; i < detalles.length; i++) {
                    var detalle = detalles[i];
                    var texto = detalle.detalle;
                    var tiempoDesarrollo = detalle.tiempoDesarrollo;
                    var tiempoPrueba = detalle.tiempoPrueba;
                    var tiempoQA = detalle.tiempoQA;
                    var nivel = detalle.nivel;

                    var hijo = undefined;
                    if (i === 0) {
                        hijo = hijosDivDetalles[0];
                    }
                    else {//Adicionar una nueva fila
                        $scope.adicionarDetalleEdicion();
                        hijo = angular.element(divDetalles).children().last();
                    }

                    angular.element(hijo).find('textarea').val(texto);
                    angular.element(hijo).find('input').eq(0).val(tiempoDesarrollo);
                    angular.element(hijo).find('input').eq(1).val(tiempoPrueba);
                    angular.element(hijo).find('input').eq(2).val(tiempoQA);
                    angular.element(hijo).find('select').val(nivel);
                }
                angular.element("#modal-edicion-entregable-ticket").modal('show');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };

    $scope.editarEstimacion = function () {
        $scope.edicionEstimacion = false;
        angular.element('#frm-modal-estimar-ticket button').prop('disabled', false);
        angular.element('#frm-modal-estimar-ticket textarea').prop('disabled', false);
        angular.element('#frm-modal-estimar-ticket select').prop('disabled', false);
        angular.element('#frm-modal-estimar-ticket input').prop('disabled', false);

        if ($scope.itemsEstimacion.length > 0 || $scope.itemsAdicionales.length > 0) {
            angular.element("#add-items-especiales").prop("disabled", true);
        } else {
            angular.element("#add-items-especiales").prop("disabled", false);
        }
    };

    //Adicionar Detalle de ticket
    $scope.adicionarDetalle = function () {
        angular.element("#lista-detalles-tareas").append(angular.element("#add-informacion-estimacion").html());
    };

    //Adicionar Detalle de ticket
    $scope.adicionarDetalleEdicion = function () {
        angular.element("#edicion-lista-detalles-tareas").append(angular.element("#add-informacion-estimacion").html());
    };

    $scope.eliminarDetalleEdicion = function () {
        var divHijos = angular.element("#edicion-lista-detalles-tareas").children();
        if (divHijos.length > 1) {
            angular.element(divHijos).last().remove();
        }
    };

    //Cuando se cierra el windows de las estimaciones
    angular.element("#modal-estimacion-dev-ticket").on('hidden.bs.modal', function (e) {
        datosInicialesEstimacion();
        $scope.cargarEstimaciones();
    });

    function datosInicialesEstimacion() {
        $scope.edicionEstimacion = false;
        $scope.entregablesAdicionales = "";
        $scope.informacionComplementaria = "";

        angular.element("#lista-detalles-tareas").html("");
        angular.element("#lista-detalles-tareas").append(angular.element("#add-informacion-estimacion-inicial").html());

        angular.element('#frm-modal-estimar-ticket button').prop('disabled', false);
        angular.element('#frm-modal-estimar-ticket textarea').prop('disabled', false);
        angular.element('#frm-modal-estimar-ticket select').prop('disabled', false);
        angular.element('#frm-modal-estimar-ticket input').prop('disabled', false);
    }

    function datosInicialesEstimacionEdicion() {
        angular.element("#edicion-lista-detalles-tareas").html("");
        angular.element("#edicion-lista-detalles-tareas").append(angular.element("#add-informacion-estimacion-inicial").html());
    }

    //Estimar el desarrollo del ticket
    $scope.estimarDesarrolloTicket = function () {
        waitingDialog.show('Enviando Estimación...', { dialogSize: 'sm', progressType: 'success' });

        if ($scope.requerimientoNuevo === true) {
            $scope.tiempoInicial = 0;
        } else {
            $scope.tiempoPegado = 0;
        }

        var estimacionesEnvio = $http.post("user/guardar-estimacion",
            {
                idEstimacion: $scope.idEstimacion,
                entregables: $scope.entregablesAdicionales,
                informacionComplementaria: $scope.informacionComplementaria,
                requerimientoNuevo: $scope.requerimientoNuevo,
                tiempoInicial: $scope.tiempoInicial,
                tiempoPegado: $scope.tiempoPegado
            });

        estimacionesEnvio.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-estimacion-dev-ticket").modal('hide');
                $scope.cargarEstimaciones();
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };

    //Guardar entregable por actividades del ticket
    $scope.guardarEntregable = function () {
        waitingDialog.show('Guardando Entregable...', { dialogSize: 'sm', progressType: 'success' });
        var arreglo = [];
        var detallesHijos = angular.element("#lista-detalles-tareas").children();
        angular.element.each(detallesHijos, function (key, divHijo) {
            var detalle = angular.element(divHijo).find('textarea').val();
            var tiempoDesarrollo = angular.element(divHijo).find('input').eq(0).val();
            var tiempoPrueba = angular.element(divHijo).find('input').eq(1).val();
            var tiempoQA = angular.element(divHijo).find('input').eq(2).val();
            var nivel = angular.element(divHijo).find('select').val();

            var datoDetalle = {
                detalle: detalle,
                tiempoEstimacion: '0',
                tiempoDesarrollo: tiempoDesarrollo,
                tiempoPrueba: tiempoPrueba,
                tiempoQA: tiempoQA,
                nivel: nivel
            };
            arreglo.push(datoDetalle);
        });

        var estimacionesEntregableEnvio = $http.post("user/guardar-entregable-estimacion",
            {
                idEstimacion: $scope.idEstimacion,
                entregable: $scope.nombreEntregable,
                jsonDetalles: angular.toJson(arreglo)
            });

        estimacionesEntregableEnvio.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-estimacion-entregable-ticket").modal('hide');
                $scope.mostrarWindowEstimacion($scope.idEstimacion);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };

    //Guardar entregable por actividades del ticket
    $scope.guardarEdicionEntregable = function () {
        waitingDialog.show('Guardando Entregable...', { dialogSize: 'sm', progressType: 'success' });
        var arreglo = [];
        var detallesHijos = angular.element("#edicion-lista-detalles-tareas").children();
        angular.element.each(detallesHijos, function (key, divHijo) {
            var detalle = angular.element(divHijo).find('textarea').val();
            var tiempoDesarrollo = angular.element(divHijo).find('input').eq(0).val();
            var tiempoPrueba = angular.element(divHijo).find('input').eq(1).val();
            var tiempoQA = angular.element(divHijo).find('input').eq(2).val();
            var nivel = angular.element(divHijo).find('select').val();

            var datoDetalle = {
                detalle: detalle,
                tiempoEstimacion: '0',
                tiempoDesarrollo: tiempoDesarrollo,
                tiempoPrueba: tiempoPrueba,
                tiempoQA: tiempoQA,
                nivel: nivel
            };
            arreglo.push(datoDetalle);
        });

        var estimacionesEntregableEnvio = $http.post("user/editar-entregable-estimacion",
            {
                idEstimacion: $scope.idEstimacion,
                idEntregable: $scope.secuencialEdicionEntregable,
                nombreEntregable: $scope.nombreEdicionEntregable,
                jsonDetalles: angular.toJson(arreglo)
            });

        estimacionesEntregableEnvio.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-edicion-entregable-ticket").modal('hide');
                $scope.mostrarWindowEstimacion($scope.idEstimacion);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };

    //Guardar entregable por actividades del ticket
    $scope.eliminarEntregable = function () {
        waitingDialog.show('Eliminando Entregable...', { dialogSize: 'sm', progressType: 'success' });

        var entregable = $http.post("user/eliminar-entregable-estimacion",
            {
                idEstimacion: $scope.idEstimacion,
                idEntregable: $scope.secuencialEdicionEntregable
            });

        entregable.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-edicion-entregable-ticket").modal('hide');
                $scope.mostrarWindowEstimacion($scope.idEstimacion);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };

    //Pantalla de información del ticket
    $scope.verInformacionTicket = function () {
        var informacionTicket = $http.post("user/datos-ticket/",
            {
                idEstimacion: $scope.idEstimacion
            });
        informacionTicket.success(function (data) {
            if (data.success === true) {
                $scope.$parent.idTicketHistorico = data.datosTicket.id;
                $scope.fechaTicket = data.datosTicket.fecha;
                $scope.nombreCliente = data.datosTicket.cliente;
                $scope.usuarioCliente = data.datosTicket.usuarioCliente;
                $scope.usuarioTelefono = data.datosTicket.clienteTelefono;
                $scope.estadoTicket = data.datosTicket.estado;
                $scope.prioridadTicket = data.datosTicket.prioridad;
                $scope.categoriaTicket = data.datosTicket.categoria;
                $scope.clienteReporta = data.datosTicket.reporto;
                $scope.telefonoCliente = data.datosTicket.telefono;
                $scope.reputacionCliente = data.reputacion;
                $scope.detalleTicket = data.datosTicket.detalle;

                $scope.adjuntosTicket = data.datosTicket.adjuntos;

                angular.element("#modal-vista-ticket").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Pantalla de información del ticket
    $scope.adicionarEntregable = function () {
        $scope.nombreEntregable = "";
        angular.element("#modal-estimacion-entregable-ticket").modal("show");
    };

    $scope.adicionarItemsEspeciales = function () {

        $scope.itemEspecialDescripcion = "";
        $scope.itemEspecialNivelColab = "";
        $scope.itemEspecialTiempoEstimacion = 0;

        $scope.itemEspecialesEstimacion = $scope.itemsEspecialesCatalogo.map(item => ({
            secuencialItemEspecialCatalogo: item.id,
            descripcion: item.descripcion,
            secuencialEstimacion: $scope.estimacionIdTmp,
            secuencialNivelColaborador: "",
            tiempoEstimacion: "",
            obligatorio: item.obligatorio === 1 ? true : false
        }));

        $scope.itemEspecialesEstimacionLength = $scope.itemEspecialesEstimacion.length;
        $scope.itemEspecialesAdicionales = [];
        angular.element("#modal-estimacion-items-ticket").modal("show");
    };

    $scope.nuevaFila = function () {

        var nuevoItem = {
            descripcion: "",
            secuencialEstimacion: $scope.estimacionIdTmp,
            secuencialNivelColaborador: "",
            tiempoEstimacion: "",
        };
        $scope.itemEspecialesAdicionales.push(nuevoItem);
    };

    $scope.nuevaFilaEdit = function () {

        var nuevoItem = {
            descripcion: "",
            secuencialEstimacion: $scope.estimacionIdTmp,
            idNivelColab: "",
            nivel: "",
            tiempoEstimacion: "",
        };
        $scope.itemsAdicionalesEdit.push(nuevoItem);
    };

    $scope.eliminarFila = function () {
        $scope.itemEspecialesAdicionales.pop();
    };

    $scope.eliminarFilaEdit = function () {
        $scope.itemsAdicionalesEdit.pop();
    };

    $scope.cargarItemsEspeciales = function (id) {
        var ajaxDarItemsEspeciales = $http.post("user/dar-items-especiales/", {
            idEstimacion: id
        });

        ajaxDarItemsEspeciales.success(function (data) {
            if (data.success === true) {

                $scope.itemsEstimacion = data.itemsEstimacion;
                $scope.itemsEstimacionLength = $scope.itemsEstimacion.length;
                $scope.itemsAdicionales = data.itemsAdicionales;

            } else {
                messageDialog.show('Información', data.msg);
            }

        });
    };

    $scope.cargarItemsEspecialesCatalogo = function (id) {
        var ajaxDarItemsEspecialesCatalogo = $http.post("user/dar-items-especiales-catalogo/", {});

        ajaxDarItemsEspecialesCatalogo.success(function (data) {
            if (data.success === true) {
                $scope.itemsEspecialesCatalogo = data.items;
            } else {
                messageDialog.show('Información', data.msg);
            }

        });
    };

    $scope.guardarItemEspeciales = function () {
        $scope.itemEspecialesEstimacion = $scope.itemEspecialesEstimacion.filter(item => {
            return item.obligatorio !== false || (item.tiempoEstimacion !== "" && item.secuencialNivelColaborador !== "");
        });

        var ajaxGuardarItemEspeciales = $http.post("user/guardar-item-especiales/", {
            itemEspecialesEstimacion: angular.toJson($scope.itemEspecialesEstimacion),
            itemEspecialesAdicionales: angular.toJson($scope.itemEspecialesAdicionales)
        });

        ajaxGuardarItemEspeciales.success(function (data) {
            if (data.success === true) {
                angular.element("#modal-estimacion-items-ticket").modal("hide");
                messageDialog.show('Información', data.msg);
            } else {
                messageDialog.show('Información', data.msg);
            }

        });
    };

    $scope.eliminarItemEspecial = function (itemId) {
        var ajaxEliminarItemEspecial = $http.post("user/eliminar-item-especial/", {
            itemId: itemId,
        });

        ajaxEliminarItemEspecial.success(function (data) {
            if (data.success === true) {
                var itemIndex = $scope.itemsAdicionalesEdit.findIndex(item => item.id === itemId);

                if (itemIndex != -1) {
                    $scope.itemsAdicionalesEdit.splice(itemIndex, 1);
                }

            } else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.editarItems = function (itemsEstimacion, itemsAdicionales) {

        if (itemsEstimacion.length > 0 || itemsAdicionales.length > 0) {

            $scope.itemsEstimacionEdit = itemsEstimacion;
            $scope.itemsEstimacionEditLength = $scope.itemsEstimacionEdit.length;
            $scope.itemsAdicionalesEdit = [];
            $scope.itemsAdicionalesEdit = itemsAdicionales;

            angular.element("#modal-edicion-items-ticket").modal("show");
        }
    };

    $scope.editarItemsEspeciales = function () {

        var ajaxEditarItemEspeciales = $http.post("user/editar-item-especiales/", {
            itemEspecialesEstimacionEdit: angular.toJson($scope.itemsEstimacionEdit),
            itemEspecialesAdicionalesEdit: angular.toJson($scope.itemsAdicionalesEdit)
        });

        ajaxEditarItemEspeciales.success(function (data) {
            if (data.success === true) {

                $scope.itemsEstimacionEdit = [];
                $scope.itemsAdicionalesEdit = [];

                angular.element("#modal-edicion-items-ticket").modal("hide");
                $scope.cargarItemsEspeciales($scope.estimacionIdTmp);
                messageDialog.show('Información', data.msg);
            } else {
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