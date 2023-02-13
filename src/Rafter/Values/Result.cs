using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafter.Values;
public readonly struct Result<TResult, TError>
{
    public Result(TResult result)
    {
        Success = true;
        _result = result;
        _error = default;
    }

    public Result(TError error)
    {
        Success = false;
        _result = default;
        _error = error;
    }


    private readonly TResult? _result;
    private readonly TError? _error;

    public bool Success { get; }
    public TResult Value => Success
        ? _result!
        : throw new InvalidOperationException("Result in failed state has no value!");

    public TError Error => !Success 
        ? _error!
        : throw new InvalidOperationException("Result in success state has no error!");

    public static implicit operator Result<TResult, TError>(TResult result) => new(result);
    public static implicit operator Result<TResult, TError>(TError error) => new(error);
}
