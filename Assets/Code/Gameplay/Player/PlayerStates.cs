using System;
using System.Collections;
using System.Collections.Generic;
using CurlyUtility.DSA;
using UnityEngine;

using PlayerContext = Ascendead.Player.PlayerController.PlayerContext;

namespace Ascendead.Player
{
    public class PlayerIdle : IState<PlayerContext>
    {

    }

    public class PlayerRun : IState<PlayerContext>
    {
        public void OnLogic(PlayerContext context)
        {
            Debug.Log("Moving player");
            Vector2 movementVector = context.MovementInput;
            float speed = context.Configuration.MovementSpeed;

            context.Rigidbody.velocity = new Vector2(movementVector.x * speed, context.Rigidbody.velocity.y);
        }
    }

    public class PlayerJump : IState<PlayerContext>
    {

    }

    public class PlayerFreefall : IState<PlayerContext>
    {

    }

    public class PlayerWallSlide : IState<PlayerContext>
    {

    }
}
