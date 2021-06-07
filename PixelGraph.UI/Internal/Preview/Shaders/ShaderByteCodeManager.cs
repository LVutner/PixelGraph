﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Shaders;

namespace PixelGraph.UI.Internal.Preview.Shaders
{
    public interface IShaderByteCodeManager
    {
        void Add(string name, ShaderSourceDescription shader);
        bool LoadAll(out ShaderCompileError[] compileErrors);
        ShaderDescription BuildDescription(string name, ShaderStage type);
    }

    internal class ShaderByteCodeManager : IShaderByteCodeManager, IDisposable
    {
        private readonly Dictionary<string, ShaderSourceDescription> map;


        public ShaderByteCodeManager()
        {
            map = new Dictionary<string, ShaderSourceDescription>(StringComparer.InvariantCultureIgnoreCase);
        }

        public void Dispose()
        {
            foreach (var shader in map.Values)
                shader.Dispose();

            map.Clear();
        }

        public void Add(string name, ShaderSourceDescription shader)
        {
            map[name] = shader;
        }

        public bool LoadAll(out ShaderCompileError[] compileErrors)
        {
            var shaderPath = Path.GetFullPath("shaders");

#if DEBUG
            if (Debugger.IsAttached) {
                var p = Path.GetDirectoryName(Environment.CurrentDirectory);

                while (p != null) {
                    var t = Path.Combine(p, "Resources", "Shaders");
                    if (Directory.Exists(t)) {
                        shaderPath = t;
                        break;
                    }

                    p = Path.GetDirectoryName(p);
                }
            }
#endif

            var hasShaderFolder = Directory.Exists(shaderPath);
            var _errors = new List<ShaderCompileError>();

            foreach (var shader in map.Values) {
                if (hasShaderFolder && shader.TryLoadFromPath(shaderPath, _errors)) {
                    continue;
                }

                shader.LoadFromAssembly();
            }

            compileErrors = _errors.ToArray();
            return compileErrors.Length == 0;
        }

        public ShaderDescription BuildDescription(string name, ShaderStage type)
        {
            if (!map.TryGetValue(name, out var shader))
                throw new ApplicationException($"Shader '{name}' not found!");

            return new ShaderDescription(name, type, shader.Code);
        }
    }
}
