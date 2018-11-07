using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using DemoContentLoader;
using DemoEngine;
using DemoRenderer;
using DemoRenderer.UI;
using Demos;
using DemoUtilities;

namespace SharpNeatWalker
{
    public class WalkerDemo : DemoEngine.Demo
    {
        public WalkerDemo()
        {
            var masks = new BodyProperty<ulong>();
            var callbacks = new Demos.Demos.RagdollCallbacks { Masks = masks };
            Simulation = Simulation.Create(BufferPool, callbacks);
            Simulation.PoseIntegrator.Gravity = new Vector3(0, -9.8f, 0);

            FixedTimeScale = 1; // TODO: Statistics doesn't work without this because ElapsedMilliseconds is only computed then

            Objects.Add(new World());
            Objects.Add(new Quadruped());
            Objects.Add(new DemoEngine.Objects.Controls());
            Objects.Add(new DemoEngine.Objects.Statistics(this));
        }
    }

    public class World : SimulatedObject
    {
        public override void InitializePhysics(Simulation simulation, BufferPool bufferPool, SimpleThreadDispatcher threadDispatcher)
        {
            /*var boxShape = new Box(0.5f, 0.5f, 2.5f);
            boxShape.ComputeInertia(1, out var boxLocalInertia);
            var boxDescription = new BodyDescription
            {
                Activity = new BodyActivityDescription { MinimumTimestepCountUnderThreshold = 32, SleepThreshold = -0.01f },
                LocalInertia = boxLocalInertia,
                Pose = new RigidPose
                {
                    Orientation = BepuUtilities.Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 0),
                    Position = new Vector3(1, -0.5f, 0)
                },
                Collidable = new CollidableDescription { SpeculativeMargin = 50.1f, Shape = simulation.Shapes.Add(boxShape) }
            };
            simulation.Bodies.Add(boxDescription);

            simulation.Statics.Add(new StaticDescription(new Vector3(0, -3, 0), new CollidableDescription { SpeculativeMargin = 0.1f, Shape = simulation.Shapes.Add(new Box(4, 1, 4)) }));*/

            var staticShape = new Box(300, 1, 300);
            var staticShapeIndex = simulation.Shapes.Add(staticShape);
            var staticDescription = new StaticDescription
            {
                Collidable = new CollidableDescription
                {
                    Continuity = new ContinuousDetectionSettings { Mode = ContinuousDetectionMode.Discrete },
                    Shape = staticShapeIndex,
                    SpeculativeMargin = 0.1f
                },
                Pose = new RigidPose
                {
                    Position = new Vector3(0, -0.5f, 0),
                    Orientation = BepuUtilities.Quaternion.Identity
                }
            };
            simulation.Statics.Add(staticDescription);
        }

        public override void InitializeRendering(ContentArchive content, Camera camera)
        {
            /*camera.Position = new Vector3(-20, 10, -20);
            camera.Yaw = MathHelper.Pi * 3f / 4;
            camera.Pitch = MathHelper.Pi * 0.05f;*/
            camera.Position = new Vector3(-10, 5, -10);
            camera.Yaw = MathHelper.Pi * 3f / 4;
            camera.Pitch = MathHelper.Pi * 0.1f;
        }

        public override bool CheckInput(Input input)
        {
            if (input.WasDown(OpenTK.Input.Key.P)) Log("$"); // Simple test
            return base.CheckInput(input);
        }

        public override void Render(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font)
        {
            text.Clear().Append("Press P to print $!");
            renderer.TextBatcher.Write(text, new Vector2(20, renderer.Surface.Resolution.Y - 20), 16, new Vector3(1, 1, 1), font);
        }
    }
}
