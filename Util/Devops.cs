using System;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using SifizPlanning.Models;
using System.Diagnostics;

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

                var requestBody = new AsociacionRequest
                {
                    Organizacion = dap.Organizacion,
                    NombreUsuario = dap.NombreUsuario,
                    Usuario = dap.Usuario,
                    Modulo = dap.Modulo,
                    EsTCK = dap.EsTck,
                    EsCTR = dap.EsReq,
                    EsDEV = dap.EsDev,
                    SerieTicket = dap.EsTck ? dap.SerieTicket.ToString() : "",
                    SerieContrato = dap.EsReq ? dap.SerieRequerimiento.ToString() : "",
                    SerieDesarrollo = dap.EsDev ? dap.SerieDesarrollo.ToString() + "-" + dap.SerieTicket.ToString() : "",
                    Identificador = dap.SecuencialTarea.ToString()
                };

                client.DefaultRequestHeaders.Add("X-API-KEY", key);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsJsonAsync(requestUrl, requestBody);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error al dar acceso Devops: {e.Message}");
                return false;
            }
        }

        public static async Task<bool> QuitarAccesoDevops(string identificador)
        {
            try
            {
                string key = ConfigurationManager.AppSettings.Get("Devops");
                var client = new HttpClient();
                var requestUrl = "https://api-sifizops.sifizsoft.com/api/AsignacionPermisos/DisociarPermiso";

                var requestBody = new
                {
                    Identificador = identificador
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
                Debug.WriteLine($"Error al quitar acceso Devops: {e.Message}");
                return false;
            }
        }
    }
    public class AsociacionRequest
    {
        public string Organizacion { get; set; }
        public string NombreUsuario { get; set; }
        public string Usuario { get; set; }
        public string Modulo { get; set; }
        public bool EsTCK { get; set; } = false;
        public bool EsCTR { get; set; } = false;
        public bool EsDEV { get; set; } = false;
        public string SerieTicket { get; set; }
        public string SerieDesarrollo { get; set; }
        public string SerieContrato { get; set; }
        public string Identificador { get; set; }
    }
}