
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Text;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace Photosort
{
    public partial class MainWindow : Window
    {
        private string _currentInputFolderPath;
        private string _currentOutputFolderPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private string CurrentInputFolderPath
        {
            get
            {
                return _currentInputFolderPath;
            }
            set
            {
                _currentInputFolderPath = value;
                UpdateInputTextBoxes();
            }
        }

        private string CurrentOutputFolderPath
        {
            get
            {
                return _currentOutputFolderPath;
            }
            set
            {
                _currentOutputFolderPath = value;
                UpdateOutputTextBoxes();
            }
        }

        private void _inputFolderSelectionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog().Equals(CommonFileDialogResult.Ok))
                CurrentInputFolderPath = dialog.FileName;
            return;
        }
        
        private void _outputFolderSelectionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog().Equals(CommonFileDialogResult.Ok))
                CurrentOutputFolderPath = dialog.FileName;
            return;
        }

        private IEnumerable<string> GetImagePaths(
            string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return new string[0];

            var fileNames = Directory.GetFiles(folderPath);
            return fileNames.Where(fileName => fileName.ToLower().EndsWith(".jpg"));
        }

        private void UpdateInputTextBoxes()
        {
            _inputFolderTextBox.Text = CurrentInputFolderPath;
            
            var imageNames = GetImagePaths(CurrentInputFolderPath);
            var firstImageNames = imageNames.Take(5);
            var firstImageNamesString = firstImageNames.Any()
                ? firstImageNames.Aggregate((current, next) => current + "," + Environment.NewLine + next) + Environment.NewLine + "..."
                : "No files found.";
            _inputFilesTextBox.Text = firstImageNamesString;
        }

        private void UpdateOutputTextBoxes()
        {
            _outputFolderTextBox.Text = CurrentOutputFolderPath;

            var fileNames = Directory.GetFiles(CurrentOutputFolderPath);
            if (fileNames.Any())
                MessageBox.Show("Attention: Output folder is not empty!");
        }

        private void _sortButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentOutputFolderPath))
            {
                MessageBox.Show("Select output folder.");
                return;
            }

            var imagePaths = GetImagePaths(CurrentInputFolderPath);

            var imagesWithErrorsStringBuilder = new StringBuilder();
            foreach (var imagePath in imagePaths)
            {
                try
                {
                    var imageName = imagePath.Split('\\').Last();

                    using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var bitmap = BitmapFrame.Create(fileStream);
                        var metadata = (BitmapMetadata)bitmap.Metadata;
                        var dateString = metadata.DateTaken;
                        var date = DateTime.ParseExact(dateString, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        var newName = _currentOutputFolderPath + "\\" + date.ToString("yyyy-MM-dd HH-mm-ss");
                        var newPath = newName + ".jpg";

                        while (File.Exists(newPath))
                        {
                            newName = newName + "_2";
                            newPath = newName + ".jpg";
                        }

                        File.Copy(imagePath, newPath);
                    }
                }
                catch
                {
                    imagesWithErrorsStringBuilder.AppendLine(imagePath);
                }
            }

            if (imagesWithErrorsStringBuilder.Length > 0)
                MessageBox.Show("Done with errors:" + Environment.NewLine + imagesWithErrorsStringBuilder.ToString());
            else
                MessageBox.Show("Done.");
        }
    }
}
