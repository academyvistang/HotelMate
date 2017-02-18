
using Google.API.Search;
using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using POSService;
using POSService.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class ItemController : Controller
    {

        private  IPurchaseOrderService _purchaseOrderService;
        private  IPurchaseOrderItemService _purchaseOrderItemService;
        private  IPersonService _personService;
        private  IStoreService _storeService;
        private  IStoreItemService _storeItemService;
        private  IStockItemService _stockItemService;
        private  IInvoiceService _invoiceService;
        private  IDistributionPointService _distributionPointService;
        private  IDistributionPointItemService _distributionPointItemService;
        private  IBatchService _batchService;
        private  IPOSItemService _pOSItemService;
        private  IDamagedBatchItemService _damagedBatchItemService;

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && _purchaseOrderService != null)
        //    {
        //        _purchaseOrderService.Dispose();
        //        _purchaseOrderService = null;
        //    }

        //    if (disposing && _purchaseOrderItemService != null)
        //    {
        //        _purchaseOrderItemService.Dispose();
        //        _purchaseOrderItemService = null;
        //    }

        //    if (disposing && _personService != null)
        //    {
        //        _personService.Dispose();
        //        _personService = null;
        //    }

        //    if (disposing && _storeService != null)
        //    {
        //        _storeService.Dispose();
        //        _storeService = null;
        //    }


        //    if (disposing && _storeItemService != null)
        //    {
        //        _storeItemService.Dispose();
        //        _storeItemService = null;
        //    }

        //    if (disposing && _stockItemService != null)
        //    {
        //        _stockItemService.Dispose();
        //        _stockItemService = null;
        //    }

        //    if (disposing && _invoiceService != null)
        //    {
        //        _invoiceService.Dispose();
        //        _invoiceService = null;
        //    }

        //    if (disposing && _distributionPointService != null)
        //    {
        //        _distributionPointService.Dispose();
        //        _distributionPointService = null;
        //    }

        //    if (disposing && _distributionPointItemService != null)
        //    {
        //        _distributionPointItemService.Dispose();
        //        _distributionPointItemService = null;
        //    }

        //    if (disposing && _batchService != null)
        //    {
        //        _batchService.Dispose();
        //        _batchService = null;
        //    }

        //    if (disposing && _pOSItemService != null)
        //    {
        //        _pOSItemService.Dispose();
        //        _pOSItemService = null;
        //    }

        //    if (disposing && _damagedBatchItemService != null)
        //    {
        //        _damagedBatchItemService.Dispose();
        //        _damagedBatchItemService = null;
        //    }


        //    base.Dispose(disposing);
        //}



        public ItemController()
        {
            _purchaseOrderItemService = new PurchaseOrderItemService();
            _purchaseOrderService = new PurchaseOrderService();
            _personService = new PersonService();
            _storeService = new StoreService();
            _stockItemService = new StockActualItemService();
            _invoiceService = new InvoiceService();
            _storeItemService = new StoreItemService();
            _distributionPointService = new DistributionPointService();
            _distributionPointItemService = new DistributionPointItemService();
            _batchService = new BatchService();
            _pOSItemService = new POSItemService();
            _damagedBatchItemService = new DamagedBatchItemService();
        }

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
                            decimal totalPrice  = dr.GetDecimal(1);
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

        private IEnumerable<ItemModel> GetAllItems()
        {
            List<ItemModel> lst = new List<ItemModel>();

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("GetStockItems", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();

                    //SqlParameter custId = cmd.Parameters.AddWithValue("@CustomerId", 10);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int id = dr.GetInt32(0);    // Weight int
                            decimal unitPrice = dr.GetDecimal(1);
                            string description = dr.GetString(2);  // Name string
                            string picturePath = dr.GetString(3);  // Name string
                            bool isActive = dr.GetBoolean(4); // Breed string 
                            string status = dr.GetString(5);  // Name string
                            int qty = dr.GetInt32(6);    // Weight int
                            int categoryId = dr.GetInt32(7);    // Weight int
                            decimal origPrice = dr.GetDecimal(8);
                            int notNumber = dr.GetInt32(9);
                            string notStatus = dr.GetString(10);
                            string name = dr.GetString(11); // Breed string 
                            //object obj = dr.GetByte(12); // Breed string 
                            int totalQuantity = dr.GetInt32(13);    // Weight int
                            string barcode = dr.GetString(14); // Breed string 
                            string orderStatus = dr.GetString(15); // Breed string 
                            int hotelId = dr.GetInt32(20);    // Weight int  
                            bool cookedFood = dr.GetBoolean(25);
                            bool kitchenOnly = dr.GetBoolean(26);
                            //lst.Add(new ItemModel { Id = id, Description = description, IsActive = isActive, Name = name });
                            yield return new ItemModel { Id = id, UnitPrice = unitPrice,  Description = description, PicturePath = picturePath, NotStatus = notStatus,
                                IsActive = isActive, StockItemName = name, Quantity = qty, NotNumber = notNumber, CategoryId = categoryId, Barcode = barcode,
                                                         OrigPrice = origPrice,
                                                         CookedFood = cookedFood,
                                                         KitchenOnly = kitchenOnly
                            };

                        }
                    }
                }
            }

            //return lst;

        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        }

        //[OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult Delete(int? id)
        {
            var cats = GetAllItems();
            ItemModel cm = cats.FirstOrDefault(x => x.Id == id.Value);
            return View(cm);
        }

        [HttpPost]
        public ActionResult Delete(ItemModel cm)
        {
            var existingItem = POSService.StockItemService.GetSpecificItem(cm.Id).FirstOrDefault();            

            existingItem.IsActive = false;

            var id = UpdateItem(existingItem);

            return RedirectToAction("Index");
        }

        //[OutputCache(Duration = 3600, VaryByParam = "searchText")]

        public ActionResult GetGoogleImages(string searchText)
        {
            GimageSearchClient client = new GimageSearchClient("www.c-sharpcorner.com");

            try
            {

                IList<IImageResult> results = client.Search(searchText, 10);
                var imageResults = results.Select(x => new GoogleImageClass { OriginalContextUrl = x.OriginalContextUrl, Title = x.Title, Url = x.Url }).ToList();

                return PartialView("_ImageResults", imageResults);
            }
            catch(Exception)
            {
            }

            return null;
        }

        public void GetAllImages(string searchText)
        {
           
        }

        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult Sales()
        {
            var items = GetAllSoldItems();
            SoldItemIndexModel siim = new SoldItemIndexModel { ItemList = items.OrderByDescending(x => x.DateSold).ToList()};
            return View(siim);

        }

        //
        public ActionResult PORecieved(int? id, bool? saved)
        {
            var po = _purchaseOrderService.GetById(id.Value);

            var pm = new PurchaseOrderModel {  Id = po.Id, Description = po.Description, Value = po.NetValue };

           

            var persons = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList();

            var stores = _storeService.GetAll().ToList();

            persons.Insert(0, new Person { DisplayName = "-- Please Select--", PersonID = 0 });            

           
            pm.Items = null;
            var list = po.PurchaseOrderItems.ToList();
            var existingItemList = list.Select(x => x.ItemId).ToList();
            var allExistingStockItems = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();
            var newPoList = allExistingStockItems.Where(x => !existingItemList.Contains(x.Id)).Select(x => new PurchaseOrderItem { ItemId = x.Id, StockItem = x, Qty = 0 }).ToList();
            list.AddRange(newPoList);
            pm.Items = list;
            pm.Saved = saved;

            return View("CreateReceivable", pm);
        }


       

        public ActionResult POView(int? id, bool? saved)
        {
            var po = _purchaseOrderService.GetById(id.Value);

            var pm = new PurchaseOrderModel { Id = po.Id, Description = po.Description, Value = po.NetValue };

           

            var persons = new List<Person>();

            if (po.PurchaseOrderItems.Count > 0)
            {
                persons = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList();
                persons.Insert(0, new Person { DisplayName = "-- Please Select--", PersonID = 0 });
            }
            else
            {
                persons = _personService.GetAllForLogin().ToList();
            }

            

            pm.Items = null;
            var list = po.PurchaseOrderItems.ToList();
            var existingItemList = list.Select(x => x.ItemId).ToList();
            var allExistingStockItems = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();
            var newPoList = allExistingStockItems.Where(x => !existingItemList.Contains(x.Id)).Select(x => new PurchaseOrderItem { ItemId = x.Id, StockItem = x, Qty = 0 }).ToList();
            list.AddRange(newPoList);
            pm.Items = list;
            pm.Saved = saved;
            return View("POView", pm);
        }

        
        public ActionResult PODelete(int? id, bool? saved)
        {
            var pos = _purchaseOrderItemService.GetAll().Where(x => x.PurchaseOrderId == id.Value);
            
            foreach (var e in pos)
            {
                var poi = _purchaseOrderItemService.GetById(e.Id);
                _purchaseOrderItemService.Delete(poi);
            }

            var poDelete = _purchaseOrderService.GetById(id.Value);
          
            _purchaseOrderService.Delete(poDelete);

            return RedirectToAction("IndexPO");
        }


        public ActionResult ReassignToStoreManager(int? id, bool? saved)
        {
            var po = _purchaseOrderService.GetById(id.Value);

            var pm = new PurchaseOrderModel { Id = po.Id, Description = po.Description, Value = po.NetValue };

           
            var persons = new List<Person>();

            if (po.PurchaseOrderItems.Count > 0)
            {
                persons = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList();
                persons.Insert(0, new Person { DisplayName = "-- Please Select--", PersonID = 0 });
            }
            else
            {
                persons = _personService.GetAllForLogin().ToList();
            }

           

            pm.Items = null;
            var list = po.PurchaseOrderItems.ToList();
            var existingItemList = list.Select(x => x.ItemId).ToList();
            var allExistingStockItems = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();
            var newPoList = allExistingStockItems.Where(x => !existingItemList.Contains(x.Id)).Select(x => new PurchaseOrderItem { ItemId = x.Id, StockItem = x, Qty = 0 }).ToList();
            list.AddRange(newPoList);
            pm.Items = list;
            pm.Saved = saved;
            return View("ReassignToStoreManager", pm);
        }

        public ActionResult Damages(int? id)
        {
            var allExistingStockItems = _stockItemService.GetAll().ToList();
            var pm = new PurchaseOrderModel { DamagedGoods = allExistingStockItems };
            pm.Id = id.Value;
            return View(pm);
        }

        [HttpPost]
        public ActionResult Damages(int? id, int[] dummy)
        {
            var allRealStock = _stockItemService.GetAll().ToList();

            var allStockItemIds = allRealStock.Select(x => x.Id).ToList();

            int totalNumberOfItems = 0;

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            foreach (var itemId in allStockItemIds)
            {
                var name = "DamagedItem_" + itemId.ToString();

                var realStock = allRealStock.FirstOrDefault(x => x.Id == itemId);

                if (Request.Form[name] != null)
                {
                    var qty = 0;
                    int.TryParse(Request.Form[name].ToString(), out qty);

                    if (qty == 0)
                        continue;

                    totalNumberOfItems++;

                    var lastBtch = _batchService.GetAll().LastOrDefault(x => x.DistributionPointId == id.Value);

                    if (lastBtch != null)
                    {
                        var existingdbi = _damagedBatchItemService.GetAll().FirstOrDefault(x => x.ItemId == itemId);

                        if (existingdbi != null)
                        {
                            existingdbi.NumberDamaged = qty;
                            _damagedBatchItemService.Update(existingdbi);
                        }
                        else
                        {
                            DamagedBatchItem dbi = new DamagedBatchItem();
                            dbi.NumberDamaged = qty;
                            dbi.ItemId = itemId;
                            _damagedBatchItemService.Create(dbi);
                        }

                    }
                    else
                    {
                        return RedirectToAction("DamagesIndex");
                    }                    
                }  
            }

            return RedirectToAction("Damages", new { id, saved = true });
        }

       

        //public ActionResult POEdit(int? id, bool? saved)
        //{
        //    var po = _purchaseOrderService.GetById(id.Value);

        //    var pm = new PurchaseOrderModel {  Id = po.Id, Description = po.Description, Value = po.NetValue, SupplierReference = po.SupplierInvoice, Recieved = po.GoodsRecieved };

        //    var persons = new List<Person>();

        //    if(po.PurchaseOrderItems.Count == 0)
        //    {
               
        //        persons.Insert(0, new Person { DisplayName = "-- Please Select--", PersonID = 0 });
        //    }
        //    else
        //    {
        //        persons = _personService.GetAllForLogin().ToList();
        //    }

            
        //    pm.Items = null;
        //    var list = po.PurchaseOrderItems.ToList();
        //    var existingItemList = list.Select(x => x.ItemId).ToList();
        //    var allExistingStockItems = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();
        //    var newPoList = allExistingStockItems.Where(x => !existingItemList.Contains(x.Id)).Select(x => new PurchaseOrderItem { ItemId = x.Id, StockItem = x, Qty = 0}).ToList();
        //    list.AddRange(newPoList);
        //    pm.Items = list;
        //    pm.Saved = saved;
        //    return View("CreatePO", pm);
        //}

        [HttpGet]
        public ActionResult Edit(int? id, bool? saved)
        {
            var items = GetAllItems();

            var item = items.FirstOrDefault(x => x.Id == id.Value);

            int catId = item.CategoryId;

            var cats = GetAllCategories();

            var categories = cats.ToList();

            categories.Insert(0, new CategoryModel { Name = "-- Please Select--", Id = 0 });

            IEnumerable<SelectListItem> selectList =
                from c in categories
                select new SelectListItem
                {
                    Selected = (c.Id == catId),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };

            
            ItemModel cm = item;

            cm.selectList = selectList;
            cm.Saved = saved;
            cm.Id = id.Value;
            //cm.CookedFood = true;

            return View("Create",cm);
        }

        [HttpPost]
        public ActionResult Edit(ItemModel cm, string[] orderNumbers)
        {
            int id = 0;

            POSService.Entities.StockItem existingBarcode = null;

            if (!string.IsNullOrEmpty(cm.Barcode))
            {
                existingBarcode = POSService.StockItemService.GetStockItems(1).FirstOrDefault(x => x.Barcode == cm.Barcode);

                if (cm.Id == 0)
                {
                    if (existingBarcode != null)
                        ModelState.AddModelError("BarCode", "Please use a different barcode. This barcode exists for a different item");
                }
                else
                {
                    if (existingBarcode != null && existingBarcode.Id != cm.Id)
                        ModelState.AddModelError("BarCode", "Please use a different barcode. This barcode exists for a different item");
                }
            }

            if (cm.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category");
            }

            var url = string.Empty;

            var extension = string.Empty;

            var imageName = RemoveWhitespace(cm.StockItemName);

            if (orderNumbers != null && orderNumbers.Count() > 0)
            {
                url = orderNumbers.FirstOrDefault();
                extension = Path.GetExtension(url);
                //bool vv = !extension.EndsWith(".JPG", StringComparison.InvariantCultureIgnoreCase);

                if (!extension.EndsWith(".PNG", StringComparison.InvariantCultureIgnoreCase) &&
                    !extension.EndsWith(".GIF", StringComparison.InvariantCultureIgnoreCase)
                    && !extension.EndsWith(".BMP", StringComparison.InvariantCultureIgnoreCase) &&
                    !extension.EndsWith(".JPG", StringComparison.InvariantCultureIgnoreCase)
                    && !extension.EndsWith(".BMP", StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("_Form", "Please select a different image, image selected is corrupt.");
                }

                cm.PicturePath = imageName + extension;
            }


            if (ModelState.IsValid)
            {
                var existingId = GetExistingItem(cm.StockItemName);

                if (existingId.HasValue && cm.Id == 0)
                {
                    var existingItem = StockItemService.GetSpecificItem(cm.Id).FirstOrDefault();

                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "";
                    }

                    existingItem.IsActive = true;
                    existingItem.Status = "LIve";
                    existingItem.TotalQuantity = cm.Quantity;
                    existingItem.HotelId = 1;
                    existingItem.Barcode = cm.Barcode;
                    existingItem.StockItemName = cm.StockItemName;
                    existingItem.Description = cm.Description;
                    existingItem.UnitPrice = cm.UnitPrice;
                    existingItem.OrigPrice = cm.OrigPrice;
                    existingItem.NotNumber = cm.NotNumber;
                    existingItem.CategoryId = cm.CategoryId;
                    existingItem.CookedFood = cm.CookedFood;

                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        existingItem.PicturePath = fileName;
                        //return path;
                    }


                    id = UpdateItem(existingItem);


                }
                else if (cm.Id > 0)
                {
                    var existingItem = StockItemService.GetSpecificItem(cm.Id).FirstOrDefault();

                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "";
                    }

                    existingItem.IsActive = true;
                    existingItem.Status = "LIve";
                    existingItem.TotalQuantity = cm.Quantity;
                    existingItem.HotelId = 1;
                    existingItem.Barcode = cm.Barcode;
                    existingItem.StockItemName = cm.StockItemName;
                    existingItem.Description = cm.Description;
                    existingItem.UnitPrice = cm.UnitPrice;
                    existingItem.OrigPrice = cm.OrigPrice;
                    existingItem.NotNumber = cm.NotNumber;
                    existingItem.CategoryId = cm.CategoryId;
                    existingItem.CookedFood = cm.CookedFood;


                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        existingItem.PicturePath = fileName;
                        //return path;
                    }

                    id = UpdateItem(existingItem);

                   
                }
                else
                {
                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.PicturePath))
                    {
                        cm.PicturePath = "default.png";
                    }

                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        cm.PicturePath = fileName;
                        //return path;
                    }

                    id = InsertItem(cm);

                    
                }

                bool saved = true;

                return RedirectToAction("Edit", new { id, saved });
            }

            int catId = cm.CategoryId;
            var cats = GetAllCategories();
            var categories = cats.ToList();

            categories.Insert(0, new CategoryModel { Name = "-- Please Select--", Id = 0 });

            IEnumerable<SelectListItem> selectList =
                from c in categories
                select new SelectListItem
                {
                    Selected = (c.Id == catId),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };


            cm.selectList = selectList;

            return View(cm);
        }

        [HttpPost]
        public ActionResult EditB4(ItemModel cm, string[] orderNumbers)
        {
            int id = 0;

            POSService.Entities.StockItem existingBarcode = null;

            if (!string.IsNullOrEmpty(cm.Barcode))
            {
                existingBarcode = POSService.StockItemService.GetStockItems(1).FirstOrDefault(x => x.Barcode == cm.Barcode);

                if (cm.Id == 0)
                {
                    if (existingBarcode != null)
                        ModelState.AddModelError("BarCode", "Please use a different barcode. This barcode exists for a different item");
                }
                else
                {
                    if (existingBarcode != null && existingBarcode.Id != cm.Id)
                        ModelState.AddModelError("BarCode", "Please use a different barcode. This barcode exists for a different item");
                }
            }
            

            if (cm.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category");
            }


            var url = string.Empty;

            var extension = string.Empty;

            var imageName = RemoveWhitespace(cm.StockItemName);

            if (orderNumbers != null && orderNumbers.Count() > 0)
            {
                url = orderNumbers.FirstOrDefault();
                extension = Path.GetExtension(url);
                //bool vv = !extension.EndsWith(".JPG", StringComparison.InvariantCultureIgnoreCase);

                if (!extension.EndsWith(".PNG", StringComparison.InvariantCultureIgnoreCase) &&
                    !extension.EndsWith(".GIF", StringComparison.InvariantCultureIgnoreCase)
                    && !extension.EndsWith(".BMP", StringComparison.InvariantCultureIgnoreCase) &&
                    !extension.EndsWith(".JPG", StringComparison.InvariantCultureIgnoreCase)
                    && !extension.EndsWith(".BMP", StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("_Form", "Please select a different image, image selected is corrupt.");                   
                }

                cm.PicturePath = imageName + extension;
            }                    


            if (ModelState.IsValid)
            {
                if (cm.Id > 0)
                {
                    var existingItem = StockItemService.GetSpecificItem(cm.Id).FirstOrDefault();

                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "";
                    }

                    existingItem.IsActive = true;
                    existingItem.Status = "LIve";
                    existingItem.TotalQuantity = cm.Quantity;
                    existingItem.HotelId = 1;

                    id = UpdateItem(existingItem);


                    if (id > 0 && !string.IsNullOrEmpty(url))
                    {
                        Stream imageStream = new WebClient().OpenRead(url);
                        Image img = Image.FromStream(imageStream);
                        var path = Path.Combine(Server.MapPath("~/Products"), imageName + extension);
                        try
                        {
                            img.Save(path);
                        }
                        catch
                        {

                        }

                    }
                }
                else
                {
                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.PicturePath))
                    {
                        cm.PicturePath = "default.png";
                    }

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "1111111111111";
                    }

                    cm.Description = cm.StockItemName;

                    id = InsertItem(cm);
                }

                bool saved = true;

                return RedirectToAction("Edit", new { id, saved });
            }

            int catId = cm.CategoryId;
            var cats = GetAllCategories();
            var categories = cats.ToList();

            categories.Insert(0, new CategoryModel { Name = "-- Please Select--", Id = 0 });

            IEnumerable<SelectListItem> selectList =
                from c in categories
                select new SelectListItem
                {
                    Selected = (c.Id == catId),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };


            cm.selectList = selectList;

            return View(cm);
        }

        public IEnumerable<CategoryModel> GetAllCategories()
        {
            List<CategoryModel> lst = new List<CategoryModel>();

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("GetCategories", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();

                    //SqlParameter custId = cmd.Parameters.AddWithValue("@CustomerId", 10);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int id = dr.GetInt32(0);    // Weight int
                            string description = dr.GetString(1);  // Name string
                            string name = dr.GetString(2); // Breed string 
                            bool isActive = dr.GetBoolean(3); // Breed string 
                            //lst.Add(new CategoryModel { Id = id, Description = description, IsActive = isActive, Name = name });
                            yield return new CategoryModel { Id = id, Description = description, IsActive = isActive, Name = name };

                        }
                    }
                }
            }
        }


        public ActionResult DistributeOLd(int? storeId)
        {
            var store = _storeService.GetById(storeId.Value);

            var points = _distributionPointService.GetAll();

            ItemIndexModel cim1 = new ItemIndexModel();

            cim1.StoreId = storeId.Value;

            cim1.DistributionPoints = points.ToList();

            return View(cim1);
        }

        public ActionResult DistributeToPoint(int? id, int? poId)
        {
            var point = _distributionPointService.GetById(id.Value);

            ItemIndexModel cim1 = new ItemIndexModel();
            
            cim1.DistributionPoint = point;

            cim1.POItem = _purchaseOrderService.GetById(poId);

            return View(cim1);
        }


        public ActionResult IndexStore()
        {
            var stores = _storeService.GetAll().ToList();
            ItemIndexModel cim1 = new ItemIndexModel { StoreList = stores };           
            return View(cim1);
        }


        public ActionResult IndexReceivables(int? id)
        {
            var poItems = new List<PurchaseOrder>();
            ItemIndexModel cim1 = new ItemIndexModel();

            if (id.HasValue)
            {
                poItems = _purchaseOrderService.GetAll().ToList();
                cim1 = new ItemIndexModel { POItemList = poItems };
                cim1.CanCreatePO = true;
                return View(cim1);
            }

            poItems = _purchaseOrderService.GetAll().OrderByDescending(x => x.OrderDate).ToList();
            cim1 = new ItemIndexModel { POItemList = poItems };
            cim1.CanCreatePO = false;
            return View(cim1);
        }


        public ActionResult IndexForPurchase()
        {
            var poItems = new List<PurchaseOrder>();

            ItemIndexModel cim1 = new ItemIndexModel();

            var thisUser = _personService.GetAllForLogin().ToList().Where(x => x.Username.ToUpper().Equals(User.Identity.Name)).FirstOrDefault();           

            poItems = _purchaseOrderService.GetAll().OrderByDescending(x => x.OrderDate).ToList();

            cim1 = new ItemIndexModel { POItemList = poItems };

            cim1.CanCreatePO = false;

            cim1.ThisUserId = thisUser.PersonID;

            return View(cim1);
        }

        
        public ActionResult IndexStockTaking()
        {

            ItemIndexModel cim1 = new ItemIndexModel();

            var thisUser = _personService.GetAllForLogin().ToList().Where(x => x.Username.ToUpper().Equals(User.Identity.Name)).FirstOrDefault();

            var allStores = _storeService.GetAll().ToList();

            return View(cim1);
        }

        
        public ActionResult OrderStock()
        {

            ItemIndexModel cim1 = new ItemIndexModel();

            var thisUser = _personService.GetAllForLogin().ToList().Where(x => x.Username.ToUpper().Equals(User.Identity.Name)).FirstOrDefault();

            var allStores = _storeService.GetAll().ToList();

            return View(cim1);
        }


        public ActionResult IndexStock(int? id)
        {
           
            ItemIndexModel cim1 = new ItemIndexModel();

            var thisUser = _personService.GetAllForLogin().ToList().Where(x => x.Username.ToUpper().Equals(User.Identity.Name)).FirstOrDefault();

            var allStores = _storeService.GetAll().ToList();

            cim1.StoreId = id.Value;

            if (id.HasValue)
            {
                allStores = allStores.Where(x => x.Id == id.Value).ToList();
                cim1 = new ItemIndexModel { allStores = allStores };
                cim1.CanCreatePO = true;
                cim1.ThisUserId = thisUser.PersonID;
                cim1.StoreId = id.Value;

                return View(cim1);
            }


            cim1 = new ItemIndexModel { allStores = allStores };
            cim1.CanCreatePO = false;
            cim1.ThisUserId = thisUser.PersonID;
            cim1.StoreId = id.Value;

            return View(cim1);
        }

        
        public ActionResult DamagesIndex()
        {

            var points = _distributionPointService.GetAll();

            ItemIndexModel cim1 = new ItemIndexModel();

            cim1.DistributionPoints = points.ToList();

            cim1.CanRecordDamages = _batchService.GetAll().LastOrDefault() != null;

            return View(cim1);
        }

        public ActionResult Distribute(int? id)
        {
            var points = _distributionPointService.GetAll();

            ItemIndexModel cim1 = new ItemIndexModel();

            cim1.DistributionPoints = points.ToList();

            cim1.PoId = id.Value;

            return View(cim1);
        }



        public ActionResult TransferPO()
        {
            var poItems = new List<PurchaseOrder>();

            ItemIndexModel cim1 = new ItemIndexModel();

            var thisUser = _personService.GetAllForLogin().ToList().Where(x => x.Username.ToUpper().Equals(User.Identity.Name)).FirstOrDefault();

            poItems = _purchaseOrderService.GetAll().Where(x => x.GoodsRecieved && !x.Completed && x.PurchaseOrderItems.Count > 0).OrderByDescending(x => x.OrderDate).ToList();

            cim1 = new ItemIndexModel { POItemList = poItems };

            cim1.CanCreatePO = false;

            cim1.ThisUserId = thisUser.PersonID;

            return View(cim1);
        }


        public ActionResult IndexPO()
        {
            var poItems = new List<PurchaseOrder>();

            ItemIndexModel cim1 = new ItemIndexModel();

            var thisUser = _personService.GetAllForLogin().ToList().Where(x => x.Username.ToUpper().Equals(User.Identity.Name)).FirstOrDefault();

            poItems = _purchaseOrderService.GetAll().OrderByDescending(x => x.OrderDate).ToList();

            cim1 = new ItemIndexModel { POItemList = poItems };

            cim1.CanCreatePO = false;

            cim1.ThisUserId = thisUser.PersonID;

            return View(cim1);
        }

        public ActionResult Index(int? id)
        {
            if (id.HasValue)
            {
                var items = GetAllItems();
                items = items.ToList();
                ItemIndexModel cim = new ItemIndexModel { ItemList = items };
                return View("ProductAlerts",cim);
            }

            var items1 = GetAllItems();
            ItemIndexModel cim1 = new ItemIndexModel { ItemList = items1 };
            return View(cim1);
        }

        

        [HttpGet]
        public ActionResult CreatePO()
        {

            PurchaseOrderModel pm = new PurchaseOrderModel { Id = 0, Description = "" };
           
            return View(pm);
        }


        [HttpGet]
        public ActionResult Create()
        {
            int catId = 0;
            var cats = GetAllCategories();
            var categories = cats.ToList();

            categories.Insert(0, new CategoryModel {Name = "-- Please Select--", Id = 0 });            

            IEnumerable<SelectListItem> selectList =
                from c in categories
                select new SelectListItem
                {
                    Selected = (c.Id == catId),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };

            ItemModel cm = new ItemModel { CookedFood = false , Id = 0, selectList = selectList, NotNumber = 100, OrigPrice = 100, Barcode = "", Quantity = 0, TotalQuantity = 0, Description = "Description" };

            return View(cm);
        }

        
        [HttpPost]
        public ActionResult CREATEPORECIEVED(PurchaseOrderModel pm, string submitButton)
        {       
            int id = pm.Id;
            int storeId = 1;

            if (ModelState.IsValid)
            {
                if (pm.Id > 0)
                {
                    var existingPo = _purchaseOrderService.GetById(pm.Id);
                   
                    bool addToStock = false;

                    if (submitButton.StartsWith("A"))
                    {
                        existingPo.Completed = true;
                        existingPo.Notes = "ACCEPTED";
                        addToStock = true;
                    }                        
                    else
                    {
                        existingPo.InvoiceRaised = false;
                    
                        existingPo.Notes = "REJECTED, Incorrectc Items count";
                    }

                    _purchaseOrderService.Update(existingPo);

                    //if (addToStock)
                        //AddToStock(existingPo, existingPo.AssignedTo);
                }


                return RedirectToAction("IndexStore");
            }


            return RedirectToAction("IndexStore");
        }

        private void AddToStock(PurchaseOrder existingPo, int thisUserId)
        {
            var today = DateTime.Now;

            foreach(var poi in existingPo.PurchaseOrderItems.Where(x => x.Qty > 0))
            {
                var storeItem = _storeItemService.GetAll().FirstOrDefault(x => x.ItemId == poi.ItemId);
                if(storeItem == null)
                {
                    var si = new StoreItem();
                    si.DateAdded = today;
                    si.ItemId = poi.ItemId;
                    si.Quantity = poi.Qty;
                    si.RecievedBy = thisUserId;
                
                    si.Remaining = si.Quantity;
                    _storeItemService.Create(si);

                }
                else
                {
                    storeItem.DateAdded = today;
                    storeItem.Quantity = poi.Qty;
                    storeItem.Remaining = storeItem.Remaining + poi.Qty;
                    _storeItemService.Update(storeItem);

                }
               
            }
        }

        
        [HttpPost]
        public ActionResult ReassignToStoreManager(PurchaseOrderModel pm)
        {
            int id = pm.Id;

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

            if (ModelState.IsValid)
            {
                if (pm.Id > 0)
                {
                    var existingPo = _purchaseOrderService.GetById(pm.Id);
                    var allExistingPoItems = existingPo.PurchaseOrderItems.ToList();

                   
                    existingPo.Description = pm.Description;

                    var allRealStock = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();

                    var allStockItemIds = allRealStock.Select(x => x.Id).ToList();

                    int totalNumberOfItems = 0;

                    var totalValue = decimal.Zero;

                    foreach (var itemId in allStockItemIds)
                    {
                        var name = "POItem_" + itemId.ToString();

                        var realStock = allRealStock.FirstOrDefault(x => x.Id == itemId);

                        if (Request.Form[name] != null)
                        {
                            var qty = 0;
                            int.TryParse(Request.Form[name].ToString(), out qty);

                            if (qty == 0)
                                continue;

                            totalNumberOfItems++;

                            var existingPoItem = _purchaseOrderItemService.GetAll().FirstOrDefault(x => x.ItemId == itemId && x.PurchaseOrderId == existingPo.Id);

                            if (existingPoItem != null)
                            {
                                existingPoItem.Qty = qty;
                                var thisValue = (decimal)(realStock.UnitPrice * qty);
                                totalValue += thisValue;
                                existingPoItem.TotalPrice = thisValue;
                                _purchaseOrderItemService.Update(existingPoItem);
                            }
                            else
                            {
                                var thisValue = (decimal)(realStock.UnitPrice * qty);
                                totalValue += thisValue;
                                var poItem = new PurchaseOrderItem { PurchaseOrderId = pm.Id, ItemId = itemId, Qty = qty, QtyRecieved = 0, TotalPrice = thisValue };
                                _purchaseOrderItemService.Create(poItem);
                            }
                        }
                    }

                    existingPo.NetValue = totalValue;

                    existingPo.BaseNetValue = totalValue;
                    existingPo.InvoiceRaised = true;

                    existingPo.GoodsBought = true;

                    existingPo.GoodsRecieved = false;

                    _purchaseOrderService.Update(existingPo);

                    RaiseInvoice(existingPo, thisUser.PersonID);
                
                }


                return RedirectToAction("IndexForPurchase");
            }

           

            var persons = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList();

            if (pm.Id == 0)
            {
                persons = new List<Person>() { thisUser };
            }
            else
            {
                persons.Insert(0, new Person { DisplayName = "-- Please Select--", PersonID = 0 });
            }

           

            return View(pm);
        }

        private void RaiseInvoice(PurchaseOrder existingPo, int creatorId)
        {
            Invoice invoice = _invoiceService.GetAll().FirstOrDefault(x => x.PurchaseOrderId == existingPo.Id);

            if (invoice == null)
            {
                invoice = new Invoice();
                invoice.AssignedTo = creatorId;
                invoice.CreatedDate = DateTime.Now;
                invoice.InvoiceDate = DateTime.Now;
                invoice.StatusType = (int)InvoiceStatusEnum.Paid;
                invoice.IsActive = true;
                invoice.RaisedBy = creatorId;
                invoice.PurchaseOrderId = existingPo.Id;
                invoice.TotalAmount = existingPo.NetValue;
                _invoiceService.Create(invoice);

            }
            else
            {
                invoice.TotalAmount = existingPo.NetValue;
                _invoiceService.Update(invoice);
            }
        }

        [HttpPost]
        public ActionResult DistributeToPoint(int? id, int? poId, int? dummy)
        {
            var allRealStock = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();

            var allStockItemIds = allRealStock.Select(x => x.Id).ToList();

            int totalNumberOfItems = 0;

            bool itemsTransfered = false;

            var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();                   

            foreach (var itemId in allStockItemIds)
            {
                var name = "StoreItem_" + itemId.ToString();

                var realStock = allRealStock.FirstOrDefault(x => x.Id == itemId);

                if (Request.Form[name] != null)
                {
                    var qty = 0;
                    int.TryParse(Request.Form[name].ToString(), out qty);

                    if (qty == 0)
                        continue;

                    totalNumberOfItems++;

                    int previousRemaining = 0;

                    var lastBtch = _batchService.GetAll().LastOrDefault(x => x.DistributionPointId == id.Value);

                    if(lastBtch != null)
                    {
                       var lstDistribution =  _distributionPointItemService.GetAll().FirstOrDefault(x => x.BatchId == lastBtch.Id && x.DistributionPointId == id.Value && x.ItemId == itemId);
                        if(lstDistribution != null)
                        {
                            previousRemaining = lstDistribution.Remaining;
                        }
                    }

                    Batch batch = new Batch { BatchDate = DateTime.Now, DistributionPointId = id.Value };

                    _batchService.Create(batch);

                    qty += previousRemaining;

                    //var distributionPointItem = new DistributionPointItem { PreviousRemaining = previousRemaining, BatchId = batch.Id, DateAdded = DateTime.Now, DistributionPointId = id.Value, ItemId = itemId, Quantity = qty, Remaining = qty, RecievedBy = thisUser.PersonID };
                   
                    //_distributionPointItemService.Create(distributionPointItem);

                    var existingPOSItem = _pOSItemService.GetAllInclude("StockItem").FirstOrDefault(x => x.DistributionPointId == id.Value && x.ItemId == itemId);

                    if (existingPOSItem != null)
                    {
                        existingPOSItem.Quantity += qty;
                        existingPOSItem.Remaining += qty;
                        _pOSItemService.Update(existingPOSItem);
                        itemsTransfered = true;
                    }
                    else
                    {
                        POSItem posItem = new POSItem();
                        posItem.DistributionPointId = id.Value;
                        posItem.ItemId = itemId;
                        posItem.Quantity = qty;
                        posItem.Remaining = qty;
                        posItem.IsActive = true;
                        _pOSItemService.Create(posItem);
                        itemsTransfered = true;
                    }
                }
            }

            if (itemsTransfered)
            {
                var po = _purchaseOrderService.GetById(poId.Value);

                if (po != null)
                {
                    po.Completed = true;
                    _purchaseOrderService.Update(po);
                }
            }

            return RedirectToAction("IndexPO");
        }

        //[HttpPost]
        //public ActionResult CreatePO(PurchaseOrderModel pm)
        //{
        //    int id = pm.Id;

        //    var thisUser = _personService.GetAllForLogin().Where(x => x.Username.ToUpper().Equals(User.Identity.Name.ToUpper())).FirstOrDefault();

        //    if (ModelState.IsValid)
        //    {
        //        if(pm.Id > 0)
        //        {
        //            var existingPo = _purchaseOrderService.GetById(pm.Id);
        //            var allExistingPoItems = existingPo.PurchaseOrderItems.ToList();

        //            existingPo.OrderDate = pm.OrderDate;
        //            existingPo.SupplierInvoice = pm.SupplierReference;
        //            existingPo.Description = pm.Description;
        //            existingPo.GoodsRecieved = pm.Recieved;

        //            var allRealStock = _stockItemService.GetAll().Where(x => !x.CookedFood).ToList();

        //            var allStockItemIds = allRealStock.Select(x => x.Id).ToList();

        //            int totalNumberOfItems = 0;
        //            var totalValue = decimal.Zero;

        //            foreach(var itemId in allStockItemIds)
        //            {
        //                var name = "POItem_" + itemId.ToString();
        //                var realStock = allRealStock.FirstOrDefault(x => x.Id == itemId);

        //                if(Request.Form[name] != null)
        //                {
        //                    var qty = 0;
        //                    int.TryParse(Request.Form[name].ToString(), out qty);

        //                    if(qty == 0)
        //                        continue;

        //                    totalNumberOfItems++;

        //                    var existingPoItem = _purchaseOrderItemService.GetAll().FirstOrDefault(x => x.ItemId == itemId && x.PurchaseOrderId == existingPo.Id);

        //                    if(existingPoItem != null)
        //                    {
        //                        existingPoItem.Qty = qty;
        //                        var thisValue = (decimal)(realStock.UnitPrice * qty);
        //                        totalValue += thisValue;
        //                        existingPoItem.TotalPrice = thisValue;
        //                        _purchaseOrderItemService.Update(existingPoItem);
        //                    }
        //                    else
        //                    {
        //                        var thisValue = (decimal)(realStock.UnitPrice * qty);
        //                        totalValue += thisValue;
        //                        var poItem = new PurchaseOrderItem { PurchaseOrderId = pm.Id, ItemId = itemId, Qty = qty, QtyRecieved = 0, TotalPrice = thisValue };
        //                        _purchaseOrderItemService.Create(poItem);
        //                    }
        //                }
        //            }

        //            existingPo.NetValue = totalValue;
        //            existingPo.BaseNetValue = totalValue;
                   
        //            _purchaseOrderService.Update(existingPo);
        //        }
        //        else
        //        {
                    
        //            var po = new PurchaseOrder();
        //            po.Description = pm.Description;
        //            po.OrderDate = DateTime.Now;
        //            po.RaisedBy = thisUser.PersonID;
        //            po.CreatedDate = DateTime.Now;
        //            po.NetValue = decimal.Zero;
        //            po.BaseNetValue = decimal.Zero;
        //            po.IsActive = true;
        //            po.SupplierInvoice = pm.SupplierReference;
        //            _purchaseOrderService.Create(po);

        //            id = po.Id;

        //        }

        //        return RedirectToAction("POEdit", new { id, saved = true });
        //    }

            

        //    var persons = _personService.GetAllForLogin().Where(x => x.IsActive && x.PersonTypeId != (int)PersonTypeEnum.Guest).ToList();

        //    if(pm.Id == 0)
        //    {
        //        persons = new List<Person>() { thisUser };
        //    }
        //    else
        //    {
        //        persons.Insert(0, new Person { DisplayName = "-- Please Select--", PersonID = 0 });
        //    }
          

        //    return View(pm);
        //}

        //CreateOpenTill
        [HttpPost]
        public ActionResult CreateOpenTill(string itemname, decimal? itemamount)
        {
            var existing = GetExistingItem(itemname);
            var cats = GetAllCategories();
            var categories = cats.ToList();

            if (!existing.HasValue && !string.IsNullOrEmpty(itemname) && itemamount.HasValue)
            {
                     ItemModel cm = new ItemModel();
                     cm.StockItemName = itemname;
                     cm.Description = itemname;
                     cm.OrigPrice = 100;
                     cm.UnitPrice = itemamount.Value;
                     cm.Quantity = 10000000;
                     cm.CategoryId = categories.LastOrDefault().Id;
                     cm.NotNumber = 10000;
                     cm.TotalQuantity = 10000000;

                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if(string.IsNullOrEmpty(cm.PicturePath))
                    {
                        cm.PicturePath = "default.png";
                    }

                    cm.Description = cm.StockItemName;
                    //cm.CookedFood = true;
                    
                    var id = InsertItem(cm);

                    if (cm.CookedFood)
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        if(dPoint != null)
                        {
                            var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                            if (existingPointOfServiceItem == null)
                            {
                                POSItem posItem = new POSItem();
                                posItem.DistributionPointId = dPoint.Id;
                                posItem.Invinsible = true;
                                posItem.IsActive = true;
                                posItem.ItemId = id;
                                posItem.Quantity = 10000000;
                                posItem.Remaining = 10000000;
                                _pOSItemService.Create(posItem);
                            }
                            else
                            {
                                existingPointOfServiceItem.Invinsible = true;
                                existingPointOfServiceItem.Quantity = 10000000;
                                existingPointOfServiceItem.Remaining = 10000000;
                                _pOSItemService.Update(existingPointOfServiceItem);
                            }

                        }
                       
                    }
                    else
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                        if (existingPointOfServiceItem != null)
                        {
                            existingPointOfServiceItem.IsActive = false;
                            _pOSItemService.Update(existingPointOfServiceItem);
                        }
                    }
                }

            return RedirectToAction("Index","POS");
        }

        [HttpPost]
        public ActionResult Create(ItemModel cm, string[] orderNumbers)
        {
            int id = 0;

            POSService.Entities.StockItem existingBarcode = null;

            if(!string.IsNullOrEmpty(cm.Barcode))
            {
                existingBarcode = POSService.StockItemService.GetStockItems(1).FirstOrDefault(x => x.Barcode == cm.Barcode);

                if (cm.Id == 0)
                {
                    if (existingBarcode != null)
                        ModelState.AddModelError("BarCode", "Please use a different barcode. This barcode exists for a different item");
                }
                else
                {
                    if (existingBarcode != null && existingBarcode.Id != cm.Id)
                        ModelState.AddModelError("BarCode", "Please use a different barcode. This barcode exists for a different item");
                }
            }
            
            if (cm.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category");
            }

            var url = string.Empty;

            var extension = string.Empty;

            var imageName = RemoveWhitespace(cm.StockItemName);

            if (orderNumbers != null && orderNumbers.Count() > 0)
            {
                url = orderNumbers.FirstOrDefault();
                extension = Path.GetExtension(url);
                //bool vv = !extension.EndsWith(".JPG", StringComparison.InvariantCultureIgnoreCase);

                if (!extension.EndsWith(".PNG", StringComparison.InvariantCultureIgnoreCase) &&
                    !extension.EndsWith(".GIF", StringComparison.InvariantCultureIgnoreCase)
                    && !extension.EndsWith(".BMP", StringComparison.InvariantCultureIgnoreCase) &&
                    !extension.EndsWith(".JPG", StringComparison.InvariantCultureIgnoreCase)
                    && !extension.EndsWith(".BMP", StringComparison.InvariantCultureIgnoreCase))
                {
                    ModelState.AddModelError("_Form", "Please select a different image, image selected is corrupt.");
                }

                cm.PicturePath = imageName + extension;
            }                    


            if (ModelState.IsValid)
            {
                var existingId = GetExistingItem(cm.StockItemName);

                if (existingId.HasValue && cm.Id == 0)
                {
                    var existingItem = StockItemService.GetSpecificItem(cm.Id).FirstOrDefault();

                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "";
                    }

                    existingItem.IsActive = true;
                    existingItem.Status = "LIve";
                    existingItem.TotalQuantity = cm.Quantity;
                    existingItem.HotelId = 1;
                    existingItem.Barcode = cm.Barcode;
                    existingItem.StockItemName = cm.StockItemName;
                    existingItem.Description = cm.StockItemName;
                    existingItem.UnitPrice = cm.UnitPrice;
                    existingItem.OrigPrice = cm.OrigPrice;
                    existingItem.NotNumber = cm.NotNumber;
                    existingItem.CategoryId = cm.CategoryId;
                    existingItem.CookedFood = cm.CookedFood;
                    existingItem.KitchenOnly = cm.KitchenOnly;

                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        existingItem.PicturePath = fileName;
                        //return path;
                    }


                    id = UpdateItem(existingItem);

                    if (cm.CookedFood)
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        if (dPoint != null)
                        {
                            var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                            if (existingPointOfServiceItem == null)
                            {
                                POSItem posItem = new POSItem();
                                posItem.DistributionPointId = dPoint.Id;
                                posItem.Invinsible = true;
                                posItem.IsActive = true;
                                posItem.ItemId = id;
                                posItem.Quantity = 10000000;
                                posItem.Remaining = 10000000;
                                _pOSItemService.Create(posItem);
                            }
                            else
                            {
                                existingPointOfServiceItem.Invinsible = true;
                                existingPointOfServiceItem.Quantity = 10000000;
                                existingPointOfServiceItem.Remaining = 10000000;
                                _pOSItemService.Update(existingPointOfServiceItem);
                            }

                        }

                    }
                    else
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                        if (existingPointOfServiceItem != null)
                        {
                            existingPointOfServiceItem.IsActive = false;
                            _pOSItemService.Update(existingPointOfServiceItem);
                        }
                    }

                   
                }
                else if (cm.Id > 0)
                {
                    var existingItem = StockItemService.GetSpecificItem(cm.Id).FirstOrDefault();

                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "";
                    }

                    existingItem.IsActive = true;
                    existingItem.Status = "LIve";
                    existingItem.TotalQuantity = cm.Quantity;
                    existingItem.HotelId = 1;
                    existingItem.Barcode = cm.Barcode;
                    existingItem.StockItemName = cm.StockItemName;
                    existingItem.Description = cm.Description;
                    existingItem.UnitPrice = cm.UnitPrice;
                    existingItem.OrigPrice = cm.OrigPrice;
                    existingItem.NotNumber = cm.NotNumber;
                    existingItem.CategoryId = cm.CategoryId;
                    existingItem.CookedFood = cm.CookedFood;
                    existingItem.KitchenOnly = cm.KitchenOnly;


                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        existingItem.PicturePath = fileName;
                        //return path;
                    }

       

                    id = UpdateItem(existingItem);

                    if (cm.CookedFood)
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        if (dPoint != null)
                        {
                            var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                            if (existingPointOfServiceItem == null)
                            {
                                POSItem posItem = new POSItem();
                                posItem.DistributionPointId = dPoint.Id;
                                posItem.Invinsible = true;
                                posItem.IsActive = true;
                                posItem.ItemId = id;
                                posItem.Quantity = 10000000;
                                posItem.Remaining = 10000000;
                                _pOSItemService.Create(posItem);
                            }
                            else
                            {
                                existingPointOfServiceItem.Invinsible = true;
                                existingPointOfServiceItem.Quantity = 10000000;
                                existingPointOfServiceItem.Remaining = 10000000;
                                _pOSItemService.Update(existingPointOfServiceItem);
                            }

                        }

                    }
                    else
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                        if (existingPointOfServiceItem != null)
                        {
                            existingPointOfServiceItem.IsActive = false;
                            _pOSItemService.Delete(existingPointOfServiceItem);
                        }
                    }

                    
                }
                else
                {
                    cm.IsActive = true;
                    cm.NotStatus = "Live";
                    cm.Status = "LIve";
                    cm.TotalQuantity = cm.Quantity;
                    cm.HotelId = 1;

                    if(string.IsNullOrEmpty(cm.PicturePath))
                    {
                        cm.PicturePath = "default.png";
                    }

                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        cm.PicturePath = fileName;
                        //return path;
                    }


                    cm.Description = cm.StockItemName;
                    
                    id = InsertItem(cm);

                    if (cm.CookedFood)
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        if(dPoint != null)
                        {
                            var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                            if (existingPointOfServiceItem == null)
                            {
                                POSItem posItem = new POSItem();
                                posItem.DistributionPointId = dPoint.Id;
                                posItem.Invinsible = true;
                                posItem.IsActive = true;
                                posItem.ItemId = id;
                                posItem.Quantity = 10000000;
                                posItem.Remaining = 10000000;
                                _pOSItemService.Create(posItem);
                            }
                            else
                            {
                                existingPointOfServiceItem.Invinsible = true;
                                existingPointOfServiceItem.Quantity = 10000000;
                                existingPointOfServiceItem.Remaining = 10000000;
                                _pOSItemService.Update(existingPointOfServiceItem);
                            }

                        }
                       
                    }
                    else
                    {
                        var dPoint = _distributionPointService.GetAll().FirstOrDefault(x => x.Name.ToUpper().Contains("K"));

                        var existingPointOfServiceItem = _pOSItemService.GetByQuery().FirstOrDefault(x => x.DistributionPointId == dPoint.Id && x.ItemId == id);

                        if (existingPointOfServiceItem != null)
                        {
                            existingPointOfServiceItem.IsActive = false;
                            _pOSItemService.Update(existingPointOfServiceItem);
                        }
                    }
                    
                }

                bool saved = true;

                return RedirectToAction("Edit", new { id, saved });
            }

            int catId = cm.CategoryId;
            var cats = GetAllCategories();
            var categories = cats.ToList();

            categories.Insert(0, new CategoryModel { Name = "-- Please Select--", Id = 0 });

            IEnumerable<SelectListItem> selectList =
                from c in categories
                select new SelectListItem
                {
                    Selected = (c.Id == catId),
                    Text = c.Name,
                    Value = c.Id.ToString()
                };


            cm.selectList = selectList;

            return View(cm);            
        }

        public static string RemoveWhitespace(string input)
        {
            StringBuilder output = new StringBuilder(input.Length);

            for (int index = 0; index < input.Length; index++)
            {
                if (!Char.IsWhiteSpace(input, index))
                {
                    output.Append(input[index]);
                }
            }

            return output.ToString();
        }
         

        public int? GetExistingItem(string name)
        {
            int itemId = 0;

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("Select Id FROM STOCKITEM WHERE StockItemName = '" + name + "'", myConnection))
                {
                    //cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();
                    try
                    {
                        int.TryParse(cmd.ExecuteScalar().ToString(), out itemId);
                    }
                    catch
                    {

                    }

                }
            }

            if (itemId > 0)
                return itemId;
            else
                return null;
        }

        private int InsertItem(ItemModel cm)
        {
            int id = 0;

            if(string.IsNullOrEmpty(cm.Barcode))
            {
                var ticks = (int)DateTime.Now.Ticks;
                cm.Barcode = ticks.ToString();
            }
            

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("stockItemInsert", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();                    

                    cmd.Parameters.AddWithValue("@UnitPrice", cm.UnitPrice);
                    cmd.Parameters.AddWithValue("@Description", cm.Description);
                    cmd.Parameters.AddWithValue("@PicturePath", cm.PicturePath);
                    cmd.Parameters.AddWithValue("@IsActive", cm.IsActive);
                    cmd.Parameters.AddWithValue("@Status", cm.Status);
                    cmd.Parameters.AddWithValue("@Quantity", cm.Quantity);
                    cmd.Parameters.AddWithValue("@CategoryId", cm.CategoryId);
                    cmd.Parameters.AddWithValue("@OrigPrice", cm.OrigPrice);
                    cmd.Parameters.AddWithValue("@NotNumber", cm.NotNumber);
                    cmd.Parameters.AddWithValue("@NotStatus", cm.NotStatus);
                    cmd.Parameters.AddWithValue("@StockItemName", cm.StockItemName);
                    cmd.Parameters.AddWithValue("@TotalQuantity", cm.TotalQuantity);
                    cmd.Parameters.AddWithValue("@Barcode", cm.Barcode);
                    cmd.Parameters.AddWithValue("@DISCOUNT", "NONE");
                    cmd.Parameters.AddWithValue("@OrderStatus", "NONE");
                    cmd.Parameters.AddWithValue("@DISCOUNTSTARTDATE", DateTime.Now);
                    cmd.Parameters.AddWithValue("@DISCOUNTENDDATE", DateTime.Now);
                    cmd.Parameters.AddWithValue("@DISCOUNTEDPERCENTAGE", decimal.Zero);
                    cmd.Parameters.AddWithValue("@HotelId",1);
                    cmd.Parameters.AddWithValue("@CookedFood", cm.CookedFood);
                    cmd.Parameters.AddWithValue("@KitchenOnly", cm.KitchenOnly);

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

        private int UpdateItem(POSService.Entities.StockItem cm)
        {
            int id = 0;

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("stockItemUpdate", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();

                    //SqlParameter custId = cmd.Parameters.AddWithValue("@CustomerId", 10);
                    cmd.Parameters.AddWithValue("@Id", cm.Id);
                    cmd.Parameters.AddWithValue("@UnitPrice", cm.UnitPrice);
                    cmd.Parameters.AddWithValue("@Description", cm.Description);
                    cmd.Parameters.AddWithValue("@PicturePath", cm.PicturePath);
                    cmd.Parameters.AddWithValue("@IsActive", cm.IsActive);
                    cmd.Parameters.AddWithValue("@Status", cm.Status);
                    cmd.Parameters.AddWithValue("@Quantity", cm.Quantity);
                    cmd.Parameters.AddWithValue("@CategoryId", cm.CategoryId);
                    cmd.Parameters.AddWithValue("@OrigPrice", cm.OrigPrice);
                    cmd.Parameters.AddWithValue("@NotNumber", cm.NotNumber);
                    cmd.Parameters.AddWithValue("@NotStatus", "LIVE");
                    cmd.Parameters.AddWithValue("@StockItemName", cm.StockItemName);
                    cmd.Parameters.AddWithValue("@TotalQuantity", cm.TotalQuantity);
                    cmd.Parameters.AddWithValue("@Barcode", cm.Barcode);
                    cmd.Parameters.AddWithValue("@DISCOUNT", "NONE");
                    cmd.Parameters.AddWithValue("@OrderStatus", "NONE");
                    cmd.Parameters.AddWithValue("@DISCOUNTSTARTDATE", DateTime.Now);
                    cmd.Parameters.AddWithValue("@DISCOUNTENDDATE", DateTime.Now);
                    cmd.Parameters.AddWithValue("@DISCOUNTEDPERCENTAGE", decimal.Zero);
                    cmd.Parameters.AddWithValue("@HotelId", 1);
                    cmd.Parameters.AddWithValue("@CookedFood", cm.CookedFood);
                    cmd.Parameters.AddWithValue("@KitchenOnly", cm.KitchenOnly);



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