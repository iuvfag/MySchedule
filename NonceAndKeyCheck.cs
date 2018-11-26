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
    public partial class NonceAndKeyCheck : Form
    {

        public String userId { get; set; }
        UpdateHistoryDAO uhDAO = new UpdateHistoryDAO();
        UpdateHistoryDTO uhDTO = new UpdateHistoryDTO();
        List<UpdateHistoryDTO> uhDTOList = new List<UpdateHistoryDTO>();

        public NonceAndKeyCheck()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                return;
            }
            button1.Enabled = false;
            label1.Text = "処理中";
            uhDTOList = uhDAO.getAllInfoWhichHasNoNonce(userId);

            progressBar1.Minimum = 0;
            progressBar1.Maximum = uhDTOList.Count;
            progressBar1.Value = 0;
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = (BackgroundWorker)sender;
            
            List<int> emptyNonceList = new List<int>();
            List<int> nonceList = new List<int>();
            List<String> hashKeyList = new List<String>();
            BlockChain bc = new BlockChain();
            uhDTOList = uhDAO.getAllInfoWhichHasNoNonce(userId);
            int maxLoops = 0;

            if (uhDTOList.Count > 0)
            {
                for (int i = 0; i < uhDTOList.Count; i++)
                {
                    maxLoops = uhDTOList.Count;
                    if (i == 0)
                    {
                        uhDTOList[i].previousHashKey = uhDAO.GetPreviousHashKey(userId, uhDTOList[i].historyId - 1);
                    }
                    else
                    {
                        uhDTOList[i].previousHashKey = uhDTOList[i - 1].hashKey;
                    }
                    uhDTOList[i] = bc.Block(uhDTOList[i]);
                    emptyNonceList.Add(uhDTOList[i].historyId);
                    nonceList.Add(uhDTOList[i].nonce);
                    hashKeyList.Add(uhDTOList[i].hashKey);
                    bgWorker.ReportProgress(i);
                }
                uhDAO.UpdateNonce(emptyNonceList, nonceList, hashKeyList);

                e.Result = maxLoops;

            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                label1.Text = "エラー：" + e.Error.Message;
            }

            this.Dispose();
        }
    }
}
