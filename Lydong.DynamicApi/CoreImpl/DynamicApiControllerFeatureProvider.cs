using Lydong.DynamicApi.Marks;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lydong.DynamicApi.CoreImpl
{
    public class DynamicApiControllerFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            //判断是否继承了指定的接口
            if (typeof(IDynamicApi).IsAssignableFrom(typeInfo))
            {
                if (!typeInfo.IsInterface &&
                    !typeInfo.IsAbstract &&
                    !typeInfo.IsGenericType &&
                    typeInfo.IsPublic)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
