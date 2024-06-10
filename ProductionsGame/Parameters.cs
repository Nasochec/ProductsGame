using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Converters;
using System.Windows.Media.Media3D;

namespace ProductionsGame
{
    public class Parameter
    {
        public String Id { get; private set; }
        public string Name { get; private set; }
        public Parameter(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class IntParameter:Parameter
    {
        public int Value { get; set; }
        public IntParameter(string id, string name, int value):base(id,name)
        {
            Value = value;
        }
    }

    public class DoubleParameter : Parameter
    {
        public double Value { get; set; }
        public DoubleParameter(string id, string name, double value) : base(id, name)
        {
            Value = value;
        }
    }

    public class Parameters
    {
        List<Parameter> parameters = new List<Parameter>();
        public Parameters() { }

        public void addParameter(Parameter parameter)
        {
            parameters.Add(parameter);
        }

        public Parameter getParameter(string id)
        {
            foreach (var parameter in parameters)
                if (parameter.Id == id)
                    return parameter;
            return null;
        }

        //public int getParameterValue(string id)
        //{
        //    foreach (var parameter in parameters)
        //        if (parameter.Id == id)
        //            return parameter.Value;
        //    return 0;
        //}

        public IEnumerable<Parameter> getParameters()
        {
            return parameters;
        }
    }
}
