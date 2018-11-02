using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace MySchedule
{
    public partial class UpdateHistoryData : Form
    {

        public String userId { get; set; }
        DataTable dt = new DataTable();

        public UpdateHistoryData()
        {
            InitializeComponent();
        }

        private void UpdateHistoryData_Load(object sender, EventArgs e)
        {
            RegistHistoryDAO rhDAO = new RegistHistoryDAO();
            dt = rhDAO.getRegistHistoryData(userId);
            dataGridView1.DataSource = dt;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Point p = dataGridView1.PointToClient(Cursor.Position);

                DataGridView.HitTestInfo hti = dataGridView1.HitTest(p.X, p.Y);

                String updateType = dataGridView1.Rows[hti.RowIndex].Cells[1].Value.ToString();
                int scheduleId = (int)dataGridView1.Rows[hti.RowIndex].Cells[2].Value;

                String ust = dataGridView1.Rows[hti.RowIndex].Cells[3].Value.ToString();
                String uet = dataGridView1.Rows[hti.RowIndex].Cells[4].Value.ToString();

                DateTime updateStartTime = DateTime.Parse(ust);
                DateTime updateEndingTime = DateTime.Parse(uet);
                String subject = dataGridView1.Rows[hti.RowIndex].Cells[5].Value.ToString();
                String detail = dataGridView1.Rows[hti.RowIndex].Cells[6].Value.ToString();
                String key = dataGridView1.Rows[hti.RowIndex].Cells[7].Value.ToString();

                String previousHashKey;

                if (hti.RowIndex == 0)
                {
                    previousHashKey = "";
                }
                else
                {
                    previousHashKey = dataGridView1.Rows[hti.RowIndex - 1].Cells[7].Value.ToString();
                }
                String checkKey = $"{userId}{scheduleId}{updateType}{updateStartTime}{updateEndingTime}{subject}{detail}" +
                    $"{previousHashKey}";

                byte[] hash = null;
                var bytes = Encoding.Unicode.GetBytes(checkKey);

                using (var sha256 = new SHA256CryptoServiceProvider())
                {
                    hash = sha256.ComputeHash(bytes);
                }
                checkKey = String.Join("", hash.Select(x => x.ToString()));

                if (key == checkKey)
                {
                    MessageBox.Show("履歴は正常です");
                }
                else
                {
                    MessageBox.Show("履歴が改ざんされている恐れがあります");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }
    }
}
