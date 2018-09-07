# Magic of the Stars
Exploration-based collectathon platformer

## Credits
Developed by [Kai Kuehner](http://kai.bvk.nu).

### Art
Some placeholder graphics by [Kenney](http://kenney.nl/) and [Zuhria Alfitra](https://www.gameart2d.com/).

### Font
[Poetsen One by Rodrigo Fuenzalida](https://www.dafont.com/poetsen-one.font).

### Sound
- UI sounds by broumbroun
	- https://freesound.org/people/broumbroum/sounds/50565/
	- https://freesound.org/people/broumbroum/sounds/50561/
	- https://freesound.org/people/broumbroum/sounds/50557/
- Star twinkle modified from sound by MrCisum
	- https://freesound.org/people/MrCisum/sounds/336664/
- Star collect sound by elmasmalo1
	- https://freesound.org/people/elmasmalo1/sounds/350841/
- Pause sound by legoluft
	- https://opengameart.org/content/atmospheric-interaction-sound-pack

## TODO

### Movement
- Sometimes you fall slowly (related to holding or new sprites)
- Sprite is wrong for a moment when holding towards wall and walljumping off it
- Vanishing platforms (when touched, shake, crumble, respawn) (shy expressions)
- Physics platforms that squish down when you land on them then slowly sink into lava
- Holding crates
	- Throw from holdspot, coroutine to moveposition from holdspot to drop spot when dropping (long raycast to ensure valid along drop path)
	- edgecolliders? to prevent catching on tiles
	- forward grabbable collisions to player, then test collisions with wall above head, ceiling
	- 3d sfx on hit with loudness (cut off below min)
- Hanging on corners? Kind of unnecessary with rolling over corners
	- If:
		- sliding down ground tile
		- free space above that tile
		- at a certain position to that tile (halfway down?)
	- Stop movement, go into hanging animstate
	- If enough space above tile and jump pressed: hop up
		- Space test: raycast from player's side a bit above tile towards the center, at height where player would be hanging
	- If down/(L/R away from wall) pressed: release and fall normally
	- Test with one-tall gaps, two-tall, etc.
- Should tiles be full tile rect or sized to image? (setting in tile asset)
- Try unrolling and moving the player down a bit (since rolling up into a corner will engage anti-softlock and bounce the player instead of stopping them)
- Adjustable gravity direction
	- Might need to mess with IsWall() etc. dot checks 
- Slopes?
	- Sliding down steep angles, walljumping from really steep ones
	- Hitsnapping
		- note issues when rolling off of platform and immediately under ledge
- Moving platform- use most recent last platform to have player standing mostly on it (currently sticks on corner of player)
- Animator
	- Don't use Resources.Load
	- Use skeleton animation instead of individual frames
	- Get rid of NUM\_whatever\_FRAMES
- Jump speed- 14 or 16 (3 or 4 blocks)?
	- Test with better player proportions/size
	- Tweak walljump push
- Adjust jump & walljump grace period based on playtesting
- Slide particles and sound?
- Sideways one-way platforms?
- Apply moving wall velocity?
- Powerups
	- Particles
	- Glowy effect for can midair jump
- Visual indication for SetCanRoll (like mario galaxy spin recharge)
	- star glowing out?
- Can't jump and slide up rotating platform
- More moves?
	- Roll + jump = long jump?
	- Roll cancel jump in midair?
	- Speed boost for timing jump/roll somehow?

### Health
- 5 points of health- show sometimes or always?
	- sometimes for >2 health, always for <=2?
	- always if below max?
	- If you die you restart from the last checkpoint (reload scene)
- Invincibility PlayerPref
	- infinite pits or getting squished- respawn without losing health
	- ignore enemy collisions that would hurt?
- Obstacles
	- Spikes
	- Thwomps (spiky)
	- Collapsing blocks
	- Rotating blocks
	- Appearing blocks
	- Rollthroughable blocks
	- Bounce on things while rolling (spin jump like)
- Enemies
	- Damage by rolling
		- you bounce back out of roll
		- roll again while bouncing back (timed) to get extra speed & damage on roll
		- up to 3rd time combo (2 increasing rolls)
	- Should make the game FUN
	- Slimes- have to roll under ledge and squish one to teach mechanic
	- Can also jump on enemies to damage them
	- Spiky shell enemies- bounce off them (or along top) while rolling
- Bosses
	- Super Stars (gives star x3, different overlay text, image, music)
	- Don't spawn boss if superstar was collected
	- Slime King
	- Hatter
		- purple ghost magician
		- bomb from hat attack
		- spinning hats attack
	- Grob
		- tunnels through walls
		- grows legs for last phase
	- Magmarock
		- lava dragon

### Collectables
- Stars
	- Different color for each level
	- Unlock doors to new areas & levels
	- Star chips- collect 5 to make star
- Coins
	- Shops?
	- Throughout level
	- Drop randomly from enemies
- Health
	- Full recharge points in levels (checkpoints)
	- Single heart refills dropped randomly from enemies 
- Color schemes
- Special collectibles for an NPC
- Suits
	- New abilities
	- Swim
	- Moon gravity?
	- Ability to place limited number of jump/roll powerups?
	- Floaty fall
- Achievements?????
- 5 rare "blue pipe"-like things that go to secret areas, something special when you find all 5 

### Level design
- Player should visit doors before being able to open them
- Use scenery, zoomouts to draw attention to offscreen stuff
- Tutorial: jump, roll under ledge, roll through breakable wall, walljump, jump & roll in midair through breakable high wall
- Boss doors require a bunch of that level's color
- Secret final door requires all 11 of all colors
- Map of level in dead ends
- Show stuff the player can't get to yet
- Mix of exploration, puzzles, combat
- Introduce main mechanic safely, then iterate & complicate
- Sneak peeks of other levels
- Invisible secrets (hints on map & elsewhere)
- Mini-speedruns
- Speed paths
	- Camera-based cycles
- Puzzles
- Fake walls (indication of fakeness- different color?)
- P-switch like things that make permanent changes to sections of levels
- Levels tell a story
	- Story should be reflected in level mechanics
	- Conspiracy- detective sends you to "pick up clues", he was the one who did the crime and wants you to collect evidence
	- Tournament
	- Race- one in swimming level, one in land level
	- Exploring ancient deep ruins
	- Climbing a mountain
	- Digging to the core
	- Fixing the village
- Levels:
	- left: Slime forest
		- Slime pads bounce you
		- Reverse direction of roll
		- Slime conserves vertial momentum (if >= max) or add to it (if below max)
		- Auto walljump corridor
	- up: Crystal mountain
		- Climbers
		- Something at the top
	- down: Rock
		- Tumbler
		- Get on top of the elevator puzzle
		- Spinny platforms (gears?) (rotating, revolving)
		- Mining operation
		- Foreman- angry
		- Mining bots- scared
		- Bug enemies
		- Grob
	- right: Charred forest
		- Roll over coals (low ceiling)
	- other levels:
		- Lake
			- Surfer dude fish (Flotsam, Jetsam)
			- Streams of water that push you faster
		- Cloud kingdom
			- Princess- authoritarian
			- Bird pirates
			- Have to resolve war
		- Haunted Mansion
			- Draculas
			- Appearing platforms
		- Temple of the Stars
			- Quiet, echoes, humming
			- Symmetry
			- Bells
			- Near end game
		- Beehive
		- Lava streams
		- Gravity direction
		- The Tower
- Level bits:
	- Multiple columns of ascending/descending platforms
	- Star bits at corners of area
	- Water jetstreams
	- Platforms revolving around center (windmill style)
	- Spin entire big area
	- Little islands over water
	- Air roll course with PowerUpRoll
	- Roll after fall
	- Use roll powerups to do multiple walljumps
	- Reuse roll/jump powerup after it resets while you're still in midair
	- Towers over water
	- Climb small mountain island
	- Crushers
	- Flying islands
	- Diagonal moving platforms
	- Moving blocks push you into spikes
	- Chase someone
	- Stuff with one-way platforms (feel less stable)
	- Spring off vertical moving platform
	- Throw crate on vanishing platform so it doesn't go away when you step on it
	- Jumping between conveyors in different directions
	- Jumping between multiple spinning platforms
	- Giant hourglass that slows down time when you roll (or reverses it or something?)

### UI
- logo animation scene (press any key to skip)
- Don't play select sound on automatic start selection (check if close to menu's creation?)
- TextMeshPro
	- Outline on star # GUI (white text, dark outline)
- Add back HUD sliding in/out
	- Also show star count when paused
	- Use quadratic slope for in/out movement?
- Star collect overlay
	- Banner/star image color matches star color (or banner contrasts and matches level color?)
	- Fancy transitions for banner and overlay

### Camera
- Use Cinemachine for this stuff- https://blogs.unity3d.com/2018/07/17/cinemachine-for-2d-tips-and-tricks/
- Follow player- part of prefab
	- room to move back and forth a bit
	- lead the player's movement?
- 2/3 down normally? or snapped to offset from ground position
- Trigger areas
	- Lerp to fixed or relative positions
	- Zoom out
	- At top of pits, near interesting thing
- should AudioListener go on camera or player? consider when camera/player is near twinkling star
- Zoom in to star when approaching (then zoom out after collected)

### Misc
- Doors
	- why are they there? were they always there? why do colored stars unlock them?
- Gamepad/keyboard controls text
- Freeze for a bit on hit then flicker after respawning
- Checkpoint notifications
- Scene transitions (to point in scene)
	- Shrinking cutout star animation
- Portals
- Slopes
	- slide down >= 45 degree slopes, can't move/jump/roll
		- not if already rolling?
- Water
	- Decreases gravity
	- Decreases move speed?
	- Allows offground jumps after swim animation is complete? This is just mario...
		- plays full swim animation then goes to swim-stand
	- Allows full two-axis movement?
- Achievements?
	- beat game on 200% speed
- Pacifist ending?
	- no bosses?

### Sound
- Delete old sound files
- Multiple jump sounds
- Wallslide down
- Roll (whoosh)
- collect star again (bloop)
- collect powerup (ding or up-whoosh)
- Landing from high fall
- Door sliding open
- Door ascending tones
- Level music (mute during star overlay)
- Music fades in and out smoothly
- UI sounds
	- better unpause sound

### Art
- Vector art
- Player animations
- Star (juicier shading)
- Star collect image
- Door
- Menus
	- Main menu background
	- Options menu background
	- Pause/confirm/loading overlay banner
	- Buttons, sliders, checkbox
- Levels
	- Tiles- multiple sets per level
		- Tilemap collider vertical offset to not float on tiles
	- Scenery
	- Backgrounds
- Enemies
- Obstacles

- Default sprite facing is left (if not, remove ! in facing calculation and flip grabzone logic)
- Star should be white, colored by material
- When changing player dimensions, update bounding box and fully retest level design
- Player can jump up 3 blocks, across 7 blocks without rolling