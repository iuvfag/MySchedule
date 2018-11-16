using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using System.Data;


namespace MySchedule
{
    class UpdateHistoryDAO
    {
        /// <summary>
        /// 更新履歴登録用のクラス
        /// ①更新履歴登録用のメソッド
        /// ②更新履歴をログインIDごとに取得し、データテーブルに格納するメソッド
        /// ③前回のハッシュキー(最も新しい履歴IDのハッシュキー)を取得するためのメソッド
        /// ④テーブルの全レコードを取得するメソッド(管理者用)
        /// </summary>

        NpgsqlConnection con = new NpgsqlConnection(@"Server=localhost;
                                                    Port=5432;
                                                    UserId=postgres;
                                                    Password=postgres;
                                                    Database=myschedule");
        NpgsqlCommand cmd = new NpgsqlCommand();

        /// <summary>
        /// ①更新履歴登録用のメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="scheduleId">スケジュールID</param>
        /// <param name="updateType">更新した内容</param>
        /// <param name="updateStartTime">更新したスケジュールの開始時刻</param>
        /// <param name="updateEndingTime">更新したスケジュールの終了時刻</param>
        /// <param name="subject">件名</param>
        /// <param name="detail">詳細</param>
        /// <returns>更新件数を格納したint型の変数</returns>
        internal int RegistHistory(String userId, int scheduleId, String updateType, DateTime updateStartTime,
            DateTime updateEndingTime, String subject, String detail)
        {
            CommonUtility cu = new CommonUtility();
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            String previousHashKey = GetPreviousHashKey(userId);
            DateTime updateTime = DateTime.Now;

            //テーブルに格納するハッシュキーを生成する
            String key = cu.CreateHashKey(userId, scheduleId, updateType, updateStartTime, updateEndingTime,
                subject, detail, updateTime, previousHashKey);

            int nonce = cu.GetNonce(key);

            key = cu.CreateHashKey(key, nonce.ToString());

            //SQL文の作成
            cmd.CommandText = "INSERT INTO update_history (user_id, schedule_id, update_type, update_start_time, " +
                "update_ending_time, subject, detail, key, update_time) VALUES (@userId, @scheduleId, @updateType, " +
                "@updateStartTime, @updateEndingTime, @subject, @detail, @key, now())";
            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));
            cmd.Parameters.Add(new NpgsqlParameter("@updateType", updateType));
            cmd.Parameters.Add(new NpgsqlParameter("@updateStartTime", updateStartTime));
            cmd.Parameters.Add(new NpgsqlParameter("@updateEndingTime", updateEndingTime));
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));
            cmd.Parameters.Add(new NpgsqlParameter("@key", key));

            //接続開始
            con.Open();

            try
            {
                //登録結果をresultに格納
                result = cmd.ExecuteNonQuery();
                if (result > 0)
                {
                    result = nonce;
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
            cmd.Parameters.Remove("@scheduleId");
            cmd.Parameters.Remove("@updateType");
            cmd.Parameters.Remove("@updateStartTime");
            cmd.Parameters.Remove("@updateEndingTime");
            cmd.Parameters.Remove("@subject");
            cmd.Parameters.Remove("@detail");
            cmd.Parameters.Remove("@key");

            //結果を戻す
            return result;

        }

        /// <summary>
        /// ②更新履歴をログインIDごとに取得し、データテーブルに格納するメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <returns>DBの値を格納したデータテーブル</returns>
        internal DataTable GetRegistHistoryData(String userId)
        {
            DataSet ds = new DataSet();         //データセットのインスタンス化
            DataTable dt = new DataTable();     //データテーブルのインスタンス化
            cmd.Connection = con;

            //SQL文の作成
            String sql = "SELECT history_id as 履歴ID, update_type as 変更内容, schedule_id as スケジュールID, " +
                "update_start_time as 予定開始時刻, update_ending_time as 予定終了時刻, subject as 予定, " +
                " detail as 詳細, update_time as 履歴登録日時, key as キー FROM update_history " +
                "WHERE user_id = '" + userId + "' ORDER BY history_id";

            //接続
            con.Open();

            try
            {
                //SQL文を実行し、データセットに値を入れるために必要
                //データを格納する準備
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con);

                ds.Reset();             //データ格納前にリセットしておく
                da.Fill(ds);            //データ格納
                dt = ds.Tables[0];      //今回は最初のテーブルに入っているためインデックス[0]を指定
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
        /// ③前回のハッシュキー(最も新しい履歴IDのハッシュキー)を取得するためのメソッド
        /// </summary>
        /// <returns>前回のハッシュキーを格納したString型の変数</returns>
        internal String GetPreviousHashKey(String userId)
        {
            //結果を初期化
            String result = "";
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT key FROM update_history WHERE user_id = @userId AND history_id = " +
                "(SELECT MAX(history_id) FROM update_history)";

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
                        //取得してきたキーをresultに格納
                        result = reader.GetString(0);
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
        /// ④テーブルの全レコードを取得するメソッド(管理者用)
        /// </summary>
        /// <returns>DBの値を格納したデータテーブル</returns>
        internal DataTable GetAllRegistHistory()
        {
            //DataSet、DataTableの初期化
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();

            cmd.Connection = con;

            //SQL文の作成
            String sql = "SELECT history_id as 履歴ID, user_id as ログインID, update_type as 変更内容, " +
                "schedule_id as スケジュールID, update_start_time as 予定開始時刻, update_ending_time as 予定終了時刻, " +
                "subject as 予定, detail as 詳細, update_time as 履歴登録日時, key as キー FROM update_history";

            //接続開始
            con.Open();

            try
            {
                //SQL文を実行し、データセットに値を入れるために必要
                //データを格納する準備
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, con);

                ds.Reset();             //データ格納前にリセットしておく
                da.Fill(ds);            //データ格納
                dt = ds.Tables[0];      //今回は最初のテーブルに入っているためインデックス[0]を指定
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

        internal int GetHistoryId()
        {
            int result = 0;
            cmd.Connection = con;

            cmd.CommandText = "SELECT history_id FROM update_history WHERE history_id = " +
                "(SELECT MAX(history_id) FROM update_history)";

            con.Open();

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = reader.GetInt32(0);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
            finally
            {
                con.Close();
            }
            return result;
        }

    }
}
