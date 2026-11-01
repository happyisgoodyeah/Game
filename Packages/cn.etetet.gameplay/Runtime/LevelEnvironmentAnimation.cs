using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// 驱动独立风摆层和固定数量的环境粒子。解谜挂点不参与动画，无碰撞和输入组件。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelEnvironmentAnimation : MonoBehaviour
    {
        public enum WeatherEffect { None, Sakura, Leaves }

        [SerializeField, Tooltip("无天气、樱花飘落或间歇落叶。")] private WeatherEffect effect;
        [SerializeField] private Sprite particleSprite;
        [SerializeField, Range(1, 32)] private int particleCount = 20;
        [SerializeField, Min(0.1f), Tooltip("环境播放速度，使用不受玩法暂停影响的独立时间。")] private float playbackSpeed = 1f;
        [SerializeField, Min(8f), Tooltip("阵风间隔（秒）；自动保证至少三秒静止时间。")] private float gustInterval = 16f;
        [SerializeField, Range(3f, 7f), Tooltip("每片落叶飘过画面的持续秒数。")] private float gustDuration = 6f;
        [SerializeField] private bool automaticPlayback = true;

        private EnvironmentSwayLayer[] swayingLayers;
        private SpriteRenderer[] particles;
        private double elapsed;

        public int ActiveParticleCount { get; private set; }
        public WeatherEffect Effect => effect;

        private void OnEnable()
        {
            elapsed = 0;
            Sample(0);
        }

        private void Update()
        {
            if (!automaticPlayback) return;
            elapsed += Time.unscaledDeltaTime * playbackSpeed;
            Sample(elapsed);
        }

        /// <summary>使用绝对时间采样，供可重复的预览和实际运行共同使用。</summary>
        public void Sample(double seconds)
        {
            seconds = System.Math.Max(0, seconds);
            if (swayingLayers == null) swayingLayers = GetComponentsInChildren<EnvironmentSwayLayer>(true);
            foreach (EnvironmentSwayLayer layer in swayingLayers)
                if (layer != null && layer.isActiveAndEnabled) layer.Sample(seconds);

            if (effect == WeatherEffect.None || particleSprite == null) return;
            EnsureParticles();
            ActiveParticleCount = 0;
            for (int i = 0; i < particles.Length; ++i)
            {
                SpriteRenderer particle = particles[i];
                float seed = Noise(i, 1);
                float age;
                float lifetime;
                Vector3 position;
                if (effect == WeatherEffect.Sakura)
                {
                    lifetime = 13f + Noise(i, 2) * 8f;
                    age = (float)((seconds + seed * lifetime) % lifetime);
                    float progress = age / lifetime;
                    // 由右上方的樱花树冠进入画面，缓慢下落并左右翻飞。
                    position = new Vector3(9f + Noise(i, 3) * 13f - age * (0.55f + seed * 0.5f) +
                        Mathf.Sin(age * 1.6f + i) * 0.7f, 12.8f - progress * 27f, 0);
                }
                else
                {
                    lifetime = gustDuration;
                    float interval = Mathf.Max(gustInterval, 1.5f + (particles.Length - 1) * 0.22f + gustDuration + 3);
                    float cycle = (float)(seconds % interval);
                    age = cycle - 1.5f - i * 0.22f;
                    bool active = age >= 0 && age < lifetime;
                    particle.enabled = active;
                    if (!active) continue;
                    // 每阵风之间保留明显静止间隔，落叶沿露台下半部飘过。
                    position = new Vector3(-24f + age / lifetime * 49f,
                        -9f + Noise(i, 4) * 3.5f + Mathf.Sin(age * 1.8f + i) * 0.8f, 0);
                }

                particle.enabled = true;
                particle.transform.localPosition = position;
                particle.transform.localRotation = Quaternion.Euler(0, 0, seed * 360 + age * (25 + i * 3));
                float flutter = 0.55f + Mathf.Abs(Mathf.Sin(age * 2.1f + i)) * 0.45f;
                float scale = effect == WeatherEffect.Sakura ? 0.2f + seed * 0.1f : 0.24f + seed * 0.12f;
                particle.transform.localScale = new Vector3(scale * flutter, scale, 1);
                float fade = Mathf.Min(Mathf.Clamp01(age / 0.65f), Mathf.Clamp01((lifetime - age) / 1.2f));
                particle.color = new Color(1, 1, 1, fade * 0.82f);
                ++ActiveParticleCount;
            }
        }

        private void EnsureParticles()
        {
            if (particles != null) return;
            Transform weather = transform.Find("Weather");
            if (weather == null)
            {
                GameObject root = new GameObject("Weather");
                root.transform.SetParent(transform, false);
                weather = root.transform;
            }
            particles = new SpriteRenderer[Mathf.Clamp(particleCount, 1, 32)];
            for (int i = 0; i < particles.Length; ++i)
            {
                GameObject child = new GameObject($"{effect}_{i:00}");
                child.transform.SetParent(weather, false);
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = particleSprite;
                renderer.sortingLayerName = "BackGround";
                renderer.sortingOrder = 120;
                particles[i] = renderer;
            }
        }

        private static float Noise(int index, int salt)
        {
            return Mathf.Repeat(Mathf.Sin(index * 127.1f + salt * 311.7f) * 43758.5453f, 1f);
        }

        /// <summary>预览保存前移除临时粒子，运行时仍复用固定数量的对象。</summary>
        public void ClearPreview()
        {
            if (swayingLayers != null)
                foreach (EnvironmentSwayLayer layer in swayingLayers)
                    if (layer != null) layer.ReleaseMesh();
            if (particles == null) return;
            foreach (SpriteRenderer particle in particles)
            {
                if (particle == null) continue;
                if (Application.isPlaying) Destroy(particle.gameObject);
                else DestroyImmediate(particle.gameObject);
            }
            particles = null;
            ActiveParticleCount = 0;
        }

        private void OnDisable()
        {
            ActiveParticleCount = 0;
            if (particles != null)
                foreach (SpriteRenderer particle in particles)
                    if (particle != null) particle.enabled = false;
        }
    }
}
