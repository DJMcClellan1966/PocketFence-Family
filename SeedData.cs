using PocketFence_AI.Dashboard;

namespace PocketFence_AI;

/// <summary>
/// Adds sample blocked content for testing the dashboard
/// </summary>
public static class SeedData
{
    public static void AddSampleBlocks(BlockedContentStore store)
    {
        // Add some historical data
        store.AddBlock("inappropriate-site.com/page1", "Domain 'inappropriate-site.com' is in blocklist", "Adult Content");
        store.AddBlock("violent-game.com/download", "Contains blocked keyword 'violent'", "Violence");
        store.AddBlock("how to hack wifi password", "Contains blocked keyword 'hack'", "Dangerous Activities");
        store.AddBlock("online-casino.net/play", "Contains blocked keyword 'gambling'", "Gambling");
        store.AddBlock("hate-forum.org/posts", "Domain 'hate-forum.org' is in blocklist", "Hate Speech");
        
        Console.WriteLine("✅ Added sample blocked content for dashboard testing");
    }
}
