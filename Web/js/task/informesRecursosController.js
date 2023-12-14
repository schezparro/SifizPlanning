taskApp.controller('informesRecursosController', ['$scope', '$http', function ($scope, $http) {

    $scope.cargarInformeRecursos = function () {
        var ajaxInformeRecursos = $http.post("task/dar-informes-recursos/", {});

        ajaxInformeRecursos.success(function (data) {
            if (data.success === true) {
                var roles = data.roles;
                var isInOperacionesRol = roles.some(role => role.rol === 'OPERACIONES');
                var isInLideresRol = roles.some(role => role.rol === 'LIDERES');
                var isInAdminRol = roles.some(role => role.rol === 'ADMIN');

                if (isInLideresRol && !(isInOperacionesRol && isInAdminRol)) {
                    angular.element("#buttonAdjuntoModal").prop("disabled", "true");
                }

                $scope.file = data.file;
            } else {
                messageDialog.show('Información', data.msg);
            }
        });
    };
    $scope.cargarInformeRecursos();

    $scope.resetearAdjunto = function () {
        angular.element("#btn-subir-archivo").css("display", "block");

        if (document.getElementById('selectedFile').files[0].name.length > 20) {
            document.getElementById('buttonAdjuntoModal').innerHTML = "..." + document.getElementById('selectedFile').files[0].name.substring(document.getElementById('selectedFile').files[0].name.length - 17, document.getElementById('selectedFile').files[0].name.length);
            document.getElementById('buttonAdjunto').innerHTML = "..." + document.getElementById('selectedFile').files[0].name.substring(document.getElementById('selectedFile').files[0].name.length - 17, document.getElementById('selectedFile').files[0].name.length);
        } else {
            document.getElementById('buttonAdjuntoModal').innerHTML = document.getElementById('selectedFile').files[0].name.substring(0, 20);
            document.getElementById('buttonAdjunto').innerHTML = "..." + document.getElementById('selectedFile').files[0].name.substring(document.getElementById('selectedFile').files[0].name.length - 17, document.getElementById('selectedFile').files[0].name.length);
        }
    }

    $scope.subirArchivo = function () {

        $scope.loading.show();

        var formData = new FormData();
        var file = document.getElementById("selectedFile").files[0];
        formData.append("file", file);

        $.ajax({
            url: "task/subir-informes-recursos/",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            async: false,
            success: function (data) {
                if (data.success == true) {

                    angular.element("#btn-subir-archivo").css("display", "none");
                    angular.element('#selectedFile').val("");

                    messageDialog.show('Información', data.msg);
                    $scope.file = data.file;
                } else {
                    $scope.loading.hide();
                    messageDialog.show('Información', data.msg);
                }
            },
            error: function (data) {
                $scope.loading.hide();
                messageDialog.show('Información', data.msg);
            }
        });
    };

    $scope.descargarArchivo = function () {
        $http.get("task/descargar-informes-recursos/", {
            responseType: 'blob'
        }).then(function (response) {
            var blob = response.data;
            var a = document.createElement('a');
            a.href = window.URL.createObjectURL(blob);
            a.download = $scope.file.fullname;
            a.dispatchEvent(new MouseEvent('click'));
        }, function (response) {

            console.log(response);
        });
    };
}]);