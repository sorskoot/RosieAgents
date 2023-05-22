using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using RosieAgents.CodeSkills;

namespace RosieAgents.SkillFunctions
{
    public class WebsiteFunctions : SkillFunctionBase
    {

        public WebsiteFunctions(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }


        [Function("WebsiteSummary")]
        public async Task<HttpResponseData> Summary(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedUrl = queryDictionary["url"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedUrl))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> scrapeSkill = Kernel.ImportSkill(new WebsiteScapeSkill());
            IDictionary<string, ISKFunction> summarizeSkill = GetSemanticsSkill("SummarizeSkill");
            IDictionary<string, ISKFunction> myText = Kernel.ImportSkill(new MyTextSkills());

            var variables = new ContextVariables();
            variables.Set("url", requestedUrl);
            variables.Set("length", "2000");

            SKContext result = await Kernel.RunAsync(
                variables,
                scrapeSkill["Scrape"],
                myText["Shorten"],
                summarizeSkill["Summarize"]
            );
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(result.Result);
            return response;
        }

        [Function("WebsiteTopics")]
        public async Task<HttpResponseData> Topics(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedUrl = queryDictionary["url"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedUrl))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> scrapeSkill = Kernel.ImportSkill(new WebsiteScapeSkill());
            IDictionary<string, ISKFunction> summarizeSkill = GetSemanticsSkill("SummarizeSkill");
            IDictionary<string, ISKFunction> myText = Kernel.ImportSkill(new MyTextSkills());

            var variables = new ContextVariables();
            variables.Set("url", requestedUrl);
            variables.Set("length", "4000");

            SKContext result = await Kernel.RunAsync(
                variables,
                scrapeSkill["Scrape"],
                myText["Shorten"],
                summarizeSkill["Topics"]
            );
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            await response.WriteStringAsync(result.Result);
            return response;
        }

        [Function("WebsiteLinkedIn")]
        public async Task<HttpResponseData> LinkedIn([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedUrl = queryDictionary["url"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedUrl))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> scrapeSkill = Kernel.ImportSkill(new WebsiteScapeSkill());
            IDictionary<string, ISKFunction> socialMediaSkill = GetSemanticsSkill("SocialMediaSkill");
            IDictionary<string, ISKFunction> myText = Kernel.ImportSkill(new MyTextSkills());

            var variables = new ContextVariables();
            variables.Set("url", requestedUrl);
            variables.Set("length", "4000");

            SKContext result = await Kernel.RunAsync(
                variables,
                scrapeSkill["Scrape"],
                myText["Shorten"],
                socialMediaSkill["LinkedinPost"]
            );
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            var resultResult = 
                $"{result.Result}\n\n{requestedUrl}";
            await response.WriteStringAsync(resultResult);
            return response;
        }

        [Function("WebsiteComment")]
        public async Task<HttpResponseData> Comment([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedUrl = queryDictionary["url"] ?? string.Empty;
            string requestedOpinion = queryDictionary["opinion"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedUrl))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> scrapeSkill = Kernel.ImportSkill(new WebsiteScapeSkill());
            IDictionary<string, ISKFunction> socialMediaSkill = GetSemanticsSkill("SocialMediaSkill");
            IDictionary<string, ISKFunction> myText = Kernel.ImportSkill(new MyTextSkills());

            var variables = new ContextVariables();
            variables.Set("url", requestedUrl);
            variables.Set("length", "4000");
            variables.Set("opinion", requestedOpinion);

            SKContext result = await Kernel.RunAsync(
                variables,
                scrapeSkill["Scrape"],
                myText["Shorten"],
                socialMediaSkill["PositiveComment"]
            );
            var response = req.CreateResponse();
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            var resultResult = $"{result.Result}";
            await response.WriteStringAsync(resultResult);
            return response;
        }
    }
}