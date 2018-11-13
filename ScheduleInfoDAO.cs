using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using Npgsql;

namespace MySchedule
{
    internal class ScheduleInfoDAO
    {

        //スケジュール関連のDBとのやり取りに使用するクラス
        /* 
         * ①スケジュール登録用メソッド
         * ②TODOデータ取得のためのメソッド
         * ③スケジュールの情報をDBから獲得し、DTOに格納するメソッド
         * ④既存のスケジュールの削除メソッド
         * ⑤スケジュール修正用のメソッド
         * ⑥週間スケジュールに表示する情報、スケジュールIDを取得するメソッド
         * ⑦既にその時間に予定が登録されているかチェックするメソッド(スケジュール修正用)
         * ⑧既にその時間に予定が登録されているかチェックするメソッド(新規登録用)
         * ⑨ログインIDをもとにすべての予定を取得し、データテーブルに格納するメソッド
         * ⑩スケジュールIDをもとに該当する予定が存在するかを調べるメソッド
         */

        //DBへの接続の準備
        NpgsqlConnection con = new NpgsqlConnection(@"Server=localhost;" +
                                                    "Port=5432;" +
                                                    "UserId=postgres;" +
                                                    "Password=postgres;" +
                                                    "Database=myschedule");

        NpgsqlCommand cmd = new NpgsqlCommand();

        /// <summary>
        /// ①スケジュール登録用メソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="startTime">スケジュールの開始時刻</param>
        /// <param name="endingTime">スケジュールの終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>更新件数を格納したint型の変数</returns>
        internal int RegistSchedule(String userId, DateTime startTime, DateTime endingTime, String subject,
            String detail)
        {

            CommonUtility cu = new CommonUtility();

            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            //DBに登録するハッシュキーの作成
            String key = cu.CreateHashKey(userId, startTime, endingTime, subject, detail);

            //SQL文の作成
            cmd.CommandText = "INSERT INTO schedule_info (user_id, start_time, ending_time, subject, detail, " +
                "schedule_key) VALUES(@userId, @startTime, @endingTime, @subject, @detail, @key)";

            //@部分への値の代入
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@startTime", startTime));
            cmd.Parameters.Add(new NpgsqlParameter("@endingTime", endingTime));
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));
            cmd.Parameters.Add(new NpgsqlParameter("@key", key));

            //接続
            con.Open();

            try
            {
                result = cmd.ExecuteNonQuery();     //更新件数をresultに代入
            }
            catch (Exception ex)
            {
                //例外があれば表示
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@startTime");
            cmd.Parameters.Remove("@endingTime");
            cmd.Parameters.Remove("@subject");
            cmd.Parameters.Remove("@detail");
            cmd.Parameters.Remove("@key");

            //結果を戻す
            return result;
        }

        /// <summary>
        /// ②TODOデータ習得のためのメソッド(データを格納したデータテーブルを返す)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="today">日付(検索したい日付)</param>
        /// <returns>DBの値を格納したデータテーブル</returns>
        internal DataTable GetTodo(String userId, String today)
        {
            //データベースの値をデータグリッドに格納するために、データセット・データテーブルもインスタンス化しておく
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            cmd.Connection = con;

            //SQL文の作成。今回は直接接続して作成
            String sql = "SELECT schedule_id, CAST(start_time as time) as 開始時刻, " +
                "CAST(ending_time as time) as 終了時刻, subject as 予定 " +
                "FROM schedule_info WHERE user_id = '" + userId + "' " +
                "AND CAST(start_time as date) = '" + today + "' " +
                "ORDER BY start_time";

            /* スケジュールID、開始時刻、終了時刻、予定を
             * ログインIDと日付から取得し、カラム名も変更　*/

            con.Open();
            try
            {
                //SQL文を実行しデータセットに値を格納するために必要。データ格納の準備
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con);

                ds.Reset();     //データ格納前にリセットしておく            
                da.Fill(ds);    //データセットにデータアダプターが取得してきた値を格納             
                dt = ds.Tables[0];//複数テーブル存在するが、今回は最初のテーブルに入っているためインデックス[0]を指定

            }
            catch (Exception ex)
            {
                //例外があれば表示
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }
            //データテーブルを返す
            return dt;

        }

        /// <summary>
        /// ③スケジュールの詳細をDBから取得しDTOに格納して、そのDTOを戻り値として渡すメソッド
        /// </summary>
        /// <param name="scheduleId">スケジュールID</param>
        /// <returns>値を格納したScheduleInfoDTOクラスのインスタンス</returns>
        internal ScheduleInfoDTO GetScheduleDetail(int scheduleId)
        {

            //DTOをインスタンス化しておく
            ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT schedule_id, start_time, ending_time, subject, detail FROM " +
                "schedule_info WHERE schedule_id = @scheduleId";
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));

            //接続
            con.Open();
            try
            {
                //リーダーの使用
                using (var reader = cmd.ExecuteReader())
                {
                    //リーダーが読み取っている間は
                    while (reader.Read() == true)
                    {
                        //DTOにリーダーがゲットしてきた値を格納(それぞれデータ型は合わせる)
                        siDTO.scheduleId = reader.GetInt32(0);
                        siDTO.startTime = reader.GetDateTime(1);
                        siDTO.endingTime = reader.GetDateTime(2);
                        siDTO.subject = reader.GetString(3);
                        siDTO.detail = reader.GetString(4);
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@scheduleId");

            //値を格納したDTOを返す
            return siDTO;
        }

        /// <summary>
        /// ④既存のスケジュールの削除を行うメソッド(戻り値は実行結果【件数】)
        /// </summary>
        /// <param name="scheduleId">該当するスケジュールを割り出すために使用</param>
        /// <returns>削除されたスケジュールの件数</returns>
        internal int DeleteSchedule(int scheduleId)
        {
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "DELETE FROM schedule_info WHERE schedule_id = @scheduleId";

            //@に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));

            //接続開始
            con.Open();

            try
            {
                //SQL文の実行結果をresultに格納
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("scheduleId");

            //結果を返す
            return result;
        }

        /// <summary>
        /// ⑤スケジュール修正用のメソッド
        /// </summary>
        /// <param name="scheduleId">スケジュールの開始時刻</param>
        /// <param name="startTime">スケジュールの開始時刻</param>
        /// <param name="endingTime">スケジュールの終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>更新件数を格納したint型の変数</returns>
        internal int UpdeteSchedule(String userId, int scheduleId, DateTime startTime, DateTime endingTime,
            String subject, String detail)
        {
            CommonUtility cu = new CommonUtility();

            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            String key = cu.CreateHashKey(userId, startTime, endingTime, subject, detail);

            //SQL文の作成
            cmd.CommandText = "UPDATE schedule_info SET start_time = @startTime, ending_time = @endingTime, " +
                "subject = @subject, detail = @detail, schedule_key = @key WHERE schedule_id = @scheduleId";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@startTime", startTime));
            cmd.Parameters.Add(new NpgsqlParameter("@endingTime", endingTime));
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));
            cmd.Parameters.Add(new NpgsqlParameter("@key", key));
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));

            //接続開始
            con.Open();

            try
            {
                //SQL文を実行し、その結果をresultに格納する
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@startTime");
            cmd.Parameters.Remove("@endingTime");
            cmd.Parameters.Remove("@subject");
            cmd.Parameters.Remove("@detail");
            cmd.Parameters.Remove("@key");
            cmd.Parameters.Remove("@scheduleId");

            //結果を戻す
            return result;
        }

        /// <summary>
        /// ⑥週間スケジュールに表示する情報、スケジュールIDを取得するメソッド
        /// </summary>
        /// <param name="userid">ログインID</param>
        /// <param name="startTime">スケジュール委開始時間(SQL文の結合の都合で今回はString型)</param>
        /// <param name="endingTime">スケジュールの終了時刻(SQL文の接合の都合で今回はString型)</param>
        /// <returns>値を格納したScheduleInfoDTOクラスのインスタンス</returns>
        internal ScheduleInfoDTO GetWeeklySchedule(String userId, String startTime, String endingTime)
        {
            //結果を格納するDTOをインスタンス化しておく
            ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
            cmd.Connection = con;

            //SQL文の作成。今回は複雑なので検索条件は後述
            cmd.CommandText = "SELECT subject, schedule_id FROM schedule_info WHERE user_id = @userId " +
                "AND ( (start_time >= '" + startTime + "' AND start_time <= '" + endingTime + "') " +        //①
                "OR (ending_time > '" + startTime + "' AND ending_time <= '" + endingTime + "') " +         //②
                "OR (start_time < '" + startTime + "' AND ending_time > '" + startTime + "') " +          //③
                "OR (start_time < '" + endingTime + "' AND ending_time > '" + endingTime + "') )";          //④

            /* 検索条件
             * 
             * 引数として渡された開始時刻と終了時刻について
             * ①その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻に囲まれているもの
             * ②その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻を囲んでいるもの
             * ③その開始時刻が既存のスケジュールの開始時刻と終了時刻の間にあるもの
             * ④その終了時刻が既存のスケジュールの開始時刻と終了時刻の間にあるもの
             * 
             * を確認する
             * 
             * 今回引数として渡すのは週間スケジュール(データグリッド)の開始時刻と終了時刻
             * (つまり、24時間のうち特定の1時間)
             * その1時間に被るかどうかを判定し、被るなら週間スケジュールに表示するためこの条件になる
             */

            //SQLの@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            //接続開始
            con.Open();

            try
            {
                //リーダーの呼び出し
                using (var reader = cmd.ExecuteReader())
                {
                    //リーダーが読み取っている間は
                    while (reader.Read())
                    {
                        //DTOに値を格納していく
                        siDTO.subject = reader.GetString(0);
                        siDTO.subjectList.Add(reader.GetString(0));
                        siDTO.scheduleId = reader.GetInt32(1);
                        siDTO.scheduleIdList.Add(reader.GetInt32(1));
                        siDTO.scheduleCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@userId");

            //値を格納したDTOを返す
            return siDTO;

        }

        /// <summary>
        /// ⑦既にその時間に予定が登録されているかチェックするメソッド(スケジュール修正用)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="startTime">スケジュールの開始時刻</param>
        /// <param name="endingTime">スケジュールの終了時刻</param>
        /// <returns>int型の変数</returns>
        internal bool IsExistsSchedule(String userId, int scheduleId, String startTime, String endingTime)
        {
            //結果を初期化
            bool result = false;
            cmd.Connection = con;

            //SQL文の作成。今回は複雑なので検索条件は後述
            cmd.CommandText = "SELECT * FROM schedule_info WHERE user_id = @userId AND schedule_id != @scheduleId " +
                "AND ( (start_time <= '" + startTime + "' AND ending_time >= '" + endingTime + "') " +    //①
                "OR (start_time >= '" + startTime + "' AND ending_time <= '" + endingTime + "') " +     //②
                "OR (start_time <= '" + startTime + "' AND ending_time >= '" + startTime + "' ) " +     //③
                "OR (start_time <= '" + endingTime + "' AND ending_time >= '" + endingTime + "') )";      //④
            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            /* 検索条件
             * 
             * ユーザーIDは一致するがスケジュールIDは一致しないもので(そうしないと同じスケジュールの時間修正も不可になる)
             * 以下の条件で検索
             * 
             * 引数として渡された開始時刻と終了時刻について
             * ①その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻に囲まれているもの
             * ②その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻を囲んでいるもの
             * ③その開始時刻が既存のスケジュールの開始時刻と終了時刻の間にあるもの
             * ④その終了時刻が既存のスケジュールの開始時刻と終了時刻の間にあるもの
             * 
             * を検索
             * 
             * 今回は検索条件が多いため括弧()の位置には気を付ける
             */

            //接続開始
            con.Open();

            try
            {
                //リーダーの呼び出し
                using (var reader = cmd.ExecuteReader())
                {
                    //リーダーが読み取ることが可能なら
                    while (reader.Read() == true)
                    {
                        //resultに1を代入
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@scheduleId");
            cmd.Parameters.Remove("@userId");

            //結果を戻す
            return result;
        }

        /// <summary>
        /// ⑧既にその時間に予定が登録されているかチェックするメソッド(新規登録用)
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="startTime">スケジュールの開始時刻</param>
        /// <param name="endingTime"スケジュールの終了時刻></param>
        /// <returns>int型の変数</returns>
        internal bool IsExistsSchedule(String userId, String startTime, String endingTime)
        {
            //結果を初期化
            bool result = false;
            cmd.Connection = con;

            //SQL文の作成。今回は複雑なので検索条件は後述
            cmd.CommandText = "SELECT * FROM schedule_info WHERE user_id = @userId " +
                "AND ((start_time <= '" + startTime + "' AND ending_time >= '" + endingTime + "') " +    //①
                "OR (start_time >= '" + startTime + "' AND ending_time <= '" + endingTime + "') " +     //②
                "OR (start_time <= '" + startTime + "' AND ending_time >= '" + startTime + "' ) " +     //③
                "OR (start_time <= '" + endingTime + "' AND ending_time >= '" + endingTime + "'))";      //④
            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            /* 検索条件
             * 
             * 引数として渡された開始時刻と終了時刻について
             * ①その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻に囲まれているもの
             * ②その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻を囲んでいるもの
             * ③その開始時刻が既存のスケジュールの開始時刻と終了時刻の間にあるもの
             * ④その終了時刻が既存のスケジュールの開始時刻と終了時刻の間にあるもの
             * 
             * を検索
             */

            //接続開始
            con.Open();

            try
            {
                //リーダーの呼び出し
                using (var reader = cmd.ExecuteReader())
                {
                    //リーダーが読み取ることが可能なら
                    while (reader.Read() == true)
                    {
                        //resultに1を代入
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@userId");

            //結果を戻す
            return result;
        }

        /// <summary>
        /// ログインIDなどの情報をもとにスケジュールIDを求めるメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="startTime">スケジュールの開始時刻</param>
        /// <param name="endingTime">スケジュールの終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>スケジュールIDを格納したint型変数</returns>
        internal int GetScheduleInfomation(String userId, DateTime startTime,
            DateTime endingTime, String subject, String detail)
        {
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT schedule_id FROM schedule_info " +
                "WHERE user_id = @userId AND start_time = @startTime AND ending_time = @endingTime " +
                "AND subject = @subject AND detail = @detail";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@startTime", startTime));
            cmd.Parameters.Add(new NpgsqlParameter("@endingTime", endingTime));
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));

            //接続開始
            con.Open();

            try
            {
                //リーダーの呼び出し
                using (var reader = cmd.ExecuteReader())
                {
                    //リーダーが読み取っている間は

                    while (reader.Read())
                    {
                        //結果に取得してきたスケジュールIDを格納
                        result = reader.GetInt32(0);
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }

            //パラメーターの値はremoveしておく
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@startTime");
            cmd.Parameters.Remove("@endingTime");
            cmd.Parameters.Remove("@subject");
            cmd.Parameters.Remove("@detail");

            //結果を戻す
            return result;
        }

        /// <summary>
        /// ⑨ログインIDをもとにすべての予定を取得し、データテーブルに格納するメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <returns></returns>
        internal DataTable GetAllSchedule(String userId)
        {
            //データベースの値をデータグリッドに格納するために、データセット・データテーブルもインスタンス化しておく
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            cmd.Connection = con;

            //SQL文の作成(データテーブルに格納するため、カラム名も変えておく)
            String sql = "SELECT schedule_id, CAST(start_time as date) as 日付, CAST(start_time as time) as 開始時刻, " +
                "CAST(ending_time as time) as 終了時刻, subject as 件名, detail as 詳細, " +
                "schedule_key as スケジュールキー FROM schedule_info WHERE user_id = '" + userId + "' " +
                "ORDER BY CAST(start_time as date) ASC";

            //接続開始
            con.Open();

            try
            {
                //SQL文を実行し、データセットに値を入れるために必要
                //データを格納する準備
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con);

                ds.Reset();         //データ格納前にリセットしておく
                da.Fill(ds);        //データ格納
                dt = ds.Tables[0];  //今回は最初のテーブルに入っているためインデックス[0]を指定
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }
            //データテーブルを返す
            return dt;
        }

        /// <summary>
        /// ⑩スケジュールIDをもとに該当する予定が存在するかを調べるメソッド
        /// </summary>
        /// <param name="scheduleId">スケジュールID</param>
        /// <returns>該当する予定が存在するかどうかのbool値</returns>
        internal bool IsExistsSchedule(int scheduleId)
        {
            //結果を宣言(デフォルトはfalse)
            bool result = false;

            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT * FROM schedule_info WHERE schedule_id = @scheduleId";
            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));
            //接続開始
            con.Open();
            try
            {
                //リーダーの呼び出し
                using (var reader = cmd.ExecuteReader())
                {
                    //リーダーが読み取ることができたら
                    while (reader.Read())
                    {
                        result = true;      //resultにtrueを格納
                    }
                }
            }
            catch (Exception ex)
            {
                //例外処理
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                //最終的に接続は閉じておく
                con.Close();
            }
            //resultを戻す
            return result;
        }

    }
}
