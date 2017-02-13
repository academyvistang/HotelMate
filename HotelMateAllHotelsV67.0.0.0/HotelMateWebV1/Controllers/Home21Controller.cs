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
    //[System.Web.Mvc.OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    //[Authorize]
    public class Home21Controller : Controller
    {
        private UnitOfWork unitOfWork = null;

        public Home21Controller()
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
            var thisUserName = User.Identity.Name;

            var indexViewModel = new IndexViewModel21();
            var currentUser = GetUser(thisUserName);
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
            var thisUserName = User.Identity.Name;

            var userName = thisUserName;

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
                    var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

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

            indexViewModel.CardOwners = cardOwners;

            indexViewModel.GameId = newGame.Id;

            var globalUrlAction = Url.Action("GetMyCards", "Home21", new { gameId = newGame.Id }).ToString();

            indexViewModel.GlobalUrlAction = globalUrlAction;

            indexViewModel.GameUsers = gameUsers;

            indexViewModel.CurrentUserName = userName;

            return View("StartNewGame", indexViewModel);
        }

        public ActionResult ZeroBalance()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }
            var thisUserName = User.Identity.Name;

            var indexViewModel = new IndexViewModel21();
            var currentUser = GetUser(thisUserName);
            indexViewModel.CurrentUserName = currentUser.UserName;
            indexViewModel.CurrentUserBalance = currentUser.RealMoneyBalance;
            indexViewModel.LiveGamesCount = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.GameStage == 0).Count();
            return View(indexViewModel);
        }

        public virtual GameCard PlaySecondHand(GameCard[] cardStackers, GameCard[] usersStack, bool secondCardHasPriority)
        {
            cardStackers = cardStackers.OrderBy(x => x.OrderSeq).ToArray();

            var firstCard = cardStackers.FirstOrDefault();
            var secondCard = cardStackers.Where(x => x.Id != firstCard.Id).FirstOrDefault();

            var testCards = usersStack;

            Dictionary<decimal, GameCard> TryCardPickDict = new Dictionary<decimal, GameCard>();

            foreach (var myCard in testCards)
            {
                var sumCards = usersStack.Where(x => x.Card.Id != myCard.Card.Id).ToList();
                decimal totalWithOutThisCard = sumCards.Sum(x => x.Card.CardNumberValue);

                try
                {
                    if (totalWithOutThisCard <= 21)
                    {
                        TryCardPickDict.Add(totalWithOutThisCard, myCard);
                    }
                }
                catch
                {
                }
            }

            Dictionary<decimal, GameCard> FirstCardPickDict = new Dictionary<decimal, GameCard>();
            Dictionary<decimal, GameCard> SecondCardPickDict = new Dictionary<decimal, GameCard>();

            IEnumerator<KeyValuePair<decimal, GameCard>> CardEnum = TryCardPickDict.GetEnumerator();

            while (CardEnum.MoveNext())
            {
                var total = CardEnum.Current.Key + firstCard.Card.CardNumberValue;
                if (total <= 21 && total > 19)
                {
                    try
                    {
                        FirstCardPickDict.Add(total, CardEnum.Current.Value);
                    }
                    catch
                    {
                    }
                }
            }

            while (CardEnum.MoveNext())
            {
                var total = CardEnum.Current.Key + secondCard.Card.CardNumberValue;
                if (total <= 21 && total > 19)
                {
                    try
                    {
                        SecondCardPickDict.Add(total, CardEnum.Current.Value);
                    }
                    catch
                    {
                    }
                }
            }

            FirstCardPickDict.OrderBy(x => x.Key);
            SecondCardPickDict.OrderBy(x => x.Key);
            TryCardPickDict.OrderBy(x => x.Key);

            GameCard cardToDrop = null;

            if (secondCardHasPriority)
            {
                if (SecondCardPickDict.Count > 0)
                {
                    cardToDrop = SecondCardPickDict.LastOrDefault().Value;
                }
                else if (TryCardPickDict.Count > 0)
                {
                    return TryCardPickDict.FirstOrDefault().Value;
                }
            }

            if (cardToDrop != null)
            {
                return cardToDrop;
            }

            if (FirstCardPickDict.Count > 0)
            {
                return FirstCardPickDict.FirstOrDefault().Value;
            }

            if (SecondCardPickDict.Count > 0)
            {
                cardToDrop = SecondCardPickDict.LastOrDefault().Value;
            }

            if (cardToDrop != null && cardToDrop.Card.CardNumberValue < 6)
            {
                return cardToDrop;
            }

            if (TryCardPickDict.Count > 0)
            {
                return TryCardPickDict.FirstOrDefault().Value;
            }

            return usersStack.PickRandom();
        }

        public virtual GameCard PlayFirstHand(GameCard[] cardStackers, GameCard[] usersStack)
        {
            GameCard removeFromAces = AcesPresent(usersStack.ToList());

            if (removeFromAces != null)
                return removeFromAces;

            cardStackers = cardStackers.OrderBy(x => x.OrderSeq).ToArray();

            var firstCard = cardStackers.FirstOrDefault();
            var secondCard = cardStackers.Where(x => x.Id != firstCard.Id).FirstOrDefault();


            var testCards = usersStack;

            Dictionary<decimal, GameCard> TryCardPickDict = new Dictionary<decimal, GameCard>();

            foreach (var myCard in testCards)
            {
                var sumCards = usersStack.Where(x => x.Card.Id != myCard.Card.Id).ToList();
                decimal totalWithOutThisCard = sumCards.Sum(x => x.Card.CardNumberValue);

                try
                {
                    if (totalWithOutThisCard <= 21)
                    {
                        TryCardPickDict.Add(totalWithOutThisCard, myCard);
                    }
                }
                catch
                {
                }
            }

            Dictionary<decimal, GameCard> FirstCardPickDict = new Dictionary<decimal, GameCard>();
            Dictionary<decimal, GameCard> SecondCardPickDict = new Dictionary<decimal, GameCard>();


            IEnumerator<KeyValuePair<decimal, GameCard>> CardEnum = TryCardPickDict.GetEnumerator();

            while (CardEnum.MoveNext())
            {
                var total = CardEnum.Current.Key + firstCard.Card.CardNumberValue;
                if (total <= 21 && total > 18)
                {
                    try
                    {
                        FirstCardPickDict.Add(total, CardEnum.Current.Value);
                    }
                    catch
                    {
                    }
                }
            }

            while (CardEnum.MoveNext())
            {
                var total = CardEnum.Current.Key + secondCard.Card.CardNumberValue;
                if (total <= 21 && total > 18)
                {
                    try
                    {
                        SecondCardPickDict.Add(total, CardEnum.Current.Value);
                    }
                    catch
                    {
                    }
                }
            }

            FirstCardPickDict.OrderBy(x => x.Key);
            SecondCardPickDict.OrderBy(x => x.Key);
            TryCardPickDict.OrderBy(x => x.Key);


            if (FirstCardPickDict.Count > 0)
            {
                return FirstCardPickDict.FirstOrDefault().Value;
            }

            GameCard cardToDrop = null;

            if (SecondCardPickDict.Count > 0)
            {
                cardToDrop = SecondCardPickDict.LastOrDefault().Value;
            }

            if (cardToDrop != null && cardToDrop.Card.CardNumberValue < 6)
            {
                return cardToDrop;
            }

            if (TryCardPickDict.Count > 0)
            {
                return TryCardPickDict.FirstOrDefault().Value;
            }

            return usersStack.PickRandom();
        }

        [HttpGet]
        public ActionResult PlayAgainstComputerComputerStart(int? prevId)
        {
            var thisUserName = User.Identity.Name;

            var userName = thisUserName;

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

            User agboFloor = GetUser("AGBO FLOOR");

            var agboFloorGameUser = new GameUser { User = agboFloor };

            gameUsers.Add(agboFloorGameUser);

            listGameCard = GetGameCardByGameByGameID(newGame.Id);

            GameCard firstCardPlayedByComputer = null;

            if (computersTurnToPlayFirst)
            {
                var pfc = PlayFirstHand(listGameCard.Where(x => x.User.Id == agboFloor.Id).ToArray(), listGameCard.Where(x => x.User.Id == userComputer.Id).ToArray());

                pfc.IsABlank = false;// Change this property name               

                pfc.ShowNumberedSide = true;
                pfc.TradingFloorCard = true;
                unitOfWork.GameCardRepository.Update(pfc);

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
                    var action = Url.Action("GameCardClickedComputerGame", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

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

                if (gu.User.Id != agboFloor.Id && gu.User.Id != currentUser.Id)
                {
                    //hide this users cards
                    cmLst.ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                var cardsPlayed = new List<CardModel>();

                if (firstCardPlayedByComputer != null && gu.User.Id == userComputer.Id)
                {
                    cardsPlayed.Add(new CardModel { ActionUrl = "", Card = firstCardPlayedByComputer.Card });
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
                string topView = RenderRazorViewToString("_CardSharer", cardOwners);

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

                var globalUrlAction = Url.Action("GetMyCardsComputerGame", "Home21", new { gameId = newGame.Id }).ToString();

                indexViewModel.GlobalUrlAction = globalUrlAction;

                indexViewModel.DontShowDiscussionDiv = true;

                return View("StartNewGame", indexViewModel);
            }
        }

        [HttpGet]
        public ActionResult PlayAgainstComputer()
        {
            var userName = User.Identity.Name;

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
                    var action = Url.Action("GameCardClickedComputerGame", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

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

                if (gu.User.Id != agboFloor.Id && gu.User.Id != currentUser.Id)
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
                string topView = RenderRazorViewToString("_CardSharer", cardOwners);

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

                var globalUrlAction = Url.Action("GetMyCardsComputerGame", "Home21", new { gameId = newGame.Id }).ToString();

                indexViewModel.GlobalUrlAction = globalUrlAction;

                indexViewModel.DontShowDiscussionDiv = true;

                return View("StartNewGame", indexViewModel);
            }
        }

        public ActionResult StartNewGame(int? id, int? previousGameId, int? userId, int? numOfPlayers, decimal? stake)
        {
            var thisUserName = User.Identity.Name;


            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("LogOn", "Account");
            }

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

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
                        var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

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

                    //if (gu.User.Id != agboFloor.Id && gu.User.Id != currentUser.Id)
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
                    string topView = RenderRazorViewToString("_CardSharer", cardOwners);

                    string topMessage = string.Empty;

                    return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    indexViewModel.CardOwners = cardOwners;

                    indexViewModel.CurrentUserName = currentUser.UserName;

                    indexViewModel.GameId = newGame.Id;

                    var globalUrlAction = Url.Action("GetMyCards", "Home21", new { gameId = newGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlAction;

                    indexViewModel.GameUsers = gameUsers;

                    return View(indexViewModel);
                }
            }

            if (!id.HasValue)
            {
                var thisUserName1 = User.Identity.Name;

                var userName = thisUserName1;

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

                newGame.CreatedBy = "21";
                unitOfWork.GameRepository.Update(newGame);
                unitOfWork.Save();

                indexViewModel.GameUsers = gameUsers;

                List<CardOwner> cardOwners = new List<CardOwner>();

                cardOwners.Add(new CardOwner { GameStake = newGame.ModifiedBy, Owner = gameUser1.User, CardModelsPlayingStack = new List<CardModel>(), CardModelsPlayedStack = new List<CardModel>() });

                if (Request.IsAjaxRequest())
                {
                    string topView = RenderRazorViewToString("_CardSharer", cardOwners);

                    string topMessage = string.Empty;

                    return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    indexViewModel.CardOwners = cardOwners;

                    indexViewModel.GameId = newGame.Id;

                    var globalUrlAction = Url.Action("GetMyCards", "Home21", new { gameId = newGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlAction;

                    indexViewModel.CurrentUserName = userName;

                    return View(indexViewModel);
                }

            }
            else
            {
                var userName = thisUserName;

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

                    var globalUrlActionQuick = Url.Action("GetMyCards", "Home21", new { gameId = lastGame.Id }).ToString();

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
                    //gameCards = GetGameCardByGameByGameID(newGame.Id);
                }

                newGame.CreatedBy = "21";
                unitOfWork.GameRepository.Update(newGame);
                unitOfWork.Save();

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
                        var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Id }).ToString();

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
                    string topView = RenderRazorViewToString("_CardSharer", cardOwners);

                    string topMessage = string.Empty;

                    return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    indexViewModel.CardOwners = cardOwners;

                    indexViewModel.GameId = newGame.Id;

                    indexViewModel.GameUsers = gameUsers;

                    indexViewModel.CurrentUserName = userName;

                    var globalUrlAction = Url.Action("GetMyCards", "Home21", new { gameId = newGame.Id }).ToString();

                    indexViewModel.GlobalUrlAction = globalUrlAction;

                    return View(indexViewModel);
                }
            }
        }

        [HttpGet]
        public ActionResult GameContested(int? gameId, int? userId, int? cardId, bool? floorFinishClicked)
        {
            var thisUserName = User.Identity.Name;

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

            gpn.Contested = true;

            unitOfWork.GamePlayingNowRepository.Update(gpn);

            unitOfWork.Save();

            var thisUser = unitOfWork.UserRepository.GetByID(userId.Value);

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            List<GameUser> supperWinner = new List<GameUser>();

            User gameWinner = null;

            if (gameIsOver)
            {
                var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
                var allCorrectPlayedCards = listGameCard.Where(x => x.TradingFloorCard && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();

                if (allCorrectPlayedCards.Count == (gameUsers.Count - 1))
                {
                    andTheWinnerIs = "Game contested, contest is invalid, all the players answered to the calling cards.";
                }
                else
                {
                    var tradingFloor = listGameCard.Where(x => !x.TradingFloorCard && x.User.Id != AgboFloor.Id).ToList();
                    var allFloorCardsVioalting = listGameCard.Where(x => x.Status == "USERSSTACK" && x.User.Id != AgboFloor.Id && !x.TradingFloorCard &&
                        x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                    if (allFloorCardsVioalting.Count > 0)
                    {
                        andTheWinnerIs = "Game contested, contest is valid, and the new winner is " + gameFirstCard.User.UserName;
                        gameWinner = gameFirstCard.User;
                    }
                }
            }

            if (gameWinner != null)
            {
                var possibleWinners = gameUsers.Where(x => x.User.Id != AgboFloor.Id).ToList();

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

                if (cds.Count == 0)
                    gameIsOver = true;

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds)
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

                var canClickFinishText = "Done!";

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gameIsOver)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();

                    if (gu.User.Id == AgboFloor.Id)
                        finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();
                }
                else
                {
                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
                    {
                        //hide this users cards                   
                        cmLst.Where(x => !x.Card.ShowNumberedSide).ToList().ForEach(x => x.Card = blankCard);
                        cmLst.ForEach(x => x.ActionUrl = "");
                    }
                }

                contestAction = string.Empty;
                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, TheWinnerIs = andTheWinnerIs, GameIsOver = gameIsOver, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_CardSharer", cardOwners);

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
                return RedirectToAction("GetMyCardsComputerGameNonAjax", new { gameId = gameId });
            }

            var thisUserHasMuttude = false;

            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

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
                        var floorCards = new List<GameCard>();
                        var gameCards = GetComputerSomeCards(compuserGameCards, listGameCard.Where(x => x.User.Id == AgboFloor.Id && !x.TradingFloorCard).ToList());

                        foreach (var gc in gameCards)
                        {
                            gc.User = computer;
                            unitOfWork.GameCardRepository.Update(gc);
                            unitOfWork.Save();
                        }

                        //Supper thinking
                       
                        var nowRandy = new Random().Next(1, 10);

                        if (nowRandy <= 4)
                        {

                        floorCards = gameCards;

                        var thekokoGameCards = listGameCard.Where(x => x.User.Id == computer.Id && !x.TradingFloorCard).ToList();

                        var winnerFound = false;

                        var userTotal = CalculateUserTotal(thekokoGameCards.ToArray(), ref winnerFound);

                        if (userTotal < 19)
                        {
                            var replaceableCards = thekokoGameCards.Where(x => !floorCards.Contains(x)).ToList();

                            var cardToReplace = replaceableCards.FirstOrDefault( x => x.Card.CardNumberValue != 11);

                            if (cardToReplace != null)
                            {
                                var valueOfCard = cardToReplace.Card.CardNumberValue;

                                var cardToFind = 21 - (userTotal - valueOfCard);

                                Card magicCard = null;

                                var cardInUseIds = listGameCard.Select(x => x.Card.Id).ToList();

                                var availableCards = unitOfWork.CardRepository.Get().ToList().Where(x => !cardInUseIds.Contains(x.Id)).ToList();

                                if (cardToFind > 11)
                                {
                                    magicCard = availableCards.FirstOrDefault(x => x.CardNumberValue > 9);
                                }
                                else
                                {
                                    magicCard = availableCards.OrderByDescending(x => x.CardNumberValue).FirstOrDefault(x => x.CardNumberValue <= cardToFind);
                                }

                                if (magicCard != null)
                                {
                                    GameCard gc = new GameCard();
                                    gc.User = unitOfWork.UserRepository.GetByID(computer.Id);
                                    gc.Game = unitOfWork.GameRepository.GetByID(currentGame.Id);
                                    gc.Card = unitOfWork.CardRepository.GetByID(magicCard.Id);
                                    gc.OrderSeq = cardToReplace.OrderSeq;
                                    gc.ShowNumberedSide = cardToReplace.ShowNumberedSide;//dont show numbered side apart from current user
                                    gc.IsABlank = cardToReplace.IsABlank; //Assume card is a blank apart for current user
                                    gc.Status = "USERSSTACK";

                                    var listGameCardMagic = new List<GameCard> { gc };

                                    foreach (GameCard gameCard in listGameCardMagic)
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
                                        //unitOfWork.Save();
                                    }

                                    unitOfWork.GameCardRepository.Delete(cardToReplace);

                                    try
                                    {
                                        unitOfWork.Save();
                                        //gameToShowNumberSide.Add(gameChanger);
                                    }
                                    catch (Exception)
                                    {

                                    }

                                }
                            }

                           
                        }
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

                if(gu.User.UserName.ToUpper().StartsWith("CLOUD"))
                {
                   
                }

                if (cds.Count == 0)
                    gameIsOver = true;

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClickedComputerGame", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }
                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card, Status = c.Status });
                    }
                    else
                    {
                        var action = string.Empty;
                        cmPlayedLst.Add(new CardModel { ActionUrl = action, Card = c.Card, Status = c.Status});
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
                            finishedPickingAction = Url.Action("PlayAgainstComputerComputerStart", "Home21", new { prevId = currentGame.Id }).ToString();
                        }
                        else if (currentGame.Status == "USERTOPLAY")
                        {
                            currentGame.Status = "COMPUTERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputer", "Home21").ToString();
                        }
                        else
                        {
                            currentGame.Status = "USERTOPLAY";
                            finishedPickingAction = Url.Action("PlayAgainstComputerComputerStart", "Home21", new { prevId = currentGame.Id }).ToString();
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
                        cmLst.Where(x => x.Status.ToUpper().StartsWith("AGBO")).ToList().ForEach(x => x.Card.ShowNumberedSide = true);                   
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
                return RedirectToAction("GetMyCardsNonAjax", new { gameId = gameId });
            }

            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

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
                        var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
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
                        contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();


                        if (gu.User.Id == AgboFloor.Id)
                            finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                    }

                    if (floorFinishClicked.HasValue && floorFinishClicked.Value)
                    {
                        cmLst.ForEach(x => x.ActionUrl = "");
                        canClickFinishText = "Start New Game";
                        canClickContestText = "Contest Game";
                        contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();

                        if (gu.User.Id == AgboFloor.Id)
                            finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();
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
            return RedirectToAction("Index", "Home21");

            bool thisIsAContestedStage = false;

            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var agboFloorCards = listGameCard.Where(x => x.User.Id == AgboFloor.Id).Count();

            if (agboFloorCards == 0)
            {
                gameIsOver = true;
            }

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && currentUser.Id == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var grandTotal = -1;

            List<GameUser> supperWinner = new List<GameUser>();

            User contestedGameWinner = null;

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
                    }
                    else if (userTotal == grandTotal)
                    {
                        andTheWinnerIs = "ITS A TIE";
                    }
                }

                if (supperWinner.Count > 1)
                {
                    andTheWinnerIs = "ITS A TIE";
                }
                else if (supperWinner.Count == 1)
                {
                    andTheWinnerIs = supperWinner.FirstOrDefault().User.UserName;
                }

                var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

                if (gpn.Contested)
                {
                    thisIsAContestedStage = true;

                    var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
                    var allCorrectPlayedCards = listGameCard.Where(x => x.TradingFloorCard && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                    if (allCorrectPlayedCards.Count == (gameUsers.Count - 1))
                    {
                        andTheWinnerIs = "Game contested, contest is invalid, all the players answered to the calling cards.";
                    }
                    else
                    {
                        var tradingFloor = listGameCard.Where(x => !x.TradingFloorCard && x.User.Id != AgboFloor.Id).ToList();
                        var allFloorCardsVioalting = listGameCard.Where(x => x.Status == "USERSSTACK" && x.User.Id != AgboFloor.Id && !x.TradingFloorCard && x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                        if (allFloorCardsVioalting.Count > 0)
                        {
                            andTheWinnerIs = "Game contested, contest is valid, and the new winner is " + gameFirstCard.User.UserName;
                            contestedGameWinner = gameFirstCard.User;
                        }
                    }
                }
            }


            string finishedPickingAction = string.Empty;
            string contestAction = string.Empty;

            var globalOverride = false;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                if (cds.Count == 0)
                {
                    gameIsOver = true;
                    globalOverride = true;
                }

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();

                List<CardModel> cmPlayedLst = new List<CardModel>();

                var agboFirstCardIncluded = false;

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }

                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "Done!";
                var canClickContestText = "";

                if (gameIsOver)
                {
                    if (globalOverride)
                    {
                        cmLst.ForEach(x => x.ActionUrl = "");
                        canClickFinishText = "Start New Game";
                        canClickContestText = "Contest Game";
                        contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();

                        if (gu.User.Id == AgboFloor.Id)
                            finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                    }

                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    if (gu.User.Id == AgboFloor.Id)
                        finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                }
                else
                {
                    contestAction = string.Empty;

                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
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

            string topView = RenderRazorViewToString("_CardSharer", cardOwners);

            string topMessage = string.Empty;

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
            }

            IndexViewModel21 indexViewModel = new IndexViewModel21();

            indexViewModel.CardOwners = cardOwners;

            indexViewModel.GameId = currentGame.Id;

            var globalUrlAction = Url.Action("GetMyCards", "Home21", new { gameId = currentGame.Id }).ToString();

            indexViewModel.GlobalUrlAction = globalUrlAction;

            indexViewModel.CurrentUserName = currentUser.UserName;

            return View("StartNewGame", indexViewModel);
        }

        [HttpGet]
        public ActionResult GetMyCards(int? gameId)
        {
            bool thisIsAContestedStage = false;

            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var agboFloorCards = listGameCard.Where(x => x.User.Id == AgboFloor.Id).Count();

            if (agboFloorCards == 0)
            {
                gameIsOver = true;
            }

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && currentUser.Id == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var grandTotal = -1;

            List<GameUser> supperWinner = new List<GameUser>();

            User contestedGameWinner = null;

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
                    }
                    else if (userTotal == grandTotal)
                    {
                        andTheWinnerIs = "ITS A TIE";
                    }
                }

                if (supperWinner.Count > 1)
                {
                    andTheWinnerIs = "ITS A TIE";
                }
                else if (supperWinner.Count == 1)
                {
                    andTheWinnerIs = supperWinner.FirstOrDefault().User.UserName;
                }

                var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

                if (gpn.Contested)
                {
                    thisIsAContestedStage = true;

                    var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
                    var allCorrectPlayedCards = listGameCard.Where(x => x.TradingFloorCard && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                    if (allCorrectPlayedCards.Count == (gameUsers.Count - 1))
                    {
                        andTheWinnerIs = "Game contested, contest is invalid, all the players answered to the calling cards.";
                    }
                    else
                    {
                        var tradingFloor = listGameCard.Where(x => !x.TradingFloorCard && x.User.Id != AgboFloor.Id).ToList();
                        var allFloorCardsVioalting = listGameCard.Where(x => x.Status == "USERSSTACK" && x.User.Id != AgboFloor.Id && !x.TradingFloorCard && x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                        if (allFloorCardsVioalting.Count > 0)
                        {
                            andTheWinnerIs = "Game contested, contest is valid, and the new winner is " + gameFirstCard.User.UserName;
                            contestedGameWinner = gameFirstCard.User;
                        }
                    }
                }
            }


            string finishedPickingAction = string.Empty;
            string contestAction = string.Empty;

            var globalOverride = false;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                if (cds.Count == 0)
                {
                    gameIsOver = true;
                    globalOverride = true;
                }

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();

                List<CardModel> cmPlayedLst = new List<CardModel>();

                var agboFirstCardIncluded = false;

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }

                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "Done!";
                var canClickContestText = "";

                if (gameIsOver)
                {
                    if (globalOverride)
                    {
                        cmLst.ForEach(x => x.ActionUrl = "");
                        canClickFinishText = "Start New Game";
                        canClickContestText = "Contest Game";
                        contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                        currentGame.IsActive = false;
                        unitOfWork.GameRepository.Update(currentGame);
                        unitOfWork.Save();

                        if (gu.User.Id == AgboFloor.Id)
                            finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                    }

                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "Contest Game";
                    contestAction = Url.Action("GameContested", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = false }).ToString();

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();
                    if (gu.User.Id == AgboFloor.Id)
                        finishedPickingAction = Url.Action("StartNewGame", "Home21", new { previousGameId = currentGame.Id, userId = userPlayingNow.User.Id }).ToString();

                }
                else
                {
                    contestAction = string.Empty;

                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
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

            string topView = RenderRazorViewToString("_CardSharer", cardOwners);

            string topMessage = string.Empty;

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
            }

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMyCardsComputerGame(int? gameId)
        {
            bool thisIsAContestedStage = false;

            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var agboFloorCards = listGameCard.Where(x => x.User.Id == AgboFloor.Id).Count();

            if (agboFloorCards == 0)
            {
                gameIsOver = true;
            }

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && currentUser.Id == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var grandTotal = -1;

            List<GameUser> supperWinner = new List<GameUser>();

            var computer = GetUser("CLOUD");

            var thisUserHasMuttude = false;

            GameCard gameFirstCard = null;

            if (gameIsOver)
            {
                bool winnerFound = false;

                var gameIsOverUserList = gameUsers.Where(x => x.User.Id != AgboFloor.Id).ToList();

                gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);

                if (gameFirstCard.User.Id == computer.Id)
                {
                    //var get all trading floor cards 
                    var tfcards = listGameCard.Where(x => x.TradingFloorCard).ToList();
                    if (tfcards.Where(x => x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).Count() < 2)
                    {
                        //var possibleCheatCards = listGameCard.Where(x => x.User.Id != computer.Id && !x.TradingFloorCard).ToList();
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
                    }
                    else if (userTotal == grandTotal)
                    {
                        andTheWinnerIs = "ITS A TIE";
                    }
                }

                if (supperWinner.Count > 1)
                {
                    andTheWinnerIs = "ITS A TIE";
                }
                else if (supperWinner.Count == 1)
                {
                    andTheWinnerIs = supperWinner.FirstOrDefault().User.UserName;
                }

                if (thisUserHasMuttude)
                {
                    andTheWinnerIs = "You have muttued and not answered the calling card,  The Winner is Computer";
                    //gameWinner = computer;
                }

                var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

                if (gpn.Contested)
                {
                    thisIsAContestedStage = true;

                    gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
                    var allCorrectPlayedCards = listGameCard.Where(x => x.TradingFloorCard && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                    if (allCorrectPlayedCards.Count == (gameUsers.Count - 1))
                    {
                        andTheWinnerIs = "Game contested, contest is invalid, all the players answered to the calling cards.";
                    }
                    else
                    {
                        var tradingFloor = listGameCard.Where(x => !x.TradingFloorCard && x.User.Id != AgboFloor.Id).ToList();
                        var allFloorCardsVioalting = listGameCard.Where(x => x.Status == "USERSSTACK" && x.User.Id != AgboFloor.Id && !x.TradingFloorCard && x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                        if (allFloorCardsVioalting.Count > 0)
                        {
                            andTheWinnerIs = "Game contested, contest is valid, and the new winner is " + gameFirstCard.User.UserName;
                        }
                    }
                }
            }


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

                var agboFirstCardIncluded = false;

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClickedComputerGame", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }

                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "Done!";
                var canClickContestText = "";

                if (gameIsOver)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "";

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();


                    if (gu.User.Id == AgboFloor.Id)
                        finishedPickingAction = Url.Action("PlayAgainstComputer", "Home21").ToString();
                }
                else
                {
                    contestAction = string.Empty;

                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
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

        [HttpGet]
        public ActionResult GetMyCardsComputerGameNonAjax(int? gameId)
        {
            return RedirectToAction("Index", "Home21");

            var thisUserName = User.Identity.Name;

            bool thisIsAContestedStage = false;

            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            var playBalance = currentUser.RealMoneyBalance;

            if (playBalance < 1)
            {
                return RedirectToAction("ZeroBalance");
            }

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var gameIsOver = gameUsers.Where(x => x.Finished == 1).Count() == gameUsers.Count();

            gameUsers.Add(agboFloorgameUser);

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var agboFloorCards = listGameCard.Where(x => x.User.Id == AgboFloor.Id).Count();

            if (agboFloorCards == 0)
            {
                gameIsOver = true;
            }

            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && currentUser.Id == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            string andTheWinnerIs = gameUsers.FirstOrDefault().User.UserName;

            var grandTotal = -1;

            List<GameUser> supperWinner = new List<GameUser>();

            var computer = GetUser("CLOUD");

            var thisUserHasMuttude = false;

            GameCard gameFirstCard = null;

            if (gameIsOver)
            {
                bool winnerFound = false;

                var gameIsOverUserList = gameUsers.Where(x => x.User.Id != AgboFloor.Id).ToList();

                gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);

                if (gameFirstCard.User.Id == computer.Id)
                {
                    //var get all trading floor cards 
                    var tfcards = listGameCard.Where(x => x.TradingFloorCard).ToList();
                    if (tfcards.Where(x => x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).Count() < 2)
                    {
                        //var possibleCheatCards = listGameCard.Where(x => x.User.Id != computer.Id && !x.TradingFloorCard).ToList();
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
                    }
                    else if (userTotal == grandTotal)
                    {
                        andTheWinnerIs = "ITS A TIE";
                    }
                }

                if (supperWinner.Count > 1)
                {
                    andTheWinnerIs = "ITS A TIE";
                }
                else if (supperWinner.Count == 1)
                {
                    andTheWinnerIs = supperWinner.FirstOrDefault().User.UserName;
                }

                if (thisUserHasMuttude)
                {
                    andTheWinnerIs = "You have muttued and not answered the calling card,  The Winner is Computer";
                    //gameWinner = computer;
                }

                var gpn = unitOfWork.GamePlayingNowRepository.Get().Where(x => x.Game.Id == currentGame.Id).FirstOrDefault();

                if (gpn.Contested)
                {
                    thisIsAContestedStage = true;

                    gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
                    var allCorrectPlayedCards = listGameCard.Where(x => x.TradingFloorCard && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                    if (allCorrectPlayedCards.Count == (gameUsers.Count - 1))
                    {
                        andTheWinnerIs = "Game contested, contest is invalid, all the players answered to the calling cards.";
                    }
                    else
                    {
                        var tradingFloor = listGameCard.Where(x => !x.TradingFloorCard && x.User.Id != AgboFloor.Id).ToList();
                        var allFloorCardsVioalting = listGameCard.Where(x => x.Status == "USERSSTACK" && x.User.Id != AgboFloor.Id && !x.TradingFloorCard && x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                        if (allFloorCardsVioalting.Count > 0)
                        {
                            andTheWinnerIs = "Game contested, contest is valid, and the new winner is " + gameFirstCard.User.UserName;
                        }
                    }
                }
            }


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

                var agboFirstCardIncluded = false;

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClickedComputerGame", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            finishedPickingAction = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
                                agboFirstCardIncluded = true;
                            }

                        }

                        cmLst.Add(new CardModel { ActionUrl = action, Card = c.Card });
                    }
                    else
                    {
                        cmPlayedLst.Add(new CardModel { ActionUrl = "", Card = c.Card });
                    }
                }

                var blankCard = unitOfWork.CardRepository.Get().Where(x => x.Rank.Id == 10 && x.Suit.Id == 5).FirstOrDefault();

                string playingNow = @"border:5px Solid Red;";

                if (gu.OrderSequence == 1)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (timeToPickFromAgboFloor && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                var canClickFinishText = "Done!";
                var canClickContestText = "";

                if (gameIsOver)
                {
                    cmLst.ForEach(x => x.ActionUrl = "");
                    canClickFinishText = "Start New Game";
                    canClickContestText = "";

                    currentGame.IsActive = false;
                    unitOfWork.GameRepository.Update(currentGame);
                    unitOfWork.Save();


                    if (gu.User.Id == AgboFloor.Id)
                        finishedPickingAction = Url.Action("PlayAgainstComputer", "Home21").ToString();
                }
                else
                {
                    contestAction = string.Empty;

                    if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
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

            IndexViewModel21 indexViewModel = new IndexViewModel21();

            indexViewModel.CardOwners = cardOwners;

            indexViewModel.GameId = currentGame.Id;

            var globalUrlAction = Url.Action("GetMyCardsComputerGame", "Home21", new { gameId = currentGame.Id }).ToString();

            indexViewModel.GlobalUrlAction = globalUrlAction;

            indexViewModel.CurrentUserName = currentUser.UserName;

            return View("StartNewGame", indexViewModel);
        }


        [HttpGet]
        public ActionResult GameCardClickedComputerGame(int? gameId, int? userId, int? cardId)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("GetMyCardsComputerGameNonAjax", new { gameId = gameId });
            }

            //var thisUserName = User.Identity.Name;
            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            var computerUser = GetUser("CLOUD");

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gameCardPlayed = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == currentGame.Id && x.Card.Id == cardId).FirstOrDefault();

            var noFirstCardClicked = unitOfWork.GameCardRepository.Get().Where(x => x.TradingFloorCard && x.Game.Id == currentGame.Id).Count() == 0;

            if (noFirstCardClicked)
            {
                gameCardPlayed.IsABlank = false;// Change this property name
            }

            gameCardPlayed.ShowNumberedSide = true;
            gameCardPlayed.TradingFloorCard = true;
            unitOfWork.GameCardRepository.Update(gameCardPlayed);
            unitOfWork.Save();

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

       

            var overrideCanClickFinish = false;

            //if (floorCount == gameUsers.Count)
            //{
            //    var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
            //    var allFloorCards = listGameCard.Where(x => x.TradingFloorCard && x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
            //    var seniorCard = allFloorCards.Where(x => x.Card.CardNumberValue > gameFirstCard.Card.CardNumberValue).OrderByDescending(x => x.Card.CardNumberValue).FirstOrDefault();

            //    if (seniorCard != null)
            //    {
            //        var seniorUser = seniorCard.User;
            //        gameUsers = RearrangeGameSequence(gameUsers, currentGame.Id, seniorUser);
            //        doRearrange = false;
            //        overrideCanClickFinish = true;
            //    }
            //}

            var allComputerCards = listGameCard.Where(x => x.User.Id == computerUser.Id).ToArray();

            var computerCanClickNow = false;

            GameCard computersCard = null;

            if (!allComputerCards.Any(x => x.TradingFloorCard))
            {
                computersCard = GetComputersResponseNew(currentUser, gameCardPlayed, listGameCard.ToArray(), allComputerCards);

                computersCard.ShowNumberedSide = true;
                computersCard.TradingFloorCard = true;
                unitOfWork.GameCardRepository.Update(computersCard);
                unitOfWork.Save();
            }
            else
            {
                computersCard = allComputerCards.FirstOrDefault(x => x.TradingFloorCard);
            }

            var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);

            if (computersCard.Card.Id == gameFirstCard.Card.Id)
            {
                if (gameCardPlayed.Card.Suit.Name == computersCard.Card.Suit.Name && gameCardPlayed.Card.CardNumberValue > computersCard.Card.CardNumberValue)
                {
                    computerCanClickNow = false;
                }
                else
                {
                    computerCanClickNow = true;
                }
            }
            else
            {
                if (computersCard.Card.CardNumberValue > gameFirstCard.Card.CardNumberValue && gameFirstCard.Card.Suit.Id == computersCard.Card.Suit.Id)
                {
                    computerCanClickNow = true;
                }
            }

            var gameToShowNumberSide = new List<GameCard>();

            if (computerCanClickNow)
            {
                var computer = GetUser("CLOUD");

                var compuserGameCards = listGameCard.Where(x => x.User.Id == computer.Id && !x.TradingFloorCard).ToList();

                var floorCards = new List<GameCard>();

                if (compuserGameCards.Count == 2)
                {
                    var gameCards = GetComputerSomeCards(compuserGameCards, listGameCard.Where(x => x.User.Id == AgboFloor.Id && !x.TradingFloorCard).ToList());

                    foreach (var gc in gameCards)
                    {
                        gc.User = computer;
                        unitOfWork.GameCardRepository.Update(gc);
                        unitOfWork.Save();
                    }

                    gameToShowNumberSide = gameCards;
                    floorCards = gameCards;
                }

                //Supper thinking
                var nowRandy = new Random().Next(1, 10);

                if (nowRandy <= 4)
                {
                    var thekokoGameCards = listGameCard.Where(x => x.User.Id == computer.Id && !x.TradingFloorCard).ToList();

                    var winnerFound = false;

                    var userTotal = CalculateUserTotal(thekokoGameCards.ToArray(), ref winnerFound);

                    if (userTotal < 20)
                    {
                        var replaceableCards = thekokoGameCards.Where(x => !floorCards.Contains(x)).ToList();

                        var cardToReplace = replaceableCards.FirstOrDefault(x => x.Card.CardNumberValue != 11);

                        if (cardToReplace != null)
                        {
                            var valueOfCard = cardToReplace.Card.CardNumberValue;

                            var cardToFind = 21 - (userTotal - valueOfCard);

                            Card magicCard = null;

                            var cardInUseIds = listGameCard.Select(x => x.Card.Id).ToList();

                            var availableCards = unitOfWork.CardRepository.Get().ToList().Where(x => !cardInUseIds.Contains(x.Id)).ToList();

                            if (cardToFind > 11)
                            {
                                magicCard = availableCards.FirstOrDefault(x => x.CardNumberValue > 9);
                            }
                            else
                            {
                                magicCard = availableCards.OrderByDescending(x => x.CardNumberValue).FirstOrDefault(x => x.CardNumberValue <= cardToFind);
                            }

                            

                            if (magicCard != null)
                            {
                                GameCard gc = new GameCard();                                
                                gc.User = unitOfWork.UserRepository.GetByID(computer.Id);
                                gc.Game = unitOfWork.GameRepository.GetByID(currentGame.Id);
                                gc.Card = unitOfWork.CardRepository.GetByID(magicCard.Id);
                                gc.OrderSeq = cardToReplace.OrderSeq;
                                gc.ShowNumberedSide = cardToReplace.ShowNumberedSide;//dont show numbered side apart from current user
                                gc.IsABlank = cardToReplace.IsABlank; //Assume card is a blank apart for current user
                                gc.Status = "USERSSTACK";

                                var listGameCardMagic = new List<GameCard> { gc };


                                foreach (GameCard gameCard in listGameCardMagic)
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
                                }

                                unitOfWork.GameCardRepository.Delete(cardToReplace);

                                try
                                {
                                    unitOfWork.Save();
                                }
                                catch (Exception)
                                {
                                }

                            }
                        }
                    }
                }                
            }

            listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var whoIsPlayingMessage = gameUsers.Where(x => x.User.Id == currentUser.Id).FirstOrDefault().User.UserName;

            gameUsers.Add(agboFloorgameUser);

            var timeToPickFromAgboFloor = true;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            var agboFirstCardIncluded = false;

            var finishedPickingAction = string.Empty;

            var canClickFinishText = "";

            overrideCanClickFinish = true;

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClickedComputerGame", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            if (overrideCanClickFinish)
                            {
                                canClickFinishText = "Done!";
                                finishedPickingAction = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            }

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClickedComputerGame", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
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

                var thisUserIsPlayingNow = (gu.User.Id == AgboFloor.Id);

                if ((gameUsers.Count - 1) == floorCount && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
                {
                    var ids = gameToShowNumberSide.Select(x => x.Card.Id).ToList();
                    //hide this users cards                   
                    cmLst.Where(x => !ids.Contains(x.Card.Id)).ToList().ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                var playingNowFlashMessage = string.Empty;

                if (thisUserIsPlayingNow)
                {
                    playingNowFlashMessage = "If you choose to, please click a floor card in a clockwise direction. See direction of arrow.";
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, CanShowFlashMessage = thisUserIsPlayingNow, CanShowFlashMessageMessage = playingNowFlashMessage, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
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

        public GameCard GetComputersResponseNew(User homeuser, GameCard PlayersCard, GameCard[] CardStackers, GameCard[] usersStack)
        {
            string strFirstLetter = PlayersCard.Card.Suit.Name;

            List<GameCard> possibleChoices = new List<GameCard>();
            Dictionary<decimal, GameCard> dict = new Dictionary<decimal, GameCard>();

            possibleChoices = usersStack.Where(x => x.Card.Suit.Name == strFirstLetter).ToList();

            if (possibleChoices.Count == 1)
            {
                return possibleChoices.FirstOrDefault();
            }

            if (possibleChoices.Count == 2)// There is a flaw here, ut should also take into consideration the possible floor cards
            {
                var testCards = possibleChoices;

                Dictionary<decimal, GameCard> TryCardPickDict = new Dictionary<decimal, GameCard>();

                foreach (var myCard in testCards)
                {
                    var sumCards = usersStack.Where(x => x.Card.Id != myCard.Card.Id).ToList();
                    decimal totalWithOutThisCard = sumCards.Sum(x => x.Card.CardNumberValue);

                    try
                    {
                        TryCardPickDict.Add(totalWithOutThisCard, myCard);
                    }
                    catch
                    {
                    }
                }

                if(TryCardPickDict.Count > 0)
                {
                    var lowestValueToUs = TryCardPickDict.Keys.ToList().OrderByDescending(x => x).LastOrDefault();
                    return TryCardPickDict[lowestValueToUs];
                }  
            }

            if (possibleChoices.Count == 0 && (usersStack.Count(x => x.Card.CardNumberValue == 11) == 2))
            {
                return usersStack.FirstOrDefault(x => x.Card.CardNumberValue != 11);
            }




            if (PlayersCard.Card.CardNumberValue >= 8)//Most likely pick first Card
            {
                if (DateTime.Now.Second > 5)
                {
                    return PlaySecondHand(CardStackers, usersStack, true);
                }
                else
                {
                    return PlaySecondHand(CardStackers, usersStack, false);
                }
            }
            else
            {
                if (DateTime.Now.Second > 50)
                {
                    return PlaySecondHand(CardStackers, usersStack, true);
                }
                else
                {
                    return PlaySecondHand(CardStackers, usersStack, false);
                }
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
            ArrayList arAces = new ArrayList();
            ArrayList arNoAces = new ArrayList();

            foreach (var gCard in usersStack)
            {
                bool ace = gCard.Card.CardNumberValue == 11;
                if (ace)
                {
                    arAces.Add(gCard);
                }
            }

            if (arAces.Count == 2)
            {
                if (usersStack.Count > 2)
                {
                    return usersStack.FirstOrDefault(x => x.Card.CardNumberValue != 11);
                }
            }
            else if (arAces.Count == 3)
            {
                return usersStack.FirstOrDefault();
            }

            return null;
        }


        [HttpGet]
        public ActionResult GameCardClicked(int? gameId, int? userId, int? cardId)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("GetMyCardsNonAjax", new { gameId = gameId });
            }

            var thisUserName = User.Identity.Name;


            var currentUser = unitOfWork.UserRepository.Get().Where(u => u.UserName.ToUpper() == thisUserName.ToUpper()).FirstOrDefault();

            List<CardOwner> cardOwners = new List<CardOwner>();

            var gameUsers = unitOfWork.GameUserRepository.Get().Where(x => x.Game21.Id == gameId).ToList();

            var currentGame = gameUsers.FirstOrDefault().Game21;

            var gameCardPlayed = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == currentGame.Id && x.Card.Id == cardId).FirstOrDefault();

            var noFirstCardClicked = unitOfWork.GameCardRepository.Get().Where(x => x.TradingFloorCard && x.Game.Id == currentGame.Id).Count() == 0;

            if (noFirstCardClicked)
            {
                gameCardPlayed.IsABlank = false;// Change this property name
            }

            gameCardPlayed.ShowNumberedSide = true;
            gameCardPlayed.TradingFloorCard = true;
            unitOfWork.GameCardRepository.Update(gameCardPlayed);
            unitOfWork.Save();

            User AgboFloor = GetUser("AGBO FLOOR");

            var agboFloorgameUser = new GameUser { User = AgboFloor, Game21 = currentGame };

            var listGameCard = unitOfWork.GameCardRepository.Get().Where(x => x.Game.Id == gameId).ToList();

            var floorCount = listGameCard.Where(x => x.TradingFloorCard).Count();

            var doRearrange = true;

            var overrideCanClickFinish = false;

            if (floorCount == gameUsers.Count)
            {
                var gameFirstCard = listGameCard.FirstOrDefault(x => !x.IsABlank);
                var allFloorCards = listGameCard.Where(x => x.TradingFloorCard && x.Id != gameFirstCard.Id && x.Card.Suit.Id == gameFirstCard.Card.Suit.Id).ToList();
                var seniorCard = allFloorCards.Where(x => x.Card.CardNumberValue > gameFirstCard.Card.CardNumberValue).OrderByDescending(x => x.Card.CardNumberValue).FirstOrDefault();

                if (seniorCard != null)
                {
                    var seniorUser = seniorCard.User;
                    gameUsers = RearrangeGameSequence(gameUsers, currentGame.Id, seniorUser);
                    doRearrange = false;
                    overrideCanClickFinish = true;
                }
            }

            if (doRearrange)
            {
                gameUsers = RearrangeGameSequence(gameUsers, currentGame.Id);
            }


            var whoIsPlayingMessage = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault().User.UserName;

            gameUsers.Add(agboFloorgameUser);

            var timeToPickFromAgboFloor = false;

            var userPlayingNow = gameUsers.Where(x => x.OrderSequence == 1).FirstOrDefault();

            if ((gameUsers.Count - 1) == floorCount && userId == userPlayingNow.User.Id)
            {
                timeToPickFromAgboFloor = true;
            }

            var agboFirstCardIncluded = false;

            var finishedPickingAction = string.Empty;
            var canClickFinishText = "";

            foreach (var gu in gameUsers)
            {
                var cds = listGameCard.Where(x => x.User.Id == gu.User.Id).ToList();

                var userhasDroppedACard = cds.Where(x => x.TradingFloorCard).Any();

                List<CardModel> cmLst = new List<CardModel>();
                List<CardModel> cmPlayedLst = new List<CardModel>();

                foreach (var c in cds)
                {
                    if (!c.TradingFloorCard)
                    {
                        var action = Url.Action("GameCardClicked", "Home21", new { gameId = gu.Game21.Id, userId = gu.User.Id, cardId = c.Card.Id }).ToString();

                        if (userhasDroppedACard || gu.OrderSequence != 1 || timeToPickFromAgboFloor)
                        {
                            action = "";
                        }

                        if (gu.User.Id == AgboFloor.Id && timeToPickFromAgboFloor)
                        {
                            action = string.Empty;

                            if (overrideCanClickFinish)
                            {
                                canClickFinishText = "Done!";
                                finishedPickingAction = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = 0, floorFinishClicked = true }).ToString();
                            }

                            if (!agboFirstCardIncluded)
                            {
                                action = Url.Action("AgboFloorCardClicked", "Home21", new { gameId = currentGame.Id, userId = userPlayingNow.User.Id, cardId = c.Card.Id }).ToString();
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

                if ((gameUsers.Count - 1) == floorCount && gu.User.Id == AgboFloor.Id)
                {
                    playingNow = @"border:5px Solid Green;";
                }

                if (gu.User.Id != AgboFloor.Id && gu.User.Id != currentUser.Id)
                {
                    //hide this users cards                   
                    cmLst.ForEach(x => x.Card = blankCard);
                    cmLst.ForEach(x => x.ActionUrl = "");
                }

                cardOwners.Add(new CardOwner { GameStake = currentGame.ModifiedBy, CanClickFinishText = canClickFinishText, CanClickFinish = finishedPickingAction, WhoIsPlayingMessage = whoIsPlayingMessage, PlayingNow = playingNow, Owner = gu.User, CardModelsPlayingStack = cmLst, CardModelsPlayedStack = cmPlayedLst });
            }

            string topView = RenderRazorViewToString("_CardSharer", cardOwners);

            string topMessage = string.Empty;

            var thisGuyCardOwner = cardOwners.FirstOrDefault(x => x.Owner.Id == AgboFloor.Id);

            if (thisGuyCardOwner.CanClickFinishText == "Done!" && !string.IsNullOrEmpty(thisGuyCardOwner.CanClickFinish))
                topMessage = "info";

            return Json(new { TopView = topView, TopMessage = topMessage }, JsonRequestBehavior.AllowGet);

            //return Json(new { TopView = topView }, JsonRequestBehavior.AllowGet);     
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

        private MultiUserStackOfCards GetMultiUserPlayingCards(GameUser[] gameUsers)
        {

            int numOfUsers = gameUsers.Length;

            int MaxResult = 9;

            if (numOfUsers == 2)
                MaxResult = 9;
            else if (numOfUsers == 3)
                MaxResult = 13;
            else
                MaxResult = 17;



            IList<Card> list = null;
            IList<Card> list1 = null;

            try
            {
                //ITransaction transaction = session.BeginTransaction();
                var cards = unitOfWork.CardRepository.Get().Where(x => x.Suit.Id != 5).OrderBy(x => Guid.NewGuid()).ToList();

                list = cards.Take(MaxResult).ToList();

                list1 = cards.Take(21).ToList();

                return new MultiUserStackOfCards((List<Card>)list, (List<Card>)list1, gameUsers);
            }
            catch
            {
                return new MultiUserStackOfCards((List<Card>)list, (List<Card>)list1, gameUsers);
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
            MultiUserStackOfCards msoc = GetMultiUserPlayingCards(gameUsers);

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

        private Game21 CreateNewGameWithUserId(GameUser[] gameUsers, bool silentMode, int totalNumOfPlayers, decimal gameStake = 250)
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
            var thisUserName = User.Identity.Name;

            //game.GameUsers = gameUsers;
            GamePlayingNow gpn = new GamePlayingNow();
            gpn.Game = game;
            gpn.GameStage = 1;
            gpn.ValueNum = 1;
            gpn.CreatedBy = thisUserName;
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
