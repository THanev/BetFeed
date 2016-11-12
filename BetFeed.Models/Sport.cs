﻿using System.Collections.Generic;

namespace BetFeed.Models
{
    public class Sport
    {
        public Sport()
        {
            this.Events = new HashSet<Event>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Event> Events { get; set; }
    }
}