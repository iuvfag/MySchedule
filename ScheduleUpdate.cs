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
        internal ScheduleInfoDTO siDTO { get; set; }



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
            scheduleDatePicker.Value = DateTime.Parse(siDTO.startTime.ToShortDateString());
            //開始時刻
            startTimePicker.Value = siDTO.startTime;
            //終了時刻
            endingTimePicker.Value = siDTO.endingTime;
            //件名
            subjectTextbox.Text = siDTO.subject;
            //詳細
            detailTextbox.Text = siDTO.detail;
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
        private async void Button2_Click(object sender, EventArgs e)
        {
            //何らかの不具合が発生した場合強制終了させるtry-catch文
            try
            {
                //「件名」、「詳細」を入力欄から取得してフィールドに格納
                siDTO.subject = subjectTextbox.Text;
                siDTO.detail = detailTextbox.Text;

                //「件名」、「詳細」の入力チェック
                String subjectCheck = siDTO.subject.DoCheck2("件名", 250);
                String detailCheck = siDTO.detail.DoCheck3("詳細", 1000);

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
                    siDTO.startTime = DateTime.Parse(st);
                    siDTO.endingTime = DateTime.Parse(et);

                    //先ほど連結した時刻で開始時刻と終了時刻が前後していないかどうか調べる
                    if (siDTO.startTime <= siDTO.endingTime)
                    {
                        //DAOクラスのインスタンス化
                        ScheduleInfoDAO siDAO = new ScheduleInfoDAO();
                        //スケジュールの更新を行い結果(更新数)をresultに格納
                        int result = siDAO.UpdeteSchedule(siDTO.userId, siDTO.scheduleId, siDTO.startTime, 
                            siDTO.endingTime, siDTO.subject, siDTO.detail);

                        //更新されたスケジュールが1件でも存在すれば次の処理へ
                        if (result > 0)
                        {
                            //メッセージ表示
                            MessageBox.Show("スケジュール情報を修正しました", "更新完了");

                            //処理に時間がかかるため、マルチスレッド処理を行う
                            var task = Task.Run(() =>
                            {
                                //BlockChainクラスをインスタンス化
                                BlockChain bc = new BlockChain();
                                //UpdateHistoryDTOクラスに値を格納し、インスタンス化
                                UpdateHistoryDTO uhDTO = new UpdateHistoryDTO()
                                {
                                    userId = siDTO.userId,
                                    scheduleId = siDTO.scheduleId,
                                    updateType = "スケジュール更新",
                                    updateStartTime = siDTO.startTime,
                                    updateEndingTime = siDTO.endingTime,
                                    subject = siDTO.subject,
                                    detail = siDTO.detail,
                                };
                                //UpdateHistoryDAOクラスをインスタンス化
                                UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();

                                //BlockChainクラスのBlockメソッド
                                //uhDTO = bc.Block(uhDTO);
                                //履歴登録用のメソッドを呼び出し、情報を登録
                                uhDAO.RegistHistory(uhDTO.userId, uhDTO.scheduleId, uhDTO.updateType,
                                    uhDTO.updateStartTime, uhDTO.updateEndingTime, uhDTO.subject, uhDTO.detail);

                            });

                            await task;


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
                else if (!(String.IsNullOrWhiteSpace(detailCheck)))
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
