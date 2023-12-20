using System.Diagnostics;using System.Text;
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
        private const byte PLAYER_POSITION_TYPE = 99; 
        private Level _currentLevel = new Level();
        private EditState currentEditState = EditState.Walls;
#if DEBUG
        const string GAME_FILE_LOCATION = @"c:\projects\Neko";
#else
        const string GAME_FILE_LOCATION = @"..\";
#endif
        private const int gridSize = 64;
        private const int cellSize = 15; // Adjust the cell size as needed
        private Bitmap gridImage;
        private bool isMousePressed = false;
        private byte currentElementNumber = 0;

        Color selectedMapColour = Color.FromArgb(255, 255, 20, 147);
        private byte _selectedHeight = 0;
    

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
            codeEditor.Text = System.IO.File.ReadAllText(GAME_FILE_LOCATION + @"\game.h");
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
                string scriptPath = $"{GAME_FILE_LOCATION}\\assets\\img2array.py";

                // Construct the command with the provided arguments
                string command = $"python {scriptPath} -t -c -x32 -y32 -p{GAME_FILE_LOCATION}\\assets\\palette565.png {_imagePath}";

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
            Process.Start($"{GAME_FILE_LOCATION}\\anarch.exe");
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
                process.StandardInput.WriteLine($"code {GAME_FILE_LOCATION}");
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
                string scriptPath = $"{GAME_FILE_LOCATION}\\assets\\snd2array.py";

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
            System.IO.File.WriteAllText(GAME_FILE_LOCATION + @"\game.h", codeEditor.Text);
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
            var position = ToGridPosition(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                isMousePressed = true;
                DrawOnGrid(position);
            }
            else if (e.Button == MouseButtons.Right)
            {
                RemoveColorFromGrid(position);
                RemoveELementClickedCell(position);
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMousePressed)
            {
                DrawOnGrid(ToGridPosition(e.X, e.Y));
            }
        }

        private void DrawOnGrid(GridPosition position)
        {
            if (_currentLevel != null)
            {
                switch (currentEditState)
                {
                    case EditState.Walls:
                        RecordMapArrayClick(position);
                        break;
                    case EditState.Elements:
                        RecoredElementClicked(position);
                        break;
                }

                using (Graphics g = Graphics.FromImage(gridImage))
                {
                    ClearGrid(g, position);
                    DrawMapArrayBlock(g, position);
                    DrawElementBLock(g, _currentLevel.GetElementAtPosition(position));                    
                    DrawGrid(g);
                    pictureBox.Invalidate();
                }
            }
            else
            {
                throw new Exception("Level object is null for some reason");
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isMousePressed = false;
        }

        private void RecordMapArrayClick(GridPosition position)
        {
            int col = position.Column;
            int row = position.Row;

            if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
            {
                var index = position.MapArrayIndex();

                _currentLevel.HeightArray[index] = _selectedHeight;
                    
                if (_currentLevel != null) 
                {
                    _currentLevel.MapArray[index] = GetMapArrayTextureIdFromPosition(position);
                }
            }
        }


        private byte GetMapArrayTextureIdFromPosition(in GridPosition pos)
        {   
            if (_currentLevel != null)
            {
                return (byte)(GetTextureIndexFromColour(selectedMapColour) + (7 * _currentLevel.HeightArray[pos.MapArrayIndex()]));
            }
            throw new Exception("Level is null for some reason");
        }


        private int GetMapArrayDrawHeightFromIndex(int index)
        {
            if (index < 15 || index > 63) { return cellSize; }

            if (index >= 15 && index < 22) { return 3; }
            if (index >= 22 && index < 29) { return 4; }
            if (index >= 29 && index < 36) { return 5; }
            if (index >= 36 && index < 43) { return 6; }
            if (index >= 43 && index < 50) { return 7; }
            if (index >= 50 && index < 57) { return 8; }
            if (index >= 57 && index < 64) { return 9; }

            return cellSize;
        }
            

        private void RecoredElementClicked(GridPosition position)
        {
            if (!RecordELementClickedCell(position))
            {
                isMousePressed = false;
                return;
            };
        }

        private void RemoveColorFromGrid(GridPosition position)
        {
            if (_currentLevel != null && _currentLevel.MapArray != null)
            {
                var index = (position.Row * gridSize) + position.Column;
                _currentLevel.MapArray[index] = 0;

                int col = position.Column;
                int row = position.Row;

                if (col >= 0 && col < gridSize && row >= 0 && row < gridSize)
                {
                    using (Graphics g = Graphics.FromImage(gridImage))
                    {
                        ClearGrid(g, position);
                        DrawGrid(g);
                    }

                    pictureBox.Invalidate(); // Force redraw
                }
            }
        }

        private void ClearGrid(in Graphics g, GridPosition pos)
        {
            g.FillRectangle(Brushes.White, pos.Column * cellSize, pos.Row * cellSize, cellSize, cellSize);
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

        private byte[] GetColoredCellsArray()
        {
            byte[] coloredCellsArray = new byte[gridSize * gridSize];

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Color cellColor = gridImage.GetPixel((j * cellSize) + (cellSize / 2), (i * cellSize) + (cellSize - 2));
                    
                    coloredCellsArray[i * gridSize + j] = (byte)(GetTextureIndexFromColour(cellColor) + (7 * _currentLevel.HeightArray[i * gridSize + j]));
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

        const int DOOR_MASK = 0xc0;
        private byte GetTextureIndexFromColour(Color color)
        {

            if (color.RgbEquels(Color.FromArgb(255, 26, 26, 26)))
            {
                return 1 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 77, 77, 77)))
            {
                return 2 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 128, 128, 128)))
            {
                return 3 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 153, 153, 153)))
            {
                return 4 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 179, 179, 179)))
            {
                return 5 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 204, 204, 204)))
            {
                return 6 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 230, 230, 230)))
            {
                return 7 | DOOR_MASK;
            }

            if (color.RgbEquels(Color.FromArgb(255, 255, 20, 147)))
            { 
                return color.GetIndexFromColour(8);
            }

            if (color.RgbEquels(Color.FromArgb(255, 255, 0, 0)))
            {
                return color.GetIndexFromColour(9);
            }

            if (color.RgbEquels(Color.FromArgb(255, 192, 192, 0)))
            {
                return color.GetIndexFromColour(10);
            }

            if (color.RgbEquels(Color.FromArgb(255, 0, 0, 255)))
            {
                return color.GetIndexFromColour(11);
            }

            if (color.RgbEquels(Color.FromArgb(255, 128, 0, 0)))
            {
                return color.GetIndexFromColour(12);
            }

            if (color.RgbEquels(Color.FromArgb(255, 255, 255, 0)))
            {
                return color.GetIndexFromColour(13);
            }

            if (color.RgbEquels(Color.FromArgb(255, 0, 128, 0)))
            {
                return color.GetIndexFromColour(14);
            }

            return 0;
        }

        private Color GetColourFromTextureIndex(int index)
        {
            var newColour = Color.FromArgb(255, 255, 255, 255);
            switch (index)
            {
                case 1 | DOOR_MASK:
                    newColour = Color.FromArgb(255, 26, 26, 26);
                    break;
                case 2 | DOOR_MASK:
                    newColour = Color.FromArgb(255, 77, 77, 77);
                    break;
                case 3 | DOOR_MASK:
                    newColour = Color.FromArgb(255, 128, 128, 128);
                    break;
                case 4 | DOOR_MASK:
                    newColour =  Color.FromArgb(255, 153, 153, 153);
                    break;
                case 5 | DOOR_MASK:
                    newColour = Color.FromArgb(255, 179, 179, 179);
                    break;
                case 6 | DOOR_MASK:
                    newColour = Color.FromArgb(255, 204, 204, 204);
                    break;
                 case 7 | DOOR_MASK:
                    newColour = Color.FromArgb(255, 230, 230, 230);
                    break;
                case 8:
                case 8 + 7:
                case 8 + 14:
                case 8 + 21:
                case 8 + 28:
                case 8 + 35:
                case 8 + 42:
                case 8 + 49:
                    newColour =  Color.FromArgb(255, 255, 20, 147);
                    break;
                case 9:
                case 9 + 7:
                case 9 + 14:
                case 9 + 21:
                case 9 + 28:
                case 9 + 35:
                case 9 + 42:
                case 9 + 49:
                    newColour = Color.FromArgb(255, 255, 0, 0);
                    break;
                case 10:
                case 10 + 7:
                case 10 + 14:
                case 10 + 21:
                case 10 + 28:
                case 10 + 35:
                case 10 + 42:
                case 10 + 49:
                    newColour = Color.FromArgb(255, 192, 192, 0);
                    break;
                case 11:
                case 11 + 7:
                case 11 + 14:
                case 11 + 21:
                case 11 + 28:
                case 11 + 35:
                case 11 + 42:
                case 11 + 49:
                    newColour = Color.FromArgb(255, 0, 0, 255);
                    break;
                case 12:
                case 12 + 7:
                case 12 + 14:
                case 12 + 21:
                case 12 + 28:
                case 12 + 35:
                case 12 + 42:
                case 12 + 49:
                    newColour =Color.FromArgb(255, 128, 0, 0);
                    break;
                case 13:
                case 13 + 7:
                case 13 + 14:
                case 13 + 21:
                case 13 + 28:
                case 13 + 35:
                case 13 + 42:
                case 13 + 49:
                    newColour = Color.FromArgb(255, 255, 255, 0);
                    break;
                case 14:
                case 14 + 7:
                case 14 + 14:
                case 14 + 21:
                case 14 + 28:
                case 14 + 35:
                case 14 + 42:
                case 14 + 49:
                    newColour = Color.FromArgb(255, 0, 128, 0);
                    break;
            }

            return newColour;
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
            currentElementNumber = PLAYER_POSITION_TYPE;
            currentEditState = EditState.Elements;
        }

        private bool RecordELementClickedCell(GridPosition position)
        {
            int col = position.Column;
            int row = position.Row;

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
        

        private void RemoveELementClickedCell(GridPosition position)
        {
            int col = position.Column;
            int row = position.Row;

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
            _currentLevel.PlayerStart[2] = (byte)value;
        }

        private void LoadMap_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "HAD Files|*.HAD|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Serialize the class to a binary file
                    using (FileStream fs = new(openFileDialog.FileName, FileMode.Open))
                    {
                        _currentLevel.Deserialise(new BinaryReader(fs));
                    }
                }
            }

        
       
            using (Graphics g = Graphics.FromImage(gridImage))
            {
                RedrawWholeMapArray(g);

                // Draw Elements
                var originalType = currentElementNumber;
                RedrawAllElements(g);

                // Draw player position
                currentElementNumber = PLAYER_POSITION_TYPE;
                RecoredElementClicked(new GridPosition(_currentLevel.PlayerStart[0], _currentLevel.PlayerStart[1]));
                currentElementNumber = originalType;

                // Update GUI values
                PlayerRotationUpDown.Value = _currentLevel.PlayerStart[2];
                FloorHeightUpDown.Value = _currentLevel.floorHeight;
                CeilingHeightUpDown.Value = _currentLevel.ceilHeight;
                CeilingColourUpDown.Value = _currentLevel.CeilingColor;
                FloorColourUpDown4.Value = _currentLevel.FloorColor;

                DrawGrid(g);
                pictureBox.Invalidate();
            }
        }

        private void RedrawWholeMapArray(Graphics g)
        {
            int row = 0;
            int col = 0;
 
            for (int i = 0; i < _currentLevel.MapArray.Length; i++)
            {
                if (i > 0 && i % gridSize == 0)
                {
                    row++;
                    col = 0;
                }

                DrawMapArrayBlock(g, new GridPosition(col, row));
                col++;
            }
        }

        private void DrawMapArrayBlock(in Graphics g, GridPosition position)
        {
            var index = position.MapArrayIndex();

            var mapColour = GetColourFromTextureIndex(_currentLevel.MapArray[index]);

            if (position.Column >= 0 && position.Column < gridSize && position.Row >= 0 && position.Row < gridSize)
            {
                var brushHeight = GetMapArrayDrawHeightFromIndex(_currentLevel.MapArray[index]);
                Brush brush = new SolidBrush(mapColour);

                int startY = ((position.Row + 1) * cellSize) - (brushHeight);

                g.FillRectangle(brush, position.Column * cellSize, startY, cellSize, brushHeight);
            }
        }


        private void RedrawAllElements(Graphics g)
        {
            foreach (var element in _currentLevel.elements)
            {
                DrawElementBLock(g, element);
            }
        }

        private void DrawElementBLock(in Graphics g, Elements element)
        {
            if (element.Type > 0)
            {
                currentElementNumber = element.Type;

                Color textColor = Color.Black;

                // If enemy, draw as red
                if (currentElementNumber >= 20)
                {
                    textColor = Color.Red;
                }

                if (currentElementNumber == PLAYER_POSITION_TYPE)
                {
                    if (_currentLevel?.PlayerStart != null)
                    {
                        RemoveColorFromGrid(new GridPosition(_currentLevel.PlayerStart[0], _currentLevel.PlayerStart[1]));
                    }
                    if (_currentLevel != null)
                    {
                        _currentLevel.PlayerStart = new byte[3] { element.Coords[0], element.Coords[1], _currentLevel.PlayerStart[2] };
                    }

                    textColor = Color.Blue;
                }

                Brush textBrush = new SolidBrush(textColor);
                // Draw the number in the cell

                PointF textLocation = new PointF((element.Coords[0] * cellSize) + cellSize / 30, (element.Coords[1] * cellSize) + cellSize / 30);
                if (currentElementNumber == PLAYER_POSITION_TYPE)
                {
                    g.DrawString("P", DefaultFont, textBrush, textLocation);
                }
                else
                {
                    g.DrawString(currentElementNumber.ToString(), DefaultFont, textBrush, textLocation);
                }
            }
        }

        private static GridPosition ToGridPosition(int x, int y) => new(x / cellSize, y / cellSize);

        private void CellHeight_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            int value = (int)numericUpDown.Value;
            _selectedHeight = (byte)value;
        }
    }

    internal record GridPosition(int Column, int Row)
    { 
        internal int MapArrayIndex()
        {
            var index = (Row * 64) + Column;
            return index;
        }
    };
}