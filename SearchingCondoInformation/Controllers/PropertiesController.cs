using FluentValidation;
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
        /// Refactor建議直接將 PropertyRequest 放在參數
        /// 參數放PropertyRequest request【代表request body】
        /// request body的情況下只能用[HttpPost]
        /// [HttpGet]只能FromQuery，參數要直接放在Get方法，不能在參數放PropertyRequest request【代表request body】
        /// [HttpPost]原則上FromBody，但也能轉[FromQuery]，如下面兩個方法
        /// <方法1.>[HttpPost]...Get(PropertyRequest request)...下面http檔案【參數FromBody】
        /// POST https://localhost:7080/api/Properties
        /// Content-Type: application/json
        /// {
        /// "keyword": "南港",
        /// "maxPrice": 5000000000,
        /// "minPrice": 3000000000
        /// }
        /// <方法2.>[HttpPost]...Get([FromQuery]PropertyRequest request)...下面http檔案【參數FromQuery】
        /// POST https://localhost:7080/api/Properties?keyword=大樓&minPrice=2000000000&maxPrice=5000000000
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]

        public IActionResult Get([FromQuery]PropertyRequest request)
        {
            //驗證 1.new Validator 2. validator.Validate(request)若為false回傳BadRequest
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

            if (!string.IsNullOrEmpty(request.Keyword))//不是Null或空字串//加上地址的篩選
            {
                result = result.Where(c => c.CondoName.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) || c.Address.City.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) || c.Address.District.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) || c.Address.Road.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) || c.Address.Number.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase));
                //keyword忽略大寫//c代表的是PropertyResponse
            }
            if (request.MinPrice >= 0)
            {
                result = result.Where(c => c.Price >= request.MinPrice);
            }
            if (request.MaxPrice>= 0)
            {
                result = result.Where(c => c.Price <= request.MaxPrice);
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
