using UnityEngine.SceneManagement;

namespace ET.Client
{
    public static partial class ResourcesLoaderHelper
    {
        public static async ETTask LoadSceneAsync(Scene scene , string path)
        {
            ResourcesLoaderComponent resourcesLoaderComponent = scene.GetComponent<ResourcesLoaderComponent>();
            if (resourcesLoaderComponent == null)
            {
                return;
            }
            //todo 修改场景路径luban
            await resourcesLoaderComponent.LoadSceneAsync($"Packages/cn.etetet.gameplay/Resources/Scenes/{path}.unity", LoadSceneMode.Single);
        }
        
        public static async ETTask<T> LoadAssetPrefabAsync<T>(Scene scene , string path) where T : UnityEngine.Object
        {
            ResourcesLoaderComponent resourcesLoaderComponent = scene.GetComponent<ResourcesLoaderComponent>();
            if (resourcesLoaderComponent == null)
            {
                return null;
            }
            return await resourcesLoaderComponent.LoadAssetAsync<T>($"Packages/cn.etetet.gameplay/Resources/{path}.prefab");
        }
        
        public static async ETTask<T> LoadAssetSpriteAsync<T>(Scene scene , string path) where T : UnityEngine.Object
        {
            ResourcesLoaderComponent resourcesLoaderComponent = scene.GetComponent<ResourcesLoaderComponent>();
            if (resourcesLoaderComponent == null)
            {
                return null;
            }
            return await resourcesLoaderComponent.LoadAssetAsync<T>($"Packages/cn.etetet.gameplay/Resources/{path}.sprite");
        }
    }   
}