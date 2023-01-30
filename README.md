# Fathomless Voidling

A Voidling rework mod that makes the fight more fun. Stats are reduced by default but can be increased in either the in-game config or the R2Modman config (for those who want to tweak things even more). Makes the Void Locus an alternate moon by adding a stage 5 portal, removing void fog from void pillars, and adds item cauldrons (Can toggle the alt moon/void fog).

Phase info:

Phase 1:

- Primary: Disillusion
  - Fires waves of void missiles and summons 3 large predictive death bombs
- Secondary: Vacuum (Vanilla)
  - Creates a black hole underneath itself
- Utility: Transpose
  - Slips deeper into the void, reappears near the target
- Special: Rend
  - Fires an inaccurate barrage of lasers

Phase 2:

- Primary: Disill??usion
  - Fires waves of void missiles and summons 3 faster predictive death bombs
- Secondary: Singularity
  - Creates a black hole at the center of the donut
- Utility: Transpose
  - Slips deeper into the void, reappears near the target
- Special: SpinBeam (Vanilla)
  - Fires a giant laser and spins in a circle

Phase 3:

- Primary: ?Disill??usio?n
  - Fires waves of void missiles and summons 3 tracking death bombs
- Secondary: Crush
  - Pulls meteors from the surrounding area and launches them at the player
- Utility: Transpose
  - Slips deeper into the void, reappears near the target
- Special: Reap
  - ???

## Plans

- Finishing P3 Laser Special
- Making Locus a Moon alternative (portal on stage 5, adding cauldrons, removing pillar void fog)

## Changelog

**0.8.6**

- Fixed Voidling not using its special ability in all phases
- AI Changes

**0.8.5**

- Fixed Phase 2 and 3 having extra HP
- Set up in-game configs (Toggles, Stats, CDs) (RiskOfOptions support)
- Removed some wonky AI changes
- Added singularity (center vacuum)
- README edit

**0.8.0**

- Rewrote a lot of the code (fixed console spam, makes sure things work properly, better organized, etc...)
- R2API updates
- Slightly buffs HP (1100 -> 1250 base | 325 -> 350 level)
- Slows down meteor speed (150 -> 100)
- Updates Disillusion to change death bombs each phase and actually be predictive
- Moves Crush to Phase 2/3
- Adds SpinBeam to Phase 2/3
- Adds more missile waves to Disillusion
- README Updates

**0.6.2**

- Fixes buffed HP value

**0.6.2**

- Fixes blink to stay near donut
- Fixes Interrupt States so blink doesn't interrupt skills mid-fire
- Increased CD for Transpose (20 sec total)
- Increased CD for Crush (40 sec total)
- Decreased CD for Desolate (30 sec total)
- Reduced Desolate duration (4 sec total)
- Increased Crush duration (6 sec total)
- Adds a charge state to Crush (more obvious when it's going to activate)
- Adds Captain abilities to the Planetarium (not really an alt boss if u can't use half ur kit)

**0.6.1**

- Reduces Crush AoE by 25%
- Reduces Crush damage
- Reduces Desolate laser damage by 50%
- Reduces Desolate pool damage by 75%
- Prevents Voidling from Blinking out of bounds and dying
- Adds a Deep Void portal to soup island so you can choose (to be polished)

**0.6.0**

- Phase 1 Kit and Stats completed
