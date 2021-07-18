using AutoMapper;
using FakeXiecheng.API.Dtos;
using FakeXiecheng.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FakeXiecheng.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TouristRoutePictureController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private IMapper _mapper;

        public TouristRoutePictureController(ITouristRouteRepository touristRouteRepository, IMapper mapper)
        {
            _touristRouteRepository = touristRouteRepository ??
            throw new ArgumentNullException(nameof(touristRouteRepository));
            _mapper=mapper ??
            throw new ArgumentNullException(nameof(mapper));
        }
       [HttpGet]
       public IActionResult GetPictureListForTouristRoute(Guid touristRouteId)
        {
            if(!_touristRouteRepository.TouristRouteExists(touristRouteId))
            {
                return NotFound("旅游路线不存在");
            }
            var picturesFromRepo = _touristRouteRepository.GetPicturesByTouristRouteId(touristRouteId);
            if (picturesFromRepo == null || picturesFromRepo.Count()<=0)
            {
                return NotFound("照片不存在");
            }
            return Ok(_mapper.Map<IEnumerable<TouristRoutePictureDto>>(picturesFromRepo));
        }
        [HttpGet("{pictureId}")]
        public IActionResult GetPicture(Guid touristRouteId,int pictureId)
        {
            if (!_touristRouteRepository.TouristRouteExists(touristRouteId))
            {
                return NotFound("旅游路线不存在");
            }
            var picturesFromRepo = _touristRouteRepository.GetPicture(pictureId);
            if(picturesFromRepo==null)
            {
                return NotFound("相片不存在！");
            }
            return Ok(_mapper.Map<TouristRoutePictureDto>(picturesFromRepo));
        }
    }
}
