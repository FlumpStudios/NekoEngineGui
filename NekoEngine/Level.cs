using static NekoEngine.Tools;
namespace NekoEngine
{
    [Serializable]
    public class Elements
    {
        public Elements()
        {
            Coords = new byte[2];
        }
        public byte Type;
        public byte[] Coords;
    }

    [Serializable]
    public class Level
    {
        public const int MAP_DIMENSION = 64;
        public const byte PLAYER_POSITION_TYPE_INDEX = 99;
        public const int MAX_ELEMENT_SIZE = 128;
        public byte ceilHeight = 10;
        public byte floorHeight = 10;
        public byte[] HeightArray;

        public Level()
        {
            DoorTextureIndex = 1;
            FloorColor = 20;
            CeilingColor = 3;

            // 7 is transparent
            TextureIndices = new byte[7] { 5, 2, 3, 4, 5, 6, 8 };

            HeightArray = new byte[4096];
            MapArray = new byte[4096];

            PlayerStart = new byte[3] { 32, 32, 0 };
            elements = new Elements[MAX_ELEMENT_SIZE];
            BackgroundImage = 1;

            for (int i = 0; i < MAX_ELEMENT_SIZE; i++)
            {
                elements[i] = new Elements
                {
                    Coords = new byte[2],
                    Type = 0
                };
            }

            this.TileDictionary = new ushort[64];
            GenerateTileDictionary();
        }
        
        public byte[] MapArray;
        public UInt16[] TileDictionary;
        public byte[] TextureIndices;
        public byte DoorTextureIndex;
        public byte FloorColor { get; set; }
        public byte CeilingColor { get; set; }

        public byte[] PlayerStart;
        public byte BackgroundImage;
        public Elements[] elements;

        public static byte[] InvertArrayWidth(byte[] inputArray)
        {
            byte[] outputArray = new byte[inputArray.Length];

            for (int i = 0; i < inputArray.Length; i++)
            {
                int x = i % MAP_DIMENSION;
                int y = i / MAP_DIMENSION;

                // Reverse the order of columns
                int adjustedX = MAP_DIMENSION - 1 - x;

                // Calculate the new index in the output array
                int newIndex = adjustedX + y * MAP_DIMENSION;

                // Copy the value from the input array to the corresponding position in the output array
                outputArray[newIndex] = inputArray[i];
            }

            return outputArray;
        }

        public void Deserialise(BinaryReader br)
        {   
            for (int i = 0; i < 4096; i++)
            {
                this.MapArray[i] = br.ReadByte();
            }
            this.MapArray = InvertArrayWidth(this.MapArray);

            for (int i = 0; i < 64; i++)
            {
                this.TileDictionary[i] = br.ReadUInt16();
            }
                
            for (int i = 0; i < 7; i++)
            {
                this.TextureIndices[i] = br.ReadByte();
            }

            this.DoorTextureIndex = br.ReadByte();
            this.FloorColor = br.ReadByte();
            this.CeilingColor = br.ReadByte();

            

            this.PlayerStart[0] = (byte)(MAP_DIMENSION - 1 - br.ReadByte());
            this.PlayerStart[1] = br.ReadByte();
            this.PlayerStart[2] = br.ReadByte();


            this.BackgroundImage = br.ReadByte();

            for (int i = 0; i < MAX_ELEMENT_SIZE; i++)
            {
                this.elements[i].Type = br.ReadByte();                
                this.elements[i].Coords[0] = (byte)(MAP_DIMENSION - 1 - br.ReadByte());
                this.elements[i].Coords[1] = br.ReadByte();
            }

            this.ceilHeight = br.ReadByte();
            this.floorHeight = br.ReadByte();
        }

        public void Serialise(BinaryWriter bw)
        {
            GenerateTileDictionary();

            // Ensure player position isn't saved as an element
            foreach (var element in this.elements)
            {
                if (element.Type == PLAYER_POSITION_TYPE_INDEX)
                {
                    element.Type = 0;
                    element.Coords[0] = 0;
                    element.Coords[1] = 0;
                }
            }

            // Ensure that all elements with a type are at the beginning of the array so can be ignore game/engine side.
            this.elements = this.elements.OrderByDescending(x => x.Type).ToArray();

            var toWrite = InvertArrayWidth(this.MapArray);
            foreach (var maptItem in toWrite)
            {
                bw.Write(maptItem);
            }

            foreach (var tile in this.TileDictionary)
            {
                bw.Write(tile);
            }

            foreach (var textureIndex in this.TextureIndices)
            {
                bw.Write(textureIndex);
            }

            bw.Write(this.DoorTextureIndex);
            bw.Write(this.FloorColor);
            bw.Write(this.CeilingColor);


            bw.Write((byte)(MAP_DIMENSION - 1 - this.PlayerStart[0]));
            bw.Write(this.PlayerStart[1]);
            bw.Write(this.PlayerStart[2]); 
            bw.Write(this.BackgroundImage);


            foreach (var element in this.elements)
            {
                bw.Write(element.Type);
                bw.Write((byte)(MAP_DIMENSION - 1 - element.Coords[0]));
                bw.Write(element.Coords[1]);
            }

            bw.Write(ceilHeight);
            bw.Write(floorHeight);
        }

        internal Elements GetElementAtPosition(GridPosition position)
        {
            var response = new Elements();
            if (elements != null && position != null)
            { 
                response = elements.FirstOrDefault(x => x.Coords[0] == position.Column && x.Coords[1] == position.Row) ?? new Elements();
            }
            return response;
        }

        private void GenerateTileDictionary()
        { 
            this.TileDictionary = new ushort[64]
            {   
                // Open space
                SFG_TD(0,ceilHeight,0,0), // 0

                // doors 
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,0,0), // 1
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,1,1), // 2
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,2,2), // 3 
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,3,3), // 4
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,4,4), // 5
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,5,5), // 6
                SFG_TD(4,ceilHeight > 30 ? (ushort)31 : (ushort)0,6,6), // 7

                SFG_TD(floorHeight,ceilHeight,0,0), // 8
                SFG_TD(floorHeight,ceilHeight,1,1), // 9
                SFG_TD(floorHeight,ceilHeight,2,2), // 10
                SFG_TD(floorHeight,ceilHeight,3,3), // 11
                SFG_TD(floorHeight,ceilHeight,4,4), // 12
                SFG_TD(floorHeight,ceilHeight,5,5), // 13
                SFG_TD(floorHeight,ceilHeight,6,6), // 14
                
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,0,0), // 15
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,1,1), // 16
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,2,2), // 17
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,3,3), // 18
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,4,4), // 19
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,5,5), // 20
                SFG_TD(1,ceilHeight <  31 ? (ushort)(ceilHeight - 1) :  (ushort)31 ,6,6), // 21
                
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,0,0), // 22
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,1,1), // 23
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,2,2), // 24
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,3,3), // 25
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,4,4), // 26
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,5,5), // 27
                SFG_TD(2,ceilHeight <  31 ? (ushort)(ceilHeight - 2) :  (ushort)31 ,6,6), // 28

                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,0,0), // 29
                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,1,1), // 30
                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,2,2), // 31
                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,3,3), // 32
                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,4,4), // 33
                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,5,5), // 34
                SFG_TD(3,ceilHeight <  31 ? (ushort)(ceilHeight - 3) :  (ushort)31 ,6,6), // 35

                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,0,0), // 36
                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,1,1), // 37
                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,2,2), // 38
                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,3,3), // 39
                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,4,4), // 40
                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,5,5), // 41
                SFG_TD(4,ceilHeight <  31 ? (ushort)(ceilHeight - 4) :  (ushort)31 ,6,6), // 42

                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,0,0), // 43
                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,1,1), // 44
                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,2,2), // 45
                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,3,3), // 46
                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,4,4), // 47
                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,5,5), // 48
                SFG_TD(5,ceilHeight <  31 ? (ushort)(ceilHeight - 5) :  (ushort)31 ,6,6), // 49

                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,0,0), // 50
                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,1,1), // 51
                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,2,2), // 52 
                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,3,3), // 53
                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,4,4), // 54
                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,5,5), // 55
                SFG_TD(6,ceilHeight <  31 ? (ushort)(ceilHeight - 6) :  (ushort)31 ,6,6), // 56

                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,0,0), // 57
                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,1,1), // 58
                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,2,2), // 59
                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,3,3), // 60
                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,4,4), // 61
                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,5,5), // 62
                SFG_TD(7,ceilHeight <  31 ? (ushort)(ceilHeight - 7) :  (ushort)31 ,6,6), // 63
            };
        }
    }
}
