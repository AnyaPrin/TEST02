using System.Windows.Forms;

namespace TEST02
{
    public class TransparentRichTextBox : RichTextBox
    {
        public TransparentRichTextBox()
        {
            // RichTextBox が透明な背景色を正しくサポートするために、このスタイルを設定します。
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false); // コントロールが不透明でないことを示す

            // オプション: これらのスタイルはちらつきを減らすのに役立つ場合があります。
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // ダブルバッファリングを有効にする
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // 背景を描画しないことで、親コントロールの背景が透けて見えるようにします。
            // base.OnPaintBackground(pevent); // この行をコメントアウトまたは削除
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20; // WS_EX_TRANSPARENT スタイルを追加
                return cp;
            }
        }
    }
}