devApp.controller('pdfController', ['$scope', '$http', function ($scope, $http) {
    (function initController() {
        $scope.scale = 1.0;
        $scope.loadPDF = function (scale) {
            pdfjsLib.getDocument('/Web/resources/pdf/colaboradores.pdf').promise.then(function (pdf) {
                var container = document.getElementById('pdfContainer');
                container.innerHTML = ''; 
                for (var i = 1; i <= pdf.numPages; i++) {
                    pdf.getPage(i).then(function (page) {
                        var canvas = document.createElement('canvas');
                        canvas.style.display = "block";
                        canvas.style.margin = "10px";
                        var context = canvas.getContext('2d');
                        var viewport = page.getViewport({ scale: scale });
                        canvas.height = viewport.height;
                        canvas.width = viewport.width;
                        var renderContext = {
                            canvasContext: context,
                            viewport: viewport
                        };
                        page.render(renderContext);
                        container.appendChild(canvas);
                    });
                }
            });
        };
        $scope.zoomIn = function () {
            $scope.scale = (parseFloat($scope.scale) + 0.1).toFixed(2);
            $scope.loadPDF($scope.scale);
        };
        $scope.zoomOut = function () {
            if ($scope.scale > 0.1) {
                $scope.scale = (parseFloat($scope.scale) - 0.1).toFixed(2);
                $scope.loadPDF($scope.scale);
            }
        };
        $scope.resetScale = function () {
            $scope.scale = 1.0;
            $scope.loadPDF($scope.scale);
        };
        $scope.loadPDF($scope.scale);
    })();
}]);
