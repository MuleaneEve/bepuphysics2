using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpNeatLib.NeuralNetwork;
using SharpNeatLib.NeatGenome;

namespace OscillatorQuadruped
{
    class Controller
    {
        private bool useFile = false;
        //private static bool scale = true;
        private StreamReader reader;
        private float[] outputs;
        private int outputindex = 0;
        private INetwork network;
        private Boolean scale = true;
        private Boolean useSUPG;
        private NeatGenome genome;
        private INetwork cppn;
        private int[] triggerMap;

        private static int wavelength = 100;  // SUPG wavelength
        private static int compression = 50;

        private string folder = Directory.GetCurrentDirectory() + "\\logfiles\\";
        private StreamWriter SW;

        // arrays added to cache CPPN outputs for SUPG activation
        private float[,] supgOutputs;

        private bool kickstart = true;

        // This constructor should only be used when returning output values from a data file instead of from a network
        public Controller(bool useFile = false)
        {
            this.useFile = useFile;
            if (useFile)
            {
                reader = new StreamReader("C:\\Users\\Greg\\Desktop\\various\\school\\OscillatorQuadruped\\output files\\outs-10.104702-6.868093.txt");
                outputs = new float[18000];
                string sLine = "1";
                int i = 0;
                while (sLine != null && i < 18000)
                {
                    sLine = reader.ReadLine();
                    if (sLine != null)
                        outputs[i] = float.Parse(sLine);
                    i++;
                }
                reader.Close();
            }
        }

        // all of the optional parameters only need to be entered when using SUPG architecture
        public Controller(INetwork network, bool useSUPG = false, NeatGenome genome = null, INetwork cppn = null, int[] triggerMap = null)
        {            
            //SW = File.CreateText(folder + "triggers.txt");

            if (useSUPG)
            {
                supgOutputs = new float[network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount), wavelength]; // need at least as many rows as the number of hidden neurons
                // set all supgOutputs to min value to signal they have not been cached yet
                for (int i = 0; i < network.TotalNeuronCount - (network.InputNeuronCount + network.OutputNeuronCount); i++)
                    for (int j = 0; j < wavelength; j++)
                        supgOutputs[i, j] = float.MinValue;
            }

            this.network = network;
            this.useSUPG = useSUPG;
            this.genome = genome;
            this.cppn = cppn;
            this.triggerMap = triggerMap;
            if (useSUPG)
                ((FloatFastConcurrentNetwork)network).UseSUPG = true;            
        }      

        public void update(double[] sensors, float[] triggers)
        {
            if (SW != null && false)
            {
                SW.WriteLine(triggers[0] + "," + triggers[1] + "," + triggers[2] + "," + triggers[3]);
                SW.Flush();
            }

            if (network != null)
            {
                int iterations = 17; 
                
                network.ClearSignals();
                if (useSUPG)
                {
                    int cppnIterations = 2 * (cppn.TotalNeuronCount - (cppn.InputNeuronCount + cppn.OutputNeuronCount)) + 1;
                    // kickstart by setting leg timers to 1, w/2, w/2, 1
                    if (kickstart)
                    {
                        kickstart = false;
                        // set triggers to 0
                        triggers = new float[triggers.Length];

                        // set time counters to the kickstart values
                        foreach (NeuronGene neuron in genome.NeuronGeneList)
                        {
                            // get offset value from 2nd cppn output
                            if (neuron.InnovationId >= 16 && neuron.InnovationId <= 18)
                                neuron.TimeCounter = getOffset(1, cppnIterations, neuron);
                            if (neuron.InnovationId >= 19 && neuron.InnovationId <= 21)
                                neuron.TimeCounter = getOffset(2, cppnIterations, neuron);
                            if (neuron.InnovationId >= 22 && neuron.InnovationId <= 24)
                                neuron.TimeCounter = getOffset(3, cppnIterations, neuron);
                            if (neuron.InnovationId >= 25 && neuron.InnovationId <= 27)
                                neuron.TimeCounter = getOffset(4, cppnIterations, neuron);
                        }
                    }
                    
                    // set up the override array
                    float[] overrideSignals = new float[network.TotalNeuronCount];
                    for (int i = 0; i < overrideSignals.Length; i++)
                        overrideSignals[i] = float.MinValue;
                    
                    // update the SUPGs
                    foreach (NeuronGene neuron in genome.NeuronGeneList)
                    {
                        /* code for triggers */
                        // increment the time counter of any SUPG that is currently running

                        if (neuron.TimeCounter > 0)
                        {
                            neuron.TimeCounter = (neuron.TimeCounter + 1) % wavelength;
                            // if the time counter finished and went back to zero, the first step is complete
                            if (neuron.TimeCounter == 0)
                                neuron.FirstStepComplete = true;
                        }

                        // check if the neuron is a triggered neuron
                        if (triggerMap[neuron.InnovationId] != int.MinValue)
                        {
                            // check the trigger
                            if (triggers[triggerMap[neuron.InnovationId]] == 1)
                            {
                                // if the time counter was non zero, then the first step has been completed
                                if (neuron.TimeCounter > 0)
                                    neuron.FirstStepComplete = true;

                                // set the neuron's time to 1
                                neuron.TimeCounter = 1;
                            }
                        } 
                    }

                    // determine the proper outputs of the SUPGs and send the override array to the network
                    
                    foreach (NeuronGene neuron in genome.NeuronGeneList)
                    {
                        if (neuron.TimeCounter > 0)  // only need to check neurons whose time counter is non zero
                        {
                            overrideSignals[neuron.InnovationId] = getSUPGActivation(neuron, cppnIterations);
                        }
                    }
                    ((FloatFastConcurrentNetwork)network).OverrideSignals = overrideSignals;
                }
                else
                {
                    network.SetInputSignals(sensors);
                }
                network.MultipleSteps(iterations);
                
            }
        }

        private float getSUPGActivation(NeuronGene neuron, int cppnIterations)
        {
            float activation = 0;
            int offset = network.InputNeuronCount + network.OutputNeuronCount; // assume that SUPGs are placed at front of hidden neurons
            // if the element is float.min, then we have not yet cached the SUPG output
            if (supgOutputs[neuron.InnovationId - offset, neuron.TimeCounter] == float.MinValue)
            {
                var coordinates = new double[3];
                

                coordinates[0] = neuron.XValue;                
                coordinates[1] = neuron.YValue;
                
               
                coordinates[0] = coordinates[0] / compression;
                      
                coordinates[2] = (float)neuron.TimeCounter / wavelength;

                cppn.ClearSignals();
                cppn.SetInputSignals(coordinates);
                cppn.MultipleSteps(cppnIterations);
                
                if (neuron.FirstStepComplete)
                {
                    activation = cppn.GetOutputSignal(0);
                    supgOutputs[neuron.InnovationId - offset, neuron.TimeCounter] = activation;  // only cache the output if the first step is complete
                }
                else
                    activation = cppn.GetOutputSignal(0);
                
            }
            else
            {
                // get the cached value
                activation = supgOutputs[neuron.InnovationId - offset, neuron.TimeCounter];
            }

            return activation;
        }

        private int getOffset(int leg, int cppnIterations, NeuronGene neuron)
        {
            int offset = 0;            
            var coordinates = new double[3];
            coordinates[0] = neuron.XValue / compression;            
            cppn.ClearSignals();
            cppn.SetInputSignals(coordinates);
            cppn.MultipleSteps(cppnIterations);
            float activation = cppn.GetOutputSignal(1);
            offset = (int)Math.Ceiling((activation + 1) * wavelength / 2);
            if (offset <= 0)
                offset = 1;
            if (offset >= wavelength)
                offset = wavelength - 1;
            
            return offset;
        }

        public float[] getOutputs()
        {
            float[] outs = new float[12];
            if (useFile)
            {                                
                for (int i = 0; i < 12; i++)
                {
                    outs[i] = outputs[outputindex];
                    outputindex++;
                    if (outputindex > 17999)
                        outputindex = 17999;
                }
            }
            else
            {
                if (MainProgram.doClune)
                    for (int i = 0; i < 12; i++)
                        outs[i] = network.GetOutputSignal(i / 3 * 5 + (i % 3));
                else
                    for (int i = 0; i < 12; i++)
                    {
                        outs[i] = network.GetOutputSignal(i);
                        if (useSUPG)
                        {
                            // with the SUPG architecture, we need the outputs to be normalized between 0 and 1
                            // because the CPPN uses bipolar sigmoid for all outputs, which increases the range to -1, 1
                            // when SUPGs start becoming true hidden nodes, we can remove this modification
                            outs[i] = (outs[i] + 1) / 2;
                        }                                                              
                    }
            }
            return outs;
        }

        public void cleanup()
        {
            //if (SW != null)
                //SW.Close();
        }

        public bool Scale
        {
            get
            {
                return scale;
            }
        }
    }
}
