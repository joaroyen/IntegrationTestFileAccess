using System;
using System.Threading.Tasks;

namespace JoarOyen.Test
{
    public class TestBase
    {
        public static T WaitForResult<T>(Task<T> asyncTask)
        {
            asyncTask.Wait();
            return asyncTask.Result;
        }
    }
}