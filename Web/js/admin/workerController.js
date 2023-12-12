adminApp.controller('workerController', ['$scope', '$http', function ($scope, $http) {
    //Variables Iniciales
    $scope.idTrabajador = undefined;
    $scope.idTrabajadorEdit = 0;//Id del trabajador para la edicion
    $scope.rutaImages = "Web/images/";
    $scope.trabajadores = [];
    $scope.colaboradores = [];
    var filterWorkers = {
        nombre: '',
        email: '',
        departamento: '',
        cargo: '',
        sede: '',
        publicacionesDosCinco: '',
        usuario: ''
    };

    $scope.filterData = function (valor) {
        if ($scope.nombreFiltro !== undefined) {
            filterWorkers.nombre = $scope.nombreFiltro;
        }

        if ($scope.emailFiltro !== undefined) {
            filterWorkers.email = $scope.emailFiltro;
        }

        if ($scope.departamentoFiltro !== undefined) {
            filterWorkers.departamento = $scope.departamentoFiltro;
        }

        if ($scope.cargoFiltro !== undefined) {
            filterWorkers.cargo = $scope.cargoFiltro;
        }

        if ($scope.sedeFiltro !== undefined) {
            filterWorkers.sede = $scope.sedeFiltro;
        }

        if ($scope.usuarioFiltro !== undefined) {
            filterWorkers.usuario = $scope.usuarioFiltro;
        }

        switch (valor) {
            case 1:
                filterWorkers.nombre = $scope.nombreFiltro;
                $scope.FiltroNumero = false;
                break;
            case 2:
                filterWorkers.email = $scope.emailFiltro;
                $scope.FiltroNumero = false;
                break;
            case 3:
                filterWorkers.departamento = $scope.departamentoFiltro;
                $scope.FiltroNumero = false;
                break;
            case 4:
                filterWorkers.cargo = $scope.cargoFiltro;
                $scope.FiltroNumero = false;
                break;
            case 5:
                filterWorkers.sede = $scope.sedeFiltro;
                $scope.FiltroNumero = false;
                break;
            case 6:
                var filtroUsuario = angular.element('#select-usuario-activo').val();
                if (filtroUsuario === "Seleccione...") {
                    filterWorkers.usuario = "";
                } else {
                    filterWorkers.usuario = filtroUsuario;
                }
                $scope.FiltroNumero = false;
                break;
            case 7:
                var publicacionesDosCincoFiltro = angular.element('#select-publicaciones-doscinco').val();
                if (publicacionesDosCincoFiltro === "Seleccione...") {
                    filterWorkers.publicacionesDosCinco = "";
                } else {
                    filterWorkers.publicacionesDosCinco = publicacionesDosCincoFiltro;
                }
                $scope.FiltroNumero = false;
                break;
        }

        $scope.cargarTrabajadores(0, 0, '', $scope.filterWorkers);
    };

    angular.element('#select-usuario-activo').on('change', function (e) {
        $scope.filterData(6);
    });

    angular.element('#select-publicaciones-doscinco').on('change', function (e) {
        $scope.filterData(7);
    });

    //Funciones de carga de datos
    $scope.cargarTrabajadores = function (start, limit, filtroGeneral, filtro) {
        $scope.loading.show();

        if (filtro === undefined || filtro === "") {

            if (filterWorkers.nombre === undefined)
                filterWorkers.nombre = '';
            if (filterWorkers.email === undefined)
                filterWorkers.email = '';
            if (filterWorkers.departamento === undefined)
                filterWorkers.departamento = '';
            if (filterWorkers.cargo === undefined)
                filterWorkers.cargo = '';
            if (filterWorkers.sede === undefined)
                filterWorkers.sede = '';
            if (filterWorkers.publicacionesDosCinco === undefined)
                filterWorkers.publicacionesDosCinco = '';
            if (filterWorkers.usuario === undefined)
                filterWorkers.usuario = '';

            filtro = angular.toJson(filterWorkers)
        }
        //Cargando los trabajadores
        var ajaxTrabajadores = $http.post("admin/trabajadores/", {
            start: start,
            limit: limit,
            filtro: filtroGeneral,
            filtroColumna: filtro
        });
        ajaxTrabajadores.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.trabajadores = data.trabajadores;
                setTimeout(function () {
                    //Actualizando el tamaño de la tabla en height    
                    var tamanioY = angular.element("#panel_workers").height() - 35 - angular.element("#thead-colaboradores").height();
                    angular.element("#tbody-colaboradores").css({
                        height: tamanioY
                    });
                }, 100);
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };
    $scope.cargarTrabajadores(0, 0, '', $scope.filterWorkers);

    //Filtro del trabajador
    $scope.filtrarColaboradores = function () {
        $scope.cargarTrabajadores(0, 0, $scope.filtroColaboradores, $scope.filterWorkers);
    };

    //GESTION DE LOS TRABAJADORES
    //Seleccion de trabajadores
    angular.element("#panel_workers").on('click', '.table-tr', function () {
        angular.element('.table-tr').removeClass('tr-select');
        angular.element(this).addClass('tr-select');
        $scope.idTrabajador = angular.element(this).attr('data-id');
        var childrens = angular.element(this).children();
        $scope.nombreTrabajador = angular.element(childrens[1]).html();
        $scope.usuarioActivo = angular.element(childrens[6]).html() === "SI" ? 1 : 0;
    });

    angular.element("#modalNewWorker").on('hidden.bs.modal', function (e) {
        datosInicialesNewWorker();
    });

    function datosInicialesNewWorker() {
        angular.element("#modalNewWorker .form-control").val("");
        $scope.nombre1 = "";
        $scope.nombre2 = "";
        $scope.apellido1 = "";
        $scope.apellido2 = "";
        $scope.sexo = "";
        $scope.fecha_nac = "";
        $scope.fechaIngreso = "";
        $scope.nacionalidad = "";
        $scope.email = "";
        $scope.cargo = "";
        $scope.sede = "";
        //$scope.publicacionesDosCinco = "";
        $scope.departamento = "";

        angular.element("#button-borrar-foto").click();
        angular.element("#file-foto").val('');
        angular.element("#modalNewWorker").modal('hide');
        $scope.idTrabajadorEdit = 0;
        angular.element("#title-new-worker").html("Nuevo Colaborador");
    };

    ////Datos Iniciales del New Worker
    //angular.element("#btn-new-worker").click(function () {
    //    datosInicialesNewWorker();
    //});

    //Guardar el trabajador
    angular.element("#frm-newWorker").submit(function (event) {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();
        formData.append('foto', document.getElementById('file-foto').files[0]);
        formData.append('nombre1', $scope.nombre1);
        formData.append('nombre2', $scope.nombre2);
        formData.append('apellido1', $scope.apellido1);
        formData.append('apellido2', $scope.apellido2);
        formData.append('sexo', $scope.sexo);
        formData.append('fechaNac', angular.element('#dtpk-fecha_nac').val());
        formData.append('nacionalidad', $scope.nacionalidad);
        formData.append('email', $scope.email);
        formData.append('cargo', $scope.cargo);
        formData.append('sede', $scope.sede);
        formData.append('publicacionesDosCinco', $scope.publicacionesDosCinco);
        formData.append('departamento', $scope.departamento);
        formData.append('fechaIngreso', angular.element('#dtpk-fecha_ing').val());
        console.log(angular.element('#dtpk-fecha_ing').val());
        formData.append('idTrabajador', $scope.idTrabajadorEdit);

        var newWorker = $http.post("admin/nuevo-trabajador",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });

        newWorker.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modalNewTask").modal('hide');
                $scope.cargarTrabajadores(0, 0, '', $scope.filterWorkers);
                messageDialog.show('Información', data.msg);
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });

        return false;
    });

    //Edicion de Acceso del Trabajador
    $scope.modalEditarTrabajador = function () {
        if ($scope.idTrabajador == 0 || $scope.idTrabajador == undefined)
            messageDialog.show('Información', 'Por favor seleccione un trabajador.');
        else {
            var ajaxTrabajadores = $http.post("admin/rol-trabajador/", {
                idTrabajador: $scope.idTrabajador
            });
            ajaxTrabajadores.success(function (data) {
                if (data.success === true) {
                    $scope.usuarioTFS = data.usuarioTFS;
                    if (data.roles.length > 0) {
                        var checkBoxes = angular.element('#div-roles-colaboradores [type="checkbox"]');
                        angular.element(checkBoxes).prop('checked', false);

                        for (var i = 0; i < checkBoxes.length; i++) {
                            var checkBox = checkBoxes[i];
                            var value = angular.element(checkBox).val();
                            for (var j = 0; j < data.roles.length; j++) {
                                var rol = data.roles[j];
                                if (rol.id === parseInt(value)) {
                                    angular.element(checkBox).prop('checked', true);
                                }
                            }
                        }
                    }
                    //$scope.rol = data.rol.id;
                }
                else {
                    messageDialog.show('Información', "Error en el acceso a los datos.");
                }
            });
            angular.element("#modalEditUser").modal('show');
        }
    };

    //Guardar el Usuario
    angular.element("#frm-editUser").submit(function (event) {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        if ($scope.passw1 != "" && $scope.passw1 != $scope.passw2) {
            messageDialog.show("Información", "Las contraseñas no coinciden.");
            waitingDialog.hide();
            return false;
        }

        var idRoles = [];
        var checkedChekeds = angular.element('#div-roles-colaboradores [type="checkbox"]:checked');
        for (var i = 0; i < checkedChekeds.length; i++) {
            var checked = checkedChekeds[i];
            idRoles.push(angular.element(checked).val());
        }

        var formData = new FormData();

        if ($scope.passw1 === undefined)
            $scope.passw1 = "";
        if ($scope.passw2 === undefined)
            $scope.passw2 = "";

        formData.append('passw1', $scope.passw1);
        formData.append('passw2', $scope.passw2);
        formData.append('activo', $scope.usuarioActivo);
        formData.append('idTrabajador', $scope.idTrabajador);
        formData.append('usuarioTFS', $scope.usuarioTFS);
        formData.append('jsonRoles', angular.toJson(idRoles));

        var ajax = $http.post("admin/edit-user",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });

        ajax.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modalEditUser .form-control").val('');
                angular.element("#modalEditUser").modal('hide');
                messageDialog.show('Información', data.msg);
                $scope.cargarTrabajadores(0, 0, '', $scope.filterWorkers);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

        return false;
    });

    //Seleccion editando el trabajador
    $scope.editarTrabajador = function () {
        angular.element("#btn-new-worker").click();
        angular.element("#title-new-worker").html("Editar colaborador");
        $scope.idTrabajador = this.trab.idtrabajador;

        var ajax = $http.post("admin/datos-trabajador",
            { idTrabajador: this.trab.idtrabajador }
        );

        $scope.idTrabajadorEdit = $scope.idTrabajador;

        ajax.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.nombre1 = data.trabajador.nombre1;
                $scope.nombre2 = data.trabajador.nombre2;
                $scope.apellido1 = data.trabajador.apellido1;
                $scope.apellido2 = data.trabajador.apellido2;
                $scope.sexo = data.trabajador.sexo;
                $scope.fecha_nac = data.fechaNac;
                $scope.nacionalidad = data.trabajador.nacionalidad;
                $scope.sede = data.trabajador.sede;
                $scope.publicacionesDosCinco = data.trabajador.publicacionesDosCinco;
                $scope.cargo = data.trabajador.cargo;
                $scope.email = data.trabajador.email;
                $scope.departamento = data.trabajador.departamento;
                $scope.fechaIngreso = data.fechaIngreso;

                var foto = angular.element(".fileinput-preview")[0];
                $(foto).html('<img src="Web/images/' + data.trabajador.foto + '">');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Eliminando el trabajador
    angular.element("#panel_workers").on('click', '.eliminar-option', function () {
        var qMsgDialog = new questionMsgDialog();
        qMsgDialog.show('Información', 'Está seguro de querer eliminar el colaborador.', 'Si, estoy seguro', function () {
            var ajaxEliminar = $http.post("admin/eliminar-trabajador", {
                idTrabajador: $scope.idTrabajador
            });
            ajaxEliminar.success(function (data) {
                if (data.success === true) {
                    qMsgDialog.hide();
                    $scope.cargarTrabajadores(0, 0, '', $scope.filterWorkers);
                }
                messageDialog.show('Información', data.msg);
            });
        });

    });

    //Funciones o competencias de un trabajador
    var funcionesTrabajador = [];

    $scope.modalFuncionesTrabajador = function () {
        funcionesTrabajador = [];
        angular.element(".panel-valores-funciones").html("");

        if ($scope.idTrabajador == 0 || $scope.idTrabajador == undefined)
            messageDialog.show('Información', 'Por favor seleccione un trabajador.');
        else {
            var ajaxFunciones = $http.post("admin/funciones-trabajador/", {
                idTrabajador: $scope.idTrabajador
            });
            ajaxFunciones.success(function (data) {
                if (data.success === true) {
                    $scope.colaboradores = data.colaboradores;
                    var cant = data.funciones.length;
                    for (var i = 0; i < cant; i++) {
                        var func = data.funciones[i];
                        var divDato = document.createElement("div");
                        angular.element(divDato).attr({ class: "divDatosFiltro" });

                        var newCheck = document.createElement("input");
                        angular.element(newCheck).attr({ type: "checkbox", value: func.id, class: "filtroData", "data-text": func.especialidad });
                        if (func.activa === 1) {
                            angular.element(newCheck).attr({ checked: true });
                            var element = {
                                id: func.id,
                                text: func.especialidad
                            };
                            funcionesTrabajador.push(element);
                        }
                        angular.element(divDato).append(newCheck);
                        angular.element(divDato).append(":" + func.especialidad);

                        angular.element(".panel-valores-funciones").append(divDato);
                    }

                    setTimeout(function () {//Una espera para que cargue el Select
                        var cantCol = data.colaboradores.length;
                        for (var i = 0; i < cantCol; i++) {
                            var colab = data.colaboradores[i];
                            if (colab.jefe === 1) {
                                angular.element("#selectJefeInmediato").val(colab.id);
                                break;
                            }
                        }
                    }, 100);

                }
                else {
                    messageDialog.show('Información', "Error en el acceso a los datos.");
                }
            });
            angular.element("#modalEditFuncionesUser").modal('show');
        }
    };

    angular.element("#modalEditFuncionesUser").on('click', '.filtroData', function () {
        funcionesTrabajador = [];
        angular.element('#modalEditFuncionesUser .filtroData:checked').each(function () {
            var element = {
                id: angular.element(this).val(),
                text: angular.element(this).attr("data-text")
            };
            funcionesTrabajador.push(element);
        });
    });

    $scope.guardarFunciones = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var ajax = $http.post("admin/guardar-funciones-trabajador",
            {
                idTrabajador: $scope.idTrabajador,
                jsonFunciones: angular.toJson(funcionesTrabajador),
                idJefe: $scope.jefeInmediato
            }
        );

        ajax.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modalEditFuncionesUser .form-control").val('');
                angular.element("#modalEditFuncionesUser").modal('hide');
                messageDialog.show('Información', data.msg);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Nivel del Colaborador en Modulos
    $scope.modalNivelEnModulos = function () {
        if ($scope.idTrabajador == 0 || $scope.idTrabajador == undefined)
            messageDialog.show('Información', 'Por favor seleccione un trabajador.');
        else {
            var ajaxNivelesModulos = $http.post("admin/niveles-modulo-trabajador/", {
                idColaborador: $scope.idTrabajador
            });
            ajaxNivelesModulos.success(function (data) {
                if (data.success === true) {
                    $scope.nivelModulos = data.niveles;

                    setTimeout(function () {
                        var filas = angular.element(".nivel-star-colaborador");
                        angular.forEach(filas, function (fila, key) {
                            var nivelFila = angular.element(fila).attr('data-nivel');
                            var estrellasFila = angular.element(fila).children();

                            for (var i = nivelFila; i < 5; i++) {
                                angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                                angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                            }
                        });

                    }, 100);

                    angular.element("#modal-edit-nivel-modulo").modal('show');
                }
                else {
                    messageDialog.show(data.msg);
                }
            });
        }
    };

    angular.element("#tabla-nivel-colaborador-modulo").on('click', '.level-star', function () {
        var nivel = angular.element(this).index() + 1;
        var tdEstrellas = angular.element(this).parent();
        var idModulo = angular.element(tdEstrellas).attr('data-id-modulo');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerNivel = $http.post("admin/establecer-nivel-modulo-trabajador/", {
            idModulo: idModulo,
            idColaborador: $scope.idTrabajador,
            nivel: nivel
        });
        ajaxEstablecerNivel.success(function (data) {
            if (data.success === true) {
                for (var i = 0; i < 5; i++) {
                    if (i < nivel) {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star-empty');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star');
                    }
                    else {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                    }
                }
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

    angular.element("#tabla-nivel-colaborador-modulo").on('click', '.level-star-eliminar', function () {
        var nivel = 0;
        var tdEstrellas = angular.element(this).parent().prev();

        var idModulo = angular.element(tdEstrellas).attr('data-id-modulo');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerNivel = $http.post("admin/establecer-nivel-modulo-trabajador/", {
            idModulo: idModulo,
            idColaborador: $scope.idTrabajador,
            nivel: nivel
        });
        ajaxEstablecerNivel.success(function (data) {
            if (data.success === true) {
                angular.element(estrellasFila).removeClass('glyphicon-star');
                angular.element(estrellasFila).addClass('glyphicon-star-empty');
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

    //Nivel del Colaborador en Conocimiento
    $scope.modalCompetenciasGenerales = function (cambioCat) {
        if ($scope.idTrabajador == 0 || $scope.idTrabajador == undefined)
            messageDialog.show('Información', 'Por favor seleccione un trabajador.');
        else {
            //angular.element("#modal-edit-competencias-software").modal('show');
            if (cambioCat == 1) {
                $scope.subCategoriaSoftware = "";
            }
            var ajaxNivelesCompetencias = $http.post("admin/niveles-competencias-trabajador/", {
                idColaborador: $scope.idTrabajador,
                idCategoria: $scope.categoriaSoftware,
                idSubCategoria: $scope.subCategoriaSoftware
            });
            ajaxNivelesCompetencias.success(function (data) {
                if (data.success === true) {
                    $scope.categoriasSoft = data.categoriasSoft;
                    $scope.subCategoriasSoft = data.subCategoriasSoft;
                    $scope.competenciasSoft = data.competenciasSoft;

                    setTimeout(function () {
                        var filas = angular.element(".nivel-star-competencia-colaborador");
                        angular.forEach(filas, function (fila, key) {
                            var nivelFila = angular.element(fila).attr('data-nivel');
                            var estrellasFila = angular.element(fila).children();

                            for (var i = nivelFila; i < 5; i++) {
                                angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                                angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                            }
                        });

                        if ($scope.categoriaSoftware !== undefined)
                            angular.element('[ng-model="categoriaSoftware"]').val($scope.categoriaSoftware);
                        if ($scope.subCategoriaSoftware !== undefined)
                            angular.element('[ng-model="subCategoriaSoftware"]').val($scope.subCategoriaSoftware);

                        console.log($scope.categoriaSoftware);

                    }, 100);

                    angular.element("#modal-edit-competencias-software").modal('show');
                }
                else {
                    messageDialog.show(data.msg);
                }
            });
        }
    };

    angular.element("#tabla-nivel-competencias-software").on('click', '.level-star', function () {
        var nivel = angular.element(this).index() + 1;
        var tdEstrellas = angular.element(this).parent();
        var idCompetencia = angular.element(tdEstrellas).attr('data-id-competencia');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerNivel = $http.post("admin/establecer-nivel-competencia-trabajador/", {
            idCompetencia: idCompetencia,
            idColaborador: $scope.idTrabajador,
            nivel: nivel
        });
        ajaxEstablecerNivel.success(function (data) {
            if (data.success === true) {
                for (var i = 0; i < 5; i++) {
                    if (i < nivel) {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star-empty');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star');
                    }
                    else {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                    }
                }
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

    angular.element("#tabla-nivel-competencias-software").on('click', '.level-star-eliminar', function () {
        var nivel = 0;
        var tdEstrellas = angular.element(this).parent().prev();

        var idCompetencia = angular.element(tdEstrellas).attr('data-id-competencia');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerNivel = $http.post("admin/establecer-nivel-competencia-trabajador/", {
            idCompetencia: idCompetencia,
            idColaborador: $scope.idTrabajador,
            nivel: nivel
        });
        ajaxEstablecerNivel.success(function (data) {
            if (data.success === true) {
                angular.element(estrellasFila).removeClass('glyphicon-star');
                angular.element(estrellasFila).addClass('glyphicon-star-empty');
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

    //Nivel del Colaborador en Tecnologias
    $scope.modalNivelEnTecnologias = function () {
        if ($scope.idTrabajador == 0 || $scope.idTrabajador == undefined)
            messageDialog.show('Información', 'Por favor seleccione un trabajador.');
        else {
            
            var ajaxNivelesTecnologias = $http.post("admin/niveles-tecnologia-trabajador/", {
                idColaborador: $scope.idTrabajador,
            });

            ajaxNivelesTecnologias.success(function (data) {
                if (data.success === true) {
                    $scope.nivelTecnologias = data.niveles;

                    setTimeout(function () {
                        var filas = angular.element(".nivel-star-colaborador");
                        angular.forEach(filas, function (fila, key) {
                            var nivelFila = angular.element(fila).attr('data-nivel');
                            var estrellasFila = angular.element(fila).children();

                            for (var i = nivelFila; i < 5; i++) {
                                angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                                angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                            }
                        });

                    }, 100);

                    angular.element("#modal-edit-nivel-tecnologia").modal('show');
                }
                else {
                    messageDialog.show(data.msg);
                }
            });
        }
    };

    angular.element("#tabla-nivel-colaborador-tecnologia").on('click', '.level-star', function () {
        var nivel = angular.element(this).index() + 1;
        var tdEstrellas = angular.element(this).parent();
        var idTecnologia = angular.element(tdEstrellas).attr('data-id-tecnologia');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerNivel = $http.post("admin/establecer-nivel-tecnologia-trabajador/", {
            idTecnologia: idTecnologia,
            idColaborador: $scope.idTrabajador,
            nivel: nivel
        });
        ajaxEstablecerNivel.success(function (data) {
            if (data.success === true) {
                for (var i = 0; i < 5; i++) {
                    if (i < nivel) {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star-empty');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star');
                    }
                    else {
                        angular.element(estrellasFila[i]).removeClass('glyphicon-star');
                        angular.element(estrellasFila[i]).addClass('glyphicon-star-empty');
                    }
                }
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

    angular.element("#tabla-nivel-colaborador-tecnologia").on('click', '.level-star-eliminar', function () {
        var nivel = 0;
        var tdEstrellas = angular.element(this).parent().prev();

        var idTecnologia= angular.element(tdEstrellas).attr('data-id-tecnologia');
        var estrellasFila = angular.element(tdEstrellas).children();

        var ajaxEstablecerNivel = $http.post("admin/establecer-nivel-tecnologia-trabajador/", {
            idTecnologia: idTecnologia,
            idColaborador: $scope.idTrabajador,
            nivel: nivel
        });
        ajaxEstablecerNivel.success(function (data) {
            if (data.success === true) {
                angular.element(estrellasFila).removeClass('glyphicon-star');
                angular.element(estrellasFila).addClass('glyphicon-star-empty');
            }
            else {
                messageDialog.show(data.msg);
            }
        });

    });

}]);