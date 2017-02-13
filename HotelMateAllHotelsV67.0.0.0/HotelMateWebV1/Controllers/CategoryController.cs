using HotelMateWebV1.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class CategoryController : Controller
    {
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

            //return lst;

        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        }

        public ActionResult Delete(int? id)
        {
            var cats = GetAllCategories();
            CategoryModel cm = cats.FirstOrDefault(x => x.Id == id.Value);
            return View(cm);
        }

        [HttpPost]
        public ActionResult Delete(CategoryModel cm)
        {
            int id = cm.Id;
            var cats = GetAllCategories();
            CategoryModel cm1 = cats.FirstOrDefault(x => x.Id == id);
            cm1.IsActive = false;
            id = UpdateCategory(cm1);

            return RedirectToAction("Index");
        }

        //[OutputCache(Duration = 3600, VaryByParam = "id,itemSaved")]
        public ActionResult Edit(int? id, bool? saved)
        {
            var cats = GetAllCategories().ToList();
            CategoryModel cm = cats.FirstOrDefault(x => x.Id == id.Value);
            cm.Saved = saved;
            return View("Create", cm);
        }

        [HttpPost]
        public ActionResult Edit(CategoryModel cm)
        {
            int id = 0;

            if (ModelState.IsValid)
            {
                if (cm.Id > 0)
                {
                    id = UpdateCategory(cm);
                }
                else
                {
                    cm.IsActive = true;
                    id = InsertCategory(cm);
                }
            }

            bool saved = true;

            return RedirectToAction("Edit", new { id, saved });
        }

        public ActionResult Index()
        {
            //return RedirectToAction("Index", "Item");

            var cats = GetAllCategories();
            CategoryIndexModel cim = new CategoryIndexModel { CategoryList = cats };
            return View(cim);
        }

        [HttpGet]
        //[OutputCache(Duration = 3600, VaryByParam = "none")]
        public ActionResult Create()
        {
            CategoryModel cm = new CategoryModel { Id = 0 };
            return View(cm);
        }

        [HttpPost]
        public ActionResult Create(CategoryModel cm)
        {
            int id = 0;

            if (ModelState.IsValid)
            {
                if (cm.Id > 0)
                {
                    cm.IsActive = true;
                    id = UpdateCategory(cm);
                }
                else
                {
                    cm.IsActive = true;
                    id = InsertCategory(cm);
                }
            }

            bool saved = true;

            return RedirectToAction("Edit", new { id, saved });

            //return RedirectToAction("Edit", new { id });
        }

        private int InsertCategory(CategoryModel cm)
        {
            int id = 0;

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("categoryInsert", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();

                    //SqlParameter custId = cmd.Parameters.AddWithValue("@CustomerId", 10);

                    cmd.Parameters.AddWithValue("@Name", cm.Name);
                    cmd.Parameters.AddWithValue("@Description", cm.Description);
                    cmd.Parameters.AddWithValue("@IsActive", cm.IsActive);

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

        private int UpdateCategory(CategoryModel cm)
        {
            int id = 0;

            using (SqlConnection myConnection = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("categoryUpdate", myConnection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    myConnection.Open();

                    //SqlParameter custId = cmd.Parameters.AddWithValue("@CustomerId", 10);

                    cmd.Parameters.AddWithValue("@Name", cm.Name);
                    cmd.Parameters.AddWithValue("@Description", cm.Description);
                    cmd.Parameters.AddWithValue("@IsActive", cm.IsActive);
                    cmd.Parameters.AddWithValue("@Id", cm.Id);

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