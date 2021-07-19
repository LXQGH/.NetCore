using AutoMapper;
using FakeXiecheng.API.Dtos;
using FakeXiecheng.API.RouteResourceParamaters;
using FakeXiecheng.API.Services;
using Microsoft.AspNetCore.Http;
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
        public IActionResult GerTouristRoutes(
            [FromQuery] TouristRouteResourceParamaters paramaters
            //[FromQuery] string keyword,
            //string rating
            )
        {
            var touristRoutesFromRepo = _touristRouteRepository.GetTouristRoutes(paramaters.Keyword, paramaters.RatingOperator, paramaters.RatingValue);
            if(touristRoutesFromRepo == null ||touristRoutesFromRepo.Count()<=0)
            {
                return NotFound("没有旅游路线！");
            }
            var touristRouteDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
            return Ok(touristRouteDto);
        }
        [HttpGet("{touristRouteId}")]
        public IActionResult GetTouristRouteById(Guid touristRouteId)
        {
            var touristRoutesFromRepo = _touristRouteRepository.GetTouristRoute(touristRouteId);
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

    }
}
