angular.module("admin").controller("proyectosExcelController", [
  "$scope",
  "$http",
  function ($scope, $http) {
    // Arrays para los datos de la vista y los filtros
    $scope.proyectos = [];
    $scope.filtro = {};

    // Arrays para poblar los menús desplegables
    $scope.clientes = [];
    $scope.versiones = [];
    $scope.visuales = [];
    $scope.repositorios = [];
    $scope.responsables = [];
    $scope.colaboradores = [];
    $scope.versionesBd = []; // <-- AÑADIDO para Versión de BD

    // Objeto que representa el estado limpio del formulario
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
      ubicacion: null, // Campo añadido previamente
      versionBd: null, // <-- AÑADIDO para Versión de BD
    };

    $scope.proyectoFormData = angular.copy(initialFormData);

    /*
        |--------------------------------------------------------------------------
        | FUNCIONES PARA CARGAR DATOS
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
      $scope.versiones = [{ Descripcion: "2.0" }, { Descripcion: "2.5" }, { Descripcion: "Canales" }];
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

    // --- FUNCIÓN AÑADIDA para Versión de BD ---
    $scope.cargarVersionesBd = function () {
      // Asegúrate de que la ruta coincida con la que creaste en RouteConfig.cs
      $http.get("/Admin/GetVersionesBaseDatos").then(function (response) {
        if (response.data.success) {
          $scope.versionesBd = response.data.versionesBd;
        }
      });
    };

    /*
        |--------------------------------------------------------------------------
        | MANEJO DEL FORMULARIO
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

      // --- LÓGICA AÑADIDA para Versión de BD ---
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

      // Aplanado de datos para enviar al backend
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

      // --- LÓGICA AÑADIDA para Versión de BD ---
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

    // Dentro de tu controlador de AngularJS, junto a las otras funciones del $scope

    $scope.eliminarProyecto = function (proyecto, $event) {
      // Detiene la propagación para evitar que se abra la modal de edición
      if ($event) {
        $event.stopPropagation();
      }

      // 1. Pide confirmación al usuario
      var confirmacion = window.confirm(
        "¿Estás seguro de que deseas eliminar el proyecto para '" +
          proyecto.cooperativa +
          "'?"
      );

      // 2. Si el usuario confirma, procede a eliminar
      if (confirmacion) {
        // 3. Llama al nuevo endpoint del backend para eliminar
        $http.post("/Admin/EliminarProyecto", { id: proyecto.id }).then(
          function (response) {
            if (response.data.success) {
              alert("Proyecto eliminado correctamente.");
              // 4. Recarga la lista de proyectos para que el eliminado desaparezca
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
        | INICIALIZACIÓN DEL CONTROLADOR
        |--------------------------------------------------------------------------
        */
    $scope.cargarClientes();
    $scope.cargarVersiones();
    $scope.cargarVisuales();
    $scope.cargarRepositorios();
    $scope.cargarResponsables();
    $scope.cargarColaboradores();
    $scope.cargarProyectos();
    $scope.cargarVersionesBd(); // <-- LLAMADA A LA NUEVA FUNCIÓN
  },
]);
