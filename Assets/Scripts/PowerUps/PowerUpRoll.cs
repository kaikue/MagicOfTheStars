using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpRoll : PowerUp
{
    
    public override void Activate(Player player)
    {
        player.SetCanRoll();
        base.Activate(player);
    }
    
}
