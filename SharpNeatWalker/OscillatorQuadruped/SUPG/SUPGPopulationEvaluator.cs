using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.Experiments;

namespace OscillatorQuadruped
{
    class SUPGPopulationEvaluator : MultiThreadedPopulationEvaluator
    {

        public SUPGPopulationEvaluator(INetworkEvaluator eval)
            : base(eval, null)
        {

        }
    }
}
