using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductsGame
{
    public interface IWords//TODO RENAME?
    {
        IEnumerable<string> getWords();
    }
}
