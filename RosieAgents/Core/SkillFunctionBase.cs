using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using RosieAgents.SkillFunctions;

namespace RosieAgents;

public class SkillFunctionBase
{
    protected readonly ILogger Logger;
    protected IKernel Kernel;

    public SkillFunctionBase(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<IdeasSkillFunction>();
    }

    protected void InitKernel(string? alternativeModel = null)
    {
        string apiKey = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_API_KEY)!;
        string model = alternativeModel ?? Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_DEPLOYMENT_NAME)!;
        string endPoint = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_ENDPOINT)!;

        Kernel = Microsoft.SemanticKernel.Kernel.Builder
            .WithLogger(ConsoleLogger.Log)
            .Configure(c =>
            {
                c.AddAzureTextEmbeddingGenerationService("ada", "text-embedding-ada-002", endPoint, apiKey);
                c.AddAzureTextCompletionService("gpt", model, endPoint, apiKey);
            })
            .WithMemoryStorage(new VolatileMemoryStore())
            .Build();
    }

    protected IDictionary<string, ISKFunction> GetSemanticsSkill(string semanticsSkill)
    {
        string skillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "skills");
        return Kernel.ImportSemanticSkillFromDirectory(skillsDirectory, semanticsSkill);
    }
}

internal static class ConsoleLogger
{
    private static readonly Lazy<ILoggerFactory> s_loggerFactory = new(LogBuilder);
    internal static ILogger Log => LogFactory.CreateLogger<object>();

    private static ILoggerFactory LogFactory => s_loggerFactory.Value;

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