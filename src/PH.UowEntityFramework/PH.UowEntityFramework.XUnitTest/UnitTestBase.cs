using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PH.UowEntityFramework.TestCtx;
using PH.UowEntityFramework.TestCtx.Models;
using Xunit;

namespace PH.UowEntityFramework.XUnitTest
{
    public abstract class UnitTestBase
    {
        public ILifetimeScope Scope { get; }
        public static NLog.Logger Logger;


        protected UnitTestBase()
        {
            Logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            Logger.Info("Starting test");

            var serviceCollection = new ServiceCollection();

            // The Microsoft.Extensions.Logging package provides this one-liner
            // to add logging services.
            serviceCollection.AddLogging();


            serviceCollection.AddDbContext<DebugCtx>(options =>
                                           {
                                               options
                                                   .UseSqlServer("Server=.\\SQLEXPRESS01;Database=Dbg3;User Id=sa;Password=sa;MultipleActiveResultSets=true")
                                                   .UseLazyLoadingProxies(true);
                                               options.EnableDetailedErrors(true);
                                               options.EnableSensitiveDataLogging(true);
                                               

                                           }
                                          );

            serviceCollection.AddIdentity<UserDebug, RoleDebug>(options =>
                    {
                        

                        options.Lockout = new LockoutOptions()
                        {
                            AllowedForNewUsers     = true, MaxFailedAccessAttempts = 6,
                            DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3)
                        };

                        options.User = new UserOptions()
                        {
                            RequireUniqueEmail = true
                        };
                    })
                    .AddEntityFrameworkStores<DebugCtx>()
                    .AddDefaultTokenProviders();


            var containerBuilder = new ContainerBuilder();

            containerBuilder.Populate(serviceCollection);


            DbContextOptionsBuilder<DebugCtx> ctxBuilder = new DbContextOptionsBuilder<DebugCtx>();
            ctxBuilder.UseLazyLoadingProxies();
            ctxBuilder.UseSqlServer("Server=.\\SQLEXPRESS01;Database=Dbg3;User Id=sa;Password=sa;MultipleActiveResultSets=true");
            
            
            containerBuilder.RegisterInstance(Logger.Factory)
                            .AsSelf()
                            .AsImplementedInterfaces();

            containerBuilder.RegisterGeneric(typeof(Logger<>))
                            .As(typeof(ILogger<>))
                            .SingleInstance();


            containerBuilder.Register(x => new DebugCtx(ctxBuilder.Options, $"{NewId.NextGuid():N}" , "test author 2" , true ))
                            
                            .AsSelf()
                            .AsImplementedInterfaces()
                            .InstancePerLifetimeScope();





            var container       = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(container);


            

           
            

            Scope = container.BeginLifetimeScope();

        }


    }
}
