adminApp.controller('correosNoEnviadosController', ['$scope', '$http', function ($scope, $http) {
    //Variables Iniciales
    $scope.idcorreo = undefined;
    $scope.correosNoEnviados = [];
    $scope.totalCorreos = 0;

    //Funciones de carga de datos
    $scope.cargarCorreosNoEnviados = function (fechaInicio, fechaFin) {
        $scope.loading.show();
        //Cargando los correos no enviados
        var ajaxCorreosNoEnviados = $http.post("admin/correosNoEnviados/", {
            fIni: fechaInicio,
            fFin: fechaFin
        });
        ajaxCorreosNoEnviados.success(function (data) {
            $scope.loading.hide();
            
            if (data.success === true) {
                $scope.correosNoEnviados = data.correosNoEnviados;
                $scope.totalCorreos = $scope.correosNoEnviados.length;

                setTimeout(function () {
                    //Actualizando el tamaño de la tabla en height    
                    var tamanioY = angular.element("#panel_correosNoEnviados").height() - 35 - angular.element("#thead-correosNoEnviados").height();
                    angular.element("#tbody-correosNoEnviados").css({
                        height: tamanioY
                    });
                }, 100);
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos.");
            }
        });
    };
    $scope.cargarCorreosNoEnviados();

    //Función que reenvia el correo seleccionado
    $scope.reenviarEmail = function (idcorreo) {
        $scope.loading.show();
        //Obtiene el correo seleccionado
        var correoSeleccionado = $scope.correosNoEnviados[0];
        for (var i = 0; i < $scope.correosNoEnviados.length; i++) {
            if (parseInt($scope.correosNoEnviados[i].idcorreo) === parseInt(idcorreo)) {
                correoSeleccionado = $scope.correosNoEnviados[i];
                break;
            }
        }
        //Reenviando correo
        var ajaxCorreosNoEnviados = $http.post("admin/reenviarCorreo/", {
            emailFuente: correoSeleccionado.emailFuente,
            emailsDestinos: correoSeleccionado.emailsDestinos,
            emailBody: correoSeleccionado.emailBody,
            idCorreo: idcorreo,
            asunto: correoSeleccionado.asunto,
            password: correoSeleccionado.password,
            adjuntarimagen: correoSeleccionado.adjuntarimagen,
            imagenes: correoSeleccionado.imagenes,
            adjuntos: correoSeleccionado.adjuntos
        });
        ajaxCorreosNoEnviados.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {

                //if (data.sended === true)
                //    messageDialog.show('Información', "El correo se envió correctamente");
                //else
                //    messageDialog.show('Información', "El correo no pudo ser enviado");

                $scope.cargarCorreosNoEnviados();
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos." + "\n" + data.msg);
            }
        });
    };

    //Función que desactiva el correo seleccionado
    $scope.desactivarEmail = function (idcorreo) {
        $scope.loading.show();

        //Desactivando correo
        var ajaxCorreosNoEnviados = $http.post("admin/desactivarCorreo/", {
            idCorreo: idcorreo
        });
        ajaxCorreosNoEnviados.success(function (data) {
            $scope.loading.hide();
            if (data.success === true) {

                //if (data.sended === true)
                //    messageDialog.show('Información', "El correo se envió correctamente");
                //else
                //    messageDialog.show('Información', "El correo no pudo ser enviado");

                $scope.cargarCorreosNoEnviados();
            }
            else {
                messageDialog.show('Información', "Error en el acceso a los datos." + "\n" + data.msg);
            }
        });
    };

    angular.element('#dtpk-fecha-inicio').datepicker({
        startDate: new Date(),
        format: 'dd/mm/yyyy',
        locale: 'es'
    });

    angular.element('#dtpk-fecha-fin').datepicker({
        startDate: new Date(),
        format: 'dd/mm/yyyy',
        locale: 'es'
    });
        
   
}]);