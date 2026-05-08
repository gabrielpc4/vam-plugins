E-Motion 1.6 for Virt-A-Mate 1.18 or higher
By VRAdultFun

How to Install :

It is recommended to delete your old E-Motion installation (Custom\Scripts\E-Motion) when upgrading to V1.6
Copy both folders in the ZIP into your VAM installation folder (where the VAM.EXE is). Choose to overwrite if asked.

If you have an existing preset from an older version of E-Motion, move it to custom\scripts\E-Motion\Presets\

Included in the Download :

E-Motion 1.6
Hand Animator v1.2
Bulger 1.0
Light Point 1.0
V3 Long Hair + Presets
Free Morphs used by Jessica look

Option Morphs used by E-Motion if Installed :
AshAuryn 174 Sex Expressions Pack (Free, Highly Recommended)
https://www.reddit.com/r/VAMscenes/comments/czh9sm/expression_pack_sexpressions_174_morphs/

SkyBox3D's Shoulder Fix morphs (Patreon exclusive download. Not essential but worth the once-off imo)
https://www.patreon.com/skybox3D

E-Motion Overview :

The basic idea behind E-Motion is to make your virtual girl react to your presence and anything you do to her.
Your face, hands and pelvis (if you possess a Person) are tracked in relation to her body and given an interest rating which is
then used to determine what she will look at. Depending on what she is looking at, what you are looking at (or touching!) and how far
away from her you are, she will become more or less interested in (and aroused by) you.
How happy and/or aroused she becomes will effect how interested in different body parts she is, as well as what type of expressions she can show.
Depending on how interested in you she is, she will switch between various emotion or attention states that define overall facial expressions
and special looks.  Those expressions are built from 3 layers, Eye Brows, Eyes and Mouth which can be set individually or in sets.

In addition to the reactionary system above, E-Motion allows her to react to blow-job and Sex animated scenes (motioned must be created by you) 
and gives her the ability to kiss her target.

The UI for E-Motion is very complex and allows you to adjust almost all aspects of E-Motion.  See the below Instructions and included UI Description file

E-Motion is built first and foremost for Virtual Reality and it is recommended to possess a male character. It works fine in Desktop mode and she
will look at the camera if you do not select a target however it works much better if you have a body.

By default E-Motion's defaults set everything to off. This prevents accidentally effecting poses/morphs when adding E-Motion.  There are several presets
included to quickly see E-Motion in action and you can save your own presets to quickly get E-Motion up and running on new scenes.  
It is recommended to overwrite the 'E-Motion_Defaults.json' preset with your own preferred settings once you are familiar with E-Motion.  The settings in
this file are loaded when you first add the plugin to a person, or if you press the 'load defaults' button on the UI.



Basic Instructions :

E-Motion requires at minimum 1 person in the scene, however it works much better with 2. For full functionality, E-Motion requires control of the
Head and Neck position and rotations (Auto Config Head on the UI will configure these for you if enabled).  Other features like Idle and Breathing effect
Joint Drives on one or more of the following : Pelvis, Abdomen2, Chest, Neck, LShoulder, RShoulder, LArm, RArm, LElbow, RElbow, LHand, RHand
See the UI Stats on the message log for a list of morphs effected.

Select the person you wish E-Motion to control, and then select the Plugin tab
Click Add Plugin and open the E-Motion folder. Add the E-Motion_AddThisONLY.cslist file

E-Motion is now active, however by default all features are disabled.  Click the 'Open Custom UI..' button on the E-Motion entry in the Plugins tab.
On the right side of the UI click 'Load Preset' and select the file 'VRAdultFun Confident Slow.json'

If a second person exists in the scene called 'Person#2' the included presets will look at them, otherwise they will revert to the camera.
You can always select a new target from the UI.
Disabling 'Look at selected Person' on the ui will switch E-Motion to using the Camera (or headset in VR). In desktop mode hands are 'simulated' by
your side, in VR your controllers will be used if you are not possessing anyone. Unfortunately this does not currently include Leap Motion hands


How to create your own Preset :

E-Motion is driven by the personality profile, the first three sliders on the UI.  Start by setting these using the below information as a guide.
All the functions of E-Motion are tied into these values in some way, which is why most sliders are 'multipliers' rather than setting a value directly.
If you are starting from the defaults, now enable the features you wish to use.
Tweak the profile and then once the basic behavior is likable, use the rest of the UI to tweak any areas further.  The sliders are grouped together to
assist with finding settings, and the included UI Description file explains what each slider and option does.
Once you are happy with how she acts, you can click 'Save Preset' at the top.

At the top of the UI is an option 'Show Stats on Message Log'. Enabling this will show all the values and morphs used by E-Motion via the ingame message log.
This UI is very performance heavy, so it is not recommended to leave it running all the time. It is designed to help with tweaking E-Motion to get the most
out of the functions, and to help you understand what she is doing.
The Message Log will initially display as a small 3 line box next the main UI. The top of the stats is designed to fit here to make it easier to adjust things.
You can always click 'Open Log' to bring up the large log window to see all the details.


Preparing your characters for better results : 

E-Motion assumes the base look is bored or idle.  Use morphs such as 'Mouth Corners Up-Down' and 'Mouth Frown' to adjust the face into a better idle 
expression. This will help with making smiles look much better

You can use E-Motion on a completely ragdolled Person, or one with a pose that uses every atom in the body.
If you use a static pose, try lowering the rotation hold spring on the upper body atoms if you wish to use idle movement (which is Joint Drive based).

Depending on the look of your character, some of the distances may need to be adjusted in the UI for features to work properly. You will find these on the
right side of the UI underneath the target selectors.


Features :

Personality Profile
All of the functions within E-Motion are driven by a profile of 3 values : Agreeableness, Extraversion and Stableness
The randomness of each function is effected by 2 statistics : Arousal and Happiness

Agreeableness - Higher values will generate happiness faster from interaction and allow more indirect gazing
Extraversion - Higher values will generate arousal faster from interaction and allow more direct eye contact
Stableness - Higher values allow longer gazing and less frequent glancing and avoidance

Arousal - Increases breathing rate, eye contact time, pelvis/penis interest as well as changing what expressions are displayed
Happiness - Increases touch based arousal, head and hand interest as well as changing what expressions are displayed

Expression Control
E-Motion animates many facial and expression morphs to create different looks and facial reactions according to what is going on.
Eyebrows, Eyes and Mouth are animated seperately and can be mixed together for greater variation
Blinking is controlled by E-Motion and tied into eye movement for a more realistic appearance

Breathing and Moaning Sounds (Female voice only)
Over 90 sound effects are included with E-Motion to bring your character to life
You will be able to hear your character breathing, and as they become aroused, they will let out short moans of pleasure
The characters mouth is animated to sync with the moans for greater realism and immersion

Points of Interest (POI)
E-Motion gives a character the ability to look at multiple parts of the body : Head, Left Hand, Right Hand, Chest, Pelvis, Penis (if exists)
Each body part is given an interest value based on stats like proximity, movement, visibility and interaction
The two highest valued POI's are then selected as targets with the character looking at the primary target and glancing at the secondary
An object can also be selected as another POI that can be looked/glanced at.  If a Person is selected as the object, their head will be selected

Head and Eye Movement
E-Motion gives characters a complex gaze control system that is driven by the above POI and has several sub features
 Direct Gaze - Look directly at POI and follow it precisely
 Indirect Gaze - Look in direction of POI and follow it loosely
 Glance - Look at secondary POI without moving head
 Avoid - Look away from Primary POI and avoid Direct Gaze. Look at secondary if visible
 Saccade - The eyes flick around the target area to simulate human saccadic eye movement
The type of gaze, frequency of glances and avoidance are controlled by the personality profile.
Very close proximity to the characters face will override the gaze to look at whatever is very close

Body Movement and Breathing
E-Motion provides a built-in breathing morph animation and upper body movement that is tied into arousal and happiness.
Adjustments are also made to the pelvis, abdomen, chest, shoulders, neck and hands to enhance the expressions being displayed

Character Interaction
If a Person is selected as the target (or you are playing in VR without a target selected) the face/hands/pelvis/penis can be used to interact
with the E-Motion character in the following areas : Head, Left Breast, Right Breast, Pelvis (covers front and rear), Left Hand, Right Hand
Close proximity, touching and carassing these areas will effect interest, arousal and happiness as well as trigger expressions or special features

Special Feature : Kissing
The Character will attempt to line up for a kiss if the target's head is very close to their own. If contact with the lips occurs a kissing animation will activate
Two characters both running E-Motion targetting each other will kiss each other

Special Feature : Blowjob reaction
If you create a blowjob scene, the E-Motion characters face and lips will animate to enhance the experience. If the Deepthroat morph is installed, this will be animated also
Note - E-Motion does not create the thrusting motion, this must be done via the scene (such as a cycle force)

Special Feature : Sex reaction
Touching/Inserting the penis / fingers / face into the characters V will initiate sex and generate lots of Arousal and happiness as well as play a series of sex expressions
Note - E-Motion does not create the thrusting motion, this must be done via the scene (such as a cycle force)

Special Feature : Face Holding
Holding the characters face in your hand(s) will cause the character to press their head into your hand and look into your eyes.
Touching the characters lips with your fingers will have the character kiss and suck the fingers


Tips :

Getting 2 E-Motion characters to kiss works best if the head is not pinned by its position (turn Position Hold Off)
The same goes with Blowjobs, pinning the head will make it more difficult for E-Motion to control the position and alignment
Shoulder and Chest adjustment sliders may need to be adjusted depending on the pose and the strength of its hold
If you wish to possess an E-Motion character, first disable 'Auto Config head' 'Control head' and 'Adjust Hands'
If the head is animated, disable 'Auto Config head' and 'Control Head' before playing the animation.
Set the Personal Space!  Depending on the scene, you may want to increase this value to ensure the E-Motion character pays attention to you
E-Motion works on males and females, possessed and not!
For animations, possession and BVH files it is recommended to set Idle body and arm movement to 0.



Hand Animator v1.2 (disable Hands in E-Motion to use them together)
Animates the following hand morphs dynamically :
Hand Grasp, Fist, Chop, Straighten (Left and Right)
Finger Bend (Each finger both hands)
Thumb Fist, In-Out (Left and Right)
Animates based on movement speed and proximity to :
Head, Neck, Chest, Breasts, Abdomen, Hips (front and behind), hands
UI Controls for adjusting how each morph is applied, resting hand position and speed, distance and animation scales.


Bulger 1.0
Animates the custom morphs for throat and belly bulging (morphs included)
E-Motion already provides this feature when targetting a Person. Bulger can be used to allow her to react to an object like a dildo
It can also be used by itself
UI Controls allows you to set the trigger distances for the effects, as well as how much effect to apply.


Light Point 1.0
Simple plugin designed to be added to Spot Lights that will make them point at the chest of the person called 'Person'
There are no ui options, this is used by the example scene


Long Hair Style
I have included my V3 long hairstyle which can be seen in the included example scene and images.