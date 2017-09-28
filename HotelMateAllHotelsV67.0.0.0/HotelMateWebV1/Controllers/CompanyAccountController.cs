using AutoMapper;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelMateWebV1.Helpers;

namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class CompanyAccountController : Controller
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
        private readonly IBusinessAccountCorporateService _businessAccountCorporateService = null;
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


        [HttpPost]
        public ActionResult New(CompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                Mapper.CreateMap<CompanyViewModel, BusinessAccount>();
                BusinessAccount company = Mapper.Map<CompanyViewModel, BusinessAccount>(model);

                if (model.Id == 0)
                {
                    company.HotelId = HotelID;
                    company.CompanyTelephone = company.Telephone;
                    company.CompanyAddress = company.CompanyAddress;
                    company.Status = "LIVE";
                    company.Debtor = false;
                    company.AccountTypeId = 1;
                    _businessAccountService.Create(company);
                }
                else
                {
                    var existingCompany = _businessAccountService.GetById(model.Id);
                    existingCompany.Email = model.Email;
                    existingCompany.Name = model.Name;
                    existingCompany.ContactName = model.ContactName;
                    existingCompany.Address = model.Address;
                    existingCompany.CompanyAddress = model.CompanyAddress;
                    existingCompany.CompanyName = model.CompanyName;
                    existingCompany.CompanyTelephone = model.CompanyTelephone;
                    existingCompany.ContactName = model.ContactName;
                    existingCompany.ContactNumber = model.ContactNumber;
                    existingCompany.NatureOfBusiness = model.NatureOfBusiness;
                    existingCompany.Telephone = model.Telephone;
                    existingCompany.Mobile = model.Mobile;

                    _businessAccountService.Update(existingCompany);
                }

                return RedirectToAction("Edit", new { id = company.Id, itemSaved = true });
            }

            
            return View(model);
        }

        public ActionResult View(int? id, bool? itemSaved)
        {
            var existingCompany = _businessAccountService.GetById(id.Value);
            var allItemisedItems =  existingCompany.Guests.SelectMany(x => x.SoldItemsAlls.Where(y => y.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).OrderByDescending(y => y.DateSold).ToList()).ToList();
            var allPaymentsMade = _businessAccountCorporateService.GetAll(HotelID).Where(x => x.BusinessAccountId == id.Value).ToList();
            
            var model = new SearchViewModel
            {
                Company = existingCompany,
                AllPaymentsMade = allPaymentsMade,
                RoomAccounts = existingCompany.GuestRooms.SelectMany(x => x.GuestRoomAccounts).ToList(),
                ItemmisedItems = allItemisedItems
            };

            return View(model);
 
        }

        [HttpGet]
        public ActionResult Edit(int? id, bool? itemSaved)
        {
            var existingCompany = _businessAccountService.GetById(id.Value);
            
            Mapper.CreateMap<BusinessAccount, CompanyViewModel>();
            var pvm = Mapper.Map<BusinessAccount, CompanyViewModel>(existingCompany);
            pvm.ItemSaved = itemSaved;
          
            return View("New", pvm);
        }

        public CompanyAccountController()
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
            _businessAccountCorporateService = new BusinessCorporateAccountService();
            _guestLedgerService = new GuestLedgerService();
        }

       

        public ActionResult TopUpAccountEdit(int? id)
        {
            var existingAmount = _businessAccountCorporateService.GetById(id.Value);

            var model = new CorporateModel
            {
                Company = existingAmount.BusinessAccount,
                
                ItemSaved = false,
                PaymentMethodId = GetPaymentMethod(existingAmount.PaymentMethodId),
                PaymentMethodNote = existingAmount.PaymentMethodNote,
                BusinessCorporateAccount = existingAmount
            };

            return View("TopUpAccount", model);
        }

        private PaymentMethodEnum GetPaymentMethod(int p)
        {
            if(p == (int)PaymentMethodEnum.Cash)
            {
                return PaymentMethodEnum.Cash;
            }
            else if (p == (int)PaymentMethodEnum.Cheque)
            {
                return PaymentMethodEnum.Cheque;
            }
            if (p == (int)PaymentMethodEnum.CreditCard)
            {
                return PaymentMethodEnum.CreditCard;
            }
            else
            {
                return PaymentMethodEnum.Cash;
            }
        }



        public ActionResult TopUpAccount(int? id, bool? itemSaved, int? paymentMethodId, string paymentMethodNote)
        {
            var model = new CorporateModel
            {
                Company = _businessAccountService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value),
                ItemSaved = itemSaved,
                PaymentMethodId = PaymentMethodEnum.Cash,
                PaymentMethodNote = paymentMethodNote,
                BusinessCorporateAccount = new BusinessCorporateAccount()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult TopUpAccount(CorporateModel model, int? paymentMethodId, string paymentMethodNote)
        {
            var company = _businessAccountService.GetAll(HotelID).FirstOrDefault(x => x.Id == model.Company.Id);
            var username = User.Identity.Name;
            var user = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));

            if (ModelState.IsValid)
            {
                if (model.BusinessCorporateAccount.Id == 0)
                {
                    BusinessCorporateAccount bca = new BusinessCorporateAccount
                    {

                        Amount = model.BusinessCorporateAccount.Amount,
                        TransactionDate = DateTime.Now,
                        TransactionId = user.PersonID,
                        PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1,
                        PaymentMethodNote = paymentMethodNote,
                        BusinessAccountId = company.Id
                    };

                    _businessAccountCorporateService.Create(bca);

                }
                else
                {
                    var existingAmount = _businessAccountCorporateService.GetById(model.BusinessCorporateAccount.Id);
                    existingAmount.Amount = model.BusinessCorporateAccount.Amount;
                    existingAmount.TransactionDate = DateTime.Now;
                    existingAmount.TransactionId = user.PersonID;
                    existingAmount.PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1;
                    existingAmount.PaymentMethodNote = paymentMethodNote;
                    existingAmount.BusinessAccountId = company.Id;
                    _businessAccountCorporateService.Update(existingAmount);
                }
                

                return RedirectToAction("TopUpAccount", "CompanyAccount", new { id = model.Company.Id, itemSaved = true, paymentMethodId, paymentMethodNote });
            }

            var newModel = new CorporateModel
            {
                Company = company,
                ItemSaved = false,
                PaymentMethodId = PaymentMethodEnum.Cash,
                PaymentMethodNote = paymentMethodNote,
                BusinessCorporateAccount = new BusinessCorporateAccount()
            };

            return View(newModel);
        }

        

        public ActionResult CheckOutGuestCreateDebtorAccount(int? id, int? roomId, int? companyId)
        {
            var guest = _guestService.GetById(id.Value);   
        
            var company = _businessAccountService.GetById(companyId.Value);

            if (companyId.Value > 0)
            {

                var guestBalance = guest.GetGuestBalance();

                //At some point record the refund if any made to guest
                var reservationIds = guest.GuestReservations.Select(x => x.Id).ToList();
                var guestRoomsIds = guest.GuestRooms.Select(x => x.Id).ToList();
                var roomIds = guest.GuestReservations.Select(x => x.RoomId).ToList();

                //Create a new GuestRoom Account for any refund#
                var accountRooms = guest.GuestRooms.ToList();

                _businessAccountService.Update(company, accountRooms);

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

                guest.CompanyId = companyId;

                _guestService.Update(guest);

                return RedirectToAction("PrintLandingForGuest", "Home", new { id = id.Value });
            }
            else
            {
                return RedirectToAction("CheckOutGuest", "Home", new { id = id.Value, roomId = roomId.Value });
            }
        }

        public ActionResult TransferGuestBill(int? id, int? roomId, int? companyId)
        {
            var model = new SearchViewModel
            {
                Company = _businessAccountService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value),
                GuestRoomsList = _guestRoomService.GetAll(HotelID).Where(x => x.IsActive).ToList()
            };

            return View(model);
        }
        public ActionResult TransferBill(int? id)
        {
            var model = new SearchViewModel
            {
                Company = _businessAccountService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value),
                GuestRoomsList = _guestRoomService.GetAll(HotelID).Where(x => x.IsActive).ToList()
            };

            return View(model);
        }
        
        public ActionResult IndexTransfer()
        {
            var model = new SearchViewModel
            {
                CompanyList = _businessAccountService.GetAll(HotelID).ToList()
            };

            return View(model);
        }

        public ActionResult Index()
        {
            var model = new SearchViewModel
            {
                CompanyList = _businessAccountService.GetAll(HotelID).ToList()
            };

            return View(model);
        }



        public ActionResult ClearLedger(int? id)
        {
            var ledger = _guestLedgerService.GetById(id.Value);

            if(ledger != null)
            {
                ledger.IsActive = false;
                _guestLedgerService.Update(ledger);
            }
            

            var model = new SearchViewModel
            {
                LedgerList = _guestLedgerService.GetAll(HotelID).Where(x => x.IsActive).ToList()
            };

            return View("IndexLedger", model);
        }


        public ActionResult IndexLedger()
        {
            var model = new SearchViewModel
            {
                LedgerList = _guestLedgerService.GetAll(HotelID).Where(x => x.IsActive).ToList()
            };

            return View(model);
        }

        public ActionResult New()
        {
            Mapper.CreateMap<BusinessAccount, CompanyViewModel>();
            var cvm = Mapper.Map<BusinessAccount, CompanyViewModel>(new BusinessAccount { Id = 0 });            
            return View(cvm);
        }
	}
}