using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using HotelMateWebV1.Security;
using System.IO;
using Lib.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using System.Net;
using Invoicer.Services;
using Invoicer.Models;

namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class HomeController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IGuestService _guestService;
        private readonly IGuestReservationService _guestReservationService;
        private readonly IPersonService _personService = null;
        private readonly IGuestRoomService _guestRoomService;


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

        
        public HomeController()
        {
            _personService = new PersonService();
           _roomService = new RoomService();
           _guestService = new GuestService();
           _guestReservationService = new GuestReservationService();
           _guestRoomService = new GuestRoomService();

        }

        [ChildActionOnly]
        public ActionResult TopMenuMusic()
        {
            if (Request.IsAuthenticated)
            {
                var model = new RoomBookingViewModel { };
                return PartialView("_NavigationMusic", model);
            }
            else
            {
                var model = new RoomBookingViewModel { };
                return PartialView("_Navigation", model);
            }
        }

        [ChildActionOnly]
        public ActionResult TopMenu()
        {
            if (Request.IsAuthenticated)
            {
                var model = new RoomBookingViewModel {};
                return PartialView("_Navigation", model);
            }
            else
            {
                var model = new RoomBookingViewModel { };
                return PartialView("_Navigation", model);
            }
        }

        public ActionResult VideoJS()
        {
            return View();
        }

        public ActionResult OceansClip(int? id, string type)
        {

            FileInfo oceansClipInfo = null;
            string oceansClipMimeType = String.Format("videos/{0}", type);

            switch (type)
            {
                case "mp4":
                    oceansClipInfo = new FileInfo(Server.MapPath("~/Content/videos/The.Theory.of.Everything.2014.720p.BluRay.x264.YIFY.mp4"));
                    break;
                case "webm":
                    oceansClipInfo = new FileInfo(Server.MapPath("~/Content/video/oceans-clip.webm"));
                    break;
                case "ogg":
                    oceansClipInfo = new FileInfo(Server.MapPath("~/Content/video/oceans-clip.ogv"));
                    break;
            }

            return new RangeFilePathResult(oceansClipMimeType, oceansClipInfo.FullName, oceansClipInfo.LastWriteTimeUtc, oceansClipInfo.Length);
            //return new RangeFileStreamResult(oceansClipInfo.OpenRead(), oceansClipMimeType, oceansClipInfo.Name, oceansClipInfo.LastWriteTimeUtc);
        }

        [HttpPost]
        public ActionResult CheckAvailability(DateTime? arrived, DateTime? departed, int? room_select)
        {
            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();

            if(!departed.HasValue)
            {
                departed = arrived.Value.AddDays(1);
            }

            var conflicts = gr.SelectAvailable(arrived.Value, departed.Value, room_select.Value).ToList();

                //RoomsMatrixList = allRooms.ToList()

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var RoomsList = _roomService.GetAll(HotelID).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList();
                var model = new RoomBookingViewModel { CheckinDate = arrived, CheckoutDate = departed, RoomsList = RoomsList, RoomsMatrixList = RoomsList    };
                return View("Booking", model); 
            }
            else
            {
                var roomLst = _roomService.GetAll(HotelID).ToList();

                if(room_select.HasValue && room_select > 0)
                {
                    roomLst = roomLst.Where(x => x.RoomType == room_select).ToList();
                }

                var model = new RoomBookingViewModel { CheckinDate = arrived, CheckoutDate = departed, RoomsList = roomLst, RoomsMatrixList = roomLst };

                return View("Booking", model); 
            }
        }


        [HttpPost]
        public ActionResult CheckAvailabilityFuture(DateTime? arrived, DateTime? departed, int? room_select)
        {
            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();

            var conflicts = gr.SelectAvailable(arrived.Value, departed.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { CheckinDate = arrived, CheckoutDate = departed, RoomsList = _roomService.GetAll(HotelID).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                return View("NewFutureBooking", model);
            }
            else
            {
                var roomLst = _roomService.GetAll(HotelID).ToList();

                if (room_select.HasValue && room_select > 0)
                {
                    roomLst = roomLst.Where(x => x.RoomType == room_select).ToList();
                }

                var model = new RoomBookingViewModel { CheckinDate = arrived, CheckoutDate = departed, RoomsList = roomLst };

                return View("NewFutureBooking", model);
            }
        }


        [HttpPost]
        public ActionResult CheckAvailabilityGroupBooking(DateTime? arrived, DateTime? departed, int? room_select)
        {
            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();

            var conflicts = gr.SelectAvailable(arrived.Value, departed.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { CheckinDate = arrived, CheckoutDate = departed, RoomsList = _roomService.GetAll(HotelID).Where(x => (x.StatusId == (int)RoomStatusEnum.Vacant || x.StatusId == (int)RoomStatusEnum.Dirty) && !ids.Contains(x.Id)).ToList() };
                return View("NewFutureBooking", model);
            }
            else
            {
                var roomLst = _roomService.GetAll(HotelID).ToList();

                if (room_select.HasValue && room_select > 0)
                {
                    roomLst = roomLst.Where(x => x.RoomType == room_select).ToList();
                }

                var model = new RoomBookingViewModel { CheckinDate = arrived, CheckoutDate = departed, RoomsList = roomLst };

                return View("NewFutureBooking", model);
            }

        }


        //
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult NoRemoveableRooms(int? id)
        {
            var groupBookingList = _guestService.GetAll(HotelID).Where(x => x.IsActive == true && x.GuestRooms.Any(y => y.GroupBooking) && x.Id == id.Value).ToList();

            var model = new RoomBookingViewModel
            {
                CheckinDate = DateTime.Now,
                CheckoutDate = DateTime.Now.AddDays(1),
                GuestList = groupBookingList,
                NoRemoveableRooms = true
            };

            return View("AmendGroupBooking", model);
        }


        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult AmendGroupBooking()
        {
            var groupBookingList = _guestService.GetAll(HotelID).Where(x => x.IsActive == true && x.GuestRooms.Any(y => y.GroupBooking)).ToList();
            var model = new RoomBookingViewModel { CheckinDate = DateTime.Now, CheckoutDate = DateTime.Now.AddDays(1),
                                                   GuestList = groupBookingList
            };

            return View(model); 
        }

        public ActionResult Index2()
        {
            return View();
        }

        public ActionResult Index1()
        {
            return View();
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

        private string ActualMacAdress()
        {
            string actualAddress = string.Empty;

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("GetEventPics", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    myConnection.Open();

                    try
                    {
                        actualAddress = cmd.ExecuteScalar().ToString();
                    }
                    catch
                    {
                    }
                }
            }

            return actualAddress;
        }

        private bool GetMacAddress()
        {
            var actualMacadress = ActualMacAdress().ToUpper();

            return actualMacadress == "YES";
        }

        public void SendEmailLatest()
        {
            string smtpAddress = "smtp.mail.yahoo.com";
            int portNumber = 587;
            bool enableSSL = true;

            string emailFrom = "leboston@yahoo.com";
            string password = "Afeni280701@@";
            string emailTo = "leboston@yahoo.com";
            string subject = "Hello";
            string body = "Hello, I'm just writing this to say Hi!";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(emailFrom);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = body;
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
                    catch(Exception ex)
                    {
                        int p = 0;
                    }
                    
                }
            }

        }


        public void PrintInvoice()
        {
            new InvoicerApi(SizeOption.A4, OrientationOption.Landscape, "NGN")
               .TextColor("#CC0000")
               .BackColor("#FFD6CC")
               .Image(@"vodafone.jpg", 125, 32)
               .Company(Address.Make("FROM", new string[] { "Vodafone Limited", "Vodafone House", "The Connection", "Newbury", "Berkshire RG14 2FN" }, "1471587", "569953277"))
               .Client(Address.Make("BILLING TO", new string[] { "Isabella Marsh1", "Overton Circle", "Little Welland", "Worcester", "WR## 2DJ" }))
               .Items(new List<ItemRow> {
                    ItemRow.Make("Nexus 6", "Midnight Blue", (decimal)1, 20, (decimal)166.66, (decimal)199.99),
                    ItemRow.Make("24 Months (£22.50pm)", "100 minutes, Unlimited texts, 100 MB data 3G plan with 3GB of UK Wi-Fi", (decimal)1, 20, (decimal)360.00, (decimal)432.00),
                    ItemRow.Make("Special Offer", "Free case (blue)", (decimal)1, 0, (decimal)0, (decimal)0),
               })
               .Totals(new List<TotalRow> {
                    TotalRow.Make("Sub Total", (decimal)526.66),
                    TotalRow.Make("VAT @ 20%", (decimal)105.33),
                    TotalRow.Make("Total", (decimal)631.99, true),
               })
               .Details(new List<DetailRow> {
                    DetailRow.Make("PAYMENT INFORMATION", "Make all cheques payable to Vodafone UK Limited.", "", "If you have any questions concerning this invoice, contact our sales department at sales@vodafone.co.uk.", "", "Thank you for your business.")
               })
               .Footer("http://www.vodafone.co.uk")
               //.Save();
               .Save(@"C:\NewProjects\Juju\",0);
        }

       



        ////[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult Index(bool? loginFailed)
        {
            //SendEmailLatest();
            //PrintInvoice();

            var expiry = new DateTime(2018,12,31);

            var hotelName = GetHotelsName();

            var authentic = GetMacAddress();

            if (!authentic)
            {
                return View("AccessDenied");
                //return View(new BaseViewModel { FutureReservationCount = 0, LoginFailed = loginFailed, CheckinDate = DateTime.Today.Date, CheckoutDate = DateTime.Today.AddDays(1).Date });
            }

            if (Request.IsAuthenticated)
            {
                int reservationCount = _guestReservationService.GetAll(HotelID).Count(x => x.StartDate.Date == DateTime.Today.Date && x.FutureBooking);
                return View(new BaseViewModel {HotelName = hotelName, ExpiryDate = expiry, FutureReservationCount = reservationCount, CheckinDate = DateTime.Today.Date, CheckoutDate = DateTime.Today.AddDays(1).Date });             
            }
            else
            {
                if(User.IsInRole("GUEST"))
                {
                    return RedirectToAction("NewLogin", "Account");
                }

                return RedirectToAction("NewLogin", "Account");
                //int reservationCount = _guestReservationService.GetAll(HotelID).Count(x => x.StartDate.Date == DateTime.Today.Date && x.FutureBooking);
                //return View(new BaseViewModel { HotelName = hotelName, ExpiryDate = expiry, FutureReservationCount = 0, LoginFailed = loginFailed, CheckinDate = DateTime.Today.Date, CheckoutDate = DateTime.Today.AddDays(1).Date });
                //return RedirectToAction("Register", "Account");
            }
        }

        public ActionResult RemoveReservation(int? id)
        {
            var reservation = _guestReservationService.GetById(id.Value);

            if(reservation != null)
            {
                reservation.IsActive = false;
                _guestReservationService.Update(reservation);
            }
            return RedirectToAction("FutureBooking");
        }

        //[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult FutureBooking(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            var now = DateTime.Today;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelID);

            var guestList = _guestService.GetAll(HotelID).Where(x => !x.IsActive && x.IsFutureReservation).ToList();
          

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel {GuestList = guestList, RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList(), RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;
                model.UnusedReservations = allRooms.Where(x => ids.Contains(x.Id)).ToList();
                return View(model);  
            }
            else
            {
                var model = new RoomBookingViewModel {GuestList = guestList , RoomsList = allRooms, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now; return View(model);  
            }
        }

        public ActionResult FutureBookingShow(int? id)
        {
            var room = _roomService.GetById(id.Value);
            var guestLists = room.GuestReservations.Where(x => x.IsActive).Select(x => x.Guest);
            var guestList = guestLists.Where(x => !x.IsActive && x.IsFutureReservation).ToList();
            var model = new RoomBookingViewModel { GuestList = guestList };
            return View("FutureBooking", model);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult PrintLandingForGuest(int? id, DateTime? arrive, DateTime? depart, int? room_select)
        {
            var model = new RoomBookingViewModel { GuestId = id.Value};
            return View(model);
        }

        
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult PrintLandingForGuestCheckinFuture(int? id, DateTime? arrive, DateTime? depart, int? room_select)
        {
            var model = new RoomBookingViewModel { GuestId = id.Value };
            var newGuest = _guestService.GetById(id.Value);
            var path = Path.Combine(Server.MapPath("~/Products/Receipt/"));
            var imagePath = Path.Combine(Server.MapPath("~/images/Receipt"), "ReceiptLogo.jpg");
            model.FilePath = PDFReceiptPrinter.PrintInvoiceCheckingFuture(path, newGuest, imagePath);
            return View(model);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult PrintLandingForGuestCheckin(int? id, DateTime? arrive, DateTime? depart, int? room_select)
        {
            var model = new RoomBookingViewModel { GuestId = id.Value };

            var guestCreated = _guestService.GetById(id.Value);

            //var path = Path.Combine(Server.MapPath("~/Products"));
            var path = Path.Combine(Server.MapPath("~/Products/Receipt/"));

            var imagePath = Path.Combine(Server.MapPath("~/images/Receipt"), "ReceiptLogo.jpg");

            string filePath = PDFReceiptPrinter.PrintInvoiceChecking(path, guestCreated, imagePath);

            model.FilePath = filePath;

            return View(model);
        }

      

        //[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult NewFutureBooking(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            var allRooms = _roomService.GetAll(HotelID).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList() };
                model.UnusedReservations = allRooms.Where(x => ids.Contains(x.Id)).ToList();
                return View("NewFutureBooking", model);
            }
            else
            {
                var model = new RoomBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, RoomsList = _roomService.GetAll(HotelID).ToList() };
                return View("NewFutureBooking", model);
            }
        }

        //[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult GroupBooking(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, RoomsList = _roomService.GetAll(HotelID).Where(x => !ids.Contains(x.Id)).ToList() };
                return View("GroupBooking", model);
            }
            else
            {
                var model = new RoomBookingViewModel { CheckinDate = arrive, CheckoutDate = depart, RoomsList = _roomService.GetAll(HotelID).ToList() };
                return View("GroupBooking", model);
            }
        }

        //NextMonth
        public ActionResult NextMonth(int? id, int? room_select)
        {
            id++;

            if (id.Value > 12)
                id = 1;

            var nowNow = DateTime.Today;

            var now = new DateTime(nowNow.Year, id.Value, nowNow.Day);

            DateTime? arrive = new DateTime(now.Year, id.Value, 1);
            DateTime? depart = arrive.Value.AddMonths(1);



            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations.Where(y => y.IsActive)).ToList();


            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelID);

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList(), RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = arrive.Value.Month;
                model.ThisMonth = arrive.Value;
                return PartialView("_RoomMatrixDisplay", model);
            }
            else
            {
                var ids = _guestRoomService.GetAll(HotelID).Where(x => x.CheckoutDate >= arrive).Select(x => x.RoomId).ToList();
                var dontSelectRooms = allRooms.Where(x => ids.Contains(x.Id)).ToList();

                var model = new RoomBookingViewModel { RoomsList = dontSelectRooms, RoomsMatrixList = dontSelectRooms, StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = arrive.Value.Month;
                model.ThisMonth = arrive.Value;
                return PartialView("_RoomMatrixDisplay", model);
            }
        }

        public ActionResult PreviousMonth(int? id, int? room_select)
        {
            if (id.Value > 1)
                id--;

            var nowNow = DateTime.Today;

            var now = new DateTime(nowNow.Year, id.Value, nowNow.Day);

            DateTime? arrive = new DateTime(now.Year, id.Value, 1);
            DateTime? depart = arrive.Value.AddMonths(1);



            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelID);

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList(), RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = arrive.Value.Month;
                model.ThisMonth = arrive.Value;
                return PartialView("_RoomMatrixDisplay", model);
            }
            else
            {
                var model = new RoomBookingViewModel { RoomsList = allRooms, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = arrive.Value.Month;
                model.ThisMonth = arrive.Value;
                return PartialView("_RoomMatrixDisplay", model);
            }
            
        }


        public ActionResult ViewRooms(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            var now = DateTime.Today;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelID);

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList(), RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;
                return View("ViewRooms", model);
            }
            else
            {
                var model = new RoomBookingViewModel { RoomsList = allRooms, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;

                return View("ViewRooms", model);
            }
        }

       

        //
        public ActionResult MatrixRooms(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            var now = DateTime.Today;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelID);

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var model = new RoomBookingViewModel { RoomsList = allRooms.Where(x => !ids.Contains(x.Id)).ToList(), RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;
                return View("MatrixRooms", model);
            }
            else
            {
                var model = new RoomBookingViewModel { RoomsList = allRooms, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;

                return View("MatrixRooms", model);
            }
        }

        ////[OutputCache(Duration = 3600, VaryByParam = "arrive,depart,room_select")]
        public ActionResult Booking(DateTime? arrive, DateTime? depart, int? room_select)
        {
            if (!arrive.HasValue) arrive = DateTime.Now;
            if (!depart.HasValue) depart = DateTime.Now.AddDays(1);
            if (!room_select.HasValue) room_select = 0;                      

            IEnumerable<GuestReservation> gr = _roomService.GetAll(HotelID).SelectMany(x => x.GuestReservations).Where(x => x.IsActive).ToList();
            var now = DateTime.Today;

            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var daysInMonth = System.DateTime.DaysInMonth(now.Year, now.Month);
            var endOfMonth = new DateTime(now.Year, now.Month, daysInMonth);

            var conflicts = gr.SelectAvailable(arrive.Value, depart.Value, room_select.Value).ToList();
            var allRooms = _roomService.GetAll(HotelID);

            if (conflicts.Count > 0)
            {
                var ids = conflicts.Select(x => x.RoomId).ToList();
                var roomsWithoutConflict = allRooms.Where(x => !ids.Contains(x.Id)).ToList();
                var model = new RoomBookingViewModel { RoomsList = roomsWithoutConflict, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;
                return View("Booking", model);
            }
            else
            {
                var model = new RoomBookingViewModel { RoomsList = allRooms, RoomsMatrixList = allRooms.ToList(), StartOfMonth = startOfMonth, EndOfMonth = endOfMonth };
                model.MonthId = now.Month;
                model.ThisMonth = now;

                return View("Booking", model);
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
    }
}