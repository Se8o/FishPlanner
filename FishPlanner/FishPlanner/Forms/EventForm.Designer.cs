using FishPlanner.Database;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

public partial class cmbTags : Form // Třída se musí jmenovat EventForm
{
    private TextBox txtTitle;
    private RichTextBox txtDescription;
    private DateTimePicker dtStart;
    private DateTimePicker dtEnd;
    private ComboBox cmbTagsList;
    private Button btnSave;
    private DateTime selectedDate;

    public cmbTags(DateTime date) // Konstruktor musí mít stejný název jako třída
    {
        InitializeComponent();
        selectedDate = date;
        dtStart.Value = date;
        dtEnd.Value = date;
        LoadTags();
    }

    private void InitializeComponent()
    {
        this.Text = "Přidat událost";
        this.Size = new System.Drawing.Size(400, 400);

        txtTitle = new TextBox { Top = 10, Left = 10, Width = 360 };
        txtTitle.Text = "Název";

        txtDescription = new RichTextBox { Top = 40, Left = 10, Width = 360, Height = 100 };
        dtStart = new DateTimePicker { Top = 150, Left = 10, Width = 200 };
        dtEnd = new DateTimePicker { Top = 180, Left = 10, Width = 200 };
        cmbTagsList = new ComboBox { Top = 210, Left = 10, Width = 200 };
        btnSave = new Button { Text = "Uložit", Top = 250, Left = 10 };

        btnSave.Click += btnSave_Click;

        this.Controls.Add(txtTitle);
        this.Controls.Add(txtDescription);
        this.Controls.Add(dtStart);
        this.Controls.Add(dtEnd);
        this.Controls.Add(cmbTagsList);
        this.Controls.Add(btnSave);
    }

    private void LoadTags()
    {
        using (var conn = DbManager.GetConnection())
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT Id, Name FROM Tags", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                cmbTagsList.Items.Add(new ComboBoxItem(reader["Name"].ToString(), (int)reader["Id"]));
            }
        }
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        using (var conn = DbManager.GetConnection())
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("INSERT INTO Events (Title, Description, StartDate, EndDate, TagId, UserId) VALUES (@title, @desc, @start, @end, @tag, @user)", conn);
            cmd.Parameters.AddWithValue("@title", txtTitle.Text);
            cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
            cmd.Parameters.AddWithValue("@start", dtStart.Value);
            cmd.Parameters.AddWithValue("@end", dtEnd.Value);
            cmd.Parameters.AddWithValue("@tag", ((ComboBoxItem)cmbTagsList.SelectedItem).Value);
            cmd.Parameters.AddWithValue("@user", 1); // Zatím napevno

            cmd.ExecuteNonQuery();
            MessageBox.Show("Událost uložena.");
            this.Close();
        }
    }

    // Pomocná třída pro ComboBox
    public class ComboBoxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }

        public ComboBoxItem(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString() => Text;
    }
}
