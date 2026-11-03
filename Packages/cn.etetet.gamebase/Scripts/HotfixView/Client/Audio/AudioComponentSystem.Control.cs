using System;
using UnityEngine;

namespace ET
{
    public static partial class AudioComponentSystem
    {
        /// <summary>停止单次播放。负数使用配置淡出时间；0 立即停止。</summary>
        public static bool Stop(this AudioComponent self, long handle, float fadeSeconds = -1f)
        {
            if (!Finite(fadeSeconds)) throw new ArgumentOutOfRangeException(nameof(fadeSeconds));
            if (!self.Voices.TryGetValue(handle, out EntityRef<AudioVoice> reference)) return false;
            AudioVoice voice = reference;
            float duration = fadeSeconds < 0f ? voice.DefaultFadeOut : fadeSeconds;
            if (voice.Loading || voice.Paused || self.GroupPauses[(int)voice.Group] || AudioListener.pause || duration <= 0f)
            {
                self.Recycle(handle);
                return true;
            }
            voice.Stopping = true;
            SetFade(voice, 0f, duration);
            return true;
        }

        public static void StopGroup(this AudioComponent self, AudioGroup group, float fadeSeconds = 0f)
        {
            CheckGroup(group);
            self.CollectIds(group);
            foreach (long id in self.ScratchIds) self.Stop(id, fadeSeconds);
        }

        public static void StopAll(this AudioComponent self)
        {
            self.CollectIds(null);
            foreach (long id in self.ScratchIds) self.Recycle(id);
        }

        public static bool Pause(this AudioComponent self, long handle)
        {
            if (!self.Voices.TryGetValue(handle, out EntityRef<AudioVoice> reference)) return false;
            AudioVoice voice = reference;
            voice.Paused = true;
            voice.Source?.Pause();
            return true;
        }

        public static bool Resume(this AudioComponent self, long handle)
        {
            if (!self.Voices.TryGetValue(handle, out EntityRef<AudioVoice> reference)) return false;
            AudioVoice voice = reference;
            voice.Paused = false;
            if (!self.GroupPauses[(int)voice.Group]) voice.Source?.UnPause();
            return true;
        }

        public static void PauseGroup(this AudioComponent self, AudioGroup group, bool paused)
        {
            CheckGroup(group);
            self.GroupPauses[(int)group] = paused;
            foreach (EntityRef<AudioVoice> reference in self.Voices.Values)
            {
                AudioVoice voice = reference;
                if (voice.Group != group || voice.Source == null) continue;
                if (paused) voice.Source.Pause();
                else if (!voice.Paused) voice.Source.UnPause();
            }
        }

        /// <summary>包含加载中、暂停中和淡出中的请求。</summary>
        public static bool IsActive(this AudioComponent self, long handle)
        {
            return self.Voices.ContainsKey(handle);
        }

        public static int ActiveCount(this AudioComponent self)
        {
            return self.Voices.Count;
        }

        public static void ConfigureLimits(this AudioComponent self, int maxVoices, int maxPooledSources)
        {
            if (maxVoices < 2 || maxPooledSources < 0) throw new ArgumentOutOfRangeException(nameof(maxVoices));
            self.MaxVoices = maxVoices;
            self.MaxPooledSources = maxPooledSources;
            while (self.SourcePool.Count > maxPooledSources)
            {
                AudioSource source = self.SourcePool.Pop();
                if (source != null) UnityEngine.Object.Destroy(source.gameObject);
            }
            // 已有播放不被强制截断；降限后拒绝新增，直到活跃数量低于限制。
        }

        private static void Tick(this AudioComponent self, float deltaTime)
        {
            self.ScratchIds.Clear();
            foreach (var pair in self.Voices)
            {
                AudioVoice voice = pair.Value;
                if (voice == null) { self.ScratchIds.Add(pair.Key); continue; }
                if (voice.Loading) continue;
                if (voice.Source == null) { self.ScratchIds.Add(pair.Key); continue; }
                // Unity Pause 会使 isPlaying 为 false；明确暂停状态后再检查自然结束。
                if (voice.Paused || self.GroupPauses[(int)voice.Group] || AudioListener.pause) continue;
                if (voice.FadeDuration > 0f)
                {
                    voice.FadeElapsed += deltaTime;
                    voice.Gain = Mathf.Lerp(voice.FadeFrom, voice.FadeTo, voice.FadeElapsed / voice.FadeDuration);
                    self.ApplyVolume(voice);
                    if (voice.FadeElapsed >= voice.FadeDuration)
                    {
                        voice.FadeDuration = 0f;
                        if (voice.Stopping) { self.ScratchIds.Add(pair.Key); continue; }
                    }
                }
                if (!voice.Source.isPlaying) self.ScratchIds.Add(pair.Key);
            }
            foreach (long id in self.ScratchIds) self.Recycle(id);
            if (self.PreferencesDirty && Time.realtimeSinceStartupAsDouble >= self.SavePreferencesAt)
                self.SaveSettings();
        }

        private static void CollectIds(this AudioComponent self, AudioGroup? group)
        {
            self.ScratchIds.Clear();
            foreach (var pair in self.Voices)
            {
                AudioVoice voice = pair.Value;
                if (group == null || voice.Group == group) self.ScratchIds.Add(pair.Key);
            }
        }

        private static void PruneMusicRequests(this AudioComponent self)
        {
            self.ScratchIds.Clear();
            foreach (var pair in self.Voices)
            {
                AudioVoice voice = pair.Value;
                if (voice.Group == AudioGroup.Music && (voice.Loading || voice.Stopping)) self.ScratchIds.Add(pair.Key);
            }
            foreach (long id in self.ScratchIds) self.Recycle(id);
        }

        private static void FadePreviousMusic(this AudioComponent self, long newId)
        {
            self.CollectIds(AudioGroup.Music);
            foreach (long id in self.ScratchIds)
            {
                if (id != newId) self.Stop(id);
            }
        }

        private static void SetFade(AudioVoice voice, float target, float duration)
        {
            voice.FadeFrom = voice.Gain;
            voice.FadeTo = target;
            voice.FadeDuration = duration;
            voice.FadeElapsed = 0f;
            if (duration <= 0f) voice.Gain = target;
        }

        private static AudioSource RentSource(this AudioComponent self)
        {
            AudioSource source = null;
            while (source == null && self.SourcePool.Count > 0) source = self.SourcePool.Pop();
            if (source == null)
            {
                GameObject gameObject = new GameObject("AudioVoice");
                gameObject.transform.SetParent(self.AudioRoot, false);
                source = gameObject.AddComponent<AudioSource>();
            }
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.gameObject.SetActive(true);
            return source;
        }

        private static void Recycle(this AudioComponent self, long id)
        {
            if (!self.Voices.TryGetValue(id, out EntityRef<AudioVoice> reference)) return;
            self.Voices.Remove(id);
            AudioVoice voice = reference;
            if (voice == null) return;
            AudioSource source = voice.Source;
            voice.Source = null;
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.loop = false;
                source.gameObject.SetActive(false);
                if (self.SourcePool.Count < self.MaxPooledSources) self.SourcePool.Push(source);
                else UnityEngine.Object.Destroy(source.gameObject);
            }
            voice.Dispose();
        }
    }
}
