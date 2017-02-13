using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Entitities;

namespace HotelMateWebV1.Helpers
{
    public class StackOfCards
    {
        private List<Card> cardStack = null;
        private List<Card> usersStack = null;
        private List<Card> computersStack = null;
        private List<Card> computerUserStack = null;
        private List<Card> homePageStack = null;


        public StackOfCards(List<Card> cards, List<Card> AllPlayingCards)
        {
            cardStack = new List<Card>();
            usersStack = new List<Card>();
            computersStack = new List<Card>();
            computerUserStack = new List<Card>();
            homePageStack = new List<Card>();

            foreach (Card card in AllPlayingCards)
            {
                card.IsABlank = false;
                homePageStack.Add(card);// = AllPlayingCards;
            }

            int TotalCount = 0;
            int rowCount = 0;

            foreach (Card card in cards)
            {
                if (TotalCount == 6)
                    break;

                cardStack.Add(card);

                if (rowCount % 2 == 0)
                {
                    usersStack.Add(card);
                    TotalCount++;
                }
                else
                {
                    card.IsABlank = true;
                    computersStack.Add(card);
                    TotalCount++;
                }
                rowCount++;
            }


            List<Card> cardStack1 = cardStack;

            int len = cards.Count;

            if (len > 8)
            {
                for (int i = 6; i < 9; i++)
                {
                    Card card = cards[i];
                    card.IsABlank = true;
                    card.ShowNumberedSide = false;
                    computerUserStack.Add(card);
                }
            }

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
        public List<Card> ComputerUserStack
        {
            set { computerUserStack = value; }
            get { return computerUserStack; }
        }
        public List<Card> UsersStack
        {
            set { usersStack = value; }
            get { return usersStack; }
        }
    }
}