datablock AudioProfile(Chip1Sound)
{
   filename = "./chip1.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(Chip2Sound)
{
   filename = "./chip2.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(Chip3Sound)
{
   filename = "./chip3.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(CardShove1Sound)
{
   filename = "./cardShove1.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(CardShove2Sound)
{
   filename = "./cardShove2.wav";
   description = AudioClosest3d;
   preload = true;
};

datablock AudioProfile(CardShove3Sound)
{
   filename = "./cardShove3.wav";
   description = AudioClosest3d;
   preload = true;
};

function Casino_GetRandomSound(%name,%count)
{
	return %name @ getRandom(1,%count) @ "Sound";
}

exec("./lookuptables.cs");
exec("./cards.cs");
exec("./poker.cs");
exec("./seats.cs");
exec("./stringutils.cs");
exec("./holdem.cs");
exec("./holdemgame.cs");
exec("./holdemtable.cs");
exec("./commands.cs");