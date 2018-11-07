using SharpNeatLib.Experiments;
using SharpNeatLib.NeuralNetwork;

namespace OscillatorQuadruped
{
    internal class CluneNetworkEvaluator : INetworkEvaluator
    {
        public static CluneSubstrate substrate;
        private NoveltyArchive noveltyArchive;

        public CluneNetworkEvaluator(uint inputs, uint outputs, uint hidden)
        {
            substrate = new CluneSubstrate(inputs, outputs, hidden, HyperNEATParameters.substrateActivationFunction);
            noveltyArchive = new NoveltyArchive();
        }

        #region INetworkEvaluator Members

        public double[] threadSafeEvaluateNetwork(INetwork network)
        {
            var tempGenome = substrate.generateGenome(network);
            var tempNet = tempGenome.Decode(null);

            using (var quadDomain = new Domain(noveltyArchive, MainProgram.novelty))
            {
                var fitness = quadDomain.EvaluateController(new Controller(tempNet));
                return fitness;
            }
        }

        public double EvaluateNetwork(INetwork network)
        {
            var tempGenome = substrate.generateGenome(network);
            var tempNet = tempGenome.Decode(null);

            using (var quadDomain = new Domain())
            {
                var fitness = quadDomain.EvaluateController(new Controller(tempNet));
                return fitness[0];
            }
        }

        public string EvaluatorStateMessage
        {
            get { return ""; }
        }

        public void endOfGeneration()
        {
            noveltyArchive.endOfGeneration();
        }

        #endregion
    }
}
