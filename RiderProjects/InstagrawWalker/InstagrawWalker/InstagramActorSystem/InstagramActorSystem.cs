using Akka.Actor;

namespace InstagrawWalker.InstagramActorSystem
{
    public class InstagramActorSystem
    {
        private readonly ActorSystem _actorSystem;

        public InstagramActorSystem(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
            
        }
        
        
    }
}