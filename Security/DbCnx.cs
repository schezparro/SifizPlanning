using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SifizPlanning.Models;

namespace SifizPlanning.Security
{
    public class DbCnx
    {
        public static SifizPlanningEntidades getCnx()
        {
            return new SifizPlanningEntidades();
        }

        //public static TFSWarehouseEntities getCnxTfs()
        //{
        //    return new TFSWarehouseEntities();
        //}

    }
}