using System;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using HotelMateWebV1.Security;
using HotelMateWebV1.Models;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers;
using HotelMateWebV1.Helpers.Enums;
using AutoMapper;
using System.Configuration;
using System.Web;
using System.IO;
using System.Drawing;

using Agbo21.Dal;

    namespace HotelMateWebV1.Controllers
    {
        [HandleError(View = "CustomErrorView")]
        public class AccountController : Controller
        {

            private readonly IEmployeeShiftService _employeeShiftService;
            private readonly ITableItemService _tableItemService;

           private readonly IGuestService _guestService;
            private readonly IPersonService _personService = null;


        private int? _hotelId;
        private int HotelID
        {
            get { return _hotelId ?? GetHotelId(); }
            set { _hotelId = value; }
        }

        private int GetHotelId()
        {
            var username = User.Identity.Name;
            var user = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            return user.HotelId;
        }

       

            
            public ActionResult LogOn()
            {
                RoomBookingViewModel rbvm = new RoomBookingViewModel();
                return View(rbvm);
            }

            public ActionResult AdminLogin()
            {
                return View(new BaseViewModel());
            }
            public ActionResult NewLogin()
            {
                return View(new BaseViewModel());
            }

            public AccountController()
            {
                _employeeShiftService = new EmployeeShiftService();
                _tableItemService  = new TableItemService();
                //int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out _hotelAccountsTime);
                _guestService = new GuestService();
                
                _personService = new PersonService();
                //_personTypeService = new PersonTypeService();
            }

            [HttpPost]
            public ActionResult AdminLogOn(RoomBookingViewModel model, string returnUrl, string signIn)
            {
                if (signIn == "Create Account")
                {
                    Mapper.CreateMap<Person, PersonViewModel>();
                    var gu = Guid.NewGuid().ToString();
                    var pvm = Mapper.Map<Person, PersonViewModel>(new Person
                    {
                        BirthDate = DateTime.Today.AddYears(-20),
                        PersonID = 0,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(1),
                        FirstName = gu,
                        LastName = gu,
                        Address = gu,
                        Title = "MM",
                        MiddleName = gu,
                        PersonTypeId = (int)PersonTypeEnum.Guest
                    });
                    
                    return View("NewPerson", pvm);
                }
            
               

                if (ModelState.IsValid)
                {
                    bool guestLogin;
                    if (AppSecurity.Login(model.UserName, model.Password, out guestLogin))
                    {
                        //new Guest().Is
                        CreateShift(model.UserName, model.Password);

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home", new { model.UserName, guestLogin = guestLogin });
                    }

                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }

                // If we got this far, something failed, redisplay form
                model.ErrorMessage = "Log in Failed. Incorrect user credentials";

                return RedirectToAction("Index", "Home", new { loginFailed = true });

            }

            [HttpPost]
            public ActionResult LogOnUser(RoomBookingViewModel model, string returnUrl, string signIn)
            {
               

                if (ModelState.IsValid)
                {
                    bool guestLogin;
                    if (AppSecurity.Login(model.UserName, model.Password, out guestLogin))
                    {
                        CreateShift(model.UserName, model.Password);

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home", new { model.UserName, guestLogin = guestLogin });
                    }

                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }

                // If we got this far, something failed, redisplay form
                model.ErrorMessage = "Log in Failed. Incorrect user credentials";

                return RedirectToAction("Index", "Home", new { loginFailed = true });

            }



            [HttpPost]
            public ActionResult LogOn(RoomBookingViewModel model, string returnUrl, string signIn)
            {
                if (signIn == "Create Account")
                {
                    Mapper.CreateMap<Person, PersonViewModel>();
                    var gu = Guid.NewGuid().ToString();
                    var pvm = Mapper.Map<Person, PersonViewModel>(new Person
                    {
                        BirthDate = DateTime.Today.AddYears(-20),
                        PersonID = 0,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(1),
                        FirstName = gu,
                        LastName = gu,
                        Address = gu,
                        Title = "MM",
                        MiddleName = gu,
                        PersonTypeId = (int)PersonTypeEnum.Guest
                    });
                    
                    return View("NewPerson", pvm);
                }


                model.UserName = "GUEST";
                model.Password = "GUEST";

                if (ModelState.IsValid)
                {
                    bool guestLogin;
                    if (AppSecurity.Login(model.UserName, model.Password, out guestLogin))
                    {
                        CreateShift(model.UserName, model.Password);

                        if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                            && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home", new { model.UserName, guestLogin = guestLogin });
                    }

                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }

                // If we got this far, something failed, redisplay form
                model.ErrorMessage = "Log in Failed. Incorrect user credentials";

                return RedirectToAction("Index", "Home", new { loginFailed = true});
                
            }

            [HttpPost]
            public ActionResult NewPerson(PersonViewModel model, HttpPostedFileBase file)
            {
                if (!string.IsNullOrEmpty(model.UserName) || !string.IsNullOrEmpty(model.Email))
                {
                    var person = _personService.GetAllForLogin().FirstOrDefault(x => x.Email.ToUpper() == model.Email.ToUpper());

                    if (person != null)
                    {
                        if (model.PersonID == 0)
                            ModelState.AddModelError("UserName", "This email address already exists, please use a different email address.");
                        else
                        {
                            if (model.PersonID != person.PersonID)
                                ModelState.AddModelError("UserName", "This email address already exists, please use a different email address.");
                        }
                    }
                }

                if (ModelState.IsValid)
                {
                    Mapper.CreateMap<PersonViewModel, Person>();
                    Person person = Mapper.Map<PersonViewModel, Person>(model);



                    if (model.PersonID == 0)
                    {
                        person.HotelId = HotelID;
                        person.DisplayName = model.FirstName + " " + model.LastName;
                        person.Email = model.Email;
                        person.FirstName = model.FirstName;
                        person.LastName = model.LastName;
                        person.Password = model.Password;
                        person.Username = model.Email;
                        person.PersonTypeId = (int)PersonTypeEnum.Guest;
                        person.IsActive = true;
                        person.Address = model.Address;

                        person.Title = model.Title;
                        person.MiddleName = model.MiddleName;
                        person.StartDate = DateTime.Now;
                        person.EndDate = DateTime.Now;

                        person.Guardian = model.Guardian;
                        person.GuardianAddress = model.GuardianAddress;
                        person.PreviousEmployer = model.PreviousEmployer;
                        person.ReasonForLeaving = model.ReasonForLeaving;
                        person.Notes = model.Notes;
                        person.BirthDate = DateTime.Now;
                        person.PreviousEmployerStartDate = DateTime.Now;
                        person.PreviousEmployerEndDate = DateTime.Now;
                        person.IdNumber = "123456789";
                        var extension = "";

                        if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                        {
                            extension = Path.GetExtension(Request.Files[0].FileName);
                            Stream imageStream = Request.Files[0].InputStream;
                            Image img = Image.FromStream(imageStream);
                            var fileNewName = person.FirstName + person.LastName + extension;
                            var path = Path.Combine(Server.MapPath("~/Products"), fileNewName);
                            try
                            {
                                img.Save(path);
                                person.PicturePath = fileNewName;
                            }
                            catch
                            {

                            }
                        }

                        person = _personService.Create(person);

                        var guest = new Guest();
                        guest.FullName = person.FirstName + " " + person.LastName;
                        guest.Address = person.Address;
                        guest.Telephone = person.Telephone;
                        guest.Mobile = person.Mobile;
                        guest.CountryId = 0;
                        guest.Status = "LIVE";
                        guest.CarDetails = "";
                        guest.Notes = person.Notes;
                        guest.Email = person.Email;
                        guest.IsActive = true;
                        guest.CreatedDate = DateTime.Now;
                        guest.HotelId = person.HotelId;
                        guest.PersonId = person.PersonID;
                        guest.IsChild = false;

                        if (model.PersonTypeId == (int)PersonTypeEnum.Child)
                        {
                            guest.IsChild = true;
                        }

                        guest = _guestService.Create(guest);
                        person.IdNumber = guest.Id.ToString();
                        _personService.Update(person);

                        ActivateGameAccount(person);
                    }
                    else
                    {
                        var existingPerson = _personService.GetById(model.PersonID);

                        existingPerson.Email = model.Email;
                        existingPerson.FirstName = model.FirstName;
                        existingPerson.LastName = model.LastName;
                        existingPerson.Password = model.Password;
                        existingPerson.Username = model.Email;
                        //existingPerson.PersonTypeId = model.PersonTypeId;
                        existingPerson.IsActive = true;
                        existingPerson.Address = model.Address;

                        existingPerson.Title = model.Title;
                        existingPerson.MiddleName = model.MiddleName;
                        existingPerson.StartDate = DateTime.Now;
                        existingPerson.EndDate = DateTime.Now;

                        existingPerson.Guardian = model.Guardian;
                        existingPerson.GuardianAddress = model.GuardianAddress;
                        existingPerson.PreviousEmployer = model.PreviousEmployer;
                        existingPerson.ReasonForLeaving = model.ReasonForLeaving;
                        existingPerson.Notes = model.Notes;

                        if (string.IsNullOrEmpty(model.UserName))
                            model.UserName = model.Email;
                        //existingPerson.Username = model.Email;
                        existingPerson.Password = model.Password;
                        existingPerson.Email = model.Email;
                        // existingPerson.PersonTypeId = model.PersonTypeId;

                        existingPerson.BirthDate = DateTime.Now;
                        existingPerson.PreviousEmployerStartDate = DateTime.Now;
                        existingPerson.PreviousEmployerEndDate = DateTime.Now;
                        existingPerson.StartDate = DateTime.Now;
                        existingPerson.EndDate = DateTime.Now;
                        existingPerson.BirthDate = model.BirthDate;

                        existingPerson.Telephone = model.Telephone;
                        existingPerson.Mobile = model.Mobile;
                        existingPerson.JobTitle = model.JobTitle;
                        existingPerson.CityState = model.CityState;
                        existingPerson.WorkAddress = model.WorkAddress;
                        existingPerson.PlaceOfBirth = model.PlaceOfBirth;
                        existingPerson.Department = model.Department;
                        //existingPerson.IdNumber = model.IdNumber;
                        existingPerson.BankDetails = model.BankDetails;
                        existingPerson.AccountNumber = model.AccountNumber;
                        existingPerson.Salary = model.Salary;
                        existingPerson.MaritalStatus = model.MaritalStatus;
                        existingPerson.NoOfChildren = model.NoOfChildren;

                        if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                        {
                            var extension = Path.GetExtension(Request.Files[0].FileName);
                            Stream imageStream = Request.Files[0].InputStream;
                            Image img = Image.FromStream(imageStream);
                            var fileNewName = person.FirstName + person.LastName + extension;
                            var path = Path.Combine(Server.MapPath("~/Products"), fileNewName);
                            try
                            {
                                img.Save(path);
                                existingPerson.PicturePath = fileNewName;
                            }
                            catch
                            {

                            }
                        }

                        _personService.Update(existingPerson);

                        var existingGuest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == existingPerson.PersonID);

                        if (existingGuest != null)
                        {
                            existingGuest.FullName = existingPerson.FirstName + " " + existingPerson.LastName;
                            existingGuest.Address = existingPerson.Address;
                            existingGuest.Telephone = existingPerson.Telephone;
                            existingGuest.Mobile = existingPerson.Mobile;
                            existingGuest.CountryId = 0;
                            existingGuest.Status = "LIVE";
                            existingGuest.CarDetails = "";
                            existingGuest.Notes = existingPerson.Notes;
                            existingGuest.Email = existingPerson.Email;
                            existingGuest.IsActive = true;
                            existingGuest.CreatedDate = DateTime.Now;
                            existingGuest.HotelId = existingPerson.HotelId;
                            //existingGuest. = existingPerson.existingPersonID;
                            existingGuest.IsChild = false;

                            if (model.PersonTypeId == (int)PersonTypeEnum.Child)
                            {
                                existingGuest.IsChild = true;
                            }

                            _guestService.Update(existingGuest);

                        }

                    }

                    return RedirectToAction("AdminLogin");
                }

                return View(model);
            }

            private void ActivateGameAccount(Person person)
            {
                UnitOfWork unitOfWork = new UnitOfWork();

                var currentUser = new Entitities.User();

                currentUser.UserName = person.Username;
                currentUser.Password = person.Password;
                currentUser.Email = person.Email;
                currentUser.CreatedBy = "SA";
                currentUser.CreatedDate = DateTime.Now;
                currentUser.CurrentStake = 0;
                currentUser.CurrentUser = false;
                currentUser.ForfitChanceToPlay = true;
                currentUser.GameName = string.Empty;
                currentUser.IsActive = true;
                currentUser.IsComputer = false;
                currentUser.IsGameController = false;
                currentUser.LastLoggedInDate = DateTime.Now;
                currentUser.LoosingMessage = string.Empty;
                currentUser.Male = true;
                currentUser.ModifiedDate = DateTime.Now;
                currentUser.ModifiedBy = "SA";
                currentUser.PlayBalance = 10000;
                currentUser.playingNow = false;
                currentUser.playingSeq = 0;
                currentUser.PrevUserName = person.Username;
                currentUser.RealMoneyBalance = 10000;
                currentUser.ShowFinish = 0;
                currentUser.StartDate = DateTime.Now;
                currentUser.Status = "LIVE";
                currentUser.UserBalance = 10000;
                currentUser.UserPictureName = "user_anonymous.png";
                currentUser.WinningMessage = string.Empty;
                currentUser.PrevUserName = Guid.NewGuid().ToString();
                unitOfWork.UserRepository.Insert(currentUser);
                unitOfWork.Save();
            }

            private void CreateShift(string username, string password)
            {
                var person = AppSecurity.GetUser(username, password);
                var today = DateTime.Today.ToShortDateString();

                var es = _employeeShiftService.GetAll(2).FirstOrDefault(x => x.ShiftDate.ToShortDateString().Equals(today) && x.PersonId == person.PersonID);

                if(es == null)
                {
                    EmployeeShift employeeShift = new EmployeeShift();
                    employeeShift.ShiftDate = DateTime.Now;
                    employeeShift.StartTime = DateTime.Now;
                    employeeShift.EndTime = null;
                    employeeShift.PersonId = person.PersonID;
                    employeeShift.TotalSales = decimal.Zero;
                    employeeShift.TotalHotelSalesRecievable = decimal.Zero;
                    employeeShift.TotalBarSalesRecievable = decimal.Zero;
                    employeeShift.CloseMethod = "OPEN";
                    _employeeShiftService.Create(employeeShift);
                }
            }

            private void EndShift(string username, string password,decimal totalHotel, decimal totalBar, decimal totalSales, string closeShift)
            {
                var person = AppSecurity.GetUser(username, password);

                var today = DateTime.Today.ToShortDateString();

                var es = _employeeShiftService.GetAll(2).Where(x => x.PersonId == person.PersonID && x.ShiftDate.ToShortDateString().Equals(today)).LastOrDefault();

                if (es == null)
                {
                    EmployeeShift employeeShift = new EmployeeShift();
                    employeeShift.ShiftDate = DateTime.Now;
                    employeeShift.StartTime = DateTime.Now;
                    employeeShift.EndTime = DateTime.Now;
                    employeeShift.PersonId = person.PersonID;
                    employeeShift.TotalSales = decimal.Zero;
                    employeeShift.TotalHotelSalesRecievable = decimal.Zero;
                    employeeShift.TotalBarSalesRecievable = decimal.Zero;
                    employeeShift.CloseMethod = "OPEN";
                    _employeeShiftService.Create(employeeShift);
                }
                else
                {
                    es.EndTime = DateTime.Now;
                    es.TotalSales = totalSales;
                    es.CloseMethod = closeShift;
                    es.TotalHotelSalesRecievable = totalHotel;
                    es.TotalBarSalesRecievable = totalBar;
                    _employeeShiftService.Update(es);
                }
            }

            public ActionResult LogOffError()
            {
                AppSecurity.Logout();

                FormsAuthentication.SignOut();

                return RedirectToAction("Index", "Home");
            }
            
            public ActionResult LogOff()
            {
                //var person = AppSecurity.GetUserByUsername(User.Identity.Name);

                var person = _personService.GetAll(HotelID).FirstOrDefault(x => x.Username.ToUpper() == User.Identity.Name.ToUpper());

                if (person.PersonTypeId == (int)PersonTypeEnum.Guest || person.PersonTypeId == (int)PersonTypeEnum.Admin)
                {
                    if (person != null)
                    {
                        person.Salary = decimal.Zero;
                        _personService.Update(person);
                    }


                    AppSecurity.Logout();

                    FormsAuthentication.SignOut();

                    return RedirectToAction("Index", "Home");
                }

                var unclearedItems = _tableItemService.GetAll().Where(x => x.Cashier == person.PersonID).Count();

                var personModel = new PersonViewModel();

                personModel.CanCloseTill = true;

                if(unclearedItems > 0)
                {
                    personModel.CanCloseTill = false;
                }

                if(person != null)
                {
                 
                    personModel.TotalHotelRecievable = person.GetTotalHotelRecievable(DateTime.Today);
                    personModel.TotalBarRecievable = person.GetTotalBarRecievable(DateTime.Today);
                    personModel.TotalSales = personModel.TotalHotelRecievable + personModel.TotalBarRecievable;
                    personModel.UserName = person.Username;
                    personModel.Password = person.Password;
                }

                return View(personModel);
            }

            public ActionResult LogOffCompletely(string userName, string password, decimal? totalHotelRecievable, decimal? totalBarRecievable, decimal? totalSales, string closeShift)
            {
                closeShift = closeShift.Trim().ToUpper();

                EndShift(userName, password, totalHotelRecievable.Value, totalBarRecievable.Value, totalSales.Value, closeShift);

                AppSecurity.Logout();

                FormsAuthentication.SignOut();

                return RedirectToAction("Index", "Home");
            }

            //
            // GET: /Account/Register

            public ActionResult Register()
            {
                return View();
            }

            //
            // POST: /Account/Register

            //[HttpPost]
            //public ActionResult Register(RegisterModel model)
            //{
            //    if (ModelState.IsValid)
            //    {
            //        // Attempt to register the user
            //        MembershipCreateStatus createStatus;
            //        Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, null, out createStatus);

            //        if (createStatus == MembershipCreateStatus.Success)
            //        {
            //            FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
            //            return RedirectToAction("Index", "Home");
            //        }
            //        ModelState.AddModelError("", ErrorCodeToString(createStatus));
            //    }

            //    // If we got this far, something failed, redisplay form
            //    return View(model);
            //}

            //
            // GET: /Account/ChangePassword

            [Authorize]
            public ActionResult ChangePassword()
            {
                return View();
            }

            //
            // POST: /Account/ChangePassword

            //[Authorize]
            //[HttpPost]
            //public ActionResult ChangePassword(ChangePasswordModel model)
            //{
            //    if (!ModelState.IsValid) return View(model);
            //    // ChangePassword will throw an exception rather
            //    // than return false in certain failure scenarios.
            //    var changePasswordSucceeded = false;
            //    try
            //    {
            //        MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
            //        if (currentUser != null)
            //            changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
            //    }
            //    catch (Exception)
            //    {
            //        changePasswordSucceeded = false;
            //    }

            //    if (changePasswordSucceeded)
            //    {
            //        return RedirectToAction("ChangePasswordSuccess");
            //    }

            //    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");

            //    // If we got this far, something failed, redisplay form
            //    return View(model);
            //}

            //
            // GET: /Account/ChangePasswordSuccess

            public ActionResult ChangePasswordSuccess()
            {
                return View();
            }

            #region Status Codes
            private static string ErrorCodeToString(MembershipCreateStatus createStatus)
            {
                // See http://go.microsoft.com/fwlink/?LinkID=177550 for
                // a full list of status codes.
                switch (createStatus)
                {
                    case MembershipCreateStatus.DuplicateUserName:
                        return "User name already exists. Please enter a different user name.";

                    case MembershipCreateStatus.DuplicateEmail:
                        return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                    case MembershipCreateStatus.InvalidPassword:
                        return "The password provided is invalid. Please enter a valid password value.";

                    case MembershipCreateStatus.InvalidEmail:
                        return "The e-mail address provided is invalid. Please check the value and try again.";

                    case MembershipCreateStatus.InvalidAnswer:
                        return "The password retrieval answer provided is invalid. Please check the value and try again.";

                    case MembershipCreateStatus.InvalidQuestion:
                        return "The password retrieval question provided is invalid. Please check the value and try again.";

                    case MembershipCreateStatus.InvalidUserName:
                        return "The user name provided is invalid. Please check the value and try again.";

                    case MembershipCreateStatus.ProviderError:
                        return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                    case MembershipCreateStatus.UserRejected:
                        return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                    default:
                        return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
                }
            }
            #endregion
        }
    }