using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Reflection;
using System.Data.Entity.Core.Objects;
using System.Web.Script.Serialization;
using SifizPlanning.Models;
using SifizPlanning.Util;
using SifizPlanning.Security;
using Newtonsoft.Json;

using SifizPlanning.Controllers;
using System.Globalization;

namespace SifizPlanning.Controllers
{
	public class ServiciosController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();

		[HttpPost]
		public ActionResult AdicionarLogEventosFBS(string json, string tocken, HttpPostedFileBase foto = null)
		{
			try
			{
				//json = "{\"evento\":{\"cliente\":\"COOPERATIVA DE AHORRO Y CREDITO CORPORACION CENTRO LTDA.\",\"empresa\":\"COOPERATIVA DE AHORRO Y CREDITO CORPORACION CENTRO LTDA.\",\"oficina\":\"LA ROLDOS\",\"fechaMaquina\":\"18/03/2016 13:55:51\",\"fechaSistema\":\"18/03/2016 00:00:00\",\"usuario\":\"CIFUENTES RODRIGUEZ EDISON MAURICIO\",\"equipo\":{\"ip\":\"192.168.101.2\",\"nombre\":\"CAJA-ROLDOS\",\"mac\":\"E840F205CA39\"},\"rutaMenu\":\"Transacciones>Transacciones\",\"mensajeDisculpa\":\"Disculpe hubo un error en Financial Business System v2.0\",\"mensajeError\":\"ORA-00001: restricción única (FBS_LOGS.SALIDAUSUARIO_IX) violada\\n\",\"mensajeExcepcion\":\"ORA-00001: restricción única (FBS_LOGS.SALIDAUSUARIO_IX) violada\\n\",\"nombreModulo\":\"System.Data.OracleClient.dll\",\"nombreFichero\":null,\"nombreClase\":\"System.Data.OracleClient.OracleConnection\",\"nombreMetodo\":\"CheckError\",\"linea\":\"0\",\"columna\":\"0\",\"stackTrace\":\" at System.Data.OracleClient.OracleConnection.CheckError(OciErrorHandle errorHandle, Int32 rc)\\r\\n at System.Data.OracleClient.OracleCommand.Execute(OciStatementHandle statementHandle, CommandBehavior behavior, Boolean needRowid, OciRowidDescriptor& rowidDescriptor, ArrayList& resultParameterOrdinals)\\r\\n at System.Data.OracleClient.OracleCommand.ExecuteNonQueryInternal(Boolean needRowid, OciRowidDescriptor& rowidDescriptor)\\r\\n at System.Data.OracleClient.OracleCommand.ExecuteNonQuery()\\r\\n at Microsoft.Practices.EnterpriseLibrary.Data.Database.DoExecuteNonQuery(DbCommand command)\\r\\n at Microsoft.Practices.EnterpriseLibrary.Data.Database.ExecuteNonQuery(DbCommand command)\\r\\n at Logs.SalidaUsuarioDALC.Guardar(SalidaUsuario salidaUsuario) in f:\\V2013\\CorpCentro\\FBSServidor\\FBSCore\\Logs\\SalidaUsuario_DALC.cs:line 296\\r\\n at Logs.SalidaUsuarioActor.ProcesaGuardar(String codigoUsuario, String maquina, DateTime fechaSistema, DateTime fechaHoraServidor) in f:\\V2013\\CorpCentro\\FBSServidor\\FBSCore\\Logs\\SalidaUsuario_ActorNegocio.cs:line 34\\r\\n at FBSServicios.Logs.SalidaUsuarioWS.RegistraSalida(RegistraSalidaME mensajeEntrada) in f:\\V2013\\CorpCentro\\FBSServidor\\FBSServicios\\Logs\\SalidaUsuarioWS.cs:line 51\",\"codigoError\":null,\"publicacion\":\"2.0.0.42\",\"maquinaPublicacion\":\"CN=UIO-VERSIONES\\Administrador\",\"servidor\":null,\"baseDatos\":null,\"propagarError\":\"propagar-all\"}}";
				dynamic dataJson = JsonConvert.DeserializeObject<dynamic>(json);

				string mac = dataJson.evento.equipo.mac;
				string strFechaMaquina = dataJson.evento.fechaMaquina;
				string cliente = dataJson.evento.cliente;

				string ourTocken = Utiles.generarTocken(mac, strFechaMaquina, cliente);

				if(tocken != ourTocken)
				{
					throw new Exception("El tocken no es correcto");
				}

				string ip = dataJson.evento.equipo.ip;
				string nombre = dataJson.evento.equipo.nombre;

				//Guardando a la bd el evento
				FBSLogEquipo equipo = db.FBSLogEquipo.Where(x =>
										x.Ip == ip &&
										x.Mac == mac &&
										x.Nombre == nombre
									  ).FirstOrDefault();
				if(equipo == null)
				{
					equipo = new FBSLogEquipo
					{
						Ip = ip,
						Mac = mac,
						Nombre = nombre
					};
					db.FBSLogEquipo.Add(equipo);
				}

				string strFechaSistema = dataJson.evento.fechaSistema;

				//DateTime fechaMaquina = DateTime.ParseExact(strFechaMaquina, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				//DateTime fechaSistema = DateTime.ParseExact(strFechaSistema, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				DateTime fechaMaquina = DateTime.Parse(strFechaMaquina);
				DateTime fechaSistema = DateTime.Parse(strFechaSistema);

				//Guardando el FBS_LOGERROR
				FBSLogError fbsLog = new FBSLogError
				{
					fbsLogEquipo = equipo,
					Cliente = dataJson.evento.cliente,
					Empresa = dataJson.evento.empresa,
					Oficina = dataJson.evento.oficina,
					FechaMaquina = fechaMaquina,
					FechaSistema = fechaSistema,
					Usuario = dataJson.evento.usuario,
					RutaMenu = dataJson.evento.rutaMenu,
					NombreFichero = dataJson.evento.nombreFichero,
					MensajeDisculpa = dataJson.evento.mensajeDisculpa,
					MensajeError = dataJson.evento.mensajeError,
					MensajeExcepcion = dataJson.evento.mensajeExcepcion,
					NombreModulo = dataJson.evento.nombreModulo,
					NombreClase = dataJson.evento.nombreClase,
					NombreMetodo = dataJson.evento.nombreMetodo,
					Linea = dataJson.evento.linea,
					Columna = dataJson.evento.columna,
					StackTrace = dataJson.evento.stackTrace,
					CodigoError = (dataJson.evento.codigoError == null) ? "" : dataJson.evento.codigoError,
					Publicacion = dataJson.evento.publicacion,
					MaquinaPublicacion = dataJson.evento.maquinaPublicacion,
					Servidor = (dataJson.evento.servidor) == null ? "" : dataJson.evento.servidor,
					BaseDatos = (dataJson.evento.baseDatos) == null ? "" : dataJson.evento.baseDatos,
					TipoError = "S"
				};
				db.FBSLogError.Add(fbsLog);

				db.SaveChanges();

				string path = "";
				string[] adjuntos = null;
				if(foto != null)
				{
					var file = Path.Combine(Server.MapPath("~/Web/images/logFBSError"), "fotoErrorFBS.jpg");
					if(System.IO.File.Exists(file))
						System.IO.File.Delete(file);

					//Subiendo la nueva foto			
					path = Path.Combine(Server.MapPath("~/Web/images/logFBSError"), "fotoErrorFBS.jpg");
					foto.SaveAs(path);//Renombrado y Salvado	
					adjuntos = new string[] { path };
				}

				//Enviando el email a los implicados
				//string[] emailsDestinos = new string[]{                                         
				//                                        "zulest@sifizsoft.com",
				//                                        "gerencia@sifizsoft.com",
				//                                        "vhidalgo@sifizsoft.com",
				//                                        "avalencia@sifizsoft.com",
				//                                        "cmoreno@sifizsoft.com",
				//                                        "squishpe@sifizsoft.com"
				//                                      };

				string[] emailsDestinos = new string[]{
												"rcespedes@sifizsoft.com"
											 };
				if(fbsLog.Cliente == "COOPERATIVA DE AHORRO Y CREDITO CORPORACION CENTRO LTDA.")
				{
					/*
                    emailsDestinos = new string[]{  
                                                    "rcespedes@sifizsoft.com",
                                                    "squishpe@sifizsoft.com"
                                                 };
                     */
				}

				string asunto = "FBS ha detectado un error, cliente " + fbsLog.Cliente;
				string datosError = @"<div class='textoCuerpo'>
                                     <b>Cliente:</b> " + fbsLog.Cliente + "<br/>" +
									"<b>Oficina:</b> " + fbsLog.Oficina + "<br/>" +
									"<b>Hora del Error:</b> " + fbsLog.FechaMaquina.ToString("dd/MM/yyyy HH:mm:ss") + "<br/>" +
									"<b>Usuario:</b> " + fbsLog.Usuario + "<br/>" +
									"<b>Mensaje Desde la Máquina:</b> " + fbsLog.fbsLogEquipo.Nombre + "/" + fbsLog.fbsLogEquipo.Ip + "<br/>" +
									"<b>Mensaje de Error:</b> " + fbsLog.MensajeExcepcion + "<br/>" +
									"<b>Ruta del Menú:</b> " + fbsLog.RutaMenu + "<br/>" +
									"<b>Módulo:</b> " + fbsLog.NombreModulo + "<br/>" +
									"<b>Clase:</b> " + fbsLog.NombreClase + "<br/>" +
									"<b>Método:</b> " + fbsLog.NombreMetodo + "<br/><br/>" +

									"<b>Versión de Publicación:</b> " + fbsLog.Publicacion + "<br/>" +
									"<b>Publicación Realizada por:</b> " + fbsLog.MaquinaPublicacion + "<br/><br/>" +

									"Por favor tome las medidas pertinentes según la información brindada, muchas gracias.</div>";

				Utiles.EnviarEmailSistema(emailsDestinos, datosError, asunto, adjuntos);

				var resp = new
				{
					success = true
				};
				return Json(resp);
			}
			catch(Exception e)
			{
				List<string> emails = new List<string>();
				emails.Add("rcespedes@sifizsoft.com");
				string textoEmail = "<b>error:</b> " + e.Message + "<br/>";
				textoEmail += "<b>json dato:</b> " + json + "<br/><br/>";
				textoEmail += "<b>token:</b> " + tocken + "<br/><br/>";
				//Utiles.EnviarEmailSistema(emails.ToArray(), textoEmail , "error en gestión de errores");

				var resp = new
				{
					success = false,
					msg = e.Message
				};
				return Json(resp);
			}
		}
	}
}