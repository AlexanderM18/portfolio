using Data;
using Data.Providers;
using Models.Domain.Friends;
using Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Domain.Jobs;
using Models.Domain;
using Services.Interfaces;
using Newtonsoft.Json;
using Models.Requests.Friends;
using Models.Requests.Jobs;
using Models.Domain.TechCompanies;

namespace Services
{
    public class JobService : IJobService
    {
        IDataProvider _data = null;
        public JobService(IDataProvider data)
        {
            _data = data;
        }

        #region Get
        public Job Get(int id)
        {
            Job job = null;

            _data.ExecuteCmd("[dbo].[Jobs_SelectById]", delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", id);
            },
            delegate (IDataReader reader, short set)
            {
                int startingIndex = 0;
                job = MapSingleJob(reader, ref startingIndex);
            }
            );

            return job;
        }

        public List<Job> GetAll()
        {
            List<Job> list = null;

            _data.ExecuteCmd("[dbo].[Jobs_SelectAll]", inputParamMapper: null,
           singleRecordMapper: delegate (IDataReader reader, short set)
           {
               int startingIndex = 0;
               Job aJob = MapSingleJob(reader, ref startingIndex);

               if (list == null)
               {
                   list = new List<Job>();
               }

               list.Add(aJob);
           }
            );

            return list;
        }

        public Paged<Job> Pagination(int pageIndex, int pageSize)
        {
            Paged<Job> pagedList = null;
            List<Job> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Jobs_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    Job job = MapSingleJob(reader, ref startingIndex);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    if (list == null)
                    {
                        list = new List<Job>();
                    }
                    list.Add(job);
                });
            if (list != null)
            {
                pagedList = new Paged<Job>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }

        public Paged<Job> SearchPagination(int pageIndex, int pageSize, string query = "")
        {
            Paged<Job> pagedList = null;
            List<Job> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                "[dbo].[Jobs_Search_Pagination]", (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                    param.AddWithValue("@Query", query);
                },
                (reader, recordSetIndex) =>
                {
                    int startingIndex = 0;
                    Job job = MapSingleJob(reader, ref startingIndex);
                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }
                    if (list == null)
                    {
                        list = new List<Job>();
                    }
                    list.Add(job);
                });
            if (list != null)
            {
                pagedList = new Paged<Job>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;

        }
   
        public int Add(JobAddRequest model)
        {
            int jobId = 0;
            DataTable skillsTable = new DataTable();
            skillsTable.Columns.Add("Name", typeof(string));

            if (model.Skills != null && model.Skills.Count > 0)
            {
                foreach (string skill in model.Skills)
                {
                    DataRow skillRow = skillsTable.NewRow();
                    skillRow["Name"] = skill;
                    skillsTable.Rows.Add(skillRow);
                }
            }

            _data.ExecuteNonQuery("[dbo].[Jobs_Insert]", inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Title", model.Title);
                col.AddWithValue("@Description", model.Description);
                col.AddWithValue("@Summary", model.Summary);
                col.AddWithValue("@Pay", model.Pay);
                col.AddWithValue("@Slug", model.Slug);
                col.AddWithValue("@StatusId", model.StatusId);
                col.AddWithValue("@TechCompId", model.TechCompId);
                SqlParameter skillsParam = new SqlParameter("@BatchSkills", SqlDbType.Structured);
                skillsParam.Value = skillsTable;
                col.Add(skillsParam);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;
                col.Add(idOut);

            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {
                object oId = returnCollection["@Id"].Value;
                int.TryParse(oId.ToString(), out jobId);
            });

            return jobId;
        }
        public void Update(JobUpdateRequest model)
        {
            DataTable skillsTable = new DataTable();
            skillsTable.Columns.Add("Name", typeof(string));

            if (model.Skills != null && model.Skills.Count > 0)
            {
                foreach (string skill in model.Skills)
                {
                    DataRow skillRow = skillsTable.NewRow();
                    skillRow["Name"] = skill;
                    skillsTable.Rows.Add(skillRow);
                }
            }

            _data.ExecuteNonQuery("[dbo].[Jobs_UpdateById]", inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", model.Id);
                paramCollection.AddWithValue("@Title", model.Title);
                paramCollection.AddWithValue("@Description", model.Description);
                paramCollection.AddWithValue("@Summary", model.Summary);
                paramCollection.AddWithValue("@Pay", model.Pay);
                paramCollection.AddWithValue("@Slug", model.Slug);
                paramCollection.AddWithValue("@StatusId", model.StatusId);
                paramCollection.AddWithValue("@TechCompId", model.TechCompId);

                SqlParameter skillsParam = new SqlParameter("@BatchSkills", SqlDbType.Structured);
                skillsParam.Value = skillsTable;
                paramCollection.Add(skillsParam);
            });
        }
        #endregion

        #region Delete
        public void Delete(int Id)
        {
            _data.ExecuteNonQuery("[dbo].[Jobs_DeleteById]", inputParamMapper: delegate (SqlParameterCollection paramCollection)
            {
                paramCollection.AddWithValue("@Id", Id);
            },
                returnParameters: null);
        }
        #endregion

        private static Job MapSingleJob(IDataReader reader, ref int startingIndex)
        {
            Job aJob = new Job();

            aJob.Id = reader.GetSafeInt32(startingIndex++);
            aJob.Title = reader.GetSafeString(startingIndex++);
            aJob.Description = reader.GetSafeString(startingIndex++);
            aJob.Summary = reader.GetSafeString(startingIndex++);
            aJob.Pay = reader.GetSafeString(startingIndex++);
            aJob.Slug = reader.GetSafeString(startingIndex++);
            aJob.StatusId = reader.GetSafeString(startingIndex++);
            aJob.TechCompId = reader.GetSafeInt32(startingIndex++);
            aJob.TechCompany = new TechCompany();
            aJob.TechCompany.Id = reader.GetSafeInt32(startingIndex++);
            aJob.TechCompany.Name = reader.GetSafeString(startingIndex++);
            aJob.TechCompany.Headline = reader.GetSafeString(startingIndex++);
            aJob.TechCompany.Summary = reader.GetSafeString(startingIndex++);
            aJob.TechCompany.Profile = reader.GetSafeString(startingIndex++);
            aJob.TechCompany.Slug = reader.GetSafeString(startingIndex++);
            aJob.TechCompany.StatusId = reader.GetSafeString(startingIndex++);
            aJob.TechCompany.Images = reader.DeserializeObject<List<Image>>(startingIndex++);
            aJob.Skills = reader.DeserializeObject<List<Skill>>(startingIndex++);
            aJob.DateCreated = reader.GetSafeDateTime(startingIndex++);
            aJob.DateModified = reader.GetSafeDateTime(startingIndex++);

            return aJob;
        }




    }
}
