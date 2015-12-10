using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spi.IO
{
    public class Misc
    {
        public static string GetPrettyFilesize(ulong Filesize)
        {
            StringBuilder sb = new StringBuilder(50);
            Win32.StrFormatByteSize((long)Filesize, sb, 50);
            return sb.ToString();
        }
        /// <summary>
        /// delete all the dirs and files in the given directory
        /// except the dirs and files specified in the two arrays
        /// </summary>
        /// <param name="dir">Directory to clean</param>
        /// <param name="ExcludeDirs">List of directories which should not be deleted</param>
        /// <param name="ExcludeFiles">List of files which should not be deleted</param>
        public static void EmptyDirectory(string dir, ICollection<string> ExcludeDirs, ICollection<string> ExcludeFiles, Action<string> DebugCallBack)
        {
            //
            // delete all directories
            //
            foreach (string Dir2Del in System.IO.Directory.GetDirectories(dir))
            {
                if (DebugCallBack != null) DebugCallBack(String.Format("dir enumerated [{0}]", Dir2Del));
                string DirOnlyName = Path.GetFileName(Dir2Del);

                if (!Spi.StringTools.Contains_OrdinalIgnoreCase(ExcludeDirs, DirOnlyName))
                {
                    if (DebugCallBack != null) DebugCallBack(String.Format("deleting dir [{0}]", DirOnlyName));
                    System.IO.Directory.Delete(Dir2Del, true); // true = delete recurse    
                }
            }
            //
            // delete all files
            //
            foreach (string FileToDel in System.IO.Directory.GetFiles(dir))
            {
                if (DebugCallBack != null) DebugCallBack(String.Format("file enumerated [{0}]", FileToDel));
                if (!Spi.StringTools.Contains_OrdinalIgnoreCase(ExcludeFiles, Path.GetFileName(FileToDel)))
                {
                    File.SetAttributes(FileToDel, FileAttributes.Normal);
                    if (DebugCallBack != null) DebugCallBack(String.Format("deleting file [{0}]", FileToDel));
                    File.Delete(FileToDel);
                }
            }
        }
    }
}
