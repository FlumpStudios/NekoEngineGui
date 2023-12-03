using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using ScintillaNET;

namespace NekoEngine
{
    public enum EditState
    { 
        Walls = 0,
        Elements = 1
    }

    public partial class Form1 : Form
    {
        private Level _currentLevel = new Level();
        private EditState currentEditState = EditState.Walls;
        const string GAME_FILE_LOCATION = @"..\game.h";
        private const int gridSize = 64;
        private const int cellSize = 15; // Adjust the cell size as needed
        private Bitmap gridImage;
        private bool isMousePressed = false;
        private byte currentElementNumber = 0;


        Color selectedMapColour = Color.FromArgb(255, 255, 20, 147);
    

        public Form1()
        {
            InitializeComponent();
            InitializeCodeEditor();
            InitializeGrid();
        }

        private void InitializeCodeEditor()
        {
            // Set up the ScintillaNET control
            //codeEditor.Dock = DockStyle.Right;
            codeEditor.Margins[0].Width = 20; // Add a margin for line numbers
            codeEditor.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            codeEditor.Styles[ScintillaNET.Style.Default].Size = 10;
            //codeEditor.LexerName = "C++";
            codeEditor.Lexer = Lexer.Cpp; // Set the lexer for C++ syntax highlighting
            codeEditor.Text = System.IO.File.ReadAllText(GAME_FILE_LOCATION);
        }

        private string _imagePath = string.Empty;
        private string _audioPath = string.Empty;

        
        private void button1_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Load the selected image into a Bitmap
                        Bitmap selectedImage = new Bitmap(openFileDialog.FileName);

                        if (selectedImage.Width != 32 || selectedImage.Height != 32)
                        {
                            MessageBox.Show("The selected image must be 32x32 pixels.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Exit the method if the image size is incorrect
                        }


                        // Display the image in the PictureBox
                        pictureBox1.Image = selectedImage;
                        _imagePath = openFileDialog.FileName;
                        generate.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading the image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    }
                }
            }
        }

        private void generate_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the path to the Python executable and the script
                // string pythonPath = "path_to_your_python_executable";
                string scriptPath = @"..\assets\img2array.py";

                // Construct the command with the provided arguments
                string command = $"python {scriptPath} -t -c -x32 -y32 -p..\\assets\\palette565.png {_imagePath}";

                // Set up process start info
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                // Start the process
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Pass the command to the cmd.exe process
                    process.StandardInput.WriteLine(command);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();

                    // Wait for the process to exit
                    //process.WaitForExit();

                    // Optionally, you can retrieve the output of the Python script
                    string output = process.StandardOutput.ReadToEnd();

                    string pattern = @"\{[^}]*\}[^{]*\{([^}]*)\}";
                    Match match = Regex.Match(output, pattern);

                    if (match.Success)
                    {
                        string numbers = match.Groups[1].Value;
                        Output.Text = numbers;
                        copyToClipboard.Enabled = true;


                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running the Python script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"c:\msys64\mingw64.exe";
            startInfo.Arguments = @"c:\projects\anarch\make.sh sdl";
            Process.Start(startInfo);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(@"..\anarch.exe");
        }

        private void VsCode_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            // Start the process
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Pass the command to the cmd.exe process
                process.StandardInput.WriteLine(@"code ../../anarch");
                process.StandardInput.Flush();
                process.StandardInput.Close();

            }
        }

        private void copyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(Output.Text);


            MessageBox.Show("Text copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void loadAudio_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Audio Files|*.raw";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _audioPath = openFileDialog.FileName;
                        AudioFileLocation.Text = _audioPath;
                        GenerateAudioArray.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading the image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void GenerateAudioArray_Click(object sender, EventArgs e)
        {
            try
            {
                // Specify the path to the Python executable and the script
                // string pythonPath = "path_to_your_python_executable";
                string scriptPath = @"..\assets\snd2array.py";

                // Construct the command with the provided arguments
                string command = $"python {scriptPath} {_audioPath}";

                // Set up process start info
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                // Start the process
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Pass the command to the cmd.exe process
                    process.StandardInput.WriteLine(command);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();

                    // Wait for the process to exit
                    //process.WaitForExit();

                    // Optionally, you can retrieve the output of the Python script
                    string output = process.StandardOutput.ReadToEnd();

                    string pattern = @"\{([^}]*)\}";
                    Match match = Regex.Match(output, pattern);

                    if (match.Success)
                    {
                        string numbers = match.Groups[1].Value;
                        audioArrayOutput.Text = numbers;
                        copyAudioToClipboard.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running the Python script: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyAudioToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(copyAudioToClipboard.Text);

            MessageBox.Show("Text copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        private void SaveCode_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(GAME_FILE_LOCATION, codeEditor.Text);
        }

        private void CodeSaveAs_Click(object sender, EventArgs e)
        {
            // Use SaveFileDialog to specify a file for saving
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the contents of the editor to the selected file
                    System.IO.File.WriteAllText(saveFileDialog.FileName, codeEditor.Text);
                }
            }
        }

        private void InitializeGrid()
        {
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Size = new Size(gridSize * cellSize, gridSize * cellSize);

            gridImage = new Bitmap(gridSize * cellSize, gridSize * cellSize);

            using (Graphics g = Graphics.FromImage(gridImage))
            {
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        g.FillRectangle(Brushes.White, j * cellSize, i * cellSize, cellSize, cellSize);
                        g.DrawRectangle(Pens.Black, j * cellSize, i * cellSize, cellSize, cellSize);
                    }
                }
            }

            pictureBox.Image = gridImage;
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMousePressed = true;
                DrawOnGrid(e.X, e.Y);
            }
            else if (e.Button == MouseButtons.Right)
            {
                RemoveColorFromGrid(e.X, e.Y);
                RemoveELementClickedCell(e.X, e.Y);
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMousePressed)
            {
                DrawOnGrid(e.X, e.Y);
            }
        }

        private void DrawOnGrid(int x, int y)
        {
            switch (currentEditState)
            {
                case EditState.Walls:
                    DrawColoursOnGrid(x, y);
                    break;
                case EditState.Elements:
                    DrawNumbersOnGrid(x, y);
                    break;
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isMousePressed = false;
        }

        private void DrawColoursOnGrid(int x, int y)
        {
            int col = x / cellSize;
            int row = y / cellSize;

            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                using (Graphics g = Graphics.FromImage(gridImage))
                {
                    Brush brush = new SolidBrush(GetMapColour());
                    g.FillRectangle(brush, col * cellSize, row * cellSize, cellSize, cellSize);
                    DrawGrid(g);
                }

                pictureBox.Invalidate(); // Force redraw
                RemoveELementClickedCell(x, y);
            }
        }

        private void DrawNumbersOnGrid(int x, int y)
        {
            int col = x / cellSize;
            int row = y / cellSize;
            if (!RecordELementClickedCell(x, y))
            {
                isMousePressed = false;
                return;
            };

            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                using (Graphics g = Graphics.FromImage(gridImage))
                {
                    // Clear the cell
                    g.FillRectangle(Brushes.White, col * cellSize, row * cellSize, cellSize, cellSize);
                    g.DrawRectangle(Pens.Black, col * cellSize, row * cellSize, cellSize, cellSize);

                    Color textColor = Color.Black; 

                    // If enemy, draw as red
                    if (currentElementNumber >= 20)
                    {
                        textColor = Color.Red;
                    }

                    if (currentElementNumber == 0)
                    {
                        if (_currentLevel?.PlayerStart != null)
                        {
                            RemoveColorFromGridViaRowDetails(_currentLevel.PlayerStart[0], _currentLevel.PlayerStart[1]);
                        }
                        if (_currentLevel != null)
                        {
                            _currentLevel.PlayerStart = new byte[3] { (byte)col, (byte)row, _currentLevel.playerRotation };
                        }

                        textColor = Color.Blue;
                    }

                    Brush textBrush = new SolidBrush(textColor);
                    // Draw the number in the cell

                    PointF textLocation = new PointF((col * cellSize) + cellSize / 30, (row * cellSize) + cellSize / 30);
                    g.DrawString(currentElementNumber.ToString(), DefaultFont, textBrush, textLocation);
                }

                pictureBox.Invalidate(); // Force redraw
            }
        }

        private void RemoveColorFromGrid(int x, int y)
        {
            int col = x / cellSize;
            int row = y / cellSize;

            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                using (Graphics g = Graphics.FromImage(gridImage))
                {
                    // Set the cell color to white
                    g.FillRectangle(Brushes.White, col * cellSize, row * cellSize, cellSize, cellSize);

                    // Redraw grid lines
                    DrawGrid(g);
                }

                pictureBox.Invalidate(); // Force redraw
            }
        }

        private void RemoveColorFromGridViaRowDetails(int col, int row)
        {
            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                using (Graphics g = Graphics.FromImage(gridImage))
                {
                    // Set the cell color to white
                    g.FillRectangle(Brushes.White, col * cellSize, row * cellSize, cellSize, cellSize);

                    // Redraw grid lines
                    DrawGrid(g);
                }

                pictureBox.Invalidate(); // Force redraw
            }
        }

        private static void DrawGrid(Graphics g)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    g.DrawRectangle(Pens.Black, j * cellSize, i * cellSize, cellSize, cellSize);
                }
            }
        }

        private Color GetMapColour()
        {
            return selectedMapColour;            
        }
        private byte[] GetColoredCellsArray()
        {
            byte[] coloredCellsArray = new byte[gridSize * gridSize];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Color cellColor = gridImage.GetPixel((j * cellSize) + 3, (i * cellSize) + 3);
                    
                    coloredCellsArray[i * gridSize + j] = GetTextureIndexFrom(cellColor);
                }
            }

            return coloredCellsArray;
        }
  

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void MapColour_1_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {   
               
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_2_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

                
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_3_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

                
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_4_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

                
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_5_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

               
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_6_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

                
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_7_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

                
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_indoors_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

               
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {

                
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door2_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door3_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door4_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door5_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door6_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }

        private void MapColour_door7_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                selectedMapColour = button.BackColor;
            }
            currentEditState = EditState.Walls;
        }


        private void PrintMapArray()
        {
            StringBuilder sb = new();
            var a = GetColoredCellsArray();

            for (int i = 0; i < a.Length; i++)
            {
                if (i % 64 == 0 && i != 0)
                {
                    sb.AppendLine();
                }

                sb.Append(a[i].ToString());
                if (i < a.Length - 1)
                {
                    sb.Append(",");
                }
            }

            LevelArrayOutput.Text = sb.ToString();
            if (_currentLevel != null)
            {
                _currentLevel.MapArray = a;
            }
        }


        private void ClearMap_Click(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(gridImage))
            {
                g.Clear(Color.White);

                DrawGrid(g);
            }

            pictureBox.Invalidate(); // Force redraw
            _currentLevel = new Level();
        }



        private void label3_Click(object sender, EventArgs e)
        {

        }

        private byte GetTextureIndexFrom(Color color)
        {
            if (color.Equals(Color.FromArgb(255, 255, 20, 147)))
            {
                return 1;
            }

            if (color.Equals(Color.FromArgb(255, 255, 0, 0)))
            {
                return 2;
            }

            if (color.Equals(Color.FromArgb(255, 192, 192, 0)))
            {
                return 3;
            }

            if (color.Equals(Color.FromArgb(255, 0, 0, 255)))
            {
                return 4;
            }

            if (color.Equals(Color.FromArgb(255, 128, 0, 0)))
            {
                return 5;
            }

            if (color.Equals(Color.FromArgb(255, 255, 255, 0)))
            {
                return 6;
            }

            if (color.Equals(Color.FromArgb(255, 0, 128, 0)))
            {
                return 7;
            }

            if (color.Equals(Color.FromArgb(255, 192, 192, 255)))
            {
                return 8;
            }

            if (color.Equals(Color.FromArgb(255, 26, 26, 26)))
            {
                return 9 | 0xc0;
            }

            if (color.Equals(Color.FromArgb(255, 77, 77, 77)))
            {
                return 10 | 0xc0;
            }

            if (color.Equals(Color.FromArgb(255, 128, 128, 128)))
            {
                return 11 | 0xc0;
            }

            if (color.Equals(Color.FromArgb(255, 153, 153, 153)))
            {
                return 12 | 0xc0;
            }

            if (color.Equals(Color.FromArgb(255, 179, 179, 179)))
            {
                return 13 | 0xc0;
            }

            if (color.Equals(Color.FromArgb(255, 204, 204, 204)))
            {
                return 14 | 0xc0;
            }

            if (color.Equals(Color.FromArgb(255, 230, 230, 230)))
            {
                return 15 | 0xc0;
            }

            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ElementButton_1_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x01;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_2_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x02;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_3_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x03;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_4_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x04;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_5_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x05;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_6_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x06;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_7_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x07;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_8_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x08;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_9_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x09;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_10_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x10;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_11_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x11;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_12_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x12;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_13_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x13;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_14_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x14;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_15_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x15;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_16_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x16;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_17_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x17;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_18_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x18;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_19_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x19;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_20_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x20;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_21_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x21;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_22_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x22;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_23_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x23;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_24_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x24;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_25_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x25;
            currentEditState = EditState.Elements;
        }

        private void ElementButton_26_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0x26;
            currentEditState = EditState.Elements;
        }

        private void Player_Click(object sender, EventArgs e)
        {
            currentElementNumber = 0;
            currentEditState = EditState.Elements;
        }

        private bool RecordELementClickedCell(int x, int y)
        {
            int col = x / cellSize;
            int row = y / cellSize;

            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                if (_currentLevel?.elements != null)
                {
                    int index = Array.FindIndex(_currentLevel.elements, element =>
                        element.Type == 0 ||
                        (element.Coords != null && element.Coords.Length >= 2 && element.Coords[0] == col && element.Coords[1] == row));

                    if (index == -1)
                    {
                        MessageBox.Show("You have exceeded the max amount of elements allowed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (_currentLevel?.elements != null)
                    {
                        _currentLevel.elements[index] = new Elements
                        {
                            Coords = new byte[2] { (byte)col, (byte)row },
                            Type = currentElementNumber
                        };
                    }
                }
            }

            return true;
        }

        private void RemoveELementClickedCell(int x, int y)
        {
            if (currentEditState != EditState.Elements) { return; }
            int col = x / cellSize;
            int row = y / cellSize;

            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                if (_currentLevel?.elements != null)
                { 
                    int index = Array.FindIndex(_currentLevel.elements, element =>
                    (element.Coords != null && element.Coords.Length >= 2 && element.Coords[0] == col && element.Coords[1] == row));
                    if (index == -1) { return;  }
                    _currentLevel.elements[index] = new Elements
                    {
                        Coords = new byte[2] { 32, 32 },
                        Type = 0
                    };
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog openFileDialog = new SaveFileDialog())
            {
                PrintMapArray();
                openFileDialog.Filter = "HAD Files|*.HAD|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Serialize the class to a binary file
                    using (FileStream fs = new(openFileDialog.FileName, FileMode.Create))
                    {  
                        _currentLevel.Serialise(new BinaryWriter(fs));
                    }
                }
            }
        }

        private void GenerateMapBinary_Click(object sender, EventArgs e)
        {
            PrintMapArray();
        }

        private void FloorHeightUpDown_ValueChanged(object sender, EventArgs e)
        {   
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.floorHeight = (byte)value;
        }

        private void CeilingHeightUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.ceilHeight = (byte)value;
        }

        private void FloorColourUpDown4_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.FloorColor = (byte)value;
        }

        private void CeilingColourUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.CeilingColor = (byte)value;
        }

        private void PlayerRotationUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.playerRotation = (byte)value;
        }


    }
}