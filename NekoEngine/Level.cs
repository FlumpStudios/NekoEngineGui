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
     
        
        public byte ceilHeight = 10;
        public byte floorHeight = 10;
        public byte[] HeightArray;

        public Level()
        {
            DoorTextureIndex = 13;
            FloorColor = 20;
            CeilingColor = 3;

            // 7 is transparent
            TextureIndices = new byte[7] { 1, 2, 3, 4, 5, 6, 8 };

            HeightArray = new byte[4096];
            MapArray = new byte[4096];

            PlayerStart = new byte[3] { 32, 32, 0 };
            elements = new Elements[64];
            BackgroundImage = 1;

            for (int i = 0; i < 64; i++)
            {
                elements[i] = new Elements
                {
                    Coords = new byte[2],
                    Type = 0
                };
            }
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

        public void Deserialise(BinaryReader br)
        {   
            for (int i = 0; i < 4096; i++)
            {
                this.MapArray[i] = br.ReadByte();
            }

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

            for (int i = 0; i < 3; i++)
            {
                this.PlayerStart[i] = br.ReadByte();    
            }

            this.BackgroundImage = br.ReadByte();

            for (int i = 0; i < 64; i++)
            {
                this.elements[i].Type = br.ReadByte();
                for (int j = 0; j < 2; j++)
                {
                    this.elements[i].Coords[j] = br.ReadByte();
                }
            }

            this.ceilHeight = br.ReadByte();
            this.floorHeight = br.ReadByte();
        }

        public void Serialise(BinaryWriter bw)
        {
            GenerateTileDictionary();

            foreach (var maptItem in this.MapArray)
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

            foreach (var playerStart in this.PlayerStart)
            {
                bw.Write(playerStart);
            }

            bw.Write(this.BackgroundImage);


            foreach (var element in this.elements)
            {
                bw.Write(element.Type);
                foreach (var item2 in element.Coords)
                {
                    bw.Write(item2);
                }
            }

            bw.Write(ceilHeight);
            bw.Write(floorHeight);
        }

        private void GenerateTileDictionary()
        { 
            this.TileDictionary = new ushort[64]
            {   
                // Open space
                SFG_TD(0,ceilHeight,0,0), // 0

                // doors 
                SFG_TD(4,0,0,0), // 1
                SFG_TD(4,0,1,1), // 2
                SFG_TD(4,0,2,2), // 3 
                SFG_TD(4,0,3,3), // 4
                SFG_TD(4,0,4,4), // 5
                SFG_TD(4,0,5,5), // 6
                SFG_TD(4,0,6,6), // 7

                SFG_TD(floorHeight,ceilHeight,0,0), // 8
                SFG_TD(floorHeight,ceilHeight,1,1), // 9
                SFG_TD(floorHeight,ceilHeight,2,2), // 10
                SFG_TD(floorHeight,ceilHeight,3,3), // 11
                SFG_TD(floorHeight,ceilHeight,4,4), // 12
                SFG_TD(floorHeight,ceilHeight,5,5), // 13
                SFG_TD(floorHeight,ceilHeight,6,6), // 14
                
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),0,0), // 15
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),1,1), // 16
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),2,2), // 17
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),3,3), // 18
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),4,4), // 19
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),5,5), // 20
                SFG_TD((ushort)(1),(ushort)(ceilHeight - 1),6,6), // 21
                
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),0,0), // 22
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),1,1), // 23
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),2,2), // 24
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),3,3), // 25
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),4,4), // 26
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),5,5), // 27
                SFG_TD((ushort)(2),(ushort)(ceilHeight - 2),6,6), // 28

                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),0,0), // 29
                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),1,1), // 30
                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),2,2), // 31
                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),3,3), // 32
                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),4,4), // 33
                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),5,5), // 34
                SFG_TD((ushort)(3),(ushort)(ceilHeight - 3),6,6), // 35

                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),0,0), // 36
                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),1,1), // 37
                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),2,2), // 38
                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),3,3), // 39
                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),4,4), // 40
                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),5,5), // 41
                SFG_TD((ushort)(4),(ushort)(ceilHeight - 4),6,6), // 42

                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),0,0), // 43
                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),1,1), // 44
                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),2,2), // 45
                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),3,3), // 46
                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),4,4), // 47
                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),5,5), // 48
                SFG_TD((ushort)(5),(ushort)(ceilHeight - 5),6,6), // 49

                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),0,0), // 50
                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),1,1), // 51
                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),2,2), // 52
                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),3,3), // 53
                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),4,4), // 54
                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),5,5), // 55
                SFG_TD((ushort)(6),(ushort)(ceilHeight - 6),6,6), // 56

                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),0,0), // 57
                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),1,1), // 58
                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),2,2), // 59
                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),3,3), // 60
                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),4,4), // 61
                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),5,5), // 62
                SFG_TD((ushort)(7),(ushort)(ceilHeight - 7),6,6)  // 63
            };
        }
    }
}
