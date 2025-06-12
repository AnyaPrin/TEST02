using System.Drawing;
using System.Windows.Forms; // Labelコントロールを使用するために追加
using System.IO; // FileNotFoundExceptionを使用するために追加

namespace TEST02;

public partial class Form1 : Form
{
    private Font? originalFont; // Null許容に変更
    private const string ImageFileName = "test02.jpg"; // 画像ファイル名
    private TransparentRichTextBox? outputTextBox; // TransparentRichTextBox に変更
    private TextBox? inputTextBox; // Null許容に変更
    // private const string ImageFileName = "dummy.jpg"; // テスト用

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true; // フォームのダブルバッファリングを有効にする
        InitializeCustomComponents();
        this.Text = "セラピストELIZA"; // ウィンドウタイトルを設定
        LoadBackgroundImage();

        this.FormClosed += Form1_FormClosed!; // FormClosedイベントハンドラを登録
    }

    private void InitializeCustomComponents()
    {
        try
        {
            // outputTextBox を TransparentRichTextBox に変更
            outputTextBox = new TransparentRichTextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None, // スクロールバーなし
                Dock = DockStyle.Fill, // 残り領域を埋める
                BackColor = Color.Transparent, // 背景を透明に
                // ForeColor = Color.White, // 文字色を白に設定
                ForeColor = Color.Red, // デバッグ用。デバッグ時の確認を容易にするため赤に設定。最終的には白の戻す予定 by Anyaprin
                BorderStyle = BorderStyle.None, // 境界線を消す (任意)
                Text = "" // 初期テキスト
            };
        }
        catch (Exception ex)
        {
            MessageBox.Show($"outputTextBox の初期化中にエラーが発生しました。\n詳細: {ex.Message}", "初期化エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        inputTextBox = new TextBox
        {
            Dock = DockStyle.Bottom, // ウィンドウ下部にドッキング
            Height = 30 // 入力欄の高さ
        };
        inputTextBox.KeyDown += InputTextBox_KeyDown!;

        // コントロールの追加順序: Dock.Top, Dock.Bottom, Dock.Fill の順が一般的
        this.Controls.Add(this.inputTextBox); // Dock.Bottom のコントロール
        if (this.outputTextBox != null) // outputTextBoxが正常に初期化された場合のみ追加
        {
            this.Controls.Add(this.outputTextBox); // Dock.Fill で残りの領域を埋める
        }
    }

    private void LoadBackgroundImage()
    {
        try
        {
            // 背景画像を設定 (実行ファイルのディレクトリにあることを想定)
            this.BackgroundImage = Image.FromFile(ImageFileName);
        }
        catch (Exception ex) // 画像読み込みエラー一般
        {
            this.BackgroundImage = null;
            string errorMessage = $"背景画像の読み込みに失敗しました: {ImageFileName}\n詳細: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            MessageBox.Show(errorMessage, "画像読み込みエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        this.BackgroundImageLayout = ImageLayout.Stretch; // 画像をウィンドウサイズに合わせて引き伸ばします


    }


    private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true; // Enterキー入力時のビープ音を抑制
            string userInput = inputTextBox!.Text.Trim(); // inputTextBoxがnullでないことを前提とする (null免除演算子)
            if (!string.IsNullOrEmpty(userInput))
            {
                if (outputTextBox != null)
                {
                    outputTextBox.AppendText(userInput + Environment.NewLine);
                    outputTextBox.AppendText("にゃ？" + Environment.NewLine); // 「にゃ？」の応答を追加
                }
                else
                {
                    // outputTextBox が null の場合、ユーザーには InitializeCustomComponents で既にエラーメッセージが表示されているはずです。
                    System.Diagnostics.Debug.WriteLine("警告: outputTextBox が null のため、テキスト追加をスキップしました。");
                }
                inputTextBox.Clear();
            }
        }
    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        // originalFontがnullでないことを確認してからDisposeを呼び出す
        originalFont?.Dispose();
        originalFont = null;
    }
}
