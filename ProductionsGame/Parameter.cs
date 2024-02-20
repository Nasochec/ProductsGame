using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionsGame
{
    public class Parameter
    {
        public String Id { get; private set; }
        public string Name { get; private set; }
        public int Value { get;  set; }
        public Parameter(string id, string name, int value)
        {
            Id = id;
            Name = name;
            Value = value;
        }

    }
}
