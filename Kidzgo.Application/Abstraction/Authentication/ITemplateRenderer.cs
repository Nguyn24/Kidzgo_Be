namespace Kidzgo.Application.Abstraction.Authentication;

public interface ITemplateRenderer
{
    string Render(string template, IDictionary<string, string> values);
}