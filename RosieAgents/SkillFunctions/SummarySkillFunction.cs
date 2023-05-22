using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace RosieAgents.SkillFunctions
{
    public class SummarySkillFunction : SkillFunctionBase
    {
        public SummarySkillFunction(ILoggerFactory loggerFactory) : base(loggerFactory) { }

        [Function("SummarySkillFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedSkill = queryDictionary["skill"] ?? string.Empty;
            string requestedInput = queryDictionary["input"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedSkill) || string.IsNullOrWhiteSpace(requestedInput))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            IDictionary<string, ISKFunction> skill = GetSemanticsSkill("SummarizeSkill");

            if (!skill.ContainsKey(requestedSkill))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            
            var result = await Kernel.RunAsync(requestedInput, skill[requestedSkill]);


            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            await response.WriteStringAsync(result.Result);


            return response;
        }
    }
}
