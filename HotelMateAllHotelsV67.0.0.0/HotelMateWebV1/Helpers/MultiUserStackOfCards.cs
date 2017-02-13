using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entitities;

namespace HotelMateWebV1.Helpers
{
    public class MultiUserStackOfCards
    {
        private List<Card> computersStack = null;
        private List<Card> homePageStack = null;
        private List<User> usersList = null;

        public MultiUserStackOfCards(List<Card> Cards, List<Card> AllPlayingCards, GameUser[] gameUsers)
        {
            int numOfUsers = gameUsers.Length;

            int CutOff = 3 * numOfUsers;

            computersStack = new List<Card>();

            homePageStack = new List<Card>();

            UsersList = new List<User>(numOfUsers);

            foreach (GameUser gameuser in gameUsers)
            {
                User user = new User();
                user.Id = gameuser.User.Id;
                UsersList.Add(user);
            }

            int i = 0;

            List<Card> Card4Users = new List<Card>();

            int UsersTray = 0;

            int PutInUserTray = 2;

            foreach (Card Card in Cards)
            {
                Card.IsABlank = true;
                Card.ShowNumberedSide = false;

                if (i > (CutOff - 1))
                {
                    computersStack.Add(Card);
                }
                else
                {
                    Card4Users.Add(Card);

                    if (i == PutInUserTray)
                    {
                        foreach (Card Card1 in Card4Users)
                        {
                            if (UsersList[UsersTray].UsersStack == null)
                            {
                                UsersList[UsersTray].UsersStack = new List<Card>();
                            }

                            UsersList[UsersTray].UsersStack.Add(Card1);
                        }

                        Card4Users.Clear();
                        UsersTray++;
                        PutInUserTray += 3;
                    }
                }

                i++;
            }

        }


        public MultiUserStackOfCards(List<Card> Cards, List<Card> AllPlayingCards, int numOfUsers)
        {
            int CutOff = 3 * numOfUsers;

            computersStack = new List<Card>();

            homePageStack = new List<Card>();

            UsersList = new List<User>(numOfUsers);

            for (int p = 0; p < numOfUsers; p++)
            {
                User user = new User();
                UsersList.Add(user);
            }

            foreach (Card Card in AllPlayingCards)
            {
                Card.IsABlank = false;
                homePageStack.Add(Card);// = AllPlayingCards;
            }

            int i = 0;

            List<Card> Card4Users = new List<Card>();

            int UsersTray = 0;

            int PutInUserTray = 2;

            foreach (Card Card in Cards)
            {
                Card.IsABlank = true;
                Card.ShowNumberedSide = false;

                if (i > (CutOff - 1))
                {

                    computersStack.Add(Card);
                }
                else
                {
                    Card4Users.Add(Card);

                    if (i == PutInUserTray)
                    {
                        foreach (Card Card1 in Card4Users)
                        {
                            UsersList[UsersTray].UsersStack.Add(Card1);
                        }

                        Card4Users.Clear();
                        UsersTray++;
                        PutInUserTray += 3;
                    }
                }

                i++;
            }
        }
        // usersList
        public List<User> UsersList
        {
            set { usersList = value; }
            get { return usersList; }
        }

        public List<Card> HomePageStack
        {
            set { homePageStack = value; }
            get { return homePageStack; }
        }
        public List<Card> ComputersStack
        {
            set { computersStack = value; }
            get { return computersStack; }
        }


    }
}