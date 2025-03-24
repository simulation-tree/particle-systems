using Collections.Generic;
using Particles.Components;
using Simulation;
using System;
using Unmanaged;
using Worlds;

namespace Particles.Systems
{
    public partial struct ParticleSystem : ISystem
    {
        private readonly Dictionary<World, Array<ParticleEmitterState>> statesPerWorld;

        public ParticleSystem()
        {
            statesPerWorld = new(4);
        }

        public readonly void Dispose()
        {
            foreach (Array<ParticleEmitterState> states in statesPerWorld.Values)
            {
                states.Dispose();
            }

            statesPerWorld.Dispose();
        }

        readonly void ISystem.Start(in SystemContext context, in World world)
        {
        }

        readonly void ISystem.Update(in SystemContext context, in World world, in TimeSpan delta)
        {
            float deltaSeconds = (float)delta.TotalSeconds;
            int capacity = (world.MaxEntityValue + 1).GetNextPowerOf2();
            ref Array<ParticleEmitterState> states = ref statesPerWorld.TryGetValue(world, out bool contains);
            if (!contains)
            {
                states = ref statesPerWorld.Add(world);
                states = new(capacity);
            }
            else if (states.Length < capacity)
            {
                states.Length = capacity;
            }

            int emitterType = world.Schema.GetComponentType<IsParticleEmitter>();
            int arrayType = world.Schema.GetArrayType<Particle>();
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
                        Values<Particle> particles = world.GetArray<Particle>(entities[i], arrayType);
                        ref ParticleEmitterState state = ref states[i];
                        if (state.Entity != entity.value)
                        {
                            state = new(entity.value);
                        }

                        Update(entity, emitter, particles, arrayType, deltaSeconds, ref state);
                    }
                }
            }
        }

        readonly void ISystem.Finish(in SystemContext context, in World world)
        {
        }

        private static void Update(ParticleEmitter entity, IsParticleEmitter emitter, Values<Particle> particles, int arrayType, float delta, ref ParticleEmitterState state)
        {
            //advance current particles
            for (int i = 0; i < particles.Length; i++)
            {
                ref Particle particle = ref particles[i];
                particle.lifetime -= delta;
                if (particle.lifetime <= 0)
                {
                    particle.free = true;
                }
            }

            //create new particles
            int particlesToSpawn = 0;
            while (true)
            {
                if (state.spawnCooldown <= 0)
                {
                    state.spawnCooldown += emitter.emission.interval.Evaluate(state.GetRandomFloat());
                    state.Randomize();
                    particlesToSpawn++;
                }
                else
                {
                    break;
                }
            }

            if (particlesToSpawn > 0)
            {
                //reuse free particles
                for (int f = 0; f < particles.Length; f++)
                {
                    ref Particle particle = ref particles[f];
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
                    int previousLength = particles.Length;
                    particles.Resize(previousLength + particlesToSpawn);
                    int newLength = particles.Length;
                    for (int p = previousLength; p < newLength; p++)
                    {
                        Spawn(ref particles[p], emitter, ref state);
                    }
                }
            }

            state.spawnCooldown -= delta;
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