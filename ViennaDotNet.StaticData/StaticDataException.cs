using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViennaDotNet.StaticData;

public sealed class StaticDataException : Exception
{
    public StaticDataException()
    {
    }

    public StaticDataException(string? message) 
        : base(message)
    {
    }

    public StaticDataException(string? message, Exception inner) 
        : base(message, inner)
    {
    }
}
