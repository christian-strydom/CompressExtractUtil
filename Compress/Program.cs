using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

class Program
{
    [STAThread]
    static void Main()
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
            return;
        }

        var files = openFileDialog.FileNames;
        foreach (var file in files)
        {
            CopyFileToTempLocation(file, Environment.CurrentDirectory + "/temp");
        }


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

        // Create a zip file with the selected files and the text file
        ZipFile.CreateFromDirectory(Environment.CurrentDirectory + "/temp", zipFile, CompressionLevel.Optimal, false, Encoding.UTF8);
        Directory.Delete(Environment.CurrentDirectory + "/temp",true);
    }


    private static void CopyFileToTempLocation(string sourceFile, string destinationDirectory)
    {
        // Create the destination directory if it doesn't exist
        Directory.CreateDirectory(destinationDirectory);

        // Use Path.Combine to combine the destination directory and the file name
        var destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));

        // Use File.Copy to copy the file
        File.Copy(sourceFile, destinationFile, true);
    }

}
