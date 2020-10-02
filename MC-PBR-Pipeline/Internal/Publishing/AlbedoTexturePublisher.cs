﻿using McPbrPipeline.Internal.Filtering;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace McPbrPipeline.Internal.Publishing
{
    internal class AlbedoTexturePublisher : TexturePublisherBase
    {
        public AlbedoTexturePublisher(
            IProfile profile,
            IInputReader reader,
            IOutputWriter writer) : base(profile, reader, writer) {}

        public async Task PublishAsync(TextureCollection texture, CancellationToken token)
        {
            var sourcePath = Profile.GetSourcePath(texture.Path);
            if (!texture.UseGlobalMatching) sourcePath = Path.Combine(sourcePath, texture.Name);
            var destinationFilename = Path.Combine(texture.Path, $"{texture.Name}.png");
            var albedoTexture = texture.Map.Albedo;

            var filters = new FilterChain(Reader, Writer) {
                DestinationFilename = destinationFilename,
                SourceFilename = GetFilename(texture, TextureTags.Albedo, sourcePath, albedoTexture?.Texture),
            };

            Resize(filters, texture);

            await filters.ApplyAsync(token);

            if (albedoTexture?.Metadata != null)
                await PublishMcMetaAsync(albedoTexture.Metadata, destinationFilename, token);
        }
    }
}
