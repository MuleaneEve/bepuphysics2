using System;
using System.Threading.Tasks;
using BepuUtilities;
using BepuUtilities.Memory;
using DemoRenderer;
using DemoUtilities;

namespace DemoEngine
{
    public class GameLoop : IDisposable
    {
        public Window Window { get; private set; }
        public Input Input { get; private set; }
        public Camera Camera { get; private set; }
        public RenderSurface Surface { get; private set; }
        public Renderer Renderer { get; private set; }
        public DemoHarness DemoHarness { get; set; }
        public BufferPool Pool { get; } = new BufferPool();

        public GameLoop(Window window)
        {
            Window = window;
            Input = new Input(Window.window, Pool);
            var useDebugLayer =
#if DEBUG
                true;
#else
                false;
#endif
            Surface = new RenderSurface(Window.Handle, Window.Resolution, enableDeviceDebugLayer: useDebugLayer);
            Renderer = new Renderer(Surface);
            Camera = new Camera(Window.Resolution.X / (float)Window.Resolution.Y, (float)Math.PI / 3, 0.01f, 100000);

            Window.Resized += OnResize;
        }

        private void Loop() // TODO: int maxSteps = int.MaxValue, Func<bool> canContinue = null) // Similar to Demo.RunPhysics()?
        {
            int maxSteps = int.MaxValue; Func<bool> canContinue = null;
            for (var i = 0; i < maxSteps;)
            {
                if (canContinue != null && !canContinue())
                    break;
                var timeSteps = UpdateInLoop();
                if (timeSteps == null)
                    break;
                i += timeSteps.Value;
            }
        }

        private void Update()
        {
            UpdateInLoop();
        }

        private bool _first = true;
        private int? UpdateInLoop()
        {
            if (_first)
            {
                _first = false;
                OnResize(this, EventArgs.Empty);
            }

            int? timeSteps = 0;
            if (!Window.window.Exists)
                return null;
            if (Window.window.WindowState == OpenTK.WindowState.Minimized)
                return timeSteps;

            Input.Start();
            if (DemoHarness != null)
            {
                //We'll let the delegate's logic handle the variable time steps.
                timeSteps = DemoHarness.Update(1/60f); // TODO
                //At the moment, rendering just follows sequentially. Later on we might want to distinguish it a bit more with fixed time stepping or something. Maybe.
                DemoHarness.Render(Renderer);
            }
            Renderer.Render(Camera);
            Surface.Present();
            Input.End();
            return timeSteps;
        }

        public void Run(DemoHarness harness)
        {
            DemoHarness = harness;
            if (!true) // TODO: Controls like F1 don't work in this case
            {
                Task.Run(Loop);
                Window.Run(() => { });
            }
            else
                Window.Run(Update);
        }

        private void OnResize(object sender, EventArgs args)
        {
            //Note that minimizing or resizing the window to invalid sizes don't result in actual resize attempts. Zero width rendering surfaces aren't allowed.
            if (Window.window.Width <= 0 || Window.window.Height <= 0)
                return;
            var resolution = new Int2(Window.window.Width, Window.window.Height);
            //We just don't support true fullscreen in the demos. Would be pretty pointless.
            Renderer.Surface.Resize(resolution, false);
            Camera.AspectRatio = resolution.X / (float)resolution.Y;
            DemoHarness?.OnResize(resolution);
        }

        bool disposed;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Input.Dispose();
                Renderer.Dispose();
                Pool.Clear();
                //Note that we do not own the window.
            }
        }
    }
}
