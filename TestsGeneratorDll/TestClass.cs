using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorDll
{
    public class TestClass
    {
        public string ClassName { get; }
        public string Content { get; }
        public TestClass(string className, string content)
        {
            ClassName = className;
            Content = content;
        }
    }
}
