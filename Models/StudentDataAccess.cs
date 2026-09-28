using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace INFAppDatabaseDotNet.Models
{
    public class StudentDataAccess : DataAccess
    {
        public StudentDataAccess(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Ruft einen SchÃ¼ler nach Benutzer-ID ab
        /// </summary>
        public Student GetStudentByUserId(int userId)
        {
            try
            {
                string query = @"
                    SELECT StudentId, UserId, LastName, FirstName, Gender, DateOfBirth,
                           Street, PostalCode, City, Canton, PhoneMobile, PhoneHome, TeacherName, CreatedAt
                    FROM Students
                    WHERE UserId = @UserId";

                DataTable dt = ExecuteQuery(query, new SqlParameter("@UserId", userId));

                if (dt.Rows.Count > 0)
                {
                    return MapToStudent(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen des SchÃ¼lers: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft einen SchÃ¼ler nach Student-ID ab
        /// </summary>
        public Student GetStudentById(int studentId)
        {
            try
            {
                string query = @"
                    SELECT StudentId, UserId, LastName, FirstName, Gender, DateOfBirth,
                           Street, PostalCode, City, Canton, PhoneMobile, PhoneHome, TeacherName, CreatedAt
                    FROM Students
                    WHERE StudentId = @StudentId";

                DataTable dt = ExecuteQuery(query, new SqlParameter("@StudentId", studentId));

                if (dt.Rows.Count > 0)
                {
                    return MapToStudent(dt.Rows[0]);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen des SchÃ¼lers: " + ex.Message);
            }
        }

        /// <summary>
        /// Ruft alle SchÃ¼ler ab
        /// </summary>
        public List<Student> GetAllStudents()
        {
            try
            {
                string query = @"
                    SELECT StudentId, UserId, LastName, FirstName, Gender, DateOfBirth,
                           Street, PostalCode, City, Canton, PhoneMobile, PhoneHome, TeacherName, CreatedAt
                    FROM Students
                    ORDER BY LastName, FirstName";

                DataTable dt = ExecuteQuery(query);
                List<Student> students = new List<Student>();

                foreach (DataRow row in dt.Rows)
                {
                    students.Add(MapToStudent(row));
                }
                return students;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Abrufen der SchÃ¼ler: " + ex.Message);
            }
        }

        /// <summary>
        /// Erstellt einen neuen SchÃ¼ler
        /// </summary>
        public int CreateStudent(Student student)
        {
            try
            {
                string query = @"
                    INSERT INTO Students (UserId, LastName, FirstName, Gender, DateOfBirth,
                                         Street, PostalCode, City, Canton, PhoneMobile, PhoneHome, TeacherName)
                    VALUES (@UserId, @LastName, @FirstName, @Gender, @DateOfBirth,
                            @Street, @PostalCode, @City, @Canton, @PhoneMobile, @PhoneHome, @TeacherName);
                    SELECT SCOPE_IDENTITY();";

                object result = ExecuteScalar(query,
                    new SqlParameter("@UserId", student.UserId),
                    new SqlParameter("@LastName", student.LastName),
                    new SqlParameter("@FirstName", student.FirstName),
                    new SqlParameter("@Gender", student.Gender),
                    new SqlParameter("@DateOfBirth", student.DateOfBirth),
                    new SqlParameter("@Street", student.Street),
                    new SqlParameter("@PostalCode", student.PostalCode),
                    new SqlParameter("@City", student.City),
                    new SqlParameter("@Canton", student.Canton),
                    new SqlParameter("@PhoneMobile", student.PhoneMobile ?? (object)DBNull.Value),
                    new SqlParameter("@PhoneHome", student.PhoneHome ?? (object)DBNull.Value),
                    new SqlParameter("@TeacherName", student.TeacherName));

                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Erstellen des SchÃ¼lers: " + ex.Message);
            }
        }

        /// <summary>
        /// Aktualisiert einen SchÃ¼ler
        /// </summary>
        public bool UpdateStudent(Student student)
        {
            try
            {
                string query = @"
                    UPDATE Students
                    SET LastName = @LastName, FirstName = @FirstName, Gender = @Gender,
                        DateOfBirth = @DateOfBirth, Street = @Street, PostalCode = @PostalCode,
                        City = @City, Canton = @Canton, PhoneMobile = @PhoneMobile,
                        PhoneHome = @PhoneHome, TeacherName = @TeacherName
                    WHERE StudentId = @StudentId";

                ExecuteNonQuery(query,
                    new SqlParameter("@StudentId", student.StudentId),
                    new SqlParameter("@LastName", student.LastName),
                    new SqlParameter("@FirstName", student.FirstName),
                    new SqlParameter("@Gender", student.Gender),
                    new SqlParameter("@DateOfBirth", student.DateOfBirth),
                    new SqlParameter("@Street", student.Street),
                    new SqlParameter("@PostalCode", student.PostalCode),
                    new SqlParameter("@City", student.City),
                    new SqlParameter("@Canton", student.Canton),
                    new SqlParameter("@PhoneMobile", student.PhoneMobile ?? (object)DBNull.Value),
                    new SqlParameter("@PhoneHome", student.PhoneHome ?? (object)DBNull.Value),
                    new SqlParameter("@TeacherName", student.TeacherName));

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Aktualisieren des SchÃ¼lers: " + ex.Message);
            }
        }

        private Student MapToStudent(DataRow row)
        {
            return new Student
            {
                StudentId = (int)row["StudentId"],
                UserId = (int)row["UserId"],
                LastName = row["LastName"].ToString(),
                FirstName = row["FirstName"].ToString(),
                Gender = char.Parse(row["Gender"].ToString()),
                DateOfBirth = (DateTime)row["DateOfBirth"],
                Street = row["Street"].ToString(),
                PostalCode = row["PostalCode"].ToString(),
                City = row["City"].ToString(),
                Canton = row["Canton"].ToString(),
                PhoneMobile = row["PhoneMobile"] != DBNull.Value ? row["PhoneMobile"].ToString() : null,
                PhoneHome = row["PhoneHome"] != DBNull.Value ? row["PhoneHome"].ToString() : null,
                TeacherName = row["TeacherName"].ToString(),
                CreatedAt = (DateTime)row["CreatedAt"]
            };
        }
    }
}

