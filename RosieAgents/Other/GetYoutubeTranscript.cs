using System.Collections.Specialized;
using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using YoutubeTranscriptApi;

namespace RosieAgents.Other
{
    public class GetYoutubeTranscript
    {
        private readonly ILogger logger;

        public GetYoutubeTranscript(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<GetYoutubeTranscript>();
        }

        [Function("GetYoutubeTranscript")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {

            NameValueCollection queryDictionary = HttpUtility.ParseQueryString(req.Url.Query);

            if (!queryDictionary.HasKeys())
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string videoId = queryDictionary["video_id"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(videoId))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            string result;
            using (var youTubeTranscriptApi = new YouTubeTranscriptApi())
            {
                IEnumerable<TranscriptItem> transcriptItems = youTubeTranscriptApi.GetTranscript(videoId);

                result = string.Join('\n',
                    from transcriptItem in transcriptItems
                    select transcriptItem.Text);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString(result);

            return response;
        }
    }
}
