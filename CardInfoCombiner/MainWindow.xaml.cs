using CardInfoCombiner.Data;
using CardInfoCombiner.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;

namespace CardInfoCombiner
{

    public partial class MainWindow : Window
    {
        readonly string[] extensions = { ".xml", ".csv" };

        private CurrentFolder _currentFolder;
        private CardsReady _cardsReadyCount;
        private Task _cardMerdgerTask;
        private CancellationTokenSource _cts;

        ConcurrentDictionary<int, FullCardInfo> _cards;
        ConcurrentBag<int> _fullFilled;

        public MainWindow()
        {
            InitializeComponent();
            _currentFolder = new CurrentFolder();
            Binding cfBinding = new Binding("CFProperty")
            {
                Source = _currentFolder
            };
            BindingOperations.SetBinding(currentFolderTB, TextBox.TextProperty, cfBinding);

            _cardsReadyCount = new CardsReady();
            Binding crBinding = new Binding("CardReadyProperty")
            {
                Source = _cardsReadyCount
            };
            BindingOperations.SetBinding(cardsReadyTB, TextBox.TextProperty, crBinding);
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    _currentFolder.CFProperty = dialog.SelectedPath;

                    _cts?.Cancel();
                    _cts = new CancellationTokenSource();
                    _cardMerdgerTask = Task.Run(() => FileMerger(_cts.Token), _cts.Token);
                }
            }
        }

        private void FileMerger(CancellationToken token)
        {
            _cards = new ConcurrentDictionary<int, FullCardInfo>();
            _fullFilled = new ConcurrentBag<int>();
            DirectoryInfo di = new DirectoryInfo(_currentFolder.CFProperty);
            try
            {
                Parallel.ForEach(di.EnumerateFiles().Where(x => extensions.Contains(x.Extension.ToLower())),
                    new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = token },
                    (file) =>
                 {
                     switch (file.Extension)
                     {
                         case ".csv":
                             HandleCsv(file);
                             break;
                         case ".xml":
                             HandleXml(file);
                             break;
                         default:
                             throw new NotImplementedException();
                     }
                 });
            }
            catch (OperationCanceledException)
            {
                //отмена сканирования текущей папки
            }
        }

        private void HandleCsv(FileInfo file)
        {
            var content = File.ReadAllLines(file.FullName);
            foreach (var line in content)
            {
                var splitted = line.Split(',');
                var cid = int.Parse(splitted[2]);
                AddCardInfo(cid, null, new CsvInfoPart { ClientID = cid, FIO = splitted[0], Tel = splitted[1] });
            }
        }
        private void HandleXml(FileInfo file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(file.FullName);
            XmlElement root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes)
            {
                var cid = int.Parse(node["ClientID"].InnerText);
                AddCardInfo(cid, new XmlInfoPart
                {
                    ClientID = cid,
                    ExpiredDate = DateTime.Parse(node["ExpiredDate"].InnerText),
                    PAN = node["PAN"].InnerText
                }, null);
            }

        }

        private void AddProcessedCard(int cid)
        {

            _fullFilled.Add(cid);
            _cardsReadyCount.CardReadyProperty = _fullFilled.Count;

        }

        private void AddCardInfo(int cid, XmlInfoPart xml, CsvInfoPart csv)
        {
            _cards.AddOrUpdate(cid, new FullCardInfo { xml = xml, csv = csv }, (id, oldValue) =>
            {
                oldValue.Processed = true;
                if (xml != null && oldValue.csv != null)
                    oldValue.xml = xml;
                else if (csv != null && oldValue.xml != null)
                    oldValue.csv = csv;
                else
                    oldValue.Processed = false;
                if (oldValue.Processed)
                    AddProcessedCard(cid);
                return oldValue;
            });
        }

        private void generateReportButton_Click(object sender, RoutedEventArgs e)
        {
            ConcurrentBag<int> processedCards = _fullFilled;
            _fullFilled = new ConcurrentBag<int>();
            _cardsReadyCount.CardReadyProperty = _fullFilled.Count;
            Task.Run(() =>
            {
                using (StreamWriter sw = new StreamWriter($"CARDS_{DateTime.Now.ToString("dd_MM_yy_HH_mm_ss")}_{processedCards.Count}.csv"))
                {
                    foreach (var id in processedCards)
                    {
                        var current = _cards[id];
                        sw.WriteLine($"{id},{current.csv.FIO},{current.csv.Tel},{current.xml.PAN},{current.xml.ExpiredDate}");
                    }
                }
            });
        }
    }
}
