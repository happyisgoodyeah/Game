namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(SaveManagerComponent))]
    public class SaveManagerComponentLoadPath_SetPath : AEvent<Scene, SaveManagerComponentLoadPath>
    {
        protected override async ETTask Run(Scene scene, SaveManagerComponentLoadPath a)
        {
            var saveManagerComponent = a.SaveManagerComponent;
            saveManagerComponent.SaveDirectory = UnityEngine.Application.persistentDataPath;
            saveManagerComponent.GameVersion = UnityEngine.Application.version;
            await ETTask.CompletedTask;
        }
    }
}