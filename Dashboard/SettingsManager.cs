using System.Text.Json;

namespace PocketFence_AI.Dashboard;

/// <summary>
/// Manages loading and saving dashboard settings to JSON file
/// </summary>
public class SettingsManager
{
    private readonly string _settingsPath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DashboardSettings? _cachedSettings;

    public SettingsManager()
    {
        var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        Directory.CreateDirectory(dataDir);
        _settingsPath = Path.Combine(dataDir, "dashboard_settings.json");
    }

    /// <summary>
    /// Loads settings from JSON file or returns defaults
    /// </summary>
    public async Task<DashboardSettings> LoadSettingsAsync()
    {
        // Return cached if available
        if (_cachedSettings != null)
            return _cachedSettings;

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = await File.ReadAllTextAsync(_settingsPath).ConfigureAwait(false);
                _cachedSettings = JsonSerializer.Deserialize<DashboardSettings>(json) ?? new DashboardSettings();
            }
            else
            {
                // First time - create default settings and save directly (don't call SaveSettingsAsync to avoid deadlock)
                _cachedSettings = new DashboardSettings();
                
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_cachedSettings, options);
                await File.WriteAllTextAsync(_settingsPath, json).ConfigureAwait(false);
                Console.WriteLine($"[SettingsManager] Created default settings at {_settingsPath}");
            }

            return _cachedSettings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SettingsManager] Error loading settings: {ex.Message}");
            return new DashboardSettings(); // Return defaults on error
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Saves settings to JSON file
    /// </summary>
    public async Task SaveSettingsAsync(DashboardSettings settings)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsPath, json).ConfigureAwait(false);
            
            // Update cache
            _cachedSettings = settings;
            
            Console.WriteLine($"[SettingsManager] Settings saved to {_settingsPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SettingsManager] Error saving settings: {ex.Message}");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Clears the cached settings (forces reload on next access)
    /// </summary>
    public void ClearCache()
    {
        _cachedSettings = null;
    }
}
