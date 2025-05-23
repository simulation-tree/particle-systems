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

        protected override void SetUp()
        {
            base.SetUp();
            Simulator.Add(new ParticleSystem());
        }

        protected override void TearDown()
        {
            Simulator.Remove<ParticleSystem>();
            base.TearDown();
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