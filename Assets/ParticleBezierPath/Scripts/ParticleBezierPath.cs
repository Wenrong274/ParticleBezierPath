using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

namespace hyhy
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleBezierPath : MonoBehaviour
    {
        public Transform[] controlPoints;
        private ParticleSystem m_particleSystem;
        [SerializeField] private float[] bezierPointIndices;

        public bool IsJob;

        void Start()
        {
            m_particleSystem = GetComponent<ParticleSystem>();
            bezierPointIndices = GenerateBezierPointIndices(controlPoints.Length * 2);
        }

        private void Update()
        {
            if (IsJob)
            {
                UpdateParticleVelocitiesJob();
            }
            else
                UpdateParticleVelocities();
        }

        private void UpdateParticleVelocitiesJob()
        {
            PathJob job = new PathJob();

            Vector3[] pointDeltas = new Vector3[controlPoints.Length];
            pointDeltas[0] = controlPoints[0].position - transform.position;
            for (int i = 1; i < controlPoints.Length; i++)
            {
                pointDeltas[i] = controlPoints[i].position - controlPoints[i - 1].position;
            }
            NativeArray<float> bezierPointIndicesArray = new(bezierPointIndices, Allocator.TempJob);
            NativeArray<Vector3> pointDeltasArray = new(pointDeltas, Allocator.TempJob);
            job.bezierPointIndices = bezierPointIndicesArray;
            job.pointDeltas = pointDeltasArray;
            JobHandle Handle = job.Schedule(m_particleSystem);
            Handle.Complete();
            bezierPointIndicesArray.Dispose();
            pointDeltasArray.Dispose();
        }
        #region Normal
        private void UpdateParticleVelocities()
        {
            ParticleSystem.Particle[] p = new ParticleSystem.Particle[m_particleSystem.particleCount];
            int particleCount = m_particleSystem.GetParticles(p);
            Vector3[] pointDeltas = new Vector3[controlPoints.Length];
            pointDeltas[0] = controlPoints[0].position - transform.position;
            for (int i = 1; i < controlPoints.Length; i++)
            {
                pointDeltas[i] = controlPoints[i].position - controlPoints[i - 1].position;

            }

            float totalControlPointsSegment = controlPoints.Length;
            float totalControlPointsSegmentX2 = totalControlPointsSegment * 2;

            for (int i = 0; i < particleCount; i++)
            {
                bool b = false;
                for (int j = 0; j < bezierPointIndices.Length; j++)
                {
                    float f = bezierPointIndices[j];
                    if (p[i].remainingLifetime < (f * p[i].startLifetime / totalControlPointsSegmentX2))
                    {
                        b = true;
                        break;
                    }
                }

                if (p[i].remainingLifetime < (p[i].startLifetime / totalControlPointsSegmentX2))
                {
                    p[i].velocity = totalControlPointsSegment / p[i].startLifetime * pointDeltas[pointDeltas.Length - 1];
                }
                else if (b)
                {
                    for (int j = 0; j < bezierPointIndices.Length; j++)
                    {
                        if (p[i].remainingLifetime < (bezierPointIndices[j] * p[i].startLifetime / totalControlPointsSegmentX2))
                        {

                            int prevBezierPointIndex = j - 1 >= 0 ? (int)bezierPointIndices[j - 1] : 1;
                            float currentSegmentDuration = p[i].startLifetime / totalControlPointsSegment;
                            float remainingLifetimeInCurrentSegment = (p[i].startLifetime / totalControlPointsSegment) - (p[i].remainingLifetime - (prevBezierPointIndex * p[i].startLifetime / (totalControlPointsSegment * 2)));
                            float t = remainingLifetimeInCurrentSegment / currentSegmentDuration;
                            int currentBezierPointIndex = bezierPointIndices.Length - j - 1;
                            Vector3 currentDis = pointDeltas[currentBezierPointIndex];
                            Vector3 prevDis = pointDeltas[currentBezierPointIndex - 1];
                            p[i].velocity = totalControlPointsSegment / p[i].startLifetime * Bezier(prevDis, currentDis, t);
                            break;
                        }
                    }
                }
                else
                {
                    p[i].velocity = totalControlPointsSegment / p[i].startLifetime * pointDeltas[0];
                }
            }

            m_particleSystem.SetParticles(p, particleCount);
        }

        private float[] GenerateBezierPointIndices(int value)
        {
            int count = 0;
            float[] result = new float[value / 2];

            for (int i = 0; i < value; i++)
            {
                if (i % 2 == 1 && i != 1)
                {
                    result[count] = i;
                    count++;
                }
            }
            return result;
        }

        private Vector3 Bezier(Vector3 P0, Vector3 P2, float t)
        {
            Vector3 P1 = (P0 + P2) / 2f;
            Vector3 B;
            B = (1f - t) * ((1f - t) * P0 + t * P1) + t * ((1f - t) * P1 + t * P2);
            return B;
        }
        #endregion
    }

    public struct PathJob : IJobParticleSystem
    {
        [ReadOnly]
        public NativeArray<float> bezierPointIndices;
        [ReadOnly]
        public NativeArray<Vector3> pointDeltas;

        public void Execute(ParticleSystemJobData particles)
        {
            float totalControlPointsSegment = pointDeltas.Length;
            float totalControlPointsSegmentX2 = totalControlPointsSegment * 2;
            Debug.Log($"startLifetime:{particles.inverseStartLifetimes[0]}, aliveTimePercent:{particles.aliveTimePercent[0]}");

            for (int i = 0; i < particles.count; i++)
            {
                float startLifetime = particles.inverseStartLifetimes[i];
                float remainingLifetime = startLifetime * (particles.aliveTimePercent[i] / 100f);
                var velocitiesX = particles.velocities.x;
                var velocitiesY = particles.velocities.y;
                var velocitiesZ = particles.velocities.z;
                bool b = false;
                for (int j = 0; j < bezierPointIndices.Length; j++)
                {
                    float f = bezierPointIndices[j];
                    if (remainingLifetime < (f * startLifetime / totalControlPointsSegmentX2))
                    {
                        b = true;
                        break;
                    }
                }

                if (remainingLifetime < (startLifetime / totalControlPointsSegmentX2))
                {
                    Vector3 velocity = totalControlPointsSegment / startLifetime * pointDeltas[pointDeltas.Length - 1];
                    velocitiesX[i] = velocity.x;
                    velocitiesY[i] = velocity.y;
                    velocitiesZ[i] = velocity.z;
                }
                else if (b)
                {
                    for (int j = 0; j < bezierPointIndices.Length; j++)
                    {
                        if (remainingLifetime < (bezierPointIndices[j] * startLifetime / totalControlPointsSegmentX2))
                        {

                            int prevBezierPointIndex = j - 1 >= 0 ? (int)bezierPointIndices[j - 1] : 1;
                            float currentSegmentDuration = startLifetime / totalControlPointsSegment;
                            float remainingLifetimeInCurrentSegment = (startLifetime / totalControlPointsSegment) - (remainingLifetime - (prevBezierPointIndex * startLifetime / (totalControlPointsSegment * 2)));
                            float t = remainingLifetimeInCurrentSegment / currentSegmentDuration;
                            int currentBezierPointIndex = bezierPointIndices.Length - j - 1;
                            Vector3 currentDis = pointDeltas[currentBezierPointIndex];
                            Vector3 prevDis = pointDeltas[currentBezierPointIndex - 1];
                            Vector3 velocity = totalControlPointsSegment / startLifetime * Bezier(prevDis, currentDis, t);
                            velocitiesX[i] = velocity.x;
                            velocitiesY[i] = velocity.y;
                            velocitiesZ[i] = velocity.z;
                            break;
                        }
                    }
                }
                else
                {
                    Vector3 velocity = totalControlPointsSegment / startLifetime * pointDeltas[0];
                    velocitiesX[i] = velocity.x;
                    velocitiesY[i] = velocity.y;
                    velocitiesZ[i] = velocity.z;
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
    }
}
