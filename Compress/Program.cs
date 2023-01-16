using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

class Program
{
    static String tempFileLocation = Environment.CurrentDirectory + "/temp";

    [STAThread]
    static void Main()
    {
        Console.WriteLine("Welcome to Compress / Extract utility");
        Console.WriteLine("=====================================");
        Console.WriteLine("This utility compresses selected files and keeps the files' filepaths");
        Console.WriteLine("Which allows the utility to extract the files on a different PC to the same filepaths as the original PC");
        Console.WriteLine("");
        Console.WriteLine("Do you want to Compress Files (C), Extract Files (E) or Exit the uititly (X)?");

        bool loop = true;

        while (loop)
        {
            var line = Console.ReadLine()?.ToUpper();
            switch (line)
            {
                case "C":
                    {
                        Console.WriteLine("Compress selected");
                        loop = false;
                        break;
                    }
                case "E":
                    {
                        Console.WriteLine("Extract selected");
                        break;
                    }
                case "X":
                    {
                        Console.WriteLine("Exit selected selected");
                        Console.WriteLine("Goodbye");
                        return;
                    }
                default:
                    {
                        Console.WriteLine("Unkown character");
                        break;
                    }
            }

        }

        // Prompt user to select files using OpenFileDialog
        var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Environment.CurrentDirectory,
            Filter = "All Files (*.*)|*.*",
            Title = "Select files to add to archive",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var files = openFileDialog.FileNames;
        foreach (var file in files)
        {
            Console.WriteLine("Copying file: " + file);
            CopyFile(file, tempFileLocation);
        }

        Console.WriteLine("Generating file paths text file");
        var txtFile = "temp/filepaths.txt";
        // Create a text file with the file paths
        File.WriteAllLines(txtFile, files.Select(x => x));

        // Prompt user to select the location to save the zip file
        var saveFileDialog = new SaveFileDialog
        {
            DefaultExt = "zip",
            Filter = "Zip Files (*.zip)|*.zip",
            FileName = "Archive.zip",
            Title = "Save archive",
            InitialDirectory = Environment.CurrentDirectory
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        var zipFile = saveFileDialog.FileName;

        if(File.Exists(zipFile))
        {
            Console.WriteLine("Deleting old zip file");
            File.Delete(zipFile);
        }

        Console.WriteLine("Generating zip file, please be patient...");
        // Create a zip file with the selected files and the text file
        ZipFile.CreateFromDirectory(tempFileLocation, zipFile, CompressionLevel.Optimal, false, Encoding.UTF8);
        Console.WriteLine("Zip file succesfully created");
        Directory.Delete(tempFileLocation, true);
        Console.WriteLine("Temp folder deleted");
    }

    String[]? SelectFileToCompress()
    {
        // Prompt user to select files using OpenFileDialog
        var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Environment.CurrentDirectory,
            Filter = "All Files (*.*)|*.*",
            Title = "Select files to add to archive",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            return null;
        }

        return openFileDialog.FileNames;
    }


    private static void CopyFile(string sourceFile, string destinationDirectory)
    {
        // Create the destination directory if it doesn't exist
        Directory.CreateDirectory(destinationDirectory);

        // Use Path.Combine to combine the destination directory and the file name
        var destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));

        // Use File.Copy to copy the file
        File.Copy(sourceFile, destinationFile, true);
    }

}
