devApp.controller('recursosController', ['$scope', '$http', 'filtroService', function ($scope, $http, filtroService) {
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.idRecursos = 0;
    $scope.mostrarTodosRecursos = false;
    $scope.horas = 0;
    $scope.minutos = 0;

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

    //COLABORADORES ASISTENCIA
    var ajaxColaboradores = $http.post("catalogos/dar-colaboradores", {});
    ajaxColaboradores.success(function (data) {
        if (data.success === true) {

            var datos = [];
            data.colaboradores.forEach(function (item) {
                // Agrega la etapa al arreglo aplanado
                datos.push({
                    id: item.id,
                    nombre: item.nombre,
                    asignado: false,
                    asistencia: false,
                    puntuacion: 0.0
                });
            });
            $scope.datosAsistencia = datos;
        }
    });

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
        $scope.newTitulo = '';
        $scope.newDetalle = '';
        $scope.moduloSeleccionado = '';
        
        angular.element("#modal-agregar-recursos").modal("show");
    };

    $scope.abrirRegistroAsistencia = function () {
        $scope.limpiarDatosAsistencia();
        angular.element("#guardar-registro").hide();
        angular.element("#modal-asistencia-recurso").modal("show");
    };

    $scope.mostrarRegistroAsistencia = function (secuencial) {

        var ajaxObtenerRecurso = $http.post("user/dar-datos-asistencia-recursos",
            {
                secuencialRecurso: secuencial
            });
        ajaxObtenerRecurso.success(function (data) {
            if (data.success === true) {
                var datosPorId = {};
                data.datos.forEach(function (item) {
                    datosPorId[item.id] = item;
                });

                data.datos.forEach(function (itemData) {
                    $scope.datosAsistencia.forEach(function (itemAsistencia) {
                        if (itemAsistencia.id === itemData.idColaborador) {
                            itemAsistencia.asistencia = itemData.asistencia;
                            itemAsistencia.puntuacion = itemData.puntuacion;
                            itemAsistencia.asignado = true;
                        };
                    });
                });

                $scope.secuencialRecurso = secuencial;
                angular.element("#guardar-registro").show();
                angular.element("#modal-asistencia-recurso").modal("show");
            }
        });      
    };

    var ajaxTipoModulo = $http.post("user/tipo-modulo", {});

    ajaxTipoModulo.success(function (data) {
        if (data.success === true) {
            $scope.tipoModulo = data.tipoModulo;
        }
    });

    $scope.GuardarNuevoRecurso = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var fileInput = angular.element('#uniqueFileInputIDRecursos')[0];
        var modulo = angular.element("#select-modulo-recursos")[0].value;

        var fechaSistema = new Date().toISOString();

        var formData = new FormData();
        formData.append('titulo', $scope.newTitulo);
        formData.append('detalle', $scope.newDetalle);
        formData.append('fecha', fechaSistema);
        formData.append('modulo', modulo);
        formData.append('adjuntos', fileInput.files[0]);
        formData.append('adjuntoAsistencia', JSON.stringify($scope.datosAsistencia));

        var tiempo = toTotalMinutes($scope.horas, $scope.minutos)

        formData.append('tiempo', tiempo);

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

                $scope.horas = 0;
                $scope.minutos = 0;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

        ajaxEnvioDatos.error(function (data) {
            waitingDialog.hide();
            console.log('Error: ' + data);
        });
    };

    $scope.registrarAsistencia = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var datosGuardar = [];

            $scope.datosAsistencia.forEach(function (itemAsistencia) {
                if (itemAsistencia.asignado === true) {
                    if (itemAsistencia.asistencia === true || itemAsistencia.puntuacion != 0.0)
                        datosGuardar.push(itemAsistencia);
                };
            });

        var formData = new FormData();
        formData.append('idRecurso', $scope.secuencialRecurso);
        formData.append('adjuntoAsistencia', JSON.stringify($scope.datosAsistencia));

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-asistencia-recurso",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-asistencia-recurso").modal("hide");
                $scope.cargarRecursos();
            }
        });
    };

    $scope.limpiarDatosAsistencia = function () {
        if ($scope.datosAsistencia) {
            $scope.datosAsistencia.forEach(function (item) {
                item.asignado = false;
                item.asistencia = false;
                item.puntuacion = 0.0;
            });
        }
    };

    $scope.mostrarDetalleRecurso = function (secuencial) {
        $scope.secuencialRecurso = secuencial;

        var ajaxObtenerIncidencia = $http.post("user/dar-datos-recurso-usuario",
            {
                secuencialRecurso: secuencial
            });
        ajaxObtenerIncidencia.success(function (data) {
            if (data.success === true) {

                $scope.tituloV = data.recursoResult.titulo;
                $scope.detalleV = data.recursoResult.detalle;
                $scope.moduloV = data.recursoResult.modulo;
                $scope.fechaV = data.recursoResult.fecha;
                $scope.adjuntoV = data.recursoResult.adjunto;
                $scope.tiempoV = data.recursoResult.tiempo;
                $scope.adjuntoAsistenciaV = data.recursoResult.adjuntoAsistencia;

                angular.element("#modal-datos-recurso").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    // Obtén la plantilla y añade un ID único
    var fileInputTemplate = angular.element("#htmlFile").html();
    var uniqueId = "uniqueFileInputIDRecursos"; // Genera un ID único de alguna manera
    var uniqueId2 = "uniqueFileInputIDRecursos2"; // Genera un ID único de alguna manera
    var fileInputWithId = fileInputTemplate.replace('<input type="file"', '<input type="file" id="' + uniqueId + '"');
    var fileInputWithId2 = fileInputTemplate.replace('<input type="file"', '<input type="file" id="' + uniqueId2 + '"');

    // Añade la plantilla modificada al DOM
    angular.element("#panel-add-adjuntorecursos").append(fileInputWithId);
    angular.element("#panel-add-adjuntorecursos2").append(fileInputWithId2);
}]);

//Servicio del filtrado para limpiar los filtros
devApp.service('filtroService', function () {
    this.filtroRecursos = "";
});

devApp.filter('toHoursAndMinutes', function () {
    return function (totalMinutes) {
        if (totalMinutes === 0) {
            return '0';
        }

        var hours = Math.floor(totalMinutes / 60);
        var minutes = totalMinutes % 60;

        // Asegurarse de que los minutos se muestren siempre con dos dígitos
        minutes = minutes < 10 ? '0' + minutes : minutes;

        return hours + ':' + minutes;
    };
});

/*function toHoursAndMinutes(totalMinutes) {
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return { hours, minutes };
}*/

function toTotalMinutes(hours, minutes) {
    return (hours * 60) + minutes;
}




