using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class AfterCreateGrid_CreateGridViewEvent : AEvent<Scene, AfterCreateGrid>
    {
        protected override async ETTask Run(Scene scene, AfterCreateGrid data)
        {
            Grid grid = data.grid;
            
            //生成预制体
            var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene,grid.Config().PrefabPath);
            GlobalComponent globalComponent = scene.Root().GetComponent<GlobalComponent>();
            var Obj = UnityEngine.Object.Instantiate(bundleObj, globalComponent.Grid);
            
            //生成View
            var gridView = data.grid.AddComponent<GridView,Transform>(Obj.transform);
            await ETTask.CompletedTask;
        }
    }
}