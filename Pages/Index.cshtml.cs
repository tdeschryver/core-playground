using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace core_playground.Pages
{
  public class IndexModel : PageModel
  {
    readonly IContentStore _contents;
    readonly ILogger _logger;

    public IndexModel(IContentStore contents,
        ILogger<IndexModel> logger)
    {
      _contents = contents;
      _logger = logger;
    }

    public List<Content> Contents { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
      Contents = (await _contents.GetContentAsync()).ToList();
      return Page();
    }
  }
}
