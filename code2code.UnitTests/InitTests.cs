using System;
using System.Collections;
using System.Collections.Generic;
using code2code;
using NUnit.Framework;

namespace Tests.New
{
    public class InitTests
    {
        [Test]
        public void TestStringField()
        {
            Assert.AreEqual("new Tests.New.ClassStringField\n{\nStr = \"aaa\"\n}", Cd2Cd.Generate(new ClassStringField{Str = "aaa"}));
        }
        
        [Test]
        public void TestStringProp()
        {
            Assert.AreEqual("new Tests.New.ClassStringProp\n{\nStr = \"aaa\"\n}", Cd2Cd.Generate(new ClassStringProp{Str = "aaa"}));
        }
        
        [Test]
        public void Test2Props()
        {
            Assert.AreEqual("new Tests.New.Class2Props\n{\nStr = \"aaa\",\nInt = 5\n}", Cd2Cd.Generate(new Class2Props{Str = "aaa", Int = 5}));
        }
        
        [Test]
        public void ClassEnumerablePropList()
        {
            Assert.AreEqual("new Tests.New.ClassEnumerableProp1\n{\nVal = new System.Collections.Generic.List<System.String>\n{\n\"aaa\"\n}\n}", Cd2Cd.Generate(new ClassEnumerableProp1{Val = new List<string> { "aaa" }}));
        }
        
        [Test]
        public void ClassEnumerablePropArray()
        {
            Assert.AreEqual("new Tests.New.ClassEnumerableProp1\n{\nVal = new System.String[]\n{\n\"aaa\"\n}\n}", Cd2Cd.Generate(new ClassEnumerableProp1{Val = new string[] { "aaa" }}));
        }

        
        
        // [Test]
        // public void NestedClass()
        // {
        //     Assert.AreEqual("new Tests.New.InitTests.MyTest2", Cd2Cd.Generate(new MyTest2()));
        // }
        
        // [Test]
        // public void SimpleGeneric()
        // {
        //     Assert.AreEqual("Tests.MyTest3<System.String>", Cd2Cd.GetTypeName(typeof(MyTest3<string>)));
        // }
        
        // [Test]
        // public void NestedGeneric1()
        // {
        //     Assert.AreEqual("Tests.MyTest3<System.String>.MyTest4<System.Int32>", Cd2Cd.GetTypeName(typeof(MyTest3<string>.MyTest4<int>)));
        // }
        
        // [Test]
        // public void Lists()
        // {
        //     Assert.AreEqual("System.Collections.Generic.List<System.String>", Cd2Cd.GetTypeName(typeof(List<string>)));
        // }
        
        // [Test]
        // public void IEnumerableWithoutResolver()
        // {
        //     Assert.AreEqual("System.Collections.Generic.IEnumerable<System.String>", Cd2Cd.GetTypeName(typeof(IEnumerable<string>)));
        // }
        
        // [Test]
        // public void IEnumerableWithResolver()
        // {
        //     Assert.AreEqual("System.Collections.Generic.List<System.String>", Cd2Cd.GetTypeNameWithInterfaceResolution(typeof(IEnumerable<string>), new Dictionary<Type, Type> { { typeof(IEnumerable<>), typeof(List<>) } }));
        // }

        public class MyTest2
        {
        }
    }

    public class ClassStringField
    {
        public string Str;
    }

    public class ClassStringProp
    {
        public string Str;
    }

    public class ClassEnumerableProp1
    {
        public IEnumerable Val;
    }

    public class Class2Props
    {
        public string Str;
        public int Int;
    }

    public class MyTest3<T>
    {
        public class MyTest4<U> 
        {
            public T Hi;
        }
    }
}