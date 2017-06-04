using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers;
using HotelMateWebV1.Models;
using ReportManagement;
using Microsoft.PointOfService;
using System.Collections;
using System.Configuration;
using HotelMateWebV1.Helpers.Enums;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace HotelMateWebV1.Controllers
{
    [Authorize()]
    [HandleError(View = "CustomErrorView")]
    public class PrintingController : PdfViewController
    {
        private readonly IGuestRoomAccountService _guestRoomAccountService;
        private readonly IGuestService _guestService;
        private readonly IRoomService _roomService;

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

        public void PrintToPos()
        {
            PosExplorer explorer = null;
            DeviceInfo _device;
            PosPrinter _oposPrinter;
            string LDN = "";

            explorer = new PosExplorer();
            _device = explorer.GetDevice(DeviceType.PosPrinter, LDN);
            _oposPrinter = (PosPrinter)explorer.CreateInstance(_device);
            _oposPrinter.Open();
            _oposPrinter.Claim(10000);
            _oposPrinter.DeviceEnabled = true;
            // normal print
            //_oposPrinter.PrintNormal(PrinterStation.Receipt, "yourprintdata"); 
            // pulse the cash drawer pin  pulseLength-> 1 = 100ms, 2 = 200ms, pin-> 0 = pin2, 1 = pin5
            // _oposPrinter.PrintNormal(PrinterStation.Receipt, (char)16 + (char)20 + (char)1 + (char)pin + (char)pulseLength); 

             // cut the paper
             //_oposPrinter.PrintNormal(PrinterStation.Receipt, (char)29 + (char)86 + (char)66);

             // print stored bitmap
             //_oposPrinter.PrintNormal(PrinterStation.Receipt, (char)29 + (char)47 + (char)0);
        }

       


        


        public PrintingController()
        {
            _personService = new PersonService();
           _guestRoomAccountService = new GuestRoomAccountService();
           _guestService = new GuestService();
            _roomService = new RoomService();
        }


       
        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult PrintGuestPayment(int? id, bool checkout = false)
        {
            var account = _guestRoomAccountService.GetById(id.Value);

            var guest = _guestService.GetById(account.GuestRoom.GuestId);

            var allItemisedItems = guest.SoldItemsAlls.Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).OrderByDescending(x => x.DateSold).ToList();

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                RoomId = 0,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = account,
                ItemmisedItems = allItemisedItems
            };

            //gravm = PopulateModel(gravm, guest, checkout,false,true);
            gravm = PopulateModel(gravm, guest, false, true);
            gravm.GuestRoomAccount = account;

            string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            gravm.ImageUrl = url + "images/" + "LakehouseLogoNEW4.png";
            return this.ViewPdf("Guest  Receipt", "_GuestBillReceipt", gravm);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult PrintGuestAccount(int? id, bool checkout = false, bool? email = false)
        {
            var guest = _guestService.GetById(id.Value);

            var allItemisedItems = guest.SoldItemsAlls.Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).OrderByDescending(x => x.DateSold).ToList();

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                RoomId = 0,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                ItemmisedItems = allItemisedItems                
            };

            gravm = PopulateModel(gravm, guest, checkout, false);

            gravm.DisplayList = GetDisplayList(gravm);

            string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));

            gravm.ImageUrl = url + "images/" + "LakehouseLogoNEW4.png";

            var path1 = Path.Combine(Server.MapPath("~/Products/Receipt/"));

            gravm.ImageUrl = Path.Combine(Server.MapPath("~/images/Receipt"), "ReceiptLogo.jpg");

            var filename = PDFReceiptPrinter.PrintInvoiceCheckout(path1, gravm, gravm.ImageUrl);

            var path = Path.Combine(Server.MapPath("~/Products/Receipt/"), filename + ".pdf");

            if(email.HasValue && email.Value)
            {
                EmailAttachmentToGuest(guest, path);
            }

            var fileNameNew = filename + "_" + "Receipt.pdf";

            return File(path, "application/ms-excel", fileNameNew);
        }

        private void EmailAttachmentToGuest(Guest guestCreated, string path, string strName = "Checkout Receipt")
        {


            var emailTemplate = @"<p style='margin:0px;padding:0px;font-size:12px;font-family:Arial, Helvetica, sans-serif;color:#555;' id='yui_3_16_0_ym19_1_1463261898755_4224'>Warm Greetings #FULLNAME#,<br>
                                <br>This is to kindly inform you of your receipt, the details are listed in the attachment : <br>
                                <br>Please see attached file for your statement<br><br>
                                </p>";


            emailTemplate = emailTemplate.Replace("#FULLNAME#", guestCreated.FullName);


            try
            {

                var dest = guestCreated.Email;

                var emails = dest.Split(',').ToList();

                foreach (var email in emails)
                {

                    MailMessage mail = new MailMessage("academyvistang@gmail.com", email, "Your Receipt", emailTemplate);
                    mail.From = new MailAddress("academyvistang@gmail.com", strName);
                    mail.IsBodyHtml = true; // necessary if you're using html email
                    NetworkCredential credential = new NetworkCredential("academyvistang@gmail.com", "Lauren280701");
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = credential;
                    if (path != null)
                        mail.Attachments.Add(new Attachment(path));
                    smtp.Send(mail);
                }
            }
            catch
            {

            }
        }

        private List<CheckOutDisplayModel> GetDisplayList(GuestRoomAccountViewModel gravm)
        {
            var list = new List<CheckOutDisplayModel>();

            if(gravm.Guest.CompanyId.HasValue)
            {
                foreach (var acc in gravm.Guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).Where(x => (x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit
                        || x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit
                        || x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit) || (x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL)))
                {
                    
                    CheckOutDisplayModel cdm = new CheckOutDisplayModel();
                    cdm.TransactionDate = acc.TransactionDate;
                    cdm.Detail = !string.IsNullOrEmpty(acc.PaymentMethodNote) ? acc.PaymentMethodNote : acc.RoomPaymentType.Name;
                    cdm.Status = acc.RoomPaymentType.PaymentStatusId;
                    cdm.Amount = acc.Amount;
                    cdm.Type = acc.PaymentMethod.Name;
                    list.Add(cdm);
                }
            }
            else
            {
                //foreach (var acc in gravm.Guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts))
                foreach (var acc in gravm.Guest.GuestRooms.SelectMany(x => x.GuestRoomAccounts).Where(x => (x.PaymentTypeId == (int)RoomPaymentTypeEnum.CashDeposit
                         || x.PaymentTypeId == (int)RoomPaymentTypeEnum.InitialDeposit
                         || x.PaymentTypeId == (int)RoomPaymentTypeEnum.ReservationDeposit) || (x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL)))
                {
                    if(acc.Amount > 0)
                    {
                        CheckOutDisplayModel cdm = new CheckOutDisplayModel();
                        cdm.TransactionDate = acc.TransactionDate;
                        cdm.Detail = !string.IsNullOrEmpty(acc.PaymentMethodNote) ? acc.PaymentMethodNote : acc.RoomPaymentType.Name;
                        cdm.Status = acc.RoomPaymentType.PaymentStatusId;
                        cdm.Amount = acc.Amount;
                        cdm.Type = acc.PaymentMethod.Name;
                        list.Add(cdm);
                    }
                   
                }
            }

            var cdm1 = new CheckOutDisplayModel();
            cdm1.TransactionDate = DateTime.Now;
            cdm1.Detail = gravm.NoOfNight.ToString() + " Night(s)";
            cdm1.Status = (int)RoomPaymentStatusEnum.Debit;
            cdm1.Amount = gravm.Guest.GuestRooms.Summation(true);
            cdm1.Type = "---";
            list.Add(cdm1);

            var tax = gravm.Guest.Payments.Where(x => x.Type == 2).Sum(x => x.TaxAmount);

            string taxName = "TAX";

            if(tax > 0)
            {
                taxName = gravm.Guest.Payments.Where(x => x.Type == 2).LastOrDefault().Tax;
            }

            if(string.IsNullOrEmpty(taxName))
            {
                taxName = "TAX";
            }

            if (tax > 0)
            {
                var cdmtax = new CheckOutDisplayModel();
                cdmtax.TransactionDate = DateTime.Now;
                cdmtax.Detail = "TAX -- " + taxName;
                cdmtax.Status = (int)RoomPaymentStatusEnum.Debit;
                cdmtax.Amount = tax;
                cdmtax.Type = "---";
                list.Add(cdmtax);
            }

            var discount = gravm.Guest.Payments.Where(x => x.Type == 2).Sum(x => x.DiscountAmount);

            string discountName = "DISCOUNT";

            if (discount > 0)
            {
                discountName = gravm.Guest.Payments.Where(x => x.Type == 2).LastOrDefault().Discount;
            }

            if (string.IsNullOrEmpty(discountName))
            {
                taxName = "DISCOUNT";
            }

            if (discount > 0)
            {
                var cdmDiscount = new CheckOutDisplayModel();
                cdmDiscount.TransactionDate = DateTime.Now;
                cdmDiscount.Detail = "Discount -- " + discountName;
                cdmDiscount.Status = (int)RoomPaymentStatusEnum.Credit;
                cdmDiscount.Amount = discount;
                cdmDiscount.Type = "---";
                list.Add(cdmDiscount);
            }

            var returnlist = new List<CheckOutDisplayModel>();

            var lastBalance = decimal.Zero;

            foreach(var l in list.OrderBy(x => x.TransactionDate))
            {
                var cdmr = new CheckOutDisplayModel();
                cdmr.TransactionDate = l.TransactionDate;
                cdmr.Detail = l.Detail;

                if (l.Status == (int)RoomPaymentStatusEnum.Debit)
                {
                    cdmr.Debit = l.Amount;
                    cdmr.Credit = decimal.Zero;
                    lastBalance = lastBalance - l.Amount;
                }
                else
                {
                    cdmr.Credit = l.Amount;
                    cdmr.Debit = decimal.Zero;
                    lastBalance = lastBalance + l.Amount;
                }
                
                cdmr.Type = l.Type;
                cdmr.Balance = lastBalance;

                returnlist.Add(cdmr);
            }


            return returnlist.OrderByDescending(x => x.TransactionDate).ToList();
        }

        private string GetTaxName()
        {
            var tac = string.Empty;

            try
            {
                tac = ConfigurationManager.AppSettings["TaxName"].ToString();
            }
            catch
            {
                tac = "";
            }

            return tac;
        }

        private List<string> GetTAC()
        {
            var tac = string.Empty;

            try
            {
                tac = ConfigurationManager.AppSettings["TermsAndConditions"].ToString();
            }
            catch
            {
                tac = "";
            }

            if(string.IsNullOrEmpty(tac))
            {
                return new List<string>();
            }
            else
            {
                var splitter = tac.Split('@');

                if(splitter.Length > 0)
                    return splitter.ToList();
                
            }

            return new List<string>();
        }
        

        private List<string> GetAcknowledge()
        {
            var tac = string.Empty;

            try
            {
                tac = ConfigurationManager.AppSettings["Acknowledge"].ToString();
            }
            catch
            {
                tac = "";
            }

            if (string.IsNullOrEmpty(tac))
            {
                return new List<string>();
            }
            else
            {
                var splitter = tac.Split('@');

                if (splitter.Length > 0)
                    return splitter.ToList();

            }

            return new List<string>();
        }

        
        [HttpGet]
        public ActionResult PrintGuestAccountCheckInFuture(int? id)
        {
            var guest = _guestService.GetById(id.Value);


            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                RoomId = 0,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            gravm = PopulateModelFuture(gravm, guest, false, true);

            gravm.TermsAndConditions = GetTAC();

            gravm.Acknowledge = GetAcknowledge();


            string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            gravm.ImageUrl = url + "images/" + "LakehouseLogoNEW4.png";
            return this.ViewPdf("Guest CheckIn Reciept", "_GuestBillPrinterCheckin", gravm);
        }

        [HttpGet]
        public ActionResult PrintGuestAccountCheckIn(int? id)
        {
            var guest = _guestService.GetById(id.Value);
            

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest,
                RoomId = 0,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            gravm = PopulateModel(gravm, guest,false,true);

            gravm.TermsAndConditions = GetTAC();

            gravm.Acknowledge = GetAcknowledge();


            string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
            gravm.ImageUrl = url + "images/" + "LakehouseLogoNEW4.png";
            return this.ViewPdf("Guest CheckIn Reciept", "_GuestBillPrinterCheckin", gravm);
        }

        private GuestRoomAccountViewModel PopulateModel(GuestRoomAccountViewModel gravm, Guest guest, bool checkout = false, bool checking = false)
        {
            var ticks = (int)DateTime.Now.Ticks;

            var transactionId = ticks < 0 ? ticks*(-1) : ticks;

            var mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && x.IsActive) ?? guest.GuestRooms.FirstOrDefault(x => x.IsActive);

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && !x.IsActive) ?? guest.GuestRooms.FirstOrDefault(x => !x.IsActive);
            }

            if (null == mainGuestRoom) return gravm;

            gravm.GuestRoomNumber = guest.GuestRooms.Select(x => x.Room.RoomNumber).ToDelimitedString(",");
            gravm.GuestName = guest.FullName;
            gravm.ArrivalDate = mainGuestRoom.CheckinDate.ToShortDateString();
            gravm.DepartureDate = mainGuestRoom.CheckoutDate.ToShortDateString();

            if (mainGuestRoom.CheckoutDate < DateTime.Today)
                gravm.DepartureDate = DateTime.Now.ToString();

            gravm.NoOfNight = mainGuestRoom.GetNumberOfNights().ToString(CultureInfo.InvariantCulture);

            if(checking)
                gravm.NoOfNight = mainGuestRoom.GetNumberOfNightsCheckin().ToString(CultureInfo.InvariantCulture);


            if (!guest.IsActive)
            {
                gravm.NoOfNight = mainGuestRoom.GetNumberOfNightsFutureBooking().ToString(CultureInfo.InvariantCulture);
                gravm.DepartureDate = mainGuestRoom.CheckoutDate.ToShortDateString();

                if (mainGuestRoom.CheckoutDate < DateTime.Today)
                    gravm.DepartureDate = DateTime.Now.ToString();
            }

            var discount = (mainGuestRoom.Room.Price - mainGuestRoom.RoomRate) * mainGuestRoom.GetNumberOfNights();

            if (mainGuestRoom.RoomRate > mainGuestRoom.Room.Price)
            {
                discount = 0;
            }

            gravm.NoOfPersons = mainGuestRoom.Occupants.ToString(CultureInfo.InvariantCulture);
            gravm.Currency = "NGN";

            if (discount.HasValue && discount.Value < 0)
            {
                discount = decimal.Negate(discount.Value);
            }

            gravm.Discounts = discount.ToString();

           

            gravm.RoomRate = mainGuestRoom.RoomRate.ToString(CultureInfo.InvariantCulture);
            gravm.BillNo = transactionId.ToString(CultureInfo.InvariantCulture);

            return gravm;
        }


        private GuestRoomAccountViewModel PopulateModelFuture(GuestRoomAccountViewModel gravm, Guest guest, bool checkout = false, bool checking = false)
        {
            var ticks = (int)DateTime.Now.Ticks;

            var transactionId = ticks < 0 ? ticks * (-1) : ticks;

            var mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && x.IsActive) ?? guest.GuestRooms.FirstOrDefault(x => x.IsActive);

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom && !x.IsActive) ?? guest.GuestRooms.FirstOrDefault(x => !x.IsActive);
            }

            if (null == mainGuestRoom) return gravm;

            gravm.GuestRoomNumber = guest.GuestRooms.Select(x => x.Room.RoomNumber).ToDelimitedString(",");
            gravm.GuestName = guest.FullName;
            gravm.ArrivalDate = mainGuestRoom.CheckinDate.ToShortDateString();
            gravm.DepartureDate = mainGuestRoom.CheckoutDate.ToShortDateString();

            if (mainGuestRoom.CheckoutDate < DateTime.Today)
                gravm.DepartureDate = DateTime.Now.ToString();
            
            

            if (!guest.IsActive)
            {
                gravm.NoOfNight = mainGuestRoom.GetNumberOfNightsFutureBooking().ToString(CultureInfo.InvariantCulture);
                gravm.DepartureDate = mainGuestRoom.CheckoutDate.ToShortDateString();

                if (mainGuestRoom.CheckoutDate < DateTime.Today)
                    gravm.DepartureDate = DateTime.Now.ToString();
            }

            gravm.NoOfNight = mainGuestRoom.GetNumberOfNightsFuture().ToString(CultureInfo.InvariantCulture);

            var discount = (mainGuestRoom.Room.Price - mainGuestRoom.RoomRate) * mainGuestRoom.GetNumberOfNights();

            if (mainGuestRoom.RoomRate > mainGuestRoom.Room.Price)
            {
                discount = 0;
            }

            gravm.NoOfPersons = mainGuestRoom.Occupants.ToString(CultureInfo.InvariantCulture);
            gravm.Currency = "NGN";

            if (discount.HasValue && discount.Value < 0)
            {
                discount = decimal.Negate(discount.Value);
            }

            gravm.Discounts = discount.ToString();



            gravm.RoomRate = mainGuestRoom.RoomRate.ToString(CultureInfo.InvariantCulture);
            gravm.BillNo = transactionId.ToString(CultureInfo.InvariantCulture);

            return gravm;
        }

        //public ActionResult ShowCapitalisationPdf(int? id)
        //{
        //    //EventViewData evd = new EventViewData();
        //    //evd.ProjectReports = projectReportRepository.Index().ToList();
        //    //evd.PassTemplateList = GetCapitalisationDataForPdf(11);
        //    PrintingViewModel pvm
        //    string url = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
        //    evd.ImageUrl = url + "images/" + "custom-reports.png";

        //    return this.ViewPdf("Capitalisation  report", "ReportsViewerCapitalisation", evd);
        //}
	}
}