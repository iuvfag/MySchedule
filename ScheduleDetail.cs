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
    public partial class ScheduleDetail : Form
    {
        //週間スケジュール画面から受け取る情報
        public String userId { get; set; }
        public int scheduleId { get; set; }
        public String subject { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endingTime { get; set; }
        public String detail { get; set; }

        public ScheduleDetail()
        {
            InitializeComponent();
        }

        private void ScheduleDetail_Load(object sender, EventArgs e)
        {
            //DAO、DTOクラスのインスタンス化
            ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
            ScheduleInfoDAO siDAO = new ScheduleInfoDAO();

            //スケジュールの詳細をDBから取得し、DTOクラスに格納
            //siDTO = siDAO.getScheduleDetail(scheduleId);

            ////このクラスのフィールドにDTOクラスの情報を格納する
            //subject = siDTO.subject;
            //startTime = siDTO.startTime;
            //endingTime = siDTO.endingTime;
            //detail = siDTO.detail;

            //取得した情報を、対応するTextBoxに格納していく
            subjectTextBox.Text = subject;
            dateTextBox.Text = startTime.ToShortDateString();
            startTimeTextbox.Text = startTime.ToShortTimeString();
            endingTimeTextBox.Text = endingTime.ToShortTimeString();
            detailTextBox.Text = detail;

        }

        //「戻る」ボタンが押された場合の動作
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //「削除」ボタンが押された場合の動作
        private void button2_Click(object sender, EventArgs e)
        {
            //YesNoダイアログの作成
            DialogResult dr = MessageBox.Show("この予定を削除しますか？", "確認", MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);

            //押下するボタンによって処理を分岐させる
            if (dr == DialogResult.Yes)
            {
                
                RegistHistoryDAO rhDAO = new RegistHistoryDAO();
                rhDAO.registHistory(userId, scheduleId, "スケジュール削除", startTime, endingTime, subject, detail);

                //Yesの場合はDAOの削除メソッドを呼び出し、resultに結果(削除件数)を代入
                ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
                int result = siDAO.deleteSchedule(scheduleId);

                //きちんと削除されたか(削除件数が1件でもあるか)を確認
                //1件でも存在すれば
                if (result > 0)
                {
                    //メッセージを表示して、フォームを閉じる
                    MessageBox.Show("予定を削除しました", "削除完了");
                    this.Close();
                }
                //1件も削除できなければ
                else
                {
                    //メッセージを表示して、フォームを閉じる
                    MessageBox.Show("予定の削除に失敗しました", "問題が発生しました");
                    this.Close();
                }
            }

        }

        //「修正」ボタンが押された場合の動作
        private void button3_Click(object sender, EventArgs e)
        {
            //スケジュール修正フォームの呼び出し
            ScheduleUpdate su = new ScheduleUpdate();

            //値を渡していく
            su.userId = userId;
            su.scheduleId = scheduleId;
            su.subject = subject;
            su.startTime = startTime;
            su.endingTime = endingTime;
            su.detail = detail;

            //フォームの表示
            su.ShowDialog(this);
            su.Dispose();
            this.Close();
        }
    }
}
