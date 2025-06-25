# RPG Action Character Controller & Combat System (Unity, C#)

A modular, extensible character controller and combat system for third-person RPGs, built in Unity.  
This project features smooth movement, advanced camera control, combo attacks, dashing, target lock-on, and a flexible animation framework—suitable for prototyping or as a base for production RPGs.

---

## Features

### Character Movement
- **Dual Input Support:**  
  Seamlessly supports both keyboard (WASD) and gamepad controls.
- **Smooth Motion:**  
  Implements acceleration, deceleration, and analog input blending for natural movement.
- **Actions:**  
  Walking, running, dashing (with stamina or cooldown), and jumping.
- **Animation Sync:**  
  Real-time movement values drive blend trees for smooth locomotion transitions.

### Combat System
- **Basic Attacks:**  
  Single input triggers attack, fully integrated with animation events.
- **Combo Chaining:**  
  Tracks input windows and animation timing for multi-step combos.
- **Skills & Specials:**  
  Expandable framework for adding special moves, dashes, or magic.
- **Feedback Effects:**  
  Includes hit VFX, camera shake, time slowdown, and attack trails for impact.

### Camera Controller
- **Third-Person Tracking:**  
  Follows player with smooth lerp and pivot; adjustable for different genres.
- **Free-Look & Rotation:**  
  Right stick or mouse controls orbit; supports invert axes and speed adjustment.
- **Zoom Functionality:**  
  Scroll or trigger-based zoom; can enforce min/max distance.
- **Collision Handling:**  
  Automatic camera repositioning to avoid obstacles and walls.
- **Target Lock-On:**  
  Snap camera to focus on nearest enemy; supports switching targets.

### Animation Integration
- **Blend Tree Ready:**  
  Exposes all key parameters (speed, direction, attack state) for animator use.
- **State Management:**  
  Handles transition logic between idle, move, dash, jump, and attack.
- **Animator Events:**  
  Integrates Unity animation events to trigger attack/collision logic.

### Easy Customization
- **Inspector Tuning:**  
  All major parameters (speed, cooldowns, camera settings) available in Inspector.
- **Modular Scripts:**  
  Each system (movement, camera, attack) is decoupled for independent reuse or extension.
- **Rich Inspector Experience:**  
  Uses `[Header]` and `[Tooltip]` for fast onboarding and collaboration.

---

## Main Scripts

### `movementControll.cs`
- Handles input (keyboard/gamepad auto-detection).
- Processes walking, running, dashing, and jumping logic.
- Manages animation states for locomotion.
- Includes input smoothing and movement lock during special actions.

### `AttackSystem.cs`
- Controls attack input, combo step tracking, and skill activation.
- Manages hit detection, damage application, and effect triggers.
- Handles dash attacks and cooldown management.
- Integrates with particle and trail effects for feedback.
- Connects with camera system for hit lag/cinematic effects.

### `camControll.cs`
- Provides smooth third-person camera following and rotation.
- Handles zooming, collision avoidance, and environment clipping.
- Supports target lock-on mode with enemy cycling.
- Synchronizes camera states with player/attack actions.

---

## Demo & Blog

- **Full demo video and technical write-up:**  
  https://kenji-dev.vercel.app/blog/rpg

---

## Code Highlights

- **Clarity:**  
  Written for easy reading and rapid iteration; variable and method names are self-explanatory.
- **Inspector Optimization:**  
  Public fields use `[Header]` and `[Tooltip]` for streamlined parameter tuning.
- **Cross-Input Design:**  
  Built for both keyboard/mouse and gamepad out of the box.
- **Functional Separation:**  
  Movement, attack, and camera logic are each modular and independently testable.
- **Prototype to Production:**  
  Structure and comments support fast prototyping and scaling up for larger projects.

---
