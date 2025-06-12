using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace TEST02 // プロジェクトの名前空間に合わせてください
{
    public class ImageBackgroundTextBox : TextBox
    {
        private Image? _backgroundImage; // Null許容に変更
        private ImageLayout _backgroundImageLayout = ImageLayout.Stretch;

        [Browsable(true)]
        [Category("Appearance")]
        [Description("The background image used for the control.")]
        public override Image? BackgroundImage
        {
            get => _backgroundImage;
            set
            {
                _backgroundImage = value;
                this.Invalidate(); // Redraw the control when the image changes
            }
        }

        [Browsable(true)]
        [Category("Appearance")]
        [Description("Specifies the layout of the background image.")]
        [DefaultValue(ImageLayout.Stretch)] // DefaultValue属性を追加
        public new ImageLayout BackgroundImageLayout
        {
            get => _backgroundImageLayout;
            set
            {
                _backgroundImageLayout = value;
                this.Invalidate(); // Redraw the control when the layout changes
            }
        }

        public ImageBackgroundTextBox()
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: ImageBackgroundTextBox constructor. Instance created.");
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.UserPaint, true); // コントロールが自身で描画することを指定
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // WM_ERASEBKGND を無視し、ちらつきを減らす
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true); // ダブルバッファリングを有効にし、ちらつきを減らす
            SetStyle(ControlStyles.Opaque, false); // 背景を自分で描画するため
            this.BackColor = Color.Transparent; // TextBox自身の背景を透明に
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: ImageBackgroundTextBox.OnPaint called for {this.Name}. BGImage is {(BackgroundImage == null ? "null" : "NOT null")}");
            if (BackgroundImage != null)
            {
                DrawBackgroundImage(e.Graphics, BackgroundImage, this.ClientRectangle, BackgroundImageLayout);
            }
            base.OnPaint(e); // 基底クラスにテキストを描画させる
        }

        // 背景画像を描画するヘルパーメソッド (変更なし、OnPaintから呼ばれる)
        private static void DrawBackgroundImage(Graphics g, Image image, Rectangle bounds, ImageLayout layout)
        {
            System.Diagnostics.Debug.WriteLine($"DEBUG: DrawBackgroundImage called. Layout: {layout}, Bounds: {bounds}");
            switch (layout)
            {
                case ImageLayout.None:
                    g.DrawImage(image, bounds.Location);
                    break;
                case ImageLayout.Tile:
                    using (TextureBrush brush = new TextureBrush(image, System.Drawing.Drawing2D.WrapMode.Tile))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                    break;
                case ImageLayout.Center:
                    int x = bounds.X + (bounds.Width - image.Width) / 2;
                    int y = bounds.Y + (bounds.Height - image.Height) / 2;
                    g.DrawImage(image, x, y, image.Width, image.Height);
                    break;
                case ImageLayout.Stretch:
                    g.DrawImage(image, bounds);
                    break;
                case ImageLayout.Zoom:
                    float imgRatio = (float)image.Width / image.Height;
                    float ctrlRatio = (float)bounds.Width / bounds.Height;
                    Rectangle targetRect = bounds;
                    if (imgRatio > ctrlRatio) targetRect.Height = (int)(bounds.Width / imgRatio);
                    else targetRect.Width = (int)(bounds.Height * imgRatio);
                    targetRect.X = bounds.X + (bounds.Width - targetRect.Width) / 2;
                    targetRect.Y = bounds.Y + (bounds.Height - targetRect.Height) / 2;
                    g.DrawImage(image, targetRect);
                    break;
            }
        }
    }
}