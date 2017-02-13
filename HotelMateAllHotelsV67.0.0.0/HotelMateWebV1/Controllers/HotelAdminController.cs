using System;
using System.Collections.Generic;
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
using Microsoft.AspNet.Identity;

namespace HotelMateWebV1.Controllers
{
    [Authorize()]
    [HandleError(View = "CustomErrorView")]
    public class HotelAdminController : Controller
    {
         private readonly IRoomService _roomService;
         private readonly IRoomTypeService _roomTypeService;
         private readonly IRoomStatuService _roomStatusService;
         private readonly IGuestService _guestService;
         private readonly IGuestRoomService _guestRoomService;
         private readonly IGuestReservationService _guestReservationService;
         private readonly IBusinessAccountService _businessAccountService;
         private readonly ITransactionService _transactionService;
         private readonly IGuestRoomAccountService _guestRoomAccountService;

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
             User.Identity.GetUserName();
             var user = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
             return user.HotelId;
         }


        public HotelAdminController()
        {
            _personService = new PersonService();
            _roomService = new RoomService();
            _roomTypeService = new RoomTypeService();
            _guestService = new GuestService();
            _guestRoomService = new GuestRoomService();
            _businessAccountService = new BusinessAccountService();
            _guestReservationService = new GuestReservationService();
            _transactionService = new TransactionService();
            _guestRoomAccountService = new GuestRoomAccountService();
            _roomStatusService = new RoomStatuService();
        }

        public ActionResult GetGraph()
        {
            var lst = new List<string []>();
            int daysInThisMonth = DateTime.Now.Day;
            var startOfMonth = new DateTime(DateTime.Now.Year,DateTime.Now.Month,1);
            var guestSales = _guestRoomService.GetAll(HotelID).Where(x => x.CheckinDate.IsBetween(startOfMonth, DateTime.Now));

            for(int i = 1; i <= daysInThisMonth; i++)
            {
               lst.Add(GetTodaysSales(i,guestSales));
            }
            

            return Json(lst.ToArray(),JsonRequestBehavior.AllowGet);
        }

        private string[] GetTodaysSales(int i, IEnumerable<GuestRoom> guestSales)
        {
            var dateNow = new DateTime(DateTime.Now.Year,DateTime.Now.Month,i);
            var sales = guestSales.Where(x => x.CheckinDate.Date == dateNow.Date).CreditSummation();
            return new[] { i.ToString(),sales.ToString() };
        }

        public ActionResult CategoryCreate()
        {
            //var gr = _guestRoomService.GetAll(HotelID).ToList();
            return View(new AdminViewModel {  });
        }

        //
        public ActionResult Marketing()
        {            

            var adminViewModel = new AdminViewModel();
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var guestSales = _guestRoomService.GetAll(HotelID).Where(x => x.CheckinDate.IsBetween(startOfMonth, DateTime.Now));
            var creditSales = guestSales.CreditSummation();
            var debitSales = guestSales.DebitSummation();
            adminViewModel.ProfitSales = creditSales - debitSales;
            adminViewModel.MonthlyCreditSales = creditSales;
            adminViewModel.MonthlyDebitSales = debitSales;
            adminViewModel.NumberOfGuests = _guestService.GetAll(HotelID).Count(x => x.IsActive);
            adminViewModel.LongTermStay = guestSales.Count(x => x.GetNumberOfNights() > 7);
            adminViewModel.ShortTermStay = guestSales.Count(x => x.GetNumberOfNights() < 7);
            adminViewModel.ReservationCount = _guestReservationService.GetAll(HotelID).Count(x => x.StartDate > startOfMonth);
            adminViewModel.GuestSales = guestSales;
            var totalSales = guestSales.Sum(guestRoom => guestRoom.GuestRoomAccounts.Summation());
            var everything = guestSales.Sum(guestRoom => guestRoom.GuestRoomAccounts.Where(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.Laundry
                || x.PaymentTypeId == (int)RoomPaymentTypeEnum.Restuarant).Summation());
            adminViewModel.RoomOnlySales = totalSales - everything;

            return View(adminViewModel);
        }

        public ActionResult Index()
        {
           // return RedirectToAction("CategoryCreate");

            var adminViewModel = new AdminViewModel();
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var guestSales = _guestRoomService.GetAll(HotelID).Where(x => x.CheckinDate.IsBetween(startOfMonth, DateTime.Now));
            var creditSales = guestSales.CreditSummation();
            var debitSales = guestSales.DebitSummation();
            adminViewModel.ProfitSales = creditSales - debitSales;
            adminViewModel.MonthlyCreditSales = creditSales;
            adminViewModel.MonthlyDebitSales = debitSales;
            adminViewModel.NumberOfGuests = _guestService.GetAll(HotelID).Count(x => x.IsActive);
            adminViewModel.LongTermStay = guestSales.Count(x => x.GetNumberOfNights() > 7);
            adminViewModel.ShortTermStay = guestSales.Count(x => x.GetNumberOfNights() < 7);
            adminViewModel.ReservationCount = _guestReservationService.GetAll(HotelID).Count(x => x.StartDate > startOfMonth);
            adminViewModel.GuestSales = guestSales;
            var totalSales = guestSales.Sum(guestRoom => guestRoom.GuestRoomAccounts.Summation());
            var everything = guestSales.Sum(guestRoom => guestRoom.GuestRoomAccounts.Where(x => x.PaymentTypeId == (int)RoomPaymentTypeEnum.Laundry
                || x.PaymentTypeId == (int)RoomPaymentTypeEnum.Restuarant).Summation());
            adminViewModel.RoomOnlySales = totalSales - everything;



            return View(adminViewModel);
        }

        public ActionResult Tables()
        {
            var gr = _guestRoomService.GetAll(HotelID).ToList();
            return View(new AdminViewModel { GuestRooms = gr });            
        }




    

        //[HttpGet]
        //public ActionResult EditExpenses(int? id)
        //{
        //    var expenses = _expensesService.GetById(id.Value);
        //    var model = GetModelForNewExpenses(expenses);
        //    model.ExpensesTypeList = GetExpensesTypes(expenses.ExpensesType);
        //    return View(model);
        //}

        //[HttpGet]
        //public ActionResult NewExpenses()
        //{
        //    var model = GetModelForNewExpenses(null);
        //    model.ExpensesTypeList = GetExpensesTypes(null);
        //    return View("NewExpenses", model);
        //}

        //[HttpPost]
        //public ActionResult NewExpenses(ExpensesViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        Mapper.CreateMap<GuestViewModel, Guest>();
        //        var expenses = Mapper.Map<ExpensesViewModel, Expenses>(model);
        //        expenses.IsActive = true;

        //        if (expenses.Id > 0)
        //        {
        //            var existingExpenses = _expensesService.GetById(expenses.Id);
        //            existingExpenses.ExtNumber = expenses.ExtNumber;
        //            existingExpenses.NoOfBeds = expenses.NoOfBeds;
        //            existingExpenses.Notes = expenses.Notes;
        //            existingExpenses.Price = expenses.Price;
        //            existingExpenses.ExpensesNumber = expenses.ExpensesNumber;
        //            existingExpenses.ExpensesType = expenses.ExpensesType;
        //            _expensesService.Update(existingExpenses);
        //        }
        //        else
        //        {
        //            _expensesService.Create(expenses);
        //        }

        //        return RedirectToAction("Booking", "Home");
        //    }

        //    model.ExpensesTypeList = GetExpensesTypes(model.ExpensesType);
        //    return View(model);
        //}

        //private IEnumerable<SelectListItem> GetExpensesTypes(int? selectedId)
        //{
        //    if (!selectedId.HasValue)
        //        selectedId = 0;

        //    var bas =
        //        _expensesTypeService.GetAll(HotelID).ToList();
        //    bas.Insert(0, new ExpensesType { Name = "-- Please Select --", Id = 0 });
        //    return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        //}

        //private ExpensesViewModel GetModelForNewExpenses(Expenses expenses)
        //{
        //    Mapper.CreateMap<Expenses, ExpensesViewModel>();
        //    expenses = expenses ?? new Expenses { IsActive = true };
        //    var rvm = Mapper.Map<Expenses, ExpensesViewModel>(expenses);
        //    return rvm;
        //}

       //Expenses

        [HttpGet]
        public ActionResult EditCreditAccount(int? id)
        {
            var creditAccount = _businessAccountService.GetById(id.Value);
            var model = GetModelForNewCreditAccount(creditAccount);
            return View("NewCreditAccount", model);
        }

        [HttpPost]
        public ActionResult NewCreditAccount(CreditAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                Mapper.CreateMap<CreditAccountViewModel, BusinessAccount>();
                var creditAccount = Mapper.Map<CreditAccountViewModel, BusinessAccount>(model);
                creditAccount.Status = "LIVE";

                if (creditAccount.Id > 0)
                {
                    var existingAccount = _businessAccountService.GetById(creditAccount.Id);
                    existingAccount.Address = creditAccount.Address;
                    existingAccount.CompanyAddress = creditAccount.CompanyAddress;
                    existingAccount.CompanyName = creditAccount.CompanyName;
                    existingAccount.CompanyTelephone = creditAccount.CompanyTelephone;
                    existingAccount.ContactName = creditAccount.ContactName;
                    existingAccount.ContactNumber = creditAccount.ContactNumber;
                    existingAccount.Email = creditAccount.Email;
                    existingAccount.Mobile = creditAccount.Mobile;
                    existingAccount.Name = creditAccount.Name;
                    existingAccount.NatureOfBusiness = creditAccount.NatureOfBusiness;
                    existingAccount.Telephone = creditAccount.Telephone;
                    _businessAccountService.Update(existingAccount);
                }
                else
                {
                    creditAccount.Debtor = false;
                    _businessAccountService.Create(creditAccount);
                }

                return RedirectToAction("Booking", "Home");
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult NewCreditAccount()
        {
            var model = GetModelForNewCreditAccount(null);
            return View("NewCreditAccount", model);
        }

        private CreditAccountViewModel GetModelForNewCreditAccount(BusinessAccount creditAccount)
        {
            Mapper.CreateMap<BusinessAccount, CreditAccountViewModel>();
            creditAccount = creditAccount ?? new BusinessAccount {Status = "LIVE" };
            var rvm = Mapper.Map<BusinessAccount, CreditAccountViewModel>(creditAccount);
            return rvm;
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

        [HttpGet]
        public ActionResult EditRoomChangeStatus(int? id)
        {
            var room = _roomService.GetById(id.Value);


            var activeGuestRooms = room.GuestRooms.Where(x => x.IsActive).ToList().Count > 0;

            int activeReservations = room.GuestReservations.Where(x => x.IsActive).ToList().Count();


            var model = GetModelForNewRoom(room);

            model.RoomTypeList = GetRoomTypes(room.RoomType);

            if (activeGuestRooms)
            {
                model.RoomStatusList = GetRoomStatusOriginal(room.StatusId);
            }
            else
            {
                model.RoomStatusList = GetRoomStatusVacantOnly(room.StatusId);
            }
            
            return View("NewRoomChangeStatus", model);
        }

        //

        [HttpGet]
        public ActionResult NewRoom()
        {
            var model = GetModelForNewRoom(null);
            model.RoomTypeList = GetRoomTypes(null);
            model.RoomStatusList = GetRoomStatus(null);
            return View("NewRoom", model);
        }

        [HttpPost]
        public ActionResult NewRoom(RoomViewModel model)
        {
            if (ModelState.IsValid)
            {
                Mapper.CreateMap<RoomViewModel, Room>();
                var room = Mapper.Map<RoomViewModel, Room>(model);
                room.IsActive = true;

                if (room.Id > 0)
                {
                    var existingRoom = _roomService.GetById(room.Id);
                    existingRoom.ExtNumber = room.ExtNumber;
                    existingRoom.NoOfBeds = room.NoOfBeds;
                    existingRoom.Notes = room.Notes;
                    existingRoom.Price = room.Price;
                    existingRoom.RoomNumber = room.RoomNumber;
                    existingRoom.RoomType = room.RoomType;
                    existingRoom.StatusId = room.StatusId;
                    _roomService.Update(existingRoom);
                }
                else
                {
                    _roomService.Create(room);
                }

                return RedirectToAction("Booking", "Home");
            }

            model.RoomTypeList = GetRoomTypes(model.RoomType);
            model.RoomStatusList = GetRoomStatus(model.StatusId);

            return View(model);
        }

        private IEnumerable<SelectListItem> GetRoomTypes(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomTypeService.GetAll(HotelID).ToList();
            bas.Insert(0, new RoomType { Name = "-- Please Select --", Id = 0 });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }

        private IEnumerable<SelectListItem> GetRoomStatusOriginal(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomStatusService.GetAll(HotelID).Where(x => x.Id == selectedId.Value).ToList();
            //bas.Insert(0, new RoomStatu { Name = "-- Please Select --", Id = 0 });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }
        

        private IEnumerable<SelectListItem> GetRoomStatusVacantOnly(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomStatusService.GetAll(HotelID).Where(x => x.Id == (int)RoomStatusEnum.Vacant).ToList();
            //bas.Insert(0, new RoomStatu { Name = "-- Please Select --", Id = 0 });
            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }

        private IEnumerable<SelectListItem> GetRoomStatus(int? selectedId)
        {
            if (!selectedId.HasValue)
                selectedId = 0;

            var bas =
                _roomStatusService.GetAll(HotelID).Where(x => x.Id != (int)RoomStatusEnum.Occupied).ToList();

            bas.Insert(0, new RoomStatu { Name = "-- Please Select --", Id = 0 });

            return bas.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(CultureInfo.InvariantCulture), Selected = x.Id == selectedId });
        }

        private RoomViewModel GetModelForNewRoom(Room room)
        {
            Mapper.CreateMap<Room, RoomViewModel>();
            room = room ?? new Room{IsActive = true};
            var rvm = Mapper.Map<Room, RoomViewModel>(room);
            return rvm;
        }

      

        [HttpGet]
        public ActionResult GuestCheckinReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddMonths(1);

            const int pageSize = 1;
            var entirelist = _guestRoomService.GetByQuery(HotelID).Where(x => x.CheckinDate > startDate.Value && x.CheckinDate < endDate.Value && x.IsActive && x.Room.RoomStatu.Id == (int) RoomStatusEnum.Occupied);
            var paginatedList = new PaginatedList<GuestRoom>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
                {
                    PaginatedGuestRoomList = paginatedList
                };

            return View(avm);
        }

        [HttpGet]
        public ActionResult GuestCheckoutReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddMonths(1);

            const int pageSize = 1;
            var entirelist = _guestRoomService.GetByQuery(HotelID).Where(x => x.CheckoutDate > startDate.Value && x.CheckoutDate < endDate.Value && !x.Guest.IsActive);
            var paginatedList = new PaginatedList<GuestRoom>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult DueReservationReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddDays(-1);
            endDate = endDate ?? DateTime.Now.AddDays(7);

            const int pageSize = 1;
            var entirelist = _guestReservationService.GetByQuery(HotelID).Where(x => x.StartDate > startDate.Value && x.StartDate < endDate.Value && x.IsActive);
            var paginatedList = new PaginatedList<GuestReservation>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestReservationList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult GuestFolioReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddMonths(1);

            const int pageSize = 1;
            var entirelist = _guestService.GetByQuery(HotelID).Where(x => x.IsActive);
            var paginatedList = new PaginatedList<Guest>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult GuestListReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddMonths(1);

            const int pageSize = 1;
            var entirelist = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && x.CreatedDate.ReportIsBetween(startDate.Value,endDate.Value));
            var paginatedList = new PaginatedList<Guest>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult ReservationListReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestReservationService.GetByQuery(HotelID).Where(x => x.StartDate > startDate.Value && x.StartDate < endDate.Value && x.IsActive);
            var paginatedList = new PaginatedList<GuestReservation>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestReservationList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult RoomHistoryReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomService.GetByQuery(HotelID).Where(x => x.CheckinDate.IsBetween(startDate.Value, endDate.Value));
            var paginatedList = new PaginatedList<GuestRoom>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult RoomHistoryReportSummary(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomService.GetByQuery(HotelID).Where(x => x.CheckinDate.IsBetween(startDate.Value, endDate.Value));
            var paginatedList = new PaginatedList<GuestRoom>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult GroupReservationReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomService.GetByQuery(HotelID).Where(x => x.CheckinDate.IsBetween(startDate.Value, endDate.Value) && x.GroupBooking);
            var paginatedList = new PaginatedList<GuestRoom>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomList = paginatedList
            };

            return View(avm);
        }

         
        [HttpGet]
        public ActionResult OtherChargesReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomService.GetByQuery(HotelID).Where(x => x.CheckinDate.ReportIsBetween(startDate.Value, endDate.Value) && x.GroupBooking);
            var paginatedList = new PaginatedList<GuestRoom>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult AccountReceivableReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.PaymentStatusId == (int) RoomPaymentStatusEnum.Credit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult AccountPayableReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult TaxesReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult CreditGuestReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult CompanyCoporateGuestReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestService.GetByQuery(HotelID).Where(x => x.CreatedDate.ReportIsBetween(startDate.Value, endDate.Value) && x.CompanyId > 0);
            var paginatedList = new PaginatedList<Guest>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult ProfitReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.PaymentStatusId == (int)RoomPaymentStatusEnum.Debit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        //Create an expenses table so that profit can be calculated

        [HttpGet]
        public ActionResult GuestDetailsReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddMonths(1);

            const int pageSize = 1;
            var entirelist = _guestService.GetByQuery(HotelID).Where(x => x.CreatedDate.ReportIsBetween(startDate.Value, endDate.Value));
            var paginatedList = new PaginatedList<Guest>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult StateSecurityServiceGuestDetailsReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddMonths(1);

            const int pageSize = 1;
            var entirelist = _guestService.GetByQuery(HotelID).Where(x => x.IsActive && x.CreatedDate.ReportIsBetween(startDate.Value, endDate.Value));
            var paginatedList = new PaginatedList<Guest>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestList = paginatedList
            };

            return View(avm);
        }


        [HttpGet]
        public ActionResult InitialDepositReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.Id == (int)RoomPaymentTypeEnum.InitialDeposit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult ReservationDepositReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.Id == (int)RoomPaymentTypeEnum.ReservationDeposit);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult BarReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.Id == (int)RoomPaymentTypeEnum.Bar);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult RestaurantReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.Id == (int)RoomPaymentTypeEnum.Restuarant);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult MiscellenousReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.Id == (int)RoomPaymentTypeEnum.Laundry);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
        }

        [HttpGet]
        public ActionResult RefundReport(DateTime? startDate, DateTime? endDate, int? page)
        {
            startDate = startDate ?? DateTime.Now.AddMonths(-1);
            endDate = endDate ?? DateTime.Now.AddYears(3);

            const int pageSize = 1;
            var entirelist = _guestRoomAccountService.GetByQuery(HotelID).Where(x => x.TransactionDate.ReportIsBetween(startDate.Value, endDate.Value) && x.RoomPaymentType.Id == (int)RoomPaymentTypeEnum.Refund);
            var paginatedList = new PaginatedList<GuestRoomAccount>(entirelist, page ?? 0, pageSize, startDate, endDate);

            var avm = new AdminViewModel
            {
                PaginatedGuestRoomAccountList = paginatedList
            };

            return View(avm);
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
                return "LAUNDRY";
            }

            return "MISCELLANEOUS";
        }
    }
}