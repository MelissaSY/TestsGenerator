using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using TestsGeneratorDll;
using System.IO;

namespace TestsGeneratorTests
{
    public class Tests
    {
        private IOGeneratorPipeline pipeline;

        private string resultDir;

        private AttributeListSyntax setUpAttributes =
                SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName("SetUp"))));

        private AttributeListSyntax testAttributes =
            SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("Test"))));


        [SetUp]
        public void Setup()
        {
            pipeline = new IOGeneratorPipeline();
            resultDir = "../../GeneratedTests";
        }

        [Test]
        public async Task Result_Directory_Contains_Generated_Tests_TestAsync()
        {
            if (Directory.Exists(resultDir))
            {
                Directory.Delete(resultDir, true);
            }
            _ = await pipeline.StartProccess("../../SourcePath", resultDir);
            var files = Directory.GetFiles(resultDir, "*.cs");
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            Assert.Multiple(() =>
            {
                Assert.That(files.Contains("BarTests.cs"), "Bar test file not found");
                Assert.That(files.Contains("FooTests.cs"), "Foo test file not found");
                Assert.That(files.Contains("MyClassMethodsTests.cs"), "MyClassMethods test file not found");
                Assert.That(files.Contains("TestConstructorClassTests.cs"), "TestConstructorClass test file not found");
                Assert.That(files.Contains("TestOtherConstructorClassTests.cs"), "TestOtherConstructorClass test file not found");
            });
        }
        [Test]
        public async Task OneClass_Per_File_TestAsync()
        {
            if (Directory.Exists(resultDir))
            {
                Directory.Delete(resultDir, true);
            }
            _ = await pipeline.StartProccess("../../SourcePath", resultDir);
            var files = Directory.GetFiles(resultDir, "*.cs");
            bool containsOne = true;
            for (int i =0;i<files.Length && containsOne; i++)
            {
                if (files[i].EndsWith("Tests.cs"))
                {
                    SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(files[i]));
                    CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
                    var classDeclaration = from classes in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                                            select classes;
                    containsOne = (classDeclaration.Count() == 1);
                }
            }
            Assert.That(containsOne, Is.True, "There must be declared 1 class per test");
        }
        [Test]
        public void Static_Class_NoSetUp_But_Method_Test()
        {
            string staticClassText =
            @"public static class StaticClass
            {
                public static string StaticMethodToTest(int number)
                {
                    return ""aa"";
                }
            }";

            TestGenerator generator = new TestGenerator();
            
            var comparerer = new AttributeListEqualityComparere();

            List<TestClass> testClasses = generator.Generate(staticClassText);
            TestClass staticTestClass = testClasses[0];
            
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(staticTestClass.Content)
                    .GetCompilationUnitRoot();
            var classDeclar = from classes in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                                  select classes;
            var @class = classDeclar.First();
            var amethods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            var setUpMethod = from methods in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                              where methods.AttributeLists.Contains(setUpAttributes, comparerer)
                              select methods;

            var testMethod = from methods in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                             where methods.AttributeLists.Contains(testAttributes, comparerer)
                             select methods;

            int setUpCount = setUpMethod.Count();
            int testCount = testMethod.Count();

            Assert.Multiple(() =>
            {
                Assert.That(testClasses.Count, Is.EqualTo(1),
                    "There must be 1 test class generated");
                Assert.That(setUpCount, Is.EqualTo(0),
                    "There must be no setup method for testing static class");
                Assert.That(testCount, Is.EqualTo(1),
                    "There must 1 test method for testing static class wtih 1 method");
            });

        }
        [Test]
        public void Static_Class_NoFields_Test()
        {
            string staticClassText =
            @"public static class StaticClass
            {
                public static string StaticMethodToTest(int number)
                {
                    return ""static"";
                }
            }";

            TestGenerator generator = new TestGenerator();

            List<TestClass> testClasses = generator.Generate(staticClassText);
            TestClass staticTestClass = testClasses.First();
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(staticTestClass.Content)
                 .GetCompilationUnitRoot();
            var classDeclar = from classes in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                              select classes;
            var @class = classDeclar.First();

            var fields = @class.DescendantNodes().OfType<FieldDeclarationSyntax>();
            Assert.That(fields.Count(), Is.EqualTo(0), "There must be no fields for static class");
        }
        [Test]
        public void Non_Static_Class_Contains_SetUp_Test()
        {
            string notStaticClass =
                @"
                    public class TestConstructorClass
                    {
                        public static string StaticMethodString()
                        {
                            return "";
                        }
                    }
                ";

            TestGenerator generator = new TestGenerator();

            var comparerer = new AttributeListEqualityComparere();

            List<TestClass> testClasses = generator.Generate(notStaticClass);
            TestClass staticTestClass = testClasses[0];

            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(staticTestClass.Content)
                    .GetCompilationUnitRoot();
            var classDeclar = from classes in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                              select classes;
            var @class = classDeclar.First();
            var amethods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            var setUpMethod = from methods in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                              where methods.AttributeLists.Contains(setUpAttributes, comparerer)
                              select methods;

            var testMethod = from methods in root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                             where methods.AttributeLists.Contains(testAttributes, comparerer)
                             select methods;

            int setUpCount = setUpMethod.Count();
            int testCount = testMethod.Count();

            Assert.Multiple(() =>
            {
                Assert.That(testClasses.Count, Is.EqualTo(1),
                    "There must be 1 test class generated");
                Assert.That(setUpCount, Is.EqualTo(1),
                    "There must be 1 setup method for testing a non static class");
                Assert.That(testCount, Is.EqualTo(1),
                    "There must be test method for testing not static class wtih 1 method");
            });
        }
        [Test]
        public void Non_Static_Class_Fields_Test()
        {
            string classText =
           @"public class TestClass
            {
                public TestClass(int number)
                {     }
                public string StaticMethodToTest(int number)
                {
                    return ""static"";
                }
            }";

            TestGenerator generator = new TestGenerator();

            List<TestClass> testClasses = generator.Generate(classText);
            TestClass staticTestClass = testClasses.First();
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(staticTestClass.Content)
                 .GetCompilationUnitRoot();
            var classDeclar = from classes in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                              select classes;
            var @class = classDeclar.First();

            var fields = @class.DescendantNodes().OfType<FieldDeclarationSyntax>();

            string numberFieldType = "int";
            string classFieldType = "TestClass";

            Assert.That(fields, Is.Not.Null, "Two fields shoud've been generated (class and constructor parameter)");
            Assert.That(fields.Count(), Is.EqualTo(2), "Two fields shoud've been generated (class and constructor parameter)");
            Assert.Multiple(() =>
            {
                Assert.That(fields.First().Modifiers.Any(SyntaxKind.PrivateKeyword),"Fields should be private");
                Assert.That(fields.Last().Modifiers.Any(SyntaxKind.PrivateKeyword), "Fields should be private");
                Assert.That(fields.First().Declaration.Type.ToString(), Is.AnyOf(numberFieldType, classFieldType), "Field of the wrong type generated");
                Assert.That(fields.Last().Declaration.Type.ToString(), 
                    Is.AnyOf(numberFieldType, classFieldType), "Field of the wrong type generated");
                Assert.That(fields.First().Declaration.Type.ToString(),
                    Is.Not.EqualTo(fields.Last().Declaration.Type.ToString()), "Fields of the same type generated");

            });
        }
        
    }
}