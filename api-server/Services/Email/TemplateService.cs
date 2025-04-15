using HandlebarsDotNet;
using Zarichney.Services.FileSystem;

namespace Zarichney.Services.Email;

public interface ITemplateService
{
  Task<string> ApplyTemplate(string templateName, Dictionary<string, object> templateData, string title);
}

public class TemplateService : ITemplateService
{
  private readonly string _templateDirectory;
  private readonly Dictionary<string, HandlebarsTemplate<object, object>> _compiledTemplates;
  private readonly IFileService _fileService;

  public TemplateService(EmailConfig config, IFileService fileService)
  {
    _fileService = fileService;
    _templateDirectory = config.TemplateDirectory;
    _compiledTemplates = new Dictionary<string, HandlebarsTemplate<object, object>>();
    CompileBaseTemplate();
  }

  private void CompileBaseTemplate()
  {
    var baseTemplatePath = Path.Combine(_templateDirectory, "base.html");
    var baseTemplateContent = _fileService.GetFile(baseTemplatePath);
    _compiledTemplates["base"] = Handlebars.Compile(baseTemplateContent);
  }

  public async Task<string> ApplyTemplate(string templateName, Dictionary<string, object> templateData, string title)
  {
    if (!_compiledTemplates.TryGetValue(templateName, out var template))
    {
      var templatePath = Path.Combine(_templateDirectory, $"{templateName}.html");
      var templateContent = await _fileService.GetFileAsync(templatePath);
      template = Handlebars.Compile(templateContent);
      _compiledTemplates[templateName] = template;
    }

    var content = template(templateData);
    templateData["content"] = content;
    templateData["company_name"] = "Zarichney Development";
    templateData["current_year"] = DateTime.Now.Year;
    templateData["title"] = title;

    return _compiledTemplates["base"](templateData);
  }

  public static Dictionary<string, object> GetErrorTemplateData(Exception ex)
  {
    return new Dictionary<string, object>
    {
      { "timestamp", DateTime.UtcNow.ToString("O") },
      { "errorType", ex.GetType().Name },
      { "errorMessage", ex.Message },
      { "stackTrace", ex.StackTrace ?? "No stack trace available" },
      {
        "additionalContext", new Dictionary<string, string>
        {
          { "MachineName", Environment.MachineName },
          { "OsVersion", Environment.OSVersion.ToString() }
        }
      }
    };
  }
}