using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    [FriendOfAttribute(typeof(ET.Puzzle))]
    public class AfterCreatPuzzle_CreatePuzzleViewEvent : AEvent<Scene, AfterCreatePuzzle>
    {
        protected override async ETTask Run(Scene scene, AfterCreatePuzzle data)
        {
            Puzzle puzzle = data.puzzle;

            //生成预制体
            var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, puzzle.Config().PrefabPath);
            var transform = puzzle.GetParent<Grid>().GetComponent<GridView>().puzzleTransform.Find("Puzzle");
            var Obj = UnityEngine.Object.Instantiate(bundleObj, transform);
            
            Obj.transform.localPosition = new Vector3(puzzle.viewPosition.X , puzzle.viewPosition.Y , 0);
            Obj.name = $"Puzzle{puzzle.configId}";

            //生成View
            var puzzleView = puzzle.AddComponent<PuzzleView, Transform>(Obj.transform);
            var gameObjectComponent = puzzle.AddComponent<GameObjectComponent>();
            gameObjectComponent.GameObject = Obj;
            //关联
            Obj.GetComponent<GameObjectEntityRef>().Entity = puzzleView;

            await ETTask.CompletedTask;
        }
    }
}