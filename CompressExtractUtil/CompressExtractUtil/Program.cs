using CompressExtractUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;

class Program
{
    static String tempFileLocation = Path.GetTempPath() + "/CompressExtractUtil";
    static String versionCode = "1.0.0.1";
    static String userDrive = "C:\\Users\\";

    [STAThread]
    static void Main()
    {
        Console.WriteLine("===============================================================================================================");
        Console.WriteLine("                                  Welcome to Compress / Extract Utility");
        Console.WriteLine("---------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("                                            Version: " + versionCode);
        Console.WriteLine("                             Author - Christian Strydom - www.cvstrydom.co.za");
        Console.WriteLine("===============================================================================================================");
        Console.WriteLine("");
        Console.WriteLine("Description:");
        Console.WriteLine("This utility allows you to copy files from one PC to another while keeping the folder structure of the files.");
        Console.WriteLine("On the source PC use: Compress Files (C). On the destination PC use: Extract Files (E)");
        Console.WriteLine("");
        Console.WriteLine("---------------------------------------------------------------------------------------------------------------");
        Console.WriteLine("");
        Console.WriteLine("Do you want to Compress Files (C), Extract Files (E) or Exit the uititly (X)?");

        bool loop = true;
        if(Directory.Exists(tempFileLocation))
        {
            Directory.Delete(tempFileLocation, true);
        }

        while (loop)
        {
            var line = Console.ReadLine()?.ToUpper();
            switch (line)
            {
                case "C":
                    {
                        Console.WriteLine("Selected 'Compress'");
                        CompressUserFiles();
                        return;
                    }
                case "E":
                    {
                        Console.WriteLine("Selected 'Extract'");
                        ExtractUserFiles();
                        return;
                    }
                case "X":
                    {
                        Console.WriteLine("Selected 'Exit'");
                        Console.WriteLine("Goodbye");
                        EndOfProgramMessage();
                        return;
                    }
                default:
                    {
                        Console.WriteLine("Unkown character");
                        break;
                    }
            }

        }


    }

    private static void CompressUserFiles()
    {
        Console.WriteLine("Select files to add to archive");
        List<String> files = new List<string>();

        String lastFilePath = "C:\\";
        bool loop2 = true;
        while (loop2)
        {

            // Prompt user to select files using OpenFileDialog
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = lastFilePath,
                Filter = "All Files (*.*)|*.*",
                Title = "Select files to add to archive",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                Console.WriteLine("No file selected.");
                EndOfProgramMessage();
                return;
            }

            var newFiles = openFileDialog.FileNames;

            files.AddRange(newFiles);
            foreach (var file in newFiles)
            {
                bool readOnly = false;

                var fileAttributes = File.GetAttributes(file);
                if((fileAttributes  & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    Console.WriteLine("Read only file detected");
                    readOnly = true;
                    FileRemoveReadOnly(file);
                }

                Console.WriteLine("Copying file: " + file);
                CopyFile(file, tempFileLocation);

                if(readOnly)
                {
                    FileAddReadOnly(file);
                }

                lastFilePath = Path.GetDirectoryName(file);
            }

            Console.WriteLine("Do you want to Add more files (A), Start compressing (C) or Exit (X)");
            var line = Console.ReadLine()?.ToUpper();

            switch (line)
            {
                case "A":
                    {
                        Console.WriteLine("Selected 'Add more files'");
                        continue;
                    }
                case "C":
                    {
                        Console.WriteLine("Selected 'Start compressing'");
                        loop2 = false;
                        break;
                    }
                case "X":
                    {
                        Console.WriteLine("Selected 'Exit'");
                        Console.WriteLine("Goodbye");
                        EndOfProgramMessage();
                        return;
                    }
                default:
                    {
                        Console.WriteLine("Unkown character");
                        break;
                    }
            }
        }

        //Replace C:\Users\John\... with C:\Users\$UserName\...
        List<FileData> filesData = new List<FileData>();

        foreach (var file in files)
        {
            if (file.StartsWith(userDrive))
            {
                int count = 0;
                int index = -1;

                while (count < 3)
                {
                    index = file.IndexOf('\\', index + 1);
                    count++;
                }

                if (index != -1)
                {
                    var fileAttributes = File.GetAttributes(file);

                    filesData.Add(new FileData
                    {
                        FileAttributes = fileAttributes,
                        FilePath = userDrive + "$UserName" + file.Remove(0, index),
                    });
                }
                else
                {
                    Console.WriteLine("An error occured");
                    EndOfProgramMessage();
                    return;
                }

            }
            else
            {
                var fileAttributes = File.GetAttributes(file);

                filesData.Add(new FileData
                {
                    FileAttributes = fileAttributes,
                    FilePath = file,
                });
            }
        }

        Console.WriteLine("Generating file paths text file");
        var txtFile = tempFileLocation + "/filedata.json";

        var dataToStore = JsonConvert.SerializeObject(filesData);

        // Create a text file with the file paths
        File.WriteAllText(txtFile, dataToStore);

        var username = Environment.UserName;

        // Prompt user to select the location to save the zip file
        var saveFileDialog = new SaveFileDialog
        {
            InitialDirectory = "C:\\",
            DefaultExt = "zip",
            Filter = "Zip Files (*.zip)|*.zip",
            FileName = "Archive_" + username + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".zip",
            Title = "Save archive",
            
        };

        if (saveFileDialog.ShowDialog() != DialogResult.OK)
        {
            Console.WriteLine("No file selected.");
            EndOfProgramMessage();
            return;
        }

        var zipFile = saveFileDialog.FileName;

        if (File.Exists(zipFile))
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

        Console.WriteLine("Sucess, goodbye!");
        EndOfProgramMessage();
    }

    private static void ExtractUserFiles()
    {
        Console.WriteLine("Select zip archive to extract");

        // Prompt user to select files using OpenFileDialog
        var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = "C:\\",
            Filter = "Zip Files|*.zip;",
            Title = "Select zip archive to extract",
        };

        if (openFileDialog.ShowDialog() != DialogResult.OK)
        {
            Console.WriteLine("No file selected.");
            EndOfProgramMessage();
            return;
        }

        var zipFile = openFileDialog.FileName;

        Console.WriteLine("Extracting zip file. Please be patient...");
        if (!Directory.Exists(tempFileLocation))
        {
            Directory.CreateDirectory(tempFileLocation);
        }
        else
        {
            Directory.Delete(tempFileLocation, true);
            Directory.CreateDirectory(tempFileLocation);
        }

        ZipFile.ExtractToDirectory(zipFile, tempFileLocation);
        Console.WriteLine("Zip file successfully extracted");

        if (!File.Exists(tempFileLocation + "/filedata.json"))
        {
            Console.WriteLine("Error, invalid archive file, filedata.json not found!");
            EndOfProgramMessage();
            return;
        }

        Console.WriteLine("Start copying files to correct locations");
        List<FileData> filesData = JsonConvert.DeserializeObject<List<FileData>>(File.ReadAllText(tempFileLocation + "/filedata.json"));

        //string[] filepaths = File.ReadAllLines(tempFileLocation + "/filedata.json");
        string userFilesStoragePath = "";
        OverwritePolicy overwritePolicy = OverwritePolicy.None;

        foreach (FileData fileData in filesData)
        {
            var filepath = fileData.FilePath;
            string fileName = Path.GetFileName(filepath);
            string finalFilePath = filepath;

            if (finalFilePath.StartsWith(userDrive + "$UserName"))
            {
                if (userFilesStoragePath == "")
                {
                    var username = Environment.UserName.ToLower();
                    Console.WriteLine("");
                    Console.WriteLine("!###############################################################!");
                    Console.WriteLine("The file we're trying to copy originated from C:\\Users\\UserName\\... which location differs for each PC depending on the account name.");
                    Console.WriteLine("We have detected you account folder name is: " + username);
                    Console.WriteLine("Would you therefore like to copy the files to: " + userDrive + username + "\\...?");
                    Console.WriteLine("Yes (Y) / I would like to specify a custom name (N) / Exit (X)");
                    var line = Console.ReadLine()?.ToUpper();

                    switch (line)
                    {
                        case "Y":
                            {
                                userFilesStoragePath = userDrive + username;
                                Console.WriteLine("User folder path set to: " + userFilesStoragePath + "\\...");
                                finalFilePath = SetFilePathToCurrentUserFolder(finalFilePath, userFilesStoragePath);
                                break;
                            }
                        case "N":
                            {
                                Console.WriteLine("Please specify a custom user folder name (Case sensitivity is extremely important!)");
                                var customUserFolderName = Console.ReadLine();
                                userFilesStoragePath = userDrive + customUserFolderName;
                                Console.WriteLine("User folder path set to: " + userFilesStoragePath + "\\...");
                                finalFilePath = SetFilePathToCurrentUserFolder(finalFilePath, userFilesStoragePath);
                                break;
                            }
                        case "X":
                            {
                                Console.WriteLine("Goodbye!");
                                EndOfProgramMessage();
                                return;
                            }
                        default:
                            {
                                Console.WriteLine("Unknown character. Goodbye!");
                                return;
                            }
                    }
                }
                else
                {
                    finalFilePath = SetFilePathToCurrentUserFolder(finalFilePath, userFilesStoragePath);
                }
            }

            string sourceFile = tempFileLocation + "/" + fileName;
            if (File.Exists(sourceFile))
            {
                if (File.Exists(finalFilePath))
                {
                    if (overwritePolicy == OverwritePolicy.OverwriteAll)
                    {
                        Console.WriteLine("Deleting file at: " + finalFilePath);
                        File.Delete(finalFilePath);
                        Console.WriteLine("Copying to: " + finalFilePath);
                        File.Copy(sourceFile, finalFilePath, true);
                        File.SetAttributes(finalFilePath, fileData.FileAttributes);
                        continue;
                    }

                    if (overwritePolicy == OverwritePolicy.SkipAll)
                    {
                        Console.WriteLine("Skipping file: " + finalFilePath);
                        continue;
                    }

                    Console.WriteLine("");
                    Console.WriteLine("!###############################################################!");
                    Console.WriteLine("The file already exists at: " + finalFilePath);
                    Console.WriteLine("Do you want to Overwrite (O) this file, Overwrite all files (OA), Skip (S) this file, Skip all files (SA) or Exit (X)?");

                    bool loop3 = true;
                    while (loop3)
                    {
                        var line = Console.ReadLine()?.ToUpper();
                        switch (line)
                        {
                            case "O":
                                {
                                    Console.WriteLine("Selected 'Overwrite'");
                                    Console.WriteLine("Overwriting file at: " + finalFilePath);
                                    File.Delete(finalFilePath);
                                    File.Copy(sourceFile, finalFilePath, true);
                                    File.SetAttributes(finalFilePath, fileData.FileAttributes);
                                    loop3 = false;
                                    break;
                                }
                            case "S":
                                {
                                    Console.WriteLine("Selected 'Skip'");
                                    Console.WriteLine("Skipping file: " + finalFilePath);
                                    loop3 = false;
                                    break;
                                }
                            case "X":
                                {
                                    Console.WriteLine("Selected 'Exit'");
                                    Console.WriteLine("Goodbye");
                                    EndOfProgramMessage();
                                    return;
                                }
                            case "OA":
                                {
                                    Console.WriteLine("Selected 'Overwrite All'");
                                    Console.WriteLine("Overwriting file at: " + finalFilePath);
                                    File.Delete(finalFilePath);
                                    File.Copy(sourceFile, finalFilePath, true);
                                    File.SetAttributes(finalFilePath, fileData.FileAttributes);
                                    overwritePolicy = OverwritePolicy.OverwriteAll;
                                    loop3 = false;
                                    break;
                                }
                            case "SA":
                                {
                                    Console.WriteLine("Selected 'Skip All'");
                                    Console.WriteLine("Skipping file: " + finalFilePath);
                                    overwritePolicy = OverwritePolicy.SkipAll;
                                    loop3 = false;
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unkown character");
                                    break;
                                }
                        }
                    }

                }
                else
                {

                    if (!Directory.Exists(Path.GetDirectoryName(finalFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath));
                    }
                    Console.WriteLine("Copying to: " + finalFilePath);
                    File.Copy(sourceFile, finalFilePath, true);
                    File.SetAttributes(finalFilePath, fileData.FileAttributes);
                }
            }
        }
        Console.WriteLine("All files succefully copied");
        Directory.Delete(tempFileLocation, true);
        Console.WriteLine("Temp folder deleted");
        Console.WriteLine("Sucess, goodbye!");
        EndOfProgramMessage();
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

    private static string SetFilePathToCurrentUserFolder(String originalFileName, String userFilesStoragePath)
    {
        int count = 0;
        int index = -1;
        String returnFilePath = "";

        while (count < 3)
        {
            index = originalFileName.IndexOf('\\', index + 1);
            count++;
        }

        if (index != -1)
        {
            returnFilePath = userFilesStoragePath + originalFileName.Remove(0, index);
        }
        else
        {
            Console.WriteLine("An error occurred!");
            EndOfProgramMessage();
        }

        return returnFilePath;
    }

    private enum OverwritePolicy
    {
        None = 0,
        OverwriteAll = 1,
        SkipAll = 2,
    }

    private static void EndOfProgramMessage()
    {
        Console.WriteLine("Press any key to close the application.");
        Console.ReadLine();
    }

    private static void FileRemoveReadOnly(string path)
    {
        Console.WriteLine("Removed read only property");
        FileAttributes attributes = File.GetAttributes(path);
        attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
        File.SetAttributes(path, attributes);
    }

    private static void FileAddReadOnly(string path)
    {
        Console.WriteLine("Added read only property");
        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
    }

    private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
    {
        return attributes & ~attributesToRemove;
    }
}
