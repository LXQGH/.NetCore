using FakeXiecheng.API.ValidationAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FakeXiecheng.API.Dtos
{
    [TouristRouteTitleMustBeDifferentFromDescriptionAttribute]
    public class TouristRouteForCreationDto //:IValidatableObject
    {
        [Required(ErrorMessage ="title 不可为空")]
        [MaxLength(100)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1500)]
        public string Description { get; set; }
        //计算方式：原价*折扣
        public decimal Price { get; set; }
        //public decimal OriginalPrice { get; set; }
        //public double? DisciuntPresnt { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public string Features { get; set; }
        public string Fees { get; set; }
        public string Notes { get; set; }
        public double? Rating { get; set; }
        public string TraveIDays { get; set; }
        public string TripType { get; set; }
        public string DepartureCity { get; set; }
        public ICollection<TouristRoutePictureForCreationDto> TouristRoutePictures { get; set; }
         = new List<TouristRoutePictureForCreationDto>();

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if(Title==Description)
        //    {
        //        yield return new ValidationResult(
        //            "路线名称必须与路线描述不同",
        //            new[] { "TouristRouteForCreationDto" }
        //            );
        //    }
        //}
    }
}
