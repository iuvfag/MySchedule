using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MySchedule
{
    public partial class NonceAndKeyCheck : Form
    {

        /* ハッシュキーとナンスを割り出し
         * 登録用のDAOメソッドに渡す処理を行うクラス*/

        public String userId { get; set; }
        UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();
        UpdateHistoryDTO uhDTO = new UpdateHistoryDTO();
        List<UpdateHistoryDTO> uhDTOList = new List<UpdateHistoryDTO>();

        public NonceAndKeyCheck()
        {
            InitializeComponent();
        }

        /// <summary>
        /// フォーム読み込み時での処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonceAndKeyCheck_Load(object sender, EventArgs e)
        {
            //進捗バーの値を初期化しておく
            progressBar1.Minimum = 0;       //バーの最小値
            progressBar1.Maximum = 1;       //バーの最大値(仮に1としておく)
            progressBar1.Value = 0;         //バーの値

            button2.Enabled = false;
        }

        //ボタン1押下時の動作
        private void button1_Click(object sender, EventArgs e)
        {
            //既にバックグラウンド処理が行われている場合
            if (backgroundWorker1.IsBusy)
            {
                //処理を終了
                return;
            }
            //一度クリックされるともう一度押せないようにしておく
            button1.Enabled = false;
            //UpdateHistoryDTO型のリストにナンスがnullになっている編集履歴の情報を取得
            uhDTOList = uhDAO.getAllInfoWhichHasNull(userId);

            //リストの要素が一つでも存在する場合
            if (uhDTOList.Count > 0)
            {
                //進捗バーの最大値はリストの要素数としておく
                progressBar1.Maximum = uhDTOList.Count;
                //BackGroundWorkerのProgressChengedイベントが発生するようにする
                backgroundWorker1.WorkerReportsProgress = true;
                //キャンセルできるようにする
                backgroundWorker1.WorkerSupportsCancellation = true;
                //バックグラウンド処理の開始
                backgroundWorker1.RunWorkerAsync();
                //ラベルに文字列を表示
                label1.Text = "処理中...(処理には時間がかかります)";
                //キャンセルボタンを有効化しておく
                button2.Enabled = true;


            }
            //リストの要素が1つも存在しなかった場合
            else
            {
                //メッセージを表示してフォームを閉じる
                MessageBox.Show("ブロックチェーンは既に生成されています", "情報は最新です");
                this.Dispose();

            }

        }

        /// <summary>
        /// バックグラウンド処理の実際の処理内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = (BackgroundWorker)sender;

            List<int> emptyNonceList = new List<int>();       //ナンスが登録されていない編集履歴の履歴IDを格納するリスト
            List<int> nonceList = new List<int>();            //実際に登録するナンスを格納するリスト
            List<String> previousHashKeyList = new List<string>();
            List<String> hashKeyList = new List<String>();    //ハッシュキーを格納するリスト
            BlockChain bc = new BlockChain();   //ブロックチェーンクラスのインスタンス化

            int maxLoops = 0;

            //リストの要素数for文を回す
            //各インデックスの要素に対して順番に処理を行う
            for (int i = 0; i < uhDTOList.Count; i++)
            {
                maxLoops = uhDTOList.Count;

                //処理がキャンセルされたかどうかを調べる
                if (bgWorker.CancellationPending)
                {
                    //キャンセルされている場合は処理を中止
                    e.Cancel = true;
                    if (maxLoops == 1)
                    {

                    }
                    return;
                }
                //まず、取得してきた履歴IDの中から最も若いIDかどうかで処理を分ける
                //最も若いIDの場合
                if (i == 0)
                {
                    //ひとつ前の履歴IDのハッシュキーをリストの「前のハッシュキー」に格納
                    uhDTOList[i].previousHashKey = uhDAO.GetPreviousHashKey(userId, uhDTOList[i].historyId - 1);
                }
                //そうでない場合
                else
                {
                    //リストの中から一つ前のインデックスのハッシュキーをリストの「前のハッシュキー」に格納
                    uhDTOList[i].previousHashKey = uhDTOList[i - 1].hashKey;
                }
                uhDTOList[i] = bc.Block(uhDTOList[i]);      //リストの要素をそのままBlockメソッドに渡す
                emptyNonceList.Add(uhDTOList[i].historyId); //履歴IDを履歴ID格納用リストに追加
                nonceList.Add(uhDTOList[i].nonce);          //ナンスをナンス用リストに追加
                previousHashKeyList.Add(uhDTOList[i].previousHashKey);
                hashKeyList.Add(uhDTOList[i].hashKey);      //ハッシュキーをハッシュキー用リストに追加
                bgWorker.ReportProgress(i);                 //バックグラウンド処理の進捗状況をfor文に合わせて更新
            }
            //ナンス、ハッシュキー登録用のメソッドにそれぞれのリストを渡す
            uhDAO.UpdateNonce(emptyNonceList, nonceList, hashKeyList, previousHashKeyList);

            e.Result = maxLoops;

        }

        /// <summary>
        /// バックグラウンド処理の身長状況が変わるたびに呼ばれる処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //進捗バーの値を進捗状況に応じて更新していく
            progressBar1.Value = e.ProgressPercentage;

        }

        /// <summary>
        /// バックグラウンド処理が終了した際に呼ばれる処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //エラーが発生した場合
            if (e.Error != null)
            {
                //エラー内容を表示
                label1.Text = "エラー：" + e.Error.Message;
            }
            //処理がキャンセルされた場合
            else if (e.Cancelled)
            {
                //メッセージを表示してフォームを閉じる
                MessageBox.Show("処理をキャンセルしました", "処理中止");
                this.Dispose();
            }
            //エラーが発生しなかった場合
            else
            {
                //ラベルの文字更新
                label1.Text = "完了";
                //進捗バーの値を最大値まで更新
                progressBar1.Value = progressBar1.Maximum;
                //次の処理までの時間を少し開ける
                Thread.Sleep(1000);
                //メッセージを表示してフォームを閉じる
                MessageBox.Show("処理は正常に終了しました", "処理終了");
                this.Dispose();
            }

        }

        /// <summary>
        /// フォームが閉じられる場合の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NonceAndKeyCheck_FormClosing(object sender, FormClosingEventArgs e)
        {
            //バックグラウンド処理がまだ処理中かどうかをチェックする
            //処理中の場合
            if (backgroundWorker1.IsBusy)
            {
                //フォームが閉じられないようにする
                e.Cancel = true;
            }
        }

        /// <summary>
        /// キャンセルボタンが押下された場合の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //既にバックグラウンド処理が実行中であれば
            if (backgroundWorker1.IsBusy)
            {
                //ボタンの操作を禁止しておく
                button2.Enabled = false;
                //ラベルの文字の更新
                label1.Text = "処理を中止しています...(これには時間がかかることがあります)";
                //キャンセルする
                backgroundWorker1.CancelAsync();
            }
            //実行中でなければ何もしない
            else
            {
                return;
            }
        }
    }
}
