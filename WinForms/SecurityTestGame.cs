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
        public int nextAttackBonus = 0; // 力量手套：單次攻擊加成

        // === 敵人數值 ===
        public int enemyHP = 30;
        public int enemyMaxHP = 30;
        public int enemyAttack = 5;
        public bool enemyAlive = true;
        public int enemyLevel = 1;

        // === 商人 ===
        public int potionPrice = 1;              // 回血瓶 1 金幣回 2 HP
        public int potionHealAmount = 30;
        public int gloveePrice = 5;              // 力量手套 5 金幣，下次攻擊 +10
        public int gloveBonus = 10;
        public int berserkerPrice = 20;          // 狂戰士藥水 20 金幣，永久攻擊 +5
        public int berserkerBonus = 5;
        public int legendarySwordPrice = 9999;
        public bool hasLegendarySword = false;
        public int legendaryRingPrice = 200000000; // 2 億 - 通關條件
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
        private Button btnBuyPotion;
        private Button btnBuyGlove;
        private Button btnBuyBerserker;
        private Button btnBuySword;
        private Button btnBuyRing;
        private Label lblPotion;
        private Label lblGlove;
        private Label lblBerserker;
        private Label lblSword;
        private Label lblRing;

        public GameForm()
        {
            this.Text = "資安測試遊戲";
            this.ClientSize = new Size(680, 490);
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
            lblMessage.Text = "歡迎！攻擊敵人賺金幣。去商人買傳說之戒(2 億)即通關。";
            this.Controls.Add(lblMessage);

            grpMerchant = new GroupBox();
            grpMerchant.Text = "商人";
            grpMerchant.Location = new Point(10, 225);
            grpMerchant.Size = new Size(650, 210);
            grpMerchant.Visible = false;
            this.Controls.Add(grpMerchant);

            // Row 1: 回血瓶
            lblPotion = new Label();
            lblPotion.Location = new Point(15, 30);
            lblPotion.Size = new Size(420, 24);
            lblPotion.Text = "[1] 回血瓶  價格: " + potionPrice + " 金幣  (回復 " + potionHealAmount + " HP)";
            grpMerchant.Controls.Add(lblPotion);

            btnBuyPotion = new Button();
            btnBuyPotion.Text = "購買回血瓶";
            btnBuyPotion.Location = new Point(445, 25);
            btnBuyPotion.Size = new Size(190, 30);
            btnBuyPotion.Click += delegate { BuyPotion(); };
            grpMerchant.Controls.Add(btnBuyPotion);

            // Row 2: 力量手套
            lblGlove = new Label();
            lblGlove.Location = new Point(15, 65);
            lblGlove.Size = new Size(420, 24);
            lblGlove.Text = "[2] 力量手套  價格: " + gloveePrice + " 金幣  (下次攻擊 +" + gloveBonus + "，單次使用)";
            grpMerchant.Controls.Add(lblGlove);

            btnBuyGlove = new Button();
            btnBuyGlove.Text = "購買力量手套";
            btnBuyGlove.Location = new Point(445, 60);
            btnBuyGlove.Size = new Size(190, 30);
            btnBuyGlove.Click += delegate { BuyGlove(); };
            grpMerchant.Controls.Add(btnBuyGlove);

            // Row 3: 狂戰士藥水
            lblBerserker = new Label();
            lblBerserker.Location = new Point(15, 100);
            lblBerserker.Size = new Size(420, 24);
            lblBerserker.Text = "[3] 狂戰士藥水  價格: " + berserkerPrice + " 金幣  (攻擊力永久 +" + berserkerBonus + ")";
            grpMerchant.Controls.Add(lblBerserker);

            btnBuyBerserker = new Button();
            btnBuyBerserker.Text = "購買狂戰士藥水";
            btnBuyBerserker.Location = new Point(445, 95);
            btnBuyBerserker.Size = new Size(190, 30);
            btnBuyBerserker.Click += delegate { BuyBerserker(); };
            grpMerchant.Controls.Add(btnBuyBerserker);

            // Row 4: 傳說之劍
            lblSword = new Label();
            lblSword.Location = new Point(15, 135);
            lblSword.Size = new Size(420, 24);
            lblSword.Text = "[4] 傳說之劍  價格: " + legendarySwordPrice + " 金幣  (攻擊力 +1000)";
            grpMerchant.Controls.Add(lblSword);

            btnBuySword = new Button();
            btnBuySword.Text = "購買傳說之劍";
            btnBuySword.Location = new Point(445, 130);
            btnBuySword.Size = new Size(190, 30);
            btnBuySword.Click += delegate { BuySword(); };
            grpMerchant.Controls.Add(btnBuySword);

            // Row 5: 傳說之戒
            lblRing = new Label();
            lblRing.Location = new Point(15, 170);
            lblRing.Size = new Size(420, 24);
            lblRing.Text = "[5] 傳說之戒  價格: " + legendaryRingPrice.ToString("N0") + " 金幣  (★ 購買即通關 ★)";
            grpMerchant.Controls.Add(lblRing);

            btnBuyRing = new Button();
            btnBuyRing.Text = "購買傳說之戒";
            btnBuyRing.Location = new Point(445, 165);
            btnBuyRing.Size = new Size(190, 30);
            btnBuyRing.Click += delegate { BuyRing(); };
            grpMerchant.Controls.Add(btnBuyRing);

            btnRestart = new Button();
            btnRestart.Text = "重新開始";
            btnRestart.Location = new Point(540, 445);
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
                "攻擊力: " + attackPower + (nextAttackBonus > 0 ? " (+" + nextAttackBonus + " 下次)" : "") + "\r\n" +
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

            btnBuyPotion.Enabled = !gameEnded && playerHP < playerMaxHP;
            btnBuyGlove.Enabled = !gameEnded && nextAttackBonus == 0;
            btnBuyGlove.Text = nextAttackBonus > 0 ? "已裝備(下次攻擊)" : "購買力量手套";
            btnBuyBerserker.Enabled = !gameEnded;
            btnBuySword.Enabled = !hasLegendarySword && !gameEnded;
            btnBuySword.Text = hasLegendarySword ? "已擁有" : "購買傳說之劍";
            btnBuyRing.Enabled = !hasLegendaryRing && !gameEnded;
            btnBuyRing.Text = hasLegendaryRing ? "已擁有" : "購買傳說之戒";
        }

        private void AttackEnemy()
        {
            if (!enemyAlive || gameEnded) return;

            int damage = attackPower + nextAttackBonus;
            string msg;
            if (nextAttackBonus > 0)
            {
                msg = "力量手套發動！你對敵人造成 " + damage + " 點傷害 (基礎 " + attackPower + " + 手套 " + nextAttackBonus + ")！";
                nextAttackBonus = 0;
            }
            else
            {
                msg = "你對敵人造成 " + damage + " 點傷害！";
            }

            enemyHP -= damage;

            if (enemyHP <= 0)
            {
                enemyAlive = false;
                coins += 2; // 殺一隻只給 1 金幣
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
            // 每擊殺一隻，下一隻 HP +20、攻擊力 +1
            enemyMaxHP = 30 + (enemyLevel - 1) * 20;
            enemyHP = enemyMaxHP;
            enemyAttack = 5 + (enemyLevel - 1);
            enemyAlive = true;
            lblMessage.Text = "等級 " + enemyLevel + " 的敵人出現了！(HP " + enemyMaxHP + "、攻擊 " + enemyAttack + ")";
            UpdateUI();
        }

        private void ToggleMerchant()
        {
            grpMerchant.Visible = !grpMerchant.Visible;
            btnMerchant.Text = grpMerchant.Visible ? "關閉商人" : "前往商人";
        }

        private void BuyPotion()
        {
            if (gameEnded) return;
            if (playerHP >= playerMaxHP)
            {
                lblMessage.Text = "HP 已滿，不需要回血瓶。";
                return;
            }
            if (coins >= potionPrice)
            {
                coins -= potionPrice;
                int before = playerHP;
                playerHP += potionHealAmount;
                if (playerHP > playerMaxHP) playerHP = playerMaxHP;
                lblMessage.Text = "喝下回血瓶！HP " + before + " → " + playerHP + "。";
            }
            else
            {
                lblMessage.Text = "金幣不足！回血瓶需要 " + potionPrice + " 金幣，你只有 " + coins + "。";
            }
            UpdateUI();
        }

        private void BuyGlove()
        {
            if (gameEnded) return;
            if (nextAttackBonus > 0)
            {
                lblMessage.Text = "你已經裝備了力量手套，先用掉再買下一個。";
                return;
            }
            if (coins >= gloveePrice)
            {
                coins -= gloveePrice;
                nextAttackBonus = gloveBonus;
                lblMessage.Text = "裝備力量手套！下一次攻擊 +" + gloveBonus + "。";
            }
            else
            {
                lblMessage.Text = "金幣不足！力量手套需要 " + gloveePrice + " 金幣，你只有 " + coins + "。";
            }
            UpdateUI();
        }

        private void BuyBerserker()
        {
            if (gameEnded) return;
            if (coins >= berserkerPrice)
            {
                coins -= berserkerPrice;
                attackPower += berserkerBonus;
                lblMessage.Text = "喝下狂戰士藥水！攻擊力永久 +" + berserkerBonus + "，現在是 " + attackPower + "。";
            }
            else
            {
                lblMessage.Text = "金幣不足！狂戰士藥水需要 " + berserkerPrice + " 金幣，你只有 " + coins + "。";
            }
            UpdateUI();
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
            nextAttackBonus = 0;
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
