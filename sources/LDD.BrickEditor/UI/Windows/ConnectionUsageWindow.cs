using LDD.Core;
using LDD.Core.Primitives;
using LDD.Core.Primitives.Connectors;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LDD.BrickEditor.UI.Windows
{
    public partial class ConnectionUsageWindow : Form
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private bool IsAnalysing { get; set; }
        private Task AnalyseTask { get; set; }
        private bool StopAnalysing { get; set; }

        private List<ConnUsageModel> ConnectionsList { get; set; }

        public ConnectionUsageWindow()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = false;
            progressBar1.Visible = false;
        }

        class ConnUsageModel
        {
            public Core.Primitives.Connectors.ConnectorType ConnectionType { get; set; }
            public int SubType { get; set; }
            public int RefCount { get; set; }
            public string Parts { get; set; }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            if (!IsAnalysing)
            {
                IsAnalysing = true;
                AnalyseTask = Task.Factory.StartNew(FetchConnectionsUsage);
                progressBar1.Visible = true;
                SearchButton.Text = CancelAnalysingLabel;
            }
            else
            {
                if (!AnalyseTask.IsCompleted)
                {
                    StopAnalysing = true;
                    SearchButton.Text = CancelingLabel;
                    SearchButton.Enabled = false;
                    AnalyseTask.Wait();
                }
                SearchButton.Text = StartAnalysingLabel;
                SearchButton.Enabled = true;
                progressBar1.Visible = false;
                IsAnalysing = false;
            }
        }

        private void UpdateProgress(int value, int max)
        {
            BeginInvoke((Action)(() =>
            {
                progressBar1.Maximum = max;
                progressBar1.Value = value;
            }));
        }

        private void FetchConnectionsUsage()
        {
            var primitivesPath = LDDEnvironment.Current.GetAppDataSubDir("db\\Primitives");

            var connDic = new Dictionary<Tuple<int, int>, List<string>>();
            var fileList = Directory.GetFiles(primitivesPath, "*.xml");
            int totalFiles = fileList.Length;
            UpdateProgress(0, totalFiles);

            int progress = 0;

            foreach (string xmlPath in fileList)
            {
                if (StopAnalysing)
                    return;

                try
                {
                    var primitive = Primitive.Load(xmlPath);
                    var allConnectors = primitive.Connectors.Concat(primitive.FlexBones.SelectMany(b => b.Connectors));

                    foreach (var connGroup in allConnectors.GroupBy(x => new Tuple<int,int> ((int)x.Type, x.SubType)))
                    {
                        if (!connDic.ContainsKey(connGroup.Key))
                            connDic.Add(connGroup.Key, new List<string>());

                        connDic[connGroup.Key].Add(primitive.ID.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Error in {nameof(FetchConnectionsUsage)}");
                }
                UpdateProgress(++progress, totalFiles);
            }

            ConnectionsList = new List<ConnUsageModel>();

            foreach (var typeGroup in connDic.Keys.OrderBy(x => x.Item1).GroupBy(x => x.Item1))
            {
                var connType = ((ConnectorType)typeGroup.Key);
                var connections = typeGroup.Select(x => new ConnUsageModel
                {
                    ConnectionType = connType,
                    SubType = x.Item2,
                    RefCount = connDic[x].Count,
                    Parts = string.Join(", ", connDic[x])
                }).ToList();

                if (!(connType == ConnectorType.Axel ||
                      connType == ConnectorType.Custom2DField ||
                      connType == ConnectorType.Gear))
                {
                    int minType = connections.Min(c => c.SubType);
                    int maxType = connections.Where(c => c.SubType < 900000).Max(c => c.SubType);
                    if (maxType % 2 == 0)
                        maxType++;

                    int consecutiveItems = 0;
                    int lastMissing = 0;
                    bool isSkipping = false;

                    for (int i = minType; i <= maxType; i++)
                    {
                        
                        if (!connections.Any(c => c.SubType == i))
                        {
                            if (isSkipping)
                                continue;

                            if (lastMissing + 1 == i)
                            {
                                consecutiveItems++;
                                if (consecutiveItems >= 20)
                                {
                                    isSkipping = true;
                                    int removeFrom = connections.Count - consecutiveItems;
                                    int removeCount = (connections.Count - removeFrom);
                                    connections.RemoveRange(removeFrom, removeCount);
                                    continue;
                                }
                            }

                            lastMissing = i;

                            connections.Add(new ConnUsageModel
                            {
                                ConnectionType = connType,
                                SubType = i,
                                RefCount = 0,
                                Parts = "Not used"
                            });
                            
                        }
                        else
                        {
                            isSkipping = false;
                            consecutiveItems = 0;
                        }
                    }
                }

                ConnectionsList.AddRange(connections.OrderBy(c => c.SubType));
            }

            IsAnalysing = false;
            BeginInvoke(new MethodInvoker(OnSearchFinished));
        }

        private void OnSearchFinished()
        {
            dataGridView1.DataSource = ConnectionsList;
            ExportButton.Enabled = true;
            progressBar1.Visible = false;
            SearchButton.Text = StartAnalysingLabel;
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            var slDoc = CreateExcelDoc();

            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = ExcelFileFilterLabel.Text + "|*.xlsx";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        slDoc.SaveAs(dlg.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBoxEX.ShowException(this, "TODO", "TODO", ex);
                    }
                }
            }
        }
    
        private SLDocument CreateExcelDoc()
        {
            var slDoc = new SLDocument();
            var boldStyle = slDoc.CreateStyle();
            boldStyle.Font.Bold = true;

            var altRowStyle = slDoc.CreateStyle();
            var altRowColor = Color.FromArgb(220, 230, 241);
            altRowStyle.Fill.SetPattern(DocumentFormat.OpenXml.Spreadsheet.PatternValues.Solid,
                altRowColor,
                altRowColor);
            bool isFirst = true;

            foreach (var typeGroup in ConnectionsList.GroupBy(x => x.ConnectionType))
            {
                if (isFirst)
                {
                    slDoc.RenameWorksheet(slDoc.GetCurrentWorksheetName(), typeGroup.Key.ToString());
                    isFirst = false;
                }
                else
                {
                    slDoc.AddWorksheet(typeGroup.Key.ToString());
                }

                slDoc.SetRowStyle(1, boldStyle);
                slDoc.SetCellValue("A1", "Connection Type");
                slDoc.SetColumnWidth("A", 18);
                slDoc.SetCellValue("B1", "Sub-Type");
                slDoc.SetColumnWidth("B", 12);
                slDoc.SetCellValue("C1", "Total usage");
                slDoc.SetColumnWidth("C", 12);
                slDoc.SetCellValue("D1", "Parts");
                slDoc.SetColumnWidth("D", 150);
                int currentRow = 2;

                bool isAltRow = false;
                int lastSubTypeId = -1;

                foreach (var conn in typeGroup.OrderBy(x => x.SubType))
                {

                    //if (conn.SubType < 900000 && !(typeGroup.Key == ConnectorType.Axel ||
                    //    typeGroup.Key == ConnectorType.Custom2DField ||
                    //    typeGroup.Key == ConnectorType.Gear))
                    //{

                    //    if (conn.SubType < 900000 && conn.SubType % 2 == 1)
                    //        isAltRow = !isAltRow;
                    //}

                    if (isAltRow && conn.SubType < 900000)
                    {
                        slDoc.SetRowStyle(currentRow, altRowStyle);
                    }

                    slDoc.SetCellValue($"A{currentRow}", conn.ConnectionType.ToString());
                    slDoc.SetCellValue($"B{currentRow}", conn.SubType);
                    slDoc.SetCellValue($"C{currentRow}", conn.RefCount);
                    slDoc.SetCellValue($"D{currentRow}", conn.Parts);

                    if (!(typeGroup.Key == ConnectorType.Axel ||
                        typeGroup.Key == ConnectorType.Custom2DField ||
                        typeGroup.Key == ConnectorType.Gear))
                    {
                        if (conn.SubType % 2 == 1)
                            isAltRow = !isAltRow;
                    }

                    lastSubTypeId = conn.SubType;
                    currentRow++;
                }
            }

            slDoc.SelectWorksheet(slDoc.GetWorksheetNames()[0]);

            return slDoc;
        }
    }
}
