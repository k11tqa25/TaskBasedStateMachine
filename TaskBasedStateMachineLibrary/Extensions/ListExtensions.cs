using System.Collections.Generic;
using System.ComponentModel;

namespace TaskBasedStateMachineLibrary
{
    /// <summary>
    /// The extention class for <see cref="List{T}"/>
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Join the item in the list with a deliminator.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="deliminator">The deliminator that concatenate the items.</param>
        /// <returns>Returns a string</returns>
        public static string JoinWith<T>(this List<T> list, string deliminator = " ")
        {
            string s = "";

            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    s += list[i].ToString();
                    if (i != list.Count - 1) s += deliminator;
                }
            }
            return s;
        }

        /// <summary>
        /// Pop the list form the start as a queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns>Returns the popped item.</returns>
        public static T PopFromStart<T>(this List<T> list)
        {
            if (list.Count == 0) return default(T);
 
            T pop = list[0];

            list.RemoveAt(0);

            return pop;
        }
    }
}
