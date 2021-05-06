using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GA
{
    public class Population
    {
        private readonly int _count;
        public List<Individual> Individuals { get; set; }
        public int Count => _count;
        public float FitnessFunctionSum { get; set; }

        public Population(int count)
        {
            _count = count;
            Individuals = new List<Individual>();
        }
    }
}
