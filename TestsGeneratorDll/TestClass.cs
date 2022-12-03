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
        public string Namespace { get; }
        public string Content { get; }
        public TestClass(string className, string content, string @namespace)
        {
            ClassName = className;
            Content = content;
            Namespace = @namespace;
        }
    }
}
