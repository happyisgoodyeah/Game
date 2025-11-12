using UnityEngine;
using DG.Tweening;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class PuzzleShake_PuzzleViewUpdate : AEvent<Scene, PuzzleShake>
    {
        protected override async ETTask Run(Scene scene, PuzzleShake a)
        {
            PuzzleView puzzleView = a.puzzle.GetComponent<PuzzleView>();
            //puzzleView.transform.eulerAngles = new Vector3(0f , 0f , a.puzzle.rotate);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(puzzleView.transform.DOLocalRotate(new Vector3(0f, 0f, a.puzzle.rotate + 15), 0.04f))
                    .Append(puzzleView.transform.DOLocalRotate(new Vector3(0f, 0f, a.puzzle.rotate), 0.03f));
            await ETTask.CompletedTask;
        }
    }
}