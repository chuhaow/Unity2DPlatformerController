# Unity2DPlatformerController
A platformer controller with some extra mechanics
## Mechanics
### Corner correction
- If the player bops their head near the edge of the character when jumping, rather than being stopped they are push slighty out of the way. The zone on the character where this occurs can be modified to your liking
### Coyote time
- A some amount of time where the player is still considered ground after they walk off a platform. This allows more wiggly for errors giving the player a feel of better control. The time can be modified.
### Jump length Control
- Holding down the jump button allows the player to jump higher and gives more air time. A light tap will result in a shorter jumps
### Wall slide and Wall Jump
- If the player is in the air and moving towards a wall they will being to slide down slowly. Pressing the jump button while in this state causes the player to jump.
### Grapple
- If the player is near a grapple point, they are able to launch themselves towards it
