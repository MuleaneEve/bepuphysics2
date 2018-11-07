using System;
using System.Collections.Generic;
using BepuPhysics;
using BepuUtilities.Memory;
using DemoContentLoader;
using DemoRenderer;
using DemoRenderer.UI;
using DemoUtilities;
using Demos;

namespace DemoEngine
{
    public enum Status
    {
        Starting,
        Running,
        Paused,
        Stopped,
        Disposed
    }

    public abstract class Demo : IDisposable
    {
        private bool _initialized;

        public bool IsRendering { get; private set; }
        public Status Status { get; private set; }


        private readonly SimulatedObjectSet<SimulatedObject> _set = new SimulatedObjectSet<SimulatedObject>();
        public List<SimulatedObject> Objects => _set.Objects;

        //TODO: While for the sake of the demos, using one update per render is probably the easiest/best choice,
        //we can't assume that every monitor has a 60hz refresh rate. One simple option here is to just measure the primary display's refresh rate ahead of time
        //and use that as the simulation timeStep duration. Different displays would affect the simulation, but it wouldn't be too bad, and it would be locally consistent.
        public float Dt { get; set; } = 1 / 60f;

        public float? FixedTimeScale; // Set to 1.0f for real-time simulations
        private System.Diagnostics.Stopwatch _watch;
        public long ElapsedMilliseconds { get; private set; }


        /// <summary>
        /// Gets the simulation created by the demo.
        /// </summary>
        public Simulation Simulation { get; protected set; }

        //Note that the buffer pool used by the simulation is not considered to be *owned* by the simulation. The simulation merely uses the pool.
        //Disposing the simulation will not dispose or clear the buffer pool.
        /// <summary>
        /// Gets the buffer pool used by the demo's simulation.
        /// </summary>
        public BufferPool BufferPool { get; }

        /// <summary>
        /// Gets the thread dispatcher available for use by the simulation.
        /// </summary>
        public SimpleThreadDispatcher ThreadDispatcher { get; }

        protected Demo()
        {
            BufferPool = new BufferPool();
            ThreadDispatcher = new SimpleThreadDispatcher(Environment.ProcessorCount);
        }


        public void TogglePaused() // TODO: Input binding
        {
            if (Status == Status.Running)
                Status = Status.Paused;
            else if (Status == Status.Paused)
                Status = Status.Running;
            // Else, do nothing
        }

        private void Stop()
        {
            if (Status < Status.Stopped)
                Status = Status.Stopped;
        }


        private void InitializePhysics()
        {
            if (_initialized)
                throw new Exception("Already Initialized");
            _initialized = true;

            _set.InitializePhysics(Simulation, BufferPool, ThreadDispatcher);
            Status = Status.Running;
        }

        public void InitializeRendering(ContentArchive content, Camera camera)
        {
            IsRendering = true;
            InitializePhysics();
            _set.InitializeRendering(content, camera);
        }


        protected virtual void OnDispose()
        {
        }

        public void Dispose()
        {
            if (Status != Status.Disposed)
            {
                Status = Status.Disposed;
                OnDispose();
                _set.Dispose();
                Simulation.Dispose();
                BufferPool.Clear();
                ThreadDispatcher.Dispose();
            }
        }

#if DEBUG
        ~Demo()
        {
            Helpers.CheckForUndisposed(Status == Status.Disposed, this);
        }
#endif


        public void RunPhysics(int maxSteps = int.MaxValue, Func<bool> canContinue = null)
        {
            InitializePhysics();

            for (var i = 0; i < maxSteps;)
            {
                if (canContinue != null && !canContinue())
                    break;
                var timeSteps = TimeSteps();
                if (timeSteps == null)
                    break;
                i += timeSteps.Value;
            }

            Stop();
        }

        public bool CheckInput(Input input)
        {
            return _set.CheckInput(input); // Returning false stops the simulation loop
        }

        /*public bool TimeStepsAndRender(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font) // TODO: Moved to GameLoop?
        {
            if (!isPaused)
                if (TimeSteps() == null)
                    return false;

            Render(renderer, text, font, isPaused);
            return true;
        }*/



        public int? TimeSteps()
        {
            int stepCount;
            if (FixedTimeScale == null) // Not fixed
                stepCount = 1;
            else if (FixedTimeScale.Value <= 0 || Dt <= 0)
                stepCount = 0; // Paused
            else
            {
                if (_watch == null) // First step, start the watch
                    _watch = System.Diagnostics.Stopwatch.StartNew();
                var stepSizeMs = 1000.0 * Dt / FixedTimeScale.Value;
                var deltaMs = _watch.ElapsedMilliseconds - ElapsedMilliseconds;
                stepCount = (int)Math.Floor(deltaMs / stepSizeMs); // Note: If TimeStep()+Render() continuously take longer than StepSize to complete, we will fall further and further behind real-time
                ElapsedMilliseconds += (long)Math.Round(stepCount * stepSizeMs);
            }

            for (var i = 0; i < stepCount; i++)
                if (!TimeStep())
                {
                    Stop();
                    return null;
                }
            return stepCount;
        }

        private bool TimeStep()
        {
            if (!_set.PreTimeStep(Dt))
                return false;

            Simulation.Timestep(Dt, ThreadDispatcher);

            if (!_set.PostTimeStep(Dt))
                return false;

            return true;
        }


        public void Render(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font)
        {
            _set.Render(renderer, camera, input, text, font);
        }


        protected void Log(string message, params object[] objs)
        {
            Utils.Logger.Log(this, message, objs);
        }
    }
}