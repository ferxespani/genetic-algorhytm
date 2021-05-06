using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GA
{
    public class Individual
    {
        public List<Chromosome> Сhromosomes { get; set; }
        public float FitnessFunctionValue { get; set; }
    }
}
