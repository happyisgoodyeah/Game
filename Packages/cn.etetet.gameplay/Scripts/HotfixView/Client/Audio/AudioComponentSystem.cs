using ET.Client;
using UnityEngine;

namespace ET
{

    public enum AUDIO
    {
        bg1,
        bg2,
    }
    
    //表
    //PlayAudio
    //bgm
    
    [EntitySystemOf(typeof(AudioComponent))]
    public static partial class AudioComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.AudioComponent self)
        {
            self.resourcesLoader = self.Root().GetComponent<ResourcesLoaderComponent>();
            self.audio = self.Root().GetComponent<GlobalComponent>().Audio.transform;
            // self.audioSource = self.Root().GetComponent<GlobalComponent>().Audio.GetComponent<AudioSource>();
            //self.PlayAudio(AUDIO.bg1).NoContext();        示例
            self.PlayAudioLoop(AUDIO.bg1).NoContext();        
            self.PlayAudioOne(AUDIO.bg2).NoContext();        
        }
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static AudioConfig Config(this ET.AudioComponent self , AUDIO type)
        {
            return AudioConfigCategory.Instance.Get((int)type);
        }
        /// <summary>
        /// 播放单次音频
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type">音频枚举类型</param>
        /// <param name="loop">是否循环</param>
        public static async ETTask PlayAudioOne(this AudioComponent self, AUDIO type, bool loop = false)
        {
            var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(self.Root(), "Audio/Audio");

            var audioSource = UnityEngine.Object.Instantiate(bundleObj, self.audio).GetComponent<AudioSource>();

            var path = self.Config(type).Path;
            audioSource.clip = await self.resourcesLoader.Entity.LoadAssetAsync<AudioClip>(path);
            audioSource.loop = loop;

            audioSource.Play();
        }
        /// <summary>
        /// 播放循环音频
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type">音频枚举类型</param>
        public static async ETTask PlayAudioLoop(this AudioComponent self, AUDIO type)
        {
            await self.PlayAudioOne(type,true);        
        }
    }
}