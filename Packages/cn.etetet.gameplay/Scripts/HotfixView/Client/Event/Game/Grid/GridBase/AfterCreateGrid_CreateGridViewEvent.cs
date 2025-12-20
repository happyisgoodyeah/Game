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
            //将之前已经通关过的关卡删除
            // for (int i = 0; i < globalComponent.Grid.childCount; i++)
            // {
            //     GameObject.Destroy(globalComponent.Grid.GetChild(i).gameObject);
            // }
            var Obj = UnityEngine.Object.Instantiate(bundleObj, globalComponent.Grid);
            
            //生成View
            var gridView = grid.AddComponent<GridView,Transform>(Obj.transform);
            var gameObjectComponent = grid.AddComponent<GameObjectComponent>();
            gameObjectComponent.GameObject = Obj;
            //绑定GameObjectEntityRef
            Obj.AddComponent<GameObjectEntityRef>().Entity = gridView;
            
            await ETTask.CompletedTask;
        }
    }
}