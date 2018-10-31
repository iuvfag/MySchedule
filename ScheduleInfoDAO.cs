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
         * ⑦既にその時間に予定が登録されているかチェックするメソッド
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
        internal int registSchedule(String userId, DateTime startTime, DateTime endingTime, String subject, String detail) {

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
            finally {
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
        internal DataTable getTodo(String userId, String today) {

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
            finally {
                //最終的に接続は閉じておく
                con.Close();
            }
            //データテーブルを返す
            return dt;

        }

    }
}
