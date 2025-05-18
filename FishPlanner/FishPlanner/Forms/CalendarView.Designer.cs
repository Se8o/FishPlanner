using FishPlanner.Database;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;

namespace FishPlanner.Forms
{
    public partial class CalendarView : Form
    {
        private FlowLayoutPanel flpCalendar;
        private ComboBox cmbFilterTags;
        private DateTimePicker dtpFilterDate;
        private Button btnFilter;

      /* 
         public CalendarView()
        {
            InitializeComponent();
            this.Load += CalendarView_Load;
        }
      */

        private void InitializeComponent()
        {
            this.flpCalendar = new FlowLayoutPanel();
            this.cmbFilterTags = new ComboBox();
            this.dtpFilterDate = new DateTimePicker();
            this.btnFilter = new Button();

            // Nastavení vlastností
            this.cmbFilterTags.Top = 10;
            this.cmbFilterTags.Left = 10;
            this.cmbFilterTags.Width = 150;

            this.dtpFilterDate.Top = 10;
            this.dtpFilterDate.Left = 170;
            this.dtpFilterDate.Width = 120;
            this.dtpFilterDate.Format = DateTimePickerFormat.Short;
            this.dtpFilterDate.ShowCheckBox = true; // umožní zapnout/vypnout filtr data

            this.btnFilter.Top = 10;
            this.btnFilter.Left = 300;
            this.btnFilter.Text = "Filtrovat";
            this.btnFilter.Click += BtnFilter_Click;

            this.flpCalendar.Dock = DockStyle.Fill;
            this.flpCalendar.Top = 50;
            this.flpCalendar.Left = 10;
            this.flpCalendar.Width = 780;
            this.flpCalendar.Height = 390;

            // Přidání na form
            this.Controls.Add(this.cmbFilterTags);
            this.Controls.Add(this.dtpFilterDate);
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.flpCalendar);

            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "Kalendář s filtrem";
        }

        private void CalendarView_Load(object sender, EventArgs e)
        {
            LoadTags();
            ShowSeasonalTip();
            ShowTodayNotifications();
            GenerateCalendar(DateTime.Now);
        }

        private void LoadTags()
        {
            cmbFilterTags.Items.Clear();
            cmbFilterTags.Items.Add("Všechny"); // možnost bez filtru

            using (var conn = DbManager.GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Name FROM Tags", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbFilterTags.Items.Add(reader["Name"].ToString());
                    }
                }
            }

            cmbFilterTags.SelectedIndex = 0;
        }

        private void BtnFilter_Click(object sender, EventArgs e)
        {
            DateTime filterDate = dtpFilterDate.Checked ? dtpFilterDate.Value.Date : DateTime.MinValue;
            string selectedTag = cmbFilterTags.SelectedItem?.ToString() ?? "Všechny";

            GenerateCalendar(DateTime.Now, selectedTag, dtpFilterDate.Checked ? (DateTime?)filterDate : null);
        }

        private void GenerateCalendar(DateTime date, string filterTag = "Všechny", DateTime? filterDate = null)
        {
            flpCalendar.Controls.Clear();

            DateTime startOfMonth = new DateTime(date.Year, date.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            int dayOfWeek = (int)startOfMonth.DayOfWeek;

            var eventsByDate = GetEventsGroupedByDate(date.Year, date.Month, filterTag, filterDate);

            for (int i = 0; i < dayOfWeek; i++)
            {
                flpCalendar.Controls.Add(new Label
                {
                    Width = 100,
                    Height = 80
                }); // prázdný slot
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDay = new DateTime(date.Year, date.Month, day);

                if (filterDate.HasValue && currentDay != filterDate.Value)
                    continue;

                var btn = new Button
                {
                    Width = 100,
                    Height = 80,
                    Text = day.ToString()
                };

                if (eventsByDate.ContainsKey(currentDay))
                {
                    btn.BackColor = System.Drawing.Color.LightBlue;
                    btn.Text += "\n" + eventsByDate[currentDay].Count + " událostí";
                }

                btn.Click += (s, e) =>
                {
                    var addForm = new EventForm(currentDay);
                    addForm.ShowDialog();
                    BtnFilter_Click(null, null); // obnovit kalendář po zavření formuláře
                };

                flpCalendar.Controls.Add(btn);
            }
        }

        private Dictionary<DateTime, List<string>> GetEventsGroupedByDate(int year, int month, string filterTag, DateTime? filterDate)
        {
            var result = new Dictionary<DateTime, List<string>>();

            using (var conn = DbManager.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT e.StartDate, t.Name AS TagName
                    FROM Events e
                    LEFT JOIN Tags t ON e.TagId = t.Id
                    WHERE YEAR(e.StartDate) = @year AND MONTH(e.StartDate) = @month";

                if (filterTag != "Všechny")
                    sql += " AND t.Name = @tag";

                if (filterDate.HasValue)
                    sql += " AND CAST(e.StartDate AS DATE) = @filterDate";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@month", month);
                if (filterTag != "Všechny")
                    cmd.Parameters.AddWithValue("@tag", filterTag);
                if (filterDate.HasValue)
                    cmd.Parameters.AddWithValue("@filterDate", filterDate.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime startDate = (DateTime)reader["StartDate"];
                        string tagName = reader["TagName"]?.ToString() ?? "Bez štítku";

                        if (!result.ContainsKey(startDate.Date))
                            result[startDate.Date] = new List<string>();

                        result[startDate.Date].Add(tagName);
                    }
                }
            }

            return result;
        }

        private void ShowSeasonalTip()
        {
            string tip = "";
            int month = DateTime.Now.Month;

            if (month >= 3 && month <= 5)
                tip = "Jaro je ideální období pro výměnu náčiní a přípravu na sezónu.";
            else if (month >= 6 && month <= 8)
                tip = "Léto je čas pro rybolov v ranních a večerních hodinách.";
            else if (month >= 9 && month <= 11)
                tip = "Podzim je vhodný pro lov dravých ryb.";
            else
                tip = "Zima je ideální pro odpočinek a plánování další sezóny.";

            MessageBox.Show(tip, "Tip pro období");
        }

        private void ShowTodayNotifications()
        {
            using (var conn = DbManager.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT Title 
                    FROM Events 
                    WHERE CAST(StartDate AS DATE) = @today";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@today", DateTime.Today);

                var eventsToday = new StringBuilder();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        eventsToday.AppendLine("- " + reader["Title"].ToString());
                    }
                }

                if (eventsToday.Length > 0)
                {
                    MessageBox.Show($"Dnešní události:\n{eventsToday}", "Notifikace");
                }
            }
        }
    }
}
