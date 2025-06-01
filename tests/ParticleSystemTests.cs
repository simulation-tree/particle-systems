using Particles.Messages;
using Simulation.Tests;
using Types;
using Worlds;

namespace Particles.Systems.Tests
{
    public abstract class ParticleSystemTests : SimulationTests
    {
        public World world;

        static ParticleSystemTests()
        {
            MetadataRegistry.Load<ParticlesMetadataBank>();
            MetadataRegistry.Load<ParticlesSystemsMetadataBank>();
        }

        protected override void SetUp()
        {
            base.SetUp();
            Schema schema = new();
            schema.Load<ParticlesSchemaBank>();
            schema.Load<ParticlesSystemsSchemaBank>();
            world = new(schema);
            Simulator.Add(new ParticleSystem(Simulator, world));
        }

        protected override void TearDown()
        {
            Simulator.Remove<ParticleSystem>();
            world.Dispose();
            base.TearDown();
        }

        protected override void Update(double deltaTime)
        {
            Simulator.Broadcast(new ParticleUpdate((float)deltaTime));
        }
    }
}