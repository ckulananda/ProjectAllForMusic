using Microsoft.Data.SqlClient;
using ProjectAllForMusic.Model;
using System;
using System.Collections.Generic;
using Azure;
using System.Data;
using System.Diagnostics.Metrics;

namespace ProjectAllForMusic.Model
{
    public class Dal
    {
        // Add User
        public Response AddUser(User user, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Users (Username, PasswordHash, Email, Role, ProfilePicture) " +
                    "VALUES (@username, @passwordHash, @email, @role, @profilePicture)",
                    connection);

                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                cmd.Parameters.AddWithValue("@email", user.Email);
                cmd.Parameters.AddWithValue("@role", user.Role);
                cmd.Parameters.AddWithValue("@profilePicture", (object)user.ProfilePicture ?? DBNull.Value);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "User added successfully." : "Failed to add user.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to add user: {ex.Message}";
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return response;
        }

        // User Login
        public Response UserLogin(UserLogin login, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                connection.Open();
                using SqlCommand cmd = new SqlCommand("SELECT UserID, Username, Email, Role, PasswordHash, ProfilePicture FROM Users WHERE Email = @Email", connection);
                cmd.Parameters.AddWithValue("@Email", login.Email);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string storedPasswordHash = reader["PasswordHash"].ToString();
                    if (BCrypt.Net.BCrypt.Verify(login.Password, storedPasswordHash))
                    {
                        response.StatusCode = 200;
                        response.StatusMessage = "Login successful.";
                        response.Data = new
                        {
                            UserID = reader["UserID"],
                            Username = reader["Username"],
                            Email = reader["Email"],
                            Role = reader["Role"],
                            ProfilePicture = reader["ProfilePicture"].ToString()
                        };
                    }
                    else
                    {
                        response.StatusCode = 401;
                        response.StatusMessage = "Invalid credentials.";
                    }
                }
                else
                {
                    response.StatusCode = 401;
                    response.StatusMessage = "Invalid credentials.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Get Users
        public Response GetUsers(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand("SELECT UserID, Username, Email, Role, ProfilePicture FROM Users", connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                List<User> users = new List<User>();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserID = (int)reader["UserID"],
                        Username = reader["Username"].ToString(),
                        Email = reader["Email"].ToString(),
                        Role = reader["Role"].ToString(),
                        ProfilePicture = reader["ProfilePicture"].ToString()
                    });
                }

                response.StatusCode = 200;
                response.Data = users;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching users: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Remove User by ID
        public Response RemoveUserById(int userId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE UserID = @UserID", connection);
                cmd.Parameters.AddWithValue("@UserID", userId);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "User removed successfully." : "Failed to remove user.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to remove user: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Update User by ID
        public Response UpdateUser(User updatedUser, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand("UPDATE Users SET Username = @username, PasswordHash = @passwordHash, Role = @role, ProfilePicture = @profilePicture WHERE UserID = @UserID", connection);
                cmd.Parameters.AddWithValue("@username", updatedUser.Username);
                cmd.Parameters.AddWithValue("@passwordHash", updatedUser.PasswordHash);
                cmd.Parameters.AddWithValue("@role", updatedUser.Role);
                cmd.Parameters.AddWithValue("@profilePicture", (object)updatedUser.ProfilePicture ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserID", updatedUser.UserID);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "User updated successfully." : "Failed to update user.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to update user: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        public List<User> SearchUserByName(string username, SqlConnection connection)
        {
            var users = new List<User>();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT UserID, Username, Email, ProfilePicture FROM Users WHERE Username LIKE @Username";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Username", "%" + username + "%");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                ProfilePicture = reader.IsDBNull(reader.GetOrdinal("ProfilePicture")) ? null : reader.GetString(reader.GetOrdinal("ProfilePicture"))
                            };

                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return users;
        }

        // Get User by ID
        public User GetUserById(int userId, SqlConnection connection)
        {
            User user = null;

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT UserID, Username, Email, Role, ProfilePicture FROM Users WHERE UserID = @UserID";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Role = reader.GetString(reader.GetOrdinal("Role")),
                                ProfilePicture = reader.IsDBNull(reader.GetOrdinal("ProfilePicture")) ? null : reader.GetString(reader.GetOrdinal("ProfilePicture"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return user;
        }

        // Get Users by Role
        public List<User> GetUsersByRole(string role, SqlConnection connection)
        {
            var users = new List<User>();

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "SELECT UserID, Username, Email, ProfilePicture FROM Users WHERE Role = @Role";

                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Role", role);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                ProfilePicture = reader.IsDBNull(reader.GetOrdinal("ProfilePicture")) ? null : reader.GetString(reader.GetOrdinal("ProfilePicture"))
                            };

                            users.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return users;
        }

        //=======================================================================================================================================

        // Add Learning Package
        public Response AddLearningPackage(LearningPackage package, SqlConnection connection)
        {
            Response response = new Response();

            try
            {
                using (connection) // Ensure connection is disposed properly
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using SqlCommand cmd = new SqlCommand(
                        "INSERT INTO LearningPackages (LearningPackageName, InstructorID, InstructorName, LearningMaterials, Videos, Description) " +
                        "VALUES (@name, @instructorID, @instructorName, @materials, @videos, @description)", connection);

                    cmd.Parameters.AddWithValue("@name", package.LearningPackageName);
                    cmd.Parameters.AddWithValue("@instructorID", package.InstructorID);
                    cmd.Parameters.AddWithValue("@instructorName", package.InstructorName);
                    cmd.Parameters.AddWithValue("@materials", (object)package.LearningMaterials ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@videos", (object)package.Videos ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@description", (object)package.Description ?? DBNull.Value);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    response.StatusCode = rowsAffected > 0 ? 200 : 400;
                    response.StatusMessage = rowsAffected > 0 ? "Learning package added successfully." : "Failed to add package.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error adding package: {ex.Message}";
            }

            return response;
        }

        // Get All Learning Packages
        public Response GetLearningPackages(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand("SELECT * FROM LearningPackages", connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                List<LearningPackage> packages = new List<LearningPackage>();
                while (reader.Read())
                {
                    packages.Add(new LearningPackage
                    {
                        PackageID = (int)reader["PackageID"],
                        LearningPackageName = reader["LearningPackageName"].ToString(),
                        InstructorID = (int)reader["InstructorID"],
                        InstructorName = reader["InstructorName"].ToString(),
                        LearningMaterials = reader["LearningMaterials"].ToString(),
                        Videos = reader["Videos"].ToString(),
                        Description = reader["Description"].ToString(),
                        CreatedDate = (DateTime)reader["CreatedDate"]
                    });
                }

                response.StatusCode = 200;
                response.Data = packages;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching learning packages: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Get Learning Package by ID
        public Response GetLearningPackageById(int packageId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand("SELECT * FROM LearningPackages WHERE PackageID = @packageID", connection);
                cmd.Parameters.AddWithValue("@packageID", packageId);

                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    LearningPackage package = new LearningPackage
                    {
                        PackageID = (int)reader["PackageID"],
                        LearningPackageName = reader["LearningPackageName"].ToString(),
                        InstructorID = (int)reader["InstructorID"],
                        InstructorName = reader["InstructorName"].ToString(),
                        LearningMaterials = reader["LearningMaterials"].ToString(),
                        Videos = reader["Videos"].ToString(),
                        Description = reader["Description"].ToString(),
                        CreatedDate = (DateTime)reader["CreatedDate"]
                    };

                    response.StatusCode = 200;
                    response.Data = package;
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Learning package not found.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching package: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Update Learning Package
        public Response UpdateLearningPackage(LearningPackage package, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand(
                    "UPDATE LearningPackages SET LearningPackageName = @name, InstructorID = @instructorID, " +
                    "InstructorName = @instructorName, LearningMaterials = @materials, Videos = @videos, Description = @description " +
                    "WHERE PackageID = @packageID", connection);

                cmd.Parameters.AddWithValue("@name", package.LearningPackageName);
                cmd.Parameters.AddWithValue("@instructorID", package.InstructorID);
                cmd.Parameters.AddWithValue("@instructorName", package.InstructorName);
                cmd.Parameters.AddWithValue("@materials", (object)package.LearningMaterials ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@videos", (object)package.Videos ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@description", (object)package.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@packageID", package.PackageID);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Learning package updated successfully." : "Failed to update package.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to update package: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Remove Learning Package by ID
        public Response RemoveLearningPackageById(int packageId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                using SqlCommand cmd = new SqlCommand("DELETE FROM LearningPackages WHERE PackageID = @packageID", connection);
                cmd.Parameters.AddWithValue("@packageID", packageId);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Learning package removed successfully." : "Failed to remove package.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to remove package: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Search Learning Packages by Name
        public Response SearchLearningPackageByName(string name, SqlConnection connection)
        {
            List<LearningPackage> packages = new List<LearningPackage>();
            Response response = new Response();  // Response object to hold status and data

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT * FROM LearningPackages WHERE LearningPackageName LIKE @name";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", "%" + name + "%");

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    packages.Add(new LearningPackage
                    {
                        PackageID = (int)reader["PackageID"],
                        LearningPackageName = reader["LearningPackageName"].ToString(),
                        InstructorID = (int)reader["InstructorID"],
                        InstructorName = reader["InstructorName"].ToString(),
                        LearningMaterials = reader["LearningMaterials"].ToString(),
                        Videos = reader["Videos"].ToString(),
                        Description = reader["Description"].ToString(),
                        CreatedDate = (DateTime)reader["CreatedDate"]
                    });
                }

                if (packages.Count == 0)
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "No learning packages found";
                }
                else
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Success";
                    response.Data = packages;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }

        // Get Learning Packages by InstructorID
        public Response GetLearningPackagesByInstructorID(int instructorID, SqlConnection connection)
        {
            Response response = new Response();
            List<LearningPackage> packages = new List<LearningPackage>();

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT * FROM LearningPackages WHERE InstructorID = @instructorID";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@instructorID", instructorID);

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    packages.Add(new LearningPackage
                    {
                        PackageID = (int)reader["PackageID"],
                        LearningPackageName = reader["LearningPackageName"].ToString(),
                        InstructorID = (int)reader["InstructorID"],
                        InstructorName = reader["InstructorName"].ToString(),
                        LearningMaterials = reader["LearningMaterials"].ToString(),
                        Videos = reader["Videos"].ToString(),
                        Description = reader["Description"].ToString(),
                        CreatedDate = (DateTime)reader["CreatedDate"]
                    });
                }

                if (packages.Count == 0)
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "No learning packages found for this instructor.";
                }
                else
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Success";
                    response.Data = packages;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            return response;
        }


        //=======================================================================================================================================
        // Add a new instrument
        public Response AddInstrument(Instruments instrument, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query = "INSERT INTO Instruments (InstrumentName, Description, Condition, Price, SellerID, InstrumentPicture) " +
                               "VALUES (@InstrumentName, @Description, @Condition, @Price, @SellerID, @InstrumentPicture)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@InstrumentName", instrument.InstrumentName);
                cmd.Parameters.AddWithValue("@Description", instrument.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Condition", instrument.Condition ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", instrument.Price);
                cmd.Parameters.AddWithValue("@SellerID", instrument.SellerID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InstrumentPicture", instrument.InstrumentPicture ?? (object)DBNull.Value);

                int rowsAffected = cmd.ExecuteNonQuery();
                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Instrument added successfully." : "Failed to add instrument.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error adding instrument: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Get all instruments
        public Response GetInstruments(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT InstrumentID, InstrumentName, Description, Condition, Price, SellerID, InstrumentPicture, DateAdded FROM Instruments";

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<Instruments> instruments = new List<Instruments>();

                while (reader.Read())
                {
                    instruments.Add(new Instruments
                    {
                        InstrumentID = (int)reader["InstrumentID"],
                        InstrumentName = reader["InstrumentName"].ToString(),
                        Description = reader["Description"].ToString(),
                        Condition = reader["Condition"].ToString(),
                        Price = (decimal)reader["Price"],
                        SellerID = reader.IsDBNull(reader.GetOrdinal("SellerID")) ? (int?)null : (int)reader["SellerID"],
                        InstrumentPicture = reader.IsDBNull(reader.GetOrdinal("InstrumentPicture")) ? null : reader["InstrumentPicture"].ToString(),
                        DateAdded = (DateTime)reader["DateAdded"]
                    });
                }

                response.StatusCode = instruments.Count > 0 ? 200 : 404;
                response.StatusMessage = instruments.Count > 0 ? "Instruments found." : "No instruments found.";
                response.Data = instruments;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching instruments: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Update an instrument's details
        public Response UpdateInstrument(Instruments updatedInstrument, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "UPDATE Instruments SET InstrumentName = @InstrumentName, Description = @Description, " +
                               "Condition = @Condition, Price = @Price, SellerID = @SellerID, InstrumentPicture = @InstrumentPicture WHERE InstrumentID = @InstrumentID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@InstrumentName", updatedInstrument.InstrumentName);
                cmd.Parameters.AddWithValue("@Description", updatedInstrument.Description ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Condition", updatedInstrument.Condition ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", updatedInstrument.Price);
                cmd.Parameters.AddWithValue("@SellerID", updatedInstrument.SellerID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InstrumentPicture", updatedInstrument.InstrumentPicture ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@InstrumentID", updatedInstrument.InstrumentID);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Instrument updated successfully." : "Failed to update instrument.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Failed to update instrument: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Remove an instrument by ID
        public Response RemoveInstrumentById(int id, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "DELETE FROM Instruments WHERE InstrumentID = @InstrumentID";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@InstrumentID", id);
                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    connection.Close();

                    response.StatusCode = rowsAffected > 0 ? 200 : 404;
                    response.StatusMessage = rowsAffected > 0 ? "Instrument removed successfully" : "Instrument not found";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return response;
        }

        // Search for instruments by name
        public Response SearchInstrumentsByName(string instrumentName, SqlConnection connection)
        {
            Response response = new Response();
            List<Instruments> instruments = new List<Instruments>();

            try
            {
                string query = "SELECT * FROM Instruments WHERE InstrumentName LIKE @InstrumentName";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@InstrumentName", "%" + instrumentName + "%");

                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Instruments instrument = new Instruments
                            {
                                InstrumentID = Convert.ToInt32(reader["InstrumentID"]),
                                InstrumentName = reader["InstrumentName"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"])
                            };
                            instruments.Add(instrument);
                        }
                    }
                    connection.Close();
                }

                response.StatusCode = instruments.Count > 0 ? 200 : 404;
                response.StatusMessage = instruments.Count > 0 ? "Instruments found" : "No instruments found";
                response.Data = instruments;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return response;
        }


        // Get instruments by SellerID
        public Response GetInstrumentsBySellerID(int sellerID, SqlConnection connection)
        {
            Response response = new Response();
            List<Instruments> instruments = new List<Instruments>();

            try
            {
                string query = "SELECT * FROM Instruments WHERE SellerID = @SellerID";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SellerID", sellerID);

                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Instruments instrument = new Instruments
                            {
                                InstrumentID = Convert.ToInt32(reader["InstrumentID"]),
                                InstrumentName = reader["InstrumentName"].ToString(),
                                Description = reader["Description"].ToString(),
                                Condition = reader["Condition"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                SellerID = reader.IsDBNull(reader.GetOrdinal("SellerID")) ? (int?)null : Convert.ToInt32(reader["SellerID"]),
                                InstrumentPicture = reader.IsDBNull(reader.GetOrdinal("InstrumentPicture")) ? null : reader["InstrumentPicture"].ToString(),
                                DateAdded = Convert.ToDateTime(reader["DateAdded"])
                            };
                            instruments.Add(instrument);
                        }
                    }
                    connection.Close();
                }

                response.StatusCode = instruments.Count > 0 ? 200 : 404;
                response.StatusMessage = instruments.Count > 0 ? "Instruments found" : "No instruments found for this seller";
                response.Data = instruments;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return response;
        }

        //========================================================================================================================================

        public Response AddMusicLyrics(MusicLyrics musicLyrics, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "INSERT INTO MusicLyrics (Title, Content, AuthorID, Price, FilePath) " +
                               "VALUES (@Title, @Content, @AuthorID, @Price, @FilePath)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Title", musicLyrics.Title);
                cmd.Parameters.AddWithValue("@Content", musicLyrics.Content);
                cmd.Parameters.AddWithValue("@AuthorID", musicLyrics.AuthorID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", musicLyrics.Price ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FilePath", musicLyrics.FilePath ?? (object)DBNull.Value);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Music lyric added successfully." : "Failed to add music lyric.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error adding music lyric: {ex.Message}";
            }
            return response;
        }

        public Response GetMusicLyrics(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT LyricID, Title, Content, AuthorID, Price, FilePath, DateAdded FROM MusicLyrics";

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<MusicLyrics> lyrics = new List<MusicLyrics>();

                while (reader.Read())
                {
                    lyrics.Add(new MusicLyrics
                    {
                        LyricID = (int)reader["LyricID"],
                        Title = reader["Title"].ToString(),
                        Content = reader["Content"].ToString(),
                        AuthorID = reader.IsDBNull(reader.GetOrdinal("AuthorID")) ? (int?)null : (int)reader["AuthorID"],
                        Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : (decimal)reader["Price"],
                        FilePath = reader.IsDBNull(reader.GetOrdinal("FilePath")) ? null : reader["FilePath"].ToString(),
                        DateAdded = (DateTime)reader["DateAdded"]
                    });
                }

                response.StatusCode = lyrics.Count > 0 ? 200 : 404;
                response.StatusMessage = lyrics.Count > 0 ? "Music lyrics found." : "No music lyrics found.";
                response.Data = lyrics;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching music lyrics: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        public Response UpdateMusicLyrics(MusicLyrics updatedMusicLyrics, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "UPDATE MusicLyrics SET Title = @Title, Content = @Content, AuthorID = @AuthorID, " +
                               "Price = @Price, FilePath = @FilePath WHERE LyricID = @LyricID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Title", updatedMusicLyrics.Title);
                cmd.Parameters.AddWithValue("@Content", updatedMusicLyrics.Content);
                cmd.Parameters.AddWithValue("@AuthorID", updatedMusicLyrics.AuthorID ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Price", updatedMusicLyrics.Price ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FilePath", updatedMusicLyrics.FilePath ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@LyricID", updatedMusicLyrics.LyricID);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Music lyric updated successfully." : "Failed to update music lyric.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error updating music lyric: {ex.Message}";
            }
            return response;
        }

        public Response RemoveMusicLyricsById(int id, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "DELETE FROM MusicLyrics WHERE LyricID = @LyricID";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@LyricID", id);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 404;
                response.StatusMessage = rowsAffected > 0 ? "Music lyric removed successfully." : "Music lyric not found.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            return response;
        }

        public Response SearchMusicLyricsByTitle(string title, SqlConnection connection)
        {
            Response response = new Response();
            List<MusicLyrics> lyrics = new List<MusicLyrics>();

            try
            {
                string query = "SELECT * FROM MusicLyrics WHERE Title LIKE @Title";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Title", "%" + title + "%");

                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    MusicLyrics lyric = new MusicLyrics
                    {
                        LyricID = Convert.ToInt32(reader["LyricID"]),
                        Title = reader["Title"].ToString(),
                        Content = reader["Content"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"])
                    };
                    lyrics.Add(lyric);
                }
                connection.Close();

                response.StatusCode = lyrics.Count > 0 ? 200 : 404;
                response.StatusMessage = lyrics.Count > 0 ? "Music lyrics found." : "No music lyrics found.";
                response.Data = lyrics;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            return response;
        }

        public Response GetMusicLyricsByAuthorId(int authorId, SqlConnection connection)
        {
            Response response = new Response();
            List<MusicLyrics> lyrics = new List<MusicLyrics>();

            try
            {
                string query = "SELECT LyricID, Title, Content, AuthorID, Price, FilePath, DateAdded FROM MusicLyrics WHERE AuthorID = @AuthorID";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AuthorID", authorId);

                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MusicLyrics lyric = new MusicLyrics
                    {
                        LyricID = Convert.ToInt32(reader["LyricID"]),
                        Title = reader["Title"].ToString(),
                        Content = reader["Content"].ToString(),
                        AuthorID = reader.IsDBNull(reader.GetOrdinal("AuthorID")) ? (int?)null : Convert.ToInt32(reader["AuthorID"]),
                        Price = reader.IsDBNull(reader.GetOrdinal("Price")) ? (decimal?)null : Convert.ToDecimal(reader["Price"]),
                        FilePath = reader.IsDBNull(reader.GetOrdinal("FilePath")) ? null : reader["FilePath"].ToString(),
                        DateAdded = Convert.ToDateTime(reader["DateAdded"])
                    };
                    lyrics.Add(lyric);
                }
                connection.Close();

                response.StatusCode = lyrics.Count > 0 ? 200 : 404;
                response.StatusMessage = lyrics.Count > 0 ? "Music lyrics found." : "No music lyrics found for the given Author ID.";
                response.Data = lyrics;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching music lyrics: {ex.Message}";
            }
            return response;
        }

        //========================================================================================================================================

        // Add a new request
        public Response AddRequest(Request request, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "INSERT INTO Requests (RequestType, RequesterID, RequestedEntityID, Status, DateRequested, RequestBody) " +
                               "VALUES (@RequestType, @RequesterID, @RequestedEntityID, @Status, @DateRequested, @RequestBody)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RequestType", request.RequestType);
                cmd.Parameters.AddWithValue("@RequesterID", request.RequesterID);
                cmd.Parameters.AddWithValue("@RequestedEntityID", request.RequestedEntityID);
                cmd.Parameters.AddWithValue("@Status", request.Status);
                cmd.Parameters.AddWithValue("@DateRequested", request.DateRequested);
                cmd.Parameters.AddWithValue("@RequestBody", request.RequestBody);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Request added successfully." : "Failed to add request.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error adding request: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Get all requests
        public Response GetRequests(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT RequestID, RequestType, RequesterID, RequestedEntityID, Status, DateRequested, RequestBody FROM Requests";

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<Request> requests = new List<Request>();

                while (reader.Read())
                {
                    requests.Add(new Request
                    {
                        RequestID = reader.GetInt32(0),
                        RequestType = reader.GetString(1),
                        RequesterID = reader.GetInt32(2),
                        RequestedEntityID = reader.GetInt32(3),
                        Status = reader.GetString(4),
                        DateRequested = reader.GetDateTime(5),
                        RequestBody = reader.GetString(6)
                    });
                }

                response.StatusCode = requests.Count > 0 ? 200 : 404;
                response.StatusMessage = requests.Count > 0 ? "Requests found." : "No requests found.";
                response.Data = requests;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching requests: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Update request status
        public Response UpdateRequestStatus(int requestId, string status, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "UPDATE Requests SET Status = @Status WHERE RequestID = @RequestID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@RequestID", requestId);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                response.StatusCode = rowsAffected > 0 ? 200 : 404;
                response.StatusMessage = rowsAffected > 0 ? "Request status updated successfully." : "Request not found.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error updating request status: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Remove request by ID
        public Response RemoveRequestById(int requestId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "DELETE FROM Requests WHERE RequestID = @RequestID";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RequestID", requestId);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                response.StatusCode = rowsAffected > 0 ? 200 : 404;
                response.StatusMessage = rowsAffected > 0 ? "Request removed successfully." : "Request not found.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Get requests by RequesterID
        public Response GetRequestsByRequesterId(int requesterId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT RequestID, RequestType, RequesterID, RequestedEntityID, Status, DateRequested, RequestBody " +
                               "FROM Requests WHERE RequesterID = @RequesterID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RequesterID", requesterId);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<Request> requests = new List<Request>();

                while (reader.Read())
                {
                    requests.Add(new Request
                    {
                        RequestID = reader.GetInt32(0),
                        RequestType = reader.GetString(1),
                        RequesterID = reader.GetInt32(2),
                        RequestedEntityID = reader.GetInt32(3),
                        Status = reader.GetString(4),
                        DateRequested = reader.GetDateTime(5),
                        RequestBody = reader.GetString(6)
                    });
                }

                response.StatusCode = requests.Count > 0 ? 200 : 404;
                response.StatusMessage = requests.Count > 0 ? "Requests found." : "No requests found.";
                response.Data = requests;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching requests: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Get requests by RequestedEntityID
        public Response GetRequestsByRequestedEntityId(int requestedEntityId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT RequestID, RequestType, RequesterID, RequestedEntityID, Status, DateRequested, RequestBody " +
                               "FROM Requests WHERE RequestedEntityID = @RequestedEntityID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RequestedEntityID", requestedEntityId);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<Request> requests = new List<Request>();

                while (reader.Read())
                {
                    requests.Add(new Request
                    {
                        RequestID = reader.GetInt32(0),
                        RequestType = reader.GetString(1),
                        RequesterID = reader.GetInt32(2),
                        RequestedEntityID = reader.GetInt32(3),
                        Status = reader.GetString(4),
                        DateRequested = reader.GetDateTime(5),
                        RequestBody = reader.GetString(6)
                    });
                }

                response.StatusCode = requests.Count > 0 ? 200 : 404;
                response.StatusMessage = requests.Count > 0 ? "Requests found." : "No requests found.";
                response.Data = requests;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching requests: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }


        //========================================================================================================================================
        // Add a new transaction
        public Response AddTransaction(Transaction transaction, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "INSERT INTO Transactions (BuyerID, ItemType, ItemID, Amount, PaymentMethod) " +
                               "VALUES (@BuyerID, @ItemType, @ItemID, @Amount, @PaymentMethod)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BuyerID", transaction.BuyerID);
                cmd.Parameters.AddWithValue("@ItemType", transaction.ItemType);
                cmd.Parameters.AddWithValue("@ItemID", transaction.ItemID);
                cmd.Parameters.AddWithValue("@Amount", transaction.Amount);
                cmd.Parameters.AddWithValue("@PaymentMethod", transaction.PaymentMethod);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Transaction added successfully." : "Failed to add transaction.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error adding transaction: {ex.Message}";
            }
            return response;
        }

        // Get all transactions
        public Response GetTransactions(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT TransactionID, BuyerID, ItemType, ItemID, Amount, PaymentMethod, DatePurchased FROM Transactions";

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<Transaction> transactions = new List<Transaction>();

                while (reader.Read())
                {
                    transactions.Add(new Transaction
                    {
                        TransactionID = (int)reader["TransactionID"],
                        BuyerID = (int)reader["BuyerID"],
                        ItemType = reader["ItemType"].ToString(),
                        ItemID = (int)reader["ItemID"],
                        Amount = (decimal)reader["Amount"],
                        PaymentMethod = reader["PaymentMethod"].ToString(),
                        DatePurchased = (DateTime)reader["DatePurchased"]
                    });
                }

                response.StatusCode = transactions.Count > 0 ? 200 : 404;
                response.StatusMessage = transactions.Count > 0 ? "Transactions found." : "No transactions found.";
                response.Data = transactions;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching transactions: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Get transactions by BuyerID
        public Response GetTransactionsByBuyerId(int buyerId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT TransactionID, BuyerID, ItemType, ItemID, Amount, PaymentMethod, DatePurchased " +
                               "FROM Transactions WHERE BuyerID = @BuyerID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BuyerID", buyerId);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<Transaction> transactions = new List<Transaction>();

                while (reader.Read())
                {
                    transactions.Add(new Transaction
                    {
                        TransactionID = (int)reader["TransactionID"],
                        BuyerID = (int)reader["BuyerID"],
                        ItemType = reader["ItemType"].ToString(),
                        ItemID = (int)reader["ItemID"],
                        Amount = (decimal)reader["Amount"],
                        PaymentMethod = reader["PaymentMethod"].ToString(),
                        DatePurchased = (DateTime)reader["DatePurchased"]
                    });
                }

                response.StatusCode = transactions.Count > 0 ? 200 : 404;
                response.StatusMessage = transactions.Count > 0 ? "Transactions found." : "No transactions found.";
                response.Data = transactions;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching transactions: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Remove transaction by ID
        public Response RemoveTransactionById(int transactionId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "DELETE FROM Transactions WHERE TransactionID = @TransactionID";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@TransactionID", transactionId);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 404;
                response.StatusMessage = rowsAffected > 0 ? "Transaction removed successfully." : "Transaction not found.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            return response;
        }
        //========================================================================================================================================

        // Add a new payment method
        public Response AddPaymentMethod(PaymentMethod paymentMethod, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "INSERT INTO PaymentMethods (MethodName, Details) " +
                               "VALUES (@MethodName, @Details)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MethodName", paymentMethod.MethodName);
                cmd.Parameters.AddWithValue("@Details", paymentMethod.Details);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 400;
                response.StatusMessage = rowsAffected > 0 ? "Payment method added successfully." : "Failed to add payment method.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error adding payment method: {ex.Message}";
            }
            return response;
        }

        // Get all payment methods
        public Response GetPaymentMethods(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT PaymentMethodID, MethodName, Details FROM PaymentMethods";

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                List<PaymentMethod> paymentMethods = new List<PaymentMethod>();

                while (reader.Read())
                {
                    paymentMethods.Add(new PaymentMethod
                    {
                        PaymentMethodID = (int)reader["PaymentMethodID"],
                        MethodName = reader["MethodName"].ToString(),
                        Details = reader["Details"].ToString()
                    });
                }

                response.StatusCode = paymentMethods.Count > 0 ? 200 : 404;
                response.StatusMessage = paymentMethods.Count > 0 ? "Payment methods found." : "No payment methods found.";
                response.Data = paymentMethods;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching payment methods: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Get a payment method by ID
        public Response GetPaymentMethodById(int id, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT PaymentMethodID, MethodName, Details FROM PaymentMethods WHERE PaymentMethodID = @PaymentMethodID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@PaymentMethodID", id);
                connection.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                PaymentMethod paymentMethod = null;

                if (reader.Read())
                {
                    paymentMethod = new PaymentMethod
                    {
                        PaymentMethodID = (int)reader["PaymentMethodID"],
                        MethodName = reader["MethodName"].ToString(),
                        Details = reader["Details"].ToString()
                    };
                }

                response.StatusCode = paymentMethod != null ? 200 : 404;
                response.StatusMessage = paymentMethod != null ? "Payment method found." : "Payment method not found.";
                response.Data = paymentMethod;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error fetching payment method: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        // Remove a payment method by ID
        public Response RemovePaymentMethodById(int id, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "DELETE FROM PaymentMethods WHERE PaymentMethodID = @PaymentMethodID";
                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@PaymentMethodID", id);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                response.StatusCode = rowsAffected > 0 ? 200 : 404;
                response.StatusMessage = rowsAffected > 0 ? "Payment method removed successfully." : "Payment method not found.";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            return response;
        }
        // Update an existing payment method
        public Response UpdatePaymentMethod(PaymentMethod paymentMethod, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                // SQL query to update a payment method by ID
                string query = "UPDATE PaymentMethods SET MethodName = @MethodName, Details = @Details " +
                               "WHERE PaymentMethodID = @PaymentMethodID";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@MethodName", paymentMethod.MethodName);
                cmd.Parameters.AddWithValue("@Details", paymentMethod.Details);
                cmd.Parameters.AddWithValue("@PaymentMethodID", paymentMethod.PaymentMethodID);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                // Check if any rows were updated
                if (rowsAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Payment method updated successfully.";
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Payment method not found.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error updating payment method: {ex.Message}";
            }
            return response;
        }

        //========================================================================================================================================

        // Add new feedback
        public Response AddFeedback(Feedback feedback, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                // SQL query to insert feedback into the database
                string query = "INSERT INTO Feedback (UserID, FeedbackText, DateSubmitted) " +
                               "VALUES (@UserID, @FeedbackText, @DateSubmitted)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserID", feedback.UserID);
                cmd.Parameters.AddWithValue("@FeedbackText", feedback.FeedbackText);
                cmd.Parameters.AddWithValue("@DateSubmitted", feedback.DateSubmitted);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                if (rowsAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Feedback submitted successfully.";
                }
                else
                {
                    response.StatusCode = 500;
                    response.StatusMessage = "Error submitting feedback.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            return response;
        }

        // Get all feedbacks
        public Response GetAllFeedbacks(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT * FROM Feedback";
                List<Feedback> feedbacks = new List<Feedback>();

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    feedbacks.Add(new Feedback
                    {
                        FeedbackID = (int)reader["FeedbackID"],
                        UserID = (int)reader["UserID"],
                        FeedbackText = reader["FeedbackText"].ToString(),
                        DateSubmitted = (DateTime)reader["DateSubmitted"]
                    });
                }
                connection.Close();

                response.StatusCode = 200;
                response.StatusMessage = "Feedbacks retrieved successfully.";
                response.Data = feedbacks;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error retrieving feedbacks: {ex.Message}";
            }
            return response;
        }

        // Get feedback by UserID
        public Response GetFeedbackByUserId(int userId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT * FROM Feedback WHERE UserID = @UserID";
                List<Feedback> feedbacks = new List<Feedback>();

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserID", userId);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    feedbacks.Add(new Feedback
                    {
                        FeedbackID = (int)reader["FeedbackID"],
                        UserID = (int)reader["UserID"],
                        FeedbackText = reader["FeedbackText"].ToString(),
                        DateSubmitted = (DateTime)reader["DateSubmitted"]
                    });
                }
                connection.Close();

                if (feedbacks.Count > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Feedbacks retrieved successfully.";
                    response.Data = feedbacks;
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "No feedbacks found for this user.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error retrieving feedback: {ex.Message}";
            }
            return response;
        }
        //========================================================================================================================================

        // Add new progress record
        public Response AddProgressTracking(ProgressTracking progress, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                // SQL query to insert progress into the database
                string query = "INSERT INTO ProgressTracking (UserID, Details, LastUpdated) " +
                               "VALUES (@UserID, @Details, @LastUpdated)";

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserID", progress.UserID);
                cmd.Parameters.AddWithValue("@Details", progress.Details);
                cmd.Parameters.AddWithValue("@LastUpdated", progress.LastUpdated);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();

                if (rowsAffected > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Progress tracking added successfully.";
                }
                else
                {
                    response.StatusCode = 500;
                    response.StatusMessage = "Error adding progress tracking.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            return response;
        }

        // Get all progress records
        public Response GetAllProgressTracking(SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT * FROM ProgressTracking";
                List<ProgressTracking> progressList = new List<ProgressTracking>();

                using SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    progressList.Add(new ProgressTracking
                    {
                        ProgressID = (int)reader["ProgressID"],
                        UserID = (int)reader["UserID"],
                        Details = reader["Details"].ToString(),
                        LastUpdated = (DateTime)reader["LastUpdated"]
                    });
                }
                connection.Close();

                response.StatusCode = 200;
                response.StatusMessage = "Progress records retrieved successfully.";
                response.Data = progressList;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error retrieving progress records: {ex.Message}";
            }
            return response;
        }

        // Get progress by UserID
        public Response GetProgressByUserId(int userId, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                string query = "SELECT * FROM ProgressTracking WHERE UserID = @UserID";
                List<ProgressTracking> progressList = new List<ProgressTracking>();

                using SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserID", userId);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    progressList.Add(new ProgressTracking
                    {
                        ProgressID = (int)reader["ProgressID"],
                        UserID = (int)reader["UserID"],
                        Details = reader["Details"].ToString(),
                        LastUpdated = (DateTime)reader["LastUpdated"]
                    });
                }
                connection.Close();

                if (progressList.Count > 0)
                {
                    response.StatusCode = 200;
                    response.StatusMessage = "Progress records retrieved successfully.";
                    response.Data = progressList;
                }
                else
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "No progress records found for this user.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error retrieving progress records: {ex.Message}";
            }
            return response;
        }
        //========================================================================================================================================

        // Add new song notation
        public Response AddSongNotation(LatestSongNotations songNotation, SqlConnection connection)
        {
            Response response = new Response();
            try
            {
                const string query = @"
            INSERT INTO LatestSongNotations (SongTitle, ArtistName, Genre, DifficultyLevel, Notation, DateAdded) 
            VALUES (@SongTitle, @ArtistName, @Genre, @DifficultyLevel, @Notation, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SongTitle", songNotation.SongTitle);
                    cmd.Parameters.AddWithValue("@ArtistName", string.IsNullOrEmpty(songNotation.ArtistName) ? DBNull.Value : songNotation.ArtistName);
                    cmd.Parameters.AddWithValue("@Genre", string.IsNullOrEmpty(songNotation.Genre) ? DBNull.Value : songNotation.Genre);
                    cmd.Parameters.AddWithValue("@DifficultyLevel", string.IsNullOrEmpty(songNotation.DifficultyLevel) ? DBNull.Value : songNotation.DifficultyLevel);
                    cmd.Parameters.AddWithValue("@Notation", songNotation.Notation);

                    connection.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    response.StatusCode = rowsAffected > 0 ? 200 : 500;
                    response.StatusMessage = rowsAffected > 0 ? "Song notation added successfully." : "Error adding song notation.";
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return response;
        }

        // Get all song notations
        public Response GetAllSongNotations(SqlConnection connection)
        {
            Response response = new Response();
            List<LatestSongNotations> songNotations = new List<LatestSongNotations>();

            try
            {
                const string query = "SELECT * FROM LatestSongNotations ORDER BY DateAdded DESC";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            songNotations.Add(new LatestSongNotations
                            {
                                NotationID = Convert.ToInt32(reader["NotationID"]),
                                SongTitle = reader["SongTitle"].ToString(),
                                ArtistName = reader["ArtistName"] != DBNull.Value ? reader["ArtistName"].ToString() : null,
                                Genre = reader["Genre"] != DBNull.Value ? reader["Genre"].ToString() : null,
                                DifficultyLevel = reader["DifficultyLevel"] != DBNull.Value ? reader["DifficultyLevel"].ToString() : null,
                                Notation = reader["Notation"].ToString(),
                                DateAdded = Convert.ToDateTime(reader["DateAdded"])
                            });
                        }
                    }
                }

                response.StatusCode = songNotations.Count > 0 ? 200 : 404;
                response.StatusMessage = songNotations.Count > 0 ? "Song notations retrieved successfully." : "No song notations found.";
                response.Data = songNotations;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error retrieving song notations: {ex.Message}";
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return response;
        }

        // Get notation by Song Title
        public Response GetNotationBySongTitle(string songTitle, SqlConnection connection)
        {
            Response response = new Response();
            List<LatestSongNotations> songNotations = new List<LatestSongNotations>();

            try
            {
                const string query = "SELECT * FROM LatestSongNotations WHERE SongTitle = @SongTitle";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@SongTitle", songTitle);

                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            songNotations.Add(new LatestSongNotations
                            {
                                NotationID = Convert.ToInt32(reader["NotationID"]),
                                SongTitle = reader["SongTitle"].ToString(),
                                ArtistName = reader["ArtistName"] != DBNull.Value ? reader["ArtistName"].ToString() : null,
                                Genre = reader["Genre"] != DBNull.Value ? reader["Genre"].ToString() : null,
                                DifficultyLevel = reader["DifficultyLevel"] != DBNull.Value ? reader["DifficultyLevel"].ToString() : null,
                                Notation = reader["Notation"].ToString(),
                                DateAdded = Convert.ToDateTime(reader["DateAdded"])
                            });
                        }
                    }
                }

                response.StatusCode = songNotations.Count > 0 ? 200 : 404;
                response.StatusMessage = songNotations.Count > 0 ? "Song notation retrieved successfully." : "No notation found for this song.";
                response.Data = songNotations;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"Error retrieving song notation: {ex.Message}";
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }

            return response;
        }
        //======================================================================================

        public Response AddMusician(Musician musician, SqlConnection connection)
        {
            try
            {
                string query = "INSERT INTO Musician (MusicianID, FullName, BirthDate, Country, ContactNumber, Genre) VALUES (@MusicianID, @FullName, @BirthDate, @Country, @ContactNumber, @Genre)";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@MusicianID", musician.MusicianID);
                cmd.Parameters.AddWithValue("@FullName", musician.FullName);
                cmd.Parameters.AddWithValue("@BirthDate", musician.BirthDate);
                cmd.Parameters.AddWithValue("@Country", musician.Country);
                cmd.Parameters.AddWithValue("@ContactNumber", musician.ContactNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Genre", musician.Genre ?? (object)DBNull.Value);

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
                return new Response { StatusCode = 200, StatusMessage = "Musician added successfully." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }

        public List<Musician> GetAllMusicians(SqlConnection connection)
        {
            List<Musician> musicians = new();
            try
            {
                string query = "SELECT * FROM Musician";
                using SqlCommand cmd = new(query, connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    musicians.Add(new Musician
                    {
                        MusicianID = reader["MusicianID"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                        Country = reader["Country"].ToString(),
                        ContactNumber = reader["ContactNumber"].ToString(),
                        Genre = reader["Genre"].ToString()
                    });
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return musicians;
        }

        public Response UpdateMusician(Musician musician, SqlConnection connection)
        {
            try
            {
                string query = "UPDATE Musician SET FullName=@FullName, BirthDate=@BirthDate, Country=@Country, ContactNumber=@ContactNumber, Genre=@Genre WHERE MusicianID=@MusicianID";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@MusicianID", musician.MusicianID);
                cmd.Parameters.AddWithValue("@FullName", musician.FullName);
                cmd.Parameters.AddWithValue("@BirthDate", musician.BirthDate);
                cmd.Parameters.AddWithValue("@Country", musician.Country);
                cmd.Parameters.AddWithValue("@ContactNumber", musician.ContactNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Genre", musician.Genre ?? (object)DBNull.Value);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                return rowsAffected > 0 ? new Response { StatusCode = 200, StatusMessage = "Musician updated successfully." } : new Response { StatusCode = 400, StatusMessage = "Update failed." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }

        public Response DeleteMusician(string id, SqlConnection connection)
        {
            try
            {
                string query = "DELETE FROM Musician WHERE MusicianID=@MusicianID";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@MusicianID", id);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                return rowsAffected > 0 ? new Response { StatusCode = 200, StatusMessage = "Musician deleted successfully." } : new Response { StatusCode = 400, StatusMessage = "Delete failed." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }
            //==================================================================================
             public Response AddArtist(Artist artist, SqlConnection connection)
        {
            try
            {
                string query = "INSERT INTO Artist (ArtistID, FullName, BirthDate, Country, ContactNumber, ArtStyle) VALUES (@ArtistID, @FullName, @BirthDate, @Country, @ContactNumber, @ArtStyle)";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@ArtistID", artist.ArtistID);
                cmd.Parameters.AddWithValue("@FullName", artist.FullName);
                cmd.Parameters.AddWithValue("@BirthDate", artist.BirthDate);
                cmd.Parameters.AddWithValue("@Country", artist.Country);
                cmd.Parameters.AddWithValue("@ContactNumber", artist.ContactNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ArtStyle", artist.ArtStyle ?? (object)DBNull.Value);

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
                return new Response { StatusCode = 200, StatusMessage = "Artist added successfully." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }

        public List<Artist> GetAllArtists(SqlConnection connection)
        {
            List<Artist> artists = new();
            try
            {
                string query = "SELECT * FROM Artist";
                using SqlCommand cmd = new(query, connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    artists.Add(new Artist
                    {
                        ArtistID = reader["ArtistID"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                        Country = reader["Country"].ToString(),
                        ContactNumber = reader["ContactNumber"].ToString(),
                        ArtStyle = reader["ArtStyle"].ToString()
                    });
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return artists;
        }

        public Response UpdateArtist(Artist artist, SqlConnection connection)
        {
            try
            {
                string query = "UPDATE Artist SET FullName=@FullName, BirthDate=@BirthDate, Country=@Country, ContactNumber=@ContactNumber, ArtStyle=@ArtStyle WHERE ArtistID=@ArtistID";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@ArtistID", artist.ArtistID);
                cmd.Parameters.AddWithValue("@FullName", artist.FullName);
                cmd.Parameters.AddWithValue("@BirthDate", artist.BirthDate);
                cmd.Parameters.AddWithValue("@Country", artist.Country);
                cmd.Parameters.AddWithValue("@ContactNumber", artist.ContactNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@ArtStyle", artist.ArtStyle ?? (object)DBNull.Value);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                return rowsAffected > 0 ? new Response { StatusCode = 200, StatusMessage = "Artist updated successfully." } : new Response { StatusCode = 400, StatusMessage = "Update failed." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }

        public Response DeleteArtist(string id, SqlConnection connection)
        {
            try
            {
                string query = "DELETE FROM Artist WHERE ArtistID=@ArtistID";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@ArtistID", id);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                return rowsAffected > 0 ? new Response { StatusCode = 200, StatusMessage = "Artist deleted successfully." } : new Response { StatusCode = 400, StatusMessage = "Delete failed." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }
            //==================================================================================

              public Response AddInstructor(Instructor instructor, SqlConnection connection)
        {
            try
            {
                string query = "INSERT INTO Instructor (InstructorID, FullName, BirthDate, Country, ContactNumber, Specialization) VALUES (@InstructorID, @FullName, @BirthDate, @Country, @ContactNumber, @Specialization)";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@InstructorID", instructor.InstructorID);
                cmd.Parameters.AddWithValue("@FullName", instructor.FullName);
                cmd.Parameters.AddWithValue("@BirthDate", instructor.BirthDate);
                cmd.Parameters.AddWithValue("@Country", instructor.Country);
                cmd.Parameters.AddWithValue("@ContactNumber", instructor.ContactNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", instructor.Specialization ?? (object)DBNull.Value);

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
                return new Response { StatusCode = 200, StatusMessage = "Instructor added successfully." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }

        public List<Instructor> GetAllInstructors(SqlConnection connection)
        {
            List<Instructor> instructors = new();
            try
            {
                string query = "SELECT * FROM Instructor";
                using SqlCommand cmd = new(query, connection);
                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    instructors.Add(new Instructor
                    {
                        InstructorID = reader["InstructorID"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                        Country = reader["Country"].ToString(),
                        ContactNumber = reader["ContactNumber"].ToString(),
                        Specialization = reader["Specialization"].ToString()
                    });
                }
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return instructors;
        }

        public Response UpdateInstructor(Instructor instructor, SqlConnection connection)
        {
            try
            {
                string query = "UPDATE Instructor SET FullName=@FullName, BirthDate=@BirthDate, Country=@Country, ContactNumber=@ContactNumber, Specialization=@Specialization WHERE InstructorID=@InstructorID";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@InstructorID", instructor.InstructorID);
                cmd.Parameters.AddWithValue("@FullName", instructor.FullName);
                cmd.Parameters.AddWithValue("@BirthDate", instructor.BirthDate);
                cmd.Parameters.AddWithValue("@Country", instructor.Country);
                cmd.Parameters.AddWithValue("@ContactNumber", instructor.ContactNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Specialization", instructor.Specialization ?? (object)DBNull.Value);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                return rowsAffected > 0 ? new Response { StatusCode = 200, StatusMessage = "Instructor updated successfully." } : new Response { StatusCode = 400, StatusMessage = "Update failed." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }

        public Response DeleteInstructor(string id, SqlConnection connection)
        {
            try
            {
                string query = "DELETE FROM Instructor WHERE InstructorID=@InstructorID";
                using SqlCommand cmd = new(query, connection);
                cmd.Parameters.AddWithValue("@InstructorID", id);

                connection.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                connection.Close();
                return rowsAffected > 0 ? new Response { StatusCode = 200, StatusMessage = "Instructor deleted successfully." } : new Response { StatusCode = 400, StatusMessage = "Delete failed." };
            }
            catch (Exception ex)
            {
                return new Response { StatusCode = 500, StatusMessage = ex.Message };
            }
        }


            //====================================================================================================================================================


            // Add a new response to the database
            public Response AddResponse(Respond respond, SqlConnection connection)
            {
                try
                {
                    using SqlCommand command = new SqlCommand("INSERT INTO Respond (RequestID, ResponderID, RespondBody, DateResponded, RequesterID) " +
                                                              "VALUES (@RequestID, @ResponderID, @RespondBody, @DateResponded, @RequesterID)", connection);
                    command.Parameters.AddWithValue("@RequestID", respond.RequestID);
                    command.Parameters.AddWithValue("@ResponderID", respond.ResponderID);
                    command.Parameters.AddWithValue("@RespondBody", respond.RespondBody);
                    command.Parameters.AddWithValue("@DateResponded", respond.DateResponded);
                    command.Parameters.AddWithValue("@RequesterID", respond.RequesterID);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    return result > 0 ? new Response { StatusCode = 200, StatusMessage = "Response added successfully." } :
                                        new Response { StatusCode = 400, StatusMessage = "Failed to add response." };
                }
                catch (Exception ex)
                {
                    return new Response { StatusCode = 500, StatusMessage = $"Error: {ex.Message}" };
                }
            }

            // Get all responses
            public List<Respond> GetAllResponses(SqlConnection connection)
            {
                List<Respond> responses = new List<Respond>();

                try
                {
                    using SqlCommand command = new SqlCommand("SELECT ResponseID, RequestID, ResponderID, RespondBody, DateResponded, RequesterID FROM Respond", connection);
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        responses.Add(new Respond
                        {
                            ResponseID = reader.GetInt32(0),
                            RequestID = reader.GetInt32(1),
                            ResponderID = reader.GetInt32(2),
                            RespondBody = reader.GetString(3),
                            DateResponded = reader.GetDateTime(4),
                            RequesterID = reader.GetString(5)
                        });
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    // Handle error
                    Console.WriteLine($"Error: {ex.Message}");
                }

                return responses;
            }

            // Get responses by RequesterID
            public List<Respond> GetResponsesByRequester(string requesterId, SqlConnection connection)
            {
                List<Respond> responses = new List<Respond>();

                try
                {
                    using SqlCommand command = new SqlCommand("SELECT ResponseID, RequestID, ResponderID, RespondBody, DateResponded, RequesterID FROM Respond WHERE RequesterID = @RequesterID", connection);
                    command.Parameters.AddWithValue("@RequesterID", requesterId);

                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        responses.Add(new Respond
                        {
                            ResponseID = reader.GetInt32(0),
                            RequestID = reader.GetInt32(1),
                            ResponderID = reader.GetInt32(2),
                            RespondBody = reader.GetString(3),
                            DateResponded = reader.GetDateTime(4),
                            RequesterID = reader.GetString(5)
                        });
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    // Handle error
                    Console.WriteLine($"Error: {ex.Message}");
                }

                return responses;
            }

            // Get responses by ResponderID
            public List<Respond> GetResponsesByResponder(string responderId, SqlConnection connection)
            {
                List<Respond> responses = new List<Respond>();

                try
                {
                    using SqlCommand command = new SqlCommand("SELECT ResponseID, RequestID, ResponderID, RespondBody, DateResponded, RequesterID FROM Respond WHERE ResponderID = @ResponderID", connection);
                    command.Parameters.AddWithValue("@ResponderID", responderId);

                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        responses.Add(new Respond
                        {
                            ResponseID = reader.GetInt32(0),
                            RequestID = reader.GetInt32(1),
                            ResponderID = reader.GetInt32(2),
                            RespondBody = reader.GetString(3),
                            DateResponded = reader.GetDateTime(4),
                            RequesterID = reader.GetString(5)
                        });
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    // Handle error
                    Console.WriteLine($"Error: {ex.Message}");
                }

                return responses;
            }

            // Update an existing response
            public Response UpdateResponse(Respond respond, SqlConnection connection)
            {
                try
                {
                    using SqlCommand command = new SqlCommand("UPDATE Respond SET RequestID = @RequestID, ResponderID = @ResponderID, " +
                                                              "RespondBody = @RespondBody, DateResponded = @DateResponded, RequesterID = @RequesterID " +
                                                              "WHERE ResponseID = @ResponseID", connection);
                    command.Parameters.AddWithValue("@RequestID", respond.RequestID);
                    command.Parameters.AddWithValue("@ResponderID", respond.ResponderID);
                    command.Parameters.AddWithValue("@RespondBody", respond.RespondBody);
                    command.Parameters.AddWithValue("@DateResponded", respond.DateResponded);
                    command.Parameters.AddWithValue("@RequesterID", respond.RequesterID);
                    command.Parameters.AddWithValue("@ResponseID", respond.ResponseID);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    return result > 0 ? new Response { StatusCode = 200, StatusMessage = "Response updated successfully." } :
                                        new Response { StatusCode = 400, StatusMessage = "Failed to update response." };
                }
                catch (Exception ex)
                {
                    return new Response { StatusCode = 500, StatusMessage = $"Error: {ex.Message}" };
                }
            }

            // Remove a response
            public Response RemoveResponse(string id, SqlConnection connection)
            {
                try
                {
                    using SqlCommand command = new SqlCommand("DELETE FROM Respond WHERE ResponseID = @ResponseID", connection);
                    command.Parameters.AddWithValue("@ResponseID", id);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();

                    return result > 0 ? new Response { StatusCode = 200, StatusMessage = "Response removed successfully." } :
                                        new Response { StatusCode = 400, StatusMessage = "Failed to remove response." };
                }
                catch (Exception ex)
                {
                    return new Response { StatusCode = 500, StatusMessage = $"Error: {ex.Message}" };
                }
            }
        }
    }





    
    


