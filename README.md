# Empire

(link off)

Link to a live coding session if somebody wants to join and comment. :)




EmpireTheGame app
    Player  foggymap, armies



+unit waiting for action
+display unit?
+timer?
+fog





model:
 -map
 -list of units
 -fogs?


view model:
 -combined foggy map, units, and unit requests





 display army to move:
	-select 



+scroll
-long goto (on newwin?)
-sentry
-wait
-when do long standing orders get executed
-longMove will have problems with no-route feedback
-as it is now, you can feel the terrain under fog by longMoving to it
-if all units are in standing orders, activeUnit becomes null


app
  -players
  -map
  

MapViewModel
	-old: collection of CellViewModels (linear) -view binds it to itemsControl with Columns
	      loc is from order in the coll
	-new: 2d array of CellViewModels,  view code behind creates blocks, two sets of blocks, map and armies
	      loc is from the cellVM

Fog map for a player is 2d array of ?


+map2 is created only on window open


-transport
-city attack
-armies in cities


-sentrie army, no end of turn trigger

+conquering army is not deleted from map


-small delay after an army move (on planes)
-sentry visuals
-city owner visuals
-long move target and logic of every step
-empty/full transport visuals
-wait should put it at the end
-make units decontained when exiting a city

-activate next army logic -> priority to the armies "behind"  (if moved to the right, activate first in the left cone region)



-after standing move longGoto, if more steps are left, no active army animation is run






do 
	-player move
	-ai move

until game over


2023-06-06
Total reorganization to the game loop and iteractions model. It turns out that ActivateArmy method was important, and
it was called from all over the place.  So we move it into the main loop, and only the main loop calls it.
Also, main loop will start any interaction, and all the moves will pass trough it.  Old implementatio was calling
ActivateUnit and CheckForEndOfTurn all over the place and this was crashing all over.  So:

mail game loop:
	-activate unit (more as a hint)
	-start interaction
	-send interaction result (game move?) to the game model
	-loop while there are still units with steps left


Here we can also check if there were turns without any requested interactions, and then stop if there was any 
attempted interupts (any mouse clicks or key clicks while there were no interactions requested?).


-implement playable text dumps?

2023-06-10

Switch to the new asynchrony model is going nicely.  Now I just need to move the rest of the old code into the
new model.  Before, async calls went to the deepest levels of the model code, so that ment that once you 
use async and await, it contaminates all the call chain up to the top.

Observables have a much nicer model, and you can reference obserbavles just in the places where they are needed.
Right now, it only does the text dumps, and the game model has global subject(s) where it can send the change 
notifications.  The other option is to always pass it as a parameter.  

In order to start conquering the world:
+transporters
+city fight
+city production
+mark active somehow 
-production change somehow (in text mode)
-unsentry 
-load map A

bugs: 
+fresly produced units are not contained
-while flashing, units do not respond instantly because flashes also append delay after them
-transporters flash weirdly when on their second step
-armies refuse to load into transporters
-when moved as a flash, armies stay as flases :()
+seems like armies are not producing when only handling standing orders
-when leaving the city, transport should pick up as many armies as it can
-transports inside cities flash (city-flash sign) instead of (city-transport) 
-if you sentry all units without owning a city, we get infinite sentry loop without a way to break out
-unloading armies were not flashing


