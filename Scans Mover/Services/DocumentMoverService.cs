using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Scans_Mover.Services
{
    public static class DocumentMoverService
    {
        /// <summary>
        /// Moves scanned documents to the correct folder.
        /// </summary>
        /// <returns>IEnumerable of files that appear to be duplcates or do not have a folder to be put in.</returns>
        public static async Task<IEnumerable<string>> MoveToFolderAsync(MoverViewModel theModel, IMessenger theMessenger)
        {
            List<string> noFoldersFound = [];
            IEnumerable<FileInfo> theFiles = await FileAccessService.GetFilesAsync(theModel.MainFolder, theMessenger);
            List<string> moveLog = [];
            theFiles = await Task.Run(() => theFiles.Where(x => x.Name.StartsWith(theModel.Prefix) && !x.Name.Contains("batch",StringComparison.OrdinalIgnoreCase)));
            string rootDestination = GetRootDestination(theModel);
            string finalDestination = string.Empty;
            string newFileName = string.Empty;
            string baseDirectory = string.Empty;

            if (await FileAccessService.DirectoryExistsAsync(rootDestination, theMessenger))
            {
                foreach (FileInfo theInfo in theFiles)
                {
                    finalDestination = await GetFinalDestinationAsync(theModel, theInfo.Name, theMessenger);
                    if (await FileAccessService.DirectoryExistsAsync(finalDestination, theMessenger))
                    {
                        newFileName = Path.Combine(finalDestination, theInfo.Name);
                        if (!await FileAccessService.FileExistsAsync(newFileName, theMessenger))
                        {
                            baseDirectory = GetBaseDirectory(theModel, newFileName);
                            moveLog.Add(baseDirectory + " - " + theInfo.Name);
                            await FileAccessService.MoveFileAsync(theInfo.FullName, newFileName, theMessenger);
                        }
                        else
                        {
                            noFoldersFound.Add("File " + theInfo.Name + " Already Exists In Folder " + finalDestination);
                        }
                    }
                    else
                    {
                        noFoldersFound.Add("No folder for " + theInfo.Name);
                    }
                }
            }
            else
            {
                noFoldersFound.Add("Folder does not exist for " + theModel.SelectedScanType.ToString() + "s");
            }

            if (moveLog.Count > 0)
            {
                string logFile = theModel.SelectedScanType.ToString() + " Move Log - " + GetTimeStamp(DateTime.Now) + ".txt";
                theMessenger.Send<MoveLogMessage>(new MoveLogMessage(moveLog, logFile));
            }

            return noFoldersFound;
        }

        #region GetMethods
        /// <summary>
        /// Gets the base directory for a given path.
        /// </summary>
        /// <param name="thePath">The path..</param>
        /// <returns>The base directory.</returns>
        private static string GetBaseDirectory(MoverViewModel theModel, string thePath)
        {
            return thePath.Split(GetRootDestination(theModel))[1].Substring(1).Split(Path.DirectorySeparatorChar)[0];
        }

        /// <summary>
        /// Gets a timestamp from a DateTime.
        /// </summary>
        /// <param name="currentDT">the date time to get the timestamp from.</param>
        /// <returns>The timestamp as a string.</returns>
        private static string GetTimeStamp(DateTime currentDT)
        {
            return currentDT.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        /// Gets the root destination folder based on scan type.
        /// </summary>
        /// <returns>The root destination folder.</returns>
        private static string GetRootDestination(MoverViewModel theModel)
        {
            if (theModel.SelectedScanType == ScanType.Delivery || theModel.SelectedScanType == ScanType.PO)
            {
                return theModel.Settings.DeliveriesFolder;
            }
            else if (theModel.SelectedScanType == ScanType.RMA)
            {
                return theModel.Settings.RMAsFolder;
            }
            else if (theModel.SelectedScanType == ScanType.Shipping)
            {
                return theModel.Settings.ShippingLogsFolder;
            }
            else
            {
                return theModel.Settings.ServiceFolder;
            }
        }

        /// <summary>
        /// Gets the final destination of a file based on scan type.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>Destination folder.</returns>
        private async static Task<string> GetFinalDestinationAsync(MoverViewModel theModel, string fileName, IMessenger theMessenger)
        {
            if (theModel.SelectedScanType == ScanType.Delivery)
            {
                return GetDeliveryDestination(theModel, fileName, theModel.SpecifiedDate);
            }
            else if (theModel.SelectedScanType == ScanType.RMA)
            {
                return await GetRMADestinationAsync(theModel, fileName, theMessenger);
            }
            else if (theModel.SelectedScanType == ScanType.Shipping)
            {
                return GetShippingLogDestination(theModel, fileName);
            }
            else if (theModel.SelectedScanType == ScanType.PO)
            {
                return await GetPODestinationAsync(theModel, fileName, theMessenger);
            }
            else
            {
                return await GetServiceDestinationAsync(theModel, fileName, theMessenger);
            }
        }

        /// <summary>
        /// Gets the destination of a delivery document.
        /// </summary>
        /// <param name="fileName">File name of the delivery document.</param>
        /// <returns>Destination of the delivery document.</returns>
        private static string GetDeliveryDestination(MoverViewModel theModel, string fileName, DateTime specifiedDate)
        {
            return Path.Combine(theModel.DeliveriesFolder, specifiedDate.Year.ToString() +
                GetFormattedMonthNumber(specifiedDate.Month) + " " +
                GetMonthName(specifiedDate.Month), GetMonthName(specifiedDate.Month) + " " + GetFormattedDay(specifiedDate.Day),
                fileName.Replace(theModel.Prefix, "").Replace(".pdf", ""));
        }

        /// <summary>
        /// Gets the destination of a PO document.
        /// </summary>
        /// <param name="fileName">File name of the PO document.</param>
        /// <returns>Destination of the PO document.</returns>
        private async static Task<string> GetPODestinationAsync(MoverViewModel theModel, string fileName, IMessenger theMessenger)
        {
            string deliveryNum = fileName.Replace(theModel.Prefix, "").Replace(".pdf", "");
            string poDestination = await CheckDeliveryDestinationsAsync(theModel, deliveryNum, DateTime.Now, theMessenger);

            if (string.IsNullOrEmpty(poDestination))
            {
                poDestination = await CheckDeliveryDestinationsAsync(theModel, deliveryNum, DateTime.Now.AddMonths(-1), theMessenger);

                if (string.IsNullOrEmpty(poDestination))
                {
                    poDestination = await CheckDeliveryDestinationsAsync(theModel, deliveryNum, DateTime.Now.AddMonths(-2), theMessenger);
                }
            }

            return poDestination;
        }

        /// <summary>
        /// Gets the destination of a service document.
        /// </summary>
        /// <param name="fileName">File name of the PO document.</param>
        /// <returns>Destination of the service document.</returns>
        private async static Task<string> GetServiceDestinationAsync(MoverViewModel theModel, string fileName, IMessenger theMessenger)
        {
            string deliveryNum = fileName.Split("_")[3];
            DateTime theDT = DateTime.Now;
            string serviceDestination = await CheckDeliveryDestinationsAsync(theModel, deliveryNum, theDT, theMessenger);

            if (string.IsNullOrEmpty(serviceDestination))
            {
                theDT = theDT.AddMonths(-1);
                serviceDestination = await CheckDeliveryDestinationsAsync(theModel, deliveryNum, theDT, theMessenger);

                if (string.IsNullOrEmpty(serviceDestination))
                {
                    theDT = theDT.AddMonths(-1);
                    serviceDestination = await CheckDeliveryDestinationsAsync(theModel, deliveryNum, theDT, theMessenger);
                }
            }

            return serviceDestination;
        }

        /// <summary>
        /// Gets the destination of an RMA document.
        /// </summary>
        /// <param name="fileName">File name of the RMA document.</param>
        /// <returns>Destination of the RMA document.</returns>
        private static async Task<string> GetRMADestinationAsync(MoverViewModel theModel, string fileName, IMessenger theMessenger)
        {
            string rmaDestination = string.Empty;
            if (double.TryParse(fileName.Replace(theModel.Prefix, "").Replace(".pdf", ""), out double rmaNum))
            {
                double rmaMin = 0;
                double rmaMax = 0;
                IEnumerable<string> theRMAFolders = await GetRMAFoldersAsync(theModel, theMessenger);
                await Task.Run(() =>
                {
                    foreach (string theFolder in theRMAFolders)
                    {
                        rmaMin = double.Parse(theFolder.Split(' ')[1].Split('-')[0]);
                        rmaMax = rmaMin + 99;
                        if (rmaNum >= rmaMin && rmaNum <= rmaMax)
                        {
                            rmaDestination = Path.Combine(theModel.RMAsFolder, theFolder);
                            break;
                        }
                    }
                });
            }
            return rmaDestination;
        }

        /// <summary>
        /// Gets the 2 digit day of the month.
        /// </summary>
        /// <param name="theDay">The day of the month.</param>
        /// <returns>The 2 digit day of the month.</returns>
        private static string GetFormattedDay(int theDay)
        {
            if (theDay <= 9)
            {
                return "0" + theDay.ToString();
            }
            return theDay.ToString();
        }

        /// <summary>
        /// Gets the 2 digit month number.
        /// </summary>
        /// <param name="theMonth">The month number.</param>
        /// <returns>2 digit month number.</returns>
        private static string GetFormattedMonthNumber(int theMonth)
        {
            if (theMonth <= 9)
            {
                return "0" + theMonth.ToString();
            }
            return theMonth.ToString();
        }

        /// <summary>
        /// Gets the name of a month based on the month number.
        /// </summary>
        /// <param name="theMonth">The month number</param>
        /// <returns>The month name.</returns>
        private static string GetMonthName(int theMonth)
        {
            return new DateTimeFormatInfo().GetMonthName(theMonth);
        }

        /// <summary>
        /// Gets the available RMA folders.
        /// </summary>
        /// <returns>IEnumerable of RMA folder names.</returns>
        private static async Task<IEnumerable<string>> GetRMAFoldersAsync(MoverViewModel theModel, IMessenger theMessenger)
        {
            return await FileAccessService.GetSubDirectoriesAsync(theModel.RMAsFolder, theMessenger);
        }

        /// <summary>
        /// Gets the destination of a shipping log document.
        /// </summary>
        /// <param name="fileName">File name of the shipping log document.</param>
        /// <returns>Destination of the shipping log document.</returns>
        private static string GetShippingLogDestination(MoverViewModel theModel, string fileName)
        {
            string[] shippingParts = fileName.Split('-');
            int monthNum = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Select(x => x.ToLower()).ToList().IndexOf(shippingParts[1].ToLower()) + 1;
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNum);
            return Path.Combine(theModel.ShippingLogsFolder, monthNum + " " + monthName + " " + DateTime.Now.Year.ToString());
        }
        #endregion

        /// <summary>
        /// Checks if there is a folder for a delivery number in a given month.
        /// </summary>
        /// <param name="deliveryNum">The delivery number to check.</param>
        /// <param name="month">The month to look in.</param>
        /// <returns>The folder location if found or an empty string if not found.</returns>
        private async static Task<string> CheckDeliveryDestinationsAsync(MoverViewModel theModel, string deliveryNum, DateTime month, IMessenger theMessenger)
        {
            int days = DateTime.DaysInMonth(month.Year, month.Month) + 1;
            string deliveryDestination = string.Empty;
            DateTime currentDate = new();

            for (int i = 1; i < days; i++)
            {
                currentDate = new DateTime(month.Year, month.Month, i);
                deliveryDestination = GetDeliveryDestination(theModel, deliveryNum, currentDate);

                if (await FileAccessService.DirectoryExistsAsync(deliveryDestination, theMessenger))
                {
                    return deliveryDestination;
                }
            }

            return string.Empty;
        }
    }
}
