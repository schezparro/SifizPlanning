devApp.controller('propuestaVacacionesController', ['$scope', '$http', function ($scope, $http) {

    // Días feriados
    $scope.ObtenerFeriados = function (anno, mes) {
        var ajaxObtenerFeriados = $http.post("user/dar-feriados-mes",
            {
                anno: anno,
                mes: mes
            });

        ajaxObtenerFeriados.success(function (data) {
            if (data.success === true) {
                $scope.diasFeriados = data.diasFeriados;
                $scope.getDaysOfMonth(anno, mes);
            }
        });
    };

    // Variable para manejar la selección
    $scope.isSelecting = false;

    // Función para obtener los días del mes
    $scope.getDaysOfMonth = function (anno, mes) {
        let days = [];
        let firstDay = new Date(anno, mes - 1, 1);
        let lastDay = new Date(anno, mes, 0);

        // Generar los días del mes
        for (let i = 1; i <= lastDay.getDate(); i++) {
            let currentDay = new Date(anno, mes - 1, i);
            let dayOfWeek = currentDay.getDay();
            days.push({
                dayNumber: i,
                day: ['D', 'L', 'M', 'M', 'J', 'V', 'S'][dayOfWeek],
                isHoliday: $scope.diasFeriados.includes(i),
                isWeekend: dayOfWeek === 0 || dayOfWeek === 6
            });
        }

        $scope.daysOfMonth = days; // Guardar en el scope para la tabla
    };


    $scope.verTabla = false;

    $scope.ObtenerPropuesta = function (anno, mes) {
        var ajaxObtenerPropuestaVac = $http.post("user/dar-datos-dias-vacaciones",
            {
                anno: anno,
                mes: mes
            });

        ajaxObtenerPropuestaVac.success(function (data) {
            if (data.success === true) {
                $scope.verTabla = true;
                $scope.colaboradores = data.datosColaborador;
            }
            else {
                waitingDialog.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };

    // Función que se ejecuta al cambiar el mes
    $scope.onChangeMes = function (mes) {
        let anno = $scope.annos;
        $scope.ObtenerFeriados(anno, mes);
        $scope.ObtenerPropuesta(anno, mes);

    };

    // Variables para rastrear la operación
    $scope.isSelecting = false;
    $scope.isDeselecting = false;
    $scope.activeColaboradorId = null;

    // Función para iniciar la selección
    $scope.startSelection = function (colaborador, day) {
        // Validación de reglas de negocio
        if (day.isHoliday || day.isWeekend) return;

        $scope.activeColaboradorId = colaborador.id;
        if (!colaborador.tempSelectedDays) colaborador.tempSelectedDays = [];
        if (!colaborador.tempDeselectedDays) colaborador.tempDeselectedDays = [];

        colaborador.tempSelectedDays = [];
        colaborador.tempDeselectedDays = [];

        if (colaborador.diasDeVacaciones && colaborador.diasDeVacaciones.includes(day.dayNumber))
            day.isSelected = true;
        else
            day.isSelected = false;

        // Determina si es una operación de selección o deselección
        $scope.isSelecting = !day.isSelected && colaborador.diasDisponibles > 0;
        $scope.isDeselecting = day.isSelected;

        // Marca el primer día
        if ($scope.isSelecting && (colaborador.diasDisponibles - colaborador.tempSelectedDays.length) > 0) {
            $scope.markDay(colaborador, day);
        } else if ($scope.isDeselecting) {
            $scope.unmarkDay(colaborador, day);
        }
    };

    // Función para arrastrar la selección
    $scope.dragSelection = function (colaborador, day) {
        if ($scope.activeColaboradorId !== colaborador.id) return;
        if (day.isHoliday || day.isWeekend) return;

        let isCurrentlySelected = (colaborador.diasDeVacaciones && colaborador.diasDeVacaciones.includes(day.dayNumber) || colaborador.tempSelectedDays.includes(day.dayNumber)) && !colaborador.tempDeselectedDays.includes(day.dayNumber);

        if ($scope.isSelecting && !isCurrentlySelected && (colaborador.diasDisponibles - colaborador.tempSelectedDays.length) > 0) {
            $scope.markDay(colaborador, day);
        } else if ($scope.isDeselecting && isCurrentlySelected) {
            $scope.unmarkDay(colaborador, day);
        }
    };

    // Función para finalizar la selección
    $scope.endSelection = function (colaborador) {
        if ($scope.activeColaboradorId !== colaborador.id) return;

        let daysToProcessSelect = colaborador.tempSelectedDays.length > 0 ? [...colaborador.tempSelectedDays] : [];
        let daysToProcessDeselect = colaborador.tempDeselectedDays.length > 0 ? [...colaborador.tempDeselectedDays] : [];
        
        // Procesa las selecciones en bloque
        if (daysToProcessSelect.length > 0) {
            $http.post("user/actualizar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: daysToProcessSelect,
                mes: $scope.mes,
                anno: $scope.annos
            }).success(function (data) {
                if (!data.success) {
                    alert("Error al guardar las vacaciones: " + data.msg);
                } else {
                    if (!colaborador.diasDeVacaciones) colaborador.diasDeVacaciones = [];
                    colaborador.diasDeVacaciones.push(...daysToProcessSelect);
                    colaborador.diasMarcadosCount += daysToProcessSelect.length;
                    colaborador.diasDisponibles -= daysToProcessSelect.length;
                }
                colaborador.tempSelectedDays = [];
            }).error(function () {
                alert("Error al conectar con el servidor.");
                colaborador.tempSelectedDays = [];
            });
        }

        // Procesa las deselecciones en bloque
        if (daysToProcessDeselect.length > 0) {
            $http.post("user/eliminar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: daysToProcessDeselect,
                mes: $scope.mes,
                anno: $scope.annos
            }).success(function (data) {
                if (!data.success) {
                    alert("Error al eliminar las vacaciones: " + data.msg);
                } else {
                    daysToProcessDeselect.forEach(d => {
                        const index = colaborador.diasDeVacaciones.indexOf(d);
                        if (index > -1) colaborador.diasDeVacaciones.splice(index, 1);
                    });
                    colaborador.diasMarcadosCount -= daysToProcessDeselect.length;
                    colaborador.diasDisponibles += daysToProcessDeselect.length;
                }
                colaborador.tempDeselectedDays = [];
            }).error(function () {
                alert("Error al conectar con el servidor.");
                colaborador.tempDeselectedDays = [];
            });
        }

        // Reinicia las variables de estado
        $scope.isSelecting = false;
        $scope.isDeselecting = false;
        $scope.selectedDays = [];
        $scope.deselectedDays = [];
        $scope.activeColaboradorId = null;
    };

    // Marca un día como seleccionado visualmente
    $scope.markDay = function (colaborador, day) {
        if (!colaborador.tempSelectedDays.includes(day.dayNumber)) {
            colaborador.tempSelectedDays.push(day.dayNumber);
        }
    };

    // Marca un día como deseleccionado visualmente
    $scope.unmarkDay = function (colaborador, day) {
        if (!colaborador.tempDeselectedDays.includes(day.dayNumber)) {
            colaborador.tempDeselectedDays.push(day.dayNumber);
        }
    };

    $scope.toggleSelection = function (colaborador, day) {
        let anno = $scope.annos;
        let mes = $scope.mes;

        // Asegurar que el colaborador tenga una lista de días seleccionados si no existe aún
        if (!colaborador.diasDeVacaciones) {
            colaborador.diasDeVacaciones = [];
        }

        // Verificar si el día ya está seleccionado
        const index = colaborador.diasDeVacaciones.indexOf(day.dayNumber);
        console.log(index);
        if (index > -1) {
            // Deseleccionar día
            var ajaxEliminarPV = $http.post("user/eliminar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: [day.dayNumber], // Enviar como lista, para mantener compatibilidad
                mes: mes,
                anno: anno
            });

            ajaxEliminarPV.success(function (data) {
                if (data.success === true) {
                    day.isSelected = false;
                    colaborador.diasDeVacaciones.splice(index, 1);
                    colaborador.diasMarcadosCount--;
                    colaborador.diasDisponibles++;
                    $scope.onChangeMes(mes);
                } else {
                    alert("Error al eliminar la propuesta de vacaciones: " + data.msg);
                }
            });

            ajaxEliminarPV.error(function () {
                alert("Error en la conexión al servidor. Intenta nuevamente.");
            });
        } else {
            // Seleccionar día
            if (colaborador.diasDisponibles > 0) {
                var ajaxActualizarPV = $http.post("user/actualizar-propuesta-vacaciones", {
                    idColaborador: colaborador.id,
                    days: [day.dayNumber], // Enviar como lista, para mantener compatibilidad
                    mes: mes,
                    anno: anno
                });

                ajaxActualizarPV.success(function (data) {
                    if (data.success === true) {
                        day.isSelected = true;
                        colaborador.diasDeVacaciones.push(day.dayNumber);
                        colaborador.diasMarcadosCount++;
                        colaborador.diasDisponibles--;
                        $scope.onChangeMes(mes);
                    } else {
                        alert("Error al guardar la propuesta de vacaciones: " + data.msg);
                    }
                });

                ajaxActualizarPV.error(function () {
                    alert("Error en la conexión al servidor. Intenta nuevamente.");
                });
            } else {
                alert("No puedes seleccionar más días de los disponibles.");
            }
        }
    };

    // Inicializar con el mes y año por defecto
    $scope.annos = '2025';
    $scope.mes = '0'; // Enero
    //$scope.getDaysOfMonth($scope.annos, $scope.mes);


    //$scope.toggleSelection = function (colaborador, day) {
    //    let anno = $scope.annos;
    //    let mes = $scope.mes;

    //    // Asegurar que el colaborador tenga una lista de días seleccionados si no existe aún
    //    if (!colaborador.diasDeVacaciones) {
    //        colaborador.diasDeVacaciones = [];
    //    }

    //    // Verificar si el día ya está seleccionado
    //    const index = colaborador.diasDeVacaciones.indexOf(day.dayNumber);
    //    if (index > -1) {
    //    console.log("deseleccionar dia");
    //        var ajaxEliminarPV = $http.post("user/eliminar-propuesta-vacaciones", {
    //            idColaborador: colaborador.id,
    //            dayNumber: day.dayNumber,
    //            mes: $scope.mes,
    //            anno: $scope.annos
    //        });

    //        ajaxEliminarPV.success(function (data) {
    //            if (data.success === true) {
    //                $scope.onChangeMes(mes);
    //            } else {
    //                alert("Error al eliminar la propuesta de vacaciones: " + data.msg);
    //            }
    //        });

    //        ajaxEliminarPV.error(function () {
    //            alert("Error en la conexión al servidor. Intenta nuevamente.");
    //        });
    //    } else {
    //        // Si no está seleccionado, validar límite de días disponibles y guardar el nuevo registro
    //        if (colaborador.diasDisponibles > 0) {
    //            var ajaxActualizarPV = $http.post("user/actualizar-propuesta-vacaciones", {
    //                idColaborador: colaborador.id,
    //                dayNumber: day.dayNumber,
    //                mes: $scope.mes,
    //                anno: $scope.annos,
    //                cantidadDias: 1
    //            });

    //            ajaxActualizarPV.success(function (data) {
    //                if (data.success === true) {
    //                    $scope.onChangeMes(mes);
    //                } else {
    //                    alert("Error al guardar la propuesta de vacaciones: " + data.msg);
    //                }
    //            });

    //            ajaxActualizarPV.error(function () {
    //                alert("Error en la conexión al servidor. Intenta nuevamente.");
    //            });
    //        } else {
    //            alert("No puedes seleccionar más días de los disponibles.");
    //        }
    //    }
    //};


    //$scope.initColaboradores = function () {
    //    $scope.colaboradores = [
    //        {
    //            nombre: "Juan Pérez",
    //            diasDisponibles: 10,
    //            diasMarcadosCount: 0,
    //            diasVacaciones: [5, 12, 15], // Días asignados previamente
    //        },
    //        {
    //            nombre: "Ana López",
    //            diasDisponibles: 8,
    //            diasMarcadosCount: 0,
    //            diasVacaciones: [3, 18], // Días asignados previamente
    //        },
    //    ];
    //};

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