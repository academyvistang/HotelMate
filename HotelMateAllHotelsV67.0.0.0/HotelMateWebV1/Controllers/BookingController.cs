using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR.Messaging;
using System.Net.Mail;
using System.Net;


namespace HotelMateWebV1.Controllers
{
    [Authorize()]
    [HandleError(View = "CustomErrorView")]
    public class BookingController : Controller
    {
         private readonly IRoomService _roomService;
         private readonly IGuestService _guestService;
         private readonly IGuestRoomService _guestRoomService;
         private readonly IGuestReservationService _guestReservationService;
         private readonly IBusinessAccountService _businessAccountService;
         private readonly ITransactionService _transactionService;
         private readonly IItemService _itemService;
         private readonly IGuestRoomAccountService _guestRoomAccountService;
         private readonly IRoomTypeService _roomTypeService;
         private readonly IRoomStatuService _roomStatusService;
         private readonly IPersonService _personService = null;

         private const int HotelCancellationHours = 4;

         private int? _hotelId;
         private int HotelId
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

        public BookingController()
        {
            _personService = new PersonService();
            _itemService = new ItemService();
            _roomService = new RoomService();
            _guestService = new GuestService();
            _guestRoomService = new GuestRoomService();
            _businessAccountService = new BusinessAccountService();
            _guestReservationService = new GuestReservationService();
            _transactionService = new TransactionService();
            _guestRoomAccountService = new GuestRoomAccountService();
            _roomStatusService = new RoomStatuService();
            _roomTypeService = new RoomTypeService();
        }



        public ActionResult GetGuestByName(string name)
        {
            name = name.Trim().ToUpper();

            var guest = _guestService.GetAll(HotelId).FirstOrDefault(x => x.FullName.Trim().ToUpper().Equals(name));

            if (guest != null)
            {
                return Json(new
                {
                    Fullname = guest.FullName,
                    Address = guest.Address,
                    Telephone = guest.Telephone,
                    CarDetails = guest.CarDetails,
                    Email = guest.Email,
                    Notes = guest.Notes,
                    Found = guest == null ? 0 : 1
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    Found = 0
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetGuest(string telephone)
        {
            telephone = telephone.Trim().ToUpper();

            var guest = _guestService.GetAll(HotelId).FirstOrDefault(x => x.Telephone.Trim().ToUpper().Equals(telephone));

            if(guest != null)
            {
                return Json(new
                {
                    Fullname = guest.FullName,
                    Address = guest.Address,
                    Telephone = guest.Telephone,
                    CarDetails = guest.CarDetails,
                    Email = guest.Email,
                    Notes = guest.Notes,
                    Found = guest == null ? 0 : 1
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    Found = 0
                }, JsonRequestBehavior.AllowGet);
            }

            
        }


        [HttpPost]
        public ActionResult NewRoom(RoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                Mapper.CreateMap<GuestViewModel, Guest>();
                var room = Mapper.Map<RoomViewModel, Room>(model);
                room.IsActive = true;

                if (room.Id > 0)
                {
                    var existingRoom = _roomService.GetById(room.Id);
                    existingRoom.StatusId = room.StatusId;
                    _roomService.Update(existingRoom);
                }

                return RedirectToAction("Booking", "Home");
            }

            model.RoomTypeList = GetRoomTypes(model.RoomType);
            model.RoomStatusList = GetRoomStatus(model.StatusId);

            return View(model);
        }

        public void SendEmailToGuestReservation(EmailTemplateModel model)
        {
            string smtpAddress = "smtp.mail.yahoo.com";
            int portNumber = 587;
            bool enableSSL = true;

            string emailFrom = "leboston@yahoo.com";
            string password = "Afeni280701@@";
            string emailTo = model.Guest.Email;
            string subject = "Your Reservation at " + System.Configuration.ConfigurationManager.AppSettings["HotelName"].ToString();
            //string body = "Hello, I'm just writing this to say Hi!";

            var emailTemplate = string.Empty;

            emailTemplate = RenderRazorViewToString("_EmailTemplateGuestViewReservation", model);

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



        public void SendEmailToGuest(EmailTemplateModel model, bool checkout = false)
        {
            string smtpAddress = "smtp.mail.yahoo.com";
            smtpAddress = "smtp.gmail.com";
            int portNumber = 587;
            bool enableSSL = true;

            string emailFrom = "leboston@yahoo.com";
            emailFrom = "academyvistang@gmail.com";
            string password = "Afeni280701@@";
            password = "Lauren280701";
            string emailTo = model.Guest.Email.Trim();
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

                //SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network

                using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                {
                    smtp.Credentials = new NetworkCredential(emailFrom, password);
                    smtp.EnableSsl = enableSSL;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
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


        public void SendEmail(EmailTemplateModel model)
        {
            var dest = GetOwnersEmail();

            //dest = "leboston@yahoo.com";

            var strVacantReport = GetVacancyStatusVacant();
            var strOccupiedReport = GetVacancyStatusOccupied();
            var strDirtyReport = GetVacancyStatusDirty();

            model.Vacancy = strVacantReport;
            model.Occupied = strOccupiedReport;
            model.Dirty = strDirtyReport;

            if (string.IsNullOrEmpty(dest))
                return;

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


            string emailTo = dest;
            string subject = "A New Guest At Your Hotel" + model.Guest.FullName + "" + model.Guest.Email;
            //string body = "Hello, I'm just writing this to say Hi!";

            var emailTemplate = RenderRazorViewToString("_EmailTemplateNewGuest", model);

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
       

        public void SendEmailB4B4(EmailTemplateModel model)
        {
            var dest = GetOwnersEmail();

            //dest = "leboston@yahoo.com";

            var strVacantReport = GetVacancyStatusVacant();
            var strOccupiedReport = GetVacancyStatusOccupied();
            var strDirtyReport = GetVacancyStatusDirty();

            model.Vacancy = strVacantReport;
            model.Occupied = strOccupiedReport;
            model.Dirty = strDirtyReport;

            if (string.IsNullOrEmpty(dest))
                return;

            try
            {
                var emails = dest.Split(',').ToList();

                foreach (var email in emails)
                {
                    var emailTemplate = RenderRazorViewToString("_EmailTemplateNewGuest", model);
                    MailMessage mail = new MailMessage("academyvistang@gmail.com", email, "You have a new Guest", emailTemplate);
                    mail.From = new MailAddress("academyvistang@gmail.com", "HotelMate");
                    mail.IsBodyHtml = true; // necessary if you're using html email
                    NetworkCredential credential = new NetworkCredential("academyvistang@gmail.com", "Lauren280701");
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    //smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = credential;
                    smtp.Send(mail);
                }
            }
            catch(Exception ex)
            {
                int p = 90;
            }
        }

        private string GetVacancyStatusVacant()
        {
            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT 'NUMBER OF VACANT ROOMS : ' + CONVERT(varchar(10), Count(*))  FROM ROOM WHERE StatusId = 1", myConnection))
                {

                    myConnection.Open();

                    try
                    {
                        return cmd.ExecuteScalar().ToString();
                    }
                    catch
                    {

                    }
                }
            }

            return "";
        }

        private string GetVacancyStatusDirty()
        {
            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("select 'NUMBER OF DIRTY ROOMS : ' + CONVERT(varchar(10), Count(*))  FROM ROOM WHERE StatusId = 3", myConnection))
                {

                    myConnection.Open();

                    try
                    {
                        return cmd.ExecuteScalar().ToString();
                    }
                    catch
                    {

                    }
                }
            }

            return "";
        }

        private string GetVacancyStatusOccupied()
        {
            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("select 'NUMBER OF OCCUPIED ROOMS : ' + CONVERT(varchar(10), Count(*))  FROM ROOM WHERE StatusId = 2", myConnection))
                {
                    
                    myConnection.Open();

                    try
                    {
                        return cmd.ExecuteScalar().ToString();
                    }
                    catch
                    {

                    }
                }
            }

            return "";
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

        private string SendSMS(string telephone, string msg)
        {
            var owner = GetOwnersTelephone();

            if (!string.IsNullOrEmpty(telephone) && !string.IsNullOrEmpty(owner))
            {
                if(telephone.StartsWith("0"))
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

        private string GetOwnersEmail()
        {
            //
            var ownersTelephone = string.Empty;

            try
            {
                ownersTelephone = ConfigurationManager.AppSettings["OwnersEmail"].ToString();
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

        //
        [HttpGet]
        public ActionResult ViewVideo(int? id)
        {
            var room = _roomService.GetById(id.Value);
            var model = GetModelForNewRoom(room);
            model.RoomTypeList = GetRoomTypes(room.RoomType);
            model.RoomStatusList = GetRoomStatus(room.StatusId);
            var actualRoomType = room.RoomType1.Description;
            model.VideoPath = actualRoomType + ".mp4";
            return View("NewRoomVideo", model);
        }

        [HttpGet]
        public ActionResult EditRoom(int? id)
        {
            var room = _roomService.GetById(id.Value);
            var model = GetModelForNewRoom(room);
            model.RoomTypeList = GetRoomTypes(room.RoomType);
            model.RoomStatusList = GetRoomStatus(room.StatusId);
            return View("NewRoom", model);
        }

        [HttpPost]
        public ActionResult ViewRoomsByStatus(RoomViewModel model)
        {
            if(model.StatusId == 0)
            {                
                var complimentaryRooms = _guestRoomService.GetAll(HotelId).Where(x => x.RoomRate == decimal.Zero);
                var guestRooms = complimentaryRooms.Where(x => x.IsActive).ToList();
                RoomBookingViewModel rbkm = new RoomBookingViewModel();
                rbkm.RoomsList = guestRooms.Select(x => x.Room).ToList();
                rbkm.ComplimentaryRooms = true;
                return View("ViewAllRoomsByStatus", rbkm);
            }
            else
            {
                var statusRooms = _roomService.GetAll(HotelId).Where(x => x.StatusId == model.StatusId).ToList();

                RoomBookingViewModel rbkm = new RoomBookingViewModel();
                rbkm.RoomsList = statusRooms;
                return View("ViewAllRoomsByStatus", rbkm);
            }
            
        }

        
        [HttpGet]
        public ActionResult ViewRoomsByStatus()
        {            
            var model = new RoomViewModel();
            model.RoomStatusList = GetRoomStatusComplimentary(null);
            return View("ViewRoomsByStatus", model);
        }

        private IEnumerable<SelectListItem> GetRoomTypes(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomTypeService.GetAll(HotelId).ToList();
            bas.Insert(0, new RoomType { Name = "-- Please Select --", Id = 0 });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }

        private IEnumerable<SelectListItem> GetRoomStatusComplimentary(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomStatusService.GetAll(HotelId).Where(x => x.Id != (int)RoomStatusEnum.Occupied).ToList();

            //_roomStatusService.GetAll(HotelId).ToList();
            bas.Insert(0, new RoomStatu { Name = "-- Complimentary --", Id = 0 });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }

        private IEnumerable<SelectListItem> GetRoomStatus(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomStatusService.GetAll(HotelId).Where(x => x.Id != (int)RoomStatusEnum.Occupied).ToList();

                //_roomStatusService.GetAll(HotelId).ToList();
            bas.Insert(0, new RoomStatu { Name = "-- Please Select --", Id = 0 });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }

        private RoomViewModel GetModelForNewRoom(Room room)
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            room = room ?? new Room { IsActive = true };
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            return rvm;
        }
        
        [HttpPost]
        public ActionResult NewGroupBooking(RoomBookingViewModel model, string selectedRoomIds, int? paymentMethodId, string paymentMethodNote)
        {
            if (ModelState.IsValid)
            {
                var rmsList = new List<Room> { _roomService.GetById(model.GuestRoom.RoomId) };
                model = PopulateFromRequest(model);
                Mapper.CreateMap<GuestViewModel, Guest>();
                var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
                model.GuestRoom.IsActive = true;
                model.GuestRoom.Notes = string.Empty;
                model.GuestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
                var ticks = (int)DateTime.Now.Ticks;
                model.GuestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                {
                    Amount = model.GuestRoom.RoomRate,
                    PaymentTypeId = (int)RoomPaymentTypeEnum.InitialDeposit,
                    TransactionDate = DateTime.Now,
                    TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                    PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                });

                guest.GuestRooms = new Collection<GuestRoom> { model.GuestRoom };

                if (model.RoomBookingSelectedValues != null)
                {
                    foreach (var val in model.RoomBookingSelectedValues)
                    {
                        int extraRoomId;
                        int.TryParse(val, out extraRoomId);

                        var extraRoom = _roomService.GetById(extraRoomId);
                        rmsList.Add(extraRoom);
                        var gr = new GuestRoom
                            {
                                IsActive = true,
                                RoomRate = extraRoom.Price.Value,
                                RoomId = extraRoomId,
                                Notes = guest.Notes,
                                Occupants = 1,
                                CheckinDate = model.GuestRoom.CheckinDate,
                                CheckoutDate = model.GuestRoom.CheckoutDate,
                                Children = model.GuestDiscount,
                                RoomNumber = extraRoom.RoomNumber,
                                GuestId = guest.Id
                            };
                        guest.GuestRooms.Add(gr);
                    }
                }

                guest.GuestReservations = new Collection<GuestReservation>();

                foreach (var gr in guest.GuestRooms)
                {
                    guest.GuestReservations.Add(new GuestReservation { RoomId = gr.RoomId, StartDate = gr.CheckinDate, EndDate = gr.CheckoutDate, GuestId = gr.GuestId, IsActive = true });
                }

                guest.HotelId = HotelId;               

                var newGuest = _guestService.Create(guest);

                if(newGuest != null)
                {
                    CreateGuestCredentials(newGuest.Id, "");
                }



                foreach (var room in rmsList)
                {
                    room.StatusId = (int)RoomStatusEnum.Occupied;
                    _roomService.Update(room);
                }

                //return RedirectToAction("EditGroupBooking", "Booking", new { id = model.Room.Id, itemSaved = true });
                return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

            }

            var roomIds = selectedRoomIds.Split(',');

            if (roomIds.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString(CultureInfo.InvariantCulture))).ToList();
                var id = guestRooms.FirstOrDefault().Id;

                var rmm = new Room();
                rmm.BookRoom();
                Mapper.CreateMap<Room, RoomViewModel>();
                Mapper.CreateMap<Guest, GuestViewModel>();
                var room = _roomService.GetById(id);
                var rvm = Mapper.Map<Room, RoomViewModel>(room);
                var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest {IsActive = true, Status = "LIVE"});

                var model1 = new RoomBookingViewModel
                    {
                        Room = rvm,
                        Guest = gvm,
                        GuestRoom =
                            new GuestRoom
                                {
                                    Occupants = 1,
                                    RoomId = id,
                                    CheckinDate = DateTime.Now,
                                    CheckoutDate = DateTime.Now,
                                    RoomRate = room.Price.Value
                                },
                        RoomBookingRooms = GetMultiSelectRooms(roomIds),
                        selectedRoomIds = selectedRoomIds
                    };

                model1.SelectedRoomDisplay = model1.RoomBookingRooms.Select(x => x.Text).ToDelimitedString(",");

                return View(model1);
            }

            return RedirectToAction("Index", "Home");

        }

        private void CreateGuestCredentials(int guestId, string title)
        {
            try
            {
                var guest = _guestService.GetById(guestId);

                string roomNumber = string.Empty;

                var guestroom = guest.GuestRooms.FirstOrDefault();

                if (guestroom == null)
                    return;

                roomNumber = guestroom.RoomNumber;

                Person person = new Person();

                person.HotelId = guest.HotelId;
                person.Address = guest.Address;
                person.BirthDate = DateTime.Now;
                person.DisplayName = guest.FullName;

                person.FirstName = guest.FullName;
                person.LastName = guest.FullName;
                person.MiddleName = guest.FullName;
                person.Password = GetLastName(guest.FullName);
                person.Email = person.Password;
                person.Username = roomNumber;
                person.Title = "Mr";
                person.PersonTypeId = (int)PersonTypeEnum.Guest;
                person.IsActive = true;
                person.EndDate = DateTime.Now.AddYears(1);
                person.IdNumber = guestId.ToString();
                person.PreviousEmployerStartDate = DateTime.Now;
                person.PreviousEmployerEndDate = DateTime.Now;
                person.StartDate = DateTime.Now;
                guest.Person = person;
                _guestService.Update(guest);
            }
            catch
            {

            }
        }

        private string GetLastName(string fullname)
        {
            var splitter = fullname.Split(' ');

            var count = splitter.Length;

            if (count > 1)
            {
                return splitter[count - 1];
            }

            return fullname;
        }

        

        [HttpPost]
        public ActionResult PreGroupBooking(string selectedRoomIds)
        {
            var roomIds = selectedRoomIds.Split(',');

            if (roomIds.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString(CultureInfo.InvariantCulture))).ToList();
                var id = guestRooms.FirstOrDefault().Id;

                var rmm = new Room();
                rmm.BookRoom();
                Mapper.CreateMap<Room, RoomViewModel>();
                Mapper.CreateMap<Guest, GuestViewModel>();
                var room = _roomService.GetById(id);
                var rvm = Mapper.Map<Room, RoomViewModel>(room);
                var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest { IsActive = true, Status = "LIVE" });
                var model = new RoomBookingViewModel
                    {
                        Room = rvm,
                        Guest = gvm,
                        GuestRoom =
                            new GuestRoom
                                {
                                    Occupants = 1,
                                    RoomId = id,
                                    CheckinDate = DateTime.Now,
                                    CheckoutDate = DateTime.Now,
                                    RoomRate = room.Price.Value
                                },
                        RoomBookingRooms = GetMultiSelectRooms(roomIds),
                        selectedRoomIds = selectedRoomIds
                    };

                model.SelectedRoomDisplay = model.RoomBookingRooms.Select(x => x.Text).ToDelimitedString(",");
                return View("NewGroupBooking", model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "selectedRoomIds")]

        public ActionResult NewGroupBooking(string selectedRoomIds)
        {
            var roomIds = selectedRoomIds.Split(',');

            if (roomIds.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString(CultureInfo.InvariantCulture))).ToList();
                var id = guestRooms.FirstOrDefault().Id;
               
                var rmm = new Room();
                rmm.BookRoom();
                Mapper.CreateMap<Room, RoomViewModel>();
                Mapper.CreateMap<Guest, GuestViewModel>();
                var room = _roomService.GetById(id);
                var rvm = Mapper.Map<Room, RoomViewModel>(room);
                var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest {IsActive = true, Status = "LIVE"});
                var model = new RoomBookingViewModel
                    {
                        Room = rvm,
                        Guest = gvm,
                        GuestRoom =
                            new GuestRoom
                                {
                                    Occupants = 1,
                                    RoomId = id,
                                    CheckinDate = DateTime.Now,
                                    CheckoutDate = DateTime.Now,
                                    RoomRate = room.Price.Value
                                },
                        RoomBookingRooms = GetMultiSelectRooms(roomIds),
                        selectedRoomIds = selectedRoomIds
                    };

                model.SelectedRoomDisplay = model.RoomBookingRooms.Select(x => x.Text).ToDelimitedString(",");
                return View(model);
            }
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public ActionResult Transfer(string roomIdValues)
        {
            var roomIds = roomIdValues.Split(',');
            if (roomIdValues.Any())
            {
                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString(CultureInfo.InvariantCulture))).ToList(),
                    selectedRoomIds = roomIdValues,
                    PageTitle = "GUEST TRANSFERS"
                };

                return View("EnterGuestDates", gbvm);
            }
            else
            {
                var gbvm = new GroupBookingViewModel { GuestRooms = _roomService.GetAll(HotelId).ToList() };
                return View(gbvm);
            }

        }

        [HttpGet]
        public ActionResult Transfer()
        {
            var gbvm = new GroupBookingViewModel { GuestRooms = _roomService.GetAll(HotelId).ToList() };
            return View(gbvm);
        }

        [HttpGet]
        public ActionResult RoomGroupBooking()
        {
            var gbvm = new GroupBookingViewModel { GuestRooms = _roomService.GetAll(HotelId).ToList() };
            return View(gbvm);
        }

        //[HttpPost]
        //public ActionResult GroupReservationAddNewRooms(int? GuestId, string selectedRoomIds, RoomBookingViewModel model)
        //{
        //    var roomIds = selectedRoomIds.Split(',');

        //    if (roomIds.Any())
        //    {

        //        var rooms = _roomService.GetAll(HotelID).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
        //        var guestRooms = new List<GuestRoom>();

        //        foreach (var rm in rooms)
        //        {
        //            var dtIn = DateTime.Now;
        //            var dtOut = DateTime.Now;
        //            DateTime.TryParse(Request.Form["arrive_" + rm.Id], out dtIn);
        //            DateTime.TryParse(Request.Form["depart_" + rm.Id], out dtOut);
        //            var newGuestRoom = new GuestRoom
        //            {
        //                RoomId = rm.Id,
        //                Occupants = 1,
        //                IsActive = true,
        //                Children = 0,
        //                CheckinDate = dtIn,
        //                CheckoutDate = dtOut,
        //                RoomRate = rm.Price.Value,
        //                Notes = model.Guest.Notes,
        //                GroupBooking = true
        //            };
        //            var conflicts = rm.RoomAvailability(newGuestRoom.CheckinDate, newGuestRoom.CheckoutDate);

        //            if (conflicts.Count > 0)
        //            {
        //                ModelState.AddModelError("CheckOutDate",
        //                    "There is a reservation clash with your proposed checkin/checkout date(s)");
        //            }
        //            else
        //            {
        //                guestRooms.Add(newGuestRoom);
        //            }
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            var rmsList = rooms;
        //            Mapper.CreateMap<GuestViewModel, Guest>();
        //            var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
        //            var ticks = (int)DateTime.Now.Ticks;

        //            guestRooms.FirstOrDefault().GuestRoomAccounts = new Collection<GuestRoomAccount>
        //            {
        //                new GuestRoomAccount
        //                {
        //                    Amount = model.GuestRoom.RoomRate,
        //                    PaymentTypeId = (int) RoomPaymentTypeEnum.InitialDeposit,
        //                    TransactionDate = DateTime.Now,
        //                    
        //                }
        //            };

        //            guest.GuestRooms = guestRooms;
        //            guest.GuestReservations = new Collection<GuestReservation>();

        //            foreach (var gr in guest.GuestRooms)
        //            {
        //                guest.GuestReservations.Add(new GuestReservation
        //                {
        //                    RoomId = gr.RoomId,
        //                    StartDate = gr.CheckinDate,
        //                    EndDate = gr.CheckoutDate,
        //                    GuestId = gr.GuestId,
        //                    IsActive = true
        //                });
        //            }

        //            if (model.Guest.CompanyId > 0)
        //            {
        //                guest.CompanyId = model.Guest.CompanyId;
        //            }
        //            else
        //            {
        //                guest.BusinessAccount = null;
        //                guest.CompanyId = null;
        //            }

        //            _guestService.Create(guest);

        //            foreach (var room in rmsList)
        //            {
        //                room.StatusId = (int)RoomStatusEnum.Occupied;
        //                _roomService.Update(room);
        //            }

        //            return RedirectToAction("EditGroupBooking", "Booking", new { id = model.Room.Id, itemSaved = true });
        //        }

        //        var guestRoomsP = _roomService.GetAll(HotelID).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
        //        var firstRoomId = guestRoomsP.FirstOrDefault().Id;

        //        var gbvm = new GroupBookingViewModel
        //        {
        //            GuestRooms = guestRoomsP,
        //            selectedRoomIds = selectedRoomIds,
        //            CheckinDate = model.CheckinDate,
        //            CheckoutDate = model.CheckoutDate,
        //            RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
        //            PageTitle = "GROUP BOOKING"
        //        };

        //        return View("EnterGuestDatesGroup", gbvm);

        //    }

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpPost]
        public ActionResult GroupReservationAddNewRooms(int? GuestId, string selectedRoomIds, RoomBookingViewModel model)
        {
            var roomIds = selectedRoomIds.Split(',');
            var guest = _guestService.GetById(GuestId.Value);
            var grMainRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && !x.GroupBooking);
            bool updateMainRoomToGroup = grMainRoom != null;
            //Update InitialRoom and make it the mainRoom RegardLess XXXX

            if (roomIds.Any())
            {

                var rooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var guestRooms = new List<GuestRoom>();

                foreach (var rm in rooms)
                {
                    DateTime dtIn;
                    DateTime dtOut;
                    DateTime.TryParse(Request.Form["arrive_" + rm.Id], out dtIn);
                    DateTime.TryParse(Request.Form["depart_" + rm.Id], out dtOut);
                    var newGuestRoom = new GuestRoom
                    {
                        RoomId = rm.Id,
                        Occupants = 1,
                        IsActive = true,
                        Children = model.GuestDiscount,
                        CheckinDate = DateTime.Now,
                        CheckoutDate = dtOut,
                        RoomRate = rm.Price.Value,
                        Notes = model.Guest.Notes,
                        GroupBooking = true
                    };

                    var conflicts = rm.RoomAvailability(newGuestRoom.CheckinDate, newGuestRoom.CheckoutDate,null);

                    if (conflicts.Count > 0)
                    {
                        ModelState.AddModelError("CheckOutDate",
                            "There is a reservation clash with your proposed checkin/checkout date(s)");
                    }
                    else
                    {
                        guestRooms.Add(newGuestRoom);
                    }
                }

                if (ModelState.IsValid)
                {
                    var rmsList = rooms;
                    //guest.GuestRooms.ToList().AddRange(guestRooms);

                    foreach (var gr in guestRooms)
                    {
                        guest.GuestRooms.Add(gr);
                        guest.GuestReservations.Add(new GuestReservation
                        {
                            RoomId = gr.RoomId,
                            StartDate = gr.CheckinDate,
                            EndDate = gr.CheckoutDate,
                            GuestId = gr.GuestId,
                            IsActive = true
                        });
                    }

                    if (model.Guest.CompanyId > 0)
                    {
                        guest.CompanyId = model.Guest.CompanyId;
                    }
                    else
                    {
                        guest.BusinessAccount = null;
                        guest.CompanyId = null;
                    }

                    _guestService.Update(guest);

                    foreach (var room in rmsList)
                    {
                        room.StatusId = (int)RoomStatusEnum.Occupied;
                        _roomService.Update(room);
                    }

                    if(updateMainRoomToGroup)
                    {
                        var grnew = _guestRoomService.GetById(grMainRoom.Id);
                        grnew.GroupBooking = true;
                        _guestRoomService.Update(grnew);
                    }

                    //return RedirectToAction("EditGroupBooking", "Booking", new { id = guest.Id, itemSaved = true });
                    return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

                }

               
                var firstRoomId = guestRooms.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestId = GuestId.Value,
                    GuestRooms = guestRooms.Select(x => x.Room).ToList(),
                    CheckoutDate = model.CheckoutDate,
                    CheckinDate = model.CheckinDate,
                    selectedRoomIds = selectedRoomIds,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId, guest),
                    PageTitle = "ADD ROOMS TO GROUP BOOKING"
                };

                return View("EnterGuestDatesGroupAddRooms", gbvm);

            }

            return RedirectToAction("Index", "Home");
        }

        

        [HttpPost]
        public ActionResult RemoveRoomsGroupBookingBook(int? GuestId, string roomIdValues, DateTime? CheckOutDate, DateTime? CheckInDate)
        {
            var roomIds = roomIdValues.Split(',');
            var guest = _guestService.GetById(GuestId.Value);

            if (roomIdValues.Any())
            {
                var rooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var idsList = rooms.Select(x => x.Id).ToList();
                var guestRoomIds = guest.GuestRooms.Where(x => idsList.Contains(x.RoomId)).Select(x => x.Id).ToList();
                var guestRoomReservationsIds = guest.GuestReservations.Where(x => idsList.Contains(x.RoomId)).Select(x => x.Id).ToList();

                foreach (var id in guestRoomIds)
                {
                    var groom = _guestRoomService.GetById(id);
                    _guestRoomService.Delete(groom);
                }

                foreach (var grid in guestRoomReservationsIds)
                {
                    var greserve = _guestReservationService.GetById(grid);
                    _guestReservationService.Delete(greserve);
                }

                foreach (var rmid in idsList)
                {
                    var room = _roomService.GetById(rmid);
                    room.StatusId = (int)RoomStatusEnum.Vacant;
                    _roomService.Update(room);
                }

                guest = _guestService.GetById(GuestId.Value);

                //return RedirectToAction("EditGroupBooking", new { id = guest.Id, itemSaved = true });
                return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

               
            }
            else
            {
                
                var checkinDate = guest.GuestRooms.FirstOrDefault().CheckinDate;
                var checkoutDate = guest.GuestRooms.FirstOrDefault().CheckoutDate;

                const int roomSelect = 0;
                IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
                var conflicts = gr.SelectAvailable(checkinDate, checkoutDate, roomSelect).ToList();

                if (conflicts.Count > 0)
                {
                    var ids = conflicts.Select(x => x.RoomId).ToList();
                    var model = new GroupBookingViewModel { GuestId = guest.Id, GuestRoomsCheckedIn = guest.GuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                    return View("RemoveRooms", model);
                }
                else
                {
                    var model = new GroupBookingViewModel { GuestId = guest.Id, GuestRoomsCheckedIn = guest.GuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                    return View("RemoveRooms", model);
                }          
            }
        }

        [HttpPost]
        public ActionResult AddRoomsGroupBookingBook(int? GuestId, string roomIdValues, DateTime? CheckOutDate, DateTime? CheckInDate)
        {
            var roomIds = roomIdValues.Split(',');
            var guest = _guestService.GetById(GuestId.Value);

            if (roomIdValues.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRooms.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestId = GuestId.Value,
                    GuestRooms = guestRooms,
                    CheckoutDate = CheckOutDate,
                    CheckinDate = CheckInDate,
                    selectedRoomIds = roomIdValues,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId, guest),
                    PageTitle = "ADD ROOMS TO GROUP BOOKING"
                };

                return View("EnterGuestDatesGroupAddRooms", gbvm);
            }
            else
            {
                
                var checkinDate = guest.GuestRooms.FirstOrDefault().CheckinDate;
                var checkoutDate = guest.GuestRooms.FirstOrDefault().CheckoutDate;

                const int room_select = 0;
                var gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
                var conflicts = gr.SelectAvailable(checkinDate, checkoutDate, room_select).ToList();

                if (conflicts.Count > 0)
                {
                    var ids = conflicts.Select(x => x.RoomId).ToList();
                    var model = new GroupBookingViewModel { GuestId = guest.Id, GuestRoomsCheckedIn = guest.GuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                    return View("AddRooms", model);
                }
                else
                {
                    var model = new GroupBookingViewModel { GuestId = guest.Id, GuestRoomsCheckedIn = guest.GuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                    return View("AddRooms", model);
                }          
            }

        }


        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult AddRooms(int? id)
        {
            var guest = _guestService.GetById(id.Value);
            var checkinDate = guest.GuestRooms.FirstOrDefault().CheckinDate;
            var checkoutDate = guest.GuestRooms.FirstOrDefault().CheckoutDate;

            const int room_select = 0;
            var gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
            var conflicts = gr.SelectAvailable(checkinDate, checkoutDate, room_select).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new GroupBookingViewModel { GuestId = guest.Id, GuestRoomsCheckedIn = guest.GuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                return View(model);
            }
            else
            {
                var model = new GroupBookingViewModel {GuestId = guest.Id, GuestRoomsCheckedIn = guest.GuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                return View(model);
            }          
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        
        public ActionResult RemoveRooms(int? id)
        {
            var guest = _guestService.GetById(id.Value);

            var checkinDate = guest.GuestRooms.FirstOrDefault().CheckinDate;
            var checkoutDate = guest.GuestRooms.FirstOrDefault().CheckoutDate;

            var removeableGuestRooms = guest.GuestRooms.Where(x => !x.GuestRoomAccounts.Any() && (x.CheckinDate.AddHours(HotelCancellationHours) > DateTime.Now)).ToList();

            if (removeableGuestRooms.Count == 0)
            {
                return RedirectToAction("NoRemoveableRooms", "Home", new { id = guest.Id });
            }


            var model = new GroupBookingViewModel { GuestId = guest.Id, GuestRoomsCheckedIn = removeableGuestRooms, CheckinDate = checkinDate, CheckoutDate = checkoutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
            return View(model);

        }

        
        [HttpPost]
        public ActionResult RoomFutureBookingFromDialog(DateTime? arrive, DateTime? depart, int? room_select)
        {
            room_select = 0;
            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new GroupBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, GuestRooms = _roomService.GetAll(HotelId).Where(x => !ids.Contains(x.Id)).ToList() };
                return View("RoomFutureBooking", model);
            }
            else
            {
                //var model = new GroupBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                var model = new GroupBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, GuestRooms = _roomService.GetAll(HotelId).ToList() };

                return View("RoomFutureBooking", model);
            }
        }


        [HttpPost]
        public ActionResult RoomGroupBookingFromDialog(DateTime? arrive, DateTime? depart, int? room_select)
        {
            room_select = 0;
            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new GroupBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                return View("RoomGroupBooking", model);
            }
            else
            {
                var model = new GroupBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant)).ToList() };
                return View("RoomGroupBooking", model);
            }
        }


        public ActionResult RemoveBookingFromNewGroupBooking(int? id, string selectedRoomIds, DateTime? CheckinDate, DateTime CheckoutDate)
        {
            var roomIds = selectedRoomIds.Split(',');

            var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString()) && x.Id != id.Value).ToList();

            if (guestRooms.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            selectedRoomIds = guestRooms.Select(x => x.Id.ToString()).ToDelimitedString(",");
            var firstRoomId = guestRooms.FirstOrDefault().Id;

            var gbvm = new GroupBookingViewModel
            {
                GuestRooms = guestRooms,
                CheckoutDate = CheckoutDate,
                CheckinDate = CheckinDate,
                selectedRoomIds = selectedRoomIds,
                RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                PageTitle = "GROUP BOOKING"
            };

            return View("EnterGuestDatesGroup", gbvm);
        }


        
        [HttpPost]
        public ActionResult RoomFutureBookingBook(string roomIdValues, DateTime? CheckOutDate, DateTime? CheckInDate)
        {
            var roomIds = roomIdValues.Split(',');

            if (roomIdValues.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRooms.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRooms,
                    CheckoutDate = CheckOutDate.Value.AddDays(1),
                    CheckinDate = CheckInDate.Value.AddDays(2),
                    selectedRoomIds = roomIdValues,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "FUTURE RESERVATION"
                };


                gbvm.RoomBookingViewModel.BusinessAccounts = GetBusinessAccounts(null);
                gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);

                return View("EnterGuestDatesFuture", gbvm);
            }
            else
            {
                var model = new GroupBookingViewModel { CheckinDate = CheckInDate, CheckoutDate = CheckOutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                return View("RoomFutureBooking", model);
            }
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,CheckOutDate,CheckInDate")]

        public ActionResult RoomFutureBookingBookSingle(int? id, DateTime? CheckOutDate, DateTime? CheckInDate)
        {
            var roomIdValues = new List<int> {id.Value};
            var roomIds = new List<string> {id.Value.ToString()};
            //CheckOutDate = null;
            //CheckInDate = null;


            ModelState.Clear();

            //CheckInDate = DateTime.Today.AddDays(1);
            //CheckOutDate = DateTime.Today.AddDays(2);


            if (roomIdValues.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIdValues.Contains(x.Id)).ToList();
                var firstRoomId = guestRooms.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRooms,
                    CheckoutDate = CheckOutDate,
                    CheckinDate = CheckInDate,
                    selectedRoomIds = roomIdValues.Select(x => x.ToString()).ToDelimitedString(","),
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "FUTURE RESERVATION"
                };

                gbvm.RoomBookingViewModel.BusinessAccounts = GetBusinessAccounts(null);
                gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);

                return View("EnterGuestDatesFuture", gbvm);
            }
            else
            {
                var model = new GroupBookingViewModel { CheckinDate = CheckInDate, CheckoutDate = CheckOutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                return View("RoomFutureBooking", model);
            }
        }

               
        [HttpPost]
        public ActionResult RoomGroupBookingBook(string roomIdValues, DateTime? CheckOutDate, DateTime? CheckInDate)
        {
            var roomIds = roomIdValues.Split(',');

            if (roomIdValues.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRooms.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRooms,
                    CheckoutDate = CheckOutDate,
                    CheckinDate = CheckInDate,
                    selectedRoomIds = roomIdValues,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "GROUP BOOKING"
                };

                return View("EnterGuestDatesGroup", gbvm);
            }
            else
            {
                var model = new GroupBookingViewModel { CheckinDate = CheckInDate, CheckoutDate = CheckOutDate, GuestRooms = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant)).ToList() };
                return View("RoomGroupBooking", model);
            }
        }

        //[OutputCache(Duration = 3600, VaryByParam = "id,selectedRoomIds,CheckinDate,CheckoutDate")]

        public ActionResult RemoveBookingFromNew(int? id, string selectedRoomIds, DateTime? CheckinDate, DateTime CheckoutDate)
        {
            var roomIds = selectedRoomIds.Split(',');

            var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString()) && x.Id != id.Value).ToList();

            if(guestRooms.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            selectedRoomIds = guestRooms.Select(x => x.Id.ToString()).ToDelimitedString(",");
            var firstRoomId = guestRooms.FirstOrDefault().Id;

            var gbvm = new GroupBookingViewModel
            {
                GuestRooms = guestRooms,
                selectedRoomIds = selectedRoomIds,
                CheckinDate = CheckinDate,
                CheckoutDate = CheckoutDate,
                RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                PageTitle = "FUTURE RESERVATIONS"
            };

            return View("EnterGuestDatesFuture", gbvm);
        }


        [HttpPost]
        public ActionResult RoomFutureGroupBooking(string roomIdValues, DateTime? CheckOutDate, DateTime? CheckInDate)
        {
            var roomIds = roomIdValues.Split(',');

            if (roomIdValues.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRooms.FirstOrDefault().Id;
                
                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRooms,
                    selectedRoomIds = roomIdValues,
                    CheckinDate = CheckInDate,
                    CheckoutDate = CheckOutDate,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "FUTURE RESERVATIONS",                   
                    
                };

                return View("EnterGuestDatesFuture", gbvm);
            }
            else
            {
                var model = new RoomBookingViewModel { CheckinDate = CheckInDate, CheckoutDate  = CheckOutDate,  RoomsList = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                return View("FutureBooking", model);
            }

        }

        
      

        [HttpPost]
        public ActionResult RoomGroupBooking(string roomIdValues)
        {
            var roomIds = roomIdValues.Split(',');
            if (roomIdValues.Any())
            {
                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList(),
                    selectedRoomIds = roomIdValues,
                    PageTitle = "GROUP BOOKING"
                };

                return View("EnterGuestDates",gbvm);
            }
            else
            {
                var gbvm = new GroupBookingViewModel { GuestRooms = _roomService.GetAll(HotelId).ToList() };
                return View(gbvm);    
            }
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]

        public ActionResult GroupBooking()
        {
            var gbvm = new GroupBookingViewModel {GuestRooms = _roomService.GetAll(HotelId).ToList()};
            return View(gbvm);
        }

        [HttpPost]
        public ActionResult CheckFutureAvailability(DateTime? arrive, DateTime? depart, int? room_select)
        {
            room_select = 0;
            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, RoomsList = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                return View("FutureBookingBlock", model);
            }
            else
            {
                var model = new RoomBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, RoomsList = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                return View("FutureBookingBlock", model);
            }
        }

        [HttpPost]
        public ActionResult FutureBooking(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now.AddDays(1);
            if (!depart.HasValue) depart = DateTime.Now.AddDays(2);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { RoomsList = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                return View("FutureBooking", model);
            }
            else
            {
                var model = new RoomBookingViewModel { RoomsList = _roomService.GetAll(HotelId).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty)).ToList() };
                return View("FutureBooking", model);
            }
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]

        public ActionResult CreateRooms()
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            //var room = _roomService.GetById(id);
            var rvm = Mapper.Map<Room, RoomViewModel>(new Room { Smoking = "false", Id = 0, NoOfBeds = 1, HotelId = HotelId, IsActive = true, StatusId = (int)RoomStatusEnum.Vacant });
            rvm.RoomTypeList = GetRoomTypes(null);
            rvm.RoomStatusList = GetRoomStatus((int)RoomStatusEnum.Vacant);
            return View(rvm);
        }

        public ActionResult CreateRoomTypes()
        {
            Mapper.CreateMap<RoomType, RoomTypeViewModel>();
            //var room = _roomService.GetById(id);
            var rvm = Mapper.Map<RoomType, RoomTypeViewModel>(new RoomType {  HotelId = HotelId, IsActive = true });
            
            //rvm.RoomTypeList = GetRoomTypes(null);
            return View(rvm);
        }


       
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult ViewRooms()
        {
            var rvm = new RoomBookingViewModel { RoomsList = _roomService.GetAll(HotelId).ToList() };
            return View(rvm);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult ViewRoomTypes()
        {
            var rvm = new RoomBookingViewModel { RoomTypesList = _roomTypeService.GetAll(HotelId).ToList() };
            return View(rvm);
        }

              
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,itemSaved")]
        public ActionResult EditRooms(int? id, bool? itemSaved)
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            var room = _roomService.GetById(id.Value);
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            rvm.RoomTypeList = GetRoomTypes(null);
            rvm.RoomStatusList = GetRoomStatus(room.StatusId);
            rvm.ItemSaved = itemSaved;
            return View("CreateRooms", rvm);
        }

        //
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,itemSaved")]
        public ActionResult EditRoomTypes(int? id, bool? itemSaved)
        {
            Mapper.CreateMap<RoomType, RoomTypeViewModel>();
            var room = _roomTypeService.GetById(id.Value);
            var rvm = Mapper.Map<RoomType, RoomTypeViewModel>(room);           
            rvm.ItemSaved = itemSaved;
            return View("CreateRoomTypes", rvm);
        }

        [HttpPost]
        public ActionResult CreateRoomTypes(RoomTypeViewModel model, string send_booking)
        {
            if (ModelState.IsValid)
            {
                Mapper.CreateMap<RoomTypeViewModel, RoomType>();

                var roomType = Mapper.Map<RoomTypeViewModel, RoomType>(model);

                if (roomType.Id == 0)
                {
                    var existingRoomType = _roomTypeService.GetAll(HotelId).FirstOrDefault(x => x.Name.Trim().Equals(model.Name.Trim(), StringComparison.InvariantCultureIgnoreCase));
                    if (existingRoomType == null)
                    {
                        roomType.IsActive = true;
                        _roomTypeService.Create(roomType);
                    }
                }
                else
                {
                   
                    var existingRoomType = _roomTypeService.GetById(model.Id);
                    existingRoomType.Description = roomType.Description;
                    existingRoomType.Name = roomType.Name;
                 

                    if (send_booking.ToUpper().Contains("SAVE"))
                        _roomTypeService.Update(existingRoomType);
                    else
                    {
                        try
                        {
                            _roomTypeService.Delete(existingRoomType);
                            return RedirectToAction("ViewRoomTypes");
                        }
                        catch
                        {
                            ModelState.AddModelError("Description", "Can't delete room type due to rooms attached to this room type");                           
                            return View(model);
                        }
                    }
                }

                return RedirectToAction("EditRoomTypes", new { id = roomType.Id, itemSaved = true });
            }

            //model.RoomTypeList = GetRoomTypes(model.RoomType);
            return View(model);
        }

        [HttpPost]
        public ActionResult CreateRooms(RoomViewModel model, string send_booking)
        {
            if (ModelState.IsValid)
            {
                Mapper.CreateMap<RoomViewModel, Room>();
                
                var room = Mapper.Map<RoomViewModel, Room>(model);

                if(room.Id == 0)
                {
                    var existingRoom = _roomService.GetAll(HotelId).FirstOrDefault(x => x.RoomNumber.Trim().Equals(model.RoomNumber.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (existingRoom == null)
                    {
                        room.Smoking = "false";
                        _roomService.Create(room);  
                    }
                                  
                }
                else
                {
                    Pictural pic = null;

                    string path = GetPicturePath();

                    if(!string.IsNullOrEmpty(path))
                    {
                        pic = new Pictural { DateCreated = DateTime.Now, IsActive = true, PicturePath = path };
                    }

                    var existingRoom = _roomService.GetById(model.Id);
                    existingRoom.Description = room.Description;
                    existingRoom.NoOfBeds = room.NoOfBeds;
                    existingRoom.Price = room.Price;
                    existingRoom.BusinessPrice = room.BusinessPrice;
                    existingRoom.ExtNumber = room.ExtNumber;
                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.RoomType = room.RoomType;
                    existingRoom.StatusId = room.StatusId;

                    if (pic != null)
                    {
                        existingRoom.Picturals.Add(pic);
                    }

                    if (send_booking.ToUpper().Contains("SAVE"))
                        _roomService.Update(existingRoom);
                    else
                    {
                        try
                        {
                            _roomService.Delete(existingRoom);
                            return RedirectToAction("ViewRooms");

                        }
                        catch
                        {
                            ModelState.AddModelError("Description", "Cant delete room due to references attached to room");
                            model.RoomTypeList = GetRoomTypes(model.RoomType);
                            return View(model);
                        }
                    }
                }

                return RedirectToAction("EditRooms", new { id = room.Id, itemSaved = true });
            }

            model.RoomTypeList = GetRoomTypes(model.RoomType);
            return View(model);
        }

        private string GetPicturePath()
        {
            var file = Request.Files[0];

            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var pp = ConfigurationManager.AppSettings["ProjectPictures"].ToString(CultureInfo.InvariantCulture);
                var path = Path.Combine(pp, fileName);
                file.SaveAs(path);
                return path;
            }

            return string.Empty;
        }


        private List<TitleModel> Titles
        {
            get 
            {
                return new List<TitleModel> { new TitleModel { Id = 1, Name = "Mr" }, new TitleModel { Id = 2, Name = "Mrs" }, new TitleModel { Id = 3, Name = "Hon" }, new TitleModel { Id = 4, Name = "Engr" } };
            }
        }

        private IEnumerable<SelectListItem> GetTitles(string selectedId)
        {
            var bas = Titles;
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Name, Selected = x.Name.Equals(selectedId, StringComparison.InvariantCultureIgnoreCase) });
        }


       


        [HttpGet]
        ////[OutputCache(Duration = 3600, VaryByParam = "id,checkinDate,checkoutDate")]
        public ActionResult NewBooking(int? id, DateTime? checkinDate, DateTime? checkoutDate)
        {
            checkinDate = checkinDate ?? DateTime.Now;
            checkoutDate = checkoutDate ?? DateTime.Now.AddDays(1);
            int reservationCount = _guestReservationService.GetAll(HotelId).Count(x => x.StartDate.Date == DateTime.Today.Date && x.FutureBooking);
            var model = GetModelForNewBooking(id.Value, checkinDate.Value, checkoutDate.Value);
            model.FutureReservationCount = reservationCount;

            var list = GetGuestSelectList(null).ToList();

            model.languanges = new SelectList(list, "Value", "Text");

            model.TitleDropDown = GetTitles(null);
            
            return View(model);
        }

        private IEnumerable<SelectListItem> GetGuestSelectList(int? selectedValue)
        {
            var distinctCourseTutors = _guestService.GetAll(1).Where(x => x.FullName != "GUEST").ToList();

            var classes = distinctCourseTutors.Distinct(new GuestComparer()).ToList();

            classes.Insert(0, new Guest { Id = 0, FullName = "" });

            IEnumerable<SelectListItem> items = classes.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.FullName,
                Selected = selectedValue == c.Id
            });

            return items;
        }

        private RoomBookingViewModel GetModelForNewBooking(int id, DateTime checkinDate, DateTime checkoutDate)
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            Mapper.CreateMap<Guest, GuestViewModel>();
            var room = _roomService.GetById(id);
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest { IsActive = true, Status = "LIVE" });
          
            var model = new RoomBookingViewModel
            {
                InitialDeposit = decimal.Zero,
                Room = rvm,
                Guest = gvm,
                GuestRoom =
                    new GuestRoom
                    {
                        Occupants = 1,
                        RoomId = id,
                        CheckinDate = checkinDate,
                        CheckoutDate = checkoutDate,
                        RoomRate = room.Price.Value
                        
                    },

                     BusinessRoomRate = room.BusinessPrice.Value,
                     BusinessAccounts = GetBusinessAccounts(null)
            };
            return model;
        }



        [HttpPost]
        public ActionResult GroupReservationAddToBooking(string selectedRoomIds, RoomBookingViewModel model, int? GuestId, int? paymentMethodId, string paymentMethodNote)
        {
            var roomIds = selectedRoomIds.Split(',');

            if (roomIds.Any())
            {
                var rooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();

                var guestRooms = new List<GuestRoom>();

                var i = 0;
                foreach (var rm in rooms)
                {
                    DateTime dtIn;
                    DateTime dtOut;
                    DateTime.TryParse(Request.Form["arrive_" + rm.Id], out dtIn);
                    DateTime.TryParse(Request.Form["depart_" + rm.Id], out dtOut);
                    var groupBookingMainRoom = i == 0;

                    i++;
                    var newGuestRoom = new GuestRoom
                    {
                        RoomId = rm.Id,
                        Occupants = 1,
                        IsActive = true,
                        Children = model.GuestDiscount,
                        CheckinDate = DateTime.Now,
                        CheckoutDate = dtOut,
                        RoomRate = rm.Price.Value,
                        Notes = model.Guest.Notes,
                        GroupBooking = true,
                        GroupBookingMainRoom = groupBookingMainRoom,
                        GuestId = GuestId.Value,
                        
                    };

                    var conflicts = rm.RoomAvailability(newGuestRoom.CheckinDate, newGuestRoom.CheckoutDate, null);

                    if (conflicts.Count > 0)
                    {
                        ModelState.AddModelError("CheckOutDate",
                            "There is a reservation clash with your proposed checkin/checkout date(s)");
                    }
                    else
                    {
                        guestRooms.Add(newGuestRoom);
                    }
                }

                if (ModelState.IsValid)
                {
                    var rmsList = rooms;
                    var guest = _guestService.GetById(GuestId.Value);

                    var ticks = (int)DateTime.Now.Ticks;

                    if (model.InitialDeposit > 0)
                    {
                        guestRooms.FirstOrDefault(x => x.GroupBookingMainRoom).GuestRoomAccounts = new Collection<GuestRoomAccount>
                        {
                            new GuestRoomAccount
                            {
                                Amount = model.InitialDeposit,
                                PaymentTypeId = (int) RoomPaymentTypeEnum.CashDeposit,
                                TransactionDate = DateTime.Now,
                                TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                            }
                        };
                    }

                    guest.GuestRooms = guestRooms;
                    guest.GuestReservations = new Collection<GuestReservation>();

                    foreach (var gr in guest.GuestRooms)
                    {
                        guest.GuestReservations.Add(new GuestReservation
                        {
                            RoomId = gr.RoomId,
                            StartDate = DateTime.Now,
                            EndDate = gr.CheckoutDate,
                            GuestId = gr.GuestId,
                            IsActive = true
                        });
                    }                    

                    _guestService.Update(guest);

                    foreach (var room in rmsList)
                    {
                        room.StatusId = (int)RoomStatusEnum.Occupied;
                        _roomService.Update(room);
                    }

                    //return RedirectToAction("EditGroupBooking", "Booking", new { id = guest.Id, addToRoomCompleted = true });
                    return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

                }
               
            

                var guestRoomsP = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRoomsP.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRoomsP,
                    selectedRoomIds = selectedRoomIds,
                    CheckinDate = model.CheckinDate,
                    CheckoutDate = model.CheckoutDate,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "GROUP BOOKING"
                };

                return View("EnterGuestDatesGroup", gbvm);

            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult GroupReservationExistingBooking(string selectedRoomIds, RoomBookingViewModel model, int? GuestId, int? paymentMethodId, string paymentMethodNote)
        {
            var roomIds = selectedRoomIds.Split(',');

            var guest = _guestService.GetById(GuestId.Value);

            if (guest.GuestRooms.Any())
            {
                var guestRooms = new List<GuestRoom>();

                var i = 0;

                foreach (var gr in guest.GuestRooms)
                {
                    var rm = _roomService.GetById(gr.RoomId);
                    DateTime dtIn;
                    DateTime dtOut;
                    DateTime.TryParse(Request.Form["arrive_" + gr.Id], out dtIn);
                    DateTime.TryParse(Request.Form["depart_" + gr.Id], out dtOut);
                    var groupBookingMainRoom = i == 0;

                    i++;

                    var existingGuestRoom = _guestRoomService.GetById(gr.Id);
                    existingGuestRoom.GroupBookingMainRoom = groupBookingMainRoom;
                    existingGuestRoom.CheckinDate = DateTime.Now;
                    existingGuestRoom.CheckoutDate = dtOut;
                    existingGuestRoom.IsActive = true;
                    existingGuestRoom.GroupBooking = guestRooms.Count > 1;

                    var conflicts = rm.RoomAvailability(existingGuestRoom.CheckinDate, existingGuestRoom.CheckoutDate, GuestId.Value);

                    if (conflicts.Count > 0)
                    {
                        ModelState.AddModelError("CheckOutDate",
                            "There is a reservation clash with your proposed checkin/checkout date(s)");
                    }
                    else
                    {
                        guestRooms.Add(existingGuestRoom);
                    }
                }

                if (ModelState.IsValid)
                {
                    
                    var ticks = (int)DateTime.Now.Ticks;

                    foreach (var gr in guestRooms)
                    {
                        _guestRoomService.Update(gr);
                    }

                    if (model.InitialDeposit > 0)
                    {
                        var grId = guestRooms.FirstOrDefault(x => x.GroupBookingMainRoom).Id;
                        
                        var gra = new GuestRoomAccount
                           {
                               Amount = model.InitialDeposit,
                               PaymentTypeId = (int)RoomPaymentTypeEnum.InitialDeposit,
                               TransactionDate = DateTime.Now,
                               TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                               GuestRoomId = grId,
                               PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                           };

                        _guestRoomAccountService.Create(gra);
                    }

                    foreach (var gr in guestRooms)
                    {
                        var guestReservation = _guestReservationService.GetAll(HotelId).FirstOrDefault(x => x.GuestId == GuestId && x.RoomId == gr.RoomId);
                        guestReservation.StartDate = gr.CheckinDate;
                        guestReservation.EndDate = gr.CheckoutDate;
                        guestReservation.IsActive = true;
                        _guestReservationService.Update(guestReservation);                    
                    }

                    if (model.Guest.CompanyId > 0)
                    {
                        guest.CompanyId = model.Guest.CompanyId;
                    }
                    else
                    {
                        guest.BusinessAccount = null;
                        guest.CompanyId = null;
                    }

                    guest.IsFutureReservation = false;
                    guest.IsActive = true;
                    guest.Status = "LIVE";

                    _guestService.Update(guest);

                    var allRoomIds = guest.GuestRooms.Select(x => x.RoomId).ToList();

                    foreach (var id in allRoomIds)
                    {
                        var room = _roomService.GetById(id);
                        room.StatusId = (int)RoomStatusEnum.Occupied;
                        _roomService.Update(room);
                    }

                    //return RedirectToAction("EditGroupBooking", "Booking", new { id = model.Room.Id, itemSaved = true });
                    return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

                }

                var guestRoomsP = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRoomsP.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRoomsP,
                    selectedRoomIds = selectedRoomIds,
                    CheckinDate = model.CheckinDate,
                    CheckoutDate = model.CheckoutDate,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "GROUP BOOKING"
                };

                return View("EnterGuestDatesGroup", gbvm);

            }

            return RedirectToAction("Index", "Home");
        }

               
        [HttpPost]
        public ActionResult GroupReservationNewBooking(string selectedRoomIds, RoomBookingViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var roomIds = selectedRoomIds.Split(',');

            if (roomIds.Any())
            {
                var rooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var guestRooms = new List<GuestRoom>();

                var i = 0;

                foreach (var rm in rooms)
                {
                    DateTime dtIn;
                    DateTime dtOut;
                    DateTime.TryParse(Request.Form["arrive_" + rm.Id], out dtIn);
                    DateTime.TryParse(Request.Form["depart_" + rm.Id], out dtOut);
                    var groupBookingMainRoom = i == 0;

                    i++;
                    var newGuestRoom = new GuestRoom
                    {
                        RoomId = rm.Id,
                        Occupants = 1,
                        IsActive = true,
                        Children = model.GuestDiscount,
                        CheckinDate = DateTime.Now,
                        CheckoutDate = dtOut,
                        RoomRate = rm.Price.Value,
                        RoomNumber = rm.RoomNumber,
                        Notes = model.Guest.Notes,
                        GroupBooking = true,
                        GroupBookingMainRoom = groupBookingMainRoom
                    };
                   
                    var conflicts = rm.RoomAvailability(newGuestRoom.CheckinDate, newGuestRoom.CheckoutDate, null);

                    if (conflicts.Count > 0)
                    {
                        ModelState.AddModelError("_FORM",
                            "There is a reservation clash with your proposed checkin/checkout date(s) for " + newGuestRoom.Room.RoomNumber);
                    }
                    else
                    {
                        guestRooms.Add(newGuestRoom);
                    }
                }

                if (ModelState.IsValid)
                {
                    var rmsList = rooms;
                    Mapper.CreateMap<GuestViewModel, Guest>();
                    var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
                    var ticks = (int)DateTime.Now.Ticks;


                    if (model.InitialDeposit > 0)
                    {
                        guestRooms.FirstOrDefault(x => x.GroupBookingMainRoom).GuestRoomAccounts = new Collection<GuestRoomAccount>
                        {
                            new GuestRoomAccount
                            {
                                Amount = model.InitialDeposit,
                                PaymentTypeId = (int) RoomPaymentTypeEnum.InitialDeposit,
                                TransactionDate = DateTime.Now,
                                TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                            }
                        };
                    }
                    
                    guest.GuestRooms = guestRooms;
                    guest.GuestReservations = new Collection<GuestReservation>();

                    foreach (var gr in guest.GuestRooms)
                    {
                        guest.GuestReservations.Add(new GuestReservation
                        {
                            RoomId = gr.RoomId,
                            StartDate = DateTime.Now,
                            EndDate = gr.CheckoutDate,
                            GuestId = gr.GuestId,
                            IsActive = true
                        });
                    }

                    if (model.Guest.CompanyId > 0)
                    {
                        guest.CompanyId = model.Guest.CompanyId;
                    }
                    else
                    {
                        guest.BusinessAccount = null;
                        guest.CompanyId = null;
                    }

                    guest.IsFutureReservation = false;
                    guest.CreatedDate = DateTime.Now;
                    guest.Status = "LIVE";
                    guest.HotelId = HotelId;

                    var newGuest = _guestService.Create(guest);

                    if (newGuest != null)
                    {
                        CreateGuestCredentials(newGuest.Id, "");
                    }

                    foreach (var room in rmsList)
                    {
                        room.StatusId = (int)RoomStatusEnum.Occupied;
                        _roomService.Update(room);
                    }

                    //return RedirectToAction("EditGroupBooking", "Booking", new {id = guest.Id});
                    return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

                }

                var guestRoomsP = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var firstRoomId = guestRoomsP.FirstOrDefault().Id;

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = guestRoomsP,
                    selectedRoomIds = selectedRoomIds,
                    CheckinDate = model.CheckinDate,
                    CheckoutDate = model.CheckoutDate,
                    RoomBookingViewModel = GetModelForNewBooking(firstRoomId),
                    PageTitle = "GROUP BOOKING"
                };

                return View("EnterGuestDatesGroup", gbvm);

            }

            return RedirectToAction("Index", "Home");
        }        

        [HttpPost]
        public ActionResult EditFutureBooking(string selectedRoomIds, RoomBookingViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var guestRoomIds = selectedRoomIds.Split(',');
            var existingGuestRooms = _guestRoomService.GetAll(HotelId).Where(x => guestRoomIds.Contains(x.Id.ToString())).ToList();
            var roomIds = existingGuestRooms.Select(x => x.RoomId.ToString()).ToList();

            if (guestRoomIds.Any())
            {                
                var guestRooms = new List<GuestRoom>();
                
                int i = 0;

                foreach (var grm in existingGuestRooms)
                {
                    DateTime dtIn;
                    DateTime dtOut;
                    DateTime.TryParse(Request.Form["arrive_" + grm.Id], out dtIn);
                    DateTime.TryParse(Request.Form["depart_" + grm.Id], out dtOut);
                    var groupBookingMainRoom = i == 0;

                    i++;

                    var newGuestRoom = new GuestRoom
                    {
                        GuestId = model.GuestId,
                        Id = grm.Id,                       
                        Occupants = 1,
                        IsActive = true,
                        Children = model.GuestDiscount,
                        CheckinDate = dtIn,
                        CheckoutDate = dtOut,
                        RoomRate = grm.RoomRate,
                        Notes = model.Guest.Notes,
                        GroupBooking = true                    
                    };

                    var conflicts = grm.Room.RoomAvailability(newGuestRoom.CheckinDate, newGuestRoom.CheckoutDate,model.GuestId,null);

                    if (conflicts.Count > 0)
                    {
                        ModelState.AddModelError("CheckOutDate",
                            "There is a reservation clash with your proposed checkin/checkout date(s)");
                    }
                    else
                    {
                        guestRooms.Add(newGuestRoom);
                    }
                }

                if (ModelState.IsValid)
                {

                    Mapper.CreateMap<GuestViewModel, Guest>();
                    var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
                    var ticks = (int)DateTime.Now.Ticks;


                    //Update GuestRooms Individual
                    foreach (var editedGuestRoom in guestRooms)
                    {
                        var existingGuestRoom = _guestRoomService.GetById(editedGuestRoom.Id);
                        if (existingGuestRoom.GroupBookingMainRoom && model.InitialDeposit > 0)
                        {
                            existingGuestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                            {
                                Amount = model.InitialDeposit,
                                PaymentTypeId = (int)RoomPaymentTypeEnum.CashDeposit,
                                TransactionDate = DateTime.Now,
                                TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                            });
                        }

                        var existingReservation = _guestReservationService.GetAll(HotelId).FirstOrDefault(x => x.GuestId == existingGuestRoom.GuestId && x.RoomId == existingGuestRoom.RoomId);
                        existingGuestRoom.CheckinDate = editedGuestRoom.CheckinDate;
                        existingGuestRoom.CheckoutDate = editedGuestRoom.CheckoutDate;
                        existingReservation.StartDate = editedGuestRoom.CheckinDate;
                        existingReservation.EndDate = editedGuestRoom.CheckoutDate;
                        _guestReservationService.Update(existingReservation);
                        _guestRoomService.Update(existingGuestRoom);
                    }

                    var existingGuest = _guestService.GetById(model.GuestId);

                    if (model.Guest.CompanyId > 0)
                    {
                        guest.CompanyId = model.Guest.CompanyId;
                    }
                    else
                    {
                        guest.BusinessAccount = null;
                        guest.CompanyId = null;
                    }

                    existingGuest.FullName = guest.FullName;
                    existingGuest.Address = guest.Address;
                    existingGuest.CarDetails = guest.CarDetails;
                    existingGuest.CountryId = guest.CountryId;
                    existingGuest.Email = guest.Email;
                    existingGuest.Telephone = guest.Telephone;
                    existingGuest.Mobile = guest.Mobile;
                    existingGuest.PassportNumber = guest.PassportNumber;
                    existingGuest.Notes = guest.Notes;
                    existingGuest.IsFutureReservation = true;

                    if (model.Guest.CompanyId > 0)
                    {
                        existingGuest.CompanyId = guest.CompanyId;
                    }
                    else
                    {
                        existingGuest.BusinessAccount = null;
                        existingGuest.CompanyId = null;
                    }

                    _guestService.Update(existingGuest);

                    return RedirectToAction("EditFutureBooking", "Booking", new { id = guest.Id, itemSaved = true });
                }
                else
                {
                    var eGuest = _guestService.GetById(model.GuestId);
                    var eGuestRooms = eGuest.GuestRooms.ToList();
                    var roomIdValues = eGuestRooms.Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).ToDelimitedString(",");
                    var firstRoomId = eGuestRooms.FirstOrDefault( x => x.GroupBookingMainRoom).RoomId;

                    var gbvm = new GroupBookingViewModel
                    {
                        GuestRooms = null,
                        GuestId = eGuest.Id,
                        GuestRoomsCheckedIn = eGuestRooms,
                        selectedRoomIds = roomIdValues,
                        CheckinDate = DateTime.Now.AddDays(1),
                        CheckoutDate = DateTime.Now.AddDays(2),
                        RoomBookingViewModel = model,
                        PageTitle = "EDIT FUTURE RESERVATIONS"
                    };

                    gbvm.RoomBookingViewModel.BusinessAccounts = GetBusinessAccounts(null);
                    gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);
                    return View("EnterGuestDatesFutureEdit", gbvm);
                }

            }

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public ActionResult FutureReservationNewBooking(string selectedRoomIds, RoomBookingViewModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var roomIds = selectedRoomIds.Split(',');

            var checkingDateFuture = DateTime.Now;

            if (roomIds.Any())
            {
                var rooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var guestRooms = new List<GuestRoom>();

                int i = 0;

                foreach (var rm in rooms)
                {
                    DateTime dtIn;
                    DateTime dtOut;
                    DateTime.TryParse(Request.Form["arrive_" + rm.Id], out dtIn);
                    DateTime.TryParse(Request.Form["depart_" + rm.Id], out dtOut);
                    var groupBookingMainRoom = i == 0;

                    i++;
                    var newGuestRoom = new GuestRoom
                    {
                        RoomId = rm.Id,
                        Occupants = 1,
                        IsActive = false,
                        Children = model.GuestDiscount,
                        CheckinDate = dtIn,
                        CheckoutDate = dtOut,
                        RoomRate = rm.Price.Value,
                        Notes = model.Guest.Notes,
                        GroupBooking = true,
                        GroupBookingMainRoom = groupBookingMainRoom,
                        RoomNumber = rm.RoomNumber
                    };

                    

                    var conflicts = rm.RoomAvailability(newGuestRoom.CheckinDate, newGuestRoom.CheckoutDate, null);

                    if (conflicts.Count > 0)
                    {
                        ModelState.AddModelError("_FORM",
                             "There is a reservation clash with your proposed checkin/checkout date(s) for " + newGuestRoom.RoomNumber);
                    
                    }
                    else
                    {
                        guestRooms.Add(newGuestRoom);
                    }
                }

                if (ModelState.IsValid)
                {
                    var rmsList = rooms;
                    Mapper.CreateMap<GuestViewModel, Guest>();
                    var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
                    var ticks = (int)DateTime.Now.Ticks;

                    EmailTemplateModel eModel = new EmailTemplateModel();
                    eModel.InitialDeposit = model.InitialDeposit;

                    if (model.InitialDeposit > 0)
                    {

                        guestRooms.FirstOrDefault(x => x.GroupBookingMainRoom).GuestRoomAccounts = new Collection<GuestRoomAccount>
                        {
                            new GuestRoomAccount
                            {
                                Amount = model.InitialDeposit,
                                PaymentTypeId = (int) RoomPaymentTypeEnum.ReservationDeposit,
                                TransactionDate = DateTime.Now,
                                TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                                PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                            }
                        };
                    }

                    guest.GuestRooms = guestRooms;
                    guest.GuestReservations = new Collection<GuestReservation>();

                    foreach (var gr in guest.GuestRooms)
                    {
                        guest.GuestReservations.Add(new GuestReservation
                        {
                            RoomId = gr.RoomId,
                            StartDate = gr.CheckinDate,
                            EndDate = gr.CheckoutDate,
                            GuestId = gr.GuestId,
                            IsActive = true,
                            FutureBooking = true
                        });

                        checkingDateFuture = gr.CheckinDate;
                        eModel.ChekinDate = gr.CheckinDate;
                        eModel.ChekoutDate = gr.CheckoutDate;
                    }

                    if (model.Guest.CompanyId > 0)
                    {
                        guest.CompanyId = model.Guest.CompanyId;
                    }
                    else
                    {
                        guest.BusinessAccount = null;
                        guest.CompanyId = null;
                    }

                    guest.IsActive = false;
                    guest.IsFutureReservation = true;
                    guest.CreatedDate = DateTime.Now;
                    guest.HotelId = HotelId;
                    
                    var newGuest = _guestService.Create(guest);

                    if (newGuest != null)
                    {
                        CreateGuestCredentials(newGuest.Id,"");
                    }

                    try
                    {

                        eModel.Guest = newGuest;

                        var strMsg = "Dear " + newGuest.FullName + ", Thank you for choosing to stay at with us. Your reservation has been confirmed and your check-in date is " + checkingDateFuture.ToShortDateString();

                        SendSMS(newGuest.Telephone, strMsg);

                        SendEmailToGuestReservation(eModel);
                    }
                    catch
                    {

                    }

                    return RedirectToAction("PrintLandingForGuestCheckinFuture", "Home", new { id = guest.Id });

                }

                var eGuestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();                        

                var gbvm = new GroupBookingViewModel
                {
                    GuestRooms = eGuestRooms,
                    selectedRoomIds = selectedRoomIds,
                    CheckinDate = DateTime.Now.AddDays(1),
                    CheckoutDate = DateTime.Now.AddDays(2),
                    RoomBookingViewModel = model,
                    PageTitle = "FUTURE RESERVATIONS"
                };

                gbvm.RoomBookingViewModel.BusinessAccounts = GetBusinessAccounts(null);
                gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);
                return View("EnterGuestDatesFuture", gbvm);             

            }

            return RedirectToAction("Index", "Home");
        }

       
        [HttpPost]
        public ActionResult NewBooking(RoomBookingViewModel model, int? paymentMethodId, string paymentMethodNote, bool? complimentaryRoom)
        {
            int hotelIId = HotelId;


            model = PopulateFromRequest(model);

            var newRoom = _roomService.GetById(model.GuestRoom.RoomId);

            var conflicts = newRoom.RoomAvailability(model.GuestRoom.CheckinDate, model.GuestRoom.CheckoutDate, null);

            var commitChanges = true;        

            if (conflicts.Count > 0)
            {
                commitChanges = false;
                ModelState.AddModelError("CheckOutDate", "There is a reservation clash with your proposed checkout date");
            }

            if (ModelState.IsValid && commitChanges)
            {
                var rmsList = new List<Room> { newRoom };
                Mapper.CreateMap<GuestViewModel, Guest>();
                var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
                model.GuestRoom.IsActive = true;
                model.GuestRoom.RoomNumber = newRoom.RoomNumber;

                model.GuestRoom.Notes = string.Empty;
                model.GuestRoom.GroupBookingMainRoom = true;

                if(model.DiscountedRate > 0)
                {
                    model.GuestRoom.RoomRate = model.DiscountedRate;
                }

                if (model.GuestDiscount > 0)
                {
                    model.GuestRoom.Children = model.GuestDiscount;
                }

                if(complimentaryRoom.HasValue && complimentaryRoom.Value)
                {
                    model.GuestRoom.RoomRate = decimal.Zero;
                }

                var ticks = (int) DateTime.Now.Ticks;

                if (model.InitialDeposit > 0)
                {
                    model.GuestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>
                    {
                        new GuestRoomAccount
                        {
                            Amount = model.InitialDeposit,
                            PaymentTypeId = (int) RoomPaymentTypeEnum.InitialDeposit,
                            TransactionDate = DateTime.Now,
                            TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                            PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                        }
                    };
                }

                guest.GuestRooms = new Collection<GuestRoom> {model.GuestRoom};

                guest.GuestReservations = new Collection<GuestReservation>();

                foreach (var gr in guest.GuestRooms)
                {
                    guest.GuestReservations.Add(new GuestReservation
                    {
                        RoomId = gr.RoomId,
                        StartDate = DateTime.Now,
                        EndDate = gr.CheckoutDate,
                        GuestId = gr.GuestId,
                        IsActive = true
                    });
                }

                if (model.Guest.CompanyId > 0)
                {
                    guest.CompanyId = model.Guest.CompanyId;
                }
                else
                {
                    guest.BusinessAccount = null;
                    guest.CompanyId = null;
                }

                bool conCurencyIssue = false;

                foreach (var room in rmsList)
                {
                    try
                    {
                        room.StatusId = (int)RoomStatusEnum.Occupied;
                        _roomService.Update(room);
                    }
                    catch(Exception)
                    {
                        conCurencyIssue = true;
                    }
                }

                if (!conCurencyIssue)
                {

                    guest.IsFutureReservation = false;
                    guest.CreatedDate = DateTime.Now;
                    guest.HotelId = HotelId;

                    if(!string.IsNullOrEmpty(model.Title))
                    {
                        guest.Mobile = model.Title;
                    }

                    if (!string.IsNullOrEmpty(model.TaxiReg))
                    {
                        guest.PassportNumber = model.TaxiReg;
                    }

                    var guestCreated = _guestService.Create(guest);

                    EmailTemplateModel template = null;

                    if (guestCreated != null)
                    {
                        template = new EmailTemplateModel();
                        template.Guest = guestCreated;
                        template.ChekinDate = model.GuestRoom.CheckinDate;
                        template.ChekoutDate = model.GuestRoom.CheckoutDate;
                        template.InitialDeposit = model.InitialDeposit;
                        template.RoomNumber = model.GuestRoom.RoomNumber;
                        //template.RoomType = model.GuestRoom.Room.RoomType1.Name;
                        CreateGuestCredentials(guestCreated.Id, model.Title);
                    }

                    if(model.TaxiAmount > 0)
                    {
                        ExpenseModel em = new ExpenseModel();
                        em.Description = "Taxi for guest paid for, Room Number is : " + newRoom.RoomNumber + ", Guest Name is : " + guestCreated.FullName + ", The registration Number Of the Taxi is : " + model.TaxiReg;
                        //_personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID
                        em.StaffId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID;
                        em.Amount = model.TaxiAmount;
                        
                        InsertItem(em);
                    }

                    //var strTextMessage = string.Empty;

                    //if(model.InitialDeposit > 0)
                    //{
                    //    template.PaymentMethod = paymentMethodNote;

                    //    if(paymentMethodId.HasValue)
                    //    {
                    //        string strPaymentType = string.Empty;

                    //        if(paymentMethodId.Value == (int)PaymentMethodEnum.Cash)
                    //        {
                    //            strPaymentType = "CASH";
                    //        }
                    //        else if (paymentMethodId.Value == (int)PaymentMethodEnum.CreditCard)
                    //        {
                    //            strPaymentType = "CreditCard";
                    //        }
                    //        else if (paymentMethodId.Value == (int)PaymentMethodEnum.Cheque)
                    //        {
                    //            strPaymentType = "CHEQUE";
                    //        }
                    //        else
                    //        {
                    //            strPaymentType = "OTHER";
                    //        }

                    //        template.PaymentMethod = strPaymentType;
                    //    }

                    //    strTextMessage = "You have a new room booking, Room No. is " + newRoom.RoomNumber + ". An initial deposit was paid. NGN" + model.InitialDeposit + ", Payment Method is : "  + paymentMethodNote + ". Have a good day sir.";
                    //}
                    //else
                    //{
                    //    strTextMessage = "You have a new room booking, Room No. is " + newRoom.RoomNumber + ". No initial deposit was paid";
                    //}

                    //try
                    //{

                    //    SendSMSToOwner("", "", strTextMessage);
                    //    var strGuestMessage = "Dear " + guestCreated.Mobile + " " + guestCreated.FullName + ", You are welcome to " + GetHotelsName() + ".Thank you for choosing to stay with us. We do sincerely hope you enjoy your stay. Management";
                    //    SendSMS(guestCreated.Telephone, strGuestMessage);
                    //    template.RoomType = newRoom.RoomType1.Name;
                    //    template.Guest = guestCreated;
                    //    SendEmailToGuest(template);
                    //    SendEmail(template);
                       
                    //}
                    //catch
                    //{
                    //}

                }
                else
                {
                    ModelState.AddModelError("CheckOutDate", "ConCurrency Issues");
                    model.BusinessAccounts = GetBusinessAccounts(null);
                    return View(model);
                }

                return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });
                //return RedirectToAction("EditBooking", "Booking", new { id = model.Room.Id, itemSaved = true });
            }

            model.BusinessAccounts = GetBusinessAccounts(null);            
            return View(model);
        }


        public ActionResult EmailGuestCheckout(int? id)
        {
            var guestCreated = _guestService.GetById(id.Value);
            //
            var gr = guestCreated.GuestRooms.FirstOrDefault();

            var guestAccount = gr.GuestRoomAccounts.FirstOrDefault();

            var payment = guestCreated.Payments.LastOrDefault();

            int? paymentMethodId = 1;

            EmailTemplateModel template = new EmailTemplateModel();

            Room newRoom = null;

            if (guestCreated != null && guestAccount != null && gr != null && payment != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
                template.ChekinDate = gr.CheckinDate;
                template.ChekoutDate = DateTime.Now;
                template.InitialDeposit = payment.Total;
                template.RoomNumber = gr.RoomNumber;
                paymentMethodId = payment.PaymentMethodId;
                newRoom = gr.Room;
            }

            string strPaymentType = string.Empty;

            var strTextMessage = string.Empty;



            if (template.InitialDeposit > 0)
            {
                template.PaymentMethod = "";

                if (paymentMethodId.HasValue)
                {
                    if (paymentMethodId.Value == (int)PaymentMethodEnum.Cash)
                    {
                        strPaymentType = "CASH";
                    }
                    else if (paymentMethodId.Value == (int)PaymentMethodEnum.CreditCard)
                    {
                        strPaymentType = "CreditCard";
                    }
                    else if (paymentMethodId.Value == (int)PaymentMethodEnum.Cheque)
                    {
                        strPaymentType = "CHEQUE";
                    }
                    else
                    {
                        strPaymentType = "OTHER";
                    }

                    template.PaymentMethod = strPaymentType;
                }

                strTextMessage = "Your guest has just checked out , Room No. is " + newRoom.RoomNumber + ". An final payment was paid. NGN" + template.InitialDeposit + ", Payment Method is : " + strPaymentType + ". Have a good day sir.";
            }
            else
            {
                strTextMessage = "";
            }

            try
            {

                SendSMSToOwner("", "", strTextMessage);
                var strGuestMessage = "Dear " + guestCreated.Mobile + " " + guestCreated.FullName + ", You have been checked out of " + GetHotelsName() + ".Thank you for choosing to stay with us. We do sincerely hope you enjoyed your stay. Management";
                SendSMS(guestCreated.Telephone, strGuestMessage);
                template.RoomType = newRoom.RoomType1.Name;
                template.Guest = guestCreated;
                SendEmailToGuest(template, true);
                SendEmail(template);

            }
            catch
            {
            }

            return RedirectToAction("PrintLandingForGuest", "Home", new { id });
        }
        public ActionResult EmailGuest(int? id)
        {
            var guestCreated = _guestService.GetById(id.Value);
            //
            var gr = guestCreated.GuestRooms.FirstOrDefault();

            var guestAccount = gr.GuestRoomAccounts.FirstOrDefault();

            if(gr != null && !gr.IsActive)
            {
                return RedirectToAction("EmailGuestCheckout", new { id });
            }

            int? paymentMethodId = 1;

            EmailTemplateModel template = new EmailTemplateModel();

            Room newRoom = null;

            if (guestCreated != null && guestAccount != null && gr != null)
            {
                template = new EmailTemplateModel();
                template.Guest = guestCreated;
                template.ChekinDate = gr.CheckinDate;
                template.ChekoutDate = gr.CheckoutDate;
                template.InitialDeposit = guestAccount.Amount;
                template.RoomNumber = gr.RoomNumber;
                paymentMethodId = guestAccount.PaymentMethodId;
                newRoom = gr.Room;
            }

            string strPaymentType = string.Empty;

            var strTextMessage = string.Empty;

            if (template.InitialDeposit > 0)
            {
                template.PaymentMethod = "";

                if (paymentMethodId.HasValue)
                {
                    if (paymentMethodId.Value == (int)PaymentMethodEnum.Cash)
                    {
                        strPaymentType = "CASH";
                    }
                    else if (paymentMethodId.Value == (int)PaymentMethodEnum.CreditCard)
                    {
                        strPaymentType = "CreditCard";
                    }
                    else if (paymentMethodId.Value == (int)PaymentMethodEnum.Cheque)
                    {
                        strPaymentType = "CHEQUE";
                    }
                    else
                    {
                        strPaymentType = "OTHER";
                    }

                    template.PaymentMethod = strPaymentType;
                }

                strTextMessage = "You have a new room booking, Room No. is " + newRoom.RoomNumber + ". An initial deposit was paid. NGN" + template.InitialDeposit + ", Payment Method is : " + strPaymentType + ". Have a good day sir.";
            }
            else
            {
                strTextMessage = "";
            }

            try
            {

                SendSMSToOwner("", "", strTextMessage);
                var strGuestMessage = "Dear " + guestCreated.Mobile + " " + guestCreated.FullName + ", You are welcome to " + GetHotelsName() + ".Thank you for choosing to stay with us. We do sincerely hope you enjoy your stay. Management";
                SendSMS(guestCreated.Telephone, strGuestMessage);
                template.RoomType = newRoom.RoomType1.Name;
                template.Guest = guestCreated;
                SendEmailToGuest(template);
                SendEmail(template);

            }
            catch
            {
            }

            return RedirectToAction("PrintLandingForGuest", "Home", new { id });
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

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        }

        private int InsertItem(ExpenseModel cm)
        {
            cm.IsActive = true;
            cm.ExpenseDate = DateTime.Now;
            cm.ExpenseTypeId = 1;
            int id = 0;

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("ExpenseInsert", myConnection))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    myConnection.Open();

                    cmd.Parameters.AddWithValue("@ExpenseDate", cm.ExpenseDate);
                    cmd.Parameters.AddWithValue("@IsActive", cm.IsActive);
                    cmd.Parameters.AddWithValue("@Amount", cm.Amount);
                    cmd.Parameters.AddWithValue("@Description", cm.Description);
                    cmd.Parameters.AddWithValue("@StaffId", cm.StaffId);
                    cmd.Parameters.AddWithValue("@ExpenseTypeId", cm.ExpenseTypeId);

                    try
                    {
                        int.TryParse(cmd.ExecuteScalar().ToString(), out id);
                    }
                    catch
                    {

                    }
                }
            }

            return id;
        }

        private IEnumerable<SelectListItem> GetBusinessAccounts(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _businessAccountService.GetAll(HotelId).Where(x => !x.Debtor).ToList();
            bas.Insert(0, new BusinessAccount{ CompanyName =  "-- Please Select --", Id = 0});
            //return bas.Select(x => new SelectListItem { Text = x.CompanyName, Value = x.Id.ToString(), Selected = true });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == selectedId});
        }

        private IEnumerable<SelectListItem> GetMultiSelectRooms(int roomId)
        {
            return
                _roomService.GetAll(HotelId)
                    .Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && x.Id != roomId)
                    .Select(x => new SelectListItem{Text = x.RoomNumber, Value = x.Id.ToString(), Selected = true});
        }

        private IEnumerable<SelectListItem> ShowMultiSelectRooms(Guest guest)
        {
            return guest.GuestRooms.Select(x => new SelectListItem { Text = x.Room.RoomNumber, Value = x.Room.Id.ToString(CultureInfo.InvariantCulture), Selected = true });
        }

        private IEnumerable<SelectListItem> GetMultiSelectRooms(IEnumerable<string> ids)
        {
            return _roomService.GetAll(HotelId)
                    .Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && ids.Contains(x.Id.ToString(CultureInfo.InvariantCulture)))
                    .Select(x => new SelectListItem { Text = x.RoomNumber, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = true });
        }

        private RoomBookingViewModel PopulateFromRequest(RoomBookingViewModel model)
        {
            DateTime dtNow;
            DateTime dtFuture;
            var countryId = 1;

            DateTime.TryParse(Request.Form["arrive"], out dtNow);
            DateTime.TryParse(Request.Form["depart"], out dtFuture);
            int.TryParse(Request.Form["b-room"], out countryId);

            model.GuestRoom.CheckinDate = DateTime.Now;
            //xxxxx

            model.GuestRoom.CheckoutDate = dtFuture;
            model.Guest.CountryId = countryId;
            return model;
        }

        [HttpPost]
        public ActionResult SearchByNameFilter(string fullname, string roomNumber, DateTime? arrive, DateTime? depart)
        {
            if (!arrive.HasValue)
                arrive = DateTime.Now.AddMonths(-1);

            if (!depart.HasValue)
                depart = DateTime.Now.AddMonths(1);

            var allGuestRooms = _guestRoomService.GetAll(HotelId).ToList();

            if(!string.IsNullOrEmpty(fullname))
            {
                allGuestRooms = allGuestRooms.Where(x => (x.Guest.FullName.ToUpper().Contains(fullname.ToUpper()) || x.Guest.Email.ToUpper().Contains(fullname.ToUpper())) && x.Room.StatusId == (int)RoomStatusEnum.Occupied).ToList();
            }

            allGuestRooms = allGuestRooms.Where(x => x.CheckinDate.IsBetween(arrive.Value,depart.Value)).ToList(); 

            if(!string.IsNullOrEmpty(roomNumber))
            {
                allGuestRooms = allGuestRooms.Where(x => x.Room.RoomNumber.Contains(roomNumber)).ToList();
            }

            //SearchByNameFilter
            var guestRooms = allGuestRooms;
            var model = new SearchViewModel {GuestRoomsList = guestRooms};
            return View("Search", model);
        }
        
        [HttpGet]
        ////[OutputCache(Duration = 3600, VaryByParam = "none")]

        public ActionResult Search(DateTime? arrive, DateTime? depart, int? room_select)
        {
            var model = new SearchViewModel
            {
                GuestRoomsList = _guestRoomService.GetAll(HotelId).Where(x => (x.IsActive) && x.Room.StatusId == (int)RoomStatusEnum.Occupied).ToList()
            };

            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelId).SelectMany(x => x.GuestReservations).ToList();
            var now = DateTime.Today;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelId);

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var RBVmodel = new RoomBookingViewModel { RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList(), RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                RBVmodel.MonthId = now.Month;
                RBVmodel.ThisMonth = now;
                model.RoomBookingViewModele = RBVmodel;
                
            }
            else
            {
                var RBVmodel = new RoomBookingViewModel { RoomsList = allRooms, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                RBVmodel.MonthId = now.Month;
                RBVmodel.ThisMonth = now;
                model.RoomBookingViewModele = RBVmodel;

            }

            return View(model);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]

        public ActionResult SearchByName()
        {
            var model = new SearchViewModel {Fullname = string.Empty};
            return View(model);
        }

        [HttpPost]
        public ActionResult SearchByName(SearchViewModel model)
        {
                    
            var guestRooms =
                _guestRoomService.GetAll(HotelId)
                    .Where(x => (x.Guest.FullName.ToUpper().Contains(model.Fullname.ToUpper()) || x.Guest.Email.ToUpper().Contains(model.Fullname.ToUpper())) && x.Room.StatusId == (int)RoomStatusEnum.Occupied)
                    .ToList();
            model.GuestRoomsList = guestRooms;

            return View("Search", model);
        }

        [HttpPost]
        public ActionResult EditGroupBooking(RoomBookingViewModel model, string selectedRoomIds, int? paymentMethodId, string paymentMethodNote)
        {
            if (ModelState.IsValid)
            {
                var rmsList = new List<Room> { _roomService.GetById(model.GuestRoom.RoomId) };
                model = PopulateFromRequest(model);
                Mapper.CreateMap<GuestViewModel, Guest>();
                var guest = _guestService.GetById(model.GuestId);
                model.GuestRoom.IsActive = true;
                model.GuestRoom.Notes = string.Empty;
                model.GuestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
                var ticks = (int)DateTime.Now.Ticks;
                model.GuestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                {
                    Amount = model.GuestRoom.RoomRate,
                    PaymentTypeId = (int)RoomPaymentTypeEnum.InitialDeposit,
                    TransactionDate = DateTime.Now,
                    TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                    PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                });

                guest.GuestRooms = new Collection<GuestRoom> { model.GuestRoom };

                if (model.RoomBookingSelectedValues != null)
                {
                    foreach (var val in model.RoomBookingSelectedValues)
                    {
                        var extraRoomId = 0;
                        int.TryParse(val, out extraRoomId);

                        var extraRoom = _roomService.GetById(extraRoomId);
                        rmsList.Add(extraRoom);
                        var gr = new GuestRoom
                            {
                                IsActive = true,
                                RoomRate = extraRoom.Price.Value,
                                RoomId = extraRoomId,
                                Notes = guest.Notes,
                                Occupants = 1,
                                CheckinDate = model.GuestRoom.CheckinDate,
                                CheckoutDate = model.GuestRoom.CheckoutDate,
                                Children = model.GuestDiscount,
                                GuestId = guest.Id
                            };
                        guest.GuestRooms.Add(gr);
                    }
                }

                guest.GuestReservations = new Collection<GuestReservation>();

                foreach (var gr in guest.GuestRooms)
                {
                    guest.GuestReservations.Add(new GuestReservation { RoomId = gr.RoomId, StartDate = gr.CheckinDate, EndDate = gr.CheckoutDate, GuestId = gr.GuestId, IsActive = true });
                }

                _guestService.Update(guest);

                foreach (var room in rmsList)
                {
                    room.StatusId = (int)RoomStatusEnum.Occupied;
                    _roomService.Update(room);
                }

                //return RedirectToAction("EditGroupBooking", "Booking", new { id = model.Room.Id, itemSaved = true });
                return RedirectToAction("PrintLandingForGuestCheckin", "Home", new { id = guest.Id });

            }

            var roomIds = selectedRoomIds.Split(',');

            if (roomIds.Any())
            {
                var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
                var id = guestRooms.FirstOrDefault().Id;

                var rmm = new Room();
                rmm.BookRoom();
                Mapper.CreateMap<Room, RoomViewModel>();
                Mapper.CreateMap<Guest, GuestViewModel>();
                var room = _roomService.GetById(id);
                var rvm = Mapper.Map<Room, RoomViewModel>(room);
                var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest { IsActive = true, Status = "LIVE" });

                var model1 = new RoomBookingViewModel
                    {
                        Room = rvm,
                        Guest = gvm,
                        GuestRoom =
                            new GuestRoom
                                {
                                    Occupants = 1,
                                    RoomId = id,
                                    CheckinDate = DateTime.Now,
                                    CheckoutDate = DateTime.Now,
                                    RoomRate = room.Price.Value
                                },
                        RoomBookingRooms = GetMultiSelectRooms(roomIds),
                        selectedRoomIds = selectedRoomIds,
                        SelectedRoomDisplay = model.RoomBookingRooms.Select(x => x.Text).ToDelimitedString(",")
                    };

                return View(model1);
            }

            return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //public ActionResult EditFutureBookingRedundant(RoomBookingViewModel model, string selectedRoomIds)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var guest = _guestService.GetById(model.GuestId);
        //        var rmsList = new List<Room> { _roomService.GetById(model.GuestRoom.RoomId) };
        //        model = PopulateFromRequest(model);
        //        Mapper.CreateMap<GuestViewModel, Guest>();
        //        //var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
        //        model.GuestRoom.IsActive = true;
        //        model.GuestRoom.Notes = string.Empty;
        //        model.GuestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
        //        var ticks = (int)DateTime.Now.Ticks;
        //        model.GuestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
        //        {
        //            Amount = model.GuestRoom.RoomRate,
        //            PaymentTypeId = (int)RoomPaymentTypeEnum.InitialDeposit,
        //            TransactionDate = DateTime.Now,
        //            TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID
        //        });

        //        guest.GuestRooms = new Collection<GuestRoom> { model.GuestRoom };

        //        if (model.RoomBookingSelectedValues != null)
        //        {
        //            foreach (var val in model.RoomBookingSelectedValues)
        //            {
        //                int extraRoomId;
        //                int.TryParse(val, out extraRoomId);

        //                var extraRoom = _roomService.GetById(extraRoomId);
        //                rmsList.Add(extraRoom);
        //                var gr = new GuestRoom
        //                    {
        //                        IsActive = true,
        //                        RoomRate = extraRoom.Price.Value,
        //                        RoomId = extraRoomId,
        //                        Notes = guest.Notes,
        //                        Occupants = 1,
        //                        CheckinDate = model.GuestRoom.CheckinDate,
        //                        CheckoutDate = model.GuestRoom.CheckoutDate,
        //                        Children = 0,
        //                        GuestId = guest.Id
        //                    };
        //                guest.GuestRooms.Add(gr);
        //            }
        //        }

        //        guest.GuestReservations = new Collection<GuestReservation>();

        //        foreach (var gr in guest.GuestRooms)
        //        {
        //            guest.GuestReservations.Add(new GuestReservation { FutureBooking = true, RoomId = gr.RoomId, StartDate = gr.CheckinDate, EndDate = gr.CheckoutDate, GuestId = gr.GuestId, IsActive = true });
        //        }

        //        guest.IsFutureReservation = true;
        //        guest.CreatedDate = DateTime.Now;
        //        _guestService.Update(guest);

        //        foreach (var room in rmsList)
        //        {
        //            room.StatusId = (int)RoomStatusEnum.Occupied;
        //            _roomService.Update(room);
        //        }

        //        return RedirectToAction("EditFutureBooking", "Booking", new { id = model.Room.Id });
        //    }

        //    var roomIds = selectedRoomIds.Split(',');

        //    if (roomIds.Any())
        //    {
        //        var guestRooms = _roomService.GetAll(HotelId).Where(x => roomIds.Contains(x.Id.ToString())).ToList();
        //        var id = guestRooms.FirstOrDefault().Id;

        //        var rmm = new Room();
        //        rmm.BookRoom();
        //        Mapper.CreateMap<Room, RoomViewModel>();
        //        Mapper.CreateMap<Guest, GuestViewModel>();
        //        var room = _roomService.GetById(id);
        //        var rvm = Mapper.Map<Room, RoomViewModel>(room);
        //        var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest { IsActive = true, Status = "LIVE" });

        //        var model1 = new RoomBookingViewModel
        //            {
        //                Room = rvm,
        //                Guest = gvm,
        //                GuestRoom =
        //                    new GuestRoom
        //                        {
        //                            Occupants = 1,
        //                            RoomId = id,
        //                            CheckinDate = DateTime.Now,
        //                            CheckoutDate = DateTime.Now,
        //                            RoomRate = room.Price.Value
        //                        },
        //                RoomBookingRooms = GetMultiSelectRooms(roomIds),
        //                selectedRoomIds = selectedRoomIds,
        //                SelectedRoomDisplay = model.RoomBookingRooms.Select(x => x.Text).ToDelimitedString(",")
        //            };

        //        return View(model1);
        //    }

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]

        public ActionResult MakeRefund(int? id)
        {
            var gr = _guestRoomService.GetById(id.Value);
            var guest = _guestService.GetById(gr.GuestId);
            var guestRooms = guest.GuestRooms;
            var firstRoomId = gr.RoomId;
            var maxRefund = gr.GuestRoomAccounts.Sum(x => x.Amount);

            var gbvm = new GroupBookingViewModel
            {
                GuestRooms = null,                
                GuestId = guest.Id,
                MaxRefund = maxRefund,
                GuestRoomsCheckedIn = guestRooms,
                selectedRoomIds = string.Empty,
                CheckinDate = DateTime.Now.AddDays(1),
                CheckoutDate = DateTime.Now.AddDays(2),
                RoomBookingViewModel = GetModelForNewBooking(firstRoomId, guest, maxRefund),
                PageTitle = "REFUND & DELETE"
            };

            return View(gbvm);
        }

        private bool DeleteSingleBooking(Guest guest)
        {
            var gr = guest.GuestRooms.FirstOrDefault();
            guest.GuestReservations.Where(x => x.RoomId == gr.RoomId).ToList().ForEach(x => x.ForDelete = true);
            guest.GuestReservations.Where(x => x.RoomId == gr.RoomId).ToList().ForEach(x => x.IsActive = false);


            //foreach(var gres in guestReservations)
            //{
            //    gres.IsActive = false;
            //    gres.ForDelete = true;
            //    _guestReservationService.Update(gres);
            //}

            var guestRoomToDelete = _guestRoomService.GetById(gr.Id);
            guestRoomToDelete.IsActive = false;
            _guestRoomService.Update(guestRoomToDelete);
            guest.IsActive = false;
            guest.IsFutureReservation = false;
            _guestService.Update(guest);
            return true;
        }

        private bool DeleteMultipleBooking(Guest guest)
        {           
            foreach(var gr in guest.GuestRooms)
            {
                var guestRoomToDelete = _guestRoomService.GetById(gr.Id);
                var guestReservations = guest.GuestReservations.Where(x => x.RoomId == guestRoomToDelete.RoomId).ToList();
                guest.GuestReservations.Where(x => x.RoomId == guestRoomToDelete.RoomId).ToList().ForEach(x => x.ForDelete = true);
                guest.GuestReservations.Where(x => x.RoomId == guestRoomToDelete.RoomId).ToList().ForEach(x => x.IsActive = false);
                guestRoomToDelete.IsActive = false;
                _guestRoomService.Update(guestRoomToDelete);
            }          
           
            guest.IsActive = false;
            guest.IsFutureReservation = false;
            _guestService.Update(guest);
            return true;
        }

        

        [HttpPost]
        public ActionResult MakeRefund(GroupBookingViewModel model, decimal GuestRefund)
        {
             var guest = _guestService.GetById(model.GuestId);
            //var guestRoom = _guestRoomService.GetById(guest.GuestRoomId);
           

             var doRefund = false;
                        
            if(guest.GuestRooms.Count == 1)
            {
                doRefund = DeleteSingleBooking(guest);
            }
            else if(guest.GuestRooms.Count > 1)
            {
                doRefund = DeleteMultipleBooking(guest);
            }

            if (GuestRefund > 0 && doRefund)
            {
                var item = _itemService.GetById(1);
                var t = new Transaction { GuestId = model.GuestId, Amount = GuestRefund, IsActive = true, TranscationDate = DateTime.Now, StaffId = 1, GuestSignature = string.Empty };
                var existingItem = new Item{ HotelId = HotelId, Id = item.Id, Barcode = item.Barcode, Category = item.Category, Checker = item.Checker, Description = item.Description, DISCOUNT = item.DISCOUNT,
                    DISCOUNTEDPERCENTAGE = item.DISCOUNTEDPERCENTAGE, ItemName = item.ItemName, NotNumber = item.NotNumber, OrderStatus = item.OrderStatus, OrigPrice = item.OrigPrice, Picture = item.Picture, Quantity = item.Quantity, Status = item.Status, NotStatus = item.NotStatus, SubCategory = item.SubCategory, TotalQuantity = item.TotalQuantity, UnitPrice = item.UnitPrice, DISCOUNTENDDATE = item.DISCOUNTENDDATE, DISCOUNTSTARTDATE = item.DISCOUNTSTARTDATE};
                t.Items.Add(existingItem);
                _transactionService.Create(t);
            }

            return RedirectToAction("Index", "Home");           
        }

        
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]

        public ActionResult RemoveBookingOnEdit(int? id)
        {
            var gr = _guestRoomService.GetById(id.Value);

            var guestId = gr.GuestId;

            try
            {
                //_guestRoomService.Delete(gr);                
                var guestReservation = _guestReservationService.GetAll(HotelId).FirstOrDefault(x => x.GuestId == guestId && x.RoomId == gr.RoomId);
                _guestReservationService.Delete(guestReservation);
                _guestRoomService.Delete(gr);
            }
            catch (Exception)
            {
            }

            var guest = _guestService.GetById(guestId);
            var guestRooms = guest.GuestRooms.ToList();
            if (guestRooms.Count == 0)
            {
                var thisGuest = _guestService.GetById(guestId);
                _guestService.Delete(thisGuest);
                return RedirectToAction("Index", "Home");
            }

            var roomIdValues = guestRooms.Select(x => x.Room.Id.ToString()).ToDelimitedString(",");
            var firstRoomId = guestRooms.FirstOrDefault().RoomId;
            //var selectedRooms = guest.GuestRooms.Select(x => x.RoomId.ToString(CultureInfo.InvariantCulture)).ToArray();

            var roomIds = guestRooms.Select(x => x.RoomId.ToString()).ToList();           


            var gbvm = new GroupBookingViewModel
            {
                GuestRooms = null,
                GuestId = guest.Id,
                GuestRoomsCheckedIn = guestRooms,
                selectedRoomIds = roomIdValues,
                CheckinDate = DateTime.Now.AddDays(1),
                CheckoutDate = DateTime.Now.AddDays(2),
                RoomBookingViewModel = GetModelForNewBooking(firstRoomId, guest),
                //RoomBookingSelectedValues = selectedRooms,
                PageTitle = "EDIT FUTURE RESERVATIONS"
            };


            gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);
            gbvm.RoomBookingViewModel.GuestId = guest.Id;

            return View("EnterGuestDatesFutureEdit", gbvm);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,itemSaved")]

        public ActionResult EditFutureBooking(int? id, bool? itemSaved)
        {
            var guest = _guestService.GetById(id.Value);
            var guestRooms = guest.GuestRooms.ToList();
            var roomIdValues = guestRooms.Select(x => x.Id.ToString(CultureInfo.InvariantCulture)).ToDelimitedString(",");
            var firstRoom = guestRooms.FirstOrDefault(x => x.GroupBookingMainRoom);
            int firstRoomId = 0;

            if (null != firstRoom)
                firstRoomId = guestRooms.FirstOrDefault(x => x.GroupBookingMainRoom).RoomId;
            else
                firstRoomId = guestRooms.FirstOrDefault().Room.Id;

            var roomIds = guestRooms.Select(x => x.RoomId.ToString()).ToList();

            var gbvm = new GroupBookingViewModel
            {
                GuestRooms = null,
                GuestId = guest.Id,
                GuestRoomsCheckedIn = guestRooms,
                selectedRoomIds = roomIdValues,
                CheckinDate = DateTime.Now.AddDays(1),
                CheckoutDate = DateTime.Now.AddDays(2),
                RoomBookingViewModel = GetModelForNewBooking(firstRoomId,guest),
                PageTitle = "EDIT FUTURE RESERVATIONS",
                ItemSaved = itemSaved
            };

            gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);
            gbvm.RoomBookingViewModel.GuestId = guest.Id;
            return View("EnterGuestDatesFutureEdit", gbvm);
        }

        
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]

        public ActionResult NewBookingFromFutureReservation(int? id)
        {
            var guest = _guestService.GetById(id.Value);
            var guestRooms = guest.GuestRooms.ToList();
            var roomIdValues = guestRooms.Select(x => x.Room.Id.ToString()).ToDelimitedString(",");
            var firstRoomId = guestRooms.FirstOrDefault().RoomId;
            var roomIds = guestRooms.Select(x => x.Id.ToString()).ToList();

            var gbvm = new GroupBookingViewModel
            {
                GuestRooms = null,
                GuestId = guest.Id,
                GuestRoomsCheckedIn = guestRooms,
                selectedRoomIds = roomIdValues,
                CheckinDate = DateTime.Now.AddDays(1),
                CheckoutDate = DateTime.Now.AddDays(2),
                RoomBookingViewModel = GetModelForNewBooking(firstRoomId, guest),
                PageTitle = "NEW GUEST RESERVATIONS"
            };

            gbvm.RoomBookingViewModel.BusinessAccounts = GetBusinessAccounts(null);
            gbvm.RoomBookingViewModel.RoomBookingRooms = GetMultiSelectRooms(roomIds);
            gbvm.RoomBookingViewModel.ReservationDeposit = guest.GetGuestReservationBalance();

            return View("EnterGuestDatesBookNow", gbvm);        
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id,addToRoomCompleted,itemSaved")]

        public ActionResult EditGroupBooking(int? id, bool? addToRoomCompleted, bool? itemSaved)
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            Mapper.CreateMap<Guest, GuestViewModel>();
            var guest = _guestService.GetById(id.Value);
            var allGuestRooms = guest.GuestRooms;

            if(allGuestRooms.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var room = allGuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom).Room;
            var gr = room.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && x.IsActive);
            var rvm = Mapper.Map<Room, RoomViewModel>(gr.Room);
            var gvm = Mapper.Map<Guest, GuestViewModel>(guest);
            var initialDeposit = decimal.Zero;
            var gra = gr.GuestRoomAccounts.FirstOrDefault(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit);
            if (gra != null)
                initialDeposit = gra.Amount;
            var selectedRooms = guest.GuestRooms.Select(x => x.RoomId.ToString(CultureInfo.InvariantCulture)).ToArray();

            var model = new RoomBookingViewModel
                {
                    AddToRoomCompleted = addToRoomCompleted,
                    RoomBookingSelectedValues = selectedRooms,
                    GuestId = guest.Id,
                    Room = rvm,
                    Guest = gvm,
                    GuestRoom = gr,
                    InitialDeposit = initialDeposit,
                    RoomBookingRooms = ShowMultiSelectRooms(guest),
                    BusinessAccounts = GetBusinessAccounts(gvm.CompanyId),
                    DiscountedRate = gr.RoomRate,
                    ItemSaved = itemSaved
                };

            model.SelectedRoomDisplay = model.RoomBookingRooms.Select(x => x.Text).ToDelimitedString(",");
            model.selectedRoomIds = model.SelectedRoomDisplay;
            return View(model);
        }


        public ActionResult EditBookingAdjustable(int? id, bool? itemSaved, bool? guestTransferComplete)
        {
            var model = GetModelForEditBooking(id.Value, itemSaved, guestTransferComplete);
            return View(model);
        }
        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult EditBooking(int? id, bool? itemSaved, bool? guestTransferComplete)
        {
            var model = GetModelForEditBooking(id.Value, itemSaved, guestTransferComplete);
            return View(model);
        }


        private RoomBookingViewModel GetModelForEditBooking(int id, bool? itemSaved, bool? guestTransferComplete)
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            Mapper.CreateMap<Guest, GuestViewModel>();
            var room = _roomService.GetById(id);
            var forDelete = room.GuestRooms;

            var gr = room.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && x.IsActive) ?? room.GuestRooms.FirstOrDefault(x => x.IsActive);

            var initialDeposit = decimal.Zero;
            var gaccount = gr.GuestRoomAccounts.FirstOrDefault(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit);

            if (gaccount != null)
                initialDeposit = gaccount.Amount;
                 
            var rvm = Mapper.Map<Room, RoomViewModel>(gr.Room);

            var gvm = Mapper.Map<Guest, GuestViewModel>(gr.Guest);

            var selectList = ShowMultiSelectRooms(gr.Guest);

            var selectedRooms = gr.Guest.GuestRooms.Select(x => x.RoomId.ToString()).ToArray();          

            var model = new RoomBookingViewModel
            {
                GuestTransferComplete = guestTransferComplete,
                ItemSaved = itemSaved,
                GroupBooking = gr.GroupBooking,
                InitialDeposit = initialDeposit,
                Room = rvm,
                Guest = gvm,
                GuestRoom = gr,
                RoomBookingRooms = selectList,
                BusinessAccounts = GetBusinessAccounts(gvm.CompanyId),
                DiscountedRate = gr.RoomRate,
                GuestDiscount = gr.Children,
                RoomBookingSelectedValues = selectedRooms             
            };

            model.Title = gr.Guest.Mobile;

            model.TitleDropDown = GetTitles(model.Title);


            return model;
        }

     
        private RoomBookingViewModel GetModelForNewBooking(int id, Guest guest, decimal maxRefund)
        {
            var rmm = new Room();
            rmm.BookRoom();
            Mapper.CreateMap<Room, RoomViewModel>();
            Mapper.CreateMap<Guest, GuestViewModel>();
            var room = _roomService.GetById(id);
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            var gvm = Mapper.Map<Guest, GuestViewModel>(guest);
            var model = new RoomBookingViewModel
            {
                Room = rvm,
                Guest = gvm,
                MaxRefund  = maxRefund,
                GuestRefund = decimal.Zero,
                GuestRoom =
                    new GuestRoom
                    {
                        Occupants = 1,
                        RoomId = id,
                        CheckinDate = DateTime.Now,
                        CheckoutDate = DateTime.Now,
                        RoomRate = room.Price.Value
                    },

                BusinessAccounts = GetBusinessAccounts(null)
            };
            return model;
        }

        private RoomBookingViewModel GetModelForNewBooking(int id, Guest guest)
        {
            var rmm = new Room();
            rmm.BookRoom();
            Mapper.CreateMap<Room, RoomViewModel>();
            Mapper.CreateMap<Guest, GuestViewModel>();
            var room = _roomService.GetById(id);
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            var gvm = Mapper.Map<Guest, GuestViewModel>(guest);
            var model = new RoomBookingViewModel
            {
                Room = rvm,
                Guest = gvm,
                GuestRefund = decimal.Zero,
                GuestRoom =
                    new GuestRoom
                    {
                        Occupants = 1,
                        RoomId = id,
                        CheckinDate = DateTime.Now,
                        CheckoutDate = DateTime.Now,
                        RoomRate = room.Price.Value
                    },

                BusinessAccounts = GetBusinessAccounts(null)
            };
            return model;
        }


        private RoomBookingViewModel GetModelForNewBooking(int id)
        {
            var rmm = new Room();
            rmm.BookRoom();
            Mapper.CreateMap<Room, RoomViewModel>();
            Mapper.CreateMap<Guest, GuestViewModel>();
            var room = _roomService.GetById(id);
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            var gvm = Mapper.Map<Guest, GuestViewModel>(new Guest { IsActive = true, Status = "LIVE" });
            var model = new RoomBookingViewModel
            {
                Room = rvm,
                Guest = gvm,
                GuestRoom =
                    new GuestRoom
                    {
                        Occupants = 1,
                        RoomId = id,
                        CheckinDate = DateTime.Now,
                        CheckoutDate = DateTime.Now,
                        RoomRate = room.Price.Value
                    },

                BusinessAccounts = GetBusinessAccounts(null)
            };

            return model;
        }

        [HttpPost]
        public ActionResult EditBooking(RoomBookingViewModel model)
        {
            model = PopulateFromRequest(model);

            var newRoom = _roomService.GetById(model.GuestRoom.RoomId);

            var conflicts = newRoom.RoomAvailability(model.GuestRoom.CheckinDate, model.GuestRoom.CheckoutDate, null);

            if (conflicts.Count > 0)
            {
                var guestRoomUnavailable = conflicts.Any(x => x.GuestId != model.Guest.Id && x.RoomId != newRoom.Id);

                if (guestRoomUnavailable)
                {
                    ModelState.AddModelError("CheckOutDate", "There is a reservation clash with your proposed checkout date");
                }
            }

            if (ModelState.IsValid)
            {
                Mapper.CreateMap<GuestViewModel, Guest>();
                var guest = Mapper.Map<GuestViewModel, Guest>(model.Guest);
                var existingGuest = _guestService.GetById(model.Guest.Id);

                existingGuest.FullName = guest.FullName;

                if(!string.IsNullOrEmpty(model.Title))
                {
                    existingGuest.Mobile = model.Title;
                }

                if (!string.IsNullOrEmpty(model.TaxiReg))
                {
                    existingGuest.PassportNumber = model.TaxiReg;
                }

                existingGuest.Address = guest.Address;
                existingGuest.CarDetails = guest.CarDetails;
                existingGuest.CountryId = guest.CountryId;
                existingGuest.Email = guest.Email;
                existingGuest.Telephone = guest.Telephone;
                existingGuest.Mobile = model.Title;
                existingGuest.PassportNumber = guest.PassportNumber;
                existingGuest.Notes = guest.Notes;
                existingGuest.IsFutureReservation = false;
                existingGuest.CompanyId = guest.CompanyId;
                existingGuest.IsChild = guest.IsChild;

                if (existingGuest.CompanyId == 0)
                    existingGuest.BusinessAccount = null;

                if (model.DiscountedRate > 0)
                {
                    foreach(var gr in existingGuest.GuestRooms)
                    {
                        if(gr.RoomId == model.GuestRoom.RoomId)
                            gr.RoomRate = model.DiscountedRate;
                    }
                }

                if (model.GuestDiscount > 0)
                {
                    foreach (var gr in existingGuest.GuestRooms)
                    {
                        if (gr.RoomId == model.GuestRoom.RoomId)
                        {
                            gr.Children = model.GuestDiscount;
                        }
                    }
                }

                if(existingGuest.CompanyId == 0)
                {
                    existingGuest.BusinessAccount = null;
                    existingGuest.CompanyId = null;
                }

                _guestService.Update(existingGuest);

                return RedirectToAction("EditBooking", "Booking", new { id = model.Room.Id, itemSaved = true });
            }

            model.RoomBookingRooms = GetMultiSelectRooms(model.Room.Id);
            return View(model);
        }
	}
}