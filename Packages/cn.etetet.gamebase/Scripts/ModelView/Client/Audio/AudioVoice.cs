using UnityEngine;
using YooAsset;

namespace ET
{
    /// <summary>一次播放请求。加载期间也占用并发名额，资源句柄由该实例独占。</summary>
    [ChildOf(typeof(AudioComponent))]
    public partial class AudioVoice : Entity, IAwake, IDestroy
    {
        public int ConfigId;
        public AudioGroup Group;
        public AudioSource Source;
        public AssetHandle Handle;
        public bool Loading;
        public bool Paused;
        public bool Stopping;
        public float BaseVolume;
        public float Gain;
        public float FadeFrom;
        public float FadeTo;
        public float FadeDuration;
        public float FadeElapsed;
        public float DefaultFadeOut;
    }
}
