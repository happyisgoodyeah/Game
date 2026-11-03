using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// 标记允许场景拼图交互穿过的基础 UI 背景。
    /// 只挂在主玩法面板的空白阻挡层，按钮和弹窗不应添加此标记。
    /// 不修改 Graphic 的射线行为，其他 UI 事件仍遵循原有框架规则。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PuzzleInputPassthrough : MonoBehaviour
    {
    }
}
