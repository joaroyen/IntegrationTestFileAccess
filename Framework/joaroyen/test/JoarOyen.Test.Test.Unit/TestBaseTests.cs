using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace JoarOyen.Test.Test.Unit
{
    [TestFixture]
    public class TestBaseTests
    {
        [TestCase]
        public void Completed_tasks_returns_a_valid_resutl()
        {
            var task = new Task<string>(() => "Result");
            task.Start();

            var result = TestBase.WaitForResult(task);

            Assert.AreEqual("Result", result);
        }

        [TestCase]
        [ExpectedException(typeof(AggregateException))]
        public void Failed_tasks_throws_a_relevant_exception()
        {
            var task = new Task<string>(() => { throw new ArgumentException("ArgumentException");});
            task.Start();
            TestBase.WaitForResult(task);
        }
    }
}