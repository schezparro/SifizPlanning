using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
//using System.Net;
//using System.Net.Mail;
using System.Web.Security;
using System.IO;
using System.Net.Mime;
using SifizPlanning.Models;
using SifizPlanning.Security;
using System.Security.Cryptography;
using QRCoder;
using System.Threading.Tasks;
using System.Data.Entity.Validation;
using System.Web.Hosting;
using Hangfire;

namespace SifizPlanning.Util
{
    public class Utiles
    {

        //private static HttpServerUtility server = System.Web.HttpContext.Current.Server;

        private static SifizPlanningEntidades db = DbCnx.getCnx();

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string UpperCamelCase(string text)
        {
            string[] palabras = text.Split(new Char[] { ' ' });
            string camelCase = "";
            foreach (string palabra in palabras)
            {
                camelCase += char.ToUpper(palabra[0]) + palabra.Substring(1);
            }
            return camelCase;
        }

        public static string PrimeraMayuscula(string text)
        {
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        public static string PrimeraMinuscula(string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }

        public static bool EnviarEmailSistema(string[] emailsDestinos, string emailBody, string asunto = "Información", string[] adjuntos = null, string qr = null)
        {
            // Usar la imagen corporativa fija en lugar del QR dinámico
            string logoCorporativo = "sifizsoft.png";
            string pathImagen = HostingEnvironment.MapPath("~/Web/images/email") + "/" + logoCorporativo;

            string htmlCss = @"<style>
                                           .textoCuerpo{
                                                font-size: 12pt;
                                                font-family: ""Century Gothic"", sans-serif;
                                                color: #353535;
                                           }
                                           .cabecera{
                                                font-size: 8pt;
                                                font-family: ""Century Gothic"", sans-serif;
                                                border-bottom: 1px solid #222;
                                           }                                                                              
                                           table td{
                                                width: 160px;
                                           }
                                           table th {
                                                border: 1px solid black;
                                                border-collapse: collapse;
                                                font-size: 8pt;
                                                background: #aaa;
                                                font-family: ""Century Gothic"", sans-serif;
                                           }
                                           table, td {
                                                border: 1px solid black;
                                                border-collapse: collapse;
                                                font-size: 8pt;
                                                background: #ccc;
                                                font-family: ""Century Gothic"", sans-serif;
                                                vertical-align: top;
                                            }
                                            th, td {
                                                padding: 10px;
                                            }

                                            /* Font Definitions */
                                            @font-face
	                                            {font-family:""Century Gothic"";
	                                            panose-1:2 11 5 2 2 2 2 2 2 4;}
                                            /* Style Definitions */
                                            p.MsoNormal, li.MsoNormal, div.MsoNormal
	                                            {margin:0cm;
	                                            margin-bottom:.0001pt;
	                                            font-size:12.0pt;
	                                            font-family:""Century Gothic"",sans-serif;
	                                            color:#353535;}
                                            a:link, span.MsoHyperlink
	                                            {mso-style-priority:99;
	                                            color:#0563C1;
	                                            text-decoration:underline;}
                                            a:visited, span.MsoHyperlinkFollowed
	                                            {mso-style-priority:99;
	                                            color:#954F72;
	                                            text-decoration:underline;}
                                            span.EstiloCorreo17
	                                            {mso-style-type:personal-compose;
	                                            font-family:""Century Gothic"",sans-serif;
	                                            color:#353535;}
                                            .MsoChpDefault
	                                            {mso-style-type:export-only;
	                                            font-family:""Century Gothic"",sans-serif;}
                                            @page WordSection1
	                                            {size:612.0pt 792.0pt;
	                                            margin:70.85pt 3.0cm 70.85pt 3.0cm;}
                                            div.WordSection1
	                                            {page:WordSection1;}
                                            .firma-principal {
                                                font-family: ""Century Gothic"", sans-serif;
                                                font-size: 12pt;
                                                color: #353535;
                                                text-align: left;
                                            }
                                            .firma-copyright {
                                                font-family: ""Century Gothic"", sans-serif;
                                                font-size: 9pt;
                                                color: #000035;
                                                text-align: center;
                                            }
                                       </style>";

            string htmlfirma = @"<div class=WordSection1>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Atentamente,<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Sifizplanning<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>SISTEMA PLANIFICADOR INTEGRAL SIFIZSOFT S.A.<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Telf. (593) 2-450-4616<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Quito - Ecuador<o:p></o:p></span>
                                    </p>";
            
            // Siempre incluir el logo corporativo
            htmlfirma += "<p class=MsoNormal align=center style='text-align:center'><img style='max-width: 100%; height: auto !important;'  src='cid:" + logoCorporativo + "'></p>";

            htmlfirma += @"<p class=MsoNormal><o:p>&nbsp;</o:p></p>
                              <p class=MsoNormal align=center style='text-align:center'>
                                  <span style='font-family:""Century Gothic"",sans-serif;font-size:9.0pt;color:#000035;'>Copyrights ©SifizSoft 2004-2025 carefully reserved and preserved<o:p></o:p></span>
                              </p>
                              <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                          </div>";

            // Siempre incluir el logo corporativo
            List<string> listImagenes = new List<string>();
            listImagenes.Add(logoCorporativo);
            string[] imagenes = listImagenes.ToArray();
            try
            {
                string email = System.Configuration.ConfigurationManager.AppSettings["emailApp"];
                string password = System.Configuration.ConfigurationManager.AppSettings["passwordEmailApp"];
                string htmlMail = htmlCss + emailBody + htmlfirma;

                // Siempre adjuntar la imagen corporativa
                Utiles.EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos, -1);
            }
            catch (Exception)
            {
                //Notificar error    
            }

            return true;
        }

        public static bool EnviarEmailSistemaPersonalizadoAsync(string[] emailsDestinos, string emailBody, string emailCSS, string asunto = "Información", string[] adjuntos = null, string qr = null)
        {
            // Usar la imagen corporativa fija en lugar del QR dinámico
            string logoCorporativo = "sifizsoft.png";
            string pathImagen = HostingEnvironment.MapPath("~/Web/images/email") + "/" + logoCorporativo;

            // Combinar CSS personalizado con CSS base corporativo
            string htmlCss = @"<style>
                                            body {
                                                font-family: ""Century Gothic"", sans-serif;
                                                font-size: 12pt;
                                                color: #353535;
                                                text-align: left;
                                            }
                                            .firma-copyright {
                                                font-family: ""Century Gothic"", sans-serif;
                                                font-size: 9pt;
                                                color: #000035;
                                                text-align: center;
                                            }
                                       </style>" + emailCSS;
            string htmlfirma = @"<div class=WordSection1>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Atentamente,<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Sifizplanning<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>SISTEMA PLANIFICADOR INTEGRAL SIFIZSOFT S.A.<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Telf. (593) 2-450-4616<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Quito - Ecuador<o:p></o:p></span>
                                    </p>";
            
            // Siempre incluir el logo corporativo
            htmlfirma += "<p class=MsoNormal align=center style='text-align:center'><img style='max-width: 100%; height: auto !important;'  src='cid:" + logoCorporativo + "'></p>";

            htmlfirma += @"<p class=MsoNormal><o:p>&nbsp;</o:p></p>
                              <p class=MsoNormal align=center style='text-align:center'>
                                  <span style='font-family:""Century Gothic"",sans-serif;font-size:9.0pt;color:#000035;'>Copyrights ©SifizSoft 2004-2025 carefully reserved and preserved<o:p></o:p></span>
                              </p>
                              <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                          </div>";

            // Siempre incluir el logo corporativo
            List<string> listImagenes = new List<string>();
            listImagenes.Add(logoCorporativo);
            string[] imagenes = listImagenes.ToArray();
            try
            {
                string email = System.Configuration.ConfigurationManager.AppSettings["emailApp"];
                string password = System.Configuration.ConfigurationManager.AppSettings["passwordEmailApp"];
                string htmlMail = htmlCss + emailBody + htmlfirma;

                // Siempre adjuntar la imagen corporativa con el parámetro -1
                Utiles.EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos, -1);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return true;
        }

        public static void EnviarEmail(string emailFuente, string[] emailsDestinos, string emailBody, string asunto = "Información", string password = "", bool adjuntarimagen = false, string[] imagenes = null, string[] adjuntos = null, int idCorreo = -1)
        {
            var email = new MimeMessage();
#pragma warning disable CS0618 // Type or member is obsolete
            email.From.Add(new MailboxAddress(emailFuente));
#pragma warning restore CS0618 // Type or member is obsolete
            email.Subject = asunto;

            foreach (string emailDestino in emailsDestinos)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                email.To.Add(new MailboxAddress(emailDestino));
#pragma warning restore CS0618 // Type or member is obsolete
            }
            var builder = new BodyBuilder();
            builder.HtmlBody = emailBody; // Siempre asignar el HTML body primero
            
            if (adjuntarimagen && imagenes != null && imagenes.Length > 0)
            {
                foreach (string imagen in imagenes)
                {
                    string[] path = new string[2] { HostingEnvironment.MapPath("~/Web/images/email"), imagen };
                    string imagePath = Path.Combine(path);
                    if (File.Exists(imagePath))
                    {
                        // Usar LinkedResources para embebidas (cid:) en lugar de Attachments
                        var linkedResource = builder.LinkedResources.Add(imagePath);
                        linkedResource.ContentId = imagen; // Esto permite usar cid:imagen en el HTML
                    }
                }
            }
            if (adjuntos != null && adjuntos.Length > 0)
            {
                foreach (var adj in adjuntos)
                {
                    builder.Attachments.Add(adj);
                }
            }
            email.Body = builder.ToMessageBody();

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("email.sifizsoft.com", 465, true);
                smtp.Authenticate(emailFuente, password);

                bool enviado = false;
                string mensaje = "";

                try
                {
                    smtp.Send(email);
                    enviado = true;
                }
                catch (Exception e)
                {
                    mensaje = e.Message;
                }

                smtp.Disconnect(true);

                // The rest of your code related to database operations and error handling remains the same.
                if (!enviado)
                {
                    //Si no se pudo enviar y es un correo nuevo, se guarda en la BD
                    if (idCorreo <= 0)
                    {
                        CorreoNoEnviado objCorreoNoEnviado = new CorreoNoEnviado()
                        {
                            EmailEnvia = emailFuente,
                            Password = EncriptacionSimetrica(password),
                            FechaHora = DateTime.Now,
                            Asunto = asunto,
                            Texto = emailBody.Length > 8000 ? emailBody.Substring(0, 8000) : emailBody,
                            EmailDestinos = emailsDestinos.ToString(),
                            AdjuntarImagen = adjuntarimagen,
                            Imagenes = imagenes.ToString(),
                            Adjuntos = adjuntos.ToString(),
                            MensajeError = mensaje,
                            EstaActivo = true
                        };

                        try
                        {
                            db.CorreoNoEnviado.Add(objCorreoNoEnviado);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw new Exception(mensaje);
                        }
                    }
                    BackgroundJob.Schedule(() => EnviarEmail(emailFuente, emailsDestinos, emailBody, asunto, password, adjuntarimagen, imagenes, adjuntos, idCorreo), TimeSpan.FromMinutes(20));
                }
                else
                {
                    //Si se pudo enviar y no es un correo nuevo, se elimina de la BD
                    if (idCorreo > 0)
                    {
                        CorreoNoEnviado objCorreoNoEnviado = db.CorreoNoEnviado.Find(idCorreo);
                        if (objCorreoNoEnviado != null)
                            objCorreoNoEnviado.EstaActivo = false;
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            throw new Exception(mensaje);
                        }

                    }
                }
            }
        }

        public static string darNombreTrabajador(int idTrabajador = 0)
        {
            if (idTrabajador > 0)
            {
                //SifizPlanningEntidades db = DbCnx.getCnx();
                var trabajador = (from t in db.Colaborador
                                  join p in db.Persona on t.persona equals p
                                  where t.Secuencial == idTrabajador
                                  select new
                                  {
                                      nombre = p.Nombre1 + " " + p.Apellido1
                                  }).SingleOrDefault();

                return trabajador.nombre;
            }
            return "";
        }

        public static string darFechaDMY(DateTime fecha)
        {
            return fecha.ToString("dd/MM/yyyy");
        }

        public static DateTime strToDateTime(string fecha)
        {
            try
            {
                string[] fechas = fecha.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);
                return new System.DateTime(anno, mes, dia);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static string CalcularHorasTarea(DateTime finicio, DateTime ffin)
        {
            TimeSpan tiempoTarea = ffin - finicio;
            int horas = tiempoTarea.Hours;

            if (finicio.Hour < 13 && ffin.Hour > 13)
            {
                horas--;
            }
            if (tiempoTarea.Minutes > 0)
            {
                horas++;
            }
            return (horas == 0) ? "" : (horas == 1) ? "(1 hora)" : "(" + horas + " horas)";
        }

        public static int DarHorasTarea(DateTime finicio, DateTime ffin)
        {
            TimeSpan tiempoTarea = ffin - finicio;
            int horas = tiempoTarea.Hours;

            if (finicio.Hour < 13 && ffin.Hour > 13)
            {
                horas--;
            }
            if (tiempoTarea.Minutes > 0)
            {
                horas++;
            }
            return horas;
        }

        public static string DarNombreMes(int pos)
        {
            string[] mes = new string[12] { "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };
            return mes[pos - 1];
        }

        public static string DarHorasMinutosPorMinutos(int minutos)
        {
            string strTiempo = "";
            int tiempo = minutos / 60;
            int tiempoMinuto = minutos % 60;
            if (tiempoMinuto < 10)
            {
                string strTiempoMinuto = "0" + tiempoMinuto.ToString();
                strTiempo = tiempo.ToString() + ":" + strTiempoMinuto;
            }
            else
            {
                strTiempo = tiempo.ToString() + ":" + tiempoMinuto.ToString();
            }
            return strTiempo;
        }

        /**
         *  Funcion que ordena de forma inteligente las tareas y  los permisos de un
         *  colaborador en un día determinado
         */
        public static void OrdenarTareasPermisos(DateTime fecha, int idColaborador, Usuario user, SifizPlanningEntidades db1 = null)
        {
            if (db1 == null)
            {
                db1 = db;
            }
            DateTime dia = fecha.Date;
            DateTime diaSiguiente = dia.AddDays(1);
            DateTime fechaFinDia = dia.AddMinutes(17 * 60 + 30);
            DateTime fechaInicioTareas = dia.AddMinutes(30 + (8 * 60));

            List<Tarea> tareasDia = (from t in db1.Tarea
                                     where t.FechaInicio >= fechaInicioTareas && t.FechaInicio < diaSiguiente &&
                                           t.SecuencialColaborador == idColaborador &&
                                           t.SecuencialEstadoTarea != 4 //Anulada
                                     orderby t.FechaInicio
                                     select t).ToList<Tarea>();

            List<Permiso> permisos = (from p in db1.Permiso
                                      where
                                           p.FechaInicio >= fechaInicioTareas && p.FechaInicio < diaSiguiente &&
                                           p.SecuencialColaborador == idColaborador &&
                                           p.SecuencialEstadoPermiso == 2//solo se ordenan las aprobadas
                                      orderby p.FechaInicio
                                      select p).ToList<Permiso>();

            //Ordenando las tareas
            int cant = tareasDia.Count;
            int i = 0;
            DateTime horaInicialTarea = dia.AddMinutes(8 * 60 + 30);
            while (i < cant)
            {
                Tarea tar = tareasDia[i];
                TimeSpan tiempoTarea = tar.FechaFin - tar.FechaInicio;
                int horasTarea = tiempoTarea.Hours;
                int minutosTarea = tiempoTarea.Minutes;
                if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                {
                    horasTarea--;
                }

                tar.FechaInicio = horaInicialTarea;
                tar.FechaFin = horaInicialTarea.AddHours(horasTarea).AddMinutes(minutosTarea);

                if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                {
                    tar.FechaFin = tar.FechaFin.AddHours(1);
                }
                else if (tar.FechaInicio.Hour == 13)
                {
                    tar.FechaInicio = tar.FechaInicio.AddHours(1);
                    tar.FechaFin = tar.FechaFin.AddHours(1);
                }

                horaInicialTarea = tar.FechaFin;
                i++;
            }

            //Ordenando los permisos
            int cantP = permisos.Count;
            i = 0;
            int j = 0;
            while (i < cantP)
            {
                Permiso permiso = permisos[i];
                Tarea ultimaTareaAnalizada = null;
                j = 0;
                while (j < cant)
                {
                    Tarea tar = tareasDia[j];
                    if (
                            (permiso.FechaInicio <= tar.FechaInicio && permiso.FechaFin > tar.FechaInicio) ||
                            (permiso.FechaInicio < tar.FechaFin && permiso.FechaFin >= tar.FechaFin) ||
                            (permiso.FechaInicio >= tar.FechaInicio && permiso.FechaFin <= tar.FechaFin)
                        ) //El permiso afecta a la tarea, es decir se solapan de alguna forma
                    {
                        TimeSpan tiempoTarea = tar.FechaFin - tar.FechaInicio;
                        int horasTarea = tiempoTarea.Hours;
                        if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                        {
                            horasTarea--;
                        }

                        if (tar.FechaInicio >= permiso.FechaInicio)
                        {//Se ponen las tareas al final del permiso
                            tar.FechaInicio = permiso.FechaFin;
                            tar.FechaFin = tar.FechaInicio.AddHours(horasTarea);

                            if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                            {
                                tar.FechaFin = tar.FechaFin.AddHours(1);
                            }
                            else if (tar.FechaInicio.Hour == 13)
                            {
                                tar.FechaInicio = tar.FechaInicio.AddHours(1);
                                tar.FechaFin = tar.FechaFin.AddHours(1);
                            }

                            ultimaTareaAnalizada = tar;
                        }
                        else
                        {//Se pica en dos o más tareas
                            tar.FechaFin = permiso.FechaInicio;
                            int dif = horasTarea - (permiso.FechaInicio - tar.FechaInicio).Hours;

                            Tarea newTarea = new Tarea
                            {
                                SecuencialColaborador = tar.SecuencialColaborador,
                                SecuencialActividad = tar.SecuencialActividad,
                                SecuencialLugarTarea = tar.SecuencialLugarTarea,
                                SecuencialEstadoTarea = tar.SecuencialEstadoTarea,
                                SecuencialModulo = tar.SecuencialModulo,
                                SecuencialCliente = tar.SecuencialCliente,
                                Detalle = tar.Detalle,
                                FechaInicio = permiso.FechaFin,
                                FechaFin = permiso.FechaFin.AddHours(dif),
                                HorasUtilizadas = 0,
                                NumeroVerificador = 1
                            };

                            if (newTarea.FechaInicio.Hour < 13 && newTarea.FechaFin.Hour > 13)
                            {
                                newTarea.FechaFin = newTarea.FechaFin.AddHours(1);
                            }
                            else if (newTarea.FechaInicio.Hour == 13)
                            {
                                newTarea.FechaInicio = newTarea.FechaInicio.AddHours(1);
                                newTarea.FechaFin = newTarea.FechaFin.AddHours(1);
                            }

                            db1.Tarea.Add(newTarea);

                            //Verificando si tiene referencia de los motivos de trabajo
                            if (tar.entregableMotivoTrabajo != null)
                            {
                                newTarea.entregableMotivoTrabajo = tar.entregableMotivoTrabajo;
                            }

                            //Verificando si tiene coordinador
                            if (tar.tarea_coordinador != null)
                            {
                                db1.Tarea_Coordinador.Add(
                                    new Tarea_Coordinador
                                    {
                                        tarea = newTarea,
                                        SecuencialColaborador = tar.tarea_coordinador.SecuencialColaborador,
                                        EstaActivo = 1,
                                        NumeroVerificador = 1
                                    }
                                );
                            }

                            //El historico de los estados
                            HistoricoTareaEstado histET = new HistoricoTareaEstado
                            {
                                tarea = newTarea,
                                SecuencialEstadoTarea = newTarea.SecuencialEstadoTarea,
                                FechaOperacion = DateTime.Now,
                                SecuencialUsuario = user.Secuencial
                            };
                            db1.HistoricoTareaEstado.Add(histET);

                            //Ver si está relacionada
                            if (tar.tarea_tareaRelacionada1 != null)
                            {
                                Tarea_TareaRelacionada newTareaRelacionada = new Tarea_TareaRelacionada
                                {
                                    SecuencialTareaRelacionada = newTarea.Secuencial,
                                    SecuencialTarea = tar.tarea_tareaRelacionada1.SecuencialTarea,
                                    NumeroVerificador = 1
                                };
                                db1.Tarea_TareaRelacionada.Add(newTareaRelacionada);
                            }

                            ultimaTareaAnalizada = newTarea;
                        }

                        if (ultimaTareaAnalizada.FechaInicio > fechaFinDia)
                        {
                            ultimaTareaAnalizada.SecuencialEstadoTarea = 4;//Anulada
                        }
                        else if (ultimaTareaAnalizada.FechaFin > fechaFinDia)
                        {
                            ultimaTareaAnalizada.FechaFin = fechaFinDia;
                        }

                        j++;
                        while (j < cant)
                        {
                            tar = tareasDia[j];

                            tiempoTarea = tar.FechaFin - tar.FechaInicio;
                            horasTarea = tiempoTarea.Hours;
                            if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                            {
                                horasTarea--;
                            }

                            tar.FechaInicio = ultimaTareaAnalizada.FechaFin;
                            tar.FechaFin = tar.FechaInicio.AddHours(horasTarea);

                            if (tar.FechaInicio.Hour < 13 && tar.FechaFin.Hour > 13)
                            {
                                tar.FechaFin = tar.FechaFin.AddHours(1);
                            }
                            else if (tar.FechaInicio.Hour == 13)
                            {
                                tar.FechaInicio = tar.FechaInicio.AddHours(1);
                                tar.FechaFin = tar.FechaFin.AddHours(1);
                            }

                            ultimaTareaAnalizada = tar;
                            if (ultimaTareaAnalizada.FechaInicio > fechaFinDia)
                            {
                                ultimaTareaAnalizada.SecuencialEstadoTarea = 4;//Anulada
                            }
                            else if (ultimaTareaAnalizada.FechaFin > fechaFinDia)
                            {
                                ultimaTareaAnalizada.FechaFin = fechaFinDia;
                            }
                            j++;

                        }

                    }

                    j++;
                }
                i++;
            }

            db1.SaveChanges();
        }

        public static void AgregarTareaConReubicacion(Tarea nuevaTarea, SifizPlanningEntidades db1 = null)
        {
            try
            {
                if (db1 == null)
                {
                    db1 = db;
                }
                // Obtener el inicio y fin del día para la fecha de la nueva tarea
                DateTime inicioDelDia = nuevaTarea.FechaInicio.Date;
                DateTime finDelDia = inicioDelDia.AddDays(1).AddSeconds(-1);

                // Modificar la consulta
                DateTime finUltimaTarea = db1.Tarea
                    .Where(t => t.SecuencialColaborador == nuevaTarea.SecuencialColaborador &&
                                t.FechaInicio >= inicioDelDia &&
                                t.FechaInicio < finDelDia)
                    .OrderByDescending(t => t.FechaFin)
                    .FirstOrDefault()?.FechaFin ?? inicioDelDia.AddHours(17.5);

                // Buscar tareas existentes del colaborador que se solapen con la nueva tarea
                var tareasSolapadas = (from t in db1.Tarea
                                       where t.SecuencialColaborador == nuevaTarea.SecuencialColaborador &&
                                             t.SecuencialEstadoTarea != 4 && // Excluir tareas anuladas
                                             t.FechaInicio < nuevaTarea.FechaFin && t.FechaFin > nuevaTarea.FechaInicio
                                       select t).ToList();

                foreach (var tarea in tareasSolapadas)
                {
                    // Caso 1: Solapamiento Total
                    if (tarea.FechaInicio >= nuevaTarea.FechaInicio && tarea.FechaFin <= nuevaTarea.FechaFin)
                    {
                        Tarea tareaReubicada = ClonarSinSecuencial(tarea);
                        tareaReubicada.FechaInicio = finUltimaTarea;
                        tareaReubicada.FechaFin = finUltimaTarea.Add(tarea.FechaFin - tarea.FechaInicio);

                        db1.Tarea.Add(tareaReubicada);
                        finUltimaTarea = tareaReubicada.FechaFin;
                    }
                    // Caso 2: Solapamiento Parcial al Inicio
                    else if (tarea.FechaInicio < nuevaTarea.FechaInicio && tarea.FechaFin > nuevaTarea.FechaInicio && tarea.FechaFin <= nuevaTarea.FechaFin)
                    {
                        // Parte antes del solapamiento
                        Tarea tareaAntes = ClonarSinSecuencial(tarea);
                        tareaAntes.FechaInicio = tarea.FechaInicio;
                        tareaAntes.FechaFin = nuevaTarea.FechaInicio;
                        db1.Tarea.Add(tareaAntes);

                        // Parte dentro del solapamiento reubicada
                        Tarea tareaReubicada = ClonarSinSecuencial(tarea);
                        tareaReubicada.FechaInicio = finUltimaTarea;
                        tareaReubicada.FechaFin = finUltimaTarea.Add(tarea.FechaFin - nuevaTarea.FechaInicio);
                        db1.Tarea.Add(tareaReubicada);
                        finUltimaTarea = tareaReubicada.FechaFin;
                    }
                    // Caso 3: Solapamiento Parcial al Final
                    else if (tarea.FechaInicio >= nuevaTarea.FechaInicio && tarea.FechaInicio < nuevaTarea.FechaFin && tarea.FechaFin > nuevaTarea.FechaFin)
                    {
                        // Parte solapada al inicio
                        Tarea tareaReubicada = ClonarSinSecuencial(tarea);
                        tareaReubicada.FechaInicio = finUltimaTarea;
                        tareaReubicada.FechaFin = finUltimaTarea.Add(nuevaTarea.FechaFin - tarea.FechaInicio);
                        db1.Tarea.Add(tareaReubicada);
                        finUltimaTarea = tareaReubicada.FechaFin;

                        // Parte posterior al solapamiento
                        Tarea tareaDespues = ClonarSinSecuencial(tarea);
                        tareaDespues.FechaInicio = nuevaTarea.FechaFin;
                        tareaDespues.FechaFin = tarea.FechaFin;
                        db1.Tarea.Add(tareaDespues);
                    }
                    // Caso 4: Solapamiento Completamente Cubierto
                    else if (tarea.FechaInicio < nuevaTarea.FechaInicio && tarea.FechaFin > nuevaTarea.FechaFin)
                    {
                        // Parte antes del solapamiento
                        Tarea tareaAntes = ClonarSinSecuencial(tarea);
                        tareaAntes.FechaInicio = tarea.FechaInicio;
                        tareaAntes.FechaFin = nuevaTarea.FechaInicio;
                        db1.Tarea.Add(tareaAntes);

                        // Parte coincidente reubicada
                        Tarea tareaReubicada = ClonarSinSecuencial(tarea);
                        tareaReubicada.FechaInicio = finUltimaTarea;
                        tareaReubicada.FechaFin = finUltimaTarea.Add(nuevaTarea.FechaFin - nuevaTarea.FechaInicio);
                        db1.Tarea.Add(tareaReubicada);
                        finUltimaTarea = tareaReubicada.FechaFin;

                        // Parte después del solapamiento
                        Tarea tareaDespues = ClonarSinSecuencial(tarea);
                        tareaDespues.FechaInicio = nuevaTarea.FechaFin;
                        tareaDespues.FechaFin = tarea.FechaFin;
                        db1.Tarea.Add(tareaDespues);
                    }

                    // Marcar la tarea original como anulada
                    tarea.SecuencialEstadoTarea = 4;
                }

                // Agregar la nueva tarea
                db1.Tarea.Add(nuevaTarea);

                // Guardar todos los cambios en la base de datos
                db1.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        //public static void AgregarTareaConReubicacion(Tarea nuevaTarea, SifizPlanningEntidades db1 = null)
        //{
        //    try
        //    {
        //        if (db1 == null)
        //        {
        //            db1 = db;
        //        }
        //        // Obtener el inicio y fin del día para la fecha de la nueva tarea
        //        DateTime inicioDelDia = nuevaTarea.FechaInicio.Date;
        //        DateTime finDelDia = inicioDelDia.AddDays(1).AddSeconds(-1);

        //        // Modificar la consulta
        //        DateTime finUltimaTarea = db1.Tarea
        //            .Where(t => t.SecuencialColaborador == nuevaTarea.SecuencialColaborador &&
        //                        t.FechaInicio >= inicioDelDia &&
        //                        t.FechaInicio < finDelDia)
        //            .OrderByDescending(t => t.FechaFin)
        //            .FirstOrDefault()?.FechaFin ?? inicioDelDia.AddHours(17.5);

        //        // Buscar tareas existentes del colaborador que se solapen con la nueva tarea
        //        var tareasSolapadas = (from t in db1.Tarea
        //                               where t.SecuencialColaborador == nuevaTarea.SecuencialColaborador &&
        //                                     t.SecuencialEstadoTarea != 4 && // Excluir tareas anuladas
        //                                     t.FechaInicio < nuevaTarea.FechaFin && t.FechaFin > nuevaTarea.FechaInicio
        //                               select t).ToList();

        //        foreach (var tarea in tareasSolapadas)
        //        {
        //            // Caso 1: Solapamiento Total
        //            if (tarea.FechaInicio >= nuevaTarea.FechaInicio && tarea.FechaFin <= nuevaTarea.FechaFin)
        //            {
        //                Tarea tareaReubicada = new Tarea();
        //                tareaReubicada = tarea;
        //                tareaReubicada.Secuencial = 0;
        //                tareaReubicada.FechaInicio = finUltimaTarea;
        //                tareaReubicada.FechaFin = finUltimaTarea.Add(tarea.FechaFin - tarea.FechaInicio);

        //                db1.Tarea.Add(tareaReubicada);
        //                finUltimaTarea = tareaReubicada.FechaFin;
        //            }
        //            // Caso 2: Solapamiento Parcial al Inicio
        //            else if (tarea.FechaInicio < nuevaTarea.FechaInicio && tarea.FechaFin > nuevaTarea.FechaInicio && tarea.FechaFin <= nuevaTarea.FechaFin)
        //            {
        //                // Parte antes del solapamiento
        //                Tarea tareaAntes = new Tarea();
        //                tareaAntes = tarea;
        //                tareaAntes.Secuencial = 0;
        //                tareaAntes.FechaInicio = tarea.FechaInicio;
        //                tareaAntes.FechaFin = nuevaTarea.FechaInicio;
        //                db1.Tarea.Add(tareaAntes);

        //                // Parte dentro del solapamiento reubicada
        //                Tarea tareaReubicada = new Tarea();
        //                tareaReubicada = tarea;
        //                tareaReubicada.Secuencial = 0;
        //                tareaReubicada.FechaInicio = finUltimaTarea;
        //                tareaReubicada.FechaFin = finUltimaTarea.Add(tarea.FechaFin - nuevaTarea.FechaInicio);
        //                db1.Tarea.Add(tareaReubicada);
        //                finUltimaTarea = tareaReubicada.FechaFin;
        //            }
        //            // Caso 3: Solapamiento Parcial al Final
        //            else if (tarea.FechaInicio >= nuevaTarea.FechaInicio && tarea.FechaInicio < nuevaTarea.FechaFin && tarea.FechaFin > nuevaTarea.FechaFin)
        //            {
        //                // Parte solapada al inicio
        //                Tarea tareaReubicada = new Tarea();
        //                tareaReubicada = tarea;
        //                tareaReubicada.Secuencial = 0;
        //                tareaReubicada.FechaInicio = finUltimaTarea;
        //                tareaReubicada.FechaFin = finUltimaTarea.Add(nuevaTarea.FechaFin - tarea.FechaInicio);
        //                db1.Tarea.Add(tareaReubicada);
        //                finUltimaTarea = tareaReubicada.FechaFin;

        //                // Parte posterior al solapamiento
        //                Tarea tareaDespues = new Tarea();
        //                tareaDespues = tarea;
        //                tareaDespues.Secuencial = 0;
        //                tareaDespues.FechaInicio = nuevaTarea.FechaFin;
        //                tareaDespues.FechaFin = tarea.FechaFin;
        //                db1.Tarea.Add(tareaDespues);
        //            }
        //            // Caso 4: Solapamiento Completamente Cubierto
        //            else if (tarea.FechaInicio < nuevaTarea.FechaInicio && tarea.FechaFin > nuevaTarea.FechaFin)
        //            {
        //                // Parte antes del solapamiento
        //                Tarea tareaAntes = new Tarea();
        //                tareaAntes = tarea;
        //                tareaAntes.Secuencial = 0;
        //                tareaAntes.FechaInicio = tarea.FechaInicio;
        //                tareaAntes.FechaFin = nuevaTarea.FechaInicio;
        //                db1.Tarea.Add(tareaAntes);

        //                // Parte coincidente reubicada
        //                Tarea tareaReubicada = new Tarea();
        //                tareaReubicada = tarea;
        //                tareaReubicada.Secuencial = 0;
        //                tareaReubicada.FechaInicio = finUltimaTarea;
        //                tareaReubicada.FechaFin = finUltimaTarea.Add(nuevaTarea.FechaFin - nuevaTarea.FechaInicio);
        //                db1.Tarea.Add(tareaReubicada);
        //                finUltimaTarea = tareaReubicada.FechaFin;

        //                // Parte después del solapamiento
        //                Tarea tareaDespues = new Tarea();
        //                tareaDespues = tarea;
        //                tareaDespues.Secuencial = 0;
        //                tareaDespues.FechaInicio = nuevaTarea.FechaFin;
        //                tareaDespues.FechaFin = tarea.FechaFin;
        //                db1.Tarea.Add(tareaDespues);
        //            }

        //            // Marcar la tarea original como anulada
        //            tarea.SecuencialEstadoTarea = 4;
        //        }

        //        // Agregar la nueva tarea
        //        db1.Tarea.Add(nuevaTarea);

        //        // Guardar todos los cambios en la base de datos
        //        db1.SaveChanges();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}


        public static List<string> CorreoPorGrupoEmail(string codigoGrupoEmail)
        {
            var correos = (from p in db.Persona
                           join
                               pge in db.PersonaGrupoEmail on p.Secuencial equals pge.SecuencialPersona
                           join
                               ge in db.GrupoEmail on pge.SecuencialGrupoEmail equals ge.Secuencial
                           where ge.Codigo == codigoGrupoEmail &&
                                 ge.EstaActivo == 1 &&
                                 pge.EstaActivo == 1 &&
                                 p.usuario.FirstOrDefault().EstaActivo == 1
                           select (
                               p.usuario.FirstOrDefault().Email
                           )
                          ).Distinct().ToList();
            return correos;
        }

        public static DateTime fechaAtrasDiasLaborables(int cantDias)
        {
            try
            {
                DateTime fecha = DateTime.Today;
                int i = 0;
                while (i < cantDias)
                {
                    fecha = fecha.AddDays(-1);
                    if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (db.DiaInactivo.Where(x => x.Fecha == fecha).Count() == 0)
                        {
                            i++;
                        }
                    }
                }
                return fecha;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static DateTime[] diasSemanaFecha(DateTime fecha)
        {
            DateTime[] arregloFechas = new DateTime[2];
            int diaSemanaFecha = (int)fecha.DayOfWeek;
            int diaRestar = -1 * (diaSemanaFecha - 1);
            if (diaSemanaFecha == 0)
            {
                diaRestar = -6;
            }

            DateTime fechaLunes = fecha.AddDays(diaRestar);
            DateTime fechaDomingo = fechaLunes.AddDays(6);
            arregloFechas[0] = fechaLunes;
            arregloFechas[1] = fechaDomingo;
            return arregloFechas;
        }

        //Función para la carga de los Logs de errores del Financial en el servidor
        public static string generarTocken(string mac, string fecha, string cliente)
        {
            MD5 md5Hash = MD5.Create();
            string hash = GetMd5Hash(md5Hash, fecha + cliente + mac.Substring(4, 3));
            return hash;
        }
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        //Funcion para encriptar simetricamente
        static readonly string claveEncriptacionSimetrica = "lkjIQWUQOWjsdlkJASDIO12%&gksklER";
        static readonly string vectorEncriptacionSimetrica = "ljasdls@351$%d!3";
        public static string EncriptacionSimetrica(string cadena)
        {
            byte[] key = UTF8Encoding.UTF8.GetBytes(claveEncriptacionSimetrica);
            byte[] iv = UTF8Encoding.UTF8.GetBytes(vectorEncriptacionSimetrica);

            // Crear una instancia del algoritmo de Rijndael
            Rijndael RijndaelAlg = Rijndael.Create();

            // Establecer un flujo en memoria para el cifrado
            MemoryStream memoryStream = new MemoryStream();

            // Crear un flujo de cifrado basado en el flujo de los datos
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         RijndaelAlg.CreateEncryptor(key, iv),
                                                         CryptoStreamMode.Write);

            // Obtener la representación en bytes de la información a cifrar
            byte[] plainMessageBytes = UTF8Encoding.UTF8.GetBytes(cadena);

            // Cifrar los datos enviándolos al flujo de cifrado
            cryptoStream.Write(plainMessageBytes, 0, plainMessageBytes.Length);

            cryptoStream.FlushFinalBlock();

            // Obtener los datos datos cifrados como un arreglo de bytes
            byte[] cipherMessageBytes = memoryStream.ToArray();

            // Cerrar los flujos utilizados
            memoryStream.Close();
            cryptoStream.Close();

            // Retornar la representación de texto de los datos cifrados
            return Convert.ToBase64String(cipherMessageBytes);
        }
        public static string DesencriptacionSimetrica(string cadena)
        {
            byte[] key = UTF8Encoding.UTF8.GetBytes(claveEncriptacionSimetrica);
            byte[] iv = UTF8Encoding.UTF8.GetBytes(vectorEncriptacionSimetrica);

            // Obtener la representación en bytes del texto cifrado
            byte[] cipherTextBytes = Convert.FromBase64String(cadena);

            // Crear un arreglo de bytes para almacenar los datos descifrados
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            // Crear una instancia del algoritmo de Rijndael
            Rijndael RijndaelAlg = Rijndael.Create();

            // Crear un flujo en memoria con la representación de bytes de la información cifrada
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

            // Crear un flujo de descifrado basado en el flujo de los datos
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         RijndaelAlg.CreateDecryptor(key, iv),
                                                         CryptoStreamMode.Read);

            // Obtener los datos descifrados obteniéndolos del flujo de descifrado
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            // Cerrar los flujos utilizados
            memoryStream.Close();
            cryptoStream.Close();

            // Retornar la representación de texto de los datos descifrados
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        private static Tarea ClonarSinSecuencial(Tarea t)
        {
            // Clona la instancia actual sin el Secuencial y sin relaciones
            Tarea clon = new Tarea
            {
                SecuencialColaborador = t.SecuencialColaborador,
                SecuencialActividad = t.SecuencialActividad,
                SecuencialLugarTarea = t.SecuencialLugarTarea,
                SecuencialEstadoTarea = t.SecuencialEstadoTarea,
                SecuencialModulo = t.SecuencialModulo,
                SecuencialCliente = t.SecuencialCliente,
                Detalle = t.Detalle,
                FechaInicio = t.FechaInicio,
                FechaFin = t.FechaFin,
                HorasUtilizadas = t.HorasUtilizadas,
                NumeroVerificador = t.NumeroVerificador,
                TiempoEstimacion = t.TiempoEstimacion,
                EsReproceso = t.EsReproceso
            };

            return clon;
        }

        /// <summary>
        /// Normaliza una fecha para evitar problemas de zona horaria
        /// Convierte cualquier fecha a la zona horaria del servidor (Ecuador)
        /// </summary>
        /// <param name="fecha">Fecha a normalizar</param>
        /// <returns>Fecha normalizada en zona horaria del servidor</returns>
        public static DateTime NormalizarFecha(DateTime fecha)
        {
            // Si la fecha tiene información de zona horaria, convertir a local
            if (fecha.Kind == DateTimeKind.Utc)
            {
                return fecha.ToLocalTime().Date;
            }
            else if (fecha.Kind == DateTimeKind.Unspecified)
            {
                // Asumir que es una fecha local y tomar solo la parte de fecha
                return fecha.Date;
            }
            else
            {
                // Ya es local, tomar solo la parte de fecha
                return fecha.Date;
            }
        }

        /// <summary>
        /// Normaliza una fecha nullable para evitar problemas de zona horaria
        /// </summary>
        /// <param name="fecha">Fecha nullable a normalizar</param>
        /// <returns>Fecha normalizada en zona horaria del servidor o null</returns>
        public static DateTime? NormalizarFecha(DateTime? fecha)
        {
            return fecha.HasValue ? NormalizarFecha(fecha.Value) : (DateTime?)null;
        }
    }
}