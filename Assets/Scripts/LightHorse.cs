using System;

namespace cs
{
    class LightHorse : Piece
    {
        public override string getName(){return "轻";}
        public override string getDefLevel(){return "轻甲";}
        public override int value(){return 12;}
        public override void walk(int x, int y){ //计算棋子移动范围 
            Plate.walk(x,y,3,3,x,y);
        }
    }
}
