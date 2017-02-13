using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace HotelMateWebV1.Security
{
    public class CustomRoleProvider : RoleProvider
    {
        private readonly IPersonService _personService;

        public CustomRoleProvider()
        {
            _personService = new PersonService();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var person = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username));

            if (person == null)
                return false;

            if (person.PersonType.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase))
                return true;

            return false;
        }        

        public override string[] GetRolesForUser(string username)
        {
            var person = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username));

            var role = person.PersonType.Name;

            if (person != null)
                return new string[] { person.PersonType.Name };

            return null;

        }

        // -- Snip --
        public override string[] GetAllRoles()
        {
            return new string[] { "Admin","Staff","Guest"};            
        }
        // -- Snip --

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}