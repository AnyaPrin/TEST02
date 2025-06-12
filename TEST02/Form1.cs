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
    private Random random = new Random(); // ランダムな応答を選ぶために追加
    private string[] catResponses = { "にゃ", "にゃー", "にゃーん", "シャー！" }; // 猫の応答候補
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
                ForeColor = Color.Red, // デバッグ用。デバッグ時の確認を容易にするため赤に設定。最終的には白に戻す予定 by Anyaprin
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


    private async void InputTextBox_KeyDown(object sender, KeyEventArgs e) // async を追加
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true; // Enterキー入力時のビープ音を抑制

            // inputTextBoxがnullの場合は何もしない（念のため）
            if (inputTextBox == null) return;

            string userInput = inputTextBox!.Text.Trim(); // inputTextBoxがnullでないことを前提とする (null免除演算子)

            if (!string.IsNullOrEmpty(userInput) && outputTextBox != null)
            {
                try
                {
                    // ユーザーの入力を表示
                    outputTextBox.SelectionColor = Color.Gray; // ユーザー入力の色を設定
                    outputTextBox.AppendText(userInput + Environment.NewLine);

                    // 0.5秒待機
                    await Task.Delay(500);

                    // 待機後にコントロールが破棄されていないか確認
                    if (this.IsDisposed || outputTextBox.IsDisposed || inputTextBox.IsDisposed)
                    {
                        return; // 破棄されていれば処理を中断
                    }

                    // 猫の応答を表示
                    string response = catResponses[random.Next(catResponses.Length)]; // ランダムに応答を選択
                    outputTextBox.SelectionColor = Color.White; // 猫の応答の色を設定
                    outputTextBox.AppendText(response + Environment.NewLine); // 選択された応答を追加

                    inputTextBox.Clear();
                }
                catch (ObjectDisposedException odEx)
                {
                    System.Diagnostics.Debug.WriteLine($"UI要素が破棄されました: {odEx.Message}");
                }
                catch (Exception ex) // その他の予期せぬ例外をキャッチ
                {
                    System.Diagnostics.Debug.WriteLine($"InputTextBox_KeyDownでエラーが発生しました: {ex.Message}");
                    // 必要であればユーザーにエラーを通知
                }
            }
            else if (outputTextBox == null && !string.IsNullOrEmpty(userInput))
            {
                System.Diagnostics.Debug.WriteLine("警告: outputTextBox が null のため、テキスト追加をスキップしました。");
                if (inputTextBox != null) inputTextBox.Clear(); // この場合も入力はクリアする
            }
            else if (inputTextBox != null) // userInputが空の場合でも、inputTextBoxがnullでなければクリア
            {
                // userInputが空でもEnterが押されたら入力欄をクリアする場合
                // inputTextBox.Clear(); // この行は、ユーザーが空の入力をEnterした場合の挙動によります。現状はクリアしません。
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
