using System;
using System.Linq;

namespace ServerConsole.Model
{
    public class File
    {
        public string Name { get; set; }

        public int Length { get; set; }
        
        public string Content { get; set; }
    }
}