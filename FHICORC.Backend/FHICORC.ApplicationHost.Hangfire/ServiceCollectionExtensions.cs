﻿using Microsoft.Extensions.DependencyInjection;
using FHICORC.ApplicationHost.Hangfire.Interfaces;
using FHICORC.ApplicationHost.Hangfire.Tasks;

namespace FHICORC.ApplicationHost.Hangfire
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireTaskManagerAndTasks(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<IHangfireTaskManager, HangfireTaskManager>()
                .AddScoped<IUpdateEuCertificateRepositoryTask, UpdateEuCertificateRepositoryTask>();
        }
    }
}