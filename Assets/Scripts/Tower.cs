using System;
using UnityEngine;
namespace cs
{
    class Tower : Piece
    {
        static int arrowLength = 7;
        public override string getName() { return "塔"; }
        public override bool canAtk() { return false; }
        public override bool ismovable() { return false; }
        public override string getDefLevel() { return "机械"; }
        public override string getPrompt() { return "U-箭雨 I-狙击"; }
        public override int value() { return 2; }
        public override void walk(int x, int y) { }
        public override void calSkill(int selx, int sely)
        {
            for (int i = Math.Max(0, selx - arrowLength); i <= Math.Min(14, selx + arrowLength); ++i)
            {
                int jlen = arrowLength - Math.Abs(selx - i);
                for (int j = Math.Max(0, sely - jlen); j <= Math.Min(14, sely + jlen); ++j)
                {
                    if (i == selx && j == sely) continue;
                    if (selectedSkill == "I" && Plate.plate[i][j] != null) continue;
                    Plate.plateCol[i][j] = Color.gray;
                }
            }
        }
        public override bool releaseSkill(int srcx, int srcy, int dstx, int dsty)
        {
            if ((selectedSkill == "U" && Plate.plateCol[dstx][dsty] == Color.yellow)
                    || Plate.plateCol[dstx][dsty] == Color.gray)
            {
                if (wait == 2)
                {
                    Plate.plate[waitx][waity] = null;
                }
                waitx = dstx;
                waity = dsty;
                wait = 1;
                if (selectedSkill == "I") Plate.plate[waitx][waity] = new ArrowSnipeToken(srcx, srcy, player);
                if (selectedSkill == "U")
                {
                    PlayerControl.drawToken(waitx, waity, 0);
                    int diffx = Math.Abs(waitx - srcx);
                    int diffy = Math.Abs(waity - srcy);
                    if (diffx > diffy)
                    {
                        PlayerControl.drawToken(waitx, waity - 1, 1);
                        PlayerControl.drawToken(waitx, waity + 1, 2);
                    }
                    else if (diffx < diffy)
                    {
                        PlayerControl.drawToken(waitx - 1, waity, 1);
                        PlayerControl.drawToken(waitx + 1, waity, 2);
                    }
                    else
                    {
                        PlayerControl.drawToken(waitx, waity - Math.Sign(waity - srcy), 1);
                        PlayerControl.drawToken(waitx - Math.Sign(waitx - srcx), waity, 2);
                    }
                    wait = 0;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void turnTurn(int srcx, int srcy)
        {
            if (wait == 1) wait = 2;
            else if (wait == 2)
            {
                if (selectedSkill == "I")
                {
                    if (Plate.plate[waitx][waity] == null)
                    {
                        Plate.plate[waitx][waity] = new ArrowSnipeToken(waitx, waity, player);
                    }
                }
                if (Plate.canStrike(player, waitx, waity, 0, 0, srcx, srcy, true))
                {
                    Plate.plate[waitx][waity] = null;
                    wait = 0;
                }
                if (selectedSkill == "U")
                {
                    //箭雨和狙击技能共用一个变量。
                    //狙击技能只能放在空地，但是会放置一个token以阻止对方的前行。TODO：这个token有特殊之处，比如可以狙杀龙。
                    //箭雨技能则多两个格子。

                    int diffx = Math.Abs(waitx - srcx);
                    int diffy = Math.Abs(waity - srcy);
                    if (diffx > diffy)
                    {
                        if (Plate.canStrike(player, waitx, waity - 1, 0, 0, srcx, srcy, true)) Plate.plate[waitx][waity - 1] = null;
                        if (Plate.canStrike(player, waitx, waity + 1, 0, 0, srcx, srcy, true)) Plate.plate[waitx][waity + 1] = null;
                    }
                    else if (diffx < diffy)
                    {
                        if (Plate.canStrike(player, waitx - 1, waity, 0, 0, srcx, srcy, true)) Plate.plate[waitx - 1][waity] = null;
                        if (Plate.canStrike(player, waitx + 1, waity, 0, 0, srcx, srcy, true)) Plate.plate[waitx + 1][waity] = null;
                    }
                    else
                    {
                        if (Plate.canStrike(player, waitx, waity - Math.Sign(waity - srcy), 0, 0, srcx, srcy, true)) Plate.plate[waitx][waity - Math.Sign(waity - srcy)] = null;
                        if (Plate.canStrike(player, waitx - Math.Sign(waitx - srcx), waity, 0, 0, srcx, srcy, true)) Plate.plate[waitx - Math.Sign(waitx - srcx)][waity] = null;
                    }
                    wait = 0;
                    PlayerControl.delToken();
                }
            }
        }
    }
}

