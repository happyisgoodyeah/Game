# AGENTS.md - ET 游戏框架开发

> **后续对话请默认使用中文表述**

## 快速命令

- `dotnet build ET.sln` - 编译
- Unity F6 编译，F7 热重载
- Unity Test Runner 运行测试

## 项目结构

```
Packages/
├── cn.etetet.gameplay    # 游戏业务逻辑
├── cn.etetet.gamebase    # 游戏基础功能
└── cn.etetet.*          # ET框架/YIUI框架
```

## Package 分类

### cn.etetet.gameplay - 游戏业务逻辑
核心玩法：**网格拼图游戏**（拖拽拼图块到2D网格槽位）
- Grid - 2D网格容器
- Puzzle - 拼图实体
- Slot - 槽位

### cn.etetet.gamebase - 游戏基础功能
- Save - 存档系统
- Audio - 音频
- Drag - 拖拽组件

### cn.etetet.* - 框架包
- core/unit - 核心/实体
- ai/aoi/move - AI/区域/移动
- proto/memorypack - 协议/序列化
- yiui* - UI框架

## ET 限制分析器 (DiagnosticIds)

ET框架内置 **32个** 代码分析器（ET0001-ET0032），构建时自动检查违规：

### 程序集限制

| ID | 规则 |
|----|------|
| ET0005 | Hotfix程序集：只能声明含 `[EnableClass]` 的类或静态类 |
| ET0032 | Model/ModelView：禁止声明非实体类（除非加 `[EnableClass]`） |
| ET0022 | Server程序集：禁止引用 `ET.Client` 命名空间 |

### 实体类限制

| ID | 规则 |
|----|------|
| ET0003 | 禁止多层继承，需直接继承 Entity |
| ET0010 | 禁止声明委托字段/属性 |
| ET0020 | 禁止声明实体字段，需用 `EntityRef<T>` |
| ET0023 | LSEntity：禁止声明浮点数字段 |
| ET0027 | 实体类名HashCode禁止重复 |
| ET0028 | 禁止同时标记 `[Component]` 和 `[Child]` |
| ET0029 | 禁止声明为泛型实体类 |

### 实体操作限制

| ID | 规则 |
|----|------|
| ET0001 | AddChild类型约束：需用 `[ChildOf]` 标记父子关系 |
| ET0007 | 组件类型约束：需用 `[ComponentOf]` 标记父子关系 |
| ET0014 | Entity内禁止直接调用 Child/Component |

### 系统方法限制

| ID | 规则 |
|----|------|
| ET0024 | System必须有 `[EntitySystemOf]` 特性 |
| ET0025 | System方法必须在含 `[EntitySystemOf]` 的静态类中 |
| ET1001 | System方法必须在静态分部类中 |

### 异步方法限制

| ID | 规则 |
|----|------|
| ET0021 | 禁止 `async void`，必须 `async ETTask` |
| ET0008 | ETTask在非async方法需加 `.Coroutine()` |
| ET0009 | ETTask在async方法需加 `await` 或 `.Coroutine()` |
| ET0016 | 含CancelToken的async方法，await后必须判断 IsCancel |
| ET0017 | await表达式必须传入同一个CancelToken |
| ET0018 | CancelToken参数禁止默认值 |
| ET0019 | 函数调用禁止传 null |

### 网络消息限制

| ID | 规则 |
|----|------|
| ET0030 | 消息类禁止声明实体字段 |

### 其他限制

| ID | 规则 |
|----|------|
| ET0011 | UniqueId值必须在约束区间内 |
| ET0012 | UniqueId禁止重复 |
| ET0015 | Static字段必须标记特性 |
| ET0026 | Entity内或含Entity参数的函数必须用Fiber日志 |
| ET0031 | 含 `[DisableNew]` 的类禁止用new创建 |

## 代码规范

### Entity 与 View 关系

Entity 和 EntityView 是父子关系，可通过任一方获取另一方：

```csharp
// Entity → View
PuzzleView view = puzzle.GetComponent<PuzzleView>();

// View → Entity
Puzzle puzzle = puzzleView.GetParent<Puzzle>();
```

**重要**: 方法参数只需传递其中一个，无需同时传递两者。在方法内部可通过上述方式获取另一方。

### FriendOf 访问权限

访问其他实体类的字段需要在 System 类上声明 `[FriendOf]` 特性：

```csharp
// GridSystem 需要访问 Puzzle.bindSlots 字段
[EntitySystemOf(typeof(Grid))]
[FriendOf(typeof(Slot))]
[FriendOf(typeof(Puzzle))]  // 访问 Puzzle 字段必须声明
public static partial class GridSystem
{
    // 现在可以访问 puzzle.bindSlots
}
```

**规则**: 如果 SystemA 需要访问 EntityB 的字段，必须在 SystemA 上声明 `[FriendOf(typeof(EntityB))]`。

### ET 四层架构调用规则

ET框架分为四层，有严格的调用方向限制：

```
Model (数据层)
    ↓ 可调用
Hotfix (逻辑层)
    ↓ 可调用
ModelView (视图数据层)
    ↓ 可调用
HotfixView (视图逻辑层)
```

**调用规则**：
- Hotfix 层**禁止**调用 ModelView 层（如 PuzzleView、GridView、SlotView）
- Hotfix 层只能操作 Model 层的数据（Entity、Component）
- View 相关逻辑必须在 HotfixView 层处理
- HotfixView 层可以调用 Hotfix 层的方法

**错误示例**（Hotfix层调用View）：
```csharp
// ❌ 错误：Hotfix层直接获取View组件
PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
```

**正确做法**：
```csharp
// ✅ 正确：Hotfix层只处理数据，View层通过参数传入
public static bool TryRotatePlacedPuzzle(this Grid self, Puzzle puzzle, int angle, FloatVector2 worldPosition, out IntVector2 gridPosition)

// HotfixView层调用时传入worldPosition
var worldPosition = puzzleView.transform.position;
FloatVector2 position = new FloatVector2(worldPosition.x, worldPosition.y);
grid.TryRotatePlacedPuzzle(puzzle, 90, position, out originPosition);
```

### 实体定义

```csharp
// 实体
[MemoryPackable]
[ComponentOf(typeof(Scene))]
public partial class XxxComponent : Entity, IAwake { }

// 系统
[EntitySystemOf(typeof(XxxComponent))]
public partial class XxxSystem : EntitySystem<Awake<XxxComponent>> { }

// 消息
[MessageHandler(SceneType.Map)]
public class C2M_XxxHandler : MessageLocationHandler<Unit, C2M_Xxx, M2C_Xxx> { }
```

- Entity基类 + partial class
- [MemoryPackInclude] 序列化
- ETTask 异步（禁止async void）
- [MessageHandler] 网络消息
- EntityRef<T> 而非直接实体字段
- [ChildOf]/[ComponentOf] 标记父子关系