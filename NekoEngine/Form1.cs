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
        private const byte PLAYER_POSITION_TYPE_INDEX = 99; 
        private const int GRID_SIZE = 64;
        private const int CELL_SIZE = 15;
        private const int AVAILABLE_ELEMENTS = 64;
        
        private const int ACCESS_CARD_1 = 0x0d;
        private const int ACCESS_CARD_2 = 0x0e;
        private const int ACCESS_CARD_3 = 0x0f;

        private const int LOCK_1 = 0x10;
        private const int LOCK_2 = 0x11;
        private const int LOCK_3 = 0x12;


        private bool _isMousePressed = false;
        private byte _currentElementNumber = 0;
        private byte _selectedHeight = 0;
        private EditState _currentEditState = EditState.Walls;
        private Color _selectedMapColour = Color.FromArgb(255, 255, 20, 147);
        private Level _currentLevel = new Level();
        private Bitmap _gridImage;
    
#if DEBUG
        const string GAME_FILE_LOCATION = @"c:\projects\Neko";
        const string GAME_SETTINGS_FILE_LOCATION = GAME_FILE_LOCATION + @"\settings.h";
        const string GAME_CONSTNTS_FILE_LOCATION = GAME_FILE_LOCATION + @"\constants.h";
#else
        const string GAME_FILE_LOCATION = @"..\";
#endif

        public Form1()
        {
           
            InitializeComponent();
            InitializeCodeEditor();
            InitializeGrid();
        }

        private void InitializeCodeEditor()
        {
            codeEditor.Margins[0].Width = 20;
            codeEditor.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            codeEditor.Styles[ScintillaNET.Style.Default].Size = 10;
            codeEditor.Lexer = Lexer.Cpp;
            codeEditor.Text = System.IO.File.ReadAllText(GAME_SETTINGS_FILE_LOCATION);

            scintilla1.Margins[0].Width = 20;
            scintilla1.Styles[ScintillaNET.Style.Default].Font = "Consolas";
            scintilla1.Styles[ScintillaNET.Style.Default].Size = 10;
            scintilla1.Lexer = Lexer.Cpp;
            scintilla1.Text = System.IO.File.ReadAllText(GAME_CONSTNTS_FILE_LOCATION);
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
                        Bitmap selectedImage = new Bitmap(openFileDialog.FileName);

                        if (selectedImage.Width != 32 || selectedImage.Height != 32)
                        {
                            ShowErrorMessage("The selected image must be 32x32 pixels.");
                            return;
                        }


                        pictureBox1.Image = selectedImage;
                        _imagePath = openFileDialog.FileName;
                        generate.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Error loading the image: {ex.Message}");                        
                    }
                }
            }
        }

        private void generate_Click(object sender, EventArgs e)
        {
            try
            {
                string scriptPath = $"{GAME_FILE_LOCATION}\\assets\\img2array.py";

                string command = $"python {scriptPath} -t -c -x32 -y32 -p{GAME_FILE_LOCATION}\\assets\\palette565.png {_imagePath}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    process.StandardInput.WriteLine(command);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();

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
                ShowErrorMessage($"Error running the Python script: {ex.Message}");                
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
                        ShowErrorMessage($"Error loading the image: {ex.Message}");
                    }
                }
            }
        }

        private void GenerateAudioArray_Click(object sender, EventArgs e)
        {
            try
            {
                string scriptPath = $"{GAME_FILE_LOCATION}\\assets\\snd2array.py";

                string command = $"python {scriptPath} {_audioPath}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    process.StandardInput.WriteLine(command);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();

                
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
                ShowErrorMessage($"Error running the Python script: {ex.Message}");                
            }
        }

        private void copyAudioToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(copyAudioToClipboard.Text);

            MessageBox.Show("Text copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        private void SaveCode_Click(object sender, EventArgs e)
        {
            System.IO.File.WriteAllText(GAME_SETTINGS_FILE_LOCATION, codeEditor.Text);
        }

        private void CodeSaveAs_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, codeEditor.Text);
                }
            }
        }

        private void InitializeGrid()
        {
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Size = new Size(GRID_SIZE * CELL_SIZE, GRID_SIZE * CELL_SIZE);

            _gridImage = new Bitmap(GRID_SIZE * CELL_SIZE, GRID_SIZE * CELL_SIZE);

            using (Graphics g = Graphics.FromImage(_gridImage))
            {
                for (int i = 0; i < GRID_SIZE; i++)
                {
                    for (int j = 0; j < GRID_SIZE; j++)
                    {
                        g.FillRectangle(Brushes.White, j * CELL_SIZE, i * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                        g.DrawRectangle(Pens.Black, j * CELL_SIZE, i * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                    }
                }
            }

            pictureBox.Image = _gridImage;
            pictureBox.MouseDown += PictureBox_MouseDown;
            pictureBox.MouseMove += PictureBox_MouseMove;
            pictureBox.MouseUp += PictureBox_MouseUp;
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            var position = ToGridPosition(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                _isMousePressed = true;
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
            var gridPos = ToGridPosition(e.X, e.Y);
            if (_isMousePressed)
            {
                DrawOnGrid(gridPos);
            }
            Xpos.Text = gridPos.Column.ToString();
            Ypos.Text = gridPos.Row.ToString();
            
        }

        private void DrawOnGrid(GridPosition position)
        {
            if (_currentLevel != null)
            {
                switch (_currentEditState)
                {
                    case EditState.Walls:
                        RecordMapArrayClick(position);
                        break;
                    case EditState.Elements:
                        RecoredElementClicked(position);
                        break;
                }

                using (Graphics g = Graphics.FromImage(_gridImage))
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
            _isMousePressed = false;
        }

        private void RecordMapArrayClick(GridPosition position)
        {
            int col = position.Column;
            int row = position.Row;

            if (col >= 0 && col < GRID_SIZE && row >= 0 && row < GRID_SIZE)
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
                return (byte)(GetTextureIndexFromColour(_selectedMapColour) + (7 * _currentLevel.HeightArray[pos.MapArrayIndex()]));
            }
            throw new Exception("Level is null for some reason");
        }


        private int GetMapArrayDrawHeightFromIndex(int index)
        {
            if (index < 15 || index > 63) { return CELL_SIZE; }

            if (index >= 15 && index < 22) { return 3; }
            if (index >= 22 && index < 29) { return 4; }
            if (index >= 29 && index < 36) { return 5; }
            if (index >= 36 && index < 43) { return 6; }
            if (index >= 43 && index < 50) { return 7; }
            if (index >= 50 && index < 57) { return 8; }
            if (index >= 57 && index < 64) { return 9; }

            return CELL_SIZE;
        }

        private bool IsLockElement(byte element) => element >= 0x10 && element <= 0x12;       

        private void RecoredElementClicked(GridPosition position)
        {
            const int STEP_ELEMENT_INDEX_START = 15;
            var index = (position.Row * GRID_SIZE) + position.Column;

            if (IsLockElement(_currentElementNumber))
            {
                if ((_currentLevel.MapArray[index] & DOOR_MASK) < DOOR_MASK)
                {
                    ShowErrorMessage("Lock elements can only be placed on door entities");
                    return;
                }
            }
            else if (_currentLevel.MapArray[index] > 0 && _currentLevel.MapArray[index] < STEP_ELEMENT_INDEX_START)
            {
                ShowErrorMessage("ELements can't be placed on walls");
                return;
            }
            else if (_currentLevel.MapArray[index] > DOOR_MASK)
            {
                ShowErrorMessage("Only locks can't be placed on doors");
                return;
            }

            if (!RecordELementClickedCell(position))
            {
                _isMousePressed = false;
                return;
            };
        }

        private void RemoveColorFromGrid(GridPosition position)
        {
            if (_currentLevel != null && _currentLevel.MapArray != null)
            {
                var index = (position.Row * GRID_SIZE) + position.Column;
                _currentLevel.MapArray[index] = 0;

                int col = position.Column;
                int row = position.Row;

                if (col >= 0 && col < GRID_SIZE && row >= 0 && row < GRID_SIZE)
                {
                    using (Graphics g = Graphics.FromImage(_gridImage))
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
            g.FillRectangle(Brushes.White, pos.Column * CELL_SIZE, pos.Row * CELL_SIZE, CELL_SIZE, CELL_SIZE);
        }

        private static void DrawGrid(Graphics g)
        {
            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    g.DrawRectangle(Pens.Black, j * CELL_SIZE, i * CELL_SIZE, CELL_SIZE, CELL_SIZE);
                }
            }
        }

        private void HandlWallSelectionClicked(Color col)
        {
            _selectedMapColour = col;
            SelectedElement.Text = GetSelectedElementName();
            _currentEditState = EditState.Walls;            
        }

        private void MapColour_1_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                HandlWallSelectionClicked(button.BackColor);
            }
             _currentEditState = EditState.Walls;
        }

        private void MapColour_door_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                HandlWallSelectionClicked(button.BackColor);
            }
            ResetCellHeight();
            _currentEditState = EditState.Walls;
        }

        private void ResetCellHeight()
        {
            _selectedHeight = 0;
            CellHeight.Value = _selectedHeight;
        }

        private void ClearMap_Click(object sender, EventArgs e)
        {
            using (Graphics g = Graphics.FromImage(_gridImage))
            {
                g.Clear(Color.White);

                DrawGrid(g);
            }

            pictureBox.Invalidate(); // Force redraw
            _currentLevel = new Level();
            UpdateRemainingElements();
        }

        const byte DOOR_MASK = 0xc0;
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

        private void HandleElementClicked(byte elementNumber)
        {
            _currentElementNumber = elementNumber;
            _currentEditState = EditState.Elements;            
             SelectedElement.Text =  GetSelectedElementName();
        }

        private void ElementButton_1_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x01);
        }

        private void ElementButton_2_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x02);
        }

        private void ElementButton_3_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x03);            
        }

        private void ElementButton_4_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x04);
        }

        private void ElementButton_5_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x05);            
        }

        private void ElementButton_6_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x06);            
        }

        private void ElementButton_7_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x07);            
        }

        private void ElementButton_8_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x08);            
        }

        private void ElementButton_9_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x09);            
        }

        private void ElementButton_10_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x0a);            
        }

        private void ElementButton_11_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x0b);
        }

        private void ElementButton_12_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x0c);
        }

        private void ElementButton_13_Click(object sender, EventArgs e)
        {
            HandleElementClicked(ACCESS_CARD_1);
        }

        private void ElementButton_14_Click(object sender, EventArgs e)
        {
            HandleElementClicked(ACCESS_CARD_2);            
        }

        private void ElementButton_15_Click(object sender, EventArgs e)
        {
            HandleElementClicked(ACCESS_CARD_3);            
        }

        private void ElementButton_16_Click(object sender, EventArgs e)
        {
            HandleElementClicked(LOCK_1);            
        }

        private void ElementButton_17_Click(object sender, EventArgs e)
        {
            HandleElementClicked(LOCK_2);
        }

        private void ElementButton_18_Click(object sender, EventArgs e)
        {
            HandleElementClicked(LOCK_3);
        }

        private void ElementButton_19_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x13);
        }

        private void ElementButton_20_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x20);
        }

        private void ElementButton_21_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x21);
        }

        private void ElementButton_22_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x22);
        }

        private void ElementButton_23_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x23);
        }

        private void ElementButton_24_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x24);
        }

        private void ElementButton_25_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x25);
        }

        private void ElementButton_26_Click(object sender, EventArgs e)
        {
            HandleElementClicked(0x26);
        }

        private void Player_Click(object sender, EventArgs e)
        {
            HandleElementClicked(PLAYER_POSITION_TYPE_INDEX);            
        }

        private void UpdateRemainingElements()
        {
            var c = _currentLevel.elements.Where(x => x.Type > 0).Count();
            RemainingElementsLabel.Text = (AVAILABLE_ELEMENTS - c).ToString();
        }

        private bool RecordELementClickedCell(GridPosition position)
        {            
            int col = position.Column;
            int row = position.Row;

            if (col >= 0 && col < GRID_SIZE && row >= 0 && row < GRID_SIZE)
            {
                if (_currentLevel?.elements != null)
                {
                    int index = Array.FindIndex(_currentLevel.elements, element =>
                        element.Type == 0 ||
                        (element.Coords != null && element.Coords.Length >= 2 && element.Coords[0] == col && element.Coords[1] == row));

                    if (index == -1)
                    {
                        ShowErrorMessage($"You have exceeded the maximum amount of { AVAILABLE_ELEMENTS } available elements.");
                        return false;
                    }

                    if (_currentLevel?.elements != null)
                    {
                        _currentLevel.elements[index] = new Elements
                        {
                            Coords = new byte[2] { (byte)col, (byte)row },
                            Type = _currentElementNumber
                        };
                    }
                }
            }
            UpdateRemainingElements();
            return true;
        }
        

        private void RemoveELementClickedCell(GridPosition position)
        {            
            int col = position.Column;
            int row = position.Row;

            if (col >= 0 && col < GRID_SIZE && row >= 0 && row < GRID_SIZE)
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
                    UpdateRemainingElements();
                }
            }
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


        private void RedrawWholeMapArray(Graphics g)
        {
            int row = 0;
            int col = 0;
 
            for (int i = 0; i < _currentLevel.MapArray.Length; i++)
            {
                if (i > 0 && i % GRID_SIZE == 0)
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

            if (position.Column >= 0 && position.Column < GRID_SIZE && position.Row >= 0 && position.Row < GRID_SIZE)
            {
                var brushHeight = GetMapArrayDrawHeightFromIndex(_currentLevel.MapArray[index]);
                Brush brush = new SolidBrush(mapColour);

                int startY = ((position.Row + 1) * CELL_SIZE) - (brushHeight);

                g.FillRectangle(brush, position.Column * CELL_SIZE, startY, CELL_SIZE, brushHeight);
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
                _currentElementNumber = element.Type;

                Color textColor = Color.Black;

                // If enemy, draw as red
                if (_currentElementNumber >= 20)
                {
                    textColor = Color.Red;
                }

                else if (IsLockElement(_currentElementNumber))
                {
                    textColor = Color.FromArgb(255,0,200,0);
                }

                if (_currentElementNumber == PLAYER_POSITION_TYPE_INDEX)
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

                PointF textLocation = new PointF((element.Coords[0] * CELL_SIZE) + CELL_SIZE / 30, (element.Coords[1] * CELL_SIZE) + CELL_SIZE / 30);
                if (_currentElementNumber == PLAYER_POSITION_TYPE_INDEX)
                {
                    g.DrawString("P", DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == ACCESS_CARD_1)
                {
                    g.DrawString("K1", DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == ACCESS_CARD_2)
                {
                    g.DrawString("K2", DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == ACCESS_CARD_3)
                {
                    g.DrawString("K3", DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == LOCK_1)
                {
                    g.DrawString("L1", DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == LOCK_2)
                {
                    g.DrawString("L2", DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == LOCK_3)
                {
                    g.DrawString("L3", DefaultFont, textBrush, textLocation);
                }
                else
                {
                    g.DrawString(_currentElementNumber.ToString(), DefaultFont, textBrush, textLocation);
                }
            }
        }

        private static GridPosition ToGridPosition(int x, int y) => new(x / CELL_SIZE, y / CELL_SIZE);

        private void CellHeight_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            int value = (int)numericUpDown.Value;
            _selectedHeight = (byte)value;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog openFileDialog = new SaveFileDialog())
            {

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

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
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

            using (Graphics g = Graphics.FromImage(_gridImage))
            {
                RedrawWholeMapArray(g);

                // Draw Elements
                var originalType = _currentElementNumber;
                RedrawAllElements(g);

                // Draw player position
                _currentElementNumber = PLAYER_POSITION_TYPE_INDEX;
                RecoredElementClicked(new GridPosition(_currentLevel.PlayerStart[0], _currentLevel.PlayerStart[1]));
                _currentElementNumber = originalType;

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

        private string GetSelectedElementName()
        {
            if (_currentEditState == EditState.Elements)
            {
                switch (_currentElementNumber)
                {
                    case PLAYER_POSITION_TYPE_INDEX:
                        return "Player";
                    case ACCESS_CARD_1:
                        return "Key 1";
                    case ACCESS_CARD_2:
                        return "Key 2";
                    case ACCESS_CARD_3:
                        return "Key 3";
                    case LOCK_1:
                        return "Lock 1";
                    case LOCK_2:
                        return "Lock 2";
                    case LOCK_3:
                        return "Lock 3";
                }

                return _currentElementNumber.ToString();
            }
            else
            {                
                var mask = GetTextureIndexFromColour(_selectedMapColour) & DOOR_MASK;
                if (mask >= DOOR_MASK)
                {
                    return "Door";
                }

                if (_selectedHeight > 0)
                {
                    return "Steps";
                }

                return "Wall";
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            _isMousePressed = false;
        }

        private void ShowErrorMessage(string error)
        {
            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _isMousePressed = false;
        }

        private void SaveConstants_Click(object sender, EventArgs e)
        {        
            System.IO.File.WriteAllText(GAME_CONSTNTS_FILE_LOCATION, codeEditor.Text);            
        }

        private void SaveConstantsAs_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, scintilla1.Text);
                }
            }
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