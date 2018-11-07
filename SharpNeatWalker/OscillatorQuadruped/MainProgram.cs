using System;
using System.IO;
using System.Xml;
using SharpNeatLib.CPPNs;
using SharpNeatLib.Evolution;
using SharpNeatLib.Experiments;
using SharpNeatLib.NeatGenome;
using SharpNeatLib.NeatGenome.Xml;
using SharpNeatLib.NeuralNetwork;

namespace OscillatorQuadruped
{
    class MainProgram
    {
        private string folder = Directory.GetCurrentDirectory() + "\\logfiles\\";

        public static bool doClune = false;
        public static bool novelty = false;

        public MainProgram()
        {
            
        }

        public void run(int type, int evaluationMethod, System.Threading.CancellationToken token)
        {      
            double maxFitness = 0;
            int maxGenerations = 800;
            int populationSize = 300;
            
            IExperiment exp;

            if (evaluationMethod == 1)
                novelty = true;

            if (type == 0)
                exp = new CTRNNExperiment(4, 12, 8, 8, 4);
            else if (type == 1)
                exp = new SUPGExperiment(4, 12, 12, 3, 2);
            else
            {
                doClune = true;
                exp = new CluneExperiment(20, 20, 20, 6, 1);
            }

            XmlDocument doc;
            FileInfo oFileInfo;
            IdGenerator idgen;
            EvolutionAlgorithm ea;
            NeatGenome seedGenome = null;

            if (seedGenome == null)
            {
                idgen = new IdGenerator();
                ea = new EvolutionAlgorithm(new Population(idgen, GenomeFactory.CreateGenomeList(exp.DefaultNeatParameters, idgen, exp.InputNeuronCount, exp.OutputNeuronCount, exp.DefaultNeatParameters.pInitialPopulationInterconnections, populationSize)), exp.PopulationEvaluator, exp.DefaultNeatParameters);                
            }
            else
            {
                idgen = new IdGeneratorFactory().CreateIdGenerator(seedGenome);
                ea = new EvolutionAlgorithm(new Population(idgen, GenomeFactory.CreateGenomeList(seedGenome, populationSize, exp.DefaultNeatParameters, idgen)), exp.PopulationEvaluator, exp.DefaultNeatParameters);
            }
            Directory.CreateDirectory(folder);
            using (var logWriter = File.CreateText(folder + "Log " + DateTime.Now.ToString("u").Replace(':', '.') + ".txt"))
                for (int j = 0; j < maxGenerations; j++)
                {
                    if (token.IsCancellationRequested)
                    {
                        logWriter.WriteLine("Cancelled");
                        break;
                    }

                    DateTime dt = DateTime.Now;
                    ea.PerformOneGeneration();

                    if (ea.BestGenome.ObjectiveFitness > maxFitness)
                    {
                        maxFitness = ea.BestGenome.ObjectiveFitness;
                        doc = new XmlDocument();
                        XmlGenomeWriterStatic.Write(doc, (NeatGenome)ea.BestGenome);
                        oFileInfo = new FileInfo(folder + "bestGenome" + j + ".xml");
                        doc.Save(oFileInfo.FullName);

                        /*/ This will output the substrate
                        doc = new XmlDocument();
                        XmlGenomeWriterStatic.Write(doc, SUPGNetworkEvaluator.substrate.generateGenome(ea.BestGenome.Decode(null)));
                        oFileInfo = new FileInfo(folder + "bestNetwork" + j + ".xml");
                        doc.Save(oFileInfo.FullName);*/
                    }
                    var msg = DateTime.Now.ToLongTimeString()
                        + "; Duration=" + DateTime.Now.Subtract(dt).ToString("mm\\:ss")
                        + "; Gen=" + ea.Generation.ToString("000") + "; Neurons=" + (ea.Population.TotalNeuronCount / (float)ea.Population.GenomeList.Count).ToString("00.00") + "; Connections=" + (ea.Population.TotalConnectionCount / (float)ea.Population.GenomeList.Count).ToString("00.00")
                        + "; BestFit=" + ea.BestGenome.ObjectiveFitness.ToString("0.000") + "; MaxFit=" + maxFitness.ToString("0.000");
                    Console.WriteLine(msg);
                    logWriter.WriteLine(msg);
                    logWriter.Flush();
                    //Do any post-hoc stuff here
                }

            doc = new XmlDocument();
            XmlGenomeWriterStatic.Write(doc, (NeatGenome)ea.BestGenome, ActivationFunctionFactory.GetActivationFunction("NullFn"));
            oFileInfo = new FileInfo(folder + "bestGenome.xml");
            doc.Save(oFileInfo.FullName);
        }

#if TODO // TODO: OscillatorQuadruped
        public void showMovie(string genomeFile, int type)
        {
            if (true) // set to false to use hardcoded output values from a file
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(genomeFile);
                NeatGenome genome = XmlNeatGenomeReaderStatic.Read(doc);

                INetwork tempNet = null;
                INetwork cppn = null;
                NeatGenome tempGenome = null;

                Substrate substrate;

                if (type == 0)
                    substrate = new CTRNNSubstrate(4, 12, 8, HyperNEATParameters.substrateActivationFunction);
                else if (type == 1)
                    substrate = new SUPGSubstrate(4, 12, 12, HyperNEATParameters.substrateActivationFunction);
                else
                {
                    doClune = true;
                    substrate = new CluneSubstrate(20, 20, 20, HyperNEATParameters.substrateActivationFunction);
                }

                cppn = genome.Decode(null);
                tempGenome = substrate.generateGenome(cppn);

                tempNet = tempGenome.Decode(null);

                Controller controller;
                if (type == 0)
                    controller = new Controller(tempNet);
                else if (type == 1)
                    controller = new Controller(tempNet, true, tempGenome, cppn, ((SUPGSubstrate)substrate).getSUPGMap());
                else
                    controller = new Controller(tempNet);


                using (var domain = new Domain())
                {
                    domain.Initialize(controller);
                    domain.RunDraw();
                }
            }
            else
            {
                using (var domain = new Domain())
                {
                    domain.Initialize();
                    domain.RunDraw();
                }
            }
        }

        public void calcFitness(string genomeFile, int type)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(genomeFile);
            NeatGenome genome = XmlNeatGenomeReaderStatic.Read(doc);
            
            INetworkEvaluator eval;
            if (type == 0)
                eval = new CTRNNNetworkEvaluator(4, 12, 12);
            else if (type == 1)
                eval = new SUPGNetworkEvaluator(4, 12, 12);
            else
            {
                doClune = true;
                eval = new CluneNetworkEvaluator(20, 20, 20);
            }

            var tempNet = genome.Decode(null);
            MessageBox.Show(eval.threadSafeEvaluateNetwork(tempNet)[0].ToString());          
        }
#endif
    }
}