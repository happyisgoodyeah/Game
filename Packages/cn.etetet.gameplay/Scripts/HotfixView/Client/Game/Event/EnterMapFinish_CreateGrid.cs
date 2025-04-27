using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class EnterMapFinish_CreateGrid : AEvent<Scene , EnterMapFinish>
    {
        protected override async ETTask Run(Scene scene, EnterMapFinish a)
        {
            var grid = scene.AddChild<Grid>();
            
            await ETTask.CompletedTask;
        }
    }
}
