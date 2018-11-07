using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpNeatLib.Experiments;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;

namespace OscillatorQuadruped
{
    class SUPGExperiment : IExperiment
    {
        private uint inputs;
        private uint outputs;
        private uint hidden;
        private int cppnInputs;
        private int cppnOutputs;
        private IPopulationEvaluator populationEvaluator = null;
        private NeatParameters neatParams = null;

        public SUPGExperiment(uint inputs, uint outputs, uint hidden, int cppnInputs, int cppnOutputs)
        {
            this.inputs = inputs;
            this.outputs = outputs;
            this.hidden = hidden;
            this.cppnInputs = cppnInputs;
            this.cppnOutputs = cppnOutputs;
        }

        #region IExperiment Members

        public void LoadExperimentParameters(System.Collections.Hashtable parameterTable)
        {
            //throw new Exception("The method or operation is not implemented.");
        }

        public IPopulationEvaluator PopulationEvaluator
        {
            get
            {
                if (populationEvaluator == null)
                    ResetEvaluator(HyperNEATParameters.substrateActivationFunction);

                return populationEvaluator;
            }
        }

        public void ResetEvaluator(IActivationFunction activationFn)
        {
            populationEvaluator = new SUPGPopulationEvaluator(new SUPGNetworkEvaluator(inputs, outputs, hidden));
        }

        public int InputNeuronCount
        {
            get { return cppnInputs; }
        }

        public int OutputNeuronCount
        {
            get { return cppnOutputs; }
        }

        public NeatParameters DefaultNeatParameters
        {
            get
            {
                if (neatParams == null)
                {
                    NeatParameters np = new NeatParameters();
                    np.activationProbabilities = new double[4];
                    np.activationProbabilities[0] = .25;
                    np.activationProbabilities[1] = .25;
                    np.activationProbabilities[2] = .25;
                    np.activationProbabilities[3] = .25;
                    np.compatibilityDisjointCoeff = 1;
                    np.compatibilityExcessCoeff = 1;
                    np.compatibilityThreshold = 100;
                    np.compatibilityWeightDeltaCoeff = 3;
                    np.connectionWeightRange = 3;
                    np.elitismProportion = .1;
                    np.pInitialPopulationInterconnections = 1;
                    np.pInterspeciesMating = 0.01;
                    np.pMutateAddConnection = .06;
                    np.pMutateAddNode = .01;
                    np.pMutateConnectionWeights = .96;
                    np.pMutateDeleteConnection = 0;
                    np.pMutateDeleteSimpleNeuron = 0;                  
                    np.populationSize = 300;
                    np.pruningPhaseBeginComplexityThreshold = float.MaxValue;
                    np.pruningPhaseBeginFitnessStagnationThreshold = int.MaxValue;
                    np.pruningPhaseEndComplexityStagnationThreshold = int.MinValue;
                    np.selectionProportion = .8;
                    np.speciesDropoffAge = 1500;
                    np.targetSpeciesCountMax = np.populationSize / 10;
                    np.targetSpeciesCountMin = np.populationSize / 10 - 2;
                                        
                    neatParams = np;
                }
                return neatParams;
            }
        }

        public IActivationFunction SuggestedActivationFunction
        {
            get { return HyperNEATParameters.substrateActivationFunction; }
        }

        public AbstractExperimentView CreateExperimentView()
        {
            return null;
        }

        public string ExplanatoryText
        {
            get { return "A HyperNEAT experiemnt for quadruped locomotion"; }
        }

        #endregion
    }
}
