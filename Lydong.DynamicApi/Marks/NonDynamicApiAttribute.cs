using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lydong.DynamicApi.Marks
{
    /// <summary>
    /// 不暴露该接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NonDynamicApiAttribute: Attribute
    {
    }
}
