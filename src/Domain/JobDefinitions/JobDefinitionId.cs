using System;

namespace Scrap.JobDefinitions;

public class JobDefinitionId
{
    private readonly Guid _name;

    public JobDefinitionId()
    {
        _name = Guid.NewGuid();
    }

    public JobDefinitionId(Guid name)
    {
        _name = name;
    }

    public static implicit operator Guid(JobDefinitionId d) => d._name;
    public static implicit operator JobDefinitionId(Guid b) => new(b);
        
    public override string ToString()
    {
        return _name.ToString();
    }
}