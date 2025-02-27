using Simulation.Tests;
using Types;
using Worlds;

namespace Particles.Systems.Tests
{
    public abstract class ParticleSystemTests : SimulationTests
    {
        static ParticleSystemTests()
        {
            TypeRegistry.Load<ParticlesTypeBank>();
            TypeRegistry.Load<ParticlesSystemsTypeBank>();
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