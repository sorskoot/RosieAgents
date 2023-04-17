using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;
using RosieAgents.CodeSkills;
using System.Collections.Specialized;
using System.Web;

namespace RosieAgents.SkillFunctions
{
    public class YouTubeFunctions:SkillFunctionBase
    {
        private readonly ILogger _logger;

        public YouTubeFunctions(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        [Function("YouTubeSummary")]
        public async Task<HttpResponseData> Summary([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedVideoId = queryDictionary["videoId"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedVideoId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> yt = Kernel.ImportSkill(new YouTubeTranscriptSkill());
            IDictionary<string, ISKFunction> summarizeSkill = GetSemanticsSkill("SummarizeSkill");
            IDictionary<string, ISKFunction> myText = Kernel.ImportSkill(new MyTextSkills());

            var variables = new ContextVariables();
            variables.Set("videoId", requestedVideoId);
            variables.Set("length", "4000");

            SKContext result = await Kernel.RunAsync(
                variables,
                yt["TranscribeVideo"],
                myText["Shorten"],
                summarizeSkill["Summarize"]
                );
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(result.Result);
            return response;
        }

        [Function("YouTubePositiveComment")]
        public async Task<HttpResponseData> PositiveComment([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedVideoId = queryDictionary["videoId"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedVideoId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> yt = Kernel.ImportSkill(new YouTubeTranscriptSkill());
            IDictionary<string, ISKFunction> socialMediaSkills = GetSemanticsSkill("SocialMediaSkill");
            IDictionary<string, ISKFunction> myText = Kernel.ImportSkill(new MyTextSkills());

            var variables = new ContextVariables();
            variables.Set("videoId", requestedVideoId);
            variables.Set("length", "2000");

            SKContext result = await Kernel.RunAsync(
                variables,
                yt["TranscribeVideo"],
                myText["Shorten"],
                socialMediaSkills["PositiveComment"]
            );
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(result.Result);
            return response;
        }
    }
}
