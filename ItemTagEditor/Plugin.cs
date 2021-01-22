using System;
using System.Collections.Generic;
using System.IO;
using ItemTagEditor.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace ItemTagEditor
{
    
    public class Plugin  : BasePlugin<PluginConfiguration>, IHasThumbImage, IHasWebPages
    {
        public static Plugin Instance { get; set; }
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            
        }

        public override string Name         => "Item Tag Editor";
        public ImageFormat ThumbImageFormat => ImageFormat.Jpg;
        public override Guid Id             => new Guid("B460B122-040F-49DE-94AD-9A352DDE6CA6");
        
        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.jpg");
        }
        
        public IEnumerable<PluginPageInfo> GetPages() => new[]
        {
            new PluginPageInfo
            {
                Name = "ItemTagEditorConfigurationPag",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.ItemTagEditorConfigurationPage.html",
            },
            new PluginPageInfo
            {
                Name = "ItemTagEditorConfigurationPageJS",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.ItemTagEditorConfigurationPage.js"
            }
        };
    }
}
