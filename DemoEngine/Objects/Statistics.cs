using System.Numerics;
using DemoRenderer;
using DemoRenderer.UI;
using DemoUtilities;

namespace DemoEngine.Objects
{
    public class Statistics : SimulatedObject // TODO: Input toggle to turn it off
    {
        private readonly Demo _demo;
        private readonly System.Diagnostics.Stopwatch _renderWatch = System.Diagnostics.Stopwatch.StartNew();
        private readonly System.Diagnostics.Stopwatch _reportWatch = System.Diagnostics.Stopwatch.StartNew();
        public long TimeStepCount;
        public long RenderCount;

        private long _timeStepsAtLastReport;
        private long _elapsedMsForTimeStepsAtLastReport;
        private long _rendersAtLastReport;
        private long _elapsedMsForRendersAtLastReport;
        private string _currentStats = "...";

        public float TimeStepsPerSecond
        {
            get
            {
                var count = TimeStepCount - _timeStepsAtLastReport;
                var ms = _demo.ElapsedMilliseconds - _elapsedMsForTimeStepsAtLastReport;
                return ms == 0 ? 0 : 1000.0f * count / ms;
            }
        }

        public float RendersPerSecond
        {
            get
            {
                var count = RenderCount - _rendersAtLastReport;
                var ms = _renderWatch.ElapsedMilliseconds - _elapsedMsForRendersAtLastReport;
                return ms == 0 ? 0 : 1000.0f * count / ms;
            }
        }

        public float ElapsedMsSinceLastTimeStepReport
        {
            get
            {
                return _reportWatch.ElapsedMilliseconds;
            }
        }

        public Statistics(Demo demo)
        {
            _demo = demo;
        }

        public override bool PreTimeStep(float dt)
        {
            if (ElapsedMsSinceLastTimeStepReport > 1000) // At most every second
            {
                _currentStats = "TimeSteps: " + TimeStepCount + "; " +
                                "Cumulative: " + (1000.0f * TimeStepCount / _demo.ElapsedMilliseconds).ToString("0") +
                                " sps; " + (1000.0f * RenderCount / _demo.ElapsedMilliseconds).ToString("F") + " fps; " +
                                "Current: " + TimeStepsPerSecond.ToString("0") + " sps; " + RendersPerSecond.ToString("F") + " fps";
                Log(null, _currentStats);

                _timeStepsAtLastReport = TimeStepCount;
                _elapsedMsForTimeStepsAtLastReport = _demo.ElapsedMilliseconds; // Note: Because _demo.ElapsedMilliseconds is increased once per batch of timeSteps (instead of per timeStep), the reporting is not accurate while multiple timeSteps are being called in the same batch
                _rendersAtLastReport = RenderCount;
                _elapsedMsForRendersAtLastReport = _renderWatch.ElapsedMilliseconds;
                _reportWatch.Restart();
            }
            return true;
        }

        public override bool PostTimeStep(float dt)
        {
            TimeStepCount++;
            return true;
        }

        public override void Render(Renderer renderer, Camera camera, Input input, TextBuilder text, Font font)
        {
            RenderCount++;

            text.Clear().Append(_currentStats);
            renderer.TextBatcher.Write(text, new Vector2(20, renderer.Surface.Resolution.Y - 20), 16, new Vector3(1, 1, 1), font);
        }
    }
}