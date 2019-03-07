using code2code;
using NUnit.Framework;

namespace Tests
{
    public class ClassNameTests
    {
        [Test]
        public void SmokeTest()
        {
            Assert.AreEqual("Tests.MyTest1", Cd2Cd.GetTypeName(typeof(MyTest1)));
        }
        
        [Test]
        public void NestedClass()
        {
            Assert.AreEqual("Tests.ClassNameTests.MyTest2", Cd2Cd.GetTypeName(typeof(MyTest2)));
        }
        
        [Test]
        public void SimpleGeneric()
        {
            Assert.AreEqual("Tests.MyTest3<System.String>", Cd2Cd.GetTypeName(typeof(MyTest3<string>)));
        }
        
        [Test]
        public void NestedGeneric1()
        {
            Assert.AreEqual("Tests.MyTest3<System.String>.MyTest4<int>", Cd2Cd.GetTypeName(typeof(MyTest3<string>.MyTest4<int>)));
        }

        public class MyTest2
        {
        }
    }

    public class MyTest1
    {
    }

    public class MyTest3<T>
    {
        public class MyTest4<T> { }
    }
}