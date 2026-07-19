using AutoMapper;

namespace fredagsbar_backend.Database;

using fredagsbar_backend.Database.DTO;
using Models;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<UserCreateDto, User>();
		CreateMap<User, UserDto>();
		// 	.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
		// 	.ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count));
	}
}
