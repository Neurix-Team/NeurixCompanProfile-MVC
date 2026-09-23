using Neurix.BLL.Dtos.Cms;

namespace Neurix.Models;

public sealed class CmsSiteCopy
{
    private readonly IReadOnlyDictionary<string, CmsSiteSettingDto> _settings;

    public CmsSiteCopy(IEnumerable<CmsSiteSettingDto> settings)
    {
        _settings = settings.ToDictionary(setting => setting.Key, StringComparer.OrdinalIgnoreCase);
    }

    public string English(string key, string fallback) => Get(key, fallback, arabic: false);

    public string Arabic(string key, string fallback) => Get(key, fallback, arabic: true);

    private string Get(string key, string fallback, bool arabic)
    {
        if (!_settings.TryGetValue(key, out var setting)) return fallback;
        var value = arabic ? setting.ValueAr : setting.ValueEn;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
