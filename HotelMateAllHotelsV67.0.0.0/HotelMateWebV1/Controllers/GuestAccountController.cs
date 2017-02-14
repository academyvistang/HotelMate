using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using AutoMapper;
using System.IO;
using System.Drawing;
using System.Web;
using Agbo21.Dal;
using System.Net;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Data;

namespace HotelMateWebV1.Controllers
{
    [Authorize()]
    [HandleError(View = "CustomErrorView")]
    public class GuestAccountController : Controller
    {
        private readonly IGuestService _guestService;
        private readonly IRoomService _roomService;
        private readonly IGuestRoomService _guestRoomService;
        private readonly IGuestReservationService _guestReservationService;
        private readonly IGuestRoomAccountService _guestRoomAccountService;
        private readonly int _hotelAccountsTime = 14;
        private readonly IBusinessAccountService _businessAccountService;

        private readonly IPersonService _personService = null;
        private readonly IPersonTypeService _personTypeService = null;
        private readonly IPaymentMethodService _paymentMethodService = null;
        private readonly ISupplierService _supplierService = null;
        private readonly IGuestLedgerService _guestLedgerService = null;



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

        private int UserID()
        {
            var username = User.Identity.Name;
            var user = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            return user.PersonID;
        }

        public GuestAccountController()
        {
            int.TryParse(ConfigurationManager.AppSettings["HotelAccountsTime"].ToString(CultureInfo.InvariantCulture), out _hotelAccountsTime);
            _guestService = new GuestService();
            _businessAccountService = new BusinessAccountService();
            _roomService = new RoomService();
            _guestRoomService = new GuestRoomService();
            _guestReservationService = new GuestReservationService();
            _guestRoomAccountService = new GuestRoomAccountService();
            _personService = new PersonService();
            _personTypeService = new PersonTypeService();
            _paymentMethodService = new PaymentMethodService();
            _supplierService = new SupplierService();
            _guestLedgerService = new GuestLedgerService();
        }

        [HttpPost]
        public ActionResult GrantInternetAccess(PersonViewModel model)
        {
            var guest = _guestService.GetById(model.GuestId);

            if(!string.IsNullOrEmpty(model.Email))
            {
                var existingPerson = _personService.GetAll(HotelID).FirstOrDefault(x => x.Username.Equals(model.Email, StringComparison.InvariantCultureIgnoreCase));
                if(model.PersonID == 0 && existingPerson != null)
                    ModelState.AddModelError("Email", "This email address already exists, please enter a different email address");
                if(model.PersonID > 0 && existingPerson != null)
                {
                    if(model.PersonID != existingPerson.PersonID)
                        ModelState.AddModelError("Email", "This email address already exists, please enter a different address");
                }
            }

            if (ModelState.IsValid)
            {
                Mapper.CreateMap<PersonViewModel, Person>();
                Person person = Mapper.Map<PersonViewModel, Person>(model);
                //var person = guest.Person;
                
                if (model.PersonID == 0)
                {                    
                    person.HotelId = HotelID;
                    person.Address = guest.Address;
                    person.BirthDate = DateTime.Now;
                    person.DisplayName = guest.FullName;
                    person.Email = model.Email;
                    person.FirstName = guest.FullName;                    
                    person.LastName = guest.FullName;
                    person.MiddleName = guest.FullName;
                    person.Password = model.Password;
                    person.Username = model.Email;
                    person.Title = "Mr";
                    person.PersonTypeId = (int)PersonTypeEnum.Guest;
                    person.IsActive = true;
                    person.EndDate = DateTime.Now.AddYears(1);
                    person.IdNumber = model.GuestId.ToString();
                    person.PreviousEmployerStartDate = DateTime.Now;
                    person.PreviousEmployerEndDate = DateTime.Now;
                    person.StartDate = DateTime.Now;
                    guest.Person = person;
                    
                    _guestService.Update(guest);

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
                else
                {
                    var existingPerson = _personService.GetById(model.PersonID);
                    existingPerson.Username = model.Email;
                    existingPerson.Password = model.Email;
                    existingPerson.Email = model.Email;
                    _personService.Update(existingPerson);
                }

                return RedirectToAction("EditInternetAccess", new { id = guest.Id, itemSaved = true });
            }

            var newguest = _guestService.GetById(model.GuestId);
            model.GuestName = newguest.FullName;
            return View(model);  
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]

        public ActionResult DenyInternetAccess(int? id)
        {
            var guest = _guestService.GetById(id.Value);
            var person = _personService.GetById(guest.PersonId);
            person = null;           
            guest.PersonId = 0;
            guest.PersonId = null;
            guest.Person = person;
            _guestService.Update(guest);
            return RedirectToAction("GuestCredentials");
        }

        //
        [HttpGet]
        public ActionResult NewSupplier()
        {
            Mapper.CreateMap<Supplier, PersonViewModel>();
            var pvm = Mapper.Map<Supplier, PersonViewModel>(new Supplier { Password = "PASSWORD", SupplierID = 0, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1) });
            pvm.PersonTypes = GetPersonTypes(null);
            return View(pvm);
        }
        
        [HttpGet]
        public ActionResult NewPerson()
        {            
            Mapper.CreateMap<Person, PersonViewModel>();
            var pvm = Mapper.Map<Person, PersonViewModel>(new Person { BirthDate = DateTime.Today.AddYears(-20), Password = "PASSWORD", PersonID = 0, StartDate = DateTime.Now, EndDate = DateTime.Now.AddMonths(1) });
            pvm.PersonTypes = GetPersonTypes(null);
            return View(pvm);
        }

        private IEnumerable<SelectListItem> GetPersonTypes(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas = _personTypeService.GetAll(HotelID).Where(x => x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList();                    
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.PersonTypeId.ToString(), Selected = x.PersonTypeId == selectedId });
        }

        private IEnumerable<SelectListItem> GetBusinessAccounts(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _businessAccountService.GetAll(HotelID).Where(x => !x.Debtor).ToList();
                
            bas.Insert(0, new BusinessAccount { CompanyName = "-- Please Select --", Id = 0 });
            //return bas.Select(x => new SelectListItem { Text = x.CompanyName, Value = x.Id.ToString(), Selected = true });
            return bas.Select(x => new SelectListItem { Text = x.CompanyName, Value = x.Id.ToString(), Selected = x.Id == selectedId });
        }


        [HttpPost]
        public ActionResult NewSupplier(PersonViewModel model)
        {
            if (!string.IsNullOrEmpty(model.UserName))
            {
                var person = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Trim().ToUpper() == model.UserName.Trim().ToUpper());

                if (person != null)
                {
                    if (model.PersonID == 0)
                        ModelState.AddModelError("UserName", "This username already exists, please use a different username.");
                    else
                    {
                        if (model.PersonID != person.PersonID)
                            ModelState.AddModelError("UserName", "This username already exists, please use a different username.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                Mapper.CreateMap<PersonViewModel, Supplier>();
                Supplier supplier = Mapper.Map<PersonViewModel, Supplier>(model);

                if (model.SupplierId == 0)
                {
                    supplier.HotelId = HotelID;
                    supplier.DisplayName = model.FirstName + " " + model.LastName;
                    supplier.Email = model.Email;
                    supplier.FirstName = model.FirstName;
                    supplier.LastName = model.LastName;
                    supplier.Password = model.Password;
                    supplier.Username = model.Email;
                    supplier.IsActive = true;
                    supplier.Address = model.Address;

                    supplier.Title = model.Title;
                    supplier.MiddleName = model.MiddleName;
                    supplier.StartDate = model.StartDate;
                    supplier.EndDate = model.EndDate;

                    supplier.Guardian = model.Guardian;
                    supplier.GuardianAddress = model.GuardianAddress;
                    supplier.PreviousEmployer = model.PreviousEmployer;
                    supplier.ReasonForLeaving = model.ReasonForLeaving;
                    supplier.Notes = model.Notes;
                    supplier.BirthDate = DateTime.Now;
                    supplier.PreviousEmployerStartDate = DateTime.Now;
                    supplier.PreviousEmployerEndDate = DateTime.Now;

                    _supplierService.Create(supplier);
                }
                else
                {
                    var existingSupplier = _supplierService.GetById(model.SupplierId);

                    existingSupplier.Email = model.Email;
                    existingSupplier.FirstName = model.FirstName;
                    existingSupplier.LastName = model.LastName;
                    existingSupplier.Password = model.Password;
                    existingSupplier.Username = model.Email;
                    existingSupplier.IsActive = true;
                    existingSupplier.Address = model.Address;

                    existingSupplier.Title = model.Title;
                    existingSupplier.MiddleName = model.MiddleName;
                    existingSupplier.StartDate = model.StartDate;
                    existingSupplier.EndDate = model.EndDate;

                    existingSupplier.Guardian = model.Guardian;
                    existingSupplier.GuardianAddress = model.GuardianAddress;
                    existingSupplier.PreviousEmployer = model.PreviousEmployer;
                    existingSupplier.ReasonForLeaving = model.ReasonForLeaving;
                    existingSupplier.Notes = model.Notes;

                    if (string.IsNullOrEmpty(model.UserName))
                        model.UserName = model.Email;
                    //existingPerson.Username = model.Email;
                    existingSupplier.Password = model.Password;
                    existingSupplier.Email = model.Email;

                    existingSupplier.BirthDate = DateTime.Now;
                    existingSupplier.PreviousEmployerStartDate = DateTime.Now;
                    existingSupplier.PreviousEmployerEndDate = DateTime.Now;
                    existingSupplier.StartDate = DateTime.Now;
                    existingSupplier.EndDate = DateTime.Now;


                    _supplierService.Update(existingSupplier);
                }

                return RedirectToAction("EditSupplier", new { id = supplier.SupplierID, itemSaved = true });
            }

            model.PersonTypes = GetPersonTypes(model.PersonTypeId);
            return View(model);
        }


        [HttpPost]
        public ActionResult NewPerson(PersonViewModel model, HttpPostedFileBase file, string send_booking)
        {
            if(!string.IsNullOrEmpty(model.UserName))
            {
               var person = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Trim().ToUpper() == model.UserName.Trim().ToUpper());

                if(person != null)
                {
                    if (model.PersonID == 0)
                        ModelState.AddModelError("UserName", "This username already exists, please use a different username.");
                    else
                    {
                        if(model.PersonID != person.PersonID)
                            ModelState.AddModelError("UserName", "This username already exists, please use a different username.");
                    }
                }
            }

            if (send_booking == "Delete" && model.PersonID > 0)
            {
                var existingPersonDelete = _personService.GetById(model.PersonID);
                existingPersonDelete.IsActive = false;
                _personService.Update(existingPersonDelete);
                return RedirectToAction("ManageUsers");
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
                    person.PersonTypeId = model.PersonTypeId;
                    person.IsActive = true;
                    person.Address = model.Address;

                    person.Title = model.Title;
                    person.MiddleName = model.MiddleName;
                    person.StartDate = model.StartDate;
                    person.EndDate = model.EndDate;

                    person.Guardian = model.Guardian;
                    person.GuardianAddress = model.GuardianAddress;
                    person.PreviousEmployer = model.PreviousEmployer;
                    person.ReasonForLeaving = model.ReasonForLeaving;
                    person.Notes = model.Notes;
                    person.BirthDate = DateTime.Now;
                    person.PreviousEmployerStartDate = DateTime.Now;
                    person.PreviousEmployerEndDate = DateTime.Now;

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

                    _personService.Create(person);
                }
                else
                {
                    var existingPerson = _personService.GetById(model.PersonID);

                    existingPerson.Email = model.Email;
                    existingPerson.FirstName = model.FirstName;
                    existingPerson.LastName = model.LastName;
                    existingPerson.Password = model.Password;
                    existingPerson.Username = model.Email;
                    existingPerson.PersonTypeId = model.PersonTypeId;
                    existingPerson.IsActive = true;
                    existingPerson.Address = model.Address;

                    existingPerson.Title = model.Title;
                    existingPerson.MiddleName = model.MiddleName;
                    existingPerson.StartDate = model.StartDate;
                    existingPerson.EndDate = model.EndDate;

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
                    existingPerson.PersonTypeId = model.PersonTypeId;

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
                    existingPerson.IdNumber = model.IdNumber;
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
                }

                return RedirectToAction("EditPerson", new { id = person.PersonID, itemSaved = true });
            }

            model.PersonTypes = GetPersonTypes(model.PersonTypeId);
            return View(model);        
        }

        //[HttpGet]
        //public ActionResult NewPerson()
        //{
        //    Mapper.CreateMap<Person, PersonViewModel>();
        //    var pvm = Mapper.Map<Person, PersonViewModel>(new Person { Password = "PASSWORD", PersonID = 0 });
        //    return View(pvm);
        //}


        [HttpGet]
        public ActionResult EditPerson(int? id, bool? itemSaved)
        {
            var person = _personService.GetById(id.Value);

            if (!person.StartDate.HasValue)
                person.StartDate = DateTime.Today;

            if (!person.EndDate.HasValue)
                person.EndDate = DateTime.Today;

            Mapper.CreateMap<Person, PersonViewModel>();
            var pvm = Mapper.Map<Person, PersonViewModel>(person);
            pvm.ItemSaved = itemSaved;
            pvm.PersonTypes = GetPersonTypes(person.PersonTypeId);
            return View("NewPerson", pvm);
        }


        [HttpGet]
        public ActionResult EditSupplier(int? id, bool? itemSaved)
        {
            var supplier = _supplierService.GetById(id.Value);
            Mapper.CreateMap<Supplier, PersonViewModel>();
            var pvm = Mapper.Map<Supplier, PersonViewModel>(supplier);
            pvm.ItemSaved = itemSaved;
            pvm.SupplierId = id.Value;
            //pvm.PersonTypes = GetPersonTypes(supplier.PersonTypeId);
            return View("NewSupplier", pvm);
        }


        //


        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]

        public ActionResult GrantInternetAccess(int? id)
        {
            var guest = _guestService.GetById(id.Value);
            Mapper.CreateMap<Person, PersonViewModel>();
            Person person = null;

            ModelState.Clear();

            if(guest.PersonId.HasValue)
                _personService.GetById(guest.PersonId);

            var pvm = Mapper.Map<Person, PersonViewModel>(new Person { Password = "PASSWORD", PersonID = 0, Email = guest.Email, FirstName = guest.FullName, LastName = guest.FullName });

            if (person != null)
            {
                pvm = Mapper.Map<Person, PersonViewModel>(person);
            }
           

            pvm.GuestName = guest.FullName;
            pvm.GuestId = guest.Id;
            return View(pvm);            
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,itemSaved")]

        public ActionResult EditInternetAccess(int? id, bool? itemSaved)
        {
            var guest = _guestService.GetById(id.Value);
            var person = guest.Person;
                        
            Mapper.CreateMap<Person, PersonViewModel>();
            var pvm = Mapper.Map<Person, PersonViewModel>(person);
            pvm.GuestName = guest.FullName;
            pvm.ItemSaved = itemSaved;
            pvm.GuestId = guest.Id;
            return View("GrantInternetAccess", pvm);
        }


        [HttpGet]
        public ActionResult ManageSuppliers()
        {

            var model = new SearchViewModel
            {
                SuppliersList = _supplierService.GetAll(HotelID).Where(x => x.IsActive).ToList()
            };

            return View(model);
        }

       
        [HttpGet]
        public ActionResult ManageUsers()
        {

            var model = new SearchViewModel
            {
                PersonsList = _personService.GetAll(HotelID).Where(x => x.IsActive && x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList()
            };

            return View(model);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]

        public ActionResult GuestCredentials()
        {
            
            var model = new SearchViewModel
            {
                GuestsList = _guestService.GetAll(HotelID).Where(x => x.IsActive).ToList()
            };

            return View(model);
        }


        private IEnumerable<SelectListItem> GetOccupiedRoomAccounts(int? selectedId, int guestRoomId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _guestRoomService.GetAll(HotelID).Where(x => x.IsActive && x.Id != guestRoomId).ToList();

            bas.Insert(0, new GuestRoom {  RoomNumber = "-- Please Select --", Id = 0 });

            return bas.Select(x => new SelectListItem { Text = x.RoomNumber, Value = x.Id.ToString(), Selected = x.Id == selectedId });
        }

       

        [HttpPost]
        public ActionResult EditGuestAccountSwitch(int? gId, int? newGuestRoomId)
        {
            var guestAccount = _guestRoomAccountService.GetById(gId.Value);

            if(guestAccount != null)
            {
                var newGuestRoom = _guestRoomService.GetById(newGuestRoomId.Value);

                if(newGuestRoom != null)
                {
                    guestAccount.GuestRoomId = newGuestRoomId.Value;
                    _guestRoomAccountService.Update(guestAccount);
                }

            }

            return RedirectToAction("AdjustGuestBill");
        }


        [HttpGet]
        public ActionResult SwitchGuestAccount(int? id)
        {
            var guestAccount = _guestRoomAccountService.GetById(id.Value);

            var gravm = new GuestRoomAccountViewModel
            {
                gId = id.Value,
                Guest = guestAccount.GuestRoom.Guest,
                GuestRoomAccount = guestAccount,
                AdjustedAmount = decimal.Zero,
                Notes = string.Empty,
                Room = guestAccount.GuestRoom.Room,
                RoomAccounts = GetOccupiedRoomAccounts(null, guestAccount.GuestRoomId),
                PaymentMethodId = guestAccount.PaymentMethodId,
                PaymentMethodNote = guestAccount.PaymentMethodNote,
                PaymentTypeId = guestAccount.PaymentTypeId
            };

            return View(gravm);
        }


        [HttpGet]
        public ActionResult EditGuestAccount(int? id)
        {
            var guestAccount = _guestRoomAccountService.GetById(id.Value);

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guestAccount.GuestRoom.Guest,
                GuestRoomAccount = guestAccount,
                AdjustedAmount = decimal.Zero,
                Notes = string.Empty,
                Room = guestAccount.GuestRoom.Room,
                PaymentMethodId = guestAccount.PaymentMethodId,
                PaymentMethodNote = guestAccount.PaymentMethodNote,
                PaymentTypeId = guestAccount.PaymentTypeId
            };

            return View(gravm);
        }


        private bool SendSMSToOwner(string dest, string source, string msg)
        {
            dest = GetOwnersTelephone();

            if (!dest.StartsWith("+234") || string.IsNullOrEmpty(dest))
                return false;

            var telephones = dest.Split(',').ToList();

            foreach (var tel in telephones)
            {

                var strSms = @"http://www.easysmsconnect.com/components/com_spc/smsapi.php?username=echohomes&password=adminsms&sender=+2347069350338&recipient=#USER#&message=#MESSAGE#";
                strSms = strSms.Replace("#MESSAGE#", msg);
                strSms = strSms.Replace("#USER#", tel);

                try
                {
                    var _client = new WebClient();
                    var _stackContent = _client.DownloadString(strSms);
                    return true;
                }
                catch
                {
                }
            }

            return true;
        }

    

        private string GetOwnersTelephone()
        {
            //
            var ownersTelephone = string.Empty;

            try
            {
                ownersTelephone = ConfigurationManager.AppSettings["OwnersTelephone"].ToString();
            }
            catch
            {
                ownersTelephone = "";
            }

            return ownersTelephone;
        }

        private bool IsSMSEnabled()
        {

            var sendSMS = false;

            try
            {
                string smsplus = ConfigurationManager.AppSettings["SMSMesagingEnabled"].ToString();

                if (smsplus == "1")
                    sendSMS = true;

            }
            catch
            {
                sendSMS = false;
            }

            return sendSMS;
        }


        [HttpPost]
        public ActionResult EditGuestAccount(GuestRoomAccountViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            PaymentMethod pm = null;
            
            if(ModelState.IsValid)
            {
                if (paymentMethodId.HasValue)
                    pm = _paymentMethodService.GetById(paymentMethodId.Value);

                var existingGuestRoomAccount = _guestRoomAccountService.GetById(model.GuestRoomAccount.Id);

                existingGuestRoomAccount.PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : (int)PaymentMethodEnum.Cash;

                var strMessage = string.Empty;

                if (model.AdjustedAmount != existingGuestRoomAccount.Amount)
                {
                    strMessage = "A bill adjustment was made to Rm " + existingGuestRoomAccount.GuestRoom.Room.RoomNumber + " by " + User.Identity.Name + ", The bill was adjusted from NGN" + existingGuestRoomAccount.Amount + " to NGN " + model.AdjustedAmount.ToString();
                }
                else
                {
                    if (null != pm)
                        strMessage = "A bill adjustment was made to Rm " + existingGuestRoomAccount.GuestRoom.Room.RoomNumber + " by " + User.Identity.Name + ", The bill was adjusted from " + GetPaymentMethod(existingGuestRoomAccount) + " to " + pm.Name;
                }

                if (!string.IsNullOrEmpty(paymentMethodNote))
                    existingGuestRoomAccount.PaymentMethodNote = paymentMethodNote;

                existingGuestRoomAccount.Amount = model.AdjustedAmount;

                //existingGuestRoomAccount.PaymentTypeId = model.PaymentTypeId;
               
                _guestRoomAccountService.Update(existingGuestRoomAccount);


                if (!string.IsNullOrEmpty(strMessage))
                    SendSMSToOwner("", "", strMessage);

                return RedirectToAction("ViewGuestAccount", "GuestAccount", new { id = existingGuestRoomAccount.GuestRoom.GuestId, roomId = existingGuestRoomAccount.GuestRoomId });
            }

            var guestAccount = _guestRoomAccountService.GetById(model.GuestRoomAccount.Id);

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guestAccount.GuestRoom.Guest,
                GuestRoomAccount = guestAccount,
                AdjustedAmount = decimal.Zero,
                Notes = string.Empty,
                Room = guestAccount.GuestRoom.Room,
                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : (int)PaymentMethodEnum.Cash,
                PaymentMethodNote = paymentMethodNote

            };

            return View(gravm);           
        }

        private string GetPaymentMethod(GuestRoomAccount existingGuestRoomAccount)
        {
            if (existingGuestRoomAccount.PaymentMethod != null)
                return existingGuestRoomAccount.PaymentMethod.Name;
            else
            {
                if(existingGuestRoomAccount.PaymentMethodId ==  (int)PaymentMethodEnum.Cash)
                    return "CASH";
                else if(existingGuestRoomAccount.PaymentMethodId ==  (int)PaymentMethodEnum.Cheque)
                    return "CHEQUE";
                else if(existingGuestRoomAccount.PaymentMethodId ==  (int)PaymentMethodEnum.CreditCard)
                    return "POS";
                else if(existingGuestRoomAccount.PaymentMethodId ==  (int)PaymentMethodEnum.POSTBILL)
                    return "POSTBILL";
            }

            return "CASH";
        }


        
        [HttpGet]
        public ActionResult AdjustRoomRate(int? id, int? roomId)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }           

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                GuestRoom = mainGuestRoom,
                AdjustedAmount = decimal.Zero,
                Notes = string.Empty,
                Room = mainGuestRoom.Room
            };

            return View(gravm);
       
        }


        [HttpPost]
        public ActionResult AdjustRoomRate(GuestRoomAccountViewModel model)
        {

            if (ModelState.IsValid)
            {
                var existingGuestRoom = _guestRoomService.GetById(model.GuestRoom.Id);
                existingGuestRoom.RoomRate = model.AdjustedAmount;
                _guestRoomService.Update(existingGuestRoom);
                return RedirectToAction("ViewGuestAccount", "GuestAccount", new { id = existingGuestRoom.GuestId, roomId = existingGuestRoom.RoomId });
            }

            var guestRoom = _guestRoomService.GetById(model.GuestRoom.Id);

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guestRoom.Guest,
                GuestRoom = guestRoom,
                AdjustedAmount = decimal.Zero,
                Notes = string.Empty,
                Room = guestRoom.Room
            };

            return View(gravm);
        }


        [HttpGet]
        public ActionResult AdjustBill(int? id, int? roomId)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var paymentTypeList = new List<RoomPaymentType>();

            int? paymentTypeId = (int)RoomPaymentTypeEnum.Miscellenous;

            var pt = GetPaymentType(paymentTypeId.Value);

            paymentTypeList.Add(new RoomPaymentType { Description = pt, Id = paymentTypeId.Value, Name = pt });

            var gravm = new GuestRoomAccountViewModel
            {
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = (int)RoomPaymentTypeEnum.Miscellenous,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                PaymentTypeList = paymentTypeList
            };

            return View(gravm);
        }


        //AdjustGuestBill
        //[OutputCache(Duration = 3600, VaryByParam = "none")]

        public ActionResult AdjustGuestBill()
        {
            var model = new SearchViewModel
            {
                GuestRoomsList = _guestRoomService.GetAll(HotelID).Where(x => (x.IsActive) && x.Room.StatusId == (int)RoomStatusEnum.Occupied).ToList()
            };

            return View(model);
        }

        private void TransferGuestToAnEmptyRoom(Room newRoom, GuestRoom previousGuestRoom, Room previousRoom, Guest guest)
        {
            var veryInitialCheckinDate = previousGuestRoom.CheckinDate;
            
            var previousRoomId = previousRoom.Id;

            var todaysDate = DateTime.Now;

            var dtToday = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, todaysDate.Minute, todaysDate.Second);

            var gr = new GuestRoom
                {
                    IsActive = true,
                    RoomRate = newRoom.Price.Value,
                    RoomId = newRoom.Id,
                    Notes = previousGuestRoom.Notes,
                    Occupants = previousGuestRoom.Occupants,
                    //CheckinDate = DateTime.Now,
                    CheckinDate = dtToday,
                    CheckoutDate = previousGuestRoom.CheckoutDate,
                    Children = previousGuestRoom.Children,
                    GuestId = guest.Id,
                    GroupBookingMainRoom = previousGuestRoom.GroupBookingMainRoom,
                    GroupBooking = previousGuestRoom.GroupBooking
                };

            guest.GuestRooms.Add(gr);

            var notes = CreateTransferNote(previousGuestRoom.Guest, guest);

            var rth = new RoomTransferHistory
            {
                OldRoomNumber = previousGuestRoom.Room.RoomNumber,
                OldGuestName = previousGuestRoom.Guest.FullName,
                IsActive = true,
                NewGuestId = guest.Id,
                NewRoomId = newRoom.Id,
                OldGuestId = guest.Id,
                OldRoomId = previousRoomId,
                Notes = notes,
                TransferDate = DateTime.Now
            };
            guest.RoomTransferHistories.Add(rth);
            _guestService.Update(guest);

            previousRoom.StatusId = (int)RoomStatusEnum.Dirty;
            _roomService.Update(previousRoom);
            newRoom.StatusId = (int)RoomStatusEnum.Occupied;
            _roomService.Update(newRoom);

            previousGuestRoom.IsActive = false;

            var setToYesterdaysDate = false;

            if (gr.CheckinDate.Day == previousGuestRoom.CheckinDate.Day && gr.CheckinDate.Month == previousGuestRoom.CheckinDate.Month && gr.CheckinDate.Year == previousGuestRoom.CheckinDate.Year)
            {
                previousGuestRoom.CheckinDate = todaysDate;
                previousGuestRoom.SameDayTransfer = true;
                previousGuestRoom.RoomRate = decimal.Zero;
            }
            else
            {
                var dtTodayCheckin = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, 0, 1);
                var elapsedHoursFromCheckin = dtTodayCheckin.Subtract(veryInitialCheckinDate).TotalHours;

                if(elapsedHoursFromCheckin <= 24)
                {
                    previousGuestRoom.CheckinDate = todaysDate;
                    previousGuestRoom.SameDayTransfer = true;
                    previousGuestRoom.RoomRate = decimal.Zero;
                }
                else
                {
                    if (DateTime.Now < dtTodayCheckin)
                    {
                        setToYesterdaysDate = true;
                    }

                }
            }

            //var exactCheckoutDate = new DateTime(dtCheckoutDate.Year, dtCheckoutDate.Month, dtCheckoutDate.Day, _hotelAccountsTime, 0, 0);

            if (setToYesterdaysDate)
            {
                var earlyCheckOutDate = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, 0, 0).AddDays(-1);

                previousGuestRoom.CheckoutDate = earlyCheckOutDate; // Yesterdays Date
                var previousExistingGuestRoom = _guestRoomService.GetById(previousGuestRoom.Id);
                previousExistingGuestRoom = PopulateModel(previousExistingGuestRoom, previousGuestRoom);
                previousExistingGuestRoom.CheckoutDate = previousGuestRoom.CheckoutDate;
                _guestRoomService.Update(previousExistingGuestRoom);

                //Get the previous Reservation, update the start,end dates and new roomid

                var updateGuestRoom = _guestRoomService.GetById(gr.Id);
                updateGuestRoom.CheckinDate = earlyCheckOutDate.AddMinutes(30);
                _guestRoomService.Update(updateGuestRoom);

                var guestReservation = _guestReservationService.GetAll(HotelID).FirstOrDefault(x => x.GuestId == guest.Id && x.RoomId == previousRoomId && x.IsActive);

                if (guestReservation == null) return;
                guestReservation.StartDate = updateGuestRoom.CheckinDate;
                guestReservation.EndDate =   updateGuestRoom.CheckoutDate;
                guestReservation.IsActive = true;
                guestReservation.RoomId = newRoom.Id;
                _guestReservationService.Update(guestReservation);

            }
            else
            {

                var earlyCheckOutDate = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, 0, 0);

                previousGuestRoom.CheckoutDate = earlyCheckOutDate;

                var previousExistingGuestRoom = _guestRoomService.GetById(previousGuestRoom.Id);
                previousExistingGuestRoom = PopulateModel(previousExistingGuestRoom, previousGuestRoom);
                _guestRoomService.Update(previousExistingGuestRoom);

                //Get the previous Reservation, update the start,end dates and new roomid

                var guestReservation = _guestReservationService.GetAll(HotelID).FirstOrDefault(x => x.GuestId == guest.Id && x.RoomId == previousRoomId && x.IsActive);

                if (guestReservation == null) return;
                guestReservation.StartDate = gr.CheckinDate;
                guestReservation.EndDate = previousGuestRoom.CheckoutDate;
                guestReservation.IsActive = true;
                guestReservation.RoomId = newRoom.Id;
                _guestReservationService.Update(guestReservation);
            }
        }

        private GuestRoom PopulateModel(GuestRoom previousExistingGuestRoom, GuestRoom previousGuestRoom)
        {
            previousExistingGuestRoom.CheckinDate = previousGuestRoom.CheckinDate;
            previousExistingGuestRoom.CheckoutDate = previousGuestRoom.CheckoutDate;
            previousExistingGuestRoom.IsActive = previousGuestRoom.IsActive;
            previousExistingGuestRoom.SameDayTransfer = previousGuestRoom.SameDayTransfer;
            previousExistingGuestRoom.RoomRate = previousGuestRoom.RoomRate;
            return previousExistingGuestRoom;
        }

        private void TransferGuestBillToOccupiedRoom(Room newRoom, GuestRoom previousGuestRoom, Room previousRoom, Guest guest)
        {
            var veryInitialCheckinDate = previousGuestRoom.CheckinDate;

            var previousRoomGuestId = previousGuestRoom.GuestId;
            var previousRoomId = previousRoom.Id;
            var notes = CreateTransferNote(previousGuestRoom.Guest, guest);

            var existingGuest = _guestService.GetById(guest.Id);

            var gr = existingGuest.GuestRooms.FirstOrDefault();

            //Create Room Transfer History
            var rth = new RoomTransferHistory { OldGuestName = previousGuestRoom.Guest.FullName, OldRoomNumber = previousGuestRoom.Room.RoomNumber, IsActive = true, NewGuestId = guest.Id, NewRoomId = newRoom.Id, OldGuestId = previousRoomGuestId, OldRoomId = previousRoomId, Notes = notes, TransferDate = DateTime.Now };
            existingGuest.RoomTransferHistories.Add(rth);
            _guestService.Update(existingGuest);

            var existingPreviousGuest = _guestService.GetById(previousRoomGuestId);
            existingPreviousGuest.IsActive = false;
            existingPreviousGuest.Status = "OUT";

            if (existingPreviousGuest.Person != null)
            {
                existingPreviousGuest.Person.IsActive = false;
                existingPreviousGuest.Person.Username = "XXXXXXXX";
                existingPreviousGuest.Person.Password = "XXXXXXXX";
                existingPreviousGuest.Person.Email = "XXXXXXXX";
            }

            _guestService.Update(existingPreviousGuest);

            //Previous Room
            var existingPreviousRoom = _roomService.GetById(previousRoom.Id);
            existingPreviousRoom.StatusId = (int)RoomStatusEnum.Dirty;
            _roomService.Update(existingPreviousRoom);

            //Previous Guest Room

            var previousExistingGuestRoom = _guestRoomService.GetById(previousGuestRoom.Id);

            previousExistingGuestRoom.IsActive = false;

            var setToYesterdaysDate = false;

            var todaysDate = DateTime.Now;

            var dtToday = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, todaysDate.Minute, todaysDate.Second);


            if (gr.CheckinDate.Day == previousGuestRoom.CheckinDate.Day && gr.CheckinDate.Month == previousGuestRoom.CheckinDate.Month && gr.CheckinDate.Year == previousGuestRoom.CheckinDate.Year)
            {
                previousGuestRoom.CheckinDate = todaysDate;
                previousGuestRoom.SameDayTransfer = true;
                previousGuestRoom.RoomRate = decimal.Zero;
            }
            else
            {
                var dtTodayCheckin = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, 0, 1);
                var elapsedHoursFromCheckin = dtTodayCheckin.Subtract(veryInitialCheckinDate).TotalHours;

                if (elapsedHoursFromCheckin <= 24)
                {
                    previousGuestRoom.CheckinDate = todaysDate;
                    previousGuestRoom.SameDayTransfer = true;
                    previousGuestRoom.RoomRate = decimal.Zero;
                }
                else
                {
                    if (DateTime.Now < dtTodayCheckin)
                    {
                        setToYesterdaysDate = true;
                    }

                }
            }


            if (setToYesterdaysDate)
            {
                var earlyCheckOutDate = new DateTime(todaysDate.Year, todaysDate.Month, todaysDate.Day, _hotelAccountsTime, 0, 0);

                previousExistingGuestRoom.CheckoutDate = earlyCheckOutDate; // Yesterdays Date
                previousExistingGuestRoom.GuestId = existingGuest.Id;
                _guestRoomService.Update(previousExistingGuestRoom);

                //Modify Previous Reservation

                var guestReservation = previousRoom.GuestReservations.FirstOrDefault(x => x.GuestId == previousRoomGuestId && x.IsActive);

                if (guestReservation != null)
                {
                    var existingguestReservation = _guestReservationService.GetById(guestReservation.Id);
                    existingguestReservation.IsActive = false;
                    existingguestReservation.GuestId = existingGuest.Id;
                    _guestReservationService.Update(existingguestReservation);
                }
            }
            else
            {
                previousExistingGuestRoom.CheckoutDate = DateTime.Now;
                previousExistingGuestRoom.GuestId = existingGuest.Id;
                _guestRoomService.Update(previousExistingGuestRoom);

                //Modify Previous Reservation

                var guestReservation = previousRoom.GuestReservations.FirstOrDefault(x => x.GuestId == previousRoomGuestId && x.IsActive);
                if (guestReservation != null)
                {
                    var existingguestReservation = _guestReservationService.GetById(guestReservation.Id);
                    existingguestReservation.IsActive = false;
                    existingguestReservation.GuestId = existingGuest.Id;
                    _guestReservationService.Update(existingguestReservation);
                }

            }

             
        }

        private string CreateTransferNote(Guest previousGuest, Guest newGuest)
        {
            if (newGuest == null || previousGuest == null)
                return string.Empty;

            return previousGuest.FullName + "'s Bill was transferred from " + previousGuest.GuestRooms.FirstOrDefault().Room.RoomNumber + " to " + newGuest.FullName + "'s Bill on " + DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id, previousRoomId, guestId, CheckinDate,CheckoutDate")]
        public ActionResult TransferGuestToRoom(int? id, int? previousRoomId, int? guestId, DateTime? CheckinDate, DateTime? CheckoutDate)
        {
            var newRoom = _roomService.GetById(id.Value);
            var guest = _guestService.GetById(guestId.Value);
            var previousGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == previousRoomId.Value);
            var previousRoom = _roomService.GetById(previousRoomId.Value);            

            if (newRoom.GuestRooms.Any(x => x.IsActive))
            {
                var newGuest = newRoom.GetActualRoomGuest();
                TransferGuestBillToOccupiedRoom(newRoom, previousGuestRoom, previousRoom, newGuest);
            }
            else
            {
                TransferGuestToAnEmptyRoom(newRoom, previousGuestRoom, previousRoom, guest);
            }

            return RedirectToAction("EditBookingAdjustable", "Booking", new { id = newRoom.Id, guestTransferComplete = true });
            //For now this transfer only handles tranfer to a new Room, if that
        }
        
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id, roomId, arrive, depart, room_select")]
        public ActionResult TransferGuest(int? id, int? roomId, DateTime? arrive, DateTime? depart, int? room_select)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var guestRoomIds = guest.GuestRooms.Select(x => x.RoomId).ToList();

            if (!arrive.HasValue) arrive = mainGuestRoom.CheckinDate;
            if (!depart.HasValue) depart = mainGuestRoom.CheckoutDate;
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                ids.AddRange(guestRoomIds);
                var bookableRooms = _roomService.GetAll(HotelID).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList();
                var occupiedRooms = _roomService.GetAll(HotelID).Where(x => x.StatusId == (int)RoomStatusEnum.Occupied && !guestRoomIds.Contains(x.Id)).ToList();
                bookableRooms.AddRange(occupiedRooms);
                var model = new RoomBookingViewModel { RoomsList = bookableRooms, GuestId = id.Value, RoomId = roomId.Value };
                return View(model);
            }
            else
            {
                var ids = guestRoomIds;
                var model = new RoomBookingViewModel { RoomsList = _roomService.GetAll(HotelID).Where(x => !ids.Contains(x.Id)).ToList(), GuestId = id.Value, RoomId = roomId.Value };
                return View(model);
            }           
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId,amountToPay")]
        public ActionResult TopUpAccountFromCheckout(int? id, int? roomId, decimal? amountToPay)
        {
            var guest = _guestService.GetById(id.Value);

            if (!amountToPay.HasValue)
            {
                amountToPay = guest.GetGuestBalanceWithFullTax();

                if (amountToPay < 0)
                    amountToPay = decimal.Negate(amountToPay.Value);
            }

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var gravm = new GuestRoomAccountViewModel
            {
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = amountToPay ?? decimal.Zero }
            };

            return View("TopUpAccount", gravm);
        }


        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId,amountToPay,itemSaved")]

        public ActionResult TopUpAccount(int? id, int? roomId, decimal? amountToPay, bool? itemSaved, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(id.Value);

            if (!amountToPay.HasValue)
            {
                amountToPay = decimal.Zero;               
            }

            GuestRoom mainGuestRoom = null;
           
            if(roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var gravm = new GuestRoomAccountViewModel
            {
                Room = mainGuestRoom.Room,
                ItemSaved = itemSaved,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = amountToPay ?? decimal.Zero },
                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1,
                PaymentMethodNote = paymentMethodNote
            };

            if(itemSaved.HasValue && itemSaved.Value)
            {
                var lastPayment = guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).Where(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit).OrderByDescending(x => x.TransactionDate).FirstOrDefault();
                
                if (lastPayment != null)
                    gravm.GuestAccountId = lastPayment.Id;

            }

            return View(gravm);
        }

        [HttpGet]
        public ActionResult PaymentGuestRoom(int? guestId, int guestRoomId,
            decimal? amount, bool? doRefund, decimal? paid, decimal? total, decimal? subtotal, string tax,
            decimal? taxamount, string discount,
            decimal? discountamount, string resident,
            decimal? residentAmount, string serviceChargeString,
            decimal? serviceCharge, int rememberToChangeMainRoom = 0)
        {
            var guest = _guestService.GetById(guestId.Value);

            var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.Id == guestRoomId);

            if(rememberToChangeMainRoom > 0)
            {
                var mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.Id != guestRoom.Id);
                if(mainGuestRoom != null)
                {
                    var newMainGuestRoom = _guestRoomService.GetById(mainGuestRoom.Id);
                    newMainGuestRoom.GroupBookingMainRoom = true;
                    _guestRoomService.Update(newMainGuestRoom);
                }
            }

            CheckOutGuestWithGuestRoomSQL(guestId.Value, guestRoom.RoomId);

            guest = _guestService.GetById(guestId.Value);

            EmailTemplateModel template = null;

            var guestCreated = guest;

            if (guestCreated != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
            }

            try
            {
                var strMsg = guest.Mobile + " " + guest.FullName + " in Room " + guest.GuestRooms.FirstOrDefault().RoomNumber + " has now been checked out.";

                var strMsgGuest = "Dear " + guest.Mobile + " " + guest.FullName + ", We would like to thank you for staying with us! We hope you enjoyed your stay. We look forward to seeing you soon.";

                //SendSMSToOwner("", "", strMsg);

                //SendSMS(guest.Telephone, strMsgGuest);

                //SendEmailToGuest(template, true);
            }
            catch
            {

            }

            var returnUrl = Url.Action("PrintLandingForGuest", "Home", new { id = guestId.Value }).ToString();

            return Json(new { UrlReturn = returnUrl }, JsonRequestBehavior.AllowGet);
        }

        
        [HttpGet]
        public ActionResult PaymentCompany(int? guestId, int? guestRoomId,
            decimal? amount, bool? doRefund, decimal? paid, decimal? total, decimal? subtotal, string tax,
            decimal? taxamount, string discount,
            decimal? discountamount, string resident,
            decimal? residentAmount, string serviceChargeString,
            decimal? serviceCharge, int? companyId)
        {
            var guest = _guestService.GetById(guestId.Value);

            var grandTotal = guest.GuestRooms.TotalSummation(tax, discount, out taxamount, out discountamount);

            var creditPayments = guest.GuestRooms.PaymentsSummation();

            var outstanding = decimal.Zero;

            int paymentTypeId = (int)RoomPaymentTypeEnum.CashDeposit;

            if (grandTotal > 0)
            {
                doRefund = true;
                paymentTypeId = (int)RoomPaymentTypeEnum.Refund;
            }
            else
            {
                if (grandTotal < 0)
                {
                    grandTotal = decimal.Negate(grandTotal);
                }
            }

            subtotal = guest.GuestRooms.SubTotal();

            total = paid = amount = grandTotal;

            GuestRoom guestRoom = null;

            if (guestRoomId.HasValue && guestRoomId.Value > 0)
            {
                guestRoom = guest.GuestRooms.FirstOrDefault(x => x.Id == guestRoomId.Value);
            }
            else
            {
                guestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ??
                               guest.GuestRooms.FirstOrDefault();
            }
           
            if (guestRoom.BusinessAccounts == null)
                guestRoom.BusinessAccounts = new Collection<BusinessAccount>();

            var ticks = (int)DateTime.Now.Ticks;

            var company = guest.BusinessAccount;

            var allGuestRooms = guest.GuestRooms.ToList();

            foreach (var g in allGuestRooms)
            {
                company.GuestRooms.Add(g);
            }

            try
            {
                guest.BusinessAccount = company;
                _guestService.Update(guest);
            }
            catch (Exception)
            {
            }


            if (guestRoom.GuestRoomAccounts == null) guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();

            var cashier = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID;

            guest.GuestRooms.Add(guestRoom);

            _guestService.Update(guest);

            var receiptNumber = "12345"; // DateTime.Now.ToString();

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            resident = "0%";
            residentAmount = decimal.Zero;

            serviceChargeString = "0%";
            serviceCharge = decimal.Zero;

            int? companyAccountId = 0;

            if (guest.CompanyId.HasValue)
            {
                companyAccountId = guest.CompanyId;
            }

            var distributionPointId = GetDistributionPointId();

            if (CheckOutGuestSQL(guestId.Value))
            {
                if (total.HasValue && total.Value > 0)
                {
                    POSService.StockItemService.UpdateSales(paymentTypeId, guestId.Value, companyAccountId, cashier, 1, guestRoom.Id, conn, (int)PaymentMethodEnum.POSTBILL, "PAID AT CHECKOUT",
                     DateTime.Now, distributionPointId, true, receiptNumber, total.Value, subtotal.Value, tax, taxamount.Value, discount, discountamount.Value, resident, residentAmount.Value,
                     serviceChargeString, serviceCharge.Value, paid.Value, outstanding, cashier);
                }
            }

            guest = _guestService.GetById(guestId.Value);

            EmailTemplateModel template = null;

            var guestCreated = guest;

            if (guestCreated != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
            }

            try
            {
                var strMsg = guest.Mobile + " " + guest.FullName + " in Room " + guest.GuestRooms.FirstOrDefault().RoomNumber + " has now been checked out.";

                var strMsgGuest = "Dear " + guest.Mobile + " " + guest.FullName + ", We would like to thank you for staying with us! We hope you enjoyed your stay. We look forward to seeing you soon.";

                //SendSMSToOwner("", "", strMsg);

                //SendSMS(guest.Telephone, strMsgGuest);

                //SendEmailToGuest(template, true);
            }
            catch
            {
            }

            var returnUrl = Url.Action("PrintLandingForGuest", "Home", new { id = guestId.Value }).ToString();

            return Json(new { UrlReturn = returnUrl }, JsonRequestBehavior.AllowGet);

        }


        [HttpGet]
        public ActionResult Payment(int? guestId, int? guestRoomId,
            decimal? amount, bool? doRefund, decimal? paid, decimal? total, decimal? subtotal, string tax,
            decimal? taxamount, string discount,
            decimal? discountamount, string resident,
            decimal? residentAmount, string serviceChargeString,
            decimal? serviceCharge, int? rememberToChangeMainRoom)
        {
            if(guestRoomId.HasValue)
            {
                if(!rememberToChangeMainRoom.HasValue)
                {
                    rememberToChangeMainRoom = 0;
                }
                return PaymentGuestRoom(guestId, guestRoomId.Value, amount, doRefund, paid, total, subtotal, tax, taxamount, discount, discountamount,
                    resident, residentAmount, serviceChargeString, serviceCharge, rememberToChangeMainRoom.Value);
            }

            var guest = _guestService.GetById(guestId.Value);

            var grandTotal = guest.GuestRooms.TotalSummation(tax, discount, out taxamount, out discountamount);

            var creditPayments = guest.GuestRooms.PaymentsSummation();

            var outstanding = decimal.Zero;

            int paymentTypeId = (int)RoomPaymentTypeEnum.CashDeposit;

            if (grandTotal > 0)
            {
                doRefund = true;
                paymentTypeId = (int)RoomPaymentTypeEnum.Refund;
            }
            else
            {
                if(grandTotal < 0)
                {
                    grandTotal = decimal.Negate(grandTotal);
                }
            }

            subtotal = guest.GuestRooms.SubTotal();

            total = paid = amount = grandTotal;

            GuestRoom guestRoom = null;

            if (guestRoomId.HasValue && guestRoomId.Value > 0)
            {
                guestRoom = guest.GuestRooms.FirstOrDefault(x => x.Id == guestRoomId.Value);
            }
            else
            {
                guestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ??
                               guest.GuestRooms.FirstOrDefault();
            }
            if (guestRoom.GuestRoomAccounts == null) guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();

            var ticks = (int)DateTime.Now.Ticks;

            var cashier = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID;

            if(doRefund.HasValue && doRefund.Value)
            {
                if(grandTotal > 0)
                {
                    GuestLedger gl = new GuestLedger();
                    gl.Amount = grandTotal;
                    gl.GuestId = guest.Id;
                    gl.Cashier = cashier;
                    gl.DateCreated = DateTime.Now;
                    gl.Email = guest.Email;
                    gl.Telephone = guest.Email;
                    gl.RoomPaymentTypeId = (int)RoomPaymentTypeEnum.GuestCredit;
                    gl.IsActive = true;
                    _guestLedgerService.Create(gl);
                }
               
            }
            else
            {
                if (grandTotal > 0)
                {
                    guestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                    {
                        Amount = grandTotal,
                        PaymentTypeId = (int)RoomPaymentTypeEnum.CashDeposit,
                        TransactionDate = DateTime.Now,
                        TransactionId = cashier,
                        PaymentMethodId = (int)PaymentMethodEnum.Cash,
                        PaymentMethodNote = "BALANCE AT CHECKOUT"
                    });
                }
            }

            guest.GuestRooms.Add(guestRoom);
            _guestService.Update(guest);
            var receiptNumber = "12345"; // DateTime.Now.ToString();

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            resident = "0%";
            residentAmount = decimal.Zero;

            serviceChargeString = "0%";
            serviceCharge = decimal.Zero;

            int? companyAccountId = 0;

            if(guest.CompanyId.HasValue)
            {
                companyAccountId = guest.CompanyId;
            }

            var distributionPointId = GetDistributionPointId();
           
            if (CheckOutGuestSQL(guestId.Value))
            {
                if(total.HasValue && total.Value > 0)
                {
                    POSService.StockItemService.UpdateSales(paymentTypeId, guestId.Value, companyAccountId, cashier, 1, guestRoom.Id, conn, (int)PaymentMethodEnum.Cash, "PAID AT CHECKOUT",
                     DateTime.Now, distributionPointId, true, receiptNumber, total.Value, subtotal.Value, tax, taxamount.Value, discount, discountamount.Value, resident, residentAmount.Value,
                     serviceChargeString, serviceCharge.Value, paid.Value, outstanding, cashier);
                }
            }

            guest = _guestService.GetById(guestId.Value);

            EmailTemplateModel template = null;

            var guestCreated = guest;

            if (guestCreated != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
            }

            try
            {
                var strMsg = guest.Mobile + " " + guest.FullName + " in Room " + guest.GuestRooms.FirstOrDefault().RoomNumber + " has now been checked out.";

                var strMsgGuest = "Dear " + guest.Mobile + " " + guest.FullName + ", We would like to thank you for staying with us! We hope you enjoyed your stay. We look forward to seeing you soon.";

                //SendSMSToOwner("", "", strMsg);

                //SendSMS(guest.Telephone, strMsgGuest);

                //SendEmailToGuest(template, true);
            }
            catch
            {
            }

            var returnUrl = Url.Action("PrintLandingForGuest", "Home", new { id = guestId.Value }).ToString();

            return Json(new { UrlReturn = returnUrl }, JsonRequestBehavior.AllowGet);
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

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        }

        //
        private bool CheckOutGuestWithGuestRoomSQL(int guestId,int guestRoomId)
        {

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("CheckOutGuestCompleteWithGuestRoom", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    myConnection.Open();

                    cmd.Parameters.AddWithValue("@GuestId", guestId);
                    cmd.Parameters.AddWithValue("@RoomId", guestRoomId);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        var st = e.Message;
                        return false;
                    }
                }

                //return true;
            }

            return true;
        }

        private bool CheckOutGuestSQL(int guestId)
        {

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("CheckOutGuestComplete", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    myConnection.Open();

                    cmd.Parameters.AddWithValue("@GuestId", guestId);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch(Exception e)
                    {
                        var st = e.Message;
                        return false;
                    }
                }

                return true;
            }

            return true;
        }

        [HttpPost]
        public ActionResult TopUpAccount(GuestRoomAccountViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            if (ModelState.IsValid)
            {
                var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == model.RoomId);
                if (guestRoom.GuestRoomAccounts == null) guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
                var ticks = (int)DateTime.Now.Ticks;
                
                guestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                {
                    Amount = model.GuestRoomAccount.Amount,
                    PaymentTypeId = (int)RoomPaymentTypeEnum.CashDeposit,
                    TransactionDate = DateTime.Now,
                    TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                    PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                });

                guest.GuestRooms.Add(guestRoom);

                _guestService.Update(guest);

                return RedirectToAction("TopUpAccount", "GuestAccount", new { id = model.Guest.Id, itemSaved = true, paymentMethodId, paymentMethodNote });
            }

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                RoomId = model.RoomId,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = model.GuestRoomAccount.Amount}
            };

            return View(gravm);
        }


        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId,paymentTypeId,page")]
        public ActionResult ViewEntireList(int? id, int? roomId, int? paymentTypeId, int? page)
        {
            const int pageSize = 10;

            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var paymentTypeList = new List<RoomPaymentType>();

            var pt = GetPaymentType(paymentTypeId.Value);

            paymentTypeList.Add(new RoomPaymentType { Description = pt, Id = paymentTypeId.Value, Name = pt });

            var entirelist = _guestRoomAccountService.GetAllForGuestByType(guest.Id, paymentTypeId.Value);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize,id,roomId,paymentTypeId);
            
            var gravm = new GuestRoomAccountViewModel
            {
                PaymentTypeString = pt,
                PaginatedList = paginatedList,
                CurrentPageIndex = 0,
                PreviousPageUrl = string.Empty,
                NextPageUrl = string.Empty,
                PaymentType = pt,
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = paymentTypeId.Value,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                PaymentTypeList = paymentTypeList
            };

            return View(gravm);
        }

        [HttpPost]
        public ActionResult TopUpHalfDay(GuestRoomAccountViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            if (ModelState.IsValid)
            {
                var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == model.RoomId);
                if (guestRoom.GuestRoomAccounts == null)
                    guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
                var ticks = (int)DateTime.Now.Ticks;

                guestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                {
                    Amount = model.GuestRoomAccount.Amount,
                    PaymentTypeId = model.PaymentTypeId,
                    TransactionDate = DateTime.Now,
                    TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                    PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1,
                    PaymentMethodNote = paymentMethodNote
                });

                guest.GuestRooms.Add(guestRoom);

                _guestService.Update(guest);

                return RedirectToAction("TopUpHalfDay", "GuestAccount", new { id = model.Guest.Id, paymentTypeId = model.PaymentTypeId, itemSaved = true, paymentMethodId, paymentMethodNote });
            }

            var pt = GetPaymentType(model.PaymentTypeId);

            var gravm = new GuestRoomAccountViewModel
            {
                PaymentTypeString = pt,
                Guest = guest,
                RoomId = model.RoomId,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = model.GuestRoomAccount.Amount }
            };

            return View(gravm);
        }


        
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId,paymentTypeId,itemSaved")]
        public ActionResult TopUpHalfDay(int? id, int? roomId, int? paymentTypeId, bool? itemSaved, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var paymentTypeList = new List<RoomPaymentType>();

            var pt = GetPaymentType(paymentTypeId.Value);

            paymentTypeList.Add(new RoomPaymentType { Description = pt, Id = paymentTypeId.Value, Name = pt });

            var gravm = new GuestRoomAccountViewModel
            {
                ItemSaved = itemSaved,
                PaymentTypeString = pt,
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = paymentTypeId.Value,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                PaymentTypeList = paymentTypeList,
                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1,
                PaymentMethodNote = paymentMethodNote
            };

            return View(gravm);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId,paymentTypeId,itemSaved")]
        public ActionResult TopUpRestaurant(int? id, int? roomId, int? paymentTypeId, bool? itemSaved, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var paymentTypeList = new List<RoomPaymentType>();

            var pt = GetPaymentType(paymentTypeId.Value);

            paymentTypeList.Add(new RoomPaymentType { Description = pt, Id = paymentTypeId.Value, Name = pt });

            var gravm = new GuestRoomAccountViewModel
            {
                ItemSaved = itemSaved,
                PaymentTypeString = pt,
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = paymentTypeId.Value,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                PaymentTypeList = paymentTypeList,
                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1,
                PaymentMethodNote = paymentMethodNote
            };

            return View(gravm);
        }

        private string GetPaymentType(int paymentType)
        {
            if (paymentType == (int)RoomPaymentTypeEnum.Bar)
            {
                return "BAR";
            }

            if (paymentType == (int)RoomPaymentTypeEnum.Restuarant)
            {
                return "RESTAURANT";
            }

            if (paymentType == (int)RoomPaymentTypeEnum.Laundry)
            {
                return "LAUNDRY/CAFE/TEL";
            }

            if (paymentType == (int)RoomPaymentTypeEnum.HalfDay)
            {
                return "Half Day";
            }

            return "MISCELLANEOUS";
        }

        [HttpPost]
        public ActionResult TopUpRestaurant(GuestRoomAccountViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            if (ModelState.IsValid)
            {
                var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == model.RoomId);
                if (guestRoom.GuestRoomAccounts == null)
                    guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
                var ticks = (int) DateTime.Now.Ticks;

                guestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                    {
                        Amount = model.GuestRoomAccount.Amount,
                        PaymentTypeId = model.PaymentTypeId,
                        TransactionDate = DateTime.Now,
                        TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                        PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                    });

                guest.GuestRooms.Add(guestRoom);

                _guestService.Update(guest);

                return RedirectToAction("TopUpRestaurant", "GuestAccount", new { id = model.Guest.Id, paymentTypeId = model.PaymentTypeId, itemSaved = true, paymentMethodId, paymentMethodNote });
            }

            var pt = GetPaymentType(model.PaymentTypeId);

            var gravm = new GuestRoomAccountViewModel
                {
                    PaymentTypeString = pt,
                    Guest = guest,
                    RoomId = model.RoomId,
                    Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                    GuestRoomAccount = new GuestRoomAccount {Amount = model.GuestRoomAccount.Amount}
                };

            return View(gravm);
        }

        private List<Room> GetGuestRooms(IEnumerable<GuestRoom> collection)
        {
            var rmList = collection.Select(x => x.Room).ToList();

            if(rmList.Count > 1)
                rmList.Add( new Room{ Id = 0, RoomNumber =  "--Pls Select--"});

            return rmList;
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,guestRoomId")]

        public ActionResult CheckOutGroupGuest(int? id, int? guestRoomId, bool mainGuest)
        {
            var guest = _guestService.GetById(id.Value);

            var mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom);

            var guestRoomForCheckOut = guest.GuestRooms.FirstOrDefault(x => x.Id == guestRoomId.Value).RoomId;

            //if (mainGuestRoom.RoomId == guestRoomForCheckOut)
            //{
            //    return RedirectToAction("CheckOutEntireGroup", "GuestAccount", new { id = id.Value, roomId = mainGuestRoom.RoomId });
            //}

            var roomIdValues = guest.GuestRooms.Select(x => x.Room.Id.ToString()).ToDelimitedString(",");

            var gbvm = new GroupBookingViewModel
                {
                  
                    Guest = guest,
                    GroupCheckOut = false,
                    Room = mainGuestRoom.Room,
                    //GuestRoomsCheckedIn = guest.GuestRooms.Where(x => x.Id != mainGuestRoom.Id && x.IsActive).ToList(),
                    GuestRoomsCheckedIn = guest.GuestRooms.Where(x => x.IsActive).ToList(),
                    selectedRoomIds = roomIdValues,
                    GuestRoomId = guestRoomForCheckOut,
                    GuestId = id.Value,
                    PageTitle = "GUEST GROUP CHECKOUT"
                };

            return View(gbvm);
        }

        
        [HttpPost]
        public ActionResult CheckOutIndividualGuest(GuestRoomAccountViewModel model)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            var guestRoomForCheckout = _guestRoomService.GetById(model.GuestRoomId);
            var room = guestRoomForCheckout.Room;

            //At some point record the refund if any made to guest
            var reservationIds = room.GuestReservations.Where(x => x.GuestId == guest.Id).Select(x => x.Id).ToList();
            var guestRoomsIds = new List<int> {model.GuestRoomId};
            var roomIds = room.GuestReservations.Where(x => x.GuestId == guest.Id).Select(x => x.RoomId).ToList();

            //Create a new GuestRoom Account for any refund
            foreach (var existingReservation in reservationIds.Select(rsId => _guestReservationService.GetById(rsId)))
            {
                existingReservation.EndDate = DateTime.Now;
                existingReservation.IsActive = false;
                _guestReservationService.Update(existingReservation);
            }

            foreach (var existingGuestRoom in guestRoomsIds.Select(gsId => _guestRoomService.GetById(gsId)))
            {
                existingGuestRoom.CheckoutDate = DateTime.Now;
                existingGuestRoom.IsActive = false;
                _guestRoomService.Update(existingGuestRoom);
            }

            foreach (var existingRoom in roomIds.Select(rmId => _roomService.GetById(rmId)))
            {
                existingRoom.StatusId = (int)RoomStatusEnum.Dirty;
                _roomService.Update(existingRoom);
            }

            //guest.IsActive = false;
            //guest.Status = "PAST";
            //_guestService.Update(guest);

            return RedirectToAction("PrintLandingForGuest", "Home", new { id = model.Guest.Id });
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,guestRoomId")]

        public ActionResult CheckOutIndividualGuest(int? id, int? guestRoomId)
        {
            var guest = _guestService.GetById(id.Value);

            bool thisIsMainPerson = false;
            int rememberToChangeMainRoom = 0;

            GuestRoom mainGuestRoom = null;

            if (guestRoomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.Id == guestRoomId);
                thisIsMainPerson = mainGuestRoom.GroupBookingMainRoom;
            }

            if(thisIsMainPerson && guest.GuestRooms.Where(x => x.IsActive).Count() > 1)
            {
                rememberToChangeMainRoom = 1;
                thisIsMainPerson = false;
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var gravm = new GuestRoomAccountViewModel
            {
                GuestRoomsForCheckout = new List<GuestRoom> { mainGuestRoom },
                Room = mainGuestRoom.Room,
                Guest = guest,
                RememberToChangeMainRoom = rememberToChangeMainRoom,
                RoomId = mainGuestRoom.Room.Id,
                GuestRoomId = guestRoomId.Value,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            if(thisIsMainPerson)
            {
                return View(gravm);
            }
            else
            {
                return View("CheckoutNoBill",gravm);
            }
            
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId")]

        public ActionResult CheckOutEntireGroup(int? id, int? roomId)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var gravm = new GuestRoomAccountViewModel
            {
                GroupCheckout = true,
                GuestRoomId = mainGuestRoom.Id,
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            return View("CheckOutGuest", gravm);
        }

        
        [HttpGet]
        public ActionResult CheckOutGuest(int? id,int? roomId)
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            bool thisIsMainGuest = false;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId && x.GroupBookingMainRoom);

                if (mainGuestRoom != null)
                    thisIsMainGuest = true;
            }

            if (mainGuestRoom == null)
            {
                //mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId) ?? guest.GuestRooms.FirstOrDefault();

            }


            if (mainGuestRoom.GroupBooking && guest.GuestRooms.Count(x => x.IsActive) > 1)
            {
                if (thisIsMainGuest)
                    return RedirectToAction("CheckOutGroupGuest", "GuestAccount", new { id = id.Value, guestRoomId = mainGuestRoom.Id, mainGuest = true });
                else
                    return RedirectToAction("CheckOutGroupGuest", "GuestAccount", new { id = id.Value, guestRoomId = mainGuestRoom.Id, mainGuest = false });
            }

            var gravm = new GuestRoomAccountViewModel
            {  
                CompanyGuest = guest.CompanyId,
                GroupCheckout = true, //even though this is an individual checkout, this guest can be treated as a group with 1 room
                GuestRoomId = mainGuestRoom.Id,
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Where(x => !x.SameDayTransfer).Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            var path1 = Path.Combine(Server.MapPath("~/Products/Receipt/"));

            gravm.DownloadFilename = PDFReceiptPrinter.PrintInvoiceCheckout(path1, guest);

            return View("CheckOutGuestPOS",gravm);
        }


        [HttpPost]
        public ActionResult CheckOutGuestCompanyGuest(GuestRoomAccountViewModel model)
        {
            int? id = model.Guest.Id;
            var guest = _guestService.GetById(id);
            var guestBalance = guest.GetGuestBalanceWithFullTax();

            //At some point record the refund if any made to guest
            var reservationIds = guest.GuestReservations.Select(x => x.Id).ToList();
            var guestRoomsIds = guest.GuestRooms.Select(x => x.Id).ToList();
            var roomIds = guest.GuestReservations.Select(x => x.RoomId).ToList();

            //Create a new GuestRoom Account for any refund

            foreach (var existingReservation in reservationIds.Select(rsId => _guestReservationService.GetById(rsId)))
            {
                existingReservation.EndDate = DateTime.Now;
                existingReservation.IsActive = false;
                _guestReservationService.Update(existingReservation);
            }

            foreach (var existingGuestRoom in guestRoomsIds.Select(gsId => _guestRoomService.GetById(gsId)))
            {
                existingGuestRoom.CheckoutDate = DateTime.Now;
                existingGuestRoom.IsActive = false;
                _guestRoomService.Update(existingGuestRoom);
            }

            var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();

            if (guestRoom.BusinessAccounts == null)
                guestRoom.BusinessAccounts = new Collection<BusinessAccount>();

            var ticks = (int)DateTime.Now.Ticks;

            var company = guest.BusinessAccount;

            var allGuestRooms = guest.GuestRooms.ToList();

            foreach(var g in allGuestRooms)
            {
                company.GuestRooms.Add(g);
            }      

            try
            {
                guest.BusinessAccount = company;
               _guestService.Update(guest);
            }
            catch(Exception)
            {             
            }

            foreach (var existingRoom in roomIds.Select(rmId => _roomService.GetById(rmId)))
            {
                existingRoom.StatusId = (int)RoomStatusEnum.Dirty;
                _roomService.Update(existingRoom);
            }

            guest.IsActive = false;
            guest.Status = "OUT";

            if (guest.Person != null)
            {
                guest.Person.IsActive = false;
                guest.Person.Username = "XXXXXXXX";
                guest.Person.Password = "XXXXXXXX";
                guest.Person.Email = "XXXXXXXX";
            }

            model.Guest = guest;
           

            var guestCreated = _guestService.Update(guest);

            EmailTemplateModel template = null;

            if (guestCreated != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
                //template.ChekinDate = model.GuestRoom.CheckinDate;
                //template.ChekoutDate = model.GuestRoom.CheckoutDate;
                //template.InitialDeposit = model.InitialDeposit;
                //template.RoomNumber = model.GuestRoom.RoomNumber;
            }


            try
            {
                var strMsg = guest.Mobile + " " + guest.FullName + " in Room " + model.Guest.GuestRooms.FirstOrDefault().RoomNumber + " has now been checked out.";
                var strMsgGuest = "Dear " + guest.Mobile + " " + guest.FullName + ", We would like to thank you for staying with us! We hope you enjoyed your stay. We look forward to seeing you soon.";

                _guestService.Update(guest);

                //SendSMSToOwner("", "", strMsg);

                //SendSMS(guest.Telephone, strMsgGuest);

                //SendEmailToGuest(template, true);
            }
            catch
            {

            }
            
           

            return RedirectToAction("PrintLandingForGuest", "Home", new { id = id.Value });
        }

        private string SendSMS(string telephone, string msg)
        {
            var owner = GetOwnersTelephone();

            if (!string.IsNullOrEmpty(telephone) && !string.IsNullOrEmpty(owner))
            {
                if (telephone.StartsWith("0"))
                {
                    telephone = "+234" + telephone.Substring(1);
                }

                //var strSms = @"http://www.easysmsconnect.com/components/com_spc/smsapi.php?username=echohomes&password=adminsms&sender=SMSSENDER&recipient=RECIEPIENT&message=MESSAGE";
                var strSms = @"http://www.easysmsconnect.com/components/com_spc/smsapi.php?username=echohomes&password=adminsms&sender=+2347069350338&recipient=#USER#&message=#MESSAGE#";
                strSms = strSms.Replace("#MESSAGE#", msg);
                strSms = strSms.Replace("#USER#", telephone);

                try
                {
                    var _client = new WebClient();
                    var _stackContent = _client.DownloadString(strSms);
                    return _stackContent;
                }
                catch
                {
                }

            }

            return "";

        }

        

        [HttpPost]
        public ActionResult CheckOutGuest(GuestRoomAccountViewModel model, int? paymentMethodId, string paymentMethodNote, bool? doRefund)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            var guestBalance = guest.GetGuestBalanceWithFullTax();

            //At some point record the refund if any made to guest
            var reservationIds = guest.GuestReservations.Select(x => x.Id).ToList();
            var guestRoomsIds = guest.GuestRooms.Select(x => x.Id).ToList();
            var roomIds = guest.GuestReservations.Select(x => x.RoomId).ToList();

            //Create a new GuestRoom Account for any refund

            if(doRefund.HasValue && doRefund.Value)
            {
                GuestLedger gl = new GuestLedger();
                gl.Amount = Decimal.Negate(guestBalance);
                gl.GuestId = guest.Id;
                gl.Cashier = UserID();
                gl.DateCreated = DateTime.Now;
                gl.Email = guest.Email;
                gl.Telephone = guest.Email;
                gl.RoomPaymentTypeId = (int)RoomPaymentTypeEnum.GuestCredit;
                gl.IsActive = true;
                _guestLedgerService.Create(gl);
            }

            foreach (var existingReservation in reservationIds.Select(rsId => _guestReservationService.GetById(rsId)))
            {
                existingReservation.EndDate = DateTime.Now;
                existingReservation.IsActive = false;
                _guestReservationService.Update(existingReservation);
            }

            foreach (var existingGuestRoom in guestRoomsIds.Select(gsId => _guestRoomService.GetById(gsId)))
            {
                existingGuestRoom.CheckoutDate = DateTime.Now;
                existingGuestRoom.IsActive = false;
                _guestRoomService.Update(existingGuestRoom);
            }

            if (guestBalance > 0)
            {
                var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ??
                                guest.GuestRooms.FirstOrDefault();

                if (guestRoom.GuestRoomAccounts == null)
                    guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();

                var ticks = (int) DateTime.Now.Ticks;

                guestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                    {
                        Amount = guestBalance,
                        PaymentTypeId = (int) RoomPaymentTypeEnum.Refund,
                        TransactionDate = DateTime.Now,
                        TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                        PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                    });

                guest.GuestRooms.Add(guestRoom);
            }

            foreach (var existingRoom in roomIds.Select(rmId => _roomService.GetById(rmId)))
            {
                existingRoom.StatusId = (int) RoomStatusEnum.Dirty;
                _roomService.Update(existingRoom);
            }

            guest.IsActive = false;
            guest.Status = "PAST";

            if (guest.Person != null)
            {
                guest.Person.IsActive = false;
                guest.Person.Username = "XXXXXXXX";
                guest.Person.Password = "XXXXXXXX";
                guest.Person.Email = "XXXXXXXX";
            }


            model.Guest = guest;
            var guestCreated = _guestService.Update(guest);

            EmailTemplateModel template = null;

            if (guestCreated != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
            }

            try
            {
                var strMsg = guest.Mobile + " " + guest.FullName + " in Room " + model.Guest.GuestRooms.FirstOrDefault().RoomNumber + " has now been checked out.";

                var strMsgGuest = "Dear " + guest.Mobile + " " + guest.FullName + ", We would like to thank you for staying with us! We hope you enjoyed your stay. We look forward to seeing you soon.";

                _guestService.Update(guest);

                //SendSMSToOwner("", "", strMsg);

                //SendSMS(guest.Telephone, strMsgGuest);

                //SendEmailToGuest(template, true);
            }
            catch
            {

            }

            return RedirectToAction("PrintLandingForGuest", "Home", new { id = model.Guest.Id });
        }

        private string GetHotelsName()
        {
            //
            var hotelName = string.Empty;

            try
            {
                hotelName = ConfigurationManager.AppSettings["HotelName"].ToString();
            }
            catch
            {
                hotelName = "";
            }

            return hotelName;
        }

        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public void SendEmailToGuest(EmailTemplateModel model, bool checkout = false)
        {
            //string smtpAddress = "smtp.mail.yahoo.com";
            //int portNumber = 587;
            //bool enableSSL = true;

            //string emailFrom = "leboston@yahoo.com";
            //string password = "Afeni280701@@";

            string smtpAddress = "smtp.mail.yahoo.com";
            smtpAddress = "smtp.gmail.com";
            int portNumber = 587;
            bool enableSSL = true;

            string emailFrom = "leboston@yahoo.com";
            emailFrom = "academyvistang@gmail.com";
            string password = "Afeni280701@@";
            password = "Lauren280701";



            string emailTo = model.Guest.Email;
            string subject = "Your Reservation at " + System.Configuration.ConfigurationManager.AppSettings["HotelName"].ToString();
            //string body = "Hello, I'm just writing this to say Hi!";

            var emailTemplate = string.Empty;

            if (checkout)
            {
                emailTemplate = RenderRazorViewToString("_EmailTemplateGuestViewCheckout", model);
            }
            else
            {
                emailTemplate = RenderRazorViewToString("_EmailTemplateGuestView", model);
            }

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = emailTemplate;
                mail.IsBodyHtml = true;
                // Can set to false, if you are sending pure text.

                //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = enableSSL;
                    try
                    {
                        smtp.Send(mail);
                    }
                    catch (Exception ex)
                    {
                        int p = 0;
                    }

                }
            }

        }

        [HttpPost]
        public ActionResult CheckOutGuestCreateDebtorAccount(GuestRoomAccountViewModel model)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            var guestBalance = guest.GetGuestBalanceWithFullTax();

            //At some point record the refund if any made to guest
            var reservationIds = guest.GuestReservations.Select(x => x.Id).ToList();
            var guestRoomsIds = guest.GuestRooms.Select(x => x.Id).ToList();
            var roomIds = guest.GuestReservations.Select(x => x.RoomId).ToList();

            GuestLedger gl = new GuestLedger();
            gl.Amount = Decimal.Negate(guestBalance);
            gl.GuestId = guest.Id;
            gl.Cashier = UserID();
            gl.DateCreated = DateTime.Now;
            gl.Email = guest.Email;
            gl.Telephone = guest.Email;
            gl.RoomPaymentTypeId = (int)RoomPaymentTypeEnum.DebtorsBalance;
            gl.IsActive = true;

            _guestLedgerService.Create(gl);

            //Create a new GuestRoom Account for any refund

            foreach (var existingReservation in reservationIds.Select(rsId => _guestReservationService.GetById(rsId)))
            {
                existingReservation.EndDate = DateTime.Now;
                existingReservation.IsActive = false;
                _guestReservationService.Update(existingReservation);
            }

            foreach (var existingGuestRoom in guestRoomsIds.Select(gsId => _guestRoomService.GetById(gsId)))
            {
                existingGuestRoom.CheckoutDate = DateTime.Now;
                existingGuestRoom.IsActive = false;
                _guestRoomService.Update(existingGuestRoom);
            }

            foreach (var existingRoom in roomIds.Select(rmId => _roomService.GetById(rmId)))
            {
                existingRoom.StatusId = (int)RoomStatusEnum.Dirty;
                _roomService.Update(existingRoom);
            }

            guest.IsActive = false;

            guest.Status = "PAST";

            if(guest.Person != null)
            {
                guest.Person.IsActive = false;
                guest.Person.Username = "XXXXXXXX";
                guest.Person.Password = "XXXXXXXX";
                guest.Person.Email = "XXXXXXXX";
            }

            _guestService.Update(guest);

            return RedirectToAction("PrintLandingForGuest", "Home", new { id = model.Guest.Id });

        }

        private int CreateNewDebtorAccount(Guest guest)
        {
            var creditAccount = new BusinessAccount(); 
            creditAccount.Status = "LIVE";
            creditAccount.Address = guest.Address;
            creditAccount.CompanyAddress = guest.Address;
            creditAccount.CompanyName = guest.FullName;
            creditAccount.CompanyTelephone = guest.Telephone;
            creditAccount.ContactName = guest.FullName;
            creditAccount.ContactNumber = guest.Telephone;
            creditAccount.Email = guest.Email;
            creditAccount.Mobile = guest.Mobile;
            creditAccount.Name = guest.FullName;
            creditAccount.NatureOfBusiness = guest.FullName;
            creditAccount.Telephone = guest.Telephone;
            creditAccount.HotelId = HotelID;
            creditAccount.Debtor = true;
            _businessAccountService.Create(creditAccount);
            return creditAccount.Id;
        }


        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId")]

        public ActionResult ViewGuestAccount(int? id, int? roomId)
        {
            //return RedirectToAction("PrintGuestAccount", "Printing", new {id});

            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var allItemisedItems = guest.SoldItemsAlls.Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).OrderByDescending(x => x.DateSold).ToList();

            var gravm = new GuestRoomAccountViewModel
            {
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                ItemmisedItems = allItemisedItems
            };

            return View(gravm);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,roomId,itemSaved")]
        public ActionResult ExtendGuestStay(int? id, int? roomId, bool? itemSaved )
        {
            var guest = _guestService.GetById(id.Value);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var gravm = new GuestRoomAccountViewModel
            {
                ItemSaved = itemSaved,
                Guest = guest,
                RoomId = mainGuestRoom.RoomId,
                Room = mainGuestRoom.Room,
                NewCheckOutDate = mainGuestRoom.CheckoutDate.AddDays(1),
                PreviousCheckOutDate = mainGuestRoom.CheckoutDate,
                GuestRoom = mainGuestRoom,
                PaymentTypeId = 0,
                Rooms = GetGuestRooms(guest.GuestRooms.ToList()),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            return View(gravm);
        }


        private GuestRoomAccountViewModel PopulateFromRequest(GuestRoomAccountViewModel model)
        {
            DateTime dtNow;
            DateTime dtFuture;

            DateTime.TryParse(Request.Form["arrive"], out dtNow);
            DateTime.TryParse(Request.Form["depart"], out dtFuture);

            //model.GuestRoom.CheckinDate = dtNow;
            //model.GuestRoom.CheckoutDate = dtFuture;
            model.NewCheckOutDate = dtFuture;
           
            return model;
        }

        [HttpPost]
        public ActionResult ExtendGuestStay(GuestRoomAccountViewModel model)
        {
            var guest = _guestService.GetById(model.Guest.Id);

            model = PopulateFromRequest(model);
            var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == model.RoomId);
            var room = guestRoom.Room;

            var conflicts = room.RoomAvailability(guestRoom.CheckinDate, model.NewCheckOutDate, guest.Id);

            if (conflicts.Count > 0)
            {
                ModelState.AddModelError("NewCheckOutDate", "There is a reservation clash with your proposed checkout date");
            }

            if (ModelState.IsValid)
            {
                guestRoom.CheckoutDate = model.NewCheckOutDate;
                var guestReservation = guest.GuestReservations.FirstOrDefault(x => x.RoomId == model.RoomId);
                guestReservation.EndDate = model.NewCheckOutDate;
                guest.GuestRooms.Add(guestRoom);
                guest.GuestReservations.Add(guestReservation);
                _guestService.Update(guest);

                return RedirectToAction("ExtendGuestStay", "GuestAccount", new { id = model.Guest.Id, roomId = model.RoomId, itemSaved = true});
            }

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                RoomId = guestRoom.RoomId,
                Room = guestRoom.Room,
                NewCheckOutDate = guestRoom.CheckoutDate.AddDays(1),
                PreviousCheckOutDate = guestRoom.CheckoutDate,
                GuestRoom = guestRoom,
                PaymentTypeId = 0,
                Rooms = GetGuestRooms(guest.GuestRooms.ToList()),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            return View(gravm);
        }
	}
}