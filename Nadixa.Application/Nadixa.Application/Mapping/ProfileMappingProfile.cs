using AutoMapper;
using Nadixa.Application.DTOS.Order;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Mapping
{
    public class ProfileMappingProfile : Profile
    {
        public ProfileMappingProfile()
        {
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.GrandTotal, o => o.MapFrom(s => s.TotalPrice));
        }
    }
}
