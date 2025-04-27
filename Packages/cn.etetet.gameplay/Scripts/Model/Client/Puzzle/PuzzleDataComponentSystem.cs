namespace ET
{
    [EntitySystemOf(typeof(PuzzleDataComponent))]
    public static partial class PuzzleDataComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.PuzzleDataComponent self, ET.IntVector2 position, int rotation)
        {
            self.puzzlePosition = position;
            self.rotation = rotation;
        }
    }
}
