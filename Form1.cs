using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinFormApp
{
    public partial class Form1 : Form
    {
        private List<Hist> scoreList;
        private XmlData xmlData;
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            var pathPos = Environment.CurrentDirectory.IndexOf("bin");
            var path = Environment.CurrentDirectory.Substring(0, pathPos);
            //xmlFile = string.Format(@"{0}Data\Handicap.new.xml", path);

            ofd.InitialDirectory = path + "Data";
            //ofd.Filter = "xml files (*.xml)";
            //ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    xmlData = new XmlData();
                    scoreList = xmlData.GetData(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            


            var playerNameList = scoreList.GroupBy(p => p.Player).Select(g => g.First().Player).ToList();
            playerNameList.Insert(0,"ALL");
            cboPlayer.DataSource = playerNameList;

            ShowChart(scoreList, cboPlayer.SelectedValue.ToString());
        }

        private void ShowChart(List<Hist> scoreList, string playerName)
        {
            chart1.Visible = true;
            dataGridView1.Visible = false;
            lblHandcap.Text = string.Empty;
            btnAdd.Visible = playerName != "ALL";
            btnShowAll.Visible = playerName != "ALL";
            btnAdd.Text = "Add";
            dtpDate.Visible = false;
            nudScore.Visible = false;
            
            

            var playerList = scoreList.GroupBy(h => h.Player).ToList();
            chart1.Series.Clear();
            chart1.ChartAreas.First().AxisX.Interval = 1;
            foreach (var player in playerList)
            {
                if (playerName == "ALL" || playerName == player.First().Player)
                {
                    List<string> hDate = player.Select(h => h.DateRound.ToString("dd MMM")).ToList();
                    List<int> hScore = player.Select(h => h.Score).ToList();
                    Series a = new Series();
                    a.ChartType = SeriesChartType.Line;
                    a.Name = player.First().Player;
                    a.Points.DataBindXY(hDate, hScore);
                    chart1.Series.Add(a);
                }
            }

            chart1.ApplyPaletteColors();

            if (playerName == "ALL")
            {
                foreach (var ser in chart1.Series)
                {
                    ser.LabelForeColor = ser.Color;
                    foreach (DataPoint d in ser.Points)
                    {
                        d.Label = d.YValues[0].ToString();
                    }
                }
            }
            else
            {
                var playerScoreList = scoreList.Where(s => s.Player == playerName).ToList();

                int cRound = 10;
                if (playerScoreList.Count() < 20)
                {
                    cRound = Convert.ToInt32(Math.Floor(playerScoreList.Count() * 1.0 / 2)) - 1;
                    if (cRound < 1)
                    {
                        cRound = 1;
                    }
                }

                var handicap = (playerScoreList.OrderByDescending(p => p.DateRound).Take(20).OrderBy(p => p.Score).Take(cRound).Average(p => p.Score)) * .96;
                lblHandcap.Text = string.Format("Handicap = {0} ( from {1} )", Math.Round(handicap, 0), Math.Round(handicap, 2));

                var scoreTakenList = playerScoreList.OrderByDescending(p => p.DateRound).Take(20).OrderBy(p => p.Score).Take(cRound)
                    .Select(h => h.DateRound.ToString("dd MMM")).ToList();

                foreach (var ser in chart1.Series)
                {
                    ser.LabelForeColor = ser.Color;
                    foreach (DataPoint d in ser.Points)
                    {
                        d.Label = d.YValues[0].ToString();
                        if (scoreTakenList.Contains(d.AxisLabel))
                        {
                            d.LabelForeColor = Color.Red;
                        }
                    }
                }

                dtpDate.MinDate = playerScoreList.OrderBy(s => s.DateRound).Last().DateRound.AddDays(1);
                nudScore.Value = (int)Math.Round(handicap, 0);
                dtpDate.Value = DateTime.Now > dtpDate.MinDate ? DateTime.Now : dtpDate.MinDate;
            }
        }

        private void cboPlayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowChart(scoreList, cboPlayer.SelectedValue.ToString());
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (btnAdd.Text == "Add")
            {
                btnAdd.Text = "Save";
                dtpDate.Visible = true;
                nudScore.Visible = true;
            }
            else
            {
                scoreList.Add(new Hist { DateRound = dtpDate.Value, Score = (int)nudScore.Value, Player = cboPlayer.SelectedValue.ToString() });
                xmlData.SaveData(scoreList.OrderBy(s => s.Player).ThenBy(s => s.DateRound).ToList());
                //Refresh
                ShowChart(scoreList, cboPlayer.SelectedValue.ToString());
            }
        }

        private void btnShowAll_Click(object sender, EventArgs e)
        {
            string line = "------------------------------------------------------------------------------------------------------------------------";
            
            if (btnShowAll.Text.Contains("All"))
            {
                btnShowAll.Text = "Show 20";
                dataGridView1.Visible = true;
                chart1.Visible = false;
            }
            else
            {
                btnShowAll.Text = "Show All";
                dataGridView1.Visible = false;
                chart1.Visible = true;
            }
            var allScore = new DataTable();
            allScore.Columns.Add("Date");
            allScore.Columns.Add("Score");

            var playerScoreList = scoreList.Where(s => s.Player == cboPlayer.SelectedValue.ToString()).ToList();

            foreach (var score in playerScoreList)
            {
                DataRow row = allScore.NewRow();
                row["Date"] = score.DateRound.ToString("dd MMM yyyy");
                var lieScore = string.Format("{0}{1:00}{2}", line.Substring(0, score.Score * 2 - 1), score.Score, line.Substring(score.Score * 2 + 1));
                row["Score"] = lieScore;
                allScore.Rows.Add(row);
            }
            dataGridView1.DataSource = allScore;
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }
    }
}
