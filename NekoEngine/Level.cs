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
        // These are not currently to be serailised
        public byte playerRotation = 0;
        public byte ceilHeight = 10;
        public byte floorHeight = 10;

        public Level()
        {
            DoorTextureIndex = 13;
            FloorColor = 20;
            CeilingColor = 3;

            // 7 is transparent
            TextureIndices = new byte[7] { 1, 2, 3, 4, 5, 6, 8 };
            
            MapArray = new byte[64];

            PlayerStart = new byte[3] { 32, 32, playerRotation };
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

        public void Serialise(BinaryWriter bw)
        {
            GenerateTileDictionary();
            foreach (var item in this.MapArray)
            {
                bw.Write(item);
            }

            foreach (var item in this.TileDictionary)
            {
                bw.Write(item);
            }

            foreach (var item in this.TextureIndices)
            {
                bw.Write(item);
            }

            bw.Write(this.DoorTextureIndex);
            bw.Write(this.FloorColor);
            bw.Write(this.CeilingColor);

            foreach (var item in this.PlayerStart)
            {
                bw.Write(item);
            }

            bw.Write(this.BackgroundImage);

            foreach (var item in this.elements)
            {
                bw.Write(item.Type);
                foreach (var item2 in item.Coords)
                {
                    bw.Write(item2);
                }
            }
        }

        private void GenerateTileDictionary()
        { 
            this.TileDictionary = new ushort[64]
            {
                 SFG_TD(0,ceilHeight,0,0),

                 SFG_TD(floorHeight,ceilHeight,0,0),// 1
                 SFG_TD(floorHeight,ceilHeight,1,1),// 2
                 SFG_TD(floorHeight,ceilHeight,2,2),// 3
                 SFG_TD(floorHeight,ceilHeight,3,3),// 4
                 SFG_TD(floorHeight,ceilHeight,4,4),// 5
                 SFG_TD(floorHeight,ceilHeight,5,5),// 6
                 SFG_TD(floorHeight,ceilHeight,6,6),// 7

    
                  // Outside
                  SFG_TD(floorHeight,31,0,0), // 8

                  // door 
                  SFG_TD(4,0,0,0),
                  SFG_TD(4,0,1,1),
                  SFG_TD(4,0,2,2),
                  SFG_TD(4,0,3,3),
                  SFG_TD(4,0,4,4),
                  SFG_TD(4,0,5,5),
                  SFG_TD(4,0,6,6),
                  
                    // Currently unused
                  SFG_TD(11, 7,2,0),
                  SFG_TD(15, 0,4,3),SFG_TD(13, 2,2,2),SFG_TD( 0, 7,5,0),SFG_TD(11, 5,2,2),
                  SFG_TD(11,31,2,0),SFG_TD(11, 7,2,7),SFG_TD(18, 0,2,7),SFG_TD( 7, 0,0,0),
                  SFG_TD(11,15,5,0),SFG_TD(30,31,2,0),SFG_TD(12,31,6,0),SFG_TD( 7, 0,3,3),
                  SFG_TD(26, 4,0,0),SFG_TD(31, 0,2,0),SFG_TD(26, 5,0,0),SFG_TD(10,31,6,0),
                  SFG_TD(11, 0,1,0),SFG_TD( 7, 4,1,0),SFG_TD(10,31,2,0),SFG_TD(14,31,4,0),
                  SFG_TD(14,31,2,0),SFG_TD( 3,23,5,0),SFG_TD( 3, 4,1,0),SFG_TD(18,31,4,0),
                  SFG_TD( 8,31,2,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),
                  SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),
                  SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),
                  SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),
                  SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),
                  SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0),SFG_TD( 7,11,0,0)
            };
        }
    }
}
