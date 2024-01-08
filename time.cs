NYrefreshCommandsTab();
NYxxMusicTab.visible = true;
NYxxMusicStdPlayTracks.clear();
NYxxMusicStdPlayTracks.populated = false;
NYxxMusicLongPlayTracks.clear();
NYxxMusicLongPlayTracks.populated = false;
populateDropdown(NYxxMusicStdPlayTracks, "MusicStd");
populateDropdown(NYxxMusicLongPlayTracks, "MusicLong");
NYxxMusicStdPlayTracks.sort();
NYxxMusicLongPlayTracks.sort();

$RLCBMSINGS::_Time_Song = "Time Time Take your time We can do we can do we can do it Take your time We can do we can do we can do it We can do we can do we can do it Take your time We can do we can do we can do it You know that I been licked and I'm terminally hip And you see me crawlin' in again reeking of the same gin Through the door many times same condition broken mind Gotta start the same show all over again Powerless flowerless cannot do this any more Remember to surrender to the power of your choice Acceptance is the answer page four fourty nine Slow down take it easy Tommy ain't no hurry here Take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take your time We can do we can do we can do it Take your time We can do we can do we can do it [Scatting] Inventory purgatory checked out my soul I'm a legend in my own time legend in my own mind The plug is in the jug and reality is setting in The pink cloud is wearin thin the girls are lookin' good But I'm broke But I'm broke Hey Tommy lookin' good got a tan feelin better? Heard you readin' chapter five you know how to stay alive Every morning hit you knees ninety meetings ninety days Heard you share we care check it out Tommy time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time Take take take your time take a day at a time [Scatting] Time";
$RLCBMSINGS::_Time_Beats = "1 r1 r1 r1 1 r1 r1 r1"
SPC "r2 r8 8 8 8 r1 r8 8 8 4 8 8 4 8 8 4 4 r8 r2 r8 8 8 8 r1 r8 8 8 4 8 8 4 8 8 4 4 r8 r1 r1 r8 8 8 4 8 8 4 8 8 4 4 r8 r2 r8 8 8 8 r1 r8 8 8 4 8 8 4 8 8 4 4 r16 16"
SPC "16 16 16 16 16 16 8 4 16 16 16 8 16 8 16 8 r16 8 16 16 8 8 16 16 8 16 16 8 16 8 r16 8 8 8 16 16 8 16 8 r16 8 r4"
SPC "4 4 8 16 16 8 16 8 r16 16 8 r16 16 16 8 16 16 8 4 16 16 8 8 8 8 8 8 8 16 16 8 8 16 16 8 8 16 16"
SPC "8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 16 r2"
SPC "r2 r8 8 8 8 r1 r8 8 8 4 8 8 4 8 8 4 4 r8 r2 r8 8 8 8 r1 r8 8 8 4 8 8 4 8 8 4 4 r8 1 r1 r1 r1 r1 r1 r1 r1"
SPC "4 4 16 8 16 8 16 16 8 16 16 8 8 8 16 8 16 8 16 16 16 16 16 8 16 4 16 8 16 16 8 16 16 8 16 16 16 16 8 16 16 8 1 r2 r4 r16 16 8 1 r1"
SPC "8 8 8 8 16 16 8 8 8 16 16 8 8 8 16 16 16 16 16 8 r16 8 8 16 16 8 8 8 8 8 16 16 8 8 8 16 16 8 4"
SPC "8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 8 r16 8 8 16 16 8 16 16 16 16 16 16 r2 1 r1 r1 r1 r1 r1 r1 r1 r1 r1 1";
$RLCBMSINGS::_Time_BPM = 133;
$RLCBMSINGS::_Time_Name = "Scatman John - Time";
$RLCBMSINGS::_Time_Delay = 0;

function RLCBMSINGS(%song,%startingbeat)
{
	if(%startingbeat $= "")
	{
		commandToServer('NYMusicPlay', "long",NYxxMusicLongPlayTracks.findtext($RLCBMSINGS::_[%song,"Name"]));
	}
	
	%nextBeatIndex = 0;
	%nextWordIndex = 0;
	%nextBeatTime = 0;
	while(%startingBeat > %nextBeatIndex)
	{	
		%beat = getWord($RLCBMSINGS::_[%song,"Beats"],%nextBeatIndex);

		if(%beat $= "")
		{
			return;
		}

		if(getSubStr(%beat,0,1) > 0)
		{
			%nextWordIndex++;
		}
		else
		{
			%beat = getSubStr(%beat,1,999999);
		}
		%nextBeatTime = 4/%beat + %nextBeatTime;
		%nextBeatIndex++;
	}
	%bpms = $RLCBMSINGS::_[%song,"BPM"] / 60000;
	%elapsed = %nextBeatTime / %bpms;
	cancel($RLCBMSINGS_schedule);
	RLCBMSINGS_schedule(%song,getRealTime(),%bpms,%elapsed - $RLCBMSINGS::_[%song,"Delay"],%nextBeatTime,%nextBeatIndex,%nextWordIndex);
}

function RLCBMSINGS_schedule(%song,%scheduleTime,%bpms,%elapsed,%nextBeatTime,%nextBeatIndex,%nextWordIndex)
{
	%delta = getRealTime() - %scheduleTime;
	%elapsed += %delta;
	
	%elapsedbeats = %bpms * %elapsed;
	if(%nextBeatTime <= %elapsedbeats)
	{
		%beat = getWord($RLCBMSINGS::_[%song,"Beats"],%nextBeatIndex);

		if(%beat $= "")
		{
			return;
		}

		if(getSubStr(%beat,0,1) > 0)
		{
			%word = getWord($RLCBMSINGS::_[%song,"Song"],%nextWordIndex);
			commandToServer('LyricBottomPrint',%word);
			echo(%word);
			%nextWordIndex++;
		}
		else
		{
			if(%beat $= "blugh")
			{
				%beat = 0.00000001;
				echo("blugh");
			}
			else
			{
				%beat = getSubStr(%beat,1,999999);
			}
			
		}
		%nextBeatTime = 4/%beat  + %nextBeatTime;
		%nextBeatIndex++;
	}

	$RLCBMSINGS_schedule = schedule(33,0,"RLCBMSINGS_schedule",%song,getRealTime(),%bpms,%elapsed,%nextBeatTime,%nextBeatIndex,%nextWordIndex);
}

function serverCMDLyricBottomPrint(%client,%word)
{
	if(!(%client.isAdmin || %client.isSuperAdmin))
	{
		return;
	}
		
	for(%i = 0; %i < $NYCharCount; %i++)
	{
		if((%n=getField($NYChar[%i], 0)) $= "rlcbm")
		{
			%charName = %n;
			%charBitmap = "Add-Ons/"@ $NY::ServerAddonName @"/bottomPrintPics/"@ getField($NYChar[%i], 1);
		}
	}

	commandToAll('bottomPrint', "<bitmap:"@ %charBitmap @"> " @ "\c4rlcbm\c5:<br> \c5" @ %word, 15, true);
}