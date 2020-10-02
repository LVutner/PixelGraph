﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McPbrPipeline.Internal.Textures
{
    internal class EmissiveTextureMap
    {
        public string Texture {get; set;}

        public float? Scale {get; set;}

        [JsonProperty("meta")]
        public JToken Metadata {get; set;}
    }
}
