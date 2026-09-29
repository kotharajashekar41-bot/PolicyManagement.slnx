using Xunit;

// PolicyApiFactory configures its database via process-wide environment variables
// (see the comment in PolicyApiFactory.ConfigureWebHost), so concurrently-starting
// factory instances from different test classes could race and point at each
// other's SQLite file. Serializing keeps that simple instead of plumbing a
// per-instance config source through the minimal-hosting entry point.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
