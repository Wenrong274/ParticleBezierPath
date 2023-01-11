using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ParticleFollower : MonoBehaviour
{
    [SerializeField] private PathCreator pathCreator;
    [SerializeField] private EndOfPathInstruction endOfPathInstruction;

    private ParticleSystem particle;

    void Start()
    {
        particle = GetComponent<ParticleSystem>();

        if (pathCreator != null)
        {
            pathCreator.pathUpdated += OnPathChanged;
        }
    }

    private void Update()
    {
        UpdateParticleVelocities();
    }

    private void UpdateParticleVelocities()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particle.particleCount];
        int particleCount = particle.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            float remainingLifetimeRatio = 1f - (particles[i].remainingLifetime / particles[i].startLifetime);
            Debug.Log(remainingLifetimeRatio);
            Vector3 pathCreatorDirection = pathCreator.path.GetPointAtTime(remainingLifetimeRatio, endOfPathInstruction);
            Vector3 Direction = pathCreatorDirection - particles[i].position;

            particles[i].velocity = Direction.normalized*10;

        }
        particle.SetParticles(particles, particleCount);
    }

    private void OnPathChanged()
    {

    }
}
