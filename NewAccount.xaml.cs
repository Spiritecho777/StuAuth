using System.IO;
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
using OtpNet;

namespace MFA
{
    public partial class NewAccount : Page
    {
        private Point startPoint;
        private MainWindow main;
        private Main menu;     

        public NewAccount(MainWindow main,Main menu)
        {
            InitializeComponent();
            this.main = main;
            this.menu = menu;
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
                main.Visibility = Visibility.Visible;
                main.NewAccount(result.Text.ToString(),menu);
                return result.Text;
            }
            else
            {
                main.Visibility = Visibility.Visible;
                System.Windows.MessageBox.Show("Pas de QR code trouver");
                return "Pas de QR code trouver";
            }
        }

        private void StartSelection(Screen screen)
        {
            main.Hide();
            
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

        private void Confirm(object sender, RoutedEventArgs e)
        {

        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Fichiers texte (*.txt)|*.txt";
            ofd.Title = "Sélectionnez un fichier texte";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath= ofd.FileName;

                using StreamReader reader = new StreamReader(filePath);
                {

                    string[] lignes = File.ReadAllLines(filePath);

                    foreach (string line in lignes)
                    {
                        i++;

                        string appDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StuAuthData");

                        if (!Directory.Exists(appDirectory))
                        {
                            Directory.CreateDirectory(appDirectory);
                        }

                        string Savefile = System.IO.Path.Combine(appDirectory, "Account.dat");

                        using (StreamWriter sw = File.AppendText(Savefile))
                        {
                            sw.WriteLine( i + ";" + line);
                        }
                    }
                }
            }
            NavigationService.GoBack();
            menu.UpdateList();
        }
    }
}
