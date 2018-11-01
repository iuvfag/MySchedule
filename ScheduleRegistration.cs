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
    public partial class ScheduleRegistration : Form
    {

        //親フォームから情報を受け取るために使用(ログインID)
        public String userId { get; set; }
        public DateTime defaultDate { get; set; }

        public ScheduleRegistration()
        {
            InitializeComponent();
        }

        private void ScheduleRegistration_Load(object sender, EventArgs e)
        {
            scheduleDatePicker.Value = defaultDate;
        }

        //「戻る」ボタンが押された場合の動作
        private void button1_Click(object sender, EventArgs e)
        {
            //このフォームを閉じる
            this.Close();
        }

        //「登録」ボタンが押された場合の処理
        private void button2_Click(object sender, EventArgs e)
        {

            //入力欄の「件名」と「詳細」を取得
            String subject = subjectTextBox.Text;
            String detail = detailTextBox.Text;

            //「件名」と「詳細」の入力内容の確認
            String subjectCheck = InputChecker.doCheck2(subject, "件名", 250);
            if (detail.IsOverLength(1000))
            {
                //Overした時の処理
            }
            //String detailCheck = ic.doCheck3(detail, "詳細", 1000);
            String detailCheck = InputChecker.doCheck3(detail, "詳細", 1000);

            //入力内容が正しければ
            if (subjectCheck == "" && detailCheck == "")
            {
                //DAOクラスのインスタンス化
                ScheduleInfoDAO siDAO = new ScheduleInfoDAO();

                //「日付」、「開始時刻」、「終了時刻」をそれぞれ正しい形に変換して取得
                String date = scheduleDatePicker.Value.ToString("yyyy/MM/dd");
                String st = startTimePicker.Value.ToString("HH:mm");
                String et = endingTimePicker.Value.ToString("HH:mm");

                //「日付」と「開始時刻」、「終了時刻」をそれぞれ連結
                st = $"{date} {st}";
                et = $"{date} {et}";

                //「開始時刻」、「終了時刻」をDateTime型に変換
                DateTime startTime = DateTime.Parse(st);
                DateTime endingTime = DateTime.Parse(et);

                //開始時刻と終了時刻を比較しておく
                if (startTime <= endingTime)
                {

                    //スケジュールチェックのメソッドをここに追記
                    int check = siDAO.isExistsSchedule(userId, st, et);

                    //重複しているスケジュールがあるか確認
                    if (check == 0)
                    {


                        //登録用メソッドを呼び出し、結果をresultに格納
                        int result = siDAO.registSchedule(userId, startTime, endingTime, subject, detail);
                        //登録件数が1件以上あれば
                        if (result > 0)
                        {
                            //メッセージを表示し、画面を閉じる
                            MessageBox.Show("スケジュールを登録しました！", "登録完了！");
                            this.Close();
                        }
                        else
                        {
                            //何らかの理由で失敗した場合はメッセージ表示
                            MessageBox.Show("スケジュールの登録に失敗しました");
                        }
                    }
                    else
                    {
                        //重複しているスケジュールがあればメッセージ表示
                        MessageBox.Show("その時間には既に別の予定が登録されています！", "予定が重複しています！！");
                    }
                }
                else
                {
                    //開始時刻と終了時刻が前後している場合はメッセージ表示
                    MessageBox.Show("終了時刻が開始時刻よりも前に指定されています！！", "日付を確認してください！！");
                }

            }
            //入力内容が正しくない場合の分岐
            else
            {
                if (subjectCheck != "")
                {
                    //「件名」の入力内容が正しくない場合
                    MessageBox.Show(subjectCheck, "入力内容を確認してください");
                }
                if (detailCheck != "")
                {
                    //「詳細」の入力内容が正しくない場合
                    MessageBox.Show(detailCheck, "入力内容を確認してください");
                }
            }

        }

        private void ScheduleRegistration_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


    }
}
