namespace ET
{
    [EntitySystemOf(typeof(Puzzle))]
    public static partial class PuzzleSystem
    {
        public static PuzzleConfig Config(this Puzzle self)
        {
            return PuzzleConfigCategory.Instance.Get(self.configId);
        }

        [EntitySystem]
        private static void Awake(this ET.Puzzle self, int configId)
        {
            self.configId = configId;
        }

        public static void InitComponent(this Puzzle self, IntVector2 position, int rotation)
        {
            //self.AddComponent<PuzzleDataComponent, IntVector2, int>(new IntVector2(0, 0), 0);
        }
    }
}