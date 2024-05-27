devApp.controller("taskController", [
    "$scope",
    "$http",
    function ($scope, $http) {
        var taskProxie = angular.element.connection.websocket;
        angular.element.connection.hub.start();

        $scope.diasCalendar = [];
        $scope.numeroSemanas = 1;
        var aplicarFiltro = false;
        $scope.coordinados = false;
        $scope.importanciaActividad = "Normal";

        $scope.diasActividadTarea = [];
        $scope.diasActividad = [];
        $scope.diasActividadCord = [];
        //Cargando los dias de tareas desde el negocio
        var ajaxDiasDeTareas = $http.post("task/dias-actividades-tareas", {});
        ajaxDiasDeTareas.success(function (data) {
            if (data.success === true) {
                $scope.diasActividadTarea = data.dias;
                $scope.diasActividad = data.dias;
                $scope.diasActividadCord = data.diasCord;
            } else {
                messageDialog.show("Información", data.msg);
            }
        });
        $scope.horas = [];
        for (var i = 0; i < 24; i++) {
            $scope.horas.push(i);
        }
        $scope.minutos = [];
        for (var i = 0; i < 60; i++) {
            $scope.minutos.push(i);
        }

        // Obtén la plantilla y añade un ID único
        var fileInputTemplate = angular.element("#htmlFile").html();
        var uniqueId = "adjunto-publicacion"; // Genera un ID único de alguna manera
        var fileInputWithId = fileInputTemplate.replace(
            '<input type="file"',
            '<input type="file" id="' + uniqueId + '"'
        );
        angular.element("#panel-adjunto-publicacion").append(fileInputWithId);

        var fileInputTemplates = angular.element("#htmlFiles").html();
        var uniqueIds = "adjunto-publicacion-tarea"; // Genera un ID único de alguna manera
        var fileInputWithIds = fileInputTemplates.replace(
            '<input type="file"',
            '<input type="file" id="' + uniqueIds + '"'
        );
        angular
            .element("#panel-adjunto-publicacion-tarea")
            .append(fileInputWithIds);

        var fileInputTemplate2 = angular.element("#htmlFile2").html();
        var uniqueId2 = "adjunto-no-finalizado"; // Genera un ID único de alguna manera
        var fileInputWithId2 = fileInputTemplate.replace(
            '<input type="file"',
            '<input type="file" id="' + uniqueId2 + '"'
        );
        angular.element("#panel-adjunto-no-finalizado").append(fileInputWithId2);

        angular
            .element(document)
            .on("click", '[data-toggle="popover"]', function () {
                var obj = this;

                var elements = angular.element('[data-toggle="popover"]');
                angular.element.each(elements, function (i, e) {
                    if (e !== obj) {
                        angular.element(e).popover("hide");
                    } else {
                        angular.element(obj).popover("toggle");
                    }
                });
            });

        $scope.semanaProxima = function () {
            var ndias = 7;
            $scope.fechaLunes = sumaFecha(ndias, $scope.fechaLunes);
            if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
            else
                actualizarDatosTarea($scope.fechaLunes, angular.toJson(filtroTareas));
        };

        $scope.cambiarSemanas = function () {
            if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
            else
                actualizarDatosTarea($scope.fechaLunes, angular.toJson(filtroTareas));
        };

        $scope.semanaAtras = function () {
            var ndias = 7 * -1;
            $scope.fechaLunes = sumaFecha(ndias, $scope.fechaLunes);
            if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
            else
                actualizarDatosTarea($scope.fechaLunes, angular.toJson(filtroTareas));
        };

        $scope.diaHoy = function () {
            $scope.fechaLunes = $scope.esteLunes;
            if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
            else
                actualizarDatosTarea($scope.fechaLunes, angular.toJson(filtroTareas));
        };

        angular
            .element("#control-filtrar .datepicker-filtro")
            .datepicker({
                format: "dd/mm/yyyy",
            })
            .on("changeDate", function (e) {
                var sFecha = angular
                    .element("#control-filtrar .datepicker-filtro")
                    .val();
                var aFecha = sFecha.split("/");
                var fecha = aFecha[2] + "/" + aFecha[1] + "/" + aFecha[0];
                fecha = new Date(fecha);
                var diaSemana = fecha.getDay();
                var ndias = -6; //Por si es domingo
                if (diaSemana !== 0)
                    //Si no es domingo
                    var ndias = 1 - diaSemana;

                $scope.fechaLunes = sumaFecha(ndias, sFecha);
                if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
                else
                    actualizarDatosTarea($scope.fechaLunes, angular.toJson(filtroTareas));
            });

        //Actualizar datosTarea
        actualizarDatosTarea = function (fechaLunes, jsonFiltro) {
            if (jsonFiltro === undefined) jsonFiltro = "";
            $scope.loading.show();
            //Cargando las tareas
            var numeroSemanas = $scope.numeroSemanas;
            //Quitando los popuovers
            angular.element('[data-toggle="popover"]').popover("hide");
            var ajaxDatosTareas = $http.post("user/tareas-usuario", {
                fechaLunes: fechaLunes,
                semanas: numeroSemanas,
                json: jsonFiltro,
                coordinados: $scope.coordinados,
            });
            ajaxDatosTareas.success(function (data) {
                if (data.success === true) {
                    $scope.trabajadores = data.trabajadores;
                    $scope.actualizarListaDias(fechaLunes, $scope.fechaHoy);
                    $scope.loading.hide();
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        //Cargando el ultimo lunes
        var ajaxUltimoLunes = $http.post("user/ultimo-lunes", {});
        ajaxUltimoLunes.success(function (data) {
            if (data.success === true) {
                $scope.fechaLunes = data.lunes;
                $scope.esteLunes = data.lunes;
                $scope.fechaHoy = data.hoy;
                actualizarDatosTarea($scope.fechaLunes);
                $("#control-filtrar .datepicker-filtro").datepicker(
                    "update",
                    $scope.fechaHoy
                );
            } else {
                //alert("Error en la obtención de la fecha.")
            }
        });

        //Cargando los tipos de actividades realizadas
        var ajaxActividadesR = $http.post("catalogos/actividades-realizadas", {});
        ajaxActividadesR.success(function (data) {
            if (data.success === true) {
                $scope.actividadesTarea = data.actividadesRealizadas;
            } else {
                //alert("Error en el acceso a los datos.")
            }
        });

        //Cargando los tipos de actividad de no terminacion de tareas
        var ajaxActividadesNT = $http.post(
            "catalogos/actividades-propuestas-nt",
            {}
        );
        ajaxActividadesNT.success(function (data) {
            if (data.success === true) {
                $scope.proximasActividadesNT = data.proximasActividadesNT;
            } else {
                //alert("Error en el acceso a los datos.")
            }
        });

        //Cargando las causas de las no terminacion de tareas
        var ajaxCausasNT = $http.post("catalogos/causas-no-terminacion", {});
        ajaxCausasNT.success(function (data) {
            if (data.success === true) {
                $scope.causasNoTerminacion = data.causasNoTerminacion;
            } else {
                //alert("Error en el acceso a los datos.")
            }
        });

        //Funcion que actualiza la fecha de los dias de la semana en el calendario de las tareas
        $scope.actualizarListaDias = function (fechaLunes, fechaHoy) {
            $scope.diasCalendar = [];

            var dias = ["Lun ", "Mar ", "Mie ", "Jue ", "Vie ", "Sab ", "Dom "];
            var clases = ["dia-tarea1", "dia-tarea2", "dia-tarea3", "dia-tarea3"];
            var numeroSemanas = angular.element("#selectSemanas").val();
            var cant = numeroSemanas * 7;
            var i = 0;
            var clase = clases[numeroSemanas - 1];
            while (i < cant) {
                for (var j = 0; j < dias.length; j++) {
                    var dia = dias[j];
                    var fecha = sumaFecha(i, fechaLunes);
                    i++;

                    var obj = {
                        dia: dias[j] + fecha,
                        clase: clase,
                        date: fecha,
                    };
                    if (fecha === fechaHoy) {
                        obj.clase = obj.clase + " dia-hoy";
                    } else if (j === 5 || j === 6) {
                        obj.clase = obj.clase + " fin-semana";
                    }
                    $scope.diasCalendar.push(obj);
                }
            }
            var ancho = (1021 - numeroSemanas * 7 + 1) / (numeroSemanas * 7);
            $scope.anchoCeldaTarea = ancho;
            angular.element(".td-tareas").css({ width: ancho });
        };

        //Variable del filtro
        var filtroTareas = {
            colaboradores: [],
            clientes: [],
            estadoTarea: [],
            lugarTarea: [],
            modulo: [],
            sede: [],
        };

        var objetoFiltro = 0;

        //Filtrar las tareas
        $scope.filtrarTareas = function () {
            angular.element("#modalNewFilter").modal("show");
            //$scope.filtroText = angular.toJson(filtroTareas);
        };

        $scope.anadirFiltroColaboradores = function () {
            objetoFiltro = 0;
            angular.element(".panel-valores-filtro").html("");
            var cantTrabajadores = $scope.trabajadores.length;
            for (var i = 0; i < cantTrabajadores; i++) {
                var trab = $scope.trabajadores[i];
                var divDato = document.createElement("div");
                angular.element(divDato).attr({ class: "divDatosFiltro" });

                var newCheck = document.createElement("input");
                angular.element(newCheck).attr({
                    type: "checkbox",
                    value: trab.trab.id,
                    class: "filtroData",
                    "data-text": $scope.mySplitEmail(trab.trab.email, 0),
                });
                if (buscarEnArreglo(filtroTareas.colaboradores, trab.trab.id)) {
                    angular.element(newCheck).attr({ checked: true });
                }

                angular.element(divDato).append(newCheck);
                angular
                    .element(divDato)
                    .append(":" + $scope.mySplitEmail(trab.trab.email, 0));

                angular.element(".panel-valores-filtro").append(divDato);
            }
        };

        $scope.anadirFiltroClientes = function () {
            objetoFiltro = 1;
            angular.element(".panel-valores-filtro").html("");

            var cantTrabajadores = $scope.trabajadores.length;
            var arregloClientes = [];
            for (var i = 0; i < cantTrabajadores; i++) {
                var trab = $scope.trabajadores[i];

                var tareasPorDia = trab.tareasPorDia;
                var cantDias = tareasPorDia.length;
                for (var j = 0; j < cantDias; j++) {
                    var tareas = tareasPorDia[j].tareas;
                    var cantTareas = tareas.length;
                    for (var k = 0; k < cantTareas; k++) {
                        var tarea = tareas[k];
                        var clienteTarea = {
                            id: tarea.idCliente,
                            text: tarea.dCliente,
                        };
                        if (
                            buscarEnArreglo(arregloClientes, clienteTarea.id) === false &&
                            clienteTarea.text !== "PERMISO"
                        ) {
                            arregloClientes.push(clienteTarea);
                        }
                    }
                }
            }

            for (var i = 0; i < arregloClientes.length; i++) {
                var cliente = arregloClientes[i];
                var divDato = document.createElement("div");
                angular.element(divDato).attr({ class: "divDatosFiltro" });

                var newCheck = document.createElement("input");
                angular.element(newCheck).attr({
                    type: "checkbox",
                    value: cliente.id,
                    class: "filtroData",
                    "data-text": cliente.text,
                });
                if (buscarEnArreglo(filtroTareas.clientes, cliente.id)) {
                    angular.element(newCheck).attr({ checked: true });
                }

                angular.element(divDato).append(newCheck);
                angular.element(divDato).append(":" + cliente.text);

                angular.element(".panel-valores-filtro").append(divDato);
            }
        };

        $scope.anadirFiltroEstadoTarea = function () {
            objetoFiltro = 2;
            angular.element(".panel-valores-filtro").html("");

            var cantTrabajadores = $scope.trabajadores.length;
            var arregloEstados = [];
            for (var i = 0; i < cantTrabajadores; i++) {
                var trab = $scope.trabajadores[i];

                var tareasPorDia = trab.tareasPorDia;
                var cantDias = tareasPorDia.length;
                for (var j = 0; j < cantDias; j++) {
                    var tareas = tareasPorDia[j].tareas;
                    var cantTareas = tareas.length;
                    for (var k = 0; k < cantTareas; k++) {
                        var tarea = tareas[k];
                        var estadoTarea = {
                            id: tarea.idEstado,
                            text: tarea.estado,
                        };
                        if (
                            buscarEnArreglo(arregloEstados, estadoTarea.id) === false &&
                            estadoTarea.text !== ""
                        ) {
                            arregloEstados.push(estadoTarea);
                        }
                    }
                }
            }

            for (var i = 0; i < arregloEstados.length; i++) {
                var estado = arregloEstados[i];
                var divDato = document.createElement("div");
                angular.element(divDato).attr({ class: "divDatosFiltro" });

                var newCheck = document.createElement("input");
                angular.element(newCheck).attr({
                    type: "checkbox",
                    value: estado.id,
                    class: "filtroData",
                    "data-text": estado.text,
                });
                if (buscarEnArreglo(filtroTareas.estadoTarea, estado.id)) {
                    angular.element(newCheck).attr({ checked: true });
                }

                angular.element(divDato).append(newCheck);
                angular.element(divDato).append(":" + estado.text);

                angular.element(".panel-valores-filtro").append(divDato);
            }
        };

        $scope.anadirFiltroLugarTarea = function () {
            objetoFiltro = 3;
            angular.element(".panel-valores-filtro").html("");

            var cantTrabajadores = $scope.trabajadores.length;
            var arregloLugares = [];
            for (var i = 0; i < cantTrabajadores; i++) {
                var trab = $scope.trabajadores[i];

                var tareasPorDia = trab.tareasPorDia;
                var cantDias = tareasPorDia.length;
                for (var j = 0; j < cantDias; j++) {
                    var tareas = tareasPorDia[j].tareas;
                    var cantTareas = tareas.length;
                    for (var k = 0; k < cantTareas; k++) {
                        var tarea = tareas[k];
                        var lugarTarea = {
                            id: tarea.idLugar,
                            text: tarea.dLugar,
                        };
                        if (
                            buscarEnArreglo(arregloLugares, lugarTarea.id) === false &&
                            lugarTarea.text !== ""
                        ) {
                            arregloLugares.push(lugarTarea);
                        }
                    }
                }
            }

            for (var i = 0; i < arregloLugares.length; i++) {
                var lugar = arregloLugares[i];
                var divDato = document.createElement("div");
                angular.element(divDato).attr({ class: "divDatosFiltro" });

                var newCheck = document.createElement("input");
                angular.element(newCheck).attr({
                    type: "checkbox",
                    value: lugar.id,
                    class: "filtroData",
                    "data-text": lugar.text,
                });
                if (buscarEnArreglo(filtroTareas.lugarTarea, lugar.id)) {
                    angular.element(newCheck).attr({ checked: true });
                }

                angular.element(divDato).append(newCheck);
                angular.element(divDato).append(":" + lugar.text);

                angular.element(".panel-valores-filtro").append(divDato);
            }
        };

        $scope.anadirFiltroModulo = function () {
            objetoFiltro = 4;
            angular.element(".panel-valores-filtro").html("");

            var cantTrabajadores = $scope.trabajadores.length;
            var arregloModulos = [];
            for (var i = 0; i < cantTrabajadores; i++) {
                var trab = $scope.trabajadores[i];

                var tareasPorDia = trab.tareasPorDia;
                var cantDias = tareasPorDia.length;
                for (var j = 0; j < cantDias; j++) {
                    var tareas = tareasPorDia[j].tareas;
                    var cantTareas = tareas.length;
                    for (var k = 0; k < cantTareas; k++) {
                        var tarea = tareas[k];
                        var moduloTarea = {
                            id: tarea.idModulo,
                            text: tarea.dModulo,
                        };
                        if (
                            buscarEnArreglo(arregloModulos, moduloTarea.id) === false &&
                            moduloTarea.text !== ""
                        ) {
                            arregloModulos.push(moduloTarea);
                        }
                    }
                }
            }

            for (var i = 0; i < arregloModulos.length; i++) {
                var modulo = arregloModulos[i];
                var divDato = document.createElement("div");
                angular.element(divDato).attr({ class: "divDatosFiltro" });

                var newCheck = document.createElement("input");
                angular.element(newCheck).attr({
                    type: "checkbox",
                    value: modulo.id,
                    class: "filtroData",
                    "data-text": modulo.text,
                });
                if (buscarEnArreglo(filtroTareas.modulo, modulo.id)) {
                    angular.element(newCheck).attr({ checked: true });
                }

                angular.element(divDato).append(newCheck);
                angular.element(divDato).append(":" + modulo.text);

                angular.element(".panel-valores-filtro").append(divDato);
            }
        };

        $scope.anadirFiltroSede = function () {
            objetoFiltro = 5;
            angular.element(".panel-valores-filtro").html("");

            var cantTrabajadores = $scope.trabajadores.length;
            var arregloSedes = [];
            for (var i = 0; i < cantTrabajadores; i++) {
                var trab = $scope.trabajadores[i];
                var datosTrab = trab.trab;
                var sede = {
                    id: datosTrab.idSede,
                    text: datosTrab.dSede,
                };
                if (buscarEnArreglo(arregloSedes, sede.id) === false) {
                    arregloSedes.push(sede);
                }
            }

            for (var i = 0; i < arregloSedes.length; i++) {
                var sede = arregloSedes[i];
                var divDato = document.createElement("div");
                angular.element(divDato).attr({ class: "divDatosFiltro" });

                var newCheck = document.createElement("input");
                angular.element(newCheck).attr({
                    type: "checkbox",
                    value: sede.id,
                    class: "filtroData",
                    "data-text": sede.text,
                });
                if (buscarEnArreglo(filtroTareas.sede, sede.id)) {
                    angular.element(newCheck).attr({ checked: true });
                }

                angular.element(divDato).append(newCheck);
                angular.element(divDato).append(":" + sede.text);

                angular.element(".panel-valores-filtro").append(divDato);
            }
        };

        angular.element("#modalNewFilter").on("click", ".filtroData", function () {
            actualizarDatosFiltro();
        });

        function actualizarDatosFiltro() {
            var tipoFiltro = null;
            switch (objetoFiltro) {
                case 0:
                    tipoFiltro = "colaboradores";
                    break;
                case 1:
                    tipoFiltro = "clientes";
                    break;
                case 2:
                    tipoFiltro = "estadoTarea";
                    break;
                case 3:
                    tipoFiltro = "lugarTarea";
                    break;
                case 4:
                    tipoFiltro = "modulo";
                    break;
                case 5:
                    tipoFiltro = "sede";
                    break;
            }

            filtroTareas[tipoFiltro] = [];
            angular.element("#modalNewFilter .filtroData:checked").each(function () {
                var element = {
                    id: angular.element(this).val(),
                    text: angular.element(this).attr("data-text"),
                };
                filtroTareas[tipoFiltro].push(element);
            });
            ponerDatosFiltro();
        }

        $scope.marcarTodosValores = function () {
            angular.element(".filtroData:not(checked)").prop("checked", true);
            actualizarDatosFiltro();
        };

        $scope.desmarcarTodosValores = function () {
            angular.element(".filtroData:checked").prop("checked", false);
            actualizarDatosFiltro();
        };

        $scope.aplicarFiltroTareas = function () {
            aplicarFiltro = true;
            actualizarDatosTarea($scope.fechaLunes, angular.toJson(filtroTareas));
            angular.element("#modalNewFilter").modal("hide");
        };

        $scope.limpiarFiltroTareas = function () {
            aplicarFiltro = false;
            $scope.coordinados = false;
            actualizarDatosTarea($scope.fechaLunes);
            filtroTareas = {
                colaboradores: [],
                clientes: [],
                estadoTarea: [],
                lugarTarea: [],
                modulo: [],
                sede: [],
            };
            angular
                .element(".panel-valores-filtro")
                .html("Por favor seleccione un campo de filtrado");
            angular.element(".panel-json-filtro").html("Filtro");
        };

        function ponerDatosFiltro() {
            var html = "Filtro:<br/>Colaboradores: ";
            for (var i = 0; i < filtroTareas.colaboradores.length; i++) {
                html = html + filtroTareas.colaboradores[i].text;
                if (i < filtroTareas.colaboradores.length - 1) {
                    html = html + ", ";
                }
            }

            html = html + "<br/>Clientes:";

            for (var i = 0; i < filtroTareas.clientes.length; i++) {
                html = html + filtroTareas.clientes[i].text;
                if (i < filtroTareas.clientes.length - 1) {
                    html = html + ", ";
                }
            }

            html = html + "<br/>Estado de tarea:";

            for (var i = 0; i < filtroTareas.estadoTarea.length; i++) {
                html = html + filtroTareas.estadoTarea[i].text;
                if (i < filtroTareas.estadoTarea.length - 1) {
                    html = html + ", ";
                }
            }

            html = html + "<br/>Lugar de tarea:";

            for (var i = 0; i < filtroTareas.lugarTarea.length; i++) {
                html = html + filtroTareas.lugarTarea[i].text;
                if (i < filtroTareas.lugarTarea.length - 1) {
                    html = html + ", ";
                }
            }

            html = html + "<br/>Módulo:";

            for (var i = 0; i < filtroTareas.modulo.length; i++) {
                html = html + filtroTareas.modulo[i].text;
                if (i < filtroTareas.modulo.length - 1) {
                    html = html + ", ";
                }
            }

            html = html + "<br/>Sede:";

            for (var i = 0; i < filtroTareas.sede.length; i++) {
                html = html + filtroTareas.sede[i].text;
                if (i < filtroTareas.sede.length - 1) {
                    html = html + ", ";
                }
            }

            angular.element(".panel-json-filtro").html(html);
        }

        //Funcion auxilar
        function buscarEnArreglo(arreglo, clave) {
            var cant = arreglo.length;
            for (var i = 0; i < cant; i++) {
                var item = arreglo[i];
                if (item.id == clave) {
                    return true;
                }
            }
            return false;
        }

        //Filtrar mostrar solo los coordinados
        $scope.verCoordinadosPersona = function (idTrabajador) {
            filtroTareas.colaboradores = [];
            if ($scope.coordinados === false) {
                aplicarFiltro = true;
                $scope.coordinados = true;

                var buscarCoordinados = $http.post("user/coordinados-usuario", {
                    idTrabajador: idTrabajador,
                    fechaLunes: $scope.fechaLunes,
                    semanas: $scope.numeroSemanas,
                });
                buscarCoordinados.success(function (data) {
                    if (data.success === true) {
                        angular.element.each(data.dataFilter, function (key, obj) {
                            var element = {
                                id: obj.id.toString(),
                                text: obj.text,
                            };
                            filtroTareas.colaboradores.push(element);
                        });
                        actualizarDatosTarea(
                            $scope.fechaLunes,
                            angular.toJson(filtroTareas)
                        );
                    } else {
                        messageDialog.show("Información", data.msg);
                    }
                });
            } else {
                $scope.coordinados = false;
                aplicarFiltro = false;
                actualizarDatosTarea($scope.fechaLunes);
            }
        };

        $scope.estiloSegunEstadoCoordinacion = function (id, index) {
            if (index === 0) {
                if ($scope.coordinados === false)
                    return Array(
                        "btn-ver-coordinaciones-persona right glyphicon glyphicon-hand-right",
                        "Ver coordinados por el colaborador"
                    );
                else
                    return Array(
                        "btn-ver-coordinaciones-persona right glyphicon glyphicon-ban-circle",
                        "Quitar filtro de coordinados por el colaborador"
                    );
            } else return "";
        };

        //Cambiar de estados las tareas
        angular.element(document).on("click", ".btn-tarea-desarrollo", function () {
            waitingDialog.show("Cambiando estado de tarea la tarea...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var id = parseInt(angular.element(this).attr("data-id-tarea"));
            cambiarEstadoDeTarea(id, 2);
        });

        //Publicar Datos Tarea
        angular.element(document).on("click", ".btn-tarea-publicar", function () {
            angular.element("#modal-publicacion-tarea").modal("show");
            $scope.idTareaPublicar = parseInt(
                angular.element(this).attr("data-id-tarea")
            );
        });

        angular.element(document).on("click", ".btn-tarea-finish", function () {
            var obj = this;
            $scope.$apply(function () {
                $scope.idTareaTerminar = parseInt(
                    angular.element(obj).attr("data-id-tarea")
                );
            });

            var ajaxMotivosTrabajo = $http.post("user/dar-entregable-tarea", {
                idTarea: $scope.idTareaTerminar,
            });
            ajaxMotivosTrabajo.success(function (data) {
                if (data.success === true) {
                    console.log(data.datos);
                    $scope.referencias = data.datos;
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });

            var ajaxActividades = $http.post("user/dar-actividades-tarea", {
                idTarea: $scope.idTareaTerminar,
            });
            ajaxActividades.success(function (data) {
                if (data.success === true) {
                    $scope.actividadesRealizadas = data.actividadesTarea;
                    $scope.tiempoUtilizado = data.totalHoras;
                    if (data.tareaPropia == false) {
                        $scope.diasActividadTarea = $scope.diasActividadCord;
                    } else {
                        $scope.diasActividadTarea = $scope.diasActividad;
                    }
                    //$scope.tieneContrato = data.tieneContrato;
                    //$scope.tieneTicket = data.tieneTicket;
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });

            var ajaxActividadRealizadaTarea = $http.post(
                "user/dar-actividades-segun-actividad-tarea",
                {
                    idTarea: $scope.idTareaTerminar,
                }
            );
            ajaxActividadRealizadaTarea.success(function (data) {
                if (data.success === true) {
                    $scope.actividadesTarea = data.actividadesTarea;
                    //$scope.numeroTicket = "";
                    //$scope.entregable = "";
                    //$scope.esTicket = true;
                    angular.element("#modal-final-tarea").modal("show");
                } else {
                    angular.element("#modal-final-tarea").modal("hide");
                    messageDialog.show("Información", data.msg);
                }
            });
        });

        angular.element(document).on("click", ".btn-tarea-pausa", function () {
            waitingDialog.show("Cambiando estado de tarea la tarea...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var obj = this;
            $scope.$apply(function () {
                $scope.idTareaTerminar = parseInt(
                    angular.element(obj).attr("data-id-tarea")
                );
            });
            cambiarEstadoDeTarea($scope.idTareaTerminar, 5);
        });

        $scope.idTareaTicket = 0;
        //Para las tareas del ticket
        angular.element(document).on("click", ".btn-mensaje-ticket", function () {
            var parent = angular.element(this).parent();
            var idTarea = angular.element(parent).attr("data-id-tarea");
            $scope.idTareaTicket = idTarea;

            var buscarTiposNotificacion = $http.post(
                "user/tipo-notificacion-ticket",
                {
                    idTarea: idTarea,
                }
            );
            buscarTiposNotificacion.success(function (data) {
                if (data.success === true) {
                    $scope.tiposNotificacion = data.tipos;
                    angular.element("#modal-mensaje-desarrollo-ticket").modal("show");
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        });

        angular.element(document).on("click", ".btn-info-ticket", function () {
            var parent = angular.element(this).parent();
            var idTarea = angular.element(parent).attr("data-id-tarea");
            $scope.idTareaTicket = idTarea;

            $scope.loading.show();
            var adicionar = $http.post("user/ver-datos-desarrollo-ticket", {
                idTarea: idTarea,
            });
            adicionar.success(function (data) {
                $scope.loading.hide();
                if (data.success === true) {
                    $scope.detalleTicket = data.detalleTicket;
                    $scope.textoResolucion = data.textoResolucion;
                    $scope.adjuntosTicket = data.adjuntosTicket;
                    $scope.comentariosResueltos = data.comentarios;

                    angular.element("#modal-info-dev-ticket").modal("show");
                    angular.element("#datos-historico-ticket").html(data.comentarios);
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        });

        $scope.enviarNotificacionTicket = function () {
            waitingDialog.show("Adicionando Actividad...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var enviarNotificacion = $http.post("user/enviar-notificacion-ticket", {
                idTarea: $scope.idTareaTicket,
                tipoNotificacion: $scope.motivoNotificacionDevTicket,
                detalle: $scope.notificacionDevTicket,
            });
            enviarNotificacion.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.motivoNotificacionDevTicket = "";
                    $scope.notificacionDevTicket = "";
                    angular.element("#modal-mensaje-desarrollo-ticket").modal("hide");
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        //Adiciona una actividad a una tarea
        var dtpkHoraInicioActividad = angular
            .element("#dtpk-hora-inicio-actividad-tarea")
            .datetimepicker({
                format: "HH:mm",
                locale: "es",
            });
        angular
            .element(document)
            .on("dp.update", "#dtpk-hora-inicio-actividad-tarea", function (e) {
                $scope.horaInicioActividadTarea = e.date._i;
            });
        var dtpkHoraFinActividad = angular
            .element("#dtpk-hora-fin-actividad-tarea")
            .datetimepicker({
                format: "HH:mm",
                locale: "es",
            });
        angular
            .element(document)
            .on("dp.update", "#dtpk-hora-fin-actividad-tarea", function (e) {
                $scope.horaFinActividadTarea = e.date._i;
            });
        var dtpkCambioHoraInicioActividad = angular
            .element("#dtpk-cambio-hora-inicio-actividad-tarea")
            .datetimepicker({
                format: "HH:mm",
                locale: "es",
            });
        angular
            .element(document)
            .on(
                "dp.update",
                "#dtpk-cambio-hora-inicio-actividad-tarea",
                function (e) {
                    $scope.cambioHoraInicioActividadTarea = e.date._i;
                }
            );
        var dtpkCambioHoraFinActividad = angular
            .element("#dtpk-cambio-hora-fin-actividad-tarea")
            .datetimepicker({
                format: "HH:mm",
                locale: "es",
            });
        angular
            .element(document)
            .on("dp.update", "#dtpk-cambio-hora-fin-actividad-tarea", function (e) {
                $scope.cambioHoraFinActividadTarea = e.date._i;
            });

        $scope.adicionarActividadTarea = function () {
            //if (!$scope.tieneContrato && !$scope.tieneTicket) {
            //    if ($scope.esTicket && !$scope.numeroTicket) {
            //        alert('El número de ticket es requerido.');
            //        return;
            //    }
            //    if (!$scope.esTicket && !$scope.entregable) {
            //        alert('Seleccione un contrato.');
            //        return;
            //    }
            //}

            waitingDialog.show("Adicionando Actividad...", {
                dialogSize: "sm",
                progressType: "success",
            });

            var adicionar = $http.post("user/adicionar-actividad-tarea", {
                idTarea: $scope.idTareaTerminar,
                tipoTarea: $scope.tipoActividadTarea,
                fecha: $scope.diaActividadTara,
                horaInicio: $('[ng-model="horaInicioActividadTarea"]').val(),
                horaFin: $('[ng-model="horaFinActividadTarea"]').val(),
                //ticketTarea: $scope.numeroTicket,
                //referencia: $scope.entregable
            });
            adicionar.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.tipoActividadTarea = "";
                    $scope.diaActividadTara = "";
                    $scope.horaInicioActividadTarea = "";
                    $scope.horaFinActividadTarea = "";
                    $scope.actividadesRealizadas = data.actividadesTarea;
                    $scope.tiempoUtilizado = data.totalHoras;
                    //$scope.tieneTicket = data.tieneTicket;
                    //$scope.tieneContrato = data.tieneContrato;
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.idActividadTarea = "";
        $scope.editarHoraActividad = function (id) {
            $scope.idActividadTarea = id;
            $scope.cambioHoraInicioActividadTarea = this.actividad.horaInicio;
            $scope.cambioHoraFinActividadTarea = this.actividad.horaFin;
            angular.element("#modal-cambio-horario-actividad").modal("show");
        };

        $scope.actualizarHorasTarea = function () {
            waitingDialog.show("Actualizando Horarios de Actividad...", {
                dialogSize: "sm",
                progressType: "success",
            });

            var actualizar = $http.post("user/actualizar-hora-actividad-tarea", {
                idActividadTarea: $scope.idActividadTarea,
                horaInicio: $('[ng-model="cambioHoraInicioActividadTarea"]').val(),
                horaFin: $('[ng-model="cambioHoraFinActividadTarea"]').val(),
            });
            actualizar.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.horaInicioActividadTarea = "";
                    $scope.horaFinActividadTarea = "";

                    $scope.actividadesRealizadas = data.actividadesTarea;
                    $scope.tiempoUtilizado = data.totalHoras;
                    angular.element("#modal-cambio-horario-actividad").modal("hide");
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.eliminarActividad = function (id) {
            waitingDialog.show("Eliminando la actividad de la tarea...", {
                dialogSize: "sm",
                progressType: "success",
            });

            var ajaxActividad = $http.post("user/eliminar-actividad-tarea-usuario", {
                idActividadTarea: id,
            });
            ajaxActividad.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    var ajaxActividades = $http.post("user/dar-actividades-tarea", {
                        idTarea: $scope.idTareaTerminar,
                    });
                    ajaxActividades.success(function (data) {
                        if (data.success === true) {
                            $scope.actividadesRealizadas = data.actividadesTarea;
                            $scope.tiempoUtilizado = data.totalHoras;
                        } else {
                            messageDialog.show("Información", data.msg);
                        }
                    });
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.wind3Opciones = new questionMsgDialog3Option();

        //FUNCIONES DE FINALIZAR LA TAREA
        $scope.tipoTarea = "";
        $scope.finalizarTarea = function () {
            //PERTENECE A TICKETS O CONTRATOS
            waitingDialog.show("Verificando la tarea...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var finalizar = $http.post("user/tarea-pertenece-ticket", {
                idTarea: $scope.idTareaTerminar,
                estado: 3,
            });
            finalizar.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    if (data.pertenece === true) {
                        $scope.tipoTarea = data.tipo;
                        $scope.wind3Opciones.show(
                            "Confirmación de Finalización",
                            "La presente tarea termina la asignación referente a un ticket, proyecto o contrato, si esta tarea termina completamente la asignación, seleccione 'EL TRABAJO ESTÁ REALIZADO'; en caso contrario si no ha terminado o el trabajo necesita TESTING seleccione 'EL TRABAJO ESTA POR TERMINAR'.",
                            "EL TRABAJO ESTÁ REALIZADO",
                            "EL TRABAJO ESTA POR TERMINAR",
                            function () {
                                $scope.wind3Opciones.hide();
                                if ($scope.tipoTarea === "ticket") {
                                    angular.element("#modal-email-fin-ticket").modal("show");
                                } else if ($scope.tipoTarea === "contrato") {
                                    $scope.tipoTarea = "";
                                    $scope.ejecutarFinalizarTarea();
                                }
                            },
                            function () {
                                $scope.wind3Opciones.hide();
                                if ($scope.tipoTarea === "ticket") {
                                    angular
                                        .element("#modal-comentario-no-terminacion")
                                        .modal("show");
                                } else if ($scope.tipoTarea === "contrato") {
                                    angular
                                        .element("#modal-comentario-no-terminacion-tarea-contrato")
                                        .modal("show");
                                }
                            }
                        );
                    } else {
                        $scope.tipoTarea = "";
                        $scope.ejecutarFinalizarTarea();
                    }
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.ejecutarFinalizarTarea = function (fn) {
            waitingDialog.show("Cambiando estado de tarea la tarea...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var idTarea = $scope.idTareaTerminar;
            var finalizar = $http.post("user/actualizar-tarea-usuario", {
                idTarea: $scope.idTareaTerminar,
                estado: 3,
                publicar: $scope.requierePublicacion,
            });
            finalizar.success(function (data) {
                if (data.success === true) {
                    $scope.wind3Opciones.hide();
                    angular.element("#modal-final-tarea").modal("hide");
                    angular.element.connection.hub.start().done(function () {
                        taskProxie.server.actualizarTareas();
                    });
                    $scope.tiempoUtilizado = "";
                    if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
                    else
                        actualizarDatosTarea(
                            $scope.fechaLunes,
                            angular.toJson(filtroTareas)
                        );

                    if (fn !== undefined) {
                        fn(idTarea);
                    } else {
                        waitingDialog.hide();
                    }
                } else {
                    waitingDialog.hide();
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.enviarComentarioNoTerminacion = function () {
            waitingDialog.show("Enviando comentario de no terminación...", {
                dialogSize: "sm",
                progressType: "success",
            });

            var descripcion = "";
            var tags = [];

            if ($scope.publicarClienteNT) {
                descripcion += "Se va a publicar el cliente. ";
                tags.push("CLIENTE");
                if ($scope.actualizarDistribuidos) {
                    descripcion += "Se va a actualizar los distribuidos. ";
                }
            }

            if ($scope.publicarServidorNT) {
                descripcion += "Se va a publicar el servidor. ";
                tags.push("SERVIDOR");
                descripcion +=
                    "Los servicios a publicar son: " +
                    $scope.serviciosAActualizarNT +
                    ". ";
            }

            if ($scope.actualizarDBNT) {
                descripcion += "Se va a actualizar la base de datos. ";
                tags.push("BASE DE DATOS");
            }
            var filePub = angular.element("#adjunto-no-finalizado")[0];

            var formData = new FormData();
            formData.append("idTarea", $scope.idTareaTerminar);
            formData.append("proximaActividad", $scope.actividadNoTerminacion);
            formData.append("causaNT", $scope.causaNoTerminacion);
            formData.append("comentario", $scope.comentarioNoTerminacion);
            formData.append("publicar", $scope.requierePublicacionNT);
            formData.append("rama", $scope.ramaNT);
            formData.append("descripcion", descripcion);
            formData.append("tagsJson", JSON.stringify(tags));
            formData.append("adjuntoPublicacion", filePub.files[0]);

            var finalizar = $http.post(
                "user/guardar-comentario-no-terminacion",
                formData,
                {
                    headers: { "Content-Type": undefined },
                }
            );
            finalizar.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.wind3Opciones.hide();
                    angular.element("#modal-comentario-no-terminacion").modal("hide");
                    //$scope.ejecutarFinalizarTarea();
                    angular.element("#modal-final-tarea").modal("hide");
                    angular.element.connection.hub.start().done(function () {
                        taskProxie.server.actualizarTareas();
                    });
                    $scope.tiempoUtilizado = "";

                    if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
                    else
                        actualizarDatosTarea(
                            $scope.fechaLunes,
                            angular.toJson(filtroTareas)
                        );
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.enviarComentarioNoTerminacionTarea = function () {
            waitingDialog.show("Enviando comentario de no terminación...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var finalizar = $http.post(
                "user/guardar-comentario-no-terminacion-tarea",
                {
                    idTarea: $scope.idTareaTerminar,
                    proximaActividad: $scope.actividadNoTerminacionTarea,
                    causaNT: $scope.causaNoTerminacionTarea,
                    comentario: $scope.comentarioNoTerminacionTarea,
                }
            );
            finalizar.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    angular
                        .element("#modal-comentario-no-terminacion-tarea-contrato")
                        .modal("hide");
                    //$scope.ejecutarFinalizarTarea();
                    angular.element("#modal-final-tarea").modal("hide");
                    angular.element.connection.hub.start().done(function () {
                        taskProxie.server.actualizarTareas();
                    });
                    $scope.tiempoUtilizado = "";
                    if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
                    else
                        actualizarDatosTarea(
                            $scope.fechaLunes,
                            angular.toJson(filtroTareas)
                        );
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        //Adicionar boton de adjunto
        $scope.adicionarFileAnterior = function () {
            var jQ = angular.element;
            jQ(jQ("#htmlFile").html()).insertBefore("#barra-botones-file");
            var divAdjuntos = jQ("#addInputFileAnterior").parent().parent();
            var fileAdj = divAdjuntos.find(".file-adj-contrato");
            if (fileAdj.length > 1) {
                jQ("#deleteInputFileAnterior").prop("disabled", false);
            }
        };

        $scope.deleteFileAnterior = function () {
            var jQ = angular.element;
            var divAdjuntos = jQ("#deleteInputFileAnterior").parent().parent();
            var fileAdj = divAdjuntos.find(".file-adj-contrato");
            if (fileAdj.length == 2) {
                jQ("#deleteInputFileAnterior").prop("disabled", true);
            }
            if (fileAdj.length > 1) {
                jQ(jQ("#deleteInputFileAnterior").parent().prev()).remove();
            }
        };

        $scope.enviarInformacionFinTicket = function () {
            $scope.finalizarTarea();
            $scope.ejecutarFinalizarTarea(function (idTarea) {
                waitingDialog.show("Enviando email...", {
                    dialogSize: "sm",
                    progressType: "success",
                });

                var descripcion = "";
                var tags = [];

                if ($scope.publicarCliente) {
                    descripcion += "Se va a publicar el cliente. ";
                    tags.push("CLIENTE");
                    if ($scope.actualizarDistribuidos) {
                        descripcion += "Se va a actualizar los distribuidos. ";
                    }
                }

                if ($scope.publicarServidor) {
                    descripcion += "Se va a publicar el servidor. ";
                    tags.push("SERVIDOR");
                    descripcion +=
                        "Los servicios a publicar son: " +
                        $scope.serviciosAActualizar +
                        ". ";
                }

                if ($scope.actualizarDB) {
                    descripcion += "Se va a actualizar la base de datos. ";
                    tags.push("BASE DE DATOS");
                }
                var filePub = angular.element("#adjunto-publicacion")[0];

                var formData = new FormData();
                formData.append("idTarea", idTarea);
                formData.append("texto", $scope.textoEmail);
                formData.append("publicar", $scope.requierePublicacion);
                angular.element.each(
                    angular.element("#panel-adjuntos-email-ticket").find('[type="file"]'),
                    function (pos, fileInput) {
                        formData.append("adjuntos", fileInput.files[0]);
                    }
                );

                formData.append("rama", $scope.rama);
                formData.append("descripcion", descripcion);
                formData.append("tagsJson", JSON.stringify(tags));
                formData.append("adjuntoPublicacion", filePub.files[0]);

                var informacionFinTicket = $http.post(
                    "user/enviar-email-fin-ticket",
                    formData,
                    {
                        headers: { "Content-Type": undefined },
                    }
                );
                informacionFinTicket.success(function (data) {
                    waitingDialog.hide();
                    if (data.success === true) {
                        $scope.Errormsj = data.devopsmsj;
                        $scope.requierePublicacion = false;
                        $scope.cambioRequierePublicacion();
                        angular.element("#modal-email-fin-ticket").modal("hide");
                    } else {
                        messageDialog.show("Información", data.msg);
                    }
                });
            });
        };

        $scope.cambioRequierePublicacion = function () {
            var elementos = angular.element(".texto-finalizacion-ticket");
            if ($scope.requierePublicacion === true) {
                angular.element(elementos[0]).show();
                angular.element(elementos[1]).hide();
            } else {
                angular.element(elementos[1]).show();
                angular.element(elementos[0]).hide();
            }
        };
        //FIN DE LAS FUNCIONES DE FINALIZAR LA TAREA

        function cambiarEstadoDeTarea(id, estado) {
            var anular = $http.post("user/actualizar-tarea-usuario", {
                idTarea: id,
                estado: estado,
            });
            anular.success(function (data) {
                angular.element.connection.hub.start().done(function () {
                    taskProxie.server.actualizarTareas();
                });
                waitingDialog.hide();
                if (data.success === true) {
                    if (!aplicarFiltro) actualizarDatosTarea($scope.fechaLunes);
                    else
                        actualizarDatosTarea(
                            $scope.fechaLunes,
                            angular.toJson(filtroTareas)
                        );
                    //messageDialog.show('Información', data.msg);
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        }

        $scope.mySplitEmail = function (string, nb) {
            $scope.array = string.split("@");
            return ($scope.result = $scope.array[nb]);
        };

        //Gestión de comentarios a las actividades
        $scope.verComentarios = function (id) {
            $scope.idActividadTarea = id;
            angular.element("#modal-comentario-actividad").modal("show");
            var verComentarios = $http.post("user/dar-comentarios", {
                idActividad: $scope.idActividadTarea,
            });
            verComentarios.success(function (data) {
                if (data.success === true) {
                    $scope.comentarios = data.comentarios;
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        angular
            .element("#modal-comentario-actividad")
            .on("hidden.bs.modal", function (e) {
                $scope.$apply(function () {
                    $scope.comentarios = [];
                });
            });

        $scope.adicionarComentario = function () {
            waitingDialog.show("Adicionando comentario...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var addComentario = $http.post("user/adicionar-comentario", {
                idActividad: $scope.idActividadTarea,
                descripcion: $scope.descripcionComentario,
                importancia: $scope.importanciaActividad,
            });
            addComentario.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.descripcionComentario = "";
                    $scope.comentarios = data.comentarios;
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.eliminarComentario = function (id) {
            waitingDialog.show("Eliminando comentario...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var removeComentario = $http.post("user/eliminar-comentario", {
                idComentario: id,
            });
            removeComentario.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.comentarios = data.comentarios;
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        //Para la solicitud de las tareas
        $scope.windowSolicitarTarea = function () {
            angular.element("#modal-solicitud-tareas").modal("show");
        };

        $scope.solicitarTarea = function () {
            waitingDialog.show("Solicitando tarea...", {
                dialogSize: "sm",
                progressType: "success",
            });
            var solicitudtarea = $http.post("user/solicitar-tarea", {
                descripcion: $scope.descripcionTarea,
            });
            solicitudtarea.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    $scope.descripcionTarea = "";
                    angular.element("#modal-solicitud-tareas").modal("hide");
                    messageDialog.show("Información", data.msg);
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };

        $scope.publicarDatos = function () {
            var descripcion = "";
            var tags = [];

            if ($scope.publicarClienteTarea) {
                descripcion += "Se va a publicar el cliente. ";
                tags.push("CLIENTE");
                if ($scope.actualizarDistribuidosTarea) {
                    descripcion += "Se va a actualizar los distribuidos. ";
                }
            }

            if ($scope.publicarServidorTarea) {
                descripcion += "Se va a publicar el servidor. ";
                tags.push("SERVIDOR");
                descripcion +=
                    "Los servicios a publicar son: " + $scope.serviciosAActualizarTarea + ". ";
            }

            if ($scope.actualizarDBTarea) {
                descripcion += "Se va a actualizar la base de datos. ";
                tags.push("BASE DE DATOS");
            }
            var filePub = angular.element("#adjunto-publicacion-tarea")[0];

            var formData = new FormData();
            formData.append("idTarea", $scope.idTareaPublicar);
            formData.append("rama", $scope.ramaTarea);
            formData.append("descripcion", descripcion);
            formData.append("tagsJson", JSON.stringify(tags));
            formData.append("adjuntoPublicacion", filePub.files[0]);

            var enviarPublicacion = $http.post(
                "user/enviar-publicacion",
                formData,
                {
                    headers: { "Content-Type": undefined },
                }
            );
            enviarPublicacion.success(function (data) {
                waitingDialog.hide();
                if (data.success === true) {
                    //$scope.Errormsj = data.devopsmsj;
                    //$scope.requierePublicacion = false;
                    //$scope.cambioRequierePublicacion();
                    //angular.element("#modal-email-fin-ticket").modal("hide");
                } else {
                    messageDialog.show("Información", data.msg);
                }
            });
        };
    },
]);
