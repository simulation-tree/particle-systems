using Collections.Generic;
using Particles.Components;
using Particles.Messages;
using Simulation;
using System;
using Unmanaged;
using Worlds;

namespace Particles.Systems
{
    public partial class ParticleSystem : SystemBase, IListener<ParticleUpdate>
    {
        private readonly World world;
        private readonly Array<ParticleEmitterState> states;
        private readonly int emitterType;
        private readonly int particleArrayType;

        public ParticleSystem(Simulator simulator, World world) : base(simulator)
        {
            this.world = world;
            states = new(4);

            Schema schema = world.Schema;
            emitterType = schema.GetComponentType<IsParticleEmitter>();
            particleArrayType = schema.GetArrayType<Particle>();
        }

        public override void Dispose()
        {
            states.Dispose();
        }

        void IListener<ParticleUpdate>.Receive(ref ParticleUpdate message)
        {
            int capacity = (world.MaxEntityValue + 1).GetNextPowerOf2();
            if (states.Length < capacity)
            {
                states.Length = capacity;
            }

            foreach (Chunk chunk in world.Chunks)
            {
                if (chunk.Definition.ContainsComponent(emitterType))
                {
                    ReadOnlySpan<uint> entities = chunk.Entities;
                    ComponentEnumerator<IsParticleEmitter> components = chunk.GetComponents<IsParticleEmitter>(emitterType);
                    for (int i = 0; i < entities.Length; i++)
                    {
                        ParticleEmitter entity = new Entity(world, entities[i]).As<ParticleEmitter>();
                        IsParticleEmitter emitter = components[i];
                        Values<Particle> particles = world.GetArray<Particle>(entities[i], particleArrayType);
                        ref ParticleEmitterState state = ref states[i];
                        if (state.Entity != entity.value)
                        {
                            state = new(entity.value);
                        }

                        Update(emitter, particles, message.deltaTime, ref state);
                    }
                }
            }
        }

        private static void Update(IsParticleEmitter emitter, Values<Particle> particles, float deltaTime, ref ParticleEmitterState state)
        {
            //advance current particles
            Span<Particle> particlesSpan = particles.AsSpan();
            for (int i = 0; i < particlesSpan.Length; i++)
            {
                ref Particle particle = ref particlesSpan[i];
                particle.lifetime -= deltaTime;
                if (particle.lifetime <= 0)
                {
                    particle.free = true;
                }
            }

            //create new particles
            int particlesToSpawn = 0;
            while (state.spawnCooldown <= 0)
            {
                state.spawnCooldown += emitter.emission.interval.Evaluate(state.GetRandomFloat());
                state.Randomize();
                particlesToSpawn++;
            }

            if (particlesToSpawn > 0)
            {
                //reuse free particles
                for (int f = 0; f < particlesSpan.Length; f++)
                {
                    ref Particle particle = ref particlesSpan[f];
                    if (particle.free)
                    {
                        Spawn(ref particle, emitter, ref state);
                        particlesToSpawn--;
                        if (particlesToSpawn == 0)
                        {
                            break;
                        }
                    }
                }

                if (particlesToSpawn > 0)
                {
                    int previousLength = particlesSpan.Length;
                    particles.Resize(previousLength + particlesToSpawn);
                    particlesSpan = particles.AsSpan();
                    int newLength = particlesSpan.Length;
                    for (int p = previousLength; p < newLength; p++)
                    {
                        Spawn(ref particlesSpan[p], emitter, ref state);
                    }
                }
            }

            state.spawnCooldown -= deltaTime;
            state.Randomize();
        }

        private static void Spawn(ref Particle particle, IsParticleEmitter emitter, ref ParticleEmitterState state)
        {
            particle = new();
            particle.position = emitter.initialParticleState.position;
            particle.lifetime = emitter.initialParticleState.lifetime.Evaluate(state.GetRandomFloat());
            particle.velocity = emitter.initialParticleState.velocity.Evaluate(state.GetRandomVector3());
            particle.drag = emitter.initialParticleState.drag.Evaluate(state.GetRandomVector3());
            particle.extents = emitter.initialParticleState.Size.Evaluate(state.GetRandomVector3());
        }
    }
}