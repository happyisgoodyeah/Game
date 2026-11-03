using System;
using UnityEngine;

namespace ET
{
    public static partial class AudioComponentSystem
    {
        public static float GetMasterVolume(this AudioComponent self) => self.MasterVolume;
        public static bool IsMuted(this AudioComponent self) => self.Muted;

        public static float GetGroupVolume(this AudioComponent self, AudioGroup group)
        {
            CheckGroup(group);
            return self.GroupVolumes[(int)group];
        }

        public static bool IsGroupMuted(this AudioComponent self, AudioGroup group)
        {
            CheckGroup(group);
            return self.GroupMutes[(int)group];
        }

        public static void SetMasterVolume(this AudioComponent self, float value)
        {
            self.MasterVolume = ClampVolume(value);
            PlayerPrefs.SetFloat("PuzzleGame.Audio.Master", self.MasterVolume);
            self.SettingsChanged();
        }

        public static void SetGroupVolume(this AudioComponent self, AudioGroup group, float value)
        {
            CheckGroup(group);
            self.GroupVolumes[(int)group] = ClampVolume(value);
            PlayerPrefs.SetFloat($"PuzzleGame.Audio.{group}", self.GroupVolumes[(int)group]);
            self.SettingsChanged();
        }

        public static void SetMuted(this AudioComponent self, bool muted)
        {
            self.Muted = muted;
            PlayerPrefs.SetInt("PuzzleGame.Audio.Muted", muted ? 1 : 0);
            self.SettingsChanged();
        }

        public static void SetGroupMuted(this AudioComponent self, AudioGroup group, bool muted)
        {
            CheckGroup(group);
            self.GroupMutes[(int)group] = muted;
            PlayerPrefs.SetInt($"PuzzleGame.Audio.{group}.Muted", muted ? 1 : 0);
            self.SettingsChanged();
        }

        /// <summary>设置变更后延迟 0.5 秒落盘；设置界面关闭或应用挂起时也可显式调用。</summary>
        public static void SaveSettings(this AudioComponent self)
        {
            if (!self.PreferencesDirty) return;
            PlayerPrefs.Save();
            self.PreferencesDirty = false;
        }

        private static void SettingsChanged(this AudioComponent self)
        {
            self.PreferencesDirty = true;
            self.SavePreferencesAt = Time.realtimeSinceStartupAsDouble + 0.5d;
            foreach (EntityRef<AudioVoice> reference in self.Voices.Values)
            {
                AudioVoice voice = reference;
                self.ApplyVolume(voice);
            }
        }

        private static void ApplyVolume(this AudioComponent self, AudioVoice voice)
        {
            if (voice.Source == null) return;
            voice.Source.volume = voice.BaseVolume * voice.Gain * self.MasterVolume * self.GroupVolumes[(int)voice.Group];
            voice.Source.mute = self.Muted || self.GroupMutes[(int)voice.Group];
        }

        private static float ReadVolume(string key)
        {
            float value = PlayerPrefs.GetFloat($"PuzzleGame.Audio.{key}", 1f);
            return Finite(value) ? Mathf.Clamp01(value) : 1f;
        }

        private static float ClampVolume(float value)
        {
            if (!Finite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            return Mathf.Clamp01(value);
        }

        private static void CheckGroup(AudioGroup group)
        {
            if ((int)group < 0 || (int)group >= 4) throw new ArgumentOutOfRangeException(nameof(group));
        }
    }
}
