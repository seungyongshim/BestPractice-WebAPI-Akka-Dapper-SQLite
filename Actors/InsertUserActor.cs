﻿using Akka.Actor;
using Akka.Event;
using Database.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using Messages;

namespace Actors
{
    public class InsertUserActor : ReceiveActor
    {
        protected InsertUserActor() { }
        public InsertUserActor(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Receive<InsertUserMessage>(Handle);
        }

        public ILoggingAdapter Logger { get; } = Context.GetLogger();
        public IServiceProvider ServiceProvider { get; }

        private void Handle(InsertUserMessage msg)
        {
            using var scope = ServiceProvider.CreateScope();
            using var uow = scope.ServiceProvider.GetService<IUnitOfWork>().BeginTransaction();
            var userRepository = scope.ServiceProvider.GetService<IUserRepository>();
            try
            {
                Sender.Tell(userRepository.Insert(msg.User));
                uow.Commit();
            }
            catch (Exception ex)
            {
                uow.Rollback();
                Logger.Warning(ex, "");
            }
        }
    }
}