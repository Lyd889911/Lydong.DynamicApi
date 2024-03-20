using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lydong.DynamicApi.Marks;
using System.Reflection;

namespace Lydong.DynamicApi.CoreImpl
{

    public class DynamicApplicationModelConvention : IApplicationModelConvention
    {
        private readonly DynamicApiOption _option;
        public DynamicApplicationModelConvention(DynamicApiOption option)
        {
            _option = option;
        }
        Dictionary<string,string> apiDict = new Dictionary<string,string>();
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (typeof(IDynamicApi).IsAssignableFrom(controller.ControllerType))
                {
                    ConfigureApplicationService(controller);
                }
            }
        }
        private void ConfigureApplicationService(ControllerModel controller)
        {
            ConfigureApiExplorer(controller);//api是否允许被发现
            ConfigureSelector(controller);//路由配置
            ConfigureParameters(controller);//参数配置
        }

        #region 配置Api是否允许被发现
        /// <summary>
        /// Api是否允许被发现
        /// </summary>
        /// <param name="controller"></param>
        private void ConfigureApiExplorer(ControllerModel controller)
        {
            if(CheckMapController(controller))
                controller.ApiExplorer.IsVisible = true;
            else
                controller.ApiExplorer.IsVisible = false;

            foreach (var action in controller.Actions)
            {
                if (CheckMapAction(action))
                    action.ApiExplorer.IsVisible = true;
                else
                    action.ApiExplorer.IsVisible = false;
            }
        }
        /// <summary>
        /// 检查是暴露该Action接口，暴露true，不暴露false
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool CheckMapAction(ActionModel action)
        {
            var nonDynamicApiAttribute = action.ActionMethod.GetCustomAttribute<NonDynamicApiAttribute>();
            if (nonDynamicApiAttribute == null)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 检查是暴露该Controller接口，暴露true，不暴露false
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool CheckMapController(ControllerModel controller)
        {
            var nonDynamicApiAttribute = controller.ControllerType.GetCustomAttribute<NonDynamicApiAttribute>();
            if (nonDynamicApiAttribute == null)
                return true;
            else
                return false;
        }

        #endregion 配置Api是否允许被发现

        #region 路由配置选择器
        /// <summary>
        /// 路由的配置
        /// </summary>
        /// <param name="controller"></param>
        private void ConfigureSelector(ControllerModel controller)
        {
            //移除空的选择器，为了清理控制器中不需要的选择器，以减少不必要的路由匹配
            RemoveEmptySelectors(controller.Selectors);

            //判断是否已经配置了路由。如果存在，则直接返回，表示已经配置了路由，无需进一步处理。
            if (controller.Selectors.Any(selector => selector.AttributeRouteModel != null))
                return;

            //没有配置路由就要对每个方法进行配置
            foreach (var action in controller.Actions)
            {
                if (CheckMapAction(action))
                {
                    ConfigureSelector(action);

                    //检查路由有没有重复
                    var httpverb = action.Selectors[0].ActionConstraints.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods?.FirstOrDefault();
                    var route = action.Selectors[0].AttributeRouteModel.Template;
                    CheckApiDict(httpverb, route, action.ActionName);
                }
            }
        }
        private void ConfigureSelector(ActionModel action)
        {
            RemoveEmptySelectors(action.Selectors);

            if (action.Selectors.Count <= 0)
                AddApplicationServiceSelector(action);
            else//一般走这个
                NormalizeSelectorRoutes(action);
        }
        private void NormalizeSelectorRoutes(ActionModel action)
        {
            foreach (var selector in action.Selectors)
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel == null ?
                     new AttributeRouteModel(new RouteAttribute(GetRouteTemplate(action))) :
                     AttributeRouteModel.CombineAttributeRouteModel(new AttributeRouteModel(new RouteAttribute(GetRouteTemplate(action))), 
                     selector.AttributeRouteModel);
       

                if (selector.ActionConstraints.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods?.FirstOrDefault() == null)
                    selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { GetHttpMethod(action) }));
            }
        }
        /// <summary>
        /// 检查路由有没有重复
        /// </summary>
        /// <param name="httpverb"></param>
        /// <param name="route"></param>
        /// <param name="actionName"></param>
        /// <exception cref="Exception"></exception>
        private void CheckApiDict(string httpverb,string route,string actionName)
        {
            //路由加入集合，判断是否重复
            string key = $"{httpverb} {route}";
            if (apiDict.ContainsKey(key))
                throw new Exception($"路由重复:{key}，Action1：{apiDict[key]}，Action2：{actionName}");
            else
                apiDict.Add(key, actionName);
        }
        private void AddApplicationServiceSelector(ActionModel action)
        {
            var selector = new SelectorModel();
            selector.AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(GetRouteTemplate(action)));
            selector.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { GetHttpMethod(action) }));
            action.Selectors.Add(selector);
        }
        /// <summary>
        /// 移除空选择器
        /// </summary>
        /// <param name="selectors"></param>
        private void RemoveEmptySelectors(IList<SelectorModel> selectors)
        {
            //在循环过程中删除列表中的元素会导致索引的变化，使用倒序循环可以避免这个问题
            for (var i = selectors.Count - 1; i >= 0; i--)
            {
                var selector = selectors[i];
                /*
                 * 选择器满足以下条件之一，则被认为是空的选择器：
                 * 1.没有设置路由模型，2.没有设置动作约束，3.没有设置终结点元数据
                 */
                if (selector.AttributeRouteModel == null &&
                    (selector.ActionConstraints == null || selector.ActionConstraints.Count <= 0) &&
                    (selector.EndpointMetadata == null || selector.EndpointMetadata.Count <= 0))
                {
                    selectors.Remove(selector);
                }
            }
        }
        /// <summary>
        /// 得到路由字符串
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private string GetRouteTemplate(ActionModel action)
        {
            string controllerName = GetControllerRemovePostfixName(action.Controller.ControllerName, _option.RemoveControllerPostfixes);
            string actionName = GetActionRemovePrefixAndPostfixName(action.ActionName, _option.RemoveActionPostfixes, _option.HttpVerbs.Keys.ToList());

            string routeTemplate = $"{_option.ApiPrefix}/{_option.ApiArea}/{controllerName}/{actionName}".Replace("//", "/");

            return routeTemplate;
        }
        /// <summary>
        /// 得到去掉后缀的控制器名
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="controllerRemovePostfixes"></param>
        /// <returns></returns>
        private string GetControllerRemovePostfixName(string controllerName, List<string> controllerRemovePostfixes)
        {
            foreach (var removePostfix in controllerRemovePostfixes)
            {
                if (controllerName.EndsWith(removePostfix))
                    controllerName = controllerName.Substring(0, controllerName.Length - removePostfix.Length);
            }
            return controllerName.ToLower();
        }
        /// <summary>
        /// 得到去掉后缀和前缀的方法名
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="actionRemovePostfixes"></param>
        /// <returns></returns>
        private string GetActionRemovePrefixAndPostfixName(string actionName, List<string> actionRemovePostfixes, List<string> actionRemovePrefixes)
        {
            foreach (var removePostfix in actionRemovePostfixes)
            {
                if (actionName.EndsWith(removePostfix))
                    actionName = actionName.Substring(0, actionName.Length - removePostfix.Length);
            }
            foreach (var removePrefix in actionRemovePrefixes)
            {
                if (actionName.ToLower().StartsWith(removePrefix))
                    actionName = actionName.Substring(removePrefix.Length);
            }
            return actionName.ToLower();
        }

        /// <summary>
        /// 根据方法名http的谓词
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private string GetHttpMethod(ActionModel action)
        {
            var actionName = action.ActionName.ToLower();
            foreach (var verb in _option.HttpVerbs)
            {
                if (actionName.StartsWith(verb.Key))
                    return verb.Value;
            }
            return "POST";
        }
        #endregion 路由配置选择器

        #region 参数配置
        /// <summary>
        /// 参数配置，一些接口的参数默认配置Body传参，其他的不设置，需要手动设置
        /// </summary>
        /// <param name="controller"></param>
        private void ConfigureParameters(ControllerModel controller)
        {
            foreach (var action in controller.Actions)
            {
                foreach (var parameter in action.Parameters)
                {
                    if (parameter.BindingInfo != null)
                        continue;

                    if (!IsPrimitiveExtendedIncludingNullable(parameter.ParameterType))
                        if (CanUseFormBodyBinding(action, parameter))
                            parameter.BindingInfo = BindingInfo.GetBindingInfo(new[] { new FromBodyAttribute() });
                }
            }
        }
        /// <summary>
        /// 判断参数类型是不是原始类型，枚举和一些拓展类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsPrimitiveExtendedIncludingNullable(Type type)
        {
            //是不是原始类型，枚举类型，和一些扩展类型
            if (IsPrimitiveExtended(type))
                return true;

            //是不是可空类型
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return IsPrimitiveExtended(type.GenericTypeArguments[0]);

            return false;
        }
        private bool IsPrimitiveExtended(Type type)
        {
            if (type.IsPrimitive || type.IsEnum)
                return true;

            return type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(DateTimeOffset) ||
                   type == typeof(TimeSpan) ||
                   type == typeof(Guid);
        }
        /// <summary>
        /// 是否可以使用Body传参
        /// </summary>
        /// <param name="action"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanUseFormBodyBinding(ActionModel action, ParameterModel parameter)
        {

            DynamicApiOption option = new();
            //是否是忽略的类型
            if (option.FormBodyBindingIgnoredTypes.Any(t => t.IsAssignableFrom(parameter.ParameterType)))
                return false;

            var httpMethods = action.Selectors.SelectMany(selector => selector.ActionConstraints)
                .OfType<HttpMethodActionConstraint>().SelectMany(temp => temp.HttpMethods).ToList();

            if (httpMethods.Contains("GET") || httpMethods.Contains("TRACE") || httpMethods.Contains("HEAD"))
                return false;

            return true;
        }
        #endregion 参数配置

    }

}
