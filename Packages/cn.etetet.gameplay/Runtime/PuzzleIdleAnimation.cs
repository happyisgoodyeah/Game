using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// 持续播放拼图的待机帧，仅更新画面，不修改变换或碰撞区域。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PuzzleIdleAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames = new Sprite[0];
        [SerializeField, Min(0.1f)] private float framesPerSecond = 5f;

        private SpriteRenderer spriteRenderer;
        private double elapsed;
        private int currentFrame = -1;

        public int FrameCount => frames.Length;
        public float FramesPerSecond => framesPerSecond;

        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            elapsed = 0;
            currentFrame = -1;
            ShowFrame(0);
        }

        private void Update()
        {
            if (frames.Length == 0)
            {
                return;
            }

            // 使用独立时间，暂停玩法或拖拽时也继续播放待机动画。
            double duration = frames.Length / (double)Mathf.Max(0.1f, framesPerSecond);
            elapsed = (elapsed + Time.unscaledDeltaTime) % duration;
            int index = Mathf.Min((int)(elapsed * Mathf.Max(0.1f, framesPerSecond)), frames.Length - 1);
            ShowFrame(index);
        }

        private void ShowFrame(int index)
        {
            if (frames.Length == 0 || index == currentFrame || frames[index] == null)
            {
                return;
            }

            currentFrame = index;
            spriteRenderer.sprite = frames[index];
        }
    }
}
