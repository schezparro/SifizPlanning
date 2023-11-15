adminApp.controller('catalogosController', ['$scope', '$http', function ($scope, $http) {
    $scope.nombreTabla = "";
    $scope.idTabla = 1;
    $scope.tuplaTabla = 0;
    $scope.tupla = null;

    //Seleccionar la tabla de catalogos
    $scope.seleccionarCatalogo = function () {
        $scope.idTabla = this.tabla.id;
        $scope.nombreTabla = this.tabla.nombre;
        cargarDatosTablaCatalogo();
    };
    angular.element('#lista-catalogos').on('click', '.tabla-catalogo', function () {
        angular.element('.tabla-catalogo').removeClass('tabla-catalogo-select');
        angular.element(this).addClass('tabla-catalogo-select');
    });

    function cargarDatosTablaCatalogo() {
        $scope.loading.show();
        $scope.tuplaTabla = 0;
        //Cargando los datos de la tabla
        var ajaxDatosCatalogos = $http.post("catalogos/datos-tabla",
            {
                idTabla: $scope.idTabla,
                nombreTabla: $scope.nombreTabla,
                filtro: $scope.campoFiltroCatalogo
            });
        ajaxDatosCatalogos.success(function (data) {
            if (data.success === true) {
                $scope.loading.hide();
                $scope.campos = data.propiedades;
                $scope.datosTabla = data.datos;

                if ($scope.nombreTabla == "MODULO") {

                    $scope.datosTabla.sort((dat1, dat2) => parseInt(dat1.d6) - parseInt(dat2.d6));
                }

            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
    }

    function cargarCatalogos() {
        //Cargando los catálogos
        var ajaxCatalogos = $http.post("catalogos/catalogos", {
            filtro: $scope.filtroCatalogos
        });
        ajaxCatalogos.success(function (data) {
            if (data.success === true) {
                $scope.tablasCatalogos = data.catalogos;
                $scope.campos = [];
                $scope.datosTabla = [];
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
    }

    //Seleccionando la tupla en la tabla
    $scope.seleccionarTuplaTabla = function () {
        $scope.tupla = this;
        $scope.tuplaTabla = this.data.d1;
    };
    angular.element('#edicion-catalogos').on('click', '#data-tabla-catalogos tbody tr', function () {
        angular.element('#data-tabla-catalogos tbody tr').removeClass('tuplaSelected');
        angular.element(this).addClass('tuplaSelected');
    });

    //Funcion para adicionar una tupla nueva a un catalogo
    $scope.adicionarTuplaCatalogo = function () {
        if ($scope.nombreTabla === "" || angular.element('.tabla-catalogo-select').length === 0) {
            messageDialog.show('Información', 'Por favor seleccione una tabla de catálogo.');
            return false;
        }
        angular.element("#data-campos-catalogos").html('');
        angular.element("#data-actividades-realizadas").html('');
        angular.element("#label-actividad").html('');
        angular.element("#modalCatalogoLabel").text("Gestión Tabla " + $scope.nombreTabla);

        var campos = $scope.campos;

        var i = 1;
        for (i; i < campos.length; i++) {
            var campo = campos[i];
            var nombreCampo = campo.nombreN;
            var textNombreCampo = campo.nombre;
            var tipoCampo = campo.tipoCampo;
            var relacion = campo.relacion;

            //Los elementos a adicionar
            var divFila = document.createElement('div');
            angular.element(divFila).addClass('fila');
            var labelCampo = document.createElement('label');
            angular.element(labelCampo).text(textNombreCampo + ':');
            var inputCampo;
            if (tipoCampo === "BOOL") {
                inputCampo = document.createElement('select');
                angular.element(inputCampo).html(
                    '<option value="1">SI</option> \
                    <option value="0">NO</option>'
                );
            }
            else if (relacion > 0) {
                inputCampo = document.createElement('select');
                //Cargando los datos de la tabla
                var ajaxDatosCombo = $http.post("catalogos/datos-descripcion-catalogo",
                    {
                        idTabla: relacion,
                        pos: i
                    });
                ajaxDatosCombo.success(function (data) {
                    if (data.success === true) {
                        for (var j = 0; j < data.datos.length; j++) {
                            dato = data.datos[j];
                            var option = document.createElement('option');
                            angular.element(option).attr({ value: dato.id });
                            angular.element(option).html(dato.desc);

                            var pos = data.pos;//Numero del elemento select que necesita los datos
                            var elementosHijos = angular.element('#data-campos-catalogos').children();
                            var div = elementosHijos[pos - 1];
                            var select = angular.element(div).find("select");

                            angular.element(select).append(option);
                        }
                    }
                    else {
                        alert("Error en el acceso a los datos.");
                    }
                });
            }
            else {

                if (nombreCampo == "Modulo") {

                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-modulo" });
                    var options = "";

                    var ajaxModulosSistema = $http.post("catalogos/modulos-sistema", {});

                    ajaxModulosSistema.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.datos.length; i++) {
                                var modulo = data.datos[i];

                                options += '<option value="' + modulo + '">' + modulo + '</option> \ '
                            }

                            angular.element("#input-modulo").html(options);
                        }
                        else {
                            messageDialog.show('Información', "Error en el acceso a los datos.");
                        }
                    });

                } else if (textNombreCampo == "TIPO DE MODULO") {

                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-tipo-modulo" });
                    var options = "";

                    var ajaxTiposModulos = $http.post("catalogos/tipos-modulos-sistema", {});

                    ajaxTiposModulos.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.datos.length; i++) {
                                var tipoModulo = data.datos[i];

                                options += '<option value="' + tipoModulo + '">' + tipoModulo + '</option> \ '
                            }

                            angular.element("#input-tipo-modulo").html(options);
                        }
                        else {
                            messageDialog.show('Información', data.msg);
                        }
                    });

                } else if ($scope.nombreTabla == "FUNCIONALIDAD CLIENTE") {

                    if (nombreCampo == "SecuencialCliente") {

                        inputCampo = document.createElement('select');
                        angular.element(inputCampo).attr({ id: "input-nombre-cliente" });
                        var options = "";

                        var ajaxClientes = $http.post("catalogos/nombres-clientes", {});

                        ajaxClientes.success(function (data) {
                            if (data.success === true) {
                                for (var i = 0; i < data.clientes.length; i++) {
                                    var cliente = data.clientes[i].nombre;

                                    options += '<option value="' + cliente + '">' + cliente + '</option> \ '
                                }

                                angular.element("#input-nombre-cliente").html(options);
                            }
                            else {
                                messageDialog.show('Información', "Error al cargar los clientes");
                            }
                        });
                    } else if (nombreCampo == "SecuencialFuncionalidad") {
                        inputCampo = document.createElement('select');
                        angular.element(inputCampo).attr({ id: "input-nombre-funcionalidad" });
                        var optionsFuncionalidades = "";

                        var ajaxFuncionalidades = $http.post("catalogos/nombres-funcionalidades", {});

                        ajaxFuncionalidades.success(function (data) {
                            if (data.success === true) {
                                for (var i = 0; i < data.funcionalidades.length; i++) {
                                    var funcionalidad = data.funcionalidades[i].nombre;

                                    optionsFuncionalidades += '<option value="' + funcionalidad + '">' + funcionalidad + '</option> \ '
                                }

                                angular.element("#input-nombre-funcionalidad").html(optionsFuncionalidades);
                            }
                            else {
                                messageDialog.show('Información', "Error al cargar las funcionalidades");
                            }
                        });
                    }
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialLiderProyecto") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-lider-proyecto" });
                    var optionsLiderProyecto = "";

                    var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});

                    ajaxColaboradores.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.colaboradores.length; i++) {
                                optionsLiderProyecto += '<option value="' + data.colaboradores[i].nombre + '">' + data.colaboradores[i].nombre + '</option> \ '
                            }

                            angular.element("#input-lider-proyecto").html(optionsLiderProyecto);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los colaboradores");
                        }
                    });
                }

                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "FechaProduccion") {
                    inputCampo = document.createElement('input');
                    angular.element(inputCampo).attr({ id: "input-fecha-produccion", type: "date" });
                }

                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialUbicacion") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-ubicacion-proyecto" });
                    var optionsUbicacion = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: false  });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsUbicacion += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }

                            angular.element("#input-ubicacion-proyecto").html(optionsUbicacion);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialResponsableAcceso") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-responsable-acceso" });
                    var optionsResponsableAcceso = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: true });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsResponsableAcceso += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }

                            angular.element("#input-responsable-acceso").html(optionsResponsableAcceso);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialResponsableCodigo") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-responsable-codigo" });
                    var optionsResponsableCodigo = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: false });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsResponsableCodigo += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }

                            angular.element("#input-responsable-codigo").html(optionsResponsableCodigo);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialResponsablePublicacion") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-responsable-publicacion" });
                    var optionsResponsablePublicacion = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: false });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsResponsablePublicacion += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }
                            angular.element("#input-responsable-publicacion").html(optionsResponsablePublicacion);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if (nombreCampo == "SecuencialColaborador") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-nombre-colaborador" });
                    var optionsColaboradores = "";

                    var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});

                    ajaxColaboradores.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.colaboradores.length; i++) {
                                optionsColaboradores += '<option value="' + data.colaboradores[i].nombre + '">' + data.colaboradores[i].nombre + '</option> \ '
                            }

                            angular.element("#input-nombre-colaborador").html(optionsColaboradores);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los colaboradores");
                        }
                    });
                }
                else {
                    inputCampo = document.createElement('input');
                }

            }
            angular.element(inputCampo).attr({ "ng-model": 'data-model-' + nombreCampo, "name": 'data-model-' + nombreCampo, "required": true });
            angular.element(inputCampo).addClass('form-control');

            angular.element(divFila).append(labelCampo);
            angular.element(divFila).append(inputCampo);

            angular.element("#data-campos-catalogos").append(divFila);
        }
        $scope.idTuplaCatalogo = 0;
        angular.element("#modalEditCatalogos").modal("show");

        //Cargando las actividades realizadas en la tabla actividad
        if ($scope.nombreTabla == "ACTIVIDAD") {

            var ajaxDatosActividadesRealizadas = $http.post("catalogos/datos-actividades-realizadas",
                {
                    actividad: ""
                });

            ajaxDatosActividadesRealizadas.success(function (data) {

                if (data.success === true) {

                    $scope.nuevaActividad = true;
                    var divPanel = document.createElement('div');
                    angular.element(divPanel).attr({ class: 'panel panel-default', style: 'overflow:scroll; height:300px', id: 'divPanel' });
                    var label = document.createElement('label');
                    angular.element(label).text("ACTIVIDADES REALIZADAS:");
                    angular.element("#label-actividad").append(label);

                    for (var i = 0; i < data.datos.length; i++) {

                        var divActividad = document.createElement('div');
                        angular.element(divActividad).attr({ class: "divDatosFiltro", style: 'width:110%' });

                        var checkBox = document.createElement('input');
                        angular.element(checkBox).attr({ "type": 'checkbox', value: data.datos[i].id, id: data.datos[i].nombre });
                        angular.element(divActividad).append(checkBox);
                        angular.element(divActividad).append(":" + data.datos[i].nombre);
                        angular.element(divPanel).append(divActividad);
                    }

                    angular.element('#data-actividades-realizadas').append(divPanel);
                }
                else {
                    alert("Error en el acceso a los datos.");
                }
            });
        }
    }

    //Funcion para editar una tupla en un catalogo en edicion
    $scope.editarTuplaCatalogo = function () {
        if ($scope.nombreTabla === "" || angular.element('.tabla-catalogo-select').length === 0) {
            messageDialog.show('Información', 'Por favor seleccione una tabla de catálogo.');
            return false;
        }

        if ($scope.tuplaTabla === 0) {
            messageDialog.show('Información', 'Por favor seleccione una fila de la tabla de catálogo.');
            return false;
        }

        angular.element("#data-campos-catalogos").html('');
        angular.element("#data-actividades-realizadas").html('');
        angular.element("#label-actividad").html('');
        angular.element("#modalCatalogoLabel").text("Gestión Tabla " + $scope.nombreTabla);

        var campos = $scope.campos;
        //var celdas = angular.element($scope.tupla).children();        

        var i = 1;
        for (i; i < campos.length; i++) {
            var campo = campos[i];
            var nombreCampo = campo.nombreN;
            var textNombreCampo = campo.nombre;
            var tipoCampo = campo.tipoCampo;
            var relacion = campo.relacion;

            var clave = 'd' + (i + 1);
            var celda = $scope.tupla.data[clave];

            if ($scope.nombreTabla == "ACTIVIDAD") {
                var clave2 = 'd3';
                $scope.actividadTexto = $scope.tupla.data[clave2];
            }

            //Los elementos a adicionar
            var divFila = document.createElement('div');
            angular.element(divFila).addClass('fila');
            var labelCampo = document.createElement('label');
            angular.element(labelCampo).text(textNombreCampo + ':');
            var inputCampo;

            if (tipoCampo === "BOOL") {
                inputCampo = document.createElement('select');
                angular.element(inputCampo).html(
                    '<option value="1">SI</option> \
                    <option value="0">NO</option>'
                );
                var value = (celda === "SI") ? 1 : 0;
                angular.element(inputCampo).val(value);
            }
            else if (relacion > 0) {
                inputCampo = document.createElement('select');
                //Cargando los datos de la tabla
                var ajaxDatosCombo = $http.post("catalogos/datos-descripcion-catalogo",
                    {
                        idTabla: relacion,
                        pos: i
                    });
                ajaxDatosCombo.success(function (data) {
                    if (data.success === true) {
                        for (var j = 0; j < data.datos.length; j++) {
                            dato = data.datos[j];
                            var option = document.createElement('option');
                            angular.element(option).attr({ value: dato.id });
                            angular.element(option).html(dato.desc);

                            var pos = data.pos;//Numero del elemento select que necesita los datos
                            var elementosHijos = angular.element('#data-campos-catalogos').children();
                            var div = elementosHijos[pos - 1];
                            var select = angular.element(div).find("select");

                            angular.element(select).append(option);
                            var valorClave = 'd' + (pos + 1);
                            var valor = $scope.tupla.data[valorClave];
                            if (valor === dato.desc) {
                                angular.element(select).val(dato.id);
                            }
                        }
                    }
                    else {
                        //alert("Error en el acceso a los datos.");
                    }
                });

            }
            else {

                if (nombreCampo == "Modulo") {

                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-modulo" });
                    var options = "";

                    var ajaxModulosSistema = $http.post("catalogos/modulos-sistema", {});

                    ajaxModulosSistema.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.datos.length; i++) {
                                var modulo = data.datos[i];

                                options += '<option value="' + modulo + '">' + modulo + '</option> \ '
                            }

                            celda = $scope.tupla.data["d4"];

                            angular.element("#input-modulo").html(options);
                            angular.element("#input-modulo").val(celda);

                        }
                        else {
                            messageDialog.show('Información', "Error en el acceso a los datos.");
                        }
                    });

                } else if (textNombreCampo == "TIPO DE MODULO") {

                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-tipo-modulo" });
                    var options = "";

                    var ajaxTiposModulos = $http.post("catalogos/tipos-modulos-sistema", {});

                    ajaxTiposModulos.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.datos.length; i++) {
                                var tipoModulo = data.datos[i];

                                options += '<option value="' + tipoModulo + '">' + tipoModulo + '</option> \ '
                            }

                            angular.element("#input-tipo-modulo").html(options);
                            celda = $scope.tupla.data["d4"];
                            angular.element("#input-tipo-modulo").val(celda);
                        }
                        else {
                            messageDialog.show('Información', data.msg);
                        }
                    });

                } else if ($scope.nombreTabla == "FUNCIONALIDAD CLIENTE") {

                    if (nombreCampo == "SecuencialCliente") {

                        inputCampo = document.createElement('select');
                        angular.element(inputCampo).attr({ id: "input-nombre-cliente" });
                        var options = "";

                        var ajaxClientes = $http.post("catalogos/nombres-clientes", {});

                        ajaxClientes.success(function (data) {
                            if (data.success === true) {
                                for (var i = 0; i < data.clientes.length; i++) {
                                    var cliente = data.clientes[i].nombre;

                                    options += '<option value="' + cliente + '">' + cliente + '</option> \ '
                                }

                                angular.element("#input-nombre-cliente").html(options);
                                celda = $scope.tupla.data["d2"];
                                angular.element("#input-nombre-cliente").val(celda);
                            }
                            else {
                                messageDialog.show('Información', "Error al cargar los clientes");
                            }
                        });
                    } else if (nombreCampo == "SecuencialFuncionalidad") {
                        inputCampo = document.createElement('select');
                        angular.element(inputCampo).attr({ id: "input-nombre-funcionalidad" });
                        var optionsFuncionalidades = "";

                        var ajaxFuncionalidades = $http.post("catalogos/nombres-funcionalidades", {});

                        ajaxFuncionalidades.success(function (data) {
                            if (data.success === true) {
                                for (var i = 0; i < data.funcionalidades.length; i++) {
                                    var funcionalidad = data.funcionalidades[i].nombre;

                                    optionsFuncionalidades += '<option value="' + funcionalidad + '">' + funcionalidad + '</option> \ '
                                }

                                angular.element("#input-nombre-funcionalidad").html(optionsFuncionalidades);
                                celda = $scope.tupla.data["d3"];
                                angular.element("#input-nombre-funcionalidad").val(celda);
                            }
                            else {
                                messageDialog.show('Información', "Error al cargar las funcionalidades");
                            }
                        });
                    }
                }
                //else if ($scope.nombreTabla == "PROYECTOS" && nombreCampo == "SecuencialCliente") {
                //    inputCampo = document.createElement('select');
                //    angular.element(inputCampo).attr({ id: "input-nombre-cliente" });
                //    var options = "";

                //    var ajaxClientes = $http.post("catalogos/nombres-clientes", {});

                //    ajaxClientes.success(function (data) {
                //        if (data.success === true) {
                //            for (var i = 0; i < data.clientes.length; i++) {
                //                var cliente = data.clientes[i].nombre;

                //                options += '<option value="' + cliente + '">' + cliente + '</option> \ '
                //            }

                //            angular.element("#input-nombre-cliente").html(options);
                //        }
                //        else {
                //            messageDialog.show('Información', "Error al cargar los clientes");
                //        }
                //    });
                //}
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialLiderProyecto") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-lider-proyecto" });
                    var optionsLiderProyecto = "";

                    var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});
                    ajaxColaboradores.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.colaboradores.length; i++) {
                                optionsLiderProyecto += '<option value="' + data.colaboradores[i].nombre + '">' + data.colaboradores[i].nombre + '</option> \ '
                            }
                            angular.element("#input-lider-proyecto").html(optionsLiderProyecto);
                            celda = $scope.tupla.data["d6"];
                            angular.element("#input-lider-proyecto").val(celda);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los colaboradores");
                        }
                    });
                }

                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "FechaProduccion") {
                    inputCampo = document.createElement('input');
                    angular.element(inputCampo).attr({ id: "input-fecha-produccion", type: "date" });
                    document.body.appendChild(inputCampo);
                    celda = $scope.tupla.data["d93"];

                    var parts = celda.split("-");
                    var fecha = new Date(parts[2], parts[1] - 1, parts[0]);
                    var fechaIso = fecha.toISOString().substring(0, 10);
                    angular.element("#input-fecha-produccion").val(fechaIso);
                }

                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialUbicacion") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-ubicacion-proyecto" });
                    var optionsUbicacion = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: false });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsUbicacion += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }

                            angular.element("#input-ubicacion-proyecto").html(optionsUbicacion);
                            celda = $scope.tupla.data["d7"];
                            angular.element("#input-ubicacion-proyecto").val(celda);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialResponsableAcceso") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-responsable-acceso" });
                    var optionsResponsableAcceso = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: true });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsResponsableAcceso += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }

                            angular.element("#input-responsable-acceso").html(optionsResponsableAcceso);
                            celda = $scope.tupla.data["d9"];
                            angular.element("#input-responsable-acceso").val(celda);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialResponsableCodigo") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-responsable-codigo" });
                    var optionsResponsableCodigo = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: false });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsResponsableCodigo += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }

                            angular.element("#input-responsable-codigo").html(optionsResponsableCodigo);
                            celda = $scope.tupla.data["d90"];
                            angular.element("#input-responsable-codigo").val(celda);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                else if ($scope.nombreTabla == "CLIENTE COMPLEMENTO" && nombreCampo == "SecuencialResponsablePublicacion") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-responsable-publicacion" });
                    var optionsResponsablePublicacion = "";

                    var ajaxResponsables = $http.post("catalogos/responsables-proyecto", { esTecnico: false, todos: false });
                    ajaxResponsables.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.responsables.length; i++) {
                                optionsResponsablePublicacion += '<option value="' + data.responsables[i].nombre + '">' + data.responsables[i].nombre + '</option> \ '
                            }
                            angular.element("#input-responsable-publicacion").html(optionsResponsablePublicacion);
                            celda = $scope.tupla.data["d91"];
                            angular.element("#input-responsable-publicacion").val(celda);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los responsables");
                        }
                    });
                }
                //else if ($scope.nombreTabla == "PROYECTOS" && nombreCampo == "SecuencialGestor") {
                //    inputCampo = document.createElement('select');
                //    angular.element(inputCampo).attr({ id: "input-gestor" });
                //    var optionsColaboradores = "";

                //    var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});

                //    ajaxColaboradores.success(function (data) {
                //        if (data.success === true) {
                //            for (var i = 0; i < data.colaboradores.length; i++) {
                //                optionsColaboradores += '<option value="' + data.colaboradores[i].nombre + '">' + data.colaboradores[i].nombre + '</option> \ '
                //            }

                //            angular.element("#input-gestor").html(optionsColaboradores);
                //        }
                //        else {
                //            messageDialog.show('Información', "Error al cargar los colaboradores");
                //        }
                //    });
                //}
                else if (nombreCampo == "SecuencialColaborador") {
                    inputCampo = document.createElement('select');
                    angular.element(inputCampo).attr({ id: "input-nombre-colaborador" });
                    var optionsColaboradores = "";

                    var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});

                    ajaxColaboradores.success(function (data) {
                        if (data.success === true) {
                            for (var i = 0; i < data.colaboradores.length; i++) {
                                optionsColaboradores += '<option value="' + data.colaboradores[i].nombre + '">' + data.colaboradores[i].nombre + '</option> \ '
                            }

                            angular.element("#input-nombre-colaborador").html(optionsColaboradores);
                            celda = $scope.tupla.data["d3"];
                            angular.element("#input-nombre-colaborador").val(celda);
                        }
                        else {
                            messageDialog.show('Información', "Error al cargar los colaboradores");
                        }
                    });
                }
                else {
                    inputCampo = document.createElement('input');
                    angular.element(inputCampo).val(celda);
                }
            }


            angular.element(inputCampo).attr({ "ng-model": 'data-model-' + nombreCampo, "name": 'data-model-' + nombreCampo, "required": true });
            angular.element(inputCampo).addClass('form-control');

            angular.element(divFila).append(labelCampo);
            angular.element(divFila).append(inputCampo);

            angular.element("#data-campos-catalogos").append(divFila);
        }

        $scope.idTuplaCatalogo = $scope.tuplaTabla;

        //Buscando el NumeroVerificador
        var ajaxEnvioDatos = $http.post("catalogos/dar-verificador-catalogo",
            {
                tabla: $scope.nombreTabla,
                id: $scope.tuplaTabla
            });
        ajaxEnvioDatos.success(function (data) {
            if (data.success === true) {
                $scope.numVerificadorCatalogo = data.numero;
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
        angular.element("#modalEditCatalogos").modal("show");

        //Cargando las actividades realizadas en la tabla actividad
        if ($scope.nombreTabla == "ACTIVIDAD") {

            var ajaxDatosActividadesRealizadas = $http.post("catalogos/datos-actividades-realizadas",
                {
                    actividad: $scope.actividadTexto
                });

            ajaxDatosActividadesRealizadas.success(function (data) {

                if (data.success === true) {
                    $scope.nuevaActividad = false;

                    $scope.actividadId = data.idActividad;

                    var divPanel = document.createElement('div');
                    angular.element(divPanel).attr({ class: 'panel panel-default', style: 'overflow:scroll; height:300px', id: 'divPanel' });

                    var label = document.createElement('label');
                    angular.element(label).text("ACTIVIDADES REALIZADAS:");
                    angular.element("#label-actividad").append(label);

                    for (var i = 0; i < data.datos.length; i++) {

                        var divActividad = document.createElement('div');
                        angular.element(divActividad).attr({ style: 'width:110%' });

                        var checkBox = document.createElement('input');
                        angular.element(checkBox).attr({ "type": 'checkbox', value: data.datos[i].id, id: data.datos[i].nombre });
                        for (var j = 0; j < data.datosE.length; j++) {
                            if (data.datos[i].id == data.datosE[j].id) {
                                angular.element(checkBox).attr({ checked: true });
                            }
                        }
                        angular.element(divActividad).append(checkBox);
                        angular.element(divActividad).append(":" + data.datos[i].nombre);
                        angular.element(divPanel).append(divActividad);
                    }

                    angular.element('#data-actividades-realizadas').append(divPanel);
                }
                else {
                    alert("Error en el acceso a los datos.");
                }
            });
        }

    }

    //Adicionar o editar un catálogo.
    $scope.guardarCatalogo = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var campos = $scope.campos;

        var datos = {};
        datos.idTuplaCatalogo = $scope.idTuplaCatalogo;
        datos.verificador = $scope.numVerificadorCatalogo;

        var i = 1;
        for (i; i < campos.length; i++) {
            var campo = campos[i];
            var nombreCampo = campo.nombreN;
            var selector = '[ng-model="data-model-' + nombreCampo + '"]';
            var elementos = angular.element(selector);
            var elemento = elementos[0];
            datos[nombreCampo] = angular.element(elemento).val();
        }

        var datosEnvio = {
            tabla: $scope.nombreTabla,
            datos: datos
        };

        //Enviando los datos
        var ajaxEnvioDatos = $http.post("catalogos/guardar-datos-catalogo",
            {
                datos: angular.toJson(datosEnvio)
            });
        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {

                $scope.actividadTexto = data.nombreActividad;
                var codigoActividad = data.codigoActividad;

                //Si es la tabla actividad se envian sus actividades realizadas
                if ($scope.nombreTabla == "ACTIVIDAD") {

                    var hijos = angular.element('#divPanel').children();
                    var actividadesRealizadas = "";

                    for (var i = 0; i < hijos.length; i++) {
                        if (angular.element(hijos[i]).children()[0].checked) {
                            actividadesRealizadas += "|" + angular.element(hijos[i]).children()[0].attributes[2].value + ":" + angular.element(hijos[i]).children()[0].attributes[1].value;
                        }
                    }

                    if ($scope.nuevaActividad) {
                        $scope.actividadId = 0;
                    }

                    var ajaxEnvioActividadesRealizadas = $http.post("catalogos/guardar-actividades-realizadas",
                        {
                            idActividad: $scope.actividadId,
                            actividad: $scope.actividadTexto,
                            actividadNueva: $scope.nuevaActividad,
                            codigo: codigoActividad,
                            datos: actividadesRealizadas
                        });

                    ajaxEnvioActividadesRealizadas.success(function (data) {
                        if (!data.success === true) {
                            alert("Error en el acceso a los datos.");
                        }
                    });
                }

                mostrarAler(data.msg, "success");
                angular.element("#modalEditCatalogos").modal("hide");
                $scope.idTuplaCatalogo = 0;
                $scope.numVerificadorCatalogo = 0;
                cargarDatosTablaCatalogo();
            }
            else {
                messageDialog.show('Información', data.msg);
                if (data.errorVerificador === 1) {
                    angular.element("#modalEditCatalogos").modal("hide");
                }
            }

        });

    };

    //Funcion para eliminar una tupla de un catalogo
    $scope.eliminarTuplaCatalogo = function () {

        if ($scope.nombreTabla === "" || angular.element('.tabla-catalogo-select').length === 0) {
            messageDialog.show('Información', 'Por favor seleccione una tabla de catálogo.');
            return false;
        }

        if ($scope.tuplaTabla === 0) {
            messageDialog.show('Información', 'Por favor seleccione una fila de la tabla de catálogo.');
            return false;
        }

        waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'success' });

        //En caso de ser la tabla actividad se eliminan las actividades realizadas
        if ($scope.nombreTabla == "ACTIVIDAD") {
            var ajaxEliminarActividadesRealizadas = $http.post("catalogos/eliminar-tuplas-actividades-realizadas",
                {
                    idActividad: $scope.tuplaTabla
                });

            ajaxEliminarActividadesRealizadas.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    //Eliminando la tupla
                    var ajaxEnvioDatos = $http.post("catalogos/eliminar-tupla-catalogo",
                        {
                            tabla: $scope.nombreTabla,
                            id: $scope.tuplaTabla
                        });
                    ajaxEnvioDatos.success(function (data) {
                        waitingDialog.hide();
                        if (data.success === true) {
                            cargarDatosTablaCatalogo();
                            mostrarAler(data.msg, "success");
                        }
                        else {
                            mostrarAler(data.msg, "error");
                        }
                    });
                }
                else {
                    mostrarAler(data.msg, "error");
                }
            });
        }
        else {

            //Eliminando la tupla
            var ajaxEnvioDatos = $http.post("catalogos/eliminar-tupla-catalogo",
                {
                    tabla: $scope.nombreTabla,
                    id: $scope.tuplaTabla
                });
            ajaxEnvioDatos.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    cargarDatosTablaCatalogo();
                    mostrarAler(data.msg, "success");
                }
                else {
                    mostrarAler(data.msg, "error");
                }
            });
        }
    }

    //Funcion para el filter
    $scope.actualizarDataFiltro = function () {
        if ($scope.nombreTabla !== "") {
            cargarDatosTablaCatalogo();
        }
    };

    $scope.filtrarCatalogos = function () {
        cargarCatalogos();
    };

    //Funcion para los mensajes alert
    function mostrarAler(msg, type) {
        if (type === "success") {
            $("#divMsgSuccess").html(msg);
            $("#divMsgSuccess").show(300, function () {
                $("#divMsgSuccess").fadeOut(3000);
            });
        }
        else {
            $("#divMsgDanger").html(msg);
            $("#divMsgDanger").show(300, function () {
                $("#divMsgDanger").fadeOut(5000);
            });
        }
    }
}]);