//#define OUTPUT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.CPPNs;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;

namespace OscillatorQuadruped
{
    class CTRNNSubstrate : Substrate
    {
        private const float shiftScale = 0.2f;

        public CTRNNSubstrate(uint inputs, uint outputs, uint hidden, IActivationFunction function)
            : base(inputs, outputs, hidden, function)
        {

        }

        public override NeatGenome generateGenome(INetwork network)
        {
            var coordinates = new double[8];
            int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

            // copy the neuron list to a new list and update the biases for hidden and output nodes
            NeuronGeneList newNeurons = new NeuronGeneList(neurons);

            foreach(NeuronGene gene in newNeurons)
            {
                if (gene.NeuronType == NeuronType.Output)
                {
                    gene.NeuronBias = 3; // GWM - Bias hardcoded to 3 for output neurons in Sebastian's CTRNN architecture
                    coordinates[2] = 0;
                    coordinates[3] = 0;
                    coordinates[6] = 0;
                    coordinates[7] = 0;
                    switch (gene.InnovationId)
                    {
                        case 4:
                            coordinates[0] = -1;
                            coordinates[1] = 1;
                            coordinates[4] = -1;
                            coordinates[5] = 1;
                            break;

                        case 5:
                            coordinates[0] = 0;
                            coordinates[1] = 1;
                            coordinates[4] = -1;
                            coordinates[5] = 1;
                            break;

                        case 6:
                            coordinates[0] = 1;
                            coordinates[1] = 1;
                            coordinates[4] = -1;
                            coordinates[5] = 1;
                            break;

                        case 7:
                            coordinates[0] = -1;
                            coordinates[1] = 1;
                            coordinates[4] = 1;
                            coordinates[5] = 1;
                            break;

                        case 8:
                            coordinates[0] = 0;
                            coordinates[1] = 1;
                            coordinates[4] = 1;
                            coordinates[5] = 1;
                            break;

                        case 9:
                            coordinates[0] = 1;
                            coordinates[1] = 1;
                            coordinates[4] = 1;
                            coordinates[5] = 1;
                            break;

                        case 10:
                            coordinates[0] = -1;
                            coordinates[1] = 1;
                            coordinates[4] = -1;
                            coordinates[5] = -1;
                            break;

                        case 11:
                            coordinates[0] = 0;
                            coordinates[1] = 1;
                            coordinates[4] = -1;
                            coordinates[5] = -1;
                            break;

                        case 12:
                            coordinates[0] = 1;
                            coordinates[1] = 1;
                            coordinates[4] = -1;
                            coordinates[5] = -1;
                            break;

                        case 13:
                            coordinates[0] = -1;
                            coordinates[1] = 1;
                            coordinates[4] = 1;
                            coordinates[5] = -1;
                            break;

                        case 14:
                            coordinates[0] = 0;
                            coordinates[1] = 1;
                            coordinates[4] = 1;
                            coordinates[5] = -1;
                            break;

                        case 15:
                            coordinates[0] = 1;
                            coordinates[1] = 1;
                            coordinates[4] = 1;
                            coordinates[5] = -1;
                            break;
                    }

                    float output;
                    network.ClearSignals();
                    network.SetInputSignals(coordinates);
                    network.MultipleSteps(iterations);
                    output = network.GetOutputSignal(3);
                    gene.TimeConstant = (output + 1) * 30 + 1; // normalize output to [1,61] for the time constant
                }
                if (gene.NeuronType == NeuronType.Hidden)
                {
                    coordinates[2] = 0;
                    coordinates[3] = 0;
                    coordinates[6] = 0;
                    coordinates[7] = 0;
                    switch (gene.InnovationId)
                    {
                        case 16:
                            coordinates[0] = -1;
                            coordinates[1] = 0;
                            coordinates[4] = -1;
                            coordinates[5] = 1;
                            break;

                        case 17:
                            coordinates[0] = 1;
                            coordinates[1] = 0;
                            coordinates[4] = -1;
                            coordinates[5] = 1;
                            break;

                        case 18:
                            coordinates[0] = -1;
                            coordinates[1] = 0;
                            coordinates[4] = 1;
                            coordinates[5] = 1;
                            break;

                        case 19:
                            coordinates[0] = 1;
                            coordinates[1] = 0;
                            coordinates[4] = 1;
                            coordinates[5] = 1;
                            break;

                        case 20:
                            coordinates[0] = -1;
                            coordinates[1] = 0;
                            coordinates[4] = -1;
                            coordinates[5] = -1;
                            break;

                        case 21:
                            coordinates[0] = 1;
                            coordinates[1] = 0;
                            coordinates[4] = -1;
                            coordinates[5] = -1;
                            break;

                        case 22:
                            coordinates[0] = -1;
                            coordinates[1] = 0;
                            coordinates[4] = 1;
                            coordinates[5] = -1;
                            break;

                        case 23:
                            coordinates[0] = 1;
                            coordinates[1] = 0;
                            coordinates[4] = 1;
                            coordinates[5] = -1;
                            break;
                    }

                    float output;
                    network.ClearSignals();
                    network.SetInputSignals(coordinates);
                    network.MultipleSteps(iterations);
                    output = network.GetOutputSignal(2);
                    gene.NeuronBias = output;
                    output = network.GetOutputSignal(3);
                    gene.TimeConstant = (output + 1) * 30 + 1; // normalize output to [1,61] for the time constant
                }
            }

            ConnectionGeneList connections = new ConnectionGeneList(88);           
            uint connectionCounter = 0;            

            // intramodule connections for first subunit
            coordinates[4] = -1;
            coordinates[5] = 1;
            coordinates[6] = -1;
            coordinates[7] = 1;
            addModule(network, iterations, coordinates, 0, connections, connectionCounter);            

            // intramodule connections for second subunit
            coordinates[4] = 1;
            coordinates[5] = 1;
            coordinates[6] = 1;
            coordinates[7] = 1;
            addModule(network, iterations, coordinates, 1, connections, connectionCounter);

            // intramodule connections for third subunit
            coordinates[4] = -1;
            coordinates[5] = -1;
            coordinates[6] = -1;
            coordinates[7] = -1;
            addModule(network, iterations, coordinates, 2, connections, connectionCounter);

            // intramodule connections for fourth subunit
            coordinates[4] = 1;
            coordinates[5] = -1;
            coordinates[6] = 1;
            coordinates[7] = -1;
            addModule(network, iterations, coordinates, 3, connections, connectionCounter);

            // intermodule connections
            // vertical connections
            coordinates[4] = -1;            
            coordinates[5] = -1;
            coordinates[6] = -1;
            coordinates[7] = 1;

            coordinates[0] = -1;
            coordinates[1] = 1;
            coordinates[2] = -1;
            coordinates[3] = 0;
            addConnection(network, iterations, 10, 16, coordinates, true, connections, connectionCounter);

            coordinates[0] = 0;
            coordinates[1] = 1;
            coordinates[2] = 0;
            coordinates[3] = -1;
            addConnection(network, iterations, 11, 0, coordinates, true, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 1;
            coordinates[2] = 1;
            coordinates[3] = 0;
            addConnection(network, iterations, 12, 17, coordinates, true, connections, connectionCounter);

            coordinates[4] = -1;
            coordinates[5] = 1;
            coordinates[6] = -1;
            coordinates[7] = -1;

            coordinates[0] = -1;
            coordinates[1] = 0;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 16, 10, coordinates, true, connections, connectionCounter);

            coordinates[0] = 0;
            coordinates[1] = -1;
            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 0, 11, coordinates, true, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 0;
            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 17, 12, coordinates, true, connections, connectionCounter);

            coordinates[4] = 1;
            coordinates[5] = -1;
            coordinates[6] = 1;
            coordinates[7] = 1;

            coordinates[0] = -1;
            coordinates[1] = 1;
            coordinates[2] = -1;
            coordinates[3] = 0;
            addConnection(network, iterations, 13, 18, coordinates, true, connections, connectionCounter);

            coordinates[0] = 0;
            coordinates[1] = 1;
            coordinates[2] = 0;
            coordinates[3] = -1;
            addConnection(network, iterations, 14, 1, coordinates, true, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 1;
            coordinates[2] = 1;
            coordinates[3] = 0;
            addConnection(network, iterations, 15, 19, coordinates, true, connections, connectionCounter);

            coordinates[4] = 1;
            coordinates[5] = 1;
            coordinates[6] = 1;
            coordinates[7] = -1;

            coordinates[0] = -1;
            coordinates[1] = 0;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 18, 13, coordinates, true, connections, connectionCounter);

            coordinates[0] = 0;
            coordinates[1] = -1;
            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 1, 14, coordinates, true, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 0;
            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 19, 15, coordinates, true, connections, connectionCounter);

            // horizonal connections
            coordinates[4] = -1;
            coordinates[5] = 1;
            coordinates[6] = 1;
            coordinates[7] = 1;

            coordinates[0] = 1;
            coordinates[1] = 0;
            coordinates[2] = -1;
            coordinates[3] = 0;
            addConnection(network, iterations, 17, 18, coordinates, true, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 1;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 6, 7, coordinates, true, connections, connectionCounter);

            coordinates[4] = 1;
            coordinates[5] = 1;
            coordinates[6] = -1;
            coordinates[7] = 1;

            coordinates[0] = -1;
            coordinates[1] = 0;
            coordinates[2] = 1;
            coordinates[3] = 0;
            addConnection(network, iterations, 18, 17, coordinates, true, connections, connectionCounter);

            coordinates[0] = -1;
            coordinates[1] = 1;
            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 7, 6, coordinates, true, connections, connectionCounter);

            coordinates[4] = -1;
            coordinates[5] = -1;
            coordinates[6] = 1;
            coordinates[7] = -1;

            coordinates[0] = 1;
            coordinates[1] = 0;
            coordinates[2] = -1;
            coordinates[3] = 0;
            addConnection(network, iterations, 21, 22, coordinates, true, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 1;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 12, 13, coordinates, true, connections, connectionCounter);

            coordinates[4] = 1;
            coordinates[5] = -1;
            coordinates[6] = -1;
            coordinates[7] = -1;

            coordinates[0] = -1;
            coordinates[1] = 0;
            coordinates[2] = 1;
            coordinates[3] = 0;
            addConnection(network, iterations, 22, 21, coordinates, true, connections, connectionCounter);

            coordinates[0] = -1;
            coordinates[1] = 1;
            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 13, 12, coordinates, true, connections, connectionCounter);

            return new SharpNeatLib.NeatGenome.NeatGenome(0, newNeurons, connections, (int)inputCount, (int)outputCount);
        }

        private void addModule(INetwork network, int iterations, double[] coordinates, uint moduleOffset, ConnectionGeneList connections, uint connectionCounter)
        {
            // from input
            coordinates[0] = 0;
            coordinates[1] = -1;
            coordinates[2] = -1;
            coordinates[3] = 0;
            addConnection(network, iterations, 0 + moduleOffset, 16 + moduleOffset*2, coordinates, false, connections, connectionCounter);

            coordinates[2] = 1;
            coordinates[3] = 0;
            addConnection(network, iterations, 0 + moduleOffset, 17 + moduleOffset * 2, coordinates, false, connections, connectionCounter);

            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 0 + moduleOffset, 4 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 0 + moduleOffset, 5 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 0 + moduleOffset, 6 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            // from first hidden
            coordinates[0] = -1;
            coordinates[1] = 0;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 16 + moduleOffset * 2, 4 + moduleOffset*3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 16 + moduleOffset * 2, 5 + moduleOffset*3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 16 + moduleOffset * 2, 6 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 1;
            coordinates[3] = 0;
            addConnection(network, iterations, 16 + moduleOffset * 2, 17 + moduleOffset * 2, coordinates, false, connections, connectionCounter);

            // from second hidden
            coordinates[0] = 1;
            coordinates[1] = 0;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 17 + moduleOffset * 2, 4 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 17 + moduleOffset * 2, 5 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 17 + moduleOffset * 2, 6 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[2] = -1;
            coordinates[3] = 0;
            addConnection(network, iterations, 17 + moduleOffset * 2, 16 + moduleOffset * 2, coordinates, false, connections, connectionCounter);

            // output to output connections
            coordinates[0] = -1;
            coordinates[1] = 1;
            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 4 + moduleOffset * 3, 5 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[0] = 0;
            coordinates[1] = 1;
            coordinates[2] = -1;
            coordinates[3] = 1;
            addConnection(network, iterations, 5 + moduleOffset * 3, 4 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[0] = 1;
            coordinates[1] = 1;
            coordinates[2] = 0;
            coordinates[3] = 1;
            addConnection(network, iterations, 6 + moduleOffset * 3, 5 + moduleOffset * 3, coordinates, false, connections, connectionCounter);

            coordinates[0] = 0;
            coordinates[1] = 1;
            coordinates[2] = 1;
            coordinates[3] = 1;
            addConnection(network, iterations, 5 + moduleOffset * 3, 6 + moduleOffset * 3, coordinates, false, connections, connectionCounter);
        }

        private void addConnection(INetwork network, int iterations, uint source, uint target, double[] coordinates, bool isInter, ConnectionGeneList connections, uint connectionCounter)
        {
            float output;
            network.ClearSignals();
            network.SetInputSignals(coordinates);
            network.MultipleSteps(iterations);
            if (isInter)
                output = network.GetOutputSignal(1);
            else
                output = network.GetOutputSignal(0);

            if (Math.Abs(output) > threshold)
            {
                float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
                connections.Add(new ConnectionGene(connectionCounter++, source, target, weight));
            }
        }

        // for predator prey, ignore
        public INetwork generateMultiNetwork(INetwork network, uint numberOfAgents)
        {
            return generateMultiGenomeModulus(network, numberOfAgents).Decode(activationFunction);
        }

        // for predator prey, ignore
        public NeatGenome generateMultiGenomeModulus(INetwork network, uint numberOfAgents)
        {
#if OUTPUT
            System.IO.StreamWriter sw = new System.IO.StreamWriter("testfile.txt");
#endif
            var coordinates = new double[4];
            float output;
            uint connectionCounter = 0;

            uint inputsPerAgent = inputCount / numberOfAgents;
            uint hiddenPerAgent = hiddenCount / numberOfAgents;
            uint outputsPerAgent = outputCount / numberOfAgents;

            ConnectionGeneList connections = new ConnectionGeneList((int)((inputCount * hiddenCount) + (hiddenCount * outputCount)));

            int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;

            coordinates[0] = -1 + inputDelta / 2.0f;    //x1
            coordinates[1] = -1;                        //y1 
            coordinates[2] = -1 + hiddenDelta / 2.0f;   //x2
            coordinates[3] = 0;                         //y2

            for (uint agent = 0; agent < numberOfAgents; agent++)
            {
                coordinates[0] = -1 + (agent * inputsPerAgent * inputDelta) + inputDelta / 2.0f;
                for (uint source = 0; source < inputsPerAgent; source++, coordinates[0] += inputDelta)
                {
                    coordinates[2] = -1 + (agent * hiddenPerAgent * hiddenDelta) + hiddenDelta / 2.0f;
                    for (uint target = 0; target < hiddenPerAgent; target++, coordinates[2] += hiddenDelta)
                    {

                        //Since there are an equal number of input and hidden nodes, we check these everytime
                        network.ClearSignals();
                        network.SetInputSignals(coordinates);
                        ((FloatFastConcurrentNetwork)network).MultipleStepsWithMod(iterations, (int)numberOfAgents);
                        output = network.GetOutputSignal(0);
#if OUTPUT
                            foreach (double d in inputs)
                                sw.Write(d + " ");
                            sw.Write(output);
                            sw.WriteLine();
#endif
                        if (Math.Abs(output) > threshold)
                        {
                            float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
                            connections.Add(new ConnectionGene(connectionCounter++, (agent * inputsPerAgent) + source, (agent * hiddenPerAgent) + target + inputCount + outputCount, weight));
                        }

                        //Since every other hidden node has a corresponding output node, we check every other time
                        if (target % 2 == 0)
                        {
                            network.ClearSignals();
                            coordinates[1] = 0;
                            coordinates[3] = 1;
                            network.SetInputSignals(coordinates);
                            ((FloatFastConcurrentNetwork)network).MultipleStepsWithMod(iterations, (int)numberOfAgents);
                            output = network.GetOutputSignal(0);
#if OUTPUT
                            foreach (double d in inputs)
                                sw.Write(d + " ");
                            sw.Write(output);
                            sw.WriteLine();
#endif
                            if (Math.Abs(output) > threshold)
                            {
                                float weight = (float)(((Math.Abs(output) - (threshold)) / (1 - threshold)) * weightRange * Math.Sign(output));
                                connections.Add(new ConnectionGene(connectionCounter++, (agent * hiddenPerAgent) + source + inputCount + outputCount, ((outputsPerAgent * agent) + ((target) / 2)) + inputCount, weight));
                            }
                            coordinates[1] = -1;
                            coordinates[3] = 0;

                        }
                    }
                }
            }
#if OUTPUT
            sw.Flush();
#endif
            //Console.WriteLine(count);
            //Console.ReadLine();
            return new SharpNeatLib.NeatGenome.NeatGenome(0, neurons, connections, (int)inputCount, (int)outputCount);
        }

    }
}
