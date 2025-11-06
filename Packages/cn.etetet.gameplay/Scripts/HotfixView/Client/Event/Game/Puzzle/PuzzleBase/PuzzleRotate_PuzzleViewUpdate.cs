using UnityEngine;
using DG.Tweening;
namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class PuzzleRotate_PuzzleViewUpdate : AEvent<Scene , PuzzleRotate>
    {
        protected override async ETTask Run(Scene scene, PuzzleRotate a)
        {
            PuzzleView puzzleView = a.puzzle.GetComponent<PuzzleView>();
            //puzzleView.transform.eulerAngles = new Vector3(0f , 0f , a.puzzle.rotate);
            puzzleView.transform.DOLocalRotate(new Vector3(0f, 0f, a.puzzle.rotate), 0.05f);
            await ETTask.CompletedTask;
        }
    }    
}