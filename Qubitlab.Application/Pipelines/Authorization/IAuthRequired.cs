namespace Qubitlab.Application.Pipelines.Authorization;

public interface IAuthRequired
{
    public string[] Roles { get; }
}