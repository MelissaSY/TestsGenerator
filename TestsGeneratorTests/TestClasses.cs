using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestsGeneratorTests
{
    public class TestConstructorClass
    {
        public TestConstructorClass(ICommand command, IFormattable formattable) { }
        public int NormalMethod(string method) { return 7; }
        public static string PublicStaticMethod()
        {
            return "";
        }
    }

    public class TestOtherConstructorClass
    {
        public TestOtherConstructorClass(int i, string? s) { }

        public void VoidMethod(int i)
        { }
    }

    public class MyClassMethods
    {
        public int MyMethodTest(int number, string s, Foo foo)
        {
            return 9;
        }
        private string PrivateMethod()
        {
            return "!panic";
        }
    }
    public class Foo
    {

    }

    public static class Bar
    {
        public static string AnotherMethodToTest(ICommand s)
        {
            return "aa";
        }
    }
}
