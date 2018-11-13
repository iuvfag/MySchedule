using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
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

        /// <summary>
        /// フォームを読み込んだ際の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllSchedules_Load(object sender, EventArgs e)
        {
            //ログインIDをもとにすべてのスケジュールを取得し、データテーブルに格納する
            ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
            //格納したデータテーブルとデータグリッドを紐づける
            dataGridView1.DataSource = siDAO.GetAllSchedule(userId);
            dataGridView1.Columns[0].Visible = false;   //1行目は見えなくする(スケジュールID)
            //カラムの幅設定など
            dataGridView1.Columns[1].Width = 80;
            dataGridView1.Columns[2].Width = 80;
            dataGridView1.Columns[3].Width = 80;
            dataGridView1.Columns[4].Width = 120;
        }

        /// <summary>
        /// セルをダブルクリックした際の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //クリックされたセルを調べる
            Point p = dataGridView1.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = dataGridView1.HitTest(p.X, p.Y);

            //クリックされたセルのインデックスが不正でないことの確認
            if (hti.RowIndex > -1 && hti.ColumnIndex > -1)
            {
                //インデックス[0]番目のセルのスケジュールIDを取得し、変数に格納
                int scheduleId = (int)dataGridView1.Rows[hti.RowIndex].Cells[0].Value;
                //クリックされたセルから取得されたスケジュールIDが正しいものであれば次の処理へ
                if (scheduleId != 0 && scheduleId.ToString() != null)
                {
                    //スケジュール詳細画面を開く
                    ScheduleDetail sd = new ScheduleDetail();
                    sd.userId = userId;             //ログインIDを渡す
                    sd.scheduleId = scheduleId;     //スケジュールIDを渡す

                    sd.ShowDialog(this);
                    sd.Dispose();
                }

            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合、強制終了するためのtry-catch文
            try
            {
                //まず、処理内容をメッセージで通知
                MessageBox.Show("不正な入力があった履歴は赤色に変化します", "履歴の照合を行います");

                //何度もボタン押下される場合を想定してフィールドを初期化しておく
                String date = "";
                String st = "";
                String et = "";


                String subject = "";
                String detail = "";
                String key = "";

                String checkKey = "";

                //データグリッドの行数だけfor文を回す
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    //DBのそれぞれの値をフィールドに格納していく
                    
                    date = dataGridView1.Rows[i].Cells[1].Value.ToString();         //スケジュールの日程(String型)
                    date = date.Substring(0, 10);                                   //日付部分のみ切り出し
                    st = dataGridView1.Rows[i].Cells[2].Value.ToString();           // スケジュールの開始時刻(String型)
                    et = dataGridView1.Rows[i].Cells[3].Value.ToString();           //スケジュールの終了時刻(String型)
                    st = $"{date} {st}";                                             //開始時刻連結
                    et = $"{date} {et}";                                             //終了時刻連結

                    DateTime startTime = DateTime.Parse(st);                 //スケジュールの開始時刻(DateTime型に変換)
                    DateTime endingTime = DateTime.Parse(et);                //スケジュールの終了時刻(DateTime型に変換)
                    subject = dataGridView1.Rows[i].Cells[4].Value.ToString();      //件名
                    detail = dataGridView1.Rows[i].Cells[5].Value.ToString();       //詳細
                    key = dataGridView1.Rows[i].Cells[6].Value.ToString();          //ハッシュキー

                    //データグリッドの値をもとにハッシュキーを作成
                    checkKey = CreateHashKey(userId, startTime, endingTime, subject, detail);
                    //現在表示されているものと比較
                    if (key != checkKey)
                    {
                        //値が異なるようならその行の背景色を赤色にする
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }
                   
                }
                //処理の終了をメッセージで表示
                MessageBox.Show("履歴の照合が完了しました");
            }
            //何らかの不具合が発生した場合
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        /// ハッシュキー作成用のメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="startTime">開始時刻</param>
        /// <param name="endingTime">終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>作成したハッシュキーを格納したString型の変数</returns>
        private String CreateHashKey(String userId, DateTime startTime, DateTime endingTime, String subject,
            String detail)
        {
            //まず、引数をすべて連結
            String key = $"{userId}{startTime}{endingTime}{subject}{detail}";

            //のちに値を入れる配列
            byte[] hash = null;
            //連結した値をバイト変換する
            var bytes = Encoding.Unicode.GetBytes(key);
            //SHA256という形式でハッシュ変換する
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                //バイト変換した値でハッシュ変換する
                hash = sha256.ComputeHash(bytes);
            }
            //結果を戻すs
            String result = String.Join("", hash.Select(x => x.ToString("X")));
            return result;
        }

    }
}
