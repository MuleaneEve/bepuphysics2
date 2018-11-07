using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;

namespace OscillatorQuadruped
{
    class CluneSubstrate : Substrate
    {
        private const float shiftScale = 0.2f;

        public CluneSubstrate(uint inputs, uint outputs, uint hidden, IActivationFunction function)
            : base(inputs, outputs, hidden, function)
        {

        }

        public override NeatGenome generateGenome(INetwork network)
        {
            var coordinates = new double[6];
            int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;
            uint connectionCounter = 0;
            ConnectionGeneList connections = new ConnectionGeneList((int)((inputCount * hiddenCount) + (hiddenCount * outputCount)));


            for (int layer = -1; layer < 1; layer++)
            {
                coordinates[0] = layer;
                coordinates[3] = layer + 1;
                uint srcRow = 0;
                for (float row1 = -1; row1 <= 1; row1 += 0.5f, srcRow++)
                {
                    coordinates[1] = row1;
                    uint srcCol = 0;
                    for (float col1 = -1; col1 <= 1; col1 += 0.5f, srcCol++)
                    {
                        coordinates[2] = col1;
                        uint tarRow = 0;
                        for (float row2 = -1; row2 <= 1; row2 += 0.5f, tarRow++)
                        {
                            coordinates[4] = row2;
                            uint tarCol = 0;
                            for (float col2 = -1; col2 <= 1; col2 += 0.5f, tarCol++)
                            {
                                coordinates[5] = col2;

                                network.ClearSignals();
                                network.SetInputSignals(coordinates);
                                network.MultipleSteps(iterations);
                                float output = network.GetOutputSignal(0);
                                network.ClearSignals();

                                if (Math.Abs(output) > threshold)
                                {
                                    uint source = srcRow * 5 + srcCol;
                                    if (layer == 0)
                                        source += inputCount + outputCount;
                                    uint target = tarRow * 5 + tarCol;
                                    if (layer == -1)
                                        target += inputCount + outputCount;
                                    else
                                        target += inputCount;

                                    float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
                                    connections.Add(new ConnectionGene(connectionCounter++, source, target, weight));
                                }
                            }

                            if (row2 == -0.5f)
                                row2 += 0.5f;
                        }
                    }

                    if (row1 == -0.5f)
                        row1 += 0.5f;
                }
            }

            return new SharpNeatLib.NeatGenome.NeatGenome(0, neurons, connections, (int)inputCount, (int)outputCount);
        }
    }
}
