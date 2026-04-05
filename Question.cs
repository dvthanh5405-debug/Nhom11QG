namespace QuizServer.Models
{
    public class Question
    {
        public int MaCauHoi { get; set; }
        public string NoiDung { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string D { get; set; }
        public string DapAnDung { get; set; }
        public int MucDo { get; set; }
    }
}