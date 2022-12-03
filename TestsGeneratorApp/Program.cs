namespace TestsGeneratorApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            IOGeneratorPipeline generatorPipeline = new IOGeneratorPipeline();
            generatorPipeline.MaxNumProessingTasks = 1;
            generatorPipeline.MaxNumWritingFiles = 1;
            generatorPipeline.MaxNumReadingFiles = 1;
            List<string> pathes = new List<string>()
            {
                @"D:\5 sem\СПП\СПП\ЛР\лр3\AssemblyBrowserDll\AssemblyInformator.cs",
                @"D:\5 sem\СПП\СПП\ЛР\лр3\AssemblyBrowserDll\FieldInformator.cs",
                @"D:\5 sem\СПП\СПП\ЛР\лр3\AssemblyBrowserDll\TypeInformator.cs"
            };
            string resdir;

            resdir = @"D:\5 sem\СПП\СПП\TestDir";


           /* Console.Write("Enter result directory: ");
            resdir = Console.ReadLine();
            Console.WriteLine("Enter full pathes of files to process (key A to stop): ");
            while(!(path = Console.ReadLine()).Equals("A"))
            {
                pathes.Add(path);
            }*/
           for(int i = 0; i < 1000; i++)
            {
                var t = await generatorPipeline.StartProccess(pathes, resdir);
            }
            Console.WriteLine("a");
            Console.ReadLine();
        }
    }
}