using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class GridCreatEndEvent_GridAddView : AEvent<Scene , GridCreatEnd>
    {
        protected override async ETTask Run(Scene scene, GridCreatEnd data)
        {
            data.grid.AddComponent<GridView>();
            await ETTask.CompletedTask;
        }
    }
}
