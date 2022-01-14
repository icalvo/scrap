using System;

namespace Scrap.Jobs;

public class JobId
{
    private readonly Guid _guid;

    public JobId()
    {
        _guid = Guid.NewGuid();
    }

    public JobId(Guid guid)
    {
        _guid = guid;
    }

    public static implicit operator Guid(JobId d) => d._guid;
    public static implicit operator JobId(Guid b) => new(b);
        
    public override string ToString()
    {
        return _guid.ToString();
    }
}