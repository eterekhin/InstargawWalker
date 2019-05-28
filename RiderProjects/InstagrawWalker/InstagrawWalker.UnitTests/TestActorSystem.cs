using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Routing;
using NUnit.Framework;

namespace Tests
{
    public class OneActor : ReceiveActor
    {
        public OneActor()
        {
            Receive<ActorCoordinator.Message>(x =>
            {
                var ss = x;
            });
        }
    }

    public class TwoActor : ReceiveActor
    {
        public TwoActor()
        {
            Receive<ActorCoordinator.Message>(x =>
            {
                var ss = x;
            });
        }
    }

    public class ThreeActor : ReceiveActor
    {
        public ThreeActor()
        {
            Receive<string>(x =>
            {
                var t = x;
            });
            Receive<ActorCoordinator.Message>(x =>
            {
                Sender.Tell(new ActorCoordinator.Message());
                var ss = x;
            });
        }
    }

    public class ActorCoordinator : ReceiveActor
    {
        public IActorRef pool { get; set; }

        protected override void PreStart()
        {
            pool = Context.ActorOf(Props.Create(() => new ThreeActor())
                .WithRouter(new RoundRobinPool(5)));
            base.PreStart();
        }

        private List<string> strs { get; } = new List<string>();

        public ActorCoordinator()
        {
            Receive<bool>(x => x, x => { pool.Tell(new Message()); });

            Receive<Message>(x =>
            {
                strs.Add(Sender.Path.ToString());
                var sender = Sender;
                pool.Tell(new Message());
            });
        }

        public class Message
        {
        }
    }

    [TestFixture]
    public class TestActorSystem
    {
        private ActorSystem _actorSystem { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _actorSystem = ActorSystem.Create("actorSystem");
        }

        [Test]
        public void Test1()
        {
            var pool = _actorSystem.ActorOf(Props.Create(() => new ActorCoordinator()));
            pool.Tell(true);
            _actorSystem.WhenTerminated.Wait();
        }
    }
}