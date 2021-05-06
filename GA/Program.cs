using System;

namespace GA
{
    class Program
    {
        static void Main(string[] args)
        {
            var population = new Population(6);
            var geneticAlgorythm = new GeneticAlgorithm(population, FitnessFunction, -3, 1, 0, 3, 1);

            geneticAlgorythm.Execute();
        }

        static float FitnessFunction(float x, float y)
        {
            return MathF.Pow(x - y, 2) * MathF.Exp(x);
        }
    }
}
