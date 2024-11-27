devApp.controller('recursosController', ['$scope', '$http', 'filtroService', function ($scope, $http, filtroService) {
    $scope.esPlan = false;
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.idRecursos = 0;
    $scope.mostrarTodosRecursos = false;
    $scope.horas = 0;
    $scope.minutos = 0;
    $scope.currentDate = new Date();
    $scope.esUserAllow = esUserAllow;

    angular.element('#fecha-capacitacion').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false,
        startDate: new Date(new Date().setDate(new Date().getDate() - 1)) // Resta un día a la fecha actual
    });

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
                todos: $scope.mostrarTodosRecursos,
                esPlan: $scope.esPlan
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
    var ajaxColaboradores = $http.post("catalogos/dar-full-colaboradores", {});
    ajaxColaboradores.success(function (data) {
        if (data.success === true) {
            $scope.colaboradores = data.colaboradores;
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

            var datosAsistente = [];
            data.colaboradores.forEach(function (item) {
                datosAsistente.push({
                    id: item.id,
                    nombre: item.nombre,
                    asignado: false
                });
            });
            $scope.asistentesCapacitacion = datosAsistente;
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
        $scope.url = '';
        $scope.moduloSeleccionado = '';
        $scope.link = '';

        angular.element("#modal-agregar-recursos").modal("show");
    };

    $scope.windowAgregarPlan = function () {
        $scope.newTituloPlan = '';
        $scope.newDetallePlan = '';
        $scope.horasPlan = 0;
        $scope.minutosPlan = 0;
        $scope.moduloSeleccionadoPlan = '';
        $scope.capacitadorSeleccionadoPlan = '';
        $scope.linkPlan = '';

        angular.element("#modal-agregar-plan").modal("show");
    };

    $scope.abrirRegistroAsistencia = function () {
        angular.element("#modal-agregar-asistencia-recurso").modal("show");
    };

    $scope.datosAsistenciaReunion;
    $scope.mostrarRegistroAsistencia = function (recurso) {
        var ajaxObtenerRecurso = $http.post("user/dar-datos-asistencia-recursos", {
            secuencialRecurso: recurso.secuencial
        });

        $scope.recursoActual = recurso;

        ajaxObtenerRecurso.success(function (data) {
            if (data.success === true) {
                $scope.limpiarDatosAsistencia();

                var datosPorId = {};
                data.datos.forEach(function (item) {
                    datosPorId[item.id] = item;
                });
                var datosAsist = [];

                data.datos.forEach(function (itemData) {

                    datosAsist.push({
                        id: itemData.id,
                        idColaborador: itemData.idColaborador,
                        nombre: itemData.nombre,
                        asignado: itemData.asignado,
                        asistencia: itemData.asistencia,
                        puntuacion: itemData.puntuacion
                    });
                });

                $scope.datosAsistenciaReunion = datosAsist;
                $scope.secuencialRecurso = recurso.secuencial;
                angular.element("#guardar-registro").show();
                angular.element("#modal-asistencia-recurso").modal("show");
            }
        });
    };

    $scope.mostrarRegistroConvocados = function (recurso) {
        var ajaxObtenerRecurso = $http.post("user/dar-datos-convocados-recursos", {
            secuencialRecurso: recurso.secuencial
        });

        $scope.recursoActual = recurso;
        ajaxObtenerRecurso.success(function (data) {
            if (data.success === true) {
                $scope.limpiarDatosAsistencia();

                var datosPorId = {};
                data.datos.forEach(function (item) {
                    datosPorId[item.id] = item;
                });
                var datosAsist = [];

                data.datos.forEach(function (itemData) {

                    datosAsist.push({
                        id: itemData.id,
                        idColaborador: itemData.idColaborador,
                        nombre: itemData.nombre,
                        convocado: itemData.convocado
                    });
                });

                $scope.datosConvocadosReunion = datosAsist;

                $scope.secuencialRecurso = recurso.secuencial;
                angular.element("#modal-convocados-recurso").modal("show");
            }
        });
    };

    var ajaxCapacitadores = $http.post("catalogos/dar-full-colaboradores", {});
    ajaxCapacitadores.success(function (data) {
        if (data.success === true) {
            $scope.capacitadores = data.colaboradores;
        }
    });

    var ajaxTipoModulo = $http.post("user/tipo-modulo", {});
    ajaxTipoModulo.success(function (data) {
        if (data.success === true) {
            $scope.tipoModulo = data.tipoModulo;
        }
    });

    $scope.GuardarNuevoRecurso = function () {
        var modulo = angular.element("#select-modulo-recursos")[0].value;

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var fecha = $scope.fechaHora ? new Date($scope.fechaHora).toISOString() : new Date().toISOString();

        var formData = new FormData();
        formData.append('titulo', $scope.newTitulo);
        formData.append('detalle', $scope.newDetalle);
        formData.append('fecha', fecha);
        formData.append('modulo', modulo);
        formData.append('url', $scope.url);
        formData.append('link', $scope.link);
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
                $scope.urlNoValida = false;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

        ajaxEnvioDatos.error(function (data) {
            waitingDialog.hide();
            console.log('Error: ' + data);
        });
    };

    $scope.GuardarNuevoPlan = function () {
        var moduloPlan = angular.element("#select-modulo-plan")[0].value;
        var colaborador = angular.element("#select-capacitador-plan")[0].value;

        var fecha = $scope.fechaHoraCapacitacion ? new Date($scope.fechaHoraCapacitacion).toISOString() : new Date().toISOString();

        var asistentesSeleccionadosIds = $scope.asistentesCapacitacion
            .filter(asistente => asistente.seleccionado)
            .map(asistente => asistente.id);
        var asistentesSeleccionadosJson = JSON.stringify(asistentesSeleccionadosIds);

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();
        formData.append('titulo', $scope.newTituloPlan);
        formData.append('detalle', $scope.newDetallePlan);
        formData.append('fecha', fecha);
        formData.append('modulo', moduloPlan);
        formData.append('colaborador', colaborador);
        formData.append('asistentesJson', asistentesSeleccionadosJson);
        formData.append('link', $scope.linkPlan);

        var tiempo = toTotalMinutes($scope.horasPlan, $scope.minutosPlan);
        formData.append('tiempo', tiempo);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-plan-recurso",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-agregar-plan").modal("hide");
                $scope.cargarRecursos();

                var datosAsistente = [];
                data.colaboradores.forEach(function (item) {
                    datosAsistente.push({
                        id: item.id,
                        nombre: item.nombre,
                        asignado: false
                    });
                });
                $scope.asistentesCapacitacion = datosAsistente;

                $scope.horasPlan = 0;
                $scope.minutosPlan = 0;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

        ajaxEnvioDatos.error(function (data) {
            waitingDialog.hide();
            console.log('Error: ' + data);
        });
    };

    // Dentro del controlador AngularJS:
    $scope.editarUrl = function (recurso) {
        recurso.editandoUrl = true;
    };

    $scope.guardarUrl = function (recurso) {
        var ajax = $http.post("user/editar-plan-recurso",
            {
                secuencial: recurso.secuencial,
                url: recurso.adjunto,
            });
        ajax.success(function (data) {
            if (data.success === true) {
                recurso.editandoUrl = false;
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.cancelarEdicionUrl = function (recurso) {
        recurso.editandoUrl = false;
    };

    $scope.validarUrl = function (url) {
        return /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([\/\w .-]*)*\/?$/.test(url);
    };


    $scope.registrarAsistencia = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var datosGuardar = [];
        $scope.datosAsistenciaReunion.forEach(function (itemAsistencia) {
            console.log(itemAsistencia);
            if (itemAsistencia.asistencia === true || itemAsistencia.puntuacion != 0.0)
                datosGuardar.push(itemAsistencia);
        });

        var formData = new FormData();
        formData.append('idRecurso', $scope.secuencialRecurso);
        formData.append('adjuntoAsistencia', JSON.stringify($scope.datosAsistenciaReunion));

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

    $scope.registrarAsistenciaConvocados = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var datosGuardar = [];

        $scope.datosConvocadosReunion.forEach(function (itemAsistencia) {
            console.log(itemAsistencia);
            if (itemAsistencia.convocado === true)
                datosGuardar.push(itemAsistencia);
        });

        var formData = new FormData();
        formData.append('idRecurso', $scope.secuencialRecurso);
        formData.append('adjuntoAsistencia', JSON.stringify($scope.datosConvocadosReunion));

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/guardar-asignados-recurso",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-convocados-recurso").modal("hide");
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
        if ($scope.esPlan) {
            $scope.secuencialRecurso = secuencial;

            var ajaxObtenerIncidencia = $http.post("user/dar-datos-recurso-usuario",
                {
                    secuencialRecurso: secuencial
                });
            ajaxObtenerIncidencia.success(function (data) {
                if (data.success === true) {

                    $scope.editTituloPlan = data.recurso.titulo;
                    $scope.editDetallePlan = data.recurso.detalle;
                    $scope.editModuloSeleccionadoPlan = data.recurso.modulo;
                    angular.element("#edit-fecha-hora-capacitacion").val(data.recurso.fecha);
                    $scope.adjuntoV = data.recurso.adjunto;
                    $scope.editHorasPlan = data.recurso.horas;
                    $scope.editMinutosPlan = data.recurso.minutos;
                    $scope.editLinkPlan = data.recurso.url;
                    $scope.editCapacitadorSeleccionadoPlan = data.recurso.capacitor;

                    $scope.asistenciaEdicionRecurso = data.asistencia

                    angular.element("#modal-datos-recurso").modal("show");
                }
                else {
                    messageDialog.show('Información', data.msg);
                }
            });
        }
    };

    $scope.EditarPlan = function () {
        var moduloPlan = angular.element("#edit-select-modulo-plan")[0].value;
        var colaborador = angular.element("#edit-select-capacitador-plan")[0].value;

        var fecha = $scope.editFechaHoraCapacitacion ? new Date($scope.editFechaHoraCapacitacion).toISOString() : new Date().toISOString();

        var asistentesSeleccionadosIds = $scope.datosAsistenciaEdicion
            .filter(asistente => asistente.asignado)
            .map(asistente => asistente.id);
        var asistentesSeleccionadosJson = JSON.stringify(asistentesSeleccionadosIds);

        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var formData = new FormData();
        formData.append('id', $scope.secuencialRecurso);
        formData.append('titulo', $scope.editTituloPlan);
        formData.append('detalle', $scope.editDetallePlan);
        formData.append('fecha', fecha);
        formData.append('modulo', moduloPlan);
        formData.append('colaborador', colaborador);
        formData.append('asistentesJson', asistentesSeleccionadosJson);
        formData.append('link', $scope.editLinkPlan);

        var tiempo = toTotalMinutes($scope.editHorasPlan, $scope.editMinutosPlan);
        formData.append('tiempo', tiempo);

        var ajaxEnvioDatos = $http({
            method: 'POST',
            url: "user/editar-plan-recurso-capacitacion",
            data: formData,
            headers: { 'Content-Type': undefined },
            transformRequest: angular.identity
        });

        ajaxEnvioDatos.success(function (data) {
            waitingDialog.hide();
            if (data.success === true) {
                angular.element("#modal-datos-recurso").modal("hide");
                $scope.cargarRecursos();

                var datosAsistente = [];
                data.colaboradores.forEach(function (item) {
                    datosAsistente.push({
                        id: item.id,
                        nombre: item.nombre,
                        asignado: false
                    });
                });
                $scope.asistentesCapacitacion = datosAsistente;

                $scope.horasPlan = 0;
                $scope.minutosPlan = 0;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });

        ajaxEnvioDatos.error(function (data) {
            waitingDialog.hide();
            console.log('Error: ' + data);
        });
    };

    $scope.abrirModalSeleccionarAsistentesEdicion = function () {
        $scope.todosSeleccionadosEdicion = false;

        var datos = [];

        var asistenciaMap = {};
        $scope.asistenciaEdicionRecurso.forEach(function (asistencia) {
            asistenciaMap[asistencia.idColaborador] = true; // Asignar true si existe una asistencia
        });

        $scope.colaboradores.forEach(function (item) {
            var asignado = asistenciaMap[item.id] || false; // Verificar si existe asistencia y asignar true

            datos.push({
                id: item.id,
                nombre: item.nombre,
                asignado: asignado
            });
        });

        $scope.datosAsistenciaEdicion = datos;

        angular.element("#modal-seleccionar-asistentes-edicion").modal("show");
    };

    $scope.seleccionarTodosAsistentesEdicion = function () {
        for (var i = 0; i < $scope.asistenciaEdicionRecurso.length; i++) {
            $scope.asistenciaEdicionRecurso[i].asignado = $scope.todosSeleccionadosEdicion;
        }
    };

    $scope.abrirModalSeleccionarAsistentes = function () {
        angular.element("#modal-seleccionar-asistentes").modal("show");
    };

    $scope.seleccionarTodosAsistentes = function () {
        for (var i = 0; i < $scope.asistentesCapacitacion.length; i++) {
            $scope.asistentesCapacitacion[i].seleccionado = $scope.todosSeleccionados;
        }
    };

    $('#modal-agregar-plan').on('hidden.bs.modal', function () {
        $scope.$apply(function () {
            for (var i = 0; i < $scope.asistentesCapacitacion.length; i++) {
                $scope.asistentesCapacitacion[i].seleccionado = false;
            }
            $scope.todosSeleccionados = false;
        });
    });

    $scope.getDatosAsistenciaFiltrados = function () {
        if ($scope.esPlan) {
            return $scope.datosConvocadosReunion;
        } else {
            return $scope.datosAsistenciaReunion;
        }
    };

    $scope.generarCertificado = function (recurso) {
        var fecha = recurso.fecha;
        var timestamp = parseInt(fecha.replace(/\/Date\((\d+)\)\//, '$1'));
        var date = new Date(timestamp);

        var ajaxCertificado = $http.post('user/generar-certificado',
            {
                secuencialRecurso: recurso.secuencial
            });
        ajaxCertificado.success(function (data) {
            if (data.success === true) {
                var certificadoUrl = data.url;
                window.open(certificadoUrl, '_blank');
            } else {
                console.error('Error al generar el certificado:', data.message);
            }
        }).error(function (error) {
            console.error('Error al generar el certificado:', error);
        });
    };

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

function validarURL(url) {
    // Expresión regular para validar una URL
    var regex = new RegExp("^((https?|ftp):\\/\\/)?" + // Protocolo
        "((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|" + // Dominio
        "((\\d{1,3}\\.){3}\\d{1,3}))" + // O dirección IP
        "(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*" + // Puerto y ruta
        "(\\?[;&a-z\\d%_.~+=-]*)?" + // Parámetros de consulta
        "(\\#[-a-z\\d_]*)?$", "i"); // Fragmento
    return regex.test(url);
}




