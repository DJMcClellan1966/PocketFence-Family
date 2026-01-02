using PocketFence_AI.Dashboard;

namespace PocketFence_AI;

/// <summary>
/// Adds sample blocked content for testing the dashboard
/// </summary>
public static class SeedData
{
    public static void AddSampleBlocks(BlockedContentStore store)
    {
        // Malware & Security Threats
        store.AddBlock("https://malware-download.net/virus.exe", "Contains blocked keyword 'malware'", "Malware");
        store.AddBlock("https://phishing-bank.com/login", "Contains blocked keyword 'phishing'", "Phishing");
        store.AddBlock("https://identity-theft.net/steal", "Contains blocked keyword 'theft'", "Cybercrime");
        store.AddBlock("https://scam-lottery.com/winner", "Contains blocked keyword 'scam'", "Scam");
        
        // Adult Content
        store.AddBlock("https://adult-videos.xxx/watch", "Domain 'adult-videos.xxx' is in blocklist", "Adult Content");
        store.AddBlock("https://inappropriate-site.com/page1", "Domain in blocklist", "Adult Content");
        
        // Violence & Weapons
        store.AddBlock("https://violent-content.com/gore", "Contains blocked keyword 'violence'", "Violence");
        store.AddBlock("https://weapons-marketplace.onion/guns", "Contains blocked keyword 'weapons'", "Weapons");
        store.AddBlock("how to build a bomb tutorial", "Contains blocked keyword 'bomb'", "Dangerous Activities");
        
        // Illegal Activities
        store.AddBlock("https://drug-dealer.com/buy", "Contains blocked keyword 'drugs'", "Drugs");
        store.AddBlock("https://darkweb-market.onion/drugs", "Domain 'darkweb-market.onion' is in blocklist", "Dark Web");
        store.AddBlock("https://piracy-site.org/movies", "Domain 'piracy-site.org' is in blocklist", "Piracy");
        
        // Hate Speech & Extremism
        store.AddBlock("https://hate-speech-forum.org/posts", "Domain 'hate-speech-forum.org' is in blocklist", "Hate Speech");
        store.AddBlock("https://fake-news-propaganda.com/conspiracy", "Domain in blocklist", "Misinformation");
        
        // Gambling & Gaming
        store.AddBlock("https://casino-online.net/slots", "Contains blocked keyword 'gambling'", "Gambling");
        store.AddBlock("https://online-casino.net/play", "Contains blocked keyword 'gambling'", "Gambling");
        
        // Hacking & Exploits
        store.AddBlock("https://hack-tools.io/password-cracker", "Contains blocked keyword 'hack'", "Hacking");
        store.AddBlock("how to hack wifi password", "Contains blocked keyword 'hack'", "Dangerous Activities");
        
        Console.WriteLine("✅ Added 18 sample blocked content entries for dashboard testing");
    }
}
