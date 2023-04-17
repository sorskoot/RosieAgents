using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using YoutubeTranscriptApi;

namespace RosieAgents.CodeSkills
{
    public class YouTubeTranscriptSkill : IDisposable
    {
        private readonly ILogger _logger;

        public YouTubeTranscriptSkill(ILogger<YouTubeTranscriptSkill>? logger = null)
        {
            this._logger = logger ?? NullLogger<YouTubeTranscriptSkill>.Instance;
        }

        
        [SKFunction("Get a transcription from a youtube video")]
        [SKFunctionContextParameter(Name = "videoId", Description = "id of the video")]
        public string TranscribeVideo(SKContext context)
        {
            using var youTubeTranscriptApi = new YouTubeTranscriptApi();
            IEnumerable<TranscriptItem> transcriptItems = youTubeTranscriptApi.GetTranscript(context["videoId"]);

            string result = string.Join('\n',
                from transcriptItem in transcriptItems
                select transcriptItem.Text);

            return result;
        }

        /// <summary>
        /// Implementation of IDisposable.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Code that does the actual disposal of resources.
        /// </summary>
        /// <param name="disposing">Dispose of resources only if this is true.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
               
            }
        }
    }
}
