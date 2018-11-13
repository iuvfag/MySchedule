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
    public partial class ScheduleUpdate : Form
    {
        //親フォームとの情報のやり取りに使用
        public String userId { get; set; }
        public int scheduleId { get; set; }
        public String subject { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endingTime { get; set; }
        public String detail { get; set; }



        public ScheduleUpdate()
        {
            InitializeComponent();
        }

        /// <summary>
        /// このフォームの呼び出し時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleUpdate_Load(object sender, EventArgs e)
        {
            //詳細画面から受け取った値をそのまま表示していく
            //予定日時(開始時刻の日付部分だけに変換して表示)
            scheduleDatePicker.Value = DateTime.Parse(startTime.ToShortDateString());
            //開始時刻
            startTimePicker.Value = startTime;
            //終了時刻
            endingTimePicker.Value = endingTime;
            //件名
            subjectTextbox.Text = subject;
            //詳細
            detailTextbox.Text = detail;
        }

        /// <summary>
        /// 「戻る」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合強制終了するためのtry-catch文
            try
            {
                //このフォームを閉じる
                this.Close();
            }
            catch (Exception)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.ApplicationClose();
            }

        }

        /// <summary>
        /// 「更新」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button2_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合強制終了させるtry-catch文
            try
            {
                //「件名」、「詳細」を入力欄から取得してフィールドに格納
                subject = subjectTextbox.Text;
                detail = detailTextbox.Text;

                //「件名」、「詳細」の入力チェック
                String subjectCheck = subject.DoCheck2("件名", 250);
                String detailCheck = detail.DoCheck3("詳細", 1000);

                //入力欄に問題がなければ次の処理へ
                if (String.IsNullOrWhiteSpace(subjectCheck) && String.IsNullOrWhiteSpace(detailCheck))
                {
                    //日付には開始時刻の日付部分を切り出して格納
                    String date = scheduleDatePicker.Value.ToShortDateString();
                    //開始時刻と終了時刻は時刻部分を切り出して格納
                    String st = startTimePicker.Value.ToString("HH:mm");
                    String et = endingTimePicker.Value.ToString("HH:mm");

                    //切り出した日付と時刻を連結させる
                    st = $"{date} {st}";
                    et = $"{date} {et}";

                    //このクラスのフィールドに格納しておく
                    this.startTime = DateTime.Parse(st);
                    this.endingTime = DateTime.Parse(et);

                    //先ほど連結した時刻で開始時刻と終了時刻が前後していないかどうか調べる
                    if (startTime <= endingTime)
                    {
                        //DAOクラスのインスタンス化
                        ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
                        //スケジュールの更新を行い結果(更新数)をresultに格納
                        int result = siDAO.UpdeteSchedule(userId, scheduleId, startTime, endingTime, subject, detail);

                        //更新されたスケジュールが1件でも存在すれば次の処理へ
                        if (result > 0)
                        {
                            //メッセージ表示
                            MessageBox.Show("スケジュール情報を修正しました", "更新完了");

                            RegistHistoryDAO rhDAO = new RegistHistoryDAO();
                            rhDAO.RegistHistory(userId, scheduleId, "スケジュール修正", startTime, endingTime, subject, detail);

                            this.Close();
                        }
                        //更新できなかった場合
                        else
                        {
                            //メッセージ表示
                            MessageBox.Show("スケジュールの更新に失敗しました", "問題が発生しました");
                            this.Close();
                        }

                    }
                    //開始時刻と終了時刻が前後している場合
                    else
                    {
                        MessageBox.Show("終了時刻が開始時刻よりも前に指定されています！！", "日付を確認してください！！");
                    }
                }
                //以下入力内容に問題があった場合
                //「件名」入力内容に問題があった場合
                else if (!(String.IsNullOrWhiteSpace(subjectCheck)))
                {
                    //メッセージ表示
                    MessageBox.Show(subjectCheck, "入力内容を確認してください");
                }
                //「詳細」入力内容に問題があった場合
                else if (String.IsNullOrWhiteSpace(detailCheck))
                {
                    //メッセージ表示
                    MessageBox.Show(detailCheck, "入力内容を確認してください");
                }
            }
            catch (Exception)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.ApplicationClose();
            }

        }

    }

}
