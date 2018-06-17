using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpJump : PowerUp
{
    
    public override void Activate(Player player)
    {
        player.SetCanMidairJump();
        base.Activate(player);
    }
    
}
