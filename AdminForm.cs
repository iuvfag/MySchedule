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
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();
        }

        //管理者ページ読み込み時の動作
        private void AdminForm_Load(object sender, EventArgs e)
        {
            //データグリッドにDBの値を格納
            UserInfoDAO uiDAO = new UserInfoDAO();                          //UserInfoDAOのインスタンス化
            RegistHistoryDAO rhDAO = new RegistHistoryDAO();                //RegistHistoryDAOのインスタンス化

            //それぞれのデータグリッドにDBから渡されたデータテーブルを格納
            userInfoGrid.DataSource = uiDAO.getAllUserInfo();
            updateHistoryInfoGrid.DataSource = rhDAO.getAllRegistHistory();
        }
    }
}
