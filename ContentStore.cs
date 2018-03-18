using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace core_playground
{
  public class Content
  {
    public Content(string markdown)
    {
      Markdown = markdown;

      if (markdown.Substring(0, 3).Equals("---"))
      {
        var lastIndex = markdown.IndexOf("---", 3);
        if (lastIndex != -1)
        {
          var frontmatterRaw = markdown.Substring(4, lastIndex - 4);
          FrontMatter = frontmatterRaw
              .Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
              .Select(p => p.Split(':', StringSplitOptions.RemoveEmptyEntries))
              .ToDictionary(p => p[0].Trim().ToLower(), p => p[1].Trim());
        }
      }
    }

    Dictionary<string, string> FrontMatter { get; } = new Dictionary<string, string>();
    public string Markdown { get; }

    public string Id => FrontMatter.GetValueOrDefault("id");
    public string Title => FrontMatter.GetValueOrDefault("title");
    public string Path => FrontMatter.GetValueOrDefault("path");
    public TwitterCard TwitterCard => string.IsNullOrWhiteSpace(FrontMatter.GetValueOrDefault("twitter-title"))
      ? null
      : new TwitterCard
      {
        Card = FrontMatter.GetValueOrDefault("twitter-card"),
        Title = FrontMatter.GetValueOrDefault("twitter-title"),
        Description = FrontMatter.GetValueOrDefault("twitter-description"),
      };
  }

  public class TwitterCard
  {
    public string Card { get; set; } = "summary_large_image";
    public string Title { get; set; }
    public string Description { get; set; }
  }

  public interface IContentStore
  {
    Task<IEnumerable<Content>> GetContentAsync();
  }

  public class ContentStore : IContentStore
  {
    readonly IMemoryCache _cache;
    readonly ILogger _logger;
    static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

    public ContentStore(IMemoryCache cache, ILogger<ContentStore> logger)
    {
      _cache = cache;
      _logger = logger;
    }

    public async Task<IEnumerable<Content>> GetContentAsync()
    {
      var cacheKey = "contentList";

      if (_cache.TryGetValue(cacheKey, out IEnumerable<Content> content))
      {
        _logger.LogDebug("Getting content from the cache");
        return content;
      }

      while (true)
      {
        int retries = 1;
        await semaphoreSlim.WaitAsync();
        try
        {
          return await TryGetContentAsync();
        }
        catch (Exception e)
        {
          if (retries > 3) throw;
          _logger.LogError(e, $"Something went wrong on try {retries}");
          await Task.Delay(TimeSpan.FromMilliseconds(300));
          retries++;
        }
        finally
        {
          semaphoreSlim.Release();
        }
      }

      async Task<IEnumerable<Content>> TryGetContentAsync()
      {
        IEnumerable<Content> contents = null;
        if (_cache.TryGetValue(cacheKey, out contents))
        {
          _logger.LogDebug("Second try: getting content from the cache");
          return contents;
        }

        contents = await ReadContents();
        _logger.LogWarning("Setting the cache!");
        _cache.Set(cacheKey, contents, new MemoryCacheEntryOptions
        {
          AbsoluteExpiration = DateTime.UtcNow.AddHours(1),
          Priority = CacheItemPriority.Normal,
        });
        return contents;
      }
    }

    static async Task<IEnumerable<Content>> ReadContents()
    {
      var files = System.IO.Directory.GetFiles("./Content", "*.md");
      var readers = files.Select(file => System.IO.File.ReadAllTextAsync(file));
      var result = await Task.WhenAll(readers);
      return result.Select(p => new Content(p));
    }
  }
}
