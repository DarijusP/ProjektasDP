using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProjektasDP
{
    public partial class Ataskaitos : Form
    {
        private string connectionString = "Data source=DESKTOP-HRV85Q6\\SQLEXPRESS02;Initial catalog=projektas;User ID=sa;Password=123456";

        public Ataskaitos()
        {
            InitializeComponent();
        }

        // Mygtuko paspaudimo ivykis, kuris pereina i pagrindine forma
        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        // Mygtuko paspaudimo ivykis, kuris generuoja pajamu ir islaidu diagrama
        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
               
                chart1.Series.Clear();

                // Sukuriame naujas diagramu serijas
                Series pajamosSeries = new Series
                {
                    Name = "Pajamos",
                    IsVisibleInLegend = true,
                    ChartType = SeriesChartType.Bar
                };

                Series islaidosSeries = new Series
                {
                    Name = "Išlaidos",
                    IsVisibleInLegend = true,
                    ChartType = SeriesChartType.Bar
                };

                // Pridedame serijas i diagramą
                chart1.Series.Add(pajamosSeries);
                chart1.Series.Add(islaidosSeries);

                // SQL užklausa duomenims istraukti
                string query = "SELECT data, SUM(CASE WHEN kategorija = 'Pajamos' THEN kaina ELSE 0 END) AS Pajamos, " +
                               "SUM(CASE WHEN kategorija = 'Išlaidos' THEN kaina ELSE 0 END) AS Islaidos " +
                               "FROM istekliai " +
                               "WHERE data BETWEEN @StartDate AND @EndDate " +
                               "GROUP BY data " +
                               "ORDER BY data";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@StartDate", dateTimePicker1.Value.Date);
                    command.Parameters.AddWithValue("@EndDate", dateTimePicker2.Value.Date);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    // Duomenu skaitymas is duomenu bazes ir ju pridejimas i diagrama
                    while (reader.Read())
                    {
                        DateTime data = Convert.ToDateTime(reader["data"]);
                        double pajamos = Convert.ToDouble(reader["Pajamos"]);
                        double islaidos = Convert.ToDouble(reader["Islaidos"]);

                        pajamosSeries.Points.AddXY(data, pajamos);
                        islaidosSeries.Points.AddXY(data, islaidos);
                    }

                    reader.Close();
                }

                // Pridedame pavadinima diagramai
                chart1.Titles.Clear();
                chart1.Titles.Add("Diagrama su pajamomis ir išlaidomis");

                // Duomenu analize
                AnalyzeData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Klaida: {ex.Message}", "Klaida", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Funkcija duomenu analizei atlikti
        private void AnalyzeData()
        {
            // Paslepiame analizei skirtas etiketes
            label11.Visible = false;
            label12.Visible = false;
            label13.Visible = false;
            label9.Visible = false;
            label8.Visible = false;
            label7.Visible = false;

            // SQL uzklausa analizei atlikti
            string query = "SELECT kategorija, " +
                           "MIN(kaina) AS MinKaina, " +
                           "MAX(kaina) AS MaxKaina, " +
                           "AVG(kaina) AS VidutineKaina " +
                           "FROM istekliai " +
                           "WHERE data BETWEEN @StartDate AND @EndDate " +
                           "GROUP BY kategorija";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StartDate", dateTimePicker1.Value.Date);
                command.Parameters.AddWithValue("@EndDate", dateTimePicker2.Value.Date);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                // Skaitymas is duomenu bazes ir duomenu atvaizdavimas
                while (reader.Read())
                {
                    string kategorija = reader["kategorija"].ToString();
                    double minKaina = Convert.ToDouble(reader["MinKaina"]);
                    double maxKaina = Convert.ToDouble(reader["MaxKaina"]);
                    double vidutineKaina = Convert.ToDouble(reader["VidutineKaina"]);

                    if (kategorija == "Pajamos")
                    {
                        label11.Text = $"Minimali kaina: {minKaina}";
                        label12.Text = $"Maksimali kaina: {maxKaina}";
                        label13.Text = $"Vidutinė kaina: {vidutineKaina}";

                        // Rodyti pajamu informacija
                        label11.Visible = true;
                        label12.Visible = true;
                        label13.Visible = true;
                    }
                    else
                    {
                        label9.Text = $"Minimali kaina: {minKaina}";
                        label8.Text = $"Maksimali kaina: {maxKaina}";
                        label7.Text = $"Vidutinė kaina: {vidutineKaina}";

                        // Rodyti islaidu informacija
                        label9.Visible = true;
                        label8.Visible = true;
                        label7.Visible = true;
                    }
                }

                reader.Close();
            }
        }
    }
}
