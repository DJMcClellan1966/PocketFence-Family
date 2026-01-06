using System;
using System.Collections.Generic;

namespace PocketFence_AI.DeviceControl
{
    /// <summary>
    /// Represents a child device linked to a parent account.
    /// Stored in devices.json.
    /// </summary>
    public class ChildDevice
    {
        // Core Identity
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ParentUserId { get; set; } = ""; // Links to User.Id
        public string ChildName { get; set; } = ""; // "Emma's iPhone", "Alex's Tablet"
        public int ChildAge { get; set; } = 10;

        // Device Info
        public string Platform { get; set; } = ""; // "iOS", "Android", "Windows"
        public string DeviceType { get; set; } = ""; // "iPhone", "iPad", "Android Phone", "Windows PC"
        public string DeviceModel { get; set; } = ""; // "iPhone 14 Pro", "Samsung Galaxy S23"
        public string OSVersion { get; set; } = ""; // "iOS 17.2", "Android 14"

        // OS-Specific Identifiers (populated by providers)
        public string? iOSDeviceUDID { get; set; } // Apple Device UDID
        public string? AndroidDeviceId { get; set; } // Android Device ID
        public string? WindowsDeviceId { get; set; } // Microsoft Device ID

        // Linking Status
        public bool IsLinked { get; set; } = false;
        public DateTime? LinkedAt { get; set; }
        public string? LinkingToken { get; set; } // Temporary token for pairing
        public DateTime? LinkingTokenExpires { get; set; }

        // OAuth Tokens (encrypted in production)
        public string? OAuthAccessToken { get; set; }
        public string? OAuthRefreshToken { get; set; }
        public DateTime? OAuthTokenExpires { get; set; }

        // Current Restrictions (cached from last push)
        public string? CurrentRestrictionsJson { get; set; } // Serialized DeviceRestrictions
        public DateTime? LastRestrictionUpdate { get; set; }

        // Activity Tracking
        public DateTime? LastActivitySync { get; set; }
        public DateTime? LastSeenOnline { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Quick Stats (cached for dashboard performance)
        public int TotalBlockedContentCount { get; set; } = 0;
        public int ScreenTimeThisWeekMinutes { get; set; } = 0;
    }

    /// <summary>
    /// Storage manager for child devices (JSON-based).
    /// </summary>
    public class DeviceStore
    {
        private const string DevicesFilePath = "Data/devices.json";
        private static readonly object _lock = new object();

        public static List<ChildDevice> LoadDevices()
        {
            lock (_lock)
            {
                if (!File.Exists(DevicesFilePath))
                {
                    return new List<ChildDevice>();
                }

                var json = File.ReadAllText(DevicesFilePath);
                return System.Text.Json.JsonSerializer.Deserialize<List<ChildDevice>>(json) ?? new List<ChildDevice>();
            }
        }

        public static void SaveDevices(List<ChildDevice> devices)
        {
            lock (_lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DevicesFilePath)!);
                var json = System.Text.Json.JsonSerializer.Serialize(devices, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(DevicesFilePath, json);
            }
        }

        public static ChildDevice? GetDevice(string deviceId)
        {
            var devices = LoadDevices();
            return devices.FirstOrDefault(d => d.Id == deviceId);
        }

        public static List<ChildDevice> GetDevicesByParent(string parentUserId)
        {
            var devices = LoadDevices();
            return devices.Where(d => d.ParentUserId == parentUserId && d.IsActive).ToList();
        }

        public static void AddDevice(ChildDevice device)
        {
            var devices = LoadDevices();
            device.UpdatedAt = DateTime.UtcNow;
            devices.Add(device);
            SaveDevices(devices);
        }

        public static void UpdateDevice(ChildDevice device)
        {
            var devices = LoadDevices();
            var existingIndex = devices.FindIndex(d => d.Id == device.Id);
            if (existingIndex >= 0)
            {
                device.UpdatedAt = DateTime.UtcNow;
                devices[existingIndex] = device;
                SaveDevices(devices);
            }
        }

        public static void DeleteDevice(string deviceId)
        {
            var devices = LoadDevices();
            var device = devices.FirstOrDefault(d => d.Id == deviceId);
            if (device != null)
            {
                device.IsActive = false;
                device.UpdatedAt = DateTime.UtcNow;
                SaveDevices(devices);
            }
        }
    }
}
