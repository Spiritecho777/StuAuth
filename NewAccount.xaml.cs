using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Drawing;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Color = System.Drawing.Color;
using Cursors = System.Windows.Forms.Cursors;
using ZXing;
using ZXing.Windows.Compatibility;
//using System.Drawing.Imaging;
//using PixelFormat = System.Drawing.Imaging.PixelFormat;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
//using Page = System.Windows.Controls.Page;

namespace MFA
{
    public partial class NewAccount : Page
    {
        private Point startPoint;
        private MainWindow main;
        

        public NewAccount(MainWindow main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private string DecodeQRCode(Bitmap bitmap)
        {
            BarcodeReader reader = new BarcodeReader();

            Result result = reader.Decode(bitmap);
            if(result != null) 
            {
                System.Windows.MessageBox.Show(result.Text);
                main.NewAccount(result.Text.ToString());
                //NavigationService(new NewAccount2(result.Text.ToString()));
                //textBox.Text = result.Text;
                return result.Text;
            }
            else
            {
                System.Windows.MessageBox.Show("eux ya rien");
                return "Pas de QR code trouver";
            }
        }

        private void StartSelection(Screen screen)
        {
            //Main.Hide();

            //Point mousePosition = System.Windows.Forms.Control.MousePosition;
            //Screen mouseScreen = Screen.FromPoint(mousePosition);
            
            Rectangle screenBounds = screen.Bounds;
            using (Bitmap screenBitmap = new Bitmap(screenBounds.Width, screenBounds.Height))
            {

                using (Graphics g = Graphics.FromImage(screenBitmap))
                {
                    g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size);
                }

                using (Form selectionForm = new Form())
                {
                    selectionForm.StartPosition = FormStartPosition.Manual;
                    selectionForm.Location = screen.Bounds.Location;

                    selectionForm.FormBorderStyle = FormBorderStyle.None;
                    selectionForm.WindowState = FormWindowState.Maximized;
                    selectionForm.Cursor = Cursors.Cross;

                    PictureBox pictureBox = new PictureBox();
                    Rectangle selectedRegion = new Rectangle();
                    pictureBox.Dock = DockStyle.Fill;
                    pictureBox.Image = screenBitmap;
                    pictureBox.MouseDown += (sender, e) =>
                    {
                        startPoint = e.Location;
                    };
                    pictureBox.MouseMove += (sender, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            ControlPaint.DrawReversibleFrame(selectedRegion, Color.Black, FrameStyle.Dashed);
                            selectedRegion = new Rectangle(Math.Min(startPoint.X, e.X), Math.Min(startPoint.Y, e.Y), Math.Abs(startPoint.X - e.X), Math.Abs(startPoint.Y - e.Y));
                            ControlPaint.DrawReversibleFrame(selectedRegion, Color.Black, FrameStyle.Dashed);
                        }
                    };
                    pictureBox.MouseUp += (sender, e) =>
                    {
                        selectionForm.Close();
                        Bitmap selectedBitmap = new Bitmap(selectedRegion.Width, selectedRegion.Height);
                        Graphics selectedGraphics = Graphics.FromImage(selectedBitmap);
                        selectedGraphics.DrawImage(screenBitmap, 0, 0, selectedRegion, GraphicsUnit.Pixel);
                        string qrCodeData = DecodeQRCode(selectedBitmap);

                        //selectedBitmap.Save("c:\\users\\Asumi\\Desktop\\essai.png", ImageFormat.Png);

                    };

                    selectionForm.Controls.Add(pictureBox);
                    selectionForm.ShowDialog();
                }
            }
        }

        private void Capture(object sender, RoutedEventArgs e)
        {
            Screen currentScreen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
            StartSelection(currentScreen);
        }
    }
}
