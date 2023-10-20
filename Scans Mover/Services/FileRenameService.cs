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
    public static class FileRenameService
    {
        private static async Task<bool> RenamePDFAsync(string fileName, string newName, string prefix, IMessenger theMessenger)
        {
            string directoryName = Path.GetDirectoryName(fileName) ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                return await FileAccessService.RenameFileAsync(fileName, Path.Combine(directoryName, prefix + newName + ".pdf"), theMessenger);
            }
            else
            {
                return false;
            }

        }

        public static async Task RenamePDFsAsync(IEnumerable<string> pdfsToRename, MoverViewModel viewModel, IMessenger theMessenger, Window currentWindow)
        {
            if (pdfsToRename.Any())
            {
                FileRenameView renameView;
                foreach (string fileName in pdfsToRename)
                {
                    renameView = new FileRenameView();
                    renameView.DataContext = new FileRenameViewModel(renameView, fileName, viewModel.SelectedScanType,
                        viewModel.DocumentMinimum, viewModel.Prefix, theMessenger);
                    await renameView.ShowDialog(currentWindow);
                    if (viewModel.CurrentScanStatus == ScanStatus.OK)
                    {
                        await RenamePDFAsync(fileName, viewModel.CurrentScanNewFileName, viewModel.Prefix, theMessenger);
                    }
                    else if (viewModel.CurrentScanStatus == ScanStatus.Cancel)
                    {
                        break;
                    }
                }
            }
        }
    }
}
