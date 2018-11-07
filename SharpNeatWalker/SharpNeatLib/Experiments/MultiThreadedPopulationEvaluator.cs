using System;
using System.Threading;
using SharpNeatLib.Evolution;
using SharpNeatLib.NeuralNetwork;

namespace SharpNeatLib.Experiments
{
    /// <summary>
    /// An implementation of IPopulationEvaluator that evaluates all new genomes(EvaluationCount==0)
    /// within the population using multiple threads, using an INetworkEvaluator provided at construction time.
    /// 
    /// This class provides an IPopulationEvaluator for use within the EvolutionAlgorithm by simply
    /// providing an INetworkEvaluator to its constructor. This usage is intended for experiments
    /// where the genomes are evaluated independently of each other (e.g. not simultaneoulsy in 
    /// a simulated world) using a fixed evaluation function that can be described by an INetworkEvaluator.
    /// </summary>
    public class MultiThreadedPopulationEvaluator : IPopulationEvaluator
    {
        private readonly INetworkEvaluator _networkEvaluator;
        private readonly IActivationFunction _activationFn;
        private static readonly Semaphore Sem = new Semaphore(HyperNEATParameters.numThreads, HyperNEATParameters.numThreads);
        private ulong _evaluationCount;

        #region Constructor

        public MultiThreadedPopulationEvaluator(INetworkEvaluator networkEvaluator, IActivationFunction activationFn)
        {
            _networkEvaluator = networkEvaluator;
            _activationFn = activationFn;
        }

        #endregion

        #region IPopulationEvaluator Members

        public void EvaluatePopulation(Population pop, EvolutionAlgorithm ea)
        {
            int count = pop.GenomeList.Count;

            for (var i = 0; i < count; i++)
            {
                Sem.WaitOne();
                var g = pop.GenomeList[i];
                var e = new EvalPack(_networkEvaluator, _activationFn, g);

                ThreadPool.QueueUserWorkItem(EvalNet, e);

                // Update master evaluation counter.
                _evaluationCount++;
            }
            
            for (int j = 0; j < HyperNEATParameters.numThreads; j++)
                Sem.WaitOne();
            for (int j = 0; j < HyperNEATParameters.numThreads; j++)
                Sem.Release();

            _networkEvaluator.endOfGeneration(); // GWM - Update novelty stuff
        }


        public ulong EvaluationCount
        {
            get
            {
                return _evaluationCount;
            }
        }

        public string EvaluatorStateMessage
        {
            get
            {	// Pass on the network evaluator's message.
                return _networkEvaluator.EvaluatorStateMessage;
            }
        }

        public bool BestIsIntermediateChampion
        {
            get
            {	// Only relevant to incremental evolution experiments.
                return false;
            }
        }

        public bool SearchCompleted
        {
            get
            {	// This flag is not yet supported in the main search algorithm.
                return false;
            }
        }

        internal static void EvalNet(object input)
        {
            try
            {
                var e = (EvalPack)input;
                var g = e.Genome;
                if (g == null)//|| g.EvaluationCount != 0)
                    return;
                INetwork network = g.Decode(e.ActivationFn);
                if (network == null)
                {   // Future genomes may not decode - handle the possibility.
                    g.Fitness = EvolutionAlgorithm.MIN_GENOME_FITNESS;
                }
                else
                {
                    double[] fitnesses = e.NetworkEvaluator.threadSafeEvaluateNetwork(network);
                    g.Fitness = Math.Max(fitnesses[0], EvolutionAlgorithm.MIN_GENOME_FITNESS);
                    g.ObjectiveFitness = fitnesses[1];
                }

                // Reset these genome level statistics.
                g.TotalFitness += g.Fitness;
                g.EvaluationCount += 1;
            }
            //catch (Exception ex) { System.Diagnostics.Debug.WriteLine("EvalNet failed: " + ex); } // Catch? Stop?
            finally { Sem.Release(); }
        }

        #endregion
    }

    internal class EvalPack
    {
        public readonly INetworkEvaluator NetworkEvaluator;
        public readonly IActivationFunction ActivationFn;
        public readonly IGenome Genome;

        public EvalPack(INetworkEvaluator n, IActivationFunction a, IGenome g)
        {
            NetworkEvaluator = n;
            ActivationFn = a;
            Genome = g;
        }
    }
}
