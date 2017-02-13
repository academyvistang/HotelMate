using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using HotelMateWebV1.Helpers;
using Agbo21.Dal;

namespace HotelMateWebV1.TheHub
{
    public class Agbo : Hub
    {
        private UnitOfWork unitOfWork = new UnitOfWork();       

        public void Send(string message, string groupName)
        {
            // Call the addMessage method on all clients            
            //Clients.Others.addMessage(message);
            //Clients.Group(groupName).addMessage(message);
            Clients.OthersInGroup(groupName).addMessage(message);            
        }

        public void Sendmessage(string name, string message, string groupName)
        {
            // Call the addMessage method on all clients   
            name = unitOfWork.UserRepository.Get().FirstOrDefault(x => x.UserName.ToUpper() == name.ToUpper()).UserPictureName;
            Clients.Group(groupName).broadcastMessage(name, message);
        }

        public void Join(string groupName)
        {
            Groups.Add(Context.ConnectionId, groupName);
            MySingleton.Instance.contextDictionary.Add(Context.ConnectionId, groupName);
        }

        public override Task OnConnected()
        {
            return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnDisconnected()
        {
            try
            {
                string str = base.Context.Request.Url.ToString();
               
                var value = MySingleton.Instance.contextDictionary[Context.ConnectionId];

                try
                {
                    int id = 0;
                    int.TryParse(value, out id);
                    var game = unitOfWork.GameRepository.GetByID(id);
                    var gameStatus = game.Status;
                    var gpn = unitOfWork.GamePlayingNowRepository.Get().FirstOrDefault(x => x.Game.Id == id);

                    if (gameStatus == "LIVE")
                    {
                        game.IsActive = false;
                        game.Status = "CLOSED";
                        game.TimeEnded = DateTime.Now;
                        unitOfWork.GameRepository.Update(game);
                        gpn.IsActive = false;
                        gpn.ValueNum = 999;
                        gpn.GameStage = 999;
                        gpn.Contested = false;
                        unitOfWork.GamePlayingNowRepository.Update(gpn);
                        unitOfWork.Save();
                        Clients.OthersInGroup(value).closeGame("Closed");                       
                        Clients.Client(Context.ConnectionId).closeGameRedirect("Closed");
                    }

                    //closeGame
                }
                catch
                {
                }
                
            }
            catch
            {
            }

            return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnReconnected()
        {
            return Clients.All.rejoined(Context.ConnectionId, DateTime.Now.ToString());
        }
        
    }
}