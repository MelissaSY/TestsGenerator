﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsGeneratorDll;
using System.Threading.Tasks.Dataflow;

namespace TestsGeneratorApp
{
    public class IOGeneratorPipeline
    {
        private AsyncFileReader _fileReader;
        private AsyncFileWriter _fileWriter;
        private int _maxNumReadingFiles;
        private int _maxNumWritingFiles;
        private int _maxNumProcessingFiles;
        public int MaxNumReadingFiles 
        { 
            get
            {
                return _maxNumReadingFiles;
            }
            set
            {
                if(value > 0)
                {
                    _maxNumReadingFiles = value;
                }
            }
        }
        public int MaxNumWritingFiles 
        {
            get
            {
                return _maxNumWritingFiles;
            }
            set
            {
                if(value > 0)
                {
                    _maxNumWritingFiles = value;
                }
            }
        }
        //for generator
        public int MaxNumProessingTasks
        { 
            get
            {
                return _maxNumProcessingFiles;
            }
            set
            {
               if(value > 0)
               {
                    _maxNumProcessingFiles = value;
               }
            }
        }
        public TestGenerator TestGenerator { get; }
        private IOGeneratorPipeline(int maxNumReadingFiles, int maxNumWritingFiles, int maxNumProcessingTasks) 
        {
            MaxNumReadingFiles = maxNumReadingFiles;
            MaxNumWritingFiles = maxNumWritingFiles;
            MaxNumProessingTasks = maxNumProcessingTasks;

            _fileReader = new AsyncFileReader();
            _fileWriter = new AsyncFileWriter();

            TestGenerator= new TestGenerator();
        }
        public IOGeneratorPipeline():this(10, 10, 10)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFilePaths"></param>
        /// <param name="resultDir"></param>
        /// <returns>List of processed files</returns>
        public async Task<List<string>> StartProccess(List<string> sourceFilePaths, string resultDir)
        {
            var readingOptions = new ExecutionDataflowBlockOptions();
            var writingOptions = new ExecutionDataflowBlockOptions();
            var generatorOptions = new ExecutionDataflowBlockOptions();

            readingOptions.MaxDegreeOfParallelism = MaxNumReadingFiles;
            writingOptions.MaxDegreeOfParallelism = MaxNumWritingFiles;
            generatorOptions.MaxDegreeOfParallelism = MaxNumProessingTasks;

            List<string> filesToProcess = new List<string>();

            if(!Directory.Exists(resultDir))
            {
                Directory.CreateDirectory(resultDir);
            }
            foreach(string sourceFilePath in sourceFilePaths)
            {
                if(File.Exists(sourceFilePath))
                {
                    filesToProcess.Add(sourceFilePath);
                }
            }

            List<string> resultPathes = new List<string>();

            var readFile = new TransformBlock<string, string>(async filePath =>
            await _fileReader.ReadAsync(filePath), readingOptions);

            var generateTest = new TransformManyBlock<string, KeyValuePair<string, string>>(async sourceFile =>
            await ComposeResultTestFiles(sourceFile, resultDir), generatorOptions);

            var writeFile = new ActionBlock<KeyValuePair<string, string>>(async pathContent =>
            await _fileWriter.WriteAsync(pathContent.Key, pathContent.Value), writingOptions);

            var linkOPtions = new DataflowLinkOptions { PropagateCompletion = true };

            readFile.LinkTo(generateTest, linkOPtions);
            generateTest.LinkTo(writeFile, linkOPtions);

            foreach(string filePath in filesToProcess)
            {
                readFile.Post(filePath);
            }
            readFile.Complete();

            await writeFile.Completion;

            return resultPathes;
        }

        private async Task<Dictionary<string, string>> ComposeResultTestFiles(string sourceContent, string resultDir)
        {
            Dictionary<string, string> pathContent = new Dictionary<string, string>();
            List<TestClass> testClasses = await TestGenerator.Generate(sourceContent);
            string filePath = string.Empty;
            foreach (var test in testClasses)
            {
                filePath = Path.Combine(resultDir, test.ClassName);
                while(pathContent.ContainsKey(filePath + ".cs"))
                {
                    filePath = $"{filePath}_1";
                }
                pathContent.Add(filePath, test.Content);
            }
            return pathContent;
        }
    }
}
