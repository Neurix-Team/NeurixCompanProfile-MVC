using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Neurix.Localization
{
    public interface ICmsLocalizer
    {
        /// <summary>
        /// Looks the key up in the current UI language, falling back to English and then
        /// to the key itself, so a half-translated dictionary never renders blank.
        /// </summary>
        string this[string key] { get; }

        /// <summary>Same as the indexer but with composite formatting.</summary>
        string Format(string key, params object[] args);

        /// <summary>True when the key exists in the current language (used by diagnostics).</summary>
        bool HasKey(string key);
    }

    /// <summary>
    /// JSON-backed localizer for the dashboard.
    ///
    /// Deliberately NOT .resx/IStringLocalizer: the public site already localizes through
    /// plain data-en/data-ar attributes with no resource compiler in the picture, and flat
    /// JSON keeps that low-ceremony feel while giving the dashboard the central dictionary
    /// it needs. Dictionaries live in Resources/Cms and are separate from the public site's
    /// strings, which stay inline in the public views.
    ///
    /// Registered as a singleton; files are read once per language and cached.
    /// </summary>
    public sealed class CmsLocalizer : ICmsLocalizer
    {
        private sealed class CachedDictionary
        {
            public DateTime LastWriteTimeUtc { get; init; }
            public IReadOnlyDictionary<string, string> Dictionary { get; init; } = new Dictionary<string, string>();
        }

        private readonly ConcurrentDictionary<string, CachedDictionary> _cache = new();
        private readonly string _resourceRoot;
        private readonly ILogger<CmsLocalizer> _logger;

        public CmsLocalizer(IWebHostEnvironment environment, ILogger<CmsLocalizer> logger)
        {
            _resourceRoot = Path.Combine(environment.ContentRootPath, "Resources", "Cms");
            _logger = logger;
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return string.Empty;
                }

                var current = CmsLanguage.Current.Code;

                if (Load(current).TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }

                if (current != CmsLanguage.DefaultCode &&
                    Load(CmsLanguage.DefaultCode).TryGetValue(key, out var fallback) &&
                    !string.IsNullOrEmpty(fallback))
                {
                    return fallback;
                }

                // Returning the key makes untranslated strings obvious in the UI rather
                // than silently empty.
                return key;
            }
        }

        public string Format(string key, params object[] args)
        {
            var template = this[key];
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        public bool HasKey(string key) => Load(CmsLanguage.Current.Code).ContainsKey(key);

        private IReadOnlyDictionary<string, string> Load(string code)
        {
            var path = Path.Combine(_resourceRoot, $"cms.{code}.json");
            if (!File.Exists(path))
            {
                return new Dictionary<string, string>();
            }

            try
            {
                var lastWrite = File.GetLastWriteTimeUtc(path);
                if (_cache.TryGetValue(code, out var cached) && cached.LastWriteTimeUtc == lastWrite)
                {
                    return cached.Dictionary;
                }

                var dict = ReadFile(path);
                _cache[code] = new CachedDictionary { LastWriteTimeUtc = lastWrite, Dictionary = dict };
                return dict;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking or reading CMS dictionary at {Path}.", path);
                return _cache.TryGetValue(code, out var fallback) ? fallback.Dictionary : new Dictionary<string, string>();
            }
        }

        private IReadOnlyDictionary<string, string> ReadFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return parsed ?? new Dictionary<string, string>();
            }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                // Fail soft: a malformed dictionary must not take the dashboard down.
                _logger.LogError(ex, "Could not read CMS dictionary {Path}.", path);
                return new Dictionary<string, string>();
            }
        }
    }
}
