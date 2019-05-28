using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace InstagrawWalker.Models
{
    public class Comment
    {
        public string Author { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }

    public class PhotoInfo
    {
        public string Author { get; set; }

        [CanBeNull]
        public string Location { get; set; }

        public string InstagramUrl { get; set; }

        public string Description { get; set; }
        public int CountLikes { get; set; }
        public DateTime Date { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }
}