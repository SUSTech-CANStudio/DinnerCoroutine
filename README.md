# DinnerCoroutine

[![openupm](https://img.shields.io/npm/v/com.canstudio.dinner-coroutine?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.canstudio.dinner-coroutine/)

点击这里查看[中文介绍](README-zh-CN.md)

DinnerCoroutine is a simple enhancement of Unity's coroutine. The usage of DinnerCoroutine is similar with original Unity coroutine, you can change the original coroutine into DinnerCoroutine without modifying your coroutine code.

## Features

- [x] Support original yield instructions.
- [x] Support original custom yield instructions.
- [x] Support both editor and in game coroutine.
- [x] Provides full control to coroutine (manually start, pause and recover, stop or interrupt).
- [x] Provides asynchronous coroutine (you cannot access some Unity methods in this kind of coroutine).
- [x] Allow global coroutine (won't destroy when MonoBehaviour destroyed).
- [x] No dependency.
- [ ] More features...

## Installation

DinnerCoroutine is easy to install, you can use any of following methods to install it.

### OpenUPM (Recommended)

1. If you are new to OpenUPM, install [openupm-cli](https://github.com/openupm/openupm-cli#installation) first.

2. Go to your Unity project root folder (you can find an `Assets` folder under it), run this command:

   ```shell
   openupm add com.canstudio.dinner-coroutine
   ```

3. Open your Unity editor, DinnerCoroutine should be installed successfully.

### UPM

1. If you haven't installed Git, download and install it here: [download Git](https://git-scm.com/downloads)

2. Open your Unity editor, open `Window -> Package Manager` in the toolbar.

3. In Package Manager, click `+ -> add package from git URL` in the top left corner.

4. Add following package:

   `https://github.com/SUSTech-CANStudio/DinnerCoroutine.git#upm`

## Quick Start

### A sample script

```C#
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
        Debug.Log("Ping!");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Pong!");
    }
}
```

### Control a coroutine

```c#
// In a MonoBehaviour script
// The return value 'coroutine' is the handler of a coroutine
var coroutine = this.StartSpoon("MyCoroutine");
```

##### Get status


```C#
Debug.Log(coroutine.Status);
/**	This can print one of followings:
*	NotStarted
*	Running
*	Paused
*	Finished
*/
```

##### Change status

```C#
// start a paused of not started coroutine
coroutine.Start();

// pause a coroutine
coroutine.Pause();

// stop a coroutine
coroutine.Stop();

// inturrupt a coroutine
// this behaves same as Stop(), but do not invoke callback event in the coroutine.
coroutine.Interrupt();
```

### Samples

You can view [samples](Packages/DinnerCoroutine/Samples) here:

- [control coroutine](Packages/DinnerCoroutine/Samples/ControlSample/ControlCoroutine.cs)

## Limitations

As Unity doesn't have full event loop in editor made, some command works differently when coroutine runs in editor:

- `WaitForFixedUpdate`, `WaitForEndOfFrame`: performs as `yield return null` in editor.
- `WaitForSecondsRealtime`: won't work properly, will wait for a random time depends on the frame rate.

And please pay attention when using the `Time` class, it doesn't work and always return a constant value in editor mode.