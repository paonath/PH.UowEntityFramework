using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PH.UowEntityFramework.TestCtx;
using PH.UowEntityFramework.TestCtx.Models;
using Xunit;

namespace PH.UowEntityFramework.XUnitTest
{
    public abstract class UnitTestBase
    {
        public ILifetimeScope Scope { get; }


        protected UnitTestBase()
        {

            var serviceCollection = new ServiceCollection();

            // The Microsoft.Extensions.Logging package provides this one-liner
            // to add logging services.
            serviceCollection.AddLogging();

            serviceCollection.AddDbContext<DebugCtx>(options =>
                                           {
                                               options
                                                   .UseSqlServer("Server=.\\SQLEXPRESS01;Database=Dbg3;User Id=sa;Password=sa;MultipleActiveResultSets=true")
                                                   .UseLazyLoadingProxies(true);
                                              
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
            


            containerBuilder.Register(x => new DebugCtx(ctxBuilder.Options))
                            .OnActivated(x =>
                            {
                                x.Instance.InitializeContext("test author", NewId.Next().ToString());

                            })
                            .AsSelf()
                            .AsImplementedInterfaces()
                            .InstancePerLifetimeScope();





            var container       = containerBuilder.Build();
            var serviceProvider = new AutofacServiceProvider(container);


            

           
            

            Scope = container.BeginLifetimeScope();

        }


    }
}