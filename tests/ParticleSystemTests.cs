using Simulation.Tests;
using Types;
using Worlds;

namespace Particles.Systems.Tests
{
    public abstract class ParticleSystemTests : SimulationTests
    {
        static ParticleSystemTests()
        {
            MetadataRegistry.Load<ParticlesMetadataBank>();
            MetadataRegistry.Load<ParticlesSystemsMetadataBank>();
        }

        protected override Schema CreateSchema()
        {
            Schema schema = base.CreateSchema();
            schema.Load<ParticlesSchemaBank>();
            schema.Load<ParticlesSystemsSchemaBank>();
            return schema;
        }
    }
}