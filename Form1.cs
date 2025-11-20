using System.Data;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace shopDB
{
    public partial class Form1 : Form
    {
        SqlConnection connect = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=tooded;Integrated Security=True");
        SqlCommand command;
        SqlDataAdapter adapter;
        OpenFileDialog open;
        byte[] imageData;

        TextBox toodeTextB, kogusTextB, hindTextB, kat_textBox;
        ComboBox kat_box;
        PictureBox toode_pb;
        DataGridView dataGridView1;
        Button lisaBtn, uuendaBtn, kustutaBtn, otsifailBtn, puhastaBtn;

        public Form1()
        {
            InitializeComponent();
            DesignForm();
            NaitaKategooriad();
            NaitaAndmed();
        }

        private void DesignForm()
        {
            this.Text = "Pood (Administraator)";
            this.Size = new Size(1100, 830);
            this.BackColor = Color.FromArgb(240, 240, 245);
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;

            int labelWidth = 90;
            int inputWidth = 220;
            int startX = 30;
            int startY = 30;
            int space = 50;

            Label l1 = new Label { Text = "Toode", Location = new Point(startX, startY), Width = labelWidth };
            toodeTextB = new TextBox { Location = new Point(startX + labelWidth + 10, startY - 3), Width = inputWidth };

            Label l2 = new Label { Text = "Kogus", Location = new Point(startX, startY + space), Width = labelWidth };
            kogusTextB = new TextBox { Location = new Point(startX + labelWidth + 10, startY + space - 3), Width = inputWidth };

            Label l3 = new Label { Text = "Hind", Location = new Point(startX, startY + space * 2), Width = labelWidth };
            hindTextB = new TextBox { Location = new Point(startX + labelWidth + 10, startY + space * 2 - 3), Width = inputWidth };

            Label l4 = new Label { Text = "Kategooria", Location = new Point(startX, startY + space * 3), Width = labelWidth };
            kat_box = new ComboBox { Location = new Point(startX + labelWidth + 10, startY + space * 3 - 5), Width = inputWidth, DropDownStyle = ComboBoxStyle.DropDownList };

            kat_textBox = new TextBox
            {
                Location = new Point(kat_box.Left, kat_box.Bottom + 5),
                Width = inputWidth,
                PlaceholderText = "Uus kategooria (valikuline)"
            };

            toode_pb = new PictureBox
            {
                Location = new Point(startX + labelWidth + inputWidth + 60, startY),
                Size = new Size(200, 200),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            otsifailBtn = new Button
            {
                Text = "Vali pilt",
                Location = new Point(toode_pb.Left, toode_pb.Bottom + 10),
                Width = toode_pb.Width,
                BackColor = Color.MediumSlateBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            otsifailBtn.Click += OtsifailBtn_Click;

            lisaBtn = NewBtn("Lisa", Color.SeaGreen, startX, startY + space * 4 + 40);
            lisaBtn.Click += LisaBtn_Click;

            uuendaBtn = NewBtn("Uuenda", Color.DarkOrange, startX + 130, startY + space * 4 + 40);
            uuendaBtn.Click += UuendaBtn_Click;

            kustutaBtn = NewBtn("Kustuta", Color.Crimson, startX + 260, startY + space * 4 + 40);
            kustutaBtn.Click += KustutaBtn_Click;

            puhastaBtn = NewBtn("Tühjenda", Color.Gray, startX + 390, startY + space * 4 + 40);
            puhastaBtn.Click += PuhastaBtn_Click;

            dataGridView1 = new DataGridView
            {
                Location = new Point(startX, startY + space * 4 + 120),
                Size = new Size(950, 360),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                RowTemplate = { Height = 35 },
                AllowUserToAddRows = false
            };
            dataGridView1.CellClick += DataGridView1_CellClick;

            Button orderBtn = new Button
            {
                Text = "Tee tellimus (Klient)",
                Size = new Size(200, 40),
                Location = new Point(dataGridView1.Left, dataGridView1.Bottom + 10),
                BackColor = Color.MediumVioletRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            orderBtn.Click += OrderBtn_Click;

            this.Controls.AddRange(new Control[] {
                l1, toodeTextB, l2, kogusTextB, l3, hindTextB, l4, kat_box, kat_textBox,
                toode_pb, otsifailBtn,
                lisaBtn, uuendaBtn, kustutaBtn, puhastaBtn,
                dataGridView1, orderBtn
            });
        }

        private Button NewBtn(string text, Color color, int x, int y)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(110, 35),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
        }

        private void OtsifailBtn_Click(object sender, EventArgs e)
        {
            open = new OpenFileDialog();
            open.Filter = "Pildid|*.jpg;*.jpeg;*.png;*.bmp";

            if (open.ShowDialog() == DialogResult.OK)
            {
                toode_pb.Image = Image.FromFile(open.FileName);
                using (MemoryStream ms = new MemoryStream())
                {
                    toode_pb.Image.Save(ms, toode_pb.Image.RawFormat);
                    imageData = ms.ToArray();
                }
            }
        }

        private int GetCategoryId(string name)
        {
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Kategooria WHERE Kategooria_nimetus=@n", connect);
            cmd.Parameters.AddWithValue("@n", name);

            object res = cmd.ExecuteScalar();
            if (res != null) return (int)res;

            SqlCommand add = new SqlCommand("INSERT INTO Kategooria (Kategooria_nimetus) OUTPUT INSERTED.Id VALUES (@n)", connect);
            add.Parameters.AddWithValue("@n", name);
            int newId = (int)add.ExecuteScalar();
            NaitaKategooriad();
            return newId;
        }

        private void LisaBtn_Click(object sender, EventArgs e)
        {
            if (toodeTextB.Text == "" || kogusTextB.Text == "" || hindTextB.Text == "")
            {
                MessageBox.Show("Täida kõik väljad!");
                return;
            }

            connect.Open();

            string kat = (kat_textBox.Text.Trim() != "")
                ? kat_textBox.Text.Trim()
                : kat_box.Text;

            int katId = GetCategoryId(kat);

            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Toodetabel (Toodenimetus, Kogus, Hind, Kategooriad, Bpilt) VALUES (@t,@k,@h,@kat,@p)",
                connect);

            cmd.Parameters.AddWithValue("@t", toodeTextB.Text);
            cmd.Parameters.AddWithValue("@k", int.Parse(kogusTextB.Text));
            cmd.Parameters.AddWithValue("@h", int.Parse(hindTextB.Text));
            cmd.Parameters.AddWithValue("@kat", katId);
            cmd.Parameters.AddWithValue("@p", imageData ?? (object)DBNull.Value);
            cmd.ExecuteNonQuery();

            connect.Close();
            NaitaAndmed();
            ClearFields();
        }

        private void UuendaBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            int id = (int)dataGridView1.CurrentRow.Cells["Id"].Value;

            connect.Open();

            string kat = (kat_textBox.Text.Trim() != "")
                ? kat_textBox.Text.Trim()
                : kat_box.Text;

            int katId = GetCategoryId(kat);

            SqlCommand cmd = new SqlCommand(
                "UPDATE Toodetabel SET Toodenimetus=@t, Kogus=@k, Hind=@h, Kategooriad=@kat, Bpilt=@p WHERE Id=@id",
                connect);

            cmd.Parameters.AddWithValue("@t", toodeTextB.Text);
            cmd.Parameters.AddWithValue("@k", int.Parse(kogusTextB.Text));
            cmd.Parameters.AddWithValue("@h", int.Parse(hindTextB.Text));
            cmd.Parameters.AddWithValue("@kat", katId);
            cmd.Parameters.AddWithValue("@p", imageData ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();

            connect.Close();

            NaitaAndmed();
            ClearFields();
        }

        private void KustutaBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            int id = (int)dataGridView1.CurrentRow.Cells["Id"].Value;

            connect.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM Toodetabel WHERE Id=@id", connect);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            connect.Close();

            NaitaAndmed();
            ClearFields();
        }

        private void PuhastaBtn_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void NaitaKategooriad()
        {
            using (SqlConnection conn = new SqlConnection(connect.ConnectionString))
            {
                conn.Open();
                DataTable dt = new DataTable();
                adapter = new SqlDataAdapter("SELECT * FROM Kategooria", conn);
                adapter.Fill(dt);

                kat_box.DataSource = dt;
                kat_box.DisplayMember = "Kategooria_nimetus";
                kat_box.ValueMember = "Id";
            }
        }


        private void NaitaAndmed()
        {
            connect.Open();
            DataTable dt = new DataTable();
            adapter = new SqlDataAdapter("SELECT Id, Toodenimetus, Kogus, Hind, Kategooriad, Bpilt FROM Toodetabel", connect);
            adapter.Fill(dt);
            connect.Close();

            DataTable dtDisplay = dt.Clone();
            dtDisplay.Columns["Bpilt"].DataType = typeof(Image);

            foreach (DataRow row in dt.Rows)
            {
                DataRow newRow = dtDisplay.NewRow();
                newRow["Id"] = row["Id"];
                newRow["Toodenimetus"] = row["Toodenimetus"];
                newRow["Kogus"] = row["Kogus"];
                newRow["Hind"] = row["Hind"];
                newRow["Kategooriad"] = row["Kategooriad"];

                if (row["Bpilt"] != DBNull.Value)
                {
                    byte[] imgBytes = (byte[])row["Bpilt"];
                    using (MemoryStream ms = new MemoryStream(imgBytes))
                    {
                        newRow["Bpilt"] = Image.FromStream(ms);
                    }
                }
                else
                {
                    newRow["Bpilt"] = null;
                }

                dtDisplay.Rows.Add(newRow);
            }

            dataGridView1.Columns.Clear();
            dataGridView1.DataSource = dtDisplay;

            if (!dataGridView1.Columns.Contains("Bpilt"))
                return;

            DataGridViewImageColumn imgCol = (DataGridViewImageColumn)dataGridView1.Columns["Bpilt"];
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
            dataGridView1.RowTemplate.Height = 80;
        }


        private void ClearFields()
        {
            toodeTextB.Clear();
            kogusTextB.Clear();
            hindTextB.Clear();
            kat_textBox.Clear();
            kat_box.SelectedIndex = -1;
            toode_pb.Image = null;
            imageData = null;
        }

        private void OrderBtn_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            int id = (int)dataGridView1.CurrentRow.Cells["Id"].Value;

            connect.Open();
            SqlCommand cmd = new SqlCommand("SELECT * FROM Toodetabel WHERE Id=@id", connect);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataReader r = cmd.ExecuteReader();

            if (r.Read())
            {
                toodeTextB.Text = r["Toodenimetus"].ToString();
                kogusTextB.Text = r["Kogus"].ToString();
                hindTextB.Text = r["Hind"].ToString();

                kat_box.SelectedValue = r["Kategooriad"];
                kat_textBox.Text = kat_box.Text;

                if (r["Bpilt"] != DBNull.Value)
                {
                    byte[] img = (byte[])r["Bpilt"];
                    using (MemoryStream ms = new MemoryStream(img))
                    {
                        toode_pb.Image = Image.FromStream(ms);
                    }
                    imageData = img;
                }
                else
                {
                    toode_pb.Image = null;
                    imageData = null;
                }
            }

            connect.Close();
        }
    }
}
