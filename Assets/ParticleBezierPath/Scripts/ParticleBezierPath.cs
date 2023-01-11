using UnityEngine;

namespace hyhy
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleBezierPath : MonoBehaviour
    {
        public Transform[] paths;
        private ParticleSystem particle;
       [SerializeField] private float[] singu;

        void Start()
        {
            particle = GetComponent<ParticleSystem>();
            singu = SinguList(paths.Length * 2);
        }

        private void Update()
        {
            Trail();
        }

        private void Trail()
        {
            ParticleSystem.Particle[] p = new ParticleSystem.Particle[particle.particleCount + 1];
            int l = particle.GetParticles(p);
            Vector3[] Dis_List = new Vector3[paths.Length];
            Dis_List[0] = paths[0].position - transform.position;
            for (int j = 1; j < paths.Length; j++)
            {
                Dis_List[j] = paths[j].position - paths[j - 1].position;

            }

            int i = 0;
            float ListLen = paths.Length;
            float ListLen2 = ListLen * 2;
            while (i < l)
            {
                bool b = false;
                for (int k = 0; k < singu.Length; k++)
                {
                    float f = singu[k];
                    if (p[i].remainingLifetime < (f * p[i].startLifetime / ListLen2))
                    {
                        b = true;
                        break;
                    }
                }

                if (p[i].remainingLifetime < (p[i].startLifetime / ListLen2))
                {
                    p[i].velocity = ListLen / p[i].startLifetime * Dis_List[Dis_List.Length - 1];
                }
                else if (b)
                {
                    for (int j = 0; j < singu.Length; j++)
                    {
                        if (p[i].remainingLifetime < (singu[j] * p[i].startLifetime / ListLen2))
                        {
                            int n = 1;
                            try
                            {
                                n = (int)singu[j - 1];
                            }
                            catch
                            {

                            }
                            int ID = (singu.Length - j - 1);

                            float t = ((p[i].startLifetime / ListLen) - (p[i].remainingLifetime - ((n * p[i].startLifetime) / ListLen2))) / (p[i].startLifetime / ListLen);

                            Vector3 D1 = Dis_List[ID - 1], D2 = Dis_List[ID];
                            p[i].velocity = ListLen / p[i].startLifetime * Bezier(D1, D2, t);
                            break;
                        }
                    }
                }
                else
                {
                    p[i].velocity = ListLen / p[i].startLifetime * Dis_List[0];
                }
                i++;
            }
            particle.SetParticles(p, l);
        }

        private float[] SinguList(int value)
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
    }
}
