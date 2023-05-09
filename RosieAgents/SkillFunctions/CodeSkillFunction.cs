using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;

namespace RosieAgents.SkillFunctions
{
    public class CodeSkillFunction: SkillFunctionBase
    {
        
        public CodeSkillFunction(ILoggerFactory loggerFactory):base(loggerFactory)
        {
            
        }

        [Function(nameof(WL1))]
        public async Task<HttpResponseData> WL1([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            InitKernel();

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string requestedInput = queryDictionary["input"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(requestedInput))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var skill = GetSemanticsSkill("CodeSkills");


            var result = await Kernel.RunAsync(requestedInput, skill["WLUpgrade"]);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync(result.Result);


            return response;
        }
    }
}
