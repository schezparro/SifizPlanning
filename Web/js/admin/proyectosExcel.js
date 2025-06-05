angular.module("admin").controller("proyectosExcelController", [
  "$scope",
  "$http",
  function ($scope, $http) {
    $scope.proyectos = [];
    $scope.filtro = {};
    $scope.proyectoFormData = {};

    $scope.clientes = [];
    $scope.versiones = [];
    $scope.repositorios = [];
    $scope.responsables = [];
    $scope.colaboradores = [];

    $scope.cargarClientes = function () {
      $http.get("/Admin/GetClientes").then(
        function (response) {
          if (response.data.success) {
            $scope.clientes = response.data.clientes;
          } else {
            alert("Error al cargar clientes: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición de clientes: " + error.statusText);
        }
      );
    };

    $scope.cargarVersiones = function () {
      $http.get("/Admin/GetVersionesDesarrollo").then(
        function (response) {
          if (response.data.success) {
            $scope.versiones = response.data.versiones;
          } else {
            alert("Error al cargar versiones: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición de versiones: " + error.statusText);
        }
      );
    };

    $scope.cargarRepositorios = function () {
      $http.get("/Admin/GetRepositorios").then(
        function (response) {
          if (response.data.success) {
            $scope.repositorios = response.data.repositorios;
          } else {
            alert("Error al cargar repositorios: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición de repositorios: " + error.statusText);
        }
      );
    };

    $scope.cargarResponsables = function () {
      $http.get("/Admin/GetResponsablesProyectos").then(
        function (response) {
          if (response.data.success) {
            $scope.responsables = response.data.responsables;
          } else {
            alert("Error al cargar responsables: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición de responsables: " + error.statusText);
        }
      );
    };

    $scope.cargarColaboradores = function () {
      $http.get("/Admin/GetColaboradores").then(
        function (response) {
          if (response.data.success) {
            $scope.colaboradores = response.data.colaboradores;
          } else {
            alert("Error al cargar colaboradores: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición de colaboradores: " + error.statusText);
        }
      );
    };

    $scope.cargarProyectos = function () {
      $http.post("/Admin/DarProyectosExcel").then(
        function (response) {
          if (response.data.success) {
            $scope.proyectos = response.data.proyectos;
          } else {
            alert("Error al cargar proyectos: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición: " + error.statusText);
        }
      );
    };

    $scope.filtrarDatos = function () {
      // Filtering is handled by AngularJS filter in the view
    };

    $scope.seleccionarProyecto = function (proyecto) {
      $scope.proyectoFormData = angular.copy(proyecto);
    };

    $scope.editarProyecto = function (proyecto, $event) {
      $event.stopPropagation();
      $scope.seleccionarProyecto(proyecto);
    };

    $scope.limpiarFormulario = function () {
      $scope.proyectoFormData = {};
      $scope.proyectoForm.$setPristine();
      $scope.proyectoForm.$setUntouched();
    };

    $scope.guardarProyecto = function () {
      if ($scope.proyectoForm.$invalid) {
        alert("Por favor, complete los campos requeridos.");
        return;
      }

      var url = $scope.proyectoFormData.id
        ? "/Admin/ActualizarProyecto"
        : "/Admin/CrearProyecto";

      $http.post(url, $scope.proyectoFormData).then(
        function (response) {
          if (response.data.success) {
            alert("Proyecto guardado correctamente.");
            $scope.limpiarFormulario();
            $scope.cargarProyectos();
          } else {
            alert("Error al guardar proyecto: " + response.data.msg);
          }
        },
        function (error) {
          alert("Error en la petición: " + error.statusText);
        }
      );
    };

    // Call these functions on controller initialization
    $scope.cargarClientes();
    $scope.cargarVersiones();
    $scope.cargarRepositorios();
    $scope.cargarResponsables();
    $scope.cargarColaboradores();

    // Initial load
    $scope.cargarProyectos();
  },
]);
