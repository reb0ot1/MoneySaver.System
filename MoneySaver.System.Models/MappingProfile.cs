using System;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace MoneySaver.System.Models;

public class MappingProfile : Profile
{
	public MappingProfile(Assembly assembly)
	{
		ApplyMappingFromAssembly(assembly);
	}

	private void ApplyMappingFromAssembly(Assembly assembly)
	{
		foreach (Type item in (from t in assembly.GetExportedTypes()
			where t.GetInterfaces().Any((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>))
			select t).ToList())
		{
			object obj = Activator.CreateInstance(item);
			(item.GetMethod("Mapping") ?? item.GetInterface("IMapFrom`1")?.GetMethod("Mapping"))?.Invoke(obj, new object[1] { this });
		}
	}
}
