var adminApp = angular.module('admin', ['angularUtils.directives.dirPagination']);

adminApp.config(function ($provide, $httpProvider) {

    // Intercept http calls.
    $provide.factory('MyHttpInterceptor', function ($q) {
        return {
            // On request success
            request: function (config) {
                return config || $q.when(config);
            },
            // On request failure
            requestError: function (rejection) {
                return $q.reject(rejection);
            },
            // On response success
            response: function (response) {
                var x = false
                if (typeof response.data === "string" || (typeof response.data == "object" && response.data.constructor === String)) {
                    x = true;
                }
                if (x && response.data.indexOf('ng-app="login"') !== -1) {
                    window.location.assign("sifizplanning/redirect");
                }
                else
                    return response || $q.when(response);
            },
            // On response failture
            responseError: function (rejection) {
                return $q.reject(rejection);
            }
        };
    });

    // Add the interceptor to the $httpProvider.
    $httpProvider.interceptors.push('MyHttpInterceptor');
});

adminApp.controller('adminController', ['$scope', '$http', function ($scope, $http) {
    $scope.funcionalidad = 'COLABORADORES';

    //CARGANDO LOS DATOS INICIALES
    //Cargando los paises
    var ajaxPaises = $http.post("catalogos/paises", {});
    ajaxPaises.success(function (data) {
        if (data.success === true) {
            $scope.paises = data.paises;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });
    //Cargando las sedes
    var ajaxSedes = $http.post("catalogos/sedes", {});
    ajaxSedes.success(function (data) {
        if (data.success === true) {
            $scope.sedes = data.sedes;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });
    //Cargando los cargos
    var ajaxCargos = $http.post("catalogos/cargos", {});
    ajaxCargos.success(function (data) {
        if (data.success === true) {
            $scope.cargos = data.cargos;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });
    //Cargando los roles
    var ajaxRoles = $http.post("catalogos/roles", {});
    ajaxRoles.success(function (data) {
        if (data.success === true) {
            //$scope.roles = data.roles;

            angular.element("#div-roles-colaboradores").html("<div><label>Roles:</label></div>");
            angular.element.each(data.roles, function (key, rol) {
                var newDiv = angular.element("<div/>");
                angular.element(newDiv).css({ display: "inline" });

                var newCheck = angular.element("<input>");
                newCheck.attr({ type: "checkbox", value: rol.id, "ng-model": "rol", });

                angular.element(newDiv).append(newCheck);
                angular.element(newDiv).append(":" + rol.nombre + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                angular.element("#div-roles-colaboradores").append(newDiv);
            });
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });
    //Cargando los departamentos
    var ajaxDepartamentos = $http.post("catalogos/departamentos", {});
    ajaxDepartamentos.success(function (data) {
        if (data.success === true) {
            $scope.departamentos = data.departamentos;
        }
        else {
            messageDialog.show('Información', "Error en el acceso a los datos.");
        }
    });

    //Para la clase de la seleccion
    angular.element('#menu-principal').on('click', '[role="presentation"]', function () {
        angular.element('#menu-principal [role="presentation"]').removeClass('active');
        angular.element(this).addClass('active');
    });

    function ocultar() {
        angular.element("#panel_workers").addClass('invisible');
        angular.element("#panel_user_clientes").addClass('invisible');
        angular.element("#panel_contratos_clientes").addClass('invisible');
        angular.element("#panel_catalogos").addClass('invisible');
        angular.element("#panel_proyectos_excel").addClass('invisible');
    };

     $scope.IrProyectosExcel = function () {
        ocultar();
        angular.element("#panel_proyectos_excel").removeClass('invisible');
        $scope.funcionalidad = 'PROYECTOS EXCEL';
    };

    $scope.IrTrabajador = function () {
        ocultar();
        angular.element("#panel_workers").removeClass('invisible');
        $scope.funcionalidad = 'COLABORADORES';

        angular.element('#dtpk-fecha_nac').datepicker({
            format: 'dd/mm/yyyy'
        });

        //Cargando los paises
        var ajaxPaises = $http.post("catalogos/paises", {});
        ajaxPaises.success(function (data) {
            if (data.success === true) {
                $scope.paises = data.paises;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        //Cargando las sedes
        var ajaxSedes = $http.post("catalogos/sedes", {});
        ajaxSedes.success(function (data) {
            if (data.success === true) {
                $scope.sedes = data.sedes;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        //Cargando los cargos
        var ajaxCargos = $http.post("catalogos/cargos", {});
        ajaxCargos.success(function (data) {
            if (data.success === true) {
                $scope.cargos = data.cargos;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        //Cargando los roles
        var ajaxRoles = $http.post("catalogos/roles", {});
        ajaxRoles.success(function (data) {
            if (data.success === true) {
                //$scope.roles = data.roles;

                angular.element("#div-roles-colaboradores").html("<div><label>Roles:</label></div>");
                angular.element.each(data.roles, function (key, rol) {
                    var newDiv = angular.element("<div/>");
                    angular.element(newDiv).css({ display: "inline" });

                    var newCheck = angular.element("<input>");
                    newCheck.attr({ type: "checkbox", value: rol.id, "ng-model": "rol", });

                    angular.element(newDiv).append(newCheck);
                    angular.element(newDiv).append(":" + rol.nombre + "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");

                    angular.element("#div-roles-colaboradores").append(newDiv);
                });

            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
        //Cargando los departamentos
        var ajaxDepartamentos = $http.post("catalogos/departamentos", {});
        ajaxDepartamentos.success(function (data) {
            if (data.success === true) {
                $scope.departamentos = data.departamentos;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });

        //Actualizando el tamaño de la tabla en height    
        var tamanioY = angular.element("#panel_workers").height() - 35 - angular.element("#thead-colaboradores").height();
        angular.element("#tbody-colaboradores").css({
            height: tamanioY
        });
    };

    $scope.IrCatalogos = function () {
        ocultar();
        angular.element("#panel_catalogos").removeClass('invisible');
        $scope.funcionalidad = 'CATÁLOGOS';

        //Cargando los catálogos
        var ajaxCatalogos = $http.post("catalogos/catalogos", {});
        ajaxCatalogos.success(function (data) {
            if (data.success === true) {
                $scope.tablasCatalogos = data.catalogos;
                $scope.campos = [];
                $scope.datosTabla = [];
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };

    $scope.IrCorreosNoEnviados = function () {
        ocultar();
        angular.element("#panel_correosNoEnviados").removeClass('invisible');
        $scope.funcionalidad = 'CORREOS NO ENVIADOS';
        angular.element('[data-toggle="tooltip"]').tooltip();
    };

    $scope.IrClientes = function () {
        ocultar();
        angular.element("#panel_user_clientes").removeClass('invisible');
        $scope.funcionalidad = 'USUARIOS DE CLIENTES';

        angular.element('#dtpk-fecha_nac-client').datepicker({
            format: 'dd/mm/yyyy'
        });

        //Cargando los usuarios-clientes
        var ajaxUsuariosClientes = $http.post("admin/usuarios-clientes", {});
        ajaxUsuariosClientes.success(function (data) {
            if (data.success === true) {
                $scope.usuariosClientes = data.usuariosClientes;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });

        //Cargando los clientes
        var ajaxClientes = $http.post("catalogos/clientes", {});
        ajaxClientes.success(function (data) {
            if (data.success === true) {
                $scope.clientes = data.clientes;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };

    $scope.IrContratosClientes = function () {
        ocultar();
        angular.element("#panel_contratos_clientes").removeClass('invisible');
        $scope.funcionalidad = 'CONTRATOS DE CLIENTES';

        angular.element('#dtpk-fecha_inicio_contrat').datepicker({
            format: 'dd/mm/yyyy'
        });

        angular.element('#dtpk-fecha_fin_contrat').datepicker({
            format: 'dd/mm/yyyy'
        });

        //Cargando los clientes
        var ajaxClientes = $http.post("catalogos/clientes", {});
        ajaxClientes.success(function (data) {
            if (data.success === true) {
                $scope.clientes = data.clientes;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });

        //Cargando los tipos de contratos de clientes
        var ajaxClientes = $http.post("catalogos/tipos-contratos-cliente", {});
        ajaxClientes.success(function (data) {
            if (data.success === true) {
                $scope.tiposContratos = data.tiposContrato;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };

    //Funciones generales
    // Función que suma o resta días a la fecha indicada formato d/m/Y
    sumaFecha = function (d, fecha) {
        var Fecha = new Date();
        var sFecha = fecha || (Fecha.getDate() + "/" + (Fecha.getMonth() + 1) + "/" + Fecha.getFullYear());
        var sep = sFecha.indexOf('/') != -1 ? '/' : '-';
        var aFecha = sFecha.split(sep);
        var fecha = aFecha[2] + '/' + aFecha[1] + '/' + aFecha[0];
        fecha = new Date(fecha);
        fecha.setDate(fecha.getDate() + parseInt(d));
        var anno = fecha.getFullYear();
        var mes = fecha.getMonth() + 1;
        var dia = fecha.getDate();
        mes = (mes < 10) ? ("0" + mes) : mes;
        dia = (dia < 10) ? ("0" + dia) : dia;
        var fechaFinal = dia + sep + mes + sep + anno;
        return (fechaFinal);
    };

    $scope.loadingAjax = function () {
        this.show = function (target) {
            angular.element("#loadingDiv").show();
        };
        this.hide = function (target) {
            angular.element("#loadingDiv").hide();
        };
    };

    $scope.loading = new $scope.loadingAjax();
    $scope.loading.show();

    $('.solo-numero').keyup(function () {
        this.value = (this.value).replace(/[^0-9]/g, '');
    });

    $scope.wind3Opciones = new questionMsgDialog3Option();

    dateToStr = function (dateObj, format, separator) {
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

    $scope.CargarModulos = function () {

        var ajaxModulosReportes = $http.post("admin/ModulosReportes", {});

        ajaxModulosReportes.success(function (data) {
            if (data.success === true) {
                $scope.modulosReportes = data.datos;
            }
            else {
                messageDialog.show('Información', "Error al cargar los modulos.");
            }
        });

    }

    $scope.CargarNombreReportes = function () {

        $scope.moduloText = angular.element("#select-modulos")[0].value

        var ajaxUrlReportes = $http.post("admin/NombreReportes", {
            modulo: $scope.moduloText
        });

        ajaxUrlReportes.success(function (data) {
            if (data.success === true) {
                $scope.reportesText = data.datos;
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    }

    $scope.VisualizarReporte = function () {

        angular.element("#vista-reporte").html("");

        $scope.nombreModulo = angular.element("#select-modulos")[0].value

        $scope.nombreReporte = angular.element("#select-nombre-reporte")[0].value

        if ($scope.nombreReporte == "Seleccione..." || $scope.nombreModulo == "Seleccione...") {
            messageDialog.show('Información', "Porfavor seleccione el reporte que desea visualizar");
        } else {
            angular.element("#ver-reporte").attr({ "href": "/Report/VerReporte?modulo=" + $scope.moduloText + "&reporte=" + $scope.nombreReporte });
            angular.element("#ver-reporte")[0].click();

        //var ajaxVerReporte = $http.post("report/VerReporte", {
        //    modulo: $scope.moduloText,
        //    reporte: $scope.nombreReporte
        //});

        //ajaxVerReporte.success(function (data) {
        //    angular.element("#vista-reporte").html(data)
        //});
        }
    }

}]);

//Filter de angular para las fechas
adminApp.filter("strDateToStr", function () {
    return function (textDate) {
        var fecha = new Date(parseInt(textDate.replace('/Date(', '')));
        return dateToStr(fecha);
    }
});

$(window).resize(function () {
    //Actualizando el tamaño de la tabla en height    
    var tamanioY = angular.element("#panel_workers").height() - 35 - angular.element("#thead-colaboradores").height();
    angular.element("#tbody-colaboradores").css({
        height: tamanioY
    });
});