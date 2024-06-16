using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ProjektasDP
{
    public partial class klasifikatoriai : Form
    {
        public klasifikatoriai()
        {
            InitializeComponent();
            // Pridedamas ivykio tvarkytojas DataGridViewCellClick ivykiui
            dataGridView1.CellClick += new DataGridViewCellEventHandler(dataGridView1_CellClick);
        }

        private SqlConnection cnn;
        private string connectionString = "Data source=DESKTOP-HRV85Q6\\SQLEXPRESS02;Initial catalog=projektas;User ID=sa;Password=123456";

        // Mygtuko paspaudimo ivykis, kuris grazina pagrindine forma
        private void button1_Click(object sender, EventArgs e)
        {
            Form1 Obj = new Form1();
            Obj.Show();
            this.Hide();
        }

        // Mygtuko paspaudimo ivykis, kuris uzpildo DataGridView duomenimis is duomenų bazes
        private void button5_Click(object sender, EventArgs e)
        {
            cnn = new SqlConnection(connectionString);
            try
            {
                cnn.Open();
                string sql = "SELECT ID, pavadinimas FROM pavadinimas";
                SqlDataAdapter da = new SqlDataAdapter(sql, cnn);
                DataSet ds = new DataSet();
                da.Fill(ds, "pavadinimas");
                dataGridView1.DataSource = ds.Tables["pavadinimas"].DefaultView;
                cnn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Mygtuko paspaudimo ivykis, kuris prideda nauja irasa i duomenu baze
        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string category = radioButton1.Checked ? "Pajamos" : "Išlaidos";
                        string query = "INSERT INTO pavadinimas (pavadinimas, category) VALUES (@pavadinimas, @category)";
                        SqlCommand insertSQL = new SqlCommand(query, connection);
                        insertSQL.Parameters.AddWithValue("@pavadinimas", textBox1.Text);
                        insertSQL.Parameters.AddWithValue("@category", category);
                        insertSQL.ExecuteNonQuery();
                        MessageBox.Show("Klasifikatorius pridėtas!");

                        // Atnaujinama pagrindines formos ListBox elementus
                        Form1 form1 = (Form1)Application.OpenForms["Form1"];
                        if (form1 != null)
                        {
                            form1.LoadListBoxItems();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kažkas negerai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Prašome įvesti klasifikatorių", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // DataGridView lastelės paspaudimo ivykis
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["pavadinimas"].Value != null && !string.IsNullOrWhiteSpace(row.Cells["pavadinimas"].Value.ToString()))
                {
                    textBox1.Text = row.Cells["pavadinimas"].Value.ToString();
                    textBox2.Text = row.Cells["ID"].Value.ToString();

                    string category = kategorija(row.Cells["ID"].Value.ToString());
                    if (category == "Pajamos")
                    {
                        radioButton1.Checked = true;
                    }
                    else if (category == "Išlaidos")
                    {
                        radioButton2.Checked = true;
                    }
                    else
                    {
                        radioButton1.Checked = false;
                        radioButton2.Checked = false;
                    }
                }
                else
                {
                    textBox1.Clear();
                    textBox2.Clear();
                    radioButton1.Checked = false;
                    radioButton2.Checked = false;
                }
            }
        }

        // Metodas, kuris grazina kategorija pagal ID
        private string kategorija(string id)
        {
            string category = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT category FROM pavadinimas WHERE ID = @ID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        category = result.ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Nepavyko gauti kategorijos: " + ex.Message);
                }
            }

            return category;
        }

        // Mygtuko paspaudimo ivykis, kuris istrina irašą is duomenu bazes
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cnn = new SqlConnection(connectionString))
                {
                    cnn.Open();
                    string query = "DELETE FROM pavadinimas WHERE pavadinimas=@pavadinimas";
                    using (SqlCommand delSQL = new SqlCommand(query, cnn))
                    {
                        delSQL.Parameters.AddWithValue("@pavadinimas", textBox1.Text);
                        delSQL.ExecuteNonQuery();
                    }
                    MessageBox.Show("Duomenys ištrinti iš DB!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kažkas negerai: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Mygtuko paspaudimo ivykis, kuris atnaujina esama irasa duomenu bazeje
        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(textBox2.Text))
            {
                try
                {
                    string id = textBox2.Text;
                    string query = "UPDATE pavadinimas SET pavadinimas = @pavadinimas, category = @category WHERE ID = @id";

                    using (SqlConnection cnn = new SqlConnection(connectionString))
                    {
                        SqlCommand update = new SqlCommand(query, cnn);
                        update.Parameters.Add("@pavadinimas", SqlDbType.NVarChar).Value = textBox1.Text;
                        update.Parameters.Add("@id", SqlDbType.Int).Value = id;
                        update.Parameters.Add("@category", SqlDbType.NVarChar).Value = radioButton1.Checked ? "Pajamos" : "Išlaidos";

                        cnn.Open();
                        update.ExecuteNonQuery();
                        cnn.Close();

                        MessageBox.Show("Įrašas atnaujintas sėkmingai.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Klaida atnaujinant įrašą: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Prašome įvesti klasifikatorių", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
