using System;
using BepuPhysics;
using BepuUtilities.Memory;
using DemoContentLoader;
using DemoRenderer;
using DemoRenderer.UI;
using Demos;
using DemoUtilities;

namespace DemoEngine
{
    public abstract class SimulatedObject : IDisposable
    {
        protected const float PI = (float)Math.PI;

        public virtual void InitializePhysics(Simulation simulation, BufferPool bufferPool, SimpleThreadDispatcher threadDispatcher)
        {
        }

        public virtual void InitializeRendering(ContentArchive content, Camera camera)
        {
        }

        public virtual void Dispose()
        {
        }

        public virtual bool CheckInput(Input input)
        {
            return true; // Returning false stops the simulation loop
        }

        public virtual bool PreTimeStep(float dt)
        {
            return true; // Returning false stops the simulation loop
        }

        public virtual bool PostTimeStep(float dt)
        {
            return true; // Returning false stops the simulation loop
        }

        public virtual void Render(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font)
        {
        }

        protected void Log(string message, params object[] objs)
        {
            Utils.Logger.Log(this, message, objs);
        }
    }
}