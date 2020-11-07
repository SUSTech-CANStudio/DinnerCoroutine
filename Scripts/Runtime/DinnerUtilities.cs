using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;

namespace CANStudio.DinnerCoroutine
{
    internal static class DinnerUtilities
    {
        /// <summary>
        ///     Get a coroutine from object's method.
        /// </summary>
        /// <param name="object">Object to invoke</param>
        /// <param name="name">Name of method</param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static IEnumerator GetCoroutine(object @object, string name, object param = null)
        {
            var info = @object.GetType().GetMethod(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            
            if (info is null) throw new MissingMethodException(@object.GetType().Name, name);
            
            var result = info.Invoke(@object, param is null ? null : new []{param});
            
            if (result is IEnumerator coroutine) return coroutine;
            
            throw new ArgumentException($"Function '{name}' doesn't return an {nameof(IEnumerator)}.", nameof(name));
        }
        
        /// <summary>
        ///     Convert a symbol (i.e. class, method, or var name) to plain text.
        /// </summary>
        /// <param name="symbol">String contains only letters.</param>
        /// <param name="upperInitials"></param>
        /// <returns></returns>
        /// <example>
        ///     Debug.Log(ToPlainText("AMDYes")); // output: AMD yes
        /// </example>
        [NotNull]
        public static string ToPlainText([CanBeNull] string symbol, bool upperInitials = false)
        {
            if (string.IsNullOrEmpty(symbol)) return "(null)";

            var sb = new StringBuilder();

            var abbreviation = new Queue<char>();

            foreach (var c in symbol)
                if (char.IsUpper(c))
                {
                    abbreviation.Enqueue(c);
                }
                else if (char.IsLower(c))
                {
                    while (abbreviation.Count > 1) sb.Append(abbreviation.Dequeue());
                    if (abbreviation.Count > 0)
                    {
                        if (sb.Length > 0) sb.Append(' ');
                        sb.Append(char.ToLower(abbreviation.Dequeue()));
                    }

                    sb.Append(c);
                }
                else
                {
                    throw new ArgumentException($"Unknown char '{c}'", nameof(symbol));
                }

            if (abbreviation.Count > 0) sb.Append(' ');
            while (abbreviation.Count > 0)
            {
                sb.Append(abbreviation.Dequeue());
            }

            if (upperInitials) sb[0] = char.ToUpper(sb[0]);

            return sb.ToString();
        }
    }
}