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
    public partial class ResetPassword : Form
    {

        //パスワード変更用のメソッド

        public String userId { get; set; }

        public ResetPassword()
        {
            InitializeComponent();
        }

        private void ResetPassword_Load(object sender, EventArgs e)
        {
            //入力内容を隠す
            textBox1.PasswordChar = '*';
            textBox2.PasswordChar = '*';
            textBox3.PasswordChar = '*';
        }

        /// <summary>
        /// 「戻る」ボタンが押された場合の動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //このフォームを閉じる
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //入力欄の値をそれぞれフィールド変数に格納
            String currentPassword = textBox1.Text;
            String newPassword = textBox2.Text;
            String reConfirmationNewPassword = textBox3.Text;

            //入力内容の正誤チェック
            String currentPasswordCheck = InputChecker.doCheck(currentPassword, "現在のパスワード", 5, 15);
            String newPasswordCheck = InputChecker.doCheck(newPassword, "新しいパスワード", 5, 15);
            String reConfirmarionNewPasswordCheck = InputChecker.doCheck(reConfirmationNewPassword,
                "新しいパスワード(再確認用)", 5, 15);
            
            //新しいパスワードと再確認用パスワードが等しいか調べる
            String passwordCompare = InputChecker.passwordCompare(newPassword, reConfirmationNewPassword,
                "新しいパスワード", "新しいパスワード(再確認用)");

            //すべてのチェックに問題がなければ次の処理へ
            if (currentPasswordCheck == "" && newPasswordCheck == "" && reConfirmarionNewPasswordCheck == "" &&
                passwordCompare == "")
            {
                String userIdHash = InputChecker.createHashKey(userId);

                currentPassword = InputChecker.createHashKey(currentPassword);

                currentPassword = InputChecker.createHashKey(userIdHash, currentPassword);

                newPassword = InputChecker.createHashKey(newPassword);
                newPassword = InputChecker.createHashKey(userIdHash, newPassword);

                //ログインIDとパスワードをもとに、まず該当するユーザーがいるかどうか調べる
                if (UserInfoDAO.isExistsUser(userId, currentPassword))
                {
                    //新しいパスワードを設定するメソッドの呼び出し、更新件数を格納したresultを受け取る
                    int result = UserInfoDAO.resetPassword(userId, currentPassword, newPassword);

                    //更新件数が1件以上なら
                    if (result > 0)
                    {
                        //メッセージ表示
                        MessageBox.Show("パスワードを変更しました", "パスワード再設定完了");
                        this.Close();
                    }
                    //更新されたものがなければ
                    else
                    {
                        //エラーメッセージ表示
                        MessageBox.Show("パスワードが変更できませんでした", "エラーが発生しました");
                    }
                }
                //該当するユーザーがいなかった倍
                else
                {
                    //エラーメッセージ表示
                    MessageBox.Show("パスワードが異なります", "入力内容を確認してください");
                }


            }
            //パスワードが空欄、あるいは文字数違反の場合
            if (currentPasswordCheck != "")
            {
                //エラーメッセージ表示
                MessageBox.Show(currentPasswordCheck, "入力内容を確認してください");
            }
            //新しいパスワードが空欄、あるいは文字数違反の場合
            if (newPasswordCheck != "")
            {
                //エラーメッセージ表示
                MessageBox.Show(newPasswordCheck, "入力内容を確認してください");
            }
            //新しいパスワード(再確認用)が空欄、あるいは文字列違反の場合
            if (reConfirmarionNewPasswordCheck != "")
            {
                //エラーメッセージの表示
                MessageBox.Show(reConfirmarionNewPasswordCheck, "入力内容を確認してください");
            }
            //②綱パスワードが一致しない場合
            if (passwordCompare != "")
            {
                //エラーメッセージ表示
                MessageBox.Show(passwordCompare, "入力内容を確認してください");
            }
        }


    }
}
