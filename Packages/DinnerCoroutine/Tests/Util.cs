using System;
using System.Reflection;
using CANStudio.DinnerCoroutine;

namespace Tests
{
    public static class Util
    {
        private static readonly Type DaemonType = Assembly.GetAssembly(typeof(DinnerTime)).GetType("CANStudio.DinnerCoroutine.Daemon", true);

        private static readonly PropertyInfo Instance =
            DaemonType.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.DeclaredOnly);

        private static readonly MethodInfo EditorUpdate =
            DaemonType.GetMethod("EditorUpdate",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.DeclaredOnly);

        private static readonly Lazy<Action> Update = new Lazy<Action>(() =>
            EditorUpdate.CreateDelegate(typeof(Action), Instance.GetValue(null)) as Action);

        /// <summary>
        ///     Invoke update manually for test.
        /// </summary>
        public static void UpdateDaemon() => Update.Value();
    }
}