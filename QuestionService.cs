using QuizServer.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace QuizServer.Services
{
    public class QuestionService
    {
        public List<Question> GetQuestionsForDifficulty(int difficulty)
        {
            int soDe = 0, soTB = 0, soKho = 0;

            if (difficulty == 1)
            {
                soDe = 8;
                soTB = 5;
                soKho = 2;
            }
            else if (difficulty == 2)
            {
                soDe = 10;
                soTB = 7;
                soKho = 3;
            }
            else
            {
                soDe = 10;
                soTB = 10;
                soKho = 10;
            }

            List<Question> ds = new List<Question>();
            ds.AddRange(GetRandomQuestionsByLevel(1, soDe));
            ds.AddRange(GetRandomQuestionsByLevel(2, soTB));
            ds.AddRange(GetRandomQuestionsByLevel(3, soKho));

            return ds;
        }

        private List<Question> GetRandomQuestionsByLevel(int level, int count)
        {
            List<Question> list = new List<Question>();

            using (SqlConnection conn = DBConnect.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT TOP (@count) 
                        MaCauHoi, NoiDung, A, B, C, D, DapAnDung, MucDo
                    FROM dbo.CauHoi
                    WHERE MucDo = @level
                    ORDER BY NEWID()";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@count", count);
                    cmd.Parameters.AddWithValue("@level", level);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Question q = new Question
                            {
                                MaCauHoi = (int)reader["MaCauHoi"],
                                NoiDung = reader["NoiDung"].ToString(),
                                A = reader["A"].ToString(),
                                B = reader["B"].ToString(),
                                C = reader["C"].ToString(),
                                D = reader["D"].ToString(),
                                DapAnDung = reader["DapAnDung"].ToString(),
                                MucDo = (int)reader["MucDo"]
                            };

                            list.Add(q);
                        }
                    }
                }
            }

            return list;
        }

        public int GetScoreByQuestionLevel(int level)
        {
            if (level == 1) return 10;
            if (level == 2) return 15;
            return 20;
        }

        public int GetTimeByDifficulty(int difficulty)
        {
            if (difficulty == 1) return 20;
            if (difficulty == 2) return 15;
            return 10;
        }
    }
}