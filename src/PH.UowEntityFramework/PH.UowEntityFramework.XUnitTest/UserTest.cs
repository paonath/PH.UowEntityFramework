using System;
using Autofac;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PH.UowEntityFramework.EntityFramework.Extensions;
using PH.UowEntityFramework.TestCtx;
using PH.UowEntityFramework.TestCtx.Models;
using PH.UowEntityFramework.UnitOfWork;
using Xunit;

namespace PH.UowEntityFramework.XUnitTest
{
    public class UserTest : UnitTestBase
    {
        [Fact]
        public void ResolveCxt()
        {
            var store = Scope.Resolve<DebugCtx>();

            Assert.NotNull(store);

        }

        [Fact]
        public async void CreateAUser()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow = Scope.Resolve<IUnitOfWork>();

            var rnd = DateTime.Now.Ticks;

            var u = new UserDebug()
            {
                UserName     = $"paolo.innocenti-{rnd}@estrobit.com",
                Email        = $"paolo.innocenti{rnd}@estrobit.com",
                PasswordHash = $"{Guid.NewGuid()}",
                Id           = NewId.Next().ToString()

            };

            await store.Users.AddAsync(u);

            uow.Commit("test");

            Assert.NotNull(u.CreatedTransaction);


        }

        [Fact]
        public async void CreateSomeData()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow   = Scope.Resolve<IUnitOfWork>();
            var user = await store.Users.FirstOrDefaultAsync();

            var data = new DataDebug()
            {
                Id     = $"Data from {user.Id} - {NewId.Next()}",
                Author = user,
                Title  = $"Simple title {DateTime.Now:O}"
            };

            await store.MyData.AddAsync(data);
            await store.SaveChangesAsync();

            uow.Commit("create some data");

            Assert.NotNull(data.CreatedTransaction);
            Assert.NotNull(data.UpdatedTransaction);

        }

        [Fact]
        public async void EditSomeData()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow   = Scope.Resolve<IUnitOfWork>();

            var data = await store.MyData.FirstOrDefaultAsync();

            data.Title = $"Mod {data.Title}";
            await store.MyData.UpdateAsync(data);

            uow.Commit("edit some data");

            Assert.NotNull(data.CreatedTransaction);
            Assert.NotNull(data.UpdatedTransaction);

            Assert.NotEqual(data.CreatedTransaction.Id, data.UpdatedTransaction.Id);
        }

    }
}