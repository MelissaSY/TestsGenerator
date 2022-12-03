﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorApp
{
    public class AsyncFileWriter
    {
        public async Task WriteAsync(string filePath, string content)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
               await writer.WriteAsync(content);
            }
        }
    }
}