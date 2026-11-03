using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>成功放置时播放六帧云尘，枕头回弹只影响外观，不移动槽位或碰撞体。</summary>
    [DisallowMultipleComponent]
    public sealed class PlacementFeedback : MonoBehaviour
    {
        [SerializeField] private Sprite[] cloudFrames;
        private readonly List<Transform> pillowSurfaces = new List<Transform>();
        private readonly List<Puff> puffs = new List<Puff>();
        private Transform board;
        private Vector3 boardScale;
        private float pillowTime = 1;
        private int nextPuff;
        private bool pillow;
        private static readonly float[] Width = { 1.025f, 1.055f, 1.025f, 0.98f, 1.01f, 1 };
        private static readonly float[] Height = { 0.97f, 0.91f, 0.965f, 1.035f, 0.99f, 1 };

        public const float LandingDuration = 0.42f;
        public bool IsPlaying => pillowTime < LandingDuration || puffs.Exists(value => value.Renderer.enabled);

        public void Configure(Transform boardMat, int theme)
        {
            board = boardMat;
            boardScale = board.localScale;
            pillow = theme == 0;
        }

        public void RegisterSurface(Transform surface)
        {
            if (!pillowSurfaces.Contains(surface)) pillowSurfaces.Add(surface);
        }

        public void PlayLanding(Vector3 position)
        {
            if (pillow) pillowTime = 0;
            PlayCloud(position, 4.5f, LandingDuration);
            Advance(0);
        }

        public void PlayReveal(Vector3 position)
        {
            PlayCloud(position, 6f, 0.6f);
        }

        private void PlayCloud(Vector3 position, float size, float duration)
        {
            if (cloudFrames == null || cloudFrames.Length == 0) return;
            // 固定上限，连续快速放置时复用最早的一团云，不无限生成对象。
            Puff puff;
            if (puffs.Count < 8)
            {
                GameObject child = new GameObject("CartoonCloud");
                child.transform.SetParent(transform, false);
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                renderer.sortingLayerName = "Puzzle";
                renderer.sortingOrder = 100;
                puff = new Puff { Renderer = renderer };
                puffs.Add(puff);
            }
            else
            {
                puff = puffs[nextPuff];
                nextPuff = (nextPuff + 1) % puffs.Count;
            }
            puff.Age = 0;
            puff.Duration = duration;
            puff.Renderer.transform.position = position;
            puff.Renderer.transform.localScale = Vector3.one * size / cloudFrames[0].bounds.size.x;
            puff.Renderer.sprite = cloudFrames[0];
            puff.Renderer.enabled = true;
        }

        private void Update() => Advance(Time.deltaTime);

        /// <summary>推进动画，可用固定步长重复验证六帧和回弹终态。</summary>
        public void Advance(float seconds)
        {
            pillowTime += Mathf.Max(0, seconds);
            Vector3 scale = Vector3.one;
            if (pillow && pillowTime < LandingDuration)
            {
                int frame = Mathf.Min(5, (int)(pillowTime / LandingDuration * 6));
                scale = new Vector3(Width[frame], Height[frame], 1);
            }
            if (board != null) board.localScale = Vector3.Scale(boardScale, scale);
            foreach (Transform surface in pillowSurfaces)
                if (surface != null) surface.localScale = scale;
            foreach (Puff puff in puffs)
            {
                if (!puff.Renderer.enabled) continue;
                puff.Age += Mathf.Max(0, seconds);
                if (puff.Age >= puff.Duration)
                {
                    puff.Renderer.enabled = false;
                    continue;
                }
                int frame = Mathf.Min(cloudFrames.Length - 1, (int)(puff.Age / puff.Duration * cloudFrames.Length));
                puff.Renderer.sprite = cloudFrames[frame];
            }
        }

        private void OnDisable()
        {
            pillowTime = LandingDuration;
            if (board != null) board.localScale = boardScale;
            foreach (Transform surface in pillowSurfaces)
                if (surface != null) surface.localScale = Vector3.one;
            foreach (Puff puff in puffs)
                if (puff.Renderer != null) puff.Renderer.enabled = false;
        }

        private sealed class Puff
        {
            public SpriteRenderer Renderer;
            public float Age;
            public float Duration;
        }
    }
}
