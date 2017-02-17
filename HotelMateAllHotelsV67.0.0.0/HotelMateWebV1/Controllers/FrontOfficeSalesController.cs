using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Controllers
{
    public class FrontOfficeSalesController : Controller
    {
        
        private readonly IPOSItemService _itemService;
        private readonly IPaymentService _paymentService;


        public FrontOfficeSalesController()
        {
            _itemService = new POSItemService();
            _paymentService = new PaymentService();
        }

        public ActionResult Index()
        {
            NonAccountViewModel model = new NonAccountViewModel();
            model.ItemList = _itemService.GetAll().Where(x => x.IsActive && x.Invinsible).ToList();
            model.GuestId = 0;
            model.Quantity = 0;
            return View(model);
        }

        public ActionResult Payment(NonAccountViewModel model)
        {
            var pmid = Request.Form["PaymentMethodId"];
            var Quantity = Request.Form["Quantity"];
            var itemId = Request.Form["ItemId"];
            var note = Request.Form["PaymentMethodNote"];
            var tax = Request.Form["Tax"];
            var guestName = Request.Form["GuestName"];



            return View(model);
        }
    }
}