using System.ComponentModel;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Services;

namespace ItemTagEditor.Api
{
    public class ItemTagUpdateService : IService
    {
        [Route("/UpdateTags", "POST", Summary = "Update the tag of the media base item")]
        public class UpdateTagsRequest : IReturnVoid
        {
            [ApiMember(Name = "InternalId", Description = "Internal Ids of the item to update", IsRequired = true, DataType = "long", ParameterType = "query", Verb = "POST")]
            public long InternalId { get; set; }

            [ApiMember(Name = "Tags", Description = "Tags to add to the item", IsRequired = true, DataType = "string[]", ParameterType = "query", Verb = "POST")]
            public string[] Tags { get; set; }
        }

        private ILibraryManager LibraryManager { get; }
        public ItemTagUpdateService(ILibraryManager libMan)
        {
            LibraryManager = libMan;
        }

        public void Post(UpdateTagsRequest request)
        {
            var item = LibraryManager.GetItemById(request.InternalId);
            item.Tags = request.Tags;
            LibraryManager.UpdateItem(item, item.Parent, ItemUpdateType.MetadataEdit);
        }
    }
}
