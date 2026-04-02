namespace Kidzgo.Application.Abstraction.Services;

public interface IClientUrlProvider
{
    string GetFrontendUrl();
    string GetApiUrl();
}
