using SharpNeatLib.Experiments;
using SharpNeatLib.NeuralNetwork;

namespace OscillatorQuadruped
{
    internal class SUPGNetworkEvaluator : INetworkEvaluator
    {
        public static SUPGSubstrate substrate;
        private NoveltyArchive noveltyArchive;

        public SUPGNetworkEvaluator(uint inputs, uint outputs, uint hidden)
        {
            substrate = new SUPGSubstrate(inputs, outputs, hidden, HyperNEATParameters.substrateActivationFunction);
            noveltyArchive = new NoveltyArchive();
        }

        #region INetworkEvaluator Members

        public double[] threadSafeEvaluateNetwork(INetwork network)
        {
            var tempGenome = substrate.generateGenome(network);
            var tempNet = tempGenome.Decode(null);

            using (var quadDomain = new Domain(noveltyArchive, MainProgram.novelty))
            {
                var fitness = quadDomain.EvaluateController(new Controller(tempNet, true, tempGenome, network, substrate.getSUPGMap()));
                return fitness;
            }
        }

        public double EvaluateNetwork(INetwork network)
        {
            var tempGenome = substrate.generateGenome(network);
            var tempNet = tempGenome.Decode(null);

            using (var quadDomain = new Domain())
            {
                var fitness = quadDomain.EvaluateController(new Controller(tempNet, true, tempGenome, network, substrate.getSUPGMap()));
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
