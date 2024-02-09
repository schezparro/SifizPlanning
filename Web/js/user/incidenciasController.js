devApp.controller('incidenciasController', ['$scope', '$http', function ($scope, $http) {
    var numerosPorPagina = 10;
    var pagina = 1;
    $scope.idIncidencias = 0;
    $scope.mostrarFinDiaIncidencias = false;

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
                finDia: $scope.mostrarFinDiaIncidencias
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

    //El Paginador
    $scope.paginar = function () {
        var start = (pagina - 1) * numerosPorPagina;
        var lenght = numerosPorPagina;
        $scope.cargarIncidencias(start, lenght);
    };

    $scope.cambiarPagina = function (pag) {
        pagina = pag;
        $scope.paginar();
    };

    $scope.atrazarPagina = function () {
        if (pagina > 1) {
            pagina--;
            $scope.paginar();
        }
    };
    $scope.avanzarPagina = function () {
        if (pagina < $scope.cantPaginas) {
            pagina++;
            $scope.paginar();
        }
    };

    angular.element('#fecha-incidencia').datepicker({
        format: 'dd/mm/yyyy',
        forceParse: false
    });
    $scope.windowAgregarIncidencias = function () {
        $scope.clienteSeleccionado = '';
        $scope.tipoSeleccionado = '';
        $scope.newIncidente = '';
        $scope.newAcciones = '';
        $scope.fechaIncidencia = '';
        $scope.finDia = false;
        $scope.mostrarSelectLideres = false;
        $scope.liderSeleccionado = '';
        angular.element("#modal-agregar-incidencias").modal("show");
    };


    var ajaxTipoModulo = $http.post("user/tipo-modulo", {});

    ajaxTipoModulo.success(function (data) {
        if (data.success === true) {
            $scope.tipoModulo = data.tipoModulo;
        }
    });

    var ajaxClienteIncidencias = $http.post("user/cliente-incidencias", {});

    ajaxClienteIncidencias.success(function (data) {
        if (data.success === true) {
            $scope.clientes = data.clientes;
        }
    });

    var ajaxRolColaboradorIncidencias = $http.post("user/rol-colaborador-incidencias", {});
    ajaxRolColaboradorIncidencias.success(function (data) {
        if (data.success === true) {
            $scope.lideres = data.lideres;
        }
    });

    $scope.GuardarNuevaIncidencia = function () {
        waitingDialog.show('Guardando...', { dialogSize: 'sm', progressType: 'success' });

        var fileInput = angular.element('#uniqueFileInputID')[0];
        var modulo = angular.element("#select-tipo-modulo")[0].value;
        var cliente = angular.element("#select-cliente-incidencias")[0].value;
        var lideres = [];
        var selectMultiple = document.querySelector('#select-notificar-lideres');
        for (var i = 0; i < selectMultiple.options.length; i++) {
            if (selectMultiple.options[i].selected) {
                lideres.push(selectMultiple.options[i].value);
            }
        }
        lideresString = lideres.join(',');
            
        var formData = new FormData();
        formData.append('cliente', cliente);
        formData.append('modulo', modulo);        
        formData.append('incidente', $scope.newIncidente);
        formData.append('acciones', $scope.newAcciones);
        formData.append('fechaincidencia', $scope.fechaIncidencia);
        formData.append('findia', $scope.finDia);
        formData.append('tiempo', $scope.tiempo);
        formData.append('lideres', lideresString);
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

    $scope.mostrarDetalleIncidencia = function (secuencial) {
        $scope.secuencialIncidencia = secuencial;

        var ajaxObtenerIncidencia = $http.post("user/dar-datos-incidencia-usuario",
            {
                secuencialIncidencia: secuencial
            });
        ajaxObtenerIncidencia.success(function (data) {
            if (data.success === true) {
                $scope.clienteV = data.incidenciaResult.cliente;
                $scope.moduloV = data.incidenciaResult.modulo;
                $scope.incidenteV = data.incidenciaResult.incidente;
                $scope.accionesV = data.incidenciaResult.acciones;
                $scope.fechaV = data.incidenciaResult.fecha;
                $scope.finDiaV = data.incidenciaResult.finDeDia;
                $scope.adjuntoV = data.incidenciaResult.adjunto;
                $scope.tiempoV = data.incidenciaResult.tiempo;
                $scope.colaboradorV = data.incidenciaResult.colaborador;

                angular.element("#modal-datos-incidencia").modal("show");
            }
            else {
                messageDialog.show('Información', data.msg);
            }
        });
    };

    // Obtén la plantilla y añade un ID único
    var fileInputTemplate = angular.element("#htmlFile").html();
    var uniqueId = "uniqueFileInputID"; // Genera un ID único de alguna manera
    var fileInputWithId = fileInputTemplate.replace('<input type="file"', '<input type="file" id="' + uniqueId + '"');

    // Añade la plantilla modificada al DOM
    angular.element("#panel-add-adjunto").append(fileInputWithId);


    //Filters
    //Filter de angular para las fechas
    devApp.filter("strDateToStr", function () {
        return function (textDate) {
            if (textDate !== undefined) {
                var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
                return dateToStr(fecha);
            }
            return "";
        }
    });
    function dateToStr(dateObj, format, separator) {
        /**
         * Convert a date object to a string
         * @argument dateObj {Date} A date object
         * @argument format {string} An string representation of the date format. Default: dd-mm-yyyy. More could be added as necessary
         * @argument separator {string} Character used for join the parts of the date
         * @returns {string} An string representation of a Date
         */
        var year = dateObj.getFullYear().toString();
        var month = dateObj.getMonth() + 1;
        var month = (month < 10) ? '0' + month : month;
        var day = dateObj.getDate();
        var day = (day < 10) ? '0' + day : day;
        var sep = (separator) ? separator : '/';
        switch (format) {
            case 'mm/dd/yyyy':
                var out = [month, day, year];
                break;
            case 'yyyy/mm/dd':
                var out = [year, month, day];
                break;
            default: //dd/mm/yyyy
                var out = [day, month, year];
        }
        return out.join(sep);
    };

}]);