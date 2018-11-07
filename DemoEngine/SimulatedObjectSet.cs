using System.Collections.Generic;
using BepuPhysics;
using BepuUtilities.Memory;
using DemoContentLoader;
using DemoRenderer;
using DemoRenderer.UI;
using Demos;
using DemoUtilities;

namespace DemoEngine
{
    public class SimulatedObjectSet<TObject> : SimulatedObject where TObject : SimulatedObject
    {
        public readonly List<TObject> Objects = new List<TObject>();

        public override void InitializePhysics(Simulation simulation, BufferPool bufferPool, SimpleThreadDispatcher threadDispatcher)
        {
            foreach (var o in Objects)
                o.InitializePhysics(simulation, bufferPool, threadDispatcher);
        }

        public override void InitializeRendering(ContentArchive content, Camera camera)
        {
            foreach (var o in Objects)
                o.InitializeRendering(content, camera);
        }

        public override void Dispose()
        {
            foreach (var o in Objects)
                o.Dispose();
        }

        public override bool CheckInput(Input input)
        {
            foreach (var o in Objects)
                if (!o.CheckInput(input))
                    return false;
            return true; // Returning false stops the simulation loop
        }

        public override bool PreTimeStep(float dt)
        {
            foreach (var o in Objects)
                if (!o.PreTimeStep(dt))
                    return false;
            return true; // Returning false stops the simulation loop
        }

        public override bool PostTimeStep(float dt)
        {
            foreach (var o in Objects)
                if (!o.PostTimeStep(dt))
                    return false;
            return true; // Returning false stops the simulation loop
        }

        public override void Render(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font)
        {
            foreach (var o in Objects)
                o.Render(renderer, camera, input, text, font);
        }
    }
}