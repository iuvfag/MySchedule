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
        public bool flg { get; set; }
        public DateTime start { get; set; }
        public DateTime ending { get; set; }
        internal ScheduleInfoDTO siDTO { get; set; }

        public ScheduleRegistration()
        {
            InitializeComponent();
        }

        /// <summary>
        /// このフォーム呼び出し時の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleRegistration_Load(object sender, EventArgs e)
        {
            scheduleDatePicker.Value = defaultDate;

            //flgがtrue(週間スケジュールから飛んできた場合)
            if (flg)
            {
                //データグリッドに選択された開始時刻と終了時刻をこちらの入力欄に反映させる
                startTimePicker.Value = start;
                endingTimePicker.Value = ending;
            }
        }

        /// <summary>
        /// 「戻る」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            //このフォームを閉じる
            this.Close();
        }

        /// <summary>
        /// 「登録」ボタンが押された場合の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button2_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合強制終了するためのtry-catch文
            try
            {

                //入力欄の「件名」と「詳細」を取得
                String subject = subjectTextBox.Text;
                String detail = detailTextBox.Text;

                //「件名」と「詳細」の入力内容の確認
                String subjectCheck = subject.DoCheck2("件名", 250);
                String detailCheck = detail.DoCheck3("詳細", 1000);

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

                //入力内容が正しく、開始時刻と終了時刻が前後していなければ
                if (String.IsNullOrWhiteSpace(subjectCheck) && String.IsNullOrWhiteSpace(detailCheck) &&
                    startTime <= endingTime)
                {
                    //DAOクラスのインスタンス化
                    ScheduleInfoDAO siDAO = new ScheduleInfoDAO();

                    //登録用メソッドを呼び出し、結果をresultに格納
                    int result = siDAO.RegistSchedule(userId, startTime, endingTime, subject, detail, "");
                    //登録件数が1件以上あれば
                    if (result > 0)
                    {
                        //スケジュールIDを取得して
                        int scheduleId = siDAO.GetScheduleInfomation(userId, startTime, endingTime, subject, detail);

                        //処理に時間がかかるため、マルチスレッド処理を行う
                        var task = Task.Run(() =>
                        {
                            //BlockChainクラスをインスタンス化
                            BlockChain bc = new BlockChain();
                            //UpdateHistoryDTOクラスに値を格納してインスタンス化
                            UpdateHistoryDTO uhDTO = new UpdateHistoryDTO()
                            {
                                userId = userId,
                                scheduleId = scheduleId,
                                updateType = "スケジュール登録",
                                updateStartTime = startTime,
                                updateEndingTime = endingTime,
                                subject = subject,
                                detail = detail,
                                updateTime = DateTime.Now
                            };
                            //UpdateHistoryDAOクラスのインスタンス化
                            UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();

                            //BlockChainクラスのBlockメソッドを呼び出しハッシュキーとNonceを取得、DTOに格納
                            //uhDTO = bc.Block(uhDTO);
                            //履歴登録メソッドを使用して情報を登録
                            uhDAO.RegistHistory(uhDTO.userId, uhDTO.scheduleId, uhDTO.updateType,
                                uhDTO.updateStartTime, uhDTO.updateEndingTime, uhDTO.subject, uhDTO.detail,
                                uhDTO.updateTime);

                        });

                        await task;

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
                //入力内容が正しくない場合の分岐
                if (!(String.IsNullOrWhiteSpace(subjectCheck)))
                {
                    //「件名」の入力内容が正しくない場合
                    MessageBox.Show(subjectCheck, "入力内容を確認してください");
                }
                if (!(String.IsNullOrWhiteSpace(detailCheck)))
                {
                    //「詳細」の入力内容が正しくない場合
                    MessageBox.Show(detailCheck, "入力内容を確認してください");
                }
                if (startTime > endingTime)
                {
                    MessageBox.Show("終了時刻が開始時刻よりも前に指定されています！！", "日付を確認してください！！");
                }

            }
            //何らかの不具合が発生した場合
            catch (Exception)
            {
                //例外処理としてErrorMessageクラスの呼び出し
                ErrorMessage.ApplicationClose();
            }

        }

        private void ScheduleRegistration_FormClosed(object sender, FormClosedEventArgs e)
        {

        }


    }
}
