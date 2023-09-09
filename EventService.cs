using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Domain.TechCompanies;
using Sabio.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sabio.Models.Domain.Events;
using Sabio.Models.Domain.Friends;
using Sabio.Models.Domain.Jobs;
using Sabio.Models.Requests.TechComps;
using Sabio.Models.Requests.Events;
using Sabio.Services.Interfaces;
using Sabio.Models.Requests.Friends;

namespace Sabio.Services
{
    public class EventService : IEventService
    {
        IDataProvider _data = null;
        public EventService(IDataProvider data)
        {
            _data = data;
        }
        #region Get
        public Event Get(int id)
        {

            string procName = "[dbo].[Events_GetById]";

            Event anEvent = null;

            _data.ExecuteCmd(procName, delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)
            {
                anEvent = MapSingleEvent(reader);
            }
            );

            return anEvent;
        }

        public List<Event> GetAll()
        {
            List<Event> list = null;

            string procName = "[dbo].[Events_GetAll]";

            _data.ExecuteCmd(procName, inputParamMapper: null,
           singleRecordMapper: delegate (IDataReader reader, short set)
           {
               Event anEvent = MapSingleEvent(reader);

               if (list == null)
               {
                   list = new List<Event>();
               }

               list.Add(anEvent);
           }
            );

            return list;
        }

        public Paged<Event> Pagination(int pageIndex, int pageSize)
        {
            Paged<Event> pagedList = null;
            List<Event> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Events_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    Event anEvent = MapSingleEvent(reader);
                    totalCount = reader.GetSafeInt32(13);
                    if (list == null)
                    {
                        list = new List<Event>();
                    }
                    list.Add(anEvent);
                });
            if (list != null)
            {
                pagedList = new Paged<Event>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }

        public Paged<Event> Feed(int pageIndex, int pageSize)
        {
            Paged<Event> pagedList = null;
            List<Event> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Events_Feed]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    Event anEvent = MapSingleEvent(reader);
                    totalCount = reader.GetSafeInt32(13);
                    if (list == null)
                    {
                        list = new List<Event>();
                    }
                    list.Add(anEvent);
                });
            if (list != null)
            {
                pagedList = new Paged<Event>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }

        public Paged<Event> SearchPagination(int pageIndex, int pageSize, string query = "")
        {
            Paged<Event> pagedList = null;
            List<Event> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Events_Search_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                    param.AddWithValue("@Query", query);
                },
                (reader, recordSetIndex) =>
                {
                    Event anEvent = MapSingleEvent(reader);
                    totalCount = reader.GetSafeInt32(13);
                    if (list == null)
                    {
                        list = new List<Event>();
                    }
                    list.Add(anEvent);
                });
            if (list != null)
            {
                pagedList = new Paged<Event>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }
        #endregion

        public int Add(EventAddRequest model)
        {
            int evtId = 0;

            DataTable locationTable = new DataTable();
            locationTable.Columns.Add("latitude", typeof(Double));
            locationTable.Columns.Add("longitude", typeof(Double));
            locationTable.Columns.Add("address", typeof(string));
            locationTable.Columns.Add("city", typeof(string));
            locationTable.Columns.Add("state", typeof(string));
            locationTable.Columns.Add("zipCode", typeof(Int32));

            if (model.Location != null && model.Location.Count > 0)
            {
                foreach (var location in model.Location)
                {
                    DataRow contactRow = locationTable.NewRow();
                    contactRow["latitude"] = location.Latitude;
                    contactRow["longitude"] = location.Longitude;
                    contactRow["address"] = location.Address;
                    contactRow["city"] = location.City;
                    contactRow["state"] = location.State;
                    contactRow["zipCode"] = location.ZipCode;
                    locationTable.Rows.Add(contactRow);
                }
            }

            // Call the stored procedure
            _data.ExecuteNonQuery("[dbo].[Events_Insert]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);
                SqlParameter locationParam = new SqlParameter("@BatchLocations", SqlDbType.Structured);
                locationParam.Value = locationTable;
                col.Add(locationParam);
                SqlParameter idParam = new SqlParameter("@Id", SqlDbType.Int);
                idParam.Direction = ParameterDirection.Output;
                col.Add(idParam);
            },
            returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;
                int.TryParse(oId.ToString(), out evtId);
            });

            return evtId;
        }

        public void Update(EventUpdateRequest model)
        {
            DataTable locationTable = new DataTable();
            locationTable.Columns.Add("latitude", typeof(float));
            locationTable.Columns.Add("longitude", typeof(float));
            locationTable.Columns.Add("address", typeof(string));
            locationTable.Columns.Add("city", typeof(string));
            locationTable.Columns.Add("state", typeof(string));
            locationTable.Columns.Add("zipCode", typeof(Int32));

            if (model.Location != null && model.Location.Count > 0)
            {
                foreach (var location in model.Location)
                {
                    DataRow contactRow = locationTable.NewRow();
                    contactRow["latitude"] = location.Latitude;
                    contactRow["longitude"] = location.Longitude;
                    contactRow["address"] = location.Address;
                    contactRow["city"] = location.City;
                    contactRow["state"] = location.State;
                    contactRow["zipCode"] = location.ZipCode;
                    locationTable.Rows.Add(contactRow);
                }
            }

            // Call the stored procedure
            _data.ExecuteNonQuery("[dbo].[Events_Update]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);
                col.AddWithValue("@Id", model.Id);
                SqlParameter locationParam = new SqlParameter("@BatchLocations", SqlDbType.Structured);
                locationParam.Value = locationTable;
                col.Add(locationParam);
            });
        }

        #region Delete
        public void Delete(int Id)
        {
            string procName = "[dbo].[Events_Delete]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", Id);
            },
                returnParameters: null);
        }
        #endregion

        private static Event MapSingleEvent(IDataReader reader)
        {
            Event anEvent = new Event();

            int startingIndex = 0;

            anEvent.Id = reader.GetSafeInt32(startingIndex++);
            anEvent.DateStart = reader.GetSafeDateTime(startingIndex++);
            anEvent.DateEnd = reader.GetSafeDateTime(startingIndex++);
            anEvent.Name = reader.GetSafeString(startingIndex++);
            anEvent.Headline = reader.GetSafeString(startingIndex++);
            anEvent.Description = reader.GetSafeString(startingIndex++);
            anEvent.Summary = reader.GetSafeString(startingIndex++);
            anEvent.Slug = reader.GetSafeString(startingIndex++);
            anEvent.StatusId = reader.GetSafeString(startingIndex++);
            anEvent.TechCompId = reader.GetSafeInt32(startingIndex++);
            anEvent.Location = reader.DeserializeObject<List<Location>>(startingIndex++);
            anEvent.DateCreated = reader.GetSafeDateTime(startingIndex++);
            anEvent.DateModified = reader.GetSafeDateTime(startingIndex++);

            return anEvent;
        }


        private static void AddCommonParams(EventAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@DateStart", model.DateStart);
            col.AddWithValue("@DateEnd", model.DateEnd);
            col.AddWithValue("@Name", model.Name);
            col.AddWithValue("@Headline", model.Headline);
            col.AddWithValue("@Description", model.Description);
            col.AddWithValue("@Summary", model.Summary);
            col.AddWithValue("@Slug", model.Slug);
            col.AddWithValue("StatusId", model.StatusId);
            col.AddWithValue("@TechCompId", model.TechCompId);
        }
    }
}
