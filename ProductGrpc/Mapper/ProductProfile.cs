using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using ProductGrpc.Protos;

namespace ProductGrpc.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Models.Product, ProductModel>()
                .ForMember(dest => dest.CreatedTime, 
                    opt => opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedTime)))
                .ForMember(dest => dest.ProductId,
                    opt => opt.MapFrom(src => src.Id));

            CreateMap<ProductModel, Models.Product>()
                .ForMember(dest => dest.CreatedTime,
                    opt => opt.MapFrom(src => src.CreatedTime.ToDateTime()))
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.ProductId));
        }
    }
}