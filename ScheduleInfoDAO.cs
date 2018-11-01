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
         */

        //DBへの接続の準備
        NpgsqlConnection con = new NpgsqlConnection(@"Server=localhost;" +
                                                    "Port=5432;" +
                                                    "UserId=postgres;" +
                                                    "Password=postgres;" +
                                                    "Database=myschedule");

        NpgsqlCommand cmd = new NpgsqlCommand();
        NpgsqlDataReader dr;

        //データベースの値をデータグリッドに格納するために、データセット・データテーブルもインスタンス化しておく
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();

        /// <summary>
        /// ①スケジュール登録用メソッド
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startTime"></param>
        /// <param name="endingTime"></param>
        /// <param name="subject"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        internal int registSchedule(String userId, DateTime startTime, DateTime endingTime, String subject, String detail)
        {

            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "INSERT INTO schedule_info (user_id, start_time, ending_time, subject, detail)" +
                "VALUES(@userId, @startTime, @endingTime, @subject, @detail)";

            //@部分への値の代入
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@startTime", startTime));
            cmd.Parameters.Add(new NpgsqlParameter("@endingTime", endingTime));
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));

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
            //結果を戻す
            return result;
        }

        /// <summary>
        /// ②TODOデータ習得のためのメソッド(データを格納したデータテーブルを返す)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="today"></param>
        /// <returns></returns>
        internal DataTable getTodo(String userId, String today)
        {

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

                //データ格納前にリセットしておく
                ds.Reset();

                //データセットにデータアダプターが取得してきた値を格納
                da.Fill(ds);

                //複数テーブルに格納できるが、今回は最初のテーブルに入っているためインデックス[0]を指定
                dt = ds.Tables[0];
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
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        internal ScheduleInfoDTO getScheduleDetail(int scheduleId)
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
            //値を格納したDTOを返す
            return siDTO;
        }

        /// <summary>
        /// ④既存のスケジュールの削除を行うメソッド(戻り値は実行結果【件数】)
        /// </summary>
        /// <param name="scheduleId">該当するスケジュールを割り出すために使用</param>
        /// <returns></returns>
        internal int deleteSchedule(int scheduleId)
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
            //結果を返す
            return result;
        }

        /// <summary>
        /// ⑤スケジュール修正用のメソッド
        /// </summary>
        /// <param name="scheduleId"></param>
        /// <param name="startTime"></param>
        /// <param name="endingTime"></param>
        /// <param name="subject"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        internal int updeteSchedule(int scheduleId, DateTime startTime, DateTime endingTime, String subject,
            String detail)
        {
            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "UPDATE schedule_info SET start_time = @startTime, ending_time = @endingTime, " +
                "subject = @subject, detail = @detail WHERE schedule_id = @scheduleId";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@startTime", startTime));
            cmd.Parameters.Add(new NpgsqlParameter("@endingTime", endingTime));
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));
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
            //結果を戻す
            return result;
        }

        /// <summary>
        /// ⑥週間スケジュールに表示する情報、スケジュールIDを取得するメソッド
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="startTime">SQL文の結合の都合で今回はString型</param>
        /// <param name="endingTime">SQL文の接合の都合で今回はString型</param>
        /// <returns></returns>
        internal ScheduleInfoDTO getWeeklySchedule(String userid, String startTime, String endingTime)
        {
            //結果を格納するDTOをインスタンス化しておく
            ScheduleInfoDTO siDTO = new ScheduleInfoDTO();
            cmd.Connection = con;

            //SQL文の作成。今回は複雑なので検索条件は後述
            cmd.CommandText = "SELECT subject, schedule_id FROM schedule_info WHERE user_id = @userId " +
                "AND (start_time <= '" + startTime + "' AND ending_time >= '" + endingTime + "') " +        //①
                "OR (start_time >= '" + startTime + "' AND ending_time <= '" + endingTime + "') " +         //②
                "OR (start_time <= '" + startTime + "' AND ending_time >= '" + startTime + "') " +          //③
                "OR (start_time <= '" + endingTime + "' AND ending_time >= '" + endingTime + "')";          //④

            /* 検索条件
             * 
             * 引数として渡された開始時刻と終了時刻について
             * ①その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻を囲んでしまうもの
             * ②その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻に囲まれているもの
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
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userid));

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
                        siDTO.scheduleId = reader.GetInt32(1);
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
            //値を格納したDTOを返す
            return siDTO;

        }

        /// <summary>
        /// ⑦既にその時間に予定が登録されているかチェックするメソッド(スケジュール修正用)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startTime"></param>
        /// <param name="endingTime"></param>
        /// <returns></returns>
        internal int isExistsSchedule(String userId, int scheduleId, String startTime, String endingTime)
        {
            //結果を初期化
            int result = 0;
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
             * ①その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻を囲んでしまうもの
             * ②その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻に囲まれているもの
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
                        result = 1;
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
            //結果を戻す
            return result;
        }

        /// <summary>
        /// ⑧既にその時間に予定が登録されているかチェックするメソッド(新規登録用)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startTime"></param>
        /// <param name="endingTime"></param>
        /// <returns></returns>
        internal int isExistsSchedule(String userId, String startTime, String endingTime)
        {
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成。今回は複雑なので検索条件は後述
            cmd.CommandText = "SELECT * FROM schedule_info WHERE user_id = @userId " +
                "AND (start_time <= '" + startTime + "' AND ending_time >= '" + endingTime + "') " +    //①
                "OR (start_time >= '" + startTime + "' AND ending_time <= '" + endingTime + "') " +     //②
                "OR (start_time <= '" + startTime + "' AND ending_time >= '" + startTime + "' ) " +     //③
                "OR (start_time <= '" + endingTime + "' AND ending_time >= '" + endingTime + "')";      //④
            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));

            /* 検索条件
             * 
             * 引数として渡された開始時刻と終了時刻について
             * ①その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻を囲んでしまうもの
             * ②その開始時刻と終了時刻が既存のスケジュールの開始時刻と終了時刻に囲まれているもの
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
                        result = 1;
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
            //結果を戻す
            return result;
        }
    }
}
