var consultasApp = angular.module('consultas', ['ngRoute']);

consultasApp.config(function ($provide, $httpProvider, $routeProvider) {

    $(function () {
        $('[data-toggle="popover"]').popover()
    })
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
                var x = false;
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

consultasApp.controller('mainConsultasController', ['$scope', '$http', function ($scope, $http) {

    $scope.funcionalidad = 'MODULOS POR CLIENTE';

    var orderStatus = [1, 1, 1, 1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('[role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel_modulos_clientes").addClass('invisible');
        angular.element("#panel_actas_contratos").addClass('invisible');
        angular.element("#panel_consulta_contratos").addClass('invisible');
        angular.element("#panel_consulta_garantias").addClass('invisible');
        angular.element("#panel_garantia_tickets").addClass('invisible');
        angular.element("#panel_oferta_tickets").addClass('invisible');
    };

    //Funciones de Menu

    $scope.IrModulosClientes = function () {
        ocultar();
        angular.element("#panel_modulos_clientes").removeClass('invisible');
        $scope.funcionalidad = 'MODULOS POR CLIENTE';

        cargarDatosModulosClientes();
    };

    $scope.IrActasContratos = function () {
        ocultar();
        angular.element("#panel_actas_contratos").removeClass('invisible');
        $scope.funcionalidad = 'ACTAS DE LOS CONTRATOS';

        cargarDatosActas();
    };

    $scope.IrConsultaContratos = function () {
        ocultar();
        angular.element("#panel_consulta_contratos").removeClass('invisible');
        $scope.funcionalidad = 'CONTRATOS';

        cargarDatosContratos();
    };

    $scope.IrConsultaGarantias = function () {
        ocultar();
        angular.element("#panel_consulta_garantias").removeClass('invisible');
        $scope.funcionalidad = 'GARANTIAS';

        cargarDatosGarantias();
    };

    $scope.IrGarantiaTickets = function () {
        ocultar();
        angular.element("#panel_garantia_tickets").removeClass('invisible');
        $scope.funcionalidad = 'GARANTIA TICKETS';

        cargarDatosTickets();
    };

    $scope.IrOfertaTickets = function () {
        ocultar();
        angular.element("#panel_oferta_tickets").removeClass('invisible');
        $scope.funcionalidad = 'VER OFERTAS';

        $scope.cargarDatosOfertas();
    };

    //PARA LOS CLIENTES
    cargarDatosModulosClientes = function () {

        var numerosPorPagina = 10;
        var pagina = 1;
        $scope.listaPaginas = [];

        var ajaxEstadosMC = $http.post("task/estados-modulos-clientes", {});

        ajaxEstadosMC.success(function (data) {
            if (data.success === true) {
                $scope.estados = data.estados;
            }
        });

        var filterModulosClientes = {
            cliente: '',
            modulo: '',
            submod: '',
            estado: '',
        };

        filtro = angular.toJson(filterModulosClientes);

        var ajaxModulosClientes = $http.post("task/cargar-modulos-clientes", {
            start: 0,
            lenght: 10,
            filtro: filtro
        });

        ajaxModulosClientes.success(function (data) {
            if (data.success === true) {
                $scope.datosModulosClientes = data.modulosClientes;
                $scope.totalElementos = data.cantidadModulosClientes;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadModulosClientes / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
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
                    var listaPaginador = angular.element("#tabla-consulta-modulos-clientes .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            }
        });

    }

    //PARA LAS ACTAS
    cargarDatosActas = function () {

        var ajaxTiposMotivoTrabajo = $http.post("catalogos/tipo-motivo-trabajo", {});

        ajaxTiposMotivoTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.tiposMotivoTrabajo = data.tipoMotivoTrabajo;
            }
        });

        var ajaxColaboradores = $http.post("catalogos/coordinadores", {});

        ajaxColaboradores.success(function (data) {
            if (data.success === true) {
                $scope.colaboradores = data.coordinadores;
            }
        });

        var ajaxTiposActas = $http.post("task/tipos-actas", {});

        ajaxTiposActas.success(function (data) {
            if (data.success === true) {
                $scope.tiposActas = data.tiposActas;
            }
        });

        var ajaxEstadosActas = $http.post("task/estados-actas", {});

        ajaxEstadosActas.success(function (data) {
            if (data.success === true) {
                $scope.estadosActas = data.estadosActas;
            }
        });

        var numerosPorPagina = 10;
        var pagina = 1;
        $scope.listaPaginas = [];

        var filterActas = {
            codigo: '',
            numeroContrato: '',
            cliente: '',
            asunto: '',
            fecha: '',
            colaborador: '',
            tipo: '',
            estado: '',
            linkOpenkm: '',
            contrato: localStorage.getItem("codCont") !== null ? localStorage.getItem("codCont") : ''
        };

        filtro = angular.toJson(filterActas);

        var ajaxActas = $http.post("task/cargar-actas", {
            start: 0,
            lenght: 10,
            filtro: filtro
        });

        ajaxActas.success(function (data) {
            if (data.success === true) {
                $scope.datosActas = data.actas;
                $scope.totalActas = data.cantidadActas;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadActas / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
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
                    var listaPaginador = angular.element("#tabla-actas-contratos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            }
        });

    }

    //PARA LOS CONTRATOS
    cargarDatosContratos = function () {

        var ajaxColaboradores = $http.post("catalogos/coordinadores", {});

        ajaxColaboradores.success(function (data) {
            if (data.success === true) {
                $scope.colaboradores = data.coordinadores;
            }
        });
        //Cargando los estados de los Contratos
        var ajaxEstadosContrato = $http.post("catalogos/estados-contrato", {});
        ajaxEstadosContrato.success(function (data) {
            if (data.success === true) {
                $scope.estadosContrato = data.estadosContrato;
            }
            else {
                messageDialog.show('Información', "Error en los datos de acceso");
            }
        });

        //Cargando las fases de los Contratos
        var ajaxFasesContrato = $http.post("catalogos/fases-contrato", {});
        ajaxFasesContrato.success(function (data) {
            if (data.success === true) {
                $scope.fasesContrato = data.fasesContrato;
            }
            else {
                messageDialog.show('Información', "Error en los datos de acceso");
            }
        });

        var ajaxTiposMotivoTrabajo = $http.post("catalogos/tipo-motivo-trabajo", {});

        ajaxTiposMotivoTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.tiposMotivoTrabajo = data.tipoMotivoTrabajo;
            }
        });

        var numerosPorPagina = 10;
        var pagina = 1;
        $scope.listaPaginas = [];

        var filterContratos = {
            codigo: '',
            cliente: '',
            descripcion: '',
            fechaInicio: '',
            fechaVencimiento: '',
            estado: '',
            diasRestantes: '',
            responsable: ''
        };

        filtro = angular.toJson(filterContratos);

        var ajaxContratos = $http.post("task/cargar-motivos-trabajo", {
            start: 0,
            lenght: 10,
            filtro: filtro
        });

        ajaxContratos.success(function (data) {
            if (data.success === true) {
                $scope.datosContratos = data.contratos;
                $scope.totalContratos = data.cantidadContratos;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadContratos / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
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
                    var listaPaginador = angular.element("#tabla-consulta-contratos .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    //PARA LAS GARANTIAS
    cargarDatosGarantias = function () {

        var ajaxTiposMotivoTrabajo = $http.post("catalogos/tipo-motivo-trabajo", {});

        ajaxTiposMotivoTrabajo.success(function (data) {
            if (data.success === true) {
                $scope.tiposMotivoTrabajo = data.tipoMotivoTrabajo;
            }
        });

        var numerosPorPagina = 10;
        var pagina = 1;
        $scope.listaPaginas = [];

        var filterGarantias = {
            codigo: '',
            cliente: '',
            descripcion: '',
            fechaProduccion: '',
            fechaVencimiento: '',
            diasRestantes: ''
        };

        filtro = angular.toJson(filterGarantias);

        var ajaxGarantias = $http.post("task/cargar-garantias-trabajo", {
            start: 0,
            lenght: 10,
            filtro: filtro
        });

        ajaxGarantias.success(function (data) {
            if (data.success === true) {
                $scope.datosGarantias = data.garantias;
                $scope.totalGarantias = data.cantidadGarantias;

                if (data.garantias[0].entregables)
                    $scope.entregablesPorContrato = data.garantias[0].entregables;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadGarantias / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
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
                    var listaPaginador = angular.element("#tabla-consulta-garantias .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    //PARA LOS TICKETS
    cargarDatosTickets = function () {
        waitingDialog.show('Cargando lista de Tickets..', { dialogSize: 'sm', progressType: 'success' });

        var numerosPorPagina = 10;
        var pagina = 1;
        $scope.listaPaginas = [];

        var filterTickets = {
            noTicket: '',
            cliente: '',
            asunto: '',
            asignado: '',
            fecha: '',
            fechaVencimiento: '',
            diasRestantes: ''
        };

        filtro = angular.toJson(filterTickets);

        var ajaxTickets = $http.post("consultas/cargar-garantia-tickets", {
            start: 0,
            lenght: 10,
            filtro: filtro
        });

        ajaxTickets.success(function (data) {
            if (data.success === true) {
                waitingDialog.hide();
                $scope.datosTickets = data.tickets;
                $scope.totalTickets = data.totalTickets;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.totalTickets / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
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
                    var listaPaginador = angular.element("#tabla-garantia-tickets .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.cargarDatosOfertas = function () {
        //Cargando los clientes
        var ajaxClientes = $http.post("catalogos/clientes", {});
        ajaxClientes.success(function (data) {
            if (data.success === true) {
                $scope.clientes = data.clientes;
                console.log($scope.clientes);

                $scope.clientes = $scope.clientes.filter(function (c) { return c.id != 78; });
            }
        });

        //Cargando los colaboradores
        var ajaxColaboradores = $http.post("catalogos/coordinadores", {});
        ajaxColaboradores.success(function (data) {
            if (data.success === true) {
                $scope.colaboradores = data.coordinadores;
                console.log($scope.colaboradores);
                $scope.colaboradores = $scope.colaboradores.filter(function (c) { return c.id != 2122; });
            }
        });

        $scope.getClienteNombre = function (secuencialCliente) {
            var cliente = $scope.clientes.find(function (cliente) {
                return cliente.Secuencial === secuencialCliente;
            });

            return cliente ? cliente.Nombre : '';
        };

        $scope.getColaboradorNombre = function (secuencialColaborador) {
            var colaborador = $scope.colaboradores.find(function (colaborador) {
                return colaborador.Secuencial === secuencialColaborador;
            });

            return colaborador ? colaborador.Nombre : '';
        };

        var ajaxOfertas = $http.post("consultas/dar-ofertas-tickets");
        ajaxOfertas.success(function (data) {
            if (data.success === true) {
                $scope.ofertas = data.ofertas;
                console.log(data.ofertas);
            }
        });

    }

    angular.element('#fecha-registro').datepicker({
        format: 'dd/mm/yyyy',
        language: 'es'
    });

    angular.element('#fecha-produccion').datepicker({
        format: 'dd/mm/yyyy',
        language: 'es'
    });

    angular.element('#fecha-disponibilidad').datepicker({
        format: 'dd/mm/yyyy',
        language: 'es'
    });

    angular.element('#adenda-fechaVencimiento').datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element('#adenda-fechaVencimiento-Contrato').datepicker({
        format: 'dd/mm/yyyy'
    });
    angular.element('#fecha-seguimiento').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fecha-adenda-editar').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fechaProduccion').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    angular.element('#fechaActa').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });

    if (localStorage.getItem("codCont") !== "null" && localStorage.getItem("codCont") !== '') {
        $scope.IrActasContratos();
        $scope.numeroContrato = localStorage.getItem("codCont");
        $scope.contrato = localStorage.getItem("codCont");
        angular.element('#li-ir-actas').addClass('active');
        localStorage.setItem("codCont", '');
    } else {
        /**********************************************/
        //Carga Inicial
        angular.element('#li-ir-modulos-clientes').addClass('active');
        var numerosPorPagina = 10;
        var pagina = 1;

        var ajaxEstadosMC = $http.post("task/estados-modulos-clientes", {});

        ajaxEstadosMC.success(function (data) {
            if (data.success === true) {
                $scope.estados = data.estados;
            }
        });

        var filterModulosClientes = {
            cliente: '',
            modulo: '',
            submod: '',
            estado: '',
        };

        var ajaxModulosClientes = $http.post("task/cargar-modulos-clientes", {
            start: 0,
            lenght: 10,
            filtro: angular.toJson(filterModulosClientes)
        });

        ajaxModulosClientes.success(function (data) {
            if (data.success === true) {
                $scope.datosModulosClientes = data.modulosClientes;
                $scope.totalElementos = data.cantidadModulosClientes;

                var posPagin = pagina;
                $scope.cantPaginas = Math.ceil(data.cantidadModulosClientes / numerosPorPagina);

                if ($scope.cantPaginas === 0 || $scope.cantPaginas === undefined) {
                    $scope.cantPaginas = 1;
                }

                $scope.listaPaginas = [];
                if ($scope.cantPaginas > 5 && pagina <= 5) {
                    for (var i = 1; i <= 5; i++) {
                        $scope.listaPaginas.push(i);
                    }
                }
                else if ($scope.cantPaginas <= 5) {
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
                    var listaPaginador = angular.element("#tabla-consulta-modulos-clientes .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);

            }
        });

    }


}]);

//Filters
//Filter de angular para las fechas
consultasApp.filter("strDateToStr", function () {
    return function (textDate) {
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStr(fecha);
    }
});

consultasApp.filter("soloLetras", function () {
    return function (textDate) {
        if (esletra(textDate)) {
            return textDate;
        } else {
            return "No Tiene";
        }
    };
});

function esletra(caracter) {
    let ascii = caracter.toUpperCase().charCodeAt(0);
    return ascii > 64 && ascii < 91;
};

consultasApp.filter("trim20", function () {
    return function (textDate) {
        if (textDate.length > 20)
            return textDate.slice(0, 20) + "...";
        else
            return textDate;
    };
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

//Filter de angular para las fechas y Hora
consultasApp.filter("strDateToStrTime", function () {
    return function (textDate) {
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStrTime(fecha);
    }
});

function toStrTime(textDate) {
    var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
    return dateToStrTime(fecha);
}

function dateToStrTime(dateObj, format, separator) {
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

    var hour = dateObj.getHours();
    var hour = (hour < 10) ? '0' + hour : hour;
    var minutes = dateObj.getMinutes();
    var minutes = (minutes < 10) ? '0' + minutes : minutes;


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
    return out.join(sep) + " " + hour + ':' + minutes;
};

//filter para el usuario de el email
consultasApp.filter("userEmail", function () {
    return function (textEmail) {
        var array = textEmail.split('@');
        return array[0];
    }
});
