using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Login2048.Model
{
    public class Theme
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Uploader { get; set; }
    }

    public class ThemeTitle
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Uploader { get; set; }

        public ThemeTitle(long id, string name, string uploader)
        {
            Id = id;
            Name = name;
            Uploader = uploader;
        }
        public ThemeTitle(Theme theme)
        {
            Id = theme.Id;
            Name = theme.Name;
            Uploader = theme.Uploader;
        }
    }
}
