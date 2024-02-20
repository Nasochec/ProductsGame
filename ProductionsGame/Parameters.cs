using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ProductionsGame
{
    public class Parameters
    {
        List<Parameter> parameters = new List<Parameter>();
        public Parameters() { }


        public void addParameter(string id, string name, int defaultValue)
        {
            parameters.Add(new Parameter(id, name, defaultValue));
        }

        public void setParameter(string id, int value)
        {
            foreach (var parameter in parameters)
                if (parameter.Id == id)
                    parameter.Value = value;
        }

        public Parameter getParameter(string id)
        {
            foreach (var parameter in parameters)
                if (parameter.Id == id)
                    return parameter;
            return null;
        }

        public int getParameterValue(string id)
        {
            foreach (var parameter in parameters)
                if (parameter.Id == id)
                    return parameter.Value;
            return 0;
        }

        public IEnumerable<Parameter> getParameters()
        {
            return parameters;
        }
    }
}
