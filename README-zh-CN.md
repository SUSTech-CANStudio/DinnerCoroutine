# DinnerCoroutine

[![openupm](https://img.shields.io/npm/v/com.canstudio.dinner-coroutine?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.canstudio.dinner-coroutine/)

DinnerCoroutine是一个增强版的Unity协程，使用方式与原生Unity协程十分相似。如果你之前在项目中使用Unity原生协程，不需要对协程代码做任何修改就可以将它们换成DinnerCoroutine。

## 特点

- [x] 支持原生协程的yield指令 (`YieldInstruction`)
- [x] 支持原生的自定义yield指令 (`CustomYieldInstruction`)
- [x] 协程在游戏中与编辑器中都可以运行
- [x] 可以方便地控制协程 (手动启动/暂停和继续/停止或中断)
- [x] 提供异步协程 (异步协程可以节约主线程的运算资源, 但不能在其中使用一部分Unity的函数)
- [x] 允许使用全局协程 (不会在MonoBehaviour被销毁时停止)
- [x] 没有外部依赖
- [ ] 更多尚在开发的功能......

## 安装

安装DinnerCoroutine并不复杂，你可以选择以下任意一种方式进行安装：

### OpenUPM（推荐）

1. 如果你还没有使用过OpenUPM，请先安装[openupm-cli](https://github.com/openupm/openupm-cli#installation)。

2. 命令行转到Unity项目路径下（这个路径下应该有一个名为`Assets`的文件夹），输入并执行

   ```shell
   openupm add com.canstudio.dinner-coroutine
   ```

3. 打开Unity编辑器，DinnerCoroutine将会成功安装。

### UPM

1. 如果没有安装Git，请先[下载 Git](https://git-scm.com/downloads)并安装。

2. 打开Unity编辑器，工具栏中打开`Window -> Package Manager`。

3. 在Package Manager窗口中，点击左上角的 `+ -> add package from git URL`以输入。

4. 添加下列upm包：

   `https://github.com/SUSTech-CANStudio/DinnerCoroutine.git#upm`

## 快速上手

### 最简单的脚本示例

```c#
using System.Collections;
using UnityEngine;
using CANStudio.DinnerCoroutine;

public class MyScript : MonoBehaviour
{
    public float time = 2f;
    
    private void Start()
    {
        this.StartSpoon("MyCoroutine", time);
    }
    
    private IEnumerator MyCoroutine(float waitTime)
    {
        Debug.Log("你好!");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("再见!");
    }
}
```

### 控制协程

```c#
// 在MonoBehaviour脚本中
// 返回值'coroutine'表示你所启动的协程
var coroutine = this.StartSpoon("MyCoroutine");
```

##### 获取当前状态


```C#
Debug.Log(coroutine.Status);
/**	可能的输出有:
*	NotStarted 	// 协程尚未被启动
*	Running		// 协程正在运行
*	Paused		// 协程暂停中
*	Finished	// 协程已结束运行
*/
```

##### 控制协程状态

```C#
// 启动一个尚未开始或处于暂停状态的协程
coroutine.Start();

// 暂停一个正在运行的协程
coroutine.Pause();

// 停止一个正在运行或暂停的协程
coroutine.Stop();

// 中断一个正在运行或暂停的协程
// 这个方法和Stop()的运行结果一样, 但不会触发你给这个协程设置的callback函数.
coroutine.Interrupt();
```

### 示例

你可以在[这里](Packages/DinnerCoroutine/Samples)找到示例代码:

- [控制协程](Packages/DinnerCoroutine/Samples/ControlSample/ControlCoroutine.cs)

## 局限性

Unity引擎在编辑器模式下并没有完整的事件循环, 因此一些指令在游戏中和在编辑器中的运行结果会有所不同:

- `WaitForFixedUpdate`, `WaitForEndOfFrame`: 在编辑器下会变为`yield return null`
- `WaitForSecondsRealtime`: 在编辑器下不会等待正确的时间, 而是等待一个取决于你编辑器帧数的随机时间

在编辑器下使用`Time`类时请注意, 它的所有返回值在编辑器下都是常量, 并不会被正确地更新。在你的协程中，应该尽量使用`DinnerTime`类代替`Time`类，这个类在游戏运行时与`Time`类完全一致，而在编辑器模式下也可以提供部分时间访问。

`DinnerTime`类在编辑器模式下可以提供的时间访问有：

- `time`, `deltaTime`, `unscaledTime`, `unscaledDeltaTime`, `timeScale`：这些功能与在游戏中完全一致
- `fixedTime`, `fixedDeltaTime`, `fixedUnscaledTime`, `fixedUnscaledDeltaTime`：由于编辑器模式下没有固定刷新，这些功能会返回非固定的刷新时间，在大多数情况下可以正常工作
- 其它功能暂不支持在编辑器模式下访问