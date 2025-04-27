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
        public static void SpawnPuzzle(PuzzleSpawnComponent self, IntVector2 spawnPosition, int cellSize)
        {
            var puzzle = self.GetParent<Grid>().AddChild<Puzzle>();
            puzzle.InitComponent(spawnPosition, cellSize);
        }
    }
}
