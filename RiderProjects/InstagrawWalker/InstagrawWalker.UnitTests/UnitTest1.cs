using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Akka.Util.Internal;
using InstagrawWalker.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test0()
        {
            var driver = new ChromeDriver(Directory.GetCurrentDirectory());
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://www.instagram.com/explore/locations/221630742/kazan-tatarstan/?hl=ru");
            driver.ExecuteScript("window.scrollBy(0,1000)");
            var ss = Enumerable.Repeat(1, 20).ToList().Select(x =>
            {
                driver.ExecuteScript("window.scrollBy(0,100)");
                Thread.Sleep(1000);
                var rwe = driver.FindElementsByClassName("v1Nh3").Cast<RemoteWebElement>().ToList();
                return (rwe.Count, rwe.Select(xx => xx.FindElement(By.TagName("a")).GetAttribute("href")).ToList());
            }).ToList();


            driver.Quit();
        }

        [Test]
        public void Test1()
        {
            var driver = new ChromeDriver(Directory.GetCurrentDirectory());
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://www.instagram.com/explore/locations/221630742/kazan-tatarstan/?hl=ru");
            var elements = new List<RemoteWebElement>();
//            for (int i = 0; i < 1; i++)
//            {
            for (int ii = 0; ii < 10; ii++)
            {
                driver.ExecuteScript("window.scrollBy(0,1000)");
                Thread.Sleep(1000);
            }
//
//                var photos = driver.FindElementsByClassName("Nnq7C").Cast<RemoteWebElement>();
//                elements.AddRange(photos);
//            }


            elements = driver.FindElementsByClassName("Nnq7C").Cast<RemoteWebElement>().ToList();
            var r = elements[0];
            r.Click();
            Thread.Sleep(1000);

            var photoDate = DateTime.Parse(driver.FindElement(By.ClassName("_1o9PC")).GetAttribute("datetime"));

            var header = driver.FindElementsByClassName("Ppjfr")
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

            var comments = driver.FindElementsByClassName("C4VMK")
                .Select(x => new
                {
                    x.Text,
                    date = DateTime.Parse(x.FindElement(By.TagName("time")).GetAttribute("datetime"))
                }).ToList();


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


            var likes = driver.FindElement(By.ClassName("Nm9Fw")).FindElement(By.TagName("span")).Text
                .Split(' ').Aggregate(string.Empty, (a, cc) => $@"{a}{cc}");

            var photoInfo = new PhotoInfo
            {
                Author = header.author,
                Description = string.Join("\r\n", descriptionText.Split("\r\n").SkipLast(1).Skip(1)),
                CountLikes = int.Parse(likes.Split(' ').Aggregate(string.Empty, (a, cc) => a + "" + cc)),
                Location = header.location,
                InstagramUrl = driver.Url,
                Comments = commentStructure,
                Date = photoDate
            };
        }
    }
}