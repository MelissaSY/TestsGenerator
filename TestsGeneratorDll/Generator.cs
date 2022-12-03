using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks.Dataflow;

namespace TestsGeneratorDll
{
    public class Generator
    {
        public Task<List<TestClass>> Generate(string sourceCode)
        {
            List<TestClass> result = new List<TestClass>();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();


        }
    }
}