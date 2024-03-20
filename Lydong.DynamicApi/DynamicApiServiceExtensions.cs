using Lydong.DynamicApi.CoreImpl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lydong.DynamicApi
{
    public static class DynamicApiServiceExtensions
    {
        public static IServiceCollection AddDynamicApi(this IServiceCollection services)
        {
            var option = services.BuildServiceProvider().GetService<DynamicApiOption>();
            if(option==null)
            {
                option = new DynamicApiOption();
                services.AddSingleton(option);
            }

            var applicationPartManager = (ApplicationPartManager)services
                        .FirstOrDefault(d => d.ServiceType == typeof(ApplicationPartManager))?.ImplementationInstance;

            if (applicationPartManager == null)
            {
                throw new ArgumentNullException(nameof(applicationPartManager));
            }
            applicationPartManager.FeatureProviders.Add(new DynamicApiControllerFeatureProvider());

            services.Configure<MvcOptions>(options =>
            {
                options.Conventions.Add(new DynamicApplicationModelConvention(option));
            });
            return services;
        }
        public static IServiceCollection AddDynamicApi(this IServiceCollection services,Action<DynamicApiOption> optionAction)
        {
            var dynamicApiOption = new DynamicApiOption();

            optionAction?.Invoke(dynamicApiOption);

            return AddDynamicApi(services, dynamicApiOption);
        }
        public static IServiceCollection AddDynamicApi(this IServiceCollection services, DynamicApiOption option)
        {
            if (option == null)
                throw new ArgumentException(nameof(option));

            services.AddSingleton(option);
            return AddDynamicApi(services);
        }
    }
}
