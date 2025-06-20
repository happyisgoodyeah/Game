using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    public class AfterCreatPuzzle_CreatePuzzleViewEvent : AEvent<Scene, AfterCreatePuzzle>
    {
        protected override async ETTask Run(Scene scene, AfterCreatePuzzle data)
        {
            Puzzle puzzle = data.puzzle;

            //生成预制体
            // var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, puzzle.Config().PrefabPath);

            var transform = puzzle.GetParent<Grid>().GetComponent<GridView>().puzzleTransform.Find("Puzzle");
            var Obj = transform.GetChild(data.index).gameObject;

            //生成View
            var puzzleView = puzzle.AddComponent<PuzzleView, Transform>(Obj.transform);

            //关联
            Obj.GetComponent<GameObjectEntityRef>().Entity = puzzleView;

            await ETTask.CompletedTask;
        }
    }
}