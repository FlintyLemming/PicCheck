using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Input;

namespace PicCheck
{
    public partial class MainWindow : Window
    {
        private string sourceFolderPath;
        private string targetFolderPath;
        private DirectoryInfo currentDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectSourceFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sourceFolderPath = dialog.SelectedPath;
                SourceFolderPath.Text = sourceFolderPath;
            }
        }

        private void SelectTargetFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                targetFolderPath = dialog.SelectedPath;
                TargetFolderPath.Text = targetFolderPath;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sourceFolderPath) || string.IsNullOrEmpty(targetFolderPath))
            {
                System.Windows.Forms.MessageBox.Show("Please select both source and target folders.");
                return;
            }

            ProcessNextFolder();
        }

        private void ProcessNextFolder()
        {
            if (currentDirectory != null)
            {
                DirectoryInfo[] directories = new DirectoryInfo(sourceFolderPath).GetDirectories();
                foreach (var directory in directories)
                {
                    if (directory != currentDirectory)
                    {
                        currentDirectory = directory;
                        DisplayImagesFromFolder(directory);
                        return;
                    }
                }
            }
            else
            {
                DirectoryInfo[] directories = new DirectoryInfo(sourceFolderPath).GetDirectories();
                if (directories.Length > 0)
                {
                    currentDirectory = directories[0];
                    DisplayImagesFromFolder(currentDirectory);
                }
            }
        }

        private async void DisplayImagesFromFolder(DirectoryInfo directory)
        {
            ImagePanel.Children.Clear();
            CurrentFolderPathText.Text = "Current Folder Path: " + directory.FullName;  // 更新当前文件夹路径显示

            var images = directory.GetFiles("*.jpg").Concat(directory.GetFiles("*.png")).OrderBy(x => Guid.NewGuid()).Take(12);

            foreach (var image in images)
            {
                var imageControl = new System.Windows.Controls.Image { Width = 350, Height = 270, Margin = new Thickness(5) };
                imageControl.MouseLeftButtonDown += ImageControl_MouseLeftButtonDown;
                imageControl.Tag = image.FullName;
                ImagePanel.Children.Add(imageControl);
                await Task.Run(() =>
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(image.FullName);
                    bitmap.DecodePixelWidth = 700;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    Dispatcher.Invoke(() => imageControl.Source = bitmap);
                });
            }
        }

        private void ImageControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) // 判断是否是双击
            {
                var imageControl = sender as System.Windows.Controls.Image;
                if (imageControl != null)
                {
                    var imagePath = imageControl.Tag as string;
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
                    }
                }
            }
        }

        private void MoveAndRenameFolder(string suffix)
        {
            string newFolderPath = Path.Combine(targetFolderPath, currentDirectory.Name + suffix);
            // 首先复制文件夹到新的位置
            CopyDirectory(currentDirectory.FullName, newFolderPath);

            // 删除原始文件夹
            Directory.Delete(currentDirectory.FullName, true);
            ProcessNextFolder();
        }

        // 递归复制文件夹的方法
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            // 如果目标目录不存在，则创建它
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            // 获取源目录文件和子目录
            FileInfo[] files = dir.GetFiles();
            DirectoryInfo[] dirs = dir.GetDirectories();

            // 复制所有文件
            foreach (FileInfo file in files)
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true); // 可以选择覆盖现有文件
            }

            // 递归复制所有子目录
            foreach (DirectoryInfo subdir in dirs)
            {
                string newSubDirPath = Path.Combine(destinationDir, subdir.Name);
                CopyDirectory(subdir.FullName, newSubDirPath);
            }
        }

        private void Ex_Click(object sender, RoutedEventArgs e)
        {
            MoveAndRenameFolder(" 【Ex】");
        }

        private void Fine_Click(object sender, RoutedEventArgs e)
        {
            MoveAndRenameFolder(" 【Fine】");
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            MoveAndRenameFolder("");
        }
    }
}