using SharpNeatLib.Experiments;
using SharpNeatLib.NeuralNetwork;

namespace OscillatorQuadruped
{
    internal class CTRNNNetworkEvaluator : INetworkEvaluator
    {
        public static CTRNNSubstrate substrate;
        private NoveltyArchive noveltyArchive;

        public CTRNNNetworkEvaluator(uint inputs, uint outputs, uint hidden)
        {
            substrate = new CTRNNSubstrate(inputs, outputs, hidden, HyperNEATParameters.substrateActivationFunction);
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

        public void endOfGeneration()
        {
            noveltyArchive.endOfGeneration();
        }

        public double EvaluateNetwork(INetwork network)
        {
            return 1;
        }

        public string EvaluatorStateMessage
        {
            get { return ""; }
        }

        #endregion
    }
}
