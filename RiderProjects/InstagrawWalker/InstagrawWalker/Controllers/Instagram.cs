using Akka.Actor;
using InstagrawWalker.InstagramActorSystem;
using Microsoft.AspNetCore.Mvc;

namespace InstagrawWalker.Controllers
{
    public class PhotoInfoDto
    {
        [FromQuery]
        public string Url { get; set; }

        [FromQuery]
        public int Count { get; set; }
    }

    [Route("/api/[controller]/[action]")]
    public class Instagram : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public Instagram(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpGet]
        public IActionResult PhotoInfo(PhotoInfoDto photoInfoDto)
        {
            var s = new {photoInfoDto.Url, photoInfoDto.Count};
            var coordinator = _actorSystem.ActorOf(Props.Create(() => new SeleniumActorCoordinator(photoInfoDto)));
            return Ok(s);
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}