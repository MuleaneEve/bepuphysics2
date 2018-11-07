using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.Experiments;

namespace OscillatorQuadruped
{
    class CTRNNPopulationEvaluator : MultiThreadedPopulationEvaluator
    {

        public CTRNNPopulationEvaluator(INetworkEvaluator eval)
            : base(eval, null)
        {

        }
    }
}
