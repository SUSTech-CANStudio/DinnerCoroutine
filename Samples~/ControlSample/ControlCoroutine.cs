using System.Collections;
using CANStudio.DinnerCoroutine;
using UnityEngine;

public class ControlCoroutine : MonoBehaviour
{
    public float repeatCount = 10;
    public float repeatTime = 1;
    public string finishMessage = "Finished!";

    private SpoonCoroutine _coroutine;

    [ContextMenu("Start")]
    public void CoroutineStart()
    {
        if (_coroutine is null || _coroutine.Status == CoroutineStatus.Finished)
        {
            // create a new coroutine
            _coroutine = new SpoonCoroutine(MyCoroutine(repeatTime));
            // when coroutine stopped or finished, callback function will be invoked
            _coroutine.callback += () => Debug.Log(finishMessage);
        }
        // start the coroutine
        _coroutine.Start();
    }

    [ContextMenu("Stop")]
    public void CoroutineStop()
    {
        _coroutine.Stop();
    }

    [ContextMenu("Pause")]
    public void CoroutinePause()
    {
        _coroutine.Pause();
    }

    [ContextMenu("Interrupt")]
    public void CoroutineInterrupt()
    {
        _coroutine.Interrupt();
    }

    // This is the coroutine function
    private IEnumerator MyCoroutine(float time)
    {
        Debug.Log("Start!");

        for (int i = 0; i < repeatCount; i++)
        {
            // you can use yield instruction in both editor and game
            yield return new WaitForSeconds(time);
            Debug.Log("I'm running!");
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Start")) CoroutineStart();
        if (GUILayout.Button("Stop")) CoroutineStop();
        if (GUILayout.Button("Pause")) CoroutinePause();
        if (GUILayout.Button("Interrupt")) CoroutineInterrupt();
    }
}
