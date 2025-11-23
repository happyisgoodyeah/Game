using ET.Client;
using UnityEngine;

namespace ET
{

    public enum FMOD
    {
        button1,
        button2,
        button3,
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
            self.audioSource = self.Root().GetComponent<GlobalComponent>().Audio.GetComponent<AudioSource>();
            self.PlayAudio(FMOD.button1).NoContext();
        }
        
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static GridConfig Config(this ET.AudioComponent self , int id)
        {
            return GridConfigCategory.Instance.Get(id);
        }

        public static async ETTask PlayAudio(this AudioComponent self, FMOD type)
        {
            var path = self.Config((int)type).Path;
            self.audioSource.clip = await self.resourcesLoader.Entity.LoadAssetAsync<AudioClip>(path);
            self.audioSource.Play();
        }
    }
}