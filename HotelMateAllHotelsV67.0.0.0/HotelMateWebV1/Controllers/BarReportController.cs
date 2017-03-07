using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using ReportManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using ClosedXML.Excel;
using System.IO;

namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class BarReportController : PdfViewController
    {
        private readonly IGuestService _guestService;
        private readonly IRoomService _roomService;
        private readonly IGuestRoomService _guestRoomService;
        private readonly IGuestReservationService _guestReservationService;
        private readonly IGuestRoomAccountService _guestRoomAccountService;
        private readonly int _hotelAccountsTime = 14;
        private readonly IBusinessAccountService _businessAccountService;
        private readonly IBusinessAccountCorporateService _businessCorporateAccountService;
        private readonly IPersonService _personService = null;
        private readonly IPersonTypeService _personTypeService = null;
        private readonly IExpenseService _expenseService = null;
        private readonly IEmployeeShiftService _employeeShiftService;
        private readonly ISoldItemService _soldItemService;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentMethodService _paymentMethodService;




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

        public BarReportController()
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
            _expenseService = new ExpenseService();
            _employeeShiftService = new EmployeeShiftService();
            _soldItemService = new SoldItemService();
            _purchaseOrderService = new PurchaseOrderService();
            _businessCorporateAccountService = new BusinessCorporateAccountService();
            _paymentService = new PaymentService();
            _paymentMethodService = new PaymentMethodService();
        }


        public ActionResult GuestCheckinPDF(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            //var lst = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            var lst = _guestRoomService.GetAll(HotelID).Where(x => x.IsActive && x.CheckinDate >= startDate && x.CheckinDate <= endDate).OrderBy(x => x.CheckinDate).ToList();

            var groupByList = lst.GroupBy(x => x.CheckinDate.ToShortDateString()).Select(x => new GuestRoomModel { CheckingDate = x.Key, ItemList = x.ToList(), Balance = decimal.Zero });

            model.GroupByList = groupByList.ToList();

            return this.ViewPdf("Guest CheckIn Report", "_GuestCheckinPdf", model);

            
        }

        public ActionResult GuestCheckin(DateTime? startDate, DateTime? endDate, string clickedButton)
        {
            if(!string.IsNullOrEmpty(clickedButton) && clickedButton.ToUpper().StartsWith("P"))
            {
                return RedirectToAction("GuestCheckinPDF", new { startDate, endDate });
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }
            ReportViewModel model = new ReportViewModel();

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);
            
            //var lst = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            var lst = _guestRoomService.GetAll(HotelID).Where(x => x.IsActive && x.CheckinDate >= startDate && x.CheckinDate <= endDate).OrderBy(x => x.CheckinDate).ToList();

            var groupByList = lst.GroupBy(x => x.CheckinDate.ToShortDateString()).Select(x => new GuestRoomModel { CheckingDate = x.Key, ItemList = x.ToList(), Balance = decimal.Zero });

            model.GroupByList = groupByList.ToList();

            return View(model);
        }

        //RoomHistory

        public ActionResult GuestList(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.HotelGuests = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }

        public ActionResult GuestCheckout(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            //var lst = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            var lst = _guestRoomService.GetAll(HotelID).Where(x => !x.IsActive &&  x.CheckoutDate >= startDate && x.CheckoutDate <= endDate).OrderBy(x => x.CheckoutDate).ToList();

            var groupByList = lst.GroupBy(x => x.CheckinDate.ToShortDateString()).Select(x => new GuestRoomModel { CheckingDate = x.Key, ItemList = x.ToList(), Balance = decimal.Zero });

            model.GroupByList = groupByList.ToList();

            return View(model);
        }

        public ActionResult GuestReservation(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);
            
            model.HotelGuests = _guestService.GetByQuery(HotelID).Where(x => !x.IsActive && x.IsFutureReservation && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && !y.IsActive).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }

        public ActionResult RoomHistory(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Rooms = _roomService.GetAll(HotelID).ToList();
            
            return View(model);
        }

        public ActionResult GuestGroupReservation(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);
            
            model.HotelGuests = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && !x.IsFutureReservation && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive && y.GroupBooking).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }

        public ActionResult RoomOccupancy(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Rooms = _roomService.GetAll(HotelID).ToList();

            model.StartDate = startDate;

             //   _guestService.GetByQuery(HotelID).Where(x => x.IsActive && !x.IsFutureReservation && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive && y.GroupBooking).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }

        //
        private decimal GetReservationTax()
        {
            int hTax = 0;

            try
            {
                int.TryParse(ConfigurationManager.AppSettings["ReservationTax"].ToString(), out hTax);
            }
            catch
            {
                hTax = 0;
            }

            if (hTax > 0)
            {
                return decimal.Divide((decimal)hTax, 100M);
            }

            return decimal.Zero;
        }

        private decimal GetHotelTax()
        {
            int hTax = 0;

            try
            {
                int.TryParse(ConfigurationManager.AppSettings["HotelTax"].ToString(), out hTax);
            }
            catch
            {
                hTax = 0;
            }

            if(hTax > 0)
            {
                return decimal.Divide((decimal)hTax, 100M);
            }

            return decimal.Zero;
        }

      
        public ActionResult AccountReceivable(DateTime? startDate, DateTime? endDate, int? id)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            var ptl = _paymentMethodService.GetAll(1).Where(x => x.Id != 4 && x.Id != 6).ToList();

            ptl.Insert(0, new HotelMateWeb.Dal.DataCore.PaymentMethod { Id = 0, Name ="--All--" });

            IEnumerable<SelectListItem> selectList =
                from c in ptl
                select new SelectListItem
                {
                    Selected = (c.Id == id),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };

            model.selectList = selectList;

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Accounts = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate
                && ((x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.HalfDay)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Laundry)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Miscellenous)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit))
                && (x.PaymentMethodId != (int)PaymentMethodEnum.POSTBILL)

                ).OrderByDescending(x => x.TransactionDate).ToList();


            if(id.HasValue && id.Value > 0)
            {
                model.Accounts = model.Accounts.Where(x => x.PaymentMethodId == id.Value).ToList();
            }

            
            var hotelPayments = _paymentService.GetAllHotel().Where(x => x.Type == 2).ToList();

            if (id.HasValue && id.Value > 0)
            {
                model.Accounts = model.Accounts.Where(x => x.PaymentMethodId == id.Value).ToList();
                hotelPayments = hotelPayments.Where(x => x.PaymentMethodId == id.Value).ToList();
            }

            var overallTotal = model.Accounts.Sum(x => x.Amount);

            model.Tax = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.TaxAmount);

            model.Discount = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.DiscountAmount);

            model.GrandTotal = overallTotal + model.Tax - model.Discount;

            model.ReportName = "AccountReceivable";

            model.FileToDownloadPath = GenerateExcelSheet(model, model.ReportName);

            return View(model);
        }

        [ValidateInput(false)]
        public FileResult DownloadStatement(string id)
        {
            //id = "1930_AccountReceivable";
            var path = Path.Combine(Server.MapPath("~/Products/Receipt/"), id + ".xlsx");
            var fileName = DateTime.Now.ToShortDateString() + "_" + "Excel.xlsx";
            return File(path, "application/ms-excel", fileName);
        }

        private string GenerateExcelSheet(ReportViewModel model, string reportName)
        {

            DataTable dt = new DataTable();

           

            dt.Columns.AddRange(new DataColumn[8] {
                                new DataColumn("Date", typeof(string)),
                                new DataColumn("Room", typeof(string)),
                                new DataColumn("Purpose", typeof(string)),
                                new DataColumn("Guest",typeof(string)),
                                new DataColumn("Check In",typeof(string)),
                                new DataColumn("Check Out",typeof(string)),
                                new DataColumn("Pay Method",typeof(string)),
                                new DataColumn("Amount (NGN)",typeof(string))

            });

            int p = 1;

            foreach (var ru in model.Accounts)
            {
                dt.Rows.Add(ru.TransactionDate, ru.GuestRoom.Room.RoomNumber, ru.RoomPaymentType.Name, ru.GuestRoom.Guest.FullName,
                            ru.GuestRoom.CheckinDate.ToShortDateString(), ru.GuestRoom.CheckoutDate.ToShortDateString(), ru.PaymentMethod.Description,
                            ru.Amount);
                p++;
            }

            dt.Rows.Add("Total", "", "", "",
                         "", "", "",
                         model.Accounts.Sum(x => x.Amount));
            p++;

            dt.Rows.Add("Tax", "", "", "",
                           "", "", "",
                           model.Tax);
            p++;

            dt.Rows.Add("Discount", "", "", "",
                          "", "", "",
                          model.Discount);
            p++;

           

            dt.Rows.Add("Grand Total", "", "", "",
                          "", "", "",
                          model.GrandTotal);
            p++;

            var fileName = DateTime.Now.ToShortTimeString().Replace(":","") + "_" + reportName;

            var fileNameToUse = fileName + ".xlsx";

            var path = Path.Combine(Server.MapPath("~/Products/Receipt/"), fileNameToUse);

            //Codes for the Closed XML
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, reportName);
                wb.SaveAs(path);
            }

            return fileName;
        }

        public ActionResult AccountReceivableCash(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Accounts = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate
                && ((x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.HalfDay)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Laundry)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Miscellenous)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit))
                && (x.PaymentMethodId == (int)PaymentMethodEnum.Cash)

                ).OrderByDescending(x => x.TransactionDate).ToList();

            var overallTotal = model.Accounts.Sum(x => x.Amount);

            var hotelPayments = _paymentService.GetAllHotel().Where(x => x.Type == 2 && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).ToList();

            model.Tax = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.TaxAmount);

            model.Discount = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.DiscountAmount);

            model.GrandTotal = overallTotal + model.Tax - model.Discount;

            model.ReportName = "AccountReceivableCash";

            model.FileToDownloadPath = GenerateExcelSheet(model, model.ReportName);

            return View(model);
        }



        public ActionResult AccountReceivableCheque(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Accounts = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate
                && ((x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.HalfDay)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Laundry)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Miscellenous)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit))
                && (x.PaymentMethodId == (int)PaymentMethodEnum.Cheque)

                ).OrderByDescending(x => x.TransactionDate).ToList();


            var overallTotal = model.Accounts.Sum(x => x.Amount);

            var hotelPayments = _paymentService.GetAllHotel().Where(x => x.Type == 2 && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).ToList();

            model.Tax = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.TaxAmount);

            model.Discount = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.DiscountAmount);

            model.GrandTotal = overallTotal + model.Tax - model.Discount;

            model.ReportName = "AccountReceivableCheque";

            model.FileToDownloadPath = GenerateExcelSheet(model, model.ReportName);

            return View(model);
        }

        public ActionResult AccountReceivablePOS(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Accounts = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate
                && ((x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.HalfDay)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Laundry)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Miscellenous)
                || (x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit))
                && (x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard)

                ).OrderByDescending(x => x.TransactionDate).ToList();


            var overallTotal = model.Accounts.Sum(x => x.Amount);

            var hotelPayments = _paymentService.GetAllHotel().Where(x => x.Type == 2 && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).ToList();

            model.Tax = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.TaxAmount);

            model.Discount = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.DiscountAmount);

            model.GrandTotal = overallTotal + model.Tax - model.Discount;

            model.ReportName = "AccountReceivableCash";

            model.FileToDownloadPath = GenerateExcelSheet(model, model.ReportName);

            return View(model);
        }


        [HttpGet]
        public ActionResult SendEmail(bool? saved)
        {
            return View(new ReportViewModel { ItemSaved = saved });
        }

        [HttpPost]
        public ActionResult SendEmail(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var dest = GetOwnersTelephone();

            var now = DateTime.Today.AddDays(-1);

            var salesTotalRestaurant = GetRestaurantTotal(startDate, endDate);

            var salesTotalHotel = GetHotelTotal(startDate, endDate);

            var total = salesTotalHotel + salesTotalRestaurant;

            var emailTemplate = @"<p style='margin:0px;padding:0px;font-size:12px;font-family:Arial, Helvetica, sans-serif;color:#555;' id='yui_3_16_0_ym19_1_1463261898755_4224'>Warm Greetings,<br>
                                <br>This is to kindly inform you of your daily sales records, the sales details are listed below : <br>
                                <br>Start Date: XXDATEXX <br><br>
                                <br>End Date: XXENDDATEXX <br><br>
                                <br>Non Room Sales: XXNONROOMSALESXX <br><br>
                                <br>Room Sales: XXROOMSALESXX <br><br>
                                <br>Total Sales: XXTOTALSALESXX <br><br>
                                </p>";

            emailTemplate = emailTemplate.Replace("XXDATEXX", startDate.Value.ToString());
            emailTemplate = emailTemplate.Replace("XXENDDATEXX", endDate.Value.ToString());
            emailTemplate = emailTemplate.Replace("XXNONROOMSALESXX", salesTotalRestaurant.ToString());
            emailTemplate = emailTemplate.Replace("XXROOMSALESXX", salesTotalHotel.ToString());
            emailTemplate = emailTemplate.Replace("XXTOTALSALESXX", total.ToString());


            try
            {
                var emails = dest.Split(',').ToList();

                foreach (var email in emails)
                {

                    MailMessage mail = new MailMessage("academyvistang@gmail.com", email, "Your daily sales report", emailTemplate);
                    mail.From = new MailAddress("academyvistang@gmail.com", "HotelMate");
                    mail.IsBodyHtml = true; // necessary if you're using html email
                    NetworkCredential credential = new NetworkCredential("academyvistang@gmail.com", "Lauren280701");
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = credential;
                    smtp.Send(mail);
                }
            }
            catch
            {

            }


            return RedirectToAction("SendEmail", new { saved = true });
        }

        private string GetOwnersTelephone()
        {

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

        private decimal GetHotelTotal(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate.Value;

            DateTime end = endDate.Value;

            var total = decimal.Zero;

            GuestRoomAccountService sis = new GuestRoomAccountService();

            total = sis.GetAll(HotelID).Where(x => x.RoomPaymentType.PaymentStatusId == 2 && (x.TransactionDate >= start && x.TransactionDate <= end)).Sum(x => x.Amount);

            return total;
        }

        private decimal GetRestaurantTotal(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate.Value;

            DateTime end = endDate.Value;

            var total = decimal.Zero;

            SoldItemService sis = new SoldItemService();

            total = sis.GetAll().Where(x => x.IsActive && (x.DateSold >= start && x.DateSold <= end)).Sum(x => x.TotalPrice);

            return total;
        }
        public ActionResult AccountPayable(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            model.Accounts = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate && x.PaymentTypeId == (int)RoomPaymentTypeEnum.Refund).OrderByDescending(x => x.TransactionDate).ToList();

            var overallTotal = model.Accounts.Sum(x => x.Amount);

            var hotelPayments = _paymentService.GetAllHotel().Where(x => x.Type == 2 && x.PaymentTypeId == (int)RoomPaymentTypeEnum.Refund).ToList();

            model.Tax = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.TaxAmount);

            model.Discount = hotelPayments.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).Sum(x => x.DiscountAmount);

            model.GrandTotal = overallTotal + model.Tax - model.Discount;

            model.ReportName = "AccountPayable";

            model.FileToDownloadPath = GenerateExcelSheet(model, model.ReportName);

            return View(model);
        }

        public ActionResult CreditGuestReport(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);
            
            model.HotelGuests = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && !x.IsFutureReservation && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive && y.GroupBooking).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }

       
        public ActionResult CorporateGuestReport(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);
            
            model.HotelGuests = _guestService.GetByQuery(HotelID).Where(x => x.CompanyId.HasValue && !x.IsFutureReservation && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }


        public IEnumerable<ExpenseModel> GetAllItems()
        {
            List<ExpenseModel> lst = new List<ExpenseModel>();

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM EXPENSE", myConnection))
                {
                    myConnection.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int id = dr.GetInt32(0);    // Weight int
                            DateTime expenseDate = dr.GetDateTime(1);    // Weight int
                            bool isActive = dr.GetBoolean(2); // Breed string 
                            decimal amount = dr.GetDecimal(3);
                            int staffId = dr.GetInt32(4);
                            int expenseTypeId = dr.GetInt32(7);
                            string description = dr.GetString(8);


                            yield return new ExpenseModel
                            {
                                Id = id,
                                ExpenseDate = expenseDate,
                                IsActive = isActive,
                                Amount = amount,
                                Description = description,
                                ExpenseTypeId = expenseTypeId
                            };

                        }
                    }
                }
            }
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        }


        public ActionResult Inventory(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var all = _purchaseOrderService.GetAll("").ToList();

            var accountsPaidOut = all.Where(x => x.OrderDate >= startDate.Value && x.OrderDate <= endDate).ToList();

            model.InventoryList = accountsPaidOut;

            return View(model);

        }

        public ActionResult PandL(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var accountsPaidin = _soldItemService.GetAll().Where(x => x.IsActive && !x.BusinessAccountId.HasValue && x.DateSold >= startDate.Value && x.DateSold <= endDate && (x.PaymentMethodId != (int)PaymentMethodEnum.POSTBILL)).ToList();

            var accountsPaidin1 = _businessCorporateAccountService.GetAll(1).Where(x => x.TransactionDate >= startDate.Value && x.TransactionDate <= endDate).ToList();

            var all = _purchaseOrderService.GetAll("").ToList();

            var accountsPaidOut = all.Where(x => x.OrderDate >= startDate.Value && x.OrderDate <= endDate).ToList();

            var expenses = GetAllItems().Where(x => x.ExpenseDate >= startDate && x.ExpenseDate <= endDate).ToList();

            var balanceSheetModel = accountsPaidin.Select(x => new BalanceSheetModel { TransactionDate = DateTime.Parse(x.DateSold.Value.ToShortDateString()), AmountPaidIn = x.TotalPrice, AmountPaidOut = decimal.Zero, Cashier = x.TransactionId, PaymentMentMethod = x.PaymentMethod, PaymentType = null }).ToList();

            balanceSheetModel.AddRange(accountsPaidin1.Select(x => new BalanceSheetModel { TransactionDate = DateTime.Parse(x.TransactionDate.ToShortDateString()), AmountPaidIn = x.Amount, AmountPaidOut = decimal.Zero, Cashier = x.TransactionId, PaymentMentMethod = x.PaymentMethod, PaymentType = null }));

            balanceSheetModel.AddRange(accountsPaidOut.Select(x => new BalanceSheetModel { TransactionDate = DateTime.Parse(x.OrderDate.ToShortDateString()), AmountPaidIn = decimal.Zero, AmountPaidOut = x.NetValue, Cashier = x.RaisedBy, PaymentMentMethod = null, PaymentType = null }));

            balanceSheetModel.AddRange(expenses.Select(x => new BalanceSheetModel { TransactionDate = DateTime.Parse(x.ExpenseDate.ToShortDateString()), AmountPaidOut = x.Amount, AmountPaidIn = decimal.Zero, Cashier = x.StaffId, PaymentMentMethod = null, PaymentType = null }).ToList());

            var allStaff = _personService.GetAllForLogin().ToList();

            balanceSheetModel = balanceSheetModel.OrderByDescending(x => x.TransactionDate).ToList();

            balanceSheetModel.ForEach(x => x.UserName = GetUserName(allStaff, x.Cashier.Value));

            var tPaidIn = balanceSheetModel.Sum(x => x.AmountPaidIn);

            var tPaidOut = balanceSheetModel.Sum(x => x.AmountPaidOut);

            model.FullBalance = tPaidIn - tPaidOut;

            model.BalanceSheet = balanceSheetModel;

            model.ConciseBalanceSheetSheet = balanceSheetModel.GroupBy(x => x.TransactionDate).Select(x => new ConciseBalanceSheetModel { ActualDate = x.Key, TotalRecieveable = x.Sum(y => y.AmountPaidIn), TotalPayaeble = x.Sum(z => z.AmountPaidOut) }).ToList();


            return View(model);
        }

        public ActionResult BalanceSheetBar(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var accountsPaidin = _soldItemService.GetAll().Where(x => x.DateSold >= startDate.Value && x.DateSold <= endDate && (x.PaymentMethodId != (int)PaymentMethodEnum.POSTBILL)).ToList();

            var balanceSheetModel = accountsPaidin.Select(x => new BalanceSheetModel { TransactionDate = x.DateSold.Value, AmountPaidIn = x.TotalPrice, AmountPaidOut = decimal.Zero, 
                Cashier = x.TransactionId, PaymentMentMethod = x.PaymentMethod, PaymentTypeId = x.PaymentTypeId }).ToList();
           
            var allStaff = _personService.GetAllForLogin().ToList();

            balanceSheetModel = balanceSheetModel.OrderByDescending(x => x.TransactionDate).ToList();

            balanceSheetModel.ForEach(x => x.UserName = GetUserName(allStaff, x.Cashier.Value));

            var tPaidIn = balanceSheetModel.Sum(x => x.AmountPaidIn);

            var tPaidOut = balanceSheetModel.Sum(x => x.AmountPaidOut);

            model.FullBalance = tPaidIn - tPaidOut;

            model.BalanceSheet = balanceSheetModel;

            return View(model);
        }

        public ActionResult BalanceSheet(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);
            
            var accountsPaidIn = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate.Value && x.TransactionDate <= endDate && (x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit || x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit || x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit)).ToList();
            
            var accountsPaidOut = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate.Value && x.TransactionDate <= endDate && (x.PaymentTypeId == (int)RoomPaymentTypeEnum.Refund)).ToList();
            
            var expenses = GetAllItems().Where(x => x.ExpenseDate >= startDate && x.ExpenseDate <= endDate);

            var balanceSheetModel = accountsPaidIn.Select(x => new BalanceSheetModel { TransactionDate = x.TransactionDate, AmountPaidIn = x.Amount, AmountPaidOut = decimal.Zero, Cashier = x.TransactionId, PaymentMentMethod = x.PaymentMethod, PaymentType = x.RoomPaymentType }).ToList();

            balanceSheetModel.AddRange(accountsPaidOut.Select(x => new BalanceSheetModel { TransactionDate = x.TransactionDate, AmountPaidIn = decimal.Zero, AmountPaidOut = x.Amount, Cashier = x.TransactionId, PaymentMentMethod = x.PaymentMethod, PaymentType = x.RoomPaymentType }));

            balanceSheetModel.AddRange(expenses.Select(x => new BalanceSheetModel { TransactionDate = x.ExpenseDate, AmountPaidOut = x.Amount, AmountPaidIn = decimal.Zero, Cashier = x.StaffId, PaymentMentMethod = null, PaymentType = null }).ToList());

            var allStaff = _personService.GetAllForLogin().ToList();

            balanceSheetModel = balanceSheetModel.OrderByDescending(x => x.TransactionDate).ToList();

            balanceSheetModel.ForEach(x => x.UserName = GetUserName(allStaff, x.Cashier.Value));

            var tPaidIn = balanceSheetModel.Sum(x => x.AmountPaidIn);

            var tPaidOut = balanceSheetModel.Sum(x => x.AmountPaidOut);

            model.FullBalance = tPaidIn - tPaidOut;
           
            model.BalanceSheet = balanceSheetModel;

            return View(model);
        }

        private string GetUserName(List<HotelMateWeb.Dal.DataCore.Person> allStaff, int p)
        {
            var staff = allStaff.FirstOrDefault(x => x.PersonID == p);
            if(staff != null)
                return staff.Username;

            return "";
        }

       
        public ActionResult CombinedSales(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var combinedSales = _guestRoomAccountService.GetAll(HotelID).Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate && (x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit)).ToList();

            //model.ModelGroupBy = combinedSales.GroupBy(x => x.TransactionDate.ToShortDateString()).Select(x => new SoldItemModel { PaymentTypeName = x.Key, TotalPrice = x.Sum(y => y.Amount),
                //RoomAccountList = x.ToList() }).ToList();

            var neWCombinedSalesModel = combinedSales.Select(x => new CombinedSalesModel {DateSold = x.TransactionDate, GuestRoom = x.GuestRoom, Amount = x.Amount, PaymentMethod = x.PaymentMethod.Name, 
                PaymentMethodNote = x.PaymentMethodNote, StaffName = x.Person.DisplayName, Terminal = x.RoomPaymentType.Name
            }).ToList();

            
            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            var ds = POSService.StockItemService.GetSoldItems(conn);

            var salesModel = ConvertToList(ds);

            List<SoldItemModel> simLst = new List<SoldItemModel>();

            simLst = salesModel.Where(x => x.DateSold >= startDate && x.DateSold < endDate).OrderByDescending(x => x.DateSold).ToList();

            if (!startDate.HasValue && !endDate.HasValue)
            {
                simLst = simLst.Where(x => x.DateSold.ToShortDateString().Equals(DateTime.Today.ToShortDateString(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DateSold).ToList();
            }

            var realSales = simLst.GroupBy(x => x.TimeOfSale).Select(x => new SoldItemModel { TimeOfSale = x.Key, TotalPrice = x.Sum(y => y.TotalPrice),
                PaymentTypeName = x.FirstOrDefault().PaymentTypeName, PaymentMethodName = x.FirstOrDefault().PaymentMethodName,
                PersonName = x.FirstOrDefault().PersonName, DateSold = x.FirstOrDefault().DateSold, PaymentMethodNote = x.FirstOrDefault().PaymentMethodNote, 
            }).ToList();

            neWCombinedSalesModel.AddRange(realSales.Select(x => new CombinedSalesModel
            {
                DateSold = x.DateSold,
                GuestRoom = null,
                Amount = x.TotalPrice,
                PaymentMethod = x.PaymentMethodName,
                PaymentMethodNote = x.PaymentMethodNote,
                StaffName = x.PersonName,
                Terminal = x.PaymentTypeName
            }).ToList());

            model.ModelGroupBy = neWCombinedSalesModel.GroupBy(x => x.DateSold.ToShortDateString()).Select(x => new SoldItemModel { PaymentTypeName = x.Key, TotalPrice = x.Sum(y => y.Amount), CombinedList = x.ToList() }).ToList();

            return View(model);
        }

        public ActionResult TotalExpenditure(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var expenditureList = _expenseService.GetAll().Where(x => x.ExpenseDate >= startDate && x.ExpenseDate <= endDate).ToList();

            model.ModelGroupBy = expenditureList.GroupBy(x => x.ExpenseDate.ToShortDateString()).Select(x => new SoldItemModel { PaymentTypeName = x.Key, TotalPrice = x.Sum(y => y.Amount), Expenselst = x.ToList() }).ToList();

            return View(model);
        }

        public ActionResult Sales(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var items = GetAllSoldItems();

            items = items.Where(x => x.DateSold >= startDate && x.DateSold <= endDate).ToList();

            SoldItemIndexModel siim = new SoldItemIndexModel { ItemList = items };

            return View(siim);

        }

        //private static string GetConnectionString()
        //{
        //    return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        //}


        public IEnumerable<SoldItemModel> GetAllSoldItems()
        {
            List<SoldItemModel> lst = new List<SoldItemModel>();

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("GetStockSoldItems", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    myConnection.Open();

                    //SqlParameter custId = cmd.Parameters.AddWithValue("@CustomerId", 10);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int id = dr.GetInt32(0);    // Weight int
                            decimal totalPrice = dr.GetDecimal(1);
                            decimal unitPrice = dr.GetDecimal(2);
                            int qty = dr.GetInt32(3);    // Weight int
                            DateTime datesold = dr.GetDateTime(4);  // Name string
                            string itemName = dr.GetString(5);  // Name string
                            int remainder = dr.GetInt32(6); // Breed string                                                
                            string categoryName = dr.GetString(7);    // Weight int


                            yield return new SoldItemModel
                            {
                                Id = id,
                                CategoryName = categoryName,
                                DateSold = datesold,
                                Description = itemName,
                                Quantity = qty,
                                Remainder = remainder,
                                TotalPrice = totalPrice,
                                UnitPrice = unitPrice
                            };

                        }
                    }
                }
            }

            //return lst;

        }

        public ActionResult SalesSummaryReport(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            var ds = POSService.StockItemService.GetSoldItems(conn);

            var salesModel = ConvertToList(ds);

            model.SalesModel = salesModel.Where(x => x.DateSold >= startDate && x.DateSold < endDate).OrderByDescending(x => x.DateSold).ToList();

            if (!startDate.HasValue && !endDate.HasValue)
            {
                model.SalesModel = salesModel.Where(x => x.DateSold.ToShortDateString().Equals(DateTime.Today.ToShortDateString(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DateSold).ToList();
            }

            model.ModelGroupBy = model.SalesModel.GroupBy(x => x.PaymentTypeName).Select(x => new SoldItemModel { PaymentTypeName = x.Key, ItemNewlst = x.ToList() }).ToList();

            return View(model);
        }

        //

        public ActionResult TotalSalesRoomsOnly(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            List<int> restAndBar = new List<int>();
            restAndBar.Add((int)RoomPaymentTypeEnum.Bar);
            restAndBar.Add((int)RoomPaymentTypeEnum.Laundry);
            restAndBar.Add((int)RoomPaymentTypeEnum.Restuarant);

            var allHotelPayments = _guestRoomAccountService.GetAll(HotelID).Where(x => x.PaymentMethodId != (int)PaymentMethodEnum.POSTBILL).ToList();

            var m = allHotelPayments.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate && (!restAndBar.Contains(x.PaymentTypeId))).ToList();

            var groupByGuestRoom = m.GroupBy(x => x.GuestRoom).Select(x => new AccomodationModel { ItemList = x.ToList(), GuestRoom = x.Key }).ToList();

            List<PersonAccomodationModel> pamList = new List<PersonAccomodationModel>();

            foreach (var gr in groupByGuestRoom)
            {
                var guestRoom = gr.GuestRoom;

                var gbl = gr.ItemList.GroupBy(x => x.TransactionDate.ToShortDateString()).Select(x => new PersonDateModel { DateSoldString = x.Key, ItemLst = x.ToList() }).ToList();

                foreach (var modella in gbl)
                {
                    var groupByPerson = modella.ItemLst.GroupBy(x => x.Person).Select(x => new SalesPersonModel { Person = x.Key, ItemLst = x.ToList() }).ToList();

                    foreach (var p in groupByPerson)
                    {
                        PersonAccomodationModel pamNew = new PersonAccomodationModel();

                        pamNew.GuestRoom = gr.GuestRoom;

                        pamNew.DateSold = modella.DateSoldString;

                        var totalPaidByGuest = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit).Sum(x => x.Amount);
                        var totalPaidToGuest = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit).Sum(x => x.Amount);
                        var guestTotal = totalPaidByGuest - totalPaidToGuest;

                        pamNew.Person = p.Person;
                        pamNew.TotalPaidByGuest = totalPaidByGuest;
                        pamNew.TotalPaidToGuest = totalPaidToGuest;
                        pamNew.GuestTotal = guestTotal;

                        //var pos = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).Sum(x => x.Amount);
                        //var cheque = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).Sum(x => x.Amount);
                        //var cash = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).Sum(x => x.Amount);

                        var pos = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).Sum(x => x.Amount);
                        var cheque = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).Sum(x => x.Amount);
                        var cash = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).Sum(x => x.Amount);

                        pamNew.Cash = cash;
                        pamNew.Cheque = cheque;
                        pamNew.CreditCard = pos;
                        pamNew.Terminal = p.ItemLst.GroupBy(x => x.RoomPaymentType).Select(x => new TerminalModel { Terminal = x.Key, ItemList = x.ToList() }).ToList();


                        pamList.Add(pamNew);
                    }
                }
            }

            model.ModelGroupByAccomodation = pamList.GroupBy(x => x.DateSold).Select(x => new SoldItemModelAccomodation { DateSold = x.Key, ItemLst = x.ToList(), TotalAmount = x.Sum(y => y.GuestTotal) }).ToList();

            return View(model);
        }

        
        public ActionResult EmployeeAttendance(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var listEmployee = _employeeShiftService.GetAll(HotelID).Where(x => x.ShiftDate >= startDate && x.ShiftDate <= endDate).ToList();

            var groupByList = listEmployee.GroupBy(x => x.Person).Select(x => new EmployeeGroupByModel { Person = x.Key, ItemList = x.ToList(), TotalAmount = x.Sum(y => y.TotalSales) }).ToList();

            model.EmployeeGroupByList = groupByList;

            return View(model);
        }

        public ActionResult TotalSalesBarRestaraurantOnly(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            List<int> restAndBar = new List<int>();

            restAndBar.Add((int)RoomPaymentTypeEnum.Bar);
            restAndBar.Add((int)RoomPaymentTypeEnum.Laundry);
            restAndBar.Add((int)RoomPaymentTypeEnum.Restuarant);
            
            var allHotelPayments = _guestRoomAccountService.GetAll(HotelID).Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).ToList();

            var pp = allHotelPayments.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate).ToList();

            var pp1 = pp.Select(x => x.PaymentTypeId).ToList();

            var m = allHotelPayments.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate && (restAndBar.Contains(x.PaymentTypeId))).ToList();

            var groupByGuestRoom = m.GroupBy(x => x.GuestRoom).Select(x => new AccomodationModel { ItemList = x.ToList(), GuestRoom = x.Key }).ToList();

            List<PersonAccomodationModel> pamList = new List<PersonAccomodationModel>();

            foreach (var gr in groupByGuestRoom)
            {
                var guestRoom = gr.GuestRoom;

                var gbl = gr.ItemList.GroupBy(x => x.TransactionDate.ToShortDateString()).Select(x => new PersonDateModel { DateSoldString = x.Key, ItemLst = x.ToList() }).ToList();

                foreach (var modella in gbl)
                {
                    var groupByPerson = modella.ItemLst.GroupBy(x => x.Person).Select(x => new SalesPersonModel { Person = x.Key, ItemLst = x.ToList() }).ToList();

                    foreach (var p in groupByPerson)
                    {
                        PersonAccomodationModel pamNew = new PersonAccomodationModel();

                        pamNew.GuestRoom = gr.GuestRoom;

                        pamNew.DateSold = modella.DateSoldString;

                        var totalPaidByGuest = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit).Sum(x => x.Amount);
                        var totalPaidToGuest = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit).Sum(x => x.Amount);
                        var guestTotal = totalPaidByGuest - totalPaidToGuest;

                        pamNew.Person = p.Person;
                        pamNew.TotalPaidByGuest = totalPaidByGuest;
                        pamNew.TotalPaidToGuest = totalPaidToGuest;
                        pamNew.GuestTotal = guestTotal;

                        //var pos = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).Sum(x => x.Amount);
                        //var cheque = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).Sum(x => x.Amount);
                        //var cash = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).Sum(x => x.Amount);

                        var pos = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).Sum(x => x.Amount);
                        var cheque = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).Sum(x => x.Amount);
                        var cash = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).Sum(x => x.Amount);

                        pamNew.Cash = cash;
                        pamNew.Cheque = cheque;
                        pamNew.CreditCard = pos;
                        pamNew.Terminal = p.ItemLst.GroupBy(x => x.RoomPaymentType).Select(x => new TerminalModel { Terminal = x.Key, ItemList = x.ToList() }).ToList();


                        pamList.Add(pamNew);
                    }
                }
            }

            model.ModelGroupByAccomodation = pamList.GroupBy(x => x.DateSold).Select(x => new SoldItemModelAccomodation { DateSold = x.Key, ItemLst = x.ToList(), TotalAmount = x.Sum(y => y.GuestTotal) }).ToList();

            return View(model);
        }
        public ActionResult TotalSalesRooms(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var allHotelPayments = _guestRoomAccountService.GetAll(HotelID).Where(x => x.PaymentMethodId != (int)PaymentMethodEnum.POSTBILL).ToList();

            var m = allHotelPayments.Where(x => x.TransactionDate >= startDate && x.TransactionDate <= endDate).ToList();

            var groupByGuestRoom = m.GroupBy(x => x.GuestRoom).Select(x => new AccomodationModel { ItemList = x.ToList(), GuestRoom = x.Key }).ToList();

            List<PersonAccomodationModel> pamList = new List<PersonAccomodationModel>();

            foreach(var gr in groupByGuestRoom)
            {
                var guestRoom = gr.GuestRoom;                

                var gbl = gr.ItemList.GroupBy(x => x.TransactionDate.ToShortDateString()).Select(x => new PersonDateModel { DateSoldString = x.Key, ItemLst = x.ToList() }).ToList();

                foreach(var modella in gbl)
                {
                    var groupByPerson = modella.ItemLst.GroupBy(x => x.Person).Select(x => new SalesPersonModel { Person = x.Key, ItemLst = x.ToList() }).ToList();

                    foreach(var p in groupByPerson)
                    {
                        PersonAccomodationModel pamNew = new PersonAccomodationModel();

                        pamNew.GuestRoom = gr.GuestRoom;

                        pamNew.DateSold = modella.DateSoldString;

                        var totalPaidByGuest = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit).Sum(x => x.Amount);
                        var totalPaidToGuest = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit).Sum(x => x.Amount);
                        var guestTotal = totalPaidByGuest - totalPaidToGuest;

                        pamNew.Person = p.Person;
                        pamNew.TotalPaidByGuest = totalPaidByGuest;
                        pamNew.TotalPaidToGuest = totalPaidToGuest;
                        pamNew.GuestTotal = guestTotal;

                        //var pos = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).Sum(x => x.Amount);
                        //var cheque = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).Sum(x => x.Amount);
                        //var cash = modella.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).Sum(x => x.Amount);

                        var pos = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.CreditCard).Sum(x => x.Amount);
                        var cheque = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cheque).Sum(x => x.Amount);
                        var cash = p.ItemLst.Where(x => x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Credit && x.PaymentMethodId == (int)PaymentMethodEnum.Cash).Sum(x => x.Amount);
                        
                        pamNew.Cash = cash;
                        pamNew.Cheque = cheque;
                        pamNew.CreditCard = pos;
                        pamNew.Terminal = p.ItemLst.GroupBy(x => x.RoomPaymentType).Select(x => new TerminalModel { Terminal = x.Key, ItemList = x.ToList() }).ToList();
                        

                        pamList.Add(pamNew);
                    }
                }
            }

            model.ModelGroupByAccomodation = pamList.GroupBy(x => x.DateSold).Select(x => new SoldItemModelAccomodation { DateSold = x.Key, ItemLst = x.ToList(), TotalAmount = x.Sum(y => y.GuestTotal) }).ToList();

            return View(model);
        }

        //
        public ActionResult OtherIncome(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            var ds = POSService.StockItemService.GetSoldItems(conn);

            var salesModel = ConvertToList(ds);

            model.SalesModel = salesModel.Where(x => x.DateSold >= startDate && x.DateSold < endDate && x.GuestId == 0).OrderByDescending(x => x.DateSold).ToList();

            if (!startDate.HasValue && !endDate.HasValue)
            {
                model.SalesModel = salesModel.Where(x => x.DateSold.ToShortDateString().Equals(DateTime.Today.ToShortDateString(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DateSold).ToList();
            }

            model.ModelGroupBy = model.SalesModel.GroupBy(x => x.PaymentTypeName).Select(x => new SoldItemModel { PaymentTypeName = x.Key, TotalPrice = x.Sum(y => y.TotalPrice), ItemNewlst = x.ToList() }).ToList();

            return View(model);
        }

        //

        public ActionResult TotalSalesNonGuest(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            var ds = POSService.StockItemService.GetSoldItems(conn);

            var salesModel = ConvertToList(ds);

            model.SalesModel = salesModel.Where(x => x.DateSold >= startDate && x.DateSold < endDate && !x.GuestId.HasValue).OrderByDescending(x => x.DateSold).ToList();

            if (!startDate.HasValue && !endDate.HasValue)
            {
                model.SalesModel = salesModel.Where(x => x.DateSold.ToShortDateString().Equals(DateTime.Today.ToShortDateString(), StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DateSold).ToList();
            }

            model.ModelGroupBy = model.SalesModel.GroupBy(x => x.PaymentTypeName).Select(x => new SoldItemModel { PaymentTypeName = x.Key, TotalPrice = x.Sum(y => y.TotalPrice), ItemNewlst = x.ToList() }).ToList();

            return View(model);
        }

        public ActionResult TotalSales(DateTime? startDate, DateTime? endDate)
        {
           if(startDate.HasValue && endDate.HasValue)
           {
               if(startDate.Value == endDate.Value)
               {
                   var newDate = startDate.Value.AddDays(1);
                   endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1); 
               }
           }
            
            ReportViewModel model = new ReportViewModel();

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            var ds = POSService.StockItemService.GetSoldItems(conn);

            var salesModel = ConvertToList(ds);

            model.SalesModel = salesModel.Where(x => x.DateSold >= startDate && x.DateSold < endDate).OrderByDescending(x => x.DateSold).ToList();

            if(!startDate.HasValue && !endDate.HasValue)
            {
                model.SalesModel = salesModel.Where(x => x.DateSold.ToShortDateString().Equals(DateTime.Today.ToShortDateString(),StringComparison.InvariantCultureIgnoreCase)).OrderByDescending(x => x.DateSold).ToList();
            }

            model.ModelGroupBy = model.SalesModel.GroupBy(x => x.PaymentTypeName).Select(x => new SoldItemModel { PaymentTypeName = x.Key, TotalPrice = x.Sum(y => y.TotalPrice), ItemNewlst = x.ToList()}).ToList();
            
            return View(model);
        }

        private List<SoldItemModel> ConvertToList(System.Data.DataSet ds)
        {
            List<SoldItemModel> lst = new List<SoldItemModel>();

            int count = ds.Tables[0].Rows.Count;

            for (int i = 0; i < count; i++)
            {
                SoldItemModel sim = new SoldItemModel();
                sim.Description = ds.Tables[0].Rows[i][0].ToString();
                sim.Quantity = int.Parse(ds.Tables[0].Rows[i][1].ToString());
                sim.TotalPrice = decimal.Parse(ds.Tables[0].Rows[i][2].ToString());
                sim.DateSold = DateTime.Parse(ds.Tables[0].Rows[i][3].ToString());
                sim.PersonName = ds.Tables[0].Rows[i][4].ToString();
                sim.PaymentTypeName = ds.Tables[0].Rows[i][5].ToString();
                sim.PaymentMethodNote = ds.Tables[0].Rows[i][6].ToString();
                sim.PaymentMethodName = ds.Tables[0].Rows[i][7].ToString();
                sim.TimeOfSale = DateTime.Parse(ds.Tables[0].Rows[i][8].ToString());
                try
                {
                    sim.GuestId = int.Parse(ds.Tables[0].Rows[i][9].ToString());
                }
                catch
                {
                    sim.GuestId = null;
                }
                lst.Add(sim);
            }

            return lst;
        }

        public ActionResult GuestDetails(DateTime? startDate, DateTime? endDate)
        {
            ReportViewModel model = new ReportViewModel();

            if (startDate.HasValue && endDate.HasValue)
            {
                if (startDate.Value == endDate.Value)
                {
                    var newDate = startDate.Value.AddDays(1);
                    endDate = new DateTime(newDate.Year, newDate.Month, newDate.Day, 0, 0, 1);
                }
            }

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);

            if (!endDate.HasValue)
                endDate = DateTime.Now.AddMonths(1);


            model.HotelGuests = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && !x.IsFutureReservation && x.GuestRooms.Where(y => y.CheckinDate > startDate.Value && y.IsActive && y.GroupBooking).Count() > 0).OrderByDescending(x => x.CreatedDate).ToList();

            return View(model);
        }

        

        
        
	}
}