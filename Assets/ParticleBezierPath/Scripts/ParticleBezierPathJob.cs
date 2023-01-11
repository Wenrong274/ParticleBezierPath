using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace hyhy
{
    [BurstCompile(CompileSynchronously = true)]
    public struct ParticleBezierPathJob : IJobParticleSystem
    {
        [ReadOnly]
        public NativeArray<float> bezierPointIndices;
        [ReadOnly]
        public NativeArray<Vector3> pointDeltas;
        public float lifetime;

        public void Execute(ParticleSystemJobData particles)
        {
            float pointsSegment = pointDeltas.Length;
            float pointsSegmentX2 = pointsSegment * 2;
            VelocitieWeapon velocitie = new(particles);
            for (int i = 0; i < particles.count; i++)
            {
                float startLifetime = lifetime;
                float remainingLifetime = startLifetime * (1 - particles.aliveTimePercent[i] / 100f);
                bool b = false;
                for (int j = 0; j < bezierPointIndices.Length; j++)
                {
                    float f = bezierPointIndices[j];
                    if (remainingLifetime < (f * startLifetime / pointsSegmentX2))
                    {
                        b = true;
                        break;
                    }
                }

                if (remainingLifetime < (startLifetime / pointsSegmentX2))
                {
                    Vector3 velocity = pointsSegment / startLifetime * pointDeltas[pointDeltas.Length - 1];
                    velocitie.SetVelocity(i, velocity);
                }
                else if (b)
                {
                    for (int j = 0; j < bezierPointIndices.Length; j++)
                    {
                        if (remainingLifetime < (bezierPointIndices[j] * startLifetime / pointsSegmentX2))
                        {

                            int prevBezierPointIndex = j - 1 >= 0 ? (int)bezierPointIndices[j - 1] : 1;
                            float currentSegmentDuration = startLifetime / pointsSegment;
                            float remainingLifetimeInCurrentSegment = (startLifetime / pointsSegment) - (remainingLifetime - (prevBezierPointIndex * startLifetime / (pointsSegment * 2)));
                            float t = remainingLifetimeInCurrentSegment / currentSegmentDuration;
                            int currentBezierPointIndex = bezierPointIndices.Length - j - 1;
                            Vector3 currentDis = pointDeltas[currentBezierPointIndex];
                            Vector3 prevDis = pointDeltas[currentBezierPointIndex - 1];
                            Vector3 velocity = pointsSegment / startLifetime * Bezier(prevDis, currentDis, t);
                            velocitie.SetVelocity(i, velocity);
                            break;
                        }
                    }
                }
                else
                {
                    Vector3 velocity = pointsSegment / startLifetime * pointDeltas[0];
                    velocitie.SetVelocity(i, velocity);
                }
            }
        }

        private Vector3 Bezier(Vector3 P0, Vector3 P2, float t)
        {
            Vector3 P1 = (P0 + P2) / 2f;
            Vector3 B;
            B = (1f - t) * ((1f - t) * P0 + t * P1) + t * ((1f - t) * P1 + t * P2);
            return B;
        }

        private struct VelocitieWeapon
        {
            private NativeArray<float> velocitiesX;
            private NativeArray<float> velocitiesY;
            private NativeArray<float> velocitiesZ;

            public VelocitieWeapon(ParticleSystemJobData particles)
            {
                velocitiesX = particles.velocities.x;
                velocitiesY = particles.velocities.y;
                velocitiesZ = particles.velocities.z;
            }

            public void SetVelocity(int id, Vector3 velocity)
            {
                velocitiesX[id] = velocity.x;
                velocitiesY[id] = velocity.y;
                velocitiesZ[id] = velocity.z;
            }
        }
    }
}