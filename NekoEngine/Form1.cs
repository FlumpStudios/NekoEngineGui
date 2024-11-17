using System.Diagnostics;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace NekoEngine
{
    public enum EditState
    {
        Walls = 0,
        Elements = 1
    }

    public partial class Form1 : Form
    {
        const string EXE_NAME = "Ruyn";
        const decimal DEGRESS_TO_BYTE_CONVERSION = 1.4117M;
        private const int GRID_SIZE = 64;
        private const int CELL_SIZE = 14;
        private const int AVAILABLE_ELEMENTS = Level.MAX_ELEMENT_SIZE;
        private const int ENEMIES_INDEX_OFFSET = 31;

        private const int ACCESS_CARD_1 = 0x0d;
        private const int ACCESS_CARD_2 = 0x0e;
        private const int ACCESS_CARD_3 = 0x0f;

        private const int LOCK_1 = 0x10;
        private const int LOCK_2 = 0x11;
        private const int LOCK_3 = 0x12;

        private const int BLOCKER = 0x13;


        private bool _isMousePressed = false;
        private byte _currentElementNumber = 0;
        private byte _selectedHeight = 0;
        private EditState _currentEditState = EditState.Walls;
        private Color _selectedMapColour = Color.FromArgb(255, 255, 20, 147);
        private Level _currentLevel = new Level();
        private Bitmap _gridImage;
        private SoundPlayer _soundPlayer;

        private int _selectedSfxForPreview = 0;
        private string _levelPack = "Original";

#if DEBUG
        const string GAME_FILE_LOCATION = @"c:\projects\NekoEngine\GameData";

#else
        const string GAME_FILE_LOCATION = @".\";
#endif
        const string GAME_SETTINGS_FILE_LOCATION = GAME_FILE_LOCATION + @"\settings.h";
        const string GAME_CONSTNTS_FILE_LOCATION = GAME_FILE_LOCATION + @"\constants.h";
        const string WALL_TEXTURE_FOLDER = @"\WallTextures";
        const string ITEMS_TEXTURE_FOLDER = @"\Items";
        const string BACKGROUND_TEXTURE_FOLDER = @"\Backgrounds";
        const string WEAPONS_TEXTURE_FOLDER = @"\Weapons";
        const string EFFECTS_TEXTURE_FOLDER = @"\Effects";
        const string ENEMIES_TEXTURE_FOLDER = @"\Enemies";
        const string TITLE_TEXTURE_FOLDER = @"\Title";
        const string LEVELS_FOLDER = @"Levels";
        const string SFX_FOLDER = @"\Sfx";
        const byte DEBUG_LEVEL_ID = 99;

        public Form1(string levelPack)
        {
            if (levelPack is not null)
            {
                _levelPack = levelPack;
            }

            string filePath = Path.Combine(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack);
            if (!Directory.Exists(Path.Combine(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack)))
            {
                ShowErrorMessage($"Could not find level pack {_levelPack}");
            }

            _gridImage = new Bitmap(GRID_SIZE * CELL_SIZE, GRID_SIZE * CELL_SIZE);
            InitializeComponent();
            InitializeCodeEditor();
            InitializeGrid();
            InitEnemiesTexturesFile(GAME_FILE_LOCATION + ENEMIES_TEXTURE_FOLDER, EnemiesPictureBox_DoubleClick);
            LoadPreviewLevel();
            _soundPlayer = new SoundPlayer();
            UpdateButtonTextures();

        }

        private void UpdateButtonTextures()
        {
            MapColour_door.Image = MapColour_1.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown0.Value - 1) + ".png"));
            MapColour_door2.Image = MapColour_2.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown1.Value - 1) + ".png"));
            MapColour_door3.Image = MapColour_3.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown2.Value - 1) + ".png"));
            MapColour_door4.Image = MapColour_4.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown3.Value - 1) + ".png"));
            MapColour_door5.Image = MapColour_5.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown4.Value - 1) + ".png"));
            MapColour_door6.Image = MapColour_6.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown5.Value - 1) + ".png"));
            MapColour_door7.Image = MapColour_7.Image = Image.FromFile(Path.Combine(GAME_FILE_LOCATION, "WallTextures", (TextureAllocationUpDown6.Value - 1) + ".png"));
        }


        private void InitEnemiesTexturesFile(string path, EventHandler action)
        {
            if (Directory.Exists(path))
            {
                var imageFileLocations = Directory.GetFiles(path, "*.png").Where(x => !string.IsNullOrEmpty(x) && x.Split("\\").LastOrDefault().StartsWith("o")).ToArray();


                imageFileLocations = imageFileLocations.OrderBy(GetNumericPart).ToArray();

                List<Image> imageFiles = new();

                for (int i = 0; i < imageFileLocations.Length; i++)
                {
                    using (FileStream stream = new FileStream(imageFileLocations[i], FileMode.Open, FileAccess.Read))
                    {
                        Image originalImage = Image.FromStream(stream);
                        Image resizedImage = new Bitmap(originalImage, new Size(32, 32));
                        imageFiles.Add(originalImage);
                    }
                }
            }
        }


        private void InitTexturesFile(string path, EventHandler action, FlowLayoutPanel panel)
        {
            if (Directory.Exists(path))
            {
                string[] imageFiles = Directory.GetFiles(path, "*.png").Where(x => !string.IsNullOrEmpty(x) && x.Split("\\").LastOrDefault().StartsWith("o")).ToArray();

                imageFiles = imageFiles.OrderBy(GetNumericPart).ToArray();

                for (int i = 0; i < imageFiles.Length; i++)
                {
                    using (FileStream stream = new FileStream(imageFiles[i], FileMode.Open, FileAccess.Read))
                    {
                        Image originalImage = Image.FromStream(stream);
                        Image resizedImage = new Bitmap(originalImage, new Size(32, 32));

                        PictureBox pictureBox = new PictureBox
                        {
                            Image = resizedImage,
                            Size = new Size(32, 32),
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Tag = i
                        };

                        Label label = new Label()
                        {
                            Width = 26,
                            Text = (i + 1).ToString() + "."
                        };

                        if (panel.Name.Equals("wallTextureLayoutPanel", StringComparison.OrdinalIgnoreCase) && i == 15)
                        {
                            label.Width = 33;
                            label.Text = "Door";
                        }



                        if (string.Equals(GAME_FILE_LOCATION + ITEMS_TEXTURE_FOLDER, path))
                        {

                            label.Text = GetItemNameFromIndex(i);
                            label.Width = 50;
                        }
                        else if (string.Equals(GAME_FILE_LOCATION + WEAPONS_TEXTURE_FOLDER, path))
                        {
                            label.Text = GetWeaponNameFromIndex(i);
                            label.Width = 52;
                        }
                        else if (string.Equals(GAME_FILE_LOCATION + EFFECTS_TEXTURE_FOLDER, path))
                        {
                            label.Text = GetEffectNameFromIndex(i);
                            label.Width = 60;
                        }

                        pictureBox.DoubleClick += action;
                        panel.Controls.Add(label);
                        panel.Controls.Add(pictureBox);
                    }
                }
            }
        }


        private string GetWeaponNameFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return "Knife";
                case 1:
                    return "Shotgun";
                case 2:
                    return "Auto_Shotgun";
                case 3:
                    return "Bazooka";
                case 4:
                    return "Plasma";
                case 5:
                    return "Spread";
                default:
                    return "";
            }
        }

        private string GetItemNameFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return "Barrel";
                case 1:
                    return "Health";
                case 2:
                    return "Bullets";
                case 3:
                    return "Rockets";
                case 4:
                    return "Plasma";
                case 5:
                    return "Deco 1";
                case 6:
                    return "Finish";
                case 7:
                    return "Teleport";
                case 8:
                    return "Deco 2";
                case 9:
                    return "Deco 3";
                case 10:
                    return "Gem";
                case 11:
                    return "Deco 4";
                case 12:
                    return "Key Card";
                default:
                    return "";
            }
        }

        private string GetEffectNameFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return "Explosion";
                case 1:
                    return "Fireball";
                case 2:
                    return "Plasma";
                case 3:
                    return "Dust";
                default:
                    return string.Empty;
            }
        }

        static int GetNumericPart(string fileName)
        {
            string numericPart = Path.GetFileNameWithoutExtension(fileName);

            if (numericPart.StartsWith("o", StringComparison.OrdinalIgnoreCase))
            {
                numericPart = numericPart.Split("_").Last();
            }


            // Try to parse the numeric part as an integer
            if (int.TryParse(numericPart, out int numericValue))
            {
                return numericValue;
            }
            else
            {
                return 0;
            }
        }

        private void InitializeCodeEditor()
        {
        }

        private string _imagePath = string.Empty;
        private string _audioPath = string.Empty;

        private List<byte> GetTextureArray(string fileLocation)
        {
            var response = new List<byte>();

            string scriptPath = $"{GAME_FILE_LOCATION}\\assets\\img2array.py";

            string command = $"python {scriptPath} -t -c -x32 -y32 -p{GAME_FILE_LOCATION}\\assets\\palette565.png {fileLocation}";

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

                    foreach (var n in numbers.Split(','))
                    {
                        var parseOK = byte.TryParse(n, out byte o);
                        if (parseOK)
                        {
                            response.Add(o);
                        }
                        else
                        {
                            ShowErrorMessage("Could not generate array from image");
                            break;
                        }
                    }
                }
            }

            return response;
        }

        private List<byte> GetAudioArray(string fileLocation)
        {
            var response = new List<byte>();

            string scriptPath = $"{GAME_FILE_LOCATION}\\assets\\snd2array.py";

            string command = $"python {scriptPath} {fileLocation}";

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
                    foreach (var n in numbers.Split(','))
                    {
                        var parseOK = byte.TryParse(n, out byte o);
                        if (parseOK)
                        {
                            response.Add(o);
                        }
                        else
                        {
                            ShowErrorMessage("Could not generate array from raw file");
                            break;
                        }
                    }
                }
            }

            return response;
        }

        private void RunGame()
        {
            Environment.CurrentDirectory = GAME_FILE_LOCATION;

            // Start the process with the specified working directory
            Process.Start(new ProcessStartInfo
            {

                FileName = $"{GAME_FILE_LOCATION}\\{EXE_NAME}.exe",
                WorkingDirectory = GAME_FILE_LOCATION,
                Arguments = $"-p {_levelPack}"
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RunGame();
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
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error running the Python script: {ex.Message}");
            }
        }

        private void InitializeGrid()
        {
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox.Size = new Size(GRID_SIZE * CELL_SIZE, GRID_SIZE * CELL_SIZE);

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

        private void BackgroundPictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + BACKGROUND_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private void WeaponsPictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + WEAPONS_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private void ItemPictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + ITEMS_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private void WallPictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + WALL_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private void EnemiesPictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + ENEMIES_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private void EFfectsPictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + EFFECTS_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private void TitlePictureBox_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                pictureBox.Image = LoadInTextureFile(index, GAME_FILE_LOCATION + TITLE_TEXTURE_FOLDER) ?? pictureBox.Image;
            }
        }

        private Bitmap TransparancyReplace(Bitmap originalBitmap)
        {
            Color replacementColor = Color.FromArgb(248, 0, 0);
            Bitmap modifiedBitmap = new Bitmap(originalBitmap);

            for (int x = 0; x < modifiedBitmap.Width; x++)
            {
                for (int y = 0; y < modifiedBitmap.Height; y++)
                {
                    // Check if the pixel is transparent
                    if (modifiedBitmap.GetPixel(x, y).A == 0)
                    {
                        // Replace transparent pixel with the specified color
                        modifiedBitmap.SetPixel(x, y, replacementColor);
                    }
                }
            }

            return modifiedBitmap;
        }

        private Bitmap? LoadInTextureFile(int index, string location)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.png;|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        Bitmap selectedImage = new Bitmap(openFileDialog.FileName);

                        if (selectedImage.Width != 32 || selectedImage.Height != 32)
                        {
                            ShowErrorMessage("The selected image must be 32x32 pixels.");
                        }
                        else
                        {
                            var saveLocation = location + @"\o_" + index.ToString() + ".png";
                            selectedImage.Save(saveLocation, System.Drawing.Imaging.ImageFormat.Png);

                            saveLocation = location + @"\" + index.ToString() + ".png";
                            TransparancyReplace(selectedImage).Save(saveLocation, System.Drawing.Imaging.ImageFormat.Png);
                            return selectedImage;
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Error loading the image: {ex.Message}");
                    }
                }
            }
            return null;
        }

        private string? LoadInSfxFile(int index, string location)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "RAW Files|*.raw;|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var saveLocation = location + @"\" + index.ToString() + ".raw";
                        var textToSave = File.ReadAllBytes(openFileDialog.FileName);
                        File.WriteAllBytes(saveLocation, textToSave);
                        var response = openFileDialog.FileName.Split(@"\").Last();
                        return response;
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage($"Error loading the RAW file: {ex.Message}");
                    }
                }
            }
            return null;
        }

        private void PictureBox_MouseDown(object? sender, MouseEventArgs e)
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

        private void PictureBox_MouseMove(object? sender, MouseEventArgs e)
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

        private void PictureBox_MouseUp(object? sender, MouseEventArgs e)
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
            if (index < 15 || index >= Level.SFG_TILE_DICTIONARY_SIZE)
            {
                return CELL_SIZE;
            }

            return ((index - 15) / (int)CellHeight.Maximum) + 3;
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
                // Still trying to work out if we should allow items to be put on walls, for now I think so
                // ShowErrorMessage("ELements can't be placed on walls");
                // return;
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

        private void MapColour_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TextureIndexPopup textureSelectorForm = new();
                textureSelectorForm.Show();
            }
        }

        private void MapColour_1_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                HandlWallSelectionClicked(button.BackColor);
            }
            _currentEditState = EditState.Walls;
            CellHeight.Enabled = true;
        }


        private void MapColour_door_Click(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                HandlWallSelectionClicked(button.BackColor);
            }
            ResetCellHeight();
            CellHeight.Enabled = false;
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
                    newColour = Color.FromArgb(255, 153, 153, 153);
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
                case 8 + 56:
                case 8 + 63:
                case 8 + 70:
                case 8 + 77:
                case 8 + 84:
                case 8 + 91:
                case 8 + 98:
                case 8 + 105:
                    newColour = Color.FromArgb(255, 255, 20, 147);
                    break;
                case 9:
                case 9 + 7:
                case 9 + 14:
                case 9 + 21:
                case 9 + 28:
                case 9 + 35:
                case 9 + 42:
                case 9 + 49:
                case 9 + 56:
                case 9 + 63:
                case 9 + 70:
                case 9 + 77:
                case 9 + 84:
                case 9 + 91:
                case 9 + 98:
                case 9 + 105:
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
                case 10 + 56:
                case 10 + 63:
                case 10 + 70:
                case 10 + 77:
                case 10 + 84:
                case 10 + 91:
                case 10 + 98:
                case 10 + 105:
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
                case 11 + 56:
                case 11 + 63:
                case 11 + 70:
                case 11 + 77:
                case 11 + 84:
                case 11 + 91:
                case 11 + 98:
                case 11 + 105:
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
                case 12 + 56:
                case 12 + 63:
                case 12 + 70:
                case 12 + 77:
                case 12 + 84:
                case 12 + 91:
                case 12 + 98:
                case 12 + 105:
                    newColour = Color.FromArgb(255, 128, 0, 0);
                    break;
                case 13:
                case 13 + 7:
                case 13 + 14:
                case 13 + 21:
                case 13 + 28:
                case 13 + 35:
                case 13 + 42:
                case 13 + 49:
                case 13 + 56:
                case 13 + 63:
                case 13 + 70:
                case 13 + 77:
                case 13 + 84:
                case 13 + 91:
                case 13 + 98:
                case 13 + 105:
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
                case 14 + 56:
                case 14 + 63:
                case 14 + 70:
                case 14 + 77:
                case 14 + 84:
                case 14 + 91:
                case 14 + 98:
                case 14 + 105:
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
            SelectedElement.Text = GetSelectedElementName();
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
            HandleElementClicked(Level.PLAYER_POSITION_TYPE_INDEX);
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
                        ShowErrorMessage($"You have exceeded the maximum amount of {AVAILABLE_ELEMENTS} available elements.");
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
                    if (index == -1) { return; }


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

            decimal value = numericUpDown.Value / DEGRESS_TO_BYTE_CONVERSION;

            if (value > byte.MaxValue)
            {
                value = byte.MaxValue;
            }

            _currentLevel.PlayerStart[2] = (byte)(value);
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

            if (index >= _currentLevel.MapArray.Length)
            {
                return;
            }

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
                    textColor = Color.FromArgb(255, 0, 200, 0);
                }

                if (_currentElementNumber == Level.PLAYER_POSITION_TYPE_INDEX)
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

                PointF textLocation = new PointF((element.Coords[0] * CELL_SIZE) + CELL_SIZE / 30, (element.Coords[1] * CELL_SIZE) + CELL_SIZE / 30);
                if (_currentElementNumber >= 0x20 && _currentElementNumber <= 0x26)
                {
                    g.DrawString("E" + (_currentElementNumber - ENEMIES_INDEX_OFFSET).ToString(), DefaultFont, textBrush, textLocation);
                }
                else if (_currentElementNumber == Level.PLAYER_POSITION_TYPE_INDEX)
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
                else if (_currentElementNumber == BLOCKER)
                {
                    g.DrawString("B", DefaultFont, textBrush, textLocation);
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
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Path.Combine(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack);

                saveFileDialog.Filter = "HAD Files|*.HAD|All Files|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Serialize the class to a binary file
                    using (FileStream fs = new(saveFileDialog.FileName, FileMode.Create))
                    {
                        _currentLevel.Serialise(new BinaryWriter(fs));
                    }
                }
            }
        }

        private void SavePreviewLevel()
        {
            using (FileStream fs = new(Path.Join(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack, "Level" + DEBUG_LEVEL_ID + ".HAD"), FileMode.Create))
            {
                _currentLevel.Serialise(new BinaryWriter(fs));
            }
        }

        private void LoadPreviewLevel()
        {
            string filePath = Path.Join(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack, "Level" + DEBUG_LEVEL_ID + ".HAD");
            if (!File.Exists(filePath)) { return; }

            using (FileStream fs = new(Path.Join(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack, "Level" + DEBUG_LEVEL_ID + ".HAD"), FileMode.Open))
            {
                if (fs != null)
                {
                    try
                    {
                        _currentLevel.Deserialise(new BinaryReader(fs));
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage(ex.Message);
                    }
                }
            }
            LoadLevelIntoUi();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Path.Combine(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack);
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

            LoadLevelIntoUi();
            SavePreviewLevel();
        }

        private void LoadLevelIntoUi()
        {
            using (Graphics g = Graphics.FromImage(_gridImage))
            {
                RedrawWholeMapArray(g);

                // Draw Elements
                var originalType = _currentElementNumber;

                // Draw player position
                _currentElementNumber = Level.PLAYER_POSITION_TYPE_INDEX;
                RecoredElementClicked(new GridPosition(_currentLevel.PlayerStart[0], _currentLevel.PlayerStart[1]));
                _currentElementNumber = originalType;
                RedrawAllElements(g);

                // Update GUI values
                PlayerRotationUpDown.Value = decimal.Ceiling(_currentLevel.PlayerStart[2] * DEGRESS_TO_BYTE_CONVERSION);
                FloorHeightUpDown.Value = _currentLevel.floorHeight;
                CeilingHeightUpDown.Value = _currentLevel.ceilHeight;
                CeilingColourUpDown.Value = _currentLevel.CeilingColor;
                FloorColourUpDown4.Value = _currentLevel.FloorColor;

                DrawGrid(g);
                pictureBox.Invalidate();

                TextureAllocationUpDown0.Value = _currentLevel.TextureIndices[0] + 1;
                TextureAllocationUpDown1.Value = _currentLevel.TextureIndices[1] + 1;
                TextureAllocationUpDown2.Value = _currentLevel.TextureIndices[2] + 1;
                TextureAllocationUpDown3.Value = _currentLevel.TextureIndices[3] + 1;
                TextureAllocationUpDown4.Value = _currentLevel.TextureIndices[4] + 1;
                TextureAllocationUpDown5.Value = _currentLevel.TextureIndices[5] + 1;
                TextureAllocationUpDown6.Value = _currentLevel.TextureIndices[6] + 1;
                stepHeightUpDown.Value = _currentLevel.stepSize;
                DoorHeightUpDown.Value = _currentLevel.doorLevitation;
            }
        }

        private string GetSelectedElementName()
        {
            if (_currentEditState == EditState.Elements)
            {

                if (_currentElementNumber >= 0x20 && _currentElementNumber <= 0x26)
                {
                    return "Enemy " + (_currentElementNumber - ENEMIES_INDEX_OFFSET).ToString();
                }

                switch (_currentElementNumber)
                {
                    case Level.PLAYER_POSITION_TYPE_INDEX:
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
                    case BLOCKER:
                        return "Blocker";
                }

                return "Item " + _currentElementNumber.ToString();
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



        private void GenerateTadFile(string location)
        {
            List<byte> dataArray = new List<byte>();

            string folderPath = location;

            if (Directory.Exists(folderPath))
            {
                string[] imageFiles = Directory.GetFiles(folderPath, "*.png").Where(x => !string.IsNullOrEmpty(x) && !x.Split("\\").LastOrDefault().StartsWith("o")).ToArray();
                imageFiles = imageFiles.OrderBy(GetNumericPart).ToArray();

                for (int i = 0; i < imageFiles.Length; i++)
                {
                    dataArray.AddRange(GetTextureArray(imageFiles[i]));
                }
            }

            using (FileStream fs = new(folderPath + @"\data.TAD", FileMode.Create))
            {
                var bw = new BinaryWriter(fs);
                bw.Write(dataArray.ToArray());
            }
        }

        private void GenerateSadFile(string location)
        {
            List<byte> dataArray = new List<byte>();

            string folderPath = location;

            if (Directory.Exists(folderPath))
            {
                string[] imageFiles = Directory.GetFiles(folderPath, "*.raw");
                imageFiles = imageFiles.OrderBy(GetNumericPart).ToArray();


                for (int i = 0; i < imageFiles.Length; i++)
                {
                    dataArray.AddRange(GetAudioArray(imageFiles[i]));
                }
            }

            using (FileStream fs = new(folderPath + @"\data.SAD", FileMode.Create))
            {
                var bw = new BinaryWriter(fs);
                bw.Write(dataArray.ToArray());
            }
        }

        private async Task RunTadFileGenAsync(string folder, Button x)
        {
            var originalText = x.Text;
            x.Text = "Processing...";
            x.Enabled = false;
            await Task.Run(() => GenerateTadFile(GAME_FILE_LOCATION + folder));
            x.Text = originalText;
            x.Enabled = true;
        }

        private async void GenerateWallTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(WALL_TEXTURE_FOLDER, x);
            }
        }

        private async void GenerateItemsTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(ITEMS_TEXTURE_FOLDER, x);
            }
        }

        private async void GenerateBackgroundTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(BACKGROUND_TEXTURE_FOLDER, x);
            }
        }


        private async void GenerateWeaponTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(WEAPONS_TEXTURE_FOLDER, x);
            }
        }

        private async void GenerateEffectsTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(EFFECTS_TEXTURE_FOLDER, x);
            }
        }


        private async void GenerateEnemeisTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(ENEMIES_TEXTURE_FOLDER, x);
            }
        }


        private void RunLevel_Click(object sender, EventArgs e)
        {
            bool preview = PreviewLevel.Checked;
            bool godMode = GodMode.Checked;
            bool fullsceen = FullScreenTextBox.Checked;
            string fileLocation = Path.Join(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack, @"\level" + DEBUG_LEVEL_ID + ".HAD");
            using (FileStream fs = new(fileLocation, FileMode.Create))
            {
                _currentLevel.Serialise(new BinaryWriter(fs));
            }

            Environment.CurrentDirectory = GAME_FILE_LOCATION;
            string args = "-d";

            if (godMode)
            {
                args += " -g";
            }
            if (!fullsceen)
            {
                args += " -w";
            }
            if (preview)
            {
                args += " -t";
            }

            args += " -p " + _levelPack;

            Process.Start(new ProcessStartInfo
            {
                FileName = $"{GAME_FILE_LOCATION}\\{EXE_NAME}.exe",
                WorkingDirectory = GAME_FILE_LOCATION,
                // w = windowed, d = debug
                Arguments = args
            });
        }

        private void BackgroundUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;

            _currentLevel.BackgroundImage = (byte)(value - 1);
        }

        private void TextureAllocationUpDown0_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[0] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private void TextureAllocationUpDown1_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[1] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private void TextureAllocationUpDown2_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[2] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private void TextureAllocationUpDown3_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[3] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private void TextureAllocationUpDown4_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[4] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private void TextureAllocationUpDown5_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[5] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private void TextureAllocationUpDown6_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            decimal value = numericUpDown.Value;
            _currentLevel.TextureIndices[6] = (byte)(value - 1);
            UpdateButtonTextures();
        }

        private async void SyncSfxWithGame_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                var originalText = x.Text;
                x.Text = "Processing...";
                x.Enabled = false;
                await Task.Run(() => GenerateSadFile(GAME_FILE_LOCATION + SFX_FOLDER));
                x.Text = originalText;
                x.Enabled = true;
            }
        }

        private async void GenerateTitleTextureFileAsync_Click(object sender, EventArgs e)
        {
            var x = sender as Button;
            if (x is not null)
            {
                await RunTadFileGenAsync(TITLE_TEXTURE_FOLDER, x);
            }
        }



        private void GenerateTextureArray_Click(object sender, EventArgs e)
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
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error running the Python script: {ex.Message}");
            }
        }


        private void PlaySfx_Click(object sender, EventArgs e)
        {
            // Specify the path to your .raw file
            string rawFilePath = GAME_FILE_LOCATION + SFX_FOLDER + @"\" + _selectedSfxForPreview.ToString() + ".raw";
            string wavFilePath = GAME_FILE_LOCATION + SFX_FOLDER + @"\preview.wav";

            try
            {
                ConvertRawToWav(rawFilePath, wavFilePath);

                // Load the .raw file and play it
                _soundPlayer.SoundLocation = wavFilePath;
                _soundPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing audio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void ConvertRawToWav(string rawFilePath, string wavFilePath)
        {
            int sampleRate = 8000;
            int bitsPerSample = 8;
            int channels = 1;
            try
            {
                // Calculate the total number of samples
                int numSamples = (int)new FileInfo(rawFilePath).Length / (bitsPerSample / 8);

                // Create a MemoryStream to hold the WAV file in memory
                using (MemoryStream wavStream = new MemoryStream())
                {
                    // Write WAV header
                    wavStream.Write(new byte[] { 82, 73, 70, 70 }, 0, 4); // "RIFF"
                    wavStream.Write(BitConverter.GetBytes(36 + numSamples), 0, 4); // File size
                    wavStream.Write(new byte[] { 87, 65, 86, 69 }, 0, 4); // "WAVE"
                    wavStream.Write(new byte[] { 102, 109, 116, 32 }, 0, 4); // "fmt "
                    wavStream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size
                    wavStream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat (1 for PCM)
                    wavStream.Write(BitConverter.GetBytes((short)channels), 0, 2); // NumChannels
                    wavStream.Write(BitConverter.GetBytes(sampleRate), 0, 4); // SampleRate
                    wavStream.Write(BitConverter.GetBytes(sampleRate * channels * bitsPerSample / 8), 0, 4); // ByteRate
                    wavStream.Write(BitConverter.GetBytes((short)(channels * bitsPerSample / 8)), 0, 2); // BlockAlign
                    wavStream.Write(BitConverter.GetBytes((short)bitsPerSample), 0, 2); // BitsPerSample
                    wavStream.Write(new byte[] { 100, 97, 116, 97 }, 0, 4); // "data"
                    wavStream.Write(BitConverter.GetBytes(numSamples), 0, 4); // Subchunk2Size

                    // Read the raw PCM data
                    byte[] rawBytes = File.ReadAllBytes(rawFilePath);

                    // Write the raw PCM data to the WAV file
                    wavStream.Write(rawBytes, 0, rawBytes.Length);

                    // Write the WAV data to the output file
                    File.WriteAllBytes(wavFilePath, wavStream.ToArray());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting to WAV: {ex.Message}");
            }
        }

        private void Preview3D_Click(object sender, EventArgs e)
        {
            string fileLocation = Path.Combine(GAME_FILE_LOCATION, LEVELS_FOLDER, _levelPack, @"\level" + DEBUG_LEVEL_ID + ".HAD");
            using (FileStream fs = new(fileLocation, FileMode.Create))
            {
                _currentLevel.Serialise(new BinaryWriter(fs));
            }

            Environment.CurrentDirectory = GAME_FILE_LOCATION;

            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Join(GAME_FILE_LOCATION, "NekoNeo.exe"),
                WorkingDirectory = GAME_FILE_LOCATION
            });
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {

        }

        private void DoorTextureUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            int value = (int)numericUpDown.Value;
            _currentLevel.doorLevitation = (byte)(value);
        }

        private void RefreshMap_Click(object sender, EventArgs e)
        {
            LoadPreviewLevel();
        }

        private void stepHeightUpDown_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;

            byte value = (byte)numericUpDown.Value;
            _currentLevel.stepSize = value;
        }

        private void Compile(bool run)
        {
            try
            {
                // Define the path to msbuild.exe if needed, otherwise rely on system PATH
                string msbuildPath = "msbuild";
                string solutionPath = $"{GAME_FILE_LOCATION}\\Neko.sln";
                string args = $"/p:Configuration=Release;Platform=x64";

                // Create a new process start info
                ProcessStartInfo processStartInfo = new();
                processStartInfo.FileName = msbuildPath;
                processStartInfo.Arguments = $"\"{solutionPath}\" {args}";
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.UseShellExecute = false;
                processStartInfo.CreateNoWindow = true;
                processStartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(solutionPath); // Set the working directory to the solution path

                // Copy current environment variables
                foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
                {
                    processStartInfo.EnvironmentVariables[env.Key.ToString()] = env.Value.ToString();
                }

                // Ensure the PATH environment variable is properly set
                string currentPath = Environment.GetEnvironmentVariable("Path");
                processStartInfo.EnvironmentVariables["Path"] = currentPath;

                // Start the process
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();

                    // Read the output (or error)
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    // Wait for the process to finish
                    process.WaitForExit();

                    // Display the output (for example, in a message box)
                    if (run)
                    {
                        RunGame();
                    }
                    else
                    {
                        MessageBox.Show(output, "MSBuild Output");
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            MessageBox.Show(error, "MSBuild Error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message} \n\nPlease ensure the path to your MSBuild.exe file is set in your environment variables", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void compile_Click(object sender, EventArgs e)
        {
            Compile(false);
        }

        private void CompileAndRun_Click(object sender, EventArgs e)
        {
            Compile(true);
        }

        private void SelectedElement_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

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