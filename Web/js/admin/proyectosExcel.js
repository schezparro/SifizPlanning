// 1. AÑADIMOS LA DEPENDENCIA PARA QUE FUNCIONE LA PAGINACIÓN
adminApp.controller("proyectosExcelController", [
  "$scope",
  "$http",
  function ($scope, $http) {
    // Arrays para los datos de la vista y los filtros
    $scope.proyectos = [];
    $scope.filtro = {};

    // 2. AÑADIMOS EL MODELO PARA EL NUEVO BUSCADOR GENERAL
    $scope.buscadorGeneral = "";

    // Arrays para poblar los menús desplegables (sin cambios)
    $scope.clientes = [];
    $scope.versiones = [];
    $scope.visuales = [];
    $scope.repositorios = [];
    $scope.responsables = [];
    $scope.colaboradores = [];
    $scope.versionesBd = [];

    // Objeto que representa el estado limpio del formulario (sin cambios)
    var initialFormData = {
      id: null,
      cooperativa: null,
      version: null,
      visual: null,
      repositorio: null,
      admCod: null,
      integracion: null,
      publicacion: null,
      liderProyecto: null,
      solucion: "",
      pathFuentesFinDia: "",
      ubicacion: null,
      versionBd: null,
    };

    $scope.proyectoFormData = angular.copy(initialFormData);

    /*
      |--------------------------------------------------------------------------
      | FUNCIONES PARA CARGAR DATOS (Sin cambios)
      |--------------------------------------------------------------------------
      */
    $scope.cargarClientes = function () {
      $http.get("/Admin/GetClientes").then(function (r) {
        if (r.data.success) {
          $scope.clientes = r.data.clientes;
        }
      });
    };
    $scope.cargarVersiones = function () {
      $scope.versiones = [
        { Descripcion: "2.0" },
        { Descripcion: "2.5" },
        { Descripcion: "Canales" },
      ];
    };
    $scope.cargarVisuales = function () {
      $http.get("/Admin/GetVersionesDesarrollo").then(function (r) {
        if (r.data.success) {
          $scope.visuales = r.data.versiones;
        }
      });
    };
    $scope.cargarRepositorios = function () {
      $http.get("/Admin/GetRepositorios").then(function (r) {
        if (r.data.success) {
          $scope.repositorios = r.data.repositorios;
        }
      });
    };
    $scope.cargarResponsables = function () {
      $http.get("/Admin/GetResponsablesProyectos").then(function (r) {
        if (r.data.success) {
          $scope.responsables = r.data.responsables;
        }
      });
    };
    $scope.cargarColaboradores = function () {
      $http.get("/Admin/GetColaboradores").then(function (r) {
        if (r.data.success) {
          $scope.colaboradores = r.data.colaboradores;
        }
      });
    };
    $scope.cargarProyectos = function () {
      $http.post("/Admin/DarProyectosExcel").then(function (r) {
        if (r.data.success) {
          $scope.proyectos = r.data.proyectos;
        }
      });
    };
    $scope.cargarVersionesBd = function () {
      $http.get("/Admin/GetVersionesBaseDatos").then(function (response) {
        if (response.data.success) {
          $scope.versionesBd = response.data.versionesBd;
        }
      });
    };

    /*
      |--------------------------------------------------------------------------
      | MANEJO DEL FORMULARIO (Sin cambios)
      |--------------------------------------------------------------------------
      */
    $scope.limpiarFormulario = function () {
      $scope.proyectoFormData = angular.copy(initialFormData);
      if ($scope.proyectoForm) {
        $scope.proyectoForm.$setPristine();
        $scope.proyectoForm.$setUntouched();
      }
    };

    $scope.abrirModalCrear = function () {
      $scope.limpiarFormulario();
      $("#proyectoModal").modal("show");
    };

    $scope.abrirModalEditar = function (proyecto, $event) {
      if ($event) {
        $event.stopPropagation();
      }
      $scope.proyectoFormData = angular.copy(proyecto);

      $scope.proyectoFormData.cooperativa = $scope.clientes.find(
        (c) => c.Descripcion === proyecto.cooperativa
      );
      $scope.proyectoFormData.version = $scope.versiones.find(
        (v) => v.Descripcion === proyecto.version
      );
      $scope.proyectoFormData.visual = $scope.visuales.find(
        (v) => v.Descripcion === proyecto.visual
      );
      $scope.proyectoFormData.repositorio = $scope.repositorios.find(
        (r) => r.Descripcion === proyecto.repositorio
      );
      $scope.proyectoFormData.admCod = $scope.responsables.find(
        (r) => r.Nombre === proyecto.admCod
      );
      $scope.proyectoFormData.integracion = $scope.responsables.find(
        (r) => r.Nombre === proyecto.integracion
      );
      $scope.proyectoFormData.publicacion = $scope.responsables.find(
        (r) => r.Nombre === proyecto.publicacion
      );
      $scope.proyectoFormData.liderProyecto = $scope.colaboradores.find(
        (c) => c.NombreCompleto === proyecto.liderProyecto
      );
      $scope.proyectoFormData.ubicacion = $scope.responsables.find(
        (r) => r.Nombre === proyecto.ubicacion
      );
      $scope.proyectoFormData.versionBd = $scope.versionesBd.find(
        (v) => v.Descripcion === proyecto.versionBd
      );

      $("#proyectoModal").modal("show");
    };

    $scope.cerrarModal = function () {
      $("#proyectoModal").modal("hide");
      $scope.limpiarFormulario();
    };

    $scope.guardarProyecto = function () {
      if ($scope.proyectoForm.$invalid) {
        alert("Por favor, complete los campos requeridos.");
        return;
      }

      var payload = angular.copy($scope.proyectoFormData);

      // Aplanado de datos para enviar al backend (sin cambios)
      payload.cooperativa = payload.cooperativa
        ? payload.cooperativa.Descripcion
        : null;
      payload.version = payload.version ? payload.version.Descripcion : null;
      payload.visual = payload.visual ? payload.visual.Descripcion : null;
      payload.repositorio = payload.repositorio
        ? payload.repositorio.Descripcion
        : null;
      payload.admCod = payload.admCod ? payload.admCod.Nombre : null;
      payload.integracion = payload.integracion
        ? payload.integracion.Nombre
        : null;
      payload.publicacion = payload.publicacion
        ? payload.publicacion.Nombre
        : null;
      payload.liderProyecto = payload.liderProyecto
        ? payload.liderProyecto.NombreCompleto
        : null;
      payload.ubicacion = payload.ubicacion ? payload.ubicacion.Nombre : null;
      payload.versionBd = payload.versionBd
        ? payload.versionBd.Descripcion
        : null;

      var url = payload.id
        ? "/Admin/ActualizarProyecto"
        : "/Admin/CrearProyecto";

      $http.post(url, payload).then(
        function (response) {
          if (response.data.success) {
            $scope.cerrarModal();
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

    $scope.eliminarProyecto = function (proyecto, $event) {
      if ($event) {
        $event.stopPropagation();
      }

      var confirmacion = window.confirm(
        "¿Estás seguro de que deseas eliminar el proyecto para '" +
          proyecto.cooperativa +
          "'?"
      );

      if (confirmacion) {
        $http.post("/Admin/EliminarProyecto", { id: proyecto.id }).then(
          function (response) {
            if (response.data.success) {
              alert("Proyecto eliminado correctamente.");
              $scope.cargarProyectos();
            } else {
              alert("Error al eliminar el proyecto: " + response.data.msg);
            }
          },
          function (error) {
            alert("Error de comunicación al intentar eliminar el proyecto.");
          }
        );
      }
    };

    /*
      |--------------------------------------------------------------------------
      | INICIALIZACIÓN DEL CONTROLADOR (Sin cambios)
      |--------------------------------------------------------------------------
      */
    $scope.cargarClientes();
    $scope.cargarVersiones();
    $scope.cargarVisuales();
    $scope.cargarRepositorios();
    $scope.cargarResponsables();
    $scope.cargarColaboradores();
    $scope.cargarProyectos();
    $scope.cargarVersionesBd();
  },
]);
