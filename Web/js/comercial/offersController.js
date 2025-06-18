/* Offers Excel Controller */
comercialApp.controller('offersController', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.cantidadMostrarPorPagina = 10;
    
    // Initialize filter models
    $scope.buscadorGeneral = '';
    $scope.filtro = {
        codigoOferta: '',
        cliente: '',
        tema: '',
        fechaOferta: '',
        precioOferta: '',
        formaPago: '',
        validezOferta: '',
        descuento: '',
        estado: ''
    };

    // Lista de clientes 
    $scope.clientes = ['VFE', 'SKAWSAY', 'CHONE', 'UEJID', 'COOPAR', 'MARCA'];
    
    // Datos de ejemplo completos
    $scope.allOffers = [
        {
            codigoOferta: '25-0001',
            cliente: 'VFE',
            tema: 'Actualización módulos FBS Contabilidad y FBS Facturación Electrónica',
            fechaOferta: '2025-01-08',
            precioOferta: 23940.00,
            formaPago: '70/30',
            validezOferta: '2025-01-20',
            descuento: 'NO',
            estado: 'VENCIDA'
        },
        {
            codigoOferta: '25-0002',
            cliente: 'SKAWSAY',
            tema: 'Renovación Soporte y mantenimiento',
            fechaOferta: '2025-01-06',
            precioOferta: 600.00,
            formaPago: 'Mensual',
            validezOferta: '2025-01-17',
            descuento: 'NO',
            estado: 'ACEPTADA / CONTRATO'
        },
        {
            codigoOferta: '25-0003',
            cliente: 'CHONE',
            tema: 'Modificaciones Banca Virtual y App Clientes',
            fechaOferta: '2025-01-09',
            precioOferta: 1170.00,
            formaPago: '70/30',
            validezOferta: '2025-01-20',
            descuento: 'NO',
            estado: 'VENCIDA'
        },
        {
            codigoOferta: '25-0004',
            cliente: 'UEJID',
            tema: 'Automatización de cobro de fondo mortuorio mensual',
            fechaOferta: '2025-01-07',
            precioOferta: 2400.00,
            formaPago: '70/30',
            validezOferta: '2025-01-27',
            descuento: 'NO',
            estado: 'VENCIDA'
        },
        {
            codigoOferta: '25-0005',
            cliente: 'COOPAR',
            tema: 'Capacitación funcional, administrativa y técnica del sistema Financial',
            fechaOferta: '2025-01-08',
            precioOferta: 2750.00,
            formaPago: '70/30',
            validezOferta: '2025-01-10',
            descuento: 'NO',
            estado: 'ACEPTADA / CONTRATO'
        },
        {
            codigoOferta: '25-0006',
            cliente: 'UEJID',
            tema: 'Actualización funcionalidad Reprogramación para prorrateo de intereses',
            fechaOferta: '2025-01-09',
            precioOferta: 990.00,
            formaPago: '100 % a la entrega',
            validezOferta: '2025-01-20',
            descuento: 'NO',
            estado: 'ACEPTADA / TICKET'
        },
        {
            codigoOferta: '25-0007',
            cliente: 'MARCA',
            tema: 'Renovación Soporte y mantenimiento',
            fechaOferta: '2025-01-13',
            precioOferta: 180.00,
            formaPago: 'Mensual',
            validezOferta: '2025-01-18',
            descuento: 'NO',
            estado: 'ACEPTADA / CONTRATO'
        },
        {
            codigoOferta: '25-0008',
            cliente: 'VFE',
            tema: 'Implementación módulo de Inventarios',
            fechaOferta: '2025-01-14',
            precioOferta: 15000.00,
            formaPago: '70/30',
            validezOferta: '2025-01-28',
            descuento: 'NO',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0009',
            cliente: 'CHONE',
            tema: 'Desarrollo de módulo de Recaudación Mobile',
            fechaOferta: '2025-01-15',
            precioOferta: 8500.00,
            formaPago: '70/30',
            validezOferta: '2025-01-30',
            descuento: 'SI',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0010',
            cliente: 'COOPAR',
            tema: 'Actualización sistema de Cajas',
            fechaOferta: '2025-01-16',
            precioOferta: 4200.00,
            formaPago: '70/30',
            validezOferta: '2025-01-31',
            descuento: 'NO',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0011',
            cliente: 'SKAWSAY',
            tema: 'Desarrollo API REST para integración de servicios',
            fechaOferta: '2025-01-17',
            precioOferta: 5600.00,
            formaPago: '70/30',
            validezOferta: '2025-02-01',
            descuento: 'NO',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0012',
            cliente: 'MARCA',
            tema: 'Implementación módulo de Recursos Humanos',
            fechaOferta: '2025-01-18',
            precioOferta: 12000.00,
            formaPago: '70/30',
            validezOferta: '2025-02-02',
            descuento: 'SI',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0013',
            cliente: 'VFE',
            tema: 'Actualización sistema de Reportería',
            fechaOferta: '2025-01-19',
            precioOferta: 3500.00,
            formaPago: '100 % a la entrega',
            validezOferta: '2025-02-03',
            descuento: 'NO',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0014',
            cliente: 'UEJID',
            tema: 'Desarrollo módulo de Gestión de Cobranzas',
            fechaOferta: '2025-01-20',
            precioOferta: 9800.00,
            formaPago: '70/30',
            validezOferta: '2025-02-04',
            descuento: 'NO',
            estado: 'PENDIENTE'
        },
        {
            codigoOferta: '25-0015',
            cliente: 'CHONE',
            tema: 'Implementación sistema de Notificaciones',
            fechaOferta: '2025-01-21',
            precioOferta: 2800.00,
            formaPago: '70/30',
            validezOferta: '2025-02-05',
            descuento: 'NO',
            estado: 'PENDIENTE'
        }
    ];

    // Función para aplicar filtros y actualizar vista
    function actualizarVista() {
        var filteredOffers = $scope.allOffers;
        
        // Aplicar búsqueda general
        if ($scope.buscadorGeneral) {
            var search = $scope.buscadorGeneral.toLowerCase();
            filteredOffers = filteredOffers.filter(function(offer) {
                return offer.codigoOferta.toLowerCase().includes(search) ||
                       offer.cliente.toLowerCase().includes(search) ||
                       offer.tema.toLowerCase().includes(search) ||
                       offer.estado.toLowerCase().includes(search);
            });
        }

        // Aplicar filtros específicos
        Object.keys($scope.filtro).forEach(function(key) {
            if ($scope.filtro[key]) {
                filteredOffers = filteredOffers.filter(function(offer) {
                    var offerValue = offer[key];
                    return offerValue && offerValue.toString().toLowerCase()
                        .includes($scope.filtro[key].toLowerCase());
                });
            }
        });

        // Calcular paginación
        $scope.totalOfertas = filteredOffers.length;
        $scope.cantPaginas = Math.ceil($scope.totalOfertas / numerosPorPagina);
        if ($scope.cantPaginas === 0 || !$scope.cantPaginas) {
            $scope.cantPaginas = 1;
        }

        // Aplicar paginación
        var startIndex = (pagina - 1) * numerosPorPagina;
        $scope.ofertasLista = filteredOffers.slice(startIndex, startIndex + numerosPorPagina);

        // Configurar números de página
        $scope.listaPaginas = [];
        if ($scope.cantPaginas <= 5) {
            for (var i = 1; i <= $scope.cantPaginas; i++) {
                $scope.listaPaginas.push(i);
            }
        } else if (pagina <= 3) {
            for (var i = 1; i <= 5; i++) {
                $scope.listaPaginas.push(i);
            }
        } else if (pagina >= $scope.cantPaginas - 2) {
            for (var i = $scope.cantPaginas - 4; i <= $scope.cantPaginas; i++) {
                $scope.listaPaginas.push(i);
            }
        } else {
            for (var i = pagina - 2; i <= pagina + 2; i++) {
                $scope.listaPaginas.push(i);
            }
        }

        // Asegurar que la página actual es válida
        if (pagina > $scope.cantPaginas) {
            pagina = $scope.cantPaginas;
        }
    }

    // Inicializar datos y asegurar que Angular haya compilado la vista
    $timeout(function() {
        actualizarVista();
    });

    // Watch para filtros
    $scope.$watch('buscadorGeneral', function(newVal, oldVal) {
        if (newVal !== oldVal) {
            pagina = 1;
            actualizarVista();
        }
    });

    $scope.$watch('filtro', function(newVal, oldVal) {
        if (newVal !== oldVal) {
            pagina = 1;
            actualizarVista();
        }
    }, true);

    // Funciones de paginación
    $scope.cambiarPagina = function(pag) {
        pagina = pag;
        actualizarVista();
    };

    $scope.atrazarPagina = function() {
        if (pagina > 1) {
            pagina--;
            actualizarVista();
        }
    };

    $scope.avanzarPagina = function() {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            actualizarVista();
        }
    };

    $scope.actualizarCantidadMostrar = function() {
        numerosPorPagina = parseInt($scope.cantidadMostrarPorPagina);
        pagina = 1;
        actualizarVista();
    };

    // Funciones para el CRUD
    $scope.agregarOferta = function() {
        $scope.editingOffer = {
            fechaOferta: new Date(),
            validezOferta: new Date(new Date().setDate(new Date().getDate() + 15)),
            descuento: 'NO'
        };
        angular.element('#modal-excel-offer').modal('show');
    };

    $scope.editarOferta = function(oferta) {
        $scope.editingOffer = angular.copy(oferta);
        angular.element('#modal-excel-offer').modal('show');
    };

    $scope.eliminarOferta = function(oferta) {
        if (confirm('¿Está seguro que desea eliminar esta oferta?')) {
            var index = $scope.allOffers.findIndex(o => o.codigoOferta === oferta.codigoOferta);
            if (index !== -1) {
                $scope.allOffers.splice(index, 1);
                actualizarVista();
            }
        }
    };

    $scope.guardarOferta = function() {
        if ($scope.editingOffer.id) {
            // Editar oferta existente
            var index = $scope.allOffers.findIndex(o => o.codigoOferta === $scope.editingOffer.codigoOferta);
            if (index !== -1) {
                $scope.allOffers[index] = angular.copy($scope.editingOffer);
            }
        } else {
            // Agregar nueva oferta
            $scope.editingOffer.id = Date.now();
            $scope.allOffers.unshift(angular.copy($scope.editingOffer));
        }
        actualizarVista();
        angular.element('#modal-excel-offer').modal('hide');
    };

    // Initialize datepicker
    $('.datepicker').datepicker({
        format: 'yyyy-mm-dd',
        autoclose: true,
        todayHighlight: true
    });

}]);
