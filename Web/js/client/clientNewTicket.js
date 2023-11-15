clientApp.controller('clientNewTicket', ['$scope', '$http', function ($scope, $http) {
    $scope.contratoSelectTicket = "";
    $scope.motivosTrabajoGarantia = "";
    $scope.telefonoTicket = "";

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

    //Cargando las prioridades
    var ajaxPrioridades = $http.post("catalogos/tickets-prioridades", {});
    ajaxPrioridades.success(function (data) {
        if (data.success === true) {
            $scope.prioridades = data.prioridades;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });

    //Cargando los tipos de version
    var ajaxTicketVersionCliente = $http.post("catalogos/ticketVersionCliente", {});
    ajaxTicketVersionCliente.success(function (data) {
        if (data.success === true) {
            $scope.ticketVersionClientes = data.ticketVersionClientes;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });

    //Limpiando la solicitud de los tickets
    $scope.limpiarFormularioSolicitud = function () {
        $scope.telefonoTicket = "";
        $scope.reportoTicket = "";
        $scope.detalleRequerimiento = "";
        $scope.prioridadTicket = "";
        $scope.categoriaTicket = "";
        $scope.asuntoRequerimiento = "";
        $scope.motivoPrioridad = "";
        $scope.motivoTrabajo = "";
        $scope.entregableGarantia = "";
        $scope.esGarantia = false;
        $scope.esContratoMantenimiento = false;
        $scope.contratoSeleccionado = false;
        $scope.isRequired = true;
        $scope.ticketVersionCliente = "";

        angular.element(".file-adj-contrato:visible").remove();
        $scope.adicionarFileAnterior();
    };

    //Guardando la solicitud del ticket
    $scope.enviarSolicitudTicket = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });
        var formData = new FormData();

        console.log($scope.motivoTrabajo);
        console.log($scope.entregableGarantia);

        angular.element.each(angular.element('#form-new-ticket-request').find('[type="file"]'), function (pos, fileInput) {
            formData.append('adjuntos', fileInput.files[0]);
        });
        formData.append('telefono', $scope.telefonoTicket);
        formData.append('reportadoPor', $scope.reportoTicket);
        formData.append('asunto', $scope.asuntoRequerimiento);
        formData.append('detalle', $scope.detalleRequerimiento);
        formData.append('prioridad', $scope.prioridadTicket);
        formData.append('categoriaTicket', $scope.categoriaTicket);
        formData.append('motivoPrioridad', $scope.motivoPrioridad != undefined ? $scope.motivoPrioridad : "");
        formData.append('motivoTrabajo', ($scope.motivoTrabajo != undefined && $scope.motivoTrabajo != "") ? $scope.motivoTrabajo : "0");
        formData.append('entregableGarantia', ($scope.entregableGarantia != undefined && $scope.entregableGarantia != "") ? $scope.entregableGarantia : "0");
        formData.append('esUrgente', $scope.isUrgente ? 1 : 0);
        formData.append('ticketVersionCliente', $scope.ticketVersionCliente != undefined ? $scope.ticketVersionCliente : "");

        console.log($scope.ticketVersionClientes);
        var nuevaSolicitudTicket = $http.post("clientes/nuevo-ticket",
            formData,
            {
                headers: { 'Content-Type': undefined }
            });
        nuevaSolicitudTicket.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                //recargar los seguimientos de ticket
                $scope.limpiarFormularioSolicitud();
                messageDialog.show('Información', data.msg);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
        nuevaSolicitudTicket.error(function (data) {
            waitingDialog.hide();
            messageDialog.show('Error', 'Error en la petición por favor verifique que los datos sean correctos.');
        });
    };

    $scope.SelectCategoria = function (categoria, garantias, contratos) {
        if (categoria == "GARANTÍA TÉCNICA") {
            $scope.esGarantia = true;
            $scope.esContratoMantenimiento = false;
            $scope.contratoSeleccionado = false;
            $scope.isRequired = true;
            $scope.contratoMantenimiento = "";
            $scope.motivosTrabajo = garantias;
        }
        else if (categoria == "MANTENIMIENTO") {
            $scope.esGarantia = false;
            $scope.esContratoMantenimiento = true;
            $scope.contratoSeleccionado = false;
            $scope.isRequired = true;
            $scope.contratoMantenimiento = "";
            $scope.motivosTrabajo = contratos;
        }
        else {
            $scope.esGarantia = false;
            $scope.esContratoMantenimiento = false;
            $scope.contratoSeleccionado = false;
            $scope.isRequired = false;
        }
    };

    $scope.SelectContrato = function (motivo, garantias) {
        $scope.entregablesGarantia = "";
        $scope.entregableGarantia = "";
        if ($scope.esGarantia == true && motivo != undefined) {
            $scope.contratoSeleccionado = true;
            garantias.forEach(function (elemento, indice, array) {
                if (elemento.id == motivo) {
                    $scope.entregablesGarantia = elemento.entregables;
                }
            })
        } else {
            $scope.contratoSeleccionado = false;
        }
    };

    $scope.SelectPrioridadChange = function (prioridadTicket) {
        var ajaxPrioridades = $http.post("catalogos/tickets-prioridades", {});
        ajaxPrioridades.success(function (data) {
            if (data.success === true) {
                $scope.prioridadesDelTicket = data.prioridades;
                $scope.prioridadesDelTicket.forEach(function (element, indexador) {
                    if (element.id == prioridadTicket) {
                        if (element.codigo == "URGENTE") {
                            $scope.isUrgente = true;
                        }
                        else {
                            $scope.isUrgente = false;
                        }
                    }
                });
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        $scope.motivoPrioridad = "";
    };

    angular.element('[data-toggle="popover"]').popover();

}]);