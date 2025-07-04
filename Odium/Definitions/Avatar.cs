using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odium.Definitions
{
    public class Avatar
    {
        [JsonProperty("avatarName")]
        public string AvatarName { get; set; }

        [JsonProperty("avatarId")]
        public string AvatarId { get; set; }

        [JsonProperty("vrca")]
        public string Vrca { get; set; }

        [JsonProperty("authorName")]
        public string AuthorName { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("wearer")]
        public string Wearer { get; set; }

        [JsonProperty("stealer")]
        public string Stealer { get; set; }
    }
}
