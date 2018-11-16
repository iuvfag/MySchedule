using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace MySchedule
{
    class NonceInfoDAO
    {

        /// <summary>
        /// nonce_infoテーブルに対する処理をまとめたメソッド
        /// </summary>

        NpgsqlConnection con = new NpgsqlConnection(@"Server=localhost;
                                                    Port=5432;
                                                    UserId=postgres;
                                                    Password=postgres;
                                                    Database=myschedule");
        NpgsqlCommand cmd = new NpgsqlCommand();

        /// <summary>
        /// Nonce登録用のメソッド
        /// </summary>
        /// <param name="userId">ログインID</param>
        /// <param name="historyId">履歴ID</param>
        /// <param name="scheduleId">スケジュールID</param>
        /// <param name="nonce">Nonce</param>
        /// <returns>登録件数</returns>
        internal int RegistNonce(String userId, int historyId, int scheduleId, int nonce)
        {
            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "INSERT INTO nonce_info (user_id, history_id, schedule_id, nonce)" +
                "VALUES(@userId, @historyId, @scheduleId, @nonce)";

            //SQL文の@部分に値を格納
            cmd.Parameters.Add(new NpgsqlParameter("@userId", userId));
            cmd.Parameters.Add(new NpgsqlParameter("@historyId", historyId));
            cmd.Parameters.Add(new NpgsqlParameter("@scheduleId", scheduleId));
            cmd.Parameters.Add(new NpgsqlParameter("@nonce", nonce));

            //接続開始
            con.Open();

            try
            {
                //SQLの実行結果をresultに格納
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
        /// 履歴IDをもとにテーブルからNonceを取得するためのメソッド
        /// </summary>
        /// <param name="historyId">履歴ID</param>
        /// <returns>Nonceを格納したint型の変数</returns>
        internal int GetNonce(int historyId)
        {
            //結果の初期化
            int result = 0;
            cmd.Connection = con;

            //SQL文の作成
            cmd.CommandText = "SELECT nonce FROM nonce_info WHERE history_id = @historyId";

            //SQL文の@部分に値を格納
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
                        //取得してきた値をresultに格納
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
            //結果を戻す
            return result;
        }

    }
}
