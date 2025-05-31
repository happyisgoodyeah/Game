using ET.Client;

namespace ET
{
    [EntitySystemOf(typeof(PuzzleSpawnComponent))]
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
        /// <param name="spawnPosition"></param>
        /// <param name="cellSize"></param>
        public static Puzzle SpawnPuzzle(this PuzzleSpawnComponent self, IntVector2 spawnPosition, int cellSize)
        {
            var puzzle = self.GetParent<Grid>().AddChild<Puzzle, int , int>(1001 , self.GetParent<Grid>().GetPuzzleCount());
            EventSystem.Instance.Publish(self.Scene(), new AfterCreatePuzzle(){puzzle = puzzle});
            puzzle.InitComponent(spawnPosition, cellSize);
            return puzzle;
        }
    }
}