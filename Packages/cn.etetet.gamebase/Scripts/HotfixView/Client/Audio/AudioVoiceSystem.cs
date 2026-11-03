using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(AudioVoice))]
    [FriendOf(typeof(AudioVoice))]
    public static partial class AudioVoiceSystem
    {
        [EntitySystem]
        private static void Awake(this AudioVoice self)
        {
            self.Loading = true;
            self.Paused = false;
            self.Stopping = false;
            self.Gain = 1f;
            self.FadeDuration = 0f;
            self.FadeElapsed = 0f;
        }

        [EntitySystem]
        private static void Destroy(this AudioVoice self)
        {
            // 管理器正常回收时已移走 Source；父实体直接销毁时由此处兜底清理。
            if (self.Source != null)
            {
                self.Source.Stop();
                self.Source.clip = null;
                UnityEngine.Object.Destroy(self.Source.gameObject);
                self.Source = null;
            }
            self.Handle?.Release();
            self.Handle = null;
        }
    }
}
