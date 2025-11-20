using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace shopDB
{
    public partial class Form2 : Form
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=tooded;Integrated Security=True");
        SqlDataAdapter adapter;
        DataTable dtProducts;

        decimal userBalance;
        string moneyFile = "money.txt";

        ComboBox comboProducts;
        NumericUpDown numQuantity;
        Label lblPrice, lblBalance;
        PictureBox pbProduct;
        Button btnPay;

        byte[] imageData;

        public Form2()
        {
            InitializeComponent();
            DesignForm();

            userBalance = LoadMoney();
            lblBalance.Text = $"Saldo: {userBalance} €";

            LoadProducts();
        }

        private decimal LoadMoney()
        {
            if (!File.Exists(moneyFile))
                File.WriteAllText(moneyFile, "10"); // start 10eur

            return decimal.Parse(File.ReadAllText(moneyFile));
        }

        private void SaveMoney(decimal money)
        {
            File.WriteAllText(moneyFile, money.ToString());
        }

        private void DesignForm()
        {
            this.Text = "Tellimuse vormistamine";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 245);

            Label lblSelect = new Label { Text = "Vali toode:", Location = new Point(30, 30), Width = 120 };
            comboProducts = new ComboBox { Location = new Point(160, 25), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            comboProducts.SelectedIndexChanged += ComboProducts_SelectedIndexChanged;

            Label lblQty = new Label { Text = "Kogus:", Location = new Point(30, 70), Width = 120 };
            numQuantity = new NumericUpDown { Location = new Point(160, 65), Width = 100, Minimum = 1, Maximum = 100 };
            numQuantity.ValueChanged += NumQuantity_ValueChanged;

            lblPrice = new Label { Text = "Hind: 0 €", Location = new Point(30, 110), Width = 300, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            lblBalance = new Label { Text = "Saldo: 0 €", Location = new Point(30, 140), Width = 300, Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            pbProduct = new PictureBox
            {
                Location = new Point(450, 25),
                Size = new Size(250, 250),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            btnPay = new Button
            {
                Text = "Maksma",
                Location = new Point(30, 200),
                Size = new Size(150, 40),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPay.FlatAppearance.BorderSize = 0;
            btnPay.Click += BtnPay_Click;

            this.Controls.AddRange(new Control[] {
                lblSelect, comboProducts, lblQty, numQuantity,
                lblPrice, lblBalance, pbProduct, btnPay
            });
        }

        private void LoadProducts()
        {
            connect.Open();
            dtProducts = new DataTable(); // object klass
            adapter = new SqlDataAdapter("SELECT Id, Toodenimetus, Kogus, Hind, Bpilt FROM Toodetabel", connect);
            adapter.Fill(dtProducts);
            connect.Close();

            comboProducts.DataSource = dtProducts;
            comboProducts.DisplayMember = "Toodenimetus";
            comboProducts.ValueMember = "Id";
        }

        private void ComboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboProducts.SelectedIndex == -1) return;

            DataRow row = ((DataRowView)comboProducts.SelectedItem).Row;
            decimal price = Convert.ToDecimal(row["Hind"]);
            int stock = Convert.ToInt32(row["Kogus"]);

            lblPrice.Text = $"Hind: {price * numQuantity.Value} € (saadaval: {stock})";

            if (row["Bpilt"] != DBNull.Value)
            {
                imageData = (byte[])row["Bpilt"];
                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    pbProduct.Image = Image.FromStream(ms);
                }
            }
            else
            {
                pbProduct.Image = null;
                imageData = null;
            }
        }

        private void NumQuantity_ValueChanged(object sender, EventArgs e)
        {
            ComboProducts_SelectedIndexChanged(null, null);
        }

        private void BtnPay_Click(object sender, EventArgs e)
        {
            if (comboProducts.SelectedIndex == -1) return;

            DataRow row = ((DataRowView)comboProducts.SelectedItem).Row;
            decimal price = Convert.ToDecimal(row["Hind"]);
            int stock = Convert.ToInt32(row["Kogus"]);
            int qty = (int)numQuantity.Value;

            if (qty > stock)
            {
                MessageBox.Show("Ladu ei sisalda nii palju kaupa!");
                return;
            }

            decimal totalPrice = price * qty;

            if (totalPrice > userBalance)
            {
                MessageBox.Show("Saldo ei piisa!");
                return;
            }

            userBalance -= totalPrice;
            SaveMoney(userBalance);
            lblBalance.Text = $"Saldo: {userBalance} €";

            connect.Open();
            SqlCommand cmd = new SqlCommand(
                "UPDATE Toodetabel SET Kogus = Kogus - @qty WHERE Id = @id", connect);
            cmd.Parameters.AddWithValue("@qty", qty);
            cmd.Parameters.AddWithValue("@id", row["Id"]);
            cmd.ExecuteNonQuery();
            connect.Close();

            row["Kogus"] = stock - qty;

            LoadProducts();

            MessageBox.Show($"Ostsite {qty} tk toodet '{row["Toodenimetus"]}' hinnaga {totalPrice} €");
        }
    }
}
