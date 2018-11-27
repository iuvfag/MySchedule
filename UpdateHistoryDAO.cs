using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Transactions;
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
            DateTime updateEndingTime, String subject, String detail, DateTime updateTime)
        {
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "INSERT INTO update_history (user_id, schedule_id, update_type, update_start_time, " +
                "update_ending_time, subject, detail, update_time) VALUES (@userId, @scheduleId, @updateType, " +
                "@updateStartTime, @updateEndingTime, @subject, @detail, @updateTime)";
            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));                         //ログインID
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));                 //スケジュールID
            cmd.Parameters.Add(new NpgsqlParameter("@updateType", updateType));                 //変更内容
            cmd.Parameters.Add(new NpgsqlParameter("@updateStartTime", updateStartTime));       //予定開始時刻
            cmd.Parameters.Add(new NpgsqlParameter("@updateEndingTime", updateEndingTime));     //予定終了時刻
            cmd.Parameters.Add(new NpgsqlParameter("@subject", subject));                       //件名
            cmd.Parameters.Add(new NpgsqlParameter("@detail", detail));                         //詳細
            cmd.Parameters.Add(new NpgsqlParameter("@updateTime", updateTime));                 //履歴登録日時

            //接続開始
            con.Open();

            try
            {
                //登録結果をresultに格納
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
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@scheduleId");
            cmd.Parameters.Remove("@updateType");
            cmd.Parameters.Remove("@updateStartTime");
            cmd.Parameters.Remove("@updateEndingTime");
            cmd.Parameters.Remove("@subject");
            cmd.Parameters.Remove("@detail");
            cmd.Parameters.Remove("@updateTime");

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
                " detail as 詳細, update_time as 履歴登録日時, nonce as ノンス, key as キー FROM update_history " +
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
        internal String GetPreviousHashKey(String userId, int historyId)
        {
            //結果を初期化
            String result = "";
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT key FROM update_history WHERE user_id = @userId AND history_id = @historyId";

            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));     //ログインID
            cmd.Parameters.Add(new NpgsqlParameter("@historyId", historyId));

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
            //パラメーターの値はRemoveしておく
            cmd.Parameters.Remove("@userId");
            cmd.Parameters.Remove("@historyId");
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
            String sql = "SELECT history_id as 履歴ID, update_type as 変更内容, schedule_id as スケジュールID, " +
                "update_start_time as 予定開始時刻, update_ending_time as 予定終了時刻, subject as 予定, " +
                "detail as 詳細, update_time as 履歴登録日時, nonce as ノンス, key as キー " +
                "FROM update_history";

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

        /// <summary>
        /// 最新の履歴IDを取得するためのメソッド
        /// </summary>
        /// <returns></returns>
        internal int GetHistoryId(String userId)
        {
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT history_id FROM update_history WHERE user_id = @userId AND " +
                "history_id =  (SELECT MAX(history_id) FROM update_history)";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));     //ログインID

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
                        //取得した履歴IDをresultに格納
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
            //結果を戻す
            return result;
        }

        /// <summary>
        /// 編集履歴テーブルからNonceの値が空欄なものの全情報をを取得しDTOを格納したListを戻すメソッド
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>情報を格納したDTOを格納したList</returns>
        internal List<UpdateHistoryDTO> getAllInfoWhichHasNoNonce(String userId)
        {
            //結果を格納するDTOクラスとListをインスタンス化しておく
            List<UpdateHistoryDTO> uhDTOList = new List<UpdateHistoryDTO>();
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT * FROM update_history WHERE user_id = @userId AND nonce IS null " +
                "ORDER BY history_id ASC";
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));     //ログインID

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
                        UpdateHistoryDTO uhDTO = new UpdateHistoryDTO()
                        {

                            //まず、取得した情報を該当のDTOのパラメーターに格納していく
                            historyId = reader.GetInt32(0),
                            userId = userId,
                            updateType = reader.GetString(2),
                            scheduleId = reader.GetInt32(3),
                            updateStartTime = reader.GetDateTime(4),
                            updateEndingTime = reader.GetDateTime(5),
                            subject = reader.GetString(6),
                            detail = reader.GetString(7),
                            updateTime = reader.GetDateTime(8)
                        };
                        //情報を格納したDTOをListに格納
                        uhDTOList.Add(uhDTO);
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
            //パラメーターの値はRemoveしておく
            cmd.Parameters.Remove("@userId");
            //結果を戻す
            return uhDTOList;
        }


        /// <summary>
        /// 引数として渡された履歴IDをもとにNonceとハッシュキーを編集履歴テーブルに登録するメソッド
        /// </summary>
        /// <param name="historyIdList">履歴IDを格納したリスト</param>
        /// <param name="nonceList">Nonceを格納したリスト</param>
        /// <param name="hashKeyList">ハッシュキーを格納したリスト</param>
        /// <returns></returns>
        internal int UpdateNonce(List<int> historyIdList, List<int> nonceList, List<String> hashKeyList) 
        {
            //結果を初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成(今回はSQL文内でfor文を使用)
            cmd.CommandText = "DROP FUNCTION IF EXISTS update_nonce(integer[], integer[], text[]);" +
                "CREATE OR REPLACE FUNCTION update_nonce(historyIdList integer[], nonceList integer[], " +
                    "hashKeyList text[])" +
                "RETURNS VOID AS $$ " +
                    "BEGIN " +
                        "FOR i IN 1 .. array_length(historyIdList, 1) LOOP " +
                            "UPDATE update_history SET nonce = nonceList[i], key = hashKeyList[i] " +
                            "WHERE history_id = historyIdList[i]; " +
                        "END LOOP;" +
                    "END;" +
                "$$ LANGUAGE plpgsql;" +
                "SELECT update_nonce(@historyIdList, @nonceList, @hashKeyList);";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@historyIdList", historyIdList));
            cmd.Parameters.Add(new NpgsqlParameter("@nonceList", nonceList));
            cmd.Parameters.Add(new NpgsqlParameter("@hashKeyList", hashKeyList));

            //接続開始
            con.Open();

            try
            {
                //実行結果をresultに格納
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
            //パラメーターの値はRemoveしておく
            cmd.Parameters.Remove("@historyIdList");
            cmd.Parameters.Remove("@nonceList");
            cmd.Parameters.Remove("@hashKeyList");

            //結果を戻す
            return result;
        }

    }
}
