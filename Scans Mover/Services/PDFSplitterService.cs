using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Writer;
using UglyToad.PdfPig;
using CommunityToolkit.Mvvm.Messaging;

namespace Scans_Mover.Services
{
    public static class PDFSplitterService
    {
        /// <summary>
        /// Splits multipage PDFs into single page PDFs based on the PDF file name asynchronously.
        /// </summary>
        /// <returns>List of PDFs in byte array form a proper title could not be read from.</returns>
        public static async Task<IEnumerable<string>> SplitBatchPDFsAsync(MoverViewModel viewModel, IMessenger theMessenger)
        {
            List<string> pdfsToRename = new List<string>();
            IEnumerable<FileInfo> theFiles = await FileAccessService.GetFilesAsync(viewModel.MainFolder);
            theFiles = await Task.Run(() => theFiles.Where(x => x.Name.ToLower().Contains(viewModel.Prefix.ToLower() + " batch")).ToList());
            foreach (FileInfo theInfo in theFiles)
            {
                pdfsToRename.AddRange(await SplitPDFAsync(theInfo.FullName, viewModel, theMessenger));
            }
            return pdfsToRename;
        }

        /// <summary>
        /// Splits a multipage PDF into singe page PDFs asynchronously.
        /// </summary>
        /// <returns>List of PDFs in byte array form a proper title could not be read from.</returns>
        private static async Task<IEnumerable<string>> SplitPDFAsync(string fileName, MoverViewModel viewModel, IMessenger theMessenger)
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
                if ((theDocument.NumberOfPages % viewModel.PagesPerDocument) == 0)
                {
                    for (int i = 0; i < theDocument.NumberOfPages; i++)
                    {
                        if (viewModel.DocumentHasMinimum)
                        {
                            pageText = await ExtractTextAsync(theDocument.GetPage(i + 1), viewModel.SelectedScanType);
                            title = await GetPageTitleAsync(pageText, viewModel.DocumentMinimum, viewModel.SelectedScanType, viewModel.Tolerance);
                        }
                        else
                        {
                            title = string.Empty;
                        }

                        outputDocument = await BuildDocumentAsync(theDocument, i, viewModel.PagesPerDocument);

                        i += viewModel.PagesPerDocument - 1;

                        newFileName = Path.Combine(path, viewModel.Prefix + title + ".pdf");

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
                else
                {
                    theMessenger.Send<PagesPerDocumentErrorMessage>(new PagesPerDocumentErrorMessage($"File {fileName} is not divisible by the specified pages per document."));
                }

            }
            return pdfsToRename;
        }

        /// <summary>
        /// Builds a PDF document based on the provided bacth PDF document and pages per split document limit asynchronously.
        /// </summary>
        /// <param name="theDocument">The batch document.</param>
        /// <param name="i">The current page of the batch document.</param>
        /// <returns>The splitted document in byte array form.</returns>
        private static async Task<byte[]> BuildDocumentAsync(PdfDocument theDocument, int i, int pagesPerDoc)
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
        private static async Task<string> GetPageTitleAsync(string pageText, double numMin, ScanType currentScanType, double tolerance)
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
                            if ((result - numMin < (tolerance + 1)) && (result - numMin > (-1 * (tolerance + 1))))
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
        private static async Task<string> ExtractTextAsync(Page thePage, ScanType currentScanType)
        {
            string text = string.Empty;
            if (currentScanType != ScanType.Shipping)
            {
                await Task.Run(async () =>
                {
                    List<Word> pageWords = await Task.Run(() => thePage.GetWords().ToList());

                    Parallel.For(0, pageWords.Count, index =>
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

    }
}
