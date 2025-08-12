/* Offers Excel Controller */
comercialApp.controller('offersController', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
    // Control de pagina actual para paginacion visual
    $scope.paginaActual = 1;
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.cantidadMostrarPorPagina = 10;
    
    // Initialize filter models
    $scope.buscadorGeneral = '';
    $scope.filtro = {
        codigoOferta: '',
        ticket: '',
        cliente: '',
        tema: '',
        fechaOferta: '',
        precioOferta: '',
        formaPago: '',
        validezOferta: '',
        descuento: '',
        estado: ''
    };

        // Validación visual para el campo precioOferta
    $scope.isPrecioOfertaInvalido = function() {
        if (!$scope.editingOffer) return false;
        var precioStr = ($scope.editingOffer.precioOferta || '').toString().replace(',', '.');
        var precio = parseFloat(precioStr);
        return (!$scope.editingOffer.esOfertaMayor && precio >= 1000);
    };

    // Observar cambios en precioOferta y esOfertaMayor para actualizar la validación visual
    $scope.$watchGroup([
        'editingOffer.precioOferta',
        'editingOffer.esOfertaMayor'
    ], function() {
        // Solo para disparar el digest y actualizar la UI
    });

   $scope.detalleOferta = {};
    $scope.verDetalleOferta = function (oferta, $event) {
        if ($event) $event.stopPropagation();

        const id =
            oferta.secuencial ||
            oferta.Secuencial ||
            oferta.id ||
            oferta.Id ||
            oferta.ID ||
            oferta.ticket;

        if (!id) {
            alert('No se pudo determinar el identificador de la oferta o ticket.');
            return;
        }

        $http.post('comercial/detalle-oferta', { id: id }).then(
            function (response) {
                const data = response.data;

                if (data && typeof data === 'object' && data.success) {
                    // Función para convertir fechas del formato /Date()/
                    function convertirFecha(fechaStr) {
                        if (fechaStr && typeof fechaStr === 'string' && fechaStr.indexOf('/Date(') === 0) {
                            var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                            return new Date(milisegundos).toLocaleDateString('es-ES');
                        }
                        return fechaStr || null;
                    }

                    // Convertir fechas en el detalle
                    $scope.detalleOferta = {
                        ...data.detalle,
                        fechaEstimacion: convertirFecha(data.detalle.fechaEstimacion),
                        fechaRevision: convertirFecha(data.detalle.fechaRevision),
                        fechaAprobacionGerencia: convertirFecha(data.detalle.fechaAprobacionGerencia),
                        fechaEnvioOferta: convertirFecha(data.detalle.fechaEnvioOferta),
                        fechaGeneracion: convertirFecha(data.detalle.fechaGeneracion),
                        fechaVencimiento: convertirFecha(data.detalle.fechaVencimiento)
                    };

                    // Mostrar modal de forma limpia
                    angular.element('#modal-detalle-oferta').modal('show');
                } else if (data && typeof data === 'object' && data.msg) {
                    alert(data.msg);
                } else {
                    alert('No se pudo obtener el detalle de la oferta o ticket.');
                }
            },
            function () {
                alert('No se pudo obtener el detalle de la oferta o ticket.');
            }
        );
    };

    // --- Cargar requerimientos para el select de ofertas (relación o catálogo según caso) ---
    $scope.requerimientos = [];
    $scope.pedidosRequerimientos = [];
    // Carga requerimientos relación (para ofertas existentes)
    $scope.cargarRequerimientosOfertas = function(cb, selectedId) {
        $http.post("comercial/requerimientos-ofertas", {}).success(function(data) {
            if (data.success === true) {
                $scope.requerimientos = data.requerimientos.map(function(r) {
                    // Aseguramos que Secuencial sea string para evitar problemas de binding
                    return {
                        Secuencial: r.id != null ? r.id.toString() : '',
                        ClienteNombre: r.cliente,
                        TicketNumero: r.ticket,
                        Detalle: r.detalle,
                        descripcion: r.descripcion
                    };
                });
                if (cb) cb(selectedId);
            } else {
                $scope.requerimientos = [];
            }
        });
    };
    // Carga requerimientos puros (para tickets pendientes)
    $scope.cargarCatalogoRequerimientos = function(cb) {
        $http.post("comercial/catalogo-requerimientos", {}).success(function(data) {
            if (data.success === true) {
                $scope.pedidosRequerimientos = data.requerimientos.map(function(r) {
                    return {
                        id: r.id != null ? r.id.toString() : '',
                        descripcion: r.descripcion
                    };
                });
                if (cb) cb();
            } else {
                $scope.pedidosRequerimientos = [];
            }
        });
    };

    // Al seleccionar un requerimiento, poblar fechas desde el ticket asociado al requerimiento
    $scope.onRequerimientoChange = function() {
        var req = $scope.requerimientos.find(r => r.Secuencial == $scope.editingOffer.OfertaRequerimiento);
        if (req && req.ticket) {
            $scope.editingOffer.ticket = req.ticket;
            $http.post("tickets/dar-datos-ticket-ofertas", { idTicket: req.ticket }).success(function(data) {
                if (data.success === true && data.datosTicket) {
                    function convertirFecha(fechaStr) {
                        if (fechaStr && typeof fechaStr === 'string' && fechaStr.indexOf('/Date(') === 0) {
                            var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                            return new Date(milisegundos);
                        }
                        return fechaStr ? new Date(fechaStr) : null;
                    }
                    $scope.editingOffer.fechaEstimacion = convertirFecha(data.datosTicket.FechaRecepcionEstimacion);
                    $scope.editingOffer.fechaRevision = convertirFecha(data.datosTicket.FechaEnvioRevision);
                    $scope.editingOffer.fechaAprobacionGerencia = convertirFecha(data.datosTicket.FechaAprobacionGerencia);
                    $scope.editingOffer.fechaEnvioOferta = convertirFecha(data.datosTicket.FechaEnvioOfertaCliente);
                } else {
                    $scope.editingOffer.fechaEstimacion = '';
                    $scope.editingOffer.fechaRevision = '';
                    $scope.editingOffer.fechaAprobacionGerencia = '';
                    $scope.editingOffer.fechaEnvioOferta = '';
                }
            });
        } else {
            $scope.editingOffer.ticket = '';
            $scope.editingOffer.fechaEstimacion = '';
            $scope.editingOffer.fechaRevision = '';
            $scope.editingOffer.fechaAprobacionGerencia = '';
            $scope.editingOffer.fechaEnvioOferta = '';
        }
    };

    // --- Listar ofertas reales desde backend ---
    $scope.ofertasLista = [];
    $scope.cantPaginas = 1;
    $scope.listaPaginas = [];
    $scope.totalOfertas = 0;
    $scope.cargarOfertas = function (start, lenght) {
        if (start === undefined) start = 0;
        if (lenght === undefined) lenght = $scope.cantidadMostrarPorPagina;
        $http.post("comercial/dar-ofertas-comercial", {
            start: start,
            lenght: lenght,
            filtro: $scope.buscadorGeneral || '',
            filtrosColumna: JSON.stringify($scope.filtro)
        }).success(function (data) {
            if (data.success === true) {
                // Si la pagina actual es mayor al total de paginas, volver a la primera
                if ($scope.paginaActual > Math.ceil(data.total / $scope.cantidadMostrarPorPagina)) {
                    $scope.paginaActual = 1;
                }
                $scope.ofertasLista = data.ofertasComercial.map(function(oferta) {
                    function convertirFecha(fechaStr) {
                        if (fechaStr && typeof fechaStr === 'string' && fechaStr.indexOf('/Date(') === 0) {
                            var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                            return new Date(milisegundos);
                        }
                        return fechaStr ? new Date(fechaStr) : null;
                    }
                    // Mapear precio a precioOferta para consistencia en la vista
                    var codigoOferta = oferta.codigo || oferta.codigoOferta;
                    return {
                        ...oferta,
                        precioOferta: oferta.precio,
                        fechaEstimacion: convertirFecha(oferta.fechaEstimacion),
                        fechaGeneracion: convertirFecha(oferta.fechaGeneracion),
                        fechaRevision: convertirFecha(oferta.fechaRevision),
                        fechaAprobacionGerencia: convertirFecha(oferta.fechaAprobacionGerencia),
                        fechaEnvioOferta: convertirFecha(oferta.fechaEnvioOferta),
                        fechaVencimiento: convertirFecha(oferta.fechaVencimiento),
                        codigo: (!codigoOferta || codigoOferta === '') ? 'NO APLICA' : codigoOferta,
                        proximaActividad: oferta.proximaActividad
                    };
                });
                $scope.totalOfertas = data.total;
                $scope.cantPaginas = Math.ceil(data.total / $scope.cantidadMostrarPorPagina) || 1;
                $scope.listaPaginas = Array.from({length: $scope.cantPaginas}, (_, i) => i + 1);
            } else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarOfertas();

    // Debounce para filtros y búsqueda general
    var debounceTimeout = null;
    $scope.$watch('buscadorGeneral', function(newVal, oldVal) {
        if (newVal !== oldVal) {
            if (debounceTimeout) $timeout.cancel(debounceTimeout);
            debounceTimeout = $timeout(function() {
                pagina = 1;
                $scope.cargarOfertas();
            }, 2000);
        }
    });
    $scope.$watch('filtro', function(newVal, oldVal) {
        if (newVal !== oldVal) {
            if (debounceTimeout) $timeout.cancel(debounceTimeout);
            debounceTimeout = $timeout(function() {
                pagina = 1;
                $scope.cargarOfertas();
            }, 2000);
        }
    }, true);

    // Funciones de paginación
    $scope.cambiarPagina = function(pag) {
        pagina = pag;
        $scope.paginaActual = pag;
        $scope.cargarOfertas((pagina - 1) * $scope.cantidadMostrarPorPagina, $scope.cantidadMostrarPorPagina);
    };
    $scope.atrazarPagina = function() {
        if (pagina > 1) {
            pagina--;
            $scope.paginaActual = pagina;
            $scope.cargarOfertas((pagina - 1) * $scope.cantidadMostrarPorPagina, $scope.cantidadMostrarPorPagina);
        }
    };
    $scope.avanzarPagina = function() {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.paginaActual = pagina;
            $scope.cargarOfertas((pagina - 1) * $scope.cantidadMostrarPorPagina, $scope.cantidadMostrarPorPagina);
        }
    };
    $scope.actualizarCantidadMostrar = function() {
        numerosPorPagina = parseInt($scope.cantidadMostrarPorPagina);
        pagina = 1;
        $scope.cargarOfertas(0, numerosPorPagina);
    };

    // CRUD
    $scope.agregarOferta = function() {
        $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
            if (data.success === true) {
                $scope.editingOffer = {
                    codigo: data.nuevoCodigo,
                    OfertaRequerimiento: '',
                    fechaGeneracion: '',
                    fechaVencimiento: '',
                    fechaEstimacion: '',
                    fechaRevision: '',
                    fechaAprobacionGerencia: '',
                    fechaEnvioOferta: '',
                    precioOferta: '',
                    formaPago: '',
                    descuento: 'NO',
                    estado: '',
                    tipo: '',
                    tema: '',
                    esOfertaMayor: true,
                    _esNuevo: true,
                    _esTicketPendiente: false
                };
                $scope.cargarRequerimientosOfertas(function() {
                    $scope.onOfertaMayorChange();
                    angular.element('#modal-excel-offer').modal('show');
                });
            }
        });
    };

    // Nueva función para agregar oferta desde ticket pendiente
    $scope.agregarOfertaDesdeTicketPendiente = function(ticket) {
        $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
            if (data.success === true) {
                $scope.editingOffer = {
                    codigo: data.nuevoCodigo,
                    OfertaRequerimiento: '',
                    OfertaPedidoRequerimiento: '',
                    fechaGeneracion: '',
                    fechaVencimiento: '',
                    fechaEstimacion: '',
                    fechaRevision: '',
                    fechaAprobacionGerencia: '',
                    fechaEnvioOferta: '',
                    precioOferta: '',
                    formaPago: '',
                    descuento: 'NO',
                    estado: '',
                    tipo: '',
                    tema: '',
                    _esNuevo: true,
                    _esTicketPendiente: true,
                    ticketPendienteNumero: ticket
                };
                $scope.cargarCatalogoRequerimientos(function() {
                    angular.element('#modal-excel-offer').modal('show');
                });
            }
        });
    };

    $scope.guardarOferta = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        // Validar y convertir precioOferta a número decimal
        var precioStr = ($scope.editingOffer.precioOferta || '').toString().replace(',', '.');
        var precio = parseFloat(precioStr);
        if (isNaN(precio)) {
            waitingDialog.hide();
            messageDialog.show('Error', 'El campo PRECIO OFERTA debe ser un número válido.');
            return;
        }
        // Validación: si no es oferta mayor, el precio no puede ser mayor a 1000
        if (!$scope.editingOffer.esOfertaMayor && precio > 1000) {
            waitingDialog.hide();
            messageDialog.show('Error', 'Solo se permite precio mayor a 1000 si se marca "ES OFERTA MAYOR"');
            return;
        }
        var codigoFinal = $scope.editingOffer.esOfertaMayor ? $scope.editingOffer.codigo : '';
        // Convertir "NO APLICA" a cadena vacía para el backend
        if (codigoFinal === 'NO APLICA') {
            codigoFinal = '';
        }
        var datosOferta = {
            OfertaRequerimiento: $scope.editingOffer.OfertaPedidoRequerimiento || $scope.editingOffer.OfertaRequerimiento,
            codigo: codigoFinal,
            fechaEstimacion: $scope.editingOffer.fechaEstimacion,
            fechaRevision: $scope.editingOffer.fechaRevision,
            fechaEnvioOferta: $scope.editingOffer.fechaEnvioOferta,
            fechaGeneracion: $scope.editingOffer.fechaGeneracion,
            fechaAprobacionGerencia: $scope.editingOffer.fechaAprobacionGerencia,
            fechaVencimiento: $scope.editingOffer.fechaVencimiento,
            precio: precio,
            formaPago: $scope.editingOffer.formaPago,
            descuento: $scope.editingOffer.descuento,
            estado: $scope.editingOffer.estado,
            tipo: $scope.editingOffer.tipo,
            tema: $scope.editingOffer.tema
        };
        $http.post("comercial/guardar-oferta", datosOferta)
            .success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular.element("#modal-excel-offer").modal("hide");
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
    // --- Al editar una oferta, seleccionar correctamente el requerimiento asociado ---
    var originalEditarOferta = $scope.editarOferta;
    $scope.editarOferta = function(oferta) {
        $scope.editingOffer = angular.copy(oferta);
        $scope.editingOffer.esOfertaMayor = ($scope.editingOffer.codigo && $scope.editingOffer.codigo !== 'NO APLICA' && $scope.editingOffer.codigo !== '');
        $scope.editingOffer._esNuevo = false;
        $scope.editingOffer._esTicketPendiente = false;
        // Cargar requerimientos y seleccionar el correcto solo cuando estén realmente cargados
        $scope.cargarRequerimientosOfertas(function(selectedId) {
            var idReq = $scope.editingOffer.secuencialRequerimiento || $scope.editingOffer.OfertaRequerimiento;
            if (idReq) {
                $scope.editingOffer.OfertaRequerimiento = idReq.toString();
            }
            $scope.onOfertaMayorChange();
            angular.element('#modal-excel-offer').modal('show');
        });
    };

    // Guardar nueva oferta usando la ruta moderna
    $scope.guardarOferta = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        $http.post("comercial/guardar-oferta", {
            OfertaRequerimiento: $scope.editingOffer.OfertaRequerimiento,
            codigo: $scope.editingOffer.codigo,
            fechaEstimacion: $scope.editingOffer.fechaEstimacion,
            fechaRevision: $scope.editingOffer.fechaRevision,
            fechaEnvioOferta: $scope.editingOffer.fechaEnvioOferta,
            fechaGeneracion: $scope.editingOffer.fechaGeneracion,
            fechaAprobacionGerencia: $scope.editingOffer.fechaAprobacionGerencia,
            fechaVencimiento: $scope.editingOffer.fechaVencimiento,
            precio: $scope.editingOffer.precioOferta,
            formaPago: $scope.editingOffer.formaPago,
            descuento: $scope.editingOffer.descuento,
            estado: $scope.editingOffer.estado,
            tipo: $scope.editingOffer.tipo,
            tema: $scope.editingOffer.tema
        })
        .success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-excel-offer").modal("hide");
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

    // Editar oferta usando la ruta moderna
    $scope.actualizarOferta = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        $http.post("comercial/editar-oferta", {
            id: $scope.editingOffer.id,
            OfertaRequerimiento: $scope.editingOffer.OfertaRequerimiento,
            codigo: $scope.editingOffer.codigo,
            fechaEstimacion: $scope.editingOffer.fechaEstimacion,
            fechaRevision: $scope.editingOffer.fechaRevision,
            fechaEnvioOferta: $scope.editingOffer.fechaEnvioOferta,
            fechaGeneracion: $scope.editingOffer.fechaGeneracion,
            fechaAprobacionGerencia: $scope.editingOffer.fechaAprobacionGerencia,
            fechaVencimiento: $scope.editingOffer.fechaVencimiento,
            precio: $scope.editingOffer.precioOferta,
            formaPago: $scope.editingOffer.formaPago,
            descuento: $scope.editingOffer.descuento,
            estado: $scope.editingOffer.estado,
            tipo: $scope.editingOffer.tipo,
            tema: $scope.editingOffer.tema
        })
        .success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-excel-offer").modal("hide");
                $scope.cargarOfertas();
                messageDialog.show('Éxito', 'Oferta editada correctamente');
            } else {
                messageDialog.show('Información', data.msg);
            }
        })
        .error(function () {
            waitingDialog.hide();
            messageDialog.show('Error', 'Error al editar la oferta');
        });
    };
    $scope.eliminarOferta = function(oferta) {
        if (confirm('¿Está seguro que desea eliminar esta oferta?')) {
            $http.post("comercial/EliminarOferta", { codigo: oferta.codigo })
                .success(function (data) {
                    if (data.success === true) {
                        $scope.cargarOfertas();
                        messageDialog.show('Éxito', 'Oferta eliminada correctamente');
                    } else {
                        messageDialog.show('Información', data.msg);
                    }
                });
        }
    };
    // Exportar Excel/PDF/Imprimir usando datos reales
    $scope.exportarExcel = function() {
        const rows = $scope.ofertasLista.map(oferta => ({
            'CODIGO OFERTA': oferta.codigo,
            'NUMERO TICKET': oferta.ticket,
            'CLIENTE': oferta.cliente,
            'TEMA': oferta.tema,
            'F.REC ESTIMACION': oferta.fechaEstimacion ? moment(oferta.fechaEstimacion).format('DD/MM/YYYY') : '',
            'F.GEN OFERTA': oferta.fechaGeneracion ? moment(oferta.fechaGeneracion).format('DD/MM/YYYY') : '',
            'F.ENV REVISION': oferta.fechaRevision ? moment(oferta.fechaRevision).format('DD/MM/YYYY') : '',
            'F.APR GERENCIA': oferta.fechaAprobacionGerencia ? moment(oferta.fechaAprobacionGerencia).format('DD/MM/YYYY') : '',
            'F.ENV CLIENTE': oferta.fechaEnvioOferta ? moment(oferta.fechaEnvioOferta).format('DD/MM/YYYY') : '',
            'FECHA VENCIMIENTO': oferta.fechaVencimiento ? moment(oferta.fechaVencimiento).format('DD/MM/YYYY') : '',
            'TIPO': oferta.tipo,
            'PRECIO': oferta.precioOferta != null ? oferta.precioOferta : oferta.precio,
            'PAGO': oferta.formaPago,
            'VALIDEZ': oferta.validezOferta ? moment(oferta.validezOferta).format('DD/MM/YYYY') : '',
            'DESC': oferta.descuento,
            'PROXIMA ACTIVIDAD': oferta.proximaActividad,
            'ESTADO': oferta.estado
        }));
        const worksheet = XLSX.utils.json_to_sheet(rows);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Ofertas");
        worksheet['!cols'] = [
            { wch: 15 }, { wch: 15 }, { wch: 20 }, { wch: 40 }, { wch: 15 }, { wch: 15 }, { wch: 20 }, { wch: 15 }, { wch: 15 }, { wch: 15 }, { wch: 15 }, { wch: 15 }, { wch: 15 }, { wch: 10 }, { wch: 10 }, { wch: 20 }, { wch: 15 }
        ];
        XLSX.writeFile(workbook, 'Ofertas.xlsx');
    };
    $scope.exportarPDF = function() {
        // Usar el patrón robusto de proyectosController.js, pero mejorando el ajuste de columnas
        const { jsPDF } = window.jspdf;
        var doc = new jsPDF({ orientation: 'landscape', unit: 'pt', format: 'a3' });

        // Clonar la tabla y limpiar filtros y columna de acciones
        var table = document.getElementById('tabla-ofertas-excel').cloneNode(true);
        // Eliminar la fila de filtros (segunda fila del thead)
        if (table.tHead && table.tHead.rows.length > 1) {
            table.tHead.deleteRow(1);
        }
        // Eliminar la última columna (acciones) de thead y tbody
        for (var r = 0; r < table.tHead.rows.length; r++) {
            table.tHead.rows[r].deleteCell(table.tHead.rows[r].cells.length - 1);
        }
        for (var i = 0; i < table.tBodies[0].rows.length; i++) {
            table.tBodies[0].rows[i].deleteCell(table.tBodies[0].rows[i].cells.length - 1);
        }

        // Añadir título
        doc.setFontSize(14);
        doc.text('Listado de Ofertas', 40, 30);

        // Exportar tabla con autoTable, forzando ajuste horizontal
        doc.autoTable({
            html: table,
            startY: 40,
            styles: { fontSize: 7, cellPadding: 2, overflow: 'linebreak' },
            headStyles: { fillColor: [0, 123, 255] },
            margin: { left: 10, right: 10 },
            tableWidth: 'stretch',
            theme: 'grid'
        });

        doc.save('Ofertas.pdf');
    };
    $scope.imprimir = function() {
        // Construir tabla solo con datos, sin HTML extra y asegurando todos los campos
        const columnas = [
            'CODIGO OFERTA', 'NUMERO TICKET', 'CLIENTE', 'TEMA', 'F.REC ESTIMACION', 'F.GEN OFERTA',
            'F.ENV REVISION', 'F.APR GERENCIA', 'F.ENV CLIENTE', 'FECHA VENCIMIENTO', 'TIPO', 'PRECIO',
            'PAGO', 'VALIDEZ', 'DESC', 'PROXIMA ACTIVIDAD', 'ESTADO'
        ];
        let tabla = '<table style="width:100%;border-collapse:collapse;">';
        tabla += '<thead><tr>' + columnas.map(c => `<th style="border:1px solid #ddd;padding:4px;background:#f2f2f2;">${c}</th>`).join('') + '</tr></thead>';
        tabla += '<tbody>';
        $scope.ofertasLista.forEach(oferta => {
            tabla += '<tr>' + [
                oferta.codigo,
                oferta.ticket,
                oferta.cliente,
                oferta.tema,
                oferta.fechaEstimacion ? moment(oferta.fechaEstimacion).format('DD/MM/YYYY') : '',
                oferta.fechaGeneracion ? moment(oferta.fechaGeneracion).format('DD/MM/YYYY') : '',
                oferta.fechaRevision ? moment(oferta.fechaRevision).format('DD/MM/YYYY') : '',
                oferta.fechaAprobacionGerencia ? moment(oferta.fechaAprobacionGerencia).format('DD/MM/YYYY') : '',
                oferta.fechaEnvioOferta ? moment(oferta.fechaEnvioOferta).format('DD/MM/YYYY') : '',
                oferta.fechaVencimiento ? moment(oferta.fechaVencimiento).format('DD/MM/YYYY') : '',
                oferta.tipo,
                oferta.precioOferta != null ? oferta.precioOferta : oferta.precio,
                oferta.formaPago,
                oferta.validezOferta ? moment(oferta.validezOferta).format('DD/MM/YYYY') : '',
                oferta.descuento,
                oferta.proximaActividad,
                oferta.estado
            ].map(d => `<td style="border:1px solid #ddd;padding:4px;">${d != null ? d : ''}</td>`).join('') + '</tr>';
        });
        tabla += '</tbody></table>';
        // Imprimir tabla en ventana nueva, sin divs de scroll ni estilos de recorte
        const printWindow = window.open('', '_blank');
        printWindow.document.write(`
            <html>
                <head>
                    <title>Ofertas</title>
                    <style>
                        @page { size: A3 landscape; margin: 10mm; }
                        body { font-family: Arial, sans-serif; margin: 0; }
                        html, body { width: 100%; }
                        table { width: 100% !important; border-collapse: collapse; margin-top: 20px; table-layout: fixed; }
                        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; font-size: 10px; word-break: break-word; }
                        th { background-color: #f2f2f2; }
                    </style>
                </head>
                <body>
                    <h2 style="text-align: center;">Listado de Ofertas</h2>
                    ${tabla}
                </body>
            </html>
        `);
        printWindow.document.close();
        printWindow.focus();
        setTimeout(() => {
            printWindow.print();
            printWindow.close();
        }, 250);
    };
    // Initialize datepicker
    $('.datepicker').datepicker({
        format: 'yyyy-mm-dd',
        autoclose: true,
        todayHighlight: true
    });

    // --- Lógica para el checkbox "ES OFERTA MAYOR" ---
    $scope.onOfertaMayorChange = function() {
        if ($scope.editingOffer) {
            if ($scope.editingOffer.esOfertaMayor) {
                if (!$scope.editingOffer.codigo || $scope.editingOffer.codigo === 'NO APLICA' || $scope.editingOffer.codigo === '') {
                    $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
                        if (data.success === true) {
                            $scope.editingOffer.codigo = data.nuevoCodigo;
                        }
                    });
                }
            } else {
                $scope.editingOffer.codigo = 'NO APLICA';
            }
        }
    };

}]);
