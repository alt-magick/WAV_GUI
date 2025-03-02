using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NAudio.Wave;

namespace WaveformDisplay
{
    public partial class MainForm : Form
    {
        private string filePath1;
        private string filePath2;
        private string outputFilePath;

        private Button openButton1;
        private Button openButton2;
        private Button openButton3;
        private Button runButton; // Run button
        private Button playButton; // New Play button
        private Button playButton2; // New Play button
        private Button playButton3; // New Play button

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;

        private Label outputFileLabel;
        private Label fileNameLabel; // Label to show the file name
        private Label filePathLabel2; // Label to show the full path of the file selected by openButton2
        private Label outputFilePathLabel; // Label to show the full path of the output file selected by openButton3

        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;

        private ComboBox processingComboBox; // ComboBox for selecting processing type

        private Label degreeLabel;
        private TextBox degreeTextBox;
        public MainForm()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnResolveAssembly);

            InitializeComponent();
        }
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");

            if (File.Exists(assemblyPath))
            {
                return Assembly.Load(File.ReadAllBytes(assemblyPath)); // Load into memory
            }
            return null;
        }
        private void InitializeComponent()
        {
            this.openButton1 = new Button();
            this.openButton2 = new Button();
            this.openButton3 = new Button();
            this.runButton = new Button(); // Initialize Run button
            this.pictureBox1 = new PictureBox();
            this.pictureBox2 = new PictureBox();
            this.pictureBox3 = new PictureBox();
            this.outputFileLabel = new Label();
            this.fileNameLabel = new Label();  // Initialize the label for file name
            this.filePathLabel2 = new Label(); // Initialize the label for file path
            this.outputFilePathLabel = new Label(); // Initialize the label for output file path
            this.openFileDialog = new OpenFileDialog();
            this.saveFileDialog = new SaveFileDialog();
            this.processingComboBox = new ComboBox(); // Initialize the ComboBox
            this.playButton = new Button();
            this.playButton.Location = new Point(524, 226);  // Position below the Run button
            this.playButton.Name = "playButton";
            this.playButton.Size = new Size(75, 23);
            this.playButton.Text = "Play";
            this.playButton.Click += new EventHandler(this.PlayButton_Click);
            this.Controls.Add(this.playButton);

            this.playButton2 = new Button();
            this.playButton2.Location = new Point(524, 449);  // Position below the Run button
            this.playButton2.Name = "playButton2";
            this.playButton2.Size = new Size(75, 23);
            this.playButton2.Text = "Play";
            this.playButton2.Click += new EventHandler(this.PlayButton2_Click);
            this.Controls.Add(this.playButton2);

            this.playButton3 = new Button();
            this.playButton3.Location = new Point(524, 672);  // Position below the Run button
            this.playButton3.Name = "playButton3";
            this.playButton3.Size = new Size(75, 23);
            this.playButton3.Text = "Play";
            this.playButton3.Click += new EventHandler(this.PlayButton3_Click);
            this.Controls.Add(this.playButton3);

            this.degreeLabel = new Label();
            this.degreeLabel.Location = new Point(138, 24);  // Set location below other controls
            this.degreeLabel.Size = new Size(100, 21);
            this.degreeLabel.Font = new Font("Arial", 9, FontStyle.Regular);
            this.degreeLabel.Text = "Percentage";
            this.Controls.Add(this.degreeLabel);

            // Initialize the Degree TextBox
            this.degreeTextBox = new TextBox();
            this.degreeTextBox.Location = new Point(140, 49);  // Position next to the Degree label
            this.degreeTextBox.Size = new Size(100, 23);
            this.degreeTextBox.Text = "0";  // Default value
            this.Controls.Add(this.degreeTextBox);

            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Font;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int formWidth = Math.Min(640, screenWidth - 50);
            int formHeight = Math.Min(720, screenHeight - 50);

            this.ClientSize = new Size(formWidth, formHeight);
            this.Text = "WAV Mixer";

            Label mixLabel = new Label();
            mixLabel.Location = new Point(138, 90);
            mixLabel.Size = new Size(35, 18);
            mixLabel.Font = new Font("Arial", 9, FontStyle.Regular);
            mixLabel.Text = "Mix";

            Label mixLabel2 = new Label();
            mixLabel2.Location = new Point(13, 21);
            mixLabel2.Size = new Size(126, 18);
            mixLabel2.Font = new Font("Arial", 9, FontStyle.Regular);
            mixLabel2.Text = "WAV Files";

            Label mixLabel3 = new Label();
            mixLabel3.Location = new Point(13, 145);
            mixLabel3.Size = new Size(126, 18);
            mixLabel3.Font = new Font("Arial", 9, FontStyle.Regular);
            mixLabel3.Text = "Process";

            // processingComboBox (ComboBox for processing options)
            this.processingComboBox.Location = new Point(140, 115);
            this.processingComboBox.Name = "processingComboBox";
            this.processingComboBox.Size = new Size(100, 23);
            this.processingComboBox.Items.AddRange(new string[] { "Exponential", "Logarithmic", "Sigmoid", "Average", "Additive", "Subtractive" });
            this.processingComboBox.SelectedIndexChanged += new EventHandler(this.ProcessingComboBox_SelectedIndexChanged);
            this.processingComboBox.SelectedIndex = 0;
            this.Controls.Add(mixLabel);

            // openButton1
            this.openButton1.Location = new Point(12, 45);
            this.openButton1.Name = "openButton1";
            this.openButton1.Size = new Size(75, 23);
            this.openButton1.Text = "Modifier";
            this.openButton1.Click += new EventHandler(this.OpenButton1_Click);
            this.Controls.Add(mixLabel2);

            // openButton2
            this.openButton2.Location = new Point(12, 75);
            this.openButton2.Name = "openButton2";
            this.openButton2.Size = new Size(75, 23);
            this.openButton2.Text = "Original";
            this.openButton2.Click += new EventHandler(this.OpenButton2_Click);

            // openButton3 (For setting the output file path)
            this.openButton3.Location = new Point(12, 105);
            this.openButton3.Name = "openButton3";
            this.openButton3.Size = new Size(75, 23);
            this.openButton3.Text = "Output";
            this.openButton3.Click += new EventHandler(this.OpenButton3_Click);

            // runButton (The new Run button)
            this.runButton.Location = new Point(12, 168);
            this.runButton.Name = "runButton";
            this.runButton.Size = new Size(75, 23);
            this.runButton.Text = "Run";
            this.runButton.Click += new EventHandler(this.RunButton_Click);
            this.Controls.Add(mixLabel3);

            // pictureBox1 (Moved even further to the right)
            this.pictureBox1.Location = new Point(300, 42);  // Moved further to the right
            this.pictureBox1.Size = new Size(300, 180);
            this.pictureBox1.BorderStyle = BorderStyle.FixedSingle;

            // pictureBox2 (Moved even further to the right)
            this.pictureBox2.Location = new Point(300, 265);  // Moved further to the right
            this.pictureBox2.Size = new Size(300, 180);
            this.pictureBox2.BorderStyle = BorderStyle.FixedSingle;

            // pictureBox3 (Moved even further to the right)
            this.pictureBox3.Location = new Point(300, 488);  // Moved further to the right
            this.pictureBox3.Size = new Size(300, 180);
            this.pictureBox3.BorderStyle = BorderStyle.FixedSingle;

            // fileNameLabel (Label to show the file name selected by openButton1)
            this.fileNameLabel.Location = new Point(300, 22);  // Below openButton3
            this.fileNameLabel.Size = new Size(300, 23);
            this.fileNameLabel.Text = "No file selected";  // Default message

            // filePathLabel2 (Label to show the full path of the file selected by openButton2)
            this.filePathLabel2.Location = new Point(300, 245);  // Below fileNameLabel
            this.filePathLabel2.Size = new Size(300, 23);
            this.filePathLabel2.Text = "No file selected";  // Default message

            // outputFilePathLabel (Label to show the full path of the output file selected by openButton3)
            this.outputFilePathLabel.Location = new Point(300, 470);  // Below filePathLabel2
            this.outputFilePathLabel.Size = new Size(300, 23);
            this.outputFilePathLabel.Text = "No output file selected";  // Default message

            // Add controls to the form
            this.Controls.Add(this.openButton1);
            this.Controls.Add(this.openButton2);
            this.Controls.Add(this.openButton3);
            this.Controls.Add(this.runButton); // Add the Run button
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox3);
            this.Controls.Add(this.outputFileLabel);
            this.Controls.Add(this.fileNameLabel); // Add fileNameLabel to the form
            this.Controls.Add(this.filePathLabel2); // Add filePathLabel2 to the form
            this.Controls.Add(this.outputFilePathLabel); // Add outputFilePathLabel to the form
            this.Controls.Add(this.processingComboBox); // Add processingComboBox to the form
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath1))
            {
                MessageBox.Show("No WAV file selected.");
                return;
            }

            try
            {
                using (var audioFileReader = new AudioFileReader(filePath1))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFileReader);
                    outputDevice.Play();
                    MessageBox.Show("Playing audio...");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio: " + ex.Message);
            }
        }

        private void PlayButton2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath2))
            {
                MessageBox.Show("No WAV file selected.");
                return;
            }

            try
            {
                using (var audioFileReader = new AudioFileReader(filePath2))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFileReader);
                    outputDevice.Play();
                    MessageBox.Show("Playing audio...");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio: " + ex.Message);
            }
        }


        private void PlayButton3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(outputFilePath))
            {
                MessageBox.Show("No WAV file selected.");
                return;
            }

            try
            {
                using (var audioFileReader = new AudioFileReader(outputFilePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFileReader);
                    outputDevice.Play();
                    MessageBox.Show("Playing audio...");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing audio: " + ex.Message);
            }
        }

        private void OpenButton1_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "WAV Files|*.wav";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath1 = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath1);  // Get the file name
                fileNameLabel.Text = "Selected File: " + fileName;  // Update the label with the file name
                DisplayWaveform(filePath1, pictureBox1);
            }
        }

        private void OpenButton2_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = "WAV Files|*.wav";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath2 = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath2);  // Get the file name
                filePathLabel2.Text = "Selected File: " + fileName;  // Update the label with the full file path
                DisplayWaveform(filePath2, pictureBox2);
            }
        }

        private void OpenButton3_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "WAV Files|*.wav";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                outputFilePath = saveFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(outputFilePath);  // Get the file name
                outputFilePathLabel.Text = "Output File: " + fileName;  // Update the label with the output file path
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            // Retrieve the selected processing method
            string selectedOption = processingComboBox.SelectedItem.ToString();

            // Convert processing option to a corresponding number
            int processingNumber = 0;
            if (selectedOption == "Exponential")
            {
                processingNumber = 1;
            }
            else if (selectedOption == "Logarithmic")
            {
                processingNumber = 2;
            }
            else if (selectedOption == "Sigmoid")
            {
                processingNumber = 3;
            }
            else if (selectedOption == "Average")
            {
                processingNumber = 4;
            }
            else if (selectedOption == "Additive")
            {
                processingNumber = 5;
            }
            else if (selectedOption == "Subtractive")
            {
                processingNumber = 6;
            }

            int degree = 0;
            if (!int.TryParse(degreeTextBox.Text, out degree) || degree < 0 || degree > 100)
            {
                MessageBox.Show("Please enter a valid degree between 0 and 100.");
                return;
            }

            // Prepare arguments for program.exe
            string arguments = $"\"{filePath1}\" \"{filePath2}\" \"{outputFilePath}\" {processingNumber} {degree}";

            try
            {
                // Create a new process to run program.exe
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "program.exe";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                // Start the process
                process.Start();

                // Optionally read the output (for debugging)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Show a message with execution results
                MessageBox.Show($"{output}\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error running program.exe: " + ex.Message);
            }
            DisplayWaveform(outputFilePath, pictureBox3);
        }


        private void ProcessingComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optionally handle the ComboBox selection change if needed
        }

        private void DisplayWaveform(string filePath, PictureBox pictureBox)
        {
            try
            {
                using (var reader = new AudioFileReader(filePath))
                {
                    int width = pictureBox.Width;
                    int height = pictureBox.Height;
                    long sampleCount = reader.Length / 4;  // 4 bytes per sample (32-bit float)

                    int step = (int)(sampleCount / width);
                    if (step <= 0)
                    {
                        MessageBox.Show("The audio file is too short to display a proper waveform.");
                        return;
                    }

                    int blockAlign = reader.WaveFormat.BlockAlign;
                    step = (step / blockAlign) * blockAlign;

                    Bitmap waveformBitmap = new Bitmap(width, height);
                    Graphics g = Graphics.FromImage(waveformBitmap);
                    g.Clear(Color.White);

                    float[] buffer = new float[step];
                    for (int i = 0; i < width; i++)
                    {
                        int samplesRead = reader.Read(buffer, 0, step);

                        if (samplesRead == 0)
                            break;

                        float peak = 0;
                        foreach (var sample in buffer)
                        {
                            peak = Math.Max(peak, Math.Abs(sample));
                        }

                        int positiveY = height / 2 - (int)(peak * height / 2);
                        int negativeY = height / 2 + (int)(peak * height / 2);

                        g.DrawLine(Pens.Black, i, height / 2, i, positiveY);
                        g.DrawLine(Pens.Black, i, height / 2, i, negativeY);
                    }

                    pictureBox.Image = waveformBitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the WAV file: " + ex.Message);
            }
        }
    }
}
