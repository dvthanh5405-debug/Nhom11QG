using QuizServer.Models;
using System.Linq;

namespace QuizServer.Services
{
    public class GameService
    {
        public void PrepareGame(Room room)
        {
            foreach (Player p in room.Players)
            {
                p.TongDiem = 0;
                p.IsAlive = true;
                p.LastAnswer = "";
            }

            room.CurrentQuestionIndex = 0;
            room.IsPlaying = true;
        }

        public bool AllPlayersReady(Room room)
        {
            return room.Players.Count > 0 && room.Players.All(p => p.IsReady);
        }

        public int CountAlivePlayers(Room room)
        {
            return room.Players.Count(p => p.IsAlive);
        }
    }
}