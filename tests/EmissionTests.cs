using Particles.Components;
using Shapes.Types;
using System.Numerics;

namespace Particles.Systems.Tests
{
    public class EmissionTests : ParticleSystemTests
    {
        [Test]
        public void EmitParticlesOverTime()
        {
            ParticleEmitter emitter = new(world);
            emitter.Emission.interval = new(0.2f);
            emitter.Emission.count = new(1);
            emitter.InitialParticleState.bounds = new SphereShape();
            emitter.InitialParticleState.drag = new(Vector3.Zero);
            emitter.InitialParticleState.lifetime = new(0.5f);
            emitter.InitialParticleState.velocity = new(Vector3.Zero);

            Assert.That(emitter.AliveParticles, Is.Zero);

            Update(0.1);

            Assert.That(emitter.AliveParticles, Is.EqualTo(1));
            Particle particle = emitter.GetAliveParticle(0);
            Assert.That(particle.free, Is.False);
            Assert.That(particle.lifetime, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(particle.position, Is.EqualTo(Vector3.Zero));

            Update(0.1);

            Assert.That(emitter.AliveParticles, Is.EqualTo(1));
            Particle previousParticle = emitter.GetAliveParticle(0);
            Assert.That(previousParticle.free, Is.False);
            Assert.That(previousParticle.lifetime, Is.EqualTo(0.4f).Within(0.01f));

            Update(0.1);

            Assert.That(emitter.AliveParticles, Is.EqualTo(2));
            particle = emitter.GetAliveParticle(1);
            Assert.That(particle.free, Is.False);
            Assert.That(particle.lifetime, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(particle.position, Is.EqualTo(Vector3.Zero));

            previousParticle = emitter.GetAliveParticle(0);
            Assert.That(previousParticle.free, Is.False);
            Assert.That(previousParticle.lifetime, Is.EqualTo(0.3f).Within(0.01f));

            Update(0.3);

            Assert.That(emitter.AliveParticles, Is.EqualTo(1));
            particle = emitter.GetAliveParticle(0);
            Assert.That(particle.free, Is.False);
            Assert.That(particle.lifetime, Is.EqualTo(0.2f).Within(0.01f));
        }
    }
}