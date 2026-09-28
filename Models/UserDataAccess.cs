using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace INFAppDatabaseDotNet.Models
{
    public class UserDataAccess : DataAccess
    {
        public UserDataAccess(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Authentifiziert einen Benutzer
        /// </summary>
        public User AuthenticateUser(string username, string password)
        {
            try
            {
                string passwordHash = HashPassword(password);
                string query = @"
                    SELECT UserId, Username, Role, FirstName, LastName, CreatedAt
                    FROM Users
                    WHERE Username = @Username AND PasswordHash = @PasswordHash";

                DataTable dt = ExecuteQuery(query,
                    new SqlParameter("@Username", username),
                    new SqlParameter("@PasswordHash", passwordHash));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new User
                    {
                        UserId = (int)row["UserId"],
                        Username = row["Username"].ToString(),
                        Role = row["Role"].ToString(),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        CreatedAt = (DateTime)row["CreatedAt"]
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler bei der Benutzerauthentifizierung: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft einen Benutzer nach ID ab
        /// </summary>
        public User GetUserById(int userId)
        {
            try
            {
                string query = @"
                    SELECT UserId, Username, Role, FirstName, LastName, CreatedAt
                    FROM Users
                    WHERE UserId = @UserId";

                DataTable dt = ExecuteQuery(query, new SqlParameter("@UserId", userId));

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new User
                    {
                        UserId = (int)row["UserId"],
                        Username = row["Username"].ToString(),
                        Role = row["Role"].ToString(),
                        FirstName = row["FirstName"].ToString(),
                        LastName = row["LastName"].ToString(),
                        CreatedAt = (DateTime)row["CreatedAt"]
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen des Benutzers: " + ex.Message);
            }
        }

        /// <summary>
        /// Erstellt einen neuen Benutzer
        /// </summary>
        public int CreateUser(User user)
        {
            try
            {
                string passwordHash = HashPassword(user.Password);
                string query = @"
                    INSERT INTO Users (Username, PasswordHash, Role, FirstName, LastName)
                    VALUES (@Username, @PasswordHash, @Role, @FirstName, @LastName);
                    SELECT SCOPE_IDENTITY();";

                object result = ExecuteScalar(query,
                    new SqlParameter("@Username", user.Username),
                    new SqlParameter("@PasswordHash", passwordHash),
                    new SqlParameter("@Role", user.Role),
                    new SqlParameter("@FirstName", user.FirstName),
                    new SqlParameter("@LastName", user.LastName));

                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Erstellen des Benutzers: " + ex.Message);
            }
        }

        /// <summary>
        /// Ã„ndert das Passwort eines Benutzers
        /// </summary>
        public bool ChangePassword(int userId, string newPassword)
        {
            try
            {
                string passwordHash = HashPassword(newPassword);
                string query = @"
                    UPDATE Users
                    SET PasswordHash = @PasswordHash
                    WHERE UserId = @UserId";

                ExecuteNonQuery(query,
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@PasswordHash", passwordHash));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Ã„ndern des Passworts: " + ex.Message);
            }
        }

        /// <summary>
        /// Hasht ein Passwort mit SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}

