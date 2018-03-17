using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace core_playground.Pages
{
  public class ContentModel : PageModel
  {
    readonly IContentStore _contents;

    public ContentModel(IContentStore contents)
    {
      _contents = contents;
    }

    public Content Content { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
      var contents = await _contents.GetContentAsync();
      Content = contents.SingleOrDefault(p => p.Id == id);
      if (Content == null)
      {
        return Redirect("/");
      }

      return Page();
    }
  }
}
