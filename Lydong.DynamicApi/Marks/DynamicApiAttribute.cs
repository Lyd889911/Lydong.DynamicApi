using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lydong.DynamicApi.Marks
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class DynamicApiAttribute: Attribute
    {
    }
}
