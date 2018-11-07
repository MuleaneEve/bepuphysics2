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
    class SUPGSubstrate : Substrate
    {
        private const float shiftScale = 0.2f;

        public SUPGSubstrate(uint inputs, uint outputs, uint hidden, IActivationFunction function)
            : base(inputs, outputs, hidden, function)
        {

        }

        public override NeatGenome generateGenome(INetwork network)
        {
            // copy the neuron list to a new list and update the x/y values
            NeuronGeneList newNeurons = new NeuronGeneList(neurons);

            // set the x and y value of the SUPGs
            foreach (NeuronGene neuron in newNeurons)
            {
                if (neuron.NeuronType == NeuronType.Hidden)
                {
                    // switch to grid substrate configuration
                    neuron.XValue = getXPos2(neuron.InnovationId - 16);
                    neuron.YValue = getYPos2(neuron.InnovationId - 16);
                }
            }

            ConnectionGeneList connections = new ConnectionGeneList((int)((inputCount * hiddenCount) + (hiddenCount * outputCount)));
            float[] coordinates = new float[5];
            //float output;
            uint connectionCounter = 0;
            int iterations = 2 * (network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount)) + 1;            

            // connect hidden layer to outputs
            for (uint source = 0; source < hiddenCount; source++)
            {
                coordinates[0] = getXPos(source, false);
                coordinates[1] = getYPos(source, false);

                for (uint target = 0; target < outputCount; target++)
                {
                    // only connect hidden nodes to their single nearest output
                    if (source == target)
                    {
                        coordinates[2] = getXPos(target, true);
                        coordinates[3] = getYPos(target, true);

                        // GWM - fixing weight to 1 for SUPG producing motor outputs
                        connections.Add(new ConnectionGene(connectionCounter++, source + inputCount + outputCount, target + inputCount, 1));
                    }
                }
            }

            return new SharpNeatLib.NeatGenome.NeatGenome(0, newNeurons, connections, (int)inputCount, (int)outputCount);
        }

        private float getXPos(uint index, bool isOutput)
        {
            float pos = 0;
            float shift = shiftScale;
            if (isOutput)
                shift *= 2;

            switch (index)
            {
                case 0:
                case 6:
                    pos = -1;
                    break;
                case 1:
                case 2:
                case 7:
                case 8:
                    pos = -1 + shift;
                    break;
                case 3:
                case 4:
                case 9:
                case 10:
                    pos = 1 - shift;
                    break;
                case 5:
                case 11:
                    pos = 1;
                    break;
            }
            return pos;
        }

        private float getYPos(uint index, bool isOutput)
        {
            float pos = 0;
            float shift = shiftScale;
            if (isOutput)
                shift *= 2;

            switch (index)
            {
                case 2:
                case 3:
                    pos = 1;
                    break;
                case 6:
                case 7:
                case 10:
                case 11:
                    pos = -1 + shift;
                    break;
                case 0:
                case 1:
                case 4:
                case 5:
                    pos = 1 - shift;
                    break;
                case 8:
                case 9:
                    pos = -1;
                    break;
            }
            return pos;
        }

        private float getXPos2(uint index)
        {
            float pos = 0;
            switch (index)
            {
                case 0:
                case 1:
                case 2:
                    pos = -1;
                    break;
                case 3:
                case 4:
                case 5:
                    pos = -.33f;
                    break;
                case 6:
                case 7:
                case 8:
                    pos = .33f;
                    break;
                case 9:
                case 10:
                case 11:
                    pos = 1;
                    break;

            }
            return pos;
        }

        private float getYPos2(uint index)
        {
            float pos = 0;
            switch (index)
            {
                case 0:
                case 5:
                case 6:
                case 11:
                    pos = -1;
                    break;
                case 2:
                case 3:
                case 8:
                case 9:
                    pos = 0;
                    break;
                case 1:
                case 4:
                case 7:
                case 10:
                    pos = 1;
                    break;

            }
            return pos;
        }

        // returns a map that signifies which trigger maps to which hidden neurons.. a value of float.min means that neuron has no trigger
        // any other value indicates the foot which triggers that given neuron.  example: map[16] = 0 means foot 0 triggers neuron 16
        public int[] getSUPGMap()
        {
            int[] map = new int[28];
            for (int i = 0; i < 16; i++)
                map[i] = int.MinValue;
            map[16] = 0;
            map[17] = 0;
            map[18] = 0;
            map[19] = 1;
            map[20] = 1;
            map[21] = 1;
            map[22] = 2;
            map[23] = 2;
            map[24] = 2;
            map[25] = 3;
            map[26] = 3;
            map[27] = 3;
            return map;
        }

        
    }
}
