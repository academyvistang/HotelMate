using System;
using System.Web;
using System.Web.Security;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using System.Linq;
using HotelMateWeb.Dal.DataCore;
using System.Net;
using Microsoft.AspNet.Identity;
using HotelMateWebV1.Helpers.Enums;

namespace HotelMateWebV1.Security
{
    public sealed class AppSecurity
    {
        //private AppSecurityHelper appSecurityHelper;

        //private singleton instance needed for access to services.
        private static AppSecurity instance = new AppSecurity();

        //private static readonly string EventTenderKey = "EventTenderKey";
        private static IPersonService _personService = null;

        static AppSecurity()
        {
            _personService = new PersonService();
        }

        private AppSecurity()
        {
            _personService = new PersonService();
        }

        private static AppSecurity Instance
        {
            get
            {
                return instance;
            }
        }

        private static int? GetDistributionPointId()
        {
            DistributionPointService ds = new DistributionPointService();

            int id = ds.GetAll().FirstOrDefault().Id;

            try
            {
                if (System.Configuration.ConfigurationManager.AppSettings["DistributionPointId"] != null)
                {
                    int.TryParse(System.Configuration.ConfigurationManager.AppSettings["DistributionPointId"].ToString(), out id);
                }
            }
            catch
            {

            }

            return id;
        }


        public static string GetCurrentUserRole(string username)
        {
            ///var user = personService.GetPersonByUserName(username);

            //if (user != null && user.PassMasterUserPassMasterRoles.Count > 0)
            //{
            //    return user.PassMasterUserPassMasterRoles.OrderBy(x => x.PassMasterRole.Name).FirstOrDefault().PassMasterRole.Name;
            //}

            return "";
        }

        /// <summary>
        /// Login user - authenticate against PassMaster user store
        /// </summary>
        /// <param name="domainUsername"></param>
        public static void Login(string domainUsername)
        {
            //check we have an PassMaster user account for the windows user attempting login
            var user = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(domainUsername));
            bool IsAuthenticated = false;

            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.Username, true);
                IsAuthenticated = true;

                if (IsAuthenticated)
                {
                   // user.LastLoginDateTime = DateTime.Now;
                    _personService.Update(user);
                }
                else
                {
                    //user.LoginAttempts = user.LoginAttempts + 1;
                    _personService.Update(user);
                }
            }
            else
            {
                throw new Exception("Login Failed");
            }
        }

        /// <summary>
        /// Login user - authenticate against PassMaster user store and attempt Active Directory 
        /// authentication using supplied username/password
        /// </summary>
        /// <param name="domainUsername"></param>
        /// <param name="password"></param>
        public static bool Login(string domainUsername, string password, out bool guestLogin)
        {
            string domain = string.Empty;

            string username = string.Empty;

            Person iemsUser = null;

            guestLogin = false;

            try
            {
                iemsUser = _personService.GetPersonByUserNameAndPassword(domainUsername, password);
                //if (iemsUser != null && iemsUser.ParentPassword == password)
                //{
                //    guestLogin = true;
                //}
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }

            if (iemsUser != null && iemsUser.IsActive)
            {
                bool isADAuthenticated = false;

                isADAuthenticated = true;
                if (isADAuthenticated)
                {
                    CreateCookieForAfeni(iemsUser.Username, true, iemsUser.PersonID);
                }

                if (isADAuthenticated)
                {
                    iemsUser.DistributionPointId = GetDistributionPointId();

                    if (string.IsNullOrEmpty(iemsUser.IdNumber))
                    {
                        iemsUser.IdNumber = DateTime.Now.ToShortTimeString();
                    }

                    _personService.Update(iemsUser);
                }
                else
                {
                    return false;
                    throw new Exception("Login Failed");
                }
            }
            else
            {
                return false;
                throw new Exception("Login Failed");

            }

            return true;
        }

        private static void CreateCookieForAfeni(string userName, bool createPersistentCookie, int userId)
        {


            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            //var identity = UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            //AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
           
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddYears(1), createPersistentCookie, FormsAuthentication.FormsCookiePath);

            // add cookie to response stream         
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            System.Web.HttpCookie authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            if (authTicket.IsPersistent)
            {
                authCookie.Expires = authTicket.Expiration;
            }

            System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
        }

        ///// <summary>
        ///// The pass file currently being imported.  Used to support the Preview function.
        ///// </summary>
        //public static AreaFileImport AreaFileImport
        //{
        //    get
        //    {
        //        return (AreaFileImport)HttpContext.Current.Session[AreaFileImportKey];
        //    }
        //    set
        //    {
        //        if (HttpContext.Current != null && HttpContext.Current.Session != null)
        //        {
        //            HttpContext.Current.Session.Add(AreaFileImportKey, value);
        //        }
        //    }
        //}


        /// <summary>
        /// The pass file currently being imported.  Used to support the Preview function.
        /// </summary>
        //public static Event EventModel
        //{
        //    get
        //    {
        //        return (Event)HttpContext.Current.Session[EventTenderKey];
        //    }
        //    set
        //    {
        //        if (HttpContext.Current != null && HttpContext.Current.Session != null)
        //        {
        //            HttpContext.Current.Session.Add(EventTenderKey, value);
        //        }
        //    }
        //}


        //destroy RunContext and Forms ticket
        internal static void Logout()
        {
            HttpContext.Current.Session.Abandon();
            FormsAuthentication.SignOut();
        }


        internal static Person GetUser(string domainUsername, string password)
        {
            var iemsUser = _personService.GetPersonByUserNameAndPassword(domainUsername, password);
            return iemsUser;
        }

        internal static Person GetUserByUsername(string userName)
        {
            return _personService.GetAllForLogin().FirstOrDefault(x => x.Username.ToUpper().Equals(userName.ToUpper()));
        }
    }
}