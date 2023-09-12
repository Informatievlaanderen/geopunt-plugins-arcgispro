namespace GeopuntTests1
{
    [TestClass]
    public class MyFirstTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            // Assert.Fail();
            const int a = 1;
            const int b = 2;

            Assert.AreEqual(3, a + b);

        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.Fail();
        }
    }
}