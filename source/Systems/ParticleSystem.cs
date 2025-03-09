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

        private ParticleSystem(World world)
        {
            statesPerWorld = new();
        }

        readonly void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                systemContainer.Write(new ParticleSystem(world));
            }
        }

        readonly void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
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

            ComponentType emitterType = world.Schema.GetComponentType<IsParticleEmitter>();
            ArrayElementType arrayType = world.Schema.GetArrayType<Particle>();
            foreach (Chunk chunk in world.Chunks)
            {
                if (chunk.Definition.ContainsComponent(emitterType))
                {
                    ReadOnlySpan<uint> entities = chunk.Entities;
                    Span<IsParticleEmitter> components = chunk.GetComponents<IsParticleEmitter>(emitterType);
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

        readonly void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            if (systemContainer.World == world)
            {
                foreach (Array<ParticleEmitterState> states in statesPerWorld.Values)
                {
                    states.Dispose();
                }

                statesPerWorld.Dispose();
            }
        }

        private static void Update(ParticleEmitter entity, IsParticleEmitter emitter, Values<Particle> particles, ArrayElementType arrayType, float delta, ref ParticleEmitterState state)
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
                    particles.Length += particlesToSpawn;
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