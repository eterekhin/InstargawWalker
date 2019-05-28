using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using InstagrawWalker.Controllers;
using InstagrawWalker.Models;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace InstagrawWalker.InstagramActorSystem
{
    public class SeleniumActorCoordinator : ReceiveActor
    {
        private readonly PhotoInfoDto _photoInfoDto;
        private IActorRef _seleniumActorPool;
        private IActorRef _dowloadInformationActor;
        private List<PhotoInfo> PhotoInfos { get; set; } = new List<PhotoInfo>();

        public class Photos : HasSeleniumActorId
        {
            public List<(IWebElement, string)> elements { get; set; }

            public Photos(int actorId) : base(actorId)
            {
            }
        }

        public class ContinueDowload : HasSeleniumActorId
        {
            public ContinueDowload(int actorId) : base(actorId)
            {
            }
        }

        private const int HandledElementsByStep = 33;

        private readonly int NeedDowloadCountPhotoReadonly;
        private readonly int NeedStepCount;
        private int NeedDowloadCountPhoto { get; set; }

        private string LastItem { get; set; }

        private int StepCounter = 0;


        public SeleniumActorCoordinator(PhotoInfoDto photoInfoDto)
        {
            _photoInfoDto = photoInfoDto;
            NeedDowloadCountPhotoReadonly = NeedDowloadCountPhoto = _photoInfoDto.Count;
            NeedStepCount = NeedDowloadCountPhotoReadonly % HandledElementsByStep == 0
                ? NeedDowloadCountPhotoReadonly / HandledElementsByStep
                : NeedDowloadCountPhotoReadonly / HandledElementsByStep + 1;

            _seleniumActorPool = Context
                .ActorOf(Props.Create(() => new SeleniumActor(_photoInfoDto.Url))
                    .WithRouter(new ConsistentHashingPool(NeedStepCount < 5 ? NeedStepCount : 5)));

            _seleniumActorPool.Tell(new SeleniumActor.Init(0)
                {Count = _photoInfoDto.Count > HandledElementsByStep ? HandledElementsByStep : _photoInfoDto.Count});

            _photoInfoDto = photoInfoDto;
            NeedDowloadCountPhoto = photoInfoDto.Count;

            Receive<Photos>(photos =>
            {
                if (NeedStepCount == StepCounter)
                {
                    //дописать уничтожение пула акторов, или уничтожить весь координатор
                    return;
                }

                Interlocked.Increment(ref StepCounter);

                LastItem = photos.elements.Last().Item2;

                if (NeedDowloadCountPhoto > 0)
                    Self.Tell(new ContinueDowload(GetSeleniumActorId(photos.ActorId)));

                Sender.Tell(new SeleniumActor.DowloadDetailInfo(photos.ActorId)
                    {WebElements = photos.elements.Select(x => x.Item1)});
            });

            Receive<ContinueDowload>(x =>
            {
                _seleniumActorPool.Tell(new SeleniumActor.DowloadWebElements(x.ActorId)
                {
                    FirstElementUrl = LastItem,
                    Count = HandledElementsByStep
                });
            });


            Receive<PhotoInfo>(x =>
            {
                if (PhotoInfos.Count == 80)
                {
                    var s = PhotoInfos.GroupBy(xx => new
                    {
                        xx.Author, xx.Date
                    }).ToList();
                }


                PhotoInfos.Add(x);
                // отправляет инфу в апи
            });
        }

        private int GetSeleniumActorId(int previousRouterId) => (previousRouterId + 1) % 10;

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new AllForOneStrategy(10, TimeSpan.FromSeconds(30), x => Directive.Resume);
        }
    }
}