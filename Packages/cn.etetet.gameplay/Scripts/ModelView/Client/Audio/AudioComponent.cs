using ET.Client;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class AudioComponent : Entity,IAwake
    {
        public EntityRef<ResourcesLoaderComponent>  resourcesLoader;
        public AudioSource audioSource;
        
    }
    
}
