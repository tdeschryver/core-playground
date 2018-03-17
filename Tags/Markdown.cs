using System.Threading.Tasks;
using Markdig;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace core_playground.Tags
{
  [HtmlTargetElement("markdown", TagStructure = TagStructure.NormalOrSelfClosing)]
  public class MarkdownTagHelper : TagHelper
  {
    public string Source { get; set; }
    public async override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
      if (string.IsNullOrWhiteSpace(Source))
      {
        var razorContent = await output.GetChildContentAsync();
        Source = razorContent.GetContent(NullHtmlEncoder.Default);
      }

      var pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
      var markdownHtmlContent = Markdown.ToHtml(Source, pipeline);

      output.Content.SetHtmlContent(markdownHtmlContent);
      output.TagName = null;
    }
  }
}
