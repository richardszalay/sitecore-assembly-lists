using System;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks
{
    public class AssemblyListParseException : Exception
    {
        public AssemblyListParseException(string message, string sourceFile, int sourceLine, Exception innerException) 
            : base(message, innerException)
        {
            this.SourceFile = sourceFile;
            this.SourceLine = sourceLine;
        }

        public string SourceFile { get; set; }
        public int SourceLine { get; set; }
    }
}
