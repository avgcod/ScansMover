using Avalonia.Controls;
using Scans_Mover.ViewModels;
using Scans_Mover.Views;
using System.ComponentModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Scans_Mover.Services;
using Scans_Mover.Models;
using System.Globalization;
using System.Xml.Linq;

namespace Scans_Mover.Commands
{
    public class MoveDocumentsCommand : CommandBase
    {
        private readonly MoverViewModel _moverViewModel;
        private readonly Window _currentWindow;

        public MoveDocumentsCommand(MoverViewModel moverViewModel, Window currentWindow)
        {
            _moverViewModel = moverViewModel;
            _currentWindow = currentWindow;
            _moverViewModel.PropertyChanged += OnViewModelPropertChanged;
        }

        public override bool CanExecute(object? parameter)
        {
            return !_moverViewModel.IsBusy
                && base.CanExecute(parameter);
        }

        public async override void Execute(object? parameter)
        {
            _moverViewModel.IsBusy = true;

            MessageBoxView mboxView = new MessageBoxView();
            List<string> filesNeedingFolders = await MoveToFolderAsync();

            if (filesNeedingFolders.Any())
            {
                mboxView.DataContext = new MessageBoxViewModel(mboxView, "Finished Moving." + Environment.NewLine + "Some Files Need Folders.");
            }
            else
            {
                mboxView.DataContext = new MessageBoxViewModel(mboxView, "Finished Moving");
            }

            await mboxView.ShowDialog(_currentWindow);

            _moverViewModel.IsBusy = false;
            if (_moverViewModel.MoveLog != string.Empty)
            {
                await FileAccessService.LoadDefaultApplicationAsync(_moverViewModel.MoveLog);
                _moverViewModel.MoveLog = string.Empty;
            }
        }

        /// <summary>
        /// Moves scanned documents to the correct folder.
        /// </summary>
        /// <returns>IEnumerable of files that appear to be duplcates or do not have a folder to be put in.</returns>
        private async Task<List<string>> MoveToFolderAsync()
        {
            List<string> noFoldersFound = new List<string>();
            List<FileInfo> theFiles = await FileAccessService.GetFilesAsync(_moverViewModel.MainFolder);
            List<string> moveLog = new List<string>();
            theFiles = await Task.Run(() => theFiles.Where(x => x.Name.StartsWith(_moverViewModel.CurrentPrefix) && !x.Name.ToLower().Contains("batch")).ToList());
            string rootDestination = GetRootDestination();
            string finalDestination = string.Empty;
            string newFileName = string.Empty;
            string baseDirectory = string.Empty;

            if (await FileAccessService.DirectoryExistsAsync(rootDestination))
            {
                for (int i = 0; i < theFiles.Count; i++)
                {
                    finalDestination = GetFinalDestination(theFiles[i].Name);
                    if (FileAccessService.DirectoryExists(finalDestination))
                    {
                        newFileName = Path.Combine(finalDestination, theFiles[i].Name);
                        if (!await FileAccessService.FileExistsAsync(newFileName))
                        {
                            baseDirectory = GetBaseDirectory(newFileName);
                            moveLog.Add(baseDirectory + " - " + theFiles[i].Name);
                            await FileAccessService.MoveFileAsync(theFiles[i].FullName, newFileName);
                        }
                        else
                        {
                            noFoldersFound.Add("File " + theFiles[i].Name + " Already Exists In Folder " + finalDestination);
                        }

                    }
                    else
                    {
                        noFoldersFound.Add("No folder for " + theFiles[i].Name);
                    }
                }
            }
            else
            {
                noFoldersFound.Add("Folder does not exist for " + _moverViewModel.SelectedScanType.ToString() + "s");
            }

            if (moveLog.Any())
            {
                _moverViewModel.MoveLog = _moverViewModel.SelectedScanType.ToString() + " Move Log - " + GetTimeStamp(DateTime.Now) + ".txt";
                await FileAccessService.SaveLogAsync(moveLog, _moverViewModel.MoveLog);
            }

            return noFoldersFound;

        }

        #region GetMethods
        /// <summary>
        /// Gets the base directory for a given path.
        /// </summary>
        /// <param name="thePath">The path..</param>
        /// <returns>The base directory.</returns>
        private string GetBaseDirectory(string thePath)
        {
            return thePath.Split(GetRootDestination())[1].Substring(1).Split(Path.DirectorySeparatorChar)[0];
        }

        /// <summary>
        /// Gets a timestamp from a DateTime.
        /// </summary>
        /// <param name="currentDT">the date time to get the timestamp from.</param>
        /// <returns>The timestamp as a string.</returns>
        private string GetTimeStamp(DateTime currentDT)
        {
            return currentDT.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        /// Gets the root destination folder based on scan type.
        /// </summary>
        /// <returns>The root destination folder.</returns>
        private string GetRootDestination()
        {
            if (_moverViewModel.SelectedScanType == ScanType.Delivery | _moverViewModel.SelectedScanType == ScanType.PO)
            {
                return _moverViewModel.Settings.DeliveriesFolder;
            }
            else if (_moverViewModel.SelectedScanType == ScanType.RMA)
            {
                return _moverViewModel.Settings.RMAsFolder;
            }
            else if (_moverViewModel.SelectedScanType == ScanType.Shipping)
            {
                return _moverViewModel.Settings.ShippingLogsFolder;
            }
            else
            {
                return _moverViewModel.Settings.ServiceFolder;
            }
        }

        /// <summary>
        /// Gets the final destination of a file based on scan type.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>Destination folder.</returns>
        private string GetFinalDestination(string fileName)
        {
            if (_moverViewModel.SelectedScanType == ScanType.Delivery)
            {
                return GetDeliveryDestination(fileName, _moverViewModel.SpecifiedDate);
            }
            else if (_moverViewModel.SelectedScanType == ScanType.RMA)
            {
                return GetRMADestination(fileName);
            }
            else if (_moverViewModel.SelectedScanType == ScanType.Shipping)
            {
                return GetShippingLogDestination(fileName);
            }
            else if (_moverViewModel.SelectedScanType == ScanType.PO)
            {
                return GetPODestination(fileName);
            }
            else
            {
                return GetServiceDestination(fileName);
            }
        }

        /// <summary>
        /// Gets the destination of a delivery document.
        /// </summary>
        /// <param name="fileName">File name of the delivery document.</param>
        /// <returns>Destination of the delivery document.</returns>
        private string GetDeliveryDestination(string fileName, DateTime specifiedDate)
        {
            return Path.Combine(_moverViewModel.Settings.DeliveriesFolder, specifiedDate.Year.ToString() +
                GetFormattedMonthNumber(specifiedDate.Month) + " " +
                GetMonthName(specifiedDate.Month), GetMonthName(specifiedDate.Month) + " " + GetFormattedDay(specifiedDate.Day),
                fileName.Replace(_moverViewModel.CurrentPrefix, "").Replace(".pdf", ""));

        }

        /// <summary>
        /// Gets the destination of an RMA document.
        /// </summary>
        /// <param name="fileName">File name of the RMA document.</param>
        /// <returns>Destination of the RMA document.</returns>
        private string GetRMADestination(string fileName)
        {
            if (double.TryParse(fileName.Replace(_moverViewModel.CurrentPrefix, "").Replace(".pdf", ""), out double rmaNum))
            {
                double rmaMin;
                double rmaMax;
                List<string> theRMAFolders = GetRMAFolders().ToList();
                for (int i = 0; i < theRMAFolders.Count; i++)
                {
                    rmaMin = double.Parse(theRMAFolders[i].Split(' ')[1].Split('-')[0]);
                    rmaMax = rmaMin + 99;
                    if (rmaNum >= rmaMin && rmaNum <= rmaMax)
                    {
                        return Path.Combine(_moverViewModel.Settings.RMAsFolder, theRMAFolders[i]);
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the destination of a PO document.
        /// </summary>
        /// <param name="fileName">File name of the PO document.</param>
        /// <returns>Destination of the PO document.</returns>
        private string GetPODestination(string fileName)
        {
            string deliveryNum = fileName.Replace(_moverViewModel.CurrentPrefix, "").Replace(".pdf", "");
            string poDestination = CheckDeliveryDestinations(deliveryNum, DateTime.Now);

            if (poDestination == string.Empty)
            {
                poDestination = CheckDeliveryDestinations(deliveryNum, DateTime.Now.AddMonths(-1));

                if (poDestination == string.Empty)
                {
                    poDestination = CheckDeliveryDestinations(deliveryNum, DateTime.Now.AddMonths(-2));
                }
            }

            return poDestination;
        }

        /// <summary>
        /// Gets the destination of a service document.
        /// </summary>
        /// <param name="fileName">File name of the PO document.</param>
        /// <returns>Destination of the service document.</returns>
        private string GetServiceDestination(string fileName)
        {
            string deliveryNum = fileName.Split("_")[3];
            DateTime theDT = DateTime.Now;
            string serviceDestination = CheckDeliveryDestinations(deliveryNum, theDT);

            if (serviceDestination == string.Empty)
            {
                theDT = theDT.AddMonths(-1);
                serviceDestination = CheckDeliveryDestinations(deliveryNum, theDT);

                if (serviceDestination == string.Empty)
                {
                    theDT = theDT.AddMonths(-1);
                    serviceDestination = CheckDeliveryDestinations(deliveryNum, theDT);
                }
            }

            return serviceDestination;
        }

        /// <summary>
        /// Gets the abbreviated month name from the number of a month.
        /// </summary>
        /// <param name="monthNum">Month number.</param>
        /// <returns>Abbreviated month name.</returns>
        private string GetAbbreviatedMonth(string monthNum)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(int.Parse(monthNum));
        }

        /// <summary>
        /// Gets the destination of an RMA document.
        /// </summary>
        /// <param name="fileName">File name of the RMA document.</param>
        /// <returns>Destination of the RMA document.</returns>
        private async Task<string> GetRMADestinationAsync(string fileName)
        {
            string rmaDestination = string.Empty;
            if (double.TryParse(fileName.Replace(_moverViewModel.CurrentPrefix, "").Replace(".pdf", ""), out double rmaNum))
            {
                double rmaMin = 0;
                double rmaMax = 0;
                List<string> theRMAFolders = await GetRMAFoldersAsync();
                await Task.Run(() =>
                {
                    for (int i = 0; i < theRMAFolders.Count; i++)
                    {
                        rmaMin = double.Parse(theRMAFolders[i].Split(' ')[1].Split('-')[0]);
                        rmaMax = rmaMin + 99;
                        if (rmaNum >= rmaMin && rmaNum <= rmaMax)
                        {
                            rmaDestination = Path.Combine(_moverViewModel.Settings.RMAsFolder, theRMAFolders[i]);
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
        private IEnumerable<string> GetRMAFolders()
        {
            return FileAccessService.GetSubDirectories(_moverViewModel.Settings.RMAsFolder);
        }

        /// <summary>
        /// Gets the available RMA folders.
        /// </summary>
        /// <returns>IEnumerable of RMA folder names.</returns>
        private Task<List<string>> GetRMAFoldersAsync()
        {
            return FileAccessService.GetSubDirectoriesAsync(_moverViewModel.Settings.RMAsFolder);
        }

        /// <summary>
        /// Gets the destination of a shipping log document.
        /// </summary>
        /// <param name="fileName">File name of the shipping log document.</param>
        /// <returns>Destination of the shipping log document.</returns>
        private string GetShippingLogDestination(string fileName)
        {
            string[] shippingParts = fileName.Split('-');
            int monthNum = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Select(x => x.ToLower()).ToList().IndexOf(shippingParts[1].ToLower()) + 1;
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthNum);
            return Path.Combine(_moverViewModel.Settings.ShippingLogsFolder, monthNum + " " + monthName + " " + DateTime.Now.Year.ToString());
        }
        #endregion

        /// <summary>
        /// Checks if there is a folder for a delivery number in a given month.
        /// </summary>
        /// <param name="deliveryNum">The delivery number to check.</param>
        /// <param name="month">The month to look in.</param>
        /// <returns>The folder location if found or an empty string if not found.</returns>
        private string CheckDeliveryDestinations(string deliveryNum, DateTime month)
        {
            int days = DateTime.DaysInMonth(month.Year, month.Month) + 1;
            string deliveryDestination = string.Empty;
            DateTime currentDate = new DateTime();

            for (int i = 1; i < days; i++)
            {
                //deliveryDestination = Path.Combine(_moverViewModel.Settings.DeliveriesFolder, month.Year.ToString() +
                //GetFormattedMonthNumber(month.Month) + " " +
                //GetMonthName(month.Month), GetMonthName(month.Month) + " " + GetFormattedDay(i),
                //deliveryNum);

                currentDate = new DateTime(month.Year, month.Month, i);
                deliveryDestination = GetDeliveryDestination(deliveryNum, currentDate);

                if (FileAccessService.DirectoryExists(deliveryDestination))
                {
                    return deliveryDestination;
                }
            }

            return string.Empty;
        }

        private void OnViewModelPropertChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MoverViewModel.IsBusy))
            {
                OnCanExecutedChanged();
            }
        }
    }
}
