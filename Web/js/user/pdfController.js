devApp.controller('pdfController', ['$scope', '$http', function ($scope, $http) {
    (function initController() {
        $scope.loadPDF = function () {
            pdfjsLib.getDocument('/wwwroot/pdf/download.pdf').promise.then(function (pdf) {
                var canvas = document.getElementById('pdfCanvas');
                var context = canvas.getContext('2d');
                pdf.getPage(1).then(function (page) {
                    var viewport = page.getViewport({ scale: 1.0 });
                    canvas.width = viewport.width;
                    canvas.height = viewport.height;
                    var renderContext = {
                        canvasContext: context,
                        viewport: viewport
                    };
                    page.render(renderContext);
                });
            });
        };
        $scope.loadPDF();
    })();
}]);
