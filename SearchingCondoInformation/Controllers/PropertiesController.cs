﻿using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SearchingCondoInformation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertiesController : ControllerBase
    {
        /// <summary>
        /// 將 keyword 的型別改為 string?，並將 minPrice 和 maxPrice 的型別改為 decimal?，表示它們是可選的。
        /// 同時，為這些參數提供了預設值為 null，以防止它們未被提供。
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="minPrice"></param>
        /// <param name="maxPrice"></param>
        /// <returns></returns>
        [HttpGet]

        public IActionResult Get(string? keyword, decimal? minPrice, decimal? maxPrice)
        {
            //驗證 1.new Validator 2. validator.Validate(request)若為false回傳BadRequest
            var request = new PropertyRequest { Keyword = keyword, MinPrice = minPrice, MaxPrice = maxPrice };
            var validator = new PropertyRequestValidator();
            var validationResult = validator.Validate(request);
            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors);
            }


            var allProperties = new List<PropertyResponse>
            {
                new PropertyResponse("廣達科技大樓", 3000000000m, new Address { City = "台北市", District = "內湖區", Road = "基湖路", Number = "30號" }),
                new PropertyResponse("松智路101號辦公室", 2500000000m, new Address { City = "台北市", District = "信義區", Road = "松智路", Number = "101號" }),
                new PropertyResponse("富邦南港大樓", 3500000000m, new Address { City = "台北市", District = "南港區", Road = "經貿二路", Number = "188號" }),
                new PropertyResponse("微風台北車站", 5000000000m, new Address { City = "台北市", District = "中正區", Road = "忠孝西路一段", Number = "49號" }),
                new PropertyResponse("三創數位生活園區", 2000000000m, new Address { City = "台北市", District = "南港區", Road = "市民大道六段", Number = "133號" })
            }.AsEnumerable();

            var result = allProperties;// = 右邊會覆蓋左邊

            if (!string.IsNullOrEmpty(keyword))//不是Null或空字串//加上地址的篩選
            {
                result = result.Where(c => c.CondoName.Contains(keyword, StringComparison.OrdinalIgnoreCase) || c.Address.City.Contains(keyword, StringComparison.OrdinalIgnoreCase) || c.Address.District.Contains(keyword, StringComparison.OrdinalIgnoreCase) || c.Address.Road.Contains(keyword, StringComparison.OrdinalIgnoreCase) || c.Address.Number.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                //keyword忽略大寫//c代表的是PropertyResponse
            }
            if (minPrice >= 0)
            {
                result = result.Where(c => c.Price >= minPrice);
            }
            if (maxPrice >= 0)
            {
                result = result.Where(c => c.Price <= maxPrice);
            }

            return Ok(result);//因為型別是IActionResult所以這裡回傳也要IActionResult，statuscode的文字的爸爸就都是IActionResult
        }

    }

    public class Address
    {
        public string City { get; set; }
        public string District { get; set; }
        public string Road { get; set; }
        public string Number { get; set; }
    }

    public class PropertyResponse
    {

        public string CondoName { get; set; }
        public decimal Price { get; set; }
        public Address Address { get; }

        public PropertyResponse(string condoName, decimal price, Address address)
        {
            this.CondoName = condoName;
            this.Price = price;
            Address = address;
        }
    }

    public class PropertyRequest
    //string? keyword, decimal? minPrice, decimal? maxPrice
    {
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }


    public class PropertyRequestValidator : AbstractValidator<PropertyRequest>
    {
        public PropertyRequestValidator()
        {
            RuleFor(c => c.MinPrice).GreaterThanOrEqualTo(0).WithMessage("MinPrice must larger than 0.");
            RuleFor(c => c.MaxPrice).GreaterThanOrEqualTo(0).WithMessage("MaxPrice must larger than 0.")
                .GreaterThanOrEqualTo(c=>c.MinPrice).When(c=>c.MinPrice.HasValue).WithMessage("MaxPrice must greater than minPrice.");
        }
    }

}
