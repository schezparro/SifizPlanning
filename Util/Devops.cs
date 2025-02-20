using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using SifizPlanning.Models;

namespace SifizPlanning.Util
{
    public class Devops
    {

        public static async Task<bool> DarAccesoDevops(DevopsAccesoProyectos dap)
        {
            try
            {
                string key = ConfigurationManager.AppSettings.Get("Devops");
                var client = new HttpClient();
                var requestUrl = "https://api-sifizops.sifizsoft.com/api/AsignacionPermisos/AsociarPermiso";

                if (dap == null)
                {
                    throw new Exception("No se encontró la configuración de acceso para la tarea especificada");
                }

                var requestBody = new
                {
                    organizacion = dap.Organizacion,
                    nombreUsuario = dap.NombreUsuario,
                    usuario = dap.Usuario,
                    modulo = dap.Modulo,
                    esTCK = dap.EsTck,
                    esREQ = dap.EsReq,
                    esDEV = dap.EsDev,
                    serieTicket = dap.SerieTicket.ToString(),
                    serieRequerimiento = dap.SerieRequerimiento.ToString(),
                    serieDesarrollo = dap.SerieDesarrollo.ToString(),
                    identificador = dap.SecuencialTarea.ToString()
                };

                client.DefaultRequestHeaders.Add("X-API-KEY", key);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync(requestUrl, requestBody);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al dar acceso Devops: {e.Message}", e);
            }
        }

        public static async Task<bool> QuitarAccesoDevops(int identificador)
        {
            try
            {
                string key = ConfigurationManager.AppSettings.Get("Devops");
                var client = new HttpClient();
                var requestUrl = "https://api-sifizops.sifizsoft.com/api/AsignacionPermisos/DisociarPermiso";

                var requestBody = new
                {
                    identificador = identificador.ToString()
                };

                client.DefaultRequestHeaders.Add("X-API-KEY", key);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync(requestUrl, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al quitar acceso Devops: {e.Message}", e);
            }
        }
    }
    public class AccesoProyectoDtoRequest
    {
        public string Organizacion { get; set; }
        public string NombreUsuario { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }
        public bool EsTCK { get; set; }
        public bool EsREQ { get; set; }
        public bool EsDEV { get; set; }
        public string SerieTicket { get; set; }
        public string SerieRequerimiento { get; set; }
        public string SerieDesarrollo { get; set; }
        public string Identificador { get; set; }
    }
}