using AutoMapper;
using Fredagsbar.Backend.Database.Dto;

namespace Fredagsbar.Backend.Database;

using Fredagsbar.Shared.DTO;
using Models;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		CreateMap<UserCreateDto, User>();
		CreateMap<User, UserDto>()
			.ForMember(dto => dto.BeerCasesOwed, usr => usr.MapFrom(src => -src.BeerCaseTransactions.Sum(bct => bct.Amount)));
		CreateMap<Accusation, AccusationDto>()
			.ForMember(dto => dto.YesVotes, acc => acc.MapFrom(src => src.Votes.Count(v => v.VotedFor)))
			.ForMember(dto => dto.NoVotes, acc => acc.MapFrom(src => src.Votes.Count(v => !v.VotedFor)));
		// 	.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
		// 	.ForMember(dest => dest.OrderCount, opt => opt.MapFrom(src => src.Orders.Count));
	}
}
