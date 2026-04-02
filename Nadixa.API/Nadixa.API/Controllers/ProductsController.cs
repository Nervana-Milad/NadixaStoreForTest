using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;

namespace Nadixa.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/products                 done
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductToReturnDto>>> GetProducts()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();

            // تحويل الداتا لـ DTO
            var data = _mapper.Map<IEnumerable<ProductToReturnDto>>(products);

            return Ok(data);
        }

        // GET: api/products/5                        done
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product == null) return NotFound();

            return Ok(_mapper.Map<ProductToReturnDto>(product));
        }

        // POST: api/products (للأدمن فقط لاحقاً)
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);

            // إضافة صورة افتراضية للتجربة
            product.Images.Add(new ProductImage { ImageUrl = productDto.MainImageUrl, IsMain = true });

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return Ok(product);
        }
    
        
    
    }
}
