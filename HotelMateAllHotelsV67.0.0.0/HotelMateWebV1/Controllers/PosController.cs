using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using POSService;
using POSService.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.PointOfService;


namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class PosController : Controller
    {
        private IEnumerable<POSService.Entities.StockItem> _products;
        private IEnumerable<Category> _categories;
        private IEnumerable<POSService.Entities.Guest> _guests;
        private IEnumerable<Person> _cashiers;

       // private int _personId = 2;
        private readonly IGuestService _guestService;
        private readonly IPersonService _personService;
        private readonly IPOSItemService _posItemService;
        private readonly int DistributionPointId = 5;
        private readonly ITableItemService _tableItemService;
        private readonly ISoldItemService _soldItemService;

        public PosController()
        {
            _guestService = new GuestService();
            _personService = new PersonService();
            _posItemService = new POSItemService();
            _tableItemService = new TableItemService();
            _soldItemService = new SoldItemService();
        }

        private Person _person;
        private Person Person
        {
            get { return _person ?? GetPerson(); }
            set { _person = value; }
        }

        private Person GetPerson()
        {
            var username = User.Identity.Name;
            var user = _personService.GetAllForLogin().FirstOrDefault(x => x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            return user;
        }


        public IEnumerable<POSService.Entities.StockItem> ProductsList
        {
            get
            {
                if (_products != null)
                    return _products;
                else
                {
                    _products = StockItemService.GetStockItems(1);
                    return _products;
                }
            }
            set
            {
                _products = StockItemService.GetStockItems(1);
            }
        }

        public IEnumerable<Person> CashiersList
        {
            get
            {
                if (_cashiers != null)
                    return _cashiers;
                else
                {
                    _cashiers = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId == 5).ToList(); //SAles Assistant
                    return _cashiers;
                }
            }
            set
            {
                _cashiers = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId == 5).ToList(); //SAles Assistant
            }
        }

        public IEnumerable<POSService.Entities.Guest> GuestsList
        {
            get
            {
                if (_guests != null)
                    return _guests;
                else
                {
                    _guests = StockItemService.GetCurrentGuests(1);
                    return _guests;
                }
            }
            set
            {
                _guests = StockItemService.GetCurrentGuests(1);
            }
        }

        public IEnumerable<Category> CategoriesList
        {
            get
            {
                if (_categories != null)
                    return _categories;
                else
                {
                    _categories = StockItemService.GetCategories(1);
                    return _categories;
                }
            }
            set
            {
                _categories = StockItemService.GetCategories(1);
            }
        }


        public ActionResult GetAnonymous(string name)
        {
            var availableItemsId = _posItemService.GetAll().Where(x => x.Remaining > 0 && x.DistributionPointId == DistributionPointId).Select(x => x.ItemId).ToList();

            name = name.ToUpper();

            var products = ProductsList.Where(x => x.StockItemName.ToUpper().Contains(name) && availableItemsId.Contains((int)x.Id)).ToList();

            IndexViewModel vm = new IndexViewModel();

            vm.productsList = products.OrderBy(x => x.StockItemName);

            return PartialView("_Products", vm);
        }

        //[OutputCache(Duration = int.MaxValue, VaryByParam = "category_id")]
        public ActionResult GetProducts(int? category_id, int? cat_id, int? per_page)
        {
            var availableItemsId = _posItemService.GetAll().Where(x => x.Remaining > 0 && x.DistributionPointId == DistributionPointId).Select(x => x.ItemId).ToList();

            var products = ProductsList.Where(x => x.CategoryId == category_id && availableItemsId.Contains((int)x.Id)).ToList();

            IndexViewModel vm = new IndexViewModel();

            vm.productsList = products.OrderBy(x => x.StockItemName);

            return PartialView("_Products", vm);
        }

        //[OutputCache(Duration = int.MaxValue, VaryByParam = "product_id,newQty,tableId")]
        public ActionResult GetProductCount(int? product_id, int? newQty, int? tableId)
        {
            var allPosItems = _posItemService.GetAll().ToList();

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            var availableItemsId = allPosItems.Where(x => x.Remaining > 0).Select(x => x.ItemId).ToList();

            var product = ProductsList.FirstOrDefault(x => x.Id == product_id && availableItemsId.Contains((int)x.Id));

            if(product == null)
            {
                product = new POSService.Entities.StockItem();
                product.TotalQuantity = 0;
            }
            else
            {
                var posItem = allPosItems.FirstOrDefault(x => x.ItemId == product_id.Value);
                product = new POSService.Entities.StockItem();
                product.TotalQuantity = posItem.Remaining;
            }

            if (newQty.HasValue)
            {
                if(product.TotalQuantity >= newQty.Value)
                {
                    var item = _tableItemService.GetAll().FirstOrDefault(x => x.TableId == tableId.Value && x.ItemId == product_id.Value && x.Cashier == thisUser.PersonID);

                    if(item != null)
                    {
                        item.Qty = newQty.Value;
                        _tableItemService.Update(item);
                    }

                }
            }

            return Json(new
            {
                Remainder = product.TotalQuantity

            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetTerminal(string[] terminal)
        {
            HttpContext.SetCourseListToNullCookie("PosTerminal");

            var pcs = (terminal ?? new string[0])
               .SelectMany(p => p.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            HttpContext.AmendCourseListCookie(ActionType.Add, pcs, "PosTerminal");

            return Json(new
            {                
                name = terminal,
                id = GetTerminalId(terminal.FirstOrDefault()),
            }, JsonRequestBehavior.AllowGet);
        }

        private int GetTerminalId(string terminal)
        {
            var str = terminal;

            if (terminal.ToUpper().StartsWith("B"))
                return (int)RoomPaymentTypeEnum.Bar;
            else if (terminal.ToUpper().StartsWith("R"))
                return (int)RoomPaymentTypeEnum.Restuarant;
            else if (terminal.ToUpper().StartsWith("L"))
                return (int)RoomPaymentTypeEnum.Laundry;
            else
                return (int)RoomPaymentTypeEnum.Restuarant;
        }

   

        public ActionResult GetItemByBarCode(string csrf_sma, string code)
        {
            Tax_Rate t = new Tax_Rate { id = 1, rate = 10, type = 2 };
            var product = ProductsList.FirstOrDefault(x => x.Barcode == code);

            return Json(new
            {
                item_price = product.UnitPrice,
                product_name = product.StockItemName,
                product_code = product.Id.ToString(),
                tax_rate = t
            }, JsonRequestBehavior.AllowGet);
        }

   



        public ActionResult ReduceOrDeleteQuatity(int? tableId, int? id, int? qty)
        {
            var itemIdString = id.ToString().Substring(1);

            var itemId = int.Parse(itemIdString);

            var item = _tableItemService.GetAll().FirstOrDefault(x => x.TableId == tableId.Value && x.ItemId == itemId);

            if(item != null)
            {
                var quantity = item.Qty;

                if(quantity == qty.Value)
                {
                    _tableItemService.Delete(item);                   
                }
                else
                {
                    item.Qty -= qty.Value;
                    _tableItemService.Update(item);
                }
            }           

            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);


        }


        public ActionResult DeleteFromTable(int? id)
        {
            var items = _tableItemService.GetAll().Where(x => x.TableId == id.Value).ToList();

            foreach(var item in items)
            {
                _tableItemService.Delete(item);
            }

            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPriceB4(int? code, string v)
        {
            Tax_Rate t = new Tax_Rate { id = 1, rate = 10, type = 2 };
            var product = ProductsList.FirstOrDefault(x => x.Id == code);

            return Json(new
            {
                price = product.UnitPrice,
                name = product.StockItemName,
                code = product.Id.ToString(),
                tax_rate = t,
                available = product.TotalQuantity
            }, JsonRequestBehavior.AllowGet);
        }

        //[OutputCache(Duration = int.MaxValue, VaryByParam = "code,tableId,ignore")]        
        public ActionResult GetPrice(int? code, int? tableId, bool? ignore)
        {
            Tax_Rate t = new Tax_Rate { id = 1, rate = 10, type = 2 };

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            if(!tableId.HasValue)
            {
                tableId = 0;
                ignore = false;
            }
            //var product = ProductsList.FirstOrDefault(x => x.Id == code);

            var allPosItems = _posItemService.GetAll().ToList();

            var availableItemsId = allPosItems.Where(x => x.Remaining > 0 && x.DistributionPointId == DistributionPointId).Select(x => x.ItemId).ToList();

            var product = ProductsList.FirstOrDefault(x => x.Id == code && availableItemsId.Contains((int)x.Id));

            if (product == null)
            {
                product = new POSService.Entities.StockItem();
                product.TotalQuantity = 0;
                product.StockItemName = "NOT FOUND";
                product.Id = 0;
                product.UnitPrice = decimal.Zero;
            }
            else
            {
                var posItem = allPosItems.FirstOrDefault(x => x.ItemId == code.Value);
                product.TotalQuantity = posItem.Remaining;                
            }

            if(product.TotalQuantity > 0 && tableId.HasValue && (ignore.HasValue && !ignore.Value))
            {
                var existingTable = _tableItemService.GetAll().FirstOrDefault(x => x.TableId == tableId && x.ItemId == code.Value && x.Cashier == thisUser.PersonID);

                if(existingTable != null)
                {
                    existingTable.Qty = existingTable.Qty + 1;
                    _tableItemService.Update(existingTable);
                }
                else
                {
                    TableItem ti = new TableItem();
                    ti.DateSold = DateTime.Now;
                    ti.Cashier = thisUser.PersonID;
                    ti.ItemId = code.Value;
                    ti.Qty = 1;
                    ti.TableId = tableId.Value;
                    _tableItemService.Create(ti);
                }
               
            }

            

            return Json(new
            {
                price = product.UnitPrice,
                name = product.StockItemName,
                code = product.Id.ToString(),
                tax_rate = t,
                available = product.TotalQuantity
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult TransferTill(int? id)
        {
            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            var person = CashiersList.FirstOrDefault(x => x.PersonID == id);

            if(person != null)
            {
                var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;
                StockItemService.TransferTill(thisUser.PersonID, person.PersonID, conn);
            }
            else
            {
                person = new Person { PersonID = 0, DisplayName = "Cashier" };
            }
           
            return Json(new
            {
                Details = person.DisplayName,
                SuccessText = "Cashier Successfully Added"         
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetGuestDetails(int? id)
        {
            var guest = GuestsList.FirstOrDefault(x => x.Id == id);

            if (guest == null)
                guest = new POSService.Entities.Guest { Id = 0, FullName = "Outside Customer", RoomNumber = "Outside Customer", GuestRoomId = 0 };

            return Json(new
            {
                Details = guest.RoomNumber,
                SuccessText = "Customer Successfully Added",
                Id = guest.Id,
                GuestRoomId = guest.GuestRoomId
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CloseTill()
        {
            var cashierId = Person.PersonID;

            var unclearedItems = _tableItemService.GetAll().Where(x => x.Cashier == cashierId).Count();

            if(unclearedItems > 0)
            {
                return Content(@"<script language='javascript' type='text/javascript'>
                alert('You cannot close your till as you have pending items, please transfer your till first!');
                location.href = '/POS/Index';
                </script>");
            }

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;
            
            var allSoldItems = StockItemService.GetSoldItems(cashierId, conn);
            try
            {
                PrintReceipt(allSoldItems);
            }
            catch
            {

            }
            
            StockItemService.CloseTill(cashierId, conn);
            return RedirectToAction("Index");
        }


        public ActionResult TablesIndex()
        {
            return RedirectToAction("Table", new {id = 1 });
        }

        public ActionResult Table(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction("Index");

            List<POSService.Entities.StockItem> items = new List<POSService.Entities.StockItem>();

            items.Add(new POSService.Entities.StockItem { Description = "LEE", UnitPrice = decimal.Zero, Quantity = 2 });

            //SendToPosPrinter(items);

            IndexViewModel vm = new IndexViewModel();

            vm.TableId = id.Value;

            var cl = HttpContext.GetCourseListCookie("PosTerminal");

            var terminal = "Terminal";

            if (!string.IsNullOrEmpty(cl.FirstOrDefault()))
            {
                terminal = cl.FirstOrDefault();
            }

            vm.Table = "Table " + id.Value;

            vm.Terminal = terminal;

            var lst = GuestsList.ToList();

            lst.Insert(0, new POSService.Entities.Guest { Id = 0, FullName = "Outside Customer", RoomNumber = "Outside Customer", GuestRoomId = 0 });

            vm.CurrentGuests = lst;

            vm.CurrentGuests.ToList().ForEach(x => x.RoomNumber = GetFullName(x));

            vm.CurrentCashiers = CashiersList;

            var allPosItems = _posItemService.GetAll().Where(x => x.DistributionPointId == DistributionPointId).ToList();

            var availableItemsId = allPosItems.Where(x => x.Remaining > 0).Select(x => x.ItemId).ToList();

            vm.productsList = ProductsList.Where(x => availableItemsId.Contains((int)x.Id)).ToList();

            var counter = allPosItems.Count(x => x.StockItem.NotNumber >= x.Quantity);

            vm.ProductsAlerts = counter;

            var catIds = vm.productsList.Select(x => x.CategoryId).ToList();

            vm.categoriesList = CategoriesList.Where(x => catIds.Contains(x.Id)).ToList();

            vm.productsList = vm.productsList.Where(x => x.CategoryId == 2).OrderBy(x => x.StockItemName).ToList();

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            vm.ExistingList = _tableItemService.GetAll().Where(x => x.TableId == id.Value && x.Cashier == thisUser.PersonID).ToList();

            return View(vm);
        }

        
        //[OutputCache(Duration = int.MaxValue, VaryByParam = "none")]
        public ActionResult Index(int? id)
        {
            var retrieveLastSale = false;

            if (id.HasValue && id.Value == 1)
                retrieveLastSale = true;            

            List<POSService.Entities.StockItem> items = new List<POSService.Entities.StockItem>();

            items.Add(new POSService.Entities.StockItem { Description = "LEE", UnitPrice = decimal.Zero, Quantity = 2 });

            //SendToPosPrinter(items);

            IndexViewModel vm = new IndexViewModel();

            var cl = HttpContext.GetCourseListCookie("PosTerminal");

            var terminal = "Terminal";

            if(!string.IsNullOrEmpty(cl.FirstOrDefault()))
            {
                terminal = cl.FirstOrDefault();
            }

            vm.Terminal = terminal;

            vm.Retrieve = retrieveLastSale;

            vm.Table = "Tables";

            var lst = GuestsList.ToList();

            lst.Insert(0, new POSService.Entities.Guest { Id = 0, FullName = "Outside Customer", RoomNumber = "Outside Customer" , GuestRoomId = 0});

            vm.CurrentGuests = lst;

            vm.CurrentGuests.ToList().ForEach(x => x.RoomNumber = GetFullName(x));

            vm.CurrentCashiers = CashiersList;

            var allPosItems = _posItemService.GetAll().Where(x => x.DistributionPointId == DistributionPointId).ToList();

            var availableItemsId = allPosItems.Where(x => x.Remaining > 0).Select(x => x.ItemId).ToList();

            vm.productsList = ProductsList.Where(x => availableItemsId.Contains((int)x.Id)).ToList();

            var counter = allPosItems.Count(x => x.StockItem.NotNumber >= x.Quantity);

            vm.ProductsAlerts = counter;

            var catIds = vm.productsList.Select(x => x.CategoryId).ToList();

            vm.categoriesList = CategoriesList.Where(x => catIds.Contains(x.Id)).ToList();

            vm.productsList = vm.productsList.Where(x => x.CategoryId == 2).OrderBy(x => x.StockItemName).ToList();

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();


            if (retrieveLastSale)
            {
                var itemsSold = _soldItemService.GetAll().Where(x => x.PersonId == thisUser.PersonID);

                var itemSold = itemsSold.LastOrDefault();

                if(itemSold != null)
                {
                   var entireList = _soldItemService.GetAll().Where(x => x.DateSold == itemSold.DateSold).ToList();

                   var completeList = entireList.Select(x => new TableItem { Cashier = x.PersonId, DateSold = x.DateSold.Value, ItemId = x.ItemId, Qty = x.Qty, TableId = 0 }).ToList();

                   vm.ExistingList = completeList;

                    foreach(var tableItem in completeList)
                    {
                        _tableItemService.Create(tableItem);
                    }

                    foreach (var soldItem in entireList)
                    {
                        _soldItemService.Delete(soldItem);
                    }
                }
                else
                {
                    vm.ExistingList = _tableItemService.GetAll().Where(x => x.TableId == 0 && x.Cashier == thisUser.PersonID).ToList();
                }


            }
            else
            {
                vm.ExistingList = _tableItemService.GetAll().Where(x => x.TableId == 0 && x.Cashier == thisUser.PersonID).ToList();
            }

            return View(vm);
        }

        private string GetFullName(POSService.Entities.Guest x)
        {

            var thisGuest = _guestService.GetById((int)x.Id);
            if (thisGuest != null)
            {
                var balance = thisGuest.GetGuestBalance();
                return x.RoomNumber + " Balance ( NGN " + balance.ToString() + " )";
            }
            else
            {
                return x.RoomNumber;
            }
        }

        [HttpPost]
        public ActionResult CheckIn(string suspend, int? tableId)
        {
            var printOnly = false;

            if (!string.IsNullOrEmpty(suspend))
            {
                printOnly = true;
            }

            int count = 0;
            int guestId = 0;
            int guestRoomId = 0;

            int paymentMethodId = 1;

            int.TryParse(Request.Form["rpaidby"], out paymentMethodId);

            var rpaidby = Request.Form["rpaidby"].ToString();
            paymentMethodId = GetPaymentMethod(rpaidby.ToUpper());

            
            var cc_no_val = Request.Form["cc_no_val"].ToString();
            var cc_holder_val = Request.Form["cc_holder_val"].ToString();
            var cheque_no_val = Request.Form["cheque_no_val"].ToString();

            var paymentMethodNote = cc_no_val + " " + cc_holder_val + " " + cheque_no_val;

            int.TryParse(Request.Form["count"], out count);
            int.TryParse(Request.Form["HotelGuestId"], out guestId);
            int.TryParse(Request.Form["GuestRoomId"], out guestRoomId);

            List<POSService.Entities.StockItem> lst = new List<POSService.Entities.StockItem>();

            if(paymentMethodId == (int)PaymentMethodEnum.POSTBILL && guestId == 0)
            {
                return Content(@"<script language='javascript' type='text/javascript'>
                alert('You cannot post a bill for a customer who is not staying in the hotel! Please go back and select a guest!');
                location.href = '/POS/Index';
                </script>");
            }

            var totalBill = Decimal.Zero;

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            var tableList = _tableItemService.GetAll().Where(x => x.Cashier == thisUser.PersonID && x.TableId == tableId.Value).ToList();

            foreach(var ti in tableList)
            {
                var thisProduct = ProductsList.FirstOrDefault(x => x.Id == ti.ItemId);

                var price = thisProduct.UnitPrice;

                var qty = ti.Qty;

                totalBill += (price * qty);

                var itemDescription = thisProduct.StockItemName;

                lst.Add(new POSService.Entities.StockItem { Id = thisProduct.Id, Quantity = qty, UnitPrice = price, Description = itemDescription });

            }

            //for (int i = 1; i < count; i++)
            //{
            //    string p = "product" + i.ToString();
            //    string q = "quantity" + i.ToString();
            //    string pr = "price" + i.ToString();

            //    int productId = 0;
            //    int qty = 0;
            //    decimal price = decimal.Zero;

            //    int.TryParse(Request.Form[p], out productId);
            //    int.TryParse(Request.Form[q], out qty);
            //    decimal.TryParse(Request.Form[pr], out price);

            //    if (productId == 0)
            //        continue;

            //    totalBill += (price * qty);

            //    var itemDescription = ProductsList.FirstOrDefault(x => x.Id == productId).StockItemName;

            //    lst.Add(new POSService.Entities.StockItem { Id = productId, Quantity = qty, UnitPrice = price, Description = itemDescription });
            //}

            //Save Item To Database

            var conn = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            var ticks = (int)DateTime.Now.Ticks;

            var transactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID;

            var cl = HttpContext.GetCourseListCookie("PosTerminal");

            var terminal = "Terminal";

            if (!string.IsNullOrEmpty(cl.FirstOrDefault()))
            {
                terminal = cl.FirstOrDefault();
            }

            int terminalId = GetPosTerminalId(terminal);

            var timeOfSale = DateTime.Now;

            if (!printOnly)
            {
                StockItemService.UpdateSales(lst, transactionId, guestId, Person.PersonID, 1, guestRoomId, conn, paymentMethodId, paymentMethodNote, timeOfSale, DistributionPointId, terminalId);
                
                if (tableId.HasValue)
                    StockItemService.DeleteTableItems(tableId.Value, conn, thisUser.PersonID);
            }           
     
            double dTotal = 0;

            double.TryParse(totalBill.ToString(),out dTotal);

            try
            {
                PrintReceipt(lst, dTotal, 0, 0);
            }
            catch(Exception ex)
            {
                throw ex;
            }

            if(tableId.HasValue && tableId.Value > 0)
                return RedirectToAction("Table", new {id = tableId.Value });

            return RedirectToAction("Index");
        }

        private int GetPaymentMethod(string pm)
        {
            if (pm.ToUpper().StartsWith("POST"))
                return (int)PaymentMethodEnum.POSTBILL;
            if (pm.StartsWith("CC"))
                return (int)PaymentMethodEnum.CreditCard;                
            else if (pm.StartsWith("CASH"))
                return (int)PaymentMethodEnum.Cash;                
            else
                return (int)PaymentMethodEnum.Cheque;
                
        }

        private void PostToRoom(int guestId, int guestRoomId, decimal amount, int terminalId, int? paymentMethodId, string paymentMethodNote)
        {
            var guest = _guestService.GetById(guestId);

            if (ModelState.IsValid)
            {
                var guestRoom = guest.GuestRooms.FirstOrDefault(x => x.Id == guestRoomId);
                if (guestRoom.GuestRoomAccounts == null)
                    guestRoom.GuestRoomAccounts = new Collection<GuestRoomAccount>();
                var ticks = (int)DateTime.Now.Ticks;

                guestRoom.GuestRoomAccounts.Add(new GuestRoomAccount
                {
                    Amount = amount,
                    PaymentTypeId = terminalId,
                    TransactionDate = DateTime.Now,
                    TransactionId = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault().PersonID,
                    PaymentMethodId = paymentMethodId.HasValue ? paymentMethodId.Value : 1, PaymentMethodNote = paymentMethodNote
                });

                guest.GuestRooms.Add(guestRoom);

                _guestService.Update(guest);

                //return RedirectToAction("TopUpRestaurant", "GuestAccount", new { id = model.Guest.Id, paymentTypeId = model.PaymentTypeId, itemSaved = true });
            }
        }

        private void SendToPosPrinter(List<POSService.Entities.StockItem> lst)
        {
            ArrayList ar = new ArrayList();
            ArrayList arSD = new ArrayList();
            ArrayList arVat = new ArrayList();


            var totalAmount = decimal.Zero;

            foreach (var si in lst)
            {
                var amount = si.UnitPrice * si.Quantity;
                totalAmount += amount;
                string str = MyPadright(si.StockItemName, 5) + MyPadright(si.Quantity.ToString(), 5) + MyPadright(amount.ToString(), 5);
                ar.Add(str);
            }

            DoPrintJob(arSD, ar, arVat);
            
        }

        public static void DoPrintJob(ArrayList arShopDetails, ArrayList arItemList, ArrayList arVatChange)
        {
            var printerName = ConfigurationManager.AppSettings["PrinterName"].ToString();
            printerName = "EPSON TM-T20II Receipt";

            try
            {
                byte[] DrawerOpen5 = { 0xA };

                char V = 'a';
                byte[] DrawerOpen = { 0x1B, Convert.ToByte(V), 1 };
                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen);

                V = '!';
                byte[] DrawerOpen1 = { 0x1B, Convert.ToByte(V), 0 };
                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen1);

                for (int i = 0; i < arShopDetails.Count; i++)
                {
                    RawPrinterHelper.SendStringToPrinter(printerName, arShopDetails[i].ToString());
                    RawPrinterHelper.DoSomeThing(printerName, DrawerOpen5); //LINE FEED
                }


                V = 'd';
                byte[] DrawerOpen2 = { 0x1B, Convert.ToByte(V), 3 };
                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen2);

                V = 'a';
                byte[] DrawerOpen3 = { 0x1B, Convert.ToByte(V), 0 };
                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen3);

                V = '!';
                byte[] DrawerOpen4 = { 0x1B, Convert.ToByte(V), 1 };
                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen4);

                for (int i = 0; i < arItemList.Count; i++)
                {
                    RawPrinterHelper.SendStringToPrinter(printerName, arItemList[i].ToString());
                    RawPrinterHelper.DoSomeThing(printerName, DrawerOpen5); //LINE FEED
                }


                for (int i = 0; i < arVatChange.Count; i++)
                {
                    if (i == 0)
                    {
                        V = '!';
                        byte[] DrawerOpen6 = { 0x1B, Convert.ToByte(V), 17 };
                        RawPrinterHelper.DoSomeThing(printerName, DrawerOpen6);
                    }

                    RawPrinterHelper.SendStringToPrinter(printerName, arVatChange[i].ToString());
                    RawPrinterHelper.DoSomeThing(printerName, DrawerOpen5); //LINE FEED


                    if (i == 0)
                    {
                        V = '!';
                        byte[] DrawerOpen7 = { 0x1B, Convert.ToByte(V), 0 };
                        RawPrinterHelper.DoSomeThing(printerName, DrawerOpen7);
                    }
                }

                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen5); //LINE FEED
                RawPrinterHelper.DoSomeThing(printerName, DrawerOpen5); //LINE FEED

                RawPrinterHelper.FullCut(printerName);
                RawPrinterHelper.OpenCashDrawer1(printerName);

            }
            catch (Exception)
            {
                // MessageBox.Show(ex.Message);
            }
        }

        private string MyPadright(string str, int len)
        {
            String str1 = new String(' ', len);
            return str + str1.ToString();
        }

        private int GetPosTerminalId(string terminal)
        {
            if (terminal == "Restaurant")
                return (int)RoomPaymentTypeEnum.Restuarant;

            if (terminal == "Bar")
                return (int)RoomPaymentTypeEnum.Bar;

            if (terminal == "Laundry")
                return (int)RoomPaymentTypeEnum.Laundry;

            if (terminal == "Internet")
                return (int)RoomPaymentTypeEnum.Laundry;


            return (int)RoomPaymentTypeEnum.Restuarant;

        }

        private void PrintReceipt(System.Data.DataSet allSoldItems)
        {

            PosPrinter printer = GetReceiptPrinter();

            try
            {
                ConnectToPrinter(printer);

                string[] splitDetails = null;

                var thisUserName = User.Identity.Name;

                try
                {
                    var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                    splitDetails = shopDetails.Split('@');

                    if (splitDetails.Length != 4)
                    {
                        splitDetails = null;
                    }

                }
                catch
                {
                }

                #region PrintReceipt Twice

                for (int p = 0; p < 2; p++)
                {

                    if (splitDetails != null)
                    {
                        PrintReceiptHeader(printer, splitDetails[0].Trim(), splitDetails[1].Trim(), splitDetails[2].Trim(), splitDetails[3].Trim(), DateTime.Now, thisUserName);
                    }
                    else
                    {
                        PrintReceiptHeader(printer, "ABCDEF Pte. Ltd.", "123 My Street, My City,", "My State, My Country", "012-3456789", DateTime.Now, thisUserName);
                    }

                    int count = allSoldItems.Tables[0].Rows.Count;

                    double total = 0;

                    for (int i = 0; i < count; i++)
                    {
                        //SI.STOCKITEMNAME, SIA.Qty, SIA.TOTALPRICE
                        var description = allSoldItems.Tables[0].Rows[i][0].ToString();
                        var qty = int.Parse(allSoldItems.Tables[0].Rows[i][1].ToString());
                        var totalPrice = double.Parse(allSoldItems.Tables[0].Rows[i][2].ToString());
                        total += totalPrice;
                        var unitPrice = totalPrice / qty;
                        PrintLineItem(printer, description, qty, unitPrice);
                    }


                    try
                    {
                        PrintReceiptFooter(printer, total, 0, 0, "YOUR SHIFT HAS NOW BEEN CLOSED.");
                    }
                    catch
                    {
                    }
                }

                #endregion

            }
            finally
            {
                DisconnectFromPrinter(printer);
            } 
        }


        private void PrintReceipt(List<POSService.Entities.StockItem> lst, double total, int tax, int discount)
        {
            PosPrinter printer = GetReceiptPrinter();

            try
            {
                ConnectToPrinter(printer);
                string[] splitDetails = null;

                var thisUserName = User.Identity.Name;

                try
                {
                    var shopDetails = ConfigurationManager.AppSettings["SHOPDETAILS"].ToString();

                    splitDetails = shopDetails.Split('@');

                    if(splitDetails.Length != 4)
                    {
                        splitDetails = null;
                    }

                }
                catch(Exception)
                {
                   
                }

                if(splitDetails != null)
                {
                    PrintReceiptHeader(printer, splitDetails[0].Trim(), splitDetails[1].Trim(), splitDetails[2].Trim(), splitDetails[3].Trim(), DateTime.Now, thisUserName);
                }
                else
                {
                    PrintReceiptHeader(printer, "ABCDEF Pte. Ltd.", "123 My Street, My City,", "My State, My Country", "012-3456789", DateTime.Now, thisUserName);
                }
                
                foreach(var item in lst)
                {
                    //var total = item.UnitPrice * item.Quantity;
                    PrintLineItem(printer, item.Description, item.Quantity, double.Parse(item.UnitPrice.ToString()));

                }

                //PrintLineItem(printer, "Item 1", 10, 99.99);
                //PrintLineItem(printer, "Item 2", 101, 0.00);
                //PrintLineItem(printer, "Item 3", 9, 0.1);
                //PrintLineItem(printer, "Item 4", 1000, 1);

                PrintReceiptFooter(printer, total, tax, discount, "THANK YOU FOR YOUR PATRONAGE.");
            }
            finally
            {
                DisconnectFromPrinter(printer);

            }
        }

        private void DisconnectFromPrinter(PosPrinter printer)
        {
            try
            {
                printer.Release();
                printer.Close();

            }
            catch
            {

            }
          
        }

        private void ConnectToPrinter(PosPrinter printer)
        {
            try
            {
                printer.Open();
                printer.Claim(10000);
                printer.DeviceEnabled = true;
            }
            catch
            {

            }
        }

        private PosPrinter GetReceiptPrinter()
        {
            //PosExplorer explorer = new PosExplorer();
            //return explorer.GetDevices(DeviceType.PosPrinter, DeviceCompatibilities.OposAndCompatibilityLevel1);

            PosExplorer posExplorer = null;

            try
            {

                posExplorer = new PosExplorer();
            }
            catch (Exception)
            {

                //posExplorer = new PosExplorer(this);
            }

            //var ppp = posExplorer.GetDevices(DeviceType.PosPrinter, DeviceCompatibilities.OposAndCompatibilityLevel1);
            // var pp = posExplorer.GetDevices();
            // DeviceInfo receiptPrinterDevice = posExplorer.GetDevice("EPSON TM-T20II Receipt", "EPSON TM-T20II Receipt"); //May need to change this if you don't use a logicial name or
            //use a different one.
            DeviceInfo receiptPrinterDevice = posExplorer.GetDevice(DeviceType.PosPrinter, "POSPrinter"); //May need to change this if you don't use a logicial name or//my_device
            //DeviceInfo receiptPrinterDevice = posExplorer.GetDevice(DeviceType.PosPrinter, "Microsoft PosPrinter Simulator"); //May need to change this if you don't use a logicial name or//my_device

            //DeviceInfo receiptPrinterDevice1 = posExplorer.GetDevice(DeviceType.LineDisplay, "my_device"); //May need to change this if you don't use a logicial name or//my_device
            // receiptPrinterDevice.

            return (PosPrinter)posExplorer.CreateInstance(receiptPrinterDevice);
        }

        private void PrintReceiptFooter(PosPrinter printer, double subTotal, double tax, double discount, string footerText)
        {
            string offSetString = new string(' ', printer.RecLineChars / 2);

            PrintTextLine(printer, new string('-', (printer.RecLineChars / 3) * 2));
            PrintTextLine(printer, offSetString + String.Format("SUB-TOTAL  {0}", subTotal.ToString("#0.00")));
            PrintTextLine(printer, offSetString + String.Format("TAX        {0}", tax.ToString("#0.00")));
            PrintTextLine(printer, offSetString + String.Format("DISCOUNT   {0}", discount.ToString("#0.00")));
            PrintTextLine(printer, offSetString + new string('-', (printer.RecLineChars / 3)));
            PrintTextLine(printer, offSetString + String.Format("TOTAL      {0}", (subTotal - (tax + discount)).ToString("#0.00")));
            PrintTextLine(printer, offSetString + new string('-', (printer.RecLineChars / 3)));
            PrintTextLine(printer, String.Empty);

            //Embed 'center' alignment tag on front of string below to have it printed in the center of the receipt.
            PrintTextLine(printer, System.Text.ASCIIEncoding.ASCII.GetString(new byte[] { 27, (byte)'|', (byte)'c', (byte)'A' }) + footerText);

            //Added in these blank lines because RecLinesToCut seems to be wrong on my printer and
            //these extra blank lines ensure the cut is after the footer ends.
            PrintTextLine(printer, String.Empty);
            PrintTextLine(printer, String.Empty);
            PrintTextLine(printer, String.Empty);
            PrintTextLine(printer, String.Empty);
            PrintTextLine(printer, String.Empty);

            //Print 'advance and cut' escape command.
            PrintTextLine(printer, System.Text.ASCIIEncoding.ASCII.GetString(new byte[] { 27, (byte)'|', (byte)'1', (byte)'0', (byte)'0', (byte)'P', (byte)'f', (byte)'P' }));
        }

        private void PrintLineItem(PosPrinter printer, string itemCode, int quantity, double unitPrice)
        {
            PrintText(printer, TruncateAt(itemCode.PadRight(11), 11));
            PrintText(printer, TruncateAt(quantity.ToString("#0.00").PadLeft(9), 9));
            PrintText(printer, TruncateAt(unitPrice.ToString("#0.00").PadLeft(10), 10));
            PrintTextLine(printer, TruncateAt((quantity * unitPrice).ToString("#0.00").PadLeft(10), 10));
        }

        private void PrintReceiptHeader(PosPrinter printer, string companyName, string addressLine1, string addressLine2, string taxNumber, DateTime dateTime, string cashierName)
        {
            PrintTextLine(printer, companyName);
            PrintTextLine(printer, addressLine1);
            PrintTextLine(printer, addressLine2);
            PrintTextLine(printer, taxNumber);
            PrintTextLine(printer, new string('-', printer.RecLineChars / 2));
            PrintTextLine(printer, String.Format("DATE : {0}", dateTime.ToShortDateString()));
            PrintTextLine(printer, String.Format("CASHIER : {0}", cashierName));
            PrintTextLine(printer, String.Empty);
            PrintText(printer, "Item             ");
            PrintText(printer, "Qty  ");
            PrintText(printer, "Unit Price ");
            PrintTextLine(printer, "Total      ");
            PrintTextLine(printer, new string('=', printer.RecLineChars));
            PrintTextLine(printer, String.Empty);

        }

        private void PrintText(PosPrinter printer, string text)
        {
            if (text.Length <= printer.RecLineChars)
                printer.PrintNormal(PrinterStation.Receipt, text); //Print text
            else if (text.Length > printer.RecLineChars)
                printer.PrintNormal(PrinterStation.Receipt, TruncateAt(text, printer.RecLineChars)); //Print exactly as many characters as the printer allows, truncating the rest.
        }

        private void PrintTextLine(PosPrinter printer, string text)
        {
            if (text.Length < printer.RecLineChars)
                printer.PrintNormal(PrinterStation.Receipt, text + Environment.NewLine); //Print text, then a new line character.
            else if (text.Length > printer.RecLineChars)
                printer.PrintNormal(PrinterStation.Receipt, TruncateAt(text, printer.RecLineChars)); //Print exactly as many characters as the printer allows, truncating the rest, no new line character (printer will probably auto-feed for us)
            else if (text.Length == printer.RecLineChars)
                printer.PrintNormal(PrinterStation.Receipt, text + Environment.NewLine); //Print text, no new line character, printer will probably auto-feed for us.
        }

        private string TruncateAt(string text, int maxWidth)
        {
            string retVal = text;
            if (text.Length > maxWidth)
                retVal = text.Substring(0, maxWidth);

            return retVal;
        }
    }

}