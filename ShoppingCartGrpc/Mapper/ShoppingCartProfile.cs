using AutoMapper;
using ShoppingCartGrpc.Protos;

namespace ShoppingCartGrpc.Mapper
{
    public class ShoppingCartProfile : Profile
    {
        public ShoppingCartProfile()
        {
            CreateMap<Models.ShoppingCart, ShoppingCartModel>().ReverseMap();
            //    .ForMember(dest => dest.Username,
            //        opt => opt.MapFrom(src => src.UserName));
            //
            //CreateMap<ShoppingCartModel, Models.ShoppingCart>()
            //    .ForMember(dest => dest.UserName,
            //        opt => opt.MapFrom(src => src.Username));

            CreateMap<Models.ShoppingCartItem, ShoppingCartItemModel>().ReverseMap();
        }
    }
}