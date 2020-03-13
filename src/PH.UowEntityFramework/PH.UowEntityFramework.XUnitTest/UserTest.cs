using System;
using System.Linq;
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
            await store.SaveChangesAsync();
            uow.Commit("test");

            Assert.NotNull(u.CreatedTransaction);


        }

        [Fact]
        public async void CreateSomeData()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow   = Scope.Resolve<IUnitOfWork>();
            var user = await store.Users.FirstOrDefaultAsync().ConfigureAwait(false);

            var data = new DataDebug()
            {
                Id     = $"Data from {user.Id} - {NewId.Next()}",
                Author = user,
                Title  = $"Simple title {DateTime.Now:O}"
            };

            await store.MyData.AddAsync(data).ConfigureAwait(false);
            await store.SaveChangesAsync().ConfigureAwait(false);


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

            data.Title = $"Mod {data.Title} {DateTime.UtcNow}";
            await store.MyData.UpdateAsync(data);
            

            uow.Commit("edit some data");

            Assert.NotNull(data.CreatedTransaction);
            Assert.NotNull(data.UpdatedTransaction);

            Assert.NotEqual(data.CreatedTransaction.Id, data.UpdatedTransaction.Id);
        }

        [Fact]
        public async void CreateSomeNodeData()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow   = Scope.Resolve<IUnitOfWork>();
            var user  = await store.Users.FirstOrDefaultAsync();

            var data = new DataDebug()
            {
                Id     = $"Data from {user.Id} - {NewId.Next()}",
                Author = user,
                Title  = $"Simple title {DateTime.Now:O}"
            };

            await store.MyData.AddAsync(data);


            var node = new NodeDebug()
            {
                Id       = NewId.Next().ToString(),
                Data     = data,
                NodeName = "A Test"
            };

            await store.Nodes.AddAsync(node);

            await store.SaveChangesAsync();

            uow.Commit("create some node data");

            Assert.NotNull(node.CreatedTransaction);
            Assert.NotNull(node.UpdatedTransaction);

        }
        


        [Fact]
        public async void CreateSomeNodeChildren()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow   = Scope.Resolve<IUnitOfWork>();
            var parent  = await store.Nodes.OrderByDescending(x => x.Timestamp).FirstOrDefaultAsync(x => x.Parent == null && x.Data != null);
            
            




            var node = new NodeDebug()
            {
                Id = NewId.Next().ToString(),
                Data = new DataDebug()
                    {Id = $"ATTACHED From Node {DateTime.Now.Ticks}", Author = parent.Data.Author, Title = "runtime created "},
                NodeName = "A Test",
                Parent   = parent
            };

            await store.Nodes.AddAsync(node);

            parent.NodeName = $"mod {parent.NodeName}";
            await store.Nodes.UpdateAsync(parent);


            await store.SaveChangesAsync();

            uow.Commit("create some node data adv");

            Assert.NotNull(node.CreatedTransaction);
            Assert.NotNull(node.UpdatedTransaction);

        }

        [Fact]
        public async void RemoveSomeDataAsync()
        {
            var store = Scope.Resolve<DebugCtx>();
            var uow   = Scope.Resolve<IUnitOfWork>();

            var node = new NodeDebug()
            {
                Id       = NewId.Next().ToString(),
                NodeName = "A Test for delete"
            };

            await store.Nodes.AddAsync(node);
            await store.SaveChangesAsync();



            var rem = store.Nodes.Remove(node);


            await uow.CommitAsync("Delete a node");


            Assert.NotNull(rem.Entity.DeletedTransaction);
            Assert.True(rem.Entity.Deleted);


        }

        [Fact]
        public async void GetAudit()
        {
            var store = Scope.Resolve<DebugCtx>();
            var info = await store.FindAuditInfoAsync("00010000-0faa-0009-e0c9-08d74295d0da");


            var perEntity = await store.FindAuditInfoAsync<NodeDebug, string>("00010000-0faa-0009-d43d-08d74295cf55");


            var t = info.NewValues;


            Assert.NotNull(info);
            Assert.NotNull(perEntity);
            Assert.NotEmpty(perEntity);

        }

    }
}