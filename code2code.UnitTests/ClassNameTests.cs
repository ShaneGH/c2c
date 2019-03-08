using System;
using System.Collections.Generic;
using code2code;
using NUnit.Framework;

namespace Tests.Name
{
    public class ClassNameTests
    {
        [Test]
        public void SmokeTest()
        {
            Assert.AreEqual("Tests.Name.MyTest1", Cd2Cd.GetTypeName(typeof(MyTest1)));
        }
        
        [Test]
        public void NestedClass()
        {
            Assert.AreEqual("Tests.Name.ClassNameTests.MyTest2", Cd2Cd.GetTypeName(typeof(MyTest2)));
        }
        
        [Test]
        public void SimpleGeneric()
        {
            Assert.AreEqual("Tests.Name.MyTest3<System.String>", Cd2Cd.GetTypeName(typeof(MyTest3<string>)));
        }
        
        [Test]
        public void NestedGeneric1()
        {
            Assert.AreEqual("Tests.Name.MyTest3<System.String>.MyTest4<System.Int32>", Cd2Cd.GetTypeName(typeof(MyTest3<string>.MyTest4<int>)));
        }
        
        [Test]
        public void Lists()
        {
            Assert.AreEqual("System.Collections.Generic.List<System.String>", Cd2Cd.GetTypeName(typeof(List<string>)));
        }
        
        [Test]
        public void Arrays()
        {
            Assert.AreEqual("System.String[]", Cd2Cd.GetTypeName(typeof(string[])));
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
        public class MyTest4<U> 
        {
            public T Hi;
        }
    }
}