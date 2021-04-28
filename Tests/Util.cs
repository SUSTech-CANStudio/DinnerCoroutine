using System;
using System.Reflection;
using CANStudio.DinnerCoroutine;

namespace Tests
{
    public static class Util
    {
        private static readonly FieldInfo Instance =
            typeof(DinnerTime).GetField("Instance",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField | BindingFlags.DeclaredOnly);

        private static readonly MethodInfo EditorUpdate =
            typeof(DinnerTime).GetMethod("EditorUpdate",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.DeclaredOnly);

        private static readonly Lazy<Action> Update = new Lazy<Action>(() =>
            EditorUpdate.CreateDelegate(typeof(Action), (Instance.GetValue(null) as Lazy<DinnerTime>)?.Value) as Action);

        /// <summary>
        ///     Invoke update manually for test.
        /// </summary>
        public static void UpdateDinnerTime()
        {
            Update.Value();
        }
    }
}