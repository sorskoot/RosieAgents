using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Orchestration;
using RosieAgents.SkillFunctions;

namespace RosieAgents;

public class SkillFunctionBase
{
    protected readonly ILogger Logger;
    protected IKernel Kernel;

    public SkillFunctionBase(ILoggerFactory loggerFactory)
    {
        this.Logger = loggerFactory.CreateLogger<IdeasSkillFunction>();
    }

    protected void InitKernel(string? alternativeModel=null)
    {
        string apiKey = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_API_KEY)!;
        string model = alternativeModel??Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_DEPLOYMENT_NAME)!;
        string endPoint = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_ENDPOINT)!;

        this.Kernel = Microsoft.SemanticKernel.Kernel.Builder.WithLogger(ConsoleLogger.Log).Build();

        this.Kernel.Config.AddAzureOpenAITextCompletionService("gpt", model, endPoint, apiKey);
    }

    protected IDictionary<string, ISKFunction> GetSemanticsSkill(string semanticsSkill)
    {
        string skillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "skills");
        return this.Kernel.ImportSemanticSkillFromDirectory(skillsDirectory, semanticsSkill);
    }
}

internal static class ConsoleLogger
{
    internal static ILogger Log => LogFactory.CreateLogger<object>();

    private static ILoggerFactory LogFactory => s_loggerFactory.Value;

    private static readonly Lazy<ILoggerFactory> s_loggerFactory = new(LogBuilder);

    private static ILoggerFactory LogBuilder()
    {
        return LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning);

            // builder.AddFilter("Microsoft", LogLevel.Trace);
            // builder.AddFilter("Microsoft", LogLevel.Debug);
            // builder.AddFilter("Microsoft", LogLevel.Information);
            // builder.AddFilter("Microsoft", LogLevel.Warning);
            // builder.AddFilter("Microsoft", LogLevel.Error);

            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);

            builder.AddConsole();
        });
    }
}