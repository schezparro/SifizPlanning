using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using NLog;
using SifizPlanning.Util;

namespace SifizPlanning
{
    // Nota: para obtener instrucciones sobre cómo habilitar el modo clásico de IIS6 o IIS7, 
    // visite http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // TODO: Registrar ModelBinder personalizado para fechas (pendiente corrección de compatibilidad)
            // ModelBinders.Binders.Add(typeof(DateTime), new SifizPlanning.Util.DateTimeModelBinder());
            // ModelBinders.Binders.Add(typeof(DateTime?), new SifizPlanning.Util.DateTimeModelBinder());

            // Inicializar NLog
            LogManager.LoadConfiguration("NLog.config");
            LoggerManager.LogInfo("Aplicación iniciada");
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            LoggerManager.LogError(exception);
        }
    }
}