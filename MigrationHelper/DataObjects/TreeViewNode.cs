using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Evaluation;

namespace MigrationHelper.DataObjects
{
    public class TreeViewNode
    {
        /// <summary>
        /// Contains the file size
        /// </summary>
        private readonly long _fileSize;

        /// <summary>
        /// Gets or sets the name of the entry
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full path 
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the file extension
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the info
        /// </summary>
        public string Info => IsDirectory ? $"({SubNodes.Count})" : _fileSize.ToFormattedFileSize();

        /// <summary>
        /// Gets or sets the project item (only available for file entries)
        /// </summary>
        public ProjectItem ProjectItem { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the item is a directory
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets or sets the sub nodes of the entry
        /// </summary>
        public List<TreeViewNode> SubNodes { get; set; } = new List<TreeViewNode>();

        /// <summary>
        /// Gets or sets the value which indicates if the item is selected or not
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the value which indicates if the item is expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets the icon file kind
        /// </summary>
        public string Kind => IsDirectory ? "FileDirectory" : "File";

        /// <summary>
        /// Creates a new empty instance of the <see cref="TreeViewNode"/>
        /// </summary>
        private TreeViewNode() { }

        /// <summary>
        /// Creates a new instance of the <see cref="TreeViewNode"/>
        /// </summary>
        /// <param name="file">The file info object</param>
        /// <param name="projectItem">The project item</param>
        public TreeViewNode(FileInfo file, ProjectItem projectItem)
        {
            Name = file.Name;
            FullPath = file.FullName;
            FileExtension = file.Extension;
            CreationDate = file.CreationTime;
            _fileSize = file.Length;
            ProjectItem = projectItem;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="TreeViewNode"/>
        /// </summary>
        /// <param name="directory">The dir info object"/></param>
        public TreeViewNode(DirectoryInfo directory)
        {
            Name = directory.Name;
            FullPath = directory.FullName;
            CreationDate = directory.CreationTime;
            IsDirectory = true;
        }

        /// <summary>
        /// Creates a copy of the given node
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>The new tree node</returns>
        public static TreeViewNode CopyNode(TreeViewNode node)
        {
            return new TreeViewNode
            {
                Name = node.Name,
                FullPath = node.FullPath,
                CreationDate = node.CreationDate,
                ProjectItem = node.ProjectItem,
                IsDirectory = node.IsDirectory
            };
        }
    }
}
