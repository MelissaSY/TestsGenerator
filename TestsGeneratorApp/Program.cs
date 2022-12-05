using TestsGeneratorDll;
namespace TestsGeneratorApp
{
    /*
     
     -----------------------------------------------------------------------
        add
        parameters: file or directory that contains files
        
        rm
        parameters: file to be removed

        ls 
        lists the added files
        paramters: -

        out 
        no parameters: get the output dir
        parameters: the output directory to be set

        start
        paramters: -
        starts processing and generating tests

        maxread
        maxwrite
        maxproc
        
        exit
        paramters: -
     -----------------------------------------------------------------------
     
     */

    internal class Program
    {
        static async Task Main(string[] args)
        {
            IOGeneratorPipeline generatorPipeline = new IOGeneratorPipeline();
            List<string> pathes = new List<string>();
            string resdir;
            int maxReading = generatorPipeline.MaxNumReadingFiles;
            int maxWriting = generatorPipeline.MaxNumWritingFiles;
            int maxProcessing = generatorPipeline.MaxNumProessingTasks;

            resdir = "";
            string input = "";

            Console.WriteLine("To pass parameters enter '>'\nNO whitespaces (they are significant)\n");
            while(!input.Equals("exit")) 
            {
                string? tempInp = Console.ReadLine();
                input = tempInp == null ? "" : tempInp;
                string[] command = input.Split('>', StringSplitOptions.RemoveEmptyEntries);
                if(command.Length > 0 )
                {
                    switch(command[0])
                    {
                        case "add":
                            if(command.Length > 1 && File.Exists(command[1]))
                            {
                                pathes.Add(command[1]);
                                Console.WriteLine("successfully added");
                            }
                            else if(command.Length > 1 && Directory.Exists(command[1]))
                            {
                                var files = Directory.GetFiles(command[1], "*.cs");
                                if(files != null)
                                {
                                    pathes.AddRange(files);
                                    Console.WriteLine("successfully added");
                                }
                            }
                            else
                            {
                                Console.WriteLine("wrong arguments of command 'add'");
                            }
                            break;
                        case "rm":
                            if (command.Length > 1 && pathes.Contains(command[1]))
                            {
                                pathes.Remove(command[1]);
                                Console.WriteLine("successfully removed");
                            }
                            else
                            {
                                Console.WriteLine("ignored");
                            }
                            break;
                        case "ls":
                            if(pathes.Count == 0) 
                            {
                                Console.WriteLine("no files");
                            }
                            foreach(string filePath in pathes)
                            {
                                Console.WriteLine(filePath);
                            }
                            break;
                        case "out":
                            if (command.Length > 1)
                            {
                                resdir = command[1];
                                Console.WriteLine("successfully set");
                            }
                            else
                            {
                                if (resdir.Equals(""))
                                {
                                    Console.WriteLine("no directory set");
                                }
                                else
                                Console.WriteLine(resdir);
                            }
                            break;
                        case "maxread":
                            if (command.Length > 1)
                            {
                                int tempMaxRead = 0;
                                if (int.TryParse(command[1], out tempMaxRead) && tempMaxRead > 0)
                                {
                                    maxReading = tempMaxRead;
                                    Console.WriteLine("successfully set");
                                }
                                else
                                {
                                    Console.WriteLine("wrong parameters of 'maxread' instruction");
                                }
                            }
                            else
                            {
                                Console.WriteLine(maxReading);
                            }
                            break;
                        case "maxwrite":
                            if (command.Length > 1)
                            {
                                int tempMaxWrite = 0;
                                if (int.TryParse(command[1], out tempMaxWrite) && tempMaxWrite > 0)
                                {
                                    maxWriting = tempMaxWrite;
                                    Console.WriteLine("successfully set");
                                }
                                else
                                {
                                    Console.WriteLine("wrong parameters of 'maxwrite' instruction");
                                }
                            }
                            else
                            {
                                Console.WriteLine(maxProcessing);
                            }
                            break;
                        case "maxproc":
                            if (command.Length > 1)
                            {
                                int tempMaxProc = 0;
                                if (int.TryParse(command[1], out tempMaxProc) && tempMaxProc > 0)
                                {
                                    maxProcessing = tempMaxProc;
                                    Console.WriteLine("successfully set");
                                }
                                else
                                {
                                    Console.WriteLine("wrong parameters of 'maxproc' instruction");
                                }
                            }
                            else
                            {
                                Console.WriteLine(maxWriting);
                            }
                            break;
                        case "start":
                            if(!(pathes.Count == 0 || resdir.Equals("")))
                            {
                                await generatorPipeline.StartProccess(pathes, resdir);
                                Console.WriteLine("finished");
                            }
                            else
                            {
                                Console.WriteLine("some parameters are missing");
                            }
                            break;
                        case "exit":
                            break;
                        default:
                            Console.WriteLine("wrong command");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("wrong input");
                }
            }
        }
    }
}