using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using System.Collections.Specialized;
using System.Web;
using Microsoft.SemanticKernel.Skills.Core;

namespace RosieAgents.SkillFunctions;

public class TestFunction : SkillFunctionBase
{
    /// <summary>
    /// The max tokens to process in a single semantic function call.
    /// </summary>
    private const int MaxTokens = 1024;


    public TestFunction(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    //[Function(nameof(MRTK))]
    //public async Task<HttpResponseData> MRTK([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    //{
    //    const string memoryName = "MRTK";

    //    InitKernel();

    //    NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

    //    string requestedInput = queryDictionary["input"] ?? string.Empty;

    //    var ghSkill = Kernel.ImportSkill(new GitHubSkill(Kernel, new WebFileDownloadSkill()));
        
    //    SKContext result = await Kernel.RunAsync(
    //        requestedInput,
    //        ghSkill["Scrape"]);

    //    return await CreateResponse(req, result.Result);
    //}

    [Function(nameof(Test))]
    public async Task<HttpResponseData> Test([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        const string memoryName = "MemoryName";

        InitKernel();

        NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);
        string requestedInput = queryDictionary["input"] ?? string.Empty;

        List<string> lines;
        List<string> paragraphs;

        lines = TextChunker.SplitMarkDownLines(Data.Text, MaxTokens);
        paragraphs = TextChunker.SplitMarkdownParagraphs(lines, MaxTokens);

        for (int i = 0; i < paragraphs.Count; i++)
        {
            
            await Kernel.Memory.SaveInformationAsync(
                memoryName,
                text: $"{paragraphs[i]} File:repositoryUri/blob/repositoryBranch/fileUri",
                id: $"fileUri_{i}");
        }

        Kernel.ImportSkill(new TextMemorySkill());

        var memories =
            Kernel.Memory.SearchAsync(memoryName, requestedInput);
        int q = 0;
        string skPrompt = "ANSWER THE QUESTION '{{$INPUT}}' BUT ONLY USE THE FACTS BELOW:\n";

        await foreach (MemoryQueryResult memory in memories)
        {
            skPrompt += $"FACT {++q}:\n";
            skPrompt += $"{memory.Metadata.Text}\n\n";
        }

        var chatFunction = Kernel.CreateSemanticFunction(skPrompt, maxTokens: 200, temperature: 0.8);

        var result = await Kernel.RunAsync(requestedInput, chatFunction);


        return await CreateResponse(req, result.Result);
    }

    private static async Task<HttpResponseData> CreateResponse(HttpRequestData req, string result)
    {
        var response = req.CreateResponse();
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        await response.WriteStringAsync(result);
        return response;
    }
}

public class Data
{
    public const string Text =
        @"# What are Skills?

![image](https://user-images.githubusercontent.com/371009/221739773-cf43522f-c1e4-42f2-b73d-5ba84e21febb.png)

A skill refers to a domain of expertise made available to the kernel as a single
function, or as a group of functions related to the skill. The design of SK skills
has prioritized maximum flexibility for the developer to be both lightweight and
extensible.

# What is a Function?

![image](https://user-images.githubusercontent.com/371009/221742673-3ee8abb8-fe10-4669-93e5-5096d3d09580.png)

A function is the basic building block for a skill. A function can be expressed
as either:

1. an LLM AI prompt — also called a ""semantic"" function
2. native computer code -- also called a ""native"" function

When using native computer code, it's also possible to invoke an LLM AI prompt —
which means that there can be functions that are hybrid LLM AI × native code as well.

Functions can be connected end-to-end, or ""chained together,"" to create more powerful
capabilities.When they are represented as pure LLM AI prompts in semantic functions,
the word ""function"" and ""prompt"" can be used interchangeably.

# What is the relationship between semantic functions and skills?

A skill is the container in which functions live.You can think of a semantic skill
as a directory of folders that contain multiple directories of semantic functions
or a single directory as well.

```
SkillName (directory name)
│
└─── Function1Name (directory name)
│   
└─── Function2Name (directory name)
```

Each function directory will have an skprompt.txt file and a config.json file. There's
much more to learn about semantic functions in Building Semantic Functions if you
wish to go deeper.

# What is the relationship between native functions and skills?

Native functions are loosely inspired by Azure Functions and exist as individual
native skill files as in MyNativeSkill.cs below:

```
MyAppSource
│
└───MySkillsDirectory
    │
    └─── MySemanticSkill (a directory)
    |   │
    |   └─── MyFirstSemanticFunction (a directory)
    |   └─── MyOtherSemanticFunction (a directory)
    │
    └─── MyNativeSkill.cs (a file)
    └─── MyOtherNativeSkill.cs (a file)
```

Each file will contain multiple native functions that are associated with a skill.

# Where to find skills in the GitHub repo

Skills are stored in one of three places:

1. Core Skills: these are skills available at any time to the kernel that embody
   a few standard capabilities like working with time, text, files, http requests,
   and the [Planner] (PLANNER.md). The core skills can be found
   [here] (../dotnet/src/SemanticKernel/CoreSkills).

2. Semantic Skills: these skills are managed by you in a directory of your choice.

3. Native Skills: these skills are also managed by you in a directory of your choice.

For more examples of skills, and the ones that we use in our sample apps, look inside
the[/ samples / skills] (../samples/skills) folder.";
}