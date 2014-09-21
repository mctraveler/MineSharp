using System;

namespace MineProxy
{
    public static class Extensions
    {
        public static string JoinFrom(this string[] s, int startIndex)
        {
            return string.Join(" ", s, startIndex, s.Length - startIndex);			
        }

        /// <summary>
        /// Safe dispose
        /// </summary>
        public static void DisposeLog(this IDisposable disposable)
        {
            if (disposable == null)
                return;
            try
            {
                disposable.Dispose();
            } catch (Exception e)
            {
                Log.WriteServer(e);
            }
        }
    }
}

