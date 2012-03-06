﻿// ==============================================================================================================
// Microsoft patterns & practices
// CQRS Journey project
// ==============================================================================================================
// Copyright (c) Microsoft Corporation and contributors http://cqrsjourney.github.com/contributors/members
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License. You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is 
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.
// ==============================================================================================================

namespace Registration.Tests
{
	using System;
	using Registration.Database;
	using Xunit;
	using Common;
	using System.Collections.Generic;
	using System.Data.Entity;
	using Moq;

	public class OrmSagaRepositoryFixture
	{
		public OrmSagaRepositoryFixture()
		{
			using (var context = new TestOrmSagaRepository(Mock.Of<ICommandBus>()))
			{
				if (context.Database.Exists()) context.Database.Delete();

				context.Database.Create();
			}
		}

		[Fact]
		public void WhenSavingEntity_ThenCanRetrieveIt()
		{
			var id = Guid.NewGuid();

			using (var context = new TestOrmSagaRepository(Mock.Of<ICommandBus>()))
			{
				var conference = new OrmTestSaga(id);
				context.Save(conference);
			}

			using (var context = new TestOrmSagaRepository(Mock.Of<ICommandBus>()))
			{
				var conference = context.Find<OrmTestSaga>(id);

				Assert.NotNull(conference);
			}
		}

		[Fact]
		public void WhenSavingEntityTwice_ThenCanReloadIt()
		{
			var id = Guid.NewGuid();

			using (var context = new TestOrmSagaRepository(Mock.Of<ICommandBus>()))
			{
				var conference = new OrmTestSaga(id);
				context.Save(conference);
			}

			using (var context = new TestOrmSagaRepository(Mock.Of<ICommandBus>()))
			{
				var conference = context.Find<OrmTestSaga>(id);
				conference.Title = "CQRS Journey";

				context.Save(conference);

				context.Entry(conference).Reload();

				Assert.Equal("CQRS Journey", conference.Title);
			}
		}

		[Fact]
		public void WhenEntityExposesEvent_ThenRepositoryPublishesIt()
		{
			var bus = new Mock<ICommandBus>();
			var commands = new List<ICommand>();

			bus.Setup(x => x.Send(It.IsAny<IEnumerable<ICommand>>()))
				.Callback<IEnumerable<ICommand>>(x => commands.AddRange(x));

			var command = new TestCommand();

			using (var context = new TestOrmSagaRepository(bus.Object))
			{
				var aggregate = new OrmTestSaga(Guid.NewGuid());
				aggregate.AddCommand(command);
				context.Save(aggregate);
			}

			Assert.Equal(1, commands.Count);
			Assert.True(commands.Contains(command));
		}

		public class TestOrmSagaRepository : OrmSagaRepository
		{
			public TestOrmSagaRepository(ICommandBus commandBus)
				: base("TestOrmSagaRepository", commandBus)
			{
			}

			public DbSet<OrmTestSaga> TestSagas { get; set; }
		}

		public class TestCommand : ICommand
		{
			public Guid Id { get; set; }
		}
	}

	public class OrmTestSaga : IAggregateRoot, ICommandPublisher
	{
		private List<ICommand> commands = new List<ICommand>();

		protected OrmTestSaga() { }

		public OrmTestSaga(Guid id)
		{
			this.Id = id;
		}

		public Guid Id { get; set; }
		public string Title { get; set; }

		public void AddCommand(ICommand command)
		{
			this.commands.Add(command);
		}

		public IEnumerable<ICommand> Commands { get { return this.commands; } }
	}
}
