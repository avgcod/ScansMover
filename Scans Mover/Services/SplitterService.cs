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
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Scans_Mover.Services
{
    public static class SplitterService
    {
        /// <summary>
        /// Splits multipage PDFs into single page PDFs based on the PDF file name asynchronously.
        /// </summary>
        /// <returns>List of PDFs in byte array form a proper title could not be read from.</returns>
        public static async Task<IEnumerable<string>> SplitBatchDocumentsAsync(SplitSettings splitSettings, IMessenger theMessenger)
        {
            List<string> pdfsToRename = [];
            IEnumerable<FileInfo> theFiles = await FileAccessService.GetFilesAsync(splitSettings.MainFolder, ".pdf", theMessenger);
            theFiles = theFiles.Where(x => x.Name.Contains(string.Concat(splitSettings.Prefix.ToLower()," batch"), StringComparison.CurrentCultureIgnoreCase));
            foreach (FileInfo theInfo in theFiles)
            {
                pdfsToRename.AddRange(await SplitBatchDocumentAsync(theInfo.FullName, splitSettings, theMessenger));
            }
            return pdfsToRename;
        }

        /// <summary>
        /// Splits a multipage PDF into singe page PDFs asynchronously.
        /// </summary>
        /// <returns>List of PDFs in byte array form a proper title could not be read from.</returns>
        private static async Task<IEnumerable<string>> SplitBatchDocumentAsync(string fileName, SplitSettings splitSettings, IMessenger theMessenger)
        {
            string path = Path.GetDirectoryName(fileName) ?? string.Empty;
            string title = string.Empty;
            List<string> pdfsToRename = [];
            List<Task> pdfsToSave = [];

            using (PdfDocument? theDocument = await FileAccessService.LoadPDFDocumentAsync(fileName, theMessenger))
            {
                if ((theDocument?.NumberOfPages % splitSettings.PagesPerDocument) == 0)
                {
                    for (int i = 0; i < theDocument!.NumberOfPages; i++)
                    {
                        if (splitSettings.DocumentHasMinimum)
                        {
                            title = await GetPageTitleAsync(theDocument.GetPage(i + 1).GetWords(), splitSettings.DocumentMinimum, splitSettings.SelectedScanType, splitSettings.Tolerance);
                        }

                        byte[] outputDocument = await BuildDocumentAsync(theDocument, i, splitSettings.PagesPerDocument);

                        i += splitSettings.PagesPerDocument - 1;

                        string newFileName = Path.Combine(path, splitSettings.Prefix + title + ".pdf");

                        if (title.Length < 2 || await FileAccessService.FileExistsAsync(newFileName, theMessenger))
                        {
                            string ticks = DateTime.Now.Ticks.ToString();
                            newFileName = newFileName + ticks + ".pdf";
                            pdfsToSave.Add(FileAccessService.SavePDFDocumentAsync(newFileName, outputDocument, theMessenger));
                            pdfsToRename.Add(newFileName);
                        }
                        else
                        {
                            pdfsToSave.Add(FileAccessService.SavePDFDocumentAsync(newFileName, outputDocument, theMessenger));
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
            PdfDocumentBuilder docBuilder = new();

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
        private static async Task<string> GetPageTitleAsync(IEnumerable<Word> pageText, double numMin, ScanType currentScanType, double tolerance)
        {
            string title = string.Empty;
            if (currentScanType != ScanType.Shipping)
            {
                await Task.Run(() =>
                {
                    foreach (Word word in pageText)
                    {
                        if (double.TryParse(word.Text, out double result))
                        {
                            if ((result - numMin < (tolerance + 1)) && (result - numMin > (-1 * (tolerance + 1))))
                            {
                                title = word.Text;
                                break;
                            }
                        }
                    }
                });                
            }

            return title;
        }
    }
}
