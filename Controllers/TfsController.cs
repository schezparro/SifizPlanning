//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.IO;
//using System.Reflection;
//using System.Data.Entity.Core.Objects;
//using System.Web.Script.Serialization;
//using SifizPlanning.Models;
//using SifizPlanning.Util;
//using SifizPlanning.Security;
//using System.Net.Http;
//using System.Net.Http.Headers;
////using System.Net.Http.Json;

//namespace SifizPlanning.Controllers
//{
//    public class TfsController : Controller
//    {
//        TFSWarehouseEntities db = DbCnx.getCnxTfs();

//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public async System.Threading.Tasks.Task<ActionResult> DevuelveProyectos()
//        {

//            var personalaccesstoken = "jgidodbzkzvf5kzvhk5up3nozhknfcmicndn5ewi4v5qcn2lwlea";

//            using (HttpClient client = new HttpClient())
//            {
//                client.DefaultRequestHeaders.Accept.Add(
//                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

//                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
//                    Convert.ToBase64String(
//                        System.Text.ASCIIEncoding.ASCII.GetBytes(
//                            string.Format("{0}:{1}", "", personalaccesstoken))));

//                using (HttpResponseMessage response = client.GetAsync(
//                            "https://app.vssps.visualstudio.com/_apis/profile/profiles/me?api-version=6.0").Result)
//                {
//                    response.EnsureSuccessStatusCode();
//                    string responseBody = await response.Content.ReadAsStringAsync();


                    
//                }
//            }
//            try
//            {
//                //var proyectos = db.Database.SqlQuery<string>("select dtp.ProjectNodeName as [Nombre] from DimTeamProject dtp where dtp.ProjectNodeType=0").Select(p => new { nombre = p }).ToList<object>();
//                var proyectos = db.DimTeamProject.Where(p => p.ProjectNodeType == 0 && p.IsDeleted == false).Select(s => new { id = s.ProjectNodeSK, nombre = s.ProjectNodeName, path = s.ProjectPath }).ToList().OrderBy(o => o.nombre);
//                var proyectosAll = proyectos.Select(s => new
//                {
//                    id = s.id,
//                    nombre = s.nombre,
//                    path = stringBetween(s.path, @"\", @"\")
//                }).ToList();
//                var resp = new
//                {
//                    success = true,
//                    proyectos = proyectosAll
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult DevuelveUsuarios()
//        {
//            try
//            {
//                var usuariosAll = (from u in db.DimPerson
//                                   select new
//                                   {
//                                       nombre = u.Name,
//                                       alias = u.Alias
//                                   }).OrderBy(o => o.nombre).ToList();

//                var usuarios = usuariosAll.Where(s => s.alias.Any(c => char.IsUpper(c)) == false && s.nombre.Contains("TEAM FOUNDATION") == false).ToList();
//                //var usuarios = db.Database.SqlQuery<string>("select dp.Name as [Nombre] from DimPerson dp where dp.Alias like '%.%'").Select(u => new { usuario = u }).ToList<object>();
//                var resp = new
//                {
//                    success = true,
//                    usuarios = usuarios
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult DevuelveCambios(int proyecto = 0, string usuario = "", string fechaInicio = "", string fechaFin = "", string filaInicio = "1", string filaFinal = "50", bool mostrarComentariosConFormatoIncorrecto = false)
//        {
//            //string userId = Request.LogonUserIdentity.User.Value;
//            //string a = Session.SessionID;
//            bool a = Profile.IsAnonymous;
//            try
//            {
//                string consulta = "SELECT  * FROM (SELECT ROW_NUMBER() OVER ( ORDER BY d.Date, dtp.ProjectNodeName desc ) AS RowNum, dtp.ProjectNodeName as [Proyecto], SUBSTRING(dtp.ProjectPath, 2, 13) as [Financial], dp.Name as [Usuario], d.date as [Fecha], dcs.ChangesetTitle as [Comentario],dcs.ChangesetID as [ConjuntoCambiosID], count(dcs.ChangesetID) as [CantidadCambios] FROM  FactCodeChurn f INNER JOIN DimDate d ON f.DateSK = d.DateSK INNER JOIN DimTeamProject dtp ON f.TeamProjectSK = dtp.ProjectNodeSK INNER JOIN DimFile df ON f.FilenameSK = df.FileSK INNER JOIN DimChangeset dcs ON f.ChangesetSK = dcs.ChangesetSK INNER JOIN DimPerson dp ON dcs.CheckedInBySK = dp.PersonSK";
//                bool contieneWhere = false;
//                if (proyecto != 0)
//                {
//                    consulta += " where dtp.ProjectNodeSK='" + proyecto + "'";
//                    contieneWhere = true;
//                }
//                if (usuario != string.Empty)
//                {
//                    if (contieneWhere)
//                    {
//                        consulta += " and dp.Name='" + usuario + "'";
//                    }
//                    else
//                    {
//                        consulta += " where dp.Name='" + usuario + "'";
//                        contieneWhere = true;
//                    }
//                }
//                if (fechaInicio != string.Empty && fechaFin != string.Empty)
//                {
//                    if (contieneWhere)
//                    {
//                        consulta += " AND d.Date between  CAST(N'" + fechaInicio + "' AS DATE)  AND  CAST(N'" + fechaFin + "' AS DATE)";
//                    }
//                    else
//                    {
//                        consulta += " WHERE d.Date between  CAST(N'" + fechaInicio + "' AS DATE)  AND  CAST(N'" + fechaFin + "' AS DATE)";
//                        contieneWhere = true;
//                    }
//                }
//                if (mostrarComentariosConFormatoIncorrecto)
//                {
//                    if (contieneWhere)
//                    {
//                        consulta += " AND dcs.ChangesetTitle not like '%***TK:%***%'";
//                    }
//                    else
//                    {
//                        consulta += " WHERE dcs.ChangesetTitle not like '%***TK:%***%'";
//                    }
//                }
//                consulta += " GROUP BY dtp.ProjectNodeName,dtp.ProjectPath, dp.Name,d.Date,dcs.ChangesetTitle,dcs.ChangesetID) AS RowConstrainedResult";
//                var cambios = db.Database.SqlQuery<ConjuntoCambios>(consulta).OrderByDescending(o => DateTime.ParseExact(o.Fecha, "dd/MM/yyyy", null)).ToList<ConjuntoCambios>();
//                //Actualizando el estilo de las filas
//                cambios = cambios.Skip(int.Parse(filaInicio)).Take(int.Parse(filaFinal)).ToList<ConjuntoCambios>();
//                foreach (ConjuntoCambios cambio in cambios)
//                {
//                    //if (filaSeleccionada.Contains(cambio.Proyecto + cambio.ConjuntoCambiosID))
//                    //    cambio.Estilo = "tuplaSelected";
//                    //else
//                    cambio.Estilo = string.Empty;
//                }
//                var resp = new
//                {
//                    success = true,
//                    cambios = cambios
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult DevuelveCantidadTotalConjuntoCambios(int proyecto = 0, string usuario = "", string fechaInicio = "", string fechaFin = "")
//        {
//            try
//            {
//                string consulta = "SELECT count(*) cantidad from (SELECT dtp.ProjectNodeName as [Proyecto],dp.Name as [Usuario], d.date as [Fecha], dcs.ChangesetTitle as [Comentario],dcs.ChangesetID as [ConjuntoCambiosID] FROM  FactCodeChurn f INNER JOIN DimDate d ON f.DateSK = d.DateSK INNER JOIN DimTeamProject dtp ON f.TeamProjectSK = dtp.ProjectNodeSK INNER JOIN DimFile df ON f.FilenameSK = df.FileSK INNER JOIN DimChangeset dcs ON f.ChangesetSK = dcs.ChangesetSK INNER JOIN DimPerson dp ON dcs.CheckedInBySK = dp.PersonSK";
//                bool contieneWhere = false;
//                if (proyecto != 0)
//                {
//                    consulta += " where dtp.ProjectNodeSK='" + proyecto + "'";
//                    contieneWhere = true;
//                }
//                if (usuario != string.Empty)
//                {
//                    if (contieneWhere)
//                    {
//                        consulta += " and dp.Name='" + usuario + "'";
//                    }
//                    else
//                    {
//                        consulta += " where dp.Name='" + usuario + "'";
//                        contieneWhere = true;
//                    }
//                }
//                if (fechaInicio != string.Empty && fechaFin != string.Empty)
//                {
//                    if (contieneWhere)
//                    {
//                        consulta += " AND d.Date between  CAST(N'" + fechaInicio + "' AS DATE)  AND  CAST(N'" + fechaFin + "' AS DATE)";
//                    }
//                    else
//                    {
//                        consulta += " WHERE d.Date between  CAST(N'" + fechaInicio + "' AS DATE)  AND  CAST(N'" + fechaFin + "' AS DATE)";
//                    }
//                }
//                consulta += " GROUP BY dtp.ProjectNodeName,dp.Name,d.Date,dcs.ChangesetTitle,dcs.ChangesetID) AS cantidad";
//                var cantidadCambios = db.Database.SqlQuery<int>(consulta).FirstOrDefault<int>();
//                var resp = new
//                {
//                    success = true,
//                    cantidadTotalCambios = cantidadCambios
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult DevuelveConjuntoCambios(int proyecto, string financial, string usuario, string fecha, string conjuntoCambiosID)
//        {
//            try
//            {
//                string consulta = "SELECT df.FileName as [Archivo],df.FilePath as [CaminoArchivo],";
//                consulta += " CASE v.Command WHEN 2 THEN 'Edición' WHEN 5 THEN 'Adición Nueva Carpeta' WHEN 6 THEN 'Edición Archivo' WHEN 7 THEN 'Adición Nuevo Archivo' WHEN 16 THEN 'Borrado' WHEN 32 THEN 'Deshacer Borrado' WHEN 34 THEN 'Deshacer Borrado, Editar' WHEN 64 THEN 'Nueva Rama' WHEN 68 THEN 'Rama' WHEN 70 THEN 'Rama, Edit' WHEN 84 THEN 'Rama, Borrar' WHEN 128 THEN 'Mezclar' WHEN 130 THEN 'Mezclar, Editar' WHEN 134 THEN 'Mezclar,Escribir, Editar' WHEN 144 THEN 'Mezclar, Borrar' WHEN 192 THEN 'Mezclar Rama, Editar'";
//                consulta += " WHEN 196 THEN 'Mezclar Rama' WHEN 198 THEN 'Mezclar Rama, Editar' WHEN 212 THEN 'Mezclar Rama, Borrar' WHEN 261 THEN 'Bloquear' WHEN 1024 THEN 'Renombrar' WHEN 1029 THEN 'Adicionar Carpeta, Renombrar' WHEN 1031 THEN 'Adicionar ArchivoRenombrar' WHEN 1040 THEN 'Borrar, Renombrar' WHEN 1168 THEN 'Mezclar, Borrar, Renombrar' WHEN 2112 THEN 'Renombrar' WHEN 2114 THEN 'Renombrar, Editar' WHEN 2128 THEN 'Borrar, Renombrar' WHEN 2240 THEN 'Mezclar, Renombrar' WHEN 2242 THEN 'Mezclar, Renombrar, Editar' WHEN 3136 THEN 'Renombrar, Renombrar Fuentes' WHEN 3138 THEN 'Renombrar, Editar, Renombrar Funetes' WHEN 3152 THEN 'Borrar, Renombrar, Renombrar Fuentes' END as [Comando],";
//                consulta += " 'http://186.5.29.66:8080/tfs/web/diff.aspx?pcguid='+'b8fe503f-84df-4f0c-baa8-ada4dfa030f1&opath='+(replace(replace(df.FilePath,'$','%24'),'/','%2f'))+ '&ocs='+ convert(varchar,dcs.ChangesetId-1)+'&mpath='+replace(replace(df.FilePath,'$','%24'),'/','%2f') + '&mcs='+convert(varchar,dcs.ChangesetId) as [CaminoTfs] FROM  FactCodeChurn f INNER JOIN DimDate d ON f.DateSK = d.DateSK INNER JOIN DimTeamProject dtp ON f.TeamProjectSK = dtp.ProjectNodeSK INNER JOIN DimFile df ON f.FilenameSK = df.FileSK INNER JOIN DimChangeset dcs ON f.ChangesetSK = dcs.ChangesetSK INNER JOIN DimPerson dp ON dcs.CheckedInBySK = dp.PersonSK INNER JOIN Tfs_" + financial + ".dbo.tbl_Version v on v.ItemId = dcs.ChangeSetId";
//                consulta += " WHERE dtp.ProjectNodeSK = '" + proyecto + "' and dp.Name = '" + usuario + "' and d.Date = CAST(N'" + fecha + "' AS DATE) and dcs.ChangesetID =" + conjuntoCambiosID;
//                consulta += " Order by df.FileName";

//                var cambios = db.Database.SqlQuery<Cambio>(consulta).ToList<Cambio>();
//                var resp = new
//                {
//                    success = true,
//                    cambios = cambios
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        [HttpPost]
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult GuardarFilaSeleccionada(string filaId)
//        {
//            try
//            {
//                //if (!filaSeleccionada.Contains(filaId))
//                //    filaSeleccionada.Add(filaId);
//                var resp = new
//                {
//                    success = true
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        //Pruebas de conexión al TFS mediante los servicios REST api
//        //[HttpPost, HttpGet]        
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult ConsultaTFS(string filaId)
//        {
//            try
//            {
//                //ClientTfs.SampleREST();
//                //ClientTfs.AdicionarTrabajo("Financial2013", "ScrumSFZ", "Task");
//                var resp = new
//                {
//                    success = true
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }

//        //[HttpPost, HttpGet]        
//        [Authorize(Roles = "ADMIN,SOPORTE")]
//        public ActionResult DevuelveAreaProyecto(int start, int lenght, string filtro = "")
//        {
//            try
//            {

//                var proyectos = db.DimTeamProject.Where(p => p.ProjectNodeSK != p.ParentNodeSK && p.ProjectNodeName != null && p.IsDeleted == false).Select(s => new
//                {
//                    area = s.ProjectNodeName,
//                    proyecto = db.DimTeamProject.Where(t => t.ProjectNodeSK == s.ParentNodeSK).FirstOrDefault().ProjectNodeName
//                }).ToList().OrderBy(o => o.proyecto).AsEnumerable();

//                if (filtro != "")
//                {
//                    proyectos = proyectos.Where(x =>
//                                                x.proyecto.ToString().ToUpper().Contains(filtro.ToUpper()) ||
//                                                x.area.ToString().ToUpper().Contains(filtro.ToUpper())
//                                               ).ToList();
//                }

//                int total = proyectos.Count();
//                proyectos = proyectos.Skip(start).Take(lenght).ToList();

//                var resp = new
//                {
//                    success = true,
//                    total = total,
//                    proyectos = proyectos
//                };
//                return Json(resp);
//            }
//            catch (Exception e)
//            {
//                var resp = new
//                {
//                    success = false,
//                    msg = e.Message
//                };
//                return Json(resp);
//            }
//        }
//        public static string stringBetween(string Source, string Start, string End)
//        {
//            string result = "";
//            if (Source.Contains(Start) && Source.Contains(End))
//            {
//                int StartIndex = Source.IndexOf(Start, 0) + Start.Length;
//                int EndIndex = Source.IndexOf(End, StartIndex);
//                result = Source.Substring(StartIndex, EndIndex - StartIndex);
//                return result;
//            }

//            return result;
//        }
//    }

//    public class ConjuntoCambios
//    {
//        public ConjuntoCambios()
//        {

//        }
//        public string Proyecto
//        {
//            get;
//            set;
//        }
//        public string Financial
//        {
//            get;
//            set;
//        }
//        public string Usuario
//        {
//            get;
//            set;
//        }
//        public string Fecha
//        {
//            get;
//            set;
//        }

//        public string Comentario
//        {
//            get;
//            set;
//        }

//        public int ConjuntoCambiosID
//        {
//            get;
//            set;
//        }

//        public int CantidadCambios
//        {
//            get;
//            set;
//        }

//        public string Estilo
//        {
//            get;
//            set;
//        }
//    }

//    public class Cambio
//    {
//        public Cambio()
//        {

//        }

//        public string Archivo
//        {
//            get;
//            set;
//        }

//        public string CaminoArchivo
//        {
//            get;
//            set;
//        }

//        public string Comando
//        {
//            get;
//            set;
//        }

//        public string CaminoTfs
//        {
//            get;
//            set;
//        }
//    }
//}