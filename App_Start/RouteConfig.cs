using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SifizPlanning
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //RUTAS DEL ADMIN
            routes.MapRoute(
                name: "DarReportes",
                url: "admin/NombreReportes/{id}",
                defaults: new { controller = "Admin", action = "DarReportes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarModulosReportes",
                url: "admin/ModulosReportes/{id}",
                defaults: new { controller = "Admin", action = "DarModulosReportes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CorreosNoEnviados",
                url: "admin/correosNoEnviados/{id}",
                defaults: new { controller = "Admin", action = "DarCorreosNoEnviados", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ReenviarCorreo",
                url: "admin/reenviarCorreo/{id}",
                defaults: new { controller = "Admin", action = "ReenviarCorreo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DesactivarCorreo",
                url: "admin/desactivarCorreo/{id}",
                defaults: new { controller = "Admin", action = "DesactivarCorreo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevoTrabajador",
                url: "admin/nuevo-trabajador/{id}",
                defaults: new { controller = "Admin", action = "NuevoTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EditUsuario",
                url: "admin/edit-user/{id}",
                defaults: new { controller = "Admin", action = "EditarUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Trabajadores",
                url: "admin/trabajadores/{id}",
                defaults: new { controller = "Admin", action = "DarTrabajadores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RolTrabajadores",
                url: "admin/rol-trabajador/{id}",
                defaults: new { controller = "Admin", action = "DarRolTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosTrabajador",
                url: "admin/datos-trabajador/{id}",
                defaults: new { controller = "Admin", action = "DatosTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarTrabajador",
                url: "admin/eliminar-trabajador/{id}",
                defaults: new { controller = "Admin", action = "EliminarTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "FuncionesTrabajador",
                url: "admin/funciones-trabajador/{id}",
                defaults: new { controller = "Admin", action = "FuncionesTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarFuncionesTrabajador",
                url: "admin/guardar-funciones-trabajador/{id}",
                defaults: new { controller = "Admin", action = "GuardarFuncionesTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNivelesModuloColaborador",
                url: "admin/niveles-modulo-trabajador/{id}",
                defaults: new { controller = "Admin", action = "DarNivelesModuloColaborador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EstablecerNivelModuloColaborador",
                url: "admin/establecer-nivel-modulo-trabajador/{id}",
                defaults: new { controller = "Admin", action = "EstablecerNivelModuloColaborador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNivelesCompetenciasColaborador",
                url: "admin/niveles-competencias-trabajador/{id}",
                defaults: new { controller = "Admin", action = "DarNivelesCompetenciasColaborador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EstablecerNivelCompetenciaColaborador",
                url: "admin/establecer-nivel-competencia-trabajador/{id}",
                defaults: new { controller = "Admin", action = "EstablecerNivelCompetenciaColaborador", id = UrlParameter.Optional }
            );

            /*----- Gestion de los Usuarios Clientes ----- */
            routes.MapRoute(
                name: "DarUsuariosClientes",
                url: "admin/usuarios-clientes/{id}",
                defaults: new { controller = "Admin", action = "DarUsuariosClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarUsuarioCliente",
                url: "admin/guardar-cliente/{id}",
                defaults: new { controller = "Admin", action = "GuardarUsuarioCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarUsuarioCliente",
                url: "admin/eliminar-usuario-cliente/{id}",
                defaults: new { controller = "Admin", action = "EliminarUsuarioCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ModulosFuncionalidades",
                url: "admin/modulos-funcionalidades/{id}",
                defaults: new { controller = "Admin", action = "ModulosFuncionalidades", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarContratosCliente",
                url: "admin/contratos-cliente/{id}",
                defaults: new { controller = "Admin", action = "DarContratosCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosContratoCliente",
                url: "admin/datos-contrato-cliente/{id}",
                defaults: new { controller = "Admin", action = "DarDatosContratosClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevoContratoCliente",
                url: "admin/nuevo-contrato-cliente/{id}",
                defaults: new { controller = "Admin", action = "NuevoContratoCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarAdjuntoContratoCliente",
                url: "admin/eliminar-adjunto-contrato/{id}",
                defaults: new { controller = "Admin", action = "EliminarAdjuntoContratoCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin",
                url: "admin/{id}",
                defaults: new { controller = "Admin", action = "Index", id = UrlParameter.Optional }
            );

            //---------RUTAS DE AL ADMINISTRACION DE LOS CATALOGOS
            routes.MapRoute(
                name: "DarDatosTabla",
                url: "catalogos/datos-tabla/{id}",
                defaults: new { controller = "Admin", action = "DarDatosTabla", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarVerificadorCatalogo",
                url: "catalogos/dar-verificador-catalogo/{id}",
                defaults: new { controller = "Admin", action = "DarVerificadorCatalogo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarDatosCatalogo",
                url: "catalogos/guardar-datos-catalogo/{id}",
                defaults: new { controller = "Admin", action = "GuardarDatosCatalogo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarTuplaCatalogos",
                url: "catalogos/eliminar-tupla-catalogo/{id}",
                defaults: new { controller = "Admin", action = "EliminarTuplaCatalogos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosDescripcionCatalogo",
                url: "catalogos/datos-descripcion-catalogo/{id}",
                defaults: new { controller = "Admin", action = "DatosDescripcionCatalogo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosActividadesRealizadas",
                url: "catalogos/datos-actividades-realizadas/{id}",
                defaults: new { controller = "Admin", action = "DatosActividadesRealizadas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarActividadesRealizadas",
                url: "catalogos/guardar-actividades-realizadas/{id}",
                defaults: new { controller = "Admin", action = "GuardarActividadesRealizadas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarTuplasActividadesRealizadas",
                url: "catalogos/eliminar-tuplas-actividades-realizadas/{id}",
                defaults: new { controller = "Admin", action = "EliminarTuplasActividadesRealizadas", id = UrlParameter.Optional }
            );

            //---------RUTAS DE CATALOGOS-----------
            routes.MapRoute(
                name: "DarPaises",
                url: "catalogos/paises/{id}",
                defaults: new { controller = "Admin", action = "DarPaises", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTicketVersionCliente",
                url: "catalogos/ticketVersionCliente/{id}",
                defaults: new { controller = "Admin", action = "DarTicketVersionCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarSedes",
                url: "catalogos/sedes/{id}",
                defaults: new { controller = "Admin", action = "DarSedes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarCargos",
                url: "catalogos/cargos/{id}",
                defaults: new { controller = "Admin", action = "DarCargos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarRoles",
                url: "catalogos/roles/{id}",
                defaults: new { controller = "Admin", action = "DarRoles", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarModulosSistema",
                url: "catalogos/modulos-sistema/{id}",
                defaults: new { controller = "Admin", action = "DarModulosSistema", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTiposModulosSistema",
                url: "catalogos/tipos-modulos-sistema/{id}",
                defaults: new { controller = "Admin", action = "DarTiposModulosSistema", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarModulos",
                url: "catalogos/modulos/{id}",
                defaults: new { controller = "Admin", action = "DarModulos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarClientes",
                url: "catalogos/clientes/{id}",
                defaults: new { controller = "Admin", action = "DarClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNombresClientes",
                url: "catalogos/nombres-clientes/{id}",
                defaults: new { controller = "Admin", action = "DarNombresClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEstadosTickets",
                url: "catalogos/estados-tickets/{id}",
                defaults: new { controller = "Admin", action = "DarEstadosTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEstadosContrato",
                url: "catalogos/estados-contrato/{id}",
                defaults: new { controller = "Admin", action = "DarEstadosContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTiposPlazo",
                url: "catalogos/tipos-plazo/{id}",
                defaults: new { controller = "Admin", action = "DarTiposPlazo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarFasesContrato",
                url: "catalogos/fases-contrato/{id}",
                defaults: new { controller = "Admin", action = "DarFasesContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTiposMotivoTrabajo",
                url: "catalogos/tipo-motivo-trabajo/{id}",
                defaults: new { controller = "Admin", action = "DarTiposMotivoTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarLugares",
                url: "catalogos/lugares/{id}",
                defaults: new { controller = "Admin", action = "DarLugarTareas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarActividades",
                url: "catalogos/actividades/{id}",
                defaults: new { controller = "Admin", action = "DarActividades", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarCoordinadores",
                url: "catalogos/coordinadores/{id}",
                defaults: new { controller = "Admin", action = "DarCoordinadores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarResponsablesProyecto",
                url: "catalogos/responsables-proyecto/{id}",
                defaults: new { controller = "Admin", action = "DarResponsablesProyecto", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarRepositorios",
                url: "catalogos/repositorios/{id}",
                defaults: new { controller = "Admin", action = "DarRepositorios", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarVersionesDesarrollo",
                url: "catalogos/versionesDesarrollo/{id}",
                defaults: new { controller = "Admin", action = "DarVersionesDesarrollo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarVersionesBaseDatos",
                url: "catalogos/versionesBaseDatos/{id}",
                defaults: new { controller = "Admin", action = "DarVersionesBaseDatos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarMotivosDevolucionTicket",
                url: "catalogos/motivos-devolucion-ticket/{id}",
                defaults: new { controller = "Admin", action = "DarMotivosDevolucionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarCatalogos",
                url: "catalogos/catalogos/{id}",
                defaults: new { controller = "Admin", action = "DarCatalogos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDepartamentos",
                url: "catalogos/departamentos/{id}",
                defaults: new { controller = "Admin", action = "DarDepartamentos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTiposContratosClientes",
                url: "catalogos/tipos-contratos-cliente/{id}",
                defaults: new { controller = "Admin", action = "DarTiposContratosCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarPrioridadesTicket",
                url: "catalogos/tickets-prioridades/{id}",
                defaults: new { controller = "Admin", action = "DarPrioridadesTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarActividadRealizada",
                url: "catalogos/actividades-realizadas/{id}",
                defaults: new { controller = "Admin", action = "DarActividadRealizada", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarActividadPropuesta",
                url: "catalogos/actividades-propuestas-nt/{id}",
                defaults: new { controller = "Admin", action = "DarActividadPropuesta", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarCausasNoTerminacion",
                url: "catalogos/causas-no-terminacion/{id}",
                defaults: new { controller = "Admin", action = "DarCausasNoTerminacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarImplicacionErrorPuntos",
                url: "catalogos/implicaciones-error-puntos/{id}",
                defaults: new { controller = "Admin", action = "DarImplicacionErrorPuntos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarImplicacionErrorPorTipoError",
                url: "catalogos/implicacionerror-x-tipoerror/{id}",
                defaults: new { controller = "Admin", action = "DarImplicacionErrorPorTipoError", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosCatalogo",
                url: "catalogos/datos-catalogos/{id}",
                defaults: new { controller = "Admin", action = "DarDatosCatalogo", id = UrlParameter.Optional }
            );

            //RUTAS DE TASK----------GESTION DE TAREAS--------------------------------------------
            //RUTAS DEL ESTADO DE LAS TARAS
            routes.MapRoute(
                name: "GraficoTareasDia",
                url: "task/graph-tareas-dia/{id}",
                defaults: new { controller = "Task", action = "GraficoTareasDia", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "GraficoTareasSemana",
                url: "task/graph-tareas-semana/{id}",
                defaults: new { controller = "Task", action = "GraficoTareasSemana", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "GraficoTareasDesde7Dias",
                url: "task/graph-tareas-desde7dias/{id}",
                defaults: new { controller = "Task", action = "GraficoTareasDesde7Dias", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "GraficoTareasDesde30Dias",
                url: "task/graph-tareas-desde30dias/{id}",
                defaults: new { controller = "Task", action = "GraficoTareasDesde30Dias", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "UltimoLunes",
                url: "task/ultimo-lunes/{id}",
                defaults: new { controller = "Task", action = "DarUltimoLunes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTareasTrabajadores",
                url: "task/tareas-trabajadores/{id}",
                defaults: new { controller = "Task", action = "DarTareasTrabajadores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevaTarea",
                url: "task/nueva-tarea/{id}",
                defaults: new { controller = "Task", action = "NuevaTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EmailNuevaTarea",
                url: "task/email-nueva-tarea/{id}",
                defaults: new { controller = "Task", action = "EmailNuevaTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AnularTarea",
                url: "task/anular-tarea/{id}",
                defaults: new { controller = "Task", action = "AnularTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosTarea",
                url: "task/datos-tarea/{id}",
                defaults: new { controller = "Task", action = "DarDatosTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ActualizarTareaUsuarioAdmin",
                url: "task/actualizar-tarea-usuario/{id}",
                defaults: new { controller = "Task", action = "ActualizarTareaUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDiasActividadesTareas",
                url: "task/dias-actividades-tareas/{id}",
                defaults: new { controller = "Task", action = "DarDiasActividadesTareas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdicionarActividadTarea",
                url: "task/adicionar-actividad-tarea/{id}",
                defaults: new { controller = "Task", action = "AdicionarActividadTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ActualizarHoraActividadTarea",
                url: "task/actualizar-hora-actividad-tarea/{id}",
                defaults: new { controller = "Task", action = "ActualizarHoraActividadTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarActividadesTarea",
                url: "task/dar-actividades-tarea/{id}",
                defaults: new { controller = "Task", action = "DarActividadesTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarActividadesTarea",
                url: "task/eliminar-actividad-tarea/{id}",
                defaults: new { controller = "Task", action = "EliminarActividadesTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarComentariosActividad",
                url: "task/dar-comentarios/{id}",
                defaults: new { controller = "Task", action = "DarComentariosActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdicionarComentarioActividad",
                url: "task/adicionar-comentario/{id}",
                defaults: new { controller = "Task", action = "AdicionarComentarioActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarComentarioActividad",
                url: "task/eliminar-comentario/{id}",
                defaults: new { controller = "Task", action = "EliminarComentarioActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MonitoreoTareas",
                url: "task/monitoreo-tareas/{id}",
                defaults: new { controller = "Task", action = "MonitoreoTareas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AsignacionesPersona",
                url: "task/asignaciones-persona/{id}",
                defaults: new { controller = "Task", action = "AsignacionesPersona", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SolicitudAccesoTFS",
                url: "task/solicitud-acceso-tfs/{id}",
                defaults: new { controller = "Task", action = "SolicitudAccesoTFS", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MoverTarea",
                url: "task/mover-tarea/{id}",
                defaults: new { controller = "Task", action = "MoverTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "OrdenarTareas",
                url: "task/ordenar-tareas/{id}",
                defaults: new { controller = "Task", action = "OrdenarTareas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarSolicitudesTareas",
                url: "task/solicitudes-task/{id}",
                defaults: new { controller = "Task", action = "DarSolicitudesTareas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AceptarSolicitudTarea",
                url: "task/aceptar-tarea-solicitud/{id}",
                defaults: new { controller = "Task", action = "AceptarSolicitudTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevaTareasPorSolicitud",
                url: "task/nueva-tarea-solicitud/{id}",
                defaults: new { controller = "Task", action = "NuevaTareasPorSolicitud", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RechazarSolicitudTarea",
                url: "task/rechazar-tarea-solicitud/{id}",
                defaults: new { controller = "Task", action = "RechazarSolicitudTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarCoordinadosColaborador",
                url: "task/coordinados-persona/{id}",
                defaults: new { controller = "Task", action = "DarCoordinadosColaborador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "VerConsolidacionTarea",
                url: "task/consolidacion-tareas/{id}",
                defaults: new { controller = "Task", action = "VerConsolidacionTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ComprobarContrato",
                url: "task/comprobar-contrato/{id}",
                defaults: new { controller = "Task", action = "ComprobarContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarMotivosTrabajo",
                url: "task/cargar-motivos-trabajo/{id}",
                defaults: new { controller = "Task", action = "CargarMotivosTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarGarantiasTrabajo",
                url: "task/cargar-garantias-trabajo/{id}",
                defaults: new { controller = "Task", action = "CargarGarantiasTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarGarantiaTickets",
                url: "consultas/cargar-garantia-tickets/{id}",
                defaults: new { controller = "Consultas", action = "CargarGarantiaTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarSeguimientosTrabajo",
                url: "task/cargar-seguimientos-trabajo/{id}",
                defaults: new { controller = "Task", action = "CargarSeguimientosTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "GuardarAdendasContrato",
               url: "task/guardar-adendas-contrato/{id}",
               defaults: new { controller = "Task", action = "GuardarAdendasContrato", id = UrlParameter.Optional }
           );

            routes.MapRoute(
                name: "EliminarAdjuntoContrato",
                url: "task/eliminar-adjunto-contrato/{id}",
                defaults: new { controller = "Task", action = "EliminarAdjuntoContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarOfertasTickets",
                url: "consultas/dar-ofertas-tickets/{id}",
                defaults: new { controller = "Consultas", action = "DarOfertasTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AgregarOfertaTickets",
                url: "consultas/agregar-ofertas-tickets/{id}",
                defaults: new { controller = "Consultas", action = "AgregarOfertaTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EditarOfertaTickets",
                url: "consultas/editar-ofertas-tickets/{id}",
                defaults: new { controller = "Consultas", action = "EditarOfertaTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarOfertaTickets",
                url: "consultas/eliminar-oferta-tickets/{id}",
                defaults: new { controller = "Consultas", action = "EliminarOfertaTickets", id = UrlParameter.Optional }
            );

            //RUTAS PARA LAS ACTAS

            routes.MapRoute(
                name: "CargarActas",
                url: "task/cargar-actas/{id}",
                defaults: new { controller = "Task", action = "DarActas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CalcularCodigoActa",
                url: "task/calcular-codigo-acta/{id}",
                defaults: new { controller = "Task", action = "DarCodigoActa", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarActa",
                url: "task/guardar-acta/{id}",
                defaults: new { controller = "Task", action = "GuardarActa", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarActa",
                url: "task/eliminar-acta/{id}",
                defaults: new { controller = "Task", action = "EliminarActa", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ObtenerDatosActa",
                url: "task/obtener-datos-acta/{id}",
                defaults: new { controller = "Task", action = "ObtenerDatosActa", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTiposActas",
                url: "task/tipos-actas/{id}",
                defaults: new { controller = "Task", action = "DarTiposActas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEstadosActas",
                url: "task/estados-actas/{id}",
                defaults: new { controller = "Task", action = "DarEstadosActas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ObtenerContrato",
                url: "task/obtener-contrato/{id}",
                defaults: new { controller = "Task", action = "ObtenerContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarMotivoTrabajoInformacionAdicional",
                url: "task/guardar-informacion-trabajo/{id}",
                defaults: new { controller = "Task", action = "GuardarMotivoTrabajoInformacionAdicional", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarSeguimientoContrato",
                url: "task/eliminar-seguimiento-contrato/{id}",
                defaults: new { controller = "Task", action = "EliminarSeguimientoContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarSeguimientoContrato",
                url: "task/guardar-seguimiento-contrato/{id}",
                defaults: new { controller = "Task", action = "GuardarSeguimientoContrato", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevoMotivoTrabajo",
                url: "task/guardar-motivos-trabajo/{id}",
                defaults: new { controller = "Task", action = "NuevoMotivoTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevoEntregableMotivoTrabajo",
                url: "task/guardar-entregable-motivo-trabajo/{id}",
                defaults: new { controller = "Task", action = "NuevoEntregableMotivoTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarListaEntregables",
                url: "task/guardar-lista-entregables/{id}",
                defaults: new { controller = "Task", action = "GuardarListaEntregables", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEntregablesMotivosTrabajo",
                url: "task/dar-entregables-trabajo/{id}",
                defaults: new { controller = "Task", action = "DarEntregablesMotivosTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEntregablesParaActas",
                url: "task/dar-entregables-para-actas/{id}",
                defaults: new { controller = "Task", action = "DarEntregablesParaActas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosMotivoTrabajo",
                url: "task/datos-motivo-trabajo/{id}",
                defaults: new { controller = "Task", action = "DarDatosMotivoTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDetallesMotivoTrabajo",
                url: "task/dar-detalles-motivo-trabajo/{id}",
                defaults: new { controller = "Task", action = "DarDetallesMotivoTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDetallesGarantia",
                url: "task/dar-detalles-garantia/{id}",
                defaults: new { controller = "Task", action = "DarDetallesGarantia", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosEntregableTrabajo",
                url: "task/datos-entregable-trabajo/{id}",
                defaults: new { controller = "Task", action = "DarDatosEntregableTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarMotivoTrabajo",
                url: "task/eliminar-motivo-trabajo/{id}",
                defaults: new { controller = "Task", action = "EliminarMotivoTrabajo", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarEntregableTrabajo",
                url: "task/eliminar-entregable-trabajo/{id}",
                defaults: new { controller = "Task", action = "EliminarEntregableTrabajo", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "EditarPorcentajeEntregable",
                url: "task/editar-porcentaje-entregable/{id}",
                defaults: new { controller = "Task", action = "EditarPorcentajeEntregable", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosNewTaskConsolidacion",
                url: "task/datos-new-tarea-consolidacion/{id}",
                defaults: new { controller = "Task", action = "DarDatosNewTaskConsolidacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevaTareaConsolidacion",
                url: "task/new-tarea-consolidacion/{id}",
                defaults: new { controller = "Task", action = "NuevaTareaConsolidacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarProximosEntregables",
                url: "task/proximos-entregables/{id}",
                defaults: new { controller = "Task", action = "DarProximosEntregables", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AprobarPermiso",
                url: "task/aprobar-permiso/{id}",
                defaults: new { controller = "Task", action = "AprobarPermiso", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MoverPermiso",
                url: "task/mover-permiso/{id}",
                defaults: new { controller = "Task", action = "MoverPermiso", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GestionarFeriado",
                url: "task/gestionar-feriado/{id}",
                defaults: new { controller = "Task", action = "GestionarFeriado", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "VerDisponibilidadRecursos",
                url: "task/disponibilidad-recursos/{id}",
                defaults: new { controller = "Task", action = "VerDisponibilidadRecursos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosDiaTarea",
                url: "task/datos-dia-tarea/{id}",
                defaults: new { controller = "Task", action = "DatosDiaTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "InsertarIncidenciaColaborador",
                url: "task/editar-incidencia-colaborador/{id}",
                defaults: new { controller = "Task", action = "InsertarIncidenciaColaborador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarIncidenciasColaborador",
                url: "task/dar-incidencia-colaborador/{id}",
                defaults: new { controller = "Task", action = "DarIncidenciasColaborador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarIncidencia",
                url: "task/eliminar-incidencia-colaborador/{id}",
                defaults: new { controller = "Task", action = "EliminarIncidencia", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarIncidenciasRecursos",
                url: "task/incidencias-recursos/{id}",
                defaults: new { controller = "Task", action = "VerIncidenciasRecursos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarIncidenciasDeDiaColaboradorPorIdIncidencia-Recursos",
                url: "task/dar-incidencia-x-incidencia/{id}",
                defaults: new { controller = "Task", action = "DarIncidenciasDeDiaColaboradorPorIdIncidencia", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarAsignacionesSemanaEntregable",
                url: "task/asignaciones-semana-entregable/{id}",
                defaults: new { controller = "Task", action = "DarAsignacionesSemanaEntregable", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarConocimientosNivelColaboradores",
                url: "task/dar-conocimientos-colaboradores/{id}",
                defaults: new { controller = "Task", action = "DarConocimientosNivelColaboradores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RepetirTareasVerticales",
                url: "task/repetir-tarea-vertical/{id}",
                defaults: new { controller = "Task", action = "RepetirTareasVerticales", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EstablecerQuitarTareaCompensatoria",
                url: "task/establecer-quitar-tarea-compensatoria/{id}",
                defaults: new { controller = "Task", action = "EstablecerQuitarTareaCompensatoria", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AnularPermiso",
                url: "task/anular-permiso/{id}",
                defaults: new { controller = "Task", action = "AnularPermiso", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "LeerExcel",
                url: "task/leer-excel/{id}",
                defaults: new { controller = "Task", action = "LeerExcel", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SubirExcel",
                url: "task/subir-excel/{id}",
                defaults: new { controller = "Task", action = "SubirExcel", id = UrlParameter.Optional }
            );

            //---------------------- RUTAS DEL USUARIO -------------------------------
            //RUTAS DE LA EDICION DE LAS TAREAS POR EL USUARIO            

            routes.MapRoute(
                name: "UltimoLunesUsuario",
                url: "user/ultimo-lunes/{id}",
                defaults: new { controller = "User", action = "DarUltimoLunes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTareasUsuario",
                url: "user/tareas-usuario/{id}",
                defaults: new { controller = "User", action = "DarTareasUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ActualizarTareaUsuario",
                url: "user/actualizar-tarea-usuario/{id}",
                defaults: new { controller = "User", action = "ActualizarTareaUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TareaPerteneceTicket",
                url: "user/tarea-pertenece-ticket/{id}",
                defaults: new { controller = "User", action = "TareaPerteneceTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarEmailFinTicket",
                url: "user/enviar-email-fin-ticket/{id}",
                defaults: new { controller = "User", action = "EnviarEmailFinTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarCoordinadosUsuario",
                url: "user/coordinados-usuario/{id}",
                defaults: new { controller = "User", action = "DarCoordinadosUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EstimacionesUsuario",
                url: "user/estimaciones-usuario/{id}",
                defaults: new { controller = "User", action = "EstimacionesUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ProyectosUsuario",
                url: "user/proyectos-usuario/{id}",
                defaults: new { controller = "User", action = "ProyectosUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TicketsUsuario",
                url: "user/tickets-usuario/{id}",
                defaults: new { controller = "User", action = "TicketsUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosEstimacionUsuario",
                url: "user/estimacion-usuario/{id}",
                defaults: new { controller = "User", action = "DatosEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosEntregabelEstimacionUsuario",
                url: "user/edcion-estimacion-usuario/{id}",
                defaults: new { controller = "User", action = "DatosEntregabelEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNivelesColaborador",
                url: "user/niveles-colaborador/{id}",
                defaults: new { controller = "User", action = "DarNivelesColaboradores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTipoNotificacionTicket",
                url: "user/tipo-notificacion-ticket/{id}",
                defaults: new { controller = "User", action = "DarTipoNotificacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarNotificacionTicket",
                url: "user/enviar-notificacion-ticket/{id}",
                defaults: new { controller = "User", action = "EnviarNotificacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarEstimacion",
                url: "user/guardar-estimacion/{id}",
                defaults: new { controller = "User", action = "GuardarEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarEntregableEstimacionUsuario",
                url: "user/guardar-entregable-estimacion/{id}",
                defaults: new { controller = "User", action = "GuardarEntregableEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EditarEntregableEstimacionUsuario",
                url: "user/editar-entregable-estimacion/{id}",
                defaults: new { controller = "User", action = "EditarEntregableEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarEntregableEstimacionUsuario",
                url: "user/eliminar-entregable-estimacion/{id}",
                defaults: new { controller = "User", action = "EliminarEntregableEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosTicketsUser",
                url: "user/datos-ticket/{id}",
                defaults: new { controller = "User", action = "DarDatosTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "verficacionDeEstimacion",
                url: "user/verificacion-estimacion/{id}",
                defaults: new { controller = "User", action = "verficacionDeEstimacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdicionarActividadTareaUsuario",
                url: "user/adicionar-actividad-tarea/{id}",
                defaults: new { controller = "User", action = "AdicionarActividadTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ActualizarHoraActividadTareaUsuario",
                url: "user/actualizar-hora-actividad-tarea/{id}",
                defaults: new { controller = "User", action = "ActualizarHoraActividadTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarActividadesTareaUsuario",
                url: "user/dar-actividades-tarea/{id}",
                defaults: new { controller = "User", action = "DarActividadesTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarActividadesTareaUsuario",
                url: "user/eliminar-actividad-tarea-usuario/{id}",
                defaults: new { controller = "User", action = "EliminarActividadesTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarComentariosActividadUsuario",
                url: "user/dar-comentarios/{id}",
                defaults: new { controller = "User", action = "DarComentariosActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdicionarComentarioActividadUsuario",
                url: "user/adicionar-comentario/{id}",
                defaults: new { controller = "User", action = "AdicionarComentarioActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarComentarioActividadUsuario",
                url: "user/eliminar-comentario/{id}",
                defaults: new { controller = "User", action = "EliminarComentarioActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SolicitarTareaUsuario",
                url: "user/solicitar-tarea/{id}",
                defaults: new { controller = "User", action = "SolicitarTarea", id = UrlParameter.Optional }
            );

			routes.MapRoute(
			   name: "SolicitudVacacionesUsuario",
			   url: "user/solicitar-vacaciones/{id}",
			   defaults: new { controller = "User", action = "SolicitudVacacionesUsuario", id = UrlParameter.Optional }
		    );

			routes.MapRoute(
			   name: "SolicitudPermisosUsuario",
			   url: "user/solicitar-permisos/{id}",
			   defaults: new { controller = "User", action = "SolicitudPermisosUsuario", id = UrlParameter.Optional }
			);

			routes.MapRoute(
			   name: "VacacionesUsuario",
			   url: "user/vacaciones-usuario/{id}",
			   defaults: new { controller = "User", action = "VacacionesUsuario", id = UrlParameter.Optional }
			);

			routes.MapRoute(
			   name: "PermisosUsuario",
			   url: "user/permisos-usuario/{id}",
			   defaults: new { controller = "User", action = "PermisosUsuario", id = UrlParameter.Optional }
			);

            routes.MapRoute(
               name: "VacacionesUsuarios",
               url: "rrhh/vacaciones-usuarios/{id}",
               defaults: new { controller = "Rrhh", action = "VacacionesUsuarios", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "PermisosUsuarios",
               url: "rrhh/permisos-usuarios/{id}",
               defaults: new { controller = "Rrhh", action = "PermisosUsuarios", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "UsuarioTareasCompensatorias",
                url: "user/colaborador-tareas-compensatorias/{id}",
                defaults: new { controller = "User", action = "UsuarioTareasCompensatorias", id = UrlParameter.Optional }
            );

            //RUTAS DEL CHAT DE LOS USUARIOS
            routes.MapRoute(
                name: "DarUsersActivos",
                url: "user/usuarios-activos-chat/{id}",
                defaults: new { controller = "User", action = "DarUsersActivos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarMensajeChat",
                url: "user/enviar-mensaje-chat/{id}",
                defaults: new { controller = "User", action = "EnviarMensajeChat", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarMensajeChat",
                url: "user/dar-mensaje-chat/{id}",
                defaults: new { controller = "User", action = "DarMensajeChat", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNMensajeChat",
                url: "user/dar-nmensaje-chat/{id}",
                defaults: new { controller = "User", action = "DarNMensajeChat", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TicketsPorPublicar",
                url: "user/ticket-por-publicar/{id}",
                defaults: new { controller = "User", action = "TicketsPorPublicar", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TicketPublicado",
                url: "user/ticket-publicado/{id}",
                defaults: new { controller = "User", action = "TicketPublicado", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "guardarComentarioNoTerminacion",
                url: "user/guardar-comentario-no-terminacion/{id}",
                defaults: new { controller = "User", action = "guardarComentarioNoTerminacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "guardarComentarioNoTerminacionTarea",
                url: "user/guardar-comentario-no-terminacion-tarea/{id}",
                defaults: new { controller = "User", action = "guardarComentarioNoTerminacionTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "VerDatosDesarrolloTicket",
                url: "user/ver-datos-desarrollo-ticket/{id}",
                defaults: new { controller = "User", action = "VerDatosDesarrolloTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarComentariosTicketsUsuario",
                url: "user/cargar-comentarios-ticket/{id}",
                defaults: new { controller = "User", action = "CargarComentariosTicketsUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarComentariosTicketsUsuario",
                url: "user/guardar-comentario-ticket/{id}",
                defaults: new { controller = "User", action = "GuardarComentariosTicketsUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarActividadesTareaSegunActividadTarea",
                url: "user/dar-actividades-segun-actividad-tarea/{id}",
                defaults: new { controller = "User", action = "DarActividadesTareaSegunActividadTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "sistema",
                url: "user/{id}",
                defaults: new { controller = "User", action = "Sistema", id = UrlParameter.Optional }
            );

            //---------------------  RUTAS DE LOS CLIENTES ----------------
            routes.MapRoute(
                name: "DarCategoriasTicketSegunContratosClientes",
                url: "clientes/categorias-ticket/{id}",
                defaults: new { controller = "Client", action = "DarCategoriasTicketSegunContratosClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarGarantiasTicketsCliente",
                url: "clientes/garantias-ticket-cliente",
                defaults: new { controller = "Client", action = "DarGarantiasTicketsCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarSeguimientosTicketsCliente",
                url: "clientes/seguimientos-ticket",
                defaults: new { controller = "Client", action = "DarSeguimientosTicketsCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarPeticionDeTicket",
                url: "clientes/nuevo-ticket",
                defaults: new { controller = "Client", action = "GuardarPeticionDeTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarComentariosTicketsCliente",
                url: "clientes/cargar-comentarios-ticket/{id}",
                defaults: new { controller = "Client", action = "CargarComentariosTicketsCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarComentariosTicketsCliente",
                url: "clientes/guardar-comentario-ticket/{id}",
                defaults: new { controller = "Client", action = "GuardarComentariosTicketsCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarPropuestaCotizacion",
                url: "clientes/propuesta-cotizacion",
                defaults: new { controller = "Client", action = "DarPropuestaCotizacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RespuestaCotizacion",
                url: "clientes/respuesta-cotizacion",
                defaults: new { controller = "Client", action = "RespuestaCotizacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RenegociarOferta",
                url: "clientes/enviar-renegociacion",
                defaults: new { controller = "Client", action = "RenegociarOferta", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosTicketCliente",
                url: "clientes/datos-ticket",
                defaults: new { controller = "Client", action = "DarDatosTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdicionarAdjuntosTicket",
                url: "clientes/anadir-adjuntos-ticket",
                defaults: new { controller = "Client", action = "AdicionarAdjuntosTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RespuestaResolucion",
                url: "clientes/respuesta-resolucion",
                defaults: new { controller = "Client", action = "RespuestaResolucion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarEmailTicketDevuelto",
                url: "clientes/enviar-email-ticket-devuelto",
                defaults: new { controller = "Client", action = "EnviarEmailTicketDevuelto", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarEmailTicketDevueltoCliente",
                url: "clientes/enviar-email-ticket-devuelto-cliente",
                defaults: new { controller = "Client", action = "EnviarEmailTicketDevueltoCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Clientes",
                url: "clientes/{id}",
                defaults: new { controller = "Client", action = "Index", id = UrlParameter.Optional }
            );

            //-----------------  FIN DE RUTAS DE LOS CLIENTES ----------------

            //---------------------  RUTAS DE LOS TICKETS ----------------
            routes.MapRoute(
                name: "DarDatosTicket",
                url: "tickets/dar-datos-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarDatosTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTickets",
                url: "tickets/dar-tickets/{id}",
                defaults: new { controller = "Ticket", action = "DarTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarAdjuntoTicket",
                url: "tickets/eliminar-adjunto-ticket/{id}",
                defaults: new { controller = "Ticket", action = "EliminarAdjuntoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTicketsGestores",
                url: "tickets/dar-tickets-gestores/{id}",
                defaults: new { controller = "Ticket", action = "DarTicketsGestores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarEmailReporteTickets",
                url: "tickets/email-reporte-ticket/{id}",
                defaults: new { controller = "Ticket", action = "EnviarEmailReporteTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarMantenimientoTicket",
                url: "tickets/eliminar-mantenimiento/{id}",
                defaults: new { controller = "Ticket", action = "EliminarMantenimientoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AgregarTicketTarea",
                url: "tickets/agregar-ticket-tarea/{id}",
                defaults: new { controller = "Ticket", action = "AgregarTicketTarea", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AgregarComentarioHoras",
                url: "tickets/agregar-comentario-horas/{id}",
                defaults: new { controller = "Ticket", action = "AgregarComentarioHoras", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EditarMantenimientoTicket",
                url: "tickets/editar-tiempo-mantenimiento/{id}",
                defaults: new { controller = "Ticket", action = "EditarMantenimientoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarContratosMantenimiento",
                url: "tickets/dar-contratos-mantenimiento/{id}",
                defaults: new { controller = "Ticket", action = "DarContratosMantenimiento", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarModulosFuncionalidadTicket",
                url: "tickets/modulos-funcionalidades-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarModulosFuncionalidadTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTicketAsignacionesColaborador",
                url: "tickets/asignaciones-colaborador/{id}",
                defaults: new { controller = "Ticket", action = "DarAsignacionesColaborador", id = UrlParameter.Optional }
            );
            
            routes.MapRoute(
                name: "Estimaciones",
                url: "tickets/estimaciones/{id}",
                defaults: new { controller = "Ticket", action = "Estimaciones", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEstimaciones",
                url: "tickets/dar-estimaciones/{id}",
                defaults: new { controller = "Ticket", action = "DarEstimaciones", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "OrdenarEstimarTicket",
                url: "tickets/ordenar-estimar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "OrdenarEstimarTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNivelesColaboradorTicket",
                url: "tickets/niveles-colaborador/{id}",
                defaults: new { controller = "Ticket", action = "DarNivelesColaboradores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosEstimacionTicket",
                url: "tickets/datos-estimacion-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarDatosEstimacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DatosEstimacionTicketUsuario",
                url: "tickets/datos-estimacion-usuario/{id}",
                defaults: new { controller = "Ticket", action = "DatosEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CatalogarEstimacionUsuario",
                url: "tickets/catalogar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "CatalogarEstimacionUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TerminarEstimacionTicket",
                url: "tickets/terminar-estimacion-ticket/{id}",
                defaults: new { controller = "Ticket", action = "TerminarEstimacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DetalleEstimacionTicket",
                url: "tickets/detalle-estimacion-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DetalleEstimacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AceptarEstimacionTicket",
                url: "tickets/aceptar-estimacion-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AceptarEstimacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RechazarEstimacionTicket",
                url: "tickets/rechazar-estimacion-ticket/{id}",
                defaults: new { controller = "Ticket", action = "RechazarEstimacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "RechazarEstimacionesTicket",
              url: "tickets/rechazar-estimaciones-ticket/{id}",
              defaults: new { controller = "Ticket", action = "RechazarEstimacionesTicket", id = UrlParameter.Optional }
          );

            routes.MapRoute(
                name: "DarCategoriasTickets",
                url: "tickets/categorias-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarCategoriasTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarPrioridadesTickets",
                url: "tickets/prioridades-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarPrioridadesTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarTicket",
                url: "tickets/guardar-datos-ticket/{id}",
                defaults: new { controller = "Ticket", action = "GuardarTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AbrirTicket",
                url: "tickets/abrir-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AbrirTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CotizadoresTickets",
                url: "tickets/cotizadores-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarCotizadoresTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PedirCotizacionTickets",
                url: "tickets/pedir-cotizar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "PedirCotizacionTickets", id = UrlParameter.Optional }
            );

            //adiciones de tickets
            routes.MapRoute(
                name: "InformacionUsuariosClientes",
                url: "tickets/informacion-usuarios-clientes/{id}",
                defaults: new { controller = "Ticket", action = "InformacionUsuariosClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AdicionarTicket",
                url: "tickets/adicionar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AdicionarTicket", id = UrlParameter.Optional }
            );

            //Gestión de los tickets resueltos
            routes.MapRoute(
               name: "DarTicketsResueltos",
               url: "tickets/buscar-tickets-resueltos/{id}",
               defaults: new { controller = "Ticket", action = "DarTicketsResueltos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "CerrarTicketsResueltos",
               url: "tickets/cerrar-tickets-resueltos/{id}",
               defaults: new { controller = "Ticket", action = "CerrarTicketsResueltos", id = UrlParameter.Optional }
            );

            //Gestión de los tickets en aprobación
            routes.MapRoute(
               name: "DarTicketsEnAprobacion",
               url: "tickets/buscar-tickets-en-aprobacion/{id}",
               defaults: new { controller = "Ticket", action = "DarTicketsEnAprobacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "CerrarTicketsEnAprobacion",
               url: "tickets/cerrar-tickets-en-aprobacion/{id}",
               defaults: new { controller = "Ticket", action = "CerrarTicketsResueltos", id = UrlParameter.Optional }
            );

            //Gestión de los tickets en espera
            routes.MapRoute(
               name: "DarTicketsEnEspera",
               url: "tickets/buscar-tickets-en-espera/{id}",
               defaults: new { controller = "Ticket", action = "DarTicketsEnEspera", id = UrlParameter.Optional }
            );

            //cotizaciones
            routes.MapRoute(
                name: "CotizacionesTickets",
                url: "tickets/cotizaciones-ticket/{id}",
                defaults: new { controller = "Ticket", action = "CotizacionesTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarColaboradoresParaVerAsignaciones",
                url: "tickets/colaboradores-asignaciones/{id}",
                defaults: new { controller = "Ticket", action = "DarColaboradoresParaVerAsignaciones", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEstimacionesTickets",
                url: "tickets/estimaciones-aceptadas-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarEstimacionesTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "NuevaOfertaCotizacionTicket",
                url: "tickets/nueva-oferta-cotizacion/{id}",
                defaults: new { controller = "Ticket", action = "NuevaOfertaCotizacionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "TicketsNoCerradosCliente",
                url: "tickets/ticket-no-cerrados-cliente/{id}",
                defaults: new { controller = "Ticket", action = "TicketsNoCerradosCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CotizacionesCliente",
                url: "tickets/cotizaciones-cliente/{id}",
                defaults: new { controller = "Ticket", action = "CotizacionesCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EventosHistoricoTicket",
                url: "tickets/eventos-historicos-ticket/{id}",
                defaults: new { controller = "Ticket", action = "EventosHistoricoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosEventoTicket",
                url: "tickets/datos-evento-historico-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarDatosEventoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GrabarInformacionHistoricoTicket",
                url: "tickets/grabar-informacion-historico/{id}",
                defaults: new { controller = "Ticket", action = "GrabarInformacionHistoricoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GrabarInformacionHistoricoDevolucionTicket",
                url: "tickets/grabar-informacion-historico-devolucion/{id}",
                defaults: new { controller = "Ticket", action = "GrabarInformacionHistoricoDevolucionTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ComprobarTicketWorkItemTFS",
                url: "tickets/comprobar-ticket-workitem/{id}",
                defaults: new { controller = "Ticket", action = "ComprobarTicketWorkItemTFS", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AsignacionTareaPorTicket",
                url: "tickets/asignar-tarea-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AsignacionTareaPorTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AnularTareaPorTicket",
                url: "tickets/anular-tarea-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AnularTareaPorTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AnularTicket",
                url: "tickets/anular-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AnularTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AprobarTicket",
                url: "tickets/aprobar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AprobarTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CerrarTicket",
                url: "tickets/cerrar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "CerrarTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EsperarRespuestaTicket",
                url: "tickets/esperando-respuesta-ticket/{id}",
                defaults: new { controller = "Ticket", action = "EsperarRespuestaTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AceptarTicket",
                url: "tickets/aceptar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "AceptarTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ResumenTicket",
                url: "tickets/resumen-ticket/{id}",
                defaults: new { controller = "Ticket", action = "ResumenTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ReporteTicket",
                url: "tickets/reporte-ticket/{id}",
                defaults: new { controller = "Ticket", action = "ReporteTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarDesacuerdosTickets",
                url: "tickets/cargar-desacuerdos-ticket/{id}",
                defaults: new { controller = "Ticket", action = "CargarDesacuerdosTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarDesacuerdoTickets",
                url: "tickets/guardar-desacuerdo-ticket/{id}",
                defaults: new { controller = "Ticket", action = "GuardarDesacuerdoTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CargarComentariosTickets",
                url: "tickets/cargar-comentarios-ticket/{id}",
                defaults: new { controller = "Ticket", action = "CargarComentariosTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarComentariosTickets",
                url: "tickets/guardar-comentario-ticket/{id}",
                defaults: new { controller = "Ticket", action = "GuardarComentariosTickets", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EnviarEmailComentario",
                url: "tickets/enviar-email-comentario/{id}",
                defaults: new { controller = "Ticket", action = "EnviarEmailComentario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RechazarTicket",
                url: "tickets/rechazar-ticket/{id}",
                defaults: new { controller = "Ticket", action = "RechazarTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "PendienteTicket",
                url: "tickets/pendiente-ticket/{id}",
                defaults: new { controller = "Ticket", action = "PendienteTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SuspenderTicket",
                url: "tickets/suspender-ticket/{id}",
                defaults: new { controller = "Ticket", action = "SuspenderTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarProximaActividadSegunEstadoTicket",
                url: "tickets/dar-proxima-actividad-segun-ticket/{id}",
                defaults: new { controller = "Ticket", action = "DarProximaActividadSegunEstadoTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarInformacionGauges",
                url: "ticket/gaugesTicket/{id}",
                defaults: new { controller = "Ticket", action = "DarInformacionGaugesTicket", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "VerificarEstadoProximaActividad",
                url: "tickets/verificar-estado-proxima-actividad/{id}",
                defaults: new { controller = "Ticket", action = "VerificarEstadoProximaActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CambiarProximaActividad",
                url: "tickets/cambiar-proxima-actividad/{id}",
                defaults: new { controller = "Ticket", action = "CambiarProximaActividad", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Tickets",
                url: "tickets/{id}",
                defaults: new { controller = "Ticket", action = "Index", id = UrlParameter.Optional }
            );
            //------------------ FIN DE RUTAS DE LOS TICKETS ----------------

            //------------------ RUTAS DEL MODULO DE RRHH -------------------
            routes.MapRoute(
                name: "GuardarVacaciones",
                url: "rrhh/guardar-vacaciones/{id}",
                defaults: new { controller = "Rrhh", action = "GuardarVacaciones", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarTiposPermisos",
                url: "rrhh/tipos-permisos/{id}",
                defaults: new { controller = "Rrhh", action = "DarTiposPermisos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarPermiso",
                url: "rrhh/guardar-permiso/{id}",
                defaults: new { controller = "Rrhh", action = "GuardarPermiso", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarVacacionesTrabajador",
                url: "rrhh/ver-vacaciones-colaborador/{id}",
                defaults: new { controller = "Rrhh", action = "DarVacacionesTrabajador", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ColaboradoresTiempoCompesatorio",
                url: "rrhh/colaboradores-tiempo-compensatorio/{id}",
                defaults: new { controller = "Rrhh", action = "ColaboradoresTiempoCompesatorio", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ColaboradorTareasCompensatorias",
                url: "rrhh/colaborador-tareas-compensatorias/{id}",
                defaults: new { controller = "Rrhh", action = "ColaboradorTareasCompensatorias", id = UrlParameter.Optional }
            );

            //Adicionando ruteo para listar marcas electronicas
            routes.MapRoute(
                name: "MarcasElectronicas",
                url: "rrhh/marcas-electronicas/{id}",
                defaults: new { controller = "Rrhh", action = "DevolverMarcasElectronicas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "EliminarDiaVacacion",
                url: "rrhh/eliminar-vacacion/{id}",
                defaults: new { controller = "Rrhh", action = "EliminarDiaVacacion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DisponibilidadRecursosFechas",
                url: "rrhh/disponibilidad-recursos-fechas/{id}",
                defaults: new { controller = "Rrhh", action = "DisponibilidadRecursosFechas", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RRHH",
                url: "rrhh/{id}",
                defaults: new { controller = "Rrhh", action = "Index", id = UrlParameter.Optional }
            );

            //------------------ FIN DE RUTAS DEL MODULO DE RRHH -------------------

            //--------------- RUTAS DEL MODULO DE GESTION DE ERRORES ---------------

            routes.MapRoute(
                name: "InformacionGeneral",
                url: "bugs/informacion-general/{id}",
                defaults: new { controller = "GestionErrores", action = "InformacionGeneral", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ErroresEmpresa",
                url: "bugs/errores-empresa/{id}",
                defaults: new { controller = "GestionErrores", action = "ErroresEmpresa", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDataError",
                url: "bugs/dar-data-error/{id}",
                defaults: new { controller = "GestionErrores", action = "DarDataError", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ErroresTotalesGraficoCalor",
                url: "bugs/dar-errores-grafico-semana-calor/{id}",
                defaults: new { controller = "GestionErrores", action = "ErroresTotalesGraficoCalor", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GestionErroresController",
                url: "bugs/{id}",
                defaults: new { controller = "GestionErrores", action = "Index", id = UrlParameter.Optional }
            );

            //------------ FIN DE RUTAS DEL MODULO DE GESTION DE ERRORES -----------

            //------------------ RUTAS DEL MODULO DE TFS --------------------------------
            routes.MapRoute(
                 name: "Proyectos",
                 url: "tfs/proyectos-tfs/{id}",
                 defaults: new { controller = "Tfs", action = "DevuelveProyectos", id = UrlParameter.Optional }
             );

            routes.MapRoute(
                 name: "DevuelveAreaProyecto",
                 url: "tfs/area-proyectos-tfs/{id}",
                 defaults: new { controller = "Tfs", action = "DevuelveAreaProyecto", id = UrlParameter.Optional }
             );

            routes.MapRoute(
               name: "Usuarios",
               url: "tfs/usuarios-tfs/{id}",
               defaults: new { controller = "Tfs", action = "DevuelveUsuarios", id = UrlParameter.Optional }
            );

            routes.MapRoute(
              name: "Cambios",
              url: "tfs/cambios-tfs/{id}",
              defaults: new { controller = "Tfs", action = "DevuelveCambios", id = UrlParameter.Optional }
           );

            routes.MapRoute(
              name: "CantidadCambios",
              url: "tfs/cantidad-cambios-tfs/{id}",
              defaults: new { controller = "Tfs", action = "DevuelveCantidadTotalConjuntoCambios", id = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "ConjuntoCambios",
            url: "tfs/conjunto-cambios-tfs/{id}",
            defaults: new { controller = "Tfs", action = "DevuelveConjuntoCambios", id = UrlParameter.Optional }
            );

            /*
            routes.MapRoute(
                name: "ConsultaTFS",
                url: "tfs/consultaTfs/{id}",
                defaults: new { controller = "Tfs", action = "ConsultaTFS", id = UrlParameter.Optional }
            );
            */

            routes.MapRoute(
                 name: "GestionarTfs",
                 url: "tfs/{id}",
                 defaults: new { controller = "Tfs", action = "Index", id = UrlParameter.Optional }
            );


            //------------- FIN DE LAS RUTAS DEL MODULO DE TFS --------------------------

            //RUTAS DEL MODULO CONSULTAS

            routes.MapRoute(
                name: "Consultas",
                url: "consultas/{id}",
                defaults: new { controller = "Consultas", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarModulosClientes",
                url: "task/cargar-modulos-clientes/{id}",
                defaults: new { controller = "Consultas", action = "DarModulosClientes", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarDatosModuloCliente",
                url: "task/obtener-datos-modulo-cliente/{id}",
                defaults: new { controller = "Consultas", action = "DarDatosModuloCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarEstadosModuloCliente",
                url: "task/estados-modulos-clientes/{id}",
                defaults: new { controller = "Consultas", action = "DarEstadosModuloCliente", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarFuncionalidadesExtra",
                url: "task/obtener-funcionalidades-extra/{id}",
                defaults: new { controller = "Consultas", action = "DarFuncionalidadesExtra", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarNombresFuncionalidades",
                url: "catalogos/nombres-funcionalidades/{id}",
                defaults: new { controller = "Admin", action = "DarNombresFuncionalidades", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DarColaboradores",
                url: "catalogos/dar-colaboradores/{id}",
                defaults: new { controller = "Admin", action = "DarColaboradores", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "GuardarDatos",
                url: "task/guardar-datos-modulo-cliente/{id}",
                defaults: new { controller = "Consultas", action = "GuardarDatos", id = UrlParameter.Optional }
            );

            //------------------ RUTAS DEL MODULO DE SERVICIOS -------------------

            routes.MapRoute(
                name: "AdicionarLogEventosFBS",
                url: "Servicios/AdicionarLogEventosFBS/{id}",
                defaults: new { controller = "Servicios", action = "AdicionarLogEventosFBS", id = UrlParameter.Optional }
            );

            //------------------ FIN DE RUTAS DEL MODULO DE SERVICIOS -------------------

            //RUTAS DE AUTENTICACION INICIO DEL SISTEMA
            routes.MapRoute(
                name: "Login",
                url: "login/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "FullLogin",
                url: "sifizplanning/login/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CerrarSesion",
                url: "cerrar/sesion/{id}",
                defaults: new { controller = "Home", action = "CerrarSesion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "CambiarPassUser",
                url: "cambiar/user-password/{id}",
                defaults: new { controller = "Home", action = "CambiarPassword", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Redirect",
                url: "redirect/{id}",
                defaults: new { controller = "Home", action = "RedirectUser", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "FullRedirect",
                url: "sifizplanning/redirect/{id}",
                defaults: new { controller = "Home", action = "RedirectUser", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Home",
                url: "home/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MainMenu",
                url: "main-menu/{id}",
                defaults: new { controller = "Home", action = "MenuPrincipal", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Error",
                url: "error/{id}",
                defaults: new { controller = "Home", action = "Error", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "RecargarComentariosUsuario",
                url: "recargar-comentarios-usuario",
                defaults: new { controller = "Home", action = "RecargarComentariosUsuario", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MostrarTodosLosComentarios",
                url: "Mostrar-Todos-Comentarios",
                defaults: new { controller = "Home", action = "DarTodosLosComentarios", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ComentariosNoLeidos",
                url: "comentarios-no-leidos",
                defaults: new { controller = "Home", action = "DarComentariosNoLeidos", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "MarcarComentarioLeido",
                url: "marcar-comentario-general-leido",
                defaults: new { controller = "Home", action = "MarcarComentarioLeido", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //RUTAS DE REPORTES

            routes.MapRoute(
                name: "VerReport",
                url: "report/VerReporte/{id}",
                defaults: new { controller = "Report", action = "VerReporte", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Report",
                url: "report/{id}",
                defaults: new { controller = "Report", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}