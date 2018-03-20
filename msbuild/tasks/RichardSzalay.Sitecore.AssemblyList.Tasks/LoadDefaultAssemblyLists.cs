using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace RichardSzalay.Sitecore.AssemblyList.Tasks
{
    public class LoadDefaultAssemblyLists : Task
    {
        [Required]
        public ITaskItem Manifest { get; set; }

        [Output]
        public ITaskItem[] DefaultAssemblyLists { get; set; }

        public override bool Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
