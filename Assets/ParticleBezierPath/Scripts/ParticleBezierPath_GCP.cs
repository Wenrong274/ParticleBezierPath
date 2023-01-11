using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBezierPath_GCP : MonoBehaviour
{
    public Transform[] controlPoints;
    private ParticleSystem particle;
    private float[] bezierPointIndices;

    void Start()
    {
        particle = GetComponent<ParticleSystem>();
        bezierPointIndices = GenerateBezierPointIndices(controlPoints.Length);
    }

    private void Update()
    {
        UpdateParticleVelocities();
    }

    private void UpdateParticleVelocities()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particle.particleCount];
        int particleCount = particle.GetParticles(particles);

        Vector3[] pointDeltas = new Vector3[controlPoints.Length];
        pointDeltas[0] = controlPoints[0].position - transform.position;
        for (int i = 1; i < controlPoints.Length; i++)
        {
            pointDeltas[i] = controlPoints[i].position - controlPoints[i - 1].position;
        }
        for (int i = 0; i < particleCount; i++)
        {
            float remainingLifetimeRatio = particles[i].remainingLifetime / particles[i].startLifetime;
            int bezierPointIndex = GetBezierPointIndex(remainingLifetimeRatio);
            float t = GetInterpolationRatio(remainingLifetimeRatio, bezierPointIndex);

            Vector3 D1 = pointDeltas[bezierPointIndex - 1], D2 = pointDeltas[bezierPointIndex];
            particles[i].velocity = (1f / controlPoints.Length) * GetBezierPoint(D1, D2, t);
        }

        particle.SetParticles(particles, particleCount);
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
    private int GetBezierPointIndex(float remainingLifetimeRatio)
    {
        for (int i = 0; i < bezierPointIndices.Length; i++)
        {
            float f = bezierPointIndices[i];
            if (remainingLifetimeRatio < (f * (1f / controlPoints.Length)))
            {
                return (int)f;
            }
        }
        return 0;
    }
    private float GetInterpolationRatio(float remainingLifetimeRatio, int bezierPointIndex)
    {
        float bezierPointRatio = bezierPointIndex * (1f / controlPoints.Length);
        float t = (remainingLifetimeRatio - bezierPointRatio) / (1f / controlPoints.Length);
        return t;
    }

    private Vector3 GetBezierPoint(Vector3 P0, Vector3 P2, float t)
    {
        Vector3 P1 = (P0 + P2) / 2f;
        Vector3 B;
        B = (1f - t) * ((1f - t) * P0 + t * P1) + t * ((1f - t) * P1 + t * P2);
        return B;
    }
}