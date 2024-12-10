var comercialApp = angular.module('comercial', []);

comercialApp.config(function ($provide, $httpProvider) {

    // Intercept http calls.
    $provide.factory('MyHttpInterceptor', function ($q) {
        return {
            // On request success
            request: function (config) {
                return config || $q.when(config);
            },
            // On request failure
            requestError: function (rejection) {
                return $q.reject(rejection);
            },
            // On response success
            response: function (response) {
                var x = false
                if (typeof response.data === "string" || (typeof response.data == "object" && response.data.constructor === String)) {
                    x = true;
                }
                if (x && response.data.indexOf('ng-app="login"') !== -1) {
                    window.location.assign("sifizplanning/redirect");
                }
                else
                    return response || $q.when(response);
            },
            // On response failture
            responseError: function (rejection) {
                return $q.reject(rejection);
            }
        };
    });

    // Add the interceptor to the $httpProvider.
    $httpProvider.interceptors.push('MyHttpInterceptor');
});

comercialApp.controller('comercialController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'REQUERIMIENTOS';
    $scope.rutaImages = "Web/images/";
    $scope.diasCalendar = [];
    $scope.colaboradores = [];

    //Para la seleccion en el menú
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel-requerimientos").addClass('invisible');
    };

    $scope.IrRequerimientosComercial = function () {
        //ocultar();
        angular.element("#panel-requerimientos").removeClass('invisible');
        $scope.funcionalidad = 'GESTION DE REQUERIMIENTOS';
    };
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.cargarRequerimientos = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        var requerimientos = $http.post("comercial/requerimientos-comercial",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.fitroRequerimientos,
            });
        requerimientos.success(function (data) {
            if (data.success === true) {
                var posPagin = pagina;
                $scope.requerimientosLista = data.requerimientos;
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
                    var listaPaginador = angular.element("#tabla-requerimientos-comercial .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarRequerimientos();

    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.cargarRequerimientos(start, lenght);
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

    angular.element('#fecha-requerimiento').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    angular.element('#fecha-editar-requerimiento').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    var ajaxClientes = $http.post("catalogos/clientes", {});
    ajaxClientes.success(function (data) {
        if (data.success === true) {
            $scope.clientes = data.clientes;
        }
    });

    var ajaxRequerimientos = $http.post("comercial/catalogo-requerimientos", {});
    ajaxRequerimientos.success(function (data) {
        if (data.success === true) {
            $scope.requerimientos = data.requerimientos;
        }
    });
    $scope.windowAgregarRequerimiento = function () {
        $scope.habilitarTicket = false;
        $scope.clienteSeleccionado = '';
        $scope.pedReqSeleccionado = '';
        $scope.ticketSeleccionado = '';
        $scope.detalleSeleccionado = '';
        $scope.fechaSeleccionada = '';
        angular.element("#modal-agregar-requerimientos").modal("show");
        //document.getElementById("select-cliente-requerimiento").removeAttribute("disabled");
        //document.getElementById("detalle-requerimiento").removeAttribute("disabled");
        //document.getElementById("fecha-requerimiento").removeAttribute("disabled");
    };

    $scope.habilitarTicket = false; 
    $scope.ticketValido = true; // Estado inicial: el ticket es válido.
    let debounceTimeout = null; 
    // Función para permitir solo números
    $scope.validarSoloNumeros = function (event) {
        const charCode = event.which || event.keyCode;
        // Permitir solo números (48-57 son los códigos ASCII para 0-9)
        if (charCode < 48 || charCode > 57) {
            event.preventDefault();
        }
    };

    $scope.habilitarTicketCkc = function () {
        if ($scope.habilitarTicket == false) {
            $scope.ticketSeleccionado=""
            document.getElementById("select-cliente-requerimiento").removeAttribute("disabled");
            document.getElementById("detalle-requerimiento").removeAttribute("disabled");
            document.getElementById("fecha-requerimiento").removeAttribute("disabled");
        } else {
            document.getElementById("select-cliente-requerimiento").setAttribute("disabled", "disabled");
            document.getElementById("detalle-requerimiento").setAttribute("disabled", "disabled");
            document.getElementById("fecha-requerimiento").setAttribute("disabled", "disabled");
        }
    };

    $scope.habilitarTicketCkcE = function () {
        if ($scope.habilitarTicketE == false) {
            $scope.ticketSeleccionadoE=""
            document.getElementById("select-editar-cliente-requerimiento").removeAttribute("disabled");
            document.getElementById("detalle-editar-requerimiento").removeAttribute("disabled");
            document.getElementById("fecha-editar-requerimiento").removeAttribute("disabled");
        } else {
            document.getElementById("select-editar-cliente-requerimiento").setAttribute("disabled", "disabled");
            document.getElementById("detalle-editar-requerimiento").setAttribute("disabled", "disabled");
            document.getElementById("fecha-editar-requerimiento").setAttribute("disabled", "disabled");
        }
    };
     
    // Función de debounce para retrasar la validación
    $scope.debounceValidarTicket = function (ticket) {
        if (debounceTimeout) {
            clearTimeout(debounceTimeout); // Cancela el timeout anterior
        }

        debounceTimeout = setTimeout(function () {
            $scope.$apply(function () {
                $scope.validarTicket(ticket); // Llama a la validación real
            });
        }, 500); // Retraso de 500 ms
    };

    $scope.validarTicket = function (ticket) {
        waitingDialog.show('Validando el número de ticket ingresado...', { dialogSize: 'sm', progressType: 'success' });
        // Verificar que sea un número
        if (!ticket || isNaN(ticket)) {
            $scope.ticketValido = false;
            return;
        }

        var ajaxObtenerTicket = $http.post("tickets/dar-datos-ticket",
            {
                idTicket: ticket
            });

        ajaxObtenerTicket.success(function (data) {
            if (data.success === true) {
                waitingDialog.hide();
                $scope.clienteSeleccionado = data.datosTicket.idCliente;
                $scope.detalleSeleccionado = data.datosTicket.detalle;
                $scope.fechaSeleccionada = $scope.convertirFecha(data.datosTicket.fecha);
                document.getElementById("select-cliente-requerimiento").setAttribute("disabled", "disabled");
                document.getElementById("detalle-requerimiento").setAttribute("disabled", "disabled");
                document.getElementById("fecha-requerimiento").setAttribute("disabled", "disabled");
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.debounceValidarTicketE = function (ticket) {
        if (debounceTimeout) {
            clearTimeout(debounceTimeout); // Cancela el timeout anterior
        }

        debounceTimeout = setTimeout(function () {
            $scope.$apply(function () {
                $scope.validarTicketE(ticket); // Llama a la validación real
            });
        }, 500); // Retraso de 500 ms
    };


    $scope.validarTicketE = function (ticket) {
        waitingDialog.show('Validando el número de ticket ingresado...', { dialogSize: 'sm', progressType: 'success' });
        // Verificar que sea un número
        if (!ticket || isNaN(ticket)) {
            $scope.ticketValidoE = false;
            return;
        }

        var ajaxObtenerTicket = $http.post("tickets/dar-datos-ticket",
            {
                idTicket: ticket
            });

        ajaxObtenerTicket.success(function (data) {
            if (data.success === true) {
                waitingDialog.hide();
                $scope.clienteSeleccionadoE = data.datosTicket.idCliente;
                $scope.detalleSeleccionadoE = data.datosTicket.detalle;
                $scope.fechaSeleccionadaE = $scope.convertirFecha(data.datosTicket.fecha);
                document.getElementById("select-cliente-requerimiento").setAttribute("disabled", "disabled");
                document.getElementById("detalle-requerimiento").setAttribute("disabled", "disabled");
                document.getElementById("fecha-requerimiento").setAttribute("disabled", "disabled");
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.GuardarNuevoRequerimiento = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var fechaSe = angular.element("#fecha-requerimiento")[0].value;
        var datos = {
            cliente: $scope.clienteSeleccionado, 
            requerimiento: $scope.pedReqSeleccionado,
            ticket: $scope.ticketSeleccionado,
            detalle: $scope.detalleSeleccionado,
            fechaPedidoCliente: fechaSe
        };

        var insertReq = $http.post("comercial/guardar-requerimiento",
            datos);

        insertReq.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-requerimientos").modal("hide");
                $scope.cargarRequerimientos();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
        insertReq.error(function (data) {
            waitingDialog.hide();
            messageDialog.show('Error', 'Error en la petición por favor verifique que los datos sean correctos.');
        });
    };

    $scope.editarRequerimiento = function (secuencial) {
        $scope.secuencialRequerimiento = secuencial;
        var ajaxObtenerRequerimiento = $http.post("comercial/dar-datos-requerimiento",
            {
                secuencialRequerimiento: secuencial
            });
        ajaxObtenerRequerimiento.success(function (data) {
            if (data.success === true) {
                if (data.requerimientoResult.ticket != null) {
                    $scope.habilitarTicketE = true;
                    $scope.ticketSeleccionadoE = data.requerimientoResult.ticket;
                    document.getElementById("select-editar-cliente-requerimiento").setAttribute("disabled", "disabled");
                    document.getElementById("detalle-editar-requerimiento").setAttribute("disabled", "disabled");
                    document.getElementById("fecha-editar-requerimiento").setAttribute("disabled", "disabled");
                } else {
                    $scope.habilitarTicketE = false;
                    $scope.ticketSeleccionadoE = "";
                    document.getElementById("select-editar-cliente-requerimiento").removeAttribute("disabled");
                    document.getElementById("detalle-editar-requerimiento").removeAttribute("disabled");
                    document.getElementById("fecha-editar-requerimiento").removeAttribute("disabled");
                };
                $scope.clienteSeleccionadoE = data.requerimientoResult.clienteId;
                $scope.pedReqSeleccionadoE = data.requerimientoResult.requerimientoId;
                $scope.detalleSeleccionadoE = data.requerimientoResult.detalle;

                $scope.fechaSeleccionadaE = $scope.convertirFecha(data.requerimientoResult.fecha);
                angular.element('#fecha-editar-requerimiento').datepicker('setValue', new Date(data.requerimientoResult.fecha));
                angular.element("#modal-editar-requerimientos").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.modificarRequerimiento = function () {

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var fechaSe = angular.element("#fecha-editar-requerimiento")[0].value;
        var datos = {
            id: $scope.secuencialRequerimiento,
            cliente: $scope.clienteSeleccionadoE, // Usar el modelo Angular
            requerimiento: $scope.pedReqSeleccionadoE,
            ticket: $scope.ticketSeleccionadoE,
            detalle: $scope.detalleSeleccionadoE,
            fechaPedidoCliente: fechaSe
        };

        var insertReq = $http.post("comercial/editar-requerimiento",
            datos);

        insertReq.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-editar-requerimientos").modal("hide");
                $scope.cargarRequerimientos();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
        insertReq.error(function (data) {
            waitingDialog.hide();
            messageDialog.show('Error', 'Error en la petición por favor verifique que los datos sean correctos.');
        });
    };

    $scope.eliminarRequerimiento = function (secuencial) {
        console.log("en eliminar  " + secuencial);
        var ajaxObtenerRequerimiento = $http.post("comercial/eliminar-requerimiento",
            {
                id: secuencial
            });
        ajaxObtenerRequerimiento.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.cargarRequerimientos();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
        ajaxObtenerRequerimiento.error(function (data) {
            waitingDialog.hide();
            messageDialog.show('Error', 'Error en la petición por favor verifique que los datos sean correctos.');
        });
    };

    $scope.convertirFecha = function (fechaJson) {
        const timestamp = parseInt(fechaJson.match(/\d+/)[0], 10);
        const fecha = new Date(timestamp); // Crea un objeto Date

        const dia = String(fecha.getDate()).padStart(2, '0');
        const mes = String(fecha.getMonth() + 1).padStart(2, '0'); // Meses empiezan en 0
        const anio = fecha.getFullYear();

        return `${dia}/${mes}/${anio}`; // Retorna la fecha formateada
    };

}]);

    //Filter de angular para las fechas
    comercialApp.filter("strDateToStr", function () {
        return function (textDate) {
            if (textDate !== undefined) {
                var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
                return dateToStr(fecha);
            }
            return "";
        }
    });



