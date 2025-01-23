using AutoMapper;

namespace MoneySaver.System.Models;

public interface IMapFrom<T>
{
	void Mapping(Profile mapper)
	{
		mapper.CreateMap(typeof(T), GetType());
	}
}
