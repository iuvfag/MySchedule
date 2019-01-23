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
    public partial class ScheduleChoise : Form
    {

        public String userId { get; set; }
        public String startTime { get; set; }
        public String endingTime { get; set; }

        public ScheduleChoise()
        {
            InitializeComponent();
        }

        /// <summary>
        /// フォーム呼び出し時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleChoise_Load(object sender, EventArgs e)
        {
            //ログインIDと開始時刻、終了時刻から予定情報を取得するメソッドの呼び出し
            ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
            //結果として帰ってくるデータテーブルとデータグリッドを接続
            dataGridView1.DataSource = siDAO.getDuplicatedScheduleInfo(userId, startTime, endingTime);
            //セルの最初の行(スケジュールIDが格納されている)を表示にする
            dataGridView1.Columns[0].Visible = false;
        }

        /// <summary>
        /// セルがダブルクリックされた場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //クリックされた場所の取得
            Point p = dataGridView1.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = dataGridView1.HitTest(p.X, p.Y);
            //クリックされた場所がヘッダー等でないことを確認
            if (hti.RowIndex > -1 && hti.ColumnIndex > -1)
            {
                ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
                siDTO.userId = userId;           //ログインIDを格納
                //セルの最初の列(隠れているが、スケジュールIDが格納されている)の値を取得し、DTOに格納
                siDTO.scheduleId = (int)dataGridView1.Rows[hti.RowIndex].Cells[0].Value;
                //ログインIDとスケジュールIDを渡して詳細画面を開く
                ScheduleDetail sd = new ScheduleDetail()
                {
                    siDTO = siDTO           //DTOクラスを渡す
                };
                sd.ShowDialog(this);
                sd.Dispose();
            }
        }

        private void ScheduleChoise_Activated(object sender, EventArgs e)
        {
            //ログインIDと開始時刻、終了時刻から予定情報を取得するメソッドの呼び出し
            ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
            //結果として帰ってくるデータテーブルとデータグリッドを接続
            dataGridView1.DataSource = siDAO.getDuplicatedScheduleInfo(userId, startTime, endingTime);
            //セルの最初の行(スケジュールIDが格納されている)を表示にする
            dataGridView1.Columns[0].Visible = false;
        }
    }
}
