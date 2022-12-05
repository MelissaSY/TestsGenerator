using TestsGeneratorDll;

namespace TestsGeneratorTests
{
    public class Tests
    {
        private IOGeneratorPipeline pipeline;

        private string resultDir;

        [SetUp]
        public void Setup()
        {
            pipeline = new IOGeneratorPipeline();
            resultDir = "../../GeneratedTests";
        }

        [Test]
        public async Task FilesInDirectory_TestAsync()
        {
            if(Directory.Exists(resultDir))
            {
                Directory.Delete(resultDir, true);
            }
            _ = await pipeline.StartProccess("../../SourcePath", resultDir);
            var files = Directory.GetFiles(resultDir, "*.cs");
            for(int i = 0; i < files.Length; i++)
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
    }
}