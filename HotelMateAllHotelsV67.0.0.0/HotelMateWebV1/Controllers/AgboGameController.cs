using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelMateWebV1.Models;
using Agbo21.Dal;
using Entitities;

namespace HotelMateWebV1.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize]
    public class AgboGameController : Controller
    {
        private UnitOfWork unitOfWork = null;

        public AgboGameController()
        {
            unitOfWork = new UnitOfWork();
        }

        private User GetUser(string userName)
        {
            return unitOfWork.UserRepository.Get().FirstOrDefault(x => x.UserName.ToUpper() == userName.ToUpper()); 
        }

        [HttpGet]
        public ActionResult Index(bool? userBalanceFailure)
        {
            var currentUser = GetUser(User.Identity.Name);

            GameViewModel gvm = new GameViewModel();

            if(userBalanceFailure.HasValue && userBalanceFailure.Value)
            {
                gvm.UserBalanceFailure = userBalanceFailure.Value;

                var currentGames = unitOfWork.GamePlayingNowRepository.Get().Where(gpn => gpn.GameStage == 0).ToList();

                if (currentGames.Count > 0)
                {
                    return RedirectToAction("ExistingGameAdmin", "AgboGame", new { userBalanceFailure = true });
                }

            }

            gvm.CurrentUserName = currentUser.UserName;
            gvm.User = currentUser;
            gvm.CurrentUserName = currentUser.UserName;
            gvm.CurrentUserBalance = currentUser.RealMoneyBalance;
            gvm.NumberOfPlayers = 2;
            gvm.GameStake = 200;
            return View(gvm);
        }

        //search_field
        [HttpPost]
        public ActionResult JoinExistingGame(int? search_field)
        {
            var currentUser = GetUser(User.Identity.Name);

            GameViewModel gvm = new GameViewModel();

            if (search_field.HasValue)
            {
                var gameStake = decimal.Zero;

                var existingGame = unitOfWork.GameRepository.GetByID(search_field.Value);

                var thisIsAValidGame = false;

                if (existingGame != null)
                {
                    decimal.TryParse(existingGame.ModifiedBy, out gameStake);
                    var gpn = unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == existingGame.Id && x.GameStage == 0);
                    if (gpn != null)
                    {
                        thisIsAValidGame = true;
                    }
                }

                if (!thisIsAValidGame)
                {
                    ModelState.AddModelError("", "Sorry this game cannot be joined at this stage, as the game is already in progress.");
                }

                if (gameStake > 0 && gameStake > currentUser.RealMoneyBalance)
                {
                    ModelState.AddModelError("", "Sorry You do not have enough money in your account to join this game. Balance : £" + currentUser.RealMoneyBalance);
                }

                if (ModelState.IsValid)
                {
                    string str = "Home";

                    if(existingGame.CreatedBy.StartsWith("AGGARA"))
                    {
                        str = "Aggara";
                    }

                    return RedirectToAction("StartNewGame", str, new { id = search_field.Value });
                }
            }
            else
            {
                ModelState.AddModelError("", "Pin number entered not recognised");
            }

            gvm.CurrentUserName = currentUser.UserName;
            gvm.User = currentUser;
            gvm.CurrentUserName = currentUser.UserName;
            gvm.CurrentUserBalance = currentUser.RealMoneyBalance;
            gvm.NumberOfPlayers = 2;
            gvm.GameStake = 200;
            return View(gvm);
        }
              

        [HttpPost]
        public ActionResult ExistingGameAdmin(bool? userBalanceFailure, string search_field)
        {
            var currentUser = GetUser(User.Identity.Name);

            GameViewModel gvm = new GameViewModel();

            if (userBalanceFailure.HasValue && userBalanceFailure.Value)
            {
                gvm.UserBalanceFailure = userBalanceFailure.Value;
            }            

            var currentGames = unitOfWork.GamePlayingNowRepository.Get().Where(gpn => gpn.GameStage == 0 && gpn.Game.IsActive).ToList();

            if(currentGames.Count > 0)
            {
               //var exactSerach = currentGames.Where(x  => x.
            }
            

            gvm.CurrentUserName = currentUser.UserName;
            gvm.User = currentUser;
            gvm.CurrentUserName = currentUser.UserName;
            gvm.CurrentUserBalance = currentUser.RealMoneyBalance;
            gvm.NumberOfPlayers = 2;
            gvm.GameStake = 200;
            gvm.CurrentGames = currentGames;
            return View(gvm);
        }

        [HttpGet]
        public ActionResult ExistingGameAdmin(bool? userBalanceFailure)
        {
            var currentUser = GetUser(User.Identity.Name);

            GameViewModel gvm = new GameViewModel();

            if (userBalanceFailure.HasValue && userBalanceFailure.Value)
            {
                gvm.UserBalanceFailure = userBalanceFailure.Value;
            }

            var currentGames = unitOfWork.GamePlayingNowRepository.Get().Where(gpn => gpn.GameStage == 0).ToList();
            
            gvm.CurrentUserName = currentUser.UserName;
            gvm.User = currentUser;
            gvm.CurrentUserName = currentUser.UserName;
            gvm.CurrentUserBalance = currentUser.RealMoneyBalance;
            gvm.NumberOfPlayers = 2;
            gvm.GameStake = 200;
            gvm.CurrentGames = currentGames;
            return View(gvm);
        }

        [HttpPost]
        public ActionResult Index(GameViewModel gvm, int? GameType)
        {
            var currentUser = GetUser(User.Identity.Name);

            if (gvm.GameStake > currentUser.RealMoneyBalance)
            {
                ModelState.AddModelError("", "You do not have enough money in your account to place the stake. Balance : £" + currentUser.RealMoneyBalance);
            }

            if (gvm.GameStake > 20)
            {
                ModelState.AddModelError("", "We are sorry, at the moment the highest you can stake on a game is £20 or equivalent.");
            }

            if (gvm.GameStake < 1)
            {
                ModelState.AddModelError("", "We are sorry, the minimum you can stake on a game is £1.");
            }

            if (ModelState.IsValid)
            {
                if (gvm.NumberOfPlayers < 2 || gvm.NumberOfPlayers > 3)
                    gvm.NumberOfPlayers = 2;

                if(GameType.Value == 1)
                    return RedirectToAction("StartNewGame", "Home", new { numOfPlayers = gvm.NumberOfPlayers, stake = gvm.GameStake });
                else
                    return RedirectToAction("StartNewGame", "Aggara", new { numOfPlayers = gvm.NumberOfPlayers, stake = gvm.GameStake });

            }

            gvm.CurrentUserName = currentUser.UserName;
            gvm.User = currentUser;
            gvm.CurrentUserName = currentUser.UserName;
            gvm.CurrentUserBalance = currentUser.RealMoneyBalance;
            gvm.NumberOfPlayers = 2;
            gvm.GameStake = 200;
            return View(gvm);
        }

    }
}
