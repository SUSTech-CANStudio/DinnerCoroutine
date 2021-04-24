using System.Collections;
using CANStudio.DinnerCoroutine;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class BasicTests
    {
        [Test]
        public void Test0_CreateSpoonCoroutine()
        {
            var _ = new SpoonCoroutine(SimpleCoroutine());
            _ = new SpoonCoroutine(SimpleCoroutine(), () => { });
        }

        [Test]
        public void Test1_CreateForkCoroutine()
        {
            var _ = new ForkCoroutine(SimpleCoroutine());
            _ = new ForkCoroutine(SimpleCoroutine(), () => { });
        }

        [Test]
        public void Test2_RunSpoonCoroutine()
        {
            var sc = new SpoonCoroutine(SimpleCoroutine());
            Assert.AreEqual(CoroutineStatus.NotStarted, sc.Status);
            sc.Start();
            Assert.AreEqual(CoroutineStatus.Running, sc.Status);
            Util.UpdateDaemon();
            Util.UpdateDaemon();
            Assert.AreEqual(CoroutineStatus.Finished, sc.Status);
        }

        [Test]
        public void Test3_RunForkCoroutine()
        {
            var fc = new ForkCoroutine(SimpleCoroutine());
            Assert.AreEqual(CoroutineStatus.NotStarted, fc.Status);
            fc.Start();
            Assert.AreEqual(CoroutineStatus.Running, fc.Status);
            Util.UpdateDaemon();
            Util.UpdateDaemon();
            Assert.AreEqual(CoroutineStatus.Finished, fc.Status);
        }

        [Test]
        public void Test4_Callback()
        {
            var callback = false;

            var sc = new SpoonCoroutine(SimpleCoroutine(), () => callback = true);
            sc.Start();
            Util.UpdateDaemon();
            Util.UpdateDaemon();
            Assert.IsTrue(callback);

            callback = false;

            var fc = new ForkCoroutine(SimpleCoroutine(), () => callback = true);
            fc.Start();
            Util.UpdateDaemon();
            Util.UpdateDaemon();
            Assert.IsTrue(callback);
        }

        [Test]
        public void Test5_DestroyKeeper()
        {
            var keeper = new GameObject("keeper");
            var behaviour = keeper.AddComponent<TestMonoBehaviour>();
            var sc0 = behaviour.StartSpoon(SimpleCoroutine());
            var sc1 = behaviour.StartSpoon("InnerSimpleCoroutine");
            var fc0 = behaviour.StartFork(SimpleCoroutine());
            var fc1 = behaviour.StartFork("InnerSimpleCoroutine");
            Object.DestroyImmediate(keeper);
            Util.UpdateDaemon();
            Assert.AreEqual(CoroutineStatus.Finished, sc0.Status);
            Assert.AreEqual(CoroutineStatus.Finished, sc1.Status);
            Assert.AreEqual(CoroutineStatus.Finished, fc0.Status);
            Assert.AreEqual(CoroutineStatus.Finished, fc1.Status);
        }

        [Test]
        public void Test6_StopAll()
        {
            var behaviour = new GameObject().AddComponent<TestMonoBehaviour>();
            var sc0 = behaviour.StartSpoon(SimpleCoroutine());
            var sc1 = behaviour.StartSpoon("InnerSimpleCoroutine");
            var fc0 = behaviour.StartFork(SimpleCoroutine());
            var fc1 = behaviour.StartFork("InnerSimpleCoroutine");

            behaviour.StopAllDinnerCoroutines("InnerSimpleCoroutine");
            Assert.AreEqual(CoroutineStatus.Running, sc0.Status);
            Assert.AreEqual(CoroutineStatus.Finished, sc1.Status);
            Assert.AreEqual(CoroutineStatus.Running, fc0.Status);
            Assert.AreEqual(CoroutineStatus.Finished, fc1.Status);

            behaviour.StopAllDinnerCoroutines();
            Assert.AreEqual(CoroutineStatus.Finished, sc0.Status);
            Assert.AreEqual(CoroutineStatus.Finished, sc1.Status);
            Assert.AreEqual(CoroutineStatus.Finished, fc0.Status);
            Assert.AreEqual(CoroutineStatus.Finished, fc1.Status);
        }

        private IEnumerator SimpleCoroutine()
        {
            yield return null;
        }

        private class TestMonoBehaviour : MonoBehaviour
        {
            public IEnumerator InnerSimpleCoroutine()
            {
                yield return null;
            }
        }
    }
}
