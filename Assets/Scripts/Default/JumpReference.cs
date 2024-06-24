using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using static UnityEngine.EventSystems.PointerEventData;

public class JumpReference : MonoBehaviour
{

    // Fusion SampleProject _ Razor _ JumpSystem
    #region Jump
    /*
    private void Jump(NetworkButtons pressedButtons)
    {

        //Jump
        if (pressedButtons.IsSet(InputButton.JUMP) || CalculateJumpBuffer())
        {
            if (_behaviour.InputsAllowed)
            {
                if (!IsGrounded && pressedButtons.IsSet(InputButton.JUMP))
                {
                    _jumpBufferTime = Runner.SimulationTime;
                }

                if (IsGrounded || CalculateCoyoteTime())
                {
                    _rb.Rigidbody.velocity *= Vector2.right; //Reset y Velocity
                    _rb.Rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                    CoyoteTimeCD = true;
                    if (Runner.IsForward && Object.HasInputAuthority)
                    {
                        RPC_PlayJumpEffects((Vector2)transform.position - Vector2.up * .5f);
                    }
                }
                else if (_wallSliding)
                {
                    _rb.Rigidbody.velocity *= Vector2.zero; //Reset y and x Velocity
                    _rb.Rigidbody.AddForce((Vector2.up + (_wallSlidingNormal)) * _jumpForce, ForceMode2D.Impulse);
                    CoyoteTimeCD = true;
                    if (Runner.IsForward && Object.HasInputAuthority)
                    {
                        RPC_PlayJumpEffects((Vector2)transform.position - _wallSlidingNormal * .5f);
                    }
                }
            }
        }
    }

    private bool CalculateJumpBuffer()
    {
        return (Runner.SimulationTime <= _jumpBufferTime + _jumpBufferThreshold) && IsGrounded;
    }

    private bool CalculateCoyoteTime()
    {
        return (Runner.SimulationTime <= TimeLeftGrounded + CoyoteTimeThreshold);
    }

    [Rpc(sources: RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_PlayJumpEffects(Vector2 particlePos)
    {
        PlayJumpSound();
        PlayJumpParticle(particlePos);
    }

    private void PlayJumpSound()
    {
        _sfxChannel.CallSoundEvent(_jumpSound, Object.HasInputAuthority ? null : _playerSource);
    }

    private void PlayJumpParticle(Vector2 pos)
    {
        _particleManager.Get(ParticleManager.ParticleID.Jump).transform.position = pos;
    }

    /// <summary>
    /// Increases gravity force on the player based on input and current fall progress.
    /// </summary>
    /// <param name="input"></param>
    private void BetterJumpLogic(InputData input)
    {
        if (IsGrounded) { return; }
        if (_rb.Rigidbody.velocity.y < 0)
        {
            if (_wallSliding && input.AxisPressed())
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (wallSlidingMultiplier - 1) * Runner.DeltaTime;
            }
            else
            {
                _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Runner.DeltaTime;
            }
        }
        else if (_rb.Rigidbody.velocity.y > 0 && !input.GetButton(InputButton.JUMP))
        {
            _rb.Rigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Runner.DeltaTime;
        }
    }

    */
    #endregion
}
