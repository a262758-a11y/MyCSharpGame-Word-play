using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic; // 修正 using

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        private List<string> words = new List<string>() {
            "蘋果", "香蕉", "跑步", "跳躍", "學習", "程式設計", "資料庫  ", "網路", "咖啡", "電腦",
            "書本", "老師", "同學", "運動", "遊戲", "寫字", "閱讀", "教室", "午餐", "學校" };

        private Random random = new Random();

        // 字元 => 正確部首
        private Dictionary<char, string> radicalDict = new Dictionary<char, string>()
        {
            { '蘋', "艹" }, { '果', "木" }, { '香', "禾" }, { '蕉', "艹" }, { '跑', "足" }, { '跳', "足" },
            { '習', "白" }, { '資', "貝" }, { '料', "米" }, { '電', "雨" }, { '網', "糸" }, { '咖', "口" },
            { '啡', "口" }, { '程', "禾" }, { '式', "工" }, { '設', "言" }, { '計', "言" }, { '路', "足" },
            { '書', "聿" }, { '本', "木" }, { '師', "巾" }, { '同', "口" }, { '運', "辵" }, { '動', "力" },
            { '遊', "辵" }, { '戲', "戈" }, { '寫', "宀" }, { '字', "子" }, { '閱', "門" }, { '讀', "言" },
            { '教', "攵" }, { '室', "宀" }, { '午', "十" }, { '餐', "食" }, { '校', "木" }, { '學', "子" }
        };

        // 字元 => 去除部首後的部分
        private Dictionary<char, string> radicalRemovedDict = new Dictionary<char, string>()
        {
            { '蘋', "頻" }, { '果', "田" }, { '香', "日" }, { '蕉', "焦" }, { '跑', "包" }, { '跳', "兆" },
            { '習', "羽" }, { '資', "次" }, { '料', "斗" }, { '電', "申" }, { '網', "罔" }, { '咖', "加" },
            { '啡', "非" }, { '程', "呈" }, { '式', "弋" }, { '設', "殳" }, { '計', "十" }, { '路', "各" },
            { '書', "者" }, { '本', "夲" }, { '師', "帀" }, { '同', "冂" }, { '運', "軍" }, { '動', "重" },
            { '遊', "斿" }, { '戲', "虛" }, { '寫', "與" }, { '字', "宀" }, { '閱', "兌" }, { '讀', "賣" },
            { '教', "孝" }, { '室', "至" }, { '午', "干" }, { '餐', "歺" }, { '校', "交" }, { '學', "冖" }
        };

        private int score = 0;
        private int maxScore = 10; // 可調整的分數上限
        private int currentWordIndex = -1;
        private int currentCharIndex = -1;
        private char currentChar;
        private Label lblQuestion;
        private Label lblScore;
        private List<string> currentOptions = new List<string>();
        private Button btnExit; // 結束遊戲按鈕
        private Button btnHint; // 提示按鈕
        private int hintUsed = 0; // 提示次數計數

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 啟動時可讓使用者輸入分數 上限
            string input = Microsoft.VisualBasic.Interaction.InputBox("請輸入滿分（預設10分）：", "設定滿分", maxScore.ToString());
            int userMax;
            if (int.TryParse(input, out userMax) && userMax > 0)
                maxScore = userMax;

            this.Controls.Clear();
            lblScore = new Label();
            lblScore.Text = $"分數：{score} / {maxScore}";
            lblScore.Font = new Font("微軟正黑體", 14);
            lblScore.Location = new Point(10, 10);
            lblScore.AutoSize = true;
            this.Controls.Add(lblScore);

            // 新增結束遊戲按鈕
            btnExit = new Button();
            btnExit.Text = "結束遊戲";
            btnExit.Font = new Font("微軟正黑體", 14);
            btnExit.Size = new Size(120, 40);
            btnExit.Location = new Point(200, 10);
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);

            // 新增提示按鈕
            btnHint = new Button();
            btnHint.Text = "提示";
            btnHint.Font = new Font("微軟正黑體", 14);
            btnHint.Size = new Size(120, 40);
            btnHint.Location = new Point(350, 10);
            btnHint.Click += BtnHint_Click;
            this.Controls.Add(btnHint);

            NextQuestion();
        }

        private void NextQuestion()
        {
            // 檢查分數是否達到上限
            if (score >= maxScore)
            {
                MessageBox.Show("恭喜通過！", "遊戲結束");
                Application.Exit();
                return;
            }

            // 清除舊的題目和選項
            foreach (Control c in this.Controls.OfType<Button>().ToList())
            {
                if (c != btnExit) // 保留結束遊戲按鈕
                    this.Controls.Remove(c);
            }
            if (lblQuestion != null)
                this.Controls.Remove(lblQuestion);

            // 隨機選擇一個詞和字元
            currentWordIndex = random.Next(words.Count);
            string word = words[currentWordIndex];
            currentCharIndex = random.Next(word.Length);
            currentChar = word[currentCharIndex];

            // 顯示題目（去除部首後的字）
            string displayWord = word;
            if (radicalRemovedDict.ContainsKey(currentChar))
            {
                StringBuilder sb = new StringBuilder(word);
                sb[currentCharIndex] = radicalRemovedDict[currentChar][0];
                displayWord = sb.ToString();
            }

            lblQuestion = new Label();
            lblQuestion.Text = $"請選出「{word}」第{currentCharIndex + 1}個字的部首：「{displayWord}」";
            lblQuestion.Font = new Font("微軟正黑體", 16);
            lblQuestion.Location = new Point(10, 50);
            lblQuestion.AutoSize = true;
            this.Controls.Add(lblQuestion);

            // 準備部首選項（正確部首 + 2個隨機部首）
            currentOptions.Clear();
            if (radicalDict.ContainsKey(currentChar) && random.Next(4) != 0) // 25%機率不放正確答案
                currentOptions.Add(radicalDict[currentChar]);
            var allRadicals = radicalDict.Values.Distinct().ToList();
            while (currentOptions.Count < 3)
            {
                string r = allRadicals[random.Next(allRadicals.Count)];
                if (!currentOptions.Contains(r))
                    currentOptions.Add(r);
            }
            currentOptions = currentOptions.OrderBy(x => random.Next()).ToList();

            // 動態產生按鈕
            for (int i = 0; i < currentOptions.Count; i++)
            {
                Button btn = new Button();
                btn.Text = currentOptions[i];
                btn.Font = new Font("微軟正黑體", 16);
                btn.Size = new Size(80, 40);
                btn.Location = new Point(10 + i * 100, 100);
                btn.Tag = currentOptions[i];
                btn.Click += BtnRadical_Click;
                this.Controls.Add(btn);
            }

            // 新增「全部選項都錯」按鈕
            Button btnNone = new Button();
            btnNone.Text = "全部選項都錯";
            btnNone.Font = new Font("微軟正黑體", 16);
            btnNone.Size = new Size(160, 40);
            btnNone.Location = new Point(10, 160);
            btnNone.Click += BtnNone_Click;
            this.Controls.Add(btnNone);
        }

        private void BtnRadical_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string selectedRadical = btn.Tag.ToString();
            if (radicalDict.ContainsKey(currentChar) && selectedRadical == radicalDict[currentChar])
            {
                score++;
                lblScore.Text = $"分數：{score} / {maxScore}";
                MessageBox.Show("答對了！", "提示");
                NextQuestion();
            }
            else
            {
                MessageBox.Show("答錯了，請再試一次！", "提示");
            }
        }

        private void BtnNone_Click(object sender, EventArgs e)
        {
            // 正確答案不在選項中才得分
            if (!currentOptions.Contains(radicalDict.ContainsKey(currentChar) ? radicalDict[currentChar] : ""))
            {
                score++;
                lblScore.Text = $"分數：{score} / {maxScore}";
                MessageBox.Show("答對了！", "提示");
                NextQuestion();
            }
            else
            {
                MessageBox.Show("正確答案在選項中，請選擇正確部首！", "提示");
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnHint_Click(object sender, EventArgs e)
        {
            // 每使用一次提示，分數減半，並顯示正確答案
            if (hintUsed == 0)
            {
                score = (int)(score / 2.0);
                lblScore.Text = $"分數：{score} / {maxScore}";
                MessageBox.Show($"提示：正確答案是「{radicalDict[currentChar]}」", "提示");
                hintUsed++;
            }
            else
            {
                MessageBox.Show("提示次數已用完！", "提示");
            }
        }
    }
}