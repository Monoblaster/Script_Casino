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

$Persistence::MatchName["CasinoChips"] = true;
$Persistence::MatchName["CasinoLastIncomeMinute"] = true;

rregisterOutputEvent("fxDTSBrick", "CasinoIncome", "", true);

function fxDTSBrick::CasinoIncome(%brick,%client)
{ 
   %remainingMinutes = %client.CasinoLastIncomeMinute + 10 - getCurrentMinuteOfYear();
   if(%remainingMinutes > 9.92) // cooldown for a couple seconds after claiming to stop you form missing the claim message
   {
      return;
   }

   if(%elapsedMinutes > 0)
   {
      if(%elapsedMinutes <= 1)
      {
         %client.centerPrint("\c6Come again in " @ mCeil(%remainingMinutes * 60) @ " seconds.",2);
      }

      %client.centerPrint("\c6Come again in " @ mCeil(%remainingMinutes) @ " minutes.",2);
      return;
   }

   %client.CasinoLastIncomeMinute = getCurrentMinuteOfYear();
   %client.CasinoChips += 10;

   %client.centerPrint("\c6You have claimed your 10 chips! Come again in 10 minutes to get 10 more.",2);
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