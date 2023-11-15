consultasApp.controller('modulosClientesController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {


    var numerosPorPagina = 10;
    var pagina = 1;
    var globalOrder = 0;
    var globalAsc = 0;

    var orderStatus = [1, 1, 1, 1];//Estado para cada unos de los ordenes del filtro

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

    $scope.actualizarCantidadMostrar = function () {
        numerosPorPagina = $scope.cantidadMostrarPorPagina;
        $scope.paginar();
    }

    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.recargarModulosClientes(start, lenght, "", globalOrder, globalAsc);
    };

    var filterModulosClientes = {
        cliente: '',
        modulo: '',
        submod: '',
        estado: '',
    };

    $scope.ordenarColumna = function (valor) {
        globalOrder = valor;
        globalAsc = orderStatus[valor - 1];
        orderStatus[valor - 1] = 1 - orderStatus[valor - 1];

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        angular.element(".ordenar-columna i").addClass('invisible');

        var columnas = angular.element(".ordenar-columna");
        var columna = columnas[valor - 1];
        var flecha = angular.element(columna).find("i");

        angular.element(flecha).removeClass('invisible');

        if (globalAsc === 0) {
            angular.element(flecha).removeClass('glyphicon-chevron-up');
            angular.element(flecha).addClass('glyphicon-chevron-down');
        }
        else {
            angular.element(flecha).addClass('glyphicon-chevron-up');
            angular.element(flecha).removeClass('glyphicon-chevron-down');
        }
        $scope.recargarModulosClientes(start, lenght, "", globalOrder, globalAsc);
    };


    $scope.filterData = function (valor) {

        if ($scope.cliente != undefined) {
            filterModulosClientes.cliente = $scope.cliente;
        }

        if ($scope.modulo != undefined) {
            filterModulosClientes.modulo = $scope.modulo;
        }

        if ($scope.subMod != undefined) {
            filterModulosClientes.submod = $scope.subMod;
        }

        if ($scope.estado != undefined) {
            filterModulosClientes.estado = $scope.estado;
        }

        switch (valor) {
            case 1:
                filterModulosClientes.cliente = $scope.cliente;
                $scope.FiltroNumero = false;
                break;
            case 2:
                filterModulosClientes.modulo = $scope.modulo;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterModulosClientes.submod = $scope.subMod;
                $scope.FiltroNumero = false;
                break;
            case 4:
                var filtroEstado = angular.element('#select-estado-modulo-cliente').val();

                if (filtroEstado === "Seleccione...") {
                    filterModulosClientes.estado = "";
                } else {
                    filterModulosClientes.estado = filtroEstado;
                }
        };

        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;

        $scope.recargarModulosClientes(start, lenght, "", globalOrder, globalAsc);
    };

    angular.element('#select-estado-modulo-cliente').on('click', function (e) {
        $scope.filterData(3);
    });

    $scope.recargarModulosClientes = function (start, lenght, filtro, order, asc) {

        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;
        if (filtro === undefined || filtro === "") {

            if (filterModulosClientes.cliente == undefined)
                filterModulosClientes.cliente = '';
            if (filterModulosClientes.modulo == undefined)
                filterModulosClientes.modulo = '';
            if (filterModulosClientes.submod == undefined)
                filterModulosClientes.submod = '';
            if (filterModulosClientes.estado == undefined)
                filterModulosClientes.estado = '';

            filtro = angular.toJson(filterModulosClientes)
        }

        if (order === undefined)
            order = 0;
        if (asc === undefined)
            asc = 1;

        var ajaxModulosClientes = $http.post("task/cargar-modulos-clientes", {
            start: start,
            lenght: lenght,
            filtro: filtro,
            order: order,
            asc: asc
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

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.guardarDatosModuloCliente = function () {

        var cliente = $scope.clienteEditar;
        var modulo = $scope.moduloEditar;
        var estado = angular.element("#select-estado-modulo-editar")[0].value;
        var subModulo = $scope.subModulo;
        var checksFuncionalidades = angular.element("#data-funcionalidades")[0].children[0].children;
        var funcionalidades = "";

        for (var i = 0; i < checksFuncionalidades.length; i++) {
            if (checksFuncionalidades[i].children[0].checked) {

                var id = checksFuncionalidades[i].children[0].attributes[1].value;
                var nombre = checksFuncionalidades[i].children[0].attributes[2].value;

                funcionalidades = funcionalidades + "/" + id + ":" + nombre;
            }
        }

        var ajaxObtenerDatosMC = $http.post("task/guardar-datos-modulo-cliente",
            {
                secuencialModuloCliente: $scope.secuencialModuloCliente,
                estado: estado,
                funcionalidades: funcionalidades,
                subModulo: subModulo
            });

        ajaxObtenerDatosMC.success(function (data) {
            if (data.success === true) {

                angular.element("#modal-editar-modulo-cliente").modal("hide");
                $scope.paginar();
                messageDialog.show("Información", "Se ha realizado la operación correctamente.");


            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    }

    $scope.AbrirModalEditarMC = function (secuencial) {

        $scope.secuencialModuloCliente = secuencial;

        var ajaxObtenerDatosSubModulo = $http.post("task/obtener-datos-modulo-cliente",
            {
                secuencialModuloCliente: secuencial
            });

        ajaxObtenerDatosSubModulo.success(function (data) {
            if (data.success === true) {
                $scope.submodulos = data.submodulos;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

        var ajaxObtenerDatosMC = $http.post("task/obtener-datos-modulo-cliente",
            {
                secuencialModuloCliente: secuencial
            });

        ajaxObtenerDatosMC.success(function (data) {
            if (data.success === true) {

                angular.element("#data-funcionalidades").html('');
                angular.element("#data-adicionar-funcionalidades").html('');

                var divPanel = document.createElement('div');
                angular.element(divPanel).attr({ class: 'panel panel-default', style: 'overflow:scroll; height:300px', id: 'divPanel' });

                for (var i = 0; i < data.funcionalidades.length; i++) {

                    var divFuncionalidad = document.createElement('div');
                    angular.element(divFuncionalidad).attr({ style: 'width:110%' });

                    var checkBox = document.createElement('input');
                    angular.element(checkBox).attr({ "type": 'checkbox', value: data.funcionalidades[i].id, id: data.funcionalidades[i].nombre });
                    angular.element(checkBox).attr({ checked: true });

                    angular.element(divFuncionalidad).append(checkBox);
                    angular.element(divFuncionalidad).append(":" + data.funcionalidades[i].nombre);
                    angular.element(divPanel).append(divFuncionalidad);
                }

                $scope.panelFuncionalidades = divPanel;

                angular.element('#data-funcionalidades').append(divPanel);

                var selectEstado = angular.element("#select-estado-modulo-editar")[0];

                $scope.clienteEditar = data.datos[0].cliente;
                $scope.moduloEditar = data.datos[0].modulo;
                selectEstado.value = data.datos[0].estado;
                $scope.subModulo = data.datos[0].subModulo != null ? data.datos[0].subModulo : 0;

                var s = angular.element("#modal-editar-modulo-cliente").modal("show");

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.AdicionarFuncionalidad = function () {

        angular.element("#data-adicionar-funcionalidades").html('');
        var idFuncionalidades = [];

        if ($scope.panelFuncionalidades.children.length > 0) {
            var funcionalidadesActivas = $scope.panelFuncionalidades.children;

            for (var i = 0; i < funcionalidadesActivas.length; i++) {
                idFuncionalidades.push(funcionalidadesActivas[i].children[0].attributes[1].value);
            }
        }

        var ajaxFuncionalidadesExtra = $http.post("task/obtener-funcionalidades-extra",
            {
                funcionalidades: idFuncionalidades,
                modulo: $scope.moduloEditar
            });

        ajaxFuncionalidadesExtra.success(function (data) {
            if (data.success === true) {

                var divPanel = document.createElement('div');
                angular.element(divPanel).attr({ class: 'panel panel-default', style: 'overflow:scroll; height:300px', id: 'divPanel' });

                if (data.funcionalidadesExtra.length > 0) {
                    for (var i = 0; i < data.funcionalidadesExtra.length; i++) {

                        var divFuncionalidad = document.createElement('div');
                        angular.element(divFuncionalidad).attr({ style: 'width:110%' });

                        var checkBox = document.createElement('input');
                        angular.element(checkBox).attr({ "type": 'checkbox', value: data.funcionalidadesExtra[i].id, id: data.funcionalidadesExtra[i].nombre });

                        angular.element(divFuncionalidad).append(checkBox);
                        angular.element(divFuncionalidad).append(":" + data.funcionalidadesExtra[i].nombre);
                        angular.element(divPanel).append(divFuncionalidad);
                    }

                    angular.element('#data-adicionar-funcionalidades').append(divPanel);

                    angular.element("#modal-adicionar-funcionalidad").modal("show");
                } else {
                    messageDialog.show('Informacion', "No se han encontrado funcionalidades, verifique que existan funcionalidades del modulo '" + $scope.moduloEditar + "' sin agregar al cliente '" + $scope.clienteEditar + "'")
                }

            } else {
                messageDialog.show("Información", data.msg);
            }
        });

    }

    $scope.AumentarFuncionalidades = function () {

        var checksFuncionalidadesExtra = angular.element("#data-adicionar-funcionalidades")[0].children[0].children;
        var divPanel = $scope.panelFuncionalidades;

        for (var i = 0; i < checksFuncionalidadesExtra.length; i++) {
            if (checksFuncionalidadesExtra[i].children[0].checked) {
                var id = checksFuncionalidadesExtra[i].children[0].attributes[1].value;
                var nombre = checksFuncionalidadesExtra[i].children[0].attributes[2].value;

                var divFuncionalidad = document.createElement('div');
                angular.element(divFuncionalidad).attr({ style: 'width:110%' });

                var checkBox = document.createElement('input');
                angular.element(checkBox).attr({ "type": 'checkbox', value: id, id: nombre });
                angular.element(checkBox).attr({ checked: true });

                angular.element(divFuncionalidad).append(checkBox);
                angular.element(divFuncionalidad).append(":" + nombre);
                angular.element(divPanel).append(divFuncionalidad);

            }
        }

        angular.element('#data-funcionalidades').append(divPanel);
        angular.element("#modal-adicionar-funcionalidad").modal("hide");

    }

}]);