using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using Akka.Util.Internal;
using InstagrawWalker.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace InstagrawWalker.InstagramActorSystem
{
    public class HasSeleniumActorId
    {
        public HasSeleniumActorId(int actorId)
        {
            ActorId = actorId;
        }

        public int ActorId { get; set; }
    }

    public class SeleniumActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly string _path;
        private readonly ChromeDriver _chromeDriver;
        private int NeedHanled { get; set; }


        public class CountWebElement : HasSeleniumActorId, IConsistentHashable
        {
            public int Count { get; set; }

            public CountWebElement(int actorId) : base(actorId)
            {
                ConsistentHashKey = ActorId;
            }

            public object ConsistentHashKey { get; }
        }

        public class DowloadWebElements : CountWebElement
        {
            public string FirstElementUrl { get; set; }

            public DowloadWebElements(int actorId) : base(actorId)
            {
            }
        }

        public class Init : CountWebElement
        {
            public Init(int actorId) : base(actorId)
            {
            }
        }

        public class DowloadDetailInfo : HasSeleniumActorId
        {
            public IEnumerable<IWebElement> WebElements { get; set; }

            public DowloadDetailInfo(int actorId) : base(actorId)
            {
            }
        }


        protected override void PreStart()
        {
            _chromeDriver.Navigate().GoToUrl(_path);
            _chromeDriver.Manage().Window.Maximize();
            base.PreStart();
        }


        public void Ready()
        {
            Receive<Init>(dwe =>
            {
                var list = GetWebElements(dwe).ToList();
                NeedHanled = dwe.Count;
                Become(Waiting);
                Sender.Tell(new SeleniumActorCoordinator.Photos(dwe.ActorId) {elements = list});
            });

            Receive<DowloadWebElements>(dwe =>
            {
                var list = new List<(IWebElement, string)>();
                while (list.All(x => x.Item2 != dwe.FirstElementUrl))
                {
                    list = ScrollAndGetElements().ToList();
                }

                var newList = list.ToList();
                while (newList.Any(x => x.Item2 == dwe.FirstElementUrl))
                {
                    newList = ScrollAndGetElements().ToList();
                }

                var newListFirstItemIndexInList = list.IndexOf(newList[0]);

                var firstElementUrlIndex = list
                    .Select((x, i) => (x, i)).First(x => x.Item1.Item2 == dwe.FirstElementUrl).Item2;

                if (newListFirstItemIndexInList == -1)
                    newListFirstItemIndexInList = firstElementUrlIndex;

                var result = list
                    .Take(newListFirstItemIndexInList + 1)
                    .Skip(firstElementUrlIndex + 1)
                    .Concat(newList.SkipLast(newListFirstItemIndexInList - firstElementUrlIndex + 1))
                    .Take(dwe.Count);

                var join = result.Join(list, x => x.Item2, x => x.Item2, (x, y) => x);

                Become(Waiting);
                Sender.Tell(new SeleniumActorCoordinator.Photos(dwe.ActorId) {elements = result.ToList()});
            });
        }

        public void Waiting()
        {
            Receive<DowloadDetailInfo>(x => true, DowloadDetailInfoHandler);
            Receive<DowloadWebElements>(x => { Stash.Stash(); });
        }

        public SeleniumActor(string path)
        {
            var chromeDriverDirectory = $@"{Directory.GetCurrentDirectory()}/bin/debug/netcoreapp2.2/";
            _chromeDriver = new ChromeDriver(chromeDriverDirectory);
            _path = path;
            Ready();
        }

        private IEnumerable<(IWebElement, string)> GetWebElements(CountWebElement cwi)
        {
            var list = new List<(IWebElement webElement, string identifier)>();
            while (list.Count < cwi.Count)
            {
                var elements = ScrollAndGetElements().ToList();
                if (!list.Any())
                {
                    list = elements.Take(cwi.Count).ToList();
                    continue;
                }

                if (list[0].Equals(elements[0]))
                {
                    list = elements.Take(cwi.Count).ToList();
                    continue;
                }

                var distance = list.IndexOf(elements[0]);
                list.AddRange(elements.TakeLast(distance));
            }

            return list;
        }

        private IEnumerable<(IWebElement, string)> ScrollAndGetElements()
        {
            _chromeDriver.ExecuteScript("window.scrollBy(0,1000)");
            Thread.Sleep(1000);
            var elements = _chromeDriver.FindElementsByClassName("v1Nh3")
                .Select(xx => (xx, xx.FindElement(By.TagName("a")).GetAttribute("href")))
                .Skip(9)
                .ToList();
            return elements;
        }

        private void DowloadDetailInfoHandler(DowloadDetailInfo ddi)
        {
            foreach (var WebElements in ddi.WebElements)
            {
                WebElements.Click();
                Thread.Sleep(2000);
                var photoDate =
                    DateTime.Parse(_chromeDriver.FindElement(By.ClassName("_1o9PC")).GetAttribute("datetime"));

                Thread.Sleep(10);

                var header = _chromeDriver.FindElementsByClassName("Ppjfr")
                    .Select(x => x.Text)
                    .Select(x =>
                    {
                        var lines = x.Split("\r\n");
                        var author = lines[0];
                        var location = string.Empty;
                        if (lines.Length == 4)
                            location = lines[3];
                        return new {author, location};
                    }).First();
                Thread.Sleep(10);

                var comments = _chromeDriver.FindElementsByClassName("C4VMK")
                    .Select(x => new
                    {
                        x.Text,
                        date = DateTime.Parse(x.FindElement(By.TagName("time")).GetAttribute("datetime"))
                    }).ToList();
                Thread.Sleep(10);


                var descriptionText = comments[0].Text;

                var commentStructure =
                    comments.Skip(1)
                        .Select(x =>
                        {
                            var lines = x.Text.Split("\r\n");
                            var author = lines[0];
                            var description = string.Join("\r\n", lines.Skip(1).SkipLast(1));
                            return new Comment {Author = author, Date = x.date, Text = description};
                        })
                        .ToList();

                Thread.Sleep(10);

                var likes = "0";
                try
                {
                    likes = _chromeDriver.FindElement(By.ClassName("Nm9Fw")).FindElement(By.TagName("span")).Text
                        .Split(' ').Aggregate(string.Empty, (a, cc) => $@"{a}{cc}");
                }
                catch
                {
                    // ignored
                }

                var photoInfo = new PhotoInfo
                {
                    Author = header.author,
                    Description = string.Join("\r\n", descriptionText.Split("\r\n").SkipLast(1).Skip(1)),
                    CountLikes = int.Parse(likes.Split(' ').Aggregate(string.Empty, (a, cc) => a + "" + cc)),
                    Location = header.location,
                    InstagramUrl = _chromeDriver.Url,
                    Comments = commentStructure,
                    Date = photoDate
                };

                _chromeDriver.FindElement(By.ClassName("ckWGn")).Click();
                Thread.Sleep(1000);
                Sender.Tell(photoInfo);
            }

            UnbecomeStacked();
            Stash.UnstashAll();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _chromeDriver.Quit();
            base.PreRestart(reason, message);
        }

        public IStash Stash { get; set; }
    }
}