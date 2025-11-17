using ET.Client;

namespace ET
{
    [EntitySystemOf(typeof(PuzzleSpawnComponent))]
    [FriendOf(typeof(Grid))]
    public static partial class PuzzleSpawnComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.PuzzleSpawnComponent self)
        {
        }

        /// <summary>
        /// 生成Puzzle
        /// </summary>
        /// <param name="self"></param>
        /// <param name="puzzleId">puzzleConfig的id</param>
        /// <param name="index">生成的puzzle的下标 用于对应获取gameobject</param>
        /// <returns></returns>
        public static Puzzle SpawnPuzzle(this PuzzleSpawnComponent self, int puzzleId , FloatVector2 viewPosition)
        {
            var grid = self.GetParent<Grid>();
            var puzzle = self.GetParent<Grid>().AddChild<Puzzle , int , FloatVector2>(puzzleId , viewPosition);
            grid.PuzzleDic.TryAdd(grid.InstanceId, puzzle);
            EventSystem.Instance.Publish(self.Scene(), new AfterCreatePuzzle(){puzzle = puzzle});
            return puzzle;
        }
    }
}