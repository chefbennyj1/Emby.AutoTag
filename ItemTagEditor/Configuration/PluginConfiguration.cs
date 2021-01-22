using System;
using System.Collections.Generic;
using System.Text;
using MediaBrowser.Model.Plugins;

namespace ItemTagEditor.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<Rule> Rules { get; set; }

        public class Profile
        {
            public string Rating { get; set; }
            public string Container { get; set; }
            public string VideoCodec { get; set; }
            public string Resolution { get; set; }
            public int Year { get; set; }
            public string Type { get; set; }
            public string AudioCodec { get; set; }
        }
        public class Rule
        {
            public List<string> Tags { get; set; }
            public Profile Profile { get; set; }
            public long Id { get; set; }
            public bool OverwriteTags { get; set; }
        }
    }
}
