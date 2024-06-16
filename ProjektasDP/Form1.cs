using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ProjektasDP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataGridView1.CellClick += dataGridView1_CellClick;
            listBox2.SelectedIndexChanged += listBox2_SelectedIndexChanged;
            InitializeComboBox();
        }

        public SqlConnection cnn;
        string connectionString = "Data source=DESKTOP-HRV85Q6\\SQLEXPRESS02;Initial catalog=projektas;User ID=sa;Password=123456";

        // Mygtuko paspaudimo ivykis, kuris nukeliauja i "Ataskaitos" forma
        private void button4_Click(object sender, EventArgs e)
        {
            Ataskaitos Obj = new Ataskaitos();
            Obj.Show();
            this.Hide();
        }

        // Patikrina, ar "kaina" laukas turi teisinga skaitmenine reiksme
        private bool PatikrintiKaina()
        {
            if (!decimal.TryParse(textBox2.Text, out decimal kaina))
            {
                MessageBox.Show("Prašome įvesti tik skaičių 'Kaina'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (kaina < 0)
            {
                MessageBox.Show("Kaina negali būti neigiama.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        // Mygtuko paspaudimo ivykis, kuris prideda nauja irasa i duomenu baze
        private void button1_Click(object sender, EventArgs e)
        {
            if (!PatikrintiKaina())
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO istekliai (pavadinimas_Id, kaina, kategorija, data, pastabos) VALUES (@pavadinimas_Id, @kaina, @kategorija, @data, @pastabos)";
                    SqlCommand insertSQL = new SqlCommand(query, connection);
                    insertSQL.Parameters.AddWithValue("@pavadinimas_Id", GetSelectedPavadinimasId());
                    insertSQL.Parameters.AddWithValue("@kaina", textBox2.Text);
                    insertSQL.Parameters.AddWithValue("@kategorija", listBox2.Text);
                    insertSQL.Parameters.AddWithValue("@data", dateTimePicker1.Value);
                    insertSQL.Parameters.AddWithValue("@pastabos", textBox5.Text);
                    insertSQL.ExecuteNonQuery();
                    MessageBox.Show("Duomenys pateikti į duomenų bazę!");

                    // Prideda nauja kategorija i ListBox, jei jos dar nera
                    if (!listBox2.Items.Contains(listBox2.Text))
                    {
                        listBox2.Items.Add(listBox2.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Mygtuko paspaudimo ivykis, kuris uzpildo DataGridView duomenimis is duomenu bazes ir parodo likuti, pajamas ir islaidas
        private void button6_Click(object sender, EventArgs e)
        {
            cnn = new SqlConnection(connectionString);
            try
            {
                cnn.Open();
                string sql = "SELECT i.ID, p.pavadinimas, i.kaina, i.kategorija, i.data, i.pastabos FROM istekliai i JOIN pavadinimas p ON i.pavadinimas_Id = p.ID";
                SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                DataSet ds = new DataSet();
                da.Fill(ds, "istekliai");
                dataGridView1.DataSource = ds.Tables["istekliai"].DefaultView;

                decimal pajamosSum = 0;
                decimal islaidosSum = 0;

                // Apskaiciuojama pajamu ir islaidu suma
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewRow row = dataGridView1.Rows[i];
                    if (row.Cells["kategorija"].Value != null && row.Cells["kaina"].Value != null)
                    {
                        string kategorija = row.Cells["kategorija"].Value.ToString();
                        decimal kaina = Convert.ToDecimal(row.Cells["kaina"].Value);

                        if (kategorija == "Pajamos")
                        {
                            pajamosSum += kaina;
                        }
                        else if (kategorija == "Išlaidos")
                        {
                            islaidosSum += kaina;
                        }
                    }
                }

                decimal likutis = pajamosSum - islaidosSum;

                // Parodo apskaiciuotus rezultatus teksto laukeliuose
                pajamos.Text = pajamosSum.ToString();
                islaidos.Text = islaidosSum.ToString();
                likutiss.Text = likutis.ToString();

                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // DataGridViewCellClick ivykis
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["pavadinimas"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["pavadinimas"].Value.ToString()))
                {
                    comboBox1.SelectedItem = row.Cells["pavadinimas"].Value.ToString();
                    textBox2.Text = row.Cells["kaina"].Value.ToString();
                    textBox3.Text = row.Cells["ID"].Value.ToString();
                    listBox2.Text = row.Cells["kategorija"].Value.ToString();
                    dateTimePicker1.Value = Convert.ToDateTime(row.Cells["data"].Value);
                    textBox5.Text = row.Cells["pastabos"].Value.ToString();
                }
                else
                {
                    comboBox1.SelectedItem = null;
                    textBox2.Clear();
                    textBox3.Clear();
                    listBox2.Text = string.Empty;
                    dateTimePicker1.Value = DateTime.Now;
                    textBox5.Clear();
                }
            }
        }

        // Mygtuko paspaudimo ivykis, kuris atnaujina esama irasa duomenu bazeje
        private void button2_Click(object sender, EventArgs e)
        {
            if (!PatikrintiKaina())
            {
                return;
            }

            try
            {
                string id = textBox3.Text;

                string query = "UPDATE istekliai SET pavadinimas_Id = @pavadinimas_Id, kaina = @kaina, kategorija = @kategorija, data = @data, pastabos = @pastabos WHERE ID = @id";

                SqlCommand update = new SqlCommand(query, cnn);
                update.Parameters.Add("@pavadinimas_Id", SqlDbType.Int).Value = GetSelectedPavadinimasId();
                update.Parameters.Add("@kaina", SqlDbType.Decimal).Value = textBox2.Text;
                update.Parameters.Add("@kategorija", SqlDbType.NVarChar).Value = listBox2.Text;
                update.Parameters.Add("@data", SqlDbType.Date).Value = dateTimePicker1.Value;
                update.Parameters.Add("@pastabos", SqlDbType.NVarChar).Value = textBox5.Text;
                update.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cnn.Open();
                update.ExecuteNonQuery();
                cnn.Close();

                MessageBox.Show("Įrašas atnaujintas sėkmingai.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Klaida atnaujinant įrašą: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Mygtuko paspaudimo ivykis, kuris istrina irasa is duomenu bazes
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "DELETE FROM istekliai WHERE ID=@id";

                SqlCommand delSQL = new SqlCommand(query, cnn);
                delSQL.Parameters.Add("@id", SqlDbType.Int).Value = textBox3.Text;
                delSQL.Connection.Open();
                delSQL.ExecuteNonQuery();
                cnn.Close();
                MessageBox.Show("Duomenys ištrinti iš DB!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ListBox pasirinkimo ivykio tvarkytojas
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadListBoxItems();
        }

        // Inicializuoja ComboBox su pradinemis reiksmemis ir prideda kategorijas is duomenu bazes
        private void InitializeComboBox()
        {
            listBox2.Items.Add("Pajamos");
            listBox2.Items.Add("Išlaidos");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT kategorija FROM istekliai";
                SqlCommand cmd = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string kategorija = reader["kategorija"].ToString();
                    if (!listBox2.Items.Contains(kategorija))
                    {
                        listBox2.Items.Add(kategorija);
                    }
                }
                connection.Close();
            }
        }

        // Uzkrauna ComboBox elementus pagal pasirinkta kategorija ListBox'e
        public void LoadListBoxItems()
        {
            comboBox1.Items.Clear();

            if (listBox2.SelectedItem == null) return;

            string selectedCategory = listBox2.SelectedItem.ToString();
            string query = "SELECT pavadinimas FROM pavadinimas WHERE category = @category";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@category", selectedCategory);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["pavadinimas"].ToString());
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading items: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Grazina pasirinkto pavadinimo ID is duomenu bazes
        private int GetSelectedPavadinimasId()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT ID FROM pavadinimas WHERE pavadinimas = @pavadinimas";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@pavadinimas", comboBox1.SelectedItem.ToString());
                connection.Open();
                int pavadinimasId = (int)cmd.ExecuteScalar();
                connection.Close();
                return pavadinimasId;
            }
        }

        // Mygtuko paspaudimo ivykis, kuris nukeliauja "klasifikatoriai" forma
        private void button7_Click(object sender, EventArgs e)
        {
            klasifikatoriai Obj = new klasifikatoriai();
            Obj.Show();
            this.Hide();
        }
    }
}
