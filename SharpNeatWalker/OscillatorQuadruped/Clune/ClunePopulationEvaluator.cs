using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.Experiments;

namespace OscillatorQuadruped
{
    class ClunePopulationEvaluator : MultiThreadedPopulationEvaluator
    {

        public ClunePopulationEvaluator(INetworkEvaluator eval)
            : base(eval, null)
        {

        }
    }
}
