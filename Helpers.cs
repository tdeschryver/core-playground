using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace core_playground
{
  public static class Helper
  {
    public static string AsUrl(this Content content) =>
      $"{content.Id}/{(string.IsNullOrWhiteSpace(content.Path) ? content.Title : content.Path)}".Trim().ToLower();
  }
}
