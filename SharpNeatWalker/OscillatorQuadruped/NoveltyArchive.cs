using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

namespace OscillatorQuadruped
{
    class NoveltyArchive
    {
        private double novelThreshold = 1000;
        private int k = 15;
        private ArrayList noveltyBank;
        private static Semaphore sem = new Semaphore(1, 1);

        // threshold adjustment variables
        private int itemsAdded = 0;
        private int gensSinceLastAddition = 0;

        public NoveltyArchive()
        {
            noveltyBank = new ArrayList();
        }

        public float calcFitness(float[] behavior)
        {
            
            float fitness = 0;

            
            float[] neighbors = new float[k];
            for (int i = 0; i < k; i++)
            {
                neighbors[i] = float.MaxValue;
            }
            
            // make a local copy of novelty bank
            sem.WaitOne();
            ArrayList tempBank = new ArrayList(noveltyBank);
            sem.Release();

            // find the distance to the k nearest neighbors
            foreach(float[] candidate in tempBank)
            {
                float distance = calcDistance(behavior, candidate);
                int j = 0;
                while (j < k)
                {
                    if (distance < neighbors[j])
                    {
                        float temp = neighbors[j];
                        neighbors[j] = distance;
                        distance = temp;
                        j = -1;
                    }
                    j++;
                }
            }
            
            for (int i = 0; i < k; i++)
            {
                if (neighbors[i] == float.MaxValue)
                    neighbors[i] = 0;

                fitness += neighbors[i];
            }
            
            // if the fitness is above the threshold, or we don't yet have k behaviors in the bank, add this individual to the novelty bank            
            if (fitness > novelThreshold || noveltyBank.Count < k)
            {
                sem.WaitOne();
                noveltyBank.Add(behavior);
                sem.Release();

                if (fitness > novelThreshold)
                    itemsAdded++;                
            }                         
             
            return fitness;
        }

        private float calcDistance(float[] a, float[] b)
        {
            float distance = 0;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                distance += (float) Math.Pow(a[i] - b[i], 2);
            }
            return distance;
        }

        public void endOfGeneration()
        {
            // if more than 4 items have been added, raise the bar
            if (itemsAdded > 4)
                novelThreshold *= 1.2;

            // if no items have been added in 4 generations or longer, lower the bar
            if (itemsAdded == 0)
            {
                gensSinceLastAddition++;
                if (gensSinceLastAddition > 3)
                    novelThreshold *= .8;
            }
            else
                gensSinceLastAddition = 0;

            // reset items added
            itemsAdded = 0;
        }
    }
}
