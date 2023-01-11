using UnityEngine;
using UnityEngine.UI;

namespace hyhy
{
    public class PController : MonoBehaviour
    {
        [SerializeField] private Toggle Toggle;
        [SerializeField] private Text PsDate;
        private ParticleBezierPath particleBezierPath;
        private ParticleSystem ps;
        private void Awake()
        {
            particleBezierPath = FindObjectOfType<ParticleBezierPath>();
            if (particleBezierPath)
                ps = particleBezierPath.GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            SetJob(Toggle.isOn);
        }

        private void Update()
        {
            if (ps)
            {
                PsDate.text = $"maxParticles: {ps.main.maxParticles}\n" +
                              $"particleCount: {ps.particleCount}";
            }
        }

        public void SetJob(bool on)
        {
            particleBezierPath.SetJob(on);
        }
    }
}
