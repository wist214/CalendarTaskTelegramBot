using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalendarEvent.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CalendarEvent.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers Application layer services: MediatR handlers and other application services.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR: register handlers from this assembly
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
            return services;
        }
    }
}
