using System;
using System.Data.SqlClient;

namespace QuizServer.Services
{
    public class AuthService
    {
        public bool Login(string username, string password, out string email)
        {
            email = "";

            try
            {
                using (SqlConnection conn = DBConnect.GetConnection())
                {
                    conn.Open();

                    string sql = @"SELECT Email 
                                   FROM dbo.NguoiChoi 
                                   WHERE TenDangNhap = @u AND MatKhau = @p";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);

                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            email = result.ToString();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOI LOGIN: " + ex.Message);
            }

            return false;
        }

        public bool Register(string username, string password, string email, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email))
            {
                message = "Vui lòng nhập đầy đủ thông tin";
                return false;
            }

            try
            {
                using (SqlConnection conn = DBConnect.GetConnection())
                {
                    conn.Open();

                    string checkUser = "SELECT COUNT(*) FROM dbo.NguoiChoi WHERE TenDangNhap = @u";
                    using (SqlCommand cmd = new SqlCommand(checkUser, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            message = "Tài khoản đã tồn tại";
                            return false;
                        }
                    }

                    string checkEmail = "SELECT COUNT(*) FROM dbo.NguoiChoi WHERE Email = @e";
                    using (SqlCommand cmd = new SqlCommand(checkEmail, conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            message = "Email đã tồn tại";
                            return false;
                        }
                    }

                    string insertSql = @"INSERT INTO dbo.NguoiChoi
                        (
                            TenDangNhap, MatKhau, Diem, SoTranDaChoi, NgayTao, Email
                        )
                        VALUES
                        (
                            @u, @p, 0, 0, GETDATE(), @e
                        )";

                    using (SqlCommand cmd = new SqlCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);
                        cmd.Parameters.AddWithValue("@e", email);

                        cmd.ExecuteNonQuery();
                    }

                    message = "Đăng ký thành công";
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOI REGISTER: " + ex.Message);
                message = "Lỗi kết nối cơ sở dữ liệu";
                return false;
            }
        }

        public bool ForgotPassword(string email, out string password)
        {
            password = "";

            try
            {
                using (SqlConnection conn = DBConnect.GetConnection())
                {
                    conn.Open();

                    string sql = "SELECT MatKhau FROM dbo.NguoiChoi WHERE Email = @e";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            password = result.ToString();
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOI FORGOT PASSWORD: " + ex.Message);
            }

            return false;
        }

        public void SaveFinalResult(string username, int score)
        {
            try
            {
                using (SqlConnection conn = DBConnect.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        UPDATE dbo.NguoiChoi
                        SET 
                            Diem = CASE WHEN @score > ISNULL(Diem, 0) THEN @score ELSE Diem END,
                            SoTranDaChoi = ISNULL(SoTranDaChoi, 0) + 1
                        WHERE TenDangNhap = @u";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@score", score);
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("LOI SAVE FINAL RESULT: " + ex.Message);
            }
        }
    }
}