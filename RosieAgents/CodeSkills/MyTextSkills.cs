using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Orchestration;

namespace RosieAgents.CodeSkills
{
    internal class MyTextSkills
    {
        [SKFunction("Shorten the string to length")]
        public string Shorten(string text, SKContext context)
        {
            return text.Substring(0, Convert.ToInt32(context["length"]));
        }
    }
}
