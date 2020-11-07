# DinnerCoroutine

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

