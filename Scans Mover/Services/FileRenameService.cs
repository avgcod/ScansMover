using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Scans_Mover.Models;
using Scans_Mover.ViewModels;
using Scans_Mover.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Scans_Mover.Services
{
    public class FileRenameService : IRecipient<RenameMessage>
    {
        private readonly IMessenger _theMessenger;
        private ScanStatus _currentScanStatus = ScanStatus.OK;
        private string _currentScanNewFileName = string.Empty;

        public FileRenameService(IMessenger theMessenger)
        {
            _theMessenger = theMessenger;
            theMessenger.RegisterAll(this);
        }

        private async Task<bool> RenamePDFAsync(string fileName, string newName, string prefix)
        {
            string directoryName = Path.GetDirectoryName(fileName) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                return await FileAccessService.RenameFileAsync(fileName, Path.Combine(directoryName, prefix + newName + ".pdf"), _theMessenger);
            }
            else
            {
                return false;
            }
        }

        public async Task RenamePDFsAsync(IEnumerable<string> pdfsToRename, RenameSettings renameSettings, Window currentWindow)
        {
            if (pdfsToRename.Any())
            {
                FileRenameView renameView;
                FileRenameViewModel frvModel;
                foreach (string fileName in pdfsToRename)
                {
                    renameView = new FileRenameView();
                    frvModel = new FileRenameViewModel(renameView, fileName, renameSettings.SelectedScanType,
                        renameSettings.DocumentMinimum, renameSettings.Prefix, _theMessenger);
                    renameView.DataContext = frvModel;
                    renameView.SizeToContent = SizeToContent.WidthAndHeight;
                    await renameView.ShowDialog(currentWindow);
                    frvModel.IsActive = false;
                    if (_currentScanStatus == ScanStatus.OK)
                    {
                        await RenamePDFAsync(fileName, _currentScanNewFileName, renameSettings.Prefix);
                    }
                    else if (_currentScanStatus == ScanStatus.Cancel)
                    {
                        break;
                    }
                }
            }
        }

        public void Receive(RenameMessage message)
        {
            _currentScanStatus = message.ScanStatus;
            if (_currentScanStatus == ScanStatus.OK)
            {
                _currentScanNewFileName = message.NewFileName;
            }
            else if (_currentScanStatus == ScanStatus.Skip)
            {
                _theMessenger.Send(new SkippedFileMessage());
            }
        }
    }
}
