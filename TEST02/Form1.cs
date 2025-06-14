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
    private Random random = new Random();
    private string[] catResponses = Array.Empty<string>(); // 初期値は空の配列
    // private const string ImageFileName = "dummy.jpg"; // テスト用

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true; // フォームのダブルバッファリングを有効にする
        InitializeCustomComponents();
        this.Text = "NyaLIZA"; // ウィンドウタイトルを設定
        LoadBackgroundImage();
        LoadCatResponses(); // 応答リストを読み込む
         
        this.FormClosed += Form1_FormClosed!; // FormClosedイベントハンドラを登録
    }

    private void LoadCatResponses()
    {
        const string responsesFileName = "cat_responses.txt";
        // string filePath = Path.Combine(Application.StartupPath, responsesFileName); // WinFormsの場合
        string filePath = Path.Combine(AppContext.BaseDirectory, responsesFileName); // .NET Core / .NET 5+ 推奨
        System.Diagnostics.Debug.WriteLine($"応答リスト: {filePath}");

        try
        {
            if (File.Exists(filePath))
            {
                catResponses = File.ReadAllLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            }
            else
            {
                // ファイルが存在しない場合は、空のファイルを作成し、応答リストも空にする
                File.WriteAllText(filePath, string.Empty); // 空のファイルを作成
                catResponses = Array.Empty<string>();
                System.Diagnostics.Debug.WriteLine("応答リストファイルが見つからなかったので、空のファイルを作成しました。");
            }

            if (catResponses.Length == 0)
            {
                // 起動時にcatResponsesが空の場合の初期メッセージ（これはオプション）
                catResponses = new[] { "にゃ？（まだ何も知らないにゃ。/add で教えてにゃ）" };
                System.Diagnostics.Debug.WriteLine("応答リストは現在空です。");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"猫の応答リストの読み込みに失敗しました: {ex.Message}");
            catResponses = new[] { "にゃーん…（エラーだにゃ）" }; // エラー時のデフォルト応答
        }
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
            if (inputTextBox == null) return;

            string userInput = inputTextBox.Text.Trim();
            inputTextBox.Clear(); // 入力後、テキストボックスをクリア

            if (string.IsNullOrEmpty(userInput)) return; // 空の入力は何もしない

            if (outputTextBox == null || outputTextBox.IsDisposed)
            {
                System.Diagnostics.Debug.WriteLine("警告: outputTextBox が null または破棄されているため、処理をスキップしました。");
                return;
            }

            // コマンド処理
            if (userInput.StartsWith("/"))
            {
                await ProcessCommandAsync(userInput);
            }
            else // 通常のメッセージ処理
            {
                try
                {
                    // ユーザーの入力を表示
                    outputTextBox.SelectionColor = Color.Gray; // ユーザー入力の色を設定
                    outputTextBox.AppendText(userInput + Environment.NewLine);
    
                    // 0.5秒待機
                    await Task.Delay(500);
    
                    // 待機後にコントロールが破棄されていないか確認
                    if (this.IsDisposed || outputTextBox.IsDisposed || (inputTextBox != null && inputTextBox.IsDisposed))
                    {
                        return; // 破棄されていれば処理を中断
                    }
    
                    // 猫の応答を表示
                    string response;
                    if (catResponses.Length > 0)
                    {
                        response = catResponses[random.Next(catResponses.Length)]; // ランダムに応答を選択
                    }
                    else
                    {
                        response = "にゃ？（まだ何も知らないにゃ。/add で教えてにゃ）";
                    }
    
                    outputTextBox.SelectionColor = Color.White; // 猫の応答の色を設定
                    outputTextBox.AppendText(response + Environment.NewLine); // 選択された応答を追加
                }
                catch (ObjectDisposedException odEx)
                {
                    System.Diagnostics.Debug.WriteLine($"UI要素が破棄されました: {odEx.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"InputTextBox_KeyDownでエラーが発生しました: {ex.Message}");
                }
            }
        }
    }

    private async Task ProcessCommandAsync(string commandInput)
    {
        if (outputTextBox == null || outputTextBox.IsDisposed) return;

        string[] parts = commandInput.Split(new[] { ' ' }, 2);
        string command = parts[0].ToLower();

        try
        {
            if (command == "/exit")
            {
                Application.Exit();
            }
            else if (command == "/add" && parts.Length > 1)
            {
                string newResponse = parts[1].Trim();
                if (!string.IsNullOrEmpty(newResponse))
                {
                    await AddResponseToFileAsync(newResponse);
                    
                    List<string> tempResponses = catResponses.ToList();
                    if (!tempResponses.Contains(newResponse)) // 重複を避ける場合
                    {
                        tempResponses.Add(newResponse);
                        catResponses = tempResponses.ToArray();
                        outputTextBox.SelectionColor = Color.Aqua; 
                        outputTextBox.AppendText($"応答「{newResponse}」を追加しましたにゃ。{Environment.NewLine}");
                    }
                    else
                    {
                        outputTextBox.SelectionColor = Color.Yellow;
                        outputTextBox.AppendText($"応答「{newResponse}」は既にあるにゃ。{Environment.NewLine}");
                    }
                }
                else
                {
                    outputTextBox.SelectionColor = Color.Orange;
                    outputTextBox.AppendText($"追加する応答内容が空っぽだにゃ。 (例: /add にゃーん){Environment.NewLine}");
                }
            }
            else
            {
                outputTextBox.SelectionColor = Color.Orange;
                outputTextBox.AppendText($"知らないコマンドだにゃ: {commandInput}{Environment.NewLine}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"コマンド「{commandInput}」の処理中にエラー: {ex.Message}");
            if (!outputTextBox.IsDisposed)
            {
                outputTextBox.SelectionColor = Color.Red;
                outputTextBox.AppendText($"コマンド処理でエラーが起きたにゃ…{Environment.NewLine}");
            }
        }
    }

    private async Task AddResponseToFileAsync(string newResponse)
    {
        const string responsesFileName = "cat_responses.txt";
        string filePath = Path.Combine(AppContext.BaseDirectory, responsesFileName);
        try
        {
            await File.AppendAllTextAsync(filePath, newResponse + Environment.NewLine);
            System.Diagnostics.Debug.WriteLine($"応答をファイルに追加しました: {newResponse}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ファイルへの応答「{newResponse}」追加に失敗: {ex.Message}");
            if (outputTextBox != null && !outputTextBox.IsDisposed)
            {
                outputTextBox.SelectionColor = Color.Red;
                outputTextBox.AppendText($"応答「{newResponse}」のファイル保存に失敗したにゃ…{Environment.NewLine}");
            }
            throw; // ProcessCommandAsyncでキャッチさせる
        }
    }

    private void Form1_FormClosed(object sender, FormClosedEventArgs e)
    {
        // originalFontがnullでないことを確認してからDisposeを呼び出す
        originalFont?.Dispose();
        originalFont = null;
    }
}
