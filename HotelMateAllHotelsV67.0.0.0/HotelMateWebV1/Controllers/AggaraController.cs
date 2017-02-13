using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HotelMateWebV1.Models;
using Agbo21.Dal;
using Entitities;
using HotelMateWebV1.Helpers;
using System.Collections;

namespace HotelMateWebV1.Controllers
{
    [System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    [Authorize]
    public class AggaraController : Controller
    {
        private UnitOfWork unitOfWork = null;

        public AggaraController()
        {
            unitOfWork = new UnitOfWork();
        }

        public ActionResult IndexNew()
        {
            return View();
        }

        public ActionResult Index()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }

            var indexViewModel = new IndexViewModel21();
            var currentUser = GetUser(User.Identity.Name);
            indexViewModel.CurrentUserName = currentUser.UserName;
            indexViewModel.CurrentUserBalance = currentUser.RealMoneyBalance;
            indexViewModel.LiveGamesCount = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.GameStage == 0).Count();
            return View(indexViewModel);
        }

        private GamePlayingNow GetGamePlayingNowByGameId(int id)
        {
            return unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == id);
        }

        private List<GameCard> GetGameCardByGameByGameID(int id)
        {
            return unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == id).ToList();
        }

        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        private User GetUser(string userName)
        {
            return unitOfWork.UserRepository.Get().FirstOrDefault(x => x.UserName.ToUpper() == userName.ToUpper());
        }

        public ActionResult JoinExistingGame(int? id, int? userId)
        {
            var userName = User.Identity.Name;

            var indexViewModel = new IndexViewModel21();

            indexViewModel.LiveGames = unitOfWork.GameRepository.Get().Where(x => x.IsActive && x.GameUsers.Count == 1).ToList();

            var currentUser = GetUser(userName);

            //first check if this users are in a 
            //Start a new Game and Register the users to this game;
            //Select the last game they were involved in

            Game21 newGame = unitOfWork.GameRepository.GetByID(id.Value);

            var gameCards = GetGameCardByGameByGameID(newGame.Id);

            var gameUsers = newGame.GameUsers.ToList();

            int count = gameCards.Count;

            List<GameCard> listGameCard = new List<GameCard>();

            if (count == 0) // Create game cards for new game
            {
                newGame = CreateNewGameCardsForGame(gameUsers.ToArray(), out listGameCard);
                //gameCards = GetGameCardByGameByGameID(newGame.Id);
            }

            User agboFloor = GetUser("AGBO FLOOR");

            var agboFloorGameUser = new GameUser { User = agboFloor };

            gameUsers.Add(agboFloorGameUser);

            listGameCard = GetGameCardByGameByGameID(newGame.Id);

            List<CardOwner> cardOwners = new List<CardOwner>();

            gameUsers.ForEach(x => x.Game21 = newGame);

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).Select(x => x.Card).ToList();
                List<CardModel> cmLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

                    if (gu.OrderSequence != 1)
                    {
                        action = "";
                    }

                    cmLst.Add(new CardModel { ActionUrl = action, Card = c });
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gu.User.Id != agboFloor.Id && gu.User.Id != currentUser.Id)
                {
                    //hide this users cards
                    cmLst.ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                cardOwners.Add(new CardOwner { GameStake = newGame.ModifiedBy, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = new List<CardModel>() });

                int counter = cds.Count;

                gu.User.UsersPlayingStack = cds;
            }

            if (Request.IsAjaxRequest())
            {
                string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

                string topMessage = string.Empty;

                return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {

                indexViewModel.CardOwners = cardOwners;

                indexViewModel.GameId = newGame.Id;

                var globalUrlAction = Url.Action("GetMyCards", "Aggara", new { gameId = newGame.Id }).ToString();

                indexViewModel.GlobalUrlAction = globalUrlAction;

                indexViewModel.GameUsers = gameUsers;

                indexViewModel.CurrentUserName = userName;

                return View("StartNewGame", indexViewModel);
            }
        }

        public ActionResult ZeroBalance()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }

            var indexViewModel = new IndexViewModel21();
            var currentUser = GetUser(User.Identity.Name);
            indexViewModel.CurrentUserName = currentUser.UserName;
            indexViewModel.CurrentUserBalance = currentUser.RealMoneyBalance;
            indexViewModel.LiveGamesCount = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.GameStage == 0).Count();
            return View(indexViewModel);
        }

        public virtual GameCard PlayFirstHand(GameCard[] cardStackers, GameCard[] usersStack)
        {
            GameCard removeFromAces = AcesPresent(usersStack.ToList());

            if (removeFromAces != null)
                return removeFromAces;

            Dictionary<decimal, GameCard> Dict = new Dictionary<decimal, GameCard>();

            foreach (GameCard Card in usersStack)//check for highest value
            {
                int Total = GetStakeTotalWithoutCard(usersStack.ToList(), Card);

                try
                {
                    Dict.Add(IsBreakingCardUsable(Total), Card);
                }
                catch
                { }
            }

            foreach (GameCard Card in usersStack)
            {
                int Total = GetStakeTotalWithoutCard(usersStack.ToList(), Card);

                foreach (GameCard Card1 in cardStackers)
                {
                    try
                    {
                        Dict.Add(IsBreakingCardUsable(Total, Card1), Card);
                    }
                    catch
                    {
                    }
                }
            }

            var returnCard = GetHighestProbability(Dict);

            return returnCard;
        }

        [HttpGet]
        public ActionResult PlayAgainstComputerComputerStart(int? prevId)
        {
            var userName = User.Identity.Name;

            var indexViewModel = new IndexViewModel21();

            indexViewModel.LiveGames = unitOfWork.GameRepository.Get().Where(x => x.IsActive && x.GameUsers.Count == 1).ToList();

            var currentUser = GetUser(userName);

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            var userComputer = GetUser("CLOUD");

            var gameUser1 = new GameUser { User = currentUser };

            var gameUser2 = new GameUser { User = userComputer };

            var gameUsers = new List<GameUser> { gameUser1, gameUser2 };

            var ids = gameUsers.Select(x => x.User.Id).ToList();

            Game21 game = null;

            Game21 newGame = null;

            var allGameUsers = unitOfWork.GameRepository.Get().Where(x => x.IsActive).ToList();

            var prevgameuser = allGameUsers.Select(x => x.GameUsers.Where(y => y.User.Id == currentUser.Id)).FirstOrDefault();

            var computersTurnToPlayFirst = true;

            var previousGame = unitOfWork.GameRepository.GetByID(prevId.Value);

            var users = previousGame.GameUsers.Select(x => x.User).ToList();

            var computerPlayer = users.Where(x => x.Id == userComputer.Id && x.playingSeq > 1).FirstOrDefault();

            computersTurnToPlayFirst = computerPlayer != null;

            game = CreateNewGameWithUserId(gameUsers.ToArray(), false, 2);

            game.Status = "USERTOPLAY";

            unitOfWork.GameRepository.Update(game);

            unitOfWork.Save();

            var lastGame = unitOfWork.GameRepository.GetByID(game.Id);

            gameUsers.Clear();

            gameUsers.AddRange(lastGame.GameUsers);

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == lastGame.Id).FirstOrDefault();

            gpn.GameStage = 1;

            gpn.ValueNum = 2;

            unitOfWork.GamePlayingNowRepository.Update(gpn);

            unitOfWork.Save();

            newGame = lastGame;

            var gameCards = GetGameCardByGameByGameID(newGame.Id);

            int count = gameCards.Count;

            List<GameCard> listGameCard = new List<GameCard>();

            if (count == 0) // Create game cards for new game
            {
                newGame = CreateNewGameCardsForGame(gameUsers.ToArray(), out listGameCard);
            }           

            listGameCard = GetGameCardByGameByGameID(newGame.Id);

            GameCard firstCardPlayedByComputer = null;

            if (computersTurnToPlayFirst)
            {
                GameCard pfc = null;

                var sec = DateTime.Now.Second;

                if (sec < 3)
                {                    
                    pfc = listGameCard.Where(x => x.User.Id == userComputer.Id).OrderByDescending(x => x.Card.CardNumberValue).FirstOrDefault();
                }
                else if(sec >= 3 && sec <= 5)
                {
                    pfc = listGameCard.Where(x => x.User.Id == userComputer.Id).OrderBy(x => x.Card.CardNumberValue).FirstOrDefault();
                }
                else
                {
                    pfc = PlayFirstHand(listGameCard.ToArray(), listGameCard.Where(x => x.User.Id == userComputer.Id).ToArray());
                }

                pfc.IsABlank = false;// Change this property name
                pfc.ShowNumberedSide = true;
                pfc.TradingFloorCard = true;
                pfc.ModifiedDate = DateTime.Now;
                unitOfWork.GameCardRepository.Update(pfc);
                newGame.CreatedBy = computerPlayer.UserName;
                unitOfWork.GameRepository.Update(newGame);
                var thisComputerGameUser = gameUsers.FirstOrDefault(x => x.User.Id == computerPlayer.Id);
                thisComputerGameUser.ModifiedBy = pfc.Card.Id.ToString();
                unitOfWork.GameUserRepository.Update(thisComputerGameUser);
                unitOfWork.Save();
                firstCardPlayedByComputer = pfc;
            }

            listGameCard = GetGameCardByGameByGameID(newGame.Id);

            var traders = listGameCard.Where(x => x.TradingFloorCard).ToList();

            List<CardOwner> cardOwners = new List<CardOwner>();

            gameUsers.ForEach(x => x.Game21 = newGame);

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id && !x.TradingFloorCard).Select(x => x.Card).ToList();

                List<CardModel> cmLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    var action = Url.Action("GameCardClickedComputerGame", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

                    if (gu.OrderSequence != 1)
                    {
                        action = "";
                    }

                    cmLst.Add(new CardModel { ActionUrl = action, Card = c, IsStarCard = !c.IsABlank });
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                var thisUserIsPlayingNow = false;

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                    thisUserIsPlayingNow = true;
                }

                if (gu.User.Id != currentUser.Id) 
                {
                    //hide this users cards
                    cmLst.ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                var cardsPlayed = new List<CardModel>();

                if (firstCardPlayedByComputer != null && gu.User.Id == userComputer.Id)
                {
                    cardsPlayed.Add(new CardModel { ActionUrl = "", Card = firstCardPlayedByComputer.Card, IsStarCard = !firstCardPlayedByComputer.IsABlank });
                }

                var playingNowFlashMessage = string.Empty;

                if (thisUserIsPlayingNow)
                {
                    playingNowFlashMessage = "Please click on any of these cards to play them.";
                }

                thisUserIsPlayingNow = false;

                cardOwners.Add(new CardOwner
                {
                    GameStake = newGame.ModifiedBy,
                    CanShowFlashMessage = thisUserIsPlayingNow,
                    CanShowFlashMessageMessage = playingNowFlashMessage,
                    WhoIsPlayingMessage = whoIsPlayingMessage,
                    PlayingNow = playingNow,
                    Owner = gu.User,
                    CardModelsPlayingStack = cmLst,
                    CardModelsPlayedStack = cardsPlayed
                });

                int counter = cds.Count;

                gu.User.UsersPlayingStack = cds;
            }

            if (Request.IsAjaxRequest())
            {

                string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

                string topMessage = string.Empty;

                return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                indexViewModel.CardOwners = cardOwners;

                indexViewModel.GameId = newGame.Id;

                indexViewModel.GameUsers = gameUsers;

                indexViewModel.CurrentUserName = userName;

                indexViewModel.CurrentUserBalance = playBalance;

                var globalUrlAction = Url.Action("GetMyCardsComputerGame", "Aggara", new { gameId = newGame.Id }).ToString();

                indexViewModel.GlobalUrlAction = globalUrlAction;

                indexViewModel.DontShowDiscussionDiv = true;

                return View("StartNewGame", indexViewModel);
            }
        }

        [HttpGet]
        public ActionResult PlayAgainstComputer()
        {
            var userName = User.Identity.Name;

            //var userName = User.Identity.Name;

            var indexViewModel = new IndexViewModel21();

            indexViewModel.LiveGames = unitOfWork.GameRepository.Get().Where(x => x.IsActive && x.GameUsers.Count == 1).ToList();

            var currentUser = GetUser(userName);

            var establishedUser = currentUser.GameName == "ESTABLISHEDUSER";

            if (!establishedUser)
            {
                currentUser.GameName = "ESTABLISHEDUSER";
                unitOfWork.UserRepository.Update(currentUser);
                unitOfWork.Save();
            }

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            var user2 = GetUser("CLOUD");

            var gameUser1 = new GameUser { User = currentUser };

            var gameUser2 = new GameUser { User = user2 };

            var gameUsers = new List<GameUser> { gameUser1, gameUser2 };

            var ids = gameUsers.Select(x => x.User.Id).ToList();

            Game21 game = CreateNewGameWithUserId(gameUsers.ToArray(), false, 2);

            var lastGame = unitOfWork.GameRepository.GetByID(game.Id);

            gameUsers.Clear();

            gameUsers.AddRange(lastGame.GameUsers);

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == lastGame.Id).FirstOrDefault();

            gpn.GameStage = 1;

            unitOfWork.GamePlayingNowRepository.Update(gpn);

            unitOfWork.Save();

            Game21 newGame = lastGame;

            var gameCards = GetGameCardByGameByGameID(newGame.Id);

            int count = gameCards.Count;

            List<GameCard> listGameCard = new List<GameCard>();

            if (count == 0) // Create game cards for new game
            {
                newGame = CreateNewGameCardsForGame(gameUsers.ToArray(), out listGameCard);
            }


            newGame.CreatedBy = currentUser.UserName;
            unitOfWork.GameRepository.Update(newGame);
            unitOfWork.Save();

            listGameCard = GetGameCardByGameByGameID(newGame.Id);

            List<CardOwner> cardOwners = new List<CardOwner>();

            gameUsers.ForEach(x => x.Game21 = newGame);

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).Select(x => x.Card).ToList();
                List<CardModel> cmLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    var action = Url.Action("GameCardClickedComputerGame", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

                    if (gu.OrderSequence != 1)
                    {
                        action = "";
                    }

                    cmLst.Add(new CardModel { ActionUrl = action, Card = c });
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                var thisUserIsPlayingNow = false;

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                    thisUserIsPlayingNow = true;
                }

                if (gu.User.Id != currentUser.Id)//Hide the Other Users Card Here
                {
                    //hide this users cards
                    cmLst.ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                var playingNowFlashMessage = string.Empty;

                if (thisUserIsPlayingNow)
                {
                    playingNowFlashMessage = "Please click on any of these cards to play them.";
                }

                if (establishedUser)
                {
                    thisUserIsPlayingNow = false;
                }

                cardOwners.Add(new CardOwner { GameStake = newGame.ModifiedBy, CanShowFlashMessage = thisUserIsPlayingNow, CanShowFlashMessageMessage = playingNowFlashMessage, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = new List<CardModel>() });

                int counter = cds.Count;

                gu.User.UsersPlayingStack = cds;
            }

            if (Request.IsAjaxRequest())
            {
                string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

                string topMessage = string.Empty;

                return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);  
            }
            else
            {

                indexViewModel.CardOwners = cardOwners;

                indexViewModel.GameId = newGame.Id;

                indexViewModel.GameUsers = gameUsers;

                indexViewModel.CurrentUserName = userName;

                indexViewModel.CurrentUserBalance = playBalance;

                var globalUrlAction = Url.Action("GetMyCardsComputerGame", "Aggara", new { gameId = newGame.Id }).ToString();

                indexViewModel.GlobalUrlAction = globalUrlAction;

                indexViewModel.DontShowDiscussionDiv = true;

                return View("StartNewGame", indexViewModel);
            }
        }

        public ActionResult StartNewGame(int? id, int? previousGameId, int? userId, int? numOfPlayers, decimal? stake)
        {

            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            if (previousGameId.HasValue && previousGameId.Value > 0 && userId.HasValue && userId.Value > 0)
            {
                var previousGame = unitOfWork.GameRepository.GetByID(previousGameId);

                if (previousGame.Status.StartsWith("NEWGAMECREATED"))
                {
                    var sp = previousGame.Status.Split('*');
                    var lastGame = previousGame;
                    lastGame.Status = "CLOSED";
                    lastGame.TimeEnded = DateTime.Now;
                    lastGame.IsActive = false;
                    var lastGameGpn = unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == lastGame.Id);
                    lastGameGpn.GameStage = 999;
                    lastGameGpn.ValueNum = 999;
                    unitOfWork.GamePlayingNowRepository.Update(lastGameGpn);
                    unitOfWork.GameRepository.Update(lastGame);
                    unitOfWork.Save();
                    return RedirectToAction("JoinExistingGame", new { id = int.Parse(sp[1]), userId = currentUser.Id });
                }

                var indexViewModel = new IndexViewModel21();

                indexViewModel.LiveGames = unitOfWork.GameRepository.Get().Where(x => x.IsActive && x.GameUsers.Count == 1).ToList();

                var users = previousGame.GameUsers.Select(x => x.User).ToList();

                var orderedList = users.Where(x => x.playingSeq > 1).ToList();

                var takeToTheBack = users.Where(x => x.playingSeq == 1).ToList();

                var gameUsers = new List<GameUser>();

                int seq = 1;

                foreach (var u in orderedList)
                {
                    gameUsers.Add(new GameUser { User = u, OrderSequence = seq });
                    seq++;
                }

                foreach (var u in takeToTheBack)
                {
                    gameUsers.Add(new GameUser { User = u, OrderSequence = seq });
                    seq++;
                }

                //if any involved in the last game does not have any money
                var presentUsers = gameUsers.Select(x => x.User).ToList();

                var gameStake = decimal.Zero;

                decimal.TryParse(previousGame.ModifiedBy, out gameStake);

                var lastGameGpnPrev = unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == previousGame.Id);
                lastGameGpnPrev.GameStage = 999;
                lastGameGpnPrev.ValueNum = 999;
                lastGameGpnPrev.IsActive = false;
                unitOfWork.GamePlayingNowRepository.Update(lastGameGpnPrev);
                unitOfWork.Save();

                if (presentUsers.Any(x => x.RealMoneyBalance < gameStake))
                {
                    return RedirectToAction("Index", "AgboGame", new { userBalanceFailure = true });
                }

                if (!stake.HasValue)
                {
                    stake = 1;
                }

                Game21 newGame = CreateNewGameWithUserId(gameUsers.ToArray(), false, users.Count, stake.Value);

                previousGame.Status = "NEWGAMECREATED*" + newGame.Id.ToString();
                previousGame.IsActive = false;
                previousGame.TimeEnded = DateTime.Now;
                previousGame.IsActive = false;
                unitOfWork.GameRepository.Update(previousGame);
                unitOfWork.Save();

                gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == newGame.Id).ToList();

                var gameCards = GetGameCardByGameByGameID(newGame.Id);

                int count = gameCards.Count;

                List<GameCard> listGameCard = new List<GameCard>();

                if (count == 0) // Create game cards for new game
                {
                    newGame = CreateNewGameCardsForGame(gameUsers.ToArray(), out listGameCard);
                }                 

                listGameCard = GetGameCardByGameByGameID(newGame.Id);

                List<CardOwner> cardOwners = new List<CardOwner>();

                gameUsers.ForEach(x => x.Game21 = newGame);

                var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

                foreach (var gu in gameUsers)
                {
                    var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).Select(x => x.Card).ToList();
                    List<CardModel> cmLst = new List<CardModel>();

                    foreach (var c in cds)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

                        if (gu.OrderSequence != 1)
                        {
                            action = "";
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c });
                    }

                    var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                    string playingNow = @"border:5px Solid Blue;";

                    if (gu.OrderSequence == 1)
                    {
                        playingNow = @"border:5px Solid Blue;";
                    }

                    whoIsPlayingMessage = "Awaiting the other players to join game.";

                    //if (gu.User.Id != currentUser.Id)
                    //{
                        //hide this users cards

                        cmLst.ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    //}

                    cardOwners.Add(new CardOwner { GameStake = newGame.ModifiedBy, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = new List<CardModel>() });

                    int counter = cds.Count;

                    gu.User.UsersPlayingStack = cds;
                }

                if (Request.IsAjaxRequest())
                {
                    string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

                    string topMessage = string.Empty;

                    return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    indexViewModel.CardOwners = cardOwners;

                    indexViewModel.CurrentUserName = currentUser.UserName;

                    indexViewModel.GameId = newGame.Id;

                    var globalUrlAction = Url.Action("GetMyCards", "Aggara", new { gameId = newGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlAction;

                    indexViewModel.GameUsers = gameUsers;

                    return View(indexViewModel);
                }
            }

            if (!id.HasValue)
            {
                var userName = User.Identity.Name;

                var indexViewModel = new IndexViewModel21();

                indexViewModel.LiveGames = unitOfWork.GameRepository.Get().Where(x => x.IsActive && x.GameUsers.Count == 1).ToList();

                var user1 = GetUser(userName);

                var gameUser1 = new GameUser { User = user1 };

                var gameUsers = new List<GameUser> { gameUser1 };

                var ids = gameUsers.Select(x => x.User.Id).ToList();

                Game21 newGame = null;

                newGame = unitOfWork.GameUserRepository.Get().Where(x => x.User.Id == user1.Id && x.Game21.Status == "LIVE").Select(x => x.Game21).FirstOrDefault();

                if (newGame == null)
                {
                    if (!stake.HasValue)
                    {
                        stake = 1;
                    }

                    newGame = CreateNewGameWithUserId(gameUsers.ToArray(), true, numOfPlayers.Value, stake.Value);
                }

                newGame.CreatedBy = "AGGARA";
                unitOfWork.GameRepository.Update(newGame);
                unitOfWork.Save();

                indexViewModel.GameUsers = gameUsers;

                List<CardOwner> cardOwners = new List<CardOwner>();

                cardOwners.Add(new CardOwner { GameStake = newGame.ModifiedBy, Owner = gameUser1.User, CardModelsPlayingStack = new List<CardModel>(), CardModelsPlayedStack = new List<CardModel>() });


                if (Request.IsAjaxRequest())
                {
                    string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

                    string topMessage = string.Empty;

                    return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    indexViewModel.CardOwners = cardOwners;

                    indexViewModel.GameId = newGame.Id;

                    var globalUrlAction = Url.Action("GetMyCards", "Aggara", new { gameId = newGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlAction;

                    indexViewModel.CurrentUserName = userName;

                    return View(indexViewModel);
                }
            }
            else
            {
                var userName = User.Identity.Name;

                var indexViewModel = new IndexViewModel21();

                indexViewModel.LiveGames = unitOfWork.GameRepository.Get().Where(x => x.IsActive && x.GameUsers.Count == 1).ToList();

                var gameUser1 = new GameUser { User = currentUser };

                var gameUsers = new List<GameUser> { gameUser1 };

                //first check if this users are in a 
                //Start a new Game and Register the users to this game;
                //Select the last game they were involved in

                var lastGame = unitOfWork.GameRepository.GetByID(id.Value);

                gameUser1.OrderSequence = lastGame.GameUsers.Count + 1;

                lastGame.GameUsers.Add(gameUser1);

                unitOfWork.GameRepository.Update(lastGame);

                currentUser.playingSeq = gameUser1.OrderSequence;

                unitOfWork.UserRepository.Update(currentUser);

                lastGame.GameUsers.ToList().ForEach(x => x.CreatedDate = DateTime.Now);
                lastGame.GameUsers.ToList().ForEach(x => x.ModifiedDate = DateTime.Now);

                if (lastGame.GameCards != null)
                {
                    lastGame.GameCards.ToList().ForEach(x => x.User.CreatedDate = DateTime.Now);
                    lastGame.GameCards.ToList().ForEach(x => x.User.ModifiedDate = DateTime.Now);
                    lastGame.GameCards.ToList().ForEach(x => x.User.LastLoggedInDate = DateTime.Now);
                    lastGame.GameCards.ToList().ForEach(x => x.User.StartDate = DateTime.Now);
                }

                unitOfWork.Save();

                //if any involved in the last game does not have any money
                var presentUsers = unitOfWork.GameRepository.GetByID(lastGame.Id).GameUsers.Select(x => x.User).ToList();
                var gameStake = decimal.Zero;
                decimal.TryParse(lastGame.ModifiedBy, out gameStake);

                if (presentUsers.Any(x => x.RealMoneyBalance < gameStake))
                {
                    lastGame.Status = "CLOSED";
                    lastGame.TimeEnded = DateTime.Now;
                    lastGame.IsActive = false;
                    var lastGameGpn = unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == lastGame.Id);
                    lastGameGpn.GameStage = 999;
                    lastGameGpn.ValueNum = 999;
                    unitOfWork.GamePlayingNowRepository.Update(lastGameGpn);
                    unitOfWork.GameRepository.Update(lastGame);
                    unitOfWork.Save();
                    return RedirectToAction("Index", "AgboGame", new { userBalanceFailure = true });
                }

                var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == lastGame.Id).FirstOrDefault();

                if (gpn.ValueNum != lastGame.GameUsers.Count)
                {
                    indexViewModel.CardOwners = null;

                    indexViewModel.GameId = lastGame.Id;

                    indexViewModel.GameUsers = gameUsers;

                    var globalUrlActionQuick = Url.Action("GetMyCards", "Aggara", new { gameId = lastGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlActionQuick;

                    return View(indexViewModel);
                }

                lastGame = unitOfWork.GameRepository.GetByID(lastGame.Id);

                gameUsers.Clear();

                gameUsers.AddRange(lastGame.GameUsers);

                gpn.GameStage = 1;

                unitOfWork.GamePlayingNowRepository.Update(gpn);

                unitOfWork.Save();

                Game21 newGame = lastGame;

                var gameCards = GetGameCardByGameByGameID(newGame.Id);

                int count = gameCards.Count;

                List<GameCard> listGameCard = new List<GameCard>();

                if (count == 0) // Create game cards for new game
                {
                    if (!stake.HasValue)
                    {
                        stake = 1;
                    }

                    newGame = CreateNewGameCardsForGame(gameUsers.ToArray(), out listGameCard);               
                }

                newGame.CreatedBy = "AGGARA";
                unitOfWork.GameRepository.Update(newGame);
                unitOfWork.Save();

                listGameCard = GetGameCardByGameByGameID(newGame.Id);

                List<CardOwner> cardOwners = new List<CardOwner>();

                gameUsers.ForEach(x => x.Game21 = newGame);

                var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

                foreach (var gu in gameUsers)
                {
                    var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).Select(x => x.Card).ToList();
                    List<CardModel> cmLst = new List<CardModel>();

                    foreach (var c in cds)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

                        if (gu.OrderSequence != 1)
                        {
                            action = "";
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c });
                    }

                    var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                    string playingNow = @"border:5px Solid Red;";

                    if (gu.OrderSequence == 1)
                    {
                        playingNow = @"border:5px Solid Green;";
                    }

                    if (gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards
                        cmLst.ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }

                    cardOwners.Add(new CardOwner { GameStake = newGame.ModifiedBy, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = new List<CardModel>() });

                    int counter = cds.Count;

                    gu.User.UsersPlayingStack = cds;
                }

                if (Request.IsAjaxRequest())
                {
                    string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

                    string topMessage = string.Empty;

                    return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    indexViewModel.CardOwners = cardOwners;

                    indexViewModel.GameId = newGame.Id;

                    indexViewModel.GameUsers = gameUsers;

                    indexViewModel.CurrentUserName = userName;

                    var globalUrlAction = Url.Action("GetMyCards", "Aggara", new { gameId = newGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlAction;

                    return View(indexViewModel);
                }
            }
        }

        [HttpGet]
        public ActionResult GameContested(int? gameId, int? userId, int? cardId, bool? floorFinishClicked)
        {
            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

            gpn.Contested = true;

            unitOfWork.GamePlayingNowRepository.Update(gpn);

            unitOfWork.Save();

            var thisUser = unitOfWork.UserRepository.GetByID(userId.Value);

            var totalGameCount = 5 * gameUsers.Count;

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;           

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == totalGameCount;           

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            List<GameUser> supperWinner = new List<GameUser>();

            User gameWinner = null;

            if (gameIsOver)
            {
                var foundMutuError = gameUsers.Where(x => x.CreatedBy.StartsWith("MUTTUED")).Count() > 0;

                if (!foundMutuError)
                {
                    andTheWinnerIs = "Game contested, contest is invalid, all the players answered to the calling cards.";
                }
                else
                {
                    var usersWithoutMutu = gameUsers.Where(x => !x.CreatedBy.StartsWith("MUTTUED")).ToList();

                    if (usersWithoutMutu.Count > 0)
                    {
                        andTheWinnerIs = "Game contested, contest is valid, and the new winner is " + usersWithoutMutu.FirstOrDefault().User.UserName;
                        gameWinner = usersWithoutMutu.FirstOrDefault().User;
                    }
                }
            }

            if (gameWinner != null)
            {
                var possibleWinners = gameUsers.ToList();

                if (possibleWinners.Count > 2) //More than 2 players how do we handle mutu?
                {
                }
                else
                {

                    decimal currentStake = decimal.Zero;
                    decimal.TryParse(currentGame.ModifiedBy, out currentStake);

                    foreach (var possibleWinner in possibleWinners)
                    {
                        var thisCurrentWinner = unitOfWork.UserRepository.GetByID(possibleWinner.User.Id);

                        if (possibleWinner.User.Id == gameWinner.Id)
                        {
                            thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance + currentStake;
                        }
                        else
                        {
                            thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance - currentStake;
                        }

                        unitOfWork.UserRepository.Update(thisCurrentWinner);
                        unitOfWork.Save();
                    }
                }
            }

            string finishedPickingAction = string.Empty;
            string contestAction = string.Empty;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();               

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds.OrderByDescending(x => x.ModifiedDate))
                {
                    if (!c.TradingFloorCard)
                    {
                        cmLst.Add(new CardModel { ActionUrl = "", Card = c.Card });
                    }
                    else
                    {
                        var action = string.Empty;
                        cmPlayedLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "";
                finishedPickingAction = "";

                if (gameIsOver && gu.User.Id == currentUser.Id)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();

                    finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();
                }
                else
                {
                    if (gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards                   
                        cmLst.Where(x => !x.Card.ShowNumberedSide).ToList().ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                contestAction = string.Empty;
                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            if (gameWinner == null)
            {
                topMessage = "nomutu";
            }
            else
            {
                if (currentUser.Id == gameWinner.Id)
                {
                    topMessage = "otherplayermutu";
                }
                else
                {
                    topMessage = "mutu";
                }
            }

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
        }     

        [HttpGet]
        public ActionResult AgboFloorCardClickedComputerGame(int? gameId, int? userId, int? cardId, bool? floorFinishClicked)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("GetMyCardsComputerGameNonAjax", new { gameId = gameId, userId = userId, cardId = cardId, floorFinishClicked = floorFinishClicked });
            }

            var thisUserHasMuttude = false;

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gameCardPlayed = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == currentGame.Id && x.Card.Id == cardId).FirstOrDefault();

            var thisUser = unitOfWork.UserRepository.GetByID(userId.Value);

            if (gameCardPlayed != null)
            {
                gameCardPlayed.ShowNumberedSide = true;
                gameCardPlayed.User = thisUser;
                gameCardPlayed.ShowNumberedSide = true;
                unitOfWork.GameCardRepository.Update(gameCardPlayed);
                unitOfWork.Save();
            }

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var computerCanClickNow = false;

            if (floorFinishClicked.HasValue && floorFinishClicked.Value)
            {
                gameUsers.ToList().ForEach(y => y.Finished = 1);

                foreach (var gus in gameUsers)
                {
                    unitOfWork.GameUserRepository.Update(gus);
                    unitOfWork.Save();
                }

                computerCanClickNow = true;
            }

            gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var computer = GetUser("CLOUD");

            if (computerCanClickNow)
            {
                var compuserGameCards = listGameCard.Where(x => x.User.Id == computer.Id && !x.TradingFloorCard).ToList();

                var allPlayedCards = listGameCard.Where(x => x.TradingFloorCard).ToList();

                if (compuserGameCards.Count == 2)
                {
                    var onlineUserCard = allPlayedCards.Where(x => x.User.Id != computer.Id).FirstOrDefault();
                    var computerCard = allPlayedCards.Where(x => x.User.Id == computer.Id).FirstOrDefault();

                    if (computerCard.Card.Suit.Id == onlineUserCard.Card.Suit.Id && computerCard.Card.CardNumberValue > onlineUserCard.Card.CardNumberValue)
                    {
                        //Dont do anything as Computer has already had the chance to pick from floor
                    }
                    else
                    {
                        var gameCards = GetComputerSomeCards(compuserGameCards, listGameCard.Where(x => x.User.Id == AgboFloor.Id && !x.TradingFloorCard).ToList());

                        foreach (var gc in gameCards)
                        {
                            gc.User = computer;
                            unitOfWork.GameCardRepository.Update(gc);
                            unitOfWork.Save();
                        }
                    }
                }
            }

            listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && userId == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var grandTotal = -1;

            List<GameUser> supperWinner = new List<GameUser>();

            User gameWinner = null;

            var agboFloorCards = listGameCard.Where(x => x.User.Id == AgboFloor.Id).Count();

            if (agboFloorCards == 0)
            {
                gameIsOver = true;
            }

            if (gameIsOver)
            {
                bool winnerFound = false;

                var gameIsOverUserList = gameUsers.Where(x => x.User.Id != AgboFloor.Id).ToList();

                var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);

                if (gameFirstCard.User.Id == computer.Id)
                {
                    //var get all trading floor cards 
                    var tfcards = listGameCard.Where(x => x.TradingFloorCard).ToList();
                    if (tfcards.Where(x => x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).Count() < 2)
                    {
                        var possibleCheatCards = listGameCard.Where(x => x.User.Id == currentUser.Id && !x.TradingFloorCard && x.Status == "USERSSTACK").ToList();
                        if (possibleCheatCards.Any(x => x.Card.Suit.Id == gameFirstCard.Card.Suit.Id))
                        {
                            thisUserHasMuttude = true;
                        }
                    }
                }

                foreach (var guser in gameIsOverUserList)
                {
                    winnerFound = false;

                    var userCards = listGameCard.Where(x => x.User.Id == guser.User.Id && !x.TradingFloorCard).ToList();
                    var userTotal = CalculateUserTotal(userCards.ToArray(), ref winnerFound);

                    if (winnerFound)
                    {
                        supperWinner.Add(guser);
                    }

                    if (userTotal > 21)
                    {
                        userTotal = 0;
                    }

                    if (userTotal > grandTotal)
                    {
                        grandTotal = userTotal;
                        andTheWinnerIs = guser.User.UserName;
                        gameWinner = guser.User;
                    }
                    else if (userTotal == grandTotal)
                    {
                        andTheWinnerIs = "ITS A TIE";
                        gameWinner = null;
                    }
                }

                if (supperWinner.Count > 1)
                {
                    andTheWinnerIs = "ITS A TIE";
                    gameWinner = null;
                }
                else if (supperWinner.Count == 1)
                {
                    andTheWinnerIs = supperWinner.FirstOrDefault().User.UserName;
                    gameWinner = supperWinner.FirstOrDefault().User;
                }

                if (thisUserHasMuttude)
                {
                    andTheWinnerIs = "MUTTUED";
                    gameWinner = computer;
                }

                currentGame.IsActive = false;
                unitOfWork.GameRepository.Update(currentGame);
                unitOfWork.Save();
            }

            if (gameWinner != null)
            {
                var thisComputerWinner = GetUser("CLOUD");
                var thisCurrentWinner = unitOfWork.UserRepository.GetByID(currentUser.Id);

                decimal currentStake = decimal.Zero;
                decimal.TryParse(currentGame.ModifiedBy, out currentStake);

                if (gameWinner.Id == thisComputerWinner.Id)
                {
                    thisComputerWinner.RealMoneyBalance = thisComputerWinner.RealMoneyBalance + currentStake;
                    thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance - currentStake;
                }
                else
                {
                    thisComputerWinner.RealMoneyBalance = thisComputerWinner.RealMoneyBalance - currentStake;
                    thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance + currentStake;
                }

                unitOfWork.UserRepository.Update(thisComputerWinner);
                unitOfWork.Save();
                unitOfWork.UserRepository.Update(thisCurrentWinner);
                unitOfWork.Save();
            }

            var agboFirstCardIncluded = false;

            string finishedPickingAction = string.Empty;
            string contestAction = string.Empty;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                if (cds.Count == 0)
                    gameIsOver = true;

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClickedComputerGame", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClickedComputerGame", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClickedComputerGame", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        var action = string.Empty;
                        cmPlayedLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "Done!";
                var canClickContestText = "";

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gameIsOver)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "";

                    //currentGame.IsActive = false;
                    //unitOfWork.GameRepository.Update(currentGame);
                    //unitOfWork.Save();

                    if (gu.User.Id == AgboFloor.Id)
                    {
                        if (currentGame.Status == "LIVE")
                        {
                            currentGame.Status = "USERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputerComputerStart", "Aggara", new { prevId = currentGame.Id }).ToString();
                        }
                        else if (currentGame.Status == "USERTOPLAY")
                        {
                            currentGame.Status = "COMPUTERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputer", "Aggara").ToString();
                        }
                        else
                        {
                            currentGame.Status = "USERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputerComputerStart", "Aggara", new { prevId = currentGame.Id }).ToString();
                        }

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();
                    }
                }
                else
                {
                    contestAction = string.Empty;

                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards                   
                        cmLst.Where(x => !x.Card.ShowNumberedSide).ToList().ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickContestText = canClickContestText, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_CardSharer", cardOwners);
            string topMessage = string.Empty;

            var thisGuyCardOwner = cardOwners.FirstOrDefault(x => x.Owner.Id == AgboFloor.Id);

            if (thisGuyCardOwner.CanClickFinishText == "Done!" && !string.IsNullOrEmpty(thisGuyCardOwner.CanClickFinish))
                topMessage = "info";

            if (thisGuyCardOwner.GameIsOver && thisGuyCardOwner.CanClickFinishText.StartsWith("Start New Game"))
            {
                if (thisGuyCardOwner.TheWinnerIs == "ITS A TIE")
                {
                    topMessage = "success";
                }
                else if (thisGuyCardOwner.TheWinnerIs == "MUTTUED")
                {
                    topMessage = "mutu";
                }
                else
                {
                    if (currentUser.UserName == thisGuyCardOwner.TheWinnerIs)
                    {
                        topMessage = "warning";
                    }
                    else
                    {
                        topMessage = "error";
                    }
                }
            }

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);

            //return Json(new { TopView = topView }, JsonRequestBehavior.AllowGet);
        }

        private List<GameCard> GetComputerSomeCards(List<GameCard> computerUserCards, List<GameCard> agboFloorCards)
        {
            List<GameCard> returnCards = new List<GameCard>();

            var winnerFound = false;

            var userTotal = CalculateUserTotal(computerUserCards.ToArray(), ref winnerFound);

            if (userTotal > 19)
                return returnCards;

            if (winnerFound)
                return returnCards;

            var canpick = true;

            foreach (var c in agboFloorCards.OrderBy(x => x.OrderSeq))
            {
                if (!canpick) continue;

                var total = c.Card.CardNumberValue + userTotal;

                if (total <= 21)
                {
                    returnCards.Add(c);

                    userTotal += c.Card.CardNumberValue;

                    if (userTotal > 19)
                        return returnCards;
                }
                else
                {
                    canpick = false;
                }
            }


            return returnCards;
        }


        [HttpGet]
        public ActionResult AgboFloorCardClicked(int? gameId, int? userId, int? cardId, bool? floorFinishClicked)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("GetMyCardsNonAjax", new { gameId = gameId});
            }

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gameCardPlayed = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == currentGame.Id && x.Card.Id == cardId).FirstOrDefault();

            var thisUser = unitOfWork.UserRepository.GetByID(userId.Value);

            if (gameCardPlayed != null)
            {
                gameCardPlayed.ShowNumberedSide = true;
                gameCardPlayed.User = thisUser;
                gameCardPlayed.ShowNumberedSide = true;
                unitOfWork.GameCardRepository.Update(gameCardPlayed);
                unitOfWork.Save();
            }

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            if (floorFinishClicked.HasValue && floorFinishClicked.Value)
            {
                gameUsers.Where(x => x.User.Id == userId.Value).ToList().ForEach(y => y.Finished = 1);
                gameUsers = RearrangeGameSequence(gameUsers, currentGame.Id);
            }

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && userId == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var grandTotal = -1;

            List<GameUser> supperWinner = new List<GameUser>();

            User gameWinner = null;

            var agboFloorCards = listGameCard.Where(x => x.User.Id == AgboFloor.Id).Count();

            var globalOverride = false;

            if (agboFloorCards == 0)
            {
                gameIsOver = true;
                globalOverride = true;
            }

            if (gameIsOver)
            {
                bool winnerFound = false;

                var gameIsOverUserList = gameUsers.Where(x => x.User.Id != AgboFloor.Id).ToList();

                foreach (var guser in gameIsOverUserList)
                {
                    winnerFound = false;

                    var userCards = listGameCard.Where(x => x.User.Id == guser.User.Id && !x.TradingFloorCard).ToList();
                    var userTotal = CalculateUserTotal(userCards.ToArray(), ref winnerFound);

                    if (winnerFound)
                    {
                        supperWinner.Add(guser);
                    }

                    if (userTotal > 21)
                    {
                        userTotal = 0;
                    }

                    if (userTotal > grandTotal)
                    {
                        grandTotal = userTotal;
                        andTheWinnerIs = guser.User.UserName;
                        gameWinner = guser.User;
                    }
                    else if (userTotal == grandTotal)
                    {
                        andTheWinnerIs = "ITS A TIE";
                        gameWinner = null;
                    }
                }

                if (supperWinner.Count > 1)
                {
                    andTheWinnerIs = "ITS A TIE";
                    gameWinner = null;
                }
                else if (supperWinner.Count == 1)
                {
                    andTheWinnerIs = supperWinner.FirstOrDefault().User.UserName;
                    gameWinner = supperWinner.FirstOrDefault().User;
                }
            }

            if (gameWinner != null)
            {
                var possibleWinners = gameUsers.Where(x => x.User.Id != AgboFloor.Id).ToList();

                decimal currentStake = decimal.Zero;
                decimal.TryParse(currentGame.ModifiedBy, out currentStake);

                foreach (var possibleWinner in possibleWinners)
                {
                    var thisCurrentWinner = unitOfWork.UserRepository.GetByID(possibleWinner.User.Id);

                    if (possibleWinner.User.Id == gameWinner.Id)
                    {
                        thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance + currentStake;
                    }
                    else
                    {
                        thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance - currentStake;
                    }

                    unitOfWork.UserRepository.Update(thisCurrentWinner);
                    unitOfWork.Save();
                }
            }

            var agboFirstCardIncluded = false;

            string finishedPickingAction = string.Empty;
            string contestAction = string.Empty;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                if (cds.Count == 0)
                {
                    gameIsOver = true;
                }

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClicked", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClicked", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        var action = string.Empty;
                        cmPlayedLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "Done!";
                var canClickContestText = "";

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gameIsOver)
                {
                    var lastGame = currentGame;
                    lastGame.Status = "CLOSED";
                    lastGame.TimeEnded = DateTime.Now;
                    lastGame.IsActive = false;
                    var lastGameGpn = unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == lastGame.Id);
                    lastGameGpn.GameStage = 999;
                    lastGameGpn.ValueNum = 999;
                    lastGameGpn.IsActive = false;
                    unitOfWork.GamePlayingNowRepository.Update(lastGameGpn);
                    unitOfWork.GameRepository.Update(lastGame);
                    unitOfWork.Save();

                    if (globalOverride)
                    {
                        cmLst.ForEach(x => x.ActionUrl = "");
                        canClickFinishText = "Start New Game";
                        canClickContestText = "Contest Game";
                        contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();


                        if (gu.User.Id == AgboFloor.Id)
                            finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                    }

                    if (floorFinishClicked.HasValue && floorFinishClicked.Value)
                    {
                        cmLst.ForEach(x => x.ActionUrl = "");
                        canClickFinishText = "Start New Game";
                        canClickContestText = "Contest Game";
                        contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();

                        if (gu.User.Id == AgboFloor.Id)
                            finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();
                    }
                }
                else
                {
                    contestAction = string.Empty;

                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards                   
                        cmLst.Where(x => !x.Card.ShowNumberedSide).ToList().ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickContestText = canClickContestText, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_CardSharer", cardOwners);

            string topMessage = string.Empty;

            var thisGuyCardOwner = cardOwners.FirstOrDefault(x => x.Owner.Id == AgboFloor.Id);

            if (thisGuyCardOwner.CanClickFinishText == "Done!" && !string.IsNullOrEmpty(thisGuyCardOwner.CanClickFinish))
                topMessage = "info";

            if (thisGuyCardOwner.GameIsOver && thisGuyCardOwner.CanClickFinishText.StartsWith("Start New Game"))
            {
                if (currentUser.UserName == thisGuyCardOwner.TheWinnerIs)
                {
                    topMessage = "warning";
                }
                else
                {
                    topMessage = "error";
                }
            }

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);


            //return Json(new { TopView = topView }, JsonRequestBehavior.AllowGet);                      
        }       

        private int CalculateUserTotal(GameCard[] cards, ref bool WinnerFound)
        {
            int TotalValue = 0;
            int AceCard = 0;

            foreach (var gcard in cards)
            {
                if (gcard.Card.Rank.Name.StartsWith("Ace"))
                {
                    AceCard++;
                }

                if (AceCard == 2)
                {
                    WinnerFound = true;
                    return 21;
                }

                TotalValue += gcard.Card.CardNumberValue;
            }

            if (WinnerFound && cards.Length > 2)
            {
                WinnerFound = false;
            }

            return TotalValue;
        }

        [HttpGet]
        public ActionResult GetMyCardsNonAjax(int? gameId)
        {
            bool thisIsAContestedStage = false;

            return RedirectToAction("Index", "Home");

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var totalGameCount = 5 * gameUsers.Count;

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == totalGameCount;

            var gameController = string.Empty;

            if (gameIsOver)
            {
                var valueOfCardPlayed = 0;

                int.TryParse(currentGame.CreatedBy, out valueOfCardPlayed);

                if (valueOfCardPlayed > 0)
                {
                    gameController = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.Card.Id == valueOfCardPlayed).User.UserName;
                }
            }

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

            if (gpn.Contested)
            {
                thisIsAContestedStage = true;
            }

            List<GameUser> supperWinner = new List<GameUser>();

            User contestedGameWinner = null;

            string finishedPickingAction = string.Empty;

            string contestAction = string.Empty;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();

                List<CardModel> cmPlayedLst = new List<CardModel>();

                cds = cds.OrderByDescending(x => x.ModifiedDate).ToList();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (gu.OrderSequence != 1)
                        {
                            action = "";
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "";
                var canClickContestText = "";
                finishedPickingAction = "";
                contestAction = "";

                if (gameIsOver && gu.User.Id == currentUser.Id)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                }
                else
                {
                    contestAction = string.Empty;
                    if (gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards
                        cmLst.ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                if (thisIsAContestedStage)
                {
                    canClickContestText = string.Empty;
                    contestAction = string.Empty;
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickContestText = canClickContestText, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            try
            {
                contestedGameWinner = gameUsers.FirstOrDefault(x => !x.CreatedBy.StartsWith("MUTTUED")).User;
            }
            catch
            {
            }

            if (thisIsAContestedStage)
            {
                if (contestedGameWinner == null)
                {
                    topMessage = "nomutu";
                }
                else
                {
                    if (currentUser.Id == contestedGameWinner.Id)
                    {
                        topMessage = "otherplayermutu";
                    }
                    else
                    {
                        topMessage = "mutu";
                    }
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(gameController))
                {
                    if (currentUser.UserName == gameController)
                    {
                        topMessage = "warning";
                    }
                    else
                    {
                        topMessage = "error";
                    }
                }
            }

            IndexViewModel21 indexViewModel = new IndexViewModel21();

            indexViewModel.CardOwners = cardOwners;

            indexViewModel.GameId = currentGame.Id;

            indexViewModel.GameUsers = gameUsers;

            indexViewModel.CurrentUserName = currentUser.UserName;

            var globalUrlAction = Url.Action("GetMyCards", "Aggara", new { gameId = currentGame.Id }).ToString();

            indexViewModel.GlobalUrlAction = globalUrlAction;

            return View("StartNewGame", indexViewModel);

        }

        [HttpGet]
        public ActionResult GetMyCards(int? gameId)
        {
            bool thisIsAContestedStage = false;

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;  

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var totalGameCount = 5 * gameUsers.Count;

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == totalGameCount;

            var gameController = string.Empty;

            if (gameIsOver)
            {
                var valueOfCardPlayed = 0;

                int.TryParse(currentGame.CreatedBy, out valueOfCardPlayed);

                if (valueOfCardPlayed > 0)
                {
                    gameController = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.Card.Id == valueOfCardPlayed).User.UserName;
                }
            }

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();             

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();           

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

            if (gpn.Contested)
            {
                thisIsAContestedStage = true;
            }

            List<GameUser> supperWinner = new List<GameUser>();

            User contestedGameWinner = null;

            string finishedPickingAction = string.Empty;

            string contestAction = string.Empty;            

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();               

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();

                List<CardModel> cmPlayedLst = new List<CardModel>();

                cds = cds.OrderByDescending(x => x.ModifiedDate).ToList();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (gu.OrderSequence != 1)
                        {
                            action = "";
                        }                       

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "";
                var canClickContestText = "";
                finishedPickingAction = "";
                contestAction = "";

                if (gameIsOver && gu.User.Id == currentUser.Id)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                }
                else
                {
                    contestAction = string.Empty;
                    if (gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards
                        cmLst.ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                if (thisIsAContestedStage)
                {
                    canClickContestText = string.Empty;
                    contestAction = string.Empty;
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickContestText = canClickContestText, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            try
            {
                contestedGameWinner = gameUsers.FirstOrDefault(x => !x.CreatedBy.StartsWith("MUTTUED")).User;
            }
            catch
            {
            }

            if (thisIsAContestedStage)
            {
                if (contestedGameWinner == null)
                {
                    topMessage = "nomutu";
                }
                else
                {
                    if (currentUser.Id == contestedGameWinner.Id)
                    {
                        topMessage = "otherplayermutu";
                    }
                    else
                    {
                        topMessage = "mutu";
                    }
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(gameController))
                {
                    if (currentUser.UserName == gameController)
                    {
                        topMessage = "warning";
                    }
                    else
                    {
                        topMessage = "error";
                    }
                }
            }

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMyCardsComputerGame(int? gameId)
        {
            bool thisIsAContestedStage = false;

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var totalGameCount = 5 * gameUsers.Count;

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == totalGameCount;

            var gameController = string.Empty;

            if (gameIsOver)
            {
                var valueOfCardPlayed = 0;

                int.TryParse(currentGame.CreatedBy, out valueOfCardPlayed);

                if (valueOfCardPlayed > 0)
                {
                    gameController = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.Card.Id == valueOfCardPlayed).User.UserName;
                }
            }

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

            if (gpn.Contested)
            {
                thisIsAContestedStage = true;
            }

            List<GameUser> supperWinner = new List<GameUser>();

            User contestedGameWinner = null;

            string finishedPickingAction = string.Empty;

            string contestAction = string.Empty;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();

                List<CardModel> cmPlayedLst = new List<CardModel>();

                cds = cds.OrderByDescending(x => x.ModifiedDate).ToList();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (gu.OrderSequence != 1)
                        {
                            action = "";
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "";
                var canClickContestText = "";
                finishedPickingAction = "";
                contestAction = "";

                if (gameIsOver && gu.User.Id == currentUser.Id)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                }
                else
                {
                    contestAction = string.Empty;
                    if (gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards
                        cmLst.ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                if (thisIsAContestedStage)
                {
                    canClickContestText = string.Empty;
                    contestAction = string.Empty;
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickContestText = canClickContestText, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            try
            {
                contestedGameWinner = gameUsers.FirstOrDefault(x => !x.CreatedBy.StartsWith("MUTTUED")).User;
            }
            catch
            {
            }

            if (thisIsAContestedStage)
            {
                if (contestedGameWinner == null)
                {
                    topMessage = "nomutu";
                }
                else
                {
                    if (currentUser.Id == contestedGameWinner.Id)
                    {
                        topMessage = "otherplayermutu";
                    }
                    else
                    {
                        topMessage = "mutu";
                    }
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(gameController))
                {
                    if (currentUser.UserName == gameController)
                    {
                        topMessage = "warning";
                    }
                    else
                    {
                        topMessage = "error";
                    }
                }
            }

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMyCardsComputerGameNonAjax(int? gameId)
        {
            bool thisIsAContestedStage = false;

            return RedirectToAction("Index", "Home");

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var totalGameCount = 5 * gameUsers.Count;

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == totalGameCount;

            var gameController = string.Empty;

            if (gameIsOver)
            {
                var valueOfCardPlayed = 0;

                int.TryParse(currentGame.CreatedBy, out valueOfCardPlayed);

                if (valueOfCardPlayed > 0)
                {
                    gameController = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.Card.Id == valueOfCardPlayed).User.UserName;
                }
            }

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

            if (gpn.Contested)
            {
                thisIsAContestedStage = true;
            }

            List<GameUser> supperWinner = new List<GameUser>();

            User contestedGameWinner = null;

            string finishedPickingAction = string.Empty;

            string contestAction = string.Empty;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();

                List<CardModel> cmPlayedLst = new List<CardModel>();

                cds = cds.OrderByDescending(x => x.ModifiedDate).ToList();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (gu.OrderSequence != 1)
                        {
                            action = "";
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "";
                var canClickContestText = "";
                finishedPickingAction = "";
                contestAction = "";

                if (gameIsOver && gu.User.Id == currentUser.Id)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                }
                else
                {
                    contestAction = string.Empty;
                    if (gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards
                        cmLst.ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                if (thisIsAContestedStage)
                {
                    canClickContestText = string.Empty;
                    contestAction = string.Empty;
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickContestText = canClickContestText, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            try
            {
                contestedGameWinner = gameUsers.FirstOrDefault(x => !x.CreatedBy.StartsWith("MUTTUED")).User;
            }
            catch
            {
            }

            if (thisIsAContestedStage)
            {
                if (contestedGameWinner == null)
                {
                    topMessage = "nomutu";
                }
                else
                {
                    if (currentUser.Id == contestedGameWinner.Id)
                    {
                        topMessage = "otherplayermutu";
                    }
                    else
                    {
                        topMessage = "mutu";
                    }
                }
            }
            else
            {

                if (!string.IsNullOrEmpty(gameController))
                {
                    if (currentUser.UserName == gameController)
                    {
                        topMessage = "warning";
                    }
                    else
                    {
                        topMessage = "error";
                    }
                }
            }

            IndexViewModel21 indexViewModel = new IndexViewModel21();

            indexViewModel.CardOwners = cardOwners;

            indexViewModel.GameId = currentGame.Id;

            indexViewModel.GameUsers = gameUsers;

            indexViewModel.CurrentUserName = currentUser.UserName;

            var globalUrlAction = Url.Action("GetMyCardsComputerGame", "Aggara", new { gameId = currentGame.Id }).ToString();

            indexViewModel.GlobalUrlAction = globalUrlAction;

            return View("StartNewGame",indexViewModel);
        }      


        [HttpGet]
        public ActionResult GameCardClickedComputerGame(int? gameId, int? userId, int? cardId)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("GetMyCardsComputerGameNonAjax", new { gameId = gameId });
            }            

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            var computerUser = GetUser("CLOUD");

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gameController = currentGame.CreatedBy;

            var gameCardPlayed = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == currentGame.Id && x.Card.Id == cardId).FirstOrDefault();           

            var thisGameUser = gameUsers.FirstOrDefault(x => x.User.Id == currentUser.Id);

            thisGameUser.ModifiedBy = cardId.Value.ToString();

            unitOfWork.GameUserRepository.Update(thisGameUser);

            var previousMasterCard = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.TradingFloorCard && x.Game.Id == currentGame.Id && !x.IsABlank);

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            var thisGameHasJustChangedControl = false;

            if (gameController == computerUser.UserName)
            {
                if (previousMasterCard != null)
                {
                    if (gameCardPlayed.Card.Suit.Name == previousMasterCard.Card.Suit.Name && gameCardPlayed.Card.CardNumberValue > previousMasterCard.Card.CardNumberValue)
                    {
                        gameController = currentUser.UserName;                       
                        gameController = thisGameUser.User.UserName;
                        thisGameHasJustChangedControl = true;
                    }

                    previousMasterCard.IsABlank = true;
                    unitOfWork.GameCardRepository.Update(previousMasterCard);
                }
            }
            else
            {
                gameCardPlayed.IsABlank = false;// Change this property name

                if (previousMasterCard != null)
                {
                    previousMasterCard.IsABlank = true;
                    unitOfWork.GameCardRepository.Update(previousMasterCard);
                }

            }

            gameCardPlayed.ShowNumberedSide = true;
            gameCardPlayed.TradingFloorCard = true;
            gameCardPlayed.ModifiedDate = DateTime.Now;
            unitOfWork.GameCardRepository.Update(gameCardPlayed);
            unitOfWork.Save();

            var hasPlayerMuttued = false;

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();
           
            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();                  

            var allComputerCards = listGameCard.Where(x => x.User.Id == computerUser.Id && !x.TradingFloorCard).ToArray();            

            GameCard computersCard = null;

            var iAmInControl = gameController == computerUser.UserName;

            GameCard myLastCard = null;

            var thisComputerGameUser = gameUsers.FirstOrDefault(x => x.User.Id == computerUser.Id);            

            myLastCard = listGameCard.FirstOrDefault(x => x.Card.Id.ToString() == thisComputerGameUser.ModifiedBy);
            
            if (iAmInControl)
            { 
                if (myLastCard.Card.Suit.Name == gameCardPlayed.Card.Suit.Name && gameCardPlayed.Card.CardNumberValue > myLastCard.Card.CardNumberValue)
                {
                    iAmInControl = false;
                    gameController = thisGameUser.User.UserName;
                    thisGameHasJustChangedControl = true;
                }
            }            

            //first of all see if his card beats mine

            if (!thisGameHasJustChangedControl)
            {
                if (allComputerCards.Count() > 0)
                {
                    computersCard = GetComputersResponseAggara(currentUser, gameCardPlayed, listGameCard.ToArray(), allComputerCards, iAmInControl);
                    computersCard.ShowNumberedSide = true;
                    computersCard.TradingFloorCard = true;
                    computersCard.ModifiedDate = DateTime.Now;

                    if (gameController == computerUser.UserName)
                    {
                        computersCard.IsABlank = false;
                    }

                    unitOfWork.GameCardRepository.Update(computersCard);
                    thisComputerGameUser.ModifiedBy = computersCard.Card.Id.ToString();
                    unitOfWork.GameUserRepository.Update(thisComputerGameUser);

                    if (!iAmInControl)
                    {
                        if (computersCard.Card.Suit.Name == gameCardPlayed.Card.Suit.Name && computersCard.Card.CardNumberValue > gameCardPlayed.Card.CardNumberValue)
                        {
                            gameController = computerUser.UserName;
                            listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();
                            allComputerCards = listGameCard.Where(x => x.User.Id == computerUser.Id && !x.TradingFloorCard).ToArray();
                            computersCard = null;
                            iAmInControl = true;

                            if (allComputerCards.Count() > 0)
                            {
                                computersCard = GetComputersResponseAggaraSecond(currentUser, gameCardPlayed, listGameCard.ToArray(), allComputerCards, iAmInControl);
                                computersCard.ShowNumberedSide = true;
                                computersCard.TradingFloorCard = true;
                                computersCard.ModifiedDate = DateTime.Now;
                                computersCard.IsABlank = false;
                                gameCardPlayed.IsABlank = true;
                                thisComputerGameUser.ModifiedBy = computersCard.Card.Id.ToString();
                                unitOfWork.GameUserRepository.Update(thisComputerGameUser);
                                unitOfWork.GameCardRepository.Update(computersCard);
                                unitOfWork.GameCardRepository.Update(gameCardPlayed);
                                gameController = computerUser.UserName;
                            }
                        }
                    }
                }
            }
            else
            {
                gameCardPlayed.IsABlank = false;
                unitOfWork.GameCardRepository.Update(gameCardPlayed);
            }

            currentGame.CreatedBy = gameController;
            unitOfWork.GameRepository.Update(currentGame);
            unitOfWork.Save();

            var winningUser = unitOfWork.UserRepository.Get().FirstOrDefault(x => x.UserName == gameController);

            var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);

            var gameToShowNumberSide = new List<GameCard>();           

            listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == 10;

            if (myLastCard != null && myLastCard.User.Id != currentUser.Id && myLastCard.Card.Suit.Name != gameCardPlayed.Card.Suit.Name && gameController == computerUser.UserName)
            {
                var mutuCount = listGameCard.Where(x => x.User.Id == currentUser.Id && x.Card.Suit.Name == myLastCard.Card.Suit.Name && !x.TradingFloorCard).Count();

                if (mutuCount > 0)
                {
                    hasPlayerMuttued = true;
                    gameIsOver = true;
                    winningUser = computerUser;
                }
            }            

            var whoIsPlayingMessage = gameUsers.Where(x => x.User.Id == currentUser.Id).FirstOrDefault().User.UserName;

            userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();                        

            var finishedPickingAction = string.Empty;

            var canClickFinishText = "";            

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                cds = cds.OrderByDescending(x => x.ModifiedDate).ToList();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClickedComputerGame", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();
                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        var action = string.Empty;
                        cmPlayedLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var thisUserIsPlayingNow = false;

                if ((gameUsers.Count - 1) == floorCount)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gu.User.Id != currentUser.Id)//Hide the Other Users Card Here
                {
                    var ids = gameToShowNumberSide.Select(x => x.Card.Id).ToList();
                    //hide this users cards                   
                    cmLst.Where(x => !ids.Contains(x.Card.Id)).ToList().ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                var playingNowFlashMessage = string.Empty;
                canClickFinishText = string.Empty;
                finishedPickingAction = string.Empty;

                if (gameIsOver)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");

                    if (gu.User.Id == currentUser.Id)
                    {
                        canClickFinishText = "Start New Game";
                        //string canClickContestText = "";

                        if (currentGame.Status == "LIVE")
                        {
                            currentGame.Status = "USERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputerComputerStart", "Aggara", new { prevId = currentGame.Id }).ToString();
                        }
                        else if (currentGame.Status == "USERTOPLAY")
                        {
                            currentGame.Status = "COMPUTERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputer", "Aggara").ToString();
                        }
                        else
                        {
                            currentGame.Status = "USERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputerComputerStart", "Aggara", new { prevId = currentGame.Id }).ToString();
                        }

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();
                    }
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, GameIsOver = gameIsOver, CanShowFlashMessage = thisUserIsPlayingNow, CanShowFlashMessageMessage = playingNowFlashMessage, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            if (gameIsOver)
            {
                if (hasPlayerMuttued)
                {
                    topMessage = "mutu";
                }
                else
                {

                    if (winningUser.Id == currentUser.Id)
                    {
                        topMessage = "warning";
                    }
                    else
                    {
                        topMessage = "error";
                    }
                }
                

                var gameWinner = winningUser;

                if (gameWinner != null)
                {
                    var thisComputerWinner = GetUser("CLOUD");
                    var thisCurrentWinner = unitOfWork.UserRepository.GetByID(currentUser.Id);

                    decimal currentStake = decimal.Zero;
                    decimal.TryParse(currentGame.ModifiedBy, out currentStake);

                    if (gameWinner.Id == thisComputerWinner.Id)
                    {
                        thisComputerWinner.RealMoneyBalance = thisComputerWinner.RealMoneyBalance + currentStake;
                        thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance - currentStake;
                    }
                    else
                    {
                        thisComputerWinner.RealMoneyBalance = thisComputerWinner.RealMoneyBalance - currentStake;
                        thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance + currentStake;
                    }

                    unitOfWork.UserRepository.Update(thisComputerWinner);
                    unitOfWork.Save();
                    unitOfWork.UserRepository.Update(thisCurrentWinner);
                    unitOfWork.Save();
                }
            }           

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);  
        }

        private int GetStakeTotalWithoutCard(List<GameCard> UsersStack, GameCard CardPlayed)
        {
            int Total = 0;

            foreach (var gCard in UsersStack)
            {
                if (!gCard.Card.Suit.Name.Equals(CardPlayed.Card.Suit.Name))
                {
                    Total += gCard.Card.CardNumberValue;
                }
            }

            return Total;
        }

        private decimal IsBreakingCardUsable(int Total)
        {
            if (Total > 21)
                return decimal.Zero;
            else
            {
                decimal dTotal = Total;
                decimal AgboTotal = 21;
                return (dTotal / AgboTotal) * 100;
            }
        }

        private decimal IsBreakingCardUsable(int Total, GameCard card)
        {
            if ((Total + card.Card.CardNumberValue) > 21)
                return decimal.Zero;
            else
            {
                decimal dTotal = Total + card.Card.CardNumberValue;
                decimal AgboTotal = 21;
                return (dTotal / AgboTotal) * 100;
            }
        }

        public GameCard GetComputersResponseAggaraSecond(User homeuser, GameCard PlayersCard, GameCard[] CardStackers, GameCard[] usersStack, bool iAmInControl)
        {
            var thisSecond = DateTime.Now.Second;
            var secSecond = DateTime.Now.Minute;

            if (iAmInControl)
            {
                if (DateTime.Now.Second > 3)
                {
                    var smallestToLargest = usersStack.OrderBy(x => x.Card.CardNumberValue);
                    return smallestToLargest.FirstOrDefault();
                }

                //usersStack.GroupBy(x => x.Card.Suit.Id).Select(x => x.Key.
                int? theId = (from c in usersStack
                              group c by new { c.Card.Suit.Id }
                                  into g
                                  select new
                                  {
                                      g.Key.Id,
                                      Count = g.Count()
                                  }).Where(x => x.Count > 1).Select(x => x.Id).FirstOrDefault();

                if (theId.HasValue)
                {
                    var cardsWithMoreThanOneSuitMembers = usersStack.Where(x => x.Card.Suit.Id == theId.Value).ToList();
                    var smallestToLargest1 = cardsWithMoreThanOneSuitMembers.OrderBy(x => x.Card.CardNumberValue);
                    var returnCard = smallestToLargest1.FirstOrDefault();
                    var returnCardHighest = smallestToLargest1.LastOrDefault();

                    if (returnCard != null && secSecond > 10)
                    {
                        return returnCard;
                    }
                    else if (returnCardHighest != null)
                    {
                        if (usersStack.Count() == 1)
                        {
                            return returnCardHighest;
                        }
                        else
                        {
                            if (smallestToLargest1.Count() == 0)
                            {
                                //Do Nothing
                            }
                            if (smallestToLargest1.Count() == 1)
                            {
                                return smallestToLargest1.FirstOrDefault();
                            }
                            else if (smallestToLargest1.Count() > 1 && smallestToLargest1.Count(x => x.Card.CardNumberValue == 11) > 0)
                            {
                                return smallestToLargest1.Where(x => x.Card.CardNumberValue == 11).LastOrDefault();
                            }
                            else
                            {
                                return smallestToLargest1.PickRandom();
                            }
                        }
                    }
                }

                var smallestToLargestLast = usersStack.OrderBy(x => x.Card.CardNumberValue);
                return smallestToLargestLast.FirstOrDefault();
            }
            else
            {
                string strFirstLetter = PlayersCard.Card.Suit.Name;

                List<GameCard> possibleChoices = new List<GameCard>();
                List<GameCard> dict = new List<GameCard>();

                possibleChoices = usersStack.Where(x => x.Card.Suit.Name == strFirstLetter).ToList();

                if (possibleChoices.Count == 1)
                {
                    return possibleChoices.FirstOrDefault();
                }

                if (possibleChoices.Count > 0)
                {
                    dict.AddRange(possibleChoices);
                }
                else
                {
                    GameCard RemoveFromAces = AcesPresent(usersStack.ToList());

                    if (RemoveFromAces != null)
                        return RemoveFromAces;

                    foreach (var gCard in usersStack)//check for highest value
                    {
                        try
                        {
                            dict.Add(gCard);
                        }
                        catch
                        {
                        }
                    }

                    foreach (var gCard in usersStack)
                    {
                        foreach (var gCard1 in CardStackers)
                        {
                            try
                            {
                                dict.Add(gCard);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                GameCard ReturnCard = GetBestCardToDrop(dict, PlayersCard);
                return ReturnCard;
            }
        }


        public GameCard GetComputersResponseAggara(User homeuser, GameCard PlayersCard, GameCard[] CardStackers, GameCard[] usersStack, bool iAmInControl)
        {
            var thisSecond = DateTime.Now.Second;
            var secSecond = DateTime.Now.Minute;

            if (iAmInControl && thisSecond > 55)
            {
                //usersStack.GroupBy(x => x.Card.Suit.Id).Select(x => x.Key.
                int? theId = (from c in usersStack group c by new { c.Card.Suit.Id }
                               into g
                               select new
                               {
                                   g.Key.Id,
                                   Count = g.Count()
                               }).Where(x => x.Count > 1).Select(x => x .Id).FirstOrDefault();

                if(theId.HasValue)
                {
                    var cardsWithMoreThanOneSuitMembers = usersStack.Where(x => x.Card.Suit.Id == theId.Value).ToList();
                    var smallestToLargest1 = cardsWithMoreThanOneSuitMembers.OrderBy(x => x.Card.CardNumberValue);
                    var returnCard = smallestToLargest1.FirstOrDefault();
                    var returnCardHighest = smallestToLargest1.LastOrDefault();

                    if (returnCard != null && secSecond > 10)
                    {
                        return returnCard;
                    }
                    else if(returnCardHighest != null)
                    {
                        if (usersStack.Count() == 1)
                        {
                            return returnCardHighest;
                        }
                        else
                        {
                            if (smallestToLargest1.Count() == 0)
                            {
                                //Do Nothing
                            }
                            if (smallestToLargest1.Count() == 1)
                            {
                                return smallestToLargest1.FirstOrDefault();
                            }
                            else if (smallestToLargest1.Count() > 1 && smallestToLargest1.Count(x => x.Card.CardNumberValue == 11) > 0)
                            {
                                return smallestToLargest1.Where(x => x.Card.CardNumberValue == 11).LastOrDefault();
                            }
                            else
                            {
                                return smallestToLargest1.PickRandom();
                            }
                        }
                    }
                }                

                var smallestToLargest = usersStack.OrderBy(x => x.Card.CardNumberValue);
                return smallestToLargest.FirstOrDefault();
            }
            else
            {
                string strFirstLetter = PlayersCard.Card.Suit.Name;

                List<GameCard> possibleChoices = new List<GameCard>();
                List<GameCard> dict = new List<GameCard>();

                possibleChoices = usersStack.Where(x => x.Card.Suit.Name == strFirstLetter).ToList();               

                if (possibleChoices.Count == 1)
                {
                    return possibleChoices.FirstOrDefault();
                }

                if (possibleChoices.Count > 0)
                {
                    dict.AddRange(possibleChoices);                   
                }
                else
                {
                    GameCard RemoveFromAces = AcesPresent(usersStack.ToList());

                    if (RemoveFromAces != null)
                        return RemoveFromAces;

                    foreach (var gCard in usersStack)//check for highest value
                    {
                        try
                        {
                            dict.Add(gCard);
                        }
                        catch
                        {
                        }
                    }

                    foreach (var gCard in usersStack)
                    {
                        foreach (var gCard1 in CardStackers)
                        {
                            try
                            {
                                dict.Add(gCard);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                GameCard ReturnCard = GetBestCardToDrop(dict, PlayersCard);
                return ReturnCard;
            }
        }

        private GameCard GetBestCardToDrop(List<GameCard> dictionary, GameCard PlayersCard)
        {
            var breakingCards = dictionary.Where(x => x.Card.CardNumberValue > PlayersCard.Card.CardNumberValue && x.Card.Suit.Name == PlayersCard.Card.Suit.Name).ToList();

            if (breakingCards.Count == 0)
            {
                var smallestToLargest = dictionary.OrderBy(x => x.Card.CardNumberValue).ToList();
                return smallestToLargest.FirstOrDefault();
                //return dictionary.PickRandom();     
            }
            else if (breakingCards.Count == 1)
            {
                return breakingCards.FirstOrDefault();
            }
            else
            {
                var smallestToLargest = breakingCards.OrderBy(x => x.Card.CardNumberValue).ToList();
                return smallestToLargest.FirstOrDefault();
            }                  
        }

        public GameCard GetComputersResponse(User homeuser, GameCard PlayersCard, GameCard[] CardStackers, GameCard[] usersStack)
        {
            string strFirstLetter = PlayersCard.Card.Suit.Name;

            List<GameCard> PossibleChoices = new List<GameCard>();
            Dictionary<decimal, GameCard> Dict = new Dictionary<decimal, GameCard>();

            foreach (var gCard in usersStack)
            {
                string UsersFirstLetter = gCard.Card.Suit.Name;

                if (strFirstLetter == UsersFirstLetter)
                {
                    PossibleChoices.Add(gCard);
                }
            }

            if (PossibleChoices.Count > 0)
            {
                int possibility = PossibleChoices.Count;
                //int Count = 0;

                foreach (var gCard in PossibleChoices)
                {
                    int Total = GetStakeTotalWithoutCard(usersStack.ToList(), gCard);

                    if (gCard.Card.CardNumberValue > PlayersCard.Card.CardNumberValue)
                    {
                        try
                        {
                            Dict.Add(IsBreakingCardUsable(Total, CardStackers[0]), gCard);
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            Dict.Add(IsBreakingCardUsable(Total, CardStackers[1]), gCard);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            else
            {
                GameCard RemoveFromAces = AcesPresent(usersStack.ToList());

                if (RemoveFromAces != null)
                    return RemoveFromAces;

                foreach (var gCard in usersStack)//check for highest value
                {
                    int Total = GetStakeTotalWithoutCard(usersStack.ToList(), gCard);
                    try
                    {
                        Dict.Add(IsBreakingCardUsable(Total), gCard);
                    }
                    catch
                    {
                    }
                }




                foreach (var gCard in usersStack)
                {
                    int Total = GetStakeTotalWithoutCard(usersStack.ToList(), gCard);
                    foreach (var gCard1 in CardStackers)
                    {
                        try
                        {
                            Dict.Add(IsBreakingCardUsable(Total, gCard1), gCard);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            GameCard ReturnCard = GetHighestProbability(Dict);

            return ReturnCard;
        }

        private GameCard GetHighestProbability(Dictionary<decimal, GameCard> dictionary)
        {
            GameCard MaxProbCard = null;
            decimal max = decimal.Negate(1);

            IEnumerator<KeyValuePair<decimal, GameCard>> CardEnum = dictionary.GetEnumerator();
            while (CardEnum.MoveNext())
            {
                if (CardEnum.Current.Key > max)
                {
                    MaxProbCard = CardEnum.Current.Value;
                    max = CardEnum.Current.Key;
                }

            }

            return MaxProbCard;
        }

        private GameCard AcesPresent(List<GameCard> usersStack)
        {
            List<GameCard> arAces = new List<GameCard>();
            List<GameCard> arNoAces = new List<GameCard>();

            foreach (var gCard in usersStack)
            {
                bool ace = gCard.Card.CardNumberValue == 11;
            
                if (ace)
                    arAces.Add(gCard);
                else
                    arNoAces.Add(gCard);
            }

            if (arAces.Count > 0 && usersStack.Count == 1 )
            {
                return arAces.PickRandom();
            }
            else
            {
                if (DateTime.Now.Second > 3)
                {
                    var smallestToLargest = usersStack.OrderBy(x => x.Card.CardNumberValue);
                    return smallestToLargest.FirstOrDefault();
                }
                else
                {
                    //usersStack.GroupBy(x => x.Card.Suit.Id).Select(x => x.Key.
                    int? theId = (from c in usersStack
                                  group c by new { c.Card.Suit.Id }
                                      into g
                                      select new
                                      {
                                          g.Key.Id,
                                          Count = g.Count()
                                      }).Where(x => x.Count > 1).Select(x => x.Id).FirstOrDefault();

                    if (theId.HasValue)
                    {
                        var cardsWithMoreThanOneSuitMembers = usersStack.Where(x => x.Card.Suit.Id == theId.Value).ToList();
                        var smallestToLargest1 = cardsWithMoreThanOneSuitMembers.OrderBy(x => x.Card.CardNumberValue);
                        var returnCard = smallestToLargest1.FirstOrDefault();
                        var returnCardHighest = smallestToLargest1.LastOrDefault();

                        if (returnCard != null && DateTime.Now.Second > 2)
                        {
                            return returnCard;
                        }
                        else if (returnCardHighest != null)
                        {
                            return returnCardHighest;
                        }
                    }
                }

                var smallestToLargest11 = usersStack.OrderBy(x => x.Card.CardNumberValue);
                return smallestToLargest11.FirstOrDefault();
            }
        }
       
        [HttpGet]
        public ActionResult GameCardClicked(int? gameId, int? userId, int? cardId)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("GetMyCardsNonAjax", new { gameId = gameId });
            }

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == User.Identity.Name.ToUpper()).FirstOrDefault();

            List<CardOwner> cardOwners = new List<CardOwner>();            

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var gameController = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;            

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var valueOfCardPlayed = 0;

            int.TryParse(currentGame.CreatedBy, out valueOfCardPlayed);

            var thisGameUser = gameUsers.FirstOrDefault(x => x.User.Id == userId);

            var gameCardPlayed = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == currentGame.Id && x.Card.Id == cardId).FirstOrDefault();

            var noFirstCardClicked = unitOfWork.GameCardRepository.Get().Where(x => x.TradingFloorCard && x.Game.Id == currentGame.Id).Count() == 0;

            var previousMasterCard = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.TradingFloorCard && x.Game.Id == currentGame.Id && !x.IsABlank);
            
            gameCardPlayed.ShowNumberedSide = true;
            gameCardPlayed.TradingFloorCard = true;
            gameCardPlayed.ModifiedDate = DateTime.Now;

            if (previousMasterCard == null)
            {
                gameCardPlayed.IsABlank = false;
            }
            else
            {
                gameController = previousMasterCard.User.UserName;
            }
            
            unitOfWork.GameCardRepository.Update(gameCardPlayed);
            thisGameUser.ModifiedBy = gameCardPlayed.Card.Id.ToString();

            if (valueOfCardPlayed > 0)
            {
                gameController = unitOfWork.GameCardRepository.Get().FirstOrDefault(x => x.Card.Id == valueOfCardPlayed && x.Game.Id == currentGame.Id).User.UserName;
            }

            GameCard controllingCard = null;

            if (gameController == currentUser.UserName)
            {
                currentGame.CreatedBy = gameCardPlayed.Card.Id.ToString();
                unitOfWork.GameRepository.Update(currentGame);
                controllingCard = gameCardPlayed;
            }           
            
            unitOfWork.Save();          

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var doRearrange = true;            

            var gameFirstCard = listGameCard.FirstOrDefault(x => x.Card.Id.ToString() == currentGame.CreatedBy);

            if (gameFirstCard != null && gameFirstCard.User.Id != currentUser.Id && gameFirstCard.Card.Suit.Name != gameCardPlayed.Card.Suit.Name && gameController != currentUser.UserName)
            {
                var mutuCount = listGameCard.Where(x => x.User.Id == currentUser.Id && x.Card.Suit.Name == gameFirstCard.Card.Suit.Name && !x.TradingFloorCard).Count();

                if (mutuCount > 0)
                {
                    var mutudGameUser = gameUsers.FirstOrDefault(x => x.User.Id == currentUser.Id);
                    mutudGameUser.CreatedBy = "MUTTUED";
                    unitOfWork.GameUserRepository.Update(mutudGameUser);
                    //unitOfWork.Save();
                }
            }

            if (gameController != currentUser.UserName)
            {
                currentGame = unitOfWork.GameRepository.GetByID(currentGame.Id);

                if (gameCardPlayed.Card.Suit.Name == gameFirstCard.Card.Suit.Name && gameCardPlayed.Card.CardNumberValue > gameFirstCard.Card.CardNumberValue)
                {
                    var seniorUser = gameCardPlayed.User;
                    gameUsers = RearrangeGameSequence(gameUsers, currentGame.Id, seniorUser);
                    doRearrange = false;
                    currentGame.CreatedBy = gameCardPlayed.Card.Id.ToString();
                    unitOfWork.GameRepository.Update(currentGame);
                    controllingCard = gameCardPlayed;
                    gameController = gameCardPlayed.User.UserName;

                    if (previousMasterCard != null)
                    {
                        previousMasterCard.IsABlank = true;
                        unitOfWork.GameCardRepository.Update(previousMasterCard);
                    }

                    gameCardPlayed.IsABlank = false;
                    unitOfWork.GameCardRepository.Update(gameCardPlayed);
                }
            }
            else
            {
                if (previousMasterCard != null)
                {
                    previousMasterCard.IsABlank = true;
                    unitOfWork.GameCardRepository.Update(previousMasterCard);
                }

                gameCardPlayed.IsABlank = false;
                unitOfWork.GameCardRepository.Update(gameCardPlayed);   
            }

            unitOfWork.Save();

            if (doRearrange)
            {
                gameUsers = RearrangeGameSequence(gameUsers, currentGame.Id);
            }

            var totalGameCount = 5 * gameUsers.Count;

            var gameIsOver = listGameCard.Where(x => x.TradingFloorCard).Count() == totalGameCount;

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;
            
            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();           

            var finishedPickingAction = string.Empty;

            var canClickFinishText = "";

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();
                cds = cds.OrderByDescending(x => x.ModifiedDate).ToList();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Aggara", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (gu.OrderSequence != 1 )
                        {
                            action = "";
                        }                        

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                    else
                    {
                        var action = string.Empty;
                        cmPlayedLst.Add(new CardModel { ActionUrl = action, Card = c.Card, IsStarCard = !c.IsABlank });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickContestText = "";
                var contestAction = "";               
                finishedPickingAction = "";
                contestAction = "";

                if (gameIsOver && gu.User.Id == currentUser.Id)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Aggara", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    finishedPickingAction = Url.Action("StartNewGame", "Aggara", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();
                }

                if (gu.User.Id != currentUser.Id)
                {
                    //hide this users cards                   
                    cmLst.ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, CanClickContestText = canClickContestText, GameIsOver = gameIsOver, CanClickContest = contestAction, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_AggaraCardSharer", cardOwners);

            string topMessage = string.Empty;

            if (gameIsOver)
            {
                if (gameController == currentUser.UserName)
                {
                    topMessage = "warning";
                }
                else
                {
                    topMessage = "error";
                }

                User gameWinner = unitOfWork.UserRepository.Get().FirstOrDefault(x => x.UserName == gameController);

                if (gameWinner != null)
                {
                    var possibleWinners = gameUsers.ToList();

                    decimal currentStake = decimal.Zero;

                    decimal.TryParse(currentGame.ModifiedBy, out currentStake);

                    foreach (var possibleWinner in possibleWinners)
                    {
                        var thisCurrentWinner = unitOfWork.UserRepository.GetByID(possibleWinner.User.Id);

                        if (possibleWinner.User.Id == gameWinner.Id)
                        {
                            thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance + currentStake;
                        }
                        else
                        {
                            thisCurrentWinner.RealMoneyBalance = thisCurrentWinner.RealMoneyBalance - currentStake;
                        }

                        unitOfWork.UserRepository.Update(thisCurrentWinner);
                        unitOfWork.Save();
                    }
                }
            }           

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
        }

        private List<GameUser> RearrangeGameSequence(List<GameUser> gameUsers, int gameId, User seniorUser)
        {
            if (gameUsers.Count == 2)
            {
                gameUsers.Where(x => x.User.Id == seniorUser.Id).ToList().ForEach(y => y.OrderSequence = 1);
                gameUsers.Where(x => x.User.Id != seniorUser.Id).ToList().ForEach(y => y.OrderSequence = 2);
            }
            else
            {
                gameUsers.Where(x => x.User.Id == seniorUser.Id).ToList().ForEach(y => y.OrderSequence = 1);
                gameUsers.Where(x => x.User.Id != seniorUser.Id && x.OrderSequence != 3).ToList().ForEach(x => x.OrderSequence = x.OrderSequence + 1);
            }

            foreach (var gu in gameUsers)
            {
                var gameUser = unitOfWork.GameUserRepository.GetByID(gu.Id);
                gameUser.OrderSequence = gu.OrderSequence;
                gameUser.Finished = gu.Finished;
                unitOfWork.GameUserRepository.Update(gameUser);
                unitOfWork.Save();
            }

            return unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();
        }

        private List<GameUser> RearrangeGameSequence(List<GameUser> gameUsers, int gameId)
        {
            if (gameUsers.Count == 2)
            {
                gameUsers.ForEach(x => x.OrderSequence = x.OrderSequence + 1);
                gameUsers.Where(x => x.OrderSequence > 2).ToList().ForEach(y => y.OrderSequence = 1);
            }
            else
            {
                gameUsers.ForEach(x => x.OrderSequence = x.OrderSequence + 1);
                gameUsers.Where(x => x.OrderSequence > 3).ToList().ForEach(y => y.OrderSequence = 1);
            }

            foreach (var gu in gameUsers)
            {
                var gameUser = unitOfWork.GameUserRepository.GetByID(gu.Id);
                gameUser.OrderSequence = gu.OrderSequence;
                gameUser.Finished = gu.Finished;
                unitOfWork.GameUserRepository.Update(gameUser);
                unitOfWork.Save();
            }

            return unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();
        }

        private MultiUserStackOfCardsAggara GetMultiUserPlayingCards(GameUser[] gameUsers)
        {

            int numOfUsers = gameUsers.Length;

            int MaxResult = 9;

            if (numOfUsers == 2)
                MaxResult = 10;
            else if (numOfUsers == 3)
                MaxResult = 15;
            else
                MaxResult = 15;

            IList<Card> list = null;
            IList<Card> list1 = null;

            try
            {
                //ITransaction transaction = session.BeginTransaction();
                var cards = unitOfWork.CardRepository.Get().Where(x => x.Suit.Id != 5).OrderBy(x => Guid.NewGuid()).ToList();

                list = cards.Take(MaxResult).ToList();

                list1 = cards.Take(21).ToList();

                return new MultiUserStackOfCardsAggara((List<Card>)list, (List<Card>)list1, gameUsers);
            }
            catch
            {
                return new MultiUserStackOfCardsAggara((List<Card>)list, (List<Card>)list1, gameUsers);
            }
            finally
            {

            }
        }

        private Game21 CreateNewGameCardsForGame(GameUser[] gameUsers, out List<GameCard> listGameCard)
        {
            return CreateNewGame(gameUsers[0].Game21.Id, gameUsers, out listGameCard);
        }

        public Game21 CreateNewGame(int id, GameUser[] gameUsers, out List<GameCard> listGameCard)
        {
            MultiUserStackOfCardsAggara msoc = GetMultiUserPlayingCards(gameUsers);

            User AgboFloor = GetUser("AGBO FLOOR");

            listGameCard = new List<GameCard>();

            var thisGame = unitOfWork.GameRepository.GetByID(id);

            foreach (User user in msoc.UsersList)
            {
                int i = 0;
                foreach (Card card in user.UsersStack)
                {
                    GameCard gc = new GameCard();
                    gc.User = unitOfWork.UserRepository.GetByID(user.Id);
                    gc.Game = unitOfWork.GameRepository.GetByID(thisGame.Id);
                    gc.Card = unitOfWork.CardRepository.GetByID(card.Id);
                    gc.OrderSeq = ++i;
                    gc.ShowNumberedSide = false;//dont show numbered side apart from current user
                    gc.IsABlank = true; //Assume card is a blank apart for current user
                    gc.Status = "USERSSTACK";
                    listGameCard.Add(gc);
                }
            }

            int pi = 0;

            foreach (Card card1 in msoc.ComputersStack)
            {
                GameCard gc = new GameCard();
                gc.Game = thisGame;
                gc.User = unitOfWork.UserRepository.GetByID(AgboFloor.Id);
                gc.Game = unitOfWork.GameRepository.GetByID(thisGame.Id);
                gc.Card = unitOfWork.CardRepository.GetByID(card1.Id);
                gc.OrderSeq = ++pi;
                gc.ShowNumberedSide = true; //Always true
                gc.IsABlank = true; //cant click on card initially             
                gc.Status = "AGBOFLOOR";
                listGameCard.Add(gc);
            }

            //Save all new GameCards

            foreach (GameCard gameCard in listGameCard)
            {
                gameCard.ModifiedDate = DateTime.Now;
                gameCard.CreatedDate = DateTime.Now;
                gameCard.CreatedBy = "sa";
                gameCard.ModifiedBy = "sa";

                gameCard.Game.GameUsers.ToList().ForEach(x => x.CreatedDate = DateTime.Now);
                gameCard.Game.GameUsers.ToList().ForEach(x => x.ModifiedDate = DateTime.Now);
                if (gameCard.Game.GameCards != null)
                {
                    gameCard.Game.GameCards.ToList().ForEach(x => x.User.CreatedDate = DateTime.Now);
                    gameCard.Game.GameCards.ToList().ForEach(x => x.User.ModifiedDate = DateTime.Now);
                    gameCard.Game.GameCards.ToList().ForEach(x => x.User.LastLoggedInDate = DateTime.Now);
                    gameCard.Game.GameCards.ToList().ForEach(x => x.User.StartDate = DateTime.Now);
                }

                gameCard.User.CreatedDate = DateTime.Now;
                gameCard.User.ModifiedDate = DateTime.Now;
                gameCard.User.LastLoggedInDate = DateTime.Now;
                gameCard.User.StartDate = DateTime.Now;
                unitOfWork.GameCardRepository.Insert(gameCard);

                unitOfWork.Save();
            }

            return unitOfWork.GameRepository.GetByID(id);
        }

        private Game21 CreateNewGameWithUserId(GameUser[] gameUsers, bool silentMode, int totalNumOfPlayers, decimal gameStake = 1)
        {
            Game21 game = new Game21();
            game.TimeStarted = DateTime.Now;
            game.Status = "LIVE";
            game.CreatedBy = "sa";
            game.CreatedDate = DateTime.Now;
            game.IsActive = true;
            game.TimeStarted = DateTime.Now;
            game.TimeEnded = DateTime.Now;
            game.ModifiedDate = DateTime.Now;
            game.ModifiedBy = gameStake.ToString();
            unitOfWork.GameRepository.Insert(game);
            unitOfWork.Save();

            //game.GameUsers = gameUsers;
            GamePlayingNow gpn = new GamePlayingNow();
            gpn.Game = game;
            gpn.GameStage = 1;
            gpn.ValueNum = 1;
            gpn.CreatedBy = User.Identity.Name;
            gpn.CreatedDate = DateTime.Now;
            gpn.IsActive = true;
            gpn.ModifiedDate = DateTime.Now;
            gpn.GameStage = 1;
            gpn.ValueNum = totalNumOfPlayers;

            if (silentMode)
                gpn.GameStage = 0;

            unitOfWork.GamePlayingNowRepository.Insert(gpn);
            unitOfWork.Save();

            int i = 1;

            foreach (GameUser gameUser in gameUsers)
            {
                GameUser gameUser1 = new GameUser();// gameUser; 
                gameUser1.User = unitOfWork.UserRepository.Get().FirstOrDefault(x => x.Id == gameUser.User.Id);

                gameUser1.Finished = 0;
                gameUser1.OrderSequence = i;
                //This will only work for 2 people game
                ///gameUser1.OrderSequence = GetSyncOrder(gameUser.OrderSequence);

                gameUser1.Game21 = game;
                gameUser1.CreatedBy = "sa";
                gameUser1.CreatedDate = DateTime.Now;
                gameUser1.IsActive = true;
                gameUser1.ModifiedDate = DateTime.Now;

                unitOfWork.GameUserRepository.Insert(gameUser1);
                unitOfWork.Save();
                i++;
            }

            gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == game.Id).ToArray();

            foreach (GameUser gameUser in gameUsers)
            {
                var thisUser = unitOfWork.UserRepository.Get().FirstOrDefault(x => x.Id == gameUser.User.Id);
                thisUser.playingSeq = gameUser.OrderSequence;
                unitOfWork.UserRepository.Update(thisUser);
                unitOfWork.Save();
            }

            return game;
        }
    }
}
