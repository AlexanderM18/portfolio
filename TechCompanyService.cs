using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Domain.Jobs;
using Sabio.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sabio.Models.Domain.TechCompanies;
using Sabio.Models.Domain.Friends;
using Sabio.Models.Domain;
using Sabio.Services.Interfaces;
using Sabio.Models.Requests.TechComps;
using Sabio.Models.Domain.Events;

namespace Sabio.Services
{
    public class TechCompanyService : ITechCompanyService
    {
        IDataProvider _data = null;
        public TechCompanyService(IDataProvider data)
        {
            _data = data;
        }

        #region Get
        public TechCompany Get(int id)
        {

            string procName = "[dbo].[TechCompanies_SelectById]";

            TechCompany techCompany = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                techCompany = MapSingleComp(reader, ref startingIndex);
            }
            );

            return techCompany;
        }

        public List<TechCompany> GetAll()
        {
            List<TechCompany> list = null;

            string procName = "[dbo].[TechCompanies_SelectAll]";

            _data.ExecuteCmd(procName, inputParamMapper: null,
           singleRecordMapper: delegate (IDataReader reader, short set)
           {
               int startingIndex = 0;
               TechCompany aComp = MapSingleComp(reader, ref startingIndex);

               if (list == null)
               {
                   list = new List<TechCompany>();
               }

               list.Add(aComp);
           }
            );

            return list;
        }

        public Paged<TechCompany> Pagination(int pageIndex, int pageSize)
        {
            Paged<TechCompany> pagedList = null;
            List<TechCompany> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[TechCompanies_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    TechCompany techComp = MapSingleComp(reader, ref startingIndex);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }

                    if (list == null)
                    {
                        list = new List<TechCompany>();
                    }
                    list.Add(techComp);
                });
            if (list != null)
            {
                pagedList = new Paged<TechCompany>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }

        public Paged<TechCompany> SearchPagination(int pageIndex, int pageSize, string query = "")
        {
            Paged<TechCompany> pagedList = null;
            List<TechCompany> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[TechCompanies_Search_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                    param.AddWithValue("@Query", query);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    TechCompany aComp = MapSingleComp(reader, ref startingIndex);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    if (list == null)
                    {
                        list = new List<TechCompany>();
                    }
                    list.Add(aComp);
                });
            if (list != null)
            {
                pagedList = new Paged<TechCompany>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }
        #endregion

        #region Add/Update
        public int Add(TechCompAddRequest model)
        {
            int compId = 0;

            DataTable tagsTable = new DataTable();
            tagsTable.Columns.Add("Tag", typeof(string));

            if (model.Tags != null && model.Tags.Count > 0)
            {
                foreach (string tag in model.Tags)
                {
                    DataRow tagRow = tagsTable.NewRow();
                    tagRow["Tag"] = tag;
                    tagsTable.Rows.Add(tagRow);
                }
            }
            DataTable urlsTable = new DataTable();
            urlsTable.Columns.Add("Url", typeof(string));

            if (model.Urls != null && model.Urls.Count > 0)
            {
                foreach (string url in model.Urls)
                {
                    DataRow urlRow = urlsTable.NewRow();
                    urlRow["Url"] = url;
                    urlsTable.Rows.Add(urlRow);
                }
            }

            DataTable imagesTable = new DataTable();
            imagesTable.Columns.Add("Url", typeof(string));
            imagesTable.Columns.Add("TypeId", typeof(int));

            if (model.Images != null && model.Images.Count > 0)
            {
                foreach (var image in model.Images)
                {
                    DataRow imageRow = imagesTable.NewRow();
                    imageRow["Url"] = image.Url;
                    imageRow["TypeId"] = image.TypeId;
                    imagesTable.Rows.Add(imageRow);
                }
            }

            DataTable contactsTable = new DataTable();
            contactsTable.Columns.Add("Email", typeof(string));
            contactsTable.Columns.Add("PhoneNumber", typeof(string));

            if (model.ContactInfo != null && model.ContactInfo.Count > 0)
            {
                foreach (var contact in model.ContactInfo)
                {
                    DataRow contactRow = contactsTable.NewRow();
                    contactRow["Email"] = contact.Email;
                    contactRow["PhoneNumber"] = contact.PhoneNumber;
                    contactsTable.Rows.Add(contactRow);
                }
            }

            // Call the stored procedure
            _data.ExecuteNonQuery("[dbo].[TechCompanies_Insert]", inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@Headline", model.Headline);
                paramCollection.AddWithValue("@Summary", model.Summary);
                paramCollection.AddWithValue("@Profile", model.Profile);
                paramCollection.AddWithValue("@Slug", model.Slug);
                paramCollection.AddWithValue("@StatusId", model.StatusId);
                SqlParameter tagsParam = new SqlParameter("@BatchTags", SqlDbType.Structured);
                tagsParam.Value = tagsTable;
                paramCollection.Add(tagsParam);
                SqlParameter urlsParam = new SqlParameter("@BatchUrls", SqlDbType.Structured);
                urlsParam.Value = urlsTable;
                paramCollection.Add(urlsParam);

                SqlParameter imagesParam = new SqlParameter("@BatchImages", SqlDbType.Structured);
                imagesParam.Value = imagesTable;
                paramCollection.Add(imagesParam);

                SqlParameter contactsParam = new SqlParameter("@BatchContacts", SqlDbType.Structured);
                contactsParam.Value = contactsTable;
                paramCollection.Add(contactsParam);
                SqlParameter idParam = new SqlParameter("@Id", SqlDbType.Int);
                idParam.Direction = ParameterDirection.Output;
                paramCollection.Add(idParam);
            },
            returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;
                int.TryParse(oId.ToString(), out compId);
            });

            return compId;
        }
        public void Update(TechCompUpdateRequest model)
        {// DataTable for the Tags
            DataTable tagsTable = new DataTable();
            tagsTable.Columns.Add("Tag", typeof(string));

            if (model.Tags != null && model.Tags.Count > 0)
            {
                foreach (string tag in model.Tags)
                {
                    DataRow tagRow = tagsTable.NewRow();
                    tagRow["Tag"] = tag;
                    tagsTable.Rows.Add(tagRow);
                }
            }

            // DataTable for the Urls
            DataTable urlsTable = new DataTable();
            urlsTable.Columns.Add("Url", typeof(string));

            if (model.Urls != null && model.Urls.Count > 0)
            {
                foreach (string url in model.Urls)
                {
                    DataRow urlRow = urlsTable.NewRow();
                    urlRow["Url"] = url;
                    urlsTable.Rows.Add(urlRow);
                }
            }

            // DataTable for the Images
            DataTable imagesTable = new DataTable();
            imagesTable.Columns.Add("Url", typeof(string));
            imagesTable.Columns.Add("TypeId", typeof(int));

            if (model.Images != null && model.Images.Count > 0)
            {
                foreach (var image in model.Images)
                {
                    DataRow imageRow = imagesTable.NewRow();
                    imageRow["Url"] = image.Url;
                    imageRow["TypeId"] = image.TypeId;
                    imagesTable.Rows.Add(imageRow);
                }
            }

            // DataTable for the ContactInfo
            DataTable contactsTable = new DataTable();
            contactsTable.Columns.Add("Email", typeof(string));
            contactsTable.Columns.Add("PhoneNumber", typeof(string));

            if (model.ContactInfo != null && model.ContactInfo.Count > 0)
            {
                foreach (var contact in model.ContactInfo)
                {
                    DataRow contactRow = contactsTable.NewRow();
                    contactRow["Email"] = contact.Email;
                    contactRow["PhoneNumber"] = contact.PhoneNumber;
                    contactsTable.Rows.Add(contactRow);
                }
            }

            // Call the stored procedure for updating TechCompanies
            _data.ExecuteNonQuery("[dbo].[TechCompanies_Update]", inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", model.Id);
                paramCollection.AddWithValue("@Name", model.Name);
                paramCollection.AddWithValue("@Headline", model.Headline);
                paramCollection.AddWithValue("@Summary", model.Summary);
                paramCollection.AddWithValue("@Profile", model.Profile);
                paramCollection.AddWithValue("@Slug", model.Slug);
                paramCollection.AddWithValue("@StatusId", model.StatusId);

                SqlParameter tagsParam = new SqlParameter("@BatchTags", SqlDbType.Structured);
                tagsParam.Value = tagsTable;
                paramCollection.Add(tagsParam);

                SqlParameter urlsParam = new SqlParameter("@BatchUrls", SqlDbType.Structured);
                urlsParam.Value = urlsTable;
                paramCollection.Add(urlsParam);

                SqlParameter imagesParam = new SqlParameter("@BatchImages", SqlDbType.Structured);
                imagesParam.Value = imagesTable;
                paramCollection.Add(imagesParam);

                SqlParameter contactsParam = new SqlParameter("@BatchContacts", SqlDbType.Structured);
                contactsParam.Value = contactsTable;
                paramCollection.Add(contactsParam);
            });
        }
        #endregion
        #region Delete
        public void Delete(int Id)
        {
            string procName = "[dbo].[TechCompanies_Delete]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", Id);
            },
                returnParameters: null);
        }
        #endregion


        private static TechCompany MapSingleComp(IDataReader reader, ref int startingIndex)
        {
            TechCompany aComp = new TechCompany();

            aComp.Id = reader.GetSafeInt32(startingIndex++);
            aComp.Name = reader.GetSafeString(startingIndex++);
            aComp.Headline = reader.GetSafeString(startingIndex++);
            aComp.Summary = reader.GetSafeString(startingIndex++);
            aComp.Profile = reader.GetSafeString(startingIndex++);
            aComp.Slug = reader.GetSafeString(startingIndex++);
            aComp.StatusId = reader.GetSafeString(startingIndex++);
            aComp.Jobs = reader.DeserializeObject<List<Job>>(startingIndex++); 
            aComp.Friends = reader.DeserializeObject<List<Friend>>(startingIndex++);
            aComp.Tags = reader.DeserializeObject<List<Tags>>(startingIndex++);
            aComp.Urls = reader.DeserializeObject<List<Urls>>(startingIndex++);
            aComp.Contacts = reader.DeserializeObject<List<ContactInfo>>(startingIndex++);
            aComp.Images = reader.DeserializeObject<List<Image>>(startingIndex++);
            aComp.Events = reader.DeserializeObject<List<Event>>(startingIndex++);
            aComp.DateCreated = reader.GetSafeDateTime(startingIndex++);
            aComp.DateModified = reader.GetSafeDateTime(startingIndex++);

            return aComp;
        }
    }
}
