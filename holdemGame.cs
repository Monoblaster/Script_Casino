if(!isObject($HoldemGame::Group))
{
	$HoldemGame::Group = new SimSet();
}

function HoldemGame_Create(%table,%exchange)
{
	return new ScriptObject(){class = "HoldemGame";exchange = %exchange;table = %table;blinds = 10;};
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
		cancel(%c.tableProximitySchedule);
		tableProximitySchedule(%c);
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

	%player = %client.player;
	if(!isObject(%player))
	{
		%game.remove(%client);
		return;
	}

	%playerPos = setWord(%player.getPosition(),2,0);
	%handPos = setWord(%game.table.playerButton[%game.clientSeat[%client]],2,0);

	if(vectorDist(%playerPos,%handPos) > 3)
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
	
	%c.chatMessage("\c6You have been removed from Texas Hold'em and your chips have been exchanged. Please leave your seat.");
	NYgiveClientMoney(%c, mCeil(%obj.game.playerStack(%seat) / %obj.exchange));
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
		%timelimit = "You have \c215 seconds\c5.";
		%obj.takeTooLongSchedule = %obj.schedule(15000,"Command","Fold (Ran out of time)");
	}
	
	%c.chatMessage("\c5Take your turn, you can\c2" SPC stringList(%o," ","\c5,\c2","\c5or\c2") @"\c5." SPC %timelimit);
	%obj.messageAll("\c6It is\c3" SPC %c.getPlayerName() @ "\c6's turn.",%c);
	%c.playSound("BrickChangeSound");
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
			%c.chatMessage("\c5You ran out of chips!");
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

		%table.playerStake(%curr,%game.playerStake(%curr));
		%table.playerStack(%curr,%game.playerStack(%curr));
		%table.playerMarker(%curr,0.5,"0 0 0.9 1");

		%curr = %seats.next(%curr + 1);
	
		%table.playerStake(%curr,%game.playerStake(%curr));
		%table.playerStack(%curr,%game.playerStack(%curr));
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
		%obj.deal(%deal);
		return true;
	}
	%obj.messageAll("\c5There are not enough players to start a hand.");
	return false;
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
	serverPlay3d("cardPlace"@getRandom(1,4)@"Sound",%obj.table.communityCards[2]);
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
	serverPlay3d("cardPlace"@getRandom(1,4)@"Sound",%obj.table.communityCards[2]);
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

		%showdown = %obj.game.showdown();

		%count = getFieldCount(%showdown);
		for(%i = 0; %i < %count; %i++)
		{
			%currPot = getField(%showdown,%i);
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
		}
		serverPlay3d("rewardSound",%obj.table.communityCards[2]);

		%count = %seats.count();
		%player = %seats.next(0);
		for(%i = 0; %i < %count; %i++)
		{
			%table.playerStake(%player,%game.playerStake(%player));
			%table.playerStack(%player,%game.playerStack(%player));
			%player = %seats.next(%player + 1);
		}

		if(%obj.automated && %obj.game.seats.count() >= 2)
		{
			%obj.messageAll("\c6The game will restart in \c210 seconds...");
			%obj.startSchedule = %obj.schedule(10000,"start");
		}
	}
	else
	{
		%obj.promptInput();
	}
}


function HoldemGame::Fold(%obj,%vars)
{
	%obj.game.fold();
	%curr = %obj.game.curr();
	%obj.table.playerHand(%obj.seatClient[%curr],%curr,"");
	return true;
}

function HoldemGame::allin(%obj,%vars)
{
	%obj.game.allin();
	return true;
}

function HoldemGame::check(%obj,%vars)
{
	if(!%obj.game.canCheck())
	{
		return false;
	}
	%obj.game.check();
	return true;
}
function HoldemGame::call(%obj,%vars)
{
	if(!%obj.game.canCall())
	{
		return false;
	}
	%obj.game.call();
	return true;
}
function HoldemGame::raise(%obj,%vars)
{
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
		return false;
	}
	%obj.game.raise(%val);
	return true;
}
function HoldemGame::bet(%obj,%vars)
{
	%val = getWord(%vars,0) - %obj.game.minStake();
	if(!%obj.game.canBet() || %val <= 0)
	{
		return false;
	}
	%obj.game.bet(%val);
	return true;
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
			%handled = %obj.fold(%vars);
		case 1:
			%handled = %obj.check(%vars);
		case 2:
			%handled = %obj.call(%vars);
		case 3:
			%handled = %obj.allin(%vars);
		case 4:
			%handled = %obj.raise(%vars);
		case 5:
			%handled = %obj.bet(%vars);
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
		%obj.table.playerHand(%p,%seat,%obj.game.cards.get("hand"@%seat),false);
		serverPlay3d("cardPlace"@getRandom(1,4)@"Sound",%p.getPosition());
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
		%obj.table.playerHand(%p,%seat,%obj.game.cards.get("hand"@%seat),true);
		serverPlay3d("cardPick"@getRandom(1,4)@"Sound",%p.getPosition());
	}
}

function HoldemGame::CasinoMessage(%obj,%sender,%s)
{
	%group = ClientGroup;
	%count = %group.getCount ();
	for(%i = 0; %i < %count; %i++)
	{
		%client = %group.getObject(%i);
		
		if(%client.casinoGame != %sender.casinoGame && isOObject(%client.casinoGame))
		{
			continue;
		}

		if(%client.casinoGame == %sender.casinoGame)
		{
			chatMessageClient(%client, %sender, %voiceTag, %voicePitch, '\c2(Casino) \c7%1\c3%2\c7%3\c6: %4', %client.clanPrefix, %client.getPlayerName (), %client.clanSuffix, %s);
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

		chatMessageClient(%client, %sender, %voiceTag, %voicePitch, '\c2(Casino) \c7%1\c3%2\c7%3\c6: %4', %client.clanPrefix, %client.getPlayerName (), %client.clanSuffix, %s);
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

