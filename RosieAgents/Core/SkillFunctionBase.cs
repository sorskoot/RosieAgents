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

    protected void InitKernel()
    {
        string apiKey = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_API_KEY)!;
        string model = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_DEPLOYMENT_NAME)!;
        string endPoint = Environment.GetEnvironmentVariable(Constants.AZURE_OPENAI_ENDPOINT)!;

        this.Kernel = Microsoft.SemanticKernel.Kernel.Builder.Build();
        
        this.Kernel.Config.AddAzureOpenAITextCompletionService("davinci", model, endPoint, apiKey);
    }

    protected IDictionary<string, ISKFunction> GetSemanticsSkill(string semanticsSkill)
    {
        string skillsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "skills");
        return this.Kernel.ImportSemanticSkillFromDirectory(skillsDirectory, semanticsSkill);
    }
}