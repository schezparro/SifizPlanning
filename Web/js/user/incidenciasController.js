devApp.controller('incidenciasController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.idIncidencias = 0;
    $scope.mostrarTodasIncidencias = false;

    $scope.cargarIncidencias = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        $scope.loading.show();
        var incidencias = $http.post("user/incidencias-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroIncidencias,
                todos: $scope.mostrarTodasIncidencias
            });
        incidencias.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
                var posPagin = pagina;
                $scope.incidencias = data.incidencias;
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
                    var listaPaginador = angular.element("#tabla-incidencias-usuario .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarIncidencias();


    $scope.windowAgregarIncidencias = function () {
        angular.element("#modal-agregar-incidencias").modal("show");
    };


    var ajaxTipoModulo = $http.post("user/tipo-modulo", {});

    ajaxTipoModulo.success(function (data) {
        if (data.success === true) {
            $scope.tipoModulo = data.tipoModulo;
        }
    });


    $scope.GuardarNuevaIncidencia = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var fileInput = angular.element('#uniqueFileInputID')[0];
        var modulo = angular.element("#select-tipo-modulo")[0].value;

        var formData = new FormData();
        formData.append('version', $scope.newVersion);
        formData.append('modulo', modulo);
        formData.append('incidente', $scope.newIncidente);
        formData.append('adjuntos', fileInput.files[0]);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-incidencia",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-incidencias").modal("hide");
                $scope.cargarIncidencias();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
    };

    // Obtén la plantilla y añade un ID único
    var fileInputTemplate = angular.element("#htmlFile").html();
    var uniqueId = "uniqueFileInputID"; // Genera un ID único de alguna manera
    var fileInputWithId = fileInputTemplate.replace('<input type="file"', '<input type="file" id="' + uniqueId + '"');

    // Añade la plantilla modificada al DOM
    angular.element("#panel-add-adjunto").append(fileInputWithId);
}]);