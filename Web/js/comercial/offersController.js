/* Offers Excel Controller */
comercialApp.controller('offersController', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
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

    // --- Cargar requerimientos reales para el select de ofertas ---
    $scope.requerimientos = [];
    $scope.cargarRequerimientos = function() {
        $http.post("comercial/requerimientos_ofertas", {}).success(function(data) {
            if (data.success === true) {
                $scope.requerimientos = data.requerimientosComercial;
            }
        });
    };
    $scope.cargarRequerimientos();

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
                $scope.ofertasLista = data.ofertasComercial.map(function(oferta) {
                    function convertirFecha(fechaStr) {
                        if (fechaStr && typeof fechaStr === 'string' && fechaStr.indexOf('/Date(') === 0) {
                            var milisegundos = parseInt(fechaStr.match(/\d+/)[0]);
                            return new Date(milisegundos);
                        }
                        return fechaStr ? new Date(fechaStr) : null;
                    }
                    // Mapear precio a precioOferta para consistencia en la vista
                    return {
                        ...oferta,
                        precioOferta: oferta.precio,
                        fechaEstimacion: convertirFecha(oferta.fechaEstimacion),
                        fechaGeneracion: convertirFecha(oferta.fechaGeneracion),
                        fechaRevision: convertirFecha(oferta.fechaRevision),
                        fechaAprobacionGerencia: convertirFecha(oferta.fechaAprobacionGerencia),
                        fechaEnvioOferta: convertirFecha(oferta.fechaEnvioOferta),
                        fechaVencimiento: convertirFecha(oferta.fechaVencimiento),
                        codigo: oferta.codigo || oferta.codigoOferta,
                        proximaActividad: oferta.proximaActividad
                    };
                });
                $scope.totalOfertas = data.total;
                $scope.cantPaginas = Math.ceil(data.total / $scope.cantidadMostrarPorPagina) || 1;
                $scope.listaPaginas = Array.from({length: $scope.cantPaginas}, (_, i) => i + 1);
                pagina = 1;
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
        $scope.cargarOfertas((pagina - 1) * $scope.cantidadMostrarPorPagina, $scope.cantidadMostrarPorPagina);
    };
    $scope.atrazarPagina = function() {
        if (pagina > 1) {
            pagina--;
            $scope.cargarOfertas((pagina - 1) * $scope.cantidadMostrarPorPagina, $scope.cantidadMostrarPorPagina);
        }
    };
    $scope.avanzarPagina = function() {
        if (pagina < $scope.cantPaginas) {
            pagina++;
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
                    _esNuevo: true,
                    _esTicketPendiente: false // Por defecto no es ticket pendiente
                };
                angular.element('#modal-excel-offer').modal('show');
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
                // Cargar requerimientos para el select
                $scope.cargarRequerimientos();
                angular.element('#modal-excel-offer').modal('show');
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
        var datosOferta = {
            OfertaRequerimiento: $scope.editingOffer.OfertaPedidoRequerimiento || $scope.editingOffer.OfertaRequerimiento,
            codigo: $scope.editingOffer.codigo,
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
    $scope.editarOferta = function(oferta) {
        $scope.editingOffer = angular.copy(oferta);
        // Determinar si es ticket pendiente (sin código de oferta)
        if (!$scope.editingOffer.codigo) {
            $http.post("comercial/generar-codigo-oferta", {}).success(function (data) {
                if (data.success === true) {
                    $scope.editingOffer.codigo = data.nuevoCodigo;
                    $scope.editingOffer._esNuevo = true;
                    $scope.editingOffer._esTicketPendiente = true;
                    // Cargar pedidos/requerimientos para el select
                    $http.post("comercial/catalogo-requerimientos", {}).success(function (dataReq) {
                        if (dataReq.success === true) {
                            $scope.pedidosRequerimientos = dataReq.requerimientos;
                        }
                    });
                    angular.element('#modal-excel-offer').modal('show');
                }
            });
        } else {
            $scope.editingOffer._esNuevo = false;
            $scope.editingOffer._esTicketPendiente = false;
            // Cargar requerimientos para el select
            $scope.cargarRequerimientos();
            angular.element('#modal-excel-offer').modal('show');
        }
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
            'CÓDIGO OFERTA': oferta.codigo,
            'TICKET': oferta.ticket,
            'CLIENTE': oferta.cliente,
            'TEMA': oferta.tema,
            'F.Rec Estimación': oferta.fechaEstimacion,
            'F.Gen Oferta': oferta.fechaGeneracion,
            'F.Env Revisión': oferta.fechaRevision,
            'F.Apr Gerencia': oferta.fechaAprobacionGerencia,
            'F.Env Cliente': oferta.fechaEnvioOferta,
            'F.Vencimiento': oferta.fechaVencimiento,
            'TIPO': oferta.tipo,
            'PRECIO OFERTA': oferta.precio,
            'FORMA DE PAGO': oferta.formaPago,
            'DESCUENTO': oferta.descuento ? 'SI' : 'NO',
            'ESTADO': oferta.estado
        }));
        const worksheet = XLSX.utils.json_to_sheet(rows);
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, "Ofertas");
        worksheet['!cols'] = [
            { wch: 15 }, { wch: 15 }, { wch: 20 }, { wch: 40 }, { wch: 15 }, { wch: 15 }, { wch: 20 }, { wch: 15 }, { wch: 10 }, { wch: 20 }
        ];
        XLSX.writeFile(workbook, 'Ofertas.xlsx');
    };
    $scope.exportarPDF = function() {
        const htmlTabla = document.getElementById('tabla-ofertas-excel').outerHTML;
        const tempDiv = document.createElement('div');
        tempDiv.innerHTML = htmlTabla;
        const opt = {
            margin: 0.5,
            filename: 'Ofertas.pdf',
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2 },
            jsPDF: { unit: 'in', format: 'a4', orientation: 'landscape' }
        };
        html2pdf().set(opt).from(tempDiv).save();
    };
    $scope.imprimir = function() {
        const htmlTabla = document.getElementById('tabla-ofertas-excel').outerHTML;
        const printWindow = window.open('', '_blank');
        printWindow.document.write(`
            <html>
                <head>
                    <title>Ofertas</title>
                    <style>
                        body { font-family: Arial, sans-serif; }
                        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                        th { background-color: #f2f2f2; }
                        .badge {
                            padding: 3px 7px;
                            border-radius: 10px;
                            color: white;
                            font-size: 12px;
                        }
                        .bg-success { background-color: #5cb85c; }
                        .bg-warning { background-color: #f0ad4e; }
                        .bg-danger { background-color: #d9534f; }
                        .text-right { text-align: right; }
                    </style>
                </head>
                <body>
                    <h2 style="text-align: center;">Listado de Ofertas</h2>
                    ${htmlTabla}
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

}]);
