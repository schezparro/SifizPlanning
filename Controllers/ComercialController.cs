using SifizPlanning.Models;
using SifizPlanning.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SifizPlanning.Util;
using System.Web.UI.WebControls;
using System.Configuration;

namespace SifizPlanning.Controllers
{
	public class ComercialController : Controller
	{
		SifizPlanningEntidades db = DbCnx.getCnx();

		// GET: Comercial
		//PANTALLA INICIAL
		[Authorize(Roles = "COMECIAL, ADMIN")]
		public ActionResult Index()
		{
			return View();
		}
    }
}