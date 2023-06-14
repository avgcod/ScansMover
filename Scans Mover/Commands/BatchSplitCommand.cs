using Avalonia.Controls;
using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using Scans_Mover.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Scans_Mover.Services;
using UglyToad.PdfPig.Writer;
using System.Threading;

namespace Scans_Mover.Commands
{
    public class BatchSplitCommand : CommandBase
    {
        private readonly MoverViewModel _moverViewModel;
        private readonly Window _currentWindow;

        public BatchSplitCommand(Window currentWindow, MoverViewModel moverViewModel)
        {
            _currentWindow = currentWindow;
            _moverViewModel = moverViewModel;
            _moverViewModel.PropertyChanged += OnViewModelPropertChanged;
        }

        public override bool CanExecute(object? parameter)
        {
            return !_moverViewModel.IsBusy
                && base.CanExecute(parameter);
        }
        public async override void Execute(object? parameter)
        {
            bool someSkippedFiles = false;

            _moverViewModel.IsBusy = true;

            List<string> pdfFileNames = await SplitBatchPDFsAsync();
            someSkippedFiles = await RenamePDFsAsync(pdfFileNames);

            _moverViewModel.IsBusy = false;

            await DisplayMessageBoxAsync(someSkippedFiles);

        }

        private async Task DisplayMessageBoxAsync(bool someSkippedFiles)
        {
            MessageBoxView mboxView = new MessageBoxView();

            if (someSkippedFiles)
            {
                mboxView.DataContext = new MessageBoxViewModel(mboxView, "Finished Splitting." + Environment.NewLine + "Review Skipped Files.");
            }
            else
            {
                mboxView.DataContext = new MessageBoxViewModel(mboxView, "Finished Splitting");
            }

            await mboxView.ShowDialog(_currentWindow);
        }

        /// <summary>
        /// Renames a list of PDF files.
        /// </summary>
        /// <param name="pdfsToRename">List of PDFs to rename.</param>
        /// <returns>PDFs that names could not automatically be applied.</returns>
        private async Task<bool> RenamePDFsAsync(List<string> pdfsToRename)
        {
            bool someSkippedFiles = false;
            if (pdfsToRename.Any())
            {
                FileRenameView renameView;
                for (int i = 0; i < pdfsToRename.Count; i++)
                {
                    renameView = new FileRenameView();
                    renameView.DataContext = new FileRenameViewModel(renameView, pdfsToRename[i], _moverViewModel.SelectedScanType, 
                        _moverViewModel.DocumentMinimum, _moverViewModel.PrefixText);
                    await renameView.ShowDialog(_currentWindow);
                    if (((FileRenameViewModel)renameView.DataContext).Response == ScanStatus.OK)
                    {
                        await RenamePDFAsync(pdfsToRename[i], ((FileRenameViewModel)renameView.DataContext).NewFileName);
                    }
                    else if(((FileRenameViewModel)renameView.DataContext).Response == ScanStatus.Skip)
                    {
                        someSkippedFiles = true;
                    }
                    else
                    {
                        i = pdfsToRename.Count;
                    }
                }
            }

            return someSkippedFiles;
        }

        /// <summary>
        /// Checks for IsBusy to be changed on the View Model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewModelPropertChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MoverViewModel.IsBusy))
            {
                OnCanExecutedChanged();
            }
        }

        /// <summary>
        /// Splits multipage PDFs into single page PDFs based on the PDF file name asynchronously.
        /// </summary>
        /// <returns>List of PDFs in byte array form a proper title could not be read from.</returns>
        private async Task<List<string>> SplitBatchPDFsAsync()
        {
            List<string> pdfsToRename = new List<string>();
            List<FileInfo> theFiles = await FileAccessService.GetFilesAsync(_moverViewModel.MainFolder);
            theFiles = await Task.Run(() => theFiles.Where(x => x.Name.ToLower().Contains(_moverViewModel.CurrentPrefix.ToLower() + " batch")).ToList());
            for (int i = 0; i < theFiles.Count; i++)
            {
                pdfsToRename.AddRange(await SplitPDFAsync(theFiles[i].FullName));
            }
            return pdfsToRename;
        }

        /// <summary>
        /// Splits a multipage PDF into singe page PDFs asynchronously.
        /// </summary>
        /// <returns>List of PDFs in byte array form a proper title could not be read from.</returns>
        private async Task<List<string>> SplitPDFAsync(string fileName)
        {
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            string ticks = string.Empty;
            string title = string.Empty;
            string newFileName = string.Empty;
            string pageText = string.Empty;
            byte[] outputDocument = new byte[1];
            List<string> pdfsToRename = new List<string>();
            List<Task> pdfsToSave = new List<Task>();

            using (PdfDocument theDocument = await FileAccessService.LoadPDFDocumentAsync(fileName))
            {
                for (int i = 0; i < theDocument.NumberOfPages; i++)
                {
                    if(_moverViewModel.DocumentHasMinimum)
                    {
                        pageText = await ExtractTextAsync(theDocument.GetPage(i + 1), _moverViewModel.SelectedScanType);
                        title = await GetPageTitleAsync(pageText, _moverViewModel.DocumentMinimum, _moverViewModel.SelectedScanType);
                    }
                    else
                    {
                        title = string.Empty;
                    }

                    outputDocument = await BuildDocumentAsync(theDocument, i, _moverViewModel.PagesPerDocument);

                    i += _moverViewModel.PagesPerDocument - 1;

                    newFileName = Path.Combine(path, _moverViewModel.CurrentPrefix + title + ".pdf");

                    if (title.Length < 2 || await FileAccessService.FileExistsAsync(newFileName))
                    {
                        ticks = DateTime.Now.Ticks.ToString();
                        newFileName = newFileName + ticks + ".pdf";
                        pdfsToSave.Add(FileAccessService.SavePDFDocumentAsync(newFileName, outputDocument));
                        pdfsToRename.Add(newFileName);
                    }
                    else
                    {
                        pdfsToSave.Add(FileAccessService.SavePDFDocumentAsync(newFileName, outputDocument));
                    }

                }
                await Task.WhenAll(pdfsToSave);
            }

            return pdfsToRename;
        }

        /// <summary>
        /// Builds a PDF document based on the provided bacth PDF document and pages per split document limit asynchronously.
        /// </summary>
        /// <param name="theDocument">The batch document.</param>
        /// <param name="i">The current page of the batch document.</param>
        /// <returns>The splitted document in byte array form.</returns>
        private async Task<byte[]> BuildDocumentAsync(PdfDocument theDocument, int i, int pagesPerDoc)
        {
            PdfDocumentBuilder docBuilder = new PdfDocumentBuilder();

            await Task.Run(() =>
            {
                while (docBuilder.Pages.Count < pagesPerDoc)
                {
                    docBuilder.AddPage(theDocument, i + 1);
                    i++;
                }
            });

            return docBuilder.Build();
        }

        /// <summary>
        /// Gets a proper page title with invalid characters removed asynchronously.
        /// Also verifies the title fits the current length and minimum requirements.
        /// </summary>
        /// <param name="pageText">Page text.</param>
        /// <returns>Cleaned page title or empty page title if the title does not meet the requirements.</returns>
        private async Task<string> GetPageTitleAsync(string pageText, double numMin, ScanType currentScanType)
        {
            string title = string.Empty;
            if (currentScanType != ScanType.Shipping)
            {
                string[] splitted = pageText.Split(' ');
                await Task.Run(() =>
                {
                    for (int i = 0; i < splitted.Length; i++)
                    {
                        if (splitted[i].All(x => char.IsLetterOrDigit(x)) && int.TryParse(splitted[i], out int result))
                        {
                            if ((result - numMin < (_moverViewModel.Tolerance + 1)) && (result - numMin > (-1 * (_moverViewModel.Tolerance +1))))
                            {
                                title = splitted[i];
                                break;
                            }
                        }
                    }
                });

            }

            return title;
        }

        /// <summary>
        /// Returns all the the text on a PDF page as a string seperated by a space asynchronously.
        /// </summary>
        /// <param name="thePage">The PDF page to extract text from.</param>
        /// <returns>The text of the page combined into one spring.</returns>
        private async Task<string> ExtractTextAsync(Page thePage, ScanType currentScanType)
        {
            string text = string.Empty;
            if (currentScanType != ScanType.Shipping)
            {
                await Task.Run(async () =>
                {
                List<Word> pageWords = await Task.Run(() => thePage.GetWords().ToList());

                Parallel.For(0, pageWords.Count(),index =>
                        {
                    text += pageWords[index].Text + " ";
                });

                    for (int i = 0; i < pageWords.Count; i++)
                    {
                        text += pageWords[i].Text + " ";
                    }
                });
            }
            return text;
        }

        /// <summary>
        /// Renames a PDF document asynchronously.
        /// </summary>
        /// <param name="fileName">Old name.</param>
        /// <param name="newName">New name.</param>
        /// <returns>if the operation succeded.</returns>
        private async Task<bool> RenamePDFAsync(string fileName, string newName)
        {
            string directoryName = Path.GetDirectoryName(fileName) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                return await FileAccessService.RenameFileAsync(fileName, Path.Combine(directoryName, _moverViewModel.CurrentPrefix + newName + ".pdf"));
            }
            else
            {
                return false;
            }

        }

    }
}
