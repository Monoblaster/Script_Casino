function serverCmdSetTable(%c,%id,%buyIn)
{
	if(isObject(%id) && %c.isAdmin)
	{
		%buyIn += 0;
		%id.buyin = %buyin;

		if(%buyIn == 0)
		{
			$Casino::AtiveHoldemTable = %id;
			$Casino::AtiveHoldemTableClient = %c;

			%table = %id.table;
			%count = %table.handCount;
			for(%i = 0; %i < %count; %i++)
			{
				%table.playerButton(%i);
			}
			return;
		}

		%id.automated = true;
		//TODO: automated startup
		//spawn buttons at each player's position to join
		
		%table = %id.table;
		%count = %table.handCount;
		for(%i = 0; %i < %count; %i++)
		{
			%table.playerButton(%i,1,"1 0 0 1");
		}
	}
}

function serverCmdSetTableBlind(%c,%id,%blind)
{
	if(isObject(%id) && %c.isAdmin)
	{
		%id.automatedblind = %blind;
	}
}


// function serverCmdJoinTable(%c)
// {
// 	if(isObject($Casino::AtiveHoldemTable) && !$Casino::AtiveHoldemTablePrivate && (getSimTime() - %c.Holdem_lastRequestTime) >= 5000)
// 	{
// 		%group = ClientGroup;
// 		%count = %group.getCount();
// 		$Casino::AtiveHoldemTableClient.chatMessage("\c5"@%c.getPlayerName()SPC"has requested to join the table.");
// 		%c.Holdem_lastRequestTime = getSimTime();
// 	}
// }

// function serverCmdBuyTable(%client,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j)
// {
	
// 	if($Casino::AtiveHoldemTableClient == %client && !$Casino::AtiveHoldemTablePrivate)
// 	{
// 		%name = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %h SPC %h SPC %i SPC %j);
// 		%target = findClientByName(%name);
// 		if(isObject(%target))
// 		{	
// 			%buyIn = $Casino::AtiveHoldemTableBuyIn;
// 			if(%buyIn >= %target.NYmoney)
// 			{
// 				%client.chatMessage(%target.getPlayerName() SPC "doesn't have enough money.");
// 				return;
// 			}

			
// 			if($Casino::AtiveHoldemTable.add(%target,%buyIn))
// 			{
// 				%client.chatMessage(%target.getPlayerName() SPC "has been added.");
// 				%target.NYmoney -= %buyIn;
// 				%target.setScore(%target.NYmoney);

// 				messageclient(%target, '', "\c3You paid \c6" @ %buyIn @ " points;\c3 you now have \c6" @ %target.NYmoney @ "\c3 points and \c6" @ $Casino::AtiveHoldemTable.exchange * %buyIn @ "\c3 chips.");

// 				NYlogs_write("config/server/LogNewYear/money.txt",
// 					NYlogs_addTime() TAB "MONEY_UPDATE" TAB "NYgiveClientMoney" TAB %target.getBLID() TAB %target.name TAB
// 					"AMOUNT" TAB %buyIn TAB "NEW_VALUE" TAB %target.NYmoney
// 				);
// 			}
// 			else
// 			{
// 				%client.chatMessage(%target.getPlayerName() SPC "failed to add.");
// 			}
// 		}
// 	}
// }

function serverCmdGiveTable(%client,%chips,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j)
{
	
	if($Casino::AtiveHoldemTableClient == %client)
	{
		%name = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %h SPC %h SPC %i SPC %j);
		%target = findClientByName(%name);
		if(isObject(%target))
		{	
			if($Casino::AtiveHoldemTable.add(%target,mCeil(%chips / $Casino::AtiveHoldemTable.exchange)))
			{
				%client.chatMessage(%target.getPlayerName() SPC "has been added");
				%target.chatMessage("\c3You were given \c6" @ %chips @ "\c3 chips for free.");
			}
			else
			{
				%client.chatMessage(%target.getPlayerName() SPC "failed to add.");
			}
		}
	}
}

function serverCmdRemoveTable(%client,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j)
{
	if($Casino::AtiveHoldemTableClient == %client)
	{
		%name = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %h SPC %h SPC %i SPC %j);
		%target = findClientByName(%name);
		if(isObject(%target))
		{
			if($Casino::AtiveHoldemTable.remove(%target))
			{
				%client.chatMessage(%target.getPlayerName() SPC "has been removed.");
			}
			else
			{
				%client.chatMessage(%target.getPlayerName() SPC "failed to remove.");
			}
		}
	}
}

function serverCmdStartTable(%client,%blind)
{
	if(%blind <= 0)
	{
		%client.chatMessage("Please give a blind.");
		return;
	}

	if($Casino::AtiveHoldemTableClient == %client && $Casino::AtiveHoldemTable.currInput $= "")
	{
		
		if($Casino::AtiveHoldemTable.start(%blind))
		{
			%client.chatMessage("Table has been started.");
		}
		else
		{
			%client.chatMessage("Table failed to start.");
		}
	}
}

function serverCmdClosestTable(%c)
{
	%p = %c.player;
	if(%c.isAdmin && isObject(%p))
	{
		%pos = %p.getPosition();
		%group = $HoldemGame::Group;
		%count = %group.getCount();
		if(%count == 0)
		{
			return;
		}

		%closestObj = %group.getObject(0);
		%closestDist = vectorDist(getWords(%closestObj.table.communityCards[2],0,2),%pos);
		for(%i = 1; %i < %count; %i++)
		{
			%currObj = %group.getObject(%i);
			%currDist = vectorDist(getWords(%currObj.table.communityCards[2],0,2),%pos);
			
			if(%closestDist > %currDist)
			{
				%closestObj = %currObj;
				%closestDist = %currDist;
			}
		}

		%c.chatMessage(%closestObj);
	}
}

function serverCmdMessageBoxNo(%client){}

package HoldemCommands
{
	function Player::ActivateStuff(%p)
	{
		%distance = 3;

		%c = %p.client;
		if(isObject(%c.casinoGame))
		{
			return parent::ActivateStuff(%p);
		}

		%pos = %p.getPosition();
		%group = $HoldemGame::Group;
		%count = %group.getCount();
		if(%count == 0)
		{
			return parent::ActivateStuff(%p);
		}

		%game = "";
		%closestDist = 7;
		for(%i = 0; %i < %count; %i++)
		{
			%currObj = %group.getObject(%i);
			%currDist = vectorDist(getWords(%currObj.table.communityCards[2],0,2),%pos);
			
			if(%closestDist > %currDist)
			{
				%game = %currObj;
				%closestDist = %currDist;
			}
		}
		
		if(!isObject(%game))
		{
			return parent::ActivateStuff(%p);
		}
		
		%start = %p.getEyePoint();
		%end = vectorAdd(%start,vectorScale(%p.getEyeVector(), %distance));
		%mask = $TypeMasks::PlayerObjectType | $TypeMasks::StaticObjectType | $TypeMasks::FxBrickObjectType;
		%hit = containerRayCast(%start, %end, %mask, %p);

		if(!isObject(%hit) || %hit.getType() & $TypeMasks::PlayerObjectType)
		{
			return parent::ActivateStuff(%p);
		}

		%hitloc = getWords(%hit,1,3);
		%table = %game.table;
		%count = %table.handCount;
		for(%i = 0; %i < %count; %i++)
		{
			%currbutton = %table.playerButtonObj[%i];
			if(!isObject(%currbutton))
			{
				continue;
			}
			
			%dist = vectorDist(%hitloc,%currButton.getPosition());
			if(%dist > 0.5)
			{
				continue;
			}

			//player pressed button
			%buyin = %game.buyin;
			%c.promptedholdemtable = %game;
			%c.promptedholdemtableseat = %i;
			commandToClient(%c, 'MessageBoxYesNo', 'Join', 'The buy-in is %2 chips.<br>Do you wish to join?','JoinTexasHoldem',%buyin,mFloor(%game.exchange * %buyIn));

			break;
		}

		
		return parent::ActivateStuff(%p);
	}

	function serverCmdMessageBoxNo(%client)
	{
		if(%client.promptedholdemtable !$= "")
		{
			%client.promptedholdemtable = "";
		}
		parent::serverCmdMessageBoxNo(%client);
	}

};
activatePackage("HoldemCommands");

function serverCmdJoinTexasHoldem(%client)
{
	%game = %client.promptedholdemtable;
	%client.promptedholdemtable = "";
	if(!isObject(%game))
	{
		return;
	}

	%buyIn = %game.buyin;
	if(%buyIn > %client.CasinoChips)
	{
		%client.chatMessage("\c5You don't have enough chips.");
		return;
	}


	if(%game.add(%client,%buyIn,%client.promptedholdemtableseat))
	{
		%client.CasinoChips -= %buyIn;

		messageclient(%client, '', "\c3You bought in with \c6" @ %buyIn @ "\c3 chips and you now have \c6" @ %client.CasinoChips @ "\c3 chips in storage.");
		%client.savePersistence();
	}
	else
	{
		%client.chatMessage(%client.getPlayerName() SPC "failed to join.");
	}
}