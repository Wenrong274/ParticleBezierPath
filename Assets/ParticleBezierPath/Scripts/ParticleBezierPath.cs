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
        [SerializeField] private bool IsJob;
        private Vector3[] pointDeltas;

        public void SetJob(bool ob)
        {
            IsJob = ob;
        }

        void Start()
        {
            m_particleSystem = GetComponent<ParticleSystem>();
            bezierPointIndices = GenerateBezierPointIndices(controlPoints.Length * 2);
        }

        private void Update()
        {
            pointDeltas = new Vector3[controlPoints.Length];
            pointDeltas[0] = controlPoints[0].position - transform.position;
            for (int i = 1; i < controlPoints.Length; i++)
                pointDeltas[i] = controlPoints[i].position - controlPoints[i - 1].position;
            if (IsJob)
                UpdateParticleVelocitiesJob();
            else
                UpdateParticleVelocities();
        }

        private void UpdateParticleVelocitiesJob()
        {
            ParticleBezierPathJob job = new();
            NativeArray<float> bezierPointIndicesArray = new(bezierPointIndices, Allocator.TempJob);
            NativeArray<Vector3> pointDeltasArray = new(pointDeltas, Allocator.TempJob);
            job.bezierPointIndices = bezierPointIndicesArray;
            job.pointDeltas = pointDeltasArray;
            job.lifetime = m_particleSystem.main.startLifetime.constant;
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
            float pointsSegment = pointDeltas.Length;
            float pointsSegmentX2 = pointsSegment * 2;

            for (int i = 0; i < particleCount; i++)
            {
                bool b = false;
                for (int j = 0; j < bezierPointIndices.Length; j++)
                {
                    float f = bezierPointIndices[j];
                    if (p[i].remainingLifetime < (f * p[i].startLifetime / pointsSegmentX2))
                    {
                        b = true;
                        break;
                    }
                }

                if (p[i].remainingLifetime < (p[i].startLifetime / pointsSegmentX2))
                {
                    p[i].velocity = pointsSegment / p[i].startLifetime * pointDeltas[pointDeltas.Length - 1];
                }
                else if (b)
                {
                    for (int j = 0; j < bezierPointIndices.Length; j++)
                    {
                        if (p[i].remainingLifetime < (bezierPointIndices[j] * p[i].startLifetime / pointsSegmentX2))
                        {

                            int prevBezierPointIndex = j - 1 >= 0 ? (int)bezierPointIndices[j - 1] : 1;
                            float currentSegmentDuration = p[i].startLifetime / pointsSegment;
                            float remainingLifetimeInCurrentSegment = (p[i].startLifetime / pointsSegment) - (p[i].remainingLifetime - (prevBezierPointIndex * p[i].startLifetime / (pointsSegment * 2)));
                            float t = remainingLifetimeInCurrentSegment / currentSegmentDuration;
                            int currentBezierPointIndex = bezierPointIndices.Length - j - 1;
                            Vector3 currentDis = pointDeltas[currentBezierPointIndex];
                            Vector3 prevDis = pointDeltas[currentBezierPointIndex - 1];
                            p[i].velocity = pointsSegment / p[i].startLifetime * Bezier(prevDis, currentDis, t);
                            break;
                        }
                    }
                }
                else
                {
                    p[i].velocity = pointsSegment / p[i].startLifetime * pointDeltas[0];
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
}
