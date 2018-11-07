using BepuUtilities;
using DemoContentLoader;
using DemoEngine;
using DemoUtilities;
using OpenTK;

namespace SharpNeatWalker
{
    internal static class Program
    {
        private static void Main()
        {
            if (!false) // Run in the background
                using (var simulation = new WalkerDemo())
                {
                    var start = System.DateTime.UtcNow;
                    simulation.RunPhysics(canContinue: () => (System.DateTime.UtcNow - start).TotalSeconds < -5); // Run for a few secs
                }

            ContentArchive content;
            using (var stream = typeof(Demos.DemoHarness).Assembly.GetManifestResourceStream("Demos.Demos.contentarchive"))
            {
                content = ContentArchive.Load(stream);
            }

            using (var window = new DemoEngine.Window("SharpNeat Walker", new Int2((int)(DisplayDevice.Default.Width * 0.75f), (int)(DisplayDevice.Default.Height * 0.75f)), WindowMode.Windowed))
            using (var loop = new GameLoop(window))
            {
                var harness = new DemoHarness(loop, content, customDemoSet: new DemoSet().AddOption<WalkerDemo>());
                loop.Run(harness);
            }

            /*var loop = new GameLoop(window);
            var demo = new DemoHarness(loop, content, customDemoSet: new DemoSet().AddOption<Walker>());
            loop.Run(demo);
            loop.Dispose();*/
        }
    }
}
