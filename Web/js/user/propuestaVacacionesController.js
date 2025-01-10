devApp.controller('propuestaVacacionesController', ['$scope', '$http', function ($scope, $http) {

    // Inicializar con el mes y año por defecto
    $scope.annos = '2025';
    $scope.mes = '0'; // Enero

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
        console.log("firstDay ", firstDay);
        console.log("lastDay ", lastDay);

        // Generar los días del mes
        for (let i = 1; i <= lastDay.getDate(); i++) {
            days.push({
                dayNumber: i,
                day: ['D', 'L', 'M', 'M', 'J', 'V', 'S'][new Date(anno, mes - 1, i).getDay()],
                isHoliday: $scope.diasFeriados.includes(i),
            });
        }

        $scope.daysOfMonth = days; // Guardar en el scope para la tabla
    };


    $scope.verTabla = false;

    $scope.ObtenerPropuesta = function (anno, mes) {
        var ajaxObtenerPropuestaVac = $http.post("user/dar-datos-dias-vacaciones",
            {
                usuario: true,
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
    $scope.selectedDays = []; // Almacena los días seleccionados temporalmente
    $scope.deselectedDays = []; // Almacena los días deseleccionados temporalmente

    // Función para iniciar la selección
    $scope.startSelection = function (colaborador, day) {
        console.log("Inicio de selección/deselección");

        // Si el día es feriado, no se hace nada
        if (day.isHoliday) return;

        if (colaborador.diasDeVacaciones.includes(day.dayNumber))
            day.isSelected = true;
        else
            day.isSelected = false;

        // Determina si es selección o deselección
        $scope.isSelecting = !day.isSelected && colaborador.diasDisponibles > 0;
        $scope.isDeselecting = day.isSelected;

        // Limpia listas temporales
        $scope.selectedDays = [];
        $scope.deselectedDays = [];

        // Marca o desmarca el día inicial según corresponda
        if ($scope.isSelecting) {
            $scope.markDay(colaborador, day);
        } else if ($scope.isDeselecting) {
            $scope.unmarkDay(colaborador, day);
        }
    };

    //// Función para arrastrar la selección
    $scope.dragSelection = function (colaborador, day) {
        if (day.isHoliday) return;

        if ($scope.isSelecting && !day.isSelected && colaborador.diasDisponibles > 0) {
            $scope.markDay(colaborador, day);
        } else if ($scope.isDeselecting && day.isSelected) {
            $scope.unmarkDay(colaborador, day);
        }
    };

    //// Función para finalizar la selección
    $scope.endSelection = function (colaborador) {
        console.log("Finalizando selección/deselección");

        // Procesa días seleccionados
        if ($scope.selectedDays.length > 0) {
            $http.post("user/actualizar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: $scope.selectedDays,
                mes: $scope.mes,
                anno: $scope.annos
            }).then(function (response) {
                if (!response.data.success) {
                    alert("Error al guardar las vacaciones: " + response.data.msg);
                }
                $scope.onChangeMes($scope.mes);
            }).catch(function () {
                alert("Error al conectar con el servidor.");
            });
        }

        // Procesa días deseleccionados
        if ($scope.deselectedDays.length > 0) {
            $http.post("user/eliminar-propuesta-vacaciones", {
                idColaborador: colaborador.id,
                days: $scope.deselectedDays,
                mes: $scope.mes,
                anno: $scope.annos
            }).then(function (response) {
                if (!response.data.success) {
                    alert("Error al eliminar las vacaciones: " + response.data.msg);
                }
                $scope.onChangeMes($scope.mes);
            }).catch(function () {
                alert("Error al conectar con el servidor.");
            });
        }

        // Restablece variables de estado
        $scope.isSelecting = false;
        $scope.isDeselecting = false;
        $scope.selectedDays = [];
        $scope.deselectedDays = [];
    };

    $scope.markDay = function (colaborador, day) {
        if (!day.isSelected && colaborador.diasDisponibles > 0) {
            day.isSelected = true;
            colaborador.diasDeVacaciones.push(day.dayNumber);
            colaborador.diasMarcadosCount++;
            colaborador.diasDisponibles--;
            $scope.selectedDays.push(day.dayNumber);
        }
    };

    $scope.unmarkDay = function (colaborador, day) {
        if (day.isSelected) {
            day.isSelected = false;
            const index = colaborador.diasDeVacaciones.indexOf(day.dayNumber);
            if (index > -1) {
                colaborador.diasDeVacaciones.splice(index, 1);
                colaborador.diasMarcadosCount--;
                colaborador.diasDisponibles++;
                $scope.deselectedDays.push(day.dayNumber);
            }
        }
    };

    //// Inicializar variables
    //$scope.meses = [
    //    { value: 1, name: "Enero" },
    //    { value: 2, name: "Febrero" },
    //    { value: 3, name: "Marzo" },
    //    { value: 4, name: "Abril" },
    //    { value: 5, name: "Mayo" },
    //    { value: 6, name: "Junio" },
    //    { value: 7, name: "Julio" },
    //    { value: 8, name: "Agosto" },
    //    { value: 9, name: "Septiembre" },
    //    { value: 10, name: "Octubre" },
    //    { value: 11, name: "Noviembre" },
    //    { value: 12, name: "Diciembre" }
    //];

    //$scope.propuestasPorMes = [];
    //$scope.verTablas = false;

    //// Obtener propuestas y feriados por año
    //$scope.obtenerPropuestasPorAnno = function (anno) {
    //    console.log("Año seleccionado:", anno);

    //    $http.post("user/dar-datos-vacaciones-anno", { anno: anno })
    //        .success(function (data) {
    //            if (data.success === true) {
    //                console.log("Datos recibidos:", data);

    //                if (data.propuestas.length > 0) {

    //                    // Procesar propuestas para calcular los días del mes en el controlador
    //                    $scope.propuestasPorMes = data.propuestas.map(propuesta => {
    //                        const daysOfMonth = [];
    //                        const firstDay = new Date(propuesta.Anno, propuesta.Mes - 1, 1);
    //                        const lastDay = new Date(propuesta.Anno, propuesta.Mes, 0);

    //                        for (let i = 1; i <= lastDay.getDate(); i++) {
    //                            daysOfMonth.push({
    //                                dayNumber: i,
    //                                day: ['D', 'L', 'M', 'M', 'J', 'V', 'S'][new Date(propuesta.Anno, propuesta.Mes - 1, i).getDay()],
    //                                isHoliday: propuesta.DiasFeriados.includes(i),
    //                                isVacation: propuesta.DiasDeVacaciones.includes(i),
    //                            });
    //                        }

    //                        return {
    //                            mes: propuesta.Mes,
    //                            anno: propuesta.Anno,
    //                            daysOfMonth: daysOfMonth
    //                        };
    //                    });

    //                    $scope.verTablas = true;
    //                    $scope.verMensaje = false;
    //                } else {
    //                    $scope.verTablas = false;
    //                    $scope.verMensaje = true;
    //                }

    //            } else {
    //                console.error("Error al obtener las propuestas:", data.msg);
    //                messageDialog.show('Información', data.msg);
    //            }
    //        })
    //        .error(function (error) {
    //            console.error("Error en la petición:", error);
    //            messageDialog.show('Error', "Hubo un problema al obtener las propuestas.");
    //        });
    //};




    // Días feriados
    //$scope.ObtenerFeriados = function (anno, mes) {
    //    var ajaxObtenerFeriados = $http.post("user/dar-feriados-anno",
    //        {
    //            anno: anno
    //        });

    //    ajaxObtenerFeriados.success(function (data) {
    //        if (data.success === true) {
    //            $scope.diasFeriados = data.diasFeriados;
    //            $scope.getDaysOfMonth(anno);
    //        }
    //    });
    //};

    //// Función para generar los días del mes
    //$scope.generarDiasDelMes = function (anno, mes) {
    //    const days = [];
    //    const firstDay = new Date(anno, mes - 1, 1);
    //    const lastDay = new Date(anno, mes, 0);

    //    // Generar los días del mes
    //    for (let i = 1; i <= lastDay.getDate(); i++) {
    //        const currentDay = new Date(anno, mes - 1, i);
    //        days.push({
    //            dayNumber: i,
    //            day: ['D', 'L', 'M', 'M', 'J', 'V', 'S'][currentDay.getDay()],
    //            isHoliday: $scope.diasFeriados.includes(i), // Verificar si es feriado
    //        });
    //    }

    //    $scope.daysOfMonth = days;
    //};

    //$scope.verTabla = false;

    //$scope.ObtenerPropuesta = function (anno, mes) {
    //    var ajaxObtenerPropuestaVac = $http.post("user/dar-datos-dias-vacaciones",
    //        {
    //            usuario: true,
    //            anno: anno,
    //            mes: 1
    //        });

    //    ajaxObtenerPropuestaVac.success(function (data) {
    //        if (data.success === true) {
    //            $scope.verTabla = true;
    //            $scope.colaboradores = data.datosColaborador;
    //        }
    //        else {
    //            waitingDialog.hide();
    //            messageDialog.show('Información', data.msg);
    //        }
    //    });
    //};

    //// Función que se ejecuta al cambiar el mes
    //$scope.onChangeAnnos = function (anno) {
    //    let anno = $scope.annos;
    //    $scope.ObtenerFeriados(anno, mes);
    //    $scope.ObtenerPropuesta(anno, mes);

    //};

    //// Variables para rastrear la operación
    //$scope.isSelecting = false;
    //$scope.isDeselecting = false;
    //$scope.selectedDays = []; // Almacena los días seleccionados temporalmente
    //$scope.deselectedDays = []; // Almacena los días deseleccionados temporalmente

    //// Función para iniciar la selección
    //$scope.startSelection = function (colaborador, day) {
    //    // Determina si es una operación de selección o deselección
    //    $scope.isSelecting = !day.isSelected;
    //    $scope.isDeselecting = day.isSelected;
    //    $scope.selectedDays = [];
    //    $scope.deselectedDays = [];

    //    // Marca el primer día
    //    if ($scope.isSelecting && colaborador.diasDisponibles > 0) {
    //        $scope.markDay(colaborador, day);
    //    } else if ($scope.isDeselecting) {
    //        $scope.unmarkDay(colaborador, day);
    //    }
    //};

    //// Función para arrastrar la selección
    //$scope.dragSelection = function (colaborador, day) {
    //    if ($scope.isSelecting && !day.isSelected && colaborador.diasDisponibles > 0) {
    //        $scope.markDay(colaborador, day);
    //    } else if ($scope.isDeselecting && day.isSelected) {
    //        $scope.unmarkDay(colaborador, day);
    //    }
    //};

    //// Función para finalizar la selección
    //$scope.endSelection = function (colaborador) {
    //    console.log(colaborador);
    //    // Procesa las selecciones en bloque
    //    if ($scope.selectedDays.length > 0) {
    //        $http.post("user/actualizar-propuesta-vacaciones", {
    //            idColaborador: colaborador.id,
    //            days: $scope.selectedDays,
    //            mes: $scope.mes,
    //            anno: $scope.annos
    //        }).success(function (data) {
    //            if (!data.success) {
    //                alert("Error al guardar las vacaciones: " + data.msg);
    //            }
    //            $scope.onChangeMes($scope.mes);
    //        }).error(function () {
    //            alert("Error al conectar con el servidor.");
    //        });
    //    }

    //    // Procesa las deselecciones en bloque
    //    if ($scope.deselectedDays.length > 0) {
    //        $http.post("user/eliminar-propuesta-vacaciones", {
    //            idColaborador: colaborador.id,
    //            days: $scope.deselectedDays,
    //            mes: $scope.mes,
    //            anno: $scope.annos
    //        }).success(function (data) {
    //            if (!data.success) {
    //                alert("Error al eliminar las vacaciones: " + data.msg);
    //            }
    //            $scope.onChangeMes($scope.mes);
    //        }).error(function () {
    //            alert("Error al conectar con el servidor.");
    //        });
    //    }

    //    // Reinicia las variables de estado
    //    $scope.isSelecting = false;
    //    $scope.isDeselecting = false;
    //    $scope.selectedDays = [];
    //    $scope.deselectedDays = [];
    //};

    //// Marca un día como seleccionado
    //$scope.markDay = function (colaborador, day) {
    //    day.isSelected = true;
    //    colaborador.diasDeVacaciones.push(day.dayNumber);
    //    colaborador.diasMarcadosCount++;
    //    colaborador.diasDisponibles--;
    //    $scope.selectedDays.push(day.dayNumber);
    //};

    //// Marca un día como deseleccionado
    //$scope.unmarkDay = function (colaborador, day) {
    //    day.isSelected = false;
    //    const index = colaborador.diasDeVacaciones.indexOf(day.dayNumber);
    //    if (index > -1) {
    //        colaborador.diasDeVacaciones.splice(index, 1);
    //        colaborador.diasMarcadosCount--;
    //        colaborador.diasDisponibles++;
    //        $scope.deselectedDays.push(day.dayNumber);
    //    }
    //};

    //$scope.toggleSelection = function (colaborador, day) {
    //    let anno = $scope.annos;
    //    let mes = $scope.mes;

    //    // Asegurar que el colaborador tenga una lista de días seleccionados si no existe aún
    //    if (!colaborador.diasDeVacaciones) {
    //        colaborador.diasDeVacaciones = [];
    //    }

    //    // Verificar si el día ya está seleccionado
    //    const index = colaborador.diasDeVacaciones.indexOf(day.dayNumber);
    //    console.log(index);
    //    if (index > -1) {
    //        // Deseleccionar día
    //        var ajaxEliminarPV = $http.post("user/eliminar-propuesta-vacaciones", {
    //            idColaborador: colaborador.id,
    //            days: [day.dayNumber], // Enviar como lista, para mantener compatibilidad
    //            mes: mes,
    //            anno: anno
    //        });

    //        ajaxEliminarPV.success(function (data) {
    //            if (data.success === true) {
    //                day.isSelected = false;
    //                colaborador.diasDeVacaciones.splice(index, 1);
    //                colaborador.diasMarcadosCount--;
    //                colaborador.diasDisponibles++;
    //                $scope.onChangeMes(mes);
    //            } else {
    //                alert("Error al eliminar la propuesta de vacaciones: " + data.msg);
    //            }
    //        });

    //        ajaxEliminarPV.error(function () {
    //            alert("Error en la conexión al servidor. Intenta nuevamente.");
    //        });
    //    } else {
    //        // Seleccionar día
    //        if (colaborador.diasDisponibles > 0) {
    //            var ajaxActualizarPV = $http.post("user/actualizar-propuesta-vacaciones", {
    //                idColaborador: colaborador.id,
    //                days: [day.dayNumber], // Enviar como lista, para mantener compatibilidad
    //                mes: mes,
    //                anno: anno
    //            });

    //            ajaxActualizarPV.success(function (data) {
    //                if (data.success === true) {
    //                    day.isSelected = true;
    //                    colaborador.diasDeVacaciones.push(day.dayNumber);
    //                    colaborador.diasMarcadosCount++;
    //                    colaborador.diasDisponibles--;
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

    // Inicializar con el mes y año por defecto
    //$scope.anno = '0';
    //$scope.mes = '0'; // Enero
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