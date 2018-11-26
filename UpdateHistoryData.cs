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
            //表示する編集履歴をDBから取得
            UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();
            dt = uhDAO.GetRegistHistoryData(userId);
            //データテーブルとデータグリッドをつなげる
            dataGridView1.DataSource = dt;

            //今後の作業にかかわるためソートを禁止しておく
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            label1.Text = $"{userId}さんの変更履歴";
            label1.TextAlign = ContentAlignment.MiddleCenter;

        }

        private void DataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //try
            //{
            //    Point p = dataGridView1.PointToClient(Cursor.Position);

            //    DataGridView.HitTestInfo hti = dataGridView1.HitTest(p.X, p.Y);

            //    if (hti.RowIndex >0 && hti.ColumnIndex > 0)
            //    {
                    

            //        String updateType = dataGridView1.Rows[hti.RowIndex].Cells[1].Value.ToString();
            //        int scheduleId = (int)dataGridView1.Rows[hti.RowIndex].Cells[2].Value;

            //        String ust = dataGridView1.Rows[hti.RowIndex].Cells[3].Value.ToString();
            //        String uet = dataGridView1.Rows[hti.RowIndex].Cells[4].Value.ToString();

            //        DateTime updateStartTime = DateTime.Parse(ust);
            //        DateTime updateEndingTime = DateTime.Parse(uet);
            //        String subject = dataGridView1.Rows[hti.RowIndex].Cells[5].Value.ToString();
            //        String detail = dataGridView1.Rows[hti.RowIndex].Cells[6].Value.ToString();
            //        String key = dataGridView1.Rows[hti.RowIndex].Cells[7].Value.ToString();

            //        String previousHashKey;

            //        if (hti.RowIndex == 0)
            //        {
            //            previousHashKey = "";
            //        }
            //        else
            //        {
            //            previousHashKey = dataGridView1.Rows[hti.RowIndex - 1].Cells[7].Value.ToString();
            //        }

            //        String checkKey = "";

            //        checkKey = createHashKey(userId, scheduleId, updateType, updateStartTime, updateEndingTime, subject,
            //            detail, previousHashKey);

            //        if (key == checkKey)
            //        {
            //            MessageBox.Show("履歴は正常です");
            //        }
            //        else
            //        {
            //            MessageBox.Show("履歴に不正な入力値が含まれています");
            //        }
            //    }
            //    else
            //    {

            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    throw;
            //}
        }

        /// <summary>
        /// 「」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合、強制終了するためのtry-catch文
            try
            {
                //まず、処理内容をメッセージで通知
                MessageBox.Show("不正な入力があった履歴は赤色に変化します", "履歴の照合を行います");

                //何度もボタン押下される場合を想定してフィールドを初期化しておく
                int historyId = 0;
                String updateType = "";
                int scheduleId = 0;

                String ust = "";
                String uet = "";


                String subject = "";
                String detail = "";
                String ut = "";
                String key = "";

                String previousHashKey = "";

                String checkKey = "";

                int nonce = 0;

                //データグリッドの行数だけfor文を回す
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    //DBのそれぞれの値をフィールドに格納していく
                    historyId = (int)dataGridView1.Rows[i].Cells[0].Value;          //履歴ID

                    updateType = dataGridView1.Rows[i].Cells[1].Value.ToString();   //更新内容
                    scheduleId = (int)dataGridView1.Rows[i].Cells[2].Value;         //スケジュールID

                    ust = dataGridView1.Rows[i].Cells[3].Value.ToString();          //スケジュールの開始時刻(String型)
                    uet = dataGridView1.Rows[i].Cells[4].Value.ToString();          //スケジュールの終了時刻(String型)

                    DateTime updateStartTime = DateTime.Parse(ust);                 //スケジュールの開始時刻(DateTime型)
                    DateTime updateEndingTime = DateTime.Parse(uet);                //スケジュールの終了時刻(DateTime型)
                    subject = dataGridView1.Rows[i].Cells[5].Value.ToString();      //件名
                    detail = dataGridView1.Rows[i].Cells[6].Value.ToString();       //詳細

                    ut = dataGridView1.Rows[i].Cells[7].Value.ToString();           //予定登録日時(String型)
                    DateTime updateTime = DateTime.Parse(ut);                       //予定登録日時(DateTime型)

                    nonce = (int)dataGridView1.Rows[i].Cells[8].Value;              //ノンス

                    key = dataGridView1.Rows[i].Cells[9].Value.ToString();          //ハッシュキー

                    //そのユーザーで1番最初に登録された予定は1番目に来ている
                    if (i == 0)
                    {
                        //その予定に関しては前回のハッシュキーがないため、空欄を指定
                        previousHashKey = "";
                    }
                    else
                    {
                        //それ以外の予定に関しては前回のハッシュキーを指定
                        previousHashKey = checkKey;
                    }
                    
                    //データグリッドの値をもとにハッシュキーを作成
                    checkKey = CreateHashKey(userId, scheduleId, updateType, updateStartTime, updateEndingTime, subject,
                        detail, updateTime, previousHashKey, nonce);
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
        /// RegistHistoryDAOのものと同じ
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="scheduleId">スケジュールID</param>
        /// <param name="updateType">更新した内容</param>
        /// <param name="updateStartTime">更新したスケジュールの開始時刻</param>
        /// <param name="updateEndingTime">更新したスケジュールの終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <param name="previousHashKey">前回のハッシュキー</param>
        /// <returns></returns>
        private String CreateHashKey(String userId, int scheduleId, String updateType, DateTime updateStartTime,
            DateTime updateEndingTime, String subject, String detail, DateTime updateTime, String previousHashKey, 
            int nonce)
        {
            //結果を初期化しておく
            String result = "";

            //情報を連結
            String key = $"{userId}{scheduleId}{updateType}{updateStartTime}{updateEndingTime}{subject}{detail}" +
                    $"{updateTime}{previousHashKey}{nonce}";
            //後に値を入れる配列
            byte[] hash = null;
            //連結した値をバイト変換する
            var bytes = Encoding.Unicode.GetBytes(key);
            //SHA256という形式でハッシュ関数を作る
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                //バイト変換した値をハッシュ変換する
                hash = sha256.ComputeHash(bytes);
            }
            //結果を戻す
            result = String.Join("", hash.Select(x => x.ToString("X")));

            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();
            //UpdateHistoryDTO uhDTO = new UpdateHistoryDTO();
            //List<UpdateHistoryDTO> uhDTOList = new List<UpdateHistoryDTO>();
            //List<int> emptyNonceList = new List<int>();
            //List<int> nonceList = new List<int>();
            //List<String> hashKeyList = new List<String>();
            //BlockChain bc = new BlockChain();
            //uhDTOList = uhDAO.getAllInfoWhichHasNoNonce(userId);

            //if (uhDTOList.Count >0)
            //{
            //    for (int i = 0; i < uhDTOList.Count; i++)
            //    {
            //        uhDTOList[i] = bc.Block(uhDTOList[i]);
            //        emptyNonceList.Add(uhDTOList[i].historyId);
            //        nonceList.Add(uhDTOList[i].nonce);
            //        hashKeyList.Add(uhDTOList[i].hashKey);
            //    }
            //    uhDAO.UpdateNonce(emptyNonceList, nonceList, hashKeyList);

            //    dt = uhDAO.GetRegistHistoryData(userId);
            //    //データテーブルとデータグリッドをつなげる
            //    dataGridView1.DataSource = dt;

            //    //今後の作業にかかわるためソートを禁止しておく
            //    foreach (DataGridViewColumn c in dataGridView1.Columns)
            //    {
            //        c.SortMode = DataGridViewColumnSortMode.NotSortable;
            //    }
            //}

            NonceAndKeyCheck nkc = new NonceAndKeyCheck();
            nkc.userId = userId;
            nkc.Show(this);

        }

        private void UpdateHistoryData_Activated(object sender, EventArgs e)
        {
            //表示する編集履歴をDBから取得
            UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();
            dt = uhDAO.GetRegistHistoryData(userId);
            //データテーブルとデータグリッドをつなげる
            dataGridView1.DataSource = dt;

            //今後の作業にかかわるためソートを禁止しておく
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

        }
    }
}
