using AutoMapper;
using FakeXiecheng.API.Dtos;
using FakeXiecheng.API.Helper;
using FakeXiecheng.API.Models;
using FakeXiecheng.API.RouteResourceParamaters;
using FakeXiecheng.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FakeXiecheng.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristRoutesController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;

        public TouristRoutesController(
            ITouristRouteRepository touristRouteRepository,
            IMapper mapper,
             IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor
            )
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper=mapper;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
        }

        private string GenerateTouristRouteResourceURL(
            TouristRouteResourceParamaters paramaters,
            PaginationResourceParamaters paramaters2,
            ResourceUriType type
        )
        {
            return type switch
            {
                ResourceUriType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber - 1,
                        pageSize = paramaters2.PageSize
                    }),
                ResourceUriType.NextPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber + 1,
                        pageSize = paramaters2.PageSize
                    }),
                _ => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber,
                        pageSize = paramaters2.PageSize
                    })
            };
        }

        [HttpGet(Name = "GerTouristRoutes")]
        [Authorize(AuthenticationSchemes ="Bearer")]
        [Authorize]
        public async Task<IActionResult> GerTouristRoutes(
            [FromQuery] TouristRouteResourceParamaters paramaters,
            [FromQuery] PaginationResourceParamaters paramaters2
            //[FromQuery] string keyword,
            //string rating
            )
        {
            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesAsync(
                paramaters.Keyword, 
                paramaters.RatingOperator, 
                paramaters.RatingValue,
                paramaters2.PageNumber,
                paramaters2.PageSize
                );
            if(touristRoutesFromRepo == null ||touristRoutesFromRepo.Count()<=0)
            {
                return NotFound("没有旅游路线！");
            }
            var touristRouteDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);

            var previousPageLink = touristRoutesFromRepo.HasPrevious
               ? GenerateTouristRouteResourceURL(
                   paramaters, paramaters2, ResourceUriType.PreviousPage)
               : null;

            var nextPageLink = touristRoutesFromRepo.HasNext
                ? GenerateTouristRouteResourceURL(
                    paramaters, paramaters2, ResourceUriType.NextPage)
                : null;

            // x-pagination
            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = touristRoutesFromRepo.TotalCount,
                pageSize = touristRoutesFromRepo.PageSize,
                currentPage = touristRoutesFromRepo.CurrentPage,
                totalPages = touristRoutesFromRepo.TotalPages
            };

            Response.Headers.Add("x-pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            return Ok(touristRouteDto);
        }
        [HttpGet("{touristRouteId}",Name = "GetTouristRouteById")]
        public async Task<IActionResult> GetTouristRouteById(Guid touristRouteId)
        {
            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            if(touristRoutesFromRepo == null)
            {
                return NotFound($"旅游路线{touristRouteId}找不到");
            }
            //var touristRouteDto = new TouristRouteDto()
            //{ 
            //    Id=touristRoutesFromRepo.Id,
            //    Title = touristRoutesFromRepo.Title,
            //    Description = touristRoutesFromRepo.Description,
            //    Price = touristRoutesFromRepo.OriginalPrice*(decimal)(touristRoutesFromRepo.DiscountPresent ?? 1),
            //    CreateTime = touristRoutesFromRepo.CreateTime,
            //    UpdateTime = touristRoutesFromRepo.UpdateTime,
            //    Features = touristRoutesFromRepo.Features,
            //    Fees = touristRoutesFromRepo.Fees,
            //    Notes = touristRoutesFromRepo.Notes,
            //    Rating = touristRoutesFromRepo.Rating,
            //    TraveIDays = touristRoutesFromRepo.TraveIDays.ToString(),
            //    TripType = touristRoutesFromRepo.TripType.ToString(),
            //    DepartureCity = touristRoutesFromRepo.DepartureCity.ToString()
            //};
            var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRoutesFromRepo);
            return Ok(touristRouteDto);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreationDto touristRouteForCreationDto)
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreationDto);

            _touristRouteRepository.AddTouristRoute(touristRouteModel);
            await _touristRouteRepository.SaveAsync();
            var touristRouteToReture = _mapper.Map<TouristRouteDto>(touristRouteModel);
            return CreatedAtRoute(
                "GetTouristRouteById",
                new { touristRouteId = touristRouteToReture.Id },
                touristRouteToReture
            );
        }
        [HttpPut("{touristRouteId}")]
        public async Task<IActionResult> UpdateTouristRoute([FromRoute] Guid touristRouteId,
            [FromBody]TouristRouteForUpdateDto touristRouteForUpdateDto)
        {
            if(!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("旅游路线找不到！");
            }
            var touristRouteFromRepo =await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            // 1. 映射dto
            // 2. 更新dto
            // 3. 映射model
            _mapper.Map(touristRouteForUpdateDto, touristRouteFromRepo);
            await _touristRouteRepository.SaveAsync();
            return NoContent();
        }
        [HttpPatch("{touristRouteId}")]
        public async Task<IActionResult> PartiallyUpdateTouristRoute(
            [FromRoute] Guid touristRouteId,
            [FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("旅游路线找不到");
            }

            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristRouteFromRepo);
            patchDocument.ApplyTo(touristRouteToPatch,ModelState);
            if(!TryValidateModel(touristRouteToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(touristRouteToPatch, touristRouteFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{touristRouteId}")]
        public async Task<IActionResult> DeleteTouristRoute([FromRoute]Guid touristRouteId)
        {
            if(!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("旅游路线找不到");
            }
            var touristRoute = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            _touristRouteRepository.DeleteTouristRoute(touristRoute);
            await _touristRouteRepository.SaveAsync();
            return NoContent();
        }
        //批量删除
        [HttpDelete("({touristIDs})")]
        public async Task<IActionResult> DeleteByIDs(
           [ModelBinder(BinderType = typeof(ArrayModelBinder))][FromRoute] IEnumerable<Guid> touristIDs)
        {
            if (touristIDs == null)
            {
                return BadRequest();
            }

            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
            _touristRouteRepository.DeleteTouristRoutes(touristRoutesFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}
