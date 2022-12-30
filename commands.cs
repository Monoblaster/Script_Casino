function serverCmdSetTable(%c,%id,%buyIn,%private)
{
	if(isObject(%id) && %c.isAdmin)
	{
		%buyIn += 0;
		$Casino::AtiveHoldemTable = %id;
		$Casino::AtiveHoldemTablePrivate = %private;
		$Casino::AtiveHoldemTableClient = %c;
		$Casino::AtiveHoldemTableBuyIn = %buyIn;
		if(!%private)
		{
			chatMessageAll("","\c5Texas Hold'em is open with a buy-in of " @ %buyIn @ "! Use /joinTable to request to join.");
		}
	}
}

function serverCmdJoinTable(%c)
{
	if(isObject($Casino::AtiveHoldemTable) && !$Casino::AtiveHoldemTablePrivate && (getSimTime() - %c.Holdem_lastRequestTime) >= 5000)
	{
		%group = ClientGroup;
		%count = %group.getCount();
		$Casino::AtiveHoldemTableClient.chatMessage("\c5"@%c.getPlayerName()SPC"has requested to join the table.");
		%c.Holdem_lastRequestTime = getSimTime();
	}
}

function serverCmdBuyTable(%client,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j)
{
	
	if($Casino::AtiveHoldemTableClient == %client)
	{
		%name = trim(%a SPC %b SPC %c SPC %d SPC %e SPC %f SPC %h SPC %h SPC %i SPC %j);
		%target = findClientByName(%name);
		if(isObject(%target))
		{	
			%buyIn = $Casino::AtiveHoldemTableBuyIn;
			if(%buyIn > %target.NYmoney)
			{
				%client.chatMessage(%target.getPlayerName() SPC "doesn't have enough money.");
				return;
			}

			
			if($Casino::AtiveHoldemTable.add(%target,%buyIn))
			{
				%client.chatMessage(%target.getPlayerName() SPC "has been added.");
				%target.NYmoney -= %buyIn;
				%target.setScore(%target.NYmoney);

				messageclient(%target, '', "\c3You paid \c6" @ %buyIn @ " points;\c3 you now have \c6" @ %target.NYmoney @ "\c3 points and \c6" @ $Casino::AtiveHoldemTable.exchange * %buyIn @ "\c3 chips.");

				NYlogs_write("config/server/LogNewYear/money.txt",
					NYlogs_addTime() TAB "MONEY_UPDATE" TAB "NYgiveClientMoney" TAB %target.getBLID() TAB %target.name TAB
					"AMOUNT" TAB %buyIn TAB "NEW_VALUE" TAB %target.NYmoney
				);
			}
			else
			{
				%client.chatMessage(%target.getPlayerName() SPC "failed to add.");
			}
		}
	}
}

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
	if($Casino::AtiveHoldemTableClient == %client && $Casino::AtiveHoldemTable.currInput $= "")
	{
		%client.chatMessage("Table has been started.");
		$Casino::AtiveHoldemTable.start(%blind);
	}
}