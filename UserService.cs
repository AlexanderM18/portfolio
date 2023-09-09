using Data.Providers;
using Models;
using Models.Domain;
using Models.Domain.Users;
using Models.Requests.Users;
using Services.Interfaces;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data;
using System;
using System.Reflection;

namespace Services
{
    public class UserService : IUserService
    {
        private IAuthenticationService<int> _authenticationService;
        private IDataProvider _dataProvider;
        ILookUpService _lookUpService = null;

        public UserService(IAuthenticationService<int> authSerice, IDataProvider dataProvider, ILookUpService lookUpService)
        {
            _authenticationService = authSerice;
            _dataProvider = dataProvider;
            _lookUpService = lookUpService;
        }

        public async Task<bool> LogInAsync(string email, string password)
        {
            bool isSuccessful = false;
            
            IUserAuthData response = Get(email, password);

            if (response != null)
            {
                Claim fullName = new Claim("CustomClaim", "Sabio Bootcamp");
                await _authenticationService.LogInAsync(response, new Claim[] { fullName });
                isSuccessful = true;
            }

            return isSuccessful;
        }
        public async Task<bool> LogInTest(string email, string password, int id, string[] roles = null)
        {
            bool isSuccessful = false;
            var testRoles = new[] { "User", "Super", "Content Manager" };

            var allRoles = roles == null ? testRoles : testRoles.Concat(roles);

            IUserAuthData response = new UserBase
            {
                Id = id
                ,
                Name = email
                ,
                Roles = allRoles
                ,
                TenantId = "Acme Corp UId"
            };

            Claim fullName = new Claim("CustomClaim", "Sabio Bootcamp");
            await _authenticationService.LogInAsync(response, new Claim[] { fullName });

            return isSuccessful;
        }

        public int Create(UserAddRequest model)
        {
            int id = 0;

            _dataProvider.ExecuteNonQuery("[dbo].[Users_Insert]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);
            },
            returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });
            return id;
        }
        
        private IUserAuthData Get(string email, string password)
        {
            UserBase user = null;
            string passwordFromDb = "";
            bool userConfirmed = false;

            _dataProvider.ExecuteCmd("[dbo].[Users_Select_AuthData]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Email", email);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                user = new UserBase();
                user.Id = reader.GetSafeInt32(startingIndex++);
                user.Name = reader.GetSafeString(startingIndex++);
                passwordFromDb = reader.GetSafeString(startingIndex++);
                user.TenantId = new object();
                user.TenantId = "WePairhealth";
                userConfirmed = reader.GetSafeBool(startingIndex++);
                user.Roles = reader.DeserializeObject<List<string>>(startingIndex++);
            }
            );

            if (user != null)
            {
                bool isValidCredentials = BCrypt.BCryptHelper.CheckPassword(password, passwordFromDb);
                if (!isValidCredentials || !userConfirmed)
                {
                    throw new Exception("Invalid login credentials.");
                }
               
            }
            else if(user == null)
            {
                throw new Exception("An account with that Email does not exist.");
            }

            return user;
        }
        public Paged<User> Pagination(int pageIndex, int pageSize)
        {
            Paged<User> pagedList = null;
            List<User> list = null;
            int totalCount = 0;

            _dataProvider.ExecuteCmd("[dbo].[Users_SelectAll]", inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    User user = MapSingleUser(reader, ref startingIndex);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    if (list == null)
                    {
                        list = new List<User>();
                    }
                    list.Add(user);
                });
            if (list != null)
            {
                pagedList = new Paged<User>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }
        public User GetUser(int id)
        {
            User user = null;

            _dataProvider.ExecuteCmd("[dbo].[Users_Select_ById]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                user = MapSingleUser(reader, ref startingIndex);
            }
            );
            return user;
        }
        public void UpdateConfirm(UserConfirmUpdateRequest model)
        {
            _dataProvider.ExecuteNonQuery("[dbo].[Users_Confirm]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", model.Id);
                col.AddWithValue("@IsConfirmed", model.IsConfirmed);
                col.AddWithValue("@Token", model.Token);
            },
            returnParameters: null);
        }
        public void UpdateStatus(UserUpdateStatus model)
        {
            _dataProvider.ExecuteNonQuery("[dbo].[Users_UpdateStatus]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", model.Id);
                col.AddWithValue("@StatusId", model.StatusId);
            },
            returnParameters: null);
        }
        public UserAuth GetAuthData(string email)
        {
            UserAuth user = null;

            _dataProvider.ExecuteCmd("[dbo].[Users_Select_AuthData]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Email", email);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                user = new UserAuth();

                user.Id = reader.GetSafeInt32(startingIndex++);
                user.Email = reader.GetSafeString(startingIndex++);
                user.Password = reader.GetSafeString(startingIndex++);
                user.Roles = reader.DeserializeObject<List<LookUp>>(startingIndex++);
            }
            );
            return user;
        }
        public BaseUserAuth GetPassword(string email)
        {
            BaseUserAuth user = null;

            _dataProvider.ExecuteCmd("[dbo].[Users_SelectPass_ByEmail]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Email", email);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                user = new BaseUserAuth();
                user.Password = reader.GetSafeString(startingIndex++);
            }
            );
            return user;
        }
        private static void AddCommonParams(UserAddRequest model, SqlParameterCollection col)
        {

            col.AddWithValue("@Email", model.Email);
            col.AddWithValue("@FirstName", model.FirstName);
            col.AddWithValue("@LastName", model.LastName);
            col.AddWithValue("@Mi", model.Mi);
            col.AddWithValue("@AvatarUrl", model.AvatarUrl);
            col.AddWithValue("@Password", GenerateSalt(model.Password));
            col.AddWithValue("@IsConfirmed", model.IsConfirmed);
            col.AddWithValue("@StatusId", model.StatusId);
        }
        private static string GenerateSalt(string password)
        {
            string salt = BCrypt.BCryptHelper.GenerateSalt();
            return BCrypt.BCryptHelper.HashPassword(password, salt);
        }
        private User MapSingleUser(IDataReader reader, ref int startingIndex)
        {
            User aUser = new User();

            aUser.Id = reader.GetSafeInt32(startingIndex++);
            aUser.Email = reader.GetSafeString(startingIndex++);
            aUser.FirstName = reader.GetSafeString(startingIndex++);
            aUser.LastName = reader.GetSafeString(startingIndex++);
            aUser.Mi = reader.GetSafeString(startingIndex++);
            aUser.AvatarUrl = reader.GetSafeString(startingIndex++);
            aUser.IsConfirmed = reader.GetSafeBool(startingIndex++);
            aUser.Status = _lookUpService.MapSingleLookUp(reader, ref startingIndex);
            aUser.DateCreated = reader.GetSafeDateTime(startingIndex++);
            aUser.DateModified = reader.GetSafeDateTime(startingIndex++);

            return aUser;
        }
        public void AddToken(TokenAddRequest model)
        {
            _dataProvider.ExecuteNonQuery("[dbo].[UserTokens_Insert]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                TokenCommonParams(model, col);
            },
            returnParameters: null);

        }
        public void Delete(string Token)
        {
            _dataProvider.ExecuteNonQuery("[dbo].[UserTokens_Delete_ByToken]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Token", Token);
            },
                returnParameters: null);
        }
        public List<UserTokens> GetToken(int id)
        {
            List<UserTokens> list = null;

            _dataProvider.ExecuteCmd("[dbo].[UserTokens_Select_ByTokenType]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@TokenType", id);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                UserTokens tokenType = MapTokenByType(reader, ref startingIndex);

                if (list == null)
                {
                    list = new List<UserTokens>();
                }

                list.Add(tokenType);
            }
            );

            return list;
        }
        private static void TokenCommonParams(TokenAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Token", model.Token);
            col.AddWithValue("@UserId", model.UserId);
            col.AddWithValue("@TokenType", model.TokenType);
        }
        private static UserTokens MapTokenByType(IDataReader reader, ref int startingIndex)
        {
            UserTokens aToken = new UserTokens();

            aToken.Token = reader.GetSafeString(startingIndex++);
            aToken.UserId = reader.GetSafeInt32(startingIndex++);

            return aToken;
        }
        public void ConfirmUserEmail(string token)
        {
            if (token != null)
            {
                UserConfirmUpdateRequest confirmUpdate = new UserConfirmUpdateRequest
                {
                    IsConfirmed = true,
                    Token = token
                };
            }
            
        }
       
    }
}
