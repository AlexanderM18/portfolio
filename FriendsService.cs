using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Domain.Friends;
using Services.Interfaces;
using Services;
using Web.Controllers;
using Web.Models.Responses;
using System.Collections.Generic;
using System;
using Models;
using Models.Requests.Friends;
using System.Linq;

namespace Web.Api.Controllers
{
    [Route("api/v3/friends")]
    [ApiController]
    public class FriendApiControllerV3 : BaseApiController
    {
        private IFriendService _service = null;
        private IAuthenticationService<int> _authService = null;
        public FriendApiControllerV3(IFriendService service, ILogger<FriendApiControllerV3> logger, IAuthenticationService<int> authService) : base(logger)
        {
            _service = service;
            _authService = authService;
        }

      
        [HttpGet]
        public ActionResult<ItemsResponse<FriendV3>> GetAll()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<FriendV3> list = _service.GetAllV3();

                if (list == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemsResponse<FriendV3> { Items = list };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<FriendV3>> GetById(int id)
        {
            int iCode = 200;
            BaseResponse response = null;

            try
            {
                FriendV3 friend = _service.GetV3(id);

                if (friend == null)
                {
                    iCode = 404;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<FriendV3> { Item = friend };
                }
            }
            catch (Exception ex)
            {
                iCode = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(iCode, response);

        }

        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<FriendV3>>> PaginationV3(int pageIndex, int PageSize)
        {
            ActionResult result = null;

            try
            {
                Paged<FriendV3> paged = _service.PaginationV3(pageIndex, PageSize);

                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<FriendV3>> response = new ItemResponse<Paged<FriendV3>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.ToString()));
            }

            return result;
        }

        [HttpGet("search")]
        public ActionResult<ItemResponse<Paged<FriendV3>>> SearchPaginationV3(int pageIndex, int PageSize, string query="")
        {
            ActionResult result = null;

            try
            {
                Paged<FriendV3> paged = _service.SearchPaginationV3(pageIndex, PageSize, query);

                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Record Not Found"));
                }
                else
                {
                    ItemResponse<Paged<FriendV3>> response = new ItemResponse<Paged<FriendV3>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.ToString()));
            }

            return result;
        }

        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(FriendAddRequestV3 model)
        {

           
            int userId = _authService.GetCurrentUserId();
         
            IUserAuthData user = _authService.GetCurrentUser();

            ObjectResult result = null;

            try
            {
                if (model.Skills != null && model.Skills.Any())
                {
                    List<string> skillsArray = model.Skills.Select(skill => skill.Name).ToList();
                    model.Skills = skillsArray.Select(skillName => new Skill { Name = skillName }).ToList();
                }

                int id = _service.AddV3(model, user.Id);
                ItemResponse<int> response = new ItemResponse<int> { Item = id };
                result = Created201(response);
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }
            return result;
        }

        [HttpPut("{id:int}")]
        public ActionResult<ItemResponse<int>> UpdateV3(int id, FriendUpdateRequestV3 model)
        {
            int userId = _authService.GetCurrentUserId();
         
            IUserAuthData friend = _authService.GetCurrentUser();

            int code = 200;
            BaseResponse response = null;

            try
            {
                model.Id = id;
                if (model.Skills != null && model.Skills.Any())
                {
                    List<string> skillsArray = model.Skills.Select(skill => skill.Name).ToList();
                    model.Skills = skillsArray.Select(skillName => new Skill { Name = skillName }).ToList();
                }

                _service.UpdateV3(model, friend.Id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse($"Failed to update Friend: {ex.Message}");
            }

            return StatusCode(code, response);
        }


        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> DeleteV3(int id)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                _service.DeleteV3(id);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }
    }
}
