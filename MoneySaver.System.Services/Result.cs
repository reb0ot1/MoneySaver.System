using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MoneySaver.System.Services;

public class Result
{
	private readonly List<string> errors;

	public bool Succeeded { get; }

	public List<string> Errors
	{
		get
		{
			if (!Succeeded)
			{
				return errors;
			}
			return new List<string>();
		}
	}

	public static Result Success => new Result(succeeded: true, new List<string>());

	internal Result(bool succeeded, List<string> errors)
	{
		Succeeded = succeeded;
		this.errors = errors;
	}

	public static Result Failure(string error)
	{
		return error;
	}

	public static Result Failure(IEnumerable<string> errors)
	{
		return new Result(succeeded: false, errors.ToList());
	}

	public static implicit operator Result(string error)
	{
		return Failure(new List<string> { error });
	}

	public static implicit operator Result(List<string> errors)
	{
		return Failure(errors.ToList());
	}

	public static implicit operator Result(bool success)
	{
		if (!success)
		{
			return Failure(new string[1] { "Unsuccessful operation." });
		}
		return Success;
	}

	public static implicit operator bool(Result result)
	{
		return result.Succeeded;
	}

	public static implicit operator ActionResult(Result result)
	{
		if (!result.Succeeded)
		{
			return new BadRequestObjectResult(result.Errors);
		}
		return new OkResult();
	}
}
public class Result<TData> : Result
{
	private readonly TData data;

	public TData Data
	{
		get
		{
			if (!base.Succeeded)
			{
				throw new InvalidOperationException($"{"Data"} is not available with a failed result. Use {base.Errors} instead.");
			}
			return data;
		}
	}

	private Result(bool succeeded, TData data, List<string> errors)
		: base(succeeded, errors)
	{
		this.data = data;
	}

	public static Result<TData> SuccessWith(TData data)
	{
		return new Result<TData>(succeeded: true, data, new List<string>());
	}

	public new static Result<TData> Failure(IEnumerable<string> errors)
	{
		return new Result<TData>(succeeded: false, default(TData), errors.ToList());
	}

	public static implicit operator Result<TData>(string error)
	{
		return Failure(new List<string> { error });
	}

	public static implicit operator Result<TData>(List<string> errors)
	{
		return Failure(errors);
	}

	public static implicit operator Result<TData>(TData data)
	{
		return SuccessWith(data);
	}

	public static implicit operator bool(Result<TData> result)
	{
		return result.Succeeded;
	}

	public static implicit operator ActionResult<TData>(Result<TData> result)
	{
		if (!result.Succeeded)
		{
			return new BadRequestObjectResult(result.Errors);
		}
		return result.Data;
	}
}
