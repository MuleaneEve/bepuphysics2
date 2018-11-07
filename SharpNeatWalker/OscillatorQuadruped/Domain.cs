using System;
using System.Numerics;
using BepuPhysics;
using BepuUtilities;
using DemoContentLoader;
using DemoRenderer;
using DemoRenderer.UI;
using Demos;
using DemoUtilities;

namespace OscillatorQuadruped
{
    internal class Domain : Demo
    {
#if TODO // TODO: OscillatorQuadruped
        private Controller _controller;
        private Quadruped _walker;

        private float[] _behavior;
        private int _behaviorCounter;
        private int _timeCounter;
        private readonly bool _novelty;
        private readonly NoveltyArchive _noveltyArchive;
        private const int SampleRate = 2;

        private bool _cameraFollowCreature;
        private Vector3 _lastxyz;
        private Camera _camera;

        public Domain(NoveltyArchive noveltyArchive = null, bool novelty = false)
        {
            _novelty = novelty;
            _noveltyArchive = noveltyArchive;
        }

        public void Initialize(Controller controller)
        {
            _controller = controller;
            Initialize(null, new Camera(4 / 3f, (float)Math.PI / 3, 0.01f, 100000));
        }

        public override void Initialize(ContentArchive content, Camera camera)
        {
            _camera = camera;
            if (_controller == null)
                _controller = new Controller(true);

            _lastxyz = new Vector3(-2f, -2f, 2f);
            _camera.Position = _lastxyz;
            _camera.Yaw = MathHelper.Pi * 3f / 4;
            _camera.Pitch = MathHelper.Pi * 0.1f;
            _cameraFollowCreature = true;

            Simulation = Simulation.Create(BufferPool, new TestCallbacks());
            Simulation.PoseIntegrator.Gravity = new Vector3(0, 0, 9.8f);

            _walker = new Quadruped(_controller, Simulation);
            _walker.Initialize();
        }

        public override void Update(Input input, float dt)
        {
            base.Update(input, dt); // Calls Simulation.Timestep()

            _walker.Update(dt);

            if (_novelty)
            {
                if (_timeCounter == 0)
                {
                    // update the behavior vector
                    var com = _walker.CurrentCom;
                    // location based novelty
                    UpdateBehavior(com);
                }
                _timeCounter++;
                _timeCounter = _timeCounter % SampleRate;
            }
        }

        public override void Render(Renderer renderer, TextBuilder text, Font font)
        {
            // if we're watching the movie, move the camera automatically
            if (_cameraFollowCreature)
            {
                var pos = _walker.CurrentCom;
                var desiredxyz = new Vector3(pos.X - 2, pos.Y - 2, 2f);
                var xyz = _lastxyz + (desiredxyz - _lastxyz) * .01f;
                _camera.Position = _lastxyz = xyz;
            }
            base.Render(renderer, text, font);
        }

        protected override bool OnCollisionWithGround(Geom geom)
        {
            foreach (var o in Objects)
                for (var i = 0; i < o.Bodies.Count; i++)
                    if (geom.Body == o.Bodies[i])
                        o.BodiesOnGround.Add(o.Bodies[i]);
            return base.OnCollisionWithGround(geom);
        }


        public double[] EvaluateController(Controller controller)
        {
            const int simTime = 1500; // 15 seconds // 100 * 3600 * 100; // 100 hrs            
            _behavior = new float[simTime * 2 / SampleRate];
            _behaviorCounter = 0;

            Initialize(controller);
            Run(simTime);

            var com = _walker.CurrentCom;
            var fitness = _walker.CalcFitness();

            controller.cleanup();

            var objectiveFitness = fitness;

            if (_novelty)
            {
                // update the behavior vector in case the simulation was aborted
                while (_behaviorCounter < _behavior.Length)
                    UpdateBehavior(com);

                // calculate the fitness based on the novelty metric  
                fitness = _noveltyArchive.calcFitness(_behavior);
            }

            return new double[] { fitness, objectiveFitness };
        }

        private void UpdateBehavior(Vector3 com)
        {
            _behavior[_behaviorCounter++] = com.X;
            _behavior[_behaviorCounter++] = com.Y;
        }
#else
        public Domain(NoveltyArchive noveltyArchive = null, bool novelty = false)
        {
        }

        public override void Initialize(ContentArchive content, Camera camera)
        {
        }

        public double[] EvaluateController(Controller controller)
        {
            return null;
        }
#endif
    }
}