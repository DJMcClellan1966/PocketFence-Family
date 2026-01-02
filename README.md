# 🤖 PocketFence AI - Local Content Filter

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

**A lightweight local content filtering system optimized for local inference without external dependencies.**

## 🌟 Overview

PocketFence AI is a local content filter designed to analyze URLs and text content for safety using a simple AI engine. Built for speed and efficiency, it runs entirely on your local machine with no external API dependencies.


## 🚀 Key Features

- **Local AI Inference**: Lightweight threat analysis without external APIs
- **Content Filtering**: URL and text content analysis
- **Real-Time Analysis**: Fast threat level detection
- **Privacy First**: All processing happens locally
- **Simple CLI**: Easy-to-use command-line interface
- **Extensible**: Simple architecture for customization

## 📦 Installation & Setup

### Prerequisites
- [.NET 8.0](https://dotnet.microsoft.com/download) or later

### Quick Start
```powershell
# Build the project
dotnet build

# Run PocketFence AI
dotnet run
```

## 🎮 Usage

### Interactive Commands

```
pocketfence> check https://example.com
🔍 Analysis for: https://example.com
   Filter Result: ✅ ALLOWED
   AI Threat Score: 0.15/1.0
   Reason: No blocking rules matched

pocketfence> analyze "This is test content"
🧠 AI Content Analysis:
   Safety Score: 0.85/1.0
   Category: General
   Confidence: 0.45
   Recommendation: ALLOW

pocketfence> stats
📊 Filtering Statistics:
   Total Requests: 25
   Blocked: 5 (20.0%)
   Allowed: 20 (80.0%)
   AI Processed: 25

pocketfence> help
Available commands:
  check <url>      - Check if URL should be blocked
  analyze <text>   - Analyze text content for safety
  stats            - Show filtering statistics
  clear            - Clear screen
  help             - Show this help
  exit             - Exit program
```

## 🏗️ Architecture

The system consists of three main components:

### SimpleAI
Lightweight AI engine for threat detection using keyword analysis and pattern matching.

### ContentFilter
URL-based filtering with domain blocklists and keyword detection.

### Program (Main CLI)
Interactive command-line interface for content analysis.

## 🔧 Configuration

Edit threat keywords and safe patterns in the `SimpleAI` class constructor or extend with your own ML models.
- Emoji and sticker support
- Parental oversight capabilities

### 📁 **Family File Manager**
- Secure file storage and sharing
- Age-appropriate folder access
- Content scanning for safety
- Backup and recovery features

### ⏰ **Screen Time Manager**
- Automated time tracking
- Educational time bonuses
- Break reminders and healthy usage

## 📈 Performance

- **Lightweight**: Minimal resource usage
- **Fast**: Quick analysis and filtering
- **Offline**: No internet required for operation
- **Cross-Platform**: Works on Windows, Linux, and macOS

## 🛠️ Extending PocketFence AI

### Adding Custom Filters

Extend the `ContentFilter` class with your own filtering rules:

```csharp
public class ContentFilter
{
    // Add your custom domain blocklists
    private readonly HashSet<string> _blockedDomains = new() 
    {
        "malicious.com", "phishing.net", "your-custom-domain.com"
    };
    
    // Add your custom keyword filters
    private readonly List<string> _blockedKeywords = new()
    {
        "adult", "gambling", "your-custom-keyword"
    };
}
```

### Integrating ML Models

Replace the `SimpleAI` keyword-based analysis with your own ML model:

```csharp
public class SimpleAI
{
    public async Task<double> AnalyzeThreatLevelAsync(string content)
    {
        // Replace with your ML model inference
        // Example: TensorFlow, ONNX, or GPT4All integration
        return await YourMLModel.PredictThreatLevel(content);
    }
}
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built for privacy-conscious users who want local content filtering without external dependencies.

---

**PocketFence AI** - Local content filtering made simple. 🤖✨
