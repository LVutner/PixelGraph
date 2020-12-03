# PixelGraph [![Actions Status](https://github.com/null511/PixelGraph/workflows/Release/badge.svg)](https://github.com/null511/PixelGraph/actions)

PixelGraph (formerly MCPBRP) is an application for publishing Minecraft resource packs, with special tooling for post-processing PBR materials. Automate the workload of modifying and distributing your resource packs through yaml text files. Supports a WPF-based desktop application as well as a command-line version for remote/server usage. Written in .NET Core; supports Windows, Linux, Mac. Docker ready.

<img src="https://github.com/null511/PixelGraph/raw/master/media/LAB11.png" alt="PBR Workflow" />

 - **Simplify your workflow** by adjusting text instead of pixels. Getting image-based material values just right can be tedious, time consuming, and destructive.

 - **Preserve Quality** by adjusting material values through text rather than altering the original image data. Repeatedly scaling the integer-based channels of your image slowly destroys quality. Save the gradients!

 - **Support more users** by publishing multiple packs with varying quality. The resolution and included textures can be altered using either the command-line or Publishing Profiles to create multiple distributions.

 - **Automate** Normal & AO generation, resizing, and channel-swapping so that you can spend more time designing and less time repeating yourself.
 
### Normal-Map Generation

<img src="https://github.com/null511/PixelGraph/raw/master/media/NormalGeneration.png" alt="Normal-Map from Height-Map" height="140px"/>

Allows normal-map textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Strength, blur, and wrapping can be managed using the textures matching pbr-properties file.
 
### Occlusion-Map Generation

<img src="https://github.com/null511/PixelGraph/raw/master/media/OcclusionGeneration.png" alt="Occlusion-Map from Height-Map" height="140px"/>

Allows ambient-occlusion textures to be created from height-maps as needed during publishing, or by prerendering them beforehand. Quality, Z-scale, step-count, and wrapping can be managed using the materials properties.

## Installation

For manual installation, download the latest standalone executable from the [Releases](https://github.com/null511/PixelGraph/releases) page. For automated usage see [Docker Usage](https://github.com/null511/PixelGraph/wiki/Installation#docker). Visit the [wiki](https://github.com/null511/PixelGraph/wiki/Installation) for more information.

## Usage

A single Pack-Input file lives in the root of the workspace (`~/input.yml`) which specifies the default formatting of all content. The example below uses a 'raw' encoding which manages each channel in a different texture; The 'smooth' texture has been set to use 'Perceptual-Smoothness' rather than the default linear smoothness.

```yml
# ~/input.yml
format: raw
smooth:
  red: smooth2
```

One or more Pack-Profiles are used to describe a publishing routine; they also live in the project root and should match the naming convention `~/<name>.pack.yml`. Each profile can specify pack details, encoding, format, resizing, etc; this allows a single set of content to be published for multiple resolutions and encodings, ie `pbr-lab1.3-64x` or `default-128x`

```yml
# ~/pbr-lab13-x64.yml
output:
  format: default
texture-scale: 0.5
```

Material files are used to desribe a collection of textures that compose a single game "item". For more details, see the [Wiki](https://github.com/null511/PixelGraph/wiki/File-Loading).
```yml
# ~/assets/minecraft/textures/block/lantern.pbr.yml
smooth:
  scale: 1.2
metal:
  scale: 0.8
emissive:
  scale: 0.2
```

## Sample Repository

Coming Soon! I am _not_ a texture artist and need a good set of example content for a proper sample. If you own content you'd like to submit please reach out to me.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
