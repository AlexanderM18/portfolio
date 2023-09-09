using Sabio.Data.Providers;
using Sabio.Models.Domain.Users;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sabio.Models.Domain.Friends;
using Sabio.Data;
using Sabio.Models.Requests.Users;
using Sabio.Models.Requests.Friends;
using Sabio.Services.Interfaces;
using Sabio.Models;
using Sabio.Data.Extensions;
using System.Reflection;
using Sabio.Models.Domain;

namespace Sabio.Services
{
    public class FriendService : IFriendService
    {
        IDataProvider _data = null;
        public FriendService(IDataProvider data)
        {
            _data = data;
        }

        #region Get
        public Friend Get(int id)
        {

            string procName = "[dbo].[Friends_SelectById]";

            Friend friend = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {

                paramCollection.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)//single record mapper
            {   //one shape turn into second shape
                //from reader DB >>> Address

               friend = MapSingleFriend(reader);
                //had User aUser
            }
            );

            return friend;
        }

        public List<Friend> GetAll()
        {
            List<Friend> list = null;

            string procName = "[dbo].[Friends_SelectAll]";

            _data.ExecuteCmd(procName, inputParamMapper: null,
           singleRecordMapper: delegate (IDataReader reader, short set)
           {
               Friend aFriend = MapSingleFriend(reader);

               if (list == null)
               {
                   list = new List<Friend>();
               }

               list.Add(aFriend);
           }
            );

            return list;
        }

        #endregion

        #region Add

        public int Add(FriendAddRequest model, int UserId)
        {
            int id = 0;

            string procName = "[dbo].[Friends_Insert]";

            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);
                col.AddWithValue("@UserId", UserId);
                //and one output

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
        #endregion

        #region Update

        public void Update(FriendUpdateRequest model, int UserId)
        {
            string procName = "[dbo].[Friends_Update]";

            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", model.Id);
                AddCommonParams(model, col);
                col.AddWithValue("@UserId", UserId);
            },
            returnParameters: null);

        }
        #endregion

        #region Delete
        public void Delete(int Id)
        {
            string procName = "[dbo].[Friends_Delete]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", Id);
            },
                returnParameters: null);
        }
        #endregion

        #region PaginateV1
        public Paged<Friend> Pagination(int pageIndex, int pageSize)
        {
            Paged<Friend> pagedList = null;
            List<Friend> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Friends_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    Friend friend = MapSingleFriend(reader);
                    totalCount = reader.GetSafeInt32(14);
                    if (list == null)
                    {
                        list = new List<Friend>();
                    }
                    list.Add(friend);
                });
            if (list != null)
            {
                pagedList = new Paged<Friend>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }
        #endregion

        #region GetV3
        public FriendV3 GetV3(int id)
        {

            string procName = "[dbo].[Friends_SelectByIdV3]";

            FriendV3 friend = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)//single record mapper
            {   //one shape turn into second shape
                //from reader DB >>> Address
                int startingIndex = 0;
                friend = MapSingleFriendV3(reader, ref startingIndex);
                //had User aUser
            }
            );

            return friend;
        }

        public List<FriendV3> GetAllV3()
        {
            List<FriendV3> list = null;

            string procName = "[dbo].[Friends_SelectAllV3]";

            _data.ExecuteCmd(procName, inputParamMapper: null,
           singleRecordMapper: delegate (IDataReader reader, short set)
           {
               int startingIndex = 0;
               FriendV3 aFriend = MapSingleFriendV3(reader, ref startingIndex);

               if (list == null)
               {
                   list = new List<FriendV3>();
               }

               list.Add(aFriend);
           }
            );

            return list;
        }

        public Paged<FriendV3> PaginationV3(int pageIndex, int pageSize)
        {
            Paged<FriendV3> pagedList = null;
            List<FriendV3> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Friends_PaginationV3]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    FriendV3 friend = MapSingleFriendV3(reader, ref startingIndex);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    if (list == null)
                    {
                        list = new List<FriendV3>();
                    }
                    list.Add(friend);
                });
            if (list != null)
            {
                pagedList = new Paged<FriendV3>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }

        public Paged<FriendV3> SearchPaginationV3(int pageIndex, int pageSize, string query="")
        {
            Paged<FriendV3> pagedList = null;
            List<FriendV3> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Friends_Search_PaginationV3]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                    param.AddWithValue("@Query", query);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    FriendV3 friend = MapSingleFriendV3(reader, ref startingIndex);
                    if(totalCount == 0) 
                    { 
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    

                    if (list == null)
                    {
                        list = new List<FriendV3>();
                    }
                    list.Add(friend);
                });
            if (list != null)
            {
                pagedList = new Paged<FriendV3>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }
        #endregion

        #region Add/InsertV3
        public int AddV3(FriendAddRequestV3 model, int UserId)
        {
            int friendId = 0;

            // Create a DataTable for the skills data
            DataTable skillsTable = new DataTable();
            skillsTable.Columns.Add("Name", typeof(string));

            if (model.Skills != null && model.Skills.Count > 0)
            {
                foreach (var skillObject in model.Skills)
                {
                    DataRow skillRow = skillsTable.NewRow();
                    skillRow["Name"] = skillObject.Name;
                    skillsTable.Rows.Add(skillRow);
                }
            }

            // Call the stored procedure
            _data.ExecuteNonQuery("[dbo].[Friends_InsertV3]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Title", model.Title);
                col.AddWithValue("@Bio", model.Bio);
                col.AddWithValue("@Summary", model.Summary);
                col.AddWithValue("@Headline", model.Headline);
                col.AddWithValue("@Slug", model.Slug);
                col.AddWithValue("@StatusId", model.StatusId);
                col.AddWithValue("@ImageTypeId", model.ImageTypeId);
                col.AddWithValue("@ImageUrl", model.ImageUrl);
                col.AddWithValue("@UserId", UserId);
                SqlParameter skillsParam = new SqlParameter("@BatchSkills", SqlDbType.Structured);
                skillsParam.Value = skillsTable;
                col.Add(skillsParam);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;
                col.Add(idOut);

            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;
                int.TryParse(oId.ToString(), out friendId);
            });

            return friendId;
        }


        public void UpdateV3(FriendUpdateRequestV3 model, int UserId)
        {
            // Create a DataTable for the skills data
            DataTable skillsTable = new DataTable();
            skillsTable.Columns.Add("Name", typeof(string));

            if (model.Skills != null && model.Skills.Count > 0)
            {
                foreach (var skillObject in model.Skills)
                {
                    DataRow skillRow = skillsTable.NewRow();
                    skillRow["Name"] = skillObject.Name;
                    skillsTable.Rows.Add(skillRow);
                }
            }

            // Call the stored procedure
            _data.ExecuteNonQuery("[dbo].[Friends_UpdateV3]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", model.Id);  // Assuming model has Id property for update
                col.AddWithValue("@Title", model.Title);
                col.AddWithValue("@Bio", model.Bio);
                col.AddWithValue("@Summary", model.Summary);
                col.AddWithValue("@Headline", model.Headline);
                col.AddWithValue("@Slug", model.Slug);
                col.AddWithValue("@StatusId", model.StatusId);
                col.AddWithValue("@ImageTypeId", model.ImageTypeId);
                col.AddWithValue("@ImageUrl", model.ImageUrl);
                col.AddWithValue("@UserId", UserId);

                SqlParameter skillsParam = new SqlParameter("@BatchSkills", SqlDbType.Structured);
                skillsParam.Value = skillsTable;
                col.Add(skillsParam);
            });
        }
        #endregion

        #region Delete
        public void DeleteV3(int Id)
        {
            string procName = "[dbo].[Friends_DeleteV3]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", Id);
            },
                returnParameters: null);
        }
        #endregion

        private static FriendV3 MapSingleFriendV3(IDataReader reader, ref int startingIndex)
        {
            FriendV3 aFriend = new FriendV3();

            aFriend.Id = reader.GetSafeInt32(startingIndex++);
            aFriend.Title = reader.GetSafeString(startingIndex++);
            aFriend.Bio = reader.GetSafeString(startingIndex++);
            aFriend.Summary = reader.GetSafeString(startingIndex++);
            aFriend.Headline = reader.GetSafeString(startingIndex++);
            aFriend.Slug = reader.GetSafeString(startingIndex++);
            aFriend.StatusId = reader.GetSafeInt32(startingIndex++);
            aFriend.PrimaryImage = new Image();
            aFriend.PrimaryImage.Id = reader.GetSafeInt32(startingIndex++);
            aFriend.PrimaryImage.TypeId = reader.GetSafeInt32(startingIndex++);
            aFriend.PrimaryImage.Url = reader.GetSafeString(startingIndex++);
            aFriend.Skills = reader.DeserializeObject<List<Skill>>(startingIndex++);
            aFriend.UserId = reader.GetSafeInt32(startingIndex++);
            aFriend.DateCreated = reader.GetSafeDateTime(startingIndex++);
            aFriend.DateModified = reader.GetSafeDateTime(startingIndex++);

            return aFriend;
        }
        private static Friend MapSingleFriend(IDataReader reader)
        {
            Friend aFriend = new Friend();

            int startingIdex = 0;

            aFriend.Id = reader.GetSafeInt32(startingIdex++);
            aFriend.Title = reader.GetSafeString(startingIdex++);
            aFriend.Bio = reader.GetSafeString(startingIdex++);
            aFriend.Summary = reader.GetSafeString(startingIdex++);
            aFriend.Headline = reader.GetSafeString(startingIdex++);
            aFriend.Slug = reader.GetSafeString(startingIdex++);
            aFriend.StatusId = reader.GetSafeInt32(startingIdex++);
            aFriend.PrimaryImageUrl = reader.GetSafeString(startingIdex++);
            aFriend.UserId = reader.GetSafeInt32(startingIdex++);
            aFriend.DateCreated = reader.GetSafeDateTime(startingIdex++);
            aFriend.DateModified = reader.GetSafeDateTime(startingIdex++);

            return aFriend;
        }
        private static void AddCommonParams(FriendAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Title", model.Title);
            col.AddWithValue("@Bio", model.Bio);
            col.AddWithValue("@Summary", model.Summary);
            col.AddWithValue("@Headline", model.Headline);
            col.AddWithValue("@Slug", model.Slug);
            col.AddWithValue("@StatusId", model.StatusId);
            col.AddWithValue("@primaryImageUrl", model.PrimaryImageUrl);
        }

    }
}
