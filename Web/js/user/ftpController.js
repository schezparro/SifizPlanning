devApp.controller('ftpController', ['$scope', '$http', function ($scope, $http) {


    $scope.filesAndFolders = [];
    $scope.ftpPath = '/Web/resources/FTP/';
    $scope.actualPath = $scope.ftpPath;

    $scope.cargarFTP = function () {
        var ajaxFTP = $http.post("user/ftp/dar-ftp/", {});

        ajaxFTP.success(function (data) {
            if (data.success) {
                $scope.actualPath = data.rootdir;
                $scope.filesAndFolders.files = data.files;
                $scope.filesAndFolders.dirs = data.dirs;
            }
        });
    };
    $scope.cargarFTP();

    $scope.navegarFTP = function (path) {

        $scope.history.push(path);
        
        var ajaxFTP = $http.post("user/ftp/navegar-ftp/", { path: path });

        ajaxFTP.success(function (data) {
            if (data.success) {
                $scope.actualPath = data.rootdir;
                $scope.filesAndFolders.files = data.files;
                $scope.filesAndFolders.dirs = data.dirs;
            }
        });
    };


    $scope.descargarArchivo = function (file) {
        $http.get("user/ftp/descargar-archivo/", {
            params: {
                rutaArchivo: file.path,
                nombreArchivo: file.fullname
            },
            responseType: 'blob'
        }).then(function (response) {
            var blob = response.data;
            var a = document.createElement('a');
            a.href = window.URL.createObjectURL(blob);
            a.download = file.fullname;
            a.dispatchEvent(new MouseEvent('click'));
        }, function (response) {
            
            console.log(response);
        });
    };

}]);
