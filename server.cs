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

function Casino_GetCurrentSecondOfTheMinute()
{
	return mFloor(getSubStr(getDateTime(), 15, 2));
}

$Casino::IncomeTime = 10; // in minutes
$Casino::IncomeAmount = 10;

$Persistence::MatchName["CasinoChips"] = true;
$Persistence::MatchName["CasinoLastIncomeMinute"] = true;
$Persistence::MatchName["CasinoLastIncomeSecond"] = true;
$Persistence::MatchName["CasinoLastIncomeYear"] = true;

registerOutputEvent("fxDTSBrick", "CasinoIncome", "", true);
registerInputEvent("fxDTSBrick", "onCasinoIncome", "Self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame");

function serverCmdCasinoChips(%client)
{
   if(%client.CasinoChips == 1)
   {
      %client.chatMessage("\c6You have \c51 chip\c6 in storage. :(");
      return;
   }
   
   %client.chatMessage("\c6You have \c5" @ %client.CasinoChips @ " chips\c6 in storage.");
}

function fxDTSBrick::onCasinoIncome(%obj, %player, %client)
{
	$InputTarget_["Self"] = %obj;
	$InputTarget_["Player"] = %player;
	$InputTarget_["Client"] = %client;
	if ($Server::LAN)
	{
		$InputTarget_["MiniGame"] = getMiniGameFromObject(%client);
	}
	else if (getMiniGameFromObject(%obj) == getMiniGameFromObject(%client))
	{
		$InputTarget_["MiniGame"] = getMiniGameFromObject(%obj);
	}
	else
	{
		$InputTarget_["MiniGame"] = 0;
	}
	%obj.processInputEvent("onCasinoIncome", %client);
}

function fxDTSBrick::CasinoIncome(%brick,%client)
{ 
   %remainingMinutes = %client.CasinoLastIncomeMinute + $Casino::IncomeTime - getCurrentMinuteOfYear() +  (%client.CasinoLastIncomeSecond - Casino_GetCurrentSecondOfTheMinute()) / 60;
   if(getCurrentYear() > %client.CasinoLastIncomeYear)
   {
      %remainingMinutes -= 525600;
   }

   if(%remainingMinutes > 9.92) // cooldown for a couple seconds after claiming to stop you form missing the claim message
   {
      return;
   }

   if(%remainingMinutes > 0)
   {
      if(%remainingMinutes <= 1)
      {
         %client.centerPrint("\c6Come again in \c5" @ mCeil(%remainingMinutes * 60) @ " seconds\c6. Use \c5/CasinoChips\c6 to see your current balance.",3);
         return;
      }

      %client.centerPrint("\c6Come again in \c5" @ mCeil(%remainingMinutes) @ " minutes\c6. Use \c5/CasinoChips\c6 to see your current balance.",3);
      return;
   }

   %client.CasinoLastIncomeMinute = getCurrentMinuteOfYear();
   %client.CasinoLastIncomeSecond = Casino_GetCurrentSecondOfTheMinute();
   %client.CasinoLastIncomeYear = getCurrentYear();
   %client.CasinoChips += $Casino::IncomeAmount;

   %brick.onCasinoIncome(%obj, %client.player, %client);
   %client.centerPrint("\c6You have claimed your \c5" @ $Casino::IncomeAmount @ " chips\c6 and now have \c5" @ %client.CasinoChips 
   @ " chips\c6 in storage!<br>\c6 Come again in \c5" @ $Casino::IncomeTime @ " minutes\c6 to get " @ $Casino::IncomeAmount @ " more.",5);
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
exec("./extraResources");
addExtraResource("Add-Ons/Script_Casino/clubs.png");
addExtraResource("Add-Ons/Script_Casino/hearts.png");
addExtraResource("Add-Ons/Script_Casino/spades.png");
addExtraResource("Add-Ons/Script_Casino/diamonds.png");