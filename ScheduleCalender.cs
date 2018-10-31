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
    public partial class ScheduleCalender : Form
    {
        /// <summary>
        /// 今回の肝となるクラス
        /// 処理内容が複雑化する
        /// </summary>
        /// 

        //親フォームとの情報のやり取りに必要
        public String userId { get; set; }
        public int scheduleId { get; set; }
        private List<DateTime> weekList = new List<DateTime>();

        //必要なクラスのインスタンス化
        UserInfoDTO uiDTO = new UserInfoDTO();
        ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
        ScheduleInfoDAO siDAO = new ScheduleInfoDAO();

        //データテーブルもインスタンス化しておく
        DataTable dt = new DataTable();

        public ScheduleCalender()
        {
            InitializeComponent();
        }

        //このフォームが読み込まれた時点での動作
        private void ScheduleCalender_Load(object sender, EventArgs e)
        {
            //ログインIDに表示
            label1.Text = $"{userId}さんのスケジュール帳";

            //TODOの呼び出し
            setToDo();

            //週間スケジュールのひな型呼び出し
            setCalenderGrid();

            //monthCalenderの選択件数を1件のみにしておく
            monthCalendar1.MaxSelectionCount = 1;

        }


        //TODOの呼び出しに使用するメソッド
        private void setToDo() {

            //当日の日付を取得
            var today = DateTime.Today.ToShortDateString();

            //日付をもとにTODO作成
            toDo.DataSource =  siDAO.getTodo(userId, today);

            //TODOの表示設定
            toDo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;　//ヘッダーの中央寄せ
            toDo.Columns[0].Visible = false;    //最初の列(scheduleId表示列)の非表示

        }

        //週間スケジュールのひな型作成用メソッド
        private void setCalenderGrid() {
            //行数の指定(表示用の7行+スケジュールIDを入れておく7行+ヘッダーの1行)
            scheduleGrid.ColumnCount = 15;

            //曜日を頭に格納していく
            scheduleGrid.Columns[1].HeaderText = "Sun";
            scheduleGrid.Columns[2].HeaderText = "Mon";
            scheduleGrid.Columns[3].HeaderText = "Tue";
            scheduleGrid.Columns[4].HeaderText = "Wed";
            scheduleGrid.Columns[5].HeaderText = "Thu";
            scheduleGrid.Columns[6].HeaderText = "Fri";
            scheduleGrid.Columns[7].HeaderText = "Sat";

            //後で日付を入れる行を作成しておく
            scheduleGrid.Rows.Add();

            //24時間分の行作成
            for (int i = 0; i < 24; i++)
            {
                //時間を格納していく
                scheduleGrid.Rows.Add($"{i}:00");
            }

            //グリッドの右側(スケジュールIDを格納する欄)の非表示処理を回す
            for (int i = 8; i <= 14; i++)
            {
                //指定の行の非表示処理
                scheduleGrid.Columns[i].Visible = false;
            }

            //当日の日付をもとに月曜日を取得、その日付をもとに1週間分の日付を取得
            var today = DateTime.Today;
            var startDate = getSundayDate(today);
            setCalenderDate(startDate);

            //週間スケジュールの書式設定
            scheduleGrid.Columns[0].Width = 40;     //時間表示のセルの幅設定
            scheduleGrid.Rows[0].Frozen = true;     //日付セルの固定(スクロール時に動かない)
            scheduleGrid.Columns[0].Frozen = true;  //時間表示セルの固定(スクロール時に動かない)
            scheduleGrid.ReadOnly = true;           //週間スケジュールの編集(手入力)の禁止
            
            //週間スケジュールの文字表示設定(列、行の文字中央寄せ)
            scheduleGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            scheduleGrid.Rows[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            scheduleGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //週間スケジュールの行数だけfor文を回す
            foreach (DataGridViewColumn c in scheduleGrid.Columns)
            {
                //週間スケジュールのソート(並び替え)は禁止しておく
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

        }

        //渡された日付から1週間の日付をセルとリストに格納してくれるメソッド
        private void setCalenderDate(DateTime day) {
            //週間リストは初期化しておく
            weekList.Clear();
            //1週間分for文を回す
            for (int i = 0; i < 7; i++)
            {
                //日付を入れるセルに値を格納していく
                scheduleGrid.Rows[0].Cells[1 + i].Value = day.AddDays(i).ToString("MM/dd");
                weekList.Add(day.AddDays(i));       //リストにも日付を追加
            }

            
        }

        //渡された日付が含まれる週の日曜日の日付を割り出すメソッド
        private DateTime getSundayDate(DateTime day) {
            //週における日曜日の日付(0)から、渡された日が週の何日目かを引く
            return day.AddDays(DayOfWeek.Sunday - day.DayOfWeek);
        }

        //MonthCalenderの日付が変更された場合の動作
        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            var selectedDate = monthCalendar1.SelectionStart;   //選択された日付の取得
            var startDate = getSundayDate(selectedDate);        //選択された日付から、週の日曜を割り出す

            setCalenderDate(startDate);     //日付を週間スケジュールに格納していくメソッドの呼び出し

            //選択された日付をもとにTODOに予定を読み込ませる
            toDo.DataSource = siDAO.getTodo(userId, selectedDate.ToShortDateString());
        }

        //予定登録ボタンが押された場合の動作
        private void button1_Click(object sender, EventArgs e)
        {
            ScheduleRegistration sr = new ScheduleRegistration();
            sr.userId = userId;
            sr.ShowDialog(this);
            sr.Dispose();
        }

        //このページが再度アクティブになった場合の動作
        private void ScheduleCalender_Activated(object sender, EventArgs e)
        {
            setToDo();
            //setWeeklyschedeule();
        }

        //このフォームが閉じられた場合の動作
        private void ScheduleCalender_FormClosed(object sender, FormClosedEventArgs e)
        {
            //親フォームの表示
            this.Owner.Show();
        }
    }
}
