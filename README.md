# Indestructible Decompilation
 A project focused on decompiling the Android game "Indestructible" (3.0.0)

The goal of this project is to decompile the game "Indestructible" and to get it working correctly in Unity 2021.3.17f1.
I've used "AssetRipper" (https://github.com/AssetRipper/AssetRipper) to decompile the game. There obviously has been a lot of errors which I have fixed enough to get the game booting.
The game is now stuck on the initial loading screen because I assume the game is trying to look for the game servers to check if the game is up-to-date or not.
This game uses Photon 3 for its multiplayer solution and as such it uses RPC which is deprecated. Because of this and not knowing how to fix this problem, I have commented RPC lines out until the problem can be fixed.
If you or someone you know is able to fix the problem, I'd be very grateful.