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
            angular.element('#fecha-registro-' + oferta.id).datepicker('destroy');
            angular.element('#fecha-produccion-' + oferta.id).datepicker('destroy');
            angular.element('#fecha-disponibilidad-' + oferta.id).datepicker('destroy');

            oferta.editable = false;
            var adjunto = $scope.adjunto;

            var formData = new FormData();
            formData.append("ID", oferta.id);
            formData.append("FechaRegistro", new Date(...oferta.FechaRegistro.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)));
            formData.append("FechaProduccion", new Date(...oferta.FechaProduccion.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)));
            formData.append("FechaDisponibilidad", new Date(...oferta.FechaDisponibilidad.split('/').reverse().map((v, i) => i === 1 ? v - 1 : v)));
            formData.append("Detalle", oferta.Detalle);
            formData.append("HorasEstimacion", oferta.HorasEstimacion);
            formData.append("cliente", oferta.cliente.id);
            formData.append("colaborador", oferta.colaborador.id);
            formData.append("adjunto", adjunto);

            var ajaxOfertas = $http({
                method: 'POST',
                url: "consultas/editar-ofertas-tickets",
                data: formData,
                headers: { 'Content-Type': undefined },
                transformRequest: angular.identity
            });

            ajaxOfertas.success(function (data) {
                if (data.success === true) {
                    $scope.cargarDatosOfertas()

                } else {
                    messageDialog.show("Información", data.msg);
                }
            });

        $scope.adjunto = null;
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

        var adjunto = $scope.adjunto;

        var formData = new FormData();
        formData.append("fechaRegistro", nuevaOferta.FechaRegistro);
        formData.append("fechaProduccion", nuevaOferta.FechaProduccion);
        formData.append("fechaDisponibilidad", nuevaOferta.FechaDisponibilidad);
        formData.append("detalle", nuevaOferta.Detalle);
        formData.append("horasEstimacion", nuevaOferta.HorasEstimacion);
        formData.append("cliente", nuevaOferta.cliente.id);
        formData.append("colaborador", nuevaOferta.colaborador.id);
        formData.append("adjunto", adjunto);

        var ajaxOfertas = $http({
            method: 'POST',
            url: "consultas/agregar-ofertas-tickets",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxOfertas.success(function (data) {
            if (data.success === true) {
                $scope.cargarDatosOfertas()

            } else {
                messageDialog.show("Información", data.msg);
            }
        });
        $scope.adjunto = null;
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

    $scope.recargarDatosOfertas = function () {
        var ajaxOfertas = $http.post("consultas/dar-ofertas-tickets", {
            filtro: $scope.filtroOfertas,
        });
        ajaxOfertas.success(function (data) {
            if (data.success === true) {
                console.log(data);
                $scope.ofertas = data.ofertas;
            }
        });
    };

    $scope.agregarAdjuntoOferta = function () {

        if (document.getElementById('add-adjunto-oferta').files[0].name.length > 20) {
            document.getElementById('btn-add-adjunto-oferta').innerHTML = "..." + document.getElementById('add-adjunto-oferta').files[0].name.substring(document.getElementById('add-adjunto-oferta').files[0].name.length - 17, document.getElementById('add-adjunto-oferta').files[0].name.length);
        } else {
            document.getElementById('btn-add-adjunto-oferta').innerHTML = document.getElementById('add-adjunto-oferta').files[0].name.substring(0, 20);
        }

        $scope.adjunto = document.getElementById('add-adjunto-oferta').files[0];
    };

    $scope.editarAdjuntoOferta = function () {

        if (document.getElementById('edit-adjunto-oferta').files[0].name.length > 20) {
            document.getElementById('btn-edit-adjunto-oferta').innerHTML = "..." + document.getElementById('edit-adjunto-oferta').files[0].name.substring(document.getElementById('edit-adjunto-oferta').files[0].name.length - 17, document.getElementById('edit-adjunto-oferta').files[0].name.length);
        } else {
            document.getElementById('btn-edit-adjunto-oferta').innerHTML = document.getElementById('edit-adjunto-oferta').files[0].name.substring(0, 20);
        }

        $scope.adjunto = document.getElementById('edit-adjunto-oferta').files[0];
    };


}]);