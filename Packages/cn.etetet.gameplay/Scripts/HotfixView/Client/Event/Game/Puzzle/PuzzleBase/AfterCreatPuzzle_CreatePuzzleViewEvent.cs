using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    public class AfterCreatPuzzle_CreatePuzzleViewEvent : AEvent<Scene , AfterCreatePuzzle>
    {
        protected override async ETTask Run(Scene scene, AfterCreatePuzzle data)
        {
            Puzzle puzzle = data.puzzle;
            
            //生成预制体
            // var bundleObj = await ResourcesLoaderHelper.LoadAssetPrefabAsync<GameObject>(scene, puzzle.Config().PrefabPath);
            //
            // var Obj = UnityEngine.Object.Instantiate(bundleObj , puzzle.GetParent<Grid>().GetComponent<GridView>().puzzleTransform);
            //
            // //生成View
            // var puzzleView = puzzle.AddComponent<PuzzleView,Transform>(Obj.transform);
            //
            // //关联
            // Obj.GetComponent<GameObjectEntityRef>().Entity = puzzleView;
            //EntityLink.Link(Obj , puzzleView);
            
            await ETTask.CompletedTask;
        }
    }
}
