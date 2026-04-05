using System.Collections.Generic;

namespace QuizServer.Models
{
    public class Room
    {
        public string RoomId { get; set; }
        public string Owner { get; set; }
        public int Difficulty { get; set; } = 1; // 1=dễ, 2=trung bình, 3=khó
        public bool IsPlaying { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int TimePerQuestion { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}