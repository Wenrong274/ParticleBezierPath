using UnityEngine;
using UnityEngine.UI;

namespace hyhy
{
    public class ShowFPS : MonoBehaviour
    {
        public float updateInterval = 0.5f;
        [SerializeField] private Text fpsText;
        private int frames = 0;
        private float accum = 0.0f;
        private float timeleft;
        private float fps;

        private void Start()
        {
            timeleft = updateInterval;
        }

        private void Update()
        {
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            if (timeleft <= 0.0)
            {
                fps = (accum / frames);
                timeleft = updateInterval;
                accum = 0.0f;
                frames = 0;
            }

            fpsText.text = $"FPS:{fps:F2}";
        }
    }
}
