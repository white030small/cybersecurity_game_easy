using System;
using System.Drawing;
using System.Windows.Forms;

namespace SecurityTestGame
{
    /// <summary>
    /// 資安測試遊戲 - WinForms 單檔
    /// 故意保留的客戶端漏洞：
    ///   1. 所有狀態都是 public 欄位 - Cheat Engine 容易掃描修改
    ///   2. 商人價格驗證全在客戶端 - dnSpy 反編譯可直接改 if 判斷
    ///   3. 無混淆、無完整性檢查、無反調試
    /// </summary>
    public class GameForm : Form
    {
        // === 玩家數值 (public 故意的，方便記憶體掃描) ===
        public int playerHP = 100;
        public int playerMaxHP = 100;
        public int coins = 0;
        public int attackPower = 10;

        // === 敵人數值 ===
        public int enemyHP = 30;
        public int enemyMaxHP = 30;
        public int enemyAttack = 5;
        public bool enemyAlive = true;
        public int enemyLevel = 1;

        // === 商人 ===
        public int legendarySwordPrice = 9999;
        public bool hasLegendarySword = false;
        public int legendaryRingPrice = 10000000; // 1000 萬 - 通關條件
        public bool hasLegendaryRing = false;
        public bool gameEnded = false;

        // === UI 控制項 ===
        private Label lblPlayerStats;
        private Label lblEnemyStats;
        private Label lblMessage;
        private Button btnAttack;
        private Button btnSpawn;
        private Button btnMerchant;
        private Button btnRestart;
        private GroupBox grpMerchant;
        private Button btnBuySword;
        private Button btnBuyRing;
        private Label lblSword;
        private Label lblRing;

        public GameForm()
        {
            this.Text = "資安測試遊戲";
            this.ClientSize = new Size(680, 460);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft JhengHei UI", 10);

            BuildUI();
            UpdateUI();
        }

        private void BuildUI()
        {
            lblPlayerStats = new Label();
            lblPlayerStats.Location = new Point(10, 10);
            lblPlayerStats.Size = new Size(320, 100);
            lblPlayerStats.BorderStyle = BorderStyle.FixedSingle;
            lblPlayerStats.Padding = new Padding(8);
            lblPlayerStats.BackColor = Color.FromArgb(230, 245, 230);
            this.Controls.Add(lblPlayerStats);

            lblEnemyStats = new Label();
            lblEnemyStats.Location = new Point(340, 10);
            lblEnemyStats.Size = new Size(320, 100);
            lblEnemyStats.BorderStyle = BorderStyle.FixedSingle;
            lblEnemyStats.Padding = new Padding(8);
            lblEnemyStats.BackColor = Color.FromArgb(245, 230, 230);
            this.Controls.Add(lblEnemyStats);

            btnAttack = new Button();
            btnAttack.Text = "攻擊！(擊殺得 1 金幣)";
            btnAttack.Location = new Point(340, 115);
            btnAttack.Size = new Size(320, 40);
            btnAttack.Click += delegate { AttackEnemy(); };
            this.Controls.Add(btnAttack);

            btnSpawn = new Button();
            btnSpawn.Text = "召喚下一隻敵人";
            btnSpawn.Location = new Point(340, 115);
            btnSpawn.Size = new Size(320, 40);
            btnSpawn.Click += delegate { SpawnEnemy(); };
            this.Controls.Add(btnSpawn);

            btnMerchant = new Button();
            btnMerchant.Text = "前往商人";
            btnMerchant.Location = new Point(10, 115);
            btnMerchant.Size = new Size(320, 40);
            btnMerchant.Click += delegate { ToggleMerchant(); };
            this.Controls.Add(btnMerchant);

            lblMessage = new Label();
            lblMessage.Location = new Point(10, 165);
            lblMessage.Size = new Size(650, 50);
            lblMessage.BorderStyle = BorderStyle.FixedSingle;
            lblMessage.Padding = new Padding(8);
            lblMessage.BackColor = Color.FromArgb(255, 252, 230);
            lblMessage.Text = "歡迎！攻擊敵人賺金幣。去商人買傳說之戒(1000萬)即通關。";
            this.Controls.Add(lblMessage);

            grpMerchant = new GroupBox();
            grpMerchant.Text = "商人";
            grpMerchant.Location = new Point(10, 225);
            grpMerchant.Size = new Size(650, 180);
            grpMerchant.Visible = false;
            this.Controls.Add(grpMerchant);

            lblSword = new Label();
            lblSword.Location = new Point(15, 25);
            lblSword.Size = new Size(620, 22);
            lblSword.Text = "[1] 傳說之劍  價格: " + legendarySwordPrice + " 金幣  (攻擊力 +1000)";
            grpMerchant.Controls.Add(lblSword);

            btnBuySword = new Button();
            btnBuySword.Text = "購買傳說之劍";
            btnBuySword.Location = new Point(15, 50);
            btnBuySword.Size = new Size(200, 32);
            btnBuySword.Click += delegate { BuySword(); };
            grpMerchant.Controls.Add(btnBuySword);

            lblRing = new Label();
            lblRing.Location = new Point(15, 95);
            lblRing.Size = new Size(620, 22);
            lblRing.Text = "[2] 傳說之戒  價格: " + legendaryRingPrice.ToString("N0") + " 金幣  (★ 購買即通關 ★)";
            grpMerchant.Controls.Add(lblRing);

            btnBuyRing = new Button();
            btnBuyRing.Text = "購買傳說之戒";
            btnBuyRing.Location = new Point(15, 120);
            btnBuyRing.Size = new Size(200, 32);
            btnBuyRing.Click += delegate { BuyRing(); };
            grpMerchant.Controls.Add(btnBuyRing);

            btnRestart = new Button();
            btnRestart.Text = "重新開始";
            btnRestart.Location = new Point(540, 415);
            btnRestart.Size = new Size(120, 35);
            btnRestart.Click += delegate { RestartGame(); };
            this.Controls.Add(btnRestart);
        }

        private void UpdateUI()
        {
            lblPlayerStats.Text =
                "=== 玩家 ===\r\n" +
                "HP: " + playerHP + " / " + playerMaxHP + "\r\n" +
                "金幣: " + coins + "\r\n" +
                "攻擊力: " + attackPower + "\r\n" +
                "傳說之劍: " + (hasLegendarySword ? "有" : "無") +
                "   傳說之戒: " + (hasLegendaryRing ? "有" : "無");

            if (enemyAlive)
            {
                lblEnemyStats.Text =
                    "=== 敵人 (Lv." + enemyLevel + ") ===\r\n" +
                    "HP: " + enemyHP + " / " + enemyMaxHP + "\r\n" +
                    "攻擊力: " + enemyAttack;
                btnAttack.Visible = true;
                btnSpawn.Visible = false;
                btnAttack.Enabled = playerHP > 0 && !gameEnded;
            }
            else
            {
                lblEnemyStats.Text = "=== 敵人已被擊敗 ===\r\n\r\n按右邊按鈕召喚新敵人";
                btnAttack.Visible = false;
                btnSpawn.Visible = true;
                btnSpawn.Enabled = !gameEnded;
            }

            btnBuySword.Enabled = !hasLegendarySword && !gameEnded;
            btnBuySword.Text = hasLegendarySword ? "已擁有" : "購買傳說之劍";
            btnBuyRing.Enabled = !hasLegendaryRing && !gameEnded;
            btnBuyRing.Text = hasLegendaryRing ? "已擁有" : "購買傳說之戒";
        }

        private void AttackEnemy()
        {
            if (!enemyAlive || gameEnded) return;

            enemyHP -= attackPower;
            string msg = "你對敵人造成 " + attackPower + " 點傷害！";

            if (enemyHP <= 0)
            {
                enemyAlive = false;
                coins += 1; // 殺一隻只給 1 金幣
                msg = "敵人被擊敗！獲得 1 金幣。";
            }
            else
            {
                playerHP -= enemyAttack;
                msg += " 敵人反擊造成 " + enemyAttack + " 點傷害！";
                if (playerHP <= 0)
                {
                    playerHP = 0;
                    msg = "你被擊敗了... 按「重新開始」繼續。";
                }
            }
            lblMessage.Text = msg;
            UpdateUI();
        }

        private void SpawnEnemy()
        {
            enemyLevel++;
            enemyMaxHP = 30 + (enemyLevel - 1) * 5;
            enemyHP = enemyMaxHP;
            enemyAttack = 5 + (enemyLevel - 1);
            enemyAlive = true;
            lblMessage.Text = "等級 " + enemyLevel + " 的敵人出現了！";
            UpdateUI();
        }

        private void ToggleMerchant()
        {
            grpMerchant.Visible = !grpMerchant.Visible;
            btnMerchant.Text = grpMerchant.Visible ? "關閉商人" : "前往商人";
        }

        private void BuySword()
        {
            if (hasLegendarySword) return;
            if (coins >= legendarySwordPrice)
            {
                coins -= legendarySwordPrice;
                hasLegendarySword = true;
                attackPower += 1000;
                lblMessage.Text = "購買成功！傳說之劍讓攻擊力 +1000！";
            }
            else
            {
                lblMessage.Text = "金幣不足！需要 " + legendarySwordPrice + " 金幣，你只有 " + coins + "。";
            }
            UpdateUI();
        }

        private void BuyRing()
        {
            if (hasLegendaryRing) return;
            if (coins >= legendaryRingPrice)
            {
                coins -= legendaryRingPrice;
                hasLegendaryRing = true;
                gameEnded = true;
                lblMessage.Text = "★ 恭喜通關！你購買了傳說之戒！★";
                UpdateUI();

                MessageBox.Show(
                    "★★★ 遊戲通關 ★★★\r\n\r\n" +
                    "你獲得了傳說之戒，成為了這片土地的英雄！\r\n\r\n" +
                    "最終金幣: " + coins + "\r\n" +
                    "最終攻擊力: " + attackPower + "\r\n" +
                    "擊敗敵人等級: " + enemyLevel + "\r\n" +
                    "擁有傳說之劍: " + (hasLegendarySword ? "是" : "否"),
                    "通關",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                lblMessage.Text = "金幣不足！傳說之戒需要 " + legendaryRingPrice.ToString("N0") + " 金幣，你只有 " + coins + "。";
            }
            UpdateUI();
        }

        private void RestartGame()
        {
            playerHP = playerMaxHP;
            coins = 0;
            attackPower = 10;
            hasLegendarySword = false;
            hasLegendaryRing = false;
            gameEnded = false;
            enemyLevel = 0;
            SpawnEnemy();
            lblMessage.Text = "遊戲重新開始！";
            UpdateUI();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm());
        }
    }
}
