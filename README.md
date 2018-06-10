# Magic Of The Stars
Exploration and puzzle-based platformer

## TODO

- Resources folder bad???

### Movement
- Climb up ground tiles if player is more than halfway up tile and has free space above it? 
- Take velocity of moving platforms (grounds and walls?)
- One way platforms (platformeffector2d)
- Hitsnapping
	- note issues when rolling off of platform and immediately under ledge
- Redo player sounds
	- Delete old sounds
- Slopes
	- Sliding down steep angles
- Slime that bounces you (auto-walljump corridor, bouncy hallway)
- Try unrolling and moving the player down a bit (since rolling up into a corner will engage anti-softlock and bounce the player instead of stopping them)
- Animator 
- Adjust jump & walljump grace period
- Slide particles?
- Powerups
	- Reset jump (double+ jump)
	- Particles
- Visual indication for CanRoll
- Make roll longer?
	- Depends on how long you hold the trigger- is there a limit?
- More moves?
	- Roll + jump = long jump?
	- Roll cancel jump in midair?

### Health
- 5 points of health- show sometimes or always?
	- sometimes for >2 health, always for <=2?
	- If you die you restart from the last checkpoint (reload scene)
- Obstacles
	- Spikes
	- Thwomps (spiky)
	- Collapsing blocks
	- Rotating blocks
	- Appearing blocks
	- Rollthroughable blocks
	- Bounce on things while rolling (spin jump like)
- Enemies
	- Damage by rolling, you bounce back out of roll
	- Should make the game FUN
	- Slimes- have to roll under ledge and squish one to teach mechanic
- Bosses
	- Super Stars (gives star x3, different overlay text, image, music)
	- Don't spawn boss if superstar was collected
	- Slime King
	- Hatter
	- Grob

### Collectables
- Stars
	- Different color for each level
	- Unlock doors to new areas & levels
	- Star chips- collect 5 to make star
- Coins
	- Shops?
- Health
	- In levels, drops from enemies? 
- Color schemes
- Suits
	- New abilities
	- Swim
	- Moon gravity?
	- Double jump?

### Level design
- Player should visit doors before being able to open them
- Use scenery to draw attention to offscreen stuff
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
			- Surfer dude fish
			- Streams of water that push you faster
		- Cloud kingdom
			- Princess- authoritarian
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

### UI
- Import menus from zubenel
- TextMeshPro
- Outline on star # GUI (white text, dark outline)
- Add back HUD sliding in/out
	- Also show star count when paused
	- Use quadratic slope for in/out movement?
- Keyboard/gamepad support for menus
- Options buttons on title and pause
	- Volume control
	- Invincibility
- Star collect overlay
	- Banner/star image color matches star color (or banner contrasts and matches level color?)
	- Fancy transitions for banner and overlay
- Cartoony font

### Camera
- Follow player- part of prefab
	- room to move back and forth a bit
	- lead the player's movement?
- 2/3 down normally? or snapped to offset from ground position
- Trigger areas
	- Lerp to fixed or relative positions
	- Zoom out
	- At top of pits, near interesting thing

### Misc
- Doors
- Zoom in to star when approaching (then zoom out after collected)
- Gamepad/keyboard controls text
- Freeze for a bit on hit then flicker after respawning
- Checkpoint notifications
- Remove N cheat
- Scene transitions (to point in scene)
	- Shrinking cutout star animation
- Slopes
	- slide down >= 45 degree slopes, can't move/jump/roll
		- not if already rolling?
- Water
	- Decreases gravity
	- Decreases move speed?
	- Allows offground jumps after swim animation is complete? This is just mario...
		- plays full swim animation then goes to swim-stand

### Sound
- Multiple jump sounds
- Wallslide down
- Roll (whoosh)
- star collect (level up) https://freesound.org/people/elmasmalo1/sounds/350841/
- star twinkle https://freesound.org/people/MrCisum/sounds/336664/
- collect star again (bloop)
- collect powerup (ding or up-whoosh)
- Landing from high fall
- Door sliding open
- Door ascending tones
- Level music (mute during star overlay)

### Art
- Vector art
- Player animations
- Star (juicier shading)
- Star collect image
- Door
- Menus
- Levels
	- Scenery
	- Backgrounds
- Enemies
- Obstacles

- Default sprite facing is left (remove - in facing calculation if not)
- Wall slide sprite should face opposite of other sprites
- Star should be white, colored by material
- When changing player dimensions, update bounding box and fully retest level design
- Player can jump up 3 blocks, across 7 blocks without rolling