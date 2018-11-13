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

        ScheduleInfoDAO siDAO = new ScheduleInfoDAO();

        public ScheduleDetail()
        {
            InitializeComponent();
        }

        /// <summary>
        /// このフォーム呼び出し時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleDetail_Load(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合、強制終了するためのtry-catch文
            try
            {
                //DAO、DTOクラスのインスタンス化
                ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
                ScheduleInfoDAO siDAO = new ScheduleInfoDAO();

                //スケジュールの詳細をDBから取得し、DTOクラスに格納
                siDTO = siDAO.getScheduleDetail(scheduleId);

                ////このクラスのフィールドにDTOクラスの情報を格納する
                subject = siDTO.subject;
                startTime = siDTO.startTime;
                endingTime = siDTO.endingTime;
                detail = siDTO.detail;

                //取得した情報を、対応するTextBoxに格納していく
                subjectTextBox.Text = subject;
                dateTextBox.Text = startTime.ToShortDateString();
                startTimeTextbox.Text = startTime.ToShortTimeString();
                endingTimeTextBox.Text = endingTime.ToShortTimeString();
                detailTextBox.Text = detail;
            }
            //何らかの不具合が発生した場合
            catch (Exception)
            {
                ErrorMessage.errorMessage();
            }

        }

        /// <summary>
        /// 「戻る」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 「削除」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合強制終了するためのtry-catch文
            try
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
                        //メッセージを表示する
                        MessageBox.Show("予定の削除に失敗しました", "問題が発生しました");
                        //該当する予定が存在するか確認する
                        if (!(siDAO.isExistsSchedule(scheduleId)))      //存在しない場合
                        {
                            //エラーメッセージ表示
                            MessageBox.Show("該当の予定は既に存在しません。", "予定の存在が確認できませんでした");
                        }
                        this.Close();   //いずれにしてもこの画面は閉じる
                    }
                }

            }
            //何らかの不具合が発生した場合
            catch (Exception)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.errorMessage();
            }
        }

        /// <summary>
        /// 「修正」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合、強制終了するためのtry-catch文
            try
            {
                //該当する予定が既に削除されていないかどうか確認
                if (siDAO.isExistsSchedule(scheduleId))
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
                //該当する予定の存在が確認できなかった場合
                else
                {
                    //エラーメッセージ表示
                    MessageBox.Show("該当の予定は既に存在しません。", "予定の存在が確認できませんでした");
                    this.Close();       //フォームも閉じる
                }

                
            }
            //何らかの不具合が発生した場合
            catch (Exception)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.errorMessage();
            }
        }

        private void ScheduleDetail_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }
    }
}
