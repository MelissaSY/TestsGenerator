using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsGeneratorApp
{
    public class AsyncFileReader
    {
        public async Task<string> ReadAsync(string filePath) 
        {
            string result = string.Empty;
            using(StreamReader reader = new StreamReader(filePath)) 
            {
                result = await reader.ReadToEndAsync();
            }
            return result;
        }
    }
}
