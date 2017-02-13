using HotelMateWeb.Dal.DataCore;
using HotelMateWeb.Services.Core;
using HotelMateWeb.Services.ServiceApi;
using HotelMateWebV1.Helpers.Enums;
using HotelMateWebV1.Models;
using Lib.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HotelMateWebV1.Controllers
{
    [HandleError(View = "CustomErrorView")]
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;
        private readonly IGuestPlaylistService _guestPlaylistService;

        private readonly IPersonService _personService;
        private readonly IMovieService _movieService;
        private readonly IPOSItemService _posItemService;
        private readonly ITaxiService _taxiService;
        private readonly IAdventureService _adventureService;
        private readonly IMovieCategoryService _movieCategoryService;
        private readonly IEscortService _escortService;



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


        private Person _person;
        private Person Person
        {
            get { return _person ?? GetPerson(); }
            set { _person = value; }
        }

        private Person GetPerson()
        {
            var username = User.Identity.Name;
            var user = _personService.GetAllForLogin().FirstOrDefault(x => !string.IsNullOrEmpty(x.IdNumber) && x.Username.Equals(username, StringComparison.CurrentCultureIgnoreCase));
            return user;
        }


        public GuestController()
        {
            _guestService = new GuestService();
            _personService = new PersonService();
            _movieService = new MovieService();
            _posItemService = new POSItemService();
            _taxiService = new TaxiService();
            _adventureService = new AdventureService();
            _movieCategoryService = new MovieCategoryService();
            _guestPlaylistService = new GuestPlaylistService();
            _escortService = new EscortService();
        }

        //public ActionResult ConfirmPayments()
        //{
        //    //interswitchwebpay.webpay
        //    string transactionId = "";
        //    string rspdesc = "Approved";
        //    bool? isSuccess = false;
        //    string resp;
        //    string respDesc = "";
        //    string respCode = "";
        //    string respAmt = "";
        //    NameValueCollection webPayFeedback = new NameValueCollection();


        //    try
        //    {


        //        //if (Request["txnref"] != null)
        //        // transactionId = Request["txnref"].ToString();

        //        transactionId = Request.QueryString["txnref"].ToString();
        //        rspdesc = Request.QueryString["rspdesc"].ToString();

        //        CurrentCart.TransactionId = transactionId;

        //        //Declare variables...

        //        //WebPAY Service Call...
        //        interswitchwebpay.webpay wp = new interswitchwebpay.webpay();

        //        //This section works for those that use proxy. 
        //        //I get mine from the config file.
        //        //System.Net.WebProxy proxy = new System.Net.WebProxy(((System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("appSettings"))["proxy"]);
        //        //wp.Proxy = proxy;

        //        //Pass values to the getStatus. getStatus expects....
        //        //CADPID, MerchantID and Transaction ref. no(used to send the transaction)

        //        try
        //        {

        //            resp = wp.getStatus("ISW", "ZIB030010000136", CurrentCart.TransactionId);
        //            //resp = exx.Message;
        //            int p = 90;
        //            XBraand.DataAccessDataContext dataContext = new DataAccessDataContext();
        //            Employee em = new Employee();
        //            em.FirstName = "switch9";
        //            em.LastName = "switch9";
        //            em.BirthDate = DateTime.Now;


        //            //rspdesc = Request.QueryString["rspdesc"].ToString();
        //            string str = resp + " GET STATUS WAS A SUCCESS";
        //            if (str.Length > 999)
        //                str = str.Substring(0, 998);

        //            em.PhotoPath = str;
        //            dataContext.Employees.InsertOnSubmit(em);
        //            dataContext.SubmitChanges();
        //        }
        //        catch (Exception exx)
        //        {
        //            resp = exx.Message;
        //            int p = 90;
        //            XBraand.DataAccessDataContext dataContext = new DataAccessDataContext();
        //            Employee em = new Employee();
        //            em.FirstName = "switch0";
        //            em.LastName = "switch0";
        //            em.BirthDate = DateTime.Now;


        //            //rspdesc = Request.QueryString["rspdesc"].ToString();
        //            string str = exx.Message;
        //            if (str.Length > 999)
        //                str = str.Substring(0, 998);

        //            em.PhotoPath = str;
        //            dataContext.Employees.InsertOnSubmit(em);
        //            dataContext.SubmitChanges();
        //        }

        //        //First display response from WEBPAY
        //        string myResp = resp.ToString();


        //        //Split the response from WEBPAY
        //        String[] tmp = resp.Split('&');
        //        for (int i = 0; i < tmp.Length; i++)
        //        {
        //            string[] tmp2 = tmp[i].Split('=');
        //            webPayFeedback.Add(tmp2[0], tmp2[1]);
        //            respCode = webPayFeedback["rspcode"];
        //            respAmt = webPayFeedback["appamt"];
        //        }

        //        //Get the Response Description from the config file by passing in respCode;
        //        respDesc = ((System.Collections.Specialized.NameValueCollection)ConfigurationManager.GetSection("Esocket.Web.ResponseCodes"))[respCode];

        //        string responseDesc = respDesc;
        //        string amount = respAmt;

        //        decimal respAmount = decimal.Zero;

        //        try
        //        {
        //            respAmount = decimal.Parse(amount);
        //        }
        //        catch
        //        {

        //        }


        //        XBraand.DataAccessDataContext dataContext1 = new DataAccessDataContext();

        //        var origAmount = (from t in dataContext1.Transactions where t.TransactionGuid.ToString() == CurrentCart.TransactionId select t.TotalAmount).FirstOrDefault();

        //        if (respDesc.Equals(APPROVED))
        //        {
        //            if (origAmount != respAmount)
        //            {
        //                string strOut = "<p>Amount paid is incorrect, Amount paid is NGN " + amount + "</p>" + "<p> Actual amount to pay is NGN " + origAmount + "</p>";
        //                return RedirectToAction("TransactionFailure", new { transactionId = transactionId, respCode = strOut, responseDesc = strOut });
        //            }

        //            return RedirectToAction("SwitchPayment", new { transactionId = CurrentCart.TransactionId, isSuccess = true, responseDesc = respDesc });
        //        }
        //        else
        //        {
        //            //string transactionId, string respCode,string responseDesc
        //            return RedirectToAction("TransactionFailure", new { transactionId = transactionId, respCode = respCode, responseDesc = responseDesc });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        int p = 90;
        //        XBraand.DataAccessDataContext dataContext = new DataAccessDataContext();
        //        Employee em = new Employee();
        //        em.FirstName = "switch";
        //        em.LastName = "switch";
        //        em.BirthDate = DateTime.Now;


        //        //rspdesc = Request.QueryString["rspdesc"].ToString();
        //        string str = respDesc + " >> " + transactionId + " >> " + ex.Message;
        //        if (str.Length > 999)
        //            str = str.Substring(0, 998);

        //        em.PhotoPath = str;
        //        dataContext.Employees.InsertOnSubmit(em);
        //        dataContext.SubmitChanges();
        //    }

        //    SiteMasterFunction(null);
        //    return View();
        //}

        //[OutputCache(Duration = 3600, VaryByParam = "roomId")]

        //

        

        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        [HttpGet]
        public ActionResult BookEscort(int? id)
        {
            var personId = Person.PersonID;

            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);
            var item = _escortService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value);
            var drivername = item.Name;
            var guestName = guest.FullName;
            var guestPhone = guest.Mobile;
            var driversNumber = item.Telephone;
            var msg = "Your services are required at " + GetHotelsName() + ", Room No-" + guest.GuestRooms.FirstOrDefault().Room.RoomNumber + ", The Telephone number of the guest is : " + driversNumber + ", Guest name is " + guest.FullName;
            SendSMS(driversNumber, "", msg);

            HotelMenuModel hmm = new HotelMenuModel();
            hmm.EscortItem = item;
            return View(hmm);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        [HttpGet]
        public ActionResult BookTaxi(int? id)
        {
            var personId = Person.PersonID;

            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);
            var item = _taxiService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value);
            var drivername = item.Name;
            var guestName = guest.FullName;
            var guestPhone = guest.Mobile;
            var driversNumber = item.Telephone;
            var msg = "Your services are required at " + GetHotelsName() + ", Room No-" + guest.GuestRooms.FirstOrDefault().Room.RoomNumber + ", The Telephone number of the guest is : " + driversNumber + ", Guest name is " + guest.FullName;
            SendSMS(driversNumber, "", msg);

            HotelMenuModel hmm = new HotelMenuModel();
            hmm.CarItem = item;
            return View(hmm);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        [HttpGet]
        public ActionResult ContactAgent(int? id)
        {
            var personId = Person.PersonID;

            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);

            var agentsNumber = GetBookingAgent();
            var msg = "A Guest at " + GetHotelsName() + " needs to book a flight, Room No-" + guest.GuestRooms.FirstOrDefault().Room.RoomNumber + ", The Telephone number of the guest is : " + agentsNumber + ", Guest name is " + guest.FullName;
            SendSMS(agentsNumber, "", msg);

            HotelMenuModel hmm = new HotelMenuModel();
            hmm.BookingAgentNumber = agentsNumber;

            return View(hmm);
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

        private string GetBookingAgent()
        {
            //
            var bookingAgent = string.Empty;

            try
            {
                bookingAgent = ConfigurationManager.AppSettings["BookingAgent"].ToString();
            }
            catch
            {
                bookingAgent = "";
            }

            return bookingAgent;
        }


        private bool SendSMS(string dest, string source, string msg)
        {


            if (!dest.StartsWith("234") || string.IsNullOrEmpty(dest))
                return false;

            var telephones = dest.Split(',').ToList();

            foreach (var tel in telephones)
            {

                string username = "academyvist1";
                string password = "k9Md0uzK";
                source = "447958631557";

                var canSendSms = IsSMSEnabled();

                HTTPSMS.SendSMS sms = new HTTPSMS.SendSMS();

                sms.initialise(username, password);

                try
                {
                    if (canSendSms)
                        sms.sendSMS(tel, source, msg);
                }
                catch (HTTPSMS.SMSClientException ex)
                {
                    string msg23 = ex.Message();
                    return false;
                }
            }

            return true;
        }


        private bool IsSMSEnabled()
        {

            var sendSMS = false;

            try
            {
                string smsplus = ConfigurationManager.AppSettings["SMSMesagingEnabled"].ToString();

                if (smsplus == "1")
                    sendSMS = true;

            }
            catch
            {
                sendSMS = false;
            }

            return sendSMS;
        }


        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        [HttpGet]
        public ActionResult ViewCarImage(int? id)
        {
            var item = _taxiService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value);
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.CarItem = item;
            return View(hmm);
        }



        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        [HttpGet]
        public ActionResult ViewImage(int? id)
        {
            var item = _posItemService.GetAll().FirstOrDefault(x => x.Id == id.Value);
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.MenuItem = item;
            return View(hmm);
        }


        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        [HttpGet]
        public ActionResult CompleteViewAdventure(int? id)
        {
            var Adventureitem = _adventureService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value);
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Adventure = Adventureitem;
            return View(hmm);
        }

        //
        [OutputCache(Duration = int.MaxValue)]
        [HttpGet]
        public ActionResult LocalServices()
        {
            var items = _adventureService.GetAll(HotelID).Where(x => x.IsPlaceOfInterest).ToList();
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Adventures = items;
            return View(hmm);
        }


        [OutputCache(Duration = int.MaxValue)]
        [HttpGet]
        public ActionResult TaxiServices()
        {
            var items = _taxiService.GetAll(HotelID).ToList();
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Taxis = items;
            return View(hmm);
        }

        [OutputCache(Duration = int.MaxValue)]
        [HttpGet]
        public ActionResult EscortServices()
        {
            var items = _escortService.GetAll(HotelID).ToList();
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Escorts = items;
            return View(hmm);
        }

        //

        //[OutputCache(Duration = int.MaxValue, VaryByParam="id")]
        [HttpGet]
        public ActionResult BuyCard(int? id)
        {
            var item = _adventureService.GetAll(HotelID).FirstOrDefault(x => x.Id == id.Value);
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Adventure = item;
            return View(hmm);
        }


        [OutputCache(Duration = int.MaxValue)]
        [HttpGet]
        public ActionResult BuyRechargeCard()
        {
            var items = _adventureService.GetAll(HotelID).Where(x => !x.IsPlaceOfInterest && string.IsNullOrEmpty(x.Address)).ToList();
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Adventures = items;
            return View(hmm);
        }


        [OutputCache(Duration = int.MaxValue)]
        [HttpGet]
        public ActionResult FlightServices()
        {
            var items = _adventureService.GetAll(HotelID).Where(x => !x.IsPlaceOfInterest).ToList();
            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Adventures = items;
            return View(hmm);
        }


        [OutputCache(Duration = int.MaxValue)]
        [HttpGet]
        public ActionResult Menu()
        {
            var items = _posItemService.GetAll().GroupBy(x => x.StockItem.CategoryId).Select(x => new MenuModel { Id = x.Key, CategoryName = GetCategoryName(x.Key), Items = x.ToList() }).ToList();

            HotelMenuModel hmm = new HotelMenuModel();
            hmm.Menu = items;
            return View(hmm);
        }

        private string GetCategoryName(int id)
        {
            var categories = GetAllCategories();
            var model = categories.FirstOrDefault(x => x.Id == id);
            if (model != null)
                return model.Name;

            return string.Empty;
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

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Core"].ConnectionString;
        }

        //[OutputCache(Duration = int.MaxValue, VaryByParam = "roomId")]
        public ActionResult ExpressCheckout(int? roomId)
        {
            var id = Person.PersonID;

            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == id);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var gravm = new GuestRoomAccountViewModel
            {
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero }
            };

            return View(gravm);

        }

        //
        public ActionResult PlayMovie()
        {
            int? vidId = 108;
            CinemaModel model = new CinemaModel();
            model.VideoPath = vidId.Value.ToString() + ".mp4";
            return View(model);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "id,type")]
        public ActionResult OceansClip(int? id, string type)
        {
            var movie = _movieService.GetById(id.Value);

            string origPath = string.Empty;

            if (movie != null)
                origPath = movie.Filename;

            FileInfo oceansClipInfo = null;

            string oceansClipMimeType = String.Format("videos/{0}", type);

            var path = "~/Videos/Movies/" + origPath;
            //var path = "~/App_Data/videos/" + origPath;


            switch (type)
            {
                case "mp4":
                    oceansClipInfo = new FileInfo(Server.MapPath(path));
                    break;
                case "avi":
                    oceansClipInfo = new FileInfo(Server.MapPath(path));
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

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddRemoveChildren(CinemaModel model)
        {
            var g = _guestService.GetAll(HotelID).FirstOrDefault(x => x.Id == model.GuestId);

            var allMusic = _movieService.GetAll().Where(x => x.CategoryId == 8).ToList();

            //var existingChildren = g.Movies.ToList();

            //g.Movies.Clear();

            var selectedVideos = new List<Movie>();

            if (model.CurrentBuildingIds != null && model.CurrentBuildingIds.Count() > 0)
            {
                foreach (var id in model.CurrentBuildingIds.Distinct())
                {
                    selectedVideos.Add(allMusic.FirstOrDefault(x => x.Id == id));
                }
            }

            if (model.PlayListId == 0)
            {
                GuestPlaylist gpl = new GuestPlaylist();
                gpl.Name = model.PlaylistName;
                gpl.Description = model.PlaylistDescription;
                gpl.GuestId = g.Id;

                _guestPlaylistService.Create(gpl,selectedVideos);

            }
            else
            {
                var existingGpl = g.GuestPlaylists.FirstOrDefault(x => x.Id == model.PlayListId);
                existingGpl.Name = model.PlaylistName;
                existingGpl.Description = model.PlaylistDescription;

                _guestPlaylistService.Update(existingGpl, selectedVideos);
            }

            return RedirectToAction("MyPlayList");
        }

        [HttpGet]
        public ActionResult CreatePlayList(int? id)
        {
            var personId = Person.PersonID;

            var g = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);

            CinemaModel model = new CinemaModel();

            var everyMusicVideo = _movieService.GetAll().Where(x => x.CategoryId == 8).ToList();

            if (g.IsChild)
            {
                everyMusicVideo = everyMusicVideo.Where(x => !x.AdultOnly).ToList();
            }

            model.CurrentBuildings = From(new List<Movie>());

            model.AllBuildings = From(everyMusicVideo);

            model.GuestId = g.Id;

            GuestPlaylist gp = new GuestPlaylist();

            model.PlayListId = 0;

            if (id.HasValue && id.Value > 0)
            {
                gp = g.GuestPlaylists.FirstOrDefault(x => x.Id == id.Value);
                model.PlayListId = id.Value;
                model.PlaylistDescription = gp.Description;
                model.PlaylistName = gp.Name;
                model.CurrentBuildings = From(gp.Movies);
            }

            return View(model);
        }

        public IEnumerable<SelectListItem> From(IEnumerable<Movie> movies)
        {
            if (null == movies)
            {
                return new List<SelectListItem>();
            }

            return movies.Select(movie => new SelectListItem
            {
                Value = movie.Id.ToString(CultureInfo.InvariantCulture),
                Text = movie.Name
            });
        }

        public IEnumerable<SelectListItem> From(IEnumerable<MovieCategory> movieCategories)
        {
            if (null == movieCategories)
            {
                return new List<SelectListItem>();
            }

            return movieCategories.Select(movieCategory => new SelectListItem
            {
                Value = movieCategory.Id.ToString(CultureInfo.InvariantCulture),
                Text = movieCategory.Name
            });
        }

        [HttpPost]
        public ActionResult AddVideo(CinemaModel model, HttpPostedFileBase[] files)
        {
            if (files.Count() < 2)
                return RedirectToAction("Joromi");

            var idForPoster = 0;

            var movieHasBeenCreated = false;

            foreach (var file in files)
            {

                if (file != null && file.ContentLength > 0)
                {
                    // extract only the fielname
                    var fileName = Path.GetFileName(file.FileName);
                    string ext = Path.GetExtension(fileName);

                    if (ext.EndsWith("mp4"))
                    {
                        Movie m = new Movie();

                        if (!movieHasBeenCreated)
                        {

                            m.AdultOnly = model.AdultOnly;
                            m.CategoryId = model.CategoryId;
                            m.Name = model.VideoName;
                            m.Starring = model.MovieName;
                            m.Year = "2015";
                            m.Filename = ".mp4";

                            m = _movieService.Create(m);
                            idForPoster = m.Id;
                            m.Filename = m.Id.ToString() + ".mp4";

                            _movieService.Update(m);
                            movieHasBeenCreated = true;

                        }
                        else
                        {
                            m.Filename = idForPoster.ToString() + ".mp4";
                        }


                        var path = @"C:\inetpub\wwwroot\PublishHotelMotelFinally\Videos\Movies\" + m.Filename;

                        try
                        {
                            path = ConfigurationManager.AppSettings["MoviesStorage"].ToString() + m.Filename;
                        }
                        catch
                        {

                        }

                        file.SaveAs(path);

                    }
                    else if (ext.EndsWith(".jpg"))
                    {
                        //var path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), m.Filename);

                        Movie m = new Movie();


                        if (!movieHasBeenCreated)
                        {

                            m.AdultOnly = model.AdultOnly;
                            m.CategoryId = model.CategoryId;
                            m.Name = model.VideoName;
                            m.Starring = model.MovieName;
                            m.Year = "2015";
                            m.Filename = ".mp4";

                            m = _movieService.Create(m);
                            idForPoster = m.Id;
                            m.Filename = m.Id.ToString() + ".jpg";

                            _movieService.Update(m);
                            movieHasBeenCreated = true;

                        }
                        else
                        {
                            m.Filename = idForPoster.ToString() + ".jpg";
                        }

                        var path = @"C:\inetpub\wwwroot\PublishHotelMotelFinally\Videos\Films\" + idForPoster.ToString() + ".jpg";

                        try
                        {
                            path = ConfigurationManager.AppSettings["FilmPictureStorage"].ToString() + idForPoster.ToString() + ".jpg";
                        }
                        catch
                        {

                        }

                        file.SaveAs(path);
                    }

                }
            }

            return RedirectToAction("MyPlaylist");
        }

        public ActionResult AddVideo()
        {
            CinemaModel cm = new CinemaModel();
            Movie m = new Movie();
            cm.Categories = From(_movieCategoryService.GetAll());
            cm.Movie = m;
            return View(cm);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        public ActionResult PlayPlaylist(int? id)
        {
            var personId = Person.PersonID;

            var g = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);

            CinemaModel model = new CinemaModel();

            model.GuestName = g.FullName;

            var pl = g.GuestPlaylists.FirstOrDefault(x => x.Id == id.Value);

            var allMovies = pl.Movies.ToList();

            var movie = allMovies.FirstOrDefault();

            var posterPath = Url.Content("~/Videos/Posters/" + movie.Id.ToString() + ".jpg");

            model.MovieType = "mp4";

            model.PlaylistName = pl.Name;

            if (movie.Filename.EndsWith("avi"))
                model.MovieType = "avi";


            string origPath = string.Empty;

            if (movie != null)
                origPath = movie.Filename;

            if (movie != null)
            {
                model.MovieName = movie.Name;
            }

            model.PosterPath = posterPath;

            model.FileName = movie.Filename;

            //model.FullPath = @"http://localhost/Videos/Movies/" + model.FileName;
            //model.FullPathWebM = @"http://localhost/Videos/Movies/" + movie.Id.ToString() + ".webm";
            //model.FullPathImage = @"http://localhost/Videos/Films/" + movie.Id.ToString() + ".jpg";

            //model.MusicVideos = allMovies.Where(x => x.Id != movie.Id).Select(x => new MusicVideoModel { MovieName = x.Name, FullPath = GetFullPath(x.Id), FullPathWebM = GetFullPathWebm(x.Id), FullPathImage = GetFullPathImage(x.Id) }).ToList();


            var ipPath = GetMovieIP();

            model.FullPath = ipPath + @"Videos/Movies/" + movie.Filename;
            model.FullPathWebM = ipPath + @"Videos/Movies/" + movie.Id.ToString() + ".webm";
            model.FullPathImage = ipPath + @"Videos/Films/" + movie.Id.ToString() + ".jpg";

            //model.FullPath = ipPath + @"Videos/Movies/" + model.FileName;
            //model.FullPathWebM = ipPath + @"Videos/Movies/" + id.Value + ".webm";
            //model.FullPathImage = ipPath + @"Videos/Films/" + id.Value + ".jpg";

            //model.CategoryId = movie.CategoryId;

            model.MusicVideos = allMovies.Where(x => x.Id != movie.Id).Select(x => new MusicVideoModel { MovieName = x.Name, FullPath = GetFullPathIP(x.Id), FullPathWebM = GetFullPathWebmIP(x.Id), FullPathImage = GetFullPathImageIP(x.Id) }).ToList();


            return View(model);
        }

        public ActionResult MyPlaylist()
        {
            var id = Person.PersonID;

            var g = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == id);

            CinemaModel model = new CinemaModel();

            model.GuestName = g.FullName;

            var lst = g.GuestPlaylists.Count;

            if (lst == 0)
                return RedirectToAction("CreatePlayList");

            var pls = g.GuestPlaylists.Count;


            var allMovies = g.GuestPlaylists.FirstOrDefault().Movies.ToList();

            if (!allMovies.Any())
            {
                return RedirectToAction("CreatePlayList");
            }

            if (pls > 1)
            {
                model.PlaylistList = g.GuestPlaylists.ToList();
                return View("DisplayPlaylist", model);
            }

            model.PlaylistName = g.GuestPlaylists.FirstOrDefault().Name;

            var movie = allMovies.FirstOrDefault();

            var posterPath = Url.Content("~/Videos/Posters/" + movie.Id.ToString() + ".jpg");

            model.MovieType = "mp4";

            if (movie.Filename.EndsWith("avi"))
                model.MovieType = "avi";


            string origPath = string.Empty;

            if (movie != null)
                origPath = movie.Filename;

            if (movie != null)
            {
                model.MovieName = movie.Name;
            }

            model.PosterPath = posterPath;

            model.FileName = movie.Filename;

            //model.FullPath = @"http://localhost/Videos/Movies/" + movie.Filename;
            //model.FullPathWebM = @"http://localhost/Videos/Movies/" + movie.Id.ToString() + ".webm";
            //model.FullPathImage = @"http://localhost/Videos/Films/" + movie.Id.ToString() + ".jpg";

            //model.MusicVideos = allMovies.Where(x => x.Id != movie.Id).Select(x => new MusicVideoModel { MovieName = x.Name, FullPath = GetFullPath(x.Id), FullPathWebM = GetFullPathWebm(x.Id), FullPathImage = GetFullPathImage(x.Id) }).ToList();

            var ipPath = GetMovieIP();

            model.FullPath = ipPath + @"Videos/Movies/" + movie.Filename;
            model.FullPathWebM = ipPath + @"Videos/Movies/" + movie.Id.ToString() + ".webm";
            model.FullPathImage = ipPath + @"Videos/Films/" + movie.Id.ToString() + ".jpg";

            //model.FullPath = ipPath + @"Videos/Movies/" + model.FileName;
            //model.FullPathWebM = ipPath + @"Videos/Movies/" + id.Value + ".webm";
            //model.FullPathImage = ipPath + @"Videos/Films/" + id.Value + ".jpg";

            //model.CategoryId = movie.CategoryId;

            model.MusicVideos = allMovies.Where(x => x.Id != movie.Id).Select(x => new MusicVideoModel { MovieName = x.Name, FullPath = GetFullPathIP(x.Id), FullPathWebM = GetFullPathWebmIP(x.Id), FullPathImage = GetFullPathImageIP(x.Id) }).ToList();

            return View(model);
        }


        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        public ActionResult ShowFilm(int? id)
        {
            Guest g = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == Person.PersonID);
            var everyMovie = _movieService.GetAll().ToList();
            var allMovies = everyMovie.Where(x => x.Id == id.Value).ToList();
            var allCategories = everyMovie.Select(x => x.MovieCategory.Name).ToList();
            CinemaModel model = new CinemaModel();
            model.FilmId = id.Value;
            var movie = allMovies.FirstOrDefault(x => x.Id == id.Value);

            var posterPath = Url.Content("~/Videos/Posters/" + id.Value.ToString() + ".jpg");

            model.MovieType = "mp4";

            if (movie.Filename.EndsWith("avi"))
                model.MovieType = "avi";


            string origPath = string.Empty;

            if (movie != null)
                origPath = movie.Filename;

            if (movie != null)
            {
                model.MovieName = movie.Name;
            }

            if (g.IsChild && movie.AdultOnly)
            {
                return View("InsufficientRights");
            }

            model.PosterPath = posterPath;
            model.FileName = movie.Filename;
            //model.FullPath = @"http://localhost/Videos/Movies/11.mp4";
            //model.FullPathWebM = @"http://localhost/Videos/Movies/11.webm";

            //model.FullPath = @"http://localhost/Videos/Movies/" + model.FileName;
            //model.FullPathWebM = @"http://localhost/Videos/Movies/" + id.Value + ".webm";
            //model.FullPathImage = @"http://localhost/Videos/Films/" + id.Value + ".jpg";

            //var ipPath = GetMovieIP();

            //model.FullPath =  ipPath + @"Videos/Movies/" + model.FileName;
            //model.FullPathWebM = ipPath + @"Videos/Movies/" + id.Value + ".webm";
            //model.FullPathImage = ipPath +  @"Videos/Films/" + id.Value + ".jpg";

            //model.CategoryId = movie.CategoryId;

            //if (model.CategoryId == 8)
            //{
            //    model.MusicVideos = everyMovie.Where(x => x.CategoryId == 8 && x.Id != id.Value).Select(x => new MusicVideoModel { MovieName = x.Name, FullPath = GetFullPathIP(x.Id), FullPathWebM = GetFullPathWebmIP(x.Id), FullPathImage = GetFullPathImageIP(x.Id) }).ToList();

            //}


            return View(model);
        }

        private string GetMovieIP()
        {
            var ipDetails = @"http://localhost/";

            try
            {
                ipDetails = ConfigurationManager.AppSettings["MovieIP"].ToString();
            }
            catch
            { }

            return ipDetails;
        }


        public string GetFullPathIP(int id)
        {
            var ipPath = GetMovieIP();
            return ipPath + @"Videos/Movies/" + id.ToString() + ".mp4";
        }

        public string GetFullPathWebmIP(int id)
        {
            var ipPath = GetMovieIP();
            return ipPath + @"Videos/Movies/" + id.ToString() + ".webm";
        }

        public string GetFullPathImageIP(int id)
        {
            var ipPath = GetMovieIP();
            return ipPath + @"Videos/Films/" + id.ToString() + ".jpg";
        }

        public string GetFullPath(int id)
        {
            return @"http://localhost/Videos/Movies/" + id.ToString() + ".mp4";
        }

        public string GetFullPathWebm(int id)
        {
            return @"http://localhost/Videos/Movies/" + id.ToString() + ".webm";
        }

        public string GetFullPathImage(int id)
        {
            return @"http://localhost/Videos/Films/" + id.ToString() + ".jpg";
        }




        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        public ActionResult MusicVideos(int? id)
        {
            if (!id.HasValue)
            {
                id = 8;
            }

            var allMovies = _movieService.GetAll().Where(x => x.CategoryId == id.Value).ToList();
            var allCategories = allMovies.Select(x => x.MovieCategory.Name).ToList();


            var personId = Person.PersonID;
            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);

            if (guest.IsChild)
            {
                allMovies = allMovies.Where(x => !x.AdultOnly).ToList();
            }

            var lst = new List<CinemaModel>();
            lst = allMovies.Select(x => new CinemaModel { MovieName = x.Name, FilmName = x.Filename, Year = x.Year, Genre = "Action", Starring = x.Starring, Id = x.Id }).ToList();
            //lst.Add(new CinemaModel { FilmName = "The Vampire Diaries", Year = "2012", Genre = "Action", Starring = "Nia Farrow, Spike Lee", Id = 1 });
            //lst.Add(new CinemaModel { FilmName = "Enter The Dragon", Year = "1977", Genre = "Action", Starring = "Bruce Lee", Id = 1 });
            //lst.Add(new CinemaModel { FilmName = "Pretty Woman", Year = "1987", Genre = "Romance", Starring = "Richard Gere, Julia Roberts", Id = 1 });
            //lst.Add(new CinemaModel { FilmName = "Osawe vs 12 Badits", Year = "1997", Genre = "Action", Starring = "Peter Jack", Id = 1 });
            //lst.Add(new CinemaModel { FilmName = "Johnny Wicks", Year = "2015", Genre = "Action", Starring = "Keanu Reeves", Id = 1 });
            //lst.Add(new CinemaModel { FilmName = "History Of Violence", Year = "2007", Genre = "Action", Starring = "Von Hoight", Id = 1 });

            var gravm = new GuestRoomAccountViewModel
            {
                CinemaList = lst,
                Categories = allCategories
            };

            return View(gravm);
        }

        [OutputCache(Duration = int.MaxValue, VaryByParam = "id")]
        public ActionResult ShowFilms(int? id)
        {
            var allMovies = _movieService.GetAll().Where(x => x.CategoryId == id.Value).ToList();
            var allCategories = allMovies.Select(x => x.MovieCategory.Name).ToList();


            var personId = Person.PersonID;
            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);

            if (guest.IsChild)
            {
                allMovies = allMovies.Where(x => !x.AdultOnly).ToList();
            }

            var lst = new List<CinemaModel>();
            lst = allMovies.Select(x => new CinemaModel { MovieName = x.Name, FilmName = x.Filename, Year = x.Year, Genre = "Action", Starring = x.Starring, Id = x.Id }).ToList();

            var gravm = new GuestRoomAccountViewModel
            {
                CinemaList = lst,
                Categories = allCategories
            };

            return View(gravm);
        }

        //[OutputCache(Duration = int.MaxValue, VaryByParam = "movie,catId")]
        public ActionResult GetMoviesList(string movie, int? catId)
        {

            var allMovies = _movieService.GetAll().ToList();

            if(!string.IsNullOrEmpty(movie))
            {
                allMovies = allMovies.Where(x => x.Name.ToUpper().Contains(movie.ToUpper())).ToList();
            }

            if(catId.HasValue)
            {
                catId = 8;
                allMovies = allMovies.Where(x => x.CategoryId == catId.Value).ToList();
            }
            else
            {
                catId = 8;
                allMovies = allMovies.Where(x => x.CategoryId != catId.Value).ToList();
            }
            
            var personId = Person.PersonID;

            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == personId);

            if (guest.IsChild)
            {
                allMovies = allMovies.Where(x => !x.AdultOnly && x.Name.Contains(movie)).ToList();
            }

            var lst = new List<CinemaModel>();

            lst = allMovies.Select(x => new CinemaModel { MovieName = x.Name, FilmName = x.Filename, Year = x.Year, Genre = "Action", Starring = x.Starring, Id = x.Id }).ToList();

            var gravm = new GuestRoomAccountViewModel
            {
                CinemaList = lst
            };

            return PartialView("_Movies", gravm);

        }

        [OutputCache(Duration = int.MaxValue)]
        public ActionResult Joromi()
        {
            var allMovies = _movieService.GetAll().ToList();
            var allCategories = allMovies.Select(x => x.MovieCategory.Name).ToList();

            var lst = new List<CinemaModel>();
            lst.Add(new CinemaModel { FilmName = "The Vampire Diaries", Year = "2012", Genre = "Action", Starring = "Nia Farrow, Spike Lee", Id = 1 });
            lst.Add(new CinemaModel { FilmName = "Enter The Dragon", Year = "1977", Genre = "Action", Starring = "Bruce Lee", Id = 1 });
            lst.Add(new CinemaModel { FilmName = "Pretty Woman", Year = "1987", Genre = "Romance", Starring = "Richard Gere, Julia Roberts", Id = 1 });
            lst.Add(new CinemaModel { FilmName = "Osawe vs 12 Badits", Year = "1997", Genre = "Action", Starring = "Peter Jack", Id = 1 });
            lst.Add(new CinemaModel { FilmName = "Johnny Wicks", Year = "2015", Genre = "Action", Starring = "Keanu Reeves", Id = 1 });
            lst.Add(new CinemaModel { FilmName = "History Of Violence", Year = "2007", Genre = "Action", Starring = "Von Hoight", Id = 1 });

            var gravm = new GuestRoomAccountViewModel
            {
                CinemaList = lst,
                Categories = allCategories
            };

            //gravm.AppDataPath = GetMovieCategoryPath();

            return View("CategoryView", gravm);
        }

        private string GetMovieCategoryPath()
        {
            //
            var movieCategory = string.Empty;

            try
            {
                movieCategory = ConfigurationManager.AppSettings["MovieCategory"].ToString();
            }
            catch
            {
                movieCategory = "";
            }

            return movieCategory;
        }
        public ActionResult Feedback(int? roomId)
        {
            var id = Person.PersonID;
            var guest = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == id);

            var gravm = new GuestRoomAccountViewModel
            {
                Guest = guest
            };

            return View(gravm);
        }

        [HttpPost]
        public ActionResult GuestFeedback()
        {
            var gravm = new GuestRoomAccountViewModel
            {

            };

            return View(gravm);
        }



        //[OutputCache(Duration = 3600, VaryByParam = "roomId")]
        public ActionResult ViewAccount(int? roomId)
        {
            var id = Person.PersonID;

            var g = _guestService.GetAll(HotelID).FirstOrDefault(x => x.PersonId == id);

            if (g == null)
            {
                return View("IncorrectGuestDetails", new HotelMenuModel());
            }

            var guestId = g.Id;

            var guest = _guestService.GetById(guestId);

            GuestRoom mainGuestRoom = null;

            if (roomId.HasValue)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.RoomId == roomId);
            }

            if (mainGuestRoom == null)
            {
                mainGuestRoom = guest.GuestRooms.FirstOrDefault(x => x.GroupBookingMainRoom) ?? guest.GuestRooms.FirstOrDefault();
            }

            var allItemisedItems = guest.SoldItemsAlls.Where(x => x.PaymentMethodId == (int)PaymentMethodEnum.POSTBILL).OrderByDescending(x => x.DateSold).ToList();

            var gravm = new GuestRoomAccountViewModel
            {
                Room = mainGuestRoom.Room,
                Guest = guest,
                RoomId = mainGuestRoom.Room.Id,
                PaymentTypeId = 0,
                Rooms = guest.GuestRooms.Select(x => x.Room).ToList(),
                GuestRoomAccount = new GuestRoomAccount { Amount = decimal.Zero },
                ItemmisedItems = allItemisedItems

            };

            return View(gravm);

        }
    }
}