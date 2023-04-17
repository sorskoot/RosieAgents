using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace RosieAgents.CodeSkills
{
    internal class WebsiteScapeSkill
    {
        [SKFunction("Scape the website")]
        [SKFunctionContextParameter(Name="url", Description="URL of the website to scape")]
        public string Scrape(SKContext context)
        {
            SmartReader.Reader sr = new SmartReader.Reader(context["url"]);
            SmartReader.Article article = sr.GetArticle();
            //var images = article.GetImagesAsync();

            if (article.IsReadable)
            {
                return article.Content;
            }
            return "";
        }
    }
}
