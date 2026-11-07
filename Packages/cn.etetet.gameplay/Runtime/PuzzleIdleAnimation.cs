using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// 播放拼图的待机和拎起帧，仅更新画面，不修改变换或碰撞区域。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PuzzleIdleAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames = new Sprite[0];
        [SerializeField, Min(0.1f)] private float framesPerSecond = 5f;
        [SerializeField] private Sprite[] pickedUpFrames = new Sprite[0];
        [SerializeField, Min(0.1f)] private float pickedUpFramesPerSecond = 8f;
        [SerializeField, Min(0)] private int heldLoopStart = 2;

        private SpriteRenderer spriteRenderer;
        private double elapsed;
        private int currentFrame = -1;
        private bool isPickedUp;

        public int FrameCount => frames.Length;
        public float FramesPerSecond => framesPerSecond;
        public int PickedUpFrameCount => pickedUpFrames.Length;
        public bool IsPickedUp => isPickedUp;

        private void OnEnable()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            isPickedUp = false;
            elapsed = 0;
            currentFrame = -1;
            ShowFrame(0);
        }

        private void Update()
        {
            Advance(Time.unscaledDeltaTime);
        }

        /// <summary>输入事件切换表现状态；重复拖拽开始事件不会重播拎起过程。</summary>
        public void SetPickedUp(bool value)
        {
            value = value && pickedUpFrames.Length > 0;
            if (isPickedUp == value) return;
            isPickedUp = value;
            elapsed = 0;
            currentFrame = -1;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            ShowFrame(0);
        }

        private void OnDisable() => SetPickedUp(false);

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetPickedUp(false);
        }

        /// <summary>待机完整循环；拎起的开头播放一次，然后循环悬停段。可确定性采样验证。</summary>
        public void Advance(float deltaTime)
        {
            Sprite[] activeFrames = isPickedUp ? pickedUpFrames : frames;
            if (activeFrames.Length == 0)
            {
                return;
            }

            // 使用独立时间，暂停玩法或拖拽时也继续播放待机动画。
            double fps = Mathf.Max(0.1f, isPickedUp ? pickedUpFramesPerSecond : framesPerSecond);
            int loopStart = isPickedUp ? Mathf.Clamp(heldLoopStart, 0, activeFrames.Length - 1) : 0;
            double duration = activeFrames.Length / fps;
            elapsed += Mathf.Max(0, deltaTime);
            if (elapsed >= duration)
                elapsed = loopStart / fps + (elapsed - duration) % ((activeFrames.Length - loopStart) / fps);
            int index = Mathf.Min((int)(elapsed * fps), activeFrames.Length - 1);
            ShowFrame(index);
        }

        private void ShowFrame(int index)
        {
            Sprite[] activeFrames = isPickedUp ? pickedUpFrames : frames;
            if (activeFrames.Length == 0 || index == currentFrame || activeFrames[index] == null)
            {
                return;
            }

            currentFrame = index;
            spriteRenderer.sprite = activeFrames[index];
        }
    }
}
