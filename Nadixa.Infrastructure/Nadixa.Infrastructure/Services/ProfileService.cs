using AutoMapper;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProfileService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProfileDto> GetProfileAsync(string userId, string fullName, string email, string? phoneNumber)
        {
            var orders = await _unitOfWork.Repository<Order>()
                .FindAsync(o => o.UserId == userId);

            var orderedOrders = orders.OrderByDescending(o => o.CreatedAt).ToList();
            var orderDtos = _mapper.Map<List<OrderSummaryDto>>(orderedOrders);

            return new ProfileDto
            {
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Orders = orderDtos,
                TotalSpending = orderDtos.Sum(o => o.GrandTotal)   // 👈 الحساب هنا، مش في الـ View
            };
        }
    }
}
