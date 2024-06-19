using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGameCore
{
    public class Grammatic
    {
        public string Name { get; private set; }
        private List<ProductionGroup> productions;

        public Grammatic(string name,IEnumerable<string> productions) {
            Name = name;
            this.productions = new List<ProductionGroup>();
            foreach (var prod in productions) {
                this.productions.Add(ProductionGroup.fromString(prod));
            }
        }

        public IEnumerable<ProductionGroup> getProductions() { 
            return this.productions;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var prod in productions)
                sb.AppendLine(prod.ToString());
            return sb.ToString();
        }
    }
}
