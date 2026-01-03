using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PocketFence_AI.DeviceControl
{
    /// <summary>
    /// Unified interface for controlling child devices via native OS parental control APIs.
    /// Implementations: iOS Screen Time, Google Family Link, Microsoft Family Safety.
    /// </summary>
    public interface IDeviceControlProvider
    {
        /// <summary>
        /// The platform this provider controls (iOS, Android, Windows).
        /// </summary>
        string Platform { get; }

        /// <summary>
        /// Authenticate parent account with the OS provider (OAuth flow).
        /// </summary>
        /// <param name="parentUserId">PocketFence parent user ID</param>
        /// <returns>True if authentication successful</returns>
        Task<(bool Success, string ErrorMessage)> AuthenticateAsync(string parentUserId);

        /// <summary>
        /// Link a child device to this parent account.
        /// </summary>
        /// <param name="deviceId">PocketFence device ID</param>
        /// <param name="osDeviceIdentifier">OS-specific device identifier (UDID, Android ID, etc.)</param>
        /// <returns>True if device linked successfully</returns>
        Task<(bool Success, string ErrorMessage)> LinkDeviceAsync(string deviceId, string osDeviceIdentifier);

        /// <summary>
        /// Push content restrictions to the child device.
        /// </summary>
        /// <param name="deviceId">PocketFence device ID</param>
        /// <param name="restrictions">Content filtering rules</param>
        /// <returns>True if restrictions applied successfully</returns>
        Task<(bool Success, string ErrorMessage)> PushRestrictionsAsync(string deviceId, DeviceRestrictions restrictions);

        /// <summary>
        /// Get current restrictions configured on the device.
        /// </summary>
        /// <param name="deviceId">PocketFence device ID</param>
        /// <returns>Current restriction settings</returns>
        Task<DeviceRestrictions?> GetRestrictionsAsync(string deviceId);

        /// <summary>
        /// Retrieve activity report from the OS provider.
        /// </summary>
        /// <param name="deviceId">PocketFence device ID</param>
        /// <param name="startDate">Report start date</param>
        /// <param name="endDate">Report end date</param>
        /// <returns>Activity data (apps used, screen time, blocked content)</returns>
        Task<DeviceActivityReport?> GetActivityReportAsync(string deviceId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get real-time device status (online, offline, current app).
        /// </summary>
        /// <param name="deviceId">PocketFence device ID</param>
        /// <returns>Device status information</returns>
        Task<DeviceStatus?> GetDeviceStatusAsync(string deviceId);

        /// <summary>
        /// Unlink device from parent account.
        /// </summary>
        /// <param name="deviceId">PocketFence device ID</param>
        /// <returns>True if device unlinked successfully</returns>
        Task<(bool Success, string ErrorMessage)> UnlinkDeviceAsync(string deviceId);
    }

    /// <summary>
    /// Device restriction settings to be pushed to OS parental controls.
    /// </summary>
    public class DeviceRestrictions
    {
        // Web Content Filtering
        public List<string> BlockedDomains { get; set; } = new();
        public List<string> AllowedDomainsOnly { get; set; } = new(); // Whitelist mode
        public bool SafeSearchEnabled { get; set; } = true;
        public bool AdultContentBlocked { get; set; } = true;

        // App Restrictions
        public List<string> BlockedApps { get; set; } = new(); // Package names or bundle IDs
        public List<string> AlwaysAllowedApps { get; set; } = new(); // Educational apps
        public int MaxAgeRating { get; set; } = 12; // ESRB/PEGI equivalent

        // Screen Time Limits
        public int DailyScreenTimeLimitMinutes { get; set; } = 120; // 2 hours default
        public Dictionary<string, int> AppTimeLimits { get; set; } = new(); // App → minutes per day

        // Downtime/Bedtime
        public TimeSpan DowntimeStart { get; set; } = new TimeSpan(21, 0, 0); // 9pm
        public TimeSpan DowntimeEnd { get; set; } = new TimeSpan(7, 0, 0); // 7am
        public bool DowntimeEnabled { get; set; } = true;

        // Communication Controls (iOS only)
        public List<string> AllowedContacts { get; set; } = new(); // Phone numbers/emails

        // Location Tracking (optional)
        public bool LocationTrackingEnabled { get; set; } = false;

        // Purchase Restrictions
        public bool RequireApprovalForPurchases { get; set; } = true;

        // Age-based preset (used by AI to generate defaults)
        public int ChildAge { get; set; } = 10;
    }

    /// <summary>
    /// Activity report data retrieved from OS provider.
    /// </summary>
    public class DeviceActivityReport
    {
        public string DeviceId { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Screen Time
        public int TotalScreenTimeMinutes { get; set; }
        public Dictionary<DateTime, int> DailyScreenTime { get; set; } = new(); // Date → minutes

        // App Usage
        public Dictionary<string, int> AppUsageMinutes { get; set; } = new(); // App name → minutes
        public List<string> MostUsedApps { get; set; } = new();

        // Web Browsing
        public List<WebsiteVisit> WebsiteVisits { get; set; } = new();
        public int BlockedContentCount { get; set; }

        // Notifications
        public int NotificationCount { get; set; }
        
        // Location (if enabled)
        public List<LocationSnapshot> LocationHistory { get; set; } = new();
    }

    public class WebsiteVisit
    {
        public string Url { get; set; } = "";
        public string Domain { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public bool WasBlocked { get; set; }
        public string? BlockReason { get; set; }
    }

    public class LocationSnapshot
    {
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string LocationName { get; set; } = ""; // "Home", "School", etc.
    }

    /// <summary>
    /// Real-time device status information.
    /// </summary>
    public class DeviceStatus
    {
        public string DeviceId { get; set; } = "";
        public bool IsOnline { get; set; }
        public DateTime LastSeenAt { get; set; }
        public string? CurrentApp { get; set; } // Currently running app
        public int ScreenTimeRemainingMinutes { get; set; }
        public bool InDowntime { get; set; }
        public double? BatteryPercent { get; set; }
    }
}
