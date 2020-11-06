using System.Collections;
using CANStudio.DinnerCoroutine;
using UnityEngine;

namespace DefaultNamespace
{
    public class Test : MonoBehaviour
    {
        private ForkCoroutine _sp;
        
        [ContextMenu("test1")]
        public void Test1()
        {
            _sp = this.StartFork("Co", () => Debug.Log("callback"));
        }

        [ContextMenu("Pause")]
        public void Pause() => _sp?.Pause();

        [ContextMenu("Begin")]
        public void Begin() => _sp?.Start();

        [ContextMenu("Stop")]
        public void Stop() => _sp?.Stop();

        [ContextMenu("Interrupt")]
        public void Interrupt() => _sp?.Interrupt();
        
        private IEnumerator Co()
        {
            Debug.Log("Ping");
            yield return new WaitForSeconds(2);
            Debug.Log("Pong");
            while (true)
            {
                yield return new WaitForSeconds(1);
                Debug.Log("hi");
            }
        }
    }
}