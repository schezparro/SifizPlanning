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
            string nameQr = Guid.NewGuid().ToString() + ".png";
            //string pathImagen = System.Web.HttpContext.Current.Server.MapPath("~/Web/images/email") + "/" + nameQr;
            string pathImagen = HostingEnvironment.MapPath("~/Web/images/email") + "/" + nameQr;
            if (qr != null)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(2);
                File.WriteAllBytes(pathImagen, qrCodeImage);
            }

            string htmlCss = @"<style>
                                           .textoCuerpo{
                                                font-size: 11pt;
                                                font-family: ""Calibri"", sans-serif;
                                                color: #1F497D;
                                           }
                                           .cabecera{
                                                font-size: 8pt;
                                                font-family: ""Calibri"", sans-serif;
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
                                                font-family: ""Calibri"", sans-serif;
                                           }
                                           table, td {
                                                border: 1px solid black;
                                                border-collapse: collapse;
                                                font-size: 8pt;
                                                background: #ccc;
                                                font-family: ""Calibri"", sans-serif;
                                                vertical-align: top;
                                            }
                                            th, td {
                                                padding: 10px;
                                            }

                                            /* Font Definitions */
                                            @font-face
	                                            {font-family:""Cambria Math"";
	                                            panose-1:2 4 5 3 5 4 6 3 2 4;}
                                            @font-face
	                                            {font-family:Calibri;
	                                            panose-1:2 15 5 2 2 2 4 3 2 4;}
                                            @font-face
	                                            {font-family:Verdana;
	                                            panose-1:2 11 6 4 3 5 4 4 2 4;}
                                            @font-face
	                                            {font-family:""Palatino Linotype"";
	                                            panose-1:2 4 5 2 5 5 5 3 3 4;}
                                            /* Style Definitions */
                                            p.MsoNormal, li.MsoNormal, div.MsoNormal
	                                            {margin:0cm;
	                                            margin-bottom:.0001pt;
	                                            font-size:11.0pt;
	                                            font-family:""Calibri"",sans-serif;
	                                            mso-fareast-language:EN-US;}
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
	                                            font-family:""Calibri"",sans-serif;
	                                            color:windowtext;}
                                            .MsoChpDefault
	                                            {mso-style-type:export-only;
	                                            font-family:""Calibri"",sans-serif;
	                                            mso-fareast-language:EN-US;}
                                            @page WordSection1
	                                            {size:612.0pt 792.0pt;
	                                            margin:70.85pt 3.0cm 70.85pt 3.0cm;}
                                            div.WordSection1
	                                            {page:WordSection1;}
                                       </style>";

            string htmlfirma = @"<div class=WordSection1>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal><span lang=ES-EC style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>Atentamente,<o:p></o:p></span></p><p class=MsoNormal><i><span style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>"
                            +
                            "Sifizplanning"
                            +
                            @"<o:p></o:p></span></i></p><p class=MsoNormal><span style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>"
                            +
                            "Sistema Planificador Integral SifizSoft s.a."
                            +
                            @"<br> 
                            <b style='color:#1F497D !important;'>
                                <i>
                                    02-450-4616 <br/>
                                    Quito - Ecuador
                                </i>
                            </b>";
            if (qr != null)
            {
                htmlfirma += "<br/><img style='width: 25px !important; height: 25px !important;'  src='cid:" + nameQr + "'><br/>";
            }

            htmlfirma += @"</span>                              
                              <span lang=IT style='font-size:12.0pt;font-family:'Times New Roman',serif;color:#203864;mso-fareast-language:ES'><o:p></o:p></span></p><div class=MsoNormal align=center style='text-align:center'><span lang=EN style='font-size:9.0pt;font-family:'Verdana',sans-serif;color:#1F497D;mso-fareast-language:ES-EC'><hr size=3 width='100%' align=center></span></div><p class=MsoNormal><b><span lang=ES style='color:#1F497D;mso-fareast-language:ES-EC'>Somos líderes en la producción de software financiero-contable de última tecnología. </span></b><b><span lang=ES style='font-family:'Times New Roman',serif;color:#1F497D;mso-fareast-language:ES'><o:p></o:p></span></b></p><p class=MsoNormal><a href='http://www.sifizsoft.com/'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:2.0pt;mso-text-raise:-2.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=129 height=49 id='Imagen_x0020_2' src='cid:sifizsoft.jpg' alt='cid:image001.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;&nbsp;<span style='position:relative;top:-3.0pt;mso-text-raise:3.0pt'>&nbsp;</span></span><span lang=EN-US style='font-size:9.0pt;color:#1F497D;position:relative;top:-18.0pt;mso-text-raise:18.0pt;letter-spacing:.2pt;mso-fareast-language:ES-EC'>Like us in</span><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp; </span><a href='http://www.facebook.com/pages/SifizSoft/287494208026463?sk=app_129982580378550'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=41 height=41 id='Imagen_x0020_3' src='cid:fb.jpg' alt='cid:image002.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;</span><span lang=EN-US style='font-size:9.0pt;color:#1F497D;position:relative;top:-18.0pt;mso-text-raise:18.0pt;mso-fareast-language:ES-EC'>and Follow us on</span><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'> </span><a href='https://twitter.com/SifizSoftSA'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=41 height=41 id='Imagen_x0020_4' src='cid:tw.jpg' alt='cid:image003.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;</span><a href='http://lnkd.in/GYc2-s'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=106 height=41 id='Imagen_x0020_5' src='cid:linkedin.jpg' alt='cid:image004.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp; &nbsp;</span><a href='http://www.efqm.org/en/'><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=101 height=55 id='Imagen_x0020_6' src='cid:efqm.jpg' alt='cid:image005.png@01D244E9.77AAB2B0'></span></a><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'> </span><a href='http://www.openkm.com/en/'><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=109 height=53 id='Imagen_x0020_7' src='cid:openkm.jpg' alt='cid:image006.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'><o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center;line-height:17.0pt;mso-line-height-rule:exactly;text-autospace:none'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>OFICINA QUITO: Rumipamba E2-214 y Av. República, edificio Signature, piso 09, oficina 901</span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>.&nbsp; Teléfonos&nbsp; </span><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>02-351-7729, &nbsp;02-351-8919, 02-450-4616, 02-450-4727, </span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>desde USA 1(407)255 8532<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>OFICINA AMBATO: Av. Atahualpa y Pasaje Arajuno S/N a una cuadra del nuevo municipio.&nbsp; Teléfono 03-241-6586&nbsp; 03-241-9127<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>PO&nbsp; BOX 780066&nbsp;Orlando, FL 32878-0066 Toll free (800) 793-8369<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>Este correo electrónico es solo para uso del destinatario y puede contener información confidencial. Cualquier distribución uso o lectura de este material está expresamente prohibido. Si usted no es el destinatario o si usted ha recibido este correo electrónico por error por favor contacte al remitente y destruya todas las copias y el mensaje original.</span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES'><o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>This E-mail message is for the sole use of the intended recipient(s) and may contain confidential and privileged information. Any unauthorized review, use, disclosure or distribution is prohibited. If you are not the intended recipient, please contact the sender by reply E-mail and destroy all copies of the original message.<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>Copyrights ©SifizSoft&nbsp;2004-2016 carefully reserved and preserved</span><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES'><o:p></o:p></span></p><p class=MsoNormal><o:p>&nbsp;</o:p></p></div></body></html>";

            List<string> listImagenes = new List<string>();
            listImagenes.AddRange(new string[] { "sifizsoft.jpg", "fb.jpg", "tw.jpg", "linkedin.jpg", "efqm.jpg", "openkm.jpg" });
            if (qr != null)
            {
                listImagenes.Add(nameQr);
            }
            string[] imagenes = listImagenes.ToArray();
            try
            {
                string email = System.Configuration.ConfigurationManager.AppSettings["emailApp"];
                string password = System.Configuration.ConfigurationManager.AppSettings["passwordEmailApp"];
                string htmlMail = htmlCss + emailBody + htmlfirma;

                // Obtener la última hora en que se ejecutó una tarea finalizada
                //    var monitoringApi = JobStorage.Current.GetMonitoringApi();
                //    var jobDetails = monitoringApi.SucceededJobs(0, 1);
                //    var jobRunningDetails = monitoringApi.ProcessingJobs(0, 1);

                //    if (jobRunningDetails.Count == 0)
                //    {
                //        if (jobDetails.Count > 0)
                //        {
                //            var lastJob = jobDetails.Last();
                //            var lastExecutionTime = (lastJob.Value.SucceededAt.HasValue ? lastJob.Value.SucceededAt.Value : DateTime.Now).AddHours(-4);
                //            var waitTime = TimeSpan.FromMinutes(5) - (DateTime.Now - lastExecutionTime);
                //            if (waitTime > TimeSpan.Zero)
                //            {
                //                // Programa la siguiente tarea para ejecutarse después del tiempo de espera calculado
                //                BackgroundJob.Schedule(() => Utiles.EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos, -1), waitTime);
                //            }
                //            else
                //            {
                //                // Ejecuta la siguiente tarea inmediatamente
                //                BackgroundJob.Enqueue(() => Utiles.EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos, -1));
                //            }
                //        }
                //        else
                //        {
                //            // Ejecuta la siguiente tarea inmediatamente
                //            BackgroundJob.Enqueue(() => Utiles.EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos, -1));
                //        }
                //    }
                //    else
                //    {
                //        BackgroundJob.Schedule(() => Utiles.EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos, -1), TimeSpan.FromMinutes(5));
                //    }
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
            string nameQr = Guid.NewGuid().ToString() + ".png";
            string pathImagen = HostingEnvironment.MapPath("~/Web/images/email") + "/" + nameQr;
            if (qr != null)
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(qr, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(2);
                File.WriteAllBytes(pathImagen, qrCodeImage);
            }
            string htmlCss = emailCSS;
            string htmlfirma = @"<div class=WordSection1>
                                    <p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal><o:p>&nbsp;</o:p></p><p class=MsoNormal><span lang=ES-EC style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>Atentamente,<o:p></o:p></span></p><p class=MsoNormal><i><span style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>"
                            +
                            "Sifizplanning"
                            +
                            @"<o:p></o:p></span></i></p><p class=MsoNormal><span style='font-size:12.0pt;font-family:""Times New Roman"",serif;color:#1F497D;mso-fareast-language:ES-EC'>"
                            +
                            "Sistema Planificador Integral SifizSoft s.a."
                            +
                            @"<br> 
                            <b style='color:#1F497D !important;'>
                                <i>
                                    02-450-4616 <br/>
                                    Quito - Ecuador
                                </i>
                            </b>";
            if (qr != null)
            {
                htmlfirma += "<br/><img style='width: 25px !important; height: 25px !important;'  src='cid:" + nameQr + "'><br/>";
            }

            htmlfirma += @"</span>                              
                              <span lang=IT style='font-size:12.0pt;font-family:'Times New Roman',serif;color:#203864;mso-fareast-language:ES'><o:p></o:p></span></p><div class=MsoNormal align=center style='text-align:center'><span lang=EN style='font-size:9.0pt;font-family:'Verdana',sans-serif;color:#1F497D;mso-fareast-language:ES-EC'><hr size=3 width='100%' align=center></span></div><p class=MsoNormal><b><span lang=ES style='color:#1F497D;mso-fareast-language:ES-EC'>Somos líderes en la producción de software financiero-contable de última tecnología. </span></b><b><span lang=ES style='font-family:'Times New Roman',serif;color:#1F497D;mso-fareast-language:ES'><o:p></o:p></span></b></p><p class=MsoNormal><a href='http://www.sifizsoft.com/'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:2.0pt;mso-text-raise:-2.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=129 height=49 id='Imagen_x0020_2' src='cid:sifizsoft.jpg' alt='cid:image001.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;&nbsp;<span style='position:relative;top:-3.0pt;mso-text-raise:3.0pt'>&nbsp;</span></span><span lang=EN-US style='font-size:9.0pt;color:#1F497D;position:relative;top:-18.0pt;mso-text-raise:18.0pt;letter-spacing:.2pt;mso-fareast-language:ES-EC'>Like us in</span><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp; </span><a href='http://www.facebook.com/pages/SifizSoft/287494208026463?sk=app_129982580378550'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=41 height=41 id='Imagen_x0020_3' src='cid:fb.jpg' alt='cid:image002.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;</span><span lang=EN-US style='font-size:9.0pt;color:#1F497D;position:relative;top:-18.0pt;mso-text-raise:18.0pt;mso-fareast-language:ES-EC'>and Follow us on</span><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'> </span><a href='https://twitter.com/SifizSoftSA'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=41 height=41 id='Imagen_x0020_4' src='cid:tw.jpg' alt='cid:image003.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp;&nbsp;</span><a href='http://lnkd.in/GYc2-s'><span style='font-size:12.0pt;color:#1F497D;position:relative;top:-8.0pt;mso-text-raise:8.0pt;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=106 height=41 id='Imagen_x0020_5' src='cid:linkedin.jpg' alt='cid:image004.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'>&nbsp; &nbsp;</span><a href='http://www.efqm.org/en/'><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=101 height=55 id='Imagen_x0020_6' src='cid:efqm.jpg' alt='cid:image005.png@01D244E9.77AAB2B0'></span></a><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'> </span><a href='http://www.openkm.com/en/'><span style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC;text-decoration:none'><img border=0 width=109 height=53 id='Imagen_x0020_7' src='cid:openkm.jpg' alt='cid:image006.jpg@01D244E9.77AAB2B0'></span></a><span lang=EN-US style='font-size:12.0pt;color:#1F497D;mso-fareast-language:ES-EC'><o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center;line-height:17.0pt;mso-line-height-rule:exactly;text-autospace:none'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>OFICINA QUITO: Rumipamba E2-214 y Av. República, edificio Signature, piso 09, oficina 901</span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>.&nbsp; Teléfonos&nbsp; </span><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>02-351-7729, &nbsp;02-351-8919, 02-450-4616, 02-450-4727, </span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>desde USA 1(407)255 8532<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>OFICINA AMBATO: Av. Atahualpa y Pasaje Arajuno S/N a una cuadra del nuevo municipio.&nbsp; Teléfono 03-241-6586&nbsp; 03-241-9127<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>PO&nbsp; BOX 780066&nbsp;Orlando, FL 32878-0066 Toll free (800) 793-8369<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>Este correo electrónico es solo para uso del destinatario y puede contener información confidencial. Cualquier distribución uso o lectura de este material está expresamente prohibido. Si usted no es el destinatario o si usted ha recibido este correo electrónico por error por favor contacte al remitente y destruya todas las copias y el mensaje original.</span><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES'><o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=ES style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'><o:p>&nbsp;</o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>This E-mail message is for the sole use of the intended recipient(s) and may contain confidential and privileged information. Any unauthorized review, use, disclosure or distribution is prohibited. If you are not the intended recipient, please contact the sender by reply E-mail and destroy all copies of the original message.<o:p></o:p></span></p><p class=MsoNormal align=center style='text-align:center'><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES-EC'>Copyrights ©SifizSoft&nbsp;2004-2016 carefully reserved and preserved</span><span lang=EN-US style='font-size:10.0pt;color:#44546A;mso-fareast-language:ES'><o:p></o:p></span></p><p class=MsoNormal><o:p>&nbsp;</o:p></p></div></body></html>";

            List<string> listImagenes = new List<string>();
            listImagenes.AddRange(new string[] { "sifizsoft.jpg", "fb.jpg", "tw.jpg", "linkedin.jpg", "efqm.jpg", "openkm.jpg" });
            if (qr != null)
            {
                listImagenes.Add(nameQr);
            }
            string[] imagenes = listImagenes.ToArray();
            try
            {
                string email = System.Configuration.ConfigurationManager.AppSettings["emailApp"];
                string password = System.Configuration.ConfigurationManager.AppSettings["passwordEmailApp"];
                string htmlMail = htmlCss + emailBody + htmlfirma;
                EnviarEmail(email, emailsDestinos, htmlMail, asunto, password, true, imagenes, adjuntos);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return true;
        }

        public static void EnviarEmail(string emailFuente, string[] emailsDestinos, string emailBody, string asunto = "Información", string password = "", bool adjuntarimagen = false, string[] imagenes = null, string[] adjuntos = null, int idCorreo = -1)
        {
            //var email = new MimeMessage();
            //email.From.Add(new MailboxAddress(emailFuente));
            //email.Subject = asunto;

            //foreach(string emailDestino in emailsDestinos)
            //{
            //    email.To.Add(new MailboxAddress(emailDestino));
            //}
            //var builder = new BodyBuilder();
            //if(adjuntarimagen)
            //{
            //    foreach(string imagen in imagenes)
            //    {
            //        string[] path = new string[2] { HostingEnvironment.MapPath("~/Web/images/email"), imagen };
            //        string imagePath = Path.Combine(path);
            //        builder.HtmlBody = emailBody;
            //        builder.Attachments.Add(imagePath);
            //    }
            //}
            //else
            //{
            //    builder.HtmlBody = emailBody;
            //}
            //if(adjuntos != null && adjuntos.Length > 0)
            //{
            //    foreach(var adj in adjuntos)
            //    {
            //        builder.Attachments.Add(adj);
            //    }
            //}
            //email.Body = builder.ToMessageBody();

            //using(var smtp = new SmtpClient())
            //{
            //    smtp.Connect("email.sifizsoft.com", 465, true);
            //    smtp.Authenticate(emailFuente, password);

            //    bool enviado = false;
            //    string mensaje = "";

            //    try
            //    {
            //        smtp.Send(email);
            //        enviado = true;
            //    }
            //    catch(Exception e)
            //    {
            //        mensaje = e.Message;
            //    }

            //    smtp.Disconnect(true);

            //    // The rest of your code related to database operations and error handling remains the same.
            //    if(!enviado)
            //    {
            //        //Si no se pudo enviar y es un correo nuevo, se guarda en la BD
            //        if(idCorreo <= 0)
            //        {
            //            CorreoNoEnviado objCorreoNoEnviado = new CorreoNoEnviado()
            //            {
            //                EmailEnvia = emailFuente,
            //                Password = EncriptacionSimetrica(password),
            //                FechaHora = DateTime.Now,
            //                Asunto = asunto,
            //                Texto = emailBody.Length > 8000 ? emailBody.Substring(0, 8000) : emailBody,
            //                EmailDestinos = emailsDestinos.ToString(),
            //                AdjuntarImagen = adjuntarimagen,
            //                Imagenes = imagenes.ToString(),
            //                Adjuntos = adjuntos.ToString(),
            //                MensajeError = mensaje,
            //                EstaActivo = true
            //            };

            //            try
            //            {
            //                db.CorreoNoEnviado.Add(objCorreoNoEnviado);
            //                db.SaveChanges();
            //            }
            //            catch(Exception)
            //            {
            //                throw new Exception(mensaje);
            //            }
            //        }
            //        BackgroundJob.Schedule(() => EnviarEmail(emailFuente, emailsDestinos, emailBody, asunto, password, adjuntarimagen, imagenes, adjuntos, idCorreo), TimeSpan.FromMinutes(20));
            //    }
            //    else
            //    {
            //        //Si se pudo enviar y no es un correo nuevo, se elimina de la BD
            //        if(idCorreo > 0)
            //        {
            //            CorreoNoEnviado objCorreoNoEnviado = db.CorreoNoEnviado.Find(idCorreo);
            //            if(objCorreoNoEnviado != null)
            //                objCorreoNoEnviado.EstaActivo = false;
            //            try
            //            {
            //                db.SaveChanges();
            //            }
            //            catch(Exception)
            //            {
            //                throw new Exception(mensaje);
            //            }

            //        }
            //    }
            //}
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
        static string claveEncriptacionSimetrica = "lkjIQWUQOWjsdlkJASDIO12%&gksklER";
        static string vectorEncriptacionSimetrica = "ljasdls@351$%d!3";
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
    }
}