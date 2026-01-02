# PocketFence-AI

A lightweight local content filtering system optimized for local inference without external dependencies.

## Key Features
- Local AI inference for content analysis
- URL and text content filtering
- Real-time threat detection
- Privacy-first (all processing happens locally)
- Simple command-line interface
- Cross-platform support

## Development Guidelines
- Use .NET 8.0 for cross-platform compatibility
- Follow C# coding best practices
- Implement proper error handling and logging
- Keep dependencies minimal
- Optimize for local inference performance
- Ensure all processing remains local (no external API calls)

## Architecture
- SimpleAI: Lightweight AI engine for threat detection
- ContentFilter: URL-based filtering with domain blocklists
- Program (CLI): Interactive command-line interface
- Minimal configuration via appsettings.json
- No external dependencies or APIs required