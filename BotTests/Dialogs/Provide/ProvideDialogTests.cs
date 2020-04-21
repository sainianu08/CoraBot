﻿using System.Linq;
using System.Threading.Tasks;
using Bot.Dialogs.Provide;
using BotTests.Setup;
using Microsoft.Bot.Schema;
using Shared;
using Shared.Models;
using Xunit;

namespace BotTests.Dialogs.Provide
{
    [Collection(TestCollectionName)]
    public class ProvideDialogTests : DialogTestBase
    {
        public ProvideDialogTests(TestFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task SingleCategory()
        {
            var schema = Helpers.GetSchema();
            var category = schema.Categories.First();
            var resource = category.Resources.First();

            await CreateTestFlow(ProvideDialog.Name)
                .Test("test", StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Test(resource.Name, StartsWith(Phrases.Provide.GetQuantity(resource.Name)))
                .StartTestAsync();
        }

        /*
        [Fact]
        public async Task MultipleCategories()
        {
            // Both commented out tests need multiple categories.
            // Need to use schema that isn't in the project so
            // that it can be configured differently for tests.

            var schema = Helpers.GetSchema();
            var category = schema.Categories.Last();
            var resource = category.Resources.Last();

            await CreateTestFlow(ProvideDialog.Name)
                .Test("test", StartsWith(Phrases.Provide.GetCategory))
                .Test(category.Name, StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Test(resource.Name, StartsWith(Phrases.Provide.GetQuantity(resource.Name)))
                .StartTestAsync();
        }
        
        [Fact]
        public async Task CategoryNone()
        {
            await CreateTestFlow(ProvideDialog.Name)
                .Test("test", StartsWith(Phrases.Provide.GetCategory))
                .StartTestAsync();
        }
        */

        [Fact]
        public async Task ResourceNone()
        {
            var schema = Helpers.GetSchema();
            var category = schema.Categories.First();

            await CreateTestFlow(ProvideDialog.Name)
                .Test("test", StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Send(Phrases.None)
                .AssertNoReply()
                .StartTestAsync();
        }

        [Fact]
        public async Task QuantityNoneNoExistingResource()
        {
            var schema = Helpers.GetSchema();
            var category = schema.Categories.First();
            var resource = category.Resources.First();

            await CreateTestFlow(ProvideDialog.Name)
                .Test("test", StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Test(resource.Name, StartsWith(Phrases.Provide.GetQuantity(resource.Name)))
                .Test("0", Phrases.Provide.CompleteUpdate)
                .StartTestAsync();
        }

        [Fact]
        public async Task QuantityNone()
        {
            var schema = Helpers.GetSchema();
            var category = schema.Categories.First();
            var resource = category.Resources.First();

            await CreateTestFlow(ProvideDialog.Name)
                .Send("test")
                .StartTestAsync();

            var user = await this.Api.GetUser(this.turnContext);

            await this.Api.Create(new Resource
            {
                CreatedById = user.Id,
                Category = category.Name,
                Name = resource.Name
            });

            await CreateTestFlow(ProvideDialog.Name)
                .AssertReply(StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Test(resource.Name, StartsWith(Phrases.Provide.GetQuantity(resource.Name)))
                .Test("0", Phrases.Provide.CompleteDelete)
                .StartTestAsync();

            var existingResource = await this.Api.GetResourceForUser(user, category.Name, resource.Name);
            Assert.Null(existingResource);
        }

        [Fact]
        public async Task CreateNewResource()
        {
            var schema = Helpers.GetSchema();
            var category = schema.Categories.First();
            var resource = category.Resources.First();

            await CreateTestFlow(ProvideDialog.Name)
                .Send("test")
                .StartTestAsync();

            var user = await this.Api.GetUser(this.turnContext);

            await CreateTestFlow(ProvideDialog.Name)
                .AssertReply(StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Test(resource.Name, StartsWith(Phrases.Provide.GetQuantity(resource.Name)))
                .Test(TestHelpers.DefaultQuantity.ToString(), StartsWith(Phrases.Provide.GetIsUnopened))
                .Test(TestHelpers.DefaultIsUnopened.ToString(), Phrases.Provide.CompleteCreate(user))
                .StartTestAsync();

            var newResource = await this.Api.GetResourceForUser(user, category.Name, resource.Name);
            Assert.NotNull(newResource);
            Assert.Equal(TestHelpers.DefaultQuantity, newResource.Quantity);
            Assert.Equal(TestHelpers.DefaultIsUnopened, newResource.IsUnopened);
        }

        [Fact]
        public async Task UpdateExistingResource()
        {
            var schema = Helpers.GetSchema();
            var category = schema.Categories.First();
            var resource = category.Resources.First();

            await CreateTestFlow(ProvideDialog.Name)
                .Send("test")
                .StartTestAsync();

            var user = await this.Api.GetUser(this.turnContext);

            await this.Api.Create(new Resource
            {
                CreatedById = user.Id,
                Category = category.Name,
                Name = resource.Name
            });

            await CreateTestFlow(ProvideDialog.Name)
                .AssertReply(StartsWith(Phrases.Provide.GetResource(category.Name)))
                .Test(resource.Name, StartsWith(Phrases.Provide.GetQuantity(resource.Name)))
                .Test(TestHelpers.DefaultQuantity.ToString(), StartsWith(Phrases.Provide.GetIsUnopened))
                .Test(TestHelpers.DefaultIsUnopened.ToString(), Phrases.Provide.CompleteUpdate)
                .StartTestAsync();

            var existingResource = await this.Api.GetResourceForUser(user, category.Name, resource.Name);
            Assert.NotNull(existingResource);
            Assert.Equal(TestHelpers.DefaultQuantity, existingResource.Quantity);
            Assert.Equal(TestHelpers.DefaultIsUnopened, existingResource.IsUnopened);
        }
    }
}
