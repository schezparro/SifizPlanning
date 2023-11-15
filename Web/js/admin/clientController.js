adminApp.controller('clientController', ['$scope', '$http', function ($scope, $http) {
    function recargarUsuariosClientes() {
        $scope.loading.show();
        var ajaxUsuariosClientes = $http.post("admin/usuarios-clientes", {
            filtro: $scope.filtroClientes
        });
        ajaxUsuariosClientes.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.usuariosClientes = data.usuariosClientes;
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
    }

    function inicializarWindowsAddClientes() {
        $scope.clientNombre1 = "";
        $scope.clientNombre2 = "";
        $scope.clientApellido1 = "";
        $scope.clientApellido2 = "";
        $scope.clientSexo = "";
        $scope.clientFecha_nac = "";
        $scope.clientNacionalidad = 0;
        $scope.clientEmail = "";
        $scope.clientCliente = 0;
        $scope.clientActivo = 0;
        $scope.clientPassw1 = "";
        $scope.clientPassw2 = "";
        $scope.clientIdClient = 0;
        $scope.clientVerificador = 0;
        $scope.clientTelefono = "";
    }

    angular.element("#modalNewClient").on('hidden.bs.modal', function (e) {
        $scope.$apply(function () {
            inicializarWindowsAddClientes();
        });
    });

    //Guardando el usuario cliente en el sistema
    $scope.guardarUsuarioClient = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var guardarCliente = $http.post("admin/guardar-cliente", {
            nombre1: $scope.clientNombre1,
            nombre2: $scope.clientNombre2,
            apellido1: $scope.clientApellido1,
            apellido2: $scope.clientApellido2,
            sexo: $scope.clientSexo,
            fechaNac: $scope.clientFecha_nac,
            nacionalidad: $scope.clientNacionalidad,
            email: $scope.clientEmail,
            cliente: $scope.clientCliente,
            userActivo: $scope.clientActivo,
            telefono: $scope.clientTelefono,
            pass1: $scope.clientPassw1,
            pass2: $scope.clientPassw2,

            idUsuarioCliente: $scope.clientIdClient,
            numeroVerificador: $scope.clientVerificador
        });
        guardarCliente.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                messageDialog.show('Información', data.msg);
                recargarUsuariosClientes();
                angular.element("#modalNewClient").modal('hide');
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });

    };

    $scope.filtrarUsuariosClientes = function () {
        recargarUsuariosClientes();
    };

    $scope.editarUsuarioClient = function () {
        var fecha = new Date(parseInt(this.client.fechaNac.replace('/Date(', '')));
        var fechaNac = dateToStr(fecha);

        $scope.clientNombre1 = this.client.nombre1;
        $scope.clientNombre2 = this.client.nombre2;
        $scope.clientApellido1 = this.client.apellido1;
        $scope.clientApellido2 = this.client.apellido2;
        $scope.clientSexo = this.client.sexo;

        $scope.clientFecha_nac = dateToStr(new Date(parseInt(this.client.fechaNac.replace('/Date(', ''))));;
        angular.element("#dtpk-fecha_nac-client").datepicker('update', $scope.clientFecha_nac);

        $scope.clientNacionalidad = this.client.nacionalidad;
        $scope.clientEmail = this.client.email;
        $scope.clientCliente = this.client.idCliente;
        $scope.clientActivo = this.client.userActivoNumber;
        $scope.clientTelefono = this.client.telefono;
        $scope.clientIdClient = this.client.idUser;
        $scope.clientVerificador = this.client.verificador;

        angular.element("#modalNewClient").modal('show');
    };

    $scope.eliminarUsuarioClient = function () {
        var idClienteUser = this.client.idUser;
        var qMsgDialog = new questionMsgDialog();
        qMsgDialog.show('Información', 'Está seguro de querer eliminar el usuario del cliente.', 'Si, estoy seguro', function () {
            waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'success' });
            var eliminarCliente = $http.post("admin/eliminar-usuario-cliente", {
                idUsuarioCliente: idClienteUser
            });
            eliminarCliente.success(function (data) {
                waitingDialog.hide();
                qMsgDialog.hide();
                if (data.success === true) {
                    messageDialog.show('Información', data.msg);
                    recargarUsuariosClientes();
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        });
    };

    //CONTRATOS DE LOS CLIENTES
    $scope.idClient = 0;
    $scope.nombreClient = 0;
    $scope.idContratoCliente = 0;
    $scope.verificadorContratoCliente = 0;
    $scope.adjuntos = [];

    function filtrarClientes() {
        $scope.loading.show();
        //Cargando los clientes
        var ajaxClientes = $http.post("catalogos/clientes", {
            filtro: $scope.filtroClientes
        });
        ajaxClientes.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.clientes = data.clientes;
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
    }

    function cargarContratosCliente() {
        $scope.loading.show();
        //Cargando los clientes
        var ajaxContratos = $http.post("admin/contratos-cliente", {
            idCliente: $scope.idClient,
            filtro: $scope.filtroClientes
        });
        ajaxContratos.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                $scope.contratosClientes = data.contratosClientes;
            }
            else {
                //alert("Error en el acceso a los datos.")
            }
        });
    }

    $scope.filtrarClientes = function () {
        filtrarClientes();
    };

    $scope.filtrarContratosClientes = function () {
        cargarContratosCliente();
    };

    //Seleccion de un cliente 
    $scope.verContratosClientes = function () {
        $scope.idClient = this.client.id;
        $scope.nombreClient = this.client.nombre;
        cargarContratosCliente();
    };
    angular.element("#lista-clientes").on("click", ".cliente-nombre", function () {
        angular.element('.cliente-nombre').removeClass('tabla-catalogo-select');
        angular.element(this).addClass('tabla-catalogo-select');
    });

    //Seleccion de una tupla de contrato
    $scope.seleccionarTuplaTabla = function () {
        var id = this.contrato.id;
    };
    angular.element('#data-tabla-contratos').on('click', 'tbody tr', function () {
        angular.element('#data-tabla-contratos tbody tr').removeClass('tuplaSelected');
        angular.element(this).addClass('tuplaSelected');
    });

    //Seleccion para editar una tupla de contrato
    $scope.verEditarContratoCliente = function () {
        var idContrato = this.contrato.id;
        var datosContrato = $http.post("admin/datos-contrato-cliente",
            {
                idContrato: idContrato
            });

        angular.element("#title-new-contract-client").html("Editar Contrato de Cliente");

        datosContrato.success(function (data) {
            $scope.clientCliente = data.contrato.idCliente;
            $scope.clientTipoContrato = data.contrato.idTipoContrato;

            $scope.clientFecha_inicio_contrat = dateToStr(new Date(parseInt(data.contrato.fechaInicio.replace('/Date(', ''))));
            angular.element("#dtpk-fecha_inicio_contrat").datepicker('update', $scope.clientFecha_inicio_contrat);

            $scope.clientFecha_fin_contrat = dateToStr(new Date(parseInt(data.contrato.fechaFin.replace('/Date(', ''))));
            angular.element("#dtpk-fecha_fin_contrat").datepicker('update', $scope.clientFecha_fin_contrat);

            $scope.contratoDescripcion = data.contrato.descripcion;
            $scope.verificadorContratoCliente = data.contrato.verificador;
            $scope.idContratoCliente = data.contrato.id;

            if (data.contrato.idTipoContrato == 1) {
                angular.element("#horas-mantenimiento").show();
                angular.element("#funcionalidades-contrato").hide();
                $scope.horasMantenimiento = data.contrato.horas;
            }
            else if (data.contrato.idTipoContrato == 2) {
                var funcionalidades = data.contrato.funcionalidades;

                angular.element("#horas-mantenimiento").hide();
                angular.element("#funcionalidades-contrato").show();

                angular.element("#div-check-funcionalidades").html("")

                //cargando el arbol desde la base de datos
                angular.element('#div-check-funcionalidades').jsonList({
                    url: 'admin/modulos-funcionalidades',
                    type: 'groupedItems',
                    groups: 'modulos',
                    items: 'funcionalidades',
                    // onListItem: called for each list item created from the JSON
                    onListItem: function (event, listItem, data, isGroup) {
                        if (!isGroup) {
                            // set the id into a data value so that Bonsai createInputs
                            // can set the checkbox value
                            listItem.attr('data-value', data.id);
                        }
                    },
                    // success: called after the list is created
                    onSuccess: function (event, jsonList) {
                        // turn the list into a tree
                        $(this.el).find('> ol').bonsai({
                            checkboxes: true,
                            createInputs: 'checkbox',
                            handleDuplicateCheckboxes: true,
                            expandAll: true
                        });

                        //Marcando los que pertenecen al contrato
                        var checkBoxes = angular.element('#funcionalidades-contrato [type="checkbox"]');
                        angular.element.each(checkBoxes, function (post, checkbox) {
                            var value = angular.element(checkbox).val();
                            if (value != "") {
                                for (var i = 0; i < funcionalidades.length; i++) {
                                    var func = funcionalidades[i];
                                    if (func.idFunc == value) {
                                        angular.element(checkbox).prop('checked', true);
                                        funcionalidades.splice(i, 1);
                                        break;
                                    }
                                }
                            }
                        });

                    }
                });
            }

            //Los archivos adjuntos
            $scope.adjuntos = data.contrato.adjuntos;

            angular.element("#modalNewContratClient").modal('show');
        });
    };

    //Adicionar un campo de fichero
    $scope.adicionarFileAnterior = function () {
        var listaBotones = angular.element("#barra-botones-file");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element('#deleteInputFileAnterior').removeAttr('disabled');
        }
    };

    //Eliminar un campo de fichero
    $scope.deleteFileAnterior = function () {
        var cantFileInput = angular.element(".file-adj-contrato:visible").length;
        if (cantFileInput > 1) {
            angular.element("#barra-botones-file").prev().remove();
            cantFileInput = angular.element(".file-adj-contrato:visible").length;
            if (cantFileInput < 2) {
                angular.element('#deleteInputFileAnterior').attr('disabled', 'disabled');
            }
        }
    };

    function cargarArbolFuncionalidades() {
        angular.element("#div-check-funcionalidades").html("")

        //cargando el arbol desde la base de datos
        angular.element('#div-check-funcionalidades').jsonList({
            url: 'admin/modulos-funcionalidades',
            type: 'groupedItems',
            groups: 'modulos',
            items: 'funcionalidades',
            // onListItem: called for each list item created from the JSON
            onListItem: function (event, listItem, data, isGroup) {
                if (!isGroup) {
                    // set the id into a data value so that Bonsai createInputs
                    // can set the checkbox value
                    listItem.attr('data-value', data.id);
                }
            },
            // success: called after the list is created
            onSuccess: function (event, jsonList) {
                // turn the list into a tree
                $(this.el).find('> ol').bonsai({
                    checkboxes: true,
                    createInputs: 'checkbox',
                    handleDuplicateCheckboxes: true,
                    expandAll: false
                });
            }
        });
    }
    //Seleccion del tipo de contrato
    $scope.selectTipoContrato = function () {
        if ($scope.clientTipoContrato == 1) {//Mantenimiento
            angular.element("#horas-mantenimiento").show();
            angular.element("#funcionalidades-contrato").hide();
        }
        else if ($scope.clientTipoContrato == 2) {
            angular.element("#horas-mantenimiento").hide();
            angular.element("#funcionalidades-contrato").show();
            cargarArbolFuncionalidades();
        }
    };

    //Para la seleccion en el tab
    angular.element('#tab-contrato-cliente').on('click', '[role="presentation"]', function () {
        angular.element('#tab-contrato-cliente [role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    $scope.vistaDatosGeneral = function () {
        angular.element("#contrato-general").fadeIn(0);
        angular.element("#info-segun-contrato").fadeOut(0);
    };

    $scope.vistaDatosContrato = function () {
        angular.element("#info-segun-contrato").fadeIn(0);
        angular.element("#contrato-general").fadeOut(0);
    };

    //Cuando se oculte la ventana
    angular.element("#modalNewContratClient").on('hidden.bs.modal', function (e) {
        $scope.$apply(function () {
            datosInicialesNewContractClient();
        });
    });

    function datosInicialesNewContractClient() {
        $scope.clientCliente = '';
        $scope.clientTipoContrato = '';
        $scope.clientFecha_inicio_contrat = '';
        $scope.clientFecha_fin_contrat = '';
        $scope.contratoDescripcion = '';
        $scope.idContratoCliente = 0;
        $scope.verificadorContratoCliente = 0;
        $scope.horasMantenimiento = '';

        angular.element("#horas-mantenimiento").hide();
        angular.element("#funcionalidades-contrato").hide();

        angular.element("#frm-newContrat .file-adj-contrato").remove();

        //Adicionando los los campos de file
        var listaBotones = angular.element("#barra-botones-file");
        var html = angular.element("#htmlFile").html();
        angular.element(html).insertBefore(listaBotones);

        angular.element("#title-new-contract-client").html("Nuevo Contrato de Cliente");
    }

    //Guardando el contrato
    $scope.guardarContratClient = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var formData = new FormData();

        var files = new Array();
        angular.element.each(angular.element('#frm-newContrat').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });

        formData.append('cliente', $scope.clientCliente);
        formData.append('tipoContrato', $scope.clientTipoContrato);
        formData.append('fechaInicio', $scope.clientFecha_inicio_contrat);
        formData.append('fechaFin', $scope.clientFecha_fin_contrat);
        formData.append('descripcion', $scope.contratoDescripcion);

        formData.append('idContratoCliente', $scope.idContratoCliente);
        formData.append('verificador', $scope.verificadorContratoCliente);

        if ($scope.clientTipoContrato == 1) {//Mantenimiento
            if ($scope.horasMantenimiento == undefined || $scope.horasMantenimiento == "") {
                messageDialog.show('Información', "Debe especificar las horas de mantenimiento");
                return false;
            }
            else {
                formData.append('horas', $scope.horasMantenimiento);
            }
        }
        else if ($scope.clientTipoContrato == 2) {//Desarrollo
            var checkBoxes = angular.element('#funcionalidades-contrato [type="checkbox"]:checked');
            var valoresCheck = new Array();
            angular.element.each(checkBoxes, function (post, checkbox) {
                if (angular.element(checkbox).val() != "") {
                    valoresCheck.push(angular.element(checkbox).val());
                    //formData.append( 'funcionalidades', valoresCheck );
                    formData.append('funcionalidades', angular.element(checkbox).val());
                }
            });

            if (valoresCheck.length === 0) {
                messageDialog.show('Información', "Debe especificar las funcionalidades del contrato");
                return false;
            }
        }

        var newContrato = $http.post("admin/nuevo-contrato-cliente",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });

        newContrato.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modalNewContratClient").modal('hide');
                cargarContratosCliente();
                messageDialog.show('Información', data.msg);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    //Eliminando los adjuntos
    $scope.eliminarAdjunto = function (idAdj) {
        waitingDialog.show('Eliminando...', { dialogSize: 'sm', progressType: 'success' });
        var deleteAdjunto = $http.post("admin/eliminar-adjunto-contrato",
            {
                idAdjunto: idAdj
            });
        deleteAdjunto.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                $scope.adjuntos = data.adjuntos;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

}]);
