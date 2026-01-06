using System.Text.Json;

namespace PocketFence_AI;

/// <summary>
/// PocketFence AI - Lightweight Local Content Filter
/// Optimized for local inference like GPT4All
/// </summary>
public class Program
{
    private static readonly Dashboard.SettingsManager _settingsManager = new Dashboard.SettingsManager();
    private static readonly SimpleAI _ai = new SimpleAI(_settingsManager);
    private static readonly Dashboard.BlockedContentStore _blockedStore = Dashboard.DashboardService.BlockedContent;
    private static readonly ContentFilter _filter = new ContentFilter(_blockedStore, _settingsManager);
    private static readonly RealTimeFilterService _realTimeFilter = new RealTimeFilterService(_filter, _ai, _blockedStore);
    
    public static async Task Main(string[] args)
    {
        // Check for dashboard mode
        if (args.Length > 0 && args[0].ToLower() == "dashboard")
        {
            Console.WriteLine("🛡️  Starting PocketFence Dashboard...");
            PocketFence_AI.Dashboard.DashboardService.StartDashboard(args);
            return;
        }

        Console.WriteLine("🤖 PocketFence AI - Local Content Filter v1.0");
        Console.WriteLine("Optimized for local inference without external dependencies");
        Console.WriteLine();
        
        try
        {
            // Initialize AI model
            Console.WriteLine("Initializing AI model...");
            await _ai.InitializeAsync();
            
            // Load content filters
            Console.WriteLine("Loading content filters...");
            await _filter.LoadFiltersAsync();
            
            Console.WriteLine("✅ Ready! Type 'help' for commands or 'exit' to quit.");
            Console.WriteLine();
            
            // Start interactive CLI
            await RunInteractiveModeAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return;
        }
    }
    
    private static async Task RunInteractiveModeAsync()
    {
        while (true)
        {
            Console.Write("pocketfence> ");
            var input = Console.ReadLine()?.Trim() ?? "";
            
            switch (input.ToLower())
            {
                case "help":
                    ShowHelp();
                    break;
                case "exit":
                case "quit":
                    Console.WriteLine("👋 Goodbye!");
                    return;
                case var cmd when cmd.StartsWith("check "):
                    await CheckContentAsync(input.Substring(6));
                    break;
                case var cmd when cmd.StartsWith("analyze "):
                    await AnalyzeContentAsync(input.Substring(8));
                    break;
                case "stats":
                    ShowStats();
                    break;
                case "monitor start":
                    _realTimeFilter.StartMonitoring();
                    break;
                case "monitor stop":
                    _realTimeFilter.StopMonitoring();
                    break;
                case "monitor status":
                    Console.WriteLine($"Real-time monitoring: {(_realTimeFilter.IsMonitoring ? "🟢 ACTIVE" : "🔴 STOPPED")}");
                    break;
                case "dashboard":
                    Console.WriteLine("🛡️  Starting dashboard...");
                    Console.WriteLine("Run: dotnet run dashboard");
                    Console.WriteLine("Or restart with 'dashboard' argument");
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    Console.WriteLine("❓ Unknown command. Type 'help' for available commands.");
                    break;
            }
        }
    }
    
    private static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  check <url>      - Check if URL should be blocked");
        Console.WriteLine("  analyze <text>   - Analyze text content for safety");
        Console.WriteLine("  stats            - Show filtering statistics");
        Console.WriteLine("  monitor start    - Start real-time traffic monitoring");
        Console.WriteLine("  monitor stop     - Stop real-time traffic monitoring");
        Console.WriteLine("  monitor status   - Check monitoring status");
        Console.WriteLine("  dashboard        - Instructions to start web dashboard");
        Console.WriteLine("  clear            - Clear screen");
        Console.WriteLine("  help             - Show this help");
        Console.WriteLine("  exit             - Exit program");
    }
    
    private static async Task CheckContentAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("❌ Please provide a URL to check");
            return;
        }
        
        var result = await _filter.CheckUrlAsync(url);
        var aiScore = await _ai.AnalyzeThreatLevelAsync(url);
        
        Console.WriteLine($"🔍 Analysis for: {url}");
        Console.WriteLine($"   Filter Result: {(result.IsBlocked ? "❌ BLOCKED" : "✅ ALLOWED")}");
        Console.WriteLine($"   AI Threat Score: {aiScore:F2}/1.0");
        Console.WriteLine($"   Reason: {result.Reason}");
        
        if (result.IsBlocked || aiScore > 0.7)
        {
            Console.WriteLine($"   ⚠️  Recommendation: {(result.IsBlocked ? "Block" : "Monitor")}");
            Console.WriteLine($"   📊 Saved to dashboard (view at http://localhost:5000/Blocked)");
        }
    }
    
    private static async Task AnalyzeContentAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("❌ Please provide content to analyze");
            return;
        }
        
        var analysis = await _ai.AnalyzeContentAsync(content);
        
        Console.WriteLine("🧠 AI Content Analysis:");
        Console.WriteLine($"   Safety Score: {analysis.SafetyScore:F2}/1.0");
        Console.WriteLine($"   Category: {analysis.Category}");
        Console.WriteLine($"   Confidence: {analysis.Confidence:F2}");
        Console.WriteLine($"   Recommendation: {analysis.Recommendation}");
        
        if (analysis.Flags.Any())
        {
            Console.WriteLine($"   ⚠️  Flags: {string.Join(", ", analysis.Flags)}");
        }
    }
    
    private static void ShowStats()
    {
        var stats = _filter.GetStatistics();
        
        Console.WriteLine("📊 Filtering Statistics:");
        Console.WriteLine($"   Total Requests: {stats.TotalRequests}");
        Console.WriteLine($"   Blocked: {stats.BlockedRequests} ({stats.BlockRate:P1})");
        Console.WriteLine($"   Allowed: {stats.AllowedRequests} ({stats.AllowRate:P1})");
        Console.WriteLine($"   AI Processed: {_ai.GetProcessedCount()}");
    }
}

// Lightweight AI engine optimized for local inference
public class SimpleAI
{
    private readonly Dictionary<string, double> _threatKeywords;
    private readonly Dictionary<string, double> _safePatterns;
    private int _processedCount = 0;
    private readonly Dashboard.SettingsManager? _settingsManager;
    
    public SimpleAI(Dashboard.SettingsManager? settingsManager = null)
    {
        _settingsManager = settingsManager;
        
        _threatKeywords = new Dictionary<string, double>
        {
            // High-risk keywords
            { "malware", 0.9 }, { "virus", 0.9 }, { "phishing", 0.95 },
            { "adult", 0.8 }, { "gambling", 0.7 }, { "violence", 0.85 },
            { "drugs", 0.8 }, { "weapons", 0.85 }, { "hate", 0.9 },
            
            // Medium-risk keywords
            { "download", 0.4 }, { "free", 0.3 }, { "click", 0.3 },
            { "urgent", 0.5 }, { "limited", 0.4 }, { "offer", 0.3 }
        };
        
        _safePatterns = new Dictionary<string, double>
        {
            { "education", -0.3 }, { "learning", -0.3 }, { "school", -0.3 },
            { "tutorial", -0.2 }, { "help", -0.2 }, { "support", -0.2 },
            { "documentation", -0.3 }, { "official", -0.3 }
        };
    }
    
    public Task InitializeAsync()
    {
        // Simulate model loading - in real implementation this would load ML models
        Thread.Sleep(100);
        return Task.CompletedTask;
    }
    
    public Task<double> AnalyzeThreatLevelAsync(string content)
    {
        _processedCount++;
        
        if (string.IsNullOrWhiteSpace(content))
            return Task.FromResult(0.0);
            
        var contentLower = content.ToLowerInvariant();
        double score = 0.0;
        int matches = 0;
        
        // Check threat keywords
        foreach (var keyword in _threatKeywords)
        {
            if (contentLower.Contains(keyword.Key))
            {
                score += keyword.Value;
                matches++;
            }
        }
        
        // Check safe patterns
        foreach (var pattern in _safePatterns)
        {
            if (contentLower.Contains(pattern.Key))
            {
                score += pattern.Value;
                matches++;
            }
        }
        
        // Normalize score
        if (matches > 0)
            score = Math.Max(0.0, Math.Min(1.0, score / matches));
            
        return Task.FromResult(score);
    }
    
    public async Task<ContentAnalysis> AnalyzeContentAsync(string content)
    {
        // Get filter level for threshold adjustment
        string filterLevel = "moderate";
        if (_settingsManager != null)
        {
            var settings = await _settingsManager.LoadSettingsAsync();
            filterLevel = settings.FilterLevel;
        }
        
        var threatLevel = await AnalyzeThreatLevelAsync(content);
        var flags = new List<string>();
        var category = "General";
        
        // Determine category and flags based on content
        var contentLower = content.ToLowerInvariant();
        
        if (contentLower.Contains("adult") || contentLower.Contains("explicit"))
        {
            category = "Adult Content";
            flags.Add("NSFW");
        }
        else if (contentLower.Contains("violence") || contentLower.Contains("weapon"))
        {
            category = "Violence";
            flags.Add("Violent Content");
        }
        else if (contentLower.Contains("malware") || contentLower.Contains("virus"))
        {
            category = "Security Threat";
            flags.Add("Malicious");
        }
        
        // Adjust thresholds based on filter level
        double blockThreshold = filterLevel switch
        {
            "strict" => 0.5,   // More sensitive - block at lower threat scores
            "relaxed" => 0.85, // Less sensitive - only block high threats
            _ => 0.7           // moderate - balanced
        };
        
        double monitorThreshold = filterLevel switch
        {
            "strict" => 0.3,
            "relaxed" => 0.6,
            _ => 0.4
        };
        
        var recommendation = threatLevel > blockThreshold ? "BLOCK" : 
                           threatLevel > monitorThreshold ? "MONITOR" : "ALLOW";
        
        return new ContentAnalysis
        {
            SafetyScore = 1.0 - threatLevel,
            Category = category,
            Confidence = Math.Min(0.95, threatLevel + 0.3),
            Recommendation = recommendation,
            Flags = flags
        };
    }
    
    public int GetProcessedCount() => _processedCount;

    /// <summary>
    /// Generate age-appropriate setup recommendations based on device, age, and parent concerns
    /// </summary>
    public SetupRecommendationData GenerateSetupRecommendations(string deviceType, int childAge, List<string> concerns)
    {
        var data = new SetupRecommendationData
        {
            DeviceType = deviceType,
            ChildAge = childAge,
            Concerns = concerns
        };

        // Determine age bracket
        string ageBracket = childAge switch
        {
            <= 5 => "toddler",
            <= 12 => "child",
            _ => "teen"
        };

        // Generate summary based on age and concerns
        data.Summary = GenerateSummary(ageBracket, childAge, deviceType, concerns);

        // Generate recommendations based on device type and age
        data.Recommendations = GenerateRecommendations(deviceType, ageBracket, concerns);

        // Generate app lists
        data.AppsToBlock = GetAppsToBlock(ageBracket, deviceType, concerns);
        data.AppsToAllow = GetAppsToAllow(ageBracket);

        return data;
    }

    private string GenerateSummary(string ageBracket, int age, string deviceType, List<string> concerns)
    {
        var concernText = concerns.Any() ? $" with focus on {string.Join(", ", concerns.Take(2))}" : "";
        
        return ageBracket switch
        {
            "toddler" => $"For a {age}-year-old, we recommend maximum protection{concernText}. " +
                        "Young children need strict content filtering, no social media access, and heavy supervision. " +
                        "Focus on educational content only.",
            
            "child" => $"For a {age}-year-old, we recommend balanced protection{concernText}. " +
                      "School-age children benefit from time limits (2-3 hours), restricted social media, " +
                      "and content filtering while allowing educational apps.",
            
            "teen" => $"For a {age}-year-old, we recommend privacy-respecting safety{concernText}. " +
                     "Teens need privacy but still benefit from content filters, time management (3-4 hours), " +
                     "and communication monitoring. Focus on building trust.",
            
            _ => "Please configure parental controls based on your child's maturity level."
        };
    }

    private List<Recommendation> GenerateRecommendations(string deviceType, string ageBracket, List<string> concerns)
    {
        var recommendations = new List<Recommendation>();
        int idCounter = 1;

        // Core recommendations based on age bracket
        if (ageBracket == "toddler")
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Block All Social Media & Communication Apps",
                Description = "Prevent access to Facebook, Instagram, TikTok, Snapchat, messaging apps. Toddlers don't need social interaction online.",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Enable Maximum Content Restrictions",
                Description = "Block all movies/TV above G rating, restrict web content to only educational sites, disable app downloads.",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Set Daily Time Limit: 1 Hour",
                Description = "Limit total screen time to 1 hour per day for healthy development.",
                Priority = "High"
            });
        }
        else if (ageBracket == "child")
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Block Most Social Media Platforms",
                Description = "Block TikTok, Snapchat, Instagram. Allow YouTube Kids only (not regular YouTube).",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Set Daily Time Limit: 2-3 Hours",
                Description = "Limit screen time to 2-3 hours on weekdays, slightly more on weekends.",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Enable Moderate Content Filtering",
                Description = "Block mature content (PG-13+), restrict explicit music/books, filter web searches.",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Require Approval for App Downloads",
                Description = "All app installations must be approved by parent first.",
                Priority = "Medium"
            });
        }
        else // teen
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Enable Content Filters (Not Total Blocks)",
                Description = "Filter explicit content, violent/hateful material, but allow age-appropriate social media use.",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Set Downtime Hours (10PM - 7AM)",
                Description = "Prevent late-night usage that disrupts sleep. Device locks during bedtime hours.",
                Priority = "High"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Monitor Communication Apps",
                Description = "Enable message previews and monitoring for messaging/social apps without reading everything.",
                Priority = "Medium"
            });

            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Set Daily Time Limit: 3-4 Hours",
                Description = "Reasonable limit that allows homework and socialization but prevents addiction.",
                Priority = "Medium"
            });
        }

        // Add concern-specific recommendations
        if (concerns.Contains("SocialMedia"))
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Enable Social Media Restrictions",
                Description = deviceType == "iOS" ? 
                    "Use Screen Time to block or limit TikTok, Instagram, Snapchat." :
                    deviceType == "Android" ?
                    "Use Family Link to restrict social media apps and set time limits." :
                    "Use Microsoft Family Safety to block social media websites and apps.",
                Priority = "High"
            });
        }

        if (concerns.Contains("InappropriateContent"))
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Enable SafeSearch & Content Filtering",
                Description = "Turn on SafeSearch in Google, Bing, YouTube. Enable content restrictions for movies/TV/music.",
                Priority = "High"
            });
        }

        if (concerns.Contains("Gaming"))
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Limit Gaming Apps & In-App Purchases",
                Description = "Set time limits specifically for games, disable in-app purchases, block mature-rated games.",
                Priority = "Medium"
            });
        }

        if (concerns.Contains("OnlineStrangers"))
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Disable Location Sharing & Multiplayer",
                Description = "Turn off location services for social apps, disable chat in games, review friend requests.",
                Priority = "High"
            });
        }

        if (concerns.Contains("Cyberbullying"))
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Monitor Messages & Social Interactions",
                Description = "Enable notification alerts for messages, review social media interactions weekly.",
                Priority = "Medium"
            });
        }

        if (concerns.Contains("ScreenTime"))
        {
            recommendations.Add(new Recommendation
            {
                Id = $"rec{idCounter++}",
                Title = "Set App-Specific Time Limits",
                Description = "Limit entertainment apps (games, social, video) while keeping educational apps always available.",
                Priority = "High"
            });
        }

        return recommendations;
    }

    private List<string> GetAppsToBlock(string ageBracket, string deviceType, List<string> concerns)
    {
        var blocklist = new List<string>();

        // Base blocklist by age
        if (ageBracket == "toddler")
        {
            blocklist.AddRange(new[]
            {
                "TikTok", "Instagram", "Snapchat", "Facebook", "Twitter/X",
                "YouTube", "WhatsApp", "Messenger", "Discord", "Reddit",
                "Twitch", "OnlyFans", "Tinder", "Bumble"
            });
        }
        else if (ageBracket == "child")
        {
            blocklist.AddRange(new[]
            {
                "TikTok", "Snapchat", "Instagram", "Twitter/X", "Reddit",
                "Discord", "OnlyFans", "Tinder", "Bumble", "Omegle"
            });
        }
        else // teen
        {
            blocklist.AddRange(new[]
            {
                "OnlyFans", "Tinder", "Bumble", "Omegle", "Adult apps"
            });
        }

        // Add concern-specific blocks
        if (concerns.Contains("Gaming"))
        {
            blocklist.AddRange(new[] { "Roblox (limit hours)", "Fortnite", "Call of Duty", "GTA", "Mature-rated games" });
        }

        return blocklist.Distinct().ToList();
    }

    private List<string> GetAppsToAllow(string ageBracket)
    {
        return ageBracket switch
        {
            "toddler" => new List<string>
            {
                "YouTube Kids", "PBS Kids Video", "Khan Academy Kids",
                "ABCmouse", "Duolingo ABC", "Epic! Kids Books"
            },
            "child" => new List<string>
            {
                "YouTube Kids", "Khan Academy", "Duolingo", "Scratch",
                "Google Classroom", "Zoom", "Microsoft Teams (school)",
                "Educational apps approved by school"
            },
            "teen" => new List<string>
            {
                "YouTube (with restrictions)", "Spotify", "Netflix (age-appropriate)",
                "Google Classroom", "School-required apps", "Study/productivity apps"
            },
            _ => new List<string>()
        };
    }
}

public class SetupRecommendationData
{
    public string DeviceType { get; set; } = "";
    public int ChildAge { get; set; }
    public List<string> Concerns { get; set; } = new();
    public string Summary { get; set; } = "";
    public List<Recommendation> Recommendations { get; set; } = new();
    public List<string> AppsToBlock { get; set; } = new();
    public List<string> AppsToAllow { get; set; } = new();
}

public class Recommendation
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Priority { get; set; } = "High"; // High, Medium, Low
    public string PriorityColor => Priority switch
    {
        "High" => "danger",
        "Medium" => "warning",
        "Low" => "info",
        _ => "secondary"
    };
}

// Lightweight content filter
public class ContentFilter
{
    private readonly HashSet<string> _blockedDomains;
    private readonly List<string> _blockedKeywords;
    private int _totalRequests = 0;
    private int _blockedRequests = 0;
    private readonly Dashboard.BlockedContentStore? _blockedStore;
    private readonly Dashboard.SettingsManager? _settingsManager;
    private string _currentFilterLevel = "moderate";
    
    public ContentFilter(Dashboard.BlockedContentStore? blockedStore = null, Dashboard.SettingsManager? settingsManager = null)
    {
        _blockedStore = blockedStore;
        _settingsManager = settingsManager;
        
        // Default domains (always blocked regardless of level)
        _blockedDomains = new HashSet<string>
        {
            "malicious.com", "phishing.net", "adult-content.com",
            "gambling.org", "illegal-downloads.net"
        };
        
        // Default keywords (moderate level)
        _blockedKeywords = new List<string>
        {
            "adult", "gambling", "drugs", "weapons", "violence",
            "malware", "phishing", "illegal", "torrent"
        };
    }
    
    public Task LoadFiltersAsync()
    {
        // Simulate loading filters - in real implementation load from file/database
        Thread.Sleep(50);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Get blocked keywords based on filter level
    /// </summary>
    private List<string> GetKeywordsForLevel(string level)
    {
        return level.ToLower() switch
        {
            "strict" => new List<string>
            {
                // Strict: Block everything including educational references
                "adult", "gambling", "casino", "poker", "bet", "drugs", "marijuana", "cannabis",
                "weapons", "gun", "violence", "blood", "gore", "malware", "phishing", 
                "illegal", "torrent", "pirate", "crack", "hack", "porn", "xxx", "explicit",
                "hate", "racist", "extremist", "suicide", "self-harm"
            },
            "relaxed" => new List<string>
            {
                // Relaxed: Only block explicit content and illegal activities
                "adult", "porn", "xxx", "explicit", "malware", "phishing", "illegal"
            },
            _ => new List<string> // moderate (default)
            {
                // Moderate: Balanced approach
                "adult", "gambling", "drugs", "weapons", "violence",
                "malware", "phishing", "illegal", "torrent", "porn", "xxx"
            }
        };
    }
    
    public async Task<FilterResult> CheckUrlAsync(string url)
    {
        // Load current filter level from settings
        if (_settingsManager != null)
        {
            var settings = await _settingsManager.LoadSettingsAsync();
            _currentFilterLevel = settings.FilterLevel;
        }
        
        _totalRequests++;
        
        var urlLower = url.ToLowerInvariant();
        var domain = ExtractDomain(url);
        
        // Check blocked domains
        if (_blockedDomains.Contains(domain))
        {
            _blockedRequests++;
            var result = new FilterResult
            {
                IsBlocked = true,
                Reason = $"Domain '{domain}' is in blocklist"
            };
            
            // Auto-save to dashboard store
            _blockedStore?.AddBlock(url, result.Reason);
            
            return result;
        }
        
        // Check blocked keywords (level-specific)
        var levelKeywords = GetKeywordsForLevel(_currentFilterLevel);
        foreach (var keyword in levelKeywords)
        {
            if (urlLower.Contains(keyword))
            {
                _blockedRequests++;
                var result = new FilterResult
                {
                    IsBlocked = true,
                    Reason = $"Contains blocked keyword '{keyword}' (Filter: {_currentFilterLevel})"
                };
                
                // Auto-save to dashboard store
                _blockedStore?.AddBlock(url, result.Reason);
                
                return result;
            }
        }
        
        return new FilterResult
        {
            IsBlocked = false,
            Reason = "No blocking rules matched"
        };
    }
    
    private string ExtractDomain(string url)
    {
        try
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return url.Split('/')[0];
        }
    }
    
    public FilterStatistics GetStatistics()
    {
        return new FilterStatistics
        {
            TotalRequests = _totalRequests,
            BlockedRequests = _blockedRequests,
            AllowedRequests = _totalRequests - _blockedRequests,
            BlockRate = _totalRequests > 0 ? (double)_blockedRequests / _totalRequests : 0,
            AllowRate = _totalRequests > 0 ? (double)(_totalRequests - _blockedRequests) / _totalRequests : 0
        };
    }
}

// Simple data models
public class ContentAnalysis
{
    public double SafetyScore { get; set; }
    public string Category { get; set; } = "";
    public double Confidence { get; set; }
    public string Recommendation { get; set; } = "";
    public List<string> Flags { get; set; } = new();
}

public class FilterResult
{
    public bool IsBlocked { get; set; }
    public string Reason { get; set; } = "";
}

public class FilterStatistics
{
    public int TotalRequests { get; set; }
    public int BlockedRequests { get; set; }
    public int AllowedRequests { get; set; }
    public double BlockRate { get; set; }
    public double AllowRate { get; set; }
}