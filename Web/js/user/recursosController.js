devApp.controller('recursosController', ['$scope', '$http', 'filtroService', function ($scope, $http, filtroService) {
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.idRecursos = 0;
    $scope.mostrarTodosRecursos = false;

    $scope.filtroRecursos = filtroService.filtroRecursos;

    $scope.cargarRecursos = function (start, lenght) {
        if (start === undefined)
            start = 0;
        if (lenght === undefined)
            lenght = numerosPorPagina;

        $scope.loading.show();
        var recursos = $http.post("user/recursos-usuario",
            {
                start: start,
                lenght: lenght,
                filtro: $scope.filtroRecursos,
                todos: $scope.mostrarTodosRecursos
            });
        recursos.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {
               
                var posPagin = pagina;
                $scope.recursos = data.recursos;
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
                    var listaPaginador = angular.element("#tabla-recursos-usuario .pagination li a");
                    angular.element(listaPaginador).removeClass('pagSelect');
                    angular.element(listaPaginador[posPagin]).addClass('pagSelect');
                }, 300);
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarRecursos();

    // Función para retroceder a la página anterior
    $scope.atrazarPagina = function () {
        if (pagina > 1) {
            pagina--;
            $scope.cargarRecursos((pagina - 1) * numerosPorPagina, numerosPorPagina);
        }
    };

    // Función para avanzar a la página siguiente
    $scope.avanzarPagina = function () {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.cargarRecursos((pagina - 1) * numerosPorPagina, numerosPorPagina);
        }
    };

    // Función para cambiar a una página específica
    $scope.cambiarPagina = function (num) {
        pagina = num;
        $scope.cargarRecursos((pagina - 1) * numerosPorPagina, numerosPorPagina);
    };


    $scope.windowAgregarRecursos = function () {
        angular.element("#modal-agregar-recursos").modal("show");
    };


    var ajaxCategoriaRecursos = $http.post("user/categoria-recursos", {});

    ajaxCategoriaRecursos.success(function (data) {
        if (data.success === true) {
            $scope.categoriaRecursos = data.categoriaRecursos;
        }
    });

    $scope.GuardarNuevoRecurso = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var fileInput = angular.element('#uniqueFileInputIDRecursos')[0];
        var categoria = angular.element("#select-categoria-recursos")[0].value;

        var fechaSistema = new Date().toISOString();

        var formData = new FormData();
        formData.append('titulo', $scope.newTitulo);
        formData.append('detalle', $scope.newDetalle);
        formData.append('fecha', fechaSistema);
        formData.append('categoria', categoria);
        formData.append('adjuntos', fileInput.files[0]);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-recurso",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-recursos").modal("hide");
                $scope.cargarRecursos();
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

        ajaxEnvioDatos.error(function (data) {
            waitingDialog.hide();
            console.log('Error: ' + data);
        });
    };

    // Obtén la plantilla y añade un ID único
    var fileInputTemplate = angular.element("#htmlFile").html();
    var uniqueId = "uniqueFileInputIDRecursos"; // Genera un ID único de alguna manera
    var fileInputWithId = fileInputTemplate.replace('<input type="file"', '<input type="file" id="' + uniqueId + '"');

    // Añade la plantilla modificada al DOM
    angular.element("#panel-add-adjuntorecursos").append(fileInputWithId);
}]);

//Servicio del filtrado para limpiar los filtros
devApp.service('filtroService', function () {
    this.filtroRecursos = "";
});


