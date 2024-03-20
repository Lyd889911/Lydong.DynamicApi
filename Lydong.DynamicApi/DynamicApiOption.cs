using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lydong.DynamicApi
{
    public class DynamicApiOption
    {
        /// <summary>
        /// HTTP动词字典。默认有一些词<br/>
        /// key:方法前的单词：匹配上了会自动去掉<br/>
        /// value：根据前缀单词，匹配那种http方法
        /// </summary>
        public Dictionary<string, string> HttpVerbs = new();
        /// <summary>
        /// API接口路径的前缀，默认api
        /// </summary>
        public string ApiPrefix = "api";
        public string ApiArea = "";
        /// <summary>
        /// 需要移除的控制器固定后缀列表，默认只有一个Service
        /// </summary>
        public List<string> RemoveControllerPostfixes = new List<string>() { "Service" };
        /// <summary>
        /// 需要移除的方法后缀列表，默认只有一个Async
        /// </summary>
        public List<string> RemoveActionPostfixes = new List<string>() { "Async" };
        public List<Type> FormBodyBindingIgnoredTypes = new List<Type>() { typeof(IFormFile) };
        public DynamicApiOption()
        {
            HttpVerbs = new()
            {
                ["add"] = "POST",
                ["create"] = "POST",
                ["post"] = "POST",

                ["get"] = "GET",
                ["find"] = "GET",
                ["fetch"] = "GET",
                ["query"] = "GET",

                ["update"] = "PUT",
                ["put"] = "PUT",
                ["modify"] = "PUT",

                ["delete"] = "DELETE",
                ["remove"] = "DELETE",
            };
        }
    }
}
