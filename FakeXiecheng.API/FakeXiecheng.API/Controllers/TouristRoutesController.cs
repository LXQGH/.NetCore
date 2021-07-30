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
        
        public TouristRoutesController(ITouristRouteRepository touristRouteRepository,IMapper mapper)
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper=mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GerTouristRoutes(
            [FromQuery] TouristRouteResourceParamaters paramaters
            //[FromQuery] string keyword,
            //string rating
            )
        {
            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesAsync(paramaters.Keyword, paramaters.RatingOperator, paramaters.RatingValue);
            if(touristRoutesFromRepo == null ||touristRoutesFromRepo.Count()<=0)
            {
                return NotFound("没有旅游路线！");
            }
            var touristRouteDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
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
            //    Price = touristRoutesFromRepo.OriginalPrice*(decimal)(touristRoutesFromRepo.DisciuntPresnt ?? 1),
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
        [Authorize]
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
