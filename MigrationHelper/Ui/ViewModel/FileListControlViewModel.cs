using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using MigrationHelper.DataObjects;
using MigrationHelper.Ui.View;
using ZimLabs.Utility.Extensions;
using ZimLabs.WpfBase;

namespace MigrationHelper.Ui.ViewModel
{
    public class FileListControlViewModel : ObservableObject
    {
        /// <summary>
        /// Contains the mah apps dialog coordinator
        /// </summary>
        private IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Contains the action to set the selected file
        /// </summary>
        private Action<TreeViewNode> _setSelectedFile;

        /// <summary>
        /// Contains the list with the not included files
        /// </summary>
        private List<FileInfo> _notIncludedFiles;

        /// <summary>
        /// Contains the root node
        /// </summary>
        private TreeViewNode _rootNode;

        /// <summary>
        /// Backing field for <see cref="NodeList"/>
        /// </summary>
        private ObservableCollection<TreeViewNode> _nodeList;

        /// <summary>
        /// Gets or sets the list with the nodes
        /// </summary>
        public ObservableCollection<TreeViewNode> NodeList
        {
            get => _nodeList;
            set => SetField(ref _nodeList, value);
        }

        /// <summary>
        /// Backing field for <see cref="Filter"/>
        /// </summary>
        private string _filter;

        /// <summary>
        /// Gets or sets the filter
        /// </summary>
        public string Filter
        {
            get => _filter;
            set => SetField(ref _filter, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShowNotIncludedFilesInfo"/>
        /// </summary>
        private Visibility _showNotIncludedFilesInfo = Visibility.Hidden;

        /// <summary>
        /// Gets or sets the visibility of the not included files which should be shown when not included files available
        /// </summary>
        public Visibility ShowNotIncludedFilesInfo
        {
            get => _showNotIncludedFilesInfo;
            set => SetField(ref _showNotIncludedFilesInfo, value);
        }

        /// <summary>
        /// Init the view model
        /// </summary>
        /// <param name="dialogCoordinator">The mah apps dialog coordinator</param>
        /// <param name="setSelectedFile">The action to set the selected file</param>
        public void InitViewModel(IDialogCoordinator dialogCoordinator, Action<TreeViewNode> setSelectedFile)
        {
            _dialogCoordinator = dialogCoordinator;

            _setSelectedFile = setSelectedFile;
        }

        /// <summary>
        /// Loads the files
        /// </summary>
        public async void LoadFiles()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.ProjectFile))
                return;

            try
            {
                var (rootNode, notIncludedFiles) = Helper.LoadScriptFiles();
                _rootNode = rootNode;
                _notIncludedFiles = notIncludedFiles;

                if (_rootNode == null)
                    return;

                if (_notIncludedFiles != null && _notIncludedFiles.Any())
                    ShowNotIncludedFilesInfo = Visibility.Visible;
                else
                    ShowNotIncludedFilesInfo = Visibility.Hidden;

                FilterList();
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(LoadFiles), ex);
                //await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occured: {ex.Message}");
            }
        }

        /// <summary>
        /// The command to filter the file list
        /// </summary>
        public ICommand FilterCommand => new DelegateCommand(FilterList);

        /// <summary>
        /// The command to open the selected file
        /// </summary>
        public ICommand OpenCommand => new DelegateCommand(OpenFile);

        /// <summary>
        /// The command to delete the selected entry
        /// </summary>
        public ICommand DeleteCommand => new DelegateCommand(DeleteEntry);

        /// <summary>
        /// The command to show the not included files
        /// </summary>
        public ICommand ShowNotIncludedFilesCommand => new DelegateCommand(() =>
        {
            var dialog = new NotIncludedFilesWindow(_notIncludedFiles);
            dialog.ShowDialog();

            LoadFiles();
        });

        /// <summary>
        /// Filters the list
        /// </summary>
        private void FilterList()
        {
            if (string.IsNullOrEmpty(Filter))
            {
                NodeList = new ObservableCollection<TreeViewNode> {_rootNode};
            }
            else
            {
                var rootNode = TreeViewNode.CopyNode(_rootNode);

                foreach (var subNode in _rootNode.SubNodes)
                {
                    var tmpNode = TreeViewNode.CopyNode(subNode);
                    if (subNode.IsDirectory)
                    {
                        tmpNode.SubNodes = subNode.SubNodes.Where(w => w.Name.ContainsIgnoreCase(Filter)).ToList();
                        if (tmpNode.SubNodes.Any())
                            rootNode.SubNodes.Add(tmpNode);
                    }
                    else
                    {
                        if (tmpNode.Name.ContainsIgnoreCase(Filter))
                            rootNode.SubNodes.Add(tmpNode);
                    }
                }

                NodeList = new ObservableCollection<TreeViewNode> {rootNode};
            }

        }

        /// <summary>
        /// Opens the selected file
        /// </summary>
        private void OpenFile()
        {
            var selectedNode = GetSelectedNode();
            if (selectedNode == null)
                return;

            _setSelectedFile(selectedNode);
        }

        /// <summary>
        /// Deletes the selected entry
        /// </summary>
        private async void DeleteEntry()
        {
            var selectedNode = GetSelectedNode();
            if (selectedNode == null)
                return;

            if (await _dialogCoordinator.ShowMessageAsync(this, "Delete",
                    $"Do you really want to delete the migration file \"{selectedNode.Name}\"") ==
                MessageDialogResult.Negative)
                return;

            try
            {
                if (!Helper.DeleteProjectItem(selectedNode))
                {
                    await _dialogCoordinator.ShowMessageAsync(this, "Error",
                        "An error has occurred while deleting the file.");
                    return;
                }

                RemoveSelection();

                LoadFiles();

                _setSelectedFile(null);
            }
            catch (Exception ex)
            {
                Logger.Error(nameof(DeleteEntry), ex);
                await _dialogCoordinator.ShowMessageAsync(this, "Error",
                    $"An error has occurred.\r\n\r\nMessage: {ex.Message}");
            }
            
        }

        /// <summary>
        /// Sets the selected file
        /// </summary>
        /// <param name="filename">The name of the file</param>
        public void SetSelectedFile(string filename)
        {
            var rootNode = NodeList.FirstOrDefault();

            if (rootNode == null)
                return;

            // Deselect all nodes
            RemoveSelection();

            foreach (var node in rootNode.SubNodes)
            {
                if (!node.IsDirectory && node.Name.Equals(filename))
                {
                    node.IsSelected = true;
                    return;
                }

                var selectedSubNode = node.SubNodes.FirstOrDefault(f => f.Name.Equals(filename));
                if (selectedSubNode != null)
                {
                    // Expand the main node
                    node.IsExpanded = true;
                    selectedSubNode.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Gets the selected tree view node
        /// </summary>
        /// <returns>The selected item</returns>
        private TreeViewNode GetSelectedNode()
        {
            // The first node is the root node
            var rootNode = NodeList?.FirstOrDefault();

            if (rootNode == null)
                return null;

            // Check direct under 
            var subNode = rootNode.SubNodes.FirstOrDefault(f => f.IsSelected);
            if (subNode != null)
                return subNode;

            foreach (var node in rootNode.SubNodes)
            {
                if (node.IsDirectory && node.IsSelected)
                    return node;

                var selectedSubNode = node.SubNodes.FirstOrDefault(f => f.IsSelected);
                if (selectedSubNode != null)
                    return selectedSubNode;
            }

            return null;
        }

        /// <summary>
        /// Removes the selection
        /// </summary>
        private void RemoveSelection()
        {
            var rootNode = NodeList.FirstOrDefault();

            if (rootNode == null)
                return;

            // Deselect all nodes
            foreach (var node in rootNode.SubNodes)
            {
                node.IsSelected = false;
                node.SubNodes.ForEach(f => f.IsSelected = false);
            }
        }
    }
}
