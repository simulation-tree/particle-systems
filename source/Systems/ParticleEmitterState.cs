using System;
using System.Numerics;

namespace Particles.Systems
{
    public struct ParticleEmitterState
    {
        public float spawnCooldown;

        private uint entity;
        private ulong random;

        public readonly uint Entity => entity;

        [Obsolete("Default constructor not supported")]
        public ParticleEmitterState()
        {
            throw new NotSupportedException();
        }

        public ParticleEmitterState(uint entity)
        {
            this.entity = entity;
            spawnCooldown = 0;
            random = entity * 0x9E3779B97F4A7C15;
        }

        public void Randomize()
        {
            random ^= random << 13;
            random ^= random >> 17;
            random ^= random << 5;
        }

        /// <summary>
        /// A random <see cref="float"/> value ranging from 0 to 1.
        /// </summary>
        public readonly float GetRandomFloat()
        {
            return (random & 0x7FFFFFFF) * 4.6566128752457969E-10f;
        }

        /// <summary>
        /// A random <see cref="Vector3"/> value ranging from (0, 0, 0) to (1, 1, 1).
        /// </summary>
        public readonly Vector3 GetRandomVector3()
        {
            return new Vector3(GetRandomFloat(), GetRandomFloat(), GetRandomFloat());
        }
    }
}