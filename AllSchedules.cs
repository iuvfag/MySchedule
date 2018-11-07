using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySchedule
{
    public partial class AllSchedules : Form
    {

        public String userId { get; set; }

        public AllSchedules()
        {
            InitializeComponent();
        }

        private void AllSchedules_Load(object sender, EventArgs e)
        {
            ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
            dataGridView1.DataSource = siDAO.getAllSchedule(userId);
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 120;
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            Point p = dataGridView1.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = dataGridView1.HitTest(p.X, p.Y);

            if (hti.RowIndex > -1 && hti.ColumnIndex > -1)
            {
                int scheduleId = (int)dataGridView1.Rows[hti.RowIndex].Cells[0].Value;

                if (scheduleId != 0 && scheduleId.ToString() != null)
                {
                    ScheduleDetail sd = new ScheduleDetail();
                    sd.userId = userId;
                    sd.scheduleId = scheduleId;

                    sd.ShowDialog(this);
                    sd.Dispose();
                }
                
            }
        }
    }
}
