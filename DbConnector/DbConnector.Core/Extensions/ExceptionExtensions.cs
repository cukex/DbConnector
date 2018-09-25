using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System
{
    public static class DataAccessExceptionExtensions
    {
        public static Action<Exception> OnError;

        internal static void Log(this Exception ex)
        {
            try
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                OnError?.Invoke(ex);
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine(e.ToString());
#endif
            }
        }
    }
}
