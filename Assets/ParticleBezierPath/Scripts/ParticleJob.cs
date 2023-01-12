using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class ParticleJob : MonoBehaviour
{
    public float effectRange = 2.0f;
    public float effectStrength = 1.0f;
    public float oscillationSpeed = 12.0f;
    public bool useJobSystem = false;

    private float oscillationPhase;

    private ParticleSystem ps;
    private UpdateParticlesJob job = new UpdateParticlesJob();
    private ParticleSystem.Particle[] mainThreadParticles;

    private static float Remap(float x, float x1, float x2, float y1, float y2)
    {
        var m = (y2 - y1) / (x2 - x1);
        var c = y1 - m * x1;

        return m * x + c;
    }

    private static Vector3 CalculateVelocity(ref UpdateParticlesJob job, Vector3 delta)
    {
        float attraction = job.effectStrength / job.effectRangeSqr;
        return delta.normalized * attraction;
    }

    private static Color32 CalculateColor(ref UpdateParticlesJob job, Vector3 delta, Color32 srcColor, UInt32 seed)
    {
        var targetColor = new Color32((byte)(seed >> 24), (byte)(seed >> 16), (byte)(seed >> 8), srcColor.a);
        float lerpAmount = delta.magnitude * job.inverseEffectRange;
        lerpAmount = lerpAmount * 0.25f + 0.75f;
        return Color32.Lerp(targetColor, srcColor, lerpAmount);
    }

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        oscillationPhase = UnityEngine.Random.Range(0.0f, Mathf.PI * 2.0f);
    }

    void Update()
    {
        Vector2 mouse = Input.mousePosition;
        job.effectPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, -Camera.main.transform.position.z));
        job.effectRangeSqr = effectRange * effectRange;
        job.effectStrength = effectStrength * Remap(Mathf.Sin(oscillationPhase + Time.time * oscillationSpeed), -1, 1, -1, 0.5f);
        job.inverseEffectRange = (1.0f / effectRange);

        if (!useJobSystem)
        {
            if (mainThreadParticles == null)
                mainThreadParticles = new ParticleSystem.Particle[ps.main.maxParticles];

            var count = ps.GetParticles(mainThreadParticles);
            for (int i = 0; i < count; i++)
            {
                Vector3 position = mainThreadParticles[i].position;
                Vector3 delta = position - job.effectPosition;
                if (delta.sqrMagnitude < job.effectRangeSqr)
                {
                    mainThreadParticles[i].velocity += CalculateVelocity(ref job, delta);
                    mainThreadParticles[i].startColor = CalculateColor(ref job, delta, mainThreadParticles[i].startColor, mainThreadParticles[i].randomSeed);
                }
            }
            ps.SetParticles(mainThreadParticles, count);
        }
    }

    void OnParticleUpdateJobScheduled()
    {
        if (useJobSystem)
            job.Schedule(ps);
    }

    struct UpdateParticlesJob : IJobParticleSystem
    {
        [ReadOnly]
        public Vector3 effectPosition;

        [ReadOnly]
        public float effectRangeSqr;

        [ReadOnly]
        public float effectStrength;

        [ReadOnly]
        public float inverseEffectRange;

        public void Execute(ParticleSystemJobData particles)
        {
            var positionsX = particles.positions.x;
            var positionsY = particles.positions.y;
            var positionsZ = particles.positions.z;

            var velocitiesX = particles.velocities.x;
            var velocitiesY = particles.velocities.y;
            var velocitiesZ = particles.velocities.z;

            var colors = particles.startColors;

            var randomSeeds = particles.randomSeeds;

            for (int i = 0; i < particles.count; i++)
            {
                Vector3 position = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);
                Vector3 delta = (position - effectPosition);
                if (delta.sqrMagnitude < effectRangeSqr)
                {
                    Vector3 velocity = CalculateVelocity(ref this, delta);

                    velocitiesX[i] += velocity.x;
                    velocitiesY[i] += velocity.y;
                    velocitiesZ[i] += velocity.z;

                    colors[i] = CalculateColor(ref this, delta, colors[i], randomSeeds[i]);
                }
            }
        }
    }
}