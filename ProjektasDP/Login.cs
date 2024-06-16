using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ProjektasDP
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        // Mygtuko paspaudimo ivykis
        private void button1_Click(object sender, EventArgs e)
        {
            // Gauname vartotojo ivestus prisijungimo duomenis
            string username = textBox1.Text;
            string password = textBox2.Text;

            // Sukuriame prisijungimo prie duomenu bazes
            string connectionString = "Data Source=DESKTOP-HRV85Q6\\SQLEXPRESS02;Initial Catalog=projektas;User ID=sa;Password=123456";

            // Naudojame 'using', kad automatiskai uxdarytume rysi po naudojimo
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Uzklausa, kuri patikrina, ar egzistuoja vartotojas su ivestais duomenimis
                string query = "SELECT COUNT(*) FROM login WHERE username = @username AND password = @password";
                SqlCommand command = new SqlCommand(query, connection);

                // Priskiriame parametrus uzklausai, kad isvengtume SQL injekcijos
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                
                connection.Open();
                // Vykdome uzklausa ir gauname rezultata
                int count = (int)command.ExecuteScalar();
               
                connection.Close();

                // Tikriname, ar yra bent vienas vartotojas su ivestais duomenimis
                if (count == 1)
                {
                    // Jei prisijungimas sekmingas, atidarome pagrindinę formą
                    Form1 form1 = new Form1();
                    this.Hide();
                    form1.Show();
                }
                else
                {
                    // Jei prisijungimas nepavyko, rodome klaidos pranesima
                    MessageBox.Show("Neteisingas vartotojo vardas arba slaptažodis. Prašome bandyti dar kartą.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
