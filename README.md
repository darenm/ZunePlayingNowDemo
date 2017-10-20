# ZunePlayingNowDemo

I brief sample demoinh a Zune-like now playing experience. The effect is achieved with:

* Loading an image onto a surface
* Applying a saturation effect with zero saturation, giving a black and white image
* Adding a Purple AmbientLight
* Adding Green and Yellow PointLights
* Applying an implicit animation to the PointLight offsets and using a DispatcherTimer to periodically set a new location for the PointLights
* Creating a TextSurface and setting up a repeating, auto-reversing animation for the Text fly-in
