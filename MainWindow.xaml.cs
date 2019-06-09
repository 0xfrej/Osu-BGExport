using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Osu_BGExport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string[] Files;
        public List<string> BGFiles;

        public MainWindow()
        {
            InitializeComponent();
            Source.IsReadOnly = true;
            Destination.IsReadOnly = true;
            Source.Text = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\osu!\Songs";
            this.SourceSelectButton_Click(Source, EventArgs.Empty);
        }

        private void SourceSelectButton_Click(object sender, EventArgs e)
        {
            if (!sender.Equals(Source))
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select path to folder containing your Osu! song files (unpacked)";
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Source.Text = fbd.SelectedPath;
                }
            }
            if (Directory.Exists(Source.Text))
            {
                this.BGFiles = new List<string>();
                Regex regex = new Regex("\".+\"");
                StatusLabel.Content = "Scanning source directory...";
                this.Files = Directory.GetFiles(Source.Text, "*.osu", SearchOption.AllDirectories);
                ScannedFileCount.Content = this.Files.Length;
                foreach (string file in this.Files)
                {
                    string MapDir = System.IO.Path.GetDirectoryName(file);
                    string[] textLines = File.ReadAllLines(file);
                    foreach (string line in textLines.Where(l => l.StartsWith("0,0,\"")))
                    {
                        Match result = regex.Match(line);
                        if (result.Success)
                        {
                            this.BGFiles.Add(MapDir+'\\'+result.Value);
                        }
                    }
                }
                this.BGFiles = BGFiles.Distinct().ToList();
                TotalBackgroundFileCount.Content = this.BGFiles.Count.ToString();
                StatusLabel.Content = "";
                progress.Maximum = this.BGFiles.Count;
            }
            else
            {
                StatusLabel.Content = "Source directory doesn't exist!";
            }
        }

        private void DestinationSelectButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Destination.Text = fbd.SelectedPath;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(Source.Text != "" && Directory.Exists(Source.Text) && Destination.Text != "" && Directory.Exists(Destination.Text))
            {
                var i = 0;
                foreach (string f in this.BGFiles)
                {
                    var ff= f.Replace("\"", "");
                    string filepath = Destination.Text + "\\" + System.IO.Path.GetFileName(ff);
                    if (File.Exists(filepath))
                    {
                        string folder = System.IO.Path.GetDirectoryName(filepath);
                        string filename = System.IO.Path.GetFileNameWithoutExtension(filepath);
                        string extension = System.IO.Path.GetExtension(filepath);
                        int number = 1;

                        Match regex = Regex.Match(filepath, @"(.+) \((\d+)\)\.\w+");

                        if (regex.Success)
                        {
                            filename = regex.Groups[1].Value;
                            number = int.Parse(regex.Groups[2].Value);
                        }

                        do
                        {
                            number++;
                            filepath = System.IO.Path.Combine(folder, string.Format("{0} ({1}){2}", filename, number, extension));
                        }
                        while (File.Exists(filepath));
                    }
                    StatusLabel.Content = "Copying file " + ++i + "/" + this.BGFiles.Count.ToString();
                    File.Copy(ff, filepath);
                    progress.Value = i;
                }
                StatusLabel.Content = "";
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
