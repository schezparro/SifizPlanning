using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.IO;
using System.Web.Script.Serialization;

using SifizPlanning.Models;
using SifizPlanning.Util;
using SpreadsheetLight;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;

using SifizPlanning.Security;

using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace SifizPlanning
{
    public class Websocket : Hub
    {

        private static Websocket instance = null;

        public Websocket()
        {
            instance = this;
        }

        public static Websocket getInstance()
        {
            if (instance == null)
            {
                instance = new Websocket();
            }
            return instance;
        }

        SifizPlanningEntidades db = DbCnx.getCnx();
        public void ActualizarTareas()
        {
            /*
            DateTime now = DateTime.Now;
            Clients.All.actTareas( x );
            */
            int semana = 2;
            DateTime lunes = DateTime.Today;
            DateTime hoy = DateTime.Today;
            DayOfWeek diaSemana = hoy.DayOfWeek;
            long tiempo = (int)diaSemana;
            TimeSpan time = new TimeSpan(tiempo * 864000000000);
            DateTime domingo = hoy.Subtract(time);
            lunes = domingo.AddDays(1);

            DateTime fechaFin = lunes.AddDays(7 * semana);
            var datos = (from t in db.Tarea
                         join
                             c in db.Colaborador on t.colaborador equals c
                         join
                             p in db.Persona on c.persona equals p
                         join
                             u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                         join
                             f in db.FotoColaborador on c.Secuencial equals f.SecuencialColaborador
                         join
                             s in db.Sede on c.sede equals s
                         join
                             m in db.Modulo on t.modulo equals m
                         join
                             cl in db.Cliente on t.cliente equals cl
                         join
                             a in db.Actividad on t.actividad equals a
                         join
                             e in db.EstadoTarea on t.estadoTarea equals e
                         join
                             l in db.LugarTarea on t.lugarTarea equals l
                         join d in db.Departamento on c.departamento equals d

                         where u.EstaActivo == 1 && d.Asignable == 1 &&
                               t.FechaInicio > lunes && t.FechaFin < fechaFin && //Entre las dos fechas
                               t.SecuencialEstadoTarea != 4//Es cuando están anuladas
                         orderby t.FechaInicio, p.Nombre1, p.Apellido1
                         select new
                         {
                             idColaborador = c.Secuencial,
                             nombre = p.Nombre1 + " " + p.Apellido1,
                             email = u.Email.ToUpper(),
                             sede = s.Codigo,
                             url = f.Url,
                             idTarea = t.Secuencial,
                             sdetalle = t.Detalle.Substring(0, 20) + "...",
                             detalle = t.Detalle,
                             finicio = t.FechaInicio,
                             ffin = t.FechaFin,
                             modulo = m.Codigo,
                             idModulo = m.Secuencial,
                             cliente = cl.Codigo,
                             idCliente = cl.Secuencial,
                             dCliente = cl.Descripcion.ToUpper(),
                             actividad = a.Codigo,
                             dActividad = a.Descripcion.ToUpper(),
                             estado = e.Codigo,
                             idEstado = e.Secuencial,
                             lugar = l.Codigo,
                             idLugar = l.Secuencial,
                             clase = (t.SecuencialEstadoTarea == 1 ? "new" : (t.SecuencialEstadoTarea == 2) ? "dev" : "finish"),
                             coordinador = (from tc in db.Tarea_Coordinador
                                            join
                                                co in db.Colaborador on tc.colaborador equals co
                                            join
                                                pe in db.Persona on co.persona equals pe
                                            where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
                                            select (pe.Nombre1 + " " + pe.Apellido1)).FirstOrDefault()
                         }).ToList();

            var trabajadores = (from t in db.Colaborador
                                join
                                    p in db.Persona on t.persona equals p
                                join
                                    f in db.FotoColaborador on t.Secuencial equals f.SecuencialColaborador
                                join
                                    s in db.Sede on t.sede equals s
                                join
                                    u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                join d in db.Departamento on t.departamento equals d

                                where u.EstaActivo == 1 && d.Asignable == 1
                                orderby u.Email

                                select new
                                {
                                    id = t.Secuencial,
                                    nombre = p.Nombre1 + " " + p.Apellido1,
                                    email = u.Email.ToUpper(),
                                    sede = s.Codigo,
                                    url = f.Url
                                }).ToList();

            List<Object> tareasProgramadores = new List<Object>();
            int cant = trabajadores.Count();
            for (int i = 0; i < cant; i++)
            {
                int idTrabajador = trabajadores[i].id;
                List<Object> tareasPorDia = new List<Object>();
                int countTareas = 0;
                for (int j = 0; j < 7 * semana; j++)//son 7 Días los de la semana
                {
                    DateTime fecha = lunes.AddDays(j);
                    DateTime fechaDespues = lunes.AddDays(j + 1);
                    var tareas = (from d in datos
                                  where d.idColaborador == idTrabajador &&
                                        d.finicio > fecha && d.ffin < fechaDespues
                                  select new
                                  {
                                      id = d.idTarea,
                                      sdetalle = d.sdetalle,
                                      detalle = d.detalle,
                                      finicio = d.finicio.ToString("t"),
                                      ffin = d.ffin.ToString("t"),
                                      modulo = d.modulo,
                                      idModulo = d.idModulo,
                                      cliente = d.cliente,
                                      idCliente = d.idCliente,
                                      dCliente = d.dCliente,
                                      actividad = d.actividad,
                                      dActividad = d.dActividad,
                                      estado = d.estado,
                                      idEstado = d.idEstado,
                                      lugar = d.lugar,
                                      idLugar = d.idLugar,
                                      clase = d.clase,
                                      coordinador = d.coordinador
                                  }).ToList();

                    countTareas += tareas.Count();

                    string claseDia = "dia-normal";
                    long diaSemana1 = (int)fecha.DayOfWeek;
                    if (fecha == DateTime.Today)
                    {
                        claseDia = "dia-hoy";
                    }
                    else if (diaSemana1 == 0 || diaSemana1 == 6)
                    {
                        claseDia = "fin-semana";
                    }

                    tareasPorDia.Add(
                        new
                        {
                            tareas = tareas,
                            claseDia = claseDia
                        }
                    );
                }

                var trab = new
                {
                    trab = trabajadores[i],
                    tareasPorDia = tareasPorDia
                };
                tareasProgramadores.Add(trab);
            }

            Clients.All.actTareas(JsonConvert.SerializeObject(tareasProgramadores));
        }

        public void AsignacionesSemana(string password, string fechaLunes = "", string jsonCC = "")
        {
            string emailFuente = System.Configuration.ConfigurationManager.AppSettings["emailApp"];
            string passwordSifiz = System.Configuration.ConfigurationManager.AppSettings["passwordEmailApp"];

            DateTime lunes = DateTime.Today;
            if (fechaLunes != "")
            {
                string[] fechas = fechaLunes.Split(new Char[] { '/' });
                int dia = Int32.Parse(fechas[0]);
                int mes = Int32.Parse(fechas[1]);
                int anno = Int32.Parse(fechas[2]);
                lunes = new System.DateTime(anno, mes, dia);
            }
            else
            {
                DateTime hoy = DateTime.Today;
                DayOfWeek diaSemana = hoy.DayOfWeek;
                long tiempo = (int)diaSemana;
                TimeSpan time = new TimeSpan(tiempo * 864000000000);
                DateTime domingo = hoy.Subtract(time);
                lunes = domingo.AddDays(1);
            }

            DateTime fechaFin = lunes.AddDays(7);
            var datos = (from t in db.Tarea
                         join
                             c in db.Colaborador on t.colaborador equals c
                         join
                             p in db.Persona on c.persona equals p
                         join
                             u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                         join
                             f in db.FotoColaborador on c.Secuencial equals f.SecuencialColaborador
                         join
                             s in db.Sede on c.sede equals s
                         join
                             m in db.Modulo on t.modulo equals m
                         join
                             cl in db.Cliente on t.cliente equals cl
                         join
                             a in db.Actividad on t.actividad equals a
                         join
                             e in db.EstadoTarea on t.estadoTarea equals e
                         join
                             l in db.LugarTarea on t.lugarTarea equals l
                         join d in db.Departamento on c.departamento equals d

                         where u.EstaActivo == 1 && d.Asignable == 1 &&
                               t.FechaInicio > lunes && t.FechaFin < fechaFin && //Entre las dos fechas
                               t.SecuencialEstadoTarea != 4//Es cuando están anuladas
                         orderby t.FechaInicio, p.Nombre1, p.Apellido1
                         select new
                         {
                             idColaborador = c.Secuencial,
                             nombre = p.Nombre1 + " " + p.Apellido1,
                             email = u.Email.ToUpper(),
                             sede = s.Codigo,
                             url = f.Url,
                             idTarea = t.Secuencial,
                             sdetalle = t.Detalle.Substring(0, 20) + "...",
                             detalle = t.Detalle,
                             finicio = t.FechaInicio,
                             ffin = t.FechaFin,
                             modulo = m.Codigo,
                             idModulo = m.Secuencial,
                             cliente = cl.Codigo,
                             idCliente = cl.Secuencial,
                             dCliente = cl.Descripcion.ToUpper(),
                             actividad = a.Codigo,
                             dActividad = a.Descripcion.ToUpper(),
                             estado = e.Codigo,
                             idEstado = e.Secuencial,
                             lugar = l.Codigo,
                             idLugar = l.Secuencial,
                             clase = (t.SecuencialEstadoTarea == 1 ? "new" : (t.SecuencialEstadoTarea == 2) ? "dev" : "finish"),
                             idCoordinador = (from tc in db.Tarea_Coordinador
                                              where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
                                              select tc.SecuencialColaborador).FirstOrDefault(),
                             coordinador = (from tc in db.Tarea_Coordinador
                                            join
                                                co in db.Colaborador on tc.colaborador equals co
                                            join
                                                pe in db.Persona on co.persona equals pe
                                            where tc.SecuencialTarea == t.Secuencial && tc.EstaActivo == 1
                                            select (pe.Nombre1 + " " + pe.Apellido1)).FirstOrDefault()
                         }).ToList();

            var trabajadores = (from t in db.Colaborador
                                join
                                    p in db.Persona on t.persona equals p
                                join
                                    f in db.FotoColaborador on t.Secuencial equals f.SecuencialColaborador
                                join
                                    s in db.Sede on t.sede equals s
                                join
                                    u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                join d in db.Departamento on t.departamento equals d

                                where u.EstaActivo == 1 && d.Asignable == 1
                                orderby u.Email
                                select new
                                {
                                    id = t.Secuencial,
                                    nombre = p.Nombre1 + " " + p.Apellido1,
                                    nombre1 = p.Nombre1,
                                    apellido1 = p.Apellido1,
                                    sexo = p.Sexo,
                                    email = u.Email.ToLower(),
                                    sede = s.Codigo,
                                    url = f.Url
                                }).ToList();

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
                                       </style>";

            string emailUser = HttpContext.Current.User.Identity.Name;
            var envia = (from t in db.Colaborador
                         join
                             p in db.Persona on t.persona equals p
                         join
                             u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                         join
                             c in db.Cargo on t.cargo equals c
                         where u.Email == emailUser
                         select new
                         {
                             nombre1 = p.Nombre1.ToLower(),
                             apellido1 = p.Apellido1.ToLower(),
                             cargo = c.Descripcion.ToLower()
                         }).FirstOrDefault();

            string htmlfirma = @"<div class=WordSection1>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Atentamente,<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal>
                                        <i><span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>"
                            +
                            Utiles.UpperCamelCase(envia.nombre1) + " " + Utiles.UpperCamelCase(envia.apellido1)
                            +
                            @"<o:p></o:p></span></i>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>"
                            +
                            Utiles.PrimeraMayuscula(envia.cargo)
                            +
                            @"<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>SISTEMA PLANIFICADOR INTEGRAL SIFIZSOFT S.A.<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Telf. (593) 2-450-4616<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:12.0pt;color:#353535;'>Quito - Ecuador<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal align=center style='text-align:center'><img style='max-width: 100%; height: auto !important;'  src='cid:sifizsoft.png'></p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                    <p class=MsoNormal align=center style='text-align:center'>
                                        <span style='font-family:""Century Gothic"",sans-serif;font-size:9.0pt;color:#000035;'>Copyrights ©SifizSoft 2004-2026 carefully reserved and preserved<o:p></o:p></span>
                                    </p>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p>
                                </div>";

            string[] imagenes = new string[1] { "sifizsoft.png" };

            try
            {
                int cantCorreosEnviados = 0;
                string[] dias = new string[7] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
                int cant = trabajadores.Count();
                for (int i = 0; i < cant; i++)
                {
                    string htmlMail = "";
                    List<string> HtmlDiasTarea = new List<string>();
                    int idTrabajador = trabajadores[i].id;
                    var trabajador = trabajadores[i];
                    for (int j = 0; j < 7; j++)//son 7 Días los de la semana
                    {
                        string htmlDia = "";
                        DateTime fecha = lunes.AddDays(j);
                        DateTime fechaDespues = lunes.AddDays(j + 1);
                        var tareas = (from d in datos
                                      where d.idColaborador == idTrabajador &&
                                            d.finicio > fecha && d.ffin < fechaDespues
                                      select new
                                      {
                                          id = d.idTarea,
                                          sdetalle = d.sdetalle,
                                          detalle = d.detalle,
                                          fechaInicio = d.finicio,
                                          fechaFin = d.ffin,
                                          finicio = d.finicio.ToString("t"),
                                          ffin = d.ffin.ToString("t"),
                                          modulo = d.modulo,
                                          idModulo = d.idModulo,
                                          cliente = d.cliente,
                                          idCliente = d.idCliente,
                                          dCliente = d.dCliente,
                                          actividad = d.actividad,
                                          dActividad = d.dActividad,
                                          estado = d.estado,
                                          idEstado = d.idEstado,
                                          lugar = d.lugar,
                                          idLugar = d.idLugar,
                                          clase = d.clase,
                                          coordinador = d.coordinador
                                      }).ToList();

                        //Aquí están las tareas del día
                        int it = 1;
                        foreach (var tarea in tareas)
                        {
                            string strTiempoTarea = Utiles.CalcularHorasTarea(tarea.fechaInicio, tarea.fechaFin);

                            htmlDia += "<div class=\"cabecera\"><b>" + it + ". " + tarea.lugar + " (" + tarea.dCliente + ") " + strTiempoTarea + "</b></div>";
                            htmlDia += "<div>" + Utiles.PrimeraMayuscula(tarea.detalle.ToLower()) + "</div>";

                            if (tarea.coordinador != null)
                            {
                                htmlDia += "<div>Coordinar con: " + tarea.coordinador + "";
                            }

                            htmlDia += "<br/><br/>";

                            it++;
                        }

                        HtmlDiasTarea.Add(htmlDia);
                    }

                    htmlMail += @"<table>
                                    <thead>
                                        <th>&nbsp;</th>";
                    for (int k = 0; k < 7; k++)
                    {
                        htmlMail += "<th>" + "<div style=\"text-align: center;\"><b>" + dias[k] + lunes.AddDays(k).ToString(" dd") + "</b><div>" + "</th>";
                    }
                    htmlMail += "</thead><tr><td>" + trabajador.nombre + " (" + trabajador.sede + ")" + "</td>";

                    foreach (string htmlDia in HtmlDiasTarea)
                    {
                        htmlMail += "<td>" + htmlDia + "</td>";
                    }
                    htmlMail += "</tr></table> <br/>";

                    //Verificando si es coordinador, seleccionando colaboradores de los cuales es coordinador
                    var coordinados = (from d in datos
                                       where d.idCoordinador == idTrabajador
                                       orderby d.idColaborador
                                       select d).ToList();
                    if (coordinados.Count() > 0)//Aquí es coordinador
                    {
                        int pos = 0;
                        var coordinado = coordinados[pos];
                        int idCoordinado = coordinado.idColaborador;
                        while (pos < coordinados.Count())
                        {
                            HtmlDiasTarea.Clear();
                            int l = 0;
                            while (l < 7)//Por cada dia de la semana
                            {
                                DateTime fechaC = lunes.AddDays(l);
                                DateTime fechaCDespues = lunes.AddDays(l + 1);

                                var tareasC = (from c in coordinados
                                               where c.idColaborador == idCoordinado &&
                                                     c.finicio > fechaC && c.ffin < fechaCDespues
                                               select new
                                               {
                                                   id = c.idTarea,
                                                   sdetalle = c.sdetalle,
                                                   detalle = c.detalle,
                                                   fechaInicio = c.finicio,
                                                   fechaFin = c.ffin,
                                                   finicio = c.finicio.ToString("t"),
                                                   ffin = c.ffin.ToString("t"),
                                                   modulo = c.modulo,
                                                   idModulo = c.idModulo,
                                                   cliente = c.cliente,
                                                   idCliente = c.idCliente,
                                                   dCliente = c.dCliente,
                                                   actividad = c.actividad,
                                                   dActividad = c.dActividad,
                                                   estado = c.estado,
                                                   idEstado = c.idEstado,
                                                   lugar = c.lugar,
                                                   idLugar = c.idLugar,
                                                   clase = c.clase,
                                                   coordinador = c.coordinador
                                               }).ToList();
                                l++;

                                //Aquí están las tareas del día
                                string htmlDiaC = "";
                                int itc = 1;
                                foreach (var tarea in tareasC)
                                {
                                    string strTiempoTareaC = Utiles.CalcularHorasTarea(tarea.fechaInicio, tarea.fechaFin);

                                    htmlDiaC += "<div class=\"cabecera\"><b>" + itc + ". " + tarea.lugar + " (" + tarea.dCliente + ") " + strTiempoTareaC + "</b></div>";
                                    htmlDiaC += "<div>" + Utiles.PrimeraMayuscula(tarea.detalle.ToLower()) + "</div>";

                                    if (tarea.coordinador != null)
                                    {
                                        htmlDiaC += "<div>Coordinar con: " + tarea.coordinador + "";
                                    }

                                    htmlDiaC += "<br/><br/>";

                                    itc++;
                                }

                                HtmlDiasTarea.Add(htmlDiaC);

                            }

                            htmlMail += @"<table>
                                    <thead>
                                        <th>&nbsp;</th>";
                            for (int k = 0; k < 7; k++)
                            {
                                htmlMail += "<th>" + "<div style=\"text-align: center;\"><b>" + dias[k] + lunes.AddDays(k).ToString(" dd") + "</b><div>" + "</th>";
                            }
                            htmlMail += "</thead><tr><td>" + coordinado.nombre + " (" + coordinado.sede + ")" + "</td>";

                            foreach (string htmlDia in HtmlDiasTarea)
                            {
                                htmlMail += "<td>" + htmlDia + "</td>";
                            }
                            htmlMail += "</tr></table> <br/>";

                            //corriendo el coordinador
                            int posI = pos;
                            while (pos < coordinados.Count() && coordinados[posI].idColaborador == idCoordinado)
                            {
                                pos++;
                                if (pos < coordinados.Count() && idCoordinado != coordinados[pos].idColaborador)
                                {
                                    idCoordinado = coordinados[pos].idColaborador;
                                    coordinado = coordinados[pos];
                                }
                            }
                        }
                    }

                    string saludo = "Estimado " + Utiles.UpperCamelCase(trabajador.nombre1.ToLower()) + " " + Utiles.UpperCamelCase(trabajador.apellido1.ToLower());
                    if (trabajador.sexo == "F")
                    {
                        saludo = "Estimada " + Utiles.UpperCamelCase(trabajador.nombre1.ToLower()) + " " + Utiles.UpperCamelCase(trabajador.apellido1.ToLower());
                    }

                    htmlMail = "<div class=\"textoCuerpo\">" + saludo + ",<br/>Para su información, esta es la asignación para la semana del " + lunes.ToString("dd/MM/yyyy") + ". Si hay algún cambio se lo haremos conocer inmediatamente.<p><br/>" + htmlMail;

                    htmlMail += @"1. “En aquellas asignaciones que se requiera de la firma de acta, generación de informe, o cualquier documento por favor hacerlo en el día solicitado, el no enviar lo requerido dejará como no concluido su trabajo de ese o de los días asignados para la tarea correspondiente. Si lo solicitado se mantiene en alguna herramienta o repositorio en la nube o en nuestros servidores bastará con adjuntar el link y las credenciales de acceso al mismo. Agradecemos su cumplimiento.”<br/> 
                                  2. “Estimado técnico, en aquellas tareas en las que se ha definido un Coordinador por favor antes de realizar el trabajo comunicarse con dicho coordinador para que se explique con total detalle el trabajo que se requiere, si el coordinador por alguna razón no puede responder o solventar su requerimiento de información por favor comunicarse con Santiago Álvarez. Es responsabilidad de los Coordinadores designados detallar y dar seguimiento a las tareas de los técnicos que se les ha asignado y sobre todo llegar a determinar que cada tarea se ha resuelto con solvencia de ser posible de manera formal con un informe o acta.”<br/><br/>
                                  Gracias por su ayuda.</div>";

                    htmlMail = htmlCss + htmlMail;

                    //Enviando el correo                    
                    htmlMail += htmlfirma;
                    string asunto = "Asignaciones Semana " + lunes.ToString("dd/MM");

                    //string[] emailDestinos = new string[] { "rcespedes@sifizsoft.com" };//Borrar Aqui
                    string[] emailDestinos = new string[1] { trabajador.email };

                    Utiles.EnviarEmail(emailFuente, emailDestinos, htmlMail, asunto, passwordSifiz, true, imagenes, null, -1);
                    
                    if (true)
                    {
                        cantCorreosEnviados++;

                        var emailEnviado = new
                        {
                            terminado = false,
                            error = false,
                            cantidad = cant,
                            cantEnviados = cantCorreosEnviados,
                            msg = "Enviado email, " + cantCorreosEnviados + " de " + cant + "."
                        };

                        Clients.Caller.actualizarCorreoEnviado(JsonConvert.SerializeObject(emailEnviado));
                    }                    
                }

                var creandoExcel = new
                {
                    terminado = false,
                    error = false,
                    cantidad = cant,
                    cantEnviados = cantCorreosEnviados,
                    msg = "Creando el excel de asignaciones."
                };

                Clients.Caller.actualizarCorreoEnviado(JsonConvert.SerializeObject(creandoExcel));

                //--------- Creando el excel ----------
                string dirExcel = AppDomain.CurrentDomain.BaseDirectory + "Web\\files\\asignaciones\\";
                string nombreExcel = "asignaciones-" + lunes.ToString("ddMMyyyy") + ".xlsx";
                nombreExcel = Path.Combine(dirExcel, nombreExcel);
                SLDocument excel = new SLDocument();
                //Definicion de estilos
                SLStyle cabeceraStyle = excel.CreateStyle();
                cabeceraStyle.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.DimGray, System.Drawing.Color.DarkSalmon);
                cabeceraStyle.Font.Bold = true;
                cabeceraStyle.Font.FontColor = System.Drawing.Color.White;
                cabeceraStyle.SetHorizontalAlignment(HorizontalAlignmentValues.Center);

                SLStyle colaboradoresName1 = excel.CreateStyle();
                colaboradoresName1.SetVerticalAlignment(VerticalAlignmentValues.Center);
                colaboradoresName1.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                colaboradoresName1.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Lavender, System.Drawing.Color.DarkSalmon);

                SLStyle colaboradoresName2 = excel.CreateStyle();
                colaboradoresName2.SetVerticalAlignment(VerticalAlignmentValues.Center);
                colaboradoresName2.SetHorizontalAlignment(HorizontalAlignmentValues.Center);
                colaboradoresName2.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Gainsboro, System.Drawing.Color.DarkSalmon);

                SLStyle celdaDatoStyle1 = excel.CreateStyle();
                celdaDatoStyle1.SetWrapText(true);
                celdaDatoStyle1.SetVerticalAlignment(VerticalAlignmentValues.Top);
                celdaDatoStyle1.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Lavender, System.Drawing.Color.DarkSalmon);

                SLStyle celdaDatoStyle2 = excel.CreateStyle();
                celdaDatoStyle2.SetWrapText(true);
                celdaDatoStyle2.SetVerticalAlignment(VerticalAlignmentValues.Top);
                celdaDatoStyle2.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Gainsboro, System.Drawing.Color.DarkSalmon);

                SLStyle celdaDatoStyle3 = excel.CreateStyle();
                celdaDatoStyle3.SetWrapText(true);
                celdaDatoStyle3.SetVerticalAlignment(VerticalAlignmentValues.Top);
                celdaDatoStyle3.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.LightYellow, System.Drawing.Color.DarkSalmon);

                for (int fila = 1; fila <= cant + 1; fila++)
                {
                    var colaborador = trabajadores[0];
                    if (fila > 1)
                        colaborador = trabajadores[fila - 2];

                    for (int columna = 1; columna <= 8; columna++)
                    {
                        string value = "";
                        if (fila == 1)
                        {
                            if (columna == 1)
                            {
                                value = "Colaborador";
                            }
                            else
                            {
                                value = dias[columna - 2] + " " + lunes.AddDays(columna - 2).ToString("dd/MM");
                            }

                            excel.SetCellStyle(fila, columna, cabeceraStyle);
                        }
                        else
                        {
                            if (columna == 1)
                            {
                                value = colaborador.nombre + "\n";
                                value += "(" + colaborador.sede + ")";
                                if (fila % 2 == 0)
                                {
                                    excel.SetCellStyle(fila, columna, colaboradoresName1);
                                }
                                else
                                {
                                    excel.SetCellStyle(fila, columna, colaboradoresName2);
                                }

                            }
                            else
                            {
                                DateTime fecha = lunes.AddDays(columna - 2);
                                DateTime fechaDespues = lunes.AddDays(columna - 1);
                                var tareas = (from d in datos
                                              where d.idColaborador == colaborador.id &&
                                                    d.finicio > fecha && d.ffin < fechaDespues
                                              select new
                                              {
                                                  id = d.idTarea,
                                                  sdetalle = d.sdetalle,
                                                  detalle = d.detalle,
                                                  fechaInicio = d.finicio,
                                                  fechaFin = d.ffin,
                                                  finicio = d.finicio.ToString("t"),
                                                  ffin = d.ffin.ToString("t"),
                                                  modulo = d.modulo,
                                                  idModulo = d.idModulo,
                                                  cliente = d.cliente,
                                                  idCliente = d.idCliente,
                                                  dCliente = d.dCliente,
                                                  actividad = d.actividad,
                                                  dActividad = d.dActividad,
                                                  estado = d.estado,
                                                  idEstado = d.idEstado,
                                                  lugar = d.lugar,
                                                  idLugar = d.idLugar,
                                                  clase = d.clase,
                                                  coordinador = d.coordinador
                                              }).ToList();

                                bool fueraOficina = false;
                                //Aquí están las tareas del día
                                int it = 1;
                                foreach (var tarea in tareas)
                                {
                                    string strTiempoTarea = Utiles.CalcularHorasTarea(tarea.fechaInicio, tarea.fechaFin); ;

                                    value += it + ". " + tarea.lugar + " (" + tarea.dCliente + ") " + strTiempoTarea + "\n";
                                    value += Utiles.PrimeraMayuscula(tarea.detalle.ToLower()) + "\n";

                                    if (tarea.coordinador != null)
                                    {
                                        value += "Coordinar con: " + tarea.coordinador;
                                    }

                                    string colaboradorOficina = "OF" + colaborador.sede;
                                    if (colaboradorOficina != tarea.lugar)
                                    {
                                        fueraOficina = true;
                                    }

                                    value += "\n";

                                    it++;
                                }

                                if (fueraOficina)
                                {
                                    excel.SetCellStyle(fila, columna, celdaDatoStyle3);
                                }
                                else if (fila % 2 == 0)
                                {
                                    excel.SetCellStyle(fila, columna, celdaDatoStyle1);
                                }
                                else
                                {
                                    excel.SetCellStyle(fila, columna, celdaDatoStyle2);
                                }
                            }
                        }

                        excel.SetCellValue(fila, columna, value);
                        excel.SetColumnWidth(columna, 23);
                        excel.FreezePanes(1, 8);
                    }
                }

                excel.SaveAs(nombreExcel);
                //--------- Fin de la creacion del excel ---------

                //--------- Enviando email del excel--------------
                var enviandoExcel = new
                {
                    terminado = false,
                    error = false,
                    cantidad = cant,
                    cantEnviados = cantCorreosEnviados,
                    msg = "Enviando el excel de asignaciones."
                };

                //Buscando los directivos en BD
                List<string> listaEmailDestinos = (from p in db.Persona
                                                   join u in db.Usuario on p.Secuencial equals u.SecuencialPersona
                                                   join pge in db.PersonaGrupoEmail on p.Secuencial equals pge.SecuencialPersona
                                                   join ge in db.GrupoEmail on pge.grupoEmail equals ge
                                                   where ge.Codigo == "ASGG" && ge.EstaActivo == 1 && u.EstaActivo == 1 && pge.EstaActivo == 1
                                                   select u.Email).ToList<string>();

                // Correos adicionales soliitados por Rossy
                listaEmailDestinos.Add("galvarez@sifizsoft.com");
                listaEmailDestinos.Add("asistenterrhh@sifizsoft.com");

                // Evitar duplicados
                listaEmailDestinos = listaEmailDestinos.Distinct().ToList();


                var s = new JavaScriptSerializer();
                var jsonObj = s.Deserialize<dynamic>(jsonCC);
                foreach (string email in jsonObj)
                {
                    listaEmailDestinos.Add(email);
                }

                string[] directivos = listaEmailDestinos.ToArray();
                //string[] directivos = new string[]{//Poner los directivos //borrar aqui
                //    "rcespedes@sifizsoft.com"
                //};

                Clients.Caller.actualizarCorreoEnviado(JsonConvert.SerializeObject(enviandoExcel));

                string emailDirectivos = @"<div class=""textoCuerpo"">Estimados,<br/> Adjunto las asignaciones planificadas para la semana del " + lunes.ToString("dd/MM") + ".</div>";
                emailDirectivos = htmlCss + emailDirectivos;
                emailDirectivos += htmlfirma;

                string asuntoEmailDirectivo = "Asignaciones planificadas para la semana del " + lunes.ToString("dd/MM") + ".";
                string[] adjuntos = new string[] { nombreExcel };

                //Envío a directivos                
                Utiles.EnviarEmail(emailFuente, directivos, emailDirectivos, asuntoEmailDirectivo, passwordSifiz, true, imagenes, adjuntos, -1);
                
                //--------- Fin de enviando email del excel--------------

                string msg = "Se han enviado todos los correos";
                bool error = false;
                if (cant != cantCorreosEnviados)
                {
                    error = true;
                    msg = "No se han podido enviar todos los correos, se han enviado " + cantCorreosEnviados + " de " + cant + ".";
                }

                var finEnvioEmail = new
                {
                    terminado = true,
                    error = error,
                    cantidad = cant,
                    cantEnviados = cantCorreosEnviados,
                    msg = msg
                };

                Clients.Caller.actualizarCorreoEnviado(JsonConvert.SerializeObject(finEnvioEmail));

            }
            catch (Exception e)
            {
                var errorEnvioEmail = new
                {
                    terminado = true,
                    error = true,
                    //msg = "ha ocurrido un error en el envío de los correos.",
                    msg = e.Message
                };

                Clients.Caller.actualizarCorreoEnviado(JsonConvert.SerializeObject(errorEnvioEmail));
            }
        }

        public void ActualizarTDTareas(List<object> listaObjetos)
        {
            string json = JsonConvert.SerializeObject(listaObjetos);
            Clients.All.actualizarDiaTareaColaborador(json);
        }

        //Para el chat
        public void ExistenCambios(int idDestino)
        {
            Clients.All.existenMensajes(idDestino);
        }

        //Para la actualizacion de los comentarios
        public void NuevosComentarios()
        {
            Clients.All.recargarPanelComentarios();
        }

        //Para reproducir un sonido cuando se agrega un comentario "Muy Importante"
        public void NuevoComentarioMuyImportante()
        {
            Clients.All.nuevoComentarioMuyImportante();
        }

        //Notificacion de error en el envío del ticket
        public void NotificarErrorEnvioEmail()
        {
            Clients.Caller.erroresEnvioEmail( "Se produjeron errores en el envío de email, por favor gestiónelos." );
        }
    }
}