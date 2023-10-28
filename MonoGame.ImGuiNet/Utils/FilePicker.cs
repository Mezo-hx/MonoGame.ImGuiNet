using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Monogame.ImGuiNet.Utils
{
    public class FilePicker
    {
        public static string DefaultPath_Windows = "C:\\"; //The default path for windows 
        public static string ImagePath; //The File path from the image
        public static void BeginFilePicker()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // TODO: Stop being lazy and release the FilePicker as quick as possible
            }

            // TODO: Add support for Linux and Mac
        }

        public static void BeginFolderPicker()
        {
            // TODO: Functionality for folder picking
        }
    }
}
