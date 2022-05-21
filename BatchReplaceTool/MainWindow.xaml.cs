using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatchReplace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private ObservableCollection<string> filePathList = new ObservableCollection<string>();
        public ObservableCollection<string> FilePathList { get { return filePathList; } set { filePathList = value; OnPropertyChanged(); } }

        private int selectedindex;
        public int Selectedindex { get { return selectedindex; } set { selectedindex = value; OnPropertyChanged(); } }

        private string targetString = "";
        public string TargetString { get { return targetString; } set { targetString = value; OnPropertyChanged(); } }

        private string replaceString = "";
        public string ReplaceString { get { return replaceString; } set { replaceString = value; OnPropertyChanged(); } }


        public MainWindow()
        {
            InitializeComponent();

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ReplaceBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TargetString.Equals("")) return;

                foreach (string path in FilePathList)
                {
                    var content = File.ReadAllBytes(path);
                    var newContent = ReplaceBytes(content, Encoding.UTF8.GetBytes(TargetString), Encoding.UTF8.GetBytes(ReplaceString));

                    File.WriteAllBytes(path, newContent);
                }
                MessageBox.Show("Done.");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var f in openFileDialog.FileNames)
                    FilePathList.Add(f);
            }
        }

        private void DelBtn_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = Selectedindex;
            if (selectedIndex >= 0)
            {
                FilePathList.RemoveAt(selectedIndex);
                Selectedindex = selectedIndex >= FilePathList.Count ? selectedIndex - 1 : selectedIndex;
            }
        }

        public static List<int> FindBytes(byte[] src, byte[] find)
        {
            var indexList=new List<int>();
            int matchIndex = 0;

            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        int index = i - matchIndex;
                        indexList.Add(index);
                        matchIndex = 0;
                    }
                    else
                    {
                        matchIndex++;
                    }
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }
            }
            return indexList;
        }

        public static byte[] ReplaceBytes(byte[] src, byte[] search, byte[] replace)
        {
            var indexList = FindBytes(src, search);
            byte[] dst = new byte[src.Length + (-search.Length + replace.Length) * indexList.Count];
            int dstOffset = 0;
            for (int i = 0; i < indexList.Count; i++)
            {
                int curIndex = indexList[i];
                if (i == 0)
                {
                    // before found array
                    Buffer.BlockCopy(src, 0, dst, 0, curIndex);
                    dstOffset = curIndex;
                }
                else
                {
                    // previous to current index copy
                    int preIndex = indexList[i - 1];
                    int srcOffset = preIndex + search.Length;
                    int length = curIndex - srcOffset;
                    Buffer.BlockCopy(src, srcOffset, dst, dstOffset, length);
                    dstOffset += length;
                }

                // replace copy
                Buffer.BlockCopy(replace, 0, dst, dstOffset, replace.Length);
                dstOffset += replace.Length;
                
                if (i == indexList.Count - 1)
                {
                    // rest of src array
                    int srcOffset = curIndex + search.Length;
                    Buffer.BlockCopy(src, srcOffset, dst, dstOffset, src.Length - srcOffset);
                }
            }
            return dst;
        }



    }
}
