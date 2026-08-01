//using AutoMapper;
//using Microsoft.AspNetCore.Mvc;
//using Nadixa.Application.DTOS;
//using Nadixa.Application.Interfaces;
//using Nadixa.Application.Entities;
//using Nadixa.Application.Interfaces;

//namespace Nadixa.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class HomeController : ControllerBase
//    {

//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;
//        private readonly IPromotionService _promotionService;

//        public HomeController(IUnitOfWork unitOfWork, IMapper mapper, IPromotionService promotionService)
//        {
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//            _promotionService = promotionService;
//        }

//        // GET api/home?categoryId=3
//        [HttpGet]
//        public async Task<IActionResult> Index([FromQuery] int? categoryId)
//        {
//            // 1. المنتجات
//            var products = categoryId.HasValue
//                ? await _unitOfWork.Repository<Product>()
//                    .FindAsync(p => p.ProductCategoryId == categoryId.Value, p => p.ProductCategory, p => p.Images)
//                : await _unitOfWork.Repository<Product>()
//                    .GetAllAsync(p => p.ProductCategory, p => p.Images);

//            var productsList = products.ToList();

//            // 2. الكاتيجوريز
//            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();

//            // 3. الـ Best Sellers
//            var orderItems = await _unitOfWork.Repository<OrderItem>()
//                .FindAsync(oi => oi.Order.Status != OrderStatus.Cancelled, oi => oi.Product, oi => oi.Product.ProductCategory);

//            var bestSellers = orderItems
//                .GroupBy(oi => oi.ProductId)
//                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
//                .Take(8)
//                .Select(g => g.First().Product)
//                .ToList();

//            // 4. الـ Promotions
//            var activePromotions = await _promotionService.GetActivePromotionsAsync();

//            var productsForPromoCheck = productsList
//                .Concat(bestSellers)
//                .GroupBy(p => p.Id)
//                .Select(g => g.First());

//            var productPromotions = new Dictionary<int, ProductPromoInfo>();

//            foreach (var product in productsForPromoCheck)
//            {
//                var promo = activePromotions
//                    .Where(p =>
//                        !p.IsFirstPurchaseOnly &&
//                        (p.Scope == PromotionScope.AllProducts ||
//                         (p.Scope == PromotionScope.Category && p.ProductCategoryId == product.ProductCategoryId) ||
//                         (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == product.ProductSubCategoryId) ||
//                         (p.Scope == PromotionScope.SpecificProduct && p.ProductId == product.Id)))
//                    .OrderByDescending(p => p.Priority)
//                    .FirstOrDefault();

//                if (promo == null) continue;

//                productPromotions[product.Id] = new ProductPromoInfo
//                {
//                    BadgeText = promo.BadgeText,
//                    BadgeColorHex = promo.BadgeColorHex,
//                    DiscountedPrice = promo.Type == PromotionType.BuyXGetYFree
//                        ? null
//                        : _promotionService.CalculateDiscountedPrice(product.Price, promo)
//                };
//            }

//            // 5. تجميع كل حاجة في DTO واحد
//            var result = new HomeApiResult
//            {
//                Products = _mapper.Map<List<ProductToReturnDto>>(productsList),
//                Categories = _mapper.Map<List<CategoryToReturnDto>>(categories),
//                BestSellers = _mapper.Map<List<ProductToReturnDto>>(bestSellers),
//                ProductPromotions = productPromotions
//            };

//            return Ok(result);
//        }


//        // GET api/home/search?term=shirt
//        [HttpGet("search")]
//        public async Task<IActionResult> GlobalSearch([FromQuery] string term)
//        {
//            if (string.IsNullOrWhiteSpace(term))
//                return Ok(new List<ProductToReturnDto>());

//            var products = await _unitOfWork.Repository<Product>()
//                .FindAsync(p => p.Name.Contains(term) || p.Description.Contains(term),
//                    p => p.ProductCategory, p => p.Images);

//            var data = _mapper.Map<IEnumerable<ProductToReturnDto>>(products);
//            return Ok(data);
//        }
//    }
//}
