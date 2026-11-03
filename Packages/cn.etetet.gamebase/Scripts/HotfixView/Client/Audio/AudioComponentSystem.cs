using System;
using UnityEngine;
using YooAsset;

namespace ET
{
    [EntitySystemOf(typeof(AudioComponent))]
    [FriendOf(typeof(AudioComponent))]
    [FriendOf(typeof(AudioVoice))]
    public static partial class AudioComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AudioComponent self)
        {
            self.Package = YooAssets.GetPackage("DefaultPackage");
            self.AudioRoot = new GameObject("AudioRuntime").transform;
            self.AudioRoot.SetParent(self.Root().GetComponent<GlobalComponent>().Audio, false);
            self.MaxVoices = 32;
            self.MaxPooledSources = 16;
            self.MasterVolume = ReadVolume("Master");
            self.Muted = PlayerPrefs.GetInt("PuzzleGame.Audio.Muted", 0) != 0;
            self.PreferencesDirty = false;
            for (int i = 0; i < self.GroupVolumes.Length; ++i)
            {
                self.GroupVolumes[i] = ReadVolume(((AudioGroup)i).ToString());
                self.GroupMutes[i] = PlayerPrefs.GetInt($"PuzzleGame.Audio.{(AudioGroup)i}.Muted", 0) != 0;
                self.GroupPauses[i] = false;
            }
            self.ReloadConfigIndex();
        }

        [EntitySystem]
        private static void Update(this AudioComponent self)
        {
            self.Tick(Time.unscaledDeltaTime);
        }

        [EntitySystem]
        private static void Destroy(this AudioComponent self)
        {
            // ET 先销毁子实体，其 AudioVoiceSystem 负责正在播放的资源。
            self.SaveSettings();
            foreach (AudioSource source in self.SourcePool)
            {
                if (source != null) UnityEngine.Object.Destroy(source.gameObject);
            }
            if (self.AudioRoot != null) UnityEngine.Object.Destroy(self.AudioRoot.gameObject);
            self.SourcePool.Clear();
            self.Voices.Clear();
            self.ScratchIds.Clear();
            self.LastRequests.Clear();
            self.ConfigIds.Clear();
            self.AudioRoot = null;
            self.Package = null;
        }

        /// <summary>配置重新加载后调用。重复 Key 的所有记录均拒绝建立名称索引。</summary>
        public static void ReloadConfigIndex(this AudioComponent self)
        {
            self.ConfigIds.Clear();
            foreach (AudioConfig config in AudioConfigCategory.Instance.DataList)
            {
                if (!ValidateConfig(config, out string error))
                {
                    self.Fiber().Log.Error($"音频配置 {config.Id} 无效: {error}");
                    continue;
                }
                if (self.ConfigIds.ContainsKey(config.Key))
                {
                    self.ConfigIds[config.Key] = -1;
                    self.Fiber().Log.Error($"音频 Key 重复: {config.Key}");
                    continue;
                }
                self.ConfigIds.Add(config.Key, config.Id);
            }
        }

        public static bool ValidateConfig(AudioConfig config, out string error)
        {
            error = null;
            if (config == null) error = "配置不存在";
            else if (config.Id < 0 || string.IsNullOrWhiteSpace(config.Key) || config.Key.Trim() != config.Key ||
                     string.IsNullOrWhiteSpace(config.Path)) error = "Id、Key 或 Path 无效";
            else if (!Enum.TryParse(config.Group, out AudioGroup group) || !Enum.IsDefined(typeof(AudioGroup), group)) error = "Group 无效";
            else if (!Finite(config.Volume) || config.Volume < 0f || config.Volume > 1f) error = "Volume 必须为 0~1";
            else if (!Finite(config.PitchMin) || !Finite(config.PitchMax) || config.PitchMin < 0.1f ||
                     config.PitchMax > 3f || config.PitchMin > config.PitchMax) error = "Pitch 必须为 0.1~3，且 Min <= Max";
            else if (!Finite(config.FadeIn) || !Finite(config.FadeOut) || config.FadeIn < 0f || config.FadeOut < 0f) error = "淡入淡出时间无效";
            else if (config.MaxInstances < 1 || config.CooldownMs < 0 || config.Priority < 0 || config.Priority > 256) error = "并发数、冷却或优先级无效";
            else if (config.Overflow != "Reject" && config.Overflow != "ReplaceOldest") error = "Overflow 必须为 Reject 或 ReplaceOldest";
            return error == null;
        }

        public static async ETTask<AudioPlayResult> PlayAsync(this AudioComponent self, string key)
        {
            if (self == null || self.IsDisposed) return Result(AudioPlayStatus.Cancelled);
            if (key == null || !self.ConfigIds.TryGetValue(key, out int id) || id < 0)
                return Result(AudioPlayStatus.InvalidConfig);
            return await self.PlayAsync(id);
        }

        public static async ETTask<AudioPlayResult> PlayAsync(this AudioComponent self, int configId)
        {
            return await self.PlayInternalAsync(configId, null);
        }

        private static async ETTask<AudioPlayResult> PlayInternalAsync(this AudioComponent self, int configId, bool? loopOverride)
        {
            if (self == null || self.IsDisposed) return Result(AudioPlayStatus.Cancelled);
            AudioConfig config = AudioConfigCategory.Instance.GetOrDefault(configId);
            if (!ValidateConfig(config, out string error))
            {
                self.Fiber().Log.Error($"音频配置 {configId} 无效: {error}");
                return Result(AudioPlayStatus.InvalidConfig);
            }
            AudioGroup group = (AudioGroup)Enum.Parse(typeof(AudioGroup), config.Group);
            if (group == AudioGroup.Music)
            {
                // 新请求使旧加载失效；旧音乐一直保留到新资源成功加载。
                self.PruneMusicRequests();
                foreach (EntityRef<AudioVoice> item in self.Voices.Values)
                {
                    AudioVoice existing = item;
                    if (existing.Group == group && existing.ConfigId == configId && !existing.Stopping)
                        return Result(AudioPlayStatus.AlreadyPlaying, existing.Id);
                }
            }
            double now = Time.realtimeSinceStartupAsDouble;
            if (self.LastRequests.TryGetValue(configId, out double last) && (now - last) * 1000d < config.CooldownMs)
                return Result(AudioPlayStatus.Cooldown);

            AudioVoice oldest = null;
            int count = 0;
            foreach (EntityRef<AudioVoice> item in self.Voices.Values)
            {
                AudioVoice active = item;
                if (active.ConfigId != configId) continue;
                ++count;
                if (oldest == null || active.Id < oldest.Id) oldest = active;
            }
            if (count >= config.MaxInstances)
            {
                if (config.Overflow == "Reject") return Result(AudioPlayStatus.CapacityReached);
                self.Recycle(oldest.Id);
            }
            if (self.Voices.Count >= self.MaxVoices) return Result(AudioPlayStatus.CapacityReached);

            AudioVoice voice = self.AddChild<AudioVoice>();
            voice.ConfigId = configId;
            voice.Group = group;
            voice.BaseVolume = config.Volume;
            voice.DefaultFadeOut = config.FadeOut;
            long id = voice.Id;
            self.Voices.Add(id, voice);
            self.LastRequests[configId] = now;
            EntityRef<AudioComponent> selfRef = self;
            EntityRef<AudioVoice> voiceRef = voice;
            AssetHandle handle = null;
            try
            {
                // 加载期间由本方法持有句柄，播放后转交给 AudioVoice。
                // 不使用 Root ResourcesLoader 的永久缓存，避免音频资源直到退出才释放。
                handle = self.Package.LoadAssetAsync<AudioClip>(config.Path);
                await handle.Task;
                self = selfRef;
                voice = voiceRef;
                if (self == null || voice == null) return Result(AudioPlayStatus.Cancelled);
                if (handle.Status != EOperationStatus.Succeed || handle.AssetObject is not AudioClip clip)
                {
                    self.Fiber().Log.Error($"音频加载失败 {config.Key}: {handle.LastError}");
                    self.Recycle(id);
                    return Result(AudioPlayStatus.Failed);
                }

                AudioSource source = self.RentSource();
                voice.Source = source;
                voice.Handle = handle;
                handle = null;
                voice.Loading = false;
                source.clip = clip;
                source.loop = loopOverride ?? config.Loop;
                source.pitch = UnityEngine.Random.Range(config.PitchMin, config.PitchMax);
                source.priority = config.Priority;
                voice.Gain = config.FadeIn > 0f ? 0f : 1f;
                SetFade(voice, 1f, config.FadeIn);
                self.ApplyVolume(voice);
                source.Play();
                if (voice.Paused || self.GroupPauses[(int)group]) source.Pause();
                if (group == AudioGroup.Music) self.FadePreviousMusic(id);
                return Result(AudioPlayStatus.Started, id);
            }
            catch (Exception exception)
            {
                self = selfRef;
                if (self == null) return Result(AudioPlayStatus.Cancelled);
                self.Fiber().Log.Error($"播放音频失败 {configId}: {exception}");
                self.Recycle(id);
                return Result(AudioPlayStatus.Failed);
            }
            finally
            {
                handle?.Release();
            }
        }

        public static AudioConfig Config(this AudioComponent self, AUDIO type)
        {
            return AudioConfigCategory.Instance.Get((int)type);
        }

        /// <summary>旧接口兼容；新代码使用 PlayAsync 获得状态和播放句柄。</summary>
        public static async ETTask PlayAudioOne(this AudioComponent self, AUDIO type, bool loop = false)
        {
            await self.PlayInternalAsync((int)type, loop);
        }

        public static async ETTask PlayAudioLoop(this AudioComponent self, AUDIO type)
        {
            await self.PlayInternalAsync((int)type, true);
        }

        private static AudioPlayResult Result(AudioPlayStatus status, long handle = 0)
        {
            return new AudioPlayResult { Status = status, Handle = handle };
        }

        private static bool Finite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
