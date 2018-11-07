using System;
using System.Collections.Generic;
using DemoContentLoader;
using DemoRenderer;

namespace DemoEngine
{
    /// <summary>
    /// Constructs a demo from the set of available demos on demand.
    /// </summary>
    public class DemoSet
    {
        struct Option
        {
            public string Name;
            public Func<ContentArchive, Camera, Demo> Builder;
        }

        readonly List<Option> options = new List<Option>();
        public DemoSet AddOption<T>() where T : Demo, new()
        {
            options.Add(new Option
            {
                Builder = (content, camera) =>
                {
                    //Note that the actual work is done in the Initialize function rather than a constructor.
                    //The 'new T()' syntax actually uses reflection and repackages exceptions in an inconvenient way.
                    //By using Initialize instead, the stack trace and debugger will go right to the source.
                    var demo = new T();
                    demo.InitializeRendering(content, camera);
                    return demo;
                },
                Name = typeof(T).Name
            });
            return this;
        }

        public DemoSet AddDefaultOptions()
        {
            /*AddOption<ConstraintTestDemo>(); // TODO
            AddOption<PyramidDemo>();
            AddOption<RagdollDemo>();
            AddOption<CompoundTestDemo>();
            AddOption<MeshDemo>();
            AddOption<ClothDemo>();
            AddOption<NewtDemo>();
            AddOption<RopeStabilityDemo>();
            AddOption<BlockChainDemo>();
            AddOption<RayCastingDemo>();
            AddOption<SweepDemo>();
            AddOption<ShapePileDemo>();
            AddOption<FountainStressTestDemo>();
            AddOption<SolverBatchTestDemo>();*/
            return this;
        }

        public int Count { get { return options.Count; } }

        public string GetName(int index)
        {
            return options[index].Name;
        }

        public Demo Build(int index, ContentArchive content, Camera camera)
        {
            return options[index].Builder(content, camera);
        }
    }
}