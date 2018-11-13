﻿using System;
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

        private void ScheduleCalender_Shown(object sender, EventArgs e)
        {
            //TODO、週間スケジュールともに初期にセルが選択状態になっているのを防ぐ
            toDo.CurrentCell = null;
            scheduleGrid.CurrentCell = null;
        }

        /// <summary>
        /// このフォームが読み込まれた時点での動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCalender_Load(object sender, EventArgs e)
        {
            //ログインIDに表示
            label1.Text = $"{userId}さんのスケジュール帳";
            var today = DateTime.Today.ToShortDateString();
            label2.Text = $"{today}";

            scheduleGrid.RowTemplate.Height = 30;

            //TODOの呼び出し
            setToDo();

            //週間スケジュールのひな型呼び出し
            setCalenderGrid();

            setWeeklySchedule();

            //monthCalenderの選択件数を1件のみにしておく
            monthCalendar1.MaxSelectionCount = 1;

        }


        /// <summary>
        /// TODOの呼び出しに使用するメソッド
        /// </summary>
        private void setToDo()
        {

            //当日の日付を取得
            var today = DateTime.Today.ToShortDateString();

            //日付をもとにTODO作成
            toDo.DataSource = siDAO.getTodo(userId, today);

            //TODOの表示設定
            toDo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;　//ヘッダーの中央寄せ
            toDo.Columns[0].Visible = false;    //最初の列(scheduleId表示列)の非表示

        }

        /// <summary>
        /// 週間スケジュールのひな型作成用メソッド
        /// </summary>
        private void setCalenderGrid()
        {
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
            scheduleGrid.Columns[0].Selected = false;
            scheduleGrid.Rows[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            //週間スケジュールの行数だけfor文を回す
            foreach (DataGridViewColumn c in scheduleGrid.Columns)
            {
                //週間スケジュールのソート(並び替え)は禁止しておく
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

        }

        /// <summary>
        /// 渡された日付から1週間の日付をセルとリストに格納してくれるメソッド
        /// </summary>
        /// <param name="day"></param>
        private void setCalenderDate(DateTime day)
        {
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

        /// <summary>
        /// 週間スケジュールに予定を表示するためのメソッド(デフォルトで呼び出すメソッド)
        /// 該当する1週間の予定はすべて表示
        /// </summary>
        private void setWeeklySchedule()
        {
            //DTOクラスのインスタンス化
            ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
            //まず、1週間分for文を回す(1～7なのはグリッドの番地に合わせているため)
            for (int i = 1; i <= 7; i++)
            {
                //その週の日付を格納したリストから日付を取り出す(インデックス0から)
                String columnValue = weekList[i - 1].ToString("yyyy-MM-dd");

                //24時間分回すfor文
                for (int n = 1; n <= 24; n++)
                {
                    //開始時刻と終了時刻を設定(変数nと実際セルに格納されている時間が違うことに注意！)
                    String startTime = $"{n - 1}:00";     //開始時刻
                    String endingTime = $"{n - 1}:59";    //終了時刻(24時以上になるのを防ぐ)

                    //日付と時刻を結合させる
                    startTime = $"{columnValue} {startTime}";
                    endingTime = $"{columnValue} {endingTime}";

                    //DTOクラスに該当する時間の予定とスケジュールIDを格納
                    siDTO = siDAO.getWeeklySchedule(userId, startTime, endingTime);

                    //セルに取得してきた予定を格納
                    scheduleGrid.Rows[n].Cells[i].Value = siDTO.subject;

                    //7個右のセルにスケジュールIDを格納
                    scheduleGrid.Rows[n].Cells[i + 7].Value = siDTO.scheduleId;

                    if (siDTO.subjectList.Count > 1)
                    {
                        scheduleGrid[i, n].Style.BackColor = Color.Orange;
                    }
                    else
                    {
                        scheduleGrid[i, n].Style.BackColor = Color.White;
                    }

                }

            }
        }

        /// <summary>
        /// 週間スケジュールに予定を表示するためのメソッド(TODOクリック時に呼び出されるメソッド)
        /// TODOから渡された情報をもとに予定を上書きして表示するためのもの
        /// </summary>
        /// <param name="scheduleId">ログインID</param>
        /// <param name="todoDate">TODOの選択日付(monthCalenderから取得したもの)</param>
        /// <param name="todoStartTime">TODOの選択開始時刻</param>
        /// <param name="todoEndingTime">TODOの選択終了時刻</param>
        /// <param name="subject">TODOの選択件名</param>
        private void setWeeklySchedule(int scheduleId, String todoDate, DateTime todoStartTime,
            DateTime todoEndingTime, String subject)
        {
            scheduleGrid.ClearSelection();
            setWeeklySchedule();

            //まず、1週間分for文を回す(1～7なのはグリッドの番地に合わせているため)
            for (int i = 1; i <= 7; i++)
            {
                //その週の日付を格納したリストから日付を取り出す(インデックス0から)
                String scheduleGridDate = weekList[i - 1].ToString("yyyy/MM/dd");

                //TODOから渡された日付と週間スケジュールの日付が一致したら次の処理へ
                //※週間スケジュールの日付は実は日付リストからとってきたもの
                if (todoDate == scheduleGridDate)
                {

                    //24時間分回すfor文
                    for (int n = 1; n <= 24; n++)
                    {
                        //開始時刻と終了時刻を設定(変数nと実際セルに格納されている時間が違うことに注意！)
                        String st = $"{n - 1}:00";     //開始時刻
                        String et = $"{n - 1}:59";    //終了時刻(24時以上になるのを防ぐ)

                        //日付と開始時刻、終了時刻を連結
                        st = $"{scheduleGridDate} {st}";
                        et = $"{scheduleGridDate} {et}";
                        //連結したものをDateTime型に変換
                        DateTime scheduleGridStartTime = DateTime.Parse(st);
                        DateTime scheduleGridEndingTime = DateTime.Parse(et);

                        /* 複雑な条件分岐
                         * 週間スケジュールに予定を表示する場合は
                         * 予定時刻とグリッドの時刻(特定の1時間)が以下の関係となる場合である
                         */
                        //①予定の開始時刻と終了時刻がグリッドの時刻(特定の1時間内)に囲まれている場合
                        if (todoStartTime <= scheduleGridStartTime && todoEndingTime >= scheduleGridEndingTime)
                        {
                            //セルに件名を代入する
                            scheduleGrid.Rows[n].Cells[i].Value = subject;
                            //7個右のセルにスケジュールIDも格納しておく
                            scheduleGrid.Rows[n].Cells[i + 7].Value = scheduleId;
                            scheduleGrid.Rows[n].Cells[i].Selected = true;
                        }
                        //②予定の開始時刻と終了時刻がグリッドの時刻(特定の1時間内)に囲まれている場合
                        else if (todoStartTime >= scheduleGridStartTime && todoEndingTime <= scheduleGridEndingTime)
                        {
                            //セルに件名を代入する
                            scheduleGrid.Rows[n].Cells[i].Value = subject;
                            //7個右のセルにスケジュールIDも格納しておく
                            scheduleGrid.Rows[n].Cells[i + 7].Value = scheduleId;
                            scheduleGrid.Rows[n].Cells[i].Selected = true;
                        }
                        //③予定の開始時刻がグリッドの時刻(特定の1時間内)に囲まれているもの
                        else if (todoStartTime <= scheduleGridStartTime && todoEndingTime >= scheduleGridStartTime)
                        {
                            //セルに件名を代入する
                            scheduleGrid.Rows[n].Cells[i].Value = subject;
                            //7個右のセルにスケジュールIDも格納しておく
                            scheduleGrid.Rows[n].Cells[i + 7].Value = scheduleId;
                            scheduleGrid.Rows[n].Cells[i].Selected = true;
                        }
                        //④予定の終了時刻がグリッドの時刻(特定の1時間内)に囲まれているもの
                        else if (todoStartTime <= scheduleGridEndingTime && todoEndingTime >= scheduleGridEndingTime)
                        {
                            //セルに件名を代入する
                            scheduleGrid.Rows[n].Cells[i].Value = subject;
                            //7個右のセルにスケジュールIDも格納しておく
                            scheduleGrid.Rows[n].Cells[i + 7].Value = scheduleId;
                            scheduleGrid.Rows[n].Cells[i].Selected = true;
                        }

                    }

                }

            }
        }

        /// <summary>
        /// 渡された日付が含まれる週の日曜日の日付を割り出すメソッド
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private DateTime getSundayDate(DateTime day)
        {
            //週における日曜日の日付(0)から、渡された日が週の何日目かを引く
            return day.AddDays(DayOfWeek.Sunday - day.DayOfWeek);
        }

        /// <summary>
        /// MonthCalenderの日付が変更された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            var selectedDate = monthCalendar1.SelectionStart;   //選択された日付の取得
            var startDate = getSundayDate(selectedDate);        //選択された日付から、週の日曜を割り出す

            setCalenderDate(startDate);     //日付を週間スケジュールに格納していくメソッドの呼び出し
            setWeeklySchedule();            //週間予定を表示する

            //選択された日付をもとにTODOに予定を読み込ませる
            toDo.DataSource = siDAO.getTodo(userId, selectedDate.ToShortDateString());
            label2.Text = $"{selectedDate}";

            //ToDoの選択状態は解除しておく
            toDo.CurrentCell = null;
        }

        /// <summary>
        /// 予定登録ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //予定登録画面を開く
            ScheduleRegistration sr = new ScheduleRegistration();
            sr.userId = userId;     //ログインIDを渡しておく
            sr.defaultDate = monthCalendar1.SelectionStart;
            sr.ShowDialog(this);
            sr.Dispose();
        }

        /// <summary>
        /// このページが再度アクティブになった場合の動作(子フォームが閉じられた場合)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCalender_Activated(object sender, EventArgs e)
        {
            //monthCalenderの日付を取得(前回選択された日付のまま)
            var day = monthCalendar1.SelectionStart;
            //日付をもとにTODO作成
            toDo.DataSource = siDAO.getTodo(userId, day.ToShortDateString());
            //週間スケジュールを読み込みなおす
            setWeeklySchedule();
        }

        /// <summary>
        /// このフォームが閉じられた場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScheduleCalender_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        /// <summary>
        /// TODOがダブルクリックされた場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toDo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //ダブルクリックされたセルの場所を取得
            Point p = toDo.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hti = toDo.HitTest(p.X, p.Y);

            //クリックされたセルの最初の行(見えないがスケジュールIDが格納されている)の値を取得
            //押下されたセルがヘッダー部分ではないことを確認
            if (hti.RowIndex > -1 && hti.ColumnIndex > -1)
            {
                //スケジュールID(見えない部分に格納されている)を取得
                scheduleId = (int)toDo.Rows[hti.RowIndex].Cells[0].Value;

                //詳細画面を開く
                ScheduleDetail sd = new ScheduleDetail();
                sd.userId = userId;           //ログインIDを渡す
                sd.scheduleId = scheduleId;   //スケジュールIDを渡す
                sd.Show(this);
            }


        }

        /// <summary>
        /// 週間スケジュールが1度クリックされた場合の動作(TODOや左上のカレンダーの日付も連動させる)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleGrid_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            //クリックされたセルの場所を取得
            Point p = scheduleGrid.PointToClient(Cursor.Position);

            DataGridView.HitTestInfo hti = scheduleGrid.HitTest(p.X, p.Y);

            //日付リストから今度は日付を取得
            var selectedDate = weekList[hti.ColumnIndex - 1].ToShortDateString();

            //押下されたセルが週の部分か確認(そうでなければnull)
            if (selectedDate != null)
            {
                //週間スケジュールで押下された部分の日付をmonthCalenderの選択にも反映させる
                monthCalendar1.SelectionStart = DateTime.Parse(selectedDate.ToString());
            }

        }



        /// <summary>
        /// 週間スケジュールがダブルクリックされた場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {

                //ダブルクリックされた場所の取得
                Point p = scheduleGrid.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = scheduleGrid.HitTest(p.X, p.Y);
                if (hti.ColumnIndex > 0)
                {
                    if (hti.RowIndex > 0)
                    {
                        //7つ右のスケジュールIDを格納しているセルを取得
                        var selectedCell = scheduleGrid.Rows[hti.RowIndex].Cells[hti.ColumnIndex + 7].Value;

                        //セルの値がnullでも0でもなければ次の処理へ
                        if (selectedCell != null && (int)selectedCell != 0)
                        {
                            //ログインIDとスケジュールIDを渡して詳細画面を開く
                            ScheduleDetail sd = new ScheduleDetail();
                            sd.userId = userId;           //ログインIDを渡す
                            sd.scheduleId = (int)selectedCell;   //スケジュールIDを渡しておく
                            sd.ShowDialog(this);
                            sd.Dispose();
                        }
                        else if (selectedCell == null && (int)selectedCell == 0)
                        {
                            //セルの中身がnullの場合
                            return;
                        }
                        //セルの値が0(何も予定が入っていない)場合
                        if ((int)selectedCell == 0)
                        {
                            //新規登録フォームを開く
                            ScheduleRegistration sr = new ScheduleRegistration();
                            //flgを立てておく(これをもとにどの画面から来たかを整理する)
                            sr.flg = true;

                            //日付を日付リストから持ってくる
                            String defaultdate = weekList[hti.ColumnIndex - 1].ToShortDateString();
                            //時刻をスケジュールのセルのヘッダーから持ってくる
                            String start = scheduleGrid.Rows[hti.RowIndex].Cells[0].Value.ToString();
                            String ending;
                            //もし最後のセルなら
                            if (start == "23:00")
                            {
                                //時間が24時間を超えないよう調整
                                ending = "23:59";
                            }
                            //最後のセルではない場合
                            else
                            {
                                //1つ下のセルのヘッダーから取得
                                ending = scheduleGrid.Rows[hti.RowIndex + 1].Cells[0].Value.ToString();
                            }
                            sr.userId = userId;                             //ログインIDを渡す
                            sr.defaultDate = DateTime.Parse(defaultdate);   //日付をDateTime変換して渡す
                            sr.start = DateTime.Parse(start);               //開始時刻をDateTime変換して壊す
                            sr.ending = DateTime.Parse(ending);             //終了時間をDateTime変換して渡す

                            sr.ShowDialog(this);
                            sr.Dispose();
                        }
                        else if (selectedCell == null)
                        {
                            //セルの中身がnullの場合
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        /// 「履歴確認」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ////履歴確認画面を開く
            //UpdateHistoryData uhd = new UpdateHistoryData();
            //uhd.userId = userId;    //ログインIDを渡す
            //uhd.ShowDialog(this);
            //uhd.Dispose();
        }

        /// <summary>
        /// 「パスワード再設定」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            ////パスワード再設定画面を開く
            //ResetPassword rp = new ResetPassword();
            //rp.userId = userId;     //ログインIDを渡す
            //rp.ShowDialog(this);
            //rp.Dispose();
        }

        /// <summary>
        /// 「予定一覧」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            ////予定一覧画面を開く
            //AllSchedules als = new AllSchedules();
            //als.userId = userId;        //ログインIDを渡す
            //als.ShowDialog(this);
            //als.Dispose();
        }

        /// <summary>
        /// マウスをクリックして話した場合の動作(右クリックで作動)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scheduleGrid_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                //まず押下されたボタンが右クリックかどうか判定
                if (e.Button == MouseButtons.Right)
                {
                    //行のインデックスを格納するリストを用意
                    List<int> columnIndexList = new List<int>();
                    //列のインデックスを格納するリストを用意
                    List<int> rowIndexList = new List<int>();

                    //選択されているセルの数だけforeach文を回す
                    foreach (DataGridViewCell c in scheduleGrid.SelectedCells)
                    {
                        columnIndexList.Add(c.ColumnIndex);     //行のインデックスをリストに格納していく
                        rowIndexList.Add(c.RowIndex);           //列のインデックスをリストに格納していく
                    }

                    if (columnIndexList.Count > 1)
                    {

                    }
                    //それぞれのリストをソートしておく
                    rowIndexList.Sort();
                    columnIndexList.Sort();
                    //int型の変数「columnIndex」に行のインデックスを格納したリストの最初の要素を格納
                    int columnIndex = columnIndexList[0];
                    //int型の変数「firstIndex」に列のインデックスを格納したリストの最初の要素を格納(1番上の選択セル)
                    int firstIndex = rowIndexList[0];
                    //int型の変数「lastIndex」に列のインデックを格納したリストの最初の要素を格納(1番下の選択セル)
                    int lastIndex = rowIndexList[rowIndexList.Count - 1];
                    //日付リストから日付を取得
                    String date = weekList[columnIndex - 1].ToShortDateString();
                    //列インデックスをもとに選択範囲のセルの1番最初の時間を取得
                    String startTime = scheduleGrid.Rows[firstIndex].Cells[0].Value.ToString();
                    //列インデックスをもとに選択範囲のセルの1番下の時間を取得(ただし表示時刻に1足しておく)
                    String endingTime = scheduleGrid.Rows[lastIndex + 1].Cells[0].Value.ToString();

                    //予定登録画面の呼び出し
                    ScheduleRegistration sr = new ScheduleRegistration();
                    sr.flg = true;                          //figを立てておく(これでどの画面から来たか認識する)
                    sr.userId = userId;                     //ログインIDを渡す
                    sr.defaultDate = DateTime.Parse(date);  //日付を渡す
                    sr.start = DateTime.Parse(startTime);   //開始時刻を渡す
                    sr.ending = DateTime.Parse(endingTime); //終了時刻を渡す
                    sr.ShowDialog(this);
                    sr.Dispose();

                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }

        }

        private void toDo_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                //クリックされたセルの場所を取得する
                Point p = toDo.PointToClient(Cursor.Position);
                DataGridView.HitTestInfo hti = toDo.HitTest(p.X, p.Y);

                //クリックされたセルの最初の行(見えないがスケジュールIDが格納されている)の値を取得
                //押下されたセルがヘッダー部分ではないことを確認
                if (hti.RowIndex > -1 && hti.ColumnIndex > -1)
                {
                    //スケジュールID(見えない部分に格納されている)を取得
                    scheduleId = (int)toDo.Rows[hti.RowIndex].Cells[0].Value;

                    //TODOの日付を取得(monthCalenderの選択日付と連動していることを利用)、変数に格納
                    String todoDate = monthCalendar1.SelectionStart.ToShortDateString();
                    //TODOの開始時刻を取得、変数に格納
                    String startTime = toDo.Rows[hti.RowIndex].Cells[1].Value.ToString();
                    //TODOの終了時刻を取得、変数に格納
                    String endingTime = toDo.Rows[hti.RowIndex].Cells[2].Value.ToString();
                    //日付と時刻をそれぞれ連結、、変数に格納
                    startTime = $"{todoDate} {startTime}";
                    endingTime = $"{todoDate} {endingTime}";

                    //開始時刻、終了時刻をDateTime変換し、変数に格納
                    DateTime todoStartTime = DateTime.Parse(startTime);
                    DateTime todoEndingTime = DateTime.Parse(endingTime);
                    //件名を取得
                    String subject = toDo.Rows[hti.RowIndex].Cells[3].Value.ToString();

                    //格納した情報をもとに週間スケジュールの予定表示メソッドを呼び出す
                    setWeeklySchedule(scheduleId, todoDate, todoStartTime, todoEndingTime, subject);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
