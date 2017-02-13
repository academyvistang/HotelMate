using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Entitities;

namespace Agbo21.Dal
{
    public class UnitOfWork : IDisposable
    {
        private Agbo21Context context = new Agbo21Context();
        private GenericRepository<Card> cardRepository;
        private GenericRepository<Game21> gameRepository;
        private GenericRepository<GameCard> gameCardRepository;
        private GenericRepository<GamePlayingNow> gamePlayingNowRepository;
        private GenericRepository<GameUser> gameUserRepository;
        private GenericRepository<Message> messageRepository;
        private GenericRepository<Rank> rankRepository;
        private GenericRepository<Suit> suitRepository;
        private GenericRepository<User> userRepository;
        private GenericRepository<UserAccount> userAccountRepository;

        public GenericRepository<Card> CardRepository
        {
            get
            {

                if (this.cardRepository == null)
                {
                    this.cardRepository = new GenericRepository<Card>(context);
                }
                return cardRepository;
            }
        }

        public GenericRepository<Game21> GameRepository
        {
            get
            {

                if (this.gameRepository == null)
                {
                    this.gameRepository = new GenericRepository<Game21>(context);
                }
                return gameRepository;
            }
        }

        public GenericRepository<GameCard> GameCardRepository
        {
            get
            {

                if (this.gameCardRepository == null)
                {
                    this.gameCardRepository = new GenericRepository<GameCard>(context);
                }
                return gameCardRepository;
            }
        }

        public GenericRepository<GamePlayingNow> GamePlayingNowRepository
        {
            get
            {

                if (this.gamePlayingNowRepository == null)
                {
                    this.gamePlayingNowRepository = new GenericRepository<GamePlayingNow>(context);
                }
                return gamePlayingNowRepository;
            }
        }

        public GenericRepository<GameUser> GameUserRepository
        {
            get
            {

                if (this.gameUserRepository == null)
                {
                    this.gameUserRepository = new GenericRepository<GameUser>(context);
                }
                return gameUserRepository;
            }
        }

        public GenericRepository<Message> MessageRepository
        {
            get
            {

                if (this.messageRepository == null)
                {
                    this.messageRepository = new GenericRepository<Message>(context);
                }
                return messageRepository;
            }
        }

        public GenericRepository<Rank> RankRepository
        {
            get
            {

                if (this.rankRepository == null)
                {
                    this.rankRepository = new GenericRepository<Rank>(context);
                }
                return rankRepository;
            }
        }

        public GenericRepository<Suit> SuitRepository
        {
            get
            {

                if (this.suitRepository == null)
                {
                    this.suitRepository = new GenericRepository<Suit>(context);
                }
                return suitRepository;
            }
        }

        public GenericRepository<User> UserRepository
        {
            get
            {

                if (this.userRepository == null)
                {
                    this.userRepository = new GenericRepository<User>(context);
                }
                return userRepository;
            }
        }

        public GenericRepository<UserAccount> UserAccountRepository
        {
            get
            {
                if (this.userAccountRepository == null)
                {
                    this.userAccountRepository = new GenericRepository<UserAccount>(context);
                }
                return userAccountRepository;
            }
        }        

        public void Save()
        {
            context.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
