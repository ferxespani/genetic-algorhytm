using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GA
{
    public class GeneticAlgorithm
    {
        private readonly Population _population;
        private readonly Func<float, float, float> _fitnessFunction;

        private float hx = 0;
        private float hy = 0;

        private float Lx = 0;
        private float Ly = 0;

        private float a = 0;
        private float b = 0;
        private float c = 0;
        private float d = 0;
        private float q = 0;

        private double max = 0;

        public GeneticAlgorithm(Population population, Func<float, float, float> fitnessFunction, float a, float b, float c, float d, float q)
        {
            _population = population;
            _fitnessFunction = fitnessFunction;

            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.q = q;
        }

        public void Execute()
        {
            Initialize();

            Encode();

            for (int i = 0; i < 10000; i++)
            {
                SelectIndividuals();

                CrossOver();

                Mutate();

                Decode();

                if (max < _population.FitnessFunctionSum / _population.Count)
                {
                    max = _population.FitnessFunctionSum / _population.Count;
                }
                Console.WriteLine(_population.FitnessFunctionSum / _population.Count);
            }

            Console.WriteLine(max);
        }

        private void Initialize()
        {
            Lx = MathF.Ceiling(MathF.Log2((b - a) * MathF.Pow(10, q) + 1));
            Ly = MathF.Ceiling(MathF.Log2((d - c) * MathF.Pow(10, q) + 1));

            hx = (b - a) / (MathF.Pow(2, Lx) - 1);
            hy = (d - c) / (MathF.Pow(2, Ly) - 1);

            var points = new List<Vector2>();
            float fitnessFunctionsSum = 0;

            float stepX = (b - a) / 4f;
            float stepY = (d - c) / 3f;

            points.Add(new Vector2(a + stepX, c + stepY));

            for (int i = 1; i <= _population.Count - 1; i++)
            {
                var point = points[i - 1];

                if (i % 3 == 0)
                {
                    points.Add(new Vector2(points[0].X, point.Y + stepY));
                    continue;
                }

                points.Add(new Vector2(point.X + stepX, point.Y));
            }

            for (int i = 0; i < points.Count; i++)
            {
                var chromosomeX = new Chromosome { Parameter = points[i].X };
                var chromosomeY = new Chromosome { Parameter = points[i].Y };

                var fitnessFunctionValue = _fitnessFunction(chromosomeX.Parameter, chromosomeY.Parameter);
                fitnessFunctionsSum += fitnessFunctionValue;

                var individual = new Individual
                { 
                    Сhromosomes = new List<Chromosome> { chromosomeX, chromosomeY } ,
                    FitnessFunctionValue = fitnessFunctionValue
                };

                _population.Individuals.Add(individual);
            }

            _population.FitnessFunctionSum = fitnessFunctionsSum;

            if (CheckIfExistsFitnessFunctionValueLessZero())
            {
                RecalculateFitnessFunctionValues();
            }

            max = _population.FitnessFunctionSum / _population.Count;
            Console.WriteLine(_population.FitnessFunctionSum / _population.Count);
        }

        private void Encode()
        {
            for (int i = 0; i < _population.Count; i++)
            {
                var x = _population.Individuals[i].Сhromosomes[0].Parameter;
                var y = _population.Individuals[i].Сhromosomes[1].Parameter;

                var xStar = MathF.Ceiling((x - a) / hx);
                var yStar = MathF.Ceiling((y - c) / hy);

                var binarySequenceX = ToBinary(xStar, (int)Lx);
                var binarySequenceY = ToBinary(yStar, (int)Ly);

                _population.Individuals[i].Сhromosomes[0].BinarySequence = binarySequenceX;
                _population.Individuals[i].Сhromosomes[1].BinarySequence = binarySequenceY;
            }
        }

        private void Decode()
        {
            float fitnessFunctionSum = 0;

            for (int i = 0; i < _population.Count; i++)
            {
                var binarySequenceX = _population.Individuals[i].Сhromosomes[0].BinarySequence;
                var binarySequenceY = _population.Individuals[i].Сhromosomes[1].BinarySequence;

                var xStar = FromBinary(binarySequenceX);
                var yStar = FromBinary(binarySequenceY);

                var x = a + hx * xStar;
                var y = c + hy * yStar;

                var fitnessFunctionValue = _fitnessFunction(x, y);
                _population.Individuals[i].FitnessFunctionValue = fitnessFunctionValue;
                fitnessFunctionSum += fitnessFunctionValue;
            }

            _population.FitnessFunctionSum = fitnessFunctionSum;
        }

        private void SelectIndividuals()
        {
            var random = new Random();
            var rule = new List<float>();
            var dict = new Dictionary<float, int>();

            rule.Add(0);

            for (int i = 0; i < _population.Count; i++)
            {
                var eta = _population.Individuals[i].FitnessFunctionValue / _population.FitnessFunctionSum * 100;

                rule.Add(rule[i] + eta);
            }

            for (int i = 0; i < _population.Count; i++)
            {
                var randomNumber = (float)random.NextDouble() * 100;

                var index = FindNumberIndex(rule, randomNumber);

                dict[randomNumber] = index;
            }

            var sortedDict = dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            for (int i = 0; i < _population.Count; i++)
            {
                var individual = _population.Individuals[sortedDict.ElementAt(i).Value - 1];
                _population.Individuals[i] = individual;
            }
        }

        private void CrossOver()
        {
            for (int i = 0; i < _population.Count; i += 2)
            {
                var randomNumber = (float)new Random().NextDouble();
                if (randomNumber > 0.9)
                {
                    continue;
                }

                var binarySequenceChromosomeX1 = _population.Individuals[i].Сhromosomes[0].BinarySequence;
                var binarySequenceChromosomeX2 = _population.Individuals[i + 1].Сhromosomes[0].BinarySequence;
                var binarySequenceChromosomeY1 = _population.Individuals[i].Сhromosomes[1].BinarySequence;
                var binarySequenceChromosomeY2 = _population.Individuals[i + 1].Сhromosomes[1].BinarySequence;

                for (int j = 2; j < Lx; j++)
                {
                    var temp = binarySequenceChromosomeX1[j];
                    binarySequenceChromosomeX1[j] = binarySequenceChromosomeX2[j];
                    binarySequenceChromosomeX2[j] = temp;
                }

                for (int j = 2; j < Ly; j++)
                {
                    var temp = binarySequenceChromosomeY1[j];
                    binarySequenceChromosomeY1[j] = binarySequenceChromosomeY2[j];
                    binarySequenceChromosomeY2[j] = temp;
                }

                _population.Individuals[i].Сhromosomes[0].BinarySequence = binarySequenceChromosomeX1;
                _population.Individuals[i + 1].Сhromosomes[0].BinarySequence = binarySequenceChromosomeX2;
                _population.Individuals[i].Сhromosomes[1].BinarySequence = binarySequenceChromosomeY1;
                _population.Individuals[i + 1].Сhromosomes[1].BinarySequence = binarySequenceChromosomeY2;
            }
        }

        private void Mutate()
        {
            var randomNumber = (float)new Random().NextDouble();

            if (randomNumber < 0.05)
            {
                var randomIndividual = new Random().Next(0, 6);
                var randomChromosome = new Random().Next(0, 2);
                int randomIndex = 0;
                if (randomChromosome == 0)
                {
                    randomIndex = new Random().Next(0, (int)Lx);
                }
                else
                {
                    randomIndex = new Random().Next(0, (int)Ly);
                }

                var value = _population.Individuals[randomIndividual].Сhromosomes[randomChromosome].BinarySequence[randomIndex];
                if (value == 0)
                {
                    _population.Individuals[randomIndividual].Сhromosomes[randomChromosome].BinarySequence[randomIndex] = 1;
                }
                else
                {
                    _population.Individuals[randomIndividual].Сhromosomes[randomChromosome].BinarySequence[randomIndex] = 0;
                }
            }
        }

        private static int FindNumberIndex(List<float> numbers, float number)
        {
            for (int i = 0; i < numbers.Count; i++)
            {
                if (number < numbers[i])
                {
                    return i;
                }
            }

            return -1;
        }

        private static byte[] ToBinary(float number, int length)
        {
            var binarySequence = new byte[length];
            int i = 0;

            while (i < length)
            {
                if ((int)number % 2 == 0)
                {
                    binarySequence[i] = 0;
                }
                else
                {
                    binarySequence[i] = 1;
                }

                number /= 2;

                if ((int)number == 0)
                {
                    break;
                }

                i++;
            }

            return binarySequence
                .Reverse()
                .ToArray();
        }

        private static float FromBinary(byte[] binarySequence)
        {
            float number = 0;
            int binaryLength = binarySequence.Length;

            for (int i = 0; i < binaryLength; i++)
            {
                binaryLength--;
                number += MathF.Pow(2, binaryLength) * binarySequence[i];
            }

            return number;
        }

        private bool CheckIfExistsFitnessFunctionValueLessZero()
        {
            for (int i = 0; i < _population.Count; i++)
            {
                if (_population.Individuals[i].FitnessFunctionValue < 0)
                {
                    return true;
                }
            }

            return false;
        }

        private void RecalculateFitnessFunctionValues()
        {
            var minFitnessFunctionValue = _population.Individuals[0].FitnessFunctionValue;
            float fitnessFunctionSum = 0;

            for (int i = 1; i < _population.Count; i++)
            {
                if (_population.Individuals[i].FitnessFunctionValue < minFitnessFunctionValue)
                {
                    minFitnessFunctionValue = _population.Individuals[i].FitnessFunctionValue;
                }
            }

            for (int i = 0; i < _population.Count; i++)
            {
                _population.Individuals[i].FitnessFunctionValue = _population.Individuals[i].FitnessFunctionValue + 2 * MathF.Abs(minFitnessFunctionValue);
                fitnessFunctionSum += _population.Individuals[i].FitnessFunctionValue;
            }

            _population.FitnessFunctionSum = fitnessFunctionSum;
        }
    }
}
