if(!isObject($HoldemGame::Group))
{
	$HoldemGame::Group = new SimSet();
}

function HoldemGame_Create(%table)
{
	return new ScriptObject(){class = "HoldemGame";exchange = 1;table = %table;blinds = 10;}; //exchange rate changed to always be 1 cause buy in is in chips
}

function HoldemGame::OnAdd(%obj)
{	
	$HoldemGame::Group.add(%obj);
	%obj.listClient = new SimSet();
	%obj.game = Holdem_Create();
}

function HoldemGame::OnRemove(%obj)
{
	%group = %obj.listClient;
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%c = %group.getObject(%i);
		%obj.remove(%c);
	}

	%obj.listClient.delete();
	%obj.game.delete();
	%obj.table.delete();
}

function HoldemGame::add(%obj,%c,%buy,%seat)
{
	if(%c.casinoGame !$= "" || !isObject(%c))
	{
		return false;
	}

	%p = %c.player;
	
	%chips = mFloor(%buy * %obj.exchange);
	if(%seat $= "" || %seat >= %obj.table.handCount)
	{
		%seat = %obj.game.addPlayer(%chips);
	}
	else
	{
		%obj.game.addPlayer(%chips,%seat);
	}
	
	%obj.clientSeat[%c] = %seat;
	%obj.seatClient[%seat] = %c;
	%obj.listClient.add(%c);
	%c.chatMessage("\c6You have been added to Texas Hold'em. Take your seat at seat \c5#" @ %seat + 1 @ "\c6.");
	%obj.table.playerStack(%seat,%chips);
	%obj.table.playerStake(%seat);
	%obj.table.playerHand(%p,%seat);
	%obj.table.playerButton(%seat);

	%c.casinoGame = %obj; 
	if(%obj.automated)
	{
		// cancel(%c.tableProximitySchedule);
		// tableProximitySchedule(%c);
		if(!isEventPending(%obj.startSchedule) && %obj.currInput $= "" && %obj.game.seats.count() >= 2)
		{
			%obj.start();
		}
	}

	return true;
}

function tableProximitySchedule(%client)
{
	%game = %client.casinoGame;
	if(!isObject(%game))
	{
		return;
	}

	%canLeave = %game.currinput !$= "" || isEventPending(%game.startSchedule) || %game.game.seats.count() == 1;

	%player = %client.player;
	if(!isObject(%player) && %canLeave)
	{
		%game.remove(%client);
		return;
	}

	%playerPos = setWord(%player.getPosition(),2,0);
	%handPos = setWord(%game.table.playerButton[%game.clientSeat[%client]],2,0);

	if(vectorDist(%playerPos,%handPos) > 3 && %canLeave)
	{
		%game.remove(%client);
		return;
	}
	%client.tableProximitySchedule = schedule(500,%client,tableProximitySchedule,%client);
}

function HoldemGame::remove(%obj,%c)
{
	if(%c.casinoGame !$= %obj)
	{
		return false;
	}

	%p = %c.player;
	%seat = %obj.clientSeat[%c];
	
	if(%obj.currInput == %c)
	{
		%obj.command("Fold (Left the game)");
	}
	
	%c.chatMessage("\c6You and your chips have been removed from Texas Hold'em. Please leave your seat.");
	%c.CasinoChips += %obj.game.playerStack(%seat);
	%obj.game.setPlayer(%seat,false,0);
	%obj.seatClient[%seat] = "";
	%obj.listClient.remove(%c);

	%obj.table.playerStack(%seat);
	%obj.table.playerStake(%seat);
	%obj.table.playerHand(%p,%seat);

	if(%obj.automated)
	{
		%obj.table.playerButton(%seat,1,"1 0 0 1");
	}
	
	%c.casinoGame = ""; 
	
	return true;
}

function HoldemGame::promptInput(%obj)
{
	%c = %obj.seatClient[%obj.game.curr()];
	%obj.currInput = %c;
	//prompt user and check what moves they can do
	
	if(%obj.game.canAllIn())
	{
		%o = %o SPC "all-in";
	}
	if(%obj.game.canCheck())
	{
		%o = %o SPC "check";
	}
	if(%obj.game.canCall())
	{
		%o = %o SPC "call";
	}
	if(%obj.game.canRaise())
	{
		%o = %o SPC "raise";
	}
	if(%obj.game.canBet())
	{
		%o = %o SPC "bet";
	}
	%o = %o SPC "fold";
	%o = trim(%o);

	if(%obj.automated)
	{
		%timelimit = "You have \c220 seconds\c5.";
		%obj.takeTooLongSchedule = %obj.schedule(20000,"Command","Fold (Ran out of time)");
	}
	
	%c.chatMessage("\c5Take your turn, you can\c2" SPC stringList(%o," ","\c5,\c2","\c5or\c2") @"\c5." SPC %timelimit);
	%obj.messageAll("\c6It is\c3" SPC %c.getPlayerName() @ "\c6's turn.",%c);
	%c.playSound("Beep_Popup_Sound");
}

function HoldemGame::start(%obj,%blind)
{
	if(%obj.automated && %obj.game.seats.count() < 2)
	{
		return;
	}

	if(%blind $= "" && %obj.automated !$= "")
	{
		%blind = %obj.automatedblind;
	}

	%game = %Obj.game;
	%seats = %game.seats;
	%count = %seats.count();
	%player = %seats.next(0);
	for(%i = 0; %i < %count; %i++)
	{
		if(%game.playerStack(%player) == 0)
		{
			%c = %obj.seatClient[%player];
			%obj.remove(%c);	
		}
		%player = %seats.next(%player + 1);
	}
	

	if(%obj.game.nextHand(%blind))
	{
		//message all the game has started and place blinds + deal animation
		%obj.messageAll("\c6A hand has started. When it is your turn say one of your availble commands in chat. To peek click on your cards.");
		%game = %obj.game;
		%seats = %game.seats;
		%table = %obj.table;
		
		%obj.updateCommunity();

		for(%i = 0;%i < %table.handCount;%i++)
		{
			%table.playerMarker(%i);
		}

		%table.playerMarker(%game.dealerButton,0.5,"1 1 1 1");
		%curr = %seats.next(%game.dealerButton + 1);
		%obj.schedule(0,"blind",%curr);
		%table.playerMarker(%curr,0.5,"0 0 0.9 1");
		%curr = %seats.next(%curr + 1);
		%table.playerMarker(%curr,0.5,"0.9 0.9 0 1");	
		
		%deal1 = "";
		%deal2 = "";
		%count = %seats.count();
		%player = %seats.next(%obj.dealerButton + 1);
		for(%i = 0; %i < %count; %i++)
		{
			%obj.table.playerHand(%obj.seatClient[%player].player,%player,"");
			%deal1 = %deal1 TAB %player SPC 0;
			%deal2 = %deal2 TAB %player SPC 1;
			%player = %seats.next(%player + 1);
		}
		%deal = trim(%deal1) TAB trim(%deal2);
		%obj.schedule(1000,"deal",%deal);
		return true;
	}
	%obj.messageAll("\c5There are not enough players to start a hand.");
	return false;
}

function HoldemGame::blind(%obj,%i,%small)
{
	%table = %obj.table;
	%game = %obj.game;
	%stake = %game.playerStake(%i);

	%obj.CasinoMessage(%obj.seatClient[%i], "Bet" SPC %stake SPC "(Blind)");
	serverPlay3d(Casino_GetRandomSound("Chip",3),%table.playerStake[%i]);

	%table.playerStake(%i,%stake);
	%table.playerStack(%i,%game.playerStack(%i));

	if(%small)
	{
		return;
	}
	%obj.schedule(500,"blind",%game.seats.next(%i + 1),true);
}

function HoldemGame::deal(%obj,%list)
{	
	if(%list $= "")
	{
		%obj.promptInput();
		return"";
	}
	%list = nextToken(%list,"curr","\t");
	
	%seat = getWord(%curr,0);
	%cards = getWord(%curr,1);

	%p = %obj.seatClient[%seat].player;
	%obj.table.playerHand(%p,%seat,getWords(%obj.game.cards.get("hand" @ %seat),0,%cards));
	serverPlay3d(Casino_GetRandomSound("cardPlace",4),%obj.table.communityCards[2]);
	%obj.schedule(250,"deal",%list);
}

$c = -1;
lookupEntry("HoldemCommand","fold",$c++);
lookupEntry("HoldemCommand","check",$c++);
lookupEntry("HoldemCommand","call",$c++);
lookupEntry("HoldemCommand","all-in",$c++);
lookupEntry("HoldemCommand","raise",$c++);
lookupEntry("HoldemCommand","bet",$c++);
lookupEntry("HoldemCommand","count",$c++);

function HoldemGame::updateCommunity(%obj)
{
	%table = %obj.table;
	%game = %obj.game;
	%community = %game.cards.get("community");

	%table.setCommunity(%community);
	serverPlay3d(Casino_GetRandomSound("cardPlace",4),%obj.table.communityCards[2]);
}

function HoldemGame::messageAll(%obj,%s,%except)
{
	%group = %obj.listClient;
	%count = %group.getCount();
	for(%i = 0; %i < %count; %i++)
	{
		%curr = %group.getObject(%i);
		if(%curr == %except)
		{
			continue;
		}
		%curr.chatMessage(%s);
	}
}

function HoldemGame::next(%obj)
{
	cancel(%obj.takeTooLongSchedule);
	%table = %obj.table;
	%game = %obj.game;
	%curr = %game.curr();
	%table.playerStake(%curr,%game.playerStake(%curr));
	%table.playerStack(%curr,%game.playerStack(%curr));

	%round = %game.round();
	if(!%game.over())
	{
		if(%obj.game.passed())
		{
			//deal community cards
			if(%round == 0)
			{
				%obj.game.dealCommunity().dealCommunity().dealCommunity();
			}
			else if(%round != 3)
			{
				%obj.game.dealCommunity();
			}
			%obj.updateCommunity();
			%obj.game.nextRound();
		}
		else
		{
			%obj.game.nextTurn();
		}
		%round = %game.round();
	}

	if(%game.over())
	{
		%obj.currInput = "";

		%count = (%round == 0) * 3 + 3 - getMax(%round,1);

		for(%i = 0; %i < %count; %i++)
		{
			%game.dealCommunity();
		}
		%obj.updateCommunity();

		%seats = %game.seats;
		if(%game.reveal())
		{
			%count = %seats.count();
			%player = %seats.next(0);
			for(%i = 0; %i < %count; %i++)
			{
				if(!%game.playerFolded(%i))
				{
					%obj.table.playerHand(%obj.seatClient[%player].player,%player,%game.cards.get("hand"@%player),false,true);
				}
				%player = %seats.next(%player + 1);
			}
		}

		%obj.pots(%obj.game.showdown());
		%count = %seats.count();
		%player = %seats.next(0);
		for(%i = 0; %i < %count; %i++)
		{
			%table.playerStake(%player,%game.playerStake(%player));
			%table.playerStack(%player,%game.playerStack(%player));
			%player = %seats.next(%player + 1);
		}
	}
	else
	{
		%obj.promptInput();
	}
}

function HoldemGame::pots(%obj,%showdown)
{
	%table = %obj.table;
	%game = %obj.game;
	%seats = %game.seats;

	serverPlay3d("rewardSound",%obj.table.communityCards[2]);
	%currPot = getField(%showdown,0);
	%showdown = removeField(%showdown,0);
	%pot = getWord(%currPot,0);
	%hand = getWord(%currPot,1);
	%winnings = getWord(%currPot,2);
	%temp = getWords(%currPot,3);

	%winnerCount = getWordCount(%temp);
	for(%j = 0; %j < %winnerCount; %j++)
	{
		%winners = %winners TAB %obj.seatClient[getWord(%temp,%j)].getPlayerName();
	}
	%winners = stringList(trim(%winners),"\t",",","and");

	%potType = "the main pot";
	if(%pot > 0)
	{
		%potType = "side pot " @ %pot;
	}

	%handstring = "";
	if(%game.reveal())
	{
		%handstring = " \c6with a\c4" SPC Poker_Type(%hand);
	}
	
	%obj.messageAll("\c3" @ %winners SPC "\c6won\c2" SPC %potType SPC "\c6of\c2" SPC %winnings SPC "chips" @ %handstring @ "\c6.");

	if(getFieldCount(%showDown) == 0)
	{
		%count = %seats.count();
		%player = %seats.next(0);
		for(%i = 0; %i < %count; %i++)
		{
			if(%game.playerStack(%player) == 0)
			{
				%c = %obj.seatClient[%player];
				%c.chatMessage("\c5You ran out of chips!");
				%obj.remove(%c);	
			}
			%player = %seats.next(%player + 1);
		}

		if(%obj.automated && %obj.game.seats.count() >= 2)
		{
			%obj.messageAll("\c6The game will restart in \c210 seconds...");
			%obj.startSchedule = %obj.schedule(10000,"start");
		}
		return;
	}

	%obj.schedule(1000,"pots",%showdown);
}

function HoldemGame::Command(%obj,%s)
{
	%handled = false;
	%count = lookupValue("HoldemCommand","count");
	for(%i = 0; %i < %count; %i++)
	{
		%token = lookupString("HoldemCommand",%i);
		%pos = stripos(%s,%token);
		if(%pos == -1)
		{
			continue;
		}

		%vars = getWords(getSubStr(%s,%pos,999999),getWordCount(%token));
		switch(%i)
		{
		case 0:
			%obj.game.fold();
			%curr = %obj.game.curr();
			%obj.table.playerHand(%obj.seatClient[%curr],%curr,"");
			serverPlay3d(Casino_GetRandomSound("CardShove",3),%obj.table.playerStake[%obj.game.curr()]);
			%handled = true;
		case 1:
			if(!%obj.game.canCheck())
			{
				%handled = false;
			}
			else
			{
				%obj.game.check();
				%handled = true;
			}
		case 2:
			if(!%obj.game.canCall())
			{
				%handled = false;
			}
			else
			{
				%obj.game.call();
				%handled = true;
			}
		case 3:
			serverPlay3d(Casino_GetRandomSound("Chip",3),%obj.table.playerStake[%obj.game.curr()]);
			%obj.game.allin();
			%handled = true;
		case 4:
			%a = getWord(%vars,0);
			%b = getWord(%vars,1);
			if(%b $= "")
			{
				%val = %a - %obj.game.minStake();
			}
			else if(%a $= "to")
			{
				%val = %b - %obj.game.minStake();
			}
			else if(%a $= "by")
			{
				%val = %b + 0;
			}

			if(!%obj.game.canRaise() || %val <= 0)
			{
				%obj.seatClient[%obj.game.curr()].chatMessage("\c5You cannot raise to" SPC %val SPC "chips");
				%handled = false;
			}
			else
			{
				%obj.game.raise(%val);
				serverPlay3d(Casino_GetRandomSound("Chip",3),%obj.table.playerStake[%obj.game.curr()]);
				%handled = true;
			}
		}

		if(%handled)
		{
			%obj.CasinoMessage(%obj.currInput,%s);
			%obj.next();
			break;
		}
	}
	return %handled;
}

function HoldemGameCardViewLoop(%c,%card1,%card2)
{
	%c.centerPrint("<just:left><font:consolas:50><lmargin%:30>" @ Poker_ShortName(%card1) @ Poker_ShortName(%card2),5);
	%c.HoldemGameCardViewLoop = schedule(100,"","HoldemGameCardViewLoop",%c,%card1,%card2);
}

function HoldemGame::pickup(%obj,%c,%p)
{
	%seat = %obj.clientSeat[%c];
	if(%obj.currInput $= "" || %obj.game.playerFolded(%seat))
	{
		return "";
	}

	%mounted = %p.getMountNodeObject(7);
	if(%mounted != 0 && %mounted.getDatablock() == CasinoCardHolderPlayer.getId())
	{
		%obj.table.playerHand(%p,%seat,"0 0",false);
		cancel(%c.HoldemGameCardViewLoop);
		%c.centerPrint("");
		serverPlay3d(Casino_GetRandomSound("cardPlace",4),%p.getPosition());
		return "";
	}

	%start = %p.getEyePoint();
	%end = vectorAdd(%start,vectorScale(%p.getEyeVector(),100));
	%mask = $TypeMasks::FxBrickObjectType;
	%ray = ContainerRaycast(%start,%end,%mask,%p);
	%pos = getWords(%ray,1,3);

	%cardPos = %obj.table.playerHand[%seat,0];
	if(vectorDist(%Pos,%cardPos) < 1)
	{
		%obj.table.playerHand(%p,%seat,"0 0",true);
		%cards = %obj.game.cards.get("hand"@%seat);
		HoldemGameCardViewLoop(%c,getWord(%cards,0),getWord(%cards,1));
		serverPlay3d(Casino_GetRandomSound("cardShove",3),%p.getPosition());
	}
}

function HoldemGame::CasinoMessage(%obj,%sender,%s)
{
	%group = ClientGroup;
	%count = %group.getCount ();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getObject(%i);
		
		if(%client.casinoGame != %sender.casinoGame && isObject(%client.casinoGame))
		{
			continue;
		}

		if(%client.casinoGame == %sender.casinoGame)
		{
			chatMessageClient(%client, %sender, %voiceTag, %voicePitch, '\c2(Casino) \c7%1\c3%2\c7%3\c6: %4', %sender.clanPrefix, %sender.getPlayerName (), %sender.clanSuffix, %s);
			continue;
		}

		%player = %client.player;
		if(!isobject(%player))
		{
			continue;
		}

		if(vectorDist(%obj.table.communityCards[2], %player.getPosition()) > 7)
		{
			continue;
		}

		chatMessageClient(%client, %sender, %voiceTag, %voicePitch, '\c2(Casino) \c7%1\c3%2\c7%3\c6: %4', %sender.clanPrefix, %sender.getPlayerName (), %sender.clanSuffix, %s);
		continue;
	}
}

package HoldemGame
{
	function serverCmdMessageSent(%c,%s)
	{
		if(%c.casinoGame.currInput == %c)
		{
			if(%c.casinoGame.Command(%s))
			{
				return;
			}
			
		}
		return Parent::serverCmdMessageSent(%c,%s);
	}

	function Player::ActivateStuff(%p)
	{
		%c = %p.client;
		if(isObject(%c.casinoGame))
		{
			%c.casinoGame.pickup(%c,%p);
		}

		return Parent::ActivateStuff(%p);
	}

	function GameConnection::onClientLeaveGame(%c)
	{
		if(isObject(%c.casinoGame))
		{
			%c.casinoGame.remove(%c);
		}
		parent::onClientLeaveGame(%c);
	}
};
activatePackage("HoldemGame");

