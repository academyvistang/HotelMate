using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Controllers
{
    public class StockController : Controller
    {
        private readonly IStockItemHotelService _itemService;
        private readonly IStoreService _storeService;
        private readonly IStoreItemService _storeItemService;


        public StockController()
        {
            _itemService = new StockItemHotelService();
            _storeService = new StoreService();
            _storeItemService = new StoreItemService();
        }

        [HttpPost]
        public ActionResult Delivery(int[] array)
        {
            var allItems = _itemService.GetAll().ToList();

            var allStockItemIds = allItems.Where(x => x.IsActive.Value).Select(x => x.Id).ToList();

            var existingStore = _storeService.GetAll().FirstOrDefault();

            if(existingStore == null)
            {
                return RedirectToAction("Index");
            }

            var dateAdded = DateTime.Now;

            foreach (var itemId in allStockItemIds)
            {
                var name = "HotelItem_" + itemId.ToString();

                var realStock = allItems.FirstOrDefault(x => x.Id == itemId);

                if (Request.Form[name] != null && realStock != null)
                {
                    var qty = 0;
                    int.TryParse(Request.Form[name].ToString(), out qty);

                    if (qty == 0)
                        continue;

                    var storeItem = new StoreItem { Damaged = 0, DateAdded = dateAdded, ItemId = realStock.Id,
                        Quantity = qty, RecievedBy = 2, Remaining = qty, StoreId = existingStore.Id, Usage = false };

                    realStock.TotalQuantity = realStock.TotalQuantity + qty;
                    realStock.Quantity = realStock.Quantity + qty;

                    _itemService.Update(realStock);

                    _storeItemService.Create(storeItem);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Usage()
        {
            ItemIndexModel model = new ItemIndexModel();
            model.HotelItemsList = _itemService.GetAll().Where(x => x.IsActive.Value).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Usage(int[] array)
        {
            var allItems = _itemService.GetAll().ToList();

            var allStockItemIds = allItems.Where(x => x.IsActive.Value).Select(x => x.Id).ToList();

            var existingStore = _storeService.GetAll().FirstOrDefault();

            if (existingStore == null)
            {
                return RedirectToAction("Index");
            }

            var dateAdded = DateTime.Now;

            foreach (var itemId in allStockItemIds)
            {
                var name = "HotelItem_" + itemId.ToString();

                var realStock = allItems.FirstOrDefault(x => x.Id == itemId);

                if (Request.Form[name] != null && realStock != null)
                {
                    var qty = 0;
                    int.TryParse(Request.Form[name].ToString(), out qty);

                    if (qty == 0)
                        continue;

                    var storeItem = new StoreItem
                    {
                        Damaged = 0,
                        DateAdded = dateAdded,
                        ItemId = realStock.Id,
                        Quantity = qty,
                        RecievedBy = 2,
                        Remaining = qty,
                        StoreId = existingStore.Id,
                        Usage = true
                    };

                    realStock.TotalQuantity = realStock.TotalQuantity + qty;
                    realStock.Quantity = realStock.Quantity + qty;

                    _itemService.Update(realStock);

                    _storeItemService.Create(storeItem);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delivery()
        {
            ItemIndexModel model = new ItemIndexModel();
            model.HotelItemsList = _itemService.GetAll().Where(x => x.IsActive.Value).ToList();
            return View(model);
        }

      
        public ActionResult Inventory()
        {
            ItemIndexModel model = new ItemIndexModel();
            model.StoreItems = _storeItemService.GetAll().ToList();

            var modelledItemList = model.StoreItems.Where(x => !x.Usage).Select(x => new ItemInventoryModel { ItemId = x.ItemId, ItemName = x.StockItemHotel.Name, Quantity = x.Quantity, QuantityUsed = 0 }).ToList();
            var modelledItemListUsed = model.StoreItems.Where(x => x.Usage).Select(x => new ItemInventoryModel { ItemId = x.ItemId, ItemName = x.StockItemHotel.Name, Quantity = 0, QuantityUsed = x.Quantity }).ToList();
            modelledItemList.AddRange(modelledItemListUsed);
            model.ModelledItemList = modelledItemList.GroupBy(x => x.ItemName).Select(x => new ItemInventoryGroupModel { Name = x.Key, Remaining = (x.Sum(y => y.Quantity) - x.Sum(y => y.QuantityUsed)), Delivered = x.Sum(y => y.Quantity), ItemUsage = x.Sum(y => y.QuantityUsed) }).ToList();
            return View(model);
        }

        public ActionResult Index()
        {
            ItemIndexModel model = new ItemIndexModel();
            model.HotelItemsList = _itemService.GetAll().Where(x => x.IsActive.Value).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(HotelItemModel cm)
        {
            int id = 0;

            var alltems = _itemService.GetAll().ToList();

            StockItemHotel existingBarcode = null;

            if (!string.IsNullOrEmpty(cm.Barcode))
            {
                existingBarcode = alltems.FirstOrDefault(x => x.Barcode == cm.Barcode);

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

            if (ModelState.IsValid)
            {
                var existingItem = alltems.FirstOrDefault(x => x.Name.Equals(cm.Name, StringComparison.InvariantCultureIgnoreCase));

                if (existingItem != null && existingItem.Id > 0)
                {
                    cm.IsActive = true;

                    cm.TotalQuantity = cm.Quantity;

                    if (string.IsNullOrEmpty(cm.Barcode))
                    {
                        cm.Barcode = "";
                    }

                    existingItem.IsActive = true;
                    existingItem.Barcode = cm.Barcode;
                    existingItem.Name = cm.Name;
                    existingItem.Description = cm.Description;
                    existingItem.UnitPrice = cm.UnitPrice;
                    existingItem.NotNumber = cm.NotNumber;

                    var file = Request.Files[0];

                    if (file != null && file.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(file.FileName);
                        var path = Path.Combine(Server.MapPath("~/Products"), fileName);
                        file.SaveAs(path);
                        existingItem.PicturePath = fileName;
                    }

                    _itemService.Update(existingItem);

                    id = existingItem.Id;
                }
                else
                {
                    cm.IsActive = true;

                    cm.TotalQuantity = cm.Quantity;

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
                    }

                    var newItem =_itemService.Create(new StockItemHotel
                    {
                        Barcode = cm.Barcode,
                        Description = cm.Description, IsActive = true, Name = cm.Name, NotNumber = cm.NotNumber, PicturePath = cm.PicturePath, 
                         Quantity = cm.Quantity, TotalQuantity = cm.Quantity, UnitPrice = cm.UnitPrice
                    });

                    id = newItem.Id;
                }

                bool saved = true;

                return RedirectToAction("Create", new { id, saved });
            }
            return View(cm);
        }
        public ActionResult Delete(int? id)
        {
            var actualItem = _itemService.GetById(id.Value);
            var cm = new HotelItemModel
            {
                Id = actualItem.Id,
                NotNumber = actualItem.NotNumber,
                UnitPrice = actualItem.UnitPrice.Value,
                Barcode = actualItem.Barcode,
                Quantity = actualItem.Quantity,
                TotalQuantity = actualItem.TotalQuantity,
                Description = actualItem.Description,
                Name = actualItem.Name,
                IsActive = true
            };

            return View(cm);
        }

        [HttpPost]
        public ActionResult Delete(ItemModel cm)
        {
            var existingItem = _itemService.GetById(cm.Id);

            existingItem.IsActive = false;

            _itemService.Update(existingItem);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Create(int? id, bool? saved)
        {
            HotelItemModel cm = 
                new HotelItemModel { Id = 0, NotNumber = 100, UnitPrice = 100, Barcode = "", Quantity = 0, TotalQuantity = 0, Description = "", Name = "", IsActive = true };
            if(id.HasValue)
            {
                var actualItem = _itemService.GetById(id.Value);

                if(null != actualItem)
                {
                    cm = new HotelItemModel { Id = actualItem.Id, NotNumber = actualItem.NotNumber,
                        UnitPrice = actualItem.UnitPrice.Value, Barcode = actualItem.Barcode,
                        Quantity = actualItem.Quantity, TotalQuantity = actualItem.TotalQuantity, Description = actualItem.Description, Name = actualItem.Name, IsActive = true };
                }
            }

            cm.Saved = saved;
            return View(cm);
        }
    }
}