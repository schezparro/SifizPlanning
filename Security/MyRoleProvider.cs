using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using SifizPlanning.Models;

namespace SifizPlanning.Security
{
    public class MyRoleProvider : RoleProvider
    {
        SifizPlanningEntidades db = DbCnx.getCnx();
        public override string[] GetRolesForUser(string username)   {
            SifizPlanningEntidades db1 = new SifizPlanningEntidades();
            var objUser = db1.Usuario.Where(x => x.Email == username).FirstOrDefault();

            if (objUser == null)
            {
                return null;
            }            

            string[] ret = (from user in db1.Usuario join
                                   ur in db1.UsuarioRol on user equals ur.usuario join
                                    r in db1.Rol on ur.rol equals r
                            where r.EstaActivo == 1 && user.Secuencial == objUser.Secuencial
                            select r.Codigo).ToArray();
            return ret;
        }

        public override string[] GetAllRoles()
        {
            string[] ret = (from role in db.Rol
                            select role.Codigo).ToArray();
            return ret;
        }

        public override string ApplicationName { get; set; }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        { 
        
        }
        public override void CreateRole(string roleName) { }
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole) { return true; }
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        { 
            string[] arreglo = {"roles"};
            return arreglo; 
        }
        public override string[] GetUsersInRole(string roleName)
        {
            string[] arreglo = { "users" };
            return arreglo;
        }
        public override bool IsUserInRole(string username, string roleName) { return true; }
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames) { }
        public override bool RoleExists(string roleName) { return true; }

    }
}