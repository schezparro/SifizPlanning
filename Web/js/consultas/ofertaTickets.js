consultasApp.controller('ofertaTicketsController', ['$scope', '$http', '$sce', function ($scope, $http, $sce) {

    $scope.nuevaOferta = { FechaRegistro: '', FechaProduccion: '', FechaDisponibilidad: '', Detalle: '', HorasEstimacion: '', cliente: '', colaborador: '' };

    $scope.editarOferta = function (oferta, index) {
        oferta.editable = true;
        oferta.FechaRegistro = '';
        oferta.FechaProduccion = '';
        oferta.FechaDisponibilidad = '';

        angular.element('#fecha-registro-' + index).datepicker({
            format: 'dd/mm/yyyy',
            language: 'es'
        });

        angular.element(`#fecha-produccion-` + index).datepicker({
            format: 'dd/mm/yyyy',
            language: 'es'

        })

        angular.element('#fecha-disponibilidad-' + index).datepicker({
            format: 'dd/mm/yyyy',
            language: 'es'
        });

    };
    $scope.guardarCambios = function (oferta) {
        if (oferta.id !== "" && oferta.FechaRegistro !== "" && oferta.FechaProduccion !== "" &&
            oferta.FechaDisponibilidad !== "" && oferta.Detalle !== "" && oferta.HorasEstimacion !== "" && oferta.cliente.id !== "" &&
            oferta.colaborador.id !== "") {
            angular.element('#fecha-registro-' + oferta.id).datepicker('destroy');
            angular.element('#fecha-produccion-' + oferta.id).datepicker('destroy');
            angular.element('#fecha-disponibilidad-' + oferta.id).datepicker('destroy');

            oferta.editable = false;

            var ajaxOfertas = $http.post("consultas/editar-ofertas-tickets", {
                ID: oferta.id,
                FechaRegistro: new Date(...oferta.FechaRegistro.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)),
                FechaProduccion: new Date(...oferta.FechaProduccion.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)),
                FechaDisponibilidad: new Date(...oferta.FechaDisponibilidad.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)),
                Detalle: oferta.Detalle,
                HorasEstimacion: oferta.HorasEstimacion,
                cliente: oferta.cliente.id,
                colaborador: oferta.colaborador.id
            });

            ajaxOfertas.success(function (data) {
                if (data.success === true) {
                    $scope.cargarDatosOfertas()

                } else {
                    messageDialog.show("Información", data.msg);
                }
            });

        } else {
            messageDialog.show("Información", "Debe llenar todos los campos");
        }
    };

    $scope.agregarOferta = function () {
        var clienteSeleccionado = 0;
        var colaboradorSeleccionado = 0;
        if ($scope.nuevaOferta.cliente !== "") {
            clienteSeleccionado = $scope.clientes.find(function (cliente) {
                return cliente.id === $scope.nuevaOferta.cliente;
            });
        }
        if ($scope.nuevaOferta.colaborador !== "") {
            colaboradorSeleccionado = $scope.colaboradores.find(function (colaborador) {
                return colaborador.id === $scope.nuevaOferta.colaborador;
            });
        }

        var nuevaOferta = {
            FechaRegistro: $scope.nuevaOferta.FechaRegistro ?
                new Date(...$scope.nuevaOferta.FechaRegistro.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)) :
                null,
            FechaProduccion: $scope.nuevaOferta.FechaProduccion ?
                new Date(...$scope.nuevaOferta.FechaProduccion.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)) :
                null,
            FechaDisponibilidad: $scope.nuevaOferta.FechaDisponibilidad ?
                new Date(...$scope.nuevaOferta.FechaDisponibilidad.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)) :
                null,
            Detalle: $scope.nuevaOferta.Detalle,
            HorasEstimacion: $scope.nuevaOferta.HorasEstimacion || 0,
            cliente: clienteSeleccionado,
            colaborador: colaboradorSeleccionado,
            editable: false
        };

        var ajaxOfertas = $http.post("consultas/agregar-ofertas-tickets", {
            fechaRegistro: nuevaOferta.FechaRegistro,
            FechaProduccion: nuevaOferta.FechaProduccion,
            fechaDisponibilidad: nuevaOferta.FechaDisponibilidad,
            detalle: nuevaOferta.Detalle,
            horasEstimacion: nuevaOferta.HorasEstimacion,
            cliente: nuevaOferta.cliente.id,
            colaborador: nuevaOferta.colaborador.id
        });

        ajaxOfertas.success(function (data) {
            if (data.success === true) {
                $scope.cargarDatosOfertas()

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    $scope.eliminarOferta = function (id) {
        var ajaxOfertas = $http.post("consultas/eliminar-oferta-tickets", {
            id: id
        });

        ajaxOfertas.success(function (data) {
            if (data.success === true) {
                $scope.cargarDatosOfertas()
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };
}]);