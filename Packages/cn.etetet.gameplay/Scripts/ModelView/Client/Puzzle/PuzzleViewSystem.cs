using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(PuzzleView))]
    [FriendOf(typeof(Puzzle))]
    public static partial class PuzzleViewSystem
    {
        [EntitySystem]
        private static void Awake(this ET.PuzzleView self , Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;
            
            var puzzle = self.GetParent<Puzzle>();
            //拼图位置
            var puzzlePosition = self.parentTransform.Find("PuzzlePosition");
            if (puzzle.positionIndex < puzzlePosition.childCount)
            {
                self.transform.position = puzzlePosition.GetChild(puzzle.positionIndex).position;    
            }
        }
    }
}