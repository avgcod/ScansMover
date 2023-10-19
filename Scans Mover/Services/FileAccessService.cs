using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Scans_Mover.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Scans_Mover.Services
{
    /// <summary>
    /// Service to access filed from a local or remote file system.
    /// </summary>
    public static class FileAccessService
    {
        /// <summary>
        /// Reads settings information from a json file.
        /// </summary>
        /// <param name="fileName">Json file to load information from.</param>
        /// <returns>Settings object</returns>
        public static Settings LoadSettings(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    string jsonString = File.ReadAllText(fileName);
                    return JsonSerializer.Deserialize<Settings>(jsonString) ?? new Settings();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

            return new Settings();
        }

        /// <summary>
        /// Loads a PDF document.
        /// </summary>
        /// <param name="fileName">PDF file to load.</param>
        /// <returns>PDF document.</returns>
        public static PdfDocument LoadPDFDocument(string fileName)
        {
            return PdfDocument.Open(fileName);
        }

        /// <summary>
        /// Reads settings information from a json file asynchronously.
        /// </summary>
        /// <param name="fileName">Json file to load information from.</param>
        /// <returns>Settings object</returns>
        public static async Task<Settings> LoadSettingsAsync(string fileName)
        {
            Settings newSettings = new Settings();
            if (File.Exists(fileName))
            {
                try
                {
                    Stream theStream = File.OpenRead(fileName);
                    newSettings = await JsonSerializer.DeserializeAsync<Settings>(theStream) ?? new Settings();
                    theStream.Close();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

            }

            return newSettings;
        }

        /// <summary>
        /// Loads a PDF document asynchronously.
        /// </summary>
        /// <param name="fileName">PDF file to load.</param>
        /// <returns>PDF document.</returns>
        public static async Task<PdfDocument> LoadPDFDocumentAsync(string fileName)
        {
            return await Task.Run(() => PdfDocument.Open(fileName));
        }

        /// <summary>
        /// Saves settings to a json file.
        /// </summary>
        /// <param name="information">Settings to save.</param>
        /// <param name="fileName">File name to save to.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error trying to save.</exception>
        public static bool SaveSettings(Settings information, string fileName)
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
                string jsonText = JsonSerializer.Serialize(information, options);
                File.WriteAllText(fileName,jsonText);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Saves a PDF document.
        /// </summary>
        /// <param name="fileName">PDF file to save to.</param>
        /// <param name="thePDF">The PDF document to save.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error saving.</exception>
        public static bool SavePDFDocument(string fileName, byte[] thePDF)
        {
            if (!File.Exists(fileName))
            {
                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    File.WriteAllBytes(fileName, thePDF);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Saves settings to a json file asynchronously.
        /// </summary>
        /// <param name="information">Settings to save.</param>
        /// <param name="fileName">File name to save to.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error trying to save.</exception>
        public static async Task<bool> SaveSettingsAsync(Settings information, string fileName)
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions() { WriteIndented = true };
                string jsonText = JsonSerializer.Serialize(information, options);
                await File.WriteAllTextAsync(fileName, jsonText);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Saves a log of the months moved files.
        /// </summary>
        /// <param name="information">List of moved information</param>
        /// <param name="fileName">Text file to save to.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception"></exception>
        public static async Task<bool> SaveLogAsync(IEnumerable<string> information, string fileName)
        {
            try
            {
                using (TextWriter writer = new StreamWriter(fileName))
                {
                    foreach (string theInfo in information)
                    {
                        await writer.WriteLineAsync(theInfo);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Saves a PDF document asynchronously.
        /// </summary>
        /// <param name="fileName">PDF file to save to.</param>
        /// <param name="thePDF">The PDF document to save.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error saving.</exception>
        public static async Task<bool> SavePDFDocumentAsync(string fileName, byte[] thePDF)
        {
            if (!File.Exists(fileName))
            {
                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    await File.WriteAllBytesAsync(fileName, thePDF);

                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Renames a file.
        /// </summary>
        /// <param name="oldFileName">Old file name.</param>
        /// <param name="newFileName">New file name.</param>
        /// <returns>If the operation succeeded.</returns>
        public static bool RenameFile(string oldFileName, string newFileName)
        {
            return MoveFile(oldFileName, newFileName);
        }

        /// <summary>
        /// Renames a file asynchronously.
        /// </summary>
        /// <param name="oldFileName">Old file name.</param>
        /// <param name="newFileName">New file name.</param>
        /// <returns>If the operation succeeded.</returns>
        public static async Task<bool> RenameFileAsync(string oldFileName, string newFileName)
        {
            return await MoveFileAsync(oldFileName, newFileName);
        }

        /// <summary>
        /// Returns the names of all subdirectories in a folder.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search.</param>
        /// <returns>List of subdirectory names found.</returns>
        public static IEnumerable<string> GetSubDirectories(string rootDirectory)
        {
            return new DirectoryInfo(rootDirectory).EnumerateDirectories().Select(x => x.Name).ToList();
        }

        /// <summary>
        /// Returns the names of all subdirectories in a folder asynchronously.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search.</param>
        /// <returns>List of subdirectory names found.</returns>
        public static async Task<IEnumerable<string>> GetSubDirectoriesAsync(string rootDirectory)
        {
            return await Task.Run(() => new DirectoryInfo(rootDirectory).EnumerateDirectories().Select(x => x.Name).ToList());
        }

        /// <summary>
        /// Gets all files in a directory.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search.</param>
        /// <returns>List of files found.</returns>
        public static FileInfo[] GetFiles(string rootDirectory)
        {
            return new DirectoryInfo(rootDirectory).GetFiles();
        }

        /// <summary>
        /// Gets all files in a directory asynchronously.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search.</param>
        /// <returns>List of files found.</returns>
        public static async Task<IEnumerable<FileInfo>> GetFilesAsync(string rootDirectory)
        {
            return await Task.Run(() => new DirectoryInfo(rootDirectory).GetFiles().ToList());
        }

        /// <summary>
        /// Gets a proper page title with invalid characters removed.
        /// Also verifies the title fits the current length and minimum requirements.
        /// </summary>
        /// <param name="pageTitle">Original page title.</param>
        /// <returns>Cleaned page title or empty page title if the title does not meet the requirements.</returns>
        //private string GetPageTitle(string pageTitle, double numMin, ScanType currentScanType)
        //{
        //    if (currentScanType != ScanType.Shipping)
        //    {
        //        string[] splitted = pageTitle.Split(' ');
        //        string title = string.Empty;
        //        for (int i = 0; i < splitted.Length; i++)
        //        {
        //            if (splitted[i].All(x => char.IsLetterOrDigit(x)) && int.TryParse(splitted[i], out int result) && result.ToString().Length == numMin.ToString().Length && result >= numMin)
        //            {
        //                title = splitted[i].Replace('\\', '-').Replace('/', '-').Replace(':', '-').Replace('*', '-').Replace('?', '-').Replace('"', '-').Replace('<', '-').Replace('>', '-').Replace('|', '-');

        //                int titleNum;
        //                if (int.TryParse(title, out titleNum))
        //                {
        //                    if (titleNum > numMin && (titleNum - numMin > 130))
        //                    {
        //                        return string.Empty;
        //                    }
        //                    else if (titleNum < numMin && (titleNum - numMin < -130))
        //                    {
        //                        return string.Empty;
        //                    }
        //                    else
        //                    {
        //                        return title;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return string.Empty;
        //}

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="directoryLocation">The directory to check.</param>
        /// <returns>If the directory exists.</returns>
        public static bool DirectoryExists(string directoryLocation)
        {
            return Directory.Exists(directoryLocation);
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <returns>If the file exists.</returns>
        public static bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="directoryLocation">The directory to check.</param>
        /// <returns>If the directory exists.</returns>
        public static async Task<bool> DirectoryExistsAsync(string directoryLocation)
        {
            return await Task.Run(() => Directory.Exists(directoryLocation));
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="fileName">The file to check.</param>
        /// <returns>If the file exists.</returns>
        public static async Task<bool> FileExistsAsync(string fileName)
        {
            return await Task.Run(() => File.Exists(fileName));
        }

        /// <summary>
        /// Moves a file to a new location.
        /// </summary>
        /// <param name="source">Original file location.</param>
        /// <param name="destination">New file location.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error moving file.</exception>
        public static bool MoveFile(string source, string destination)
        {
            try
            {
                if (!FileExists(destination))
                {
                    File.Move(source, destination);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Moves a file to a new location asynchronously.
        /// </summary>
        /// <param name="source">Original file location.</param>
        /// <param name="destination">New file location.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error moving file.</exception>
        public static async Task<bool> MoveFileAsync(string source, string destination)
        {
            try
            {
                if (!FileExists(destination))
                {
                    await Task.Run(() => File.Move(source, destination));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Opens a file using the default application.
        /// </summary>
        /// <param name="fileName">The file to open.</param>
        public static void LoadDefaultApplication(string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo theProcessInfo = new ProcessStartInfo(fileName)
                {
                    UseShellExecute = true
                };

                using (Process? theProcess = Process.Start(theProcessInfo))
                {
                    theProcess?.WaitForExit();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (Process? theProcess = Process.Start("xdg-open", fileName))
                {
                    theProcess?.WaitForExit();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using (Process? theProcess = Process.Start("open", fileName))
                {
                    theProcess?.WaitForExit();
                }
            }
        }

        /// <summary>
        /// Opens a file using the default application asynchronously.
        /// </summary>
        /// <param name="fileName">The file to open.</param>
        public static async Task LoadDefaultApplicationAsync(string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo theProcessInfo = new ProcessStartInfo(fileName)
                {
                    UseShellExecute = true
                };

                using (Process? theProcess = Process.Start(theProcessInfo))
                {
                    await theProcess?.WaitForExitAsync();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                using (Process? theProcess = Process.Start("xdg-open", fileName))
                {
                    await theProcess?.WaitForExitAsync();
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using (Process? theProcess = Process.Start("open", fileName))
                {
                    await theProcess?.WaitForExitAsync();
                }
            }
        }

        /// <summary>
        /// Opens a folder chooser dialog for the user to select the desired folder.
        /// </summary>
        /// <param name="_currentWindow">The current window.</param>
        /// <returns>The selected folder.</returns>
        public static async Task<IStorageFolder?> ChooseLocationAsync(Window _currentWindow)
        {
            var folders = await _currentWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Select Destination Folder",
                AllowMultiple = false
            });

            return folders.Count >= 1 ? folders[0] : null;
        }
    }
}
